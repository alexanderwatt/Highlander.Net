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
using System.Threading;
using Core.Common;
using Orion.Contracts;
using Orion.PortfolioValuation;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Servers;
using Orion.Util.Threading;
using Orion.Workflow;

#endregion

namespace Server.TradeRevaluer
{
    internal class InternalPortfRevalRequest
    {
        public readonly Guid RequestId;
        // managed state
        public DateTimeOffset Created;
        public PortfolioValuationRequest Request;
        public CancellationRequest Cancellation;
        public WFCalculatePortfolioStressValuation Workflow;
        public HandlerResponse LatestResult;
        public InternalPortfRevalRequest(Guid requestId, HandlerResponse pvRes)
        {
            RequestId = requestId;
            Created = DateTimeOffset.Now;
            LatestResult = pvRes;
        }
        public InternalPortfRevalRequest(Guid requestId, DateTimeOffset created, PortfolioValuationRequest pvReq)
        {
            RequestId = requestId;
            Created = created;
            Request = pvReq;
            LatestResult = new HandlerResponse
                               {
                RequestId = pvReq.RequestId,
                //RequesterData = pvReq.PortfolioId,
                Status = RequestStatusEnum.Received
            };
        }
        public InternalPortfRevalRequest(Guid requestId, DateTimeOffset created)
        {
            RequestId = requestId;
            Created = created;
            LatestResult = new HandlerResponse
                               {
                RequestId = requestId.ToString()
            };
        }
    }

    public class PortfolioValuationServer : ServerBase2
    {
        // shared state
        private ISubscription _requestSubs;
        private ISubscription _cancelSubs;
        private ISubscription _resultSubs;
        private AsyncThreadQueue _manageThreadQueue;
        private IWorkContext _workContext;
        private WFCalculatePortfolioStressValuation _activeWorkflow;
        private Timer _timerRequestManager;
        private readonly TimeSpan _requestManagerPeriod = TimeSpan.FromSeconds(5);
        //private Timer _TimerRequestScheduler;
        readonly Guarded<Dictionary<Guid, InternalPortfRevalRequest>> _outstandingRequests =
            new Guarded<Dictionary<Guid, InternalPortfRevalRequest>>(new Dictionary<Guid, InternalPortfRevalRequest>());

        // manager-only state
        // worker-only state
        private AsyncEventThrottle<object> _workerEventQueue;
        //private int _WorkerRequests = 0;
        //private AsyncThreadQueue _WorkerThreadQueue;
        private DateTime _lastManagedAt = DateTime.MinValue;

        protected override void OnServerStarted()
        {
            Logger.LogDebug("Instance/FarmSize: {0}/{1}", ServerInstance, ServerFarmSize);
            // create workflow
            _workContext = new WorkContext(Logger, IntClient.Target, HostInstance, ServerInstance);
            // create event throttles
            _workerEventQueue = new AsyncEventThrottle<object>(DequeueWorkerRequests);
            _manageThreadQueue = new AsyncThreadQueue(Logger);
        }

        protected override void OnCloseCallback()
        {
            WFCalculatePortfolioStressValuation workflow = _activeWorkflow;
            workflow?.Cancel("Server shutdown");
        }

        protected override void OnServerStopping()
        {
            //DisposeHelper.SafeDispose(ref _TimerRequestScheduler);
            DisposeHelper.SafeDispose(ref _timerRequestManager);
            DisposeHelper.SafeDispose(ref _requestSubs);
            DisposeHelper.SafeDispose(ref _cancelSubs);
            DisposeHelper.SafeDispose(ref _resultSubs);
            DisposeHelper.SafeDispose(ref _manageThreadQueue);
            DisposeHelper.SafeDispose(ref _workerEventQueue);
        }

        protected override void OnFirstCallback()
        {
            // subscribe to portfolio valuation requests and results
            // note: load results first
            _resultSubs = IntClient.Target.SubscribeNoWait<HandlerResponse>(
                Expr.ALL,
                delegate(ISubscription subs, ICoreItem item)
                {
                    var pvRes = (HandlerResponse)item.Data;
                    _manageThreadQueue.Dispatch(pvRes, ReceivePortfolioValuationResult);
                },
                null);
            _requestSubs = IntClient.Target.SubscribeNoWait<PortfolioValuationRequest>(
                Expr.ALL,
                (subs, pvReqItem) => _manageThreadQueue.Dispatch(pvReqItem, ReceivePortfolioValuationRequest),
                null);
            _cancelSubs = IntClient.Target.SubscribeNoWait<CancellationRequest>(
                Expr.ALL,
                (subs, pvReqItem) => _manageThreadQueue.Dispatch(pvReqItem, ReceiveCancellationRequest),
                null);

            // start timers
            // - request queue manager
            // - request scheduler
            _timerRequestManager = new Timer(
                notUsed => MainThreadQueue.Dispatch<object>(null, RequestManagerTimeout),
                null, _requestManagerPeriod, _requestManagerPeriod);
            // hack - start the daily 4am portfolio reval schedule
            // todo - attach this as a workflow step to the daily file/trade import process
            DateTime dtNow = DateTime.Now;
            DateTime dtDue = DateTime.Today.Add(TimeSpan.FromHours(4));
            if(dtDue <= dtNow)
                dtDue += TimeSpan.FromDays(1);
            //TimeSpan tsDue = dtDue - dtNow;
            //_TimerRequestScheduler = new Timer(
            //    (notUsed) => _MainThreadQueue.Dispatch<object>(null, RequestSchedulerTimeout),
            //    null, tsDue, TimeSpan.FromDays(1));
        }

        //private void RequestSchedulerTimeoutObs(object notUsed)
        //{
        //    // time to post a portfolio valuation request
        //    TimeSpan retention = TimeSpan.FromDays(1);
        //    var counterPartyList = new List<Pair<string, string>>
        //                               {
        //                                   new Pair<string, string>("13142", "Barclays"),
        //                                   new Pair<string, string>("14859", "Woolworths")
        //                               };
        //    foreach (var item in counterPartyList)
        //    {
        //        // publish portfolio specifications
        //        var portfolioSubqueries = new List<PortfolioSubquery>
        //                                      {new PortfolioSubquery {CounterpartyId = item.First}};
        //        string portfolioId = Guid.NewGuid().ToString();
        //        var portfolio = new PortfolioSpecification
        //        {
        //            PortfolioId = portfolioId,
        //            OwnerId = new UserIdentity
        //                          {
        //                Name = IntClient.Target.ClientInfo.Name,
        //                DisplayName = "Trade Reval Server"
        //            },
        //            Description = String.Format("{1} ({0}) Scheduled", item.First, item.Second),
        //            PortfolioSubqueries = portfolioSubqueries.ToArray()
        //        };
        //        IntClient.Target.SaveObject(portfolio, retention);

        //        // publish the portfolio valuation requests
        //        foreach (string marketName in new[] { CurveConst.NAB_EOD, CurveConst.QR_EOD })
        //        {
        //            Guid requestId = Guid.NewGuid();
        //            var request = new PortfolioValuationRequest
        //                              {
        //                BaseDate = DateTime.Today,
        //                RequestId = requestId.ToString(),
        //                Retention = retention.ToString(),
        //                SubmitTime = DateTimeOffset.Now.ToString("o"),
        //                RequestDescription = String.Format("{0} [{1}]", portfolio.Description, marketName),
        //                RequesterId = new UserIdentity
        //                                  {
        //                    Name = IntClient.Target.ClientInfo.Name,
        //                    DisplayName = "Trade Reval Server"
        //                },
        //                PortfolioId = portfolioId,
        //                MarketDate = null,
        //                MarketName = marketName,
        //                ReportingCurrency = "AUD",
        //                IRScenarioNames = ScenarioConst.AllIrScenarioIds,
        //                FXScenarioNames = ScenarioConst.AllFxScenarioIds
        //            };
        //            IntClient.Target.SaveObject(request, false, retention);
        //            // send to request manager
        //            RequestBase.DispatchToManager(IntClient.Target, request);
        //        }
        //    }
        //}

        private void ReceivePortfolioValuationResult(HandlerResponse pvResult)
        {
            var requestId = new Guid(pvResult.RequestId);

            // ignore requests if we are not running
            if (GetState() != BasicServerState.Running)
                return;

            _outstandingRequests.Locked(requests =>
            {
                InternalPortfRevalRequest request;
                if (requests.TryGetValue(requestId, out request))
                {
                    // exists
                    request.LatestResult = pvResult;
                }
                else
                {
                    // not found
                    request = new InternalPortfRevalRequest(requestId, pvResult);
                    requests[requestId] = request;
                }
            });

            // dispatch worker request
            _workerEventQueue.Dispatch(null);
        }

        private void ReceivePortfolioValuationRequest(ICoreItem item)
        {
            DateTimeOffset created = item.Created;
            var pvRequest = (PortfolioValuationRequest)item.Data;
            var requestId = new Guid(pvRequest.RequestId);

            // ignore requests if we are not running
            // ignore requests not processed by this server instance
            if ((GetState() != BasicServerState.Running) ||
                ((requestId.GetHashCode() % ServerFarmSize) != ServerInstance))
                return;

            InternalPortfRevalRequest newRequest = null;
            _outstandingRequests.Locked(requests =>
            {
                InternalPortfRevalRequest oldRequest;
                if (!requests.TryGetValue(requestId, out oldRequest))
                {
                    // not found
                    newRequest = new InternalPortfRevalRequest(requestId, created);
                }
                if (newRequest != null)
                {
                    newRequest.Request = pvRequest;
                    newRequest.LatestResult.Status = RequestStatusEnum.Received;
                    requests[requestId] = newRequest;
                }
            });
            var response = new HandlerResponse
                               {
                RequestId = newRequest.RequestId.ToString(),
                RequesterId = newRequest.Request.RequesterId ?? new UserIdentity { Name = IntClient.Target.ClientInfo.Name, DisplayName = "Unknown" },
                //RequesterData = newRequest.Request.PortfolioId,
                Status = newRequest.LatestResult.Status
            };
            _workContext.Cache.SaveObject(response);

            // dispatch worker request
            _workerEventQueue.Dispatch(null);
        }

        private void ReceiveCancellationRequest(ICoreItem item)
        {
            //DateTimeOffset created = item.Created;
            var cancelRequest = (CancellationRequest)item.Data;
            var requestId = new Guid(cancelRequest.RequestId);

            // ignore requests if we are not running
            if (GetState() != BasicServerState.Running)
                return;

            // get current request and cancel it
            InternalPortfRevalRequest internalRequest = null;
            _outstandingRequests.Locked(requests =>
            {
                if (!requests.TryGetValue(requestId, out internalRequest))
                    return; // does not exist or is not ours - just exit
                internalRequest.Cancellation = cancelRequest;
                internalRequest.LatestResult.Status = RequestStatusEnum.Cancelled;
            });

            WFCalculatePortfolioStressValuation workflow = internalRequest.Workflow;
            workflow?.Cancel(cancelRequest.CancelReason);

            var response = new HandlerResponse
                               {
                RequestId = internalRequest.RequestId.ToString(),
                RequesterId = internalRequest.Request.RequesterId ?? new UserIdentity { Name = IntClient.Target.ClientInfo.Name, DisplayName = "Unknown" },
                //RequesterData = internalRequest.Request.PortfolioId,
                Status = internalRequest.LatestResult.Status
            };
            _workContext.Cache.SaveObject(response);

            // dispatch worker request
            _workerEventQueue.Dispatch(null);
        }

        private void RequestManagerTimeout(object notUsed)
        {
            // dispatch worker request
            _workerEventQueue.Dispatch(null);
        }

        private void DequeueWorkerRequests(object notUsed)
        {
            // throttle callbacks to once per timer period
            DateTime dtNow = DateTime.Now;
            if ((dtNow - _lastManagedAt) < _requestManagerPeriod)
                return;
            _lastManagedAt = dtNow;

            try
            {
                // processing algorithm
                // - find oldest unprocessed (status==received) request and process it
                bool workToDo = true;
                while (workToDo && (GetState() == BasicServerState.Running))
                {
                    workToDo = false;
                    InternalPortfRevalRequest activeRequest = null;
                    DateTimeOffset oldestRequestCreated = DateTimeOffset.MaxValue;
                    _outstandingRequests.Locked(requests =>
                    {
                        foreach (InternalPortfRevalRequest request in requests.Values)
                        {
                            if ((request.LatestResult.Status == RequestStatusEnum.Received) &&
                                (request.Created < oldestRequestCreated) &&
                                (request.Request != null))
                            {
                                activeRequest = request;
                                oldestRequestCreated = request.Created;
                            }
                        }
                        if (activeRequest != null)
                        {
                            // we have found a request to process
                            workToDo = true;
                        }
                    });

                    if (activeRequest != null)
                    {
                        // process request
                        PortfolioValuationRequest request = activeRequest.Request;
                        try
                        {
                            // run the valuation worflow
                            using (var workflow = new WFCalculatePortfolioStressValuation())
                            {
                                workflow.Initialise(_workContext);
                                activeRequest.Workflow = workflow;
                                _activeWorkflow = workflow;
                                try
                                {
                                    WorkflowOutput<HandlerResponse> output = workflow.Execute(request);
                                    WorkflowHelper.LogErrors(Logger, output.Errors);
                                }
                                finally
                                {
                                    activeRequest.Workflow = null;
                                    _activeWorkflow = null;
                                }
                            }
                        }
                        catch (Exception innerExcp)
                        {
                            Logger.Log(innerExcp);
                            // publish 'faulted' result
                            var response = new HandlerResponse
                                               {
                                RequestId = request.RequestId,
                                RequesterId = request.RequesterId,
                                //RequesterData = request.PortfolioId,
                                Status = RequestStatusEnum.Faulted,
                                FaultDetail = new ExceptionDetail(innerExcp)
                            };
                            _workContext.Cache.SaveObject(response);
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
