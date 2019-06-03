/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Core.Common;
using Orion.Util.Caching;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.V5r3.Configuration;

#endregion

namespace Server.FileImporter
{
    public class InternalRule
    {
        // from FileImportRule
        public readonly string RuleName;
        public readonly string HostName;
        public readonly string Instance;
        public readonly bool Disabled;
        public readonly bool DebugEnabled;
        public readonly NamedValueSet DebugProperties;
        public readonly IExpression CheckConstraint;
        public readonly IExpression EffectiveDateExpr;
        public readonly IExpression ImportDelayExpr;
        public readonly IExpression ImportCondition;
        public readonly IExpression DateUpdateExpr;
        public readonly string SourceSystem;
        public readonly string SourceLocation;
        public readonly string TargetLocation;
        public readonly NamedValueSet Properties;
        public readonly TimeSpan MonitorPeriod;
        public readonly int AsAtDateOffset;
        public readonly string CopyFilePatterns;
        public readonly string FileContentType;
        public readonly bool OnlyCopyUpdatedFiles;
        public readonly bool RemoveOldTargetFiles;
        // calculated
        public DateTimeOffset LastMonitored { get; set; }
        //public DateTimeOffset LastExecuted { get; set; }
        public string UniqueKey => $"[{RuleName}][{HostName}][{Instance}]".ToLower();

        public InternalRule(FileImportRule rule)
        {
            RuleName = rule.RuleName;
            HostName = rule.HostName;
            Instance = rule.Instance;
            Disabled = rule.Disabled;
            DebugEnabled = rule.DebugEnabled;
            DebugProperties = new NamedValueSet(rule.DebugProperties);
            MonitorPeriod = rule.MonitorPeriod != null ? TimeSpan.Parse(rule.MonitorPeriod) : TimeSpan.FromMinutes(5);
            CheckConstraint = rule.CheckConstraintExpr != null ? Expr.Create(rule.CheckConstraintExpr) : Expr.Const(true);
            EffectiveDateExpr = rule.EffectiveDateExpr != null ? Expr.Create(rule.EffectiveDateExpr) : Expr.FuncToday();
            ImportDelayExpr = rule.ImportDelayExpr != null ? Expr.Create(rule.ImportDelayExpr) : Expr.Const(TimeSpan.Zero);
            ImportCondition = rule.ImportConditionExpr != null ? Expr.Create(rule.ImportConditionExpr) : Expr.Const(true);
            DateUpdateExpr = rule.DateUpdateExpr != null ? Expr.Create(rule.DateUpdateExpr) : Expr.FuncToday();
            SourceSystem = rule.SourceSystem;
            SourceLocation = rule.SourceLocation;
            TargetLocation = rule.TargetLocation;
            Properties = new NamedValueSet(rule.OtherProperties);
            AsAtDateOffset = rule.AsAtDateOffset;
            CopyFilePatterns = rule.CopyFilePatterns;
            FileContentType = rule.FileContentType;
            OnlyCopyUpdatedFiles = rule.OnlyCopyUpdatedFiles;
            RemoveOldTargetFiles = rule.RemoveOldTargetFiles;
            LastMonitored = DateTimeOffset.MinValue;
        }
    }

    public class FileImportServer : ServerBase2
    {
        private ICoreCache _importRuleSet;
        private readonly Dictionary<string, InternalRule> _ruleStore = new Dictionary<string, InternalRule>();
        private Timer _timer;
        private long _updateRequestsDispatched;
        private const string AppName = "FileImportServer";

        //public FileImportServer(ILogger logger, EnvId env)
        //    : base(logger, env, null, null, null)
        //{ }

        protected override void OnServerStopping()
        {
            DisposeHelper.SafeDispose(ref _timer);
            DisposeHelper.SafeDispose(ref _importRuleSet);
        }

        protected override void OnFirstCallback()
        {
            // subscribe to import rules
            _importRuleSet = IntClient.Target.CreateCache(delegate(CacheChangeData update)
               {
                   Interlocked.Increment(ref _updateRequestsDispatched);
                   MainThreadQueue.Dispatch(update, ProcessRuleUpdate);
               } , null);
            _importRuleSet.Subscribe<FileImportRule>(
                RuleObject.MakeRuleFilter(
                    EnvHelper.EnvName(IntClient.Target.ClientInfo.ConfigEnv),
                    IntClient.Target.ClientInfo.HostName,
                    AppName,
                    IntClient.Target.ClientInfo.UserName));

            // start a 1 minute timer to periodically check the rules
            _timer = new Timer(RecvTimerEvent, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        private void RecvTimerEvent(object notUsed)
        {
            Interlocked.Increment(ref _updateRequestsDispatched);
            MainThreadQueue.Dispatch<CacheChangeData>(null, ProcessRuleUpdate);
        }

        private void ProcessRuleUpdate(CacheChangeData update)
        {
            // update the rules
            if (update != null)
            {
                switch (update.Change)
                {
                    case CacheChange.CacheCleared:
                        _ruleStore.Clear();
                        break;
                    case CacheChange.ItemExpired:
                    case CacheChange.ItemRemoved:
                        {
                            FileImportRule oldImportRule = (FileImportRule)update.OldItem.Data;
                            _ruleStore.Remove(oldImportRule.PrivateKey.ToLower());
                            Logger.LogDebug("Rule {0}: Removed.", oldImportRule.RuleName);
                        }
                        break;
                    case CacheChange.ItemCreated:
                        {
                            var newImportRule = (FileImportRule)update.NewItem.Data;
                            _ruleStore[newImportRule.PrivateKey.ToLower()] = new InternalRule(newImportRule);
                            Logger.LogDebug("Rule {0}: Created.", newImportRule.RuleName);
                        }
                        break;
                    case CacheChange.ItemUpdated:
                        {
                            //var oldImportRule = (FileImportRule)update.OldItem.Data;
                            var newImportRule = (FileImportRule)update.NewItem.Data;
                            //InternalRule oldRule = _RuleStore.GetValue(newImportRulePrivateKey.ToLower(), null);
                            var newRule = new InternalRule(newImportRule);
                            _ruleStore[newImportRule.PrivateKey.ToLower()] = newRule;
                            Logger.LogDebug("Rule {0}: Updated.", newImportRule.RuleName);
                        }
                        break;
                }
            }          
            long requestsDispatched = Interlocked.Decrement(ref _updateRequestsDispatched);
            if (requestsDispatched > 0)
            {
                // more requests following - just exit
                return;
            }
            // process the rules
            DateTimeOffset currentTime = DateTimeOffset.Now;
            foreach (InternalRule rule in _ruleStore.Values)
            {
                try
                {
                    if (rule.LastMonitored + rule.MonitorPeriod < currentTime)
                    {
                        // update LastMonitored first to prevent infinite looping
                        // when there are repeated expression evaluation exceptions
                        rule.LastMonitored = currentTime;
                        ProcessRule(IntClient.Target, rule, currentTime);
                    }
                }
                catch (Exception ruleExcp)
                {
                    Logger.LogError("Rule {0}: EXCEPTION! {1}'", rule.RuleName, ruleExcp);
                }

            }
        }

        private void ProcessRule(ICoreClient client, InternalRule rule, DateTimeOffset currentTime)
        {
            RuleStatusEnum ruleStatus;
            ILogger logger = new FilterLogger(
                Logger, $"Rule {rule.RuleName}: ", 
                rule.DebugEnabled ? LogSeverity.Debug : LogSeverity.Info);
            using (var settings = new SettingsTracker(client, "FileImporter." + rule.RuleName))
            {
                bool lastCheckFailed = false;
                string lastCheckException = "(null)";
                DateTimeOffset lastCheckDateTime = currentTime;
                //logger.LogDebug("Processing...");
                try
                {
                    ruleStatus = RuleStatusEnum.Disabled;
                    if (!rule.Disabled)
                    {
                        // evaluate rule constraint and condition
                        var properties = new NamedValueSet(settings.GetAllValues(true));
                        properties.Add(rule.Properties);
                        // last import date/time (default to 4 days ago)
                        var lastImportDateTime = settings.GetSetValue(RuleConst.LastImportDateTime, DateTimeOffset.Now.AddDays(-4));
                        properties.Set(RuleConst.LastImportDateTime, lastImportDateTime);
                        // calculate effective "as at" date
                        var thisImportDateTime = Expr.CastTo(rule.EffectiveDateExpr.Evaluate(properties, currentTime), currentTime);
                        properties.Set(RuleConst.EffectiveDateTime, thisImportDateTime);
                        // add useful date/time tokens
                        //foreach (string token in new string[] { "dd", "ddd", "MM", "MMM", "yyyy" })
                        //{
                        //    properties.Set(token, thisImportDateTime.ToString(token));
                        //}
                        // calculate import delay
                        var thisImportDelay = Expr.CastTo(rule.ImportDelayExpr.Evaluate(properties, currentTime), TimeSpan.Zero);
                        properties.Set(RuleConst.ImportDelay, thisImportDelay);
                        // evaluate rule check constraint and import condition
                        logger.LogDebug("Evaluation Params :");
                        properties.LogValues(text => logger.LogDebug("    " + text));
                        logger.LogDebug("Check Constraint  : {0}", rule.CheckConstraint);
                        ruleStatus = RuleStatusEnum.Inactive;
                        if (Expr.CastTo(rule.CheckConstraint.Evaluate(properties, currentTime), false))
                        {
                            logger.LogDebug("Import Condition  : {0}", rule.ImportCondition);
                            ruleStatus = RuleStatusEnum.NotReady;
                            if (Expr.CastTo(rule.ImportCondition.Evaluate(properties, currentTime), false))
                            {
                                ruleStatus = RuleStatusEnum.Failed;
                                // import condition is true
                                // process date/time tokens
                                string targetLocation = StringHelper.ReplaceDateTimeTokens(rule.TargetLocation, thisImportDateTime);
                                string sourceLocation = StringHelper.ReplaceDateTimeTokens(rule.SourceLocation, thisImportDateTime);
                                var importedFiles = new List<string>();
                                logger.LogInfo("Source Location  : {0}", sourceLocation);
                                logger.LogInfo("Target Location  : {0}", targetLocation);
                                logger.LogInfo("Filenames to copy: {0}", rule.CopyFilePatterns);
                                string thisImportException = "(null)";
                                try
                                {
                                    // import the file
                                    // - optionally clean up old files aged more than 7 days
                                    if (rule.RemoveOldTargetFiles)
                                    {
                                        try
                                        {
                                            string[] oldTargetFiles = Directory.GetFiles(targetLocation, "*.*", SearchOption.TopDirectoryOnly);
                                            foreach (string oldTargetFile in oldTargetFiles)
                                            {
                                                var targetFileInfo = new FileInfo(oldTargetFile);
                                                if ((currentTime - targetFileInfo.LastWriteTime) > TimeSpan.FromDays(2))
                                                {
                                                    File.Delete(oldTargetFile);
                                                }
                                            }
                                        }
                                        catch (IOException removeExcp)
                                        {
                                            logger.LogWarning("Error removing old files: {0}", removeExcp.GetType().Name);
                                            // ignored
                                        }
                                    }
                                    // - create target directory if required
                                    if (!Directory.Exists(targetLocation))
                                    {
                                        Directory.CreateDirectory(targetLocation);
                                    }
                                    // - copy file(s) from source to target
                                    foreach (string ruleFilePattern in rule.CopyFilePatterns.Split(';'))
                                    {
                                        string filePattern = StringHelper.ReplaceDateTimeTokens(ruleFilePattern, thisImportDateTime);
                                        string[] sourceFiles = Directory.GetFiles(sourceLocation, filePattern, SearchOption.TopDirectoryOnly);
                                        logger.LogInfo("Copying file(s): {0} ({1} found)", filePattern, sourceFiles.Length);
                                        int copiedCount = 0;
                                        int skippedCount = 0;
                                        foreach (string sourceFileFullname in sourceFiles)
                                        {
                                            string sourceFileBaseName = Path.GetFileName(sourceFileFullname);
                                            string targetFileFullname = $@"{targetLocation}\{sourceFileBaseName}";
                                            bool copyRequired = true;
                                            if (File.Exists(targetFileFullname) && rule.OnlyCopyUpdatedFiles)
                                            {
                                                var sourceFileInfo = new FileInfo(sourceFileFullname);
                                                var targetFileInfo = new FileInfo(targetFileFullname);
                                                copyRequired = (sourceFileInfo.LastWriteTime > targetFileInfo.LastWriteTime);
                                            }
                                            if (copyRequired)
                                            {
                                                logger.LogInfo("Copying file : {0}", sourceFileBaseName);
                                                logger.LogInfo("  From source: {0}", sourceLocation);
                                                logger.LogInfo("    To target: {0}", targetLocation);
                                                DateTime copyCommenced = DateTime.Now;
                                                File.Copy(sourceFileFullname, targetFileFullname, true);
                                                TimeSpan copyDuration = DateTime.Now - copyCommenced;
                                                importedFiles.Add(sourceFileBaseName);
                                                var targetFileInfo = new FileInfo(targetFileFullname);
                                                copiedCount++;
                                                logger.LogInfo("  Copied {0}MB in {1}s ({2}KB/sec)",
                                                    (targetFileInfo.Length / 1000000.0).ToString("N"),
                                                    copyDuration.TotalSeconds.ToString("N"),
                                                    (targetFileInfo.Length / (1000.0 * copyDuration.TotalSeconds)).ToString("N"));
                                                // publish rule import status
                                                var importFileResult = new ImportFileResult
                                                    {
                                                        hostEnvName = EnvHelper.EnvName(IntClient.Target.ClientInfo.ConfigEnv),
                                                    hostComputer = IntClient.Target.ClientInfo.HostName,
                                                    hostInstance = null,
                                                    hostUserName = client.ClientInfo.UserName,
                                                    RuleName = rule.RuleName,
                                                    FileName = sourceFileBaseName,
                                                    DebugEnabled = rule.DebugEnabled,
                                                    DebugProperties = rule.DebugProperties.Serialise(),
                                                    FileContentType = rule.FileContentType,
                                                    ImportResult = RuleStatusEnum.Completed.ToString(),
                                                    ImportException = null,
                                                    ImportDateTime = currentTime.ToString("o"),
                                                    SourceSystem = rule.SourceSystem,
                                                    SourceLocation = sourceLocation,
                                                    TargetLocation = targetLocation
                                                };
                                                IntClient.Target.SaveObject(importFileResult, true, TimeSpan.FromDays(30));
                                            }
                                            else
                                            {
                                                skippedCount++;
                                                logger.LogDebug("Skipping file : {0}", sourceFileBaseName);
                                            }
                                        } // foreach file
                                        logger.LogInfo("Copied {0} file(s), skipped {1} file(s).", copiedCount, skippedCount);
                                    }
                                    // - optionally decompress target
                                    // todo
                                    // done
                                    ruleStatus = RuleStatusEnum.Completed;
                                    lastImportDateTime = Expr.CastTo(rule.DateUpdateExpr.Evaluate(properties, currentTime), currentTime);
                                }
                                catch (Exception e2)
                                {
                                    logger.Log(e2);
                                    thisImportException = e2.ToString();
                                    ruleStatus = RuleStatusEnum.Failed;
                                }
                                finally
                                {
                                    settings.SetNewValue(RuleConst.LastImportResult, ruleStatus.ToString());
                                    settings.SetNewValue(RuleConst.LastImportException, thisImportException);
                                    settings.SetNewValue(RuleConst.LastImportDateTime, lastImportDateTime);
                                }
                                // publish rule import status
                                var importRuleResult = new ImportRuleResult
                                    {
                                        hostEnvName = EnvHelper.EnvName(IntClient.Target.ClientInfo.ConfigEnv),
                                    hostComputer = IntClient.Target.ClientInfo.HostName,
                                    hostInstance = null,
                                    hostUserName = client.ClientInfo.UserName,
                                    RuleName = rule.RuleName,
                                    ImportResult = ruleStatus.ToString(),
                                    ImportException = thisImportException,
                                    ImportDateTime = currentTime.ToString("o"),
                                    SourceSystem = rule.SourceSystem,
                                    SourceLocation = sourceLocation,
                                    TargetLocation = targetLocation,
                                    FileNames = importedFiles.ToArray()
                                };
                                IntClient.Target.SaveObject(importRuleResult, true, TimeSpan.FromDays(30));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(e);
                    lastCheckFailed = true;
                    lastCheckException = e.ToString();
                    ruleStatus = RuleStatusEnum.Failed;
                }
                settings.SetNewValue(RuleConst.LastCheckFailed, lastCheckFailed);
                settings.SetNewValue(RuleConst.LastCheckException, lastCheckException);
                settings.SetNewValue(RuleConst.LastCheckDateTime, lastCheckDateTime);
            } // commit unsaved settings
            logger.LogDebug("Status={0}", ruleStatus);
        }
    }
}
