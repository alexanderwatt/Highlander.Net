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
using Highlander.Core.Common;
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Servers;
using Highlander.Utilities.Threading;
using Highlander.Workflow;
using Highlander.Workflow.PortfolioValuation.V5r3;

#endregion

namespace Highlander.TradeRevaluer.Server.V5r3
{
    internal class InternalTradeRevaluationRequest
    {
        public readonly Guid RequestId;
        // managed state
        public DateTimeOffset Created;
        public TradeValuationRequest Request;
        public WFCalculateTradeValuation Workflow;
        public HandlerResponse LatestResult;
        public InternalTradeRevaluationRequest(Guid requestId, HandlerResponse pvRes)
        {
            RequestId = requestId;
            Created = DateTimeOffset.Now;
            LatestResult = pvRes;
        }
        public InternalTradeRevaluationRequest(Guid requestId, DateTimeOffset created, TradeValuationRequest pvReq)
        {
            RequestId = requestId;
            Created = created;
            Request = pvReq;
            LatestResult = new HandlerResponse
                               {
                RequestId = pvReq.RequestId,
                //RequesterData = pvReq.TradeId,
                Status = RequestStatusEnum.Received
            };
        }
    }

    public class TradeValuationServer : ServerBase2
    {
        // shared state
        private ISubscription _resultSubs;
        private ISubscription _requestSubs;
        private AsyncThreadQueue _manageThreadQueue;
        private IWorkContext _workContext;
        private WFCalculateTradeValuation _activeWorkflow;
        private Timer _timerRequestManager;
        private readonly TimeSpan _requestManagerPeriod = TimeSpan.FromSeconds(5);

        readonly Guarded<Dictionary<Guid, InternalTradeRevaluationRequest>> _outstandingRequests =
            new Guarded<Dictionary<Guid, InternalTradeRevaluationRequest>>(new Dictionary<Guid, InternalTradeRevaluationRequest>());

        // manager-only state
        // worker-only state
        private AsyncEventThrottle<object> _workerEventQueue;

        private DateTime _lastManagedAt = DateTime.MinValue;

        protected override void OnServerStarted()
        {
            // create workflow
            _workContext = new WorkContext(Logger, IntClient.Target, HostInstance, ServerInstance);
            // create thread queues
            _manageThreadQueue = new AsyncThreadQueue(Logger);
            _workerEventQueue = new AsyncEventThrottle<object>(DequeueWorkerRequests);
        }

        protected override void OnCloseCallback()
        {
            WFCalculateTradeValuation workflow = _activeWorkflow;
            workflow?.Cancel("Server shutdown");
        }

        protected override void OnServerStopping()
        {
            DisposeHelper.SafeDispose(ref _timerRequestManager);
            DisposeHelper.SafeDispose(ref _requestSubs);
            DisposeHelper.SafeDispose(ref _resultSubs);
            DisposeHelper.SafeDispose(ref _manageThreadQueue);
            DisposeHelper.SafeDispose(ref _workerEventQueue);
        }

        protected override void OnFirstCallback()
        {
            // subscribe to Trade valuation requests and results
            // note: load results first
            _resultSubs = IntClient.Target.SubscribeNoWait<HandlerResponse>(
                Expr.ALL,
                delegate(ISubscription subs, ICoreItem item)
                {
                    var pvRes = (HandlerResponse)item.Data;
                    _manageThreadQueue.Dispatch(pvRes, ReceiveTradeValuationResult);
                },
                null);
            _requestSubs = IntClient.Target.SubscribeNoWait<TradeValuationRequest>(
                Expr.ALL,
                (subs, pvReqItem) => _manageThreadQueue.Dispatch(pvReqItem, ReceiveTradeValuationRequest),
                null);

            // start timers
            // - request queue manager
            // - request scheduler
            _timerRequestManager = new Timer(
                notUsed => MainThreadQueue.Dispatch<object>(null, RequestManagerTimeout),
                null, _requestManagerPeriod, _requestManagerPeriod);
        }

        private void ReceiveTradeValuationResult(HandlerResponse pvRes)
        {
            var requestId = new Guid(pvRes.RequestId);

            // ignore requests if we are not running
            if (GetState() != BasicServerState.Running)
                return;

            _outstandingRequests.Locked(requests =>
            {
                if (requests.TryGetValue(requestId, out var request))
                {
                    // exists
                    request.LatestResult = pvRes;
                }
                else
                {
                    // not found
                    request = new InternalTradeRevaluationRequest(requestId, pvRes);
                    requests[requestId] = request;
                }
            });

            // dispatch worker request
            _workerEventQueue.Dispatch(null);
        }

        private void ReceiveTradeValuationRequest(ICoreItem pvReqItem)
        {
            DateTimeOffset created = pvReqItem.Created;
            var pvReq = (TradeValuationRequest)pvReqItem.Data;
            var requestId = new Guid(pvReq.RequestId);

            // ignore requests if we are not running
            // ignore requests not processed by this server instance
            if ((GetState() != BasicServerState.Running) ||
                ((requestId.GetHashCode() % ServerFarmSize) != ServerInstance))
                return;

            var newRequest = new InternalTradeRevaluationRequest(requestId, created, pvReq);
            InternalTradeRevaluationRequest oldRequest = null;
            _outstandingRequests.Locked(requests =>
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
                var response = new HandlerResponse
                                   {
                    RequestId = pvReq.RequestId,
                    RequesterId = pvReq.RequesterId ?? new UserIdentity { Name = IntClient.Target.ClientInfo.Name, DisplayName = "Unknown" },
                    //RequesterData = pvReq.TradeId,
                    Status = RequestStatusEnum.Received
                };
                _workContext.Cache.SaveObject(response);
            }

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
                    InternalTradeRevaluationRequest activeRequest = null;
                    DateTimeOffset oldestRequestCreated = DateTimeOffset.MaxValue;
                    _outstandingRequests.Locked(requests =>
                    {
                        foreach (InternalTradeRevaluationRequest request in requests.Values)
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
                        TradeValuationRequest request = activeRequest.Request;
                        try
                        {
                            // run the valuation workflow
                            using (var workflow = new WFCalculateTradeValuation())
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
                                //RequesterData = request.TradeId,
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
