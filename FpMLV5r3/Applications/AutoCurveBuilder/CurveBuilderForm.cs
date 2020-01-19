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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Threading;
using Highlander.Workflow;
using Highlander.Workflow.CurveGeneration.V5r3;
using Highlander.Workflow.Server.V5r3;
using Exception = System.Exception;

namespace Highlander.AutoCurveBuilder.V5r3
{
    public partial class CurveBuilderForm : Form
    {
        private Reference<ILogger> _loggerRef;
        private Reference<ICoreClient> _clientRef;
        private ICoreCache _cache;
        private IServerBase2 _server;
        private readonly SynchronizationContext _syncContext;
        private int _queuedCalls;
        private readonly Guarded<Queue<ICoreItem>> _queuedItems = new Guarded<Queue<ICoreItem>>(new Queue<ICoreItem>());

        //TODO make this an input parameter.
        public string NameSpace = EnvironmentProp.DefaultNameSpace;

        public CurveBuilderForm()
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
                _cache = _clientRef.Target.CreateCache();
                //_cache.SubscribeInfoOnly<Algorithm>(Expr.ALL);
                // init controls
                // - form title
                var env = _clientRef.Target.ClientInfo.ConfigEnv;
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
            _loggerRef.Target.LogInfo("Old state is: {0}", update.OldState.ToString());
            _loggerRef.Target.LogInfo("New state is: {0}", update.NewState.ToString());
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
            var item = notUsed as ICoreItem;
            if (!(item?.Data is QuotedAssetSet qas)) return;
            // 1. Get the property values that uniquely identify the curves to refresh.
            // This is the process for the workflow request. Alternatively, a direct build of the curve can occur.
            //
            var nameSpace = item.AppProps.GetValue<string>(EnvironmentProp.NameSpace);
            var market = item.AppProps.GetValue<string>(CurveProp.Market);//For real time use Market and not MarketAndDate
            var curveType = item.AppProps.GetValue<string>(CurveProp.PricingStructureType);
            var curveName = item.AppProps.GetValue<string>(CurveProp.CurveName);
            var configIdentifier = FunctionProp.Configuration + ".PricingStructures." + market + "." + curveType + "." + curveName;
            var identifier = FunctionProp.Market + "." + market + "." + curveType + "." + curveName;
            List<ICoreItem> items = null;
            // 2.Check if the dependent curves should be refreshed
            //
            if (chkBoxDependentCurves.Checked)
            {
                //Find all the QuotesAssetSet's where the ReferenceCurveName is equal to the curveType.curveName!
                var requestProperties = new NamedValueSet();
                requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
                requestProperties.Set(CurveProp.Market, market);
                requestProperties.Set(EnvironmentProp.Function, FunctionProp.Configuration.ToString());
                requestProperties.Set(CurveProp.ReferenceCurveName, curveType + '.' + curveName);
                IExpression queryExpr = Expr.BoolAND(requestProperties);
                _loggerRef.Target.LogDebug("Dependent curve property request set at {0}", DateTime.Now.ToLongTimeString());
                items = _cache.LoadItems<Market>(queryExpr);
            }
            // 3. If the build is a local build then use the curve engine.
            //
            if (!chkBoxWorkflow.Checked)
            {
                _loggerRef.Target.LogDebug("Request to build base curve {0} locally at : {1}", identifier,
                                           DateTime.Now.ToLongTimeString());
                var curve = CurveEngine.V5r3.CurveEngine.RefreshPricingStructureFromConfiguration(_loggerRef.Target, _cache, nameSpace, configIdentifier, identifier, qas, DateTime.Now, DateTime.Now);
                _loggerRef.Target.LogDebug("Built the base curve {0} locally at : {1}", curve,
                                           DateTime.Now.ToLongTimeString());
                if (items != null)
                {
                    foreach (var dataItem in items)
                    {
                        if (!(dataItem.Data is Market spreadCurve)) continue;
                        //var bootstrap = dataItem.AppProps.GetValue<bool>(CurveProp.BootStrap, false);
                        //if (!bootstrap) { dataItem.AppProps.Set(CurveProp.BootStrap, true); }
                        try
                        {
                            var curveId = spreadCurve.id;
                            if (string.IsNullOrEmpty(curveId))
                            {
                                curveId = spreadCurve.Items[0].id;
                                //use yieldCurve.id, CurveGen 1.X compatible
                            }
                            dataItem.AppProps.Set(CurveProp.BaseDate, DateTime.Now);
                            dataItem.AppProps.Set(CurveProp.BuildDateTime, DateTime.Now);
                            var marketData =
                                new Pair<PricingStructure, PricingStructureValuation>(spreadCurve.Items[0],
                                                                                      spreadCurve.Items1[0]);
                            var ps = PricingStructureFactory.Create(_loggerRef.Target, _cache, nameSpace, null, null,
                                                                    marketData, dataItem.AppProps);
                            if (ps != null)
                            {
                                CurveEngine.V5r3.CurveEngine.SaveCurve(_cache, nameSpace, ps);
                            }
                            _loggerRef.Target.LogDebug("Built the spread curve {0} locally at : {1}",
                                                       curveId,
                                                       DateTime.Now.ToLongTimeString());
                        }
                        catch (Exception e)
                        {
                            _loggerRef.Target.LogDebug(e.ToString());
                        }
                    }
                }
            }
            else
            {
                // 4. Set the parameters for the work request.
                //
                var curveGenRequest = new OrdinaryCurveGenRequest
                    {
                        NameSpace = nameSpace,
                        BaseDate = DateTime.Now,
                        RequestId = Guid.NewGuid().ToString(),
                        RequesterId = new UserIdentity
                            {
                                Name = _cache.ClientInfo.Name,
                                DisplayName = _cache.ClientInfo.UserFullName
                                //Name = _clientRef.Target.ClientInfo.Name,
                                //DisplayName = _clientRef.Target.ClientInfo.UserFullName
                            },
                        UseSavedMarketData = true,
                        ForceGenerateEODCurves = true
                    };
                // 5. Set the base curve in the curve selection for the work request.
                //
                var curveSelector = new List<CurveSelection>
                    {
                        new CurveSelection
                            {
                                NameSpace = nameSpace,
                                MarketName = market,
                                CurveType = curveType,
                                CurveName = curveName
                            }
                    };
                // 6.Include all other dependent curve names i.e. spread curves.
                //
                if (items != null)
                {
                    curveSelector.AddRange(from childCurve in items
                                           let spreadCurveType =
                                               childCurve.AppProps.GetValue<string>(
                                                   CurveProp.PricingStructureType)
                                           let spreadCurveName =
                                               childCurve.AppProps.GetValue<string>(CurveProp.CurveName)
                                           select new CurveSelection
                                               {
                                                   NameSpace = nameSpace,
                                                   MarketName = market,
                                                   CurveType = spreadCurveType,
                                                   CurveName = spreadCurveName
                                               });
                }
                curveGenRequest.CurveSelector = curveSelector.ToArray();
                // 7. Set the actual work request.
                //
                IWorkContext context = new WorkContext(_loggerRef.Target, _cache, "DEV");
                //IWorkContext context = new WorkContext(_loggerRef.Target, _clientRef.Target, "DEV");
                _loggerRef.Target.LogDebug("WorkContext set at {0}", DateTime.Now.ToLongTimeString());
                using (var workflow = new WFGenerateOrdinaryCurve())
                {
                    workflow.Initialise(context);
                    WorkflowOutput<HandlerResponse> output = workflow.Execute(curveGenRequest);
                    WorkflowHelper.ThrowErrors(output.Errors);
                    foreach (var error in output.Errors)
                    {
                        _loggerRef.Target.LogInfo("WorkFlow error: {0} at {1}", error.Message, DateTime.Now.ToLongTimeString());
                    }
                }
                _loggerRef.Target.LogDebug("WorkFlow executed at {0}", DateTime.Now.ToLongTimeString());
                //item = null;
                //_queuedItems.Locked(queue =>
                //    {
                //        if (queue.Count > 0)
                //            item = queue.Dequeue();
                //    });
            }
        }

        public void ReceiveNewItemBackUp(object notUsed)
        {
            // note: this runs on the foreground thread
            ProcessItems();
        }

        public void ProcessItems()
        {
            int count = Interlocked.Decrement(ref _queuedCalls);
            // exit if there are more callbacks following us
            //if (count % 10000 == 0)
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
                if (item.Data is QuotedAssetSet qas)
                {
                    // 1. Get the property values that uniquely identify the curves to refresh.
                    // This is the process for the workflow request. Alternatively, a direct build of the curve can occur.
                    //
                    var nameSpace = item.AppProps.GetValue<string>(EnvironmentProp.NameSpace);
                    var market = item.AppProps.GetValue<string>(CurveProp.Market);//For real time use Market and not MarketAndDate
                    var curveType = item.AppProps.GetValue<string>(CurveProp.PricingStructureType);
                    var curveName = item.AppProps.GetValue<string>(CurveProp.CurveName);
                    var configIdentifier = FunctionProp.Configuration + ".PricingStructures." + market + "." + curveType + "." + curveName;
                    var identifier = FunctionProp.Market + "." + market + "." + curveType + "." + curveName;
                    List<ICoreItem> items = null;
                    // 2.Check if the dependent curves should be refreshed
                    //
                    if (chkBoxDependentCurves.Checked)
                    {
                        //Find all the QuotedAssetSet's where the ReferenceCurveName is equal to the curveType.curveName!
                        var requestProperties = new NamedValueSet();
                        requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
                        requestProperties.Set(CurveProp.Market, market);
                        requestProperties.Set(EnvironmentProp.Function, FunctionProp.Configuration);
                        requestProperties.Set(CurveProp.ReferenceCurveName, curveType + '.' + curveName);
                        IExpression queryExpr = Expr.BoolAND(requestProperties);
                        _loggerRef.Target.LogDebug("Dependent curve property request set at {0}", DateTime.Now.ToLongTimeString());
                        items = _cache.LoadItems<Market>(queryExpr);
                    }
                    // 3. If the build is a local build then use the curve engine.
                    //
                    if (!chkBoxWorkflow.Checked)
                    {
                        _loggerRef.Target.LogDebug("Request to build base curve {0} locally at : {1}", identifier,
                          DateTime.Now.ToLongTimeString());
                        var curve = CurveEngine.V5r3.CurveEngine.RefreshPricingStructureFromConfiguration(_loggerRef.Target, _cache, nameSpace, configIdentifier, identifier, qas, DateTime.Now, DateTime.Now);
                        _loggerRef.Target.LogDebug("Built the base curve {0} locally at : {1}", curve,
                          DateTime.Now.ToLongTimeString());
                        if (items != null)
                        {
                            foreach (var dataItem in items)
                            {
                                var spreadCurve = dataItem.Data as Market;
                                if (spreadCurve == null) continue;
                                //var bootstrap = dataItem.AppProps.GetValue<bool>(CurveProp.BootStrap, false);
                                //if (!bootstrap) { dataItem.AppProps.Set(CurveProp.BootStrap, true); }
                                try
                                {
                                    var curveId = spreadCurve.id;
                                    if (string.IsNullOrEmpty(curveId))
                                    {
                                        curveId = spreadCurve.Items[0].id;
                                        //use yieldCurve.id, CurveGen 1.X compatible
                                    }
                                    dataItem.AppProps.Set(CurveProp.BaseDate, DateTime.Now);
                                    dataItem.AppProps.Set(CurveProp.BuildDateTime, DateTime.Now);
                                    var marketData =
                                        new Pair<PricingStructure, PricingStructureValuation>(spreadCurve.Items[0],
                                                                                              spreadCurve.Items1[0]);
                                    var ps = PricingStructureFactory.Create(_loggerRef.Target, _cache, nameSpace, null, null,
                                                                            marketData, dataItem.AppProps);
                                    if (ps != null)
                                    {
                                        CurveEngine.V5r3.CurveEngine.SaveCurve(_cache, nameSpace, ps);
                                    }
                                    _loggerRef.Target.LogDebug("Built the spread curve {0} locally at : {1}",
                                                               curveId,
                                                               DateTime.Now.ToLongTimeString());
                                }
                                catch (Exception e)
                                {
                                    _loggerRef.Target.LogDebug(e.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        // 4. Set the parameters for the work request.
                        //
                        var curveGenRequest = new OrdinaryCurveGenRequest
                            {
                                NameSpace = nameSpace,
                                BaseDate = DateTime.Now,
                                RequestId = Guid.NewGuid().ToString(),
                                RequesterId = new UserIdentity
                                    {
                                        Name = _cache.ClientInfo.Name,
                                        DisplayName = _cache.ClientInfo.UserFullName
                                        //Name = _clientRef.Target.ClientInfo.Name,
                                        //DisplayName = _clientRef.Target.ClientInfo.UserFullName
                                    },
                                UseSavedMarketData = true,
                                ForceGenerateEODCurves = true
                            };
                        // 5. Set the base curve in the curve selection for the work request.
                        //
                        var curveSelector = new List<CurveSelection>
                            {
                                new CurveSelection
                                    {
                                        NameSpace = nameSpace,
                                        MarketName = market,
                                        CurveType = curveType,
                                        CurveName = curveName
                                    }
                            };
                        // 6.Include all other dependent curve names i.e. spread curves.
                        //
                        if (items!=null)
                        {
                            curveSelector.AddRange(from childCurve in items
                                                   let spreadCurveType =
                                                       childCurve.AppProps.GetValue<string>(
                                                           CurveProp.PricingStructureType)
                                                   let spreadCurveName =
                                                       childCurve.AppProps.GetValue<string>(CurveProp.CurveName)
                                                   select new CurveSelection
                                                       {
                                                           NameSpace = nameSpace,
                                                           MarketName = market,
                                                           CurveType = spreadCurveType,
                                                           CurveName = spreadCurveName
                                                       });
                        }
                        curveGenRequest.CurveSelector = curveSelector.ToArray();
                        // 7. Set the actual work request.
                        //
                        IWorkContext context = new WorkContext(_loggerRef.Target, _cache, "DEV");
                        //IWorkContext context = new WorkContext(_loggerRef.Target, _clientRef.Target, "DEV");
                        _loggerRef.Target.LogDebug("WorkContext set at {0}", DateTime.Now.ToLongTimeString());
                        using (var workflow = new WFGenerateOrdinaryCurve())
                        {
                            workflow.Initialise(context);
                            WorkflowOutput<HandlerResponse> output = workflow.Execute(curveGenRequest);
                            WorkflowHelper.ThrowErrors(output.Errors);
                            foreach (var error in output.Errors)
                            {
                                _loggerRef.Target.LogInfo("WorkFlow error: {0} at {1}", error.Message, DateTime.Now.ToLongTimeString());
                            }
                        }
                        _loggerRef.Target.LogDebug("WorkFlow executed at {0}", DateTime.Now.ToLongTimeString());
                        //item = null;
                        //_queuedItems.Locked(queue =>
                        //    {
                        //        if (queue.Count > 0)
                        //            item = queue.Dequeue();
                        //    });
                    }
                }
            }
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
            _server = new WorkflowServer {LoggerRef = _loggerRef, Client = _clientRef};
            // - start
            _server.Start();
        }

        private void CleanUp()
        {
            // stop the service
            DisposeHelper.SafeDispose(ref _server);
            DisposeHelper.SafeDispose(ref _cache);
            DisposeHelper.SafeDispose(ref _clientRef);
            DisposeHelper.SafeDispose(ref _loggerRef);
        }

        private void BtnAutoCurveBuilderClick(object sender, EventArgs e)
        {
            var ccy = listBoxCurrencies.SelectedItems;
            IExpression query1 = ccy.Cast<object>().Aggregate<object, IExpression>(null, (current, cc) => Expr.BoolOR(current, Expr.IsEQU(CurveProp.Currency1, cc))); 
            var pst = listBoxCurves.SelectedItems;           
            var query2 = pst.Cast<object>().Aggregate<object, IExpression>(null, (current, ps) => Expr.BoolOR(current, Expr.IsEQU(CurveProp.PricingStructureType, ps)));
            var marketName = txtBoxMarket.Text;
            var query3 = Expr.IsEQU(CurveProp.Market, marketName);
            query2 = Expr.BoolAND(query1, query2, query3, Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace), Expr.IsEQU(EnvironmentProp.Function, FunctionProp.QuotedAssetSet));
            ISubscription newSubscription = _clientRef.Target.CreateSubscription<QuotedAssetSet>(query2);
            newSubscription.UserCallback = delegate(ISubscription subscription, ICoreItem item)
            {
                // note: this is running on a thread pool thread
                // add the item to the queue and post a callback
                _queuedItems.Locked(queue => queue.Enqueue(item));
                _loggerRef.Target.LogDebug("Queued Item {0} created at {1}", item.Name, item.Created.ToString());
                int count = Interlocked.Increment(ref _queuedCalls);
                //if (count % 10000 == 0)
                _loggerRef.Target.LogDebug("SubscribeCallback: Queued calls posted: {0}", count);
                _syncContext.Post(ReceiveNewItem, item); //changed from null
            };
            newSubscription.Start();
            _loggerRef.Target.LogDebug("Subscription started.");
        }

        private void ButtonStopBuildClick(object sender, EventArgs e)
        {
            _clientRef.Target.UnsubscribeAll();
        }
    }
}
