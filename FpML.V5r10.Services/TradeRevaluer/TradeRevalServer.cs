using System;
using System.Collections.Generic;
using System.Threading;
using National.QRSC.Contracts;
using National.QRSC.Runtime.Common;
using National.QRSC.Runtime.V33;
using National.QRSC.Utility.Logging;
using National.QRSC.Utility.Helpers;
using National.QRSC.Utility.Servers;
using National.QRSC.Utility.Expressions;
using National.QRSC.Utility.Threading;
using National.QRSC.Constants;
using National.QRSC.Workflow;
using National.QRSC.Workflow.TradeValuation;

namespace National.QRSC.Server.TradeRevaluer
{
    internal class InternalRequest
    {
        public readonly Guid RequestId;
        // managed state
        public DateTimeOffset Created;
        public PortfolioValuationRequest Request;
        public ServerRequestStatus LatestResult;
        public InternalRequest(Guid requestId, ServerRequestStatus pvRes)
        {
            RequestId = requestId;
            Created = DateTimeOffset.Now;
            LatestResult = pvRes;
        }
        public InternalRequest(Guid requestId, DateTimeOffset created, PortfolioValuationRequest pvReq)
        {
            RequestId = requestId;
            Created = created;
            Request = pvReq;
            LatestResult = new ServerRequestStatus()
            {
                RequestId = pvReq.RequestId,
                RequesterData = pvReq.PortfolioId,
                Status = RequestStatusEnum.Received
            };
        }
    }

    public class TradeRevalServer : ServerBase
    {
        // shared state
        private ISubscription _ResultSubs;
        private ISubscription _RequestSubs;
        private AsyncThreadQueue _ManageThreadQueue;
        private AsyncThreadQueue _WorkerThreadQueue;
        private IWorkContext _WorkContext;
        private WFCalculatePortfolioValuation _Workflow;
        private Timer _TimerRequestManager;
        private readonly TimeSpan RequestManagerPeriod = TimeSpan.FromSeconds(5);
        private Timer _TimerRequestScheduler;
        Guarded<Dictionary<Guid, InternalRequest>> _OutstandingRequests =
            new Guarded<Dictionary<Guid, InternalRequest>>(new Dictionary<Guid, InternalRequest>());

        // manager-only state
        // worker-only state
        private int _WorkerRequests = 0;
        private DateTime _LastManagedAt = DateTime.MinValue;

        public TradeRevalServer(ILogger logger, ICoreClient client)
            : base(logger, client)
        { }

        protected override void OnServerStarted()
        {
            // create workflow
            _WorkContext = new WorkContext(Logger, this.Client);
            _Workflow = new WFCalculatePortfolioValuation();
            // create thread queues
            _ManageThreadQueue = new AsyncThreadQueue(Logger);
            _WorkerThreadQueue = new AsyncThreadQueue(Logger);
        }

        protected override void OnServerStopping()
        {
            DisposeHelper.SafeDispose(ref _TimerRequestScheduler);
            DisposeHelper.SafeDispose(ref _TimerRequestManager);
            DisposeHelper.SafeDispose(ref _RequestSubs);
            DisposeHelper.SafeDispose(ref _ResultSubs);
            DisposeHelper.SafeDispose(ref _ManageThreadQueue);
            DisposeHelper.SafeDispose(ref _WorkerThreadQueue);
            DisposeHelper.SafeDispose(ref _Workflow);
        }

        protected override void OnFirstCallback()
        {
            base.OnFirstCallback();

            // initialise workflow
            _Workflow.Initialise(_WorkContext);

            // subscribe to portfolio valuation requests and results
            // note: load results first
            _ResultSubs = this.Client.Subscribe<ServerRequestStatus>(
                Expr.ALL,
                delegate(ISubscription subs, ICoreItem item)
                {
                    ServerRequestStatus pvRes = (ServerRequestStatus)item.Data;
                    _ManageThreadQueue.Dispatch(pvRes, ReceivePortfolioValuationResult);
                },
                null);
            _RequestSubs = this.Client.Subscribe<PortfolioValuationRequest>(
                Expr.ALL,
                delegate(ISubscription subs, ICoreItem pvReqItem)
                {
                    _ManageThreadQueue.Dispatch(pvReqItem, ReceivePortfolioValuationRequest);
                },
                null);

            // start timers
            // - request queue manager
            // - request scheduler
            _TimerRequestManager = new Timer(
                (notUsed) => _MainThreadQueue.Dispatch<object>(null, RequestManagerTimeout),
                null, RequestManagerPeriod, RequestManagerPeriod);
            // hack - start the daily 4am portfolio reval schedule
            // todo - attach this as a workflow step to the daily file/trade import process
            TimeSpan dueTime = (DateTime.Today.Add(TimeSpan.FromHours(4)) - DateTime.Now);
            if (dueTime < TimeSpan.Zero)
                dueTime += TimeSpan.FromDays(1);
            _TimerRequestScheduler = new Timer(
                (notUsed) => _MainThreadQueue.Dispatch<object>(null, RequestSchedulerTimeout),
                null, dueTime, TimeSpan.FromDays(1));
        }

        private void RequestSchedulerTimeout(object notUsed)
        {
            // time to post a portfolio valuation request
            TimeSpan retention = TimeSpan.FromDays(2);
            // publish portfolio specification
            List<PortfolioSubquery> portfolioSubqueries = new List<PortfolioSubquery>();
            portfolioSubqueries.Add(new PortfolioSubquery() { CounterpartyId = "13142" }); // Barclays
            portfolioSubqueries.Add(new PortfolioSubquery() { CounterpartyId = "14859" }); // Woolworths
            string portfolioId = Guid.NewGuid().ToString();
            var portfolio = new PortfolioSpecification
            {
                PortfolioId = portfolioId,
                OwnerId = new UserIdentity()
                {
                    Name = this.Client.ClientInfo.Name,
                    DisplayName = "Portfolio Valuation Server"
                },
                Description = "Woolworths (14859) Scheduled",
                PortfolioSubqueries = portfolioSubqueries.ToArray()
            };
            this.Client.SaveObject<PortfolioSpecification>(portfolio, retention);

            // publish the portfolio valuation requests
            foreach (string marketName in new string[] { CurveConst.NAB_EOD })
            {
                Guid requestId = Guid.NewGuid();
                PortfolioValuationRequest request = new PortfolioValuationRequest()
                {
                    BaseDate = DateTime.Today,
                    RequestId = requestId.ToString(),
                    Retention = retention.ToString(),
                    SubmitTime = DateTimeOffset.Now.ToString("o"),
                    RequesterId = new UserIdentity()
                    {
                        Name = this.Client.ClientInfo.Name,
                        DisplayName = "Portfolio Valuation Server"
                    },
                    PortfolioId = portfolioId,
                    MarketDate = null,
                    MarketName = marketName,
                    ReportingCurrency = "AUD",
                    IsDetailedReport = false,
                    IRScenarioNames = ScenarioConst.AllIrScenarioIds,
                    FXScenarioNames = ScenarioConst.AllFxScenarioIds
                };
                this.Client.SaveObject<PortfolioValuationRequest>(request, false, retention);
            }
        }

        private void ReceivePortfolioValuationResult(ServerRequestStatus pvRes)
        {
            Guid requestId = new Guid(pvRes.RequestId);
            _OutstandingRequests.Locked((requests) =>
            {
                InternalRequest request;
                if (requests.TryGetValue(requestId, out request))
                {
                    // exists
                    request.LatestResult = pvRes;
                }
                else
                {
                    // not found
                    request = new InternalRequest(requestId, pvRes);
                    requests[requestId] = request;
                }
            });

            // dispatch worker request
            EnqueueWorkerRequest();
        }

        private void ReceivePortfolioValuationRequest(ICoreItem pvReqItem)
        {
            DateTimeOffset created = pvReqItem.Created;
            PortfolioValuationRequest pvReq = (PortfolioValuationRequest)pvReqItem.Data;
            Guid requestId = new Guid(pvReq.RequestId);
            InternalRequest newRequest = new InternalRequest(requestId, created, pvReq);
            InternalRequest oldRequest = null;
            _OutstandingRequests.Locked((requests) =>
            {
                if (requests.TryGetValue(requestId, out oldRequest))
                {
                    // exists
                    newRequest.LatestResult = oldRequest.LatestResult;
                }
                requests[requestId] = newRequest;
            });
            if (oldRequest == null)
            {
                (new ServerRequestStatus()
                {
                    RequestId = pvReq.RequestId,
                    RequesterId = pvReq.RequesterId ?? new UserIdentity() { Name = this.Client.ClientInfo.Name, DisplayName = "Unknown" },
                    RequesterData = pvReq.PortfolioId,
                    Status = RequestStatusEnum.Received

                }).Publish(_WorkContext.Logger, _WorkContext.Cache, TimeSpan.FromDays(1));
            }

            // dispatch worker request
            EnqueueWorkerRequest();
        }

        private void RequestManagerTimeout(object notUsed)
        {
            // dispatch worker request
            EnqueueWorkerRequest();
        }

        private void EnqueueWorkerRequest()
        {
            Interlocked.Increment(ref _WorkerRequests);
            _WorkerThreadQueue.Dispatch<object>(null, DequeueWorkerRequests);
        }
        private void DequeueWorkerRequests(object notUsed)
        {
            // exit if more requests following
            if (Interlocked.Decrement(ref _WorkerRequests) > 0)
                return;

            // throttle callbacks to once per timer period
            DateTime dtNow = DateTime.Now;
            if ((dtNow - _LastManagedAt) < RequestManagerPeriod)
                return;
            _LastManagedAt = dtNow;

            try
            {
                // processing algorithm
                // - find oldest unprocessed (status==received) request and process it
                bool workToDo = true;
                while (workToDo)
                {
                    workToDo = false;
                    InternalRequest oldestUnstartedRequest = null;
                    DateTimeOffset oldestRequestCreated = DateTimeOffset.MaxValue;
                    _OutstandingRequests.Locked((requests) =>
                    {
                        foreach (InternalRequest request in requests.Values)
                        {
                            if ((request.LatestResult.Status == RequestStatusEnum.Received) &&
                                (request.Created < oldestRequestCreated))
                            {
                                oldestUnstartedRequest = request;
                                oldestRequestCreated = request.Created;
                            }
                        }
                        if (oldestUnstartedRequest != null)
                        {
                            // we have found a request to process
                            workToDo = true;
                        }
                    });

                    if (oldestUnstartedRequest != null)
                    {
                        // process request
                        PortfolioValuationRequest request = oldestUnstartedRequest.Request;
                        try
                        {
                            // run the valuation worflow
                            WorkflowOutput<ServerRequestStatus> output = _Workflow.Execute(request);
                            bool failed = WorkflowHelper.LogErrors(Logger, output.Errors);
                        }
                        catch (Exception innerExcp)
                        {
                            Logger.Log(innerExcp);
                            // publish 'faulted' result
                            (new ServerRequestStatus()
                            {
                                RequestId = request.RequestId,
                                RequesterId = request.RequesterId,
                                RequesterData = request.PortfolioId,
                                Status = RequestStatusEnum.Faulted,
                                FaultDetail = new ExceptionDetail(innerExcp),
                            }).Publish(_WorkContext.Logger, _WorkContext.Cache, TimeSpan.FromDays(1));
                        }
                    }
                } // while there is work to do
            }
            catch (Exception outerExcp)
            {
                // deserialisation error?
                Logger.Log(outerExcp);
            }

        }

    }
}
