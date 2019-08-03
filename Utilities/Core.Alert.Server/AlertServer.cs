/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using Core.Common;
using Orion.Util.Caching;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

#endregion

namespace Core.Alert.Server
{
    public class AlertServer : ServerBase2
    {
        private ICoreCache _ruleCache;
        private Timer _timer;
        private readonly Dictionary<string, InternalRule> _ruleStore = new Dictionary<string, InternalRule>();
        private long _dispatchedEventCount;
        private const string AppName = "AlertServer";
        private string _defaultSmtpServer;
        private string _defaultMailFrom;
        private string[] _defaultMailTo;

        protected override void OnServerStopping()
        {
            DisposeHelper.SafeDispose(ref _timer);
            DisposeHelper.SafeDispose(ref _ruleCache);
        }

        protected override void OnServerStarted()
        {
            base.OnServerStarted();
            _defaultSmtpServer = OtherSettings.GetValue(AlertRule.MailHost, "mailhost");
            _defaultMailFrom = OtherSettings.GetValue(AlertRule.MailFrom, AlertRule.UserDefaultReplyTo);
            _defaultMailTo = OtherSettings.GetArray(AlertRule.MailTo, new[] { AlertRule.GroupDefaultMailTo });
        }

        protected override void OnFirstCallback()
        {
            // subscribe to import rules
            _ruleCache = IntClient.Target.CreateCache(delegate(CacheChangeData update)
                {
                    Interlocked.Increment(ref _dispatchedEventCount);
                    MainThreadQueue.Dispatch(update, ProcessRuleUpdate);
                }, null);
            _ruleCache.Subscribe<AlertRule>(
                RuleObject.MakeRuleFilter(
                    EnvHelper.EnvName(IntClient.Target.ClientInfo.ConfigEnv),
                    IntClient.Target.ClientInfo.HostName,
                    AppName,
                    IntClient.Target.ClientInfo.UserName));
            // start a 30 second timer to periodically check the rules
            _timer = new Timer(RecvTimerEvent, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        private void RecvTimerEvent(object notUsed)
        {
            Interlocked.Increment(ref _dispatchedEventCount);
            MainThreadQueue.Dispatch<CacheChangeData>(null, ProcessRuleUpdate);
        }

        private void StopOldRule(AlertRule oldExternalRule, InternalRule newInternalRule)
        {
            if (_ruleStore.TryGetValue(oldExternalRule.PrivateKey.ToLower(), out var oldInternalRule))
            {
                if (newInternalRule != null)
                    newInternalRule.LastStatus = oldInternalRule.LastStatus;
                oldInternalRule.DataSubs?.Cancel();
            }
            // update rule store
            _ruleStore.Remove(oldExternalRule.PrivateKey.ToLower());
        }

        private void StartNewRule(AlertRule newExternalRule, InternalRule newInternalRule)
        {
            if (!newInternalRule.Disabled && newInternalRule.DataSubsExpr != null)
            {
                newInternalRule.DataSubs = IntClient.Target.CreateUntypedSubscription(null, newInternalRule.DataSubsExpr);
                newInternalRule.DataSubs.UserCallback =
                    delegate(ISubscription subscription, ICoreItem item)
                    {
                        var rule = (InternalRule)subscription.UserContext;
                        ICoreItem lastItem = rule.CurrentReceivedItem;
                        if (lastItem == null || (item.Created > lastItem.Created))
                            rule.CurrentReceivedItem = item;
                    };
                newInternalRule.DataSubs.UserContext = newInternalRule;
                newInternalRule.DataSubs.DataTypeName = newInternalRule.DataTypeName;
                newInternalRule.DataSubs.WaitForExisting = true;
                newInternalRule.DataSubs.Start();
                Thread.Sleep(2000);
            }
            // update rule store
            _ruleStore[newExternalRule.PrivateKey.ToLower()] = newInternalRule;
        }

        private void ProcessRuleUpdate(CacheChangeData update)
        {
            try
            {
                // update the rules
                if (update != null)
                {
                    UpdateRules(update);
                }

                long updatesDispatchedLocal = Interlocked.Decrement(ref _dispatchedEventCount);
                if (updatesDispatchedLocal > 0)
                {
                    // more updates following - just exit
                    return;
                }

                // process the rules
                ProcessAllRules();
            }
            catch (Exception ex)
            {
                ReportUncaughtError("ProcessRuleUpdate", null, ex, null);
                throw;
            }
        }

        private void UpdateRules(CacheChangeData update)
        {
            if (update.NewItem == null)
            {
                throw new ArgumentException("update.NewItem is null: " + update);
            }
            string ruleName = null;
            try
            {
                ruleName = update.NewItem.Name;
                switch (update.Change)
                {
                    case CacheChange.CacheCleared:
                        _ruleStore.Clear();
                        break;
                    case CacheChange.ItemExpired:
                    case CacheChange.ItemRemoved:
                        {
                            var oldExternalRule = (AlertRule) update.OldItem.Data;
                            // stop old rule
                            StopOldRule(oldExternalRule, null);
                            Logger.LogDebug("Rule {0}: Removed.", oldExternalRule.PrivateKey);
                        }
                        break;
                    case CacheChange.ItemCreated:
                        {
                            var newExternalRule = (AlertRule) update.NewItem.Data;
                            var newInternalRule = new InternalRule(newExternalRule);
                            StartNewRule(newExternalRule, newInternalRule);
                            Logger.LogDebug("Rule {0}: Created.", newExternalRule.PrivateKey);
                        }
                        break;
                    case CacheChange.ItemUpdated:
                        {
                            var oldExternalRule = (AlertRule) update.OldItem.Data;
                            var newExternalRule = (AlertRule) update.NewItem.Data;
                            var newInternalRule = new InternalRule(newExternalRule);
                            StopOldRule(oldExternalRule, newInternalRule);
                            StartNewRule(newExternalRule, newInternalRule);
                            Logger.LogDebug("Rule {0}: Updated.", newExternalRule.PrivateKey);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportUncaughtError("UpdateRules", ruleName, ex, null);
            }
        }

        private string ResolveUserName(string userName, Dictionary<string, string> parentVisitedUsers)
        {
            // recursively resolve user name
            Dictionary<string, string> visitedUsers = parentVisitedUsers ?? new Dictionary<string, string>();
            if (visitedUsers.ContainsKey(userName.ToLower()))
                throw new ArgumentException($"Recursion detected resolving userName='{userName}'");
            visitedUsers.Add(userName.ToLower(), null);

            var userDetail = IntClient.Target.LoadObject<UserDetail>((new UserDetail { UserName = userName }).NetworkKey);
            if (userDetail?.EmailAddress == null)
                return null;
            return ResolveUserName(userDetail.EmailAddress, visitedUsers) ?? userDetail.EmailAddress;
        }

        private IEnumerable<string> ResolveGroupName(string groupName, Dictionary<string, string> parentVisitedGroups)
        {
            Dictionary<string, string> visitedGroups = parentVisitedGroups ?? new Dictionary<string, string>();
            if (visitedGroups.ContainsKey(groupName.ToLower()))
                throw new ArgumentException($"Recursion detected resolving groupName='{groupName}'");
            visitedGroups.Add(groupName.ToLower(), null);

            var emailAddresses = new List<string>();
            string groupItemName = (new UserGroup { GroupName = groupName }).NetworkKey.ToLower();
            var userGroup = IntClient.Target.LoadObject<UserGroup>(groupItemName);
            if (userGroup != null)
            {
                emailAddresses.AddRange((userGroup.MemberUsers ?? new string[] {}).Select(userName => ResolveUserName(userName, null)).Where(emailAddress => emailAddress != null));
                foreach (string innerGroupName in userGroup.MemberGroups ?? new string[] { })
                {
                    emailAddresses.AddRange(ResolveGroupName(innerGroupName, visitedGroups));
                }
            }
            return emailAddresses;
        }

        private IEnumerable<string> ResolveMultipleEmailAddresses(IEnumerable<string> userAndGroupNames)
        {
            var emailAddresses = new List<string>();
            foreach (string userOrGroupName in userAndGroupNames)
            {
                string emailAddress = ResolveUserName(userOrGroupName, null);
                if (emailAddress != null)
                    emailAddresses.Add(emailAddress);
                else
                    emailAddresses.AddRange(ResolveGroupName(userOrGroupName, null));
            }
            return emailAddresses.ToArray();
        }

        private string ResolveSingleEmailAddress(string userName)
        {
            string emailAddress = ResolveUserName(userName, null);
            if (emailAddress != null)
                return emailAddress;
            throw new ArgumentException($"Failed to resolve userName='{userName}'");
        }

        private void ProcessAllRules()
        {
            // note: no locks required - as we are inside main Dispatcher
            try
            {
                // publish latest rule status
                DateTimeOffset asAtTime = DateTimeOffset.Now;
                foreach (InternalRule rule in _ruleStore.Values)
                {
                    var props = new NamedValueSet();
                    try
                    {
                        if ((rule.LastMonitored + rule.MonitorPeriod) < asAtTime)
                        {
                            // update LastMonitored first to prevent infinite looping
                            // when there are repeated expression evaluation exceptions
                            rule.LastMonitored = asAtTime;
                            // build property set
                            string smtpServer = rule.Properties.GetString(AlertRule.MailHost, null) ?? _defaultSmtpServer;
                            string mailFrom = rule.Properties.GetString(AlertRule.MailFrom, null) ?? _defaultMailFrom;
                            string[] recipients = rule.Properties.GetArray<string>(AlertRule.MailTo) ?? _defaultMailTo;
                            props.Set(AlertRule.MailHost, smtpServer);
                            props.Set(AlertRule.MailFrom, mailFrom);
                            props.Set(AlertRule.MailTo, recipients);
                            props.Set("host.AsAtTime", asAtTime);
                            props.Set("host.HostName", IntClient.Target.ClientInfo.HostName);
                            props.Set("rule.DebugEnabled", rule.Properties.GetValue<bool>("DebugEnabled", false));
                            props.Set("rule.RuleName", rule.RuleName);
                            props.Set("rule.Disabled", rule.Disabled);
                            props.Set("rule.DataSubsExpr", rule.DataSubsExpr.DisplayString());
                            props.Set("rule.Constraint", rule.Constraint.DisplayString());
                            props.Set("rule.Condition", rule.Condition.DisplayString());
                            props.Set("rule.MonitorPeriod", rule.MonitorPeriod);
                            props.Add(rule.Properties);
                            if (rule.CurrentReceivedItem != null)
                            {
                                props.Add(rule.CurrentReceivedItem.AppProps);
                                props.Set("item.IsNotNull", true);
                            }
                            else
                            {
                                // no item received
                                props.Set("item.IsNotNull", false);
                            }
                            AlertStatus newStatus;
                            string statusMessage;
                            var signal = new InternalSignal
                            {
                                RuleName = rule.RuleName,
                                AlertServer = IntClient.Target.ClientInfo.HostName
                            };
                            if (rule.Disabled)
                            {
                                newStatus = AlertStatus.Disabled;
                                statusMessage = "the rule is disabled";
                            }
                            else
                            {
                                // extend constraint
                                ICoreItem item = rule.CurrentReceivedItem;
                                object constraint = (item == null)
                                    ? rule.Constraint.Evaluate(props, null, DateTimeOffset.MinValue, DateTimeOffset.MinValue, asAtTime)
                                    : rule.Constraint.Evaluate(props, item.Name, item.Created, item.Expires, asAtTime);
                                if (Expr.CastTo(constraint, false))
                                {
                                    object condition = item == null
                                        ? rule.Condition.Evaluate(props, null, DateTimeOffset.MinValue, DateTimeOffset.MinValue, asAtTime)
                                        : rule.Condition.Evaluate(props, item.Name, item.Created, item.Expires, asAtTime);
                                    if (Expr.CastTo(condition, false))
                                    {
                                        newStatus = AlertStatus.Alerted;
                                        statusMessage = rule.SignalFormat;
                                    }
                                    else
                                    {
                                        newStatus = AlertStatus.AllClear;
                                        statusMessage = "the condition is no longer active";
                                    }
                                }
                                else
                                {
                                    newStatus = AlertStatus.Inactive;
                                    statusMessage = "the constraint is no longer satisfied";
                                }
                            }
                            props.Set("rule.Status", newStatus.ToString());
                            signal.Status = newStatus;
                            signal.LastMonitored = asAtTime;
                            signal.ReminderCount = 0;
                            if (rule.LastSignal != null)
                                signal.ReminderCount = rule.LastSignal.ReminderCount;
                            if (newStatus == AlertStatus.Alerted)
                                signal.SignalMessage = props.ReplaceTokens(statusMessage);
                            else
                            {
                                signal.SignalMessage =
                                    $"The previous alert message sent from this server ({signal.AlertServer}) " +
                                    $"for this rule ({signal.RuleName}) has been withdrawn because {statusMessage}.";
                            }
                            // set instance (repeat) count
                            if (newStatus != rule.LastStatus)
                            {
                                signal.ReminderCount = 0;
                            }
                            if ((newStatus != rule.LastStatus) || ((rule.LastPublished + rule.PublishPeriod) < asAtTime))
                            {
                                rule.LastPublished = asAtTime;
                                // publish signal
                                PublishAlertSignal(signal);
                                // email signal if status is or was alerted
                                if ((newStatus == AlertStatus.Alerted) || (rule.LastStatus == AlertStatus.Alerted))
                                {
                                    Logger.LogInfo(
                                        "Rule '{0}' {1}: {2}", signal.RuleName, signal.Status, signal.SignalMessage);
                                    SendAlertSignalEmail(signal, props);
                                }
                            }
                            // done
                            rule.LastStatus = newStatus;
                            rule.LastSignal = signal;
                        }
                    }
                    catch (Exception ex)
                    {
                        ReportUncaughtError("MainCallback", rule.RuleName, ex, props);
                    }
                } // foreach rule
            }
            catch (Exception ex)
            {
                ReportUncaughtError("MainCallback", null, ex, null);
            }
        }

        protected override void OnCloseCallback()
        {
            try
            {
                // server shutting down
                // - publish all rule status as Undefined
                DateTimeOffset now = DateTimeOffset.Now;
                foreach (InternalRule rule in _ruleStore.Values)
                {
                    try
                    {
                        var signal = new InternalSignal
                                                    {
                                                        RuleName = rule.RuleName,
                                                        AlertServer = IntClient.Target.ClientInfo.HostName,
                                                        Status = AlertStatus.Undefined,
                                                        LastMonitored = now,
                                                        SignalMessage = "AlertServer shutdown"
                                                    };
                        PublishAlertSignal(signal);
                    }
                    catch (Exception ex)
                    {
                        ReportUncaughtError("OnCloseCallback", rule.RuleName, ex, null);
                    }
                } // foreach rule
            }
            catch (Exception ex)
            {
                ReportUncaughtError("OnCloseCallback", null, ex, null);
            }
        }

        private void PublishAlertSignal(InternalSignal signal)
        {
            AlertSignal item = signal.AsAlertSignal();
            IntClient.Target.SaveObject(item, item.ItemName, item.ItemProps, TimeSpan.MaxValue);
        }

        private static string NthString(int n)
        {
            switch (n % 100)
            {
                case 11:
                case 12:
                case 13:
                    return $"{n}th";
                default:
                    switch (n % 10)
                    {
                        case 1:
                            return $"{n}st";
                        case 2:
                            return $"{n}nd";
                        case 3:
                            return $"{n}rd";
                        default:
                            return $"{n}th";
                    }
            }
        }

        private static string GetInstanceMsg(int instanceNum)
        {
            if (instanceNum == 0)
                return "Initial Notification";

            return $"{NthString(instanceNum)} Reminder";
        }

        private void SendAlertSignalEmail(InternalSignal signal, NamedValueSet props)
        {
            try
            {
                var smtpHost = props.GetValue<string>(AlertRule.MailHost);
                IEnumerable<string> recipients = ResolveMultipleEmailAddresses(props.GetArray<string>(AlertRule.MailTo));
                string sender = ResolveSingleEmailAddress(props.GetValue<string>(AlertRule.MailFrom));
                var email = new MailMessage();
                string instanceMsg = GetInstanceMsg(signal.ReminderCount);
                signal.ReminderCount += 1;
                email.Subject =
                    $"QDS Alert {EnvHelper.EnvName(IntClient.Target.ClientInfo.ConfigEnv)}. {signal.RuleName}: {signal.Status}";

                if (signal.Status == AlertStatus.Alerted)
                {
                    email.Subject += $"({instanceMsg})";
                }
                var body = new StringBuilder(signal.SignalMessage);
                if (props.GetValue<bool>("rule.DebugEnabled", false))
                {
                    body.AppendLine();
                    body.AppendLine();
                    body.AppendLine("[debug-begin]");
                    props.LogValues(text => body.AppendLine("  " + text));
                    body.AppendLine("[debug-end]");
                }
                email.Body = body.ToString();
                SendEmail(smtpHost, email, sender, recipients);
            }
            catch (Exception excp)
            {
                ReportUncaughtError("EmailAlertSignal", signal.RuleName, excp, props);
            }
        }

        private void ReportUncaughtError(string method, string ruleName, Exception ex, NamedValueSet debugProps)
        {
            // first log to EventLog
            string message = string.IsNullOrEmpty(ruleName) 
                ? $"Exception in '{method}': \n\n{ex}"
                : $"Exception in '{method}', Rule '{ruleName}': \n\n{ex}'";
            Logger.LogError(message);
            // then email
            try
            {
                string smtpHost = _defaultSmtpServer;
                IEnumerable<string> recipients = ResolveMultipleEmailAddresses(_defaultMailTo);
                string sender = ResolveSingleEmailAddress(_defaultMailFrom);
                var email = new MailMessage
                                {
                                    Subject =
                                        $"QDS Alert {EnvHelper.EnvName(IntClient.Target.ClientInfo.ConfigEnv)}. UNHANDLED ERROR"
                                };
                //new
                var body = new StringBuilder(message);
                if (debugProps != null)
                {
                    body.AppendLine();
                    body.AppendLine();
                    body.AppendLine("[debug-begin]");
                    debugProps.LogValues(text => body.AppendLine("  " + text));
                    body.AppendLine("[debug-end]");
                }
                email.Body = body.ToString();
                //end new
                SendEmail(smtpHost, email, sender, recipients);
            }
            catch (Exception excp)
            {
                Logger.LogError(excp);
            }
        }

        private void SendEmail(string smtpHost, MailMessage email, string sender, IEnumerable<string> recipients)
        {
            foreach (string recipient in recipients)
            {
                Logger.LogDebug("EmailAlertSignal: Recipient : {0}", recipient);
                email.To.Add(recipient);
            }
            Logger.LogDebug("EmailAlertSignal: Sender    : {0}", sender);
            email.From = new MailAddress(sender,
                $"QDS AlertServer {EnvHelper.EnvName(IntClient.Target.ClientInfo.ConfigEnv)}");
            string[] hostAndPorts = smtpHost.Split(':');
            int port = 25;
            if (hostAndPorts.Length > 1)
            {
                smtpHost = hostAndPorts[0];
                port = int.Parse(hostAndPorts[1]);
            }
            Logger.LogDebug("EmailAlertSignal: Connecting: {0}:{1}", smtpHost, port);
            var client = new SmtpClient(smtpHost, port);
            client.Send(email);
            Logger.LogDebug("EmailAlertSignal: Body:");
            Logger.LogDebug(email.Body);
            Logger.LogDebug("EmailAlertSignal: Sent.");
        }
    }
}
