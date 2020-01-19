/*
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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.WinTools;

namespace Highlander.Core.NetMon
{
    public partial class NetMonForm : Form
    {
        private static readonly EnvId BuildEnv = EnvHelper.ParseEnvName(BuildConst.BuildEnv);

        private const string ApplName = "Highlander.NetMon";

        private Reference<ILogger> _loggerRef;
        private CoreClientFactory _clientFactory;
        private ICoreClient _client;

        private IListViewHelper<AlertRule> _alertRuleView;
        private IViewHelper _alertRuleViewHelper;
        private IDataHelper<AlertRule> _alertRuleDataHelper;
        private ISelecter<AlertRule> _alertRuleSelecter;
        private IFilterGroup _alertRuleFilters;

        private IListViewHelper<AlertSignal> _alertSignalView;
        private IViewHelper _alertSignalViewHelper;
        private IDataHelper<AlertSignal> _alertSignalDataHelper;
        private ISelecter<AlertSignal> _alertSignalSelecter;
        private IFilterGroup _alertSignalFilters;

        private IListViewHelper<DebugLogEvent> _logEventView;
        private IViewHelper _logEventViewHelper;
        private IDataHelper<DebugLogEvent> _logEventDataHelper;
        private ISelecter<DebugLogEvent> _logEventSelecter;
        private IFilterGroup _logEventFilters;

        private const int NServers = 6;
        private readonly CheckBox[] _ping = new CheckBox[NServers];
        private readonly TextBox[] _serverAddress = new TextBox[NServers];
        private readonly TextBox[] _lastChecked = new TextBox[NServers];
        private readonly TextBox[] _lastReplied = new TextBox[NServers];
        private readonly TextBox[] _serverStatus = new TextBox[NServers];
        private readonly TextBox[] _serverReason = new TextBox[NServers];

        public NetMonForm()
        {
            InitializeComponent();
        }

        private void Form1Load(object sender, EventArgs e)
        {
            // create loggers
            _loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtMainLog));
            // create client factory and client
            _clientFactory = new CoreClientFactory(_loggerRef);
            _clientFactory.SetEnv(BuildConst.BuildEnv);
            _clientFactory.SetApplication(Assembly.GetExecutingAssembly());
            _client = _clientFactory.Create();
            // - form title
            WinFormHelper.SetAppFormTitle(this, EnvHelper.EnvName(BuildEnv));
            // setup the AlertRule view
            _alertRuleViewHelper = new AlertRuleViewHelper();
            _alertRuleDataHelper = new AlertRuleDataHelper();
            _alertRuleFilters = new ComboxBoxFilterGroup(
                panelAlertRule, _alertRuleViewHelper, AlertRuleSelectionChanged);
            _alertRuleSelecter = new AlertRuleSelecter(
                _alertRuleFilters, _alertRuleViewHelper, _alertRuleDataHelper);
            _alertRuleView = new ListViewManager<AlertRule>(
                _loggerRef.Target, lvAlertRule, _alertRuleViewHelper,
                _alertRuleSelecter, _alertRuleFilters, new AlertRuleSorter(), _alertRuleDataHelper);

            ICoreCache alertRuleCache = _client.CreateCache(
                update => _alertRuleView.UpdateData(new ViewChangeNotification<AlertRule>
                                                        {
                                                            Change = update.Change,
                                                            OldData =
                                                                (AlertRule) update.OldItem?.Data,
                                                            NewData =
                                                                (AlertRule) update.NewItem?.Data
                                                        }), SynchronizationContext.Current);
            alertRuleCache.SubscribeNoWait<AlertRule>(Expr.ALL, null, null);
            // setup the AlertSignal view
            _alertSignalViewHelper = new AlertSignalViewHelper();
            _alertSignalDataHelper = new AlertSignalDataHelper();
            _alertSignalFilters = new ComboxBoxFilterGroup(
                panelAlertSignal, _alertSignalViewHelper, AlertSignalSelectionChanged);
            _alertSignalSelecter = new AlertSignalSelecter(
                _alertSignalFilters, _alertSignalViewHelper, _alertSignalDataHelper);
            _alertSignalView = new ListViewManager<AlertSignal>(
                _loggerRef.Target, lvAlertSignal, _alertSignalViewHelper,
                _alertSignalSelecter, _alertSignalFilters, new AlertSignalSorter(), _alertSignalDataHelper);
            ICoreCache alertSignalCache = _client.CreateCache(
                update => _alertSignalView.UpdateData(new ViewChangeNotification<AlertSignal>
                                                          {
                                                              Change = update.Change,
                                                              OldData =
                                                                  (AlertSignal) update.OldItem?.Data,
                                                              NewData =
                                                                  (AlertSignal) update.NewItem?.Data
                                                          }), SynchronizationContext.Current);
            alertSignalCache.SubscribeNoWait<AlertSignal>(Expr.ALL, null, null);
            // setup the LogEvent view
            _logEventViewHelper = new LogEventViewHelper();
            _logEventDataHelper = new LogEventDataHelper();
            _logEventFilters = new ComboxBoxFilterGroup(
                panelLogEvent, _logEventViewHelper, LogEventSelectionChanged);
            _logEventSelecter = new LogEventSelecter(
                _logEventFilters, _logEventViewHelper, _logEventDataHelper);
            _logEventView = new ListViewManager<DebugLogEvent>(
                _loggerRef.Target, lvLogEvent, _logEventViewHelper,
                _logEventSelecter, _logEventFilters, new LogEventSorter(), _logEventDataHelper);
            ICoreCache logEventCache = _client.CreateCache(
                update => _logEventView.UpdateData(new ViewChangeNotification<DebugLogEvent>
                                                       {
                                                           Change = update.Change,
                                                           OldData =
                                                               (DebugLogEvent) update.OldItem?.Data,
                                                           NewData =
                                                               (DebugLogEvent) update.NewItem?.Data
                                                       }), SynchronizationContext.Current);
            logEventCache.SubscribeNoWait<DebugLogEvent>(Expr.ALL, null, null);
            // init controls
            // server 0
            _serverAddress[0] = txtServer0Address;
            _ping[0] = chkServer0Ping;
            _lastChecked[0] = txtServer0LastChecked;
            _lastReplied[0] = txtServer0LastReplied;
            _serverStatus[0] = txtServer0Status;
            _serverReason[0] = txtServer0OtherInfo;
            // server 1
            _serverAddress[1] = txtServer1Address;
            _ping[1] = chkServer1Ping;
            _lastChecked[1] = txtServer1LastChecked;
            _lastReplied[1] = txtServer1LastReplied;
            _serverStatus[1] = txtServer1Status;
            _serverReason[1] = txtServer1OtherInfo;
            // server 2
            _serverAddress[2] = txtServer2Address;
            _ping[2] = chkServer2Ping;
            _lastChecked[2] = txtServer2LastChecked;
            _lastReplied[2] = txtServer2LastReplied;
            _serverStatus[2] = txtServer2Status;
            _serverReason[2] = txtServer2OtherInfo;
            // server 3
            _serverAddress[3] = txtServer3Address;
            _ping[3] = chkServer3Ping;
            _lastChecked[3] = txtServer3LastChecked;
            _lastReplied[3] = txtServer3LastReplied;
            _serverStatus[3] = txtServer3Status;
            _serverReason[3] = txtServer3OtherInfo;
            // server 4
            _serverAddress[4] = txtServer4Address;
            _ping[4] = chkServer4Ping;
            _lastChecked[4] = txtServer4LastChecked;
            _lastReplied[4] = txtServer4LastReplied;
            _serverStatus[4] = txtServer4Status;
            _serverReason[4] = txtServer4OtherInfo;
            // server 5
            _serverAddress[5] = txtServer5Address;
            _ping[5] = chkServer5Ping;
            _lastChecked[5] = txtServer5LastChecked;
            _lastReplied[5] = txtServer5LastReplied;
            _serverStatus[5] = txtServer5Status;
            _serverReason[5] = txtServer5OtherInfo;
            for (int i = 0; i < NServers; i++)
            {
                _lastChecked[i].BackColor = Color.FromKnownColor(KnownColor.Window);
                _lastReplied[i].BackColor = Color.FromKnownColor(KnownColor.Window);
                _serverStatus[i].BackColor = Color.FromKnownColor(KnownColor.Window);
                _serverReason[i].BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }

        void AlertRuleSelectionChanged(object sender, EventArgs e)
        {
            _alertRuleView.RebuildView();
        }

        void AlertSignalSelectionChanged(object sender, EventArgs e)
        {
            _alertSignalView.RebuildView();
        }

        void LogEventSelectionChanged(object sender, EventArgs e)
        {
            _logEventView.RebuildView();
        }

        private void BtnServerClearFiltersClick(object sender, EventArgs e)
        {
            foreach (Control control in ((Control)sender).Parent.Controls)
            {
                if (control is ComboBox combo)
                {
                    combo.SelectedIndex = 0;
                }
            }
        }

        private void BtnNodesClearFiltersClick(object sender, EventArgs e)
        {
            foreach (Control control in ((Control)sender).Parent.Controls)
            {
                if (control is ComboBox combo)
                {
                    combo.SelectedIndex = 0;
                }
            }
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            _client.Dispose();
        }

        private void BtnPingAllClick(object sender, EventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            long t0 = sw.ElapsedMilliseconds;
            _loggerRef.Target.LogInfo("Ping: Starting...");
            for (int serverN = 0; serverN < NServers; serverN++)
            {
                if (_ping[serverN].Checked)
                {
                    _lastChecked[serverN].Text = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                    _lastReplied[serverN].Text = "Connecting...";
                    _serverStatus[serverN].Text = "Connecting...";
                    _serverReason[serverN].Text = "Connecting...";
                    string clientResult = "Undefined!";
                    string clientReason = "";
                    Color clientColour;
                    string activity = "Initialising";
                    long clientT0 = sw.ElapsedMilliseconds;
                    try
                    {
                        activity = "Resolving address";
                        string[] addrParts = _serverAddress[serverN].Text.Split(';');
                        if (addrParts.Length != 2)
                            throw new ApplicationException("Address not in format 'env;host[:port]'");
                        string env = addrParts[0];
                        string serverAddress = addrParts[1];
                        activity = "Connecting";
                        using (_clientFactory.SetEnv(env).SetServers(serverAddress).Create())
                        {
                            // connect done
                            activity = "Loading";
                        }
                        long clientTZ = sw.ElapsedMilliseconds;
                        TimeSpan clientTotal = TimeSpan.FromMilliseconds(clientTZ - clientT0);
                        _lastReplied[serverN].Text = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                        clientResult = $"{clientTotal.TotalMilliseconds}ms";
                        clientColour = clientTotal.TotalMilliseconds <= 1000.0 ? Color.LightGreen : Color.Orange;
                        activity = "Disposing";
                    }
                    catch (Exception excp)
                    {
                        _loggerRef.Target.LogDebug("{0}: FAILED while {1}: {2} {3}",
                            _serverAddress[serverN].Text, activity, excp.GetType().Name, excp.Message);
                        clientResult = "FAILED";
                        clientReason = excp.Message;
                        clientColour = Color.OrangeRed;
                        _loggerRef.Target.Log(excp);
                    }
                    //clientTZ = sw.ElapsedMilliseconds;
                    //TimeSpan durationClient = TimeSpan.FromMilliseconds(clientTZ - clientT0);
                    _serverStatus[serverN].Text = clientResult;
                    _serverStatus[serverN].BackColor = clientColour;
                    _serverReason[serverN].Text = clientReason;
                }
            }
            long tZ = sw.ElapsedMilliseconds;
            sw.Stop();
            TimeSpan duration = TimeSpan.FromMilliseconds(tZ - t0);
            _loggerRef.Target.LogInfo("Ping: Completed ({0}s)", duration.TotalSeconds);
        }
    }

    public enum AllEventColumn
    {
        DataType,
        ItemName,
        ApplName,
        HostName,
        UserName,
        OrigEnv,
        Created,
        Expires
    }

    public class AllEventObj
    {
        public string DataType { get; set; }
        public string ItemName { get; set; }
        public string ApplName { get; set; }
        public string HostName { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Expires { get; set; }
        public string OrgEnvId { get; set; }
    }

    internal class AllEventViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(AllEventColumn)).Length;

        public string GetColumnTitle(int column)
        {
            return ((AllEventColumn)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((AllEventColumn)column)
            {
                case AllEventColumn.DataType: return true;
                case AllEventColumn.ItemName: return false;
                case AllEventColumn.ApplName: return true;
                case AllEventColumn.HostName: return true;
                case AllEventColumn.UserName: return true;
                case AllEventColumn.OrigEnv: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class AllEventDataHelper : IDataHelper<AllEventObj>
    {
        public string GetUniqueKey(AllEventObj data)
        {
            return data.ItemName;
        }

        //private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        //{
        //    var delims = new char[1] { delim };
        //    if (input == null)
        //        return defaultValue;
        //    string[] parts = input.Split(delims, StringSplitOptions.RemoveEmptyEntries);
        //    if (minIndex >= parts.Length)
        //        return defaultValue;
        //    int index = minIndex;
        //    var result = new StringBuilder();
        //    while ((index < parts.Length) && (index <= maxIndex))
        //    {
        //        if (index > minIndex)
        //            result.Append(delim);
        //        result.Append(parts[index]);
        //        index++;
        //    }
        //    return result.ToString();
        //}
        public string GetDisplayValue(AllEventObj data, int column)
        {
            switch ((AllEventColumn)column)
            {
                case AllEventColumn.DataType: return data.DataType;
                case AllEventColumn.ItemName: return data.ItemName;
                case AllEventColumn.ApplName: return data.ApplName;
                case AllEventColumn.HostName: return data.HostName;
                case AllEventColumn.UserName: return data.UserName;
                case AllEventColumn.Created: return data.Created.ToString("g");
                case AllEventColumn.Expires: return data.Expires.ToString("g");
                case AllEventColumn.OrigEnv: return data.OrgEnvId;
                default: return null;
            }
        }
    }

    internal class AllEventSorter : IComparer<AllEventObj>
    {
        public int Compare(AllEventObj a, AllEventObj b)
        {
            // sort order column priority
            const int result = 0;

            // descending create time
            if (b != null && (a != null && a.Created < b.Created))
                return 1;
            if (b != null && (a != null && a.Created > b.Created))
                return -1;

            return result;

        }
    }

    public class AllEventSelecter : BaseSelecter<AllEventObj>
    {
        // this class is currently is a placeholder for future selection rules
        public AllEventSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<AllEventObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // CacheStats

    public enum CacheStatsColumn
    {
        HostName,
        UserWNId,
        ApplName,
        NodeId,
        OrigEnv,
        RTVersion,
        RTPubKey,
        Updated,
        ItemCount
    }

    public class CacheStatsObj
    {
        public string HostName { get; set; }
        public string UserWNId { get; set; }
        public string ApplName { get; set; }
        public Guid NodeId { get; set; }
        public string OrigEnv { get; set; }
        public string RTVersion { get; set; }
        public string RTPubKey { get; set; }
        public DateTimeOffset Updated { get; set; }
        public NamedValueSet Counters { get; set; }
        public string ItemName => $"CacheStats.{(HostName ?? "(null-host)").Replace('.', '_')}.{(UserWNId ?? "(null-user)").Replace('.', '_')}.{(ApplName ?? "(null-appl)").Replace('.', '_')}.{NodeId.ToString()}";

        public CacheStatsObj(ICoreItem item)
        {
            var temp = (DebugCacheStats)item.Data;
            HostName = temp.HostName;
            UserWNId = temp.UserWNId;
            ApplName = temp.ApplName;
            NodeId = new Guid(temp.NodeId);
            OrigEnv = temp.OrigEnv;
            RTVersion = temp.RTVersion;
            RTPubKey = temp.RTPubKey;
            Updated = DateTimeOffset.Parse(temp.Updated);
            Counters = new NamedValueSet(temp.Counters);
        }
    }

    internal class CacheStatsViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(CacheStatsColumn)).Length;

        public string GetColumnTitle(int column)
        {
            return ((CacheStatsColumn)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((CacheStatsColumn)column)
            {
                case CacheStatsColumn.ApplName: return true;
                case CacheStatsColumn.HostName: return true;
                case CacheStatsColumn.UserWNId: return true;
                case CacheStatsColumn.OrigEnv: return true;
                case CacheStatsColumn.RTVersion: return true;
                case CacheStatsColumn.RTPubKey: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class CacheStatsDataHelper : IDataHelper<CacheStatsObj>
    {
        public string GetColumnTitle(int column)
        {
            return ((CacheStatsColumn)column).ToString();
        }
        public string GetUniqueKey(CacheStatsObj data)
        {
            return data.ItemName;
        }
        //private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        //{
        //    var delims = new char[1] { delim };
        //    if (input == null)
        //        return defaultValue;
        //    string[] parts = input.Split(delims, StringSplitOptions.RemoveEmptyEntries);
        //    if (minIndex >= parts.Length)
        //        return defaultValue;
        //    int index = minIndex;
        //    var result = new StringBuilder();
        //    while ((index < parts.Length) && (index <= maxIndex))
        //    {
        //        if (index > minIndex)
        //            result.Append(delim);
        //        result.Append(parts[index]);
        //        index++;
        //    }
        //    return result.ToString();
        //}
        public string GetDisplayValue(CacheStatsObj data, int column)
        {
            switch ((CacheStatsColumn)column)
            {
                case CacheStatsColumn.ApplName: return data.ApplName;
                case CacheStatsColumn.HostName: return data.HostName;
                case CacheStatsColumn.UserWNId: return data.UserWNId;
                case CacheStatsColumn.NodeId: return data.NodeId.ToString();
                case CacheStatsColumn.OrigEnv: return data.OrigEnv;
                case CacheStatsColumn.RTVersion: return data.RTVersion;
                case CacheStatsColumn.RTPubKey: return data.RTPubKey;
                case CacheStatsColumn.Updated: return data.Updated.ToLocalTime().ToString("g");
                case CacheStatsColumn.ItemCount: return data.Counters.GetValue("Items.Cached.Current", -1).ToString(CultureInfo.InvariantCulture);
                default: return null;
            }
        }
    }

    internal class CacheStatsSorter : IComparer<CacheStatsObj>
    {
        public int Compare(CacheStatsObj a, CacheStatsObj b)
        {
            // sort order column priority
            const int result = 0;

            // descending update time
            if (b != null && (a != null && a.Updated < b.Updated))
                return 1;
            if (b != null && (a != null && a.Updated > b.Updated))
                return -1;

            return result;

        }
    }

    public class CacheStatsSelecter : BaseSelecter<CacheStatsObj>
    {
        // this class is currently is a placeholder for future selection rules
        public CacheStatsSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<CacheStatsObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // LogEvent

    public enum LogEventColumn
    {
        HostName,
        UserName,
        ApplName,
        OrigEnv,
        Created,
        ProcId,
        SeqNum,
        Severity,
        Message
    }

    internal class LogEventViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(LogEventColumn)).Length;

        public string GetColumnTitle(int column)
        {
            return ((LogEventColumn)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((LogEventColumn)column)
            {
                case LogEventColumn.ApplName: return true;
                case LogEventColumn.HostName: return true;
                case LogEventColumn.UserName: return true;
                case LogEventColumn.OrigEnv: return true;
                case LogEventColumn.ProcId: return true;
                case LogEventColumn.Severity: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class LogEventDataHelper : IDataHelper<DebugLogEvent>
    {
        public string GetColumnTitle(int column)
        {
            return ((LogEventColumn)column).ToString();
        }
        public string GetUniqueKey(DebugLogEvent data)
        {
            return data.ItemName;
        }
        //private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        //{
        //    var delims = new char[1] { delim };
        //    if (input == null)
        //        return defaultValue;
        //    string[] parts = input.Split(delims, StringSplitOptions.RemoveEmptyEntries);
        //    if (minIndex >= parts.Length)
        //        return defaultValue;
        //    int index = minIndex;
        //    var result = new StringBuilder();
        //    while ((index < parts.Length) && (index <= maxIndex))
        //    {
        //        if (index > minIndex)
        //            result.Append(delim);
        //        result.Append(parts[index]);
        //        index++;
        //    }
        //    return result.ToString();
        //}
        public string GetDisplayValue(DebugLogEvent data, int column)
        {
            switch ((LogEventColumn)column)
            {
                case LogEventColumn.ApplName: return data.ApplName;
                case LogEventColumn.HostName: return data.HostName;
                case LogEventColumn.UserName: return data.UserName;
                case LogEventColumn.OrigEnv: return data.OrigEnv;
                case LogEventColumn.Created: return DateTimeOffset.Parse(data.Created).ToLocalTime().ToString("g");
                case LogEventColumn.ProcId: return data.ProcessId.ToString(CultureInfo.InvariantCulture);
                case LogEventColumn.SeqNum: return data.SeqNumber.ToString(CultureInfo.InvariantCulture);
                case LogEventColumn.Severity: return LoggerHelper.SeverityName(data.Severity);
                case LogEventColumn.Message: return data.Message;
                default: return null;
            }
        }
    }

    internal class LogEventSorter : IComparer<DebugLogEvent>
    {
        public int Compare(DebugLogEvent a, DebugLogEvent b)
        {
            // sort order column priority

            // ascending created dateTime
            DateTimeOffset aCreated = DateTimeOffset.Parse(a.Created);
            DateTimeOffset bCreated = DateTimeOffset.Parse(b.Created);
            int result = DateTimeOffset.Compare(aCreated, bCreated);
            if (result != 0)
                return result;

            // ascending sequence
            if (a.SeqNumber > b.SeqNumber)
                return 1;
            if (a.SeqNumber < b.SeqNumber)
                return -1;

            return result;

        }
    }

    public class LogEventSelecter : BaseSelecter<DebugLogEvent>
    {
        // this class is currently is a placeholder for future selection rules
        public LogEventSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<DebugLogEvent> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // AlertRule

    public enum AlertRuleColumn
    {
        RuleName,
        Disabled,
        Condition,
        Constraint,
        DataItemKind,
        DataSubsExpr,   // IExpression
        Properties,     // NamedValueSet
        MonitorPeriod,  // TimeSpan
        PublishPeriod,  // TimeSpan
        SignalFormat
    }

    internal class AlertRuleViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(AlertRuleColumn)).Length;

        public string GetColumnTitle(int column)
        {
            return ((AlertRuleColumn)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((AlertRuleColumn)column)
            {
                case AlertRuleColumn.RuleName: return true;
                case AlertRuleColumn.Disabled: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class AlertRuleDataHelper : IDataHelper<AlertRule>
    {
        public string GetColumnTitle(int column)
        {
            return ((AlertRuleColumn)column).ToString();
        }
        public string GetUniqueKey(AlertRule data)
        {
            return data.PrivateKey;
        }
        //private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        //{
        //    var delims = new char[1] { delim };
        //    if (input == null)
        //        return defaultValue;
        //    string[] parts = input.Split(delims, StringSplitOptions.RemoveEmptyEntries);
        //    if (minIndex >= parts.Length)
        //        return defaultValue;
        //    int index = minIndex;
        //    var result = new StringBuilder();
        //    while ((index < parts.Length) && (index <= maxIndex))
        //    {
        //        if (index > minIndex)
        //            result.Append(delim);
        //        result.Append(parts[index]);
        //        index++;
        //    }
        //    return result.ToString();
        //}
        public string GetDisplayValue(AlertRule data, int column)
        {
            switch ((AlertRuleColumn)column)
            {
                case AlertRuleColumn.RuleName: return data.RuleUniqueId;
                case AlertRuleColumn.Disabled: return data.Disabled.ToString();
                case AlertRuleColumn.DataSubsExpr: return Expr.Display(Expr.Deserialise(data.DataSubsExpr));
                case AlertRuleColumn.DataItemKind: return data.DataItemKind;
                case AlertRuleColumn.Properties: return data.AlertProperties;
                case AlertRuleColumn.Condition: return Expr.Display(Expr.Deserialise(data.Condition));
                case AlertRuleColumn.Constraint: return Expr.Display(Expr.Deserialise(data.Constraint));
                case AlertRuleColumn.MonitorPeriod: return data.MonitorPeriod;
                case AlertRuleColumn.PublishPeriod: return data.PublishPeriod;
                case AlertRuleColumn.SignalFormat: return data.SignalFormat;
                default: return null;
            }
        }
    }

    internal class AlertRuleSorter : IComparer<AlertRule>
    {
        public int Compare(AlertRule a, AlertRule b)
        {
            // sort order column priority

            // ascending RuleName
            int result = String.Compare(a.RuleUniqueId, b.RuleUniqueId, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
                return result;

            return result;

        }
    }

    public class AlertRuleSelecter : BaseSelecter<AlertRule>
    {
        // this class is currently is a placeholder for future selection rules
        public AlertRuleSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<AlertRule> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // AlertSignal

    public enum AlertSignalColumn
    {
        RuleName,
        AlertServer,
        Status,
        LastCheck,  // TimeSpan
        Message
    }

    internal class AlertSignalViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(AlertSignalColumn)).Length;

        public string GetColumnTitle(int column)
        {
            return ((AlertSignalColumn)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((AlertSignalColumn)column)
            {
                case AlertSignalColumn.RuleName: return true;
                case AlertSignalColumn.AlertServer: return true;
                case AlertSignalColumn.Status: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class AlertSignalDataHelper : IDataHelper<AlertSignal>
    {
        public string GetColumnTitle(int column)
        {
            return ((AlertSignalColumn)column).ToString();
        }
        public string GetUniqueKey(AlertSignal data)
        {
            return data.ItemName;
        }
        //private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        //{
        //    var delims = new char[1] { delim };
        //    if (input == null)
        //        return defaultValue;
        //    string[] parts = input.Split(delims, StringSplitOptions.RemoveEmptyEntries);
        //    if (minIndex >= parts.Length)
        //        return defaultValue;
        //    int index = minIndex;
        //    var result = new StringBuilder();
        //    while ((index < parts.Length) && (index <= maxIndex))
        //    {
        //        if (index > minIndex)
        //            result.Append(delim);
        //        result.Append(parts[index]);
        //        index++;
        //    }
        //    return result.ToString();
        //}
        public string GetDisplayValue(AlertSignal data, int column)
        {
            switch ((AlertSignalColumn)column)
            {
                case AlertSignalColumn.RuleName: return data.RuleUniqueId;
                case AlertSignalColumn.AlertServer: return data.AlertServer;
                case AlertSignalColumn.Status: return data.Status;
                case AlertSignalColumn.LastCheck: return DateTimeOffset.Parse(data.LastMonitored).ToString();
                case AlertSignalColumn.Message: return data.SignalMessage;
                default: return null;
            }
        }
    }

    internal class AlertSignalSorter : IComparer<AlertSignal>
    {
        public int Compare(AlertSignal a, AlertSignal b)
        {
            // sort order column priority
            // ascending RuleName
            int result = String.Compare(a.RuleUniqueId, b.RuleUniqueId, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
                return result;

            return result;

        }
    }

    public class AlertSignalSelecter : BaseSelecter<AlertSignal>
    {
        // this class is currently is a placeholder for future selection rules
        public AlertSignalSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<AlertSignal> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

}
