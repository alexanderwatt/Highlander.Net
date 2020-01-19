using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using nab.QDS.Build;
using National.QRSC.Contracts;
using nab.QDS.Core.Common;
using nab.QDS.Util.Expressions;
using nab.QDS.Util.Helpers;
using nab.QDS.Util.RefCounting;
using nab.QDS.Util.Servers;
using nab.QDS.Util.Threading;

namespace National.QRSC.Grid.Manager
{
    internal class InternalRequest
    {
        public readonly Guid RequestId;
        public UnassignedWorkflowRequest ExternalRequest;
        public CancellationRequest Cancellation;
        public RequestStatusEnum Status;
        //public string WorkerComputer;
        //public string WorkerInstance;
        public ExceptionDetail FaultDetail;
        public InternalRequest(Guid requestId)
        {
            RequestId = requestId;
        }
    }

    internal class InternalAvailability
    {
        public readonly string Computer;
        public readonly string Instance;
        private int _Availability;
        public InternalAvailability(WorkerAvailability availability)
        {
            Computer = availability.WorkerHostComputer;
            Instance = availability.WorkerHostInstance;
            _Availability = availability.AvailableNodeCount;
        }
        public int Decrement() { return Interlocked.Decrement(ref _Availability); }
        public int Availability { get { return Interlocked.Add(ref _Availability, 0); } }
    }

    internal class ManagerState
    {
        public readonly Dictionary<Guid, InternalRequest> InternalRequests = new Dictionary<Guid, InternalRequest>();
        //public readonly Dictionary<string, InternalAvailability> WorkerAvailability = new Dictionary<string, InternalAvailability>();
    }

    public class GridManagerServer : ServerBase2
    {
        public class Setting
        {
            //public const string HandlerLimit = "HandlerLimit";
        }

        private readonly Guarded<ManagerState> _State = new Guarded<ManagerState>(new ManagerState());

        private ICoreCache _UnassignedRequestsCache;
        private ICoreCache _WorkerAvailabilityCache;
        private ICoreCache _CancellationsCache;
        private ICoreCache _ManagerResponseCache;
        private Timer _HousekeepTimer;
        private TimeSpan HousekeepInterval = (WorkerAvailability.Const.LifeTime - TimeSpan.FromSeconds(5));

        private AsyncEventThrottle<object> _EventQueue;

        protected override void OnServerStarted()
        {
            _EventQueue = new AsyncEventThrottle<object>(ProcessRequests);
        }

        protected override void OnFirstCallback()
        {
            _ManagerResponseCache = _IntClient.Target.CreateCache();
            _ManagerResponseCache.Subscribe<ManagerResponse>(Expr.ALL, (subs, item) => { _EventQueue.Dispatch(null); }, null);
            _CancellationsCache = _IntClient.Target.CreateCache();
            _CancellationsCache.Subscribe<CancellationRequest>(Expr.ALL, (subs, item) => { _EventQueue.Dispatch(null); }, null);
            _UnassignedRequestsCache = _IntClient.Target.CreateCache();
            _UnassignedRequestsCache.Subscribe<UnassignedWorkflowRequest>(Expr.ALL, (subs, item) => { _EventQueue.Dispatch(null); }, null);
            _WorkerAvailabilityCache = _IntClient.Target.CreateCache();
            _WorkerAvailabilityCache.Subscribe<WorkerAvailability>(Expr.ALL, (subs, item) => { _EventQueue.Dispatch(null); }, null);

            _HousekeepTimer = new Timer(HousekeepTimeout, null, HousekeepInterval, HousekeepInterval);
            _EventQueue.Dispatch(null);
        }

        private void HousekeepTimeout(object notUsed)
        {
            _EventQueue.Dispatch(null);
        }

        protected override void OnServerStopping()
        {
            // stop housekeeping timer
            DisposeHelper.SafeDispose(ref _HousekeepTimer);
            // cleanup
            DisposeHelper.SafeDispose(ref _WorkerAvailabilityCache);
            DisposeHelper.SafeDispose(ref _UnassignedRequestsCache);
            DisposeHelper.SafeDispose(ref _CancellationsCache);
            DisposeHelper.SafeDispose(ref _ManagerResponseCache);
        }

        private static void PublishManagerResponse(ICoreClient client, InternalRequest internalRequest)
        {
            if (internalRequest.Status == RequestStatusEnum.Undefined)
                throw new ArgumentNullException("status");
            ManagerResponse response = new ManagerResponse()
            {
                RequestId = internalRequest.RequestId.ToString(),
                Status = internalRequest.Status,
                FaultDetail = internalRequest.FaultDetail
            };
            UserIdentity requesterId = null;
            if (internalRequest.ExternalRequest != null)
                requesterId = internalRequest.ExternalRequest.RequesterId;
            if ((requesterId == null) && (internalRequest.Cancellation != null))
                requesterId = internalRequest.Cancellation.RequesterId;
            response.RequesterId = requesterId;
            if ((response.Status == RequestStatusEnum.Cancelled) && (internalRequest.Cancellation != null))
                response.CancelReason = internalRequest.Cancellation.CancelReason;
            client.SaveObject<ManagerResponse>(response);
        }

        private void ProcessRequests(object notUsed)
        {
            try
            {
                BasicServerState state = this.GetState();
                if (state != BasicServerState.Running)
                {
                    return;
                }

                List<InternalRequest> existingRequests = new List<InternalRequest>();
                List<InternalRequest> receivedRequests = new List<InternalRequest>();
                List<InternalRequest> cancelledRequests = new List<InternalRequest>();
                //Dictionary<string, InternalAvailability> availableWorkers = new Dictionary<string, InternalAvailability>();
                _State.Locked((managerState) =>
                {
                    // - process responses
                    foreach (ICoreItem item in _ManagerResponseCache.Items)
                    {
                        try
                        {
                            ManagerResponse response = (ManagerResponse)item.Data;
                            Guid requestId = Guid.Parse(response.RequestId);
                            InternalRequest internalRequest;
                            if (!managerState.InternalRequests.TryGetValue(requestId, out internalRequest))
                                internalRequest = new InternalRequest(requestId);
                            internalRequest.Status = response.Status;
                            managerState.InternalRequests[requestId] = internalRequest;
                        }
                        catch (Exception excp)
                        {
                            Logger.Log(excp);
                        }
                    }
                    // - process requests
                    foreach (ICoreItem item in _UnassignedRequestsCache.Items)
                    {
                        try
                        {
                            UnassignedWorkflowRequest request = (UnassignedWorkflowRequest)item.Data;
                            Guid requestId = Guid.Parse(request.RequestId);
                            InternalRequest internalRequest;
                            if (!managerState.InternalRequests.TryGetValue(requestId, out internalRequest))
                                internalRequest = new InternalRequest(requestId);
                            internalRequest.ExternalRequest = request;
                            managerState.InternalRequests[requestId] = internalRequest;
                        }
                        catch (Exception excp)
                        {
                            Logger.Log(excp);
                        }
                    }
                    // - process cancellations
                    foreach (ICoreItem item in _CancellationsCache.Items)
                    {
                        try
                        {
                            CancellationRequest cancellation = (CancellationRequest)item.Data;
                            Guid requestId = Guid.Parse(cancellation.RequestId);
                            InternalRequest internalRequest;
                            if (!managerState.InternalRequests.TryGetValue(requestId, out internalRequest))
                                internalRequest = new InternalRequest(requestId);
                            internalRequest.Cancellation = cancellation;
                            managerState.InternalRequests[requestId] = internalRequest;
                        }
                        catch (Exception excp)
                        {
                            Logger.Log(excp);
                        }
                    }

                    // determine requests to be launched or cancelled
                    foreach (InternalRequest internalRequest in managerState.InternalRequests.Values)
                    {
                        if ((internalRequest.ExternalRequest != null) &&
                            ((internalRequest.Status == RequestStatusEnum.Undefined) || (internalRequest.Status == RequestStatusEnum.Received)))
                        {
                            // find requests to launch
                            if (internalRequest.Cancellation == null)
                            {
                                if (internalRequest.Status != RequestStatusEnum.Received)
                                {
                                    // new request
                                    internalRequest.Status = RequestStatusEnum.Received;
                                    receivedRequests.Add(internalRequest);
                                }
                                existingRequests.Add(internalRequest);
                            }
                            else
                            {
                                // cancelled
                                internalRequest.Status = RequestStatusEnum.Cancelled;
                                cancelledRequests.Add(internalRequest);
                            }
                        }
                    }

                });

                // publish cancelled requests
                foreach (InternalRequest request in cancelledRequests)
                    PublishManagerResponse(_IntClient.Target, request);

                // publish received status
                foreach (InternalRequest request in receivedRequests)
                    PublishManagerResponse(_IntClient.Target, request);

                // find available workers for received requests
                //Logger.LogDebug("Assigning {0} requests ...", existingRequests.Count);
                foreach (InternalRequest request in existingRequests)
                {
                    //Logger.LogDebug("----- Request: {0}", request.RequestId);
                    WorkerAvailability chosenWorker = null;
                    int highestAvailability = 0;
                    foreach (var item in _WorkerAvailabilityCache.Items)
                    {
                        WorkerAvailability worker = (WorkerAvailability)item.Data;
                        //Logger.LogDebug("----- Worker: {0} ({1})", worker.PrivateKey, worker.AvailableNodeCount);
                        if (worker.AvailableNodeCount > highestAvailability)
                        {
                            highestAvailability = worker.AvailableNodeCount;
                            chosenWorker = worker;
                        }
                    }
                    // exit if no workers available
                    if (chosenWorker == null)
                        break;
                    Logger.LogDebug("Assigned request '{0}' to {1} ({2})", request.RequestId, chosenWorker.PrivateKey, chosenWorker.AvailableNodeCount);
                    chosenWorker.AvailableNodeCount = chosenWorker.AvailableNodeCount - 1;
                    // publish assigned status
                    request.Status = RequestStatusEnum.Assigned;
                    PublishManagerResponse(_IntClient.Target, request);
                    // reassign to worker
                    RequestBase.TransferToWorker(_IntClient.Target, request.ExternalRequest, chosenWorker.WorkerHostComputer, chosenWorker.WorkerHostInstance);
                }
                //Logger.LogDebug("Assigned.");
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }
    }
}
