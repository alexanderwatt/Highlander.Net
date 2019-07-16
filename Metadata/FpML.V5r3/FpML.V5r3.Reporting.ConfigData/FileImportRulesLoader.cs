/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using Core.Common;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.NamedValues;

namespace Orion.V5r3.Configuration
{
    public static class FileImportRuleLoader
    {
        private static FileImportRule MakeImportRule(
            string ruleName, bool disabled, bool debugEnabled,
            TimeSpan monitorPeriod, IExpression constraintExpr,
            IExpression effectiveDateExpr, IExpression importDelayExpr,
            IExpression conditionExpr, IExpression dateUpdateExpr,
            NamedValueSet otherProperties,
            string sourceSystem,
            string sourceLocation,
            string targetLocation,
            string filePatterns,
            string fileContentType,
            bool onlyCopyUpdatedFiles)
        {
            return new FileImportRule
                {
                hostEnvName = null,
                hostComputer = null,
                hostInstance = null,
                hostUserName = null,
                RuleName = ruleName,
                Disabled = disabled,
                DebugEnabled = debugEnabled,
                MonitorPeriod = monitorPeriod.ToString(),
                CheckConstraintExpr = constraintExpr.Serialise(),
                EffectiveDateExpr = effectiveDateExpr.Serialise(),
                ImportDelayExpr = importDelayExpr.Serialise(),
                ImportConditionExpr = conditionExpr.Serialise(),
                DateUpdateExpr = dateUpdateExpr.Serialise(),
                OtherProperties = (otherProperties ?? new NamedValueSet()).Serialise(),
                SourceSystem = sourceSystem,
                SourceLocation = sourceLocation,
                TargetLocation = targetLocation,
                CopyFilePatterns = filePatterns,
                FileContentType = fileContentType,
                RemoveOldTargetFiles = false, //removeOldTargetFiles,
                OnlyCopyUpdatedFiles = onlyCopyUpdatedFiles
            };
        }

        public static void Load(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            logger.LogDebug("Loading file import rules...");
            // generate import rules
            var importRules = new List<FileImportRule>();
            {
                // defaults
                // - monitor period: check rules every 5 minutes
                TimeSpan defaultMonitorPeriod = TimeSpan.FromMinutes(5);
                // - check constraint: time of day is between 2am and 11pm
                IExpression defaultCheckConstraint = Expr.BoolAND(
                    //Expr.IsGEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Monday)),
                    //Expr.IsLEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Friday)),
                    Expr.IsGEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(2, 0, 0))),
                    Expr.IsLEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(23, 59, 59))));
                // - effective date: last import date plus 1 day
                IExpression defaultEffectiveDate = Expr.NumADD(Expr.FuncDatePart(Expr.Prop(RuleConst.LastImportDateTime)), Expr.Const(TimeSpan.FromDays(1)));
                // - import delay: 26 hours
                IExpression importDelayDefault = Expr.Const(TimeSpan.FromHours(2));
                IExpression importDelayCalypso = Expr.Const(TimeSpan.FromHours(26));
                IExpression importDelayMurex = Expr.Const(TimeSpan.FromHours(26));
                // - import condition: (current date - effective date) >= import delay
                IExpression defaultImportCondition = Expr.IsGEQ(Expr.NumSUB(Expr.FuncNow(), Expr.Prop(RuleConst.EffectiveDateTime)), Expr.Prop(RuleConst.ImportDelay));
                // - next import date = effective date, or today
                IExpression nextImportDateIncrement = Expr.Prop(RuleConst.EffectiveDateTime);
                IExpression nextImportDateUseLatest = Expr.FuncToday();
                // build rules
                // -------------------- Calyspo IR trades
                importRules.Add(MakeImportRule("CalypsoFRAs",
                    false,  // rule enabled
                    true,  // debugging on
                    defaultMonitorPeriod, defaultCheckConstraint,
                    defaultEffectiveDate, importDelayCalypso,
                    defaultImportCondition, nextImportDateIncrement,
                    null, // other properties
                    "Calypso",
                    @"\\melfiler02\marketrisk\mrtdata\AU\MEL",
                    @"C:\_qrsc\Imported\Calypso\{yyyy}\{MM}_{MMM}\{dd}_{ddd}",
                    "FraCalypso.{yyyy}{MM}{dd}.gz",
                    RuleConst.FileContentGwml65,
                    false // always copy
                    ));
                importRules.Add(MakeImportRule("CalypsoSwaps",
                    false,  // rule enabled
                    false,  // debugging off
                    defaultMonitorPeriod, defaultCheckConstraint,
                    defaultEffectiveDate, importDelayCalypso,
                    defaultImportCondition, nextImportDateIncrement,
                    null, // other properties
                    "Calypso",
                    @"\\melfiler02\marketrisk\mrtdata\AU\MEL",
                    @"C:\_qrsc\Imported\Calypso\{yyyy}\{MM}_{MMM}\{dd}_{ddd}",
                    "SwCalypso.{yyyy}{MM}{dd}.gz",
                    RuleConst.FileContentGwml65,
                    false // always copy
                    ));
                // -------------------- Murex FX trades
                importRules.Add(MakeImportRule("Murex",
                    false,  // rule enabled
                    false,  // debugging off
                    defaultMonitorPeriod, defaultCheckConstraint,
                    defaultEffectiveDate, importDelayMurex,
                    defaultImportCondition, nextImportDateIncrement,
                    null, // other properties
                    "Murex",
                    @"\\melfiler02\marketrisk\mrtdata\AU\GBL",
                    @"C:\_qrsc\Imported\Murex\{yyyy}\{MM}_{MMM}\{dd}_{ddd}",
                    "FxMurex.{yyyy}{MM}{dd}.gz;FxoMurex.{yyyy}{MM}{dd}.gz",
                    RuleConst.FileContentGwml65,
                    false // always copy
                    ));
                // -------------------- Cartoo EOD curves
                importRules.Add(MakeImportRule("Cartoo",
                    true,  // rule disabled - access is denied to svc_qrbuild account - need to use svc_qrdev
                    false,  // debugging off
                    defaultMonitorPeriod, defaultCheckConstraint,
                    defaultEffectiveDate, importDelayDefault,
                    defaultImportCondition, nextImportDateIncrement,
                    null, // other properties
                    "Cartoo",
                    @"\\melfiler02\xferdata\Cartoo\Out\MELB",
                    //@"C:\_qrsc\Imported\Cartoo\{yyyy}\{MM}_{MMM}\{dd}_{ddd}",
                    @"C:\_qrsc\Imported\Cartoo\Output",
                    "*.xls",
                    RuleConst.FileContentGenericText,
                    //false // always copy
                    true // only copy if newer
                    ));
                // -------------------- Credient output (Sungard) files
                importRules.Add(MakeImportRule("CredientDetail",
                    false,  // rule enabled
                    false,  // debugging off
                    defaultMonitorPeriod, defaultCheckConstraint,
                    defaultEffectiveDate, importDelayDefault,
                    defaultImportCondition, nextImportDateUseLatest,
                    null, // other properties
                    "Credient",
                    @"\\tmelf001\sentry\API\Credient\Output",
                    @"C:\_qrsc\Imported\Credient\Output",
                    "*.xml",
                    RuleConst.FileContentGenericXML,
                    true // only copy if newer
                    ));
                // -------------------- Credient snapshot files
                importRules.Add(MakeImportRule("CredientSnapshot",
                    false,  // rule enabled
                    false,  // debugging off
                    defaultMonitorPeriod, defaultCheckConstraint,
                    defaultEffectiveDate, importDelayDefault,
                    defaultImportCondition, nextImportDateUseLatest,
                    null, // other properties
                    "Credient",
                    @"\\tmelf001\sentry\API\Credient",
                    @"C:\_qrsc\Imported\Credient\Snapshot",
                    "*.txt",
                    RuleConst.FileContentGenericText,
                    false // always copy
                    ));
            }

            // save Import rules
            foreach (FileImportRule rule in importRules)
            {
                logger.LogDebug("  Loading {0} ...", rule.RuleName);
                rule.NameSpace = nameSpace;
                TimeSpan lifetime = rule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
                targetClient.SaveObject(rule, lifetime);
            }

            logger.LogDebug("Loaded {0} file import rules.", importRules.Count);
        }
    }
}
