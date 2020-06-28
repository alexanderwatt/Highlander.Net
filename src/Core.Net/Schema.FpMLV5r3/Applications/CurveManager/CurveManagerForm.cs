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

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Highlander.Build;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Metadata.Common;
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Threading;
using Highlander.WinTools;
using Highlander.Workflow;
using Highlander.Workflow.CurveGeneration.V5r3;
using Exception = System.Exception;

#endregion

namespace Highlander.CurveManager.V5r3
{
    public partial class CurveManagerForm : Form
    {
        private static readonly EnvId BuildEnv = EnvHelper.ParseEnvName(BuildConst.BuildEnv); // todo


        private Reference<ILogger> _logRef;
        private AsyncThreadQueue _workerThreadQueue;
        private ICoreClient _client;
        private ICoreCache _cache;

        // CurveDef grid
        private IListViewHelper<CurveObj> _curveDefView;
        private IViewHelper _curveDefViewHelper;
        private IDataHelper<CurveObj> _curveDefDataHelper;
        private ISelecter<CurveObj> _curveDefSelecter;
        private IFilterGroup _curveDefFilters;

        // StressDef grid
        private IListViewHelper<StressDefObj> _stressDefView;
        private IViewHelper _stressDefViewHelper;
        private IDataHelper<StressDefObj> _stressDefDataHelper;
        private ISelecter<StressDefObj> _stressDefSelecter;
        private IFilterGroup _stressDefFilters;

        // ScenarioDef grid
        private IListViewHelper<ScenarioDefObj> _scenarioDefView;
        private IViewHelper _scenarioDefViewHelper;
        private IDataHelper<ScenarioDefObj> _scenarioDefDataHelper;
        private ISelecter<ScenarioDefObj> _scenarioDefSelecter;
        private IFilterGroup _scenarioDefFilters;

        // BaseCurve grid
        private IListViewHelper<CurveObj> _baseCurveView;
        private IViewHelper _baseCurveViewHelper;
        private IDataHelper<CurveObj> _baseCurveDataHelper;
        private ISelecter<CurveObj> _baseCurveSelecter;
        private IFilterGroup _baseCurveFilters;

        // StressCurve grid
        private IListViewHelper<CurveObj> _stressCurveView;
        private IViewHelper _stressCurveViewHelper;
        private IDataHelper<CurveObj> _stressCurveDataHelper;
        private ISelecter<CurveObj> _stressCurveSelecter;
        private IFilterGroup _stressCurveFilters;

        public CurveManagerForm()
        {
            InitializeComponent();
        }

        private void StartUp()
        {
            try
            {
                CoreClientFactory factory = new CoreClientFactory(_logRef)
                    .SetEnv(BuildEnv.ToString())
                    .SetApplication(Assembly.GetExecutingAssembly())
                    .SetProtocols(WcfConst.AllProtocolsStr);
                //if (rbDefaultServers.Checked)
                //    _Client = factory.Create();
                //else if (rbLocalhost.Checked)
                    _client = factory.SetServers("localhost").Create();
                    _cache = _client.CreateCache();
                //else
                //    _Client = factory.SetServers(txtSpecificServers.Text).Create();
                //_SyncContext.Post(OnClientStateChange, new CoreStateChange(CoreStateEnum.Initial, _Client.CoreState));
                //_Client.OnStateChange += new CoreStateHandler(_Client_OnStateChange);
            }
            catch (Exception excp)
            {
                _logRef.Target.Log(excp);
            }
        }

        private void Form1Load(object sender, EventArgs e)
        {
            // create loggers
            _logRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));
            // worker thread queue
            _workerThreadQueue = new AsyncThreadQueue(_logRef.Target);
            // init controls
            // - form title
            WinFormHelper.SetAppFormTitle(this, EnvHelper.EnvName(BuildEnv));
            // - disable dev debug tools - todo
            // loading
            StartUp();
            //_Client = new CoreClientFactory(_Logger).Create();
            //_Cache = _Client.CreateCache();
            //ServerStore.Attach(_Cache);
            dtpBaseDate.Value = DateTime.Today;
            //dtpMarketDate.Value = DateTime.Today;
            //cbMarket.SelectedIndex = 0;
            // setup the CurveDef view
            _curveDefViewHelper = new CurveDefViewHelper();
            _curveDefDataHelper = new CurveDefDataHelper();
            _curveDefFilters = new ComboxBoxFilterGroup(
                panelCurveDef, _curveDefViewHelper, CurveDefSelectionChanged);
            _curveDefSelecter = new CurveDefSelecter(
                _curveDefFilters, _curveDefViewHelper, _curveDefDataHelper);
            _curveDefView = new ListViewManager<CurveObj>(
                _logRef.Target, lvCurveDef, _curveDefViewHelper,
                _curveDefSelecter, _curveDefFilters, new CurveDefSorter(), _curveDefDataHelper);
            ICoreCache curveDefCache = _client.CreateCache(
                update => _curveDefView.UpdateData(new ViewChangeNotification<CurveObj>
                                                       {
                                                           Change = update.Change,
                                                           OldData =
                                                               update.OldItem != null
                                                                   ? new CurveObj(update.OldItem)
                                                                   : null,
                                                           NewData =
                                                               update.NewItem != null
                                                                   ? new CurveObj(update.NewItem)
                                                                   : null
                                                       }), SynchronizationContext.Current);
            //curveDefCache.SubscribeNoWait<Market>(Expr.StartsWith(Expr.SysPropItemName, cBoxNameSpaces.Text + ".Configuration."), null, null);//TODO
            curveDefCache.SubscribeNoWait<Market>(Expr.IsEQU(EnvironmentProp.Function, FunctionProp.Configuration.ToString()), null, null);
            // create the StressDef view
            _stressDefViewHelper = new StressDefViewHelper();
            _stressDefDataHelper = new StressDefDataHelper();
            _stressDefFilters = new ComboxBoxFilterGroup(
                panelShockDef, _stressDefViewHelper, StressDefSelectionChanged);
            _stressDefSelecter = new StressDefSelecter(
                _stressDefFilters, _stressDefViewHelper, _stressDefDataHelper);
            _stressDefView = new ListViewManager<StressDefObj>(
                _logRef.Target, lvShockDef, _stressDefViewHelper,
                _stressDefSelecter, _stressDefFilters, new StressDefSorter(), _stressDefDataHelper);

            ICoreCache stressDefCache = _client.CreateCache(
                update => _stressDefView.UpdateData(new ViewChangeNotification<StressDefObj>
                                                        {
                                                            Change = update.Change,
                                                            OldData =
                                                                (update.OldItem != null)
                                                                    ? new StressDefObj(update.OldItem)
                                                                    : null,
                                                            NewData =
                                                                (update.NewItem != null)
                                                                    ? new StressDefObj(update.NewItem)
                                                                    : null
                                                        }), SynchronizationContext.Current);
            stressDefCache.SubscribeNoWait<StressRule>(Expr.ALL, null, null);
            // create the ScenarioDef view
            _scenarioDefViewHelper = new ScenarioDefViewHelper();
            _scenarioDefDataHelper = new ScenarioDefDataHelper();
            _scenarioDefFilters = new ComboxBoxFilterGroup(
                panelScenarioDef, _scenarioDefViewHelper, ScenarioDefSelectionChanged);
            _scenarioDefSelecter = new ScenarioDefSelecter(
                _scenarioDefFilters, _scenarioDefViewHelper, _scenarioDefDataHelper);
            _scenarioDefView = new ListViewManager<ScenarioDefObj>(
                _logRef.Target, lvScenarioDef, _scenarioDefViewHelper,
                _scenarioDefSelecter, _scenarioDefFilters, new ScenarioDefSorter(), _scenarioDefDataHelper);
            ICoreCache scenarioDefCache = _client.CreateCache(
                update => _scenarioDefView.UpdateData(new ViewChangeNotification<ScenarioDefObj>
                                                          {
                                                              Change = update.Change,
                                                              OldData =
                                                                  (update.OldItem != null)
                                                                      ? new ScenarioDefObj(update.OldItem)
                                                                      : null,
                                                              NewData =
                                                                  (update.NewItem != null)
                                                                      ? new ScenarioDefObj(update.NewItem)
                                                                      : null
                                                          }), SynchronizationContext.Current);
            //IExpression queryExpr = Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace);
            scenarioDefCache.SubscribeNoWait<ScenarioRule>(Expr.ALL, null, null);
            // setup the BaseCurve view
            _baseCurveViewHelper = new BaseCurveViewHelper();
            _baseCurveDataHelper = new BaseCurveDataHelper();
            _baseCurveFilters = new ComboxBoxFilterGroup(
                pnlBaseCurve, _baseCurveViewHelper, BaseCurveSelectionChanged);
            _baseCurveSelecter = new BaseCurveSelecter(
                _baseCurveFilters, _baseCurveViewHelper, _baseCurveDataHelper);
            _baseCurveView = new ListViewManager<CurveObj>(
                _logRef.Target, lvBaseCurve, _baseCurveViewHelper,
                _baseCurveSelecter, _baseCurveFilters, new BaseCurveSorter(), _baseCurveDataHelper);
            ICoreCache baseCurveCache = _client.CreateCache(
                update => _baseCurveView.UpdateData(new ViewChangeNotification<CurveObj>
                                                        {
                                                            Change = update.Change,
                                                            OldData =
                                                                (update.OldItem != null)
                                                                    ? new CurveObj(update.OldItem)
                                                                    : null,
                                                            NewData =
                                                                (update.NewItem != null)
                                                                    ? new CurveObj(update.NewItem)
                                                                    : null
                                                        }), SynchronizationContext.Current);
            //baseCurveCache.SubscribeNoWait<Market>(
            //    Expr.BoolAND(
            //        Expr.IsNull(CurveProp.StressName),
            //        Expr.StartsWith(Expr.SysPropItemName, "Orion.Market.")),
            //    null, null);
            baseCurveCache.SubscribeNoWait<Market>(
                Expr.BoolAND(
                    Expr.IsNull(CurveProp.StressName),
                    Expr.IsEQU(EnvironmentProp.Function, FunctionProp.Market.ToString())),
                null, null);
            // setup the StressCurve view
            _stressCurveViewHelper = new StressCurveViewHelper();
            _stressCurveDataHelper = new StressCurveDataHelper();
            _stressCurveFilters = new ComboxBoxFilterGroup(
                pnlStressCurve, _stressCurveViewHelper, StressCurveSelectionChanged);
            _stressCurveSelecter = new StressCurveSelecter(
                _stressCurveFilters, _stressCurveViewHelper, _stressCurveDataHelper);
            _stressCurveView = new ListViewManager<CurveObj>(
                _logRef.Target, lvStressCurve, _stressCurveViewHelper,
                _stressCurveSelecter, _stressCurveFilters, new StressCurveSorter(), _stressCurveDataHelper);
            ICoreCache stressCurveCache = _client.CreateCache(
                update => _stressCurveView.UpdateData(new ViewChangeNotification<CurveObj>
                                                          {
                                                              Change = update.Change,
                                                              OldData =
                                                                  (update.OldItem != null)
                                                                      ? new CurveObj(update.OldItem)
                                                                      : null,
                                                              NewData =
                                                                  (update.NewItem != null)
                                                                      ? new CurveObj(update.NewItem)
                                                                      : null
                                                          }), SynchronizationContext.Current);
            //stressCurveCache.SubscribeNoWait<Market>(
            //    Expr.BoolAND(
            //        Expr.IsNotNull(CurveProp.StressName),
            //        Expr.StartsWith(Expr.SysPropItemName, "Orion.Market.")),
            //    null, null);
            stressCurveCache.SubscribeNoWait<Market>(
                Expr.BoolAND(
                    Expr.IsNotNull(CurveProp.StressName),
                    Expr.IsEQU(EnvironmentProp.Function, FunctionProp.Market.ToString())),
                null, null);
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeHelper.SafeDispose(ref _workerThreadQueue);
            //ServerStore.Detach();
            DisposeHelper.SafeDispose(ref _cache);
            DisposeHelper.SafeDispose(ref _client);
        }

        void CurveDefSelectionChanged(object sender, EventArgs e)
        {
            _curveDefView.RebuildView();
        }

        void StressDefSelectionChanged(object sender, EventArgs e)
        {
            _stressDefView.RebuildView();
        }

        void ScenarioDefSelectionChanged(object sender, EventArgs e)
        {
            _scenarioDefView.RebuildView();
        }

        void BaseCurveSelectionChanged(object sender, EventArgs e)
        {
            _baseCurveView.RebuildView();
        }

        void StressCurveSelectionChanged(object sender, EventArgs e)
        {
            _stressCurveView.RebuildView();
        }

        // helpers

        //private ViewChangeNotification<T> MakeViewChangeNotify<T>(CacheChange change, ICoreItem oldItem, ICoreItem newItem) where T : class
        //{
        //    return new ViewChangeNotification<T>()
        //    {
        //        Change = change,
        //        OldData = (oldItem != null) ? (T)oldItem.Data : null,
        //        NewData = (newItem != null) ? (T)newItem.Data : null
        //    };
        //}

        private void BtnRefreshRatesClick(object sender, EventArgs e)
        {
            // Curve identifiers
            //var curveItemNames = new List<string>();
            List<CurveObj> dataItemsSource = chkSelectedCurvesOnly.Checked ? _curveDefView.SelectedDataItems : _curveDefView.DataItems;
            var curveGenRequest = new OrdinaryCurveGenRequest
                                      {
                RequestId = Guid.NewGuid().ToString(),
                RequesterId = new UserIdentity
                                  {
                    Name = _client.ClientInfo.Name,
                    DisplayName = _client.ClientInfo.UserFullName
                },
                NameSpace = cbClientNameSpace.Text, 
                BaseDate = dtpBaseDate.Value.Date,
                SaveMarketData = chkSaveMarketData.Checked,
                UseSavedMarketData = chkUseSavedMarketData.Checked,
                ForceGenerateEODCurves = chkGenerateEODCurves.Checked,
                CurveSelector = dataItemsSource.Select(curveDefObj => new CurveSelection
                                                                          {
                                                                              NameSpace = curveDefObj.NameSpace, 
                                                                              MarketName = curveDefObj.MarketName, 
                                                                              CurveType = curveDefObj.CurveType, 
                                                                              CurveName = curveDefObj.CurveName
                                                                          }).ToArray()
            };
            //_WorkerThreadQueue.Dispatch<OrdinaryCurveGenRequest>(curveGenRequest, BackgroundGenerateOrdinaryCurves);
            IWorkContext context = new WorkContext(_logRef.Target, _cache, "DEV");
            using (var workflow = new WFGenerateOrdinaryCurve())
            {
                workflow.Initialise(context);
                WorkflowOutput<HandlerResponse> output = workflow.Execute(curveGenRequest);
                WorkflowHelper.ThrowErrors(output.Errors);
            }
            //_cache.SaveObject(curveGenRequest);
            //RequestBase.DispatchToManager(_cache, curveGenRequest);
        }

        //private void BackgroundGenerateOrdinaryCurves(OrdinaryCurveGenRequest input)
        //{
        //    ILogger logger = new FilterLogger(_logRef.Target, "BaseCurveGen: ");
        //    logger.LogDebug("Starting...");
        //    bool failed = false;
        //    try
        //    {
        //        IWorkContext context = new WorkContext(logger, _cache, GetType().Name);
        //        int count = 0;
        //        if (input.CurveSelector != null)
        //            count = input.CurveSelector.Length;
        //        logger.LogDebug("Processing {0} curve definitions ...", count);
        //        {
        //            using (var workflow = new WFGenerateOrdinaryCurve())
        //            {
        //                workflow.Initialise(context);
        //                WorkflowOutput<HandlerResponse> output = workflow.Execute(input);
        //                failed = WorkflowHelper.LogErrors(logger, output.Errors);
        //            }
        //        }
        //        logger.LogDebug("Processed {0} curve definitions.", count);
        //    }
        //    catch (Exception excp)
        //    {
        //        failed = true;
        //        logger.Log(excp);
        //    }
        //    finally
        //    {
        //        logger.LogDebug((failed ? "FAILED!" : "Completed successfully."));
        //    }
        //}

        //private void BackgroundCopyCurves(OrdinaryCurveGenRequest input)
        //{
        //    ILogger logger = new FilterLogger(_LogRef.Target, "CurveCopy: ");
        //    logger.LogDebug("Starting...");
        //    bool failed = false;
        //    try
        //    {
        //        IWorkContext context = new WorkContext(logger, _Cache, this.GetType().Name);
        //        int count = 0;
        //        if (input.CurveSelector != null)
        //            count = input.CurveSelector.Length;
        //        logger.LogDebug("Processing {0} curves ...", count);
        //        {
        //            using (WFCopyBaseCurve workflow = new WFCopyBaseCurve())
        //            {
        //                workflow.Initialise(context);
        //                WorkflowOutput<HandlerResponse> output = workflow.Execute(input);
        //                failed = WorkflowHelper.LogErrors(logger, output.Errors);
        //            }
        //        }
        //        logger.LogDebug("Processed {0} curve definitions.", count);
        //    }
        //    catch (Exception excp)
        //    {
        //        failed = true;
        //        logger.Log(excp);
        //    }
        //    finally
        //    {
        //        logger.LogDebug((failed ? "FAILED!" : "Completed successfully."));
        //    }
        //}

        private void BtnGenStressedCurvesClick1(object sender, EventArgs e)
        {
            List<CurveObj> dataItemsSource = chkSelectedStressesOnly.Checked ? _baseCurveView.SelectedDataItems : _baseCurveView.DataItems;
            var curveGenRequest = new StressedCurveGenRequest
                                      {
                RequestId = Guid.NewGuid().ToString(),
                RequesterId = new UserIdentity
                                  {
                    Name = _client.ClientInfo.Name,
                    DisplayName = _client.ClientInfo.UserName //The UserFullName is null alexqqq
                },
                BaseDate = dtpBaseDate.Value.Date,
                CurveSelector = dataItemsSource.Select(curveDefObj => new CurveSelection
                                                                          {
                                                                              NameSpace = curveDefObj.NameSpace,
                                                                              MarketName = curveDefObj.MarketName,
                                                                              MarketDate = curveDefObj.MarketDate,
                                                                              CurveType = curveDefObj.CurveType,
                                                                              CurveName = curveDefObj.CurveName
                                                                          }).ToArray()
            };
            //// build list of curve unique ids required
            //List<string> baseCurveUniqueIds = new List<string>();
            //foreach (var curveSelector in curveSelectors)
            //{
            //    string baseCurveUniqueId = String.Format("Orion.Market.{0}.{1}.{2}",
            //        curveSelector.MarketName, curveSelector.CurveType, curveSelector.CurveName);
            //    baseCurveUniqueIds.Add(baseCurveUniqueId);

            //}
            //AssertNotModified<Market>(baseCurveUniqueIds);
            IWorkContext context = new WorkContext(_logRef.Target, _cache, "DEV");
            using (var workflow = new WFGenerateStressedCurve())
            {
                workflow.Initialise(context);
                WorkflowOutput<HandlerResponse> output = workflow.Execute(curveGenRequest);
                WorkflowHelper.ThrowErrors(output.Errors);
            }
            //AssertNotModified<Market>(baseCurveUniqueIds);
            //_Cache.SaveObject<HandlerResponse>(result);
 //           _WorkerThreadQueue.Dispatch<StressedCurveGenRequest>(curveGenRequest, BackgroundGenerateStressedCurves);
 //           _Cache.SaveObject<StressedCurveGenRequest>(curveGenRequest);///alexqqq the stress names come through as null and this blows up!
 //           RequestBase.DispatchToManager(_Cache, curveGenRequest);
        }

        //private void BackgroundGenerateStressedCurves(StressedCurveGenRequest input)
        //{
        //    ILogger logger = new FilterLogger(_logRef.Target, "StressCurveGen: ");
        //    logger.LogDebug("Starting...");
        //    bool failed = false;
        //    try
        //    {
        //        IWorkContext context = new WorkContext(logger, _cache, GetType().Name);
        //        int count = 0;
        //        if (input.CurveSelector != null)
        //            count = input.CurveSelector.Length;
        //        logger.LogDebug("Processing {0} base curves ...", count);
        //        using (var workflow = new WFGenerateStressedCurve())
        //        {
        //            workflow.Initialise(context);
        //            WorkflowOutput<HandlerResponse> output = workflow.Execute(input);
        //            failed = WorkflowHelper.LogErrors(logger, output.Errors);
        //        }
        //        logger.LogDebug("Processed {0} base curves.", count);
        //    }
        //    catch (Exception excp)
        //    {
        //        failed = true;
        //        logger.Log(excp);
        //    }
        //    finally
        //    {
        //        logger.LogDebug((failed ? "FAILED!" : "Completed."));
        //    }
        //}

        //private void btnCopyCurves_Click(object sender, EventArgs e)
        //{
        //    List<CurveObj> dataItemsSource = chkSelectedBaseCurvesOnlyToCopy.Checked ? _baseCurveView.SelectedDataItems : _baseCurveView.DataItems;
        //    var curveGenRequest = new OrdinaryCurveGenRequest()
        //    {
        //        RequestId = Guid.NewGuid().ToString(),
        //        RequesterId = new UserIdentity()
        //        {
        //            Name = _client.ClientInfo.Name,
        //            DisplayName = _client.ClientInfo.UserFullName
        //        },
        //        BaseDate = dtpBaseDate.Value.Date,
        //        SaveMarketData = chkSaveMarketData.Checked,
        //        UseSavedMarketData = chkUseSavedMarketData.Checked,
        //        ForceGenerateEODCurves = chkGenerateEODCurves.Checked,
        //        CurveSelector = dataItemsSource.Select(curveDefObj => new CurveSelection
        //                                                                  {
        //                                                                      MarketName = curveDefObj.MarketName, MarketDate = curveDefObj.MarketDate, CurveType = curveDefObj.CurveType, CurveName = curveDefObj.CurveName
        //                                                                  }).ToArray()
        //    };
        //    //_WorkerThreadQueue.Dispatch<CurveGenerationRequest>(curveGenRequest, BackgroundCopyCurves);
        //    _cache.SaveObject(curveGenRequest);
        //    RequestBase.DispatchToManager(_cache, curveGenRequest);
        //}
    } // Form1

    // ------------------------------------------------------------------------
    // RatesHdr

    public class RatesHdrObj
    {
        public string InstrumentId { get; set; }
        public string Instrument { get; set; }
        public string Currency { get; set; }
        public Period Term { get; set; }
        public decimal Rate1 { get; set; }
        public decimal Rate2 { get; set; }
        public RatesHdrObj(Asset asset, BasicAssetValuation bav)
        {
            Instrument = asset.GetType().Name;
            if (asset is Deposit asset1)
            {
                var deposit = asset1;
                InstrumentId = deposit.instrumentId[0].Value;
                Currency = deposit.currency.Value;
                Term = deposit.term;
            }
            else
            {
                if (asset is SimpleIRSwap irSwap)
                {
                    var swap = irSwap;
                    InstrumentId = swap.instrumentId[0].Value;
                    Currency = swap.currency.Value;
                    Term = swap.term;
                }
                else
                {
                    if (asset is Future future1)
                    {
                        var future = future1;
                        InstrumentId = future.instrumentId[0].Value;
                        Currency = future.currency.Value;
                        //Term = future.term;
                    }
                }
            }
            //else
            //    throw new ApplicationException("Unknown Asset: " + asset.GetType().Name);
            if (bav.quote.Length > 0)
                Rate1 = bav.quote[0].value;
            if (bav.quote.Length > 1)
                Rate2 = bav.quote[1].value;
        }
    }

    // ------------------------------------------------------------------------
    // BaseUIObj object - base UI object used by all views

    public class BaseUIObj
    {
        // base/UI properties
        public string UniqueId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Expires { get; set; }
        public string Publisher { get; set; }
        public string Build { get; set; }
        // debug
        public string ExcpName { get; set; }
        public string ExcpText { get; set; }

        public BaseUIObj(ICoreItem item)
        {
            // common item properties
            UniqueId = item.Name;
            Created = item.Created;
            Expires = item.Expires;
            Publisher = item.SysProps.GetValue<string>(SysPropName.UserIdentity, null);
            Build = item.SysProps.GetValue<string>(SysPropName.ApplFVer, null);
            // workflow exceptions
            ExcpName = item.AppProps.GetValue<string>(WFPropName.ExcpName, null);
            ExcpText = item.AppProps.GetValue<string>(WFPropName.ExcpText, null);
        }
    }

    // ------------------------------------------------------------------------
    // CurveDef

    public enum CurveDefColEnum
    {
        NameSpace,
        Market,
        CurveType,
        Ccy1,
        Ccy2,
        Instrument,
        Term,
        CurveName,
        Created,
        Expires,
        UniqueId,
        Publisher,
        Build
    }

    internal class CurveDefViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(CurveDefColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((CurveDefColEnum)column).ToString();
        }

        public bool IsFilterColumn(int column)
        {
            switch ((CurveDefColEnum)column)
            {
                case CurveDefColEnum.NameSpace: return true;
                case CurveDefColEnum.Market: return true;
                case CurveDefColEnum.CurveType: return true;
                case CurveDefColEnum.Instrument: return true;
                case CurveDefColEnum.Term: return true;
                case CurveDefColEnum.Ccy1: return true;
                case CurveDefColEnum.Ccy2: return true;
                case CurveDefColEnum.Publisher: return true;
                default:
                    return false;
            }
        }

        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class CurveDefDataHelper : IDataHelper<CurveObj>
    {
        public string GetUniqueKey(CurveObj data)
        {
            return data.UniqueId;
        }

        //private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        //{
        //    var delims = new[] { delim };
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

        public string GetDisplayValue(CurveObj data, int column)
        {
            switch ((CurveDefColEnum)column)
            {
                case CurveDefColEnum.NameSpace: return data.NameSpace;
                case CurveDefColEnum.UniqueId: return data.UniqueId;
                case CurveDefColEnum.Publisher: return data.Publisher;
                case CurveDefColEnum.Build: return data.Build;
                case CurveDefColEnum.Market: return data.MarketName;
                case CurveDefColEnum.CurveType: return data.CurveType;
                case CurveDefColEnum.CurveName: return data.CurveName;
                case CurveDefColEnum.Instrument: return data.Instrument;
                case CurveDefColEnum.Term: return data.InstrTerm;
                case CurveDefColEnum.Ccy1: return data.Currency1;
                case CurveDefColEnum.Ccy2: return data.Currency2;
                case CurveDefColEnum.Created: return data.Created.LocalDateTime.ToString("g");
                case CurveDefColEnum.Expires: return data.Expires.LocalDateTime.ToString("g");
                default: return null;
            }
        }
    }

    internal class CurveDefSorter : IComparer<CurveObj>
    {
        //private static PeriodComparer _periodComparer = new PeriodComparer();

        public int Compare(CurveObj a, CurveObj b)
        {
            // sort order column priority

            // descending create time
            int result = DateTimeOffset.Compare(a.Created, b.Created);
            if (result != 0)
                return -1 * result;

            //// ascending currency
            //result = String.Compare(a.Currency, b.Currency, true);
            //if (result != 0)
            //    return result;

            //// ascending term
            //result = _PeriodComparer.Compare(a.Term, b.Term);
            //if (result != 0)
            //    return result;

            return result;
        }
    }

    public class CurveDefSelecter : BaseSelecter<CurveObj>
    {
        // this class is currently is a placeholder for future selection rules
        public CurveDefSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<CurveObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // QuoteObj

    public class QuoteObj : BaseUIObj
    {
        public string NameSpace { get; set; }
        public string MarketName { get; set; }
        public string StressName { get; set; }
        public string CurveType { get; set; }
        public string CurveName { get; set; }
        public string CreditInstrumentId { get; set; }
        public string CreditSeniority { get; set; }
        public string IndexName { get; set; }
        public string IndexTenor { get; set; }
        public string Currency { get; set; }

        public QuoteObj(ICoreItem item)
            : base(item)
        {
            try
            {
                //var data = (QuotedAssetSet)item.Data;
                NameSpace = item.AppProps.GetValue<string>(EnvironmentProp.NameSpace, true); 
                MarketName = item.AppProps.GetValue<string>(CurveProp.Market, true); // todo null
                StressName = item.AppProps.GetValue<string>(CurveProp.StressName, null);
                CurveType = item.AppProps.GetValue<string>("BaseCurveType", null);
                Currency = item.AppProps.GetValue<string>(CurveProp.Currency1, null);
                CurveName = item.AppProps.GetValue<string>(CurveProp.CurveName, null);
                CreditInstrumentId = item.AppProps.GetValue<string>("CreditInstrumentId", null);
                CreditSeniority = item.AppProps.GetValue<string>("CreditSeniority", null);
                IndexName = item.AppProps.GetValue<string>(CurveProp.IndexName, null);
                IndexTenor = item.AppProps.GetValue<string>(CurveProp.IndexTenor, null);
            }
            catch (Exception e)
            {
                ExcpName = e.GetType().Name;
                ExcpText = e.Message;
            }
        }
        public string Instrument
        {
            get
            {
                // todo more curve types: fx etc.
                switch (CurveType.ToLower())
                {
                    case "discountcurve": return CreditInstrumentId;
                    default: return IndexName;
                }
            }
        }
        public string InstrTerm
        {
            get
            {
                // todo more curve types: fx etc.
                switch (CurveType.ToLower())
                {
                    case "discountcurve": return CreditSeniority;
                    default: return IndexTenor;
                }
            }
        }
    }

    // ------------------------------------------------------------------------
    // StressDefObj

    public class StressDefObj : BaseUIObj
    {
        public string NameSpace { get; set; }
        public string StressId { get; set; }
        public string RuleId { get; set; }
        public bool Disabled { get; set; }
        public int Priority { get; set; }
        public IExpression FilterExpr { get; set; }
        public IExpression UpdateExpr { get; set; }

        public StressDefObj(ICoreItem item)
            : base(item)
        {
            try
            {
                NameSpace = item.AppProps.GetValue<string>(EnvironmentProp.NameSpace, true); 
                var data = (StressRule)item.Data;
                StressId = data.StressId;
                RuleId = data.RuleId;
                Disabled = data.Disabled;
                Priority = data.Priority;
                FilterExpr = data.FilterExpr != null ? Expr.Create(data.FilterExpr) : Expr.ALL;
                UpdateExpr = Expr.Create(data.UpdateExpr);
            }
            catch (Exception e)
            {
                ExcpName = e.GetType().Name;
                ExcpText = e.Message;
            }
        }
    }

    // ------------------------------------------------------------------------
    // ScenarioObj

    public class ScenarioDefObj : BaseUIObj
    {
        public string NameSpace { get; set; }
        public string ScenarioId { get; set; }
        public string RuleId { get; set; }
        public bool Disabled { get; set; }
        public int Priority { get; set; }
        public string RuleDesc { get; set; }
        public IExpression FilterExpr { get; set; }
        public string StressId { get; set; }

        public ScenarioDefObj(ICoreItem item)
            : base(item)
        {
            try
            {
                NameSpace = item.AppProps.GetValue<string>(EnvironmentProp.NameSpace, true); 
                var data = (ScenarioRule)item.Data;
                ScenarioId = data.ScenarioId;
                RuleId = data.RuleId;
                RuleDesc = data.RuleDesc;
                Disabled = data.Disabled;
                Priority = data.Priority;
                StressId = data.StressId;
                FilterExpr = data.FilterExpr != null ? Expr.Create(data.FilterExpr) : Expr.ALL;
            }
            catch (Exception e)
            {
                ExcpName = e.GetType().Name;
                ExcpText = e.Message;
            }
        }
    }

    // ------------------------------------------------------------------------
    // StressDef

    public enum StressDefColEnum
    {
        NameSpace,
        StressId,
        RuleId,
        Disabled,
        Priority,
        FilterExpr,
        UpdateExpr,
        Created,
        Expires,
        UniqueId,
        Build
    }

    internal class StressDefViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(StressDefColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((StressDefColEnum)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((StressDefColEnum)column)
            {
                //case StressDefColEnum.Market: return true;
                case StressDefColEnum.NameSpace: return true;
                case StressDefColEnum.StressId: return true;
                case StressDefColEnum.Disabled: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class StressDefDataHelper : IDataHelper<StressDefObj>
    {
        public string GetUniqueKey(StressDefObj data)
        {
            return data.UniqueId;
        }

        //private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        //{
        //    var delims = new[] { delim };
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

        public string GetDisplayValue(StressDefObj data, int column)
        {
            switch ((StressDefColEnum)column)
            {
                case StressDefColEnum.NameSpace: return data.NameSpace;
                case StressDefColEnum.UniqueId: return data.UniqueId;
                case StressDefColEnum.Build: return data.Build;
                case StressDefColEnum.StressId: return data.StressId;
                case StressDefColEnum.RuleId: return data.RuleId;
                case StressDefColEnum.Disabled: return data.Disabled.ToString(CultureInfo.InvariantCulture);
                case StressDefColEnum.Priority: return data.Priority.ToString(CultureInfo.InvariantCulture);
                case StressDefColEnum.FilterExpr: return data.FilterExpr.DisplayString();
                case StressDefColEnum.UpdateExpr: return data.UpdateExpr.DisplayString();
                case StressDefColEnum.Created: return data.Created.LocalDateTime.ToString("g");
                case StressDefColEnum.Expires: return data.Expires.LocalDateTime.ToString("g");
                default: return null;
            }
        }
    }

    internal class StressDefSorter : IComparer<StressDefObj>
    {
        //private static PeriodComparer _periodComparer = new PeriodComparer();

        public int Compare(StressDefObj a, StressDefObj b)
        {
            // sort order column priority

            // descending create time
            int result = DateTimeOffset.Compare(a.Created, b.Created);
            if (result != 0)
                return -1 * result;

            //// ascending currency
            //result = String.Compare(a.Currency, b.Currency, true);
            //if (result != 0)
            //    return result;

            //// ascending term
            //result = _PeriodComparer.Compare(a.Term, b.Term);
            //if (result != 0)
            //    return result;

            return result;
        }
    }

    public class StressDefSelecter : BaseSelecter<StressDefObj>
    {
        // this class is currently is a placeholder for future selection rules
        public StressDefSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<StressDefObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // CurveObj

    public class CurveObj : BaseUIObj
    {
        public string NameSpace { get; set; }
        public string MarketEnv { get; set; }
        public string MarketName { get; set; }
        public DateTime? MarketDate { get; set; }
        public string StressName { get; set; }
        public string CurveType { get; set; }
        public string CurveName { get; set; }
        public string CreditInstrumentId { get; set; }
        public string CreditSeniority { get; set; }
        public string IndexName { get; set; }
        public string IndexTenor { get; set; }
        public string Currency1 { get; set; }
        public string Currency2 { get; set; }

        public CurveObj(ICoreItem item)
            : base(item)
        {
            try
            {
                //Market data = (Market)item.Data;
                NameSpace = item.AppProps.GetValue<string>(EnvironmentProp.NameSpace, true);
                MarketEnv = item.AppProps.GetValue<string>(CurveProp.MarketAndDate, null);
                MarketName = item.AppProps.GetValue<string>(CurveProp.Market, true);
                MarketDate = item.AppProps.GetNullable<DateTime>(CurveProp.MarketDate);
                StressName = item.AppProps.GetValue<string>(CurveProp.StressName, null);
                CurveType = item.AppProps.GetValue<string>(CurveProp.PricingStructureType, null);
                Currency1 = item.AppProps.GetValue<string>(CurveProp.Currency1, null);
                Currency2 = item.AppProps.GetValue<string>(CurveProp.Currency2, null);
                CurveName = item.AppProps.GetValue<string>(CurveProp.CurveName, null);
                CreditInstrumentId = item.AppProps.GetValue<string>("CreditInstrumentId", null);
                CreditSeniority = item.AppProps.GetValue<string>("CreditSeniority", null);
                IndexName = item.AppProps.GetValue<string>(CurveProp.IndexName, null);
                IndexTenor = item.AppProps.GetValue<string>(CurveProp.IndexTenor, null);
            }
            catch (Exception e)
            {
                ExcpName = e.GetType().Name;
                ExcpText = e.Message;
            }
        }

        public string Instrument
        {
            get
            {
                // todo more curve types: fx etc.
                switch (CurveType.ToLower())
                {
                    case "discountcurve": return CreditInstrumentId;
                    default: return IndexName;
                }
            }
        }

        public string InstrTerm
        {
            get
            {
                // todo more curve types: fx etc.
                switch (CurveType.ToLower())
                {
                    case "discountcurve": return CreditSeniority;
                    default: return IndexTenor;
                }
            }
        }
    }

    // ------------------------------------------------------------------------
    // BaseCurve

    public enum BaseCurveColEnum
    {
        NameSpace,
        Market,
        MktDate,
        //Stress,
        CurveType,
        Publisher,
        Ccy1,
        Ccy2,
        Instrument,
        Term,
        CurveName,
        Created,
        Expires,
        UniqueId,
        Build,
        ErrType,
        ErrText
    }

    internal class BaseCurveViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(BaseCurveColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((BaseCurveColEnum)column).ToString();
        }

        public bool IsFilterColumn(int column)
        {
            switch ((BaseCurveColEnum)column)
            {
                case BaseCurveColEnum.NameSpace: return true;
                case BaseCurveColEnum.Market: return true;
                case BaseCurveColEnum.MktDate: return true;
                //case BaseCurveColEnum.Stress: return true;
                case BaseCurveColEnum.CurveType: return true;
                case BaseCurveColEnum.Publisher: return true;
                case BaseCurveColEnum.Instrument: return true;
                case BaseCurveColEnum.Term: return true;
                case BaseCurveColEnum.Ccy1: return true;
                case BaseCurveColEnum.Ccy2: return true;
                case BaseCurveColEnum.ErrType: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class BaseCurveDataHelper : IDataHelper<CurveObj>
    {
        public string GetUniqueKey(CurveObj data)
        {
            return data.UniqueId;
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

        public string GetDisplayValue(CurveObj data, int column)
        {
            switch ((BaseCurveColEnum)column)
            {
                case BaseCurveColEnum.NameSpace: return data.NameSpace;
                case BaseCurveColEnum.UniqueId: return data.UniqueId;
                case BaseCurveColEnum.Publisher: return data.Publisher;
                case BaseCurveColEnum.Build: return data.Build;
                case BaseCurveColEnum.Market: return data.MarketName;
                case BaseCurveColEnum.MktDate: return data.MarketDate?.ToShortDateString();
                //case BaseCurveColEnum.Stress: return data.StressName;
                case BaseCurveColEnum.CurveType: return data.CurveType;
                case BaseCurveColEnum.CurveName: return data.CurveName;
                case BaseCurveColEnum.Instrument: return data.Instrument;
                case BaseCurveColEnum.Term: return data.InstrTerm;
                case BaseCurveColEnum.Ccy1: return data.Currency1;
                case BaseCurveColEnum.Ccy2: return data.Currency2;
                case BaseCurveColEnum.Created: return data.Created.LocalDateTime.ToString("g");
                case BaseCurveColEnum.Expires: return data.Expires.LocalDateTime.ToString("g");
                case BaseCurveColEnum.ErrType: return data.ExcpName;
                case BaseCurveColEnum.ErrText: return data.ExcpText;
                default: return null;
            }
        }
    }

    internal class BaseCurveSorter : IComparer<CurveObj>
    {
        public int Compare(CurveObj a, CurveObj b)
        {
            // sort order column priority

            // descending create time
            int result = DateTimeOffset.Compare(a.Created, b.Created);
            if (result != 0)
                return -1 * result;

            //// ascending currency
            //result = String.Compare(a.Currency, b.Currency, true);
            //if (result != 0)
            //    return result;

            //// ascending term
            //result = _PeriodComparer.Compare(a.Term, b.Term);
            //if (result != 0)
            //    return result;

            return result;
        }
    }

    public class BaseCurveSelecter : BaseSelecter<CurveObj>
    {
        // this class is currently is a placeholder for future selection rules
        public BaseCurveSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<CurveObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // StressCurve

    public enum StressCurveColEnum
    {
        NameSpace,
        Market,
        Stress,
        CurveType,
        Publisher,
        Ccy1,
        Ccy2,
        Instrument,
        Term,
        CurveName,
        Created,
        Expires,
        UniqueId,
        Build,
        ErrType,
        ErrText
    }

    internal class StressCurveViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(StressCurveColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((StressCurveColEnum)column).ToString();
        }

        public bool IsFilterColumn(int column)
        {
            switch ((StressCurveColEnum)column)
            {
                case StressCurveColEnum.NameSpace: return true;
                case StressCurveColEnum.Market: return true;
                case StressCurveColEnum.Stress: return true;
                case StressCurveColEnum.CurveType: return true;
                case StressCurveColEnum.Publisher: return true;
                case StressCurveColEnum.Instrument: return true;
                case StressCurveColEnum.Term: return true;
                case StressCurveColEnum.Ccy1: return true;
                case StressCurveColEnum.Ccy2: return true;
                case StressCurveColEnum.ErrType: return true;
                default:
                    return false;
            }
        }

        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class StressCurveDataHelper : IDataHelper<CurveObj>
    {
        public string GetUniqueKey(CurveObj data)
        {
            return data.UniqueId;
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

        public string GetDisplayValue(CurveObj data, int column)
        {
            switch ((StressCurveColEnum)column)
            {
                case StressCurveColEnum.NameSpace: return data.NameSpace;
                case StressCurveColEnum.UniqueId: return data.UniqueId;
                case StressCurveColEnum.Publisher: return data.Publisher;
                case StressCurveColEnum.Build: return data.Build;
                case StressCurveColEnum.Market: return data.MarketEnv;
                case StressCurveColEnum.Stress: return data.StressName;
                case StressCurveColEnum.CurveType: return data.CurveType;
                case StressCurveColEnum.CurveName: return data.CurveName;
                case StressCurveColEnum.Instrument: return data.Instrument;
                case StressCurveColEnum.Term: return data.InstrTerm;
                case StressCurveColEnum.Ccy1: return data.Currency1;
                case StressCurveColEnum.Ccy2: return data.Currency2;
                case StressCurveColEnum.Created: return data.Created.LocalDateTime.ToString("g");
                case StressCurveColEnum.Expires: return data.Expires.LocalDateTime.ToString("g");
                case StressCurveColEnum.ErrType: return data.ExcpName;
                case StressCurveColEnum.ErrText: return data.ExcpText;
                default: return null;
            }
        }
    }

    internal class StressCurveSorter : IComparer<CurveObj>
    {
        public int Compare(CurveObj a, CurveObj b)
        {
            // sort order column priority

            // descending create time
            int result = DateTimeOffset.Compare(a.Created, b.Created);
            if (result != 0)
                return -1 * result;

            //// ascending currency
            //result = String.Compare(a.Currency, b.Currency, true);
            //if (result != 0)
            //    return result;

            //// ascending term
            //result = _PeriodComparer.Compare(a.Term, b.Term);
            //if (result != 0)
            //    return result;

            return result;
        }
    }

    public class StressCurveSelecter : BaseSelecter<CurveObj>
    {
        // this class is currently is a placeholder for future selection rules
        public StressCurveSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<CurveObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // ScenarioDef

    public enum ScenarioDefColEnum
    {
        NameSpace,
        ScenarioId,
        RuleId,
        Disabled,
        Priority,
        RuleDesc,
        FilterExpr,
        StressId,
        Created,
        Expires,
        UniqueId,
        Build
    }

    internal class ScenarioDefViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(ScenarioDefColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((ScenarioDefColEnum)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((ScenarioDefColEnum)column)
            {
                //case ScenarioDefColEnum.Market: return true;
                case ScenarioDefColEnum.NameSpace: return true;
                case ScenarioDefColEnum.ScenarioId: return true;
                case ScenarioDefColEnum.Disabled: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class ScenarioDefDataHelper : IDataHelper<ScenarioDefObj>
    {
        public string GetUniqueKey(ScenarioDefObj data)
        {
            return data.UniqueId;
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

        public string GetDisplayValue(ScenarioDefObj data, int column)
        {
            switch ((ScenarioDefColEnum)column)
            {
                case ScenarioDefColEnum.NameSpace: return data.NameSpace;
                case ScenarioDefColEnum.UniqueId: return data.UniqueId;
                case ScenarioDefColEnum.Build: return data.Build;
                case ScenarioDefColEnum.ScenarioId: return data.ScenarioId;
                case ScenarioDefColEnum.RuleId: return data.RuleId;
                case ScenarioDefColEnum.Disabled: return data.Disabled.ToString(CultureInfo.InvariantCulture);
                case ScenarioDefColEnum.Priority: return data.Priority.ToString(CultureInfo.InvariantCulture);
                case ScenarioDefColEnum.FilterExpr: return data.FilterExpr.DisplayString();
                case ScenarioDefColEnum.RuleDesc: return data.RuleDesc;
                case ScenarioDefColEnum.StressId: return data.StressId;
                case ScenarioDefColEnum.Created: return data.Created.LocalDateTime.ToString("g");
                case ScenarioDefColEnum.Expires: return data.Expires.LocalDateTime.ToString("g");
                default: return null;
            }
        }
    }

    internal class ScenarioDefSorter : IComparer<ScenarioDefObj>
    {
        //private static PeriodComparer _periodComparer = new PeriodComparer();

        public int Compare(ScenarioDefObj a, ScenarioDefObj b)
        {
            // sort order column priority

            // descending create time
            int result = DateTimeOffset.Compare(a.Created, b.Created);
            if (result != 0)
                return -1 * result;

            //// ascending currency
            //result = String.Compare(a.Currency, b.Currency, true);
            //if (result != 0)
            //    return result;

            //// ascending term
            //result = _PeriodComparer.Compare(a.Term, b.Term);
            //if (result != 0)
            //    return result;

            return result;
        }
    }

    public class ScenarioDefSelecter : BaseSelecter<ScenarioDefObj>
    {
        // this class is currently is a placeholder for future selection rules
        public ScenarioDefSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<ScenarioDefObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

}
