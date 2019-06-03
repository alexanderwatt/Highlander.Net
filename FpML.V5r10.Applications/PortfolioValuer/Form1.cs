using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Core.Common;
using Core.V34;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Orion.Build;
using Orion.Util.Caching;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.Util.WinTools;
using Orion.Constants;
using Orion.Contracts;
using Orion.PortfolioValuation;
using Orion.Workflow;
using Exception = System.Exception;

namespace Orion.PortfolioValuer
{
    public partial class Form1 : Form
    {
        private static readonly EnvId BuildEnv = EnvHelper.ParseEnvName(BuildConst.BuildEnv);

        private Reference<ILogger> _logRef;
        private ICoreClient _client;
        private ICoreCache _cache;
        private readonly SynchronizationContext _syncContext;
        //private Dictionary<string, string> _counterparties = new Dictionary<string, string>();

        // all trades grid
        private IListViewHelper<TradeObj> _tradeAllView;
        private IViewHelper _tradeAllViewHelper;
        private IDataHelper<TradeObj> _tradeAllDataHelper;
        private ISelecter<TradeObj> _tradeAllSelecter;
        private IFilterGroup _tradeAllFilters;
        private ICoreCache _tradeAllCache;

        // selected trades grid
        private IListViewHelper<TradeObj> _tradeSelView;
        private IViewHelper _tradeSelViewHelper;
        private IDataHelper<TradeObj> _tradeSelDataHelper;
        private ISelecter<TradeObj> _tradeSelSelecter;
        private IFilterGroup _tradeSelFilters;

        // curves grid
        private IListViewHelper<CurveObj> _curveSelView;
        private IViewHelper _curveSelViewHelper;
        private IDataHelper<CurveObj> _curveSelDataHelper;
        private ISelecter<CurveObj> _curveSelSelecter;
        private IFilterGroup _curveSelFilters;
        private ICoreCache _curveSelCache;

        // valuations grid
        private IListViewHelper<ValueRawObj> _valueRawView;
        private IViewHelper _valueRawViewHelper;
        private IDataHelper<ValueRawObj> _valueRawDataHelper;
        private ISelecter<ValueRawObj> _valueRawSelecter;
        private IFilterGroup _valueRawFilters;
        private ISubscription _valueRawSubs;

        // portfolio grid
        private IListViewHelper<PortfolioObj> _portfolioView;
        private IViewHelper _portfolioViewHelper;
        private IDataHelper<PortfolioObj> _portfolioDataHelper;
        private ISelecter<PortfolioObj> _portfolioSelecter;
        private IFilterGroup _portfolioFilters;
        //private GuardedDictionary<string, PortfolioObj> _portfolioObjects = new GuardedDictionary<string, PortfolioObj>();
        private ICoreCache _portfolioCache;

        public Form1()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;
        }

        private void StartUp()
        {
            try
            {
                var factory = new CoreClientFactory(_logRef)
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
                _syncContext.Post(OnClientStateChange, new CoreStateChange(CoreStateEnum.Initial, _client.CoreState));
                _client.OnStateChange += _Client_OnStateChange;
            }
            catch (Exception excp)
            {
                _logRef.Target.Log(excp);
            }
        }

        void _Client_OnStateChange(CoreStateChange update)
        {
            _syncContext.Post(OnClientStateChange, update);
        }

        private void OnClientStateChange(object state)
        {
            var update = (CoreStateChange)state;
            _logRef.Target.LogInfo("Old state is: {0}", update.OldState.ToString());
            _logRef.Target.LogInfo("New state is: {0}", update.NewState.ToString());
        }

        private void Form1Load(object sender, EventArgs e)
        {
            // create loggers
            _logRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));
            //
            // init controls
            // - form title
            WinFormHelper.SetAppFormTitle(this, EnvHelper.EnvName(BuildEnv));
            // - base date
            dtpBaseDate.Value = DateTime.Today;
            // - market date
            dtpMarketQRDate.Value = DateTime.Today.AddDays(-1);
            //
            // loading
            StartUp();
            //
            //debug
            Application.DoEvents();
            //enddebug
            //
            // setup the all trades view
            _tradeAllViewHelper = new TradeAllViewHelper();
            _tradeAllDataHelper = new TradeAllDataHelper();
            _tradeAllFilters = new ComboxBoxFilterGroup(
                pnlTradeAll, _tradeAllViewHelper, TradeAllSelectionChanged);
            _tradeAllSelecter = new TradeAllSelecter(
                _tradeAllFilters, _tradeAllViewHelper, _tradeAllDataHelper);
            _tradeAllView = new ListViewManager<TradeObj>(
                _logRef.Target, lvTradeAll, _tradeAllViewHelper,
                _tradeAllSelecter, _tradeAllFilters, new TradeAllSorter(), _tradeAllDataHelper);
            //
            // setup the selected trades view
            _tradeSelViewHelper = new TradeSelViewHelper();
            _tradeSelDataHelper = new TradeSelDataHelper();
            _tradeSelFilters = new ComboxBoxFilterGroup(
                pnlTradeSel, _tradeSelViewHelper, TradeSelSelectionChanged);
            _tradeSelSelecter = new TradeSelSelecter(
                _tradeSelFilters, _tradeSelViewHelper, _tradeSelDataHelper);
            _tradeSelView = new ListViewManager<TradeObj>(
                _logRef.Target, lvTradeSel, _tradeSelViewHelper,
                _tradeSelSelecter, _tradeSelFilters, new TradeSelSorter(), _tradeSelDataHelper);
            //
            // setup the selected trades curve view
            _curveSelViewHelper = new CurveSelViewHelper();
            _curveSelDataHelper = new CurveSelDataHelper();
            _curveSelFilters = new ComboxBoxFilterGroup(
                pnlCurveRaw, _curveSelViewHelper, CurveSelSelectionChanged);
            _curveSelSelecter = new CurveSelSelecter(
                _curveSelFilters, _curveSelViewHelper, _curveSelDataHelper);
            _curveSelView = new ListViewManager<CurveObj>(
                _logRef.Target, lvCurveRaw, _curveSelViewHelper,
                _curveSelSelecter, _curveSelFilters, new CurveSelSorter(), _curveSelDataHelper);
            //
            // setup the request portfolio view
            _portfolioViewHelper = new PortfolioViewHelper();
            _portfolioDataHelper = new PortfolioDataHelper();
            _portfolioFilters = new ComboxBoxFilterGroup(
                panelPortfolio, _portfolioViewHelper, PortfolioSelectionChanged);
            _portfolioSelecter = new PortfolioSelecter(
                _portfolioFilters, _portfolioViewHelper, _portfolioDataHelper);
            _portfolioView = new ListViewManager<PortfolioObj>(
                _logRef.Target, lvPortfolio, _portfolioViewHelper,
                _portfolioSelecter, _portfolioFilters, new PortfolioSorter(), _portfolioDataHelper);
            //
            // setup the raw valuations view
            // note: valuations where AggregationType == null
            _valueRawViewHelper = new ValueRawViewHelper();
            _valueRawDataHelper = new ValueRawDataHelper();
            _valueRawFilters = new ComboxBoxFilterGroup(
                panelValueRaw, _valueRawViewHelper, ValueRawSelectionChanged);
            _valueRawSelecter = new ValueRawSelecter(
                _valueRawFilters, _valueRawViewHelper, _valueRawDataHelper);
            _valueRawView = new ListViewManager<ValueRawObj>(
                _logRef.Target, lvValueRaw, _valueRawViewHelper,
                _valueRawSelecter, _valueRawFilters, new ValueRawSorter(), _valueRawDataHelper);
            //
            // - trades
            _tradeAllCache = _client.CreateCache(
                update => _tradeAllView.UpdateData(new ViewChangeNotification<TradeObj>
                                                       {
                                                           Change = update.Change,
                                                           OldData =
                                                               update.OldItem != null
                                                                   ? new TradeObj(update.OldItem)
                                                                   : null,
                                                           NewData =
                                                               update.NewItem != null
                                                                   ? new TradeObj(update.NewItem)
                                                                   : null
                                                       }), SynchronizationContext.Current);
            _tradeAllCache.SubscribeInfoOnly<Trade>(Expr.ALL);
            //
            // - curves
            _curveSelCache = _client.CreateCache(
                update => _curveSelView.UpdateData(new ViewChangeNotification<CurveObj>
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
            //
            //Only subscribe to the live curves i.e. not the configurations
            //The default market is QR_LIVE
            var market = textBox1.Text;
            //var query = Expr.BoolAND(
            //    Expr.IsEQU(EnvironmentProp.Function, FunctionProp.Market),
            //    Expr.IsEQU(CurveProp.Market, market));
            var query = Expr.IsEQU(CurveProp.Market, market);
            _curveSelCache.SubscribeNoWait<Market>(query, null, null);
            //Added the confirmation trades as well.
            //_tradeAllCache.SubscribeInfoOnly<FpML.V5r3.Confirmation.Trade>(Expr.ALL);
            // - portfolios
            _portfolioCache = _client.CreateCache(
                update => _portfolioView.UpdateData(new ViewChangeNotification<PortfolioObj>
                                                        {
                                                            Change = update.Change,
                                                            OldData =
                                                                update.OldItem != null
                                                                    ? new PortfolioObj(update.OldItem)
                                                                    : null,
                                                            NewData =
                                                                update.NewItem != null
                                                                    ? new PortfolioObj(update.NewItem)
                                                                    : null
                                                        }), SynchronizationContext.Current);
            _portfolioCache.SubscribeNoWait<PortfolioSpecification>(Expr.ALL, null, null);
            _portfolioCache.SubscribeNoWait<HandlerResponse>(Expr.ALL, null, null);
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeHelper.SafeDispose(ref _valueRawSubs);
            DisposeHelper.SafeDispose(ref _tradeAllCache);
            DisposeHelper.SafeDispose(ref _portfolioCache);
            DisposeHelper.SafeDispose(ref _curveSelCache);
            DisposeHelper.SafeDispose(ref _cache);
            DisposeHelper.SafeDispose(ref _client);
        }

        void TradeAllSelectionChanged(object sender, EventArgs e)
        {
            _tradeAllView.RebuildView();
        }

        void TradeSelSelectionChanged(object sender, EventArgs e)
        {
            _tradeSelView.RebuildView();
        }

        void CurveSelSelectionChanged(object sender, EventArgs e)
        {
            _curveSelView.RebuildView();

        }

        void ValueRawSelectionChanged(object sender, EventArgs e)
        {
            _valueRawView.RebuildView();
        }

        void PortfolioSelectionChanged(object sender, EventArgs e)
        {
            _portfolioView.RebuildView();
        }

        private void BtnTradeCalcValuationsClick(object sender, EventArgs e)
        {
            TradeCalcValuations(false);
        }

        private void TradeCalcValuations(bool realTimeFlag)
        {
            // publish portfolio spec 1st
            var retention = TimeSpan.FromHours(Convert.ToDouble(nudRetainResultsHours.Value));
            var portfolioId = Guid.NewGuid().ToString();
            var tradeList = _tradeSelView.DataItems.Select(trade => trade.UniqueId).ToArray();
            var portfolio = new PortfolioSpecification
            {
                NameSpace = cbClientNameSpace.Text,
                PortfolioId = portfolioId,
                OwnerId = new UserIdentity
                {
                    Name = _client.ClientInfo.Name,
                    DisplayName = _client.ClientInfo.UserFullName
                },
                Description = "Ad-hoc (PV Test Harness)",
                ExcludeOverridesInclude = false,
                IncludedTradeItemNames = tradeList
            };
            _cache.SaveObject(portfolio, retention);
            //
            // create portfolio valuation request
            DateTime? marketDate = null;
            if (rbMarketQREODDated.Checked)
            {
                marketDate = dtpMarketQRDate.Value;
            }
            var marketName = CurveConst.QR_LIVE;
            if (rbMarketSydneyLive.Checked)
                marketName = CurveConst.SYD_LIVE;
            if (rbMarketQREODLatest.Checked || rbMarketQREODDated.Checked)
                marketName = CurveConst.QR_EOD;
            var requestId = Guid.NewGuid();
            //
            //Create the request
            var request = new PortfolioValuationRequest
            {
                NameSpace = cbClientNameSpace.Text,
                BaseParty = comboBoxBaseParty.Text,
                BaseDate = dtpBaseDate.Value.Date,
                RequestId = requestId.ToString(),
                Retention = retention.ToString(),
                SubmitTime = DateTimeOffset.Now.ToString("o"),
                RequestDescription = portfolio.Description,
                RequesterId = new UserIdentity
                {
                    Name = _client.ClientInfo.Name,
                    DisplayName = _client.ClientInfo.UserFullName
                },
                PortfolioId = portfolioId,
                MarketDate = marketDate,
                MarketName = marketName,
                ReportingCurrency = comboBoxReportingCurrency.Text,
                IRScenarioNames = chkCalcAllIRStresses.Checked ? ScenarioConst.AllIrScenarioIds : null,
                FXScenarioNames = chkCalcAllFXStresses.Checked ? ScenarioConst.AllFxScenarioIds : null,
                RealTimePricing = realTimeFlag
            };
            if (chkRunAtServer.Checked)
            {
                // send request to server
                _cache.SaveObject(request, retention);
                // send to request manager
                RequestBase.DispatchToManager(_client, request);
                _logRef.Target.LogDebug("Request '{0}' sent to server", requestId);
            }
            else
            {
                // run locally on background worker thread
                ThreadPool.QueueUserWorkItem(BackgroundTradeValuation, request);
            }
            // now start valuation subscriptions
            // - raw valuations
            DisposeHelper.SafeDispose(ref _valueRawSubs);
            _valueRawSubs = _client.Subscribe<ValuationReport>(
                Expr.BoolAND(
                    Expr.IsEQU(RequestBase.Prop.RequestId, requestId),
                    Expr.IsNull(ValueProp.Aggregation)),
                (subscription, item) =>
                {
                    _valueRawView?.UpdateData(new ViewChangeNotification<ValueRawObj>
                    {
                        Change = CacheChange.ItemCreated,
                        NewData = new ValueRawObj(item)
                    });
                },
                null);
        }

        private void BackgroundTradeValuation(object state)
        {
            ILogger logger = new FilterLogger(_logRef.Target, "Background calculation: ", LogSeverity.Debug);
            logger.LogDebug("Starting...");
            try
            {
                IWorkContext context = new WorkContext(logger, _cache, GetType().Name);
                var request = (PortfolioValuationRequest)state;
                using (var workflow = new WFCalculatePortfolioStressValuation())
                {
                    workflow.Initialise(context);
                    WorkflowOutput<HandlerResponse> output = workflow.Execute(request);
                    //HandlerResponse result = output.Result;
                    WorkflowHelper.LogErrors(logger, output.Errors);
                }
            }
            catch (Exception excp)
            {
                logger.Log(excp);
            }
            finally
            {
                logger.LogDebug("Completed.");
            }
        }

        private void BackgroundTradeHistoricalValuation(object state)
        {
            ILogger logger = new FilterLogger(_logRef.Target, "Background calculation: ", LogSeverity.Debug);
            logger.LogDebug("Starting...");
            try
            {
                IWorkContext context = new WorkContext(logger, _cache, GetType().Name);
                var request = (PortfolioValuationRequest)state;
                using (var workflow = new WFCalculatePortfolioHistoricalValuation())
                {
                    workflow.Initialise(context);
                    WorkflowOutput<HandlerResponse> output = workflow.Execute(request);
                    //HandlerResponse result = output.Result;
                    WorkflowHelper.LogErrors(logger, output.Errors);
                }
            }
            catch (Exception excp)
            {
                logger.Log(excp);
            }
            finally
            {
                logger.LogDebug("Completed.");
            }
        }

        private void BtnTradeAddAllClick1(object sender, EventArgs e)
        {
            foreach (var tradeObj in _tradeAllView.DataItems)
                _tradeSelView.UpdateData(new ViewChangeNotification<TradeObj>
                                             {
                    Change = CacheChange.ItemCreated,
                    OldData = null,
                    NewData = tradeObj
                });
        }

        private void BtnTradeAddSelectedClick1(object sender, EventArgs e)
        {
            foreach (var tradeObj in _tradeAllView.SelectedDataItems)
                _tradeSelView.UpdateData(new ViewChangeNotification<TradeObj>
                                             {
                    Change = CacheChange.ItemCreated,
                    OldData = null,
                    NewData = tradeObj
                });
        }

        private void LvTradeAllDoubleClick1(object sender, EventArgs e)
        {
            foreach (var tradeObj in _tradeAllView.SelectedDataItems)
                _tradeSelView.UpdateData(new ViewChangeNotification<TradeObj>
                                             {
                    Change = CacheChange.ItemCreated,
                    OldData = null,
                    NewData = tradeObj
                });
        }

        private void BtnTradeRemoveSelectedClick1(object sender, EventArgs e)
        {
            foreach (var tradeObj in _tradeSelView.SelectedDataItems)
                _tradeSelView.UpdateData(new ViewChangeNotification<TradeObj>
                                             {
                    Change = CacheChange.ItemRemoved,
                    OldData = tradeObj,
                    NewData = null
                });
        }

        private void BtnTradeClearAllClick1(object sender, EventArgs e)
        {
            _tradeSelView.Clear();
        }

        private void LvTradeSelDoubleClick(object sender, EventArgs e)
        {
            foreach (TradeObj tradeObj in _tradeSelView.SelectedDataItems)
                _tradeSelView.UpdateData(new ViewChangeNotification<TradeObj>
                                             {
                    Change = CacheChange.ItemRemoved,
                    OldData = tradeObj,
                    NewData = null
                });
        }

        private void BtnTradeHistoricalValuation(object sender, EventArgs e)
        {
            TradeHistoricalValuation(false);
        }

        private void TradeHistoricalValuation(bool realTimeFlag)
        {
            // publish portfolio spec 1st
            var retention = TimeSpan.FromHours(Convert.ToDouble(nudRetainResultsHours.Value));
            var portfolioId = Guid.NewGuid().ToString();
            var portfolio = new PortfolioSpecification
            {
                NameSpace = cbClientNameSpace.Text,
                PortfolioId = portfolioId,
                OwnerId = new UserIdentity
                {
                    Name = _client.ClientInfo.Name,
                    DisplayName = _client.ClientInfo.UserFullName
                },
                Description = "Ad-hoc (PV Test Harness)",
                ExcludeOverridesInclude = false,
                IncludedTradeItemNames = _tradeSelView.DataItems.Select(trade => trade.UniqueId).ToArray()
            };
            _cache.SaveObject(portfolio, retention);
            var marketName = CurveConst.QR_LIVE;
            if (rbMarketSydneyLive.Checked)
                marketName = CurveConst.SYD_LIVE;
            if (rbMarketQREODLatest.Checked || rbMarketQREODDated.Checked)
                marketName = CurveConst.QR_EOD;
            // create portfolio valuation request
            DateTime? marketDate = null;
            List<DateTime> dateList = null;
            if (rbMarketQREODDated.Checked)
            {
                marketDate = dtpMarketQRDate.Value;
                //
                //Collect any dates required
                //
                DateTime? marketDateEnd = null;
                if ((rbMarketQREODDated.Checked) && cbMarketDateEnd.Checked)
                    marketDateEnd = dtpMarketEndDate.Value;
                if (marketDateEnd != null)
                {
                    IExpression whereExpr = Expr.IsGEQ(CurveProp.MarketDate, marketDate);//THe default start today is today.
                    whereExpr = Expr.BoolAND(Expr.IsLEQ(CurveProp.MarketDate, marketDateEnd), whereExpr);//Less than the end date.
                    whereExpr = Expr.BoolAND(Expr.IsEQU(CurveProp.Market, marketName), whereExpr);
                    List<ICoreItem> items = _client.LoadItems(typeof(Market), whereExpr);
                    dateList = items.Select(item => item.AppProps.GetValue(CurveProp.MarketDate, new DateTime())).ToList();
                }
            }
            var requestId = Guid.NewGuid();
            //
            //Create the request
            var request = new PortfolioValuationRequest
            {
                NameSpace = cbClientNameSpace.Text,
                BaseParty = comboBoxBaseParty.Text,
                BaseDate = dtpBaseDate.Value.Date,
                RequestId = requestId.ToString(),
                Retention = retention.ToString(),
                SubmitTime = DateTimeOffset.Now.ToString("o"),
                RequestDescription = portfolio.Description,
                RequesterId = new UserIdentity
                {
                    Name = _client.ClientInfo.Name,
                    DisplayName = _client.ClientInfo.UserFullName
                },
                PortfolioId = portfolioId,
                MarketDate = marketDate,
                MarketName = marketName,
                ReportingCurrency = comboBoxReportingCurrency.Text,
                RealTimePricing = realTimeFlag
            };
            //
            //Set the dates if not null
            if (dateList != null)
            {
                dateList = new List<DateTime>(dateList.Distinct());
                request.DateScenarios = dateList.ToArray();
            }
            if (chkRunAtServer.Checked)
            {
                // send request to server
                _cache.SaveObject(request, retention);
                // send to request manager
                RequestBase.DispatchToManager(_client, request);
                _logRef.Target.LogDebug("Request '{0}' sent to server", requestId);
            }
            else
            {
                // run locally on background worker thread
                ThreadPool.QueueUserWorkItem(BackgroundTradeHistoricalValuation, request);
            }
            // now start valuation subscriptions
            // - raw valuations
            DisposeHelper.SafeDispose(ref _valueRawSubs);
            _valueRawSubs = _client.Subscribe<ValuationReport>(
                Expr.BoolAND(
                    Expr.IsEQU(RequestBase.Prop.RequestId, requestId),
                    Expr.IsNull(ValueProp.Aggregation)),
                (subscription, item) =>
                {
                    _valueRawView?.UpdateData(new ViewChangeNotification<ValueRawObj>
                    {
                        Change = CacheChange.ItemCreated,
                        NewData = new ValueRawObj(item)
                    });
                },
                null);
        }

        private void BtnAutoValueClick(object sender, EventArgs e)
        {
            _logRef.Target.LogDebug("AutoValue on");
            // 1. Firstly value each trade to get a starting vlaue.
            //
            TradeCalcValuations(true);
            // 2. Find the relevant curves required
            //
            var curves = new List<string>();
            foreach (var trade in _tradeSelView.DataItems)
            {
                if (trade.Curves == null) continue;
                foreach (var curve in trade.Curves)
                {
                    var newcurve = "Market." + CurveConst.QR_LIVE + "." + curve;
                    if (!curves.Contains(newcurve))
                    {
                        curves.Add(newcurve);
                    }
                }
            }
            // 3. Set up the subscription for the curves.
            //
            var query1 = curves.Aggregate<object, IExpression>(null,
                                                                       (current, curve) =>
                                                                       Expr.BoolOR(current,
                                                                                   Expr.IsEQU(
                                                                                       CurveProp.UniqueIdentifier, curve)));
            query1 = Expr.BoolAND(query1, Expr.IsEQU(EnvironmentProp.NameSpace, cbClientNameSpace.Text));
            //
            // 4. Create a delegate to trigger the valuation on a curve update.
            //
            ISubscription newSubscription = _client.CreateSubscription<Market>(query1);
            newSubscription.UserCallback = delegate(ISubscription subscription, ICoreItem item)
            {
                // note: this is running on a thread pool thread
                _logRef.Target.LogDebug("SubscribeCallback: Queued calls posted:");
                _syncContext.Post(ValueTradeSet, item); //chnanged from null
            };
            newSubscription.Start();
            _logRef.Target.LogDebug("Subscription started.");
            // now start valuation subscriptions
            // - raw valuations
            DisposeHelper.SafeDispose(ref _valueRawSubs);
            //TODO
            //Remove some of the valuation reports subscribed to.
            //
            _valueRawSubs = _client.Subscribe<ValuationReport>(
                Expr.BoolAND(
                    Expr.IsEQU(ValueProp.RealTimePricing, true),
                    Expr.IsEQU(EnvironmentProp.NameSpace, cbClientNameSpace.Text),
                    Expr.IsNull(ValueProp.Aggregation)),
                (subscription, item) =>
                {
                    _valueRawView?.UpdateData(new ViewChangeNotification<ValueRawObj>
                    {
                        Change = CacheChange.ItemCreated,
                        NewData = new ValueRawObj(item)
                    });
                },
                null);
        }

        private void ValueTradeSet(object curveItem)
        {
            // 1. Get the trades in the selection that are dependent on the curve updated.
            //
            var trades = new List<string>();
            var updatedCurve = curveItem as ICoreItem;
            if (updatedCurve != null)
            {
                var identifier = updatedCurve.AppProps.GetValue<string>(CurveProp.UniqueIdentifier);
                foreach (var trade in _tradeSelView.DataItems)
                {
                    if (trade.Curves == null) continue;
                    foreach (var curve in trade.Curves)
                    {
                        var newcurve = "Market." + CurveConst.QR_LIVE + "." + curve;
                        if (newcurve != identifier) continue;
                        if (!trades.Contains(trade.UniqueId))
                        {
                            trades.Add(trade.UniqueId);
                        }
                    }
                }
                _logRef.Target.LogDebug("Valuation trade set: {0}",
                                        trades.ToString());
            }
            //
            // 2. Create the valuation request
            //
            // publish portfolio spec 1st
            var retention = TimeSpan.FromHours(Convert.ToDouble(nudRetainResultsHours.Value));
            var portfolioId = Guid.NewGuid().ToString();
            var portfolio = new PortfolioSpecification
            {
                NameSpace = cbClientNameSpace.Text,
                PortfolioId = portfolioId,
                OwnerId = new UserIdentity
                {
                    Name = _client.ClientInfo.Name,
                    DisplayName = _client.ClientInfo.UserFullName
                },
                Description = "Ad-hoc (PV Test Harness)",
                ExcludeOverridesInclude = false,
                IncludedTradeItemNames = trades.ToArray()
            };
            _cache.SaveObject(portfolio, retention);
            //
            // 3. Create the request
            //
            var requestId = Guid.NewGuid();
            var request = new PortfolioValuationRequest
            {
                NameSpace = cbClientNameSpace.Text,
                BaseParty = comboBoxBaseParty.Text,
                BaseDate = dtpBaseDate.Value.Date,
                RequestId = requestId.ToString(),
                Retention = retention.ToString(),
                SubmitTime = DateTimeOffset.Now.ToString("o"),
                RequestDescription = portfolio.Description,
                RequesterId = new UserIdentity
                {
                    Name = _client.ClientInfo.Name,
                    DisplayName = _client.ClientInfo.UserFullName
                },
                PortfolioId = portfolioId,
                MarketName = CurveConst.QR_LIVE,
                ReportingCurrency = comboBoxReportingCurrency.Text,
                RealTimePricing = true
            };
            // 4. Then run locally on background worker thread
            //
            ThreadPool.QueueUserWorkItem(BackgroundAutoTradeValuation, request);
            _logRef.Target.LogDebug("Valuation started.");
        }

        private void BackgroundAutoTradeValuation(object state)
        {
            ILogger logger = new FilterLogger(_logRef.Target, "Background calculation: ", LogSeverity.Debug);
            logger.LogDebug("Starting...");
            try
            {
                IWorkContext context = new WorkContext(logger, _cache, GetType().Name);
                var request = (PortfolioValuationRequest)state;
                using (var workflow = new WFCalculatePortfolioStressValuation())
                {
                    workflow.Initialise(context);
                    WorkflowOutput<HandlerResponse> output = workflow.Execute(request);
                    //HandlerResponse result = output.Result;
                    WorkflowHelper.LogErrors(logger, output.Errors);
                }
            }
            catch (Exception excp)
            {
                logger.Log(excp);
            }
            finally
            {
                logger.LogDebug("Completed.");
            }
        }

        private void BtnStopBuildClick(object sender, EventArgs e)
        {
            _logRef.Target.LogDebug("AutoValue and all updating Off");
            _client.UnsubscribeAll();
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
            // workflow exceptions
            ExcpName = item.AppProps.GetValue<string>(WFPropName.ExcpName, null);
            ExcpText = item.AppProps.GetValue<string>(WFPropName.ExcpText, null);
        }
    }

    // ------------------------------------------------------------------------
    // Trade object

    public class TradeObj : BaseUIObj
    {
        public string NameSpace { get; set; }
        public string Schema { get; set; }
        public string SourceSystem { get; set; }
        public string TradeSource { get; set; }
        public string ProductTaxonomy { get; set; }
        public string TradeId { get; set; }
        public string ProductType { get; set; }//public ProductTypeSimpleEnum ProductType { get; set; }
        public string TradeType { get; set; }
        public TradeState TradeState { get; set; }
        public string Party1 { get; set; }
        public string BaseParty { get; set; }
        public string Party2 { get; set; }
        public string CounterPartyName { get; set; }
        //public string TradingBookId { get; set; }
        public string TradingBookName { get; set; }
        public string Currencies { get; set; }
        public string[] Curves { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime AsAtDate { get; set; }

        public TradeObj(ICoreItem item): base(item)
        {
            try
            {
                // trade props
                NameSpace = item.AppProps.GetValue(EnvironmentProp.NameSpace, EnvironmentProp.DefaultNameSpace);
                Schema = item.AppProps.GetValue<string>(EnvironmentProp.Schema, null);
                SourceSystem = item.AppProps.GetValue<string>(EnvironmentProp.SourceSystem, null);
                TradeSource = item.AppProps.GetValue<string>(TradeProp.TradeSource, null);
                ProductTaxonomy = item.AppProps.GetValue<string>(TradeProp.ProductTaxonomy, null);
                TradeId = item.AppProps.GetValue<string>(TradeProp.TradeId, null);
                TradeType = EnumHelper.Parse<ItemChoiceType15>(item.AppProps.GetValue<string>(TradeProp.TradeType)).ToString();
                ProductType = item.AppProps.GetValue(TradeProp.ProductType, ProductTypeSimpleEnum.Undefined.ToString());//EnumHelper.Parse<ProductTypeSimpleEnum>(
                ProductType = item.AppProps.GetValue<string>(TradeProp.ProductType);
                TradeState = EnumHelper.Parse(item.AppProps.GetValue<string>(TradeProp.TradeState, null), true, TradeState.Undefined);
                Currencies = String.Join(",", item.AppProps.GetArray<string>(TradeProp.RequiredCurrencies));
                Curves = item.AppProps.GetArray<string>(TradeProp.RequiredPricingStructures);
                EffectiveDate = item.AppProps.GetValue(TradeProp.EffectiveDate, DateTime.MinValue);
                MaturityDate = item.AppProps.GetValue(TradeProp.MaturityDate, DateTime.MinValue);
                TradeDate = item.AppProps.GetValue(TradeProp.TradeDate, DateTime.MinValue);
                AsAtDate = item.AppProps.GetValue(TradeProp.AsAtDate, DateTime.MinValue);
                //TradingBookId = item.AppProps.GetValue<string>(TradeProp.TradingBookId, null);
                TradingBookName = item.AppProps.GetValue<string>(TradeProp.TradingBookName, null);
                Party1 = item.AppProps.GetValue<string>(TradeProp.Party1, null);
                CounterPartyName = item.AppProps.GetValue<string>(TradeProp.CounterPartyName, null);
                Party2 = item.AppProps.GetValue<string>(TradeProp.Party2, null);
                BaseParty = item.AppProps.GetValue<string>(TradeProp.BaseParty, null);
            }
            catch (Exception e)
            {
                ExcpName = e.GetType().Name;
                ExcpText = e.Message;
            }
        }
    }

    // ------------------------------------------------------------------------
    // Curve object

    public class CurveObj : BaseUIObj
    {
        public string NameSpace { get; set; }
        public string Schema { get; set; }
        public string SourceSystem { get; set; }
        public DateTime BaseDate { get; set; }
        public DateTime BuildDateTime { get; set; }
        public string Currency1 { get; set; }
        public string Currency2 { get; set; }
        public string Algorithm { get; set; }
        public string CurveName { get; set; }
        public string Market { get; set; }
        public string PricingStructureType { get; set; }
        public string StressName { get; set; }
        public string UniqueIdentifier { get; set; }

        public CurveObj(ICoreItem item)
            : base(item)
        {
            try
            {
                // trade props
                NameSpace = item.AppProps.GetValue(EnvironmentProp.NameSpace, EnvironmentProp.DefaultNameSpace);
                Schema = item.AppProps.GetValue<string>(EnvironmentProp.Schema, null);
                SourceSystem = item.AppProps.GetValue<string>(EnvironmentProp.SourceSystem, null);
                BaseDate = item.AppProps.GetValue<DateTime>(CurveProp.BaseDate, true);
                BuildDateTime = item.AppProps.GetValue<DateTime>(CurveProp.BuildDateTime, true);
                Currency1 = item.AppProps.GetValue<string>(CurveProp.Currency1);
                Currency2 = item.AppProps.GetValue<string>(CurveProp.Currency2);
                Algorithm = item.AppProps.GetValue<string>(CurveProp.Algorithm);
                CurveName = item.AppProps.GetValue<string>(CurveProp.CurveName, null);
                Market = item.AppProps.GetValue<string>(CurveProp.Market, null);
                PricingStructureType = item.AppProps.GetValue<string>(CurveProp.PricingStructureType, null);
                StressName = item.AppProps.GetValue<string>(CurveProp.StressName, null);
                UniqueIdentifier = item.AppProps.GetValue<string>(CurveProp.UniqueIdentifier, null);
            }
            catch (Exception e)
            {
                ExcpName = e.GetType().Name;
                ExcpText = e.Message;
            }
        }
    }

    // ------------------------------------------------------------------------
    // Valuation (unaggregated) object

    public class ValueRawObj : TradeObj
    {
        public DateTime ValuationDateTime { get; set; }
        public string Requester { get; set; }
        public Guid RequestId { get; set; }
        public string Market { get; set; }
        public DateTime MarketDate { get; set; }
        public string Aggregation { get; set; }
        public string IrScenario { get; set; }
        public string FxScenario { get; set; }
        public string ReportingCurrency { get; set; }
        // metrics
        public decimal[] Metrics { get; set; }
        //public decimal BreakEvenRate { get; set; }
        public decimal NPV { get; set; }
        //TODO populate this property!
        //public decimal NPVDiff { get; set; }

        public ValueRawObj(ICoreItem item)
            : base(item)
        {
            try
            {
                ValuationDateTime = item.AppProps.GetValue(CurveProp.BaseDate, new DateTime());
                Requester = item.AppProps.GetValue<string>(RequestBase.Prop.Requester, null);
                RequestId = item.AppProps.GetValue(RequestBase.Prop.RequestId, Guid.Empty);
                Market = item.AppProps.GetValue<string>(CurveProp.Market, null);
                MarketDate = item.AppProps.GetValue(CurveProp.MarketDate, new DateTime());
                Aggregation = item.AppProps.GetValue<string>(ValueProp.Aggregation, null);
                IrScenario = item.AppProps.GetValue<string>(ValueProp.IrScenario, null);
                FxScenario = item.AppProps.GetValue<string>(ValueProp.FxScenario, null);
                ReportingCurrency = item.AppProps.GetValue<string>(ValueProp.ReportingCurrency, null);
                // from valuation
                var valuationReport = (ValuationReport)item.Data;
                // standard metrics
                var metricIds = Enum.GetValues(typeof(InstrumentMetrics));
                var extraMetrics = ValueProp.ExtraMetrics;
                Metrics = new decimal[metricIds.Length + extraMetrics.Length];
                if (valuationReport.tradeValuationItem != null)
                {
                    foreach (var tradeValuationItem in valuationReport.tradeValuationItem)
                    {
                        if (tradeValuationItem.valuationSet.assetValuation != null)
                        {
                            foreach (var assetValuation in tradeValuationItem.valuationSet.assetValuation)
                            {
                                foreach (var quote in assetValuation.quote)
                                {
                                    var metricFound = false;
                                    for (var i = 0; i < extraMetrics.Length; i++)
                                    {
                                        if (
                                            String.Compare(extraMetrics[i], quote.measureType.Value,
                                                           StringComparison.OrdinalIgnoreCase) != 0) continue;
                                        Metrics[metricIds.Length + i] = quote.value;
                                        metricFound = true;
                                        break;
                                    }
                                    if (!metricFound)
                                    {
                                        // try standard
                                        InstrumentMetrics metricId;
                                        if (EnumHelper.TryParse(quote.measureType.Value, true, out metricId))
                                        {
                                            Metrics[(int)metricId] = quote.value;
                                            metricFound = true;
                                        }
                                    }
                                    if (!metricFound)
                                        throw new ArgumentException("Unknown metric: " + quote.measureType.Value);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExcpName = e.GetType().Name;
                ExcpText = e.Message;
            }
        }
    }

    // ------------------------------------------------------------------------
    // Portfolio object

    public class PortfolioObj : BaseUIObj
    {
        public string PortfolioId { get; set; }
        public string Description { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }

        public PortfolioObj(ICoreItem item) : base(item)
        {
            try
            {
                // portfolio props
                var portfolio = (PortfolioSpecification)item.Data;
                PortfolioId = portfolio.PortfolioId;
                Description = portfolio.Description;
                OwnerId = portfolio.OwnerId.Name;
                OwnerName = portfolio.OwnerId.DisplayName;
            }
            catch (Exception e)
            {
                ExcpName = e.GetType().Name;
                ExcpText = e.Message;
            }
        }
    }

    // ------------------------------------------------------------------------
    // TradeAll view

    public enum TradeAllColEnum
    {
        NameSpace,
        Schema,
        SourceSystem,
        TradeSource,
        ProductTaxonomy,
        TradeId,
        Type,
        Product,
        State,
        Party1,
        Party2,
        BaseParty,
        CPtyName,
        //TBookId,
        BookName,
        Currencies,
        EffectiveDate,
        MaturityDate,
        TradeDate,
        AsAtDate,
        ErrType,
        ErrText
    }

    internal class TradeAllViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(TradeAllColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((TradeAllColEnum)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((TradeAllColEnum)column)
            {
                case TradeAllColEnum.NameSpace: return true;
                case TradeAllColEnum.Schema: return true;
                case TradeAllColEnum.SourceSystem: return true;
                case TradeAllColEnum.TradeSource: return true;
                case TradeAllColEnum.ProductTaxonomy: return true;
                case TradeAllColEnum.Type: return true;
                case TradeAllColEnum.Product: return true;
                case TradeAllColEnum.State: return true;
                case TradeAllColEnum.Party1: return true;
                case TradeAllColEnum.Party2: return true;
                case TradeAllColEnum.BaseParty: return true;
                case TradeAllColEnum.CPtyName: return true;
                case TradeAllColEnum.BookName: return true;
                case TradeAllColEnum.TradeId: return true;
                case TradeAllColEnum.ErrType: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class TradeAllDataHelper : IDataHelper<TradeObj>
    {
        public string GetUniqueKey(TradeObj data)
        {
            return $"{data.TradeSource}.{data.ProductType}.{data.TradeId}";
        }

        public string GetDisplayValue(TradeObj data, int column)
        {
            switch ((TradeAllColEnum)column)
            {
                case TradeAllColEnum.NameSpace: return data.NameSpace;
                case TradeAllColEnum.Schema: return data.Schema;
                case TradeAllColEnum.SourceSystem: return data.SourceSystem;
                case TradeAllColEnum.TradeSource: return data.TradeSource;
                case TradeAllColEnum.ProductTaxonomy: return data.ProductTaxonomy;
                case TradeAllColEnum.TradeId: return data.TradeId;
                case TradeAllColEnum.Type: return data.TradeType;
                case TradeAllColEnum.Product: return data.ProductType ?? "Undefined";
                case TradeAllColEnum.State: return data.TradeState.ToString();
                //case TradeAllColEnum.TBookId: return data.TradingBookId;
                case TradeAllColEnum.BookName: return data.TradingBookName;
                case TradeAllColEnum.Currencies: return data.Currencies;
                case TradeAllColEnum.EffectiveDate: return data.EffectiveDate.ToShortDateString();
                case TradeAllColEnum.MaturityDate: return data.MaturityDate.ToShortDateString();
                case TradeAllColEnum.Party1: return data.Party1;
                case TradeAllColEnum.Party2: return data.Party2;
                case TradeAllColEnum.BaseParty: return data.BaseParty;
                case TradeAllColEnum.CPtyName: return data.CounterPartyName;
                case TradeAllColEnum.TradeDate: return data.TradeDate.ToShortDateString();
                case TradeAllColEnum.AsAtDate: return data.AsAtDate.ToShortDateString();
                case TradeAllColEnum.ErrType: return data.ExcpName;
                case TradeAllColEnum.ErrText: return data.ExcpText;
                default: return null;
            }
        }
    }

    internal class TradeAllSorter : IComparer<TradeObj>
    {
        public int Compare(TradeObj a, TradeObj b)
        {
            // sort order column priority
            const int result = 0;
            // descending create time
            if (b != null && (a != null && a.TradeDate < b.TradeDate))
                return 1;
            if (b != null && (a != null && a.TradeDate > b.TradeDate))
                return -1;
            return result;
        }
    }

    public class TradeAllSelecter : BaseSelecter<TradeObj>
    {
        // this class is currently is a placeholder for future selection rules
        public TradeAllSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<TradeObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // TradeSel view

    public enum TradeSelColEnum
    {
        NameSpace,
        Schema,
        SourceSystem,
        TradeSource,
        ProductTaxonomy,
        TradeId,
        TradeType,
        Product,
        State,
        Party1,
        Party2,
        BaseParty,
        CPtyName,
        //TBookId,
        BookName,
        Currencies,
        EffectiveDate,
        MaturityDate,
        TradeDate,
        AsAtDate,
        ErrType,
        ErrText
    }

    internal class TradeSelViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(TradeSelColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((TradeSelColEnum)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((TradeSelColEnum)column)
            {
                case TradeSelColEnum.NameSpace: return true;
                case TradeSelColEnum.Schema: return true;
                case TradeSelColEnum.SourceSystem: return true;
                case TradeSelColEnum.TradeSource: return true;
                case TradeSelColEnum.ProductTaxonomy: return true;
                case TradeSelColEnum.TradeType: return true;
                case TradeSelColEnum.Product: return true;
                case TradeSelColEnum.State: return true;
                case TradeSelColEnum.BookName: return true;
                case TradeSelColEnum.Party1: return true;
                case TradeSelColEnum.Party2: return true;
                case TradeSelColEnum.BaseParty: return true;
                case TradeSelColEnum.CPtyName: return true;
                case TradeSelColEnum.TradeId: return true;
                //case TradeSelColEnum.Currency: return true;
                case TradeSelColEnum.ErrType: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class TradeSelDataHelper : IDataHelper<TradeObj>
    {
        public string GetUniqueKey(TradeObj data)
        {
            return data.TradeId;
        }

        public string GetDisplayValue(TradeObj data, int column)
        {
            switch ((TradeSelColEnum)column)
            {
                case TradeSelColEnum.NameSpace: return data.NameSpace;
                case TradeSelColEnum.Schema: return data.Schema;
                case TradeSelColEnum.SourceSystem: return data.SourceSystem;
                case TradeSelColEnum.TradeSource: return data.TradeSource;
                case TradeSelColEnum.ProductTaxonomy: return data.ProductTaxonomy;
                case TradeSelColEnum.TradeId: return data.TradeId;
                case TradeSelColEnum.TradeType: return data.TradeType;
                case TradeSelColEnum.Product: return data.ProductType ?? "Undefined";
                case TradeSelColEnum.State: return data.TradeState.ToString();
                //case TradeSelColEnum.TBookId: return data.TradingBookId;
                case TradeSelColEnum.BookName: return data.TradingBookName;
                case TradeSelColEnum.Currencies: return data.Currencies;
                case TradeSelColEnum.EffectiveDate: return data.EffectiveDate.ToShortDateString();
                case TradeSelColEnum.MaturityDate: return data.MaturityDate.ToShortDateString();
                case TradeSelColEnum.Party1: return data.Party1;
                case TradeSelColEnum.Party2: return data.Party2;
                case TradeSelColEnum.BaseParty: return data.BaseParty;
                case TradeSelColEnum.CPtyName: return data.CounterPartyName;
                case TradeSelColEnum.TradeDate: return data.TradeDate.ToShortDateString();
                case TradeSelColEnum.AsAtDate: return data.AsAtDate.ToShortDateString();
                case TradeSelColEnum.ErrType: return data.ExcpName;
                case TradeSelColEnum.ErrText: return data.ExcpText;
                default: return null;
            }
        }
    }

    internal class TradeSelSorter : IComparer<TradeObj>
    {
        public int Compare(TradeObj a, TradeObj b)
        {
            // sort order column priority
            const int result = 0;
            // descending create time
            if (b != null && (a != null && a.TradeDate < b.TradeDate))
                return 1;
            if (b != null && (a != null && a.TradeDate > b.TradeDate))
                return -1;
            return result;
        }
    }

    public class TradeSelSelecter : BaseSelecter<TradeObj>
    {
        // this class is currently is a placeholder for future selection rules
        public TradeSelSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<TradeObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // CurveSel view

    public enum CurveSelColEnum
    {
        NameSpace,
        Schema,
        SourceSystem,
        BaseDate,
        BuildDateTime,
        Currency1,
        Currency2,
        Algorithm,
        CurveName,
        Market,
        PricingStructureType,
        StressName,
        UniqueIdentifier,
        ErrType,
        ErrText
    }

    internal class CurveSelViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(CurveSelColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((CurveSelColEnum)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((CurveSelColEnum)column)
            {
                case CurveSelColEnum.NameSpace: return true;
                case CurveSelColEnum.Schema: return true;
                case CurveSelColEnum.SourceSystem: return true;
                case CurveSelColEnum.BaseDate: return true;
                case CurveSelColEnum.BuildDateTime: return true;
                case CurveSelColEnum.Currency1: return true;
                case CurveSelColEnum.Currency2: return true;
                case CurveSelColEnum.Algorithm: return true;
                case CurveSelColEnum.CurveName: return true;
                case CurveSelColEnum.Market: return true;
                case CurveSelColEnum.PricingStructureType: return true;
                case CurveSelColEnum.StressName: return true;
                case CurveSelColEnum.UniqueIdentifier: return true;
                case CurveSelColEnum.ErrType: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            return HorizontalAlignment.Left;
        }
    }

    internal class CurveSelDataHelper : IDataHelper<CurveObj>
    {
        public string GetUniqueKey(CurveObj data)
        {
            return data.UniqueIdentifier;
        }

        public string GetDisplayValue(CurveObj data, int column)
        {
            switch ((CurveSelColEnum)column)
            {
                case CurveSelColEnum.NameSpace: return data.NameSpace;
                case CurveSelColEnum.Schema: return data.Schema;
                case CurveSelColEnum.SourceSystem: return data.SourceSystem;
                case CurveSelColEnum.BaseDate: return data.BaseDate.ToShortDateString();
                case CurveSelColEnum.BuildDateTime: return data.BuildDateTime.ToShortDateString() + " : " + data.BuildDateTime.ToLongTimeString();
                case CurveSelColEnum.Currency1: return data.Currency1;
                case CurveSelColEnum.Currency2: return data.Currency2;
                case CurveSelColEnum.Algorithm: return data.Algorithm ?? "Undefined";
                case CurveSelColEnum.CurveName: return data.CurveName;
                case CurveSelColEnum.Market: return data.Market;
                case CurveSelColEnum.PricingStructureType: return data.PricingStructureType;
                case CurveSelColEnum.StressName: return data.StressName ?? "Undefined";
                case CurveSelColEnum.UniqueIdentifier: return data.UniqueIdentifier;
                case CurveSelColEnum.ErrType: return data.ExcpName;
                case CurveSelColEnum.ErrText: return data.ExcpText;
                default: return null;
            }
        }
    }

    internal class CurveSelSorter : IComparer<CurveObj>
    {
        public int Compare(CurveObj a, CurveObj b)
        {
            // sort order column priority
            const int result = 0;
            // descending create time
            if (b != null && (a != null && a.BuildDateTime < b.BuildDateTime))
                return 1;
            if (b != null && (a != null && a.BuildDateTime > b.BuildDateTime))
                return -1;
            return result;
        }
    }

    public class CurveSelSelecter : BaseSelecter<CurveObj>
    {
        // this class is currently is a placeholder for future selection rules
        public CurveSelSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<CurveObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // ValueRaw

    public enum ValueRawColEnum
    {
        NameSpace,
        Schema,
        SourceSystem,
        //TradeSource,
        ProductTaxonomy,
        TradeId,
        Type,
        Product,
        State,
        Party1,
        Party2,
        BaseParty,
        CPtyName,
        //TBookId,
        BookName,
        EffectiveDate,
        MaturityDate,
        Requester,
        //RequestId,
        IrStress,
        FxStress,
        RepCcy,
        Market,
        MarketDate,
        ValuationDateTime,
        Currencies,
        TradeDate,
        AsAtDate,
        // metrics
        MarketQuote,
        ImpliedQuote,
        NPV,
        NFV,
        CVA,
        Delta1,
        Delta0,
        DeltaR,
        AccrualFactor,
        HistAccrualFactor,
        ExpectedValue,
        HistValue,
        HistDelta1,
        //BreakEvenRate,
        // other
        Calculated,
        ErrType,
        ErrText,
        UniqueId
    }

    internal class ValueRawViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(ValueRawColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((ValueRawColEnum)column).ToString();
        }

        public bool IsFilterColumn(int column)
        {
            switch ((ValueRawColEnum)column)
            {
                case ValueRawColEnum.NameSpace: return true;
                case ValueRawColEnum.Schema: return true;
                case ValueRawColEnum.SourceSystem: return true;
                //case ValueRawColEnum.TradeSource: return true;
                case ValueRawColEnum.ProductTaxonomy: return true;
                case ValueRawColEnum.TradeId: return true;
                case ValueRawColEnum.Type: return true;
                case ValueRawColEnum.Product: return true;
                case ValueRawColEnum.State: return true;
                case ValueRawColEnum.Party1: return true;
                case ValueRawColEnum.Party2: return true;
                case ValueRawColEnum.BaseParty: return true;
                case ValueRawColEnum.CPtyName: return true;
                case ValueRawColEnum.Requester: return true;
                //case ValueRawColEnum.RequestId: return true;
                case ValueRawColEnum.IrStress: return true;
                case ValueRawColEnum.FxStress: return true;
                case ValueRawColEnum.RepCcy: return true;
                case ValueRawColEnum.Market: return true;
                case ValueRawColEnum.MarketDate: return true;
                //case ValueRawColEnum.ValuationDateTime: return true;
                //case ValueRawColEnum.Currency: return true;
                case ValueRawColEnum.ErrType: return true;
                default:
                    return false;
            }
        }

        public HorizontalAlignment GetColumnAlignment(int column)
        {
            switch ((ValueRawColEnum)column)
            {
                case ValueRawColEnum.MarketQuote: return HorizontalAlignment.Right;
                case ValueRawColEnum.ImpliedQuote: return HorizontalAlignment.Right;
                case ValueRawColEnum.NPV: return HorizontalAlignment.Right;
                case ValueRawColEnum.NFV: return HorizontalAlignment.Right;
                case ValueRawColEnum.CVA: return HorizontalAlignment.Right;
                case ValueRawColEnum.Delta1: return HorizontalAlignment.Right;
                case ValueRawColEnum.Delta0: return HorizontalAlignment.Right;
                case ValueRawColEnum.DeltaR: return HorizontalAlignment.Right;
                case ValueRawColEnum.AccrualFactor: return HorizontalAlignment.Right;
                case ValueRawColEnum.HistAccrualFactor: return HorizontalAlignment.Right;
                case ValueRawColEnum.ExpectedValue: return HorizontalAlignment.Right;
                case ValueRawColEnum.HistValue: return HorizontalAlignment.Right;
                case ValueRawColEnum.HistDelta1: return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }
    }

    internal class ValueRawDataHelper : IDataHelper<ValueRawObj>
    {
        public string GetUniqueKey(ValueRawObj data)
        {
            return data.UniqueId;
        }

        public string GetDisplayValue(ValueRawObj data, int column)
        {
            const string rateFormat = "N2";
            const string valueFormat = "N0";
            string result = null;
            if (data != null)
            {
                switch ((ValueRawColEnum)column)
                {
                    case ValueRawColEnum.NameSpace: result = data.NameSpace; break;
                    case ValueRawColEnum.Schema: result = data.Schema; break;
                    case ValueRawColEnum.SourceSystem: result = data.SourceSystem; break;
                    case ValueRawColEnum.UniqueId: result = data.UniqueId; break;
                    //case ValueRawColEnum.TradeSource: result = data.TradeSource; break;
                    case ValueRawColEnum.ProductTaxonomy: result = data.ProductTaxonomy; break;
                    case ValueRawColEnum.TradeId: result = data.TradeId; break;
                    case ValueRawColEnum.Type: result = data.TradeType; break;
                    case ValueRawColEnum.Product: result = data.ProductType ?? "Undefined"; break;
                    case ValueRawColEnum.State: result = data.TradeState.ToString(); break;
                    case ValueRawColEnum.Party1: result = data.Party1; break;
                    case ValueRawColEnum.Party2: result = data.Party2; break;
                    case ValueRawColEnum.BaseParty: result = data.BaseParty; break;
                    case ValueRawColEnum.CPtyName: result = data.CounterPartyName; break;
                    //case ValueRawColEnum.TBookId: result = data.TradingBookId; break;
                    case ValueRawColEnum.BookName: result = data.TradingBookName; break;
                    case ValueRawColEnum.EffectiveDate: result = data.EffectiveDate.ToShortDateString(); break;
                    case ValueRawColEnum.MaturityDate: result = data.MaturityDate.ToShortDateString(); break;
                    case ValueRawColEnum.Requester: result = data.Requester; break;
                    case ValueRawColEnum.ValuationDateTime: result = data.ValuationDateTime.ToShortDateString() + ":" + data.ValuationDateTime.ToShortTimeString(); break;
                    //case ValueRawColEnum.RequestId: result = data.RequestId.ToString(); break;
                    case ValueRawColEnum.IrStress: result = data.IrScenario; break;
                    case ValueRawColEnum.FxStress: result = data.FxScenario; break;
                    case ValueRawColEnum.Market: result = data.Market; break;
                    case ValueRawColEnum.Currencies: result = data.Currencies; break;
                    case ValueRawColEnum.RepCcy: result = data.ReportingCurrency; break;
                    case ValueRawColEnum.TradeDate: result = data.TradeDate.ToShortDateString(); break;
                    case ValueRawColEnum.AsAtDate: result = data.AsAtDate.ToShortDateString(); break;
                    case ValueRawColEnum.Calculated: result = data.Created.LocalDateTime.ToString("g"); break;
                    // metrics - 2 decimal places
                    case ValueRawColEnum.MarketQuote: result = data.Metrics[(int)InstrumentMetrics.MarketQuote].ToString(rateFormat); break;
                    case ValueRawColEnum.ImpliedQuote: result = data.Metrics[(int)InstrumentMetrics.ImpliedQuote].ToString(rateFormat); break;
                    // metrics - 0 decimal places
                    case ValueRawColEnum.NPV: result = data.Metrics[(int)InstrumentMetrics.NPV].ToString(valueFormat); break;
                    case ValueRawColEnum.NFV: result = data.Metrics[(int)InstrumentMetrics.NFV].ToString(valueFormat); break;
                    case ValueRawColEnum.CVA: result = data.Metrics[(int)InstrumentMetrics.SimpleCVA].ToString(valueFormat); break;
                    case ValueRawColEnum.Delta1: result = data.Metrics[(int)InstrumentMetrics.Delta1].ToString(valueFormat); break;
                    case ValueRawColEnum.Delta0: result = data.Metrics[(int)InstrumentMetrics.Delta0].ToString(valueFormat); break;
                    case ValueRawColEnum.DeltaR: result = data.Metrics[(int)InstrumentMetrics.DeltaR].ToString(valueFormat); break;
                    case ValueRawColEnum.AccrualFactor: result = data.Metrics[(int)InstrumentMetrics.AccrualFactor].ToString(valueFormat); break;
                    case ValueRawColEnum.HistAccrualFactor: result = data.Metrics[(int)InstrumentMetrics.HistoricalAccrualFactor].ToString(valueFormat); break;
                    case ValueRawColEnum.ExpectedValue: result = data.Metrics[(int)InstrumentMetrics.ExpectedValue].ToString(valueFormat); break;
                    case ValueRawColEnum.HistValue: result = data.Metrics[(int)InstrumentMetrics.HistoricalValue].ToString(valueFormat); break;
                    case ValueRawColEnum.HistDelta1: result = data.Metrics[(int)InstrumentMetrics.HistoricalDelta1].ToString(valueFormat); break;
                    //case ValueRawColEnum.BreakEvenRate: result = data.BreakEvenRate.ToString(); break;
                    // other
                    case ValueRawColEnum.ErrType: result = data.ExcpName; break;
                    case ValueRawColEnum.ErrText: result = data.ExcpText; break;
                }
            }
            return result ?? "(null)";
        }
    }

    internal class ValueRawSorter : IComparer<ValueRawObj>
    {
        public int Compare(ValueRawObj a, ValueRawObj b)
        {
            // sort order column priority
            const int result = 0;
            // descending create time
            if (b != null && a != null && a.Created < b.Created)
                return 1;
            if (b != null && a != null && a.Created > b.Created)
                return -1;
            return result;
        }
    }

    public class ValueRawSelecter : BaseSelecter<ValueRawObj>
    {
        // this class is currently is a placeholder for future selection rules
        public ValueRawSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<ValueRawObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // ValueSum

    public enum ValueSumColEnum
    {
        Market,
        Party1,
        BaseParty,
        Product,
        State,
        Requester,
        //RequestId,
        RepCcy,
        IRStress,
        FxStress,
        // metrics
        MarketQuote,
        ImpliedQuote,
        NPV,
        NFV,
        CVA,
        Delta1,
        Delta0,
        DeltaR,
        AccrualFactor,
        HistAccrualFactor,
        ExpectedValue,
        HistValue,
        HistDelta1,
        //BreakEvenRate,
        // other
        Trades,
        Errors,
        AggType,
        Created,
        Expires,
        ErrType,
        ErrText,
        UniqueId
    }

    internal class ValueSumViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(ValueSumColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((ValueSumColEnum)column).ToString();
        }

        public bool IsFilterColumn(int column)
        {
            switch ((ValueSumColEnum)column)
            {
                case ValueSumColEnum.Product: return true;
                case ValueSumColEnum.State: return true;
                case ValueSumColEnum.Party1: return true;
                //case ValueSumColEnum.OPtyId: return true;
                //case ValueSumColEnum.TBookId: return true;
                case ValueSumColEnum.Requester: return true;
                //case ValueSumColEnum.RequestId: return true;
                case ValueSumColEnum.IRStress: return true;
                case ValueSumColEnum.FxStress: return true;
                case ValueSumColEnum.RepCcy: return true;
                case ValueSumColEnum.AggType: return true;
                case ValueSumColEnum.Market: return true;
                case ValueSumColEnum.ErrType: return true;
                default:
                    return false;
            }
        }

        public HorizontalAlignment GetColumnAlignment(int column)
        {
            switch ((ValueSumColEnum)column)
            {
                case ValueSumColEnum.MarketQuote: return HorizontalAlignment.Right;
                case ValueSumColEnum.ImpliedQuote: return HorizontalAlignment.Right;
                case ValueSumColEnum.NPV: return HorizontalAlignment.Right;
                case ValueSumColEnum.NFV: return HorizontalAlignment.Right;
                case ValueSumColEnum.CVA: return HorizontalAlignment.Right;
                case ValueSumColEnum.Delta1: return HorizontalAlignment.Right;
                case ValueSumColEnum.Delta0: return HorizontalAlignment.Right;
                case ValueSumColEnum.DeltaR: return HorizontalAlignment.Right;
                case ValueSumColEnum.AccrualFactor: return HorizontalAlignment.Right;
                case ValueSumColEnum.HistAccrualFactor: return HorizontalAlignment.Right;
                case ValueSumColEnum.ExpectedValue: return HorizontalAlignment.Right;
                case ValueSumColEnum.HistValue: return HorizontalAlignment.Right;
                case ValueSumColEnum.HistDelta1: return HorizontalAlignment.Right;
                case ValueSumColEnum.Trades: return HorizontalAlignment.Right;
                case ValueSumColEnum.Errors: return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }
    }

    // ------------------------------------------------------------------------
    // Portfolio

    public enum PortfolioColEnum
    {
        PortfolioId,
        Description,
        OwnerId,
        OwnerName,
        Created,
        ErrType,
        ErrText
    }

    internal class PortfolioViewHelper : IViewHelper
    {
        public int ColumnCount { get; } = Enum.GetValues(typeof(PortfolioColEnum)).Length;

        public string GetColumnTitle(int column)
        {
            return ((PortfolioColEnum)column).ToString();
        }

        public bool IsFilterColumn(int column)
        {
            switch ((PortfolioColEnum)column)
            {
                case PortfolioColEnum.OwnerId: return true;
                case PortfolioColEnum.OwnerName: return true;
                case PortfolioColEnum.ErrType: return true;
                default:
                    return false;
            }
        }

        public HorizontalAlignment GetColumnAlignment(int column)
        {
            switch ((PortfolioColEnum)column)
            {
                //case PortfolioColEnum.Portfolio: return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }
    }

    internal class PortfolioDataHelper : IDataHelper<PortfolioObj>
    {
        //private readonly int _breakEvenRateMetricIndex = Enum.GetValues(typeof(InstrumentMetrics)).Length + ValueProp.BreakEvenRateOffset;
        //private readonly int _tradeCountMetricIndex = Enum.GetValues(typeof(InstrumentMetrics)).Length + ValueProp.TradeCountOffset;
        //private readonly int _errorCountMetricIndex = Enum.GetValues(typeof(InstrumentMetrics)).Length + ValueProp.ErrorCountOffset;

        public string GetUniqueKey(PortfolioObj data)
        {
            return data.PortfolioId;
        }

        public string GetDisplayValue(PortfolioObj data, int column)
        {
            string result = null;
            if (data != null)
            {
                switch ((PortfolioColEnum)column)
                {
                    case PortfolioColEnum.PortfolioId: result = data.PortfolioId; break;
                    case PortfolioColEnum.Description: result = data.Description; break;
                    case PortfolioColEnum.OwnerId: result = data.OwnerId; break;
                    case PortfolioColEnum.OwnerName: result = data.OwnerName; break;
                    case PortfolioColEnum.Created: result = data.Created.LocalDateTime.ToString("g"); break;
                    // other
                    case PortfolioColEnum.ErrType: result = data.ExcpName; break;
                    case PortfolioColEnum.ErrText: result = data.ExcpText; break;
                }
            }
            return result ?? "(null)";
        }
    }

    internal class PortfolioSorter : IComparer<PortfolioObj>
    {
        public int Compare(PortfolioObj a, PortfolioObj b)
        {
            // descending update time
            return DateTimeOffset.Compare(b.Created, a.Created);
        }
    }

    public class PortfolioSelecter : BaseSelecter<PortfolioObj>
    {
        // this class is currently is a placeholder for future selection rules
        public PortfolioSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<PortfolioObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }
}
