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
using FpML.V5r3.Reporting;
using AlertRule = Metadata.Common.AlertRule;
using AlertSignal = Metadata.Common.AlertSignal;

namespace Orion.V5r3.Configuration
{
    public static class AlertRulesLoader
    {
        private const string AWatt = "alexawatt@hotmail.com";

        private static AlertRule MakeAlertRule(
            string ruleName, bool disabled,
            string hostEnvName,
            string hostInstance,
            IExpression constraintExpr,
            IExpression conditionExpr,
            Type dataType,
            IExpression dataSubsExpr,
            NamedValueSet alertProps,
            TimeSpan monitorPeriod,
            TimeSpan publishPeriod,
            string signalFormat)
        {
            return new AlertRule
            {
                RuleName = ruleName,
                Disabled = disabled,
                hostEnvName = hostEnvName,
                hostInstance = hostInstance,
                Constraint = constraintExpr.Serialise(),
                Condition = conditionExpr.Serialise(),
                DataTypeName = dataType.FullName,
                DataSubsExpr = dataSubsExpr.Serialise(),
                AlertProperties = alertProps.Serialise(),
                MonitorPeriod = monitorPeriod.ToString(),
                PublishPeriod = publishPeriod.ToString(),
                SignalFormat = signalFormat,
                hostComputer = null,
                hostUserName = null,
                DataItemType = typeof(object).FullName
            };
        }

        public static void Load(ILogger logger, ICoreCache targetClient, string nameSpace)
        {
            logger.LogDebug("Loading alert rules...");
            // defaults
            // - check constraint: time of day is between 2am and 11pm 7 days a week
            IExpression defaultCheckConstraint = Expr.BoolAND(
                Expr.IsGEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Monday)),
                Expr.IsLEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Friday)),
                Expr.IsGEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(4, 30, 0))),
                Expr.IsLEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(20, 30, 0))));
            // - monitor period: every minute
            TimeSpan defaultMonitorPeriod = TimeSpan.FromMinutes(1);
            // - publish period: hourly
            TimeSpan defaultPublishPeriod = TimeSpan.FromHours(1);
            // build rules
            var alertRules = new List<AlertRule>();
            alertRules.AddRange(CreateDevRules(defaultCheckConstraint, defaultMonitorPeriod, defaultPublishPeriod));
            alertRules.AddRange(CreateSitRules(defaultCheckConstraint, defaultMonitorPeriod, defaultPublishPeriod));
            alertRules.AddRange(CreateStgRules(defaultCheckConstraint, defaultMonitorPeriod, defaultPublishPeriod));
            alertRules.AddRange(CreateLegacyRules(defaultCheckConstraint, defaultMonitorPeriod, defaultPublishPeriod));
            // save Import rules
            foreach (AlertRule rule in alertRules)
            {
                logger.LogDebug("  Loading {0}.{1}.{2} ({3})...", rule.EnvName, rule.Instance, rule.RuleName, rule.Disabled ? "Disabled" : "Enabled");
                rule.NameSpace = nameSpace;
                TimeSpan lifetime = rule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
                targetClient.SaveObject(rule, lifetime);
            }
            logger.LogDebug("Loaded {0} alert rules.", alertRules.Count);

            // load from embedded xml
            //const string Prefix = "Orion.Configuration.AlertRules";
            //Dictionary<string, string> chosenFiles = ResourceHelper.GetResources(assembly, Prefix, "xml");
            //if (chosenFiles.Count == 0)
            //    throw new InvalidOperationException("Missing alert rules!");

            //foreach (KeyValuePair<string, string> file in chosenFiles)
            //{
            //    AlertRule rule = XmlSerializerHelper.DeserializeFromString<AlertRule>(file.Value);
            //    logger.LogDebug("  Loading {0} ({1})...",
            //        file.Key,
            //        rule.Disabled ? "Disabled" : "Enabled");
            //    TimeSpan lifetime = rule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
            //    LoadConfigDataHelper.AsyncSaveObject<AlertRule>(
            //        rule, rule.UniqueKeyNetwork, rule.Properties, lifetime, false, targetClient, completions);
            //}
            //logger.LogDebug("Loaded {0} alert rules.", chosenFiles.Count);
        }

        private static IEnumerable<AlertRule> CreateDevRules(IExpression defaultCheckConstraint, TimeSpan defaultMonitorPeriod, TimeSpan defaultPublishPeriod)
        {
            const string env = "DEV";
            var alertRules
                = new List<AlertRule>
                      {
                          MakeAlertRule(
                              "QR_LIVE_Curves", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromMinutes(15))),
                              typeof (Market),
                              Expr.StartsWith("UniqueIdentifier", "Market.QR_LIVE."),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt}),
                                                        new NamedValue("DebugEnabled", true)
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              $"QR_LIVE curves ({env}) are out of date!"
                          ),

                          MakeAlertRule(
                              "CartooEODCurves", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromHours(31))),
                              typeof (Market),
                              Expr.IsEQU(Expr.SysPropItemName,
                                         "Market.EOD.RateCurve.AUD-ZERO-BANK-3M"),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              string.Format("Cartoo EOD curves ({0}) have not been published today.", env)
                              ),
                              
                          MakeAlertRule(
                              "Calendar", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromDays(8))),
                              typeof (LocationCalendarYear),
                              Expr.IsEQU(Expr.SysPropItemName,
                                         "Orion.ReferenceData.RDMHolidays.ZAJO.2060"),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              new TimeSpan(0,0,2), 
                              new TimeSpan(0,2,0),
                              $"Holiday calendars ({env}) have not been downloaded in the past week."
                          ),

                          // Check that a rule has been updated in the past 2 minutes, 
                          // this rule is updated every minute
                          MakeAlertRule(
                              "Heartbeat", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromMinutes(5))),
                              typeof (AlertSignal),
                              Expr.IsEQU(Expr.Prop("RuleName"), Expr.Const("Heartbeat")),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              TimeSpan.FromMinutes(1), 
                              TimeSpan.FromMinutes(1), 
                              "No rule status has been updated in the past 2 minutes.")
                      };

            return alertRules;
        }

        private static IEnumerable<AlertRule> CreateSitRules(IExpression defaultCheckConstraint, TimeSpan defaultMonitorPeriod, TimeSpan defaultPublishPeriod)
        {
            const string env = "SIT";

            var alertRules
                = new List<AlertRule>
                      {
                          MakeAlertRule(
                              "CartooEODCurves", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromHours(31))),
                              typeof (Market),
                              Expr.IsEQU(Expr.SysPropItemName,
                                         "Market.EOD.RateCurve.AUD-ZERO-BANK-3M"),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              string.Format("Cartoo EOD curves ({0}) have not been published today.", env)
                              ),

                          MakeAlertRule(
                              "Calendar", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromDays(8))),
                              typeof (LocationCalendarYear),
                              Expr.IsEQU(Expr.SysPropItemName,
                                         "Orion.ReferenceData.RDMHolidays.ZAJO.2060"),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              string.Format("Holiday calendars ({0}) have not been downloaded in the past week.", env)
                              ),

                          // Check that a rule has been updated in the past 2 minutes, 
                          // this rule is updated every minute
                          MakeAlertRule(
                              "Heartbeat", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromMinutes(5))),
                              typeof (AlertSignal),
                              Expr.IsEQU(Expr.Prop("RuleName"), Expr.Const("Heartbeat")),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              TimeSpan.FromMinutes(1),
                              TimeSpan.FromMinutes(1),
                              "No rule status has been updated in the past 2 minutes.")
                      };
            return alertRules;
        }

        private static IEnumerable<AlertRule> CreateStgRules(IExpression defaultCheckConstraint, TimeSpan defaultMonitorPeriod, TimeSpan defaultPublishPeriod)
        {
            const string env = "STG";

            var alertRules
                = new List<AlertRule>
                      {
                          MakeAlertRule(
                              "CartooEODCurves", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                         Expr.Const(TimeSpan.FromHours(31))),
                              typeof (Market),
                              Expr.IsEQU(Expr.SysPropItemName, "Market.EOD.RateCurve.AUD-ZERO-BANK-3M"),
                              new NamedValueSet(new[] {new NamedValue("BaseDate", new DateTime(2000, 1, 1))}),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              $"Cartoo EOD curves ({env}) have not been published today."
                          ),

                          MakeAlertRule(
                              "Trader Curves (TraderMid)", false,
                              env,
                              "AlertServer",
                              Expr.BoolAND(
                                  Expr.IsGEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Monday)),
                                  Expr.IsLEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Friday)),
                                  Expr.IsGEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(8, 30, 0))),
                                  Expr.IsLEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(16, 30, 0)))),
                              Expr.IsGTR(Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                         Expr.Const(TimeSpan.FromMinutes(10))),
                              typeof (Market),
                              Expr.IsEQU(Expr.SysPropItemName, "Market.TraderMid.RateCurve.AUD-LIBOR-BBA-6M"),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailTo",
                                                                       new[]
                                                                           {
                                                                               AWatt
                                                                           })
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              string.Format("TraderMid ({0}) curve is out of date.", env)
                              ),

                          MakeAlertRule(
                              "Trader Curves (LpmCapFloor)", false,
                              env,
                              "AlertServer",
                              Expr.BoolAND(
                                  Expr.IsGEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Monday)),
                                  Expr.IsLEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Friday)),
                                  Expr.IsGEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(8, 30, 0))),
                                  Expr.IsLEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(16, 30, 0)))),
                              Expr.IsGTR(Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                         Expr.Const(TimeSpan.FromMinutes(10))),
                              typeof (Market),
                              Expr.BoolAND(
                                  Expr.IsEQU(Expr.Prop("PricingStructureType"), Expr.Const("LpmCapFloorCurve")),
                                  Expr.IsGTR(Expr.Prop(Expr.SysPropItemCreated), Expr.FuncToday())),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailTo",
                                                                       new[]
                                                                           {
                                                                               AWatt
                                                                           })
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              $"LpmCapFloor curve ({env}) is out of date."
                          ),

                          MakeAlertRule(
                              "Trader Curves (LPMSwaption)", false,
                              env,
                              "AlertServer",
                              Expr.BoolAND(
                                  Expr.IsGEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Monday)),
                                  Expr.IsLEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Friday)),
                                  Expr.IsGEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(8, 30, 0))),
                                  Expr.IsLEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(16, 30, 0)))),
                              Expr.IsGTR(Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                         Expr.Const(TimeSpan.FromMinutes(10))),
                              typeof (Market),
                              Expr.BoolAND(
                                  Expr.IsEQU(Expr.Prop("PricingStructureType"), Expr.Const("LPMSwaptionCurve")),
                                  Expr.IsGTR(Expr.Prop(Expr.SysPropItemCreated), Expr.FuncToday())),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailTo",
                                                                       new[]
                                                                           {
                                                                               AWatt
                                                                           })
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              $"LPMSwaption curve ({env}) is out of date."
                          ),

                          MakeAlertRule(
                              "Trader Curves (Swaption)", false,
                              env,
                              "AlertServer",
                              Expr.BoolAND(
                                  Expr.IsGEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Monday)),
                                  Expr.IsLEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Friday)),
                                  Expr.IsGEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(8, 30, 0))),
                                  Expr.IsLEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(16, 30, 0)))),
                              Expr.IsGTR(Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                         Expr.Const(TimeSpan.FromMinutes(10))),
                              typeof (Market),
                              Expr.IsEQU(Expr.SysPropItemName, "Market.Live.RateATMVolatilityMatrix.AUD-LIBOR-BBA-Swaption"),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailTo",
                                                                       new[]
                                                                           {
                                                                               AWatt
                                                                           })
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              $"Swaption ({env}) curve is out of date."
                          ),

                          MakeAlertRule(
                              "Trader Curves (CapFloor)", false,
                              env,
                              "AlertServer",
                              Expr.BoolAND(
                                  Expr.IsGEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Monday)),
                                  Expr.IsLEQ(Expr.DayOfWeek(Expr.FuncToday()), Expr.Const(DayOfWeek.Friday)),
                                  Expr.IsGEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(8, 30, 0))),
                                  Expr.IsLEQ(Expr.FuncTimeOfDay(), Expr.Const(new TimeSpan(16, 30, 0)))),
                              Expr.IsGTR(Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                         Expr.Const(TimeSpan.FromMinutes(10))),
                              typeof (Market),
                              Expr.IsEQU(Expr.SysPropItemName, "Market.Live.RateVolatilityMatrix.AUD-LIBOR-BBA-CapFloor"),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailTo",
                                                                       new[]
                                                                           {
                                                                               AWatt
                                                                           })
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              $"CapFloor ({env}) curve is out of date."
                          ),

                          MakeAlertRule(
                              "Calendar", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromDays(8))),
                              typeof (LocationCalendarYear),
                              Expr.IsEQU(Expr.SysPropItemName,
                                         "Orion.ReferenceData.RDMHolidays.ZAJO.2060"),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              $"Holiday calendars ({env}) have not been downloaded in the past week."
                          ),

                          // Check that a rule has been updated in the past 2 minutes, 
                          // this rule is updated every minute
                          MakeAlertRule(
                              "Heartbeat", false,
                              env,
                              "AlertServer",
                              defaultCheckConstraint,
                              Expr.IsGTR(
                                  Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                  Expr.Const(TimeSpan.FromMinutes(2))),
                              typeof (AlertSignal),
                              Expr.IsEQU(Expr.Prop("RuleName"), Expr.Const("Heartbeat")),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("MailHost", "sydwatqur01:2525"),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              TimeSpan.FromMinutes(1), 
                              TimeSpan.FromMinutes(1), 
                              "No rule status has been updated in the past 2 minutes.")
                        };
            return alertRules;
        }

        private static IEnumerable<AlertRule> CreateLegacyRules(IExpression defaultCheckConstraint, TimeSpan defaultMonitorPeriod, TimeSpan defaultPublishPeriod)
        {
            var alertRules
                = new List<AlertRule>
                      {
                          MakeAlertRule(
                              "CartooEODCurves", true, // note: disabled
                              "DEV",
                              "LegacyAlertServer", // <-- note: instance name
                              defaultCheckConstraint,
                              Expr.IsGTR(Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                                         Expr.Const(TimeSpan.FromHours(31))),
                              typeof (Market),
                              Expr.IsEQU(Expr.Prop("UniqueIdentifier"),
                                         Expr.Const("Market.EOD.RateCurve.AUD-ZERO-BANK-3M")),
                              new NamedValueSet(new[]
                                                    {
                                                        new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                                                        new NamedValue("MailTo", new[] {AWatt})
                                                    }),
                              defaultMonitorPeriod,
                              defaultPublishPeriod,
                              "DEV (Legacy) EOD Curves not published"
                              )

                      };
            return alertRules;
        }
    }
}
