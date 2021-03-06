﻿/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.Core.V34;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Servers;

namespace Highlander.Core.SvcGui
{
    public partial class CoreTestForm : Form
    {
        private Reference<ILogger> _coreLogger;
        private IBasicServer _coreServer;
        private readonly string[] _environments = { "UTT", "DEV", "SIT", "STG", "PRD" };

        public CoreTestForm()
        {
            InitializeComponent();
        }

        private void Form1Load(object sender, EventArgs e)
        {
            // create logger
            _coreLogger = Reference<ILogger>.Create(new MultiLogger(
                new TextBoxLogger(txtCoreSvrLog),
                new FileLogger(@"C:\_highlander\logs\CoreSvcGui.{dddd}.log")));
            // init controls
            foreach (var env in _environments)
                cbEnvironment.Items.Add(env);
            cbEnvironment.SelectedIndex = 0;
            foreach (NodeType mode in Enum.GetValues(typeof(NodeType)))
                cbServerMode.Items.Add(mode.ToString());
            cbServerMode.SelectedIndex = (int)NodeType.Router;
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanUp();
        }

        private void BtnStartClick(object sender, EventArgs e)
        {
            StartUp();
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            CleanUp();
        }

        private void StartUp()
        {
            // start the service
            var env = _environments[cbEnvironment.SelectedIndex];
            var settings = new NamedValueSet();
            settings.Set(CfgPropName.NodeType, cbServerMode.SelectedIndex);
            settings.Set(CfgPropName.EnvName, env);
            settings.Set(CfgPropName.DbServer, txtDbCfg.Text);
            DisposeHelper.SafeDispose(ref _coreServer);
            _coreServer = new CoreServer(_coreLogger, settings);
            _coreServer.Start();
        }

        private void CleanUp()
        {
            DisposeHelper.SafeDispose(ref _alertServer);
            DisposeHelper.SafeDispose(ref _coreServer);
            DisposeHelper.SafeDispose(ref _coreLogger);
        }

        // ------------------------------ AlertServer ------------------------------
        private IServerBase2 _alertServer;

        private void BtnAlertSvrStopClick(object sender, EventArgs e)
        {
            DisposeHelper.SafeDispose(ref _alertServer);
        }

        private void BtnAlertSvrStartClick(object sender, EventArgs e)
        {
            // start the service
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtAlertSvrLog)))
            {
                var env = _environments[cbEnvironment.SelectedIndex];
                try
                {
                    Reference<ICoreClient> clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(loggerRef).SetEnv(env).Create());//BuildConst.BuildEnv
                    _alertServer = new AlertServer.AlertServer
                                       {
                                           LoggerRef = loggerRef,
                                           Client = clientRef,
                                           HostInstance = null,
                                           OtherSettings = clientRef.Target.LoadAppSettings("AlertServer")
                                       };
                    _alertServer.Start();
                }
                catch (Exception ex)
                {
                    loggerRef.Target.Log(ex);
                }
            }
        }

        private UserDetail MakeUserDetail(string userName, string fullName, string emailAddress)
        {
            return new UserDetail
                       {
                UserName = userName,
                FullName = fullName,
                EmailAddress = emailAddress
            };
        }

        private UserGroup MakeUserGroup(string groupName, string fullName, IEnumerable<string> memberUsers, IEnumerable<string> memberGroups)
        {
            return new UserGroup
                       {
                GroupName = groupName,
                FullName = fullName,
                MemberUsers = memberUsers?.ToArray(),
                MemberGroups = memberGroups?.ToArray()
            };
        }

        private AlertRule MakeAlertRule(
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
                RuleUniqueId = ruleName,
                Disabled = disabled,
                HostEnvAbbr = hostEnvName,
                HostInstance = hostInstance,
                Constraint = constraintExpr.Serialise(),
                Condition = conditionExpr.Serialise(),
                DataTypeName = dataType.FullName,
                DataSubsExpr = dataSubsExpr.Serialise(),
                AlertProperties = alertProps.Serialise(),
                MonitorPeriod = monitorPeriod.ToString(),
                PublishPeriod = publishPeriod.ToString(),
                SignalFormat = signalFormat,
                HostComputer = null,
                HostUserName = null,
                DataItemKind = ItemKind.Object.ToString()
            };
        }

        private IEnumerable<AlertRule> CreateDevRules(IExpression defaultCheckConstraint, TimeSpan defaultMonitorPeriod, TimeSpan defaultPublishPeriod)
        {
            const string env = "DEV";
            var alertRules
                = new List<AlertRule>
                      {
                          //MakeAlertRule(
                          //    "QR_LIVE_Curves", false,
                          //    Env,
                          //    "AlertServer",
                          //    defaultCheckConstraint,
                          //    Expr.IsGTR(
                          //        Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                          //        Expr.Const(TimeSpan.FromMinutes(15))),
                          //    typeof (Market),
                          //    Expr.StartsWith("UniqueIdentifier", "Highlander.Market.QR_LIVE."),
                          //    new NamedValueSet(new[]
                          //                          {
                          //                              new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                          //                              new NamedValue(AlertRule.MailHost, "sydwatqur01:2525"),
                          //                              new NamedValue(AlertRule.MailTo, new[] {SDudley}),
                          //                              new NamedValue("DebugEnabled", true)
                          //                          }),
                          //    defaultMonitorPeriod,
                          //    defaultPublishPeriod,
                          //    string.Format("QR_LIVE curves ({0}) are out of date!", Env)
                          //    ),
                          //MakeAlertRule(
                          //    "CartooEODCurves", false,
                          //    Env,
                          //    "AlertServer",
                          //    defaultCheckConstraint,
                          //    Expr.IsGTR(
                          //        Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                          //        Expr.Const(TimeSpan.FromHours(31))),
                          //    typeof (Market),
                          //    Expr.IsEQU(Expr.SysPropItemName,
                          //               "Highlander.Market.EOD.RateCurve.AUD-ZERO-BANK-3M"),
                          //    new NamedValueSet(new[]
                          //                          {
                          //                              new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                          //                              new NamedValue(AlertRule.MailHost, "sydwatqur01:2525"),
                          //                              new NamedValue(AlertRule.MailTo, new[] { AlertRule.User_SDudley}),
                          //                          }),
                          //    defaultMonitorPeriod,
                          //    defaultPublishPeriod,
                          //    string.Format("Cartoo EOD curves ({0}) have not been published today.", Env)
                          //    ),
                          //MakeAlertRule(
                          //    "Calendar", false,
                          //    Env,
                          //    "AlertServer",
                          //    defaultCheckConstraint,
                          //    Expr.IsGTR(
                          //        Expr.NumSUB(Expr.FuncNow(), Expr.Prop(Expr.SysPropItemCreated)),
                          //        Expr.Const(TimeSpan.FromDays(8))),
                          //    typeof (LocationCalendarYear),
                          //    Expr.IsEQU(Expr.SysPropItemName,
                          //               "Highlander.ReferenceData.RDMHolidays.ZAJO.2060"),
                          //    new NamedValueSet(new[]
                          //                          {
                          //                              new NamedValue("BaseDate", new DateTime(2000, 1, 1)),
                          //                              new NamedValue(AlertRule.MailHost, "sydwatqur01:2525"),
                          //                              new NamedValue(AlertRule.MailTo, new[] {SDudley}),
                          //                          }),
                          //    new TimeSpan(0,0,2), 
                          //    new TimeSpan(0,2,0), 
                          //    string.Format("Holiday calendars ({0}) have not been downloaded in the past week.", Env)
                          //    ),
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
                              new NamedValueSet(new NamedValue[]
                                                    {
                                                        //new NamedValue(AlertRule.MailHost, "sydwatqur01:2525"),
                                                        //new NamedValue(AlertRule.MailTo, new string[] { Group_HL_Developers }),
                                                    }),
                              TimeSpan.FromMinutes(1), 
                              TimeSpan.FromMinutes(1), 
                              "No rule status has been updated in the past 2 minutes.")
                      };
            return alertRules;
        }

        private void BtnDebugLoadAlertRulesClick(object sender, EventArgs e)
        {
            var env = _environments[cbEnvironment.SelectedIndex];
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtDebugLog)))
            {
                loggerRef.Target.LogDebug("Loading alert rules...");
                using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv(env).Create())//BuildConst.BuildEnv
                {
                    // load test user details
                    //client.SaveObject<UserDetail>(MakeUserDetail(User_MLim2, "Mark Lim", "mark.lim@nab.com.au"));
                    //client.SaveObject<UserDetail>(MakeUserDetail(User_SDudley, "Simon Dudley", "simon.dudley@nab.com.au"));
                    client.SaveObject(MakeUserDetail(AlertRule.UserDefaultReplyTo, "Default ReplyTo User", "alexwatt@hotmail.com"));
                    client.SaveObject(MakeUserGroup(AlertRule.GroupDefaultMailTo, "Default MailTo Group", new[] { AlertRule.UserDefaultReplyTo }, null));
                    //client.SaveObject<UserGroup>(MakeUserGroup(Group_HL_Developers, "HL Developers Group", new string[] { User_SDudley, User_MLim2 }, null));
                    //client.SaveObject<UserGroup>(MakeUserGroup(Group_HL_Developers, "HL Developers Group", new string[] { User_SDudley }, null));
                    // defaults
                    // - check constraint: time of day is between 4:30am and 8:30pm 7 days a week
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
                    //alertRules.AddRange(CreateSitRules(defaultCheckConstraint, defaultMonitorPeriod, defaultPublishPeriod));
                    //alertRules.AddRange(CreateStgRules(defaultCheckConstraint, defaultMonitorPeriod, defaultPublishPeriod));
                    //alertRules.AddRange(CreateLegacyRules(defaultCheckConstraint, defaultMonitorPeriod, defaultPublishPeriod));
                    // save Import rules
                    foreach (AlertRule rule in alertRules)
                    {
                        loggerRef.Target.LogDebug("  Loading AlertRule: {0}.{1}.{2} ({3})...", rule.HostEnvAbbr, rule.HostInstance, rule.RuleUniqueId, rule.Disabled ? "Disabled" : "Enabled");
                        TimeSpan lifetime = rule.Disabled ? TimeSpan.FromDays(30) : TimeSpan.MaxValue;
                        client.SaveObject(rule, lifetime);
                    }
                    loggerRef.Target.LogDebug("Loaded {0} alert rules.", alertRules.Count);
                }
            }
        }
    }
}
