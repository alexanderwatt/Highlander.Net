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

namespace National.QRSC.Grid.Worker
{
    internal class InternalRequest
    {
        public readonly Guid RequestId;
        public AssignedWorkflowRequest ExternalRequest;
        public CancellationRequest Cancellation;
        public RequestStatusEnum Status;
        public ExceptionDetail FaultDetail;
        public InternalRequest(Guid requestId)
        {
            RequestId = requestId;
        }
    }

    public class GridWorkerServer : ServerBase2
    {
        public class Setting
        {
            public const string HandlerLimit = "HandlerLimit";
        }

        private readonly Guarded<Dictionary<Guid, InternalRequest>> _InternalRequests =
            new Guarded<Dictionary<Guid, InternalRequest>>(new Dictionary<Guid, InternalRequest>());

        private AsyncEventThrottle<object> _EventQueue;
        private ICoreCache _AssignedRequestsCache;
        private ICoreCache _CancellationsCache;
        private ICoreCache _WorkerResponseCache;
        private int _HandlersAvailable = Environment.ProcessorCount;
        private int _HandlersExecuting = 0;
        private int _AvailabilityChangeCount = 0;
        private DateTimeOffset _AvailabilityLastPublished = DateTimeOffset.MinValue;
        private Timer _HousekeepTimer;
        private TimeSpan HousekeepInterval = (WorkerAvailability.Const.LifeTime - TimeSpan.FromSeconds(5));

        private string _RequestHandlerFullPath = null;

        protected override void OnServerStarted()
        {
            _EventQueue = new AsyncEventThrottle<object>(ProcessRequests);

            // resolve handler location
            List<string> assemblyPaths = new List<string>();
            // unit test path
            assemblyPaths.Add(String.Format(@"..\..\..\..\..\Services\ServiceHost\RequestHandler\bin\{0}", BuildConst.BuildCfg));
            // development path only
            assemblyPaths.Add(String.Format(@"C:\Development\QRSC\Main\Source\Services\ServiceHost\RequestHandler\bin\{0}", BuildConst.BuildCfg));
            // deployed path
            assemblyPaths.Add(String.Format(@"C:\Program Files\National\QRSC_{0}\Services\Components", BuildConst.BuildEnv));
            assemblyPaths.Add(String.Format(@"C:\Program Files (x86)\National\QRSC_{0}\Services\Components", BuildConst.BuildEnv));

            string exeFilename = "National.QRSC.Grid.Handler.exe";
            // not cached - load from assembly
            foreach (string assemblyPath in assemblyPaths)
            {
                string exeFullPath = Path.GetFullPath(String.Format(@"{0}\{1}", assemblyPath, exeFilename));
                if (File.Exists(exeFullPath))
                {
                    _RequestHandlerFullPath = exeFullPath;
                    Logger.LogDebug("Request handler path: {0}", _RequestHandlerFullPath);
                    break;
                }
            }
            if(_RequestHandlerFullPath==null)
                throw new FileNotFoundException("Request handler", exeFilename);

            // settings
            _HandlersAvailable = this.OtherSettings.GetValue<int>(Setting.HandlerLimit, Environment.ProcessorCount);

            // started
            Logger.LogDebug("HostInstance: '{0}'", HostInstance);
            Logger.LogDebug("HandlerCount: {0}", _HandlersAvailable);
        }

        protected override void OnFirstCallback()
        {
            _WorkerResponseCache = _IntClient.Target.CreateCache();
            _WorkerResponseCache.Subscribe<WorkerResponse>(
                Expr.BoolAND(
                    Expr.IsEQU(RequestBase.Prop.WorkerHostComputer, Environment.MachineName),
                    (HostInstance == null) ?
                        Expr.IsNull(RequestBase.Prop.WorkerHostInstance) :
                        Expr.IsEQU(RequestBase.Prop.WorkerHostInstance, HostInstance)),
                 (subs, item) =>
                 {
                     _EventQueue.Dispatch(null);
                 }, null);
            _CancellationsCache = _IntClient.Target.CreateCache();
            _CancellationsCache.Subscribe<CancellationRequest>(Expr.ALL, (subs, item) =>
                {
                    _EventQueue.Dispatch(null);
                }, null);
            _AssignedRequestsCache = _IntClient.Target.CreateCache();
            _AssignedRequestsCache.Subscribe<AssignedWorkflowRequest>(
                Expr.BoolAND(
                    Expr.IsEQU(RequestBase.Prop.WorkerHostComputer, Environment.MachineName),
                    (HostInstance == null) ?
                        Expr.IsNull(RequestBase.Prop.WorkerHostInstance) :
                        Expr.IsEQU(RequestBase.Prop.WorkerHostInstance, HostInstance)),
                 (subs, item) =>
                 {
                     _EventQueue.Dispatch(null);
                 }, null);

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
            // wait for worker threads to stop
            WaitHelper.WaitFor(
                TimeSpan.FromSeconds(30),
                () => { return (Interlocked.Add(ref _HandlersExecuting, 0) <= 0); },
                null,
                TimeSpan.FromSeconds(5),
                () => { Logger.LogDebug("Waiting for {0} request handlers to stop...", Interlocked.Add(ref _HandlersExecuting, 0)); });
            // cleanup
            DisposeHelper.SafeDispose(ref _AssignedRequestsCache);
            DisposeHelper.SafeDispose(ref _CancellationsCache);
            DisposeHelper.SafeDispose(ref _WorkerResponseCache);
            DisposeHelper.SafeDispose(ref _EventQueue);
        }

        private static void PublishWorkerResponse(ICoreClient client, InternalRequest internalRequest, string hostInstance)
        {
            if (internalRequest.Status == RequestStatusEnum.Undefined)
                throw new ArgumentNullException("status");
            WorkerResponse response = new WorkerResponse()
            {
                RequestId = internalRequest.RequestId.ToString(),
                WorkerHostComputer = Environment.MachineName,
                WorkerHostInstance = hostInstance,
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
            client.SaveObject<WorkerResponse>(response);
        }

        private void ProcessRequests(object notUsed)
        {
            try
            {
                // process rules and run handlers as required
                BasicServerState state = this.GetState();
                if (state != BasicServerState.Running)
                {
                    return;
                }

                // subscription is ready and callback flood has stopped
                List<InternalRequest> existingRequests = new List<InternalRequest>();
                List<InternalRequest> enqueuedRequests = new List<InternalRequest>();
                List<InternalRequest> cancelledRequests = new List<InternalRequest>();
                var requestItems = _AssignedRequestsCache.Items;
                var cancellationItems = _CancellationsCache.Items;
                var responseItems = _WorkerResponseCache.Items;
                _InternalRequests.Locked((internalRequests) =>
                {
                    // - process responses
                    foreach (ICoreItem item in responseItems)
                    {
                        try
                        {
                            WorkerResponse response = (WorkerResponse)item.Data;
                            Guid requestId = Guid.Parse(response.RequestId);
                            InternalRequest internalRequest;
                            if (!internalRequests.TryGetValue(requestId, out internalRequest))
                                internalRequest = new InternalRequest(requestId);
                            internalRequest.Status = response.Status;
                            internalRequests[requestId] = internalRequest;
                        }
                        catch (Exception excp)
                        {
                            Logger.Log(excp);
                        }
                    }
                    // - process requests
                    foreach (ICoreItem item in requestItems)
                    {
                        try
                        {
                            AssignedWorkflowRequest request = (AssignedWorkflowRequest)item.Data;
                            Guid requestId = Guid.Parse(request.RequestId);
                            InternalRequest internalRequest;
                            if (!internalRequests.TryGetValue(requestId, out internalRequest))
                                internalRequest = new InternalRequest(requestId);
                            internalRequest.ExternalRequest = request;
                            internalRequests[requestId] = internalRequest;
                        }
                        catch (Exception excp)
                        {
                            Logger.Log(excp);
                        }
                    }
                    // - process cancellations
                    foreach (ICoreItem item in cancellationItems)
                    {
                        try
                        {
                            CancellationRequest cancellation = (CancellationRequest)item.Data;
                            Guid requestId = Guid.Parse(cancellation.RequestId);
                            InternalRequest internalRequest;
                            if (!internalRequests.TryGetValue(requestId, out internalRequest))
                                internalRequest = new InternalRequest(requestId);
                            internalRequest.Cancellation = cancellation;
                            internalRequests[requestId] = internalRequest;
                        }
                        catch (Exception excp)
                        {
                            Logger.Log(excp);
                        }
                    }

                    // determine requests to be launched or cancelled
                    foreach (InternalRequest internalRequest in internalRequests.Values)
                    {
                        if ((internalRequest.ExternalRequest != null) &&
                            ((internalRequest.Status == RequestStatusEnum.Undefined) || (internalRequest.Status == RequestStatusEnum.Enqueued)))
                        {
                            // find requests to launch
                            if (internalRequest.Cancellation == null)
                            {
                                if (internalRequest.Status != RequestStatusEnum.Enqueued)
                                {
                                    // new request
                                    internalRequest.Status = RequestStatusEnum.Enqueued;
                                    enqueuedRequests.Add(internalRequest);
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

                // publish status for cancelled and enqueued requests
                foreach (InternalRequest request in cancelledRequests)
                    PublishWorkerResponse(_IntClient.Target, request, HostInstance);

                // now find handlers to launch
                foreach (InternalRequest request in enqueuedRequests)
                {
                    // publish enqueued status
                    PublishWorkerResponse(_IntClient.Target, request, HostInstance);
                }
                foreach (InternalRequest request in existingRequests)
                {
                    // launch if handler available
                    int count = Interlocked.Decrement(ref _HandlersAvailable);
                    if (count >= 0)
                    {
                        Interlocked.Increment(ref _AvailabilityChangeCount);
                        Interlocked.Increment(ref _HandlersExecuting);
                        // publish launched status
                        request.Status = RequestStatusEnum.Launched;
                        PublishWorkerResponse(_IntClient.Target, request, HostInstance);
                        // launch
                        ThreadPool.QueueUserWorkItem(Launch, new LaunchPackage(_IntClient, request));
                    }
                    else
                        Interlocked.Increment(ref _HandlersAvailable);
                }

                 // publish availability (throttled)
                int changeCount = Interlocked.Exchange(ref _AvailabilityChangeCount, 0);
                if ((changeCount > 0) || ((DateTimeOffset.Now - _AvailabilityLastPublished) > TimeSpan.FromSeconds(5)))
                {
                    _IntClient.Target.SaveObject<WorkerAvailability>(new WorkerAvailability()
                    {
                        WorkerHostComputer = Environment.MachineName,
                        WorkerHostInstance = this.HostInstance,
                        AvailableNodeCount = Interlocked.Add(ref _HandlersAvailable, 0)
                    });
                    _AvailabilityLastPublished = DateTimeOffset.Now;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        private class LaunchPackage : IDisposable
        {
            private Reference<ICoreClient> _ClientRef;
            private InternalRequest _Request;
            public LaunchPackage(Reference<ICoreClient> clientRef, InternalRequest request)
            {
                _ClientRef = clientRef.Clone();
                _Request = request;
            }
            public ICoreClient Client { get { return _ClientRef.Target; } }
            public InternalRequest Request { get { return _Request; } }
            public void Dispose()
            {
                DisposeHelper.SafeDispose(ref _ClientRef);
            }
        }

        private void Launch(object state)
        {
            try
            {
                using (LaunchPackage package = (LaunchPackage)state)
                {
                    try
                    {
                        string workDir = Path.GetDirectoryName(_RequestHandlerFullPath);
                        string exeFile = _RequestHandlerFullPath;
                        string arguments = String.Format("/env:{0} /hiid:{1} /reqid:{2}",
                            EnvHelper.EnvName(package.Client.ClientInfo.ConfigEnv),
                            HostInstance ?? "Default",
                            package.Request.RequestId);
                        ProcessStartInfo psi = new ProcessStartInfo(exeFile, arguments);
                        psi.WorkingDirectory = workDir;
                        psi.UseShellExecute = true; // default - no io redir
                        psi.ErrorDialog = true;
                        psi.WindowStyle = ProcessWindowStyle.Normal;
                        //psi.UserName = xxx;
                        //psi.Password = xxx;
                        Process p = new Process();
                        p.StartInfo = psi;
                        if (p.Start())
                        {
                            Logger.LogDebug("{0} Started: {1} {2}", package.Request.RequestId, exeFile, arguments);
                            while (!p.WaitForExit(5000)) // 5 seconds
                            {
                                Logger.LogDebug("{0} Running...", package.Request.RequestId);
                            }
                            // exit codes:
                            //   1  success
                            //   0  failed (exception was published)
                            //  -1  catastrophic failure (logged to local file)
                            //  -2  catastrophic failure (logged to console only)
                            int exitCode = p.ExitCode;
                            TimeSpan duration = p.ExitTime - p.StartTime;
                            Logger.LogDebug("{0} Stopped: ExitCode={1} (Duration={2})", package.Request.RequestId, exitCode, duration);
                            if (exitCode < 0)
                            {
                                // request handler failed to publish faulted request status - so we do it here
                                throw new ApplicationException(String.Format("Request '{0}' terminated abnormally: {1}", package.Request.RequestId, exitCode));
                            }
                        }
                        else
                        {
                            throw new ApplicationException(String.Format("Request '{0}' failed to start.", package.Request.RequestId));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        package.Request.Status = RequestStatusEnum.Faulted;
                        package.Request.FaultDetail = new ExceptionDetail(ex);
                        PublishWorkerResponse(package.Client, package.Request, HostInstance);
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref _HandlersExecuting);
                Interlocked.Increment(ref _HandlersAvailable);
                Interlocked.Increment(ref _AvailabilityChangeCount);
                _EventQueue.Dispatch(null);
            }
        }
    }
}
