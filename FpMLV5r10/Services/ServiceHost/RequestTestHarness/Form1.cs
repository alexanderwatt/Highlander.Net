using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using nab.QDS.Build;
using nab.QDS.Util.Helpers;
using nab.QDS.Util.Logging;
using nab.QDS.Util.Expressions;
using nab.QDS.Util.RefCounting;
using nab.QDS.Util.NamedValues;
using nab.QDS.Util.Caching;
using nab.QDS.Util.Threading;
using nab.QDS.Core.Common;
using nab.QDS.Core.V34;
using National.QRSC.Contracts;
using National.QRSC.UI.WinTools;
using National.QRSC.Grid.Worker;
using National.QRSC.Grid.Handler;
using National.QRSC.Grid.Manager;
using National.QRSC.Constants;
using National.QRSC.FpML.V47;

namespace RequestTestHarness
{
    public partial class Form1 : Form
    {
        private ILogger _MainLog;
        private Reference<ICoreClient> _ClientRef;

        private IServerBase2 _WorkerA;
        private ILogger _WorkerALog;
        private IServerBase2 _WorkerB;
        private ILogger _WorkerBLog;
        private IServerBase2 _Manager;
        private ILogger _ManagerLog;

        // requests grid
        private IListViewHelper<ProgressObj> _ProgressView;
        private IViewHelper _ProgressViewHelper;
        private IDataHelper<ProgressObj> _ProgressDataHelper;
        private ISelecter<ProgressObj> _ProgressSelecter;
        private IFilterGroup _ProgressFilters;
        private GuardedDictionary<Guid, ProgressObj> _ProgressCache = new GuardedDictionary<Guid, ProgressObj>();

        // availability grid
        private IListViewHelper<WorkerAvailability> _AvailabilityView;
        private IViewHelper _AvailabilityViewHelper;
        private IDataHelper<WorkerAvailability> _AvailabilityDataHelper;
        private ISelecter<WorkerAvailability> _AvailabilitySelecter;
        private IFilterGroup _AvailabilityFilters;
        private ICoreCache _AvailabilityCache;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _MainLog = new TextBoxLogger(txtLog);
            _WorkerALog = new TextBoxLogger(txtWorkerALog);
            _WorkerBLog = new TextBoxLogger(txtWorkerBLog);
            _ManagerLog = new TextBoxLogger(txtManagerLog);
            _ClientRef = Reference<ICoreClient>.Create(new CoreClientFactory(_MainLog).Create());

            // init controls
            txtWorkerComputer.Text = Environment.MachineName;
            txtWorkerInstance.Text = "A";
            // - form title
            WinFormHelper.SetAppFormTitle(this, BuildConst.BuildEnv);

            cbMarketName.Items.Add(CurveConst.QR_EOD);
            cbMarketName.Items.Add(CurveConst.NAB_EOD);
            cbMarketName.SelectedIndex = 0;

            cbCounterParty.Items.Add("14859,Woolworths");
            cbCounterParty.Items.Add("13142,Barclays");
            cbCounterParty.SelectedIndex = 0;


            // setup the request progress view
            _ProgressViewHelper = new ProgressViewHelper();
            _ProgressDataHelper = new ProgressDataHelper();
            _ProgressFilters = new ComboxBoxFilterGroup(
                panelProgress, _ProgressViewHelper, new EventHandler(ProgressSelectionChanged));
            _ProgressSelecter = new ProgressSelecter(
                _ProgressFilters, _ProgressViewHelper, _ProgressDataHelper);
            _ProgressView = new ListViewManager<ProgressObj>(
                _MainLog, lvProgress, _ProgressViewHelper,
                _ProgressSelecter, _ProgressFilters, new ProgressSorter(), _ProgressDataHelper);

            _ClientRef.Target.SubscribeNoWait<PortfolioValuationRequest>(Expr.ALL, ProgressCallback, null);
            _ClientRef.Target.SubscribeNoWait<TradeValuationRequest>(Expr.ALL, ProgressCallback, null);
            _ClientRef.Target.SubscribeNoWait<OrdinaryCurveGenRequest>(Expr.ALL, ProgressCallback, null);
            _ClientRef.Target.SubscribeNoWait<StressedCurveGenRequest>(Expr.ALL, ProgressCallback, null);
            _ClientRef.Target.SubscribeNoWait<UnassignedWorkflowRequest>(Expr.ALL, ProgressCallback, null);
            _ClientRef.Target.SubscribeNoWait<AssignedWorkflowRequest>(Expr.ALL, ProgressCallback, null);
            _ClientRef.Target.SubscribeNoWait<HandlerResponse>(Expr.ALL, ProgressCallback, null);
            _ClientRef.Target.SubscribeNoWait<WorkerResponse>(Expr.ALL, ProgressCallback, null);
            _ClientRef.Target.SubscribeNoWait<ManagerResponse>(Expr.ALL, ProgressCallback, null);

            // setup the worker availability view
            _AvailabilityViewHelper = new AvailabilityViewHelper();
            _AvailabilityDataHelper = new AvailabilityDataHelper();
            _AvailabilityFilters = new ComboxBoxFilterGroup(
                panelAvailability, _AvailabilityViewHelper, new EventHandler(AvailabilitySelectionChanged));
            _AvailabilitySelecter = new AvailabilitySelecter(
                _AvailabilityFilters, _AvailabilityViewHelper, _AvailabilityDataHelper);
            _AvailabilityView = new ListViewManager<WorkerAvailability>(
                _MainLog, lvAvailability, _AvailabilityViewHelper,
                _AvailabilitySelecter, _AvailabilityFilters, new AvailabilitySorter(), _AvailabilityDataHelper);

            ICoreCache _AvailabilityCache = _ClientRef.Target.CreateCache(
                delegate(CacheChangeData update)
                {
                    _AvailabilityView.UpdateData(new ViewChangeNotification<WorkerAvailability>()
                    {
                        Change = update.Change,
                        OldData = (update.OldItem != null) ? (WorkerAvailability)update.OldItem.Data : null,
                        NewData = (update.NewItem != null) ? (WorkerAvailability)update.NewItem.Data : null
                    });
                }, WindowsFormsSynchronizationContext.Current);
            _AvailabilityCache.SubscribeNoWait<WorkerAvailability>(Expr.ALL, null, null);
        }

        private void ProgressCallback(ISubscription subscription, ICoreItem item)
        {
            try
            {
                ProgressObj oldProgress = null;
                ProgressObj newProgress = null;
                if (item != null)
                {
                    // get old request view object
                    Guid requestId = item.AppProps.GetValue<Guid>(RequestBase.Prop.RequestId, true);
                    oldProgress = _ProgressCache.Remove(requestId);
                    // build the new request view object
                    if (item.IsCurrent())
                    {
                        if (item.DataType.IsSubclassOf(typeof(RequestBase)))
                        {
                            RequestBase request = item.Data as RequestBase;
                            if (request != null)
                            {
                                newProgress = new ProgressObj(requestId, item.Created, oldProgress, request);
                            }
                        }
                        else if (item.DataType.IsSubclassOf(typeof(ResponseBase)))
                        {
                            ResponseBase response = item.Data as ResponseBase;
                            if (response != null)
                            {
                                newProgress = new ProgressObj(requestId, item.Created, oldProgress, response);
                            }
                        }
                        else
                            throw new NotSupportedException(String.Format("Type: '{0}'", item.DataType.Name));
                    }
                    if (newProgress != null)
                        _ProgressCache.Set(requestId, newProgress);
                }
                else
                    throw new ArgumentNullException("item");

                // determine the change type
                CacheChange change = CacheChange.Undefined;
                if (oldProgress != null)
                {
                    // updated or deleted
                    if (newProgress != null)
                        change = CacheChange.ItemUpdated;
                    else
                        change = CacheChange.ItemRemoved;
                }
                else
                {
                    // created or ???
                    if (newProgress != null)
                        change = CacheChange.ItemCreated;
                }
                if (change != CacheChange.Undefined)
                    _ProgressView.UpdateData(new ViewChangeNotification<ProgressObj>() { Change = change, OldData = oldProgress, NewData = newProgress });
            }
            catch (Exception excp)
            {
                _MainLog.Log(excp);
            }
        }

        void ProgressSelectionChanged(object sender, EventArgs e)
        {
            _ProgressView.RebuildView();
        }

        void AvailabilitySelectionChanged(object sender, EventArgs e)
        {
            _AvailabilityView.RebuildView();
        }

        private RequestBase _Request;
        private void ExecuteRequest()
        {
            if (_Request == null)
            {
                _MainLog.LogWarning("Create a request before executing!");
                return;
            }

            if (rbExecInprocess.Checked)
            {
                ThreadPool.QueueUserWorkItem(BackgroundWorker, _Request);
                _Request = null;
            }
            else if (rbExecAssignToWorker.Checked)
            {
                RequestBase.DispatchToWorker(_ClientRef.Target, _Request, txtWorkerComputer.Text, txtWorkerInstance.Text);
                _Request = null;
            }
            else if (rbExecSendToManager.Checked)
            {
                RequestBase.DispatchToManager(_ClientRef.Target, _Request);
                _Request = null;
            }
            else
                _MainLog.LogWarning("No request handling option chosen!");
        }

        private void BackgroundWorker(object state)
        {
            try
            {
                RequestBase request = (RequestBase)state;
                RequestBase.DispatchToWorker(_ClientRef.Target, request, Environment.MachineName, "Internal");
                int result = RequestHandler.HandleRequest(_MainLog, _ClientRef.Target, new Guid(request.RequestId), "Internal");
                _MainLog.LogDebug("HandleRequest returned: {0}", result);
            }
            catch (Exception e)
            {
                _MainLog.Log(e);
            }
        }

        private void btnGenPingRequest_Click(object sender, EventArgs e)
        {
            Guid requestId = Guid.NewGuid();
            PingHandlerRequest request = new PingHandlerRequest()
            {
                RequestId = requestId.ToString(),
                RequestDescription = String.Format("Test Ping"),
                RequesterId = new UserIdentity() { Name = _ClientRef.Target.ClientInfo.Name, DisplayName = "Grid Test Harness" },
                SubmitTime = DateTimeOffset.Now.ToString("o"),
                DelayPeriod = TimeSpan.FromSeconds(Convert.ToDouble(nudPingDelay.Value)).ToString(),
                FaultMessage = chkPingFault.Checked ? "Test Fault" : null
            };
            _ClientRef.Target.SaveObject<PingHandlerRequest>(request);

            // save request for execution
            txtRequestId.Text = requestId.ToString();
            _Request = request;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeHelper.SafeDispose(ref _WorkerA);
            DisposeHelper.SafeDispose(ref _WorkerB);
            DisposeHelper.SafeDispose(ref _Manager);
            DisposeHelper.SafeDispose(ref _AvailabilityCache);
            DisposeHelper.SafeDispose(ref _ClientRef);
            DisposeHelper.SafeDispose(ref _MainLog);
            DisposeHelper.SafeDispose(ref _WorkerALog);
            DisposeHelper.SafeDispose(ref _WorkerBLog);
            DisposeHelper.SafeDispose(ref _ManagerLog);
        }

        private void btnStartWorker_Click(object sender, EventArgs e)
        {
            DisposeHelper.SafeDispose(ref _WorkerA);
            _WorkerA = new GridWorkerServer();
            _WorkerA.Logger = _WorkerALog;
            _WorkerA.Client = _ClientRef;
            _WorkerA.HostInstance = "A";
            _WorkerA.OtherSettings = new NamedValueSet(new NamedValue(GridWorkerServer.Setting.HandlerLimit, 2));
            _WorkerA.Start();
        }

        private void btnStopWorker_Click(object sender, EventArgs e)
        {
            DisposeHelper.SafeDispose(ref _WorkerA);
        }

        private void btnStartWorkerB_Click(object sender, EventArgs e)
        {
            DisposeHelper.SafeDispose(ref _WorkerB);
            _WorkerB = new GridWorkerServer();
            _WorkerB.Logger = _WorkerBLog;
            _WorkerB.Client = _ClientRef;
            _WorkerB.HostInstance = "B";
            _WorkerB.OtherSettings = new NamedValueSet(new NamedValue(GridWorkerServer.Setting.HandlerLimit, 2));
            _WorkerB.Start();
        }

        private void btnStopWorkerB_Click(object sender, EventArgs e)
        {
            DisposeHelper.SafeDispose(ref _WorkerB);
        }

        private void btnStartManager_Click(object sender, EventArgs e)
        {
            DisposeHelper.SafeDispose(ref _Manager);
            _Manager = new GridManagerServer();
            _Manager.Logger = _ManagerLog;
            _Manager.Client = _ClientRef;
            _Manager.Start();
        }

        private void btnStopManager_Click(object sender, EventArgs e)
        {
            DisposeHelper.SafeDispose(ref _Manager);
        }

        private void btnPVRWoolworths_Click(object sender, EventArgs e)
        {
            string[] counterPartyParts = cbCounterParty.Text.Split(',');
            RequestBase request = CreatePVRequest(cbMarketName.Text, counterPartyParts[0], counterPartyParts[1], chkAllIRStresses.Checked, chkAllFXStresses.Checked);

            // save request for execution
            txtRequestId.Text = request.RequestId;
            _Request = request;
        }

        private RequestBase CreatePVRequest(string marketName, string counterPartyId, string counterPartyName, bool allIRStresses, bool allFXStresses)
        {
            // time to post a portfolio valuation request
            TimeSpan retention = TimeSpan.FromDays(1);
            // publish portfolio specifications
            List<PortfolioSubquery> portfolioSubqueries = new List<PortfolioSubquery>();
            portfolioSubqueries.Add(new PortfolioSubquery() { CounterpartyId = counterPartyId });
            string portfolioId = Guid.NewGuid().ToString();
            var portfolio = new PortfolioSpecification
            {
                PortfolioId = portfolioId,
                OwnerId = new UserIdentity() { Name = _ClientRef.Target.ClientInfo.Name, DisplayName = "Grid Test Harness" },
                Description = String.Format("{1} ({0}) Test Request", counterPartyId, counterPartyName),
                PortfolioSubqueries = portfolioSubqueries.ToArray()
            };
            _ClientRef.Target.SaveObject<PortfolioSpecification>(portfolio, retention);

            // publish the portfolio valuation requests
            Guid requestId = Guid.NewGuid();
            PortfolioValuationRequest request = new PortfolioValuationRequest()
            {
                BaseDate = DateTime.Today,
                RequestId = requestId.ToString(),
                Retention = retention.ToString(),
                SubmitTime = DateTimeOffset.Now.ToString("o"),
                RequestDescription = String.Format("{0} [{1}]", portfolio.Description, marketName),
                RequesterId = new UserIdentity() { Name = _ClientRef.Target.ClientInfo.Name, DisplayName = "Grid Test Harness" },
                PortfolioId = portfolioId,
                MarketDate = null,
                MarketName = marketName,
                ReportingCurrency = "AUD",
                IRScenarioNames = allIRStresses ? ScenarioConst.AllIrScenarioIds : null,
                FXScenarioNames = allFXStresses ? ScenarioConst.AllFxScenarioIds : null
            };
            _ClientRef.Target.SaveObject<PortfolioValuationRequest>(request);
            return request;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            ExecuteRequest();
        }

        private RequestBase CreateCurveGenRequest()
        {
            // get curve identifiers
            IExpression curveDefFilter = Expr.BoolAND(
                Expr.IsEQU(CurveProp.Function, "Configuration"),
                Expr.IsEQU(CurveProp.Market, CurveConst.QR_LIVE),
                Expr.IsNull(CurveProp.MarketDate));
            DateTime dtNow = DateTime.Now;
            // - hack get reference curves 1st
            List<string> curveItemNames = new List<string>();
            List<CurveSelection> referenceCurveSelectors = new List<CurveSelection>();
            List<CurveSelection> remainingCurveSelectors = new List<CurveSelection>();
            List<ICoreItem> curveDefItems = _ClientRef.Target.LoadItems<Market>(curveDefFilter);
            foreach (ICoreItem curveDefItem in curveDefItems)
            {
                string marketName = curveDefItem.AppProps.GetValue<string>(CurveProp.Market, true);
                string curveType = curveDefItem.AppProps.GetValue<string>(CurveProp.PricingStructureType, true);
                string curveName = curveDefItem.AppProps.GetValue<string>(CurveProp.CurveName, true);
                string refCurveName = curveDefItem.AppProps.GetValue<string>(CurveProp.ReferenceCurveName, null);
                ((refCurveName == null) ? referenceCurveSelectors : remainingCurveSelectors).Add(new CurveSelection()
                {
                    MarketName = marketName,
                    CurveType = curveType,
                    CurveName = curveName
                });
            }
            List<CurveSelection> curveSelectors = new List<CurveSelection>(referenceCurveSelectors);
            curveSelectors.AddRange(remainingCurveSelectors);
            OrdinaryCurveGenRequest request = new OrdinaryCurveGenRequest()
            {
                RequestId = Guid.NewGuid().ToString(),
                RequesterId = new UserIdentity()
                {
                    Name = _ClientRef.Target.ClientInfo.Name,
                    DisplayName = "Grid Test Harness"
                },
                SubmitTime = DateTimeOffset.Now.ToString("o"),
                BaseDate = dtNow.Date,
                SaveMarketData = false,
                UseSavedMarketData = false,
                ForceGenerateEODCurves = false,
                CurveSelector = curveSelectors.ToArray()
            };
            _ClientRef.Target.SaveObject<OrdinaryCurveGenRequest>(request);
            return request;
        }

        private void btnCurveGenRequest_Click(object sender, EventArgs e)
        {
            RequestBase request = CreateCurveGenRequest();

            // save request for execution
            txtRequestId.Text = request.RequestId;
            _Request = request;
        }

    }

    // ------------------------------------------------------------------------
    // Progress object

    public class ProgressObj
    {
        public DateTimeOffset? Created;
        public DateTimeOffset? Updated;
        public Guid RequestId { get; set; }
        public string RequesterId { get; set; }
        public string RequesterName { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? Submitted { get; set; }
        public DateTimeOffset? Commenced { get; set; }
        // manager
        public RequestStatusEnum ManagerStatus { get; set; }
        // worker
        public RequestStatusEnum WorkerStatus { get; set; }
        public string WorkerHost { get; set; }
        // handler
        public RequestStatusEnum HandlerStatus { get; set; }
        public int? ItemCount { get; set; }
        public int? ItemsPassed { get; set; }
        public int? ItemsFailed { get; set; }
        // debug
        public string ExcpName { get; set; }
        public string ExcpText { get; set; }

        public string DisplayProgress()
        {
            int itemCount = ItemCount ?? 0;
            if ((HandlerStatus >= RequestStatusEnum.Commencing) && (itemCount > 0))
            {
                double result = 0.0;
                int passed = ItemsPassed ?? 0;
                int failed = ItemsFailed ?? 0;
                result = ((passed + failed) * 100.0) / itemCount;
                return result.ToString("N0") + "%";
            }
            else
                return "";
        }

        private ProgressObj(Guid requestId, DateTimeOffset created, ProgressObj lastProgress, UserIdentity userIdentity)
        {
            if (requestId.Equals(new Guid("7281fcfe-983f-42ab-8068-51d21b292b1e")))
                RequestId = requestId;
            else
                RequestId = requestId;
            if (!Created.HasValue || (created < Created.Value))
                Created = created;
            if (!Updated.HasValue || (created > Updated.Value))
                Updated = created;
            if (lastProgress != null)
            {
                RequestId = lastProgress.RequestId;
                RequesterId = lastProgress.RequesterId;
                RequesterName = lastProgress.RequesterName;
                Description = lastProgress.Description;
                Submitted = lastProgress.Submitted;
                Commenced = lastProgress.Commenced;
                ManagerStatus = lastProgress.ManagerStatus;
                WorkerStatus = lastProgress.WorkerStatus;
                WorkerHost = lastProgress.WorkerHost;
                HandlerStatus = lastProgress.HandlerStatus;
                ItemCount = lastProgress.ItemCount;
                ItemsPassed = lastProgress.ItemsPassed;
                ItemsFailed = lastProgress.ItemsFailed;
            }
            if (userIdentity != null)
            {
                RequesterId = userIdentity.Name;
                RequesterName = userIdentity.DisplayName;
            }
        }
        public ProgressObj(Guid requestId, DateTimeOffset created, ProgressObj lastProgress, RequestBase request)
            : this(requestId, created, lastProgress, request.RequesterId)
        {
            if (request.RequestDescription != null)
                Description = request.RequestDescription;
            if (request.SubmitTime != null)
                Submitted = DateTimeOffset.Parse(request.SubmitTime);
        }
        public ProgressObj(Guid requestId, DateTimeOffset created, ProgressObj lastProgress, ResponseBase response)
            : this(requestId, created, lastProgress, response.RequesterId)
        {
            if (response.FaultDetail != null)
            {
                ExcpName = response.FaultDetail.ShortName;
                ExcpText = response.FaultDetail.Message;
            }
            // response subtypes
            HandlerResponse handlerResponse = response as HandlerResponse;
            if (handlerResponse != null)
            {
                if (response.Status == RequestStatusEnum.Undefined)
                    throw new ArgumentNullException("status");
                HandlerStatus = response.Status;
                if (handlerResponse.CommenceTime != null)
                    Commenced = DateTimeOffset.Parse(handlerResponse.CommenceTime);
                ItemCount = handlerResponse.ItemCount;
                ItemsPassed = handlerResponse.ItemsPassed;
                ItemsFailed = handlerResponse.ItemsFailed;
            }
            WorkerResponse workerResponse = response as WorkerResponse;
            if (workerResponse != null)
            {
                if (response.Status == RequestStatusEnum.Undefined)
                    throw new ArgumentNullException("status");
                WorkerStatus = response.Status;
                WorkerHost = String.Format("{0}.{1}", workerResponse.WorkerHostComputer, workerResponse.WorkerHostInstance ?? "Default");
            }
            ManagerResponse managerResponse = response as ManagerResponse;
            if (managerResponse != null)
            {
                if (response.Status == RequestStatusEnum.Undefined)
                    throw new ArgumentNullException("status");
                ManagerStatus = response.Status;
            }
        }
    }

    // ------------------------------------------------------------------------
    // Progress

    public enum ProgressColEnum
    {
        RequestId,
        RequesterId,
        RequesterName,
        RequestType,
        Description,
        Submitted,
        ManagerStatus,
        WorkerStatus,
        WorkerHost,
        HandlerStatus,
        Commenced,
        Duration,
        Progress,
        Total,
        Passed,
        Failed,
        ErrType,
        ErrText
    }

    internal class ProgressViewHelper : IViewHelper
    {
        private readonly int _ColumnCount = Enum.GetValues(typeof(ProgressColEnum)).Length;
        public int ColumnCount { get { return _ColumnCount; } }
        public string GetColumnTitle(int column)
        {
            return ((ProgressColEnum)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((ProgressColEnum)column)
            {
                case ProgressColEnum.RequesterId: return true;
                case ProgressColEnum.ManagerStatus: return true;
                case ProgressColEnum.WorkerStatus: return true;
                case ProgressColEnum.WorkerHost: return true;
                case ProgressColEnum.HandlerStatus: return true;
                case ProgressColEnum.ErrType: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            switch ((ProgressColEnum)column)
            {
                case ProgressColEnum.Progress: return HorizontalAlignment.Right;
                case ProgressColEnum.Total: return HorizontalAlignment.Right;
                case ProgressColEnum.Passed: return HorizontalAlignment.Right;
                case ProgressColEnum.Failed: return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }
    }

    internal class ProgressDataHelper : IDataHelper<ProgressObj>
    {
        public string GetUniqueKey(ProgressObj data)
        {
            return data.RequestId.ToString();
        }
        private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        {
            char[] delims = new char[1] { delim };
            if (input == null)
                return defaultValue;
            string[] parts = input.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (minIndex >= parts.Length)
                return defaultValue;
            int index = minIndex;
            StringBuilder result = new StringBuilder();
            while ((index < parts.Length) && (index <= maxIndex))
            {
                if (index > minIndex)
                    result.Append(delim);
                result.Append(parts[index]);
                index++;
            }
            return result.ToString();
        }
        private string DisplayTimespan(TimeSpan? optionalInterval)
        {
            if (!optionalInterval.HasValue)
                return "(null)";
            TimeSpan interval = optionalInterval.Value;
            double absIntervalDays = System.Math.Abs(interval.TotalDays);
            if (absIntervalDays >= 10.0)
            {
                // return days only
                return String.Format("{0}d", interval.TotalDays.ToString("N"));
            }
            if (absIntervalDays >= 1.0)
            {
                // return days and hours
                return String.Format("{0}d {1}h", interval.Days, interval.Hours);
            }
            double absIntervalMins = System.Math.Abs(interval.TotalMinutes);
            if (absIntervalMins >= 60)
            {
                // return hours and minutes
                return String.Format("{0}h {1}m", interval.Hours, interval.Minutes);
            }
            if (absIntervalMins >= 1)
            {
                // return minutes and seconds
                return String.Format("{0}m {1}s", interval.Minutes, interval.Seconds);
            }
            // return seconds only
            return String.Format("{0}s", interval.Seconds);
        }

        public string GetDisplayValue(ProgressObj data, int column)
        {
            string result = null;
            if (data != null)
            {
                TimeSpan? duration = null;
                if (data.Submitted.HasValue)
                    duration = data.Updated - data.Submitted.Value;
                if (data.Commenced.HasValue)
                    duration = data.Updated - data.Commenced.Value;

                switch ((ProgressColEnum)column)
                {
                    case ProgressColEnum.RequestId: result = data.RequestId.ToString(); break;
                    case ProgressColEnum.RequesterId: result = data.RequesterId; break;
                    case ProgressColEnum.RequesterName: result = data.RequesterName; break;
                    case ProgressColEnum.Description: result = data.Description; break;
                    case ProgressColEnum.Submitted: result = data.Submitted.HasValue ? data.Submitted.Value.LocalDateTime.ToString("g") : null; break;
                    case ProgressColEnum.Commenced: result = data.Commenced.HasValue ? data.Commenced.Value.LocalDateTime.ToString("g") : null; break;
                    case ProgressColEnum.Duration: result = DisplayTimespan(duration); break;
                    case ProgressColEnum.ManagerStatus: result = data.ManagerStatus.ToString(); break;
                    case ProgressColEnum.WorkerStatus: result = data.WorkerStatus.ToString(); break;
                    case ProgressColEnum.WorkerHost: result = data.WorkerHost; break;
                    case ProgressColEnum.HandlerStatus: result = data.HandlerStatus.ToString(); break;
                    case ProgressColEnum.Progress: result = data.DisplayProgress(); break;
                    case ProgressColEnum.Total: result = data.ItemCount.ToString(); break;
                    case ProgressColEnum.Passed: result = data.ItemsPassed.ToString(); break;
                    case ProgressColEnum.Failed: result = data.ItemsFailed.ToString(); break;
                    // other
                    case ProgressColEnum.ErrType: result = data.ExcpName; break;
                    case ProgressColEnum.ErrText: result = data.ExcpText; break;
                }
            }
            return result ?? "(null)";
        }
    }

    internal class ProgressSorter : IComparer<ProgressObj>
    {
        public int Compare(ProgressObj a, ProgressObj b)
        {
            // descending update time
            return DateTimeOffset.Compare(b.Updated.Value, a.Updated.Value);
        }
    }

    public class ProgressSelecter : BaseSelecter<ProgressObj>
    {
        // this class is currently is a placeholder for future selection rules
        public ProgressSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<ProgressObj> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

    // ------------------------------------------------------------------------
    // Availability

    public enum AvailabilityColEnum
    {
        HostName,
        Instance,
        Available
    }

    internal class AvailabilityViewHelper : IViewHelper
    {
        private readonly int _ColumnCount = Enum.GetValues(typeof(AvailabilityColEnum)).Length;
        public int ColumnCount { get { return _ColumnCount; } }
        public string GetColumnTitle(int column)
        {
            return ((AvailabilityColEnum)column).ToString();
        }
        public bool IsFilterColumn(int column)
        {
            switch ((AvailabilityColEnum)column)
            {
                case AvailabilityColEnum.HostName: return true;
                default:
                    return false;
            }
        }
        public HorizontalAlignment GetColumnAlignment(int column)
        {
            switch ((AvailabilityColEnum)column)
            {
                case AvailabilityColEnum.Available: return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }
    }

    internal class AvailabilityDataHelper : IDataHelper<WorkerAvailability>
    {
        public string GetUniqueKey(WorkerAvailability data)
        {
            return data.PrivateKey;
        }
        private string GetParts(string input, char delim, int minIndex, int maxIndex, string defaultValue)
        {
            char[] delims = new char[1] { delim };
            if (input == null)
                return defaultValue;
            string[] parts = input.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (minIndex >= parts.Length)
                return defaultValue;
            int index = minIndex;
            StringBuilder result = new StringBuilder();
            while ((index < parts.Length) && (index <= maxIndex))
            {
                if (index > minIndex)
                    result.Append(delim);
                result.Append(parts[index]);
                index++;
            }
            return result.ToString();
        }
        private string DisplayTimespan(TimeSpan? optionalInterval)
        {
            if (!optionalInterval.HasValue)
                return "(null)";
            TimeSpan interval = optionalInterval.Value;
            double absIntervalDays = System.Math.Abs(interval.TotalDays);
            if (absIntervalDays >= 10.0)
            {
                // return days only
                return String.Format("{0}d", interval.TotalDays.ToString("N"));
            }
            if (absIntervalDays >= 1.0)
            {
                // return days and hours
                return String.Format("{0}d {1}h", interval.Days, interval.Hours);
            }
            double absIntervalMins = System.Math.Abs(interval.TotalMinutes);
            if (absIntervalMins >= 60)
            {
                // return hours and minutes
                return String.Format("{0}h {1}m", interval.Hours, interval.Minutes);
            }
            if (absIntervalMins >= 1)
            {
                // return minutes and seconds
                return String.Format("{0}m {1}s", interval.Minutes, interval.Seconds);
            }
            // return seconds only
            return String.Format("{0}s", interval.Seconds);
        }

        public string GetDisplayValue(WorkerAvailability data, int column)
        {
            string result = null;
            if (data != null)
            {
                switch ((AvailabilityColEnum)column)
                {
                    case AvailabilityColEnum.HostName: result = data.WorkerHostComputer; break;
                    case AvailabilityColEnum.Instance: result = data.WorkerHostInstance ?? "Default"; break;
                    case AvailabilityColEnum.Available: result = data.AvailableNodeCount.ToString(); break;
                }
            }
            return result ?? "(null)";
        }
    }

    internal class AvailabilitySorter : IComparer<WorkerAvailability>
    {
        public int Compare(WorkerAvailability a, WorkerAvailability b)
        {
            // descending availability
            return (b.AvailableNodeCount - a.AvailableNodeCount);
            //return String.Compare(a.WorkerHostComputer, b.WorkerHostComputer, true);
        }
    }

    public class AvailabilitySelecter : BaseSelecter<WorkerAvailability>
    {
        // this class is currently is a placeholder for future selection rules
        public AvailabilitySelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<WorkerAvailability> dataHelper)
            : base(filterValues, viewHelper, dataHelper) { }
    }

}
