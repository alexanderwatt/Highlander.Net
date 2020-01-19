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
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Threading;
using Highlander.Workflow.CurveGeneration.V5r3;
using Highlander.Workflow.Server.V5r3;
using Exception = System.Exception;

namespace Highlander.Workflow.Server.TestHost.V5r3
{
    public partial class WorkflowForm : Form
    {
        private Reference<ILogger> _loggerRef;
        private Reference<ICoreClient> _clientRef;
        private IServerBase2 _server;
        private readonly SynchronizationContext _syncContext;
        private int _queuedCalls;
        private readonly Guarded<Queue<ICoreItem>> _queuedItems = new Guarded<Queue<ICoreItem>>(new Queue<ICoreItem>());

        public WorkflowForm()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;
        }

        private void Form1Load(object sender, EventArgs e)
        {
            // create loggers
            _loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));
            StartUp();
        }

        private void StartUp()
        {
            try
            {
                var factory = new CoreClientFactory(_loggerRef)
                .SetEnv("Dev")
                .SetApplication(Assembly.GetExecutingAssembly())
                .SetProtocols(WcfConst.AllProtocolsStr)
                .SetServers("localhost");
                var client = factory.Create();
                _clientRef = Reference<ICoreClient>.Create(client);
                // init controls
                // - form title
                EnvId env = _clientRef.Target.ClientInfo.ConfigEnv;
                Text += $" ({EnvHelper.EnvName(env)})";
                // - server port
                int defaultPort = EnvHelper.SvcPort(env, SvcId.GridSwitch);
                chkChangePort.Text = $"Change server port from default ({defaultPort}) to:";
                _syncContext.Post(OnClientStateChange, new CoreStateChange(CoreStateEnum.Initial, _clientRef.Target.CoreState));
                _clientRef.Target.OnStateChange += _Client_OnStateChange;
            }
            catch (Exception excp)
            {
                _loggerRef.Target.Log(excp);
            }
        }

        void _Client_OnStateChange(CoreStateChange update)
        {
            _syncContext.Post(OnClientStateChange, update);
        }

        private void OnClientStateChange(object state)
        {
            var update = (CoreStateChange)state;
            Text = update.NewState.ToString();
        }

        private void BtnStartClick(object sender, EventArgs e)
        {
            StartTheServer();
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            CleanUp();
        }

        public void ReceiveNewItem(object notUsed)
        {
            // note: this runs on the foreground thread
            ProcessItems();
        }

        public void ProcessItems()
        {
            int count = Interlocked.Decrement(ref _queuedCalls);
            // exit if there are more callbacks following us
            if (count % 10000 == 0)
                _loggerRef.Target.LogDebug("ProcessItems: Queued calls remaining: {0}", count);
            if (count != 0)
                return;
            ICoreItem item = null;
            _queuedItems.Locked(queue =>
            {
                if (queue.Count > 0)
                    item = queue.Dequeue();
            });
            while (item != null)
            {
                if (item.Data is QuotedAssetSet)
                {
                    var nameSpace = item.AppProps.GetValue<string>(EnvironmentProp.NameSpace);
                    var marketName = item.AppProps.GetValue<string>(CurveProp.MarketAndDate);
                    var curveType = item.AppProps.GetValue<string>(CurveProp.PricingStructureType);
                    var curveName = item.AppProps.GetValue<string>(CurveProp.CurveName);
                    var curveGenRequest = new OrdinaryCurveGenRequest
                        {
                            NameSpace = nameSpace,
                            BaseDate = DateTime.Now,
                            RequestId = Guid.NewGuid().ToString(),
                            RequesterId = new UserIdentity
                                {
                                    Name = _clientRef.Target.ClientInfo.Name,
                                    DisplayName = _clientRef.Target.ClientInfo.UserFullName
                                },
                            UseSavedMarketData = true,
                            ForceGenerateEODCurves = true,
                            CurveSelector = new[] { new CurveSelection
                                {
                                    NameSpace = nameSpace,
                                    MarketName = marketName,
                                    CurveType = curveType,
                                    CurveName = curveName
                                }}
                        };
                    IWorkContext context = new WorkContext(_loggerRef.Target, _clientRef.Target, "DEV");
                    using (var workflow = new WFGenerateOrdinaryCurve())
                    {
                        workflow.Initialise(context);
                        WorkflowOutput<HandlerResponse> output = workflow.Execute(curveGenRequest);
                        WorkflowHelper.ThrowErrors(output.Errors);
                    }
                    item = null;
                    _queuedItems.Locked(queue =>
                        {
                            if (queue.Count > 0)
                                item = queue.Dequeue();
                        });
                }
            }
        }

        private void BtnSubscribeClick(object sender, EventArgs e)
        {
            IExpression query = Expr.ALL;
            ISubscription newSubscription = _clientRef.Target.CreateSubscription<QuotedAssetSet>(query);
            newSubscription.UserCallback = delegate(ISubscription subscription, ICoreItem item)
            {
                // note: this is running on a thread pool thread
                // add the item to the queue and post a callback
                _queuedItems.Locked(queue => queue.Enqueue(item));
                int count = Interlocked.Increment(ref _queuedCalls);
                if (count % 10000 == 0)
                    _loggerRef.Target.LogDebug("SubscribeCallback: Queued calls posted: {0}", count);
                _syncContext.Post(ReceiveNewItem, null);
            };
            newSubscription.Start();
            _loggerRef.Target.LogDebug("Subscription started.");
        }

        private void StartTheServer()
        {
            // start the service
            // - reset
            DisposeHelper.SafeDispose(ref _server);
            // - config
            var settings = new NamedValueSet();
            if (chkChangePort.Checked)
                settings.Set(WFPropName.Port, Int32.Parse(txtChangePort.Text));
            _server = new WorkflowServer { LoggerRef = _loggerRef, Client = _clientRef };
            // - start
            _server.Start();
        }

        private void CleanUp()
        {
            // stop the service
            DisposeHelper.SafeDispose(ref _server);
            DisposeHelper.SafeDispose(ref _clientRef);
            DisposeHelper.SafeDispose(ref _loggerRef);
        }
    }
}
