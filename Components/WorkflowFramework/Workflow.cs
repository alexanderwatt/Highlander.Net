/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using Core.Common;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.Threading;

#endregion


namespace Orion.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public enum WorkStatus
    {
        Initial,
        Running,
        Stopped
    }

    /// <summary>
    /// 
    /// </summary>
    public enum GridLevel
    {
        Undefined,
        Client,
        Worker,
        Router
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IWorkstep : IDisposable
    {
        Type InputType { get; }
        Type OutputType { get; }
        void Initialise(IWorkContext context);
        void EnableGrid(GridLevel gridLevel, Guid nodeId, int routerPort, string routerHost);
        IWorkflowOutput ExecuteObj(object input);
        IAsyncResult BeginExecuteObj(object input);
        IWorkflowOutput EndExecuteObj(IAsyncResult ar);
        WorkStatus Status { get; }
        string Progress { get; }
        double ProgressRatio { get; }
    }

    [DataContract]
    public class WorkflowError
    {
        [DataMember]
        public string FullName;

        [DataMember]
        public string Message;

        [DataMember]
        public string Source;

        [DataMember]
        public string StackTrace;

        [DataMember]
        public WorkflowError InnerError;

        // constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public WorkflowError(Exception e)
        {
            FullName = e.GetType().FullName;
            Message = e.Message;
            Source = e.Source;
            StackTrace = e.StackTrace;
            if (e.InnerException != null)
                InnerError = new WorkflowError(e.InnerException);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class WorkflowException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public override string StackTrace { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        public WorkflowException(WorkflowError error)
            : base(error.Message, error.InnerError != null ? new WorkflowException(error.InnerError) : null)
        {
            Source = error.Source;
            StackTrace = error.StackTrace;
        }
    }

    public interface IWorkflowOutput
    {
        object GetResult();
        WorkflowError[] GetErrors();
    }

    [DataContract]
    public class WorkflowOutput<TOut> : IWorkflowOutput
    {
        [DataMember]
        public TOut Result;

        [DataMember]
        public WorkflowError[] Errors;

        // IWorkflowOutput methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object GetResult() { return Result; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public WorkflowError[] GetErrors() { return Errors; }
    }

    public static class WorkflowHelper
    {
        private static string FormatError(WorkflowError error)
        {
            string result = $"EXCEPTION: {error.FullName}: {error.Message}{Environment.NewLine}{error.StackTrace}";
            if (error.InnerError != null)
            {
                result += $"{Environment.NewLine}    INNER {FormatError(error.InnerError)}";
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool LogError(ILogger logger, WorkflowError error)
        {
            // returns true if error logged
            if (error != null)
                logger.LogError(FormatError(error));
            return error != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static bool LogErrors(ILogger logger, WorkflowError[] errors)
        {
            // returns true if error(s) logged
            bool failed = false;
            if (errors != null)
            {
                failed = errors.Aggregate(failed, (current, error) => current || LogError(logger, error));
            }
            return failed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errors"></param>
        public static void ThrowErrors(WorkflowError[] errors)
        {
            // throws the 1st error in the list (if any)
            if (errors != null)
            {
                foreach (WorkflowError error in errors)
                {
                    throw new WorkflowException(error);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public interface IWorkstep<in Tin, Tout> : IWorkstep
    {
        WorkflowOutput<Tout> Execute(Tin input);
        AsyncResult<WorkflowOutput<Tout>> BeginExecute(Tin input, AsyncCallback callback);
        WorkflowOutput<Tout> EndExecute(AsyncResult<WorkflowOutput<Tout>> ar);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IWorkflow
    {
        IWorkstep FirstStep { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IWorkContext
    {
        ILogger Logger { get; }
        ICoreCache Cache { get; }
        //String NameSpace { get; }
        string HostComputer { get; }
        string HostInstance { get; }
        int FarmInstance { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WorkContext : IWorkContext
    {
        public ILogger Logger { get; }
        public ICoreCache Cache { get; }
        //public String NameSpace { get; private set; }
        public string HostComputer { get; }
        public string HostInstance { get; }
        public int FarmInstance { get; }
        // constructors
        //public WorkContext(ILogger logger, ICoreCache cache, string hostInstance, int? farmInstance = null)
        //    : this(logger, cache, EnvironmentProp.DefaultNameSpace, hostInstance, farmInstance)
        //{
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="hostInstance"></param>
        /// <param name="farmInstance"></param>
        public WorkContext(ILogger logger, ICoreCache cache, string hostInstance, int? farmInstance = null)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Cache = cache;
            HostComputer = Environment.MachineName;
            HostInstance = hostInstance;
            FarmInstance = farmInstance ?? 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SubStep
    {
        public IWorkstep Step { get; }
        public long Work = 1;
        public SubStep(IWorkstep step) { Step = step; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IWorkProgress
    {
        WorkStatus Status { get; }
        long WorkDone { get; }
        long WorkToDo { get; }
        double Ratio { get; }
        TimeSpan Duration { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WorkProgress : IWorkProgress
    {
        /// <summary>
        /// 
        /// </summary>
        public WorkStatus Status { get; }

        /// <summary>
        /// 
        /// </summary>
        public long WorkDone { get; }

        /// <summary>
        /// 
        /// </summary>
        public long WorkToDo { get; }

        private readonly DateTimeOffset _timeStarted;
        private readonly DateTimeOffset _timeStopped;

        /// <summary>
        /// 
        /// </summary>
        public double Ratio
        {
            get
            {
                if (WorkToDo == 0)
                    return 0.0;
                return WorkDone / (double)WorkToDo;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="workDone"></param>
        /// <param name="workToDo"></param>
        /// <param name="timeStarted"></param>
        /// <param name="timeStopped"></param>
        public WorkProgress(WorkStatus status, long workDone, long workToDo, DateTimeOffset timeStarted, DateTimeOffset timeStopped)
        {
            Status = status;
            WorkDone = workDone;
            WorkToDo = workToDo;
            _timeStarted = timeStarted;
            _timeStopped = timeStopped;
        }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Duration => (_timeStopped - _timeStarted);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class WorkstepBase<Tin, Tout> : IWorkstep<Tin, Tout>, IWorkgridSwitchV101<Tin, Tout>
    {
        protected IWorkContext Context;

        // todo - remove progress statistics - invalid in singleton mode
        public WorkStatus Status { get; protected set; } = WorkStatus.Initial;
        protected long WorkDone;
        protected long WorkToDo;
        protected DateTimeOffset TimeStarted = DateTimeOffset.MinValue;
        protected DateTimeOffset TimeStopped = DateTimeOffset.MinValue;

        private string _cancelReason;
        public bool Cancelled => (_cancelReason != null);
        public string CancelReason => _cancelReason;
        public void Cancel(string reason) { _cancelReason = reason ?? "(unknown)"; }

        protected virtual IWorkProgress OnGetProgress()
        {
            long workToDo = Interlocked.Add(ref WorkToDo, 0);
            long workDone = Interlocked.Add(ref WorkDone, 0);
            return new WorkProgress(Status, workDone, workToDo, TimeStarted, TimeStopped);
        }

        protected virtual string OnFormatProgress(IWorkProgress progress)
        {
            return progress.Ratio.ToString("P");
        }

        /// <summary>
        /// 
        /// </summary>
        public double ProgressRatio => OnGetProgress().Ratio;

        /// <summary>
        /// 
        /// </summary>
        public string Progress => OnFormatProgress(OnGetProgress());

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                WorkStatus state = Status;
                if (state == WorkStatus.Initial)
                    return TimeSpan.Zero;
                if (state == WorkStatus.Stopped)
                    return (TimeStopped - TimeStarted);
                return (DateTimeOffset.Now - TimeStarted);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public delegate Tout ExecuteDelegate(Tin input);

        protected ExecuteDelegate ExecuteCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="excp"></param>
        /// <returns></returns>
        public delegate Tout ExceptionDelegate(Tin input, Exception excp);

        protected ExceptionDelegate ExceptionCallback;

        /// <summary>
        /// 
        /// </summary>
        public Type InputType => typeof(Tin);

        /// <summary>
        /// 
        /// </summary>
        public Type OutputType => typeof(Tout);

        protected List<SubStep> SubSteps;

        protected void AddSubstep(IWorkstep step)
        {
            if (SubSteps == null)
                SubSteps = new List<SubStep>();
            SubSteps.Add(new SubStep(step));
        }

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public WorkstepBase()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executeCallback"></param>
        /// <param name="exceptionHandler"></param>
        public WorkstepBase(ExecuteDelegate executeCallback, ExceptionDelegate exceptionHandler)
        {
            ExecuteCallback = executeCallback;
            ExceptionCallback = exceptionHandler;
        }

        protected virtual void OnInitialise() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Initialise(IWorkContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            if (Context.Logger == null)
                throw new ArgumentNullException(nameof(context));
            Status = WorkStatus.Initial;
            TimeStarted = DateTimeOffset.MinValue;
            TimeStopped = DateTimeOffset.MinValue;
            // custom initialisation
            OnInitialise();
            // initialise substeps
            if (SubSteps != null)
            {
                foreach (SubStep subStep in SubSteps)
                    subStep.Step.Initialise(Context);
            }
        }

        protected virtual void OnShutdown() { }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            // delist worker
            try
            {
                if (_gridLevel == GridLevel.Worker)
                    _clientBase.DelistWorkerV101(_gridNodeId);
            }
            catch (Exception e)
            {
                //this_crashes;
                Context?.Logger.LogDebug("Worker: Failed to delist: {0}", e);
            }
            // dispose substeps
            if (SubSteps != null)
            {
                foreach (SubStep subStep in SubSteps)
                {
                    try
                    {
                        subStep.Step.Dispose();
                    }
                    catch (Exception e)
                    {
                        Context?.Logger.LogDebug("Dispose substep failed: {0}", e);
                    }
                }
            }
            // custom initialisation
            try
            {
                OnShutdown();
            }
            catch (Exception e)
            {
                Context?.Logger.LogDebug("Failed custom shutdown: {0}", e);
            }
            // dispose this
            DisposeHelper.SafeDispose(ref _clientBase);
            DisposeHelper.SafeDispose(ref _serverHost);
            _gridWorkerPool.Dispose();
        }

        protected virtual WorkflowOutput<Tout> OnSubSteps(Tin input)
        {
            return null;
        }

        protected virtual Tout OnExecute(Tin input)
        {
            if (ExecuteCallback != null)
                return ExecuteCallback(input);
            throw new NotImplementedException();
        }

        protected virtual Tout OnException(Tin input, Exception excp)
        {
            if (ExceptionCallback != null)
                return ExceptionCallback(input, excp);
            Context.Logger.Log(excp);
            throw excp;
        }

        private WorkflowOutput<Tout> ExecuteLocal(Tin input)
        {
            Status = WorkStatus.Running;
            TimeStarted = DateTimeOffset.Now;
            TimeStopped = TimeStarted;
            try
            {
                try
                {
                    // process seq, alt, par overrides 1st
                    WorkflowOutput<Tout> result = OnSubSteps(input);
                    if (result != null)
                        return result;
                    // call user overrides (if any)
                    try
                    {
                        return new WorkflowOutput<Tout>
                                   {
                            Result = OnExecute(input),
                            Errors = new WorkflowError[0]
                        };
                    }
                    catch (Exception e3)
                    {
                        // allow user to handle exceptions
                        return new WorkflowOutput<Tout>
                                   {
                            Result = OnException(input, e3),
                            Errors = new WorkflowError[0]
                        };
                    }
                }
                catch (Exception e2)
                {
                    // unhandled exception
                    Context.Logger.LogError("{0} execution failed: {1}", GetType().Name, e2);
                    return new WorkflowOutput<Tout>
                               {
                        Result = default(Tout),
                        Errors = new[] { new WorkflowError(e2) }
                    };
                }
            }
            finally
            {
                Status = WorkStatus.Stopped;
                TimeStopped = DateTimeOffset.Now;
            }
        }

        private WorkflowError[] AccumulateErrors(IEnumerable<WorkflowError> errors1, IEnumerable<WorkflowError> errors2)
        {
            var result = new List<WorkflowError>();
            if (errors1 != null)
                result.AddRange(errors1);
            if (errors2 != null)
                result.AddRange(errors2);
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public WorkflowOutput<Tout> Execute(Tin input)
        {
            //_Context.Logger.LogDebug("Execute: Entry -->");
            // dispatch to grid if enabled
            if (_gridLevel == GridLevel.Client)
            {
                // call the router
                try
                {
                    Context.Logger.LogDebug("Execute: (call) --> DispatchToWorkerV101");
                    WorkgridOutput<Tout> gridOutput = _clientBase.DispatchToWorkerV101(_gridNodeId, input);
                    Context.Logger.LogDebug("Execute: (back) <-- DispatchToWorkerV101");
                    // todo? - handle grid error? - for now just accumulate it
                    return new WorkflowOutput<Tout>
                        {
                                   Result = gridOutput.Result,
                                   Errors = AccumulateErrors(gridOutput.FlowErrors, gridOutput.GridErrors)
                               };
                }
                catch (Exception gridExcp)
                {
                    Context.Logger.LogDebug("Execute: (excp) <-- DispatchToWorkerV101");
                    // router catastrophic error
                    Context.Logger.Log(gridExcp);
                    return new WorkflowOutput<Tout>
                               {
                                   Result = default(Tout),
                                   Errors = new[] { new WorkflowError(gridExcp) }
                               };
                }
            }
            return ExecuteLocal(input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IWorkflowOutput ExecuteObj(object input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (input.GetType() != typeof(Tin))
                throw new ArgumentException("Unknown type", nameof(input));
            return Execute((Tin)input);
        }

        // asynchronous execution
        private void AsyncExecute(object state)
        {
            var ar = (AsyncResult<WorkflowOutput<Tout>>)state;
            try
            {
                var input = (Tin)ar.AsyncState;
                WorkflowOutput<Tout> result = Execute(input);
                ar.SetAsCompleted(result, false);
            }
            catch (Exception excp)
            {
                ar.SetAsCompleted(excp, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IAsyncResult BeginExecuteObj(object input)
        {
            var ar = new AsyncResult<WorkflowOutput<Tout>>(null, (Tin)input);
            ThreadPool.QueueUserWorkItem(AsyncExecute, ar);
            return ar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AsyncResult<WorkflowOutput<Tout>> BeginExecute(Tin input, AsyncCallback callback)
        {
            var ar = new AsyncResult<WorkflowOutput<Tout>>(callback, input);
            ThreadPool.QueueUserWorkItem(AsyncExecute, ar);
            return ar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public IWorkflowOutput EndExecuteObj(IAsyncResult ar)
        {
            var result = (AsyncResult<WorkflowOutput<Tout>>)ar;
            return result.EndInvoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public WorkflowOutput<Tout> EndExecute(AsyncResult<WorkflowOutput<Tout>> ar)
        {
            return ar.EndInvoke();
        }

        // ======================================== grid extensions ========================================
        private WorkgridSwitchSenderV101<Tin, Tout> _clientBase;
        private CustomServiceHost<IWorkgridSwitchV101<Tin, Tout>, WorkgridSwitchRecverV101<Tin, Tout>> _serverHost;
        private readonly Guarded<GridWorkerPool<Tin, Tout>> _gridWorkerPool =
            new Guarded<GridWorkerPool<Tin, Tout>>(new GridWorkerPool<Tin, Tout>());
        //protected IWorkgridConfig _GridConfig;
        //private Guid _GridConfigNodeGuid;
        //private bool _GridConfigIsClient;
        //private bool _GridConfigIsWorker;
        //private bool _GridConfigIsRouter;
        private GridLevel _gridLevel;
        private Guid _gridNodeId;
        private string _gridRouterHost;
        private int _gridRouterPort;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string InterfaceNameHash()
        {
            string result =
                $"{typeof(WorkstepBase<Tin, Tout>).AssemblyQualifiedName}/{typeof(Tin).FullName}/{typeof(Tout).FullName}";
            return result.GetHashCode().ToString("X");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridLevel"></param>
        /// <param name="nodeId"></param>
        /// <param name="routerPort"></param>
        /// <param name="routerHost"></param>
        public void EnableGrid(GridLevel gridLevel, Guid nodeId, int routerPort, string routerHost)
        {
            //return; // todo - disabled until WCF threading issue sorted
            if (gridLevel == GridLevel.Undefined)
                return; // not required
            if (_gridLevel > GridLevel.Undefined)
                return; //  already initialised
            _gridLevel = gridLevel;
            _gridNodeId = nodeId;
            _gridRouterPort = routerPort;
            _gridRouterHost = routerHost;
            string svcName = EnvHelper.SvcPrefix(SvcId.GridSwitch);
            // create server host (for router or worker or both)
            if (_gridLevel == GridLevel.Worker || _gridLevel == GridLevel.Router)
            {
                const int minPort = 10000;
                const int maxPort = 65535;
                int maxAttempts = 10;
                var random = new Random(Environment.TickCount);
                int port = random.Next(minPort, maxPort);
                if (_gridLevel == GridLevel.Router)
                {
                    maxAttempts = 1;
                    port = _gridRouterPort;
                }
                int attempt = 0;
                while (_serverHost == null)
                {
                    if (attempt >= maxAttempts)
                        throw new ApplicationException(
                            $"Aborting - open host attempt limit ({maxAttempts}) reached!");
                    attempt++;
                    try
                    {
                        string endpoint = ServiceHelper.FormatEndpoint(WcfConst.NetTcp, port);
                        _serverHost = new CustomServiceHost<IWorkgridSwitchV101<Tin, Tout>, WorkgridSwitchRecverV101<Tin, Tout>>(
                            Context.Logger, new WorkgridSwitchRecverV101<Tin, Tout>(this), endpoint,
                            svcName, InterfaceNameHash(), true);
                        // log
                        foreach (string address in _serverHost.GetIpV4Addresses(null))
                        {
                            Context.Logger.LogDebug("{0}: Started  {1} -> {2}", _gridLevel, address, GetType().Name);
                        }
                    }
                    catch (AddressAlreadyInUseException e1)
                    {
                        Context.Logger.LogDebug("{0}.EnableGrid: Failed to open port {1}: {2}",
                            GetType().Name, port, e1.GetType().Name);
                        DisposeHelper.SafeDispose(ref _serverHost);
                        port = random.Next(minPort, maxPort);
                    }
                    catch (InvalidOperationException e2)
                    {
                        Context.Logger.LogDebug("{0}.EnableGrid: Failed to open port {1}: {2}",
                            GetType().Name, port, e2.GetType().Name);
                        DisposeHelper.SafeDispose(ref _serverHost);
                        port = random.Next(minPort, maxPort);
                    }
                } // while
            }
            // create client base (for worker or client)
            if (_gridLevel == GridLevel.Worker || _gridLevel == GridLevel.Client)
            {
                AddressBinding addressBinding = WcfHelper.CreateAddressBinding(
                    WcfConst.NetTcp, _gridRouterHost, _gridRouterPort,
                    svcName, InterfaceNameHash());
                _clientBase = new WorkgridSwitchSenderV101<Tin, Tout>(addressBinding);
            }
            // register worker with router
            if (_gridLevel == GridLevel.Worker)
            {
                foreach (string address in _serverHost.GetIpV4Addresses(null))
                {
                    _clientBase.EnlistWorkerV101(_gridNodeId, Environment.ProcessorCount, address);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public WorkflowOutput<Tout> ExecuteLocalCallV101(Tin input)
        {
            Context.Logger.LogDebug("ExecuteLocalCallV101: Entry -->");
            try
            {
                if (_gridLevel != GridLevel.Worker)
                    throw new NotSupportedException("Only workers can accept calls!");
                return ExecuteLocal(input);
            }
            finally
            {
                Context.Logger.LogDebug("ExecuteLocalCallV101: Leave <--");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="initFreeWorkers"></param>
        /// <param name="address"></param>
        public void EnlistWorkerV101(Guid nodeId, int initFreeWorkers, string address)
        {
            if (_gridLevel != GridLevel.Router)
                throw new NotSupportedException("Only routers can enlist workers!");

            // ensure node counter is initialised
            NodeCounterMap.GetCounter(nodeId, initFreeWorkers);

            // enlist
            AddressBinding addressBinding = WcfHelper.CreateAddressBinding(
                address);
            var clientBase = new WorkgridSwitchSenderV101<Tin, Tout>(addressBinding);
            var gws = new GridWorkerState<Tin, Tout>(nodeId, address, clientBase);
            _gridWorkerPool.Locked(workerPool => workerPool.WorkerNodeMap.Add(nodeId, gws));

            // done
            Context.Logger.LogDebug("Router: Enlisted {1} -> {0}", GetType().Name, address);
        }

        private void DelistWorker(Guid nodeId)
        {
            GridWorkerState<Tin, Tout> gws = null;
            _gridWorkerPool.Locked(workerPool =>
            {
                if (workerPool.WorkerNodeMap.TryGetValue(nodeId, out gws))
                {
                    workerPool.WorkerNodeMap.Remove(nodeId);
                }
            });
            // done
            if (gws != null)
            {
                Context.Logger.LogDebug("Router: Delisted {1} -> {0}", GetType().Name, gws.Address);
                gws.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        public void DelistWorkerV101(Guid nodeId)
        {
            if (_gridLevel != GridLevel.Router)
                throw new NotSupportedException("Only routers can delist workers!");

            DelistWorker(nodeId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceNode"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public WorkgridOutput<Tout> DispatchToWorkerV101(Guid sourceNode, Tin input)
        {
            Context.Logger.LogDebug("DispatchToWorkerV101: Entry -->");
            try
            {
                var gridErrors = new List<WorkflowError>();
                try
                {
                    if (_gridLevel != GridLevel.Router)
                        throw new NotSupportedException("Only routers can dispatch to workers!");

                    NodeCounter sourceNodeCounter = NodeCounterMap.GetCounter(sourceNode, -1);
                    sourceNodeCounter.IncrementFreeWorkers();
                    try
                    {
                        const int maxAttempts = 10;
                        int attempt = 0;
                        while (attempt < maxAttempts)
                        {
                            attempt++;
                            // find least laziest worker and forward call to it
                            GridWorkerState<Tin, Tout> worker = null;
                            _gridWorkerPool.Locked(workerPool =>
                            {
                                // - get worker ids
                                List<Guid> workerList = workerPool.WorkerNodeMap.Keys.ToList();
                                //});
                                // - find laziest
                                NodeCounter bestNodeCounter = NodeCounterMap.FindBestNode(workerList);
                                // - try to call worker
                                //_GridWorkerPool.Locked((workerPool) =>
                                //{
                                if (bestNodeCounter != null)
                                    worker = workerPool.WorkerNodeMap[bestNodeCounter.NodeId];
                            });
                            // - fail if no worker found
                            if (worker == null)
                            {
                                throw new ApplicationException("No workers enlisted!");
                            }
                            try
                            {
                                NodeCounter targetNodeCounter = NodeCounterMap.GetCounter(worker.NodeGuid, -1);
                                targetNodeCounter.DecrementFreeWorkers();
                                try
                                {
                                    // call the worker
                                    Context.Logger.LogDebug("DispatchToWorkerV101: (call) --> ExecuteLocalCallV101");
                                    WorkflowOutput<Tout> flowOutput = worker.ClientBase.ExecuteLocalCallV101(input);
                                    Context.Logger.LogDebug("DispatchToWorkerV101: (back) <-- ExecuteLocalCallV101");
                                    return new WorkgridOutput<Tout>
                                               {
                                        Result = flowOutput.Result,
                                        FlowErrors = flowOutput.Errors,
                                        GridErrors = gridErrors.ToArray()
                                    };
                                }
                                finally
                                {
                                    targetNodeCounter.IncrementFreeWorkers();
                                }
                            }
                            catch (Exception excp)
                            {
                                Context.Logger.LogDebug("DispatchToWorkerV101: (excp) <-- ExecuteLocalCallV101");
                                // all must be trapped so the worker gets delisted
                                // retry if it is a communication exception - todo
                                Context.Logger.LogWarning("Router: Call to {0} failed: {1}", worker.Address, excp);

                                // remove worker from pool
                                DelistWorker(worker.NodeGuid);
                            }
                        } // while

                        // attemp retry limit exceeded
                        throw new ApplicationException($"Attempt limit ({maxAttempts}) reached!");
                    }
                    finally
                    {
                        sourceNodeCounter.DecrementFreeWorkers();
                    }
                }
                catch (Exception excp2)
                {
                    // serious exception
                    Context.Logger.LogError("Router: ({0}) {1}", GetType().FullName, excp2);
                    gridErrors.Add(new WorkflowError(excp2));
                    return new WorkgridOutput<Tout>
                               {
                        Result = default(Tout),
                        GridErrors = gridErrors.ToArray()
                    };
                }
            }
            finally
            {
                Context.Logger.LogDebug("DispatchToWorkerV101: Leave <--");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class WorkstepBaseSeq<Tin, Tout> : WorkstepBase<Tin, Tout>
    {
        // constructors
        // - for subclasses
        /// <summary>
        /// 
        /// </summary>
        public WorkstepBaseSeq() { }

        // - for direct use
        /// <summary>
        /// 
        /// </summary>
        /// <param name="substeps"></param>
        public WorkstepBaseSeq(IEnumerable<IWorkstep> substeps)
        {
            if (substeps == null)
                throw new ArgumentNullException(nameof(substeps));
            foreach (IWorkstep substep in substeps)
                AddSubstep(substep);
        }

        // executes a sequential list of substeps
        protected override void OnInitialise()
        {
            // sequential step substep inputs and outputs must match
            if (SubSteps.Count == 0)
                throw new ApplicationException("Must have at least one substep!");
            if (SubSteps.First().Step.InputType != typeof(Tin))
                throw new ApplicationException("First substep: input type mismatch");
            if (SubSteps.Last().Step.OutputType != typeof(Tout))
                throw new ApplicationException("Last substep: output type mismatch");
            for (int i = 1; i < SubSteps.Count; i++)
            {
                if (SubSteps[i - 1].Step.OutputType != SubSteps[i].Step.InputType)
                    throw new ApplicationException(
                        "Substep[" + i + "] input type must match " +
                        "substep[" + (i - 1) + "] output type");
            }
        }

        protected override WorkflowOutput<Tout> OnSubSteps(Tin input)
        {
            object tempResult = input;
            foreach (SubStep substep in SubSteps)
            {
                Interlocked.Add(ref WorkToDo, substep.Work);
            }
            foreach (SubStep substep in SubSteps)
            {
                IWorkflowOutput stepOutput = substep.Step.ExecuteObj(tempResult);
                tempResult = stepOutput.GetResult();
                WorkflowError[] tempErrors = stepOutput.GetErrors();
                if ((tempErrors != null) && (tempErrors.Length > 0))
                {
                    return new WorkflowOutput<Tout>
                               {
                        Result = (Tout)stepOutput.GetResult(),
                        Errors = tempErrors
                    };
                }
                Interlocked.Add(ref WorkDone, substep.Work);
            }
            return new WorkflowOutput<Tout>
                       {
                Result = (Tout)tempResult,
                Errors = new WorkflowError[0]
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class WorkstepIfThenElse<Tin, Tout> : WorkstepBaseAlt<Tin, Tout>
    {
        public delegate bool ConditionDelegate(Tin input);
        protected ConditionDelegate ConditionCallback;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public WorkstepIfThenElse() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ifCondition"></param>
        /// <param name="thenStep"></param>
        /// <param name="elseStep"></param>
        public WorkstepIfThenElse(ConditionDelegate ifCondition, IWorkstep thenStep, IWorkstep elseStep)
        {
            ConditionCallback = ifCondition;
            if (thenStep == null)
                throw new ArgumentNullException(nameof(thenStep));
            if (elseStep == null)
                throw new ArgumentNullException(nameof(elseStep));
            AddSubstep(thenStep);
            AddSubstep(elseStep);
        }

        // validate
        protected override void OnInitialise()
        {
            base.OnInitialise();
            if (SubSteps.Count != 2)
                throw new ApplicationException("If-then-else step must have 2 substeps!");
            for (int i = 0; i < SubSteps.Count; i++)
            {
                if (SubSteps[i].Step.InputType != typeof(Tin))
                    throw new ApplicationException("Substep[" + i + "]: input type mismatch");
                if (SubSteps[i].Step.OutputType != typeof(Tout))
                    throw new ApplicationException("Substep[" + i + "]: output type mismatch");
            }
        }

        // chooser override
        protected virtual bool OnCondition(Tin input)
        {
            if (ConditionCallback != null)
                return ConditionCallback(input);
            throw new NotImplementedException();
        }

        protected override int OnAltChoose(Tin input)
        {
            if (OnCondition(input))
                return 0;
            return 1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class WorkstepCompare<Tin, Tout> : WorkstepBaseAlt<Tin, Tout>
    {
        public delegate int CompareDelegate(Tin input);
        protected CompareDelegate CompareCallback;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public WorkstepCompare() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compareCallback"></param>
        /// <param name="negativeStep"></param>
        /// <param name="equalToStep"></param>
        /// <param name="positiveStep"></param>
        public WorkstepCompare(CompareDelegate compareCallback, IWorkstep negativeStep, IWorkstep equalToStep, IWorkstep positiveStep)
        {
            CompareCallback = compareCallback;
            if (negativeStep == null)
                throw new ArgumentNullException(nameof(negativeStep));
            if (equalToStep == null)
                throw new ArgumentNullException(nameof(equalToStep));
            if (positiveStep == null)
                throw new ArgumentNullException(nameof(positiveStep));
            AddSubstep(negativeStep);
            AddSubstep(equalToStep);
            AddSubstep(positiveStep);
        }

        // validate
        protected override void OnInitialise()
        {
            base.OnInitialise();
            if (SubSteps.Count != 3)
                throw new ApplicationException("Compare step must have 3 substeps!");
            for (int i = 0; i < SubSteps.Count; i++)
            {
                if (SubSteps[i].Step.InputType != typeof(Tin))
                    throw new ApplicationException("Substep[" + i + "]: input type mismatch");
                if (SubSteps[i].Step.OutputType != typeof(Tout))
                    throw new ApplicationException("Substep[" + i + "]: output type mismatch");
            }
        }

        // chooser override
        protected virtual int OnCompare(Tin input)
        {
            if (CompareCallback != null)
                return CompareCallback(input);
            throw new NotImplementedException();
        }

        protected override int OnAltChoose(Tin input)
        {
            int compareValue = OnCompare(input);
            if (compareValue == 0)
                return 1; // equal-to case
            if (compareValue < 0)
                return 0; // negative case
            return 2; // positive case
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class WorkstepBaseAlt<Tin, Tout> : WorkstepBase<Tin, Tout>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public delegate int ChooserDelegate(Tin input);

        /// <summary>
        /// 
        /// </summary>
        protected ChooserDelegate ChooserCallback;
        // constructors
        /// <summary>
        /// 
        /// </summary>
        public WorkstepBaseAlt() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chooserCallback"></param>
        /// <param name="substeps"></param>
        public WorkstepBaseAlt(ChooserDelegate chooserCallback, IEnumerable<IWorkstep> substeps)
        {
            ChooserCallback = chooserCallback;
            if (substeps == null)
                throw new ArgumentNullException(nameof(substeps));
            foreach (IWorkstep substep in substeps)
                AddSubstep(substep);
        }

        // executes one substep from a set
        protected override void OnInitialise()
        {
            // all substep inputs and outputs must match
            if (SubSteps.Count == 0)
                throw new ApplicationException("Must have at least one substep!");
            for (int i = 0; i < SubSteps.Count; i++)
            {
                if (SubSteps[i].Step.InputType != typeof(Tin))
                    throw new ApplicationException("Substep[" + i + "]: input type mismatch");
                if (SubSteps[i].Step.OutputType != typeof(Tout))
                    throw new ApplicationException("Substep[" + i + "]: output type mismatch");
            }
        }

        protected virtual int OnAltChoose(Tin input)
        {
            if (ChooserCallback != null)
                return ChooserCallback(input);
            throw new NotImplementedException();
        }

        protected override WorkflowOutput<Tout> OnSubSteps(Tin input)
        {
            int index = OnAltChoose(input);
            if ((index < 0) || (index >= SubSteps.Count))
                throw new ApplicationException($"Invalid substep index: {index}");
            Interlocked.Add(ref WorkToDo, SubSteps[index].Work);
            IWorkflowOutput result = SubSteps[index].Step.ExecuteObj(input);
            Interlocked.Add(ref WorkDone, SubSteps[index].Work);
            return new WorkflowOutput<Tout> { Result = (Tout)result.GetResult(), Errors = result.GetErrors() };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tinp"></typeparam>
    /// <typeparam name="Tloop"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class WorkstepBaseLoop<Tinp, Tloop, Tout> : WorkstepBase<Tinp, Tout>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loopData"></param>
        /// <returns></returns>
        public delegate bool ConditionDelegate(Tloop loopData);

        /// <summary>
        /// 
        /// </summary>
        protected ConditionDelegate ConditionCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public delegate Tloop LoopInitDelegate(Tinp input);

        /// <summary>
        /// 
        /// </summary>
        protected LoopInitDelegate LoopInitCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public delegate Tloop LoopBodyDelegate(Tloop input);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loopData"></param>
        public delegate void LoopNextDelegate(ref Tloop loopData);

        /// <summary>
        /// 
        /// </summary>
        protected LoopNextDelegate LoopNextCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loopData"></param>
        /// <returns></returns>
        public delegate Tout LoopDoneDelegate(Tloop loopData);

        /// <summary>
        /// 
        /// </summary>
        protected LoopDoneDelegate LoopDoneCallback;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public WorkstepBaseLoop() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loopInitMethod"></param>
        /// <param name="whileCondition"></param>
        /// <param name="loopBodySubstep"></param>
        /// <param name="loopNextMethod"></param>
        /// <param name="loopDoneMethod"></param>
        public WorkstepBaseLoop(
            LoopInitDelegate loopInitMethod,
            ConditionDelegate whileCondition,
            IWorkstep loopBodySubstep,
            LoopNextDelegate loopNextMethod,
            LoopDoneDelegate loopDoneMethod)
        {
            LoopInitCallback = loopInitMethod;
            ConditionCallback = whileCondition;
            LoopNextCallback = loopNextMethod;
            LoopDoneCallback = loopDoneMethod;
            if (loopBodySubstep == null)
                throw new ArgumentNullException(nameof(loopBodySubstep));
            AddSubstep(loopBodySubstep);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loopInitMethod"></param>
        /// <param name="whileCondition"></param>
        /// <param name="loopBodyMethod"></param>
        /// <param name="loopNextMethod"></param>
        /// <param name="loopDoneMethod"></param>
        public WorkstepBaseLoop(
            LoopInitDelegate loopInitMethod,
            ConditionDelegate whileCondition,
            WorkstepBase<Tloop, Tloop>.ExecuteDelegate loopBodyMethod,
            LoopNextDelegate loopNextMethod,
            LoopDoneDelegate loopDoneMethod)
            : this(loopInitMethod, whileCondition,
                new WorkstepBase<Tloop, Tloop>(loopBodyMethod, null),
                loopNextMethod, loopDoneMethod)
        { }

        // validation
        protected override void OnInitialise()
        {
            // all substep inputs and outputs must match
            if (SubSteps.Count != 1)
                throw new ApplicationException("Looping step must have exactly one substep!");
            if (SubSteps[0].Step.InputType != typeof(Tloop))
                throw new ApplicationException("Loop substep input type must be '" + typeof(Tloop).Name + "'");
            if (SubSteps[0].Step.OutputType != typeof(Tloop))
                throw new ApplicationException("Loop substep output type must be '" + typeof(Tloop).Name + "'");
        }

        // loop initialisation
        protected virtual Tloop OnLoopInit(Tinp input)
        {
            if (LoopInitCallback != null)
                return LoopInitCallback(input);
            throw new NotImplementedException();
        }

        // loop iteration
        protected virtual void OnLoopNext(ref Tloop loopData)
        {
            if (LoopNextCallback != null)
                LoopNextCallback(ref loopData);
            else
                throw new NotImplementedException();
        }

        // loop completion
        protected virtual Tout OnLoopDone(Tloop loopData)
        {
            if (LoopDoneCallback != null)
                return LoopDoneCallback(loopData);
            throw new NotImplementedException();
        }

        // loop condition evaluator
        protected virtual bool OnCondition(Tloop loopData)
        {
            if (ConditionCallback != null)
                return ConditionCallback(loopData);
            throw new NotImplementedException();
        }

        // execution
        protected override WorkflowOutput<Tout> OnSubSteps(Tinp input)
        {
            var errors = new List<WorkflowError>();
            Tloop loopData = OnLoopInit(input);
            while (OnCondition(loopData))
            {
                Interlocked.Add(ref WorkToDo, SubSteps[0].Work);
                IWorkflowOutput tempResult = SubSteps[0].Step.ExecuteObj(loopData);
                loopData = (Tloop)tempResult.GetResult();
                errors.AddRange(tempResult.GetErrors());
                Interlocked.Add(ref WorkDone, SubSteps[0].Work);
                OnLoopNext(ref loopData);
            }
            Tout result = OnLoopDone(loopData);
            return new WorkflowOutput<Tout> { Result = result, Errors = errors.ToArray() };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tsubin"></typeparam>
    /// <typeparam name="Tsubout"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class WorkstepBasePar<Tin, Tsubin, Tsubout, Tout> : WorkstepBase<Tin, Tout>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public delegate Tsubin[] SplitterDelegate(Tin input);

        protected SplitterDelegate SplitterCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputs"></param>
        /// <returns></returns>
        public delegate Tout CombinerDelegate(Tsubout[] outputs);

        protected CombinerDelegate CombinerCallback;
        protected int DispatchLimit = Environment.ProcessorCount * 2;
        protected int ExceptionLimit;
        protected Exception LastException;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public WorkstepBasePar() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="splitterMethod"></param>
        /// <param name="workerMethod"></param>
        /// <param name="combinerMethod"></param>
        public WorkstepBasePar(
            SplitterDelegate splitterMethod,
            WorkstepBase<Tsubin, Tsubout>.ExecuteDelegate workerMethod,
            CombinerDelegate combinerMethod)
        {
            SplitterCallback = splitterMethod;
            CombinerCallback = combinerMethod;
            AddSubstep(new WorkstepBase<Tsubin, Tsubout>(workerMethod, null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="splitterMethod"></param>
        /// <param name="subSteps"></param>
        /// <param name="combinerMethod"></param>
        public WorkstepBasePar(
            SplitterDelegate splitterMethod,
            IEnumerable<IWorkstep> subSteps,
            CombinerDelegate combinerMethod)
        {
            SplitterCallback = splitterMethod;
            CombinerCallback = combinerMethod;
            if (subSteps == null)
                throw new ArgumentNullException(nameof(subSteps));
            foreach (IWorkstep substep in subSteps)
                AddSubstep(substep);
        }

        // validation
        protected override void OnInitialise()
        {
            // all substep inputs and outputs must match
            if (SubSteps.Count == 0)
                throw new ApplicationException("Parallel step must have at least one substep!");
            foreach (SubStep t in SubSteps)
            {
                if (t.Step.InputType != typeof(Tsubin))
                    throw new ApplicationException("Substep input type must match input type");
                if (t.Step.OutputType != typeof(Tsubout))
                    throw new ApplicationException("Substep output type must match output type");
            }
        }

        protected virtual Tsubin[] OnParSplit(Tin input)
        {
            if (SplitterCallback != null)
                return SplitterCallback(input);
            throw new NotImplementedException();
        }

        protected virtual Tout OnParCombine(Tsubout[] outputs)
        {
            if (CombinerCallback != null)
                return CombinerCallback(outputs);
            throw new NotImplementedException();
        }

        protected override WorkflowOutput<Tout> OnSubSteps(Tin input)
        {
            Tout result = default(Tout);
            var errors = new List<WorkflowError>();
            // call user's split method
            Tsubin[] subInputs;
            try
            {
                subInputs = OnParSplit(input);
            }
            catch (Exception excpSplit)
            {
                Context.Logger.Log(excpSplit);
                errors.Add(new WorkflowError(excpSplit));
                return new WorkflowOutput<Tout>
                           {
                    Result = result,
                    Errors = errors.ToArray()
                };
            }
            var subOutputs = new Tsubout[subInputs.Length];
            var asyncSubResults = new IAsyncResult[subInputs.Length];
            int excpCount = 0;
            if (SubSteps.Count != 1 && (SubSteps.Count != subInputs.Length))
                throw new ApplicationException("Number of substeps must be 1, or equal to number of inputs");
            // start all async substeps
            int inputIndex = 0;
            int dispatched = 0;
            int asyncIndex = 0;
            Interlocked.Add(ref WorkToDo, subInputs.Length);
            while (inputIndex < subInputs.Length && excpCount <= ExceptionLimit)
            {
                while (inputIndex < subInputs.Length && dispatched < DispatchLimit)
                {
                    if (SubSteps.Count == 1)
                    {
                        // execute many inputs in parallel with 1 common step
                        asyncSubResults[inputIndex] = SubSteps[0].Step.BeginExecuteObj(subInputs[inputIndex]);
                    }
                    else
                    {
                        // execute many inputs in parallel with individual steps
                        asyncSubResults[inputIndex] = SubSteps[inputIndex].Step.BeginExecuteObj(subInputs[inputIndex]);
                    }
                    // next
                    dispatched++;
                    inputIndex++;
                }
                // dispatch limit - wait for results
                while (dispatched > 0)
                {
                    var ar = (AsyncResult<WorkflowOutput<Tsubout>>)asyncSubResults[asyncIndex];
                    try
                    {
                        WorkflowOutput<Tsubout> subResult = ar.EndInvoke();
                        subOutputs[asyncIndex] = subResult.Result;
                        errors.AddRange(subResult.Errors);
                    }
                    catch (Exception e)
                    {
                        excpCount++;
                        LastException = e;
                        Context.Logger.Log(e);
                        errors.Add(new WorkflowError(e));
                    }
                    Interlocked.Increment(ref WorkDone);
                    // next
                    dispatched--;
                    asyncIndex++;
                }
            }
            //if (excpCount > 0)
            //    throw new ApplicationException("One or more substeps failed", _LastException);
            // call user's combiner method
            try
            {
                result = OnParCombine(subOutputs);
            }
            catch (Exception excpCombine)
            {
                Context.Logger.Log(excpCombine);
                errors.Add(new WorkflowError(excpCombine));
            }
            return new WorkflowOutput<Tout> { Result = result, Errors = errors.ToArray() };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WorkflowBase : IWorkflow
    {
        /// <summary>
        /// 
        /// </summary>
        public IWorkstep FirstStep { get; set; }
    }

    // ======================================== grid extensions ========================================
    //public interface IWorkgridConfig
    //{
    //    //Guid NodeGuid { get; }
    //    bool IsClient { get; }
    //    bool IsWorker { get; }
    //    bool IsRouter { get; }
    //    string RouterHost { get; }
    //    int RouterPort { get; }
    //}

    //public class WorkgridConfigRouter : IWorkgridConfig
    //{
    //    private readonly Guid _NodeGuid = Guid.NewGuid();
    //    private readonly int _RouterPort;
    //    public WorkgridConfigRouter(int routerPort)
    //    {
    //        if (routerPort <= 0)
    //            throw new ArgumentNullException("routerPort");
    //        _RouterPort = routerPort;
    //    }
    //    public Guid NodeGuid { get { return _NodeGuid; } }
    //    public bool IsClient { get { return false; } }
    //    public bool IsWorker { get { return false; } }
    //    public bool IsRouter { get { return true; } }
    //    public string RouterHost { get { return Dns.GetHostName(); } }
    //    public int RouterPort { get { return _RouterPort; } }
    //}

    //public class WorkgridConfigWorker : IWorkgridConfig
    //{
    //    private readonly Guid _NodeGuid = Guid.NewGuid();
    //    private readonly string _RouterHost;
    //    private readonly int _RouterPort;
    //    public WorkgridConfigWorker(int routerPort, string routerHost)
    //    {
    //        if (routerPort <= 0)
    //            throw new ArgumentNullException("routerPort");
    //        _RouterHost = routerHost ?? "localhost";
    //        _RouterPort = routerPort;
    //    }
    //    public Guid NodeGuid { get { return _NodeGuid; } }
    //    public bool IsClient { get { return true; } }
    //    public bool IsWorker { get { return true; } }
    //    public bool IsRouter { get { return false; } }
    //    public string RouterHost { get { return _RouterHost; } }
    //    public int RouterPort { get { return _RouterPort; } }
    //}

    //public class WorkgridConfigClient : IWorkgridConfig
    //{
    //    private readonly Guid _NodeGuid = Guid.NewGuid();
    //    private readonly string _RouterHost;
    //    private readonly int _RouterPort;
    //    public WorkgridConfigClient(int routerPort, string routerHost)
    //    {
    //        if (routerPort <= 0)
    //            throw new ArgumentNullException("routerPort");
    //        _RouterHost = routerHost ?? "localhost";
    //        _RouterPort = routerPort;
    //    }
    //    public Guid NodeGuid { get { return _NodeGuid; } }
    //    public bool IsClient { get { return true; } }
    //    public bool IsWorker { get { return false; } }
    //    public bool IsRouter { get { return false; } }
    //    public string RouterHost { get { return _RouterHost; } }
    //    public int RouterPort { get { return _RouterPort; } }
    //}

    //public class WorkgridConfigDisabled : IWorkgridConfig
    //{
    //    private readonly Guid _NodeGuid = Guid.NewGuid();
    //    public WorkgridConfigDisabled() { }
    //    public Guid NodeGuid { get { return _NodeGuid; } }
    //    public bool IsClient { get { return false; } }
    //    public bool IsWorker { get { return false; } }
    //    public bool IsRouter { get { return false; } }
    //    public string RouterHost { get { return null; } }
    //    public int RouterPort { get { return 0; } }
    //}

    [DataContract]
    public class WorkgridOutput<TOut>
    {
        [DataMember]
        public TOut Result;

        [DataMember]
        public WorkflowError[] FlowErrors;

        [DataMember]
        public WorkflowError[] GridErrors;
    }

    [ServiceContract]
    public interface IWorkgridSwitchV101<in Tin, Tout>
    {
        // implemented by worker (called by router)
        [OperationContract]
        WorkflowOutput<Tout> ExecuteLocalCallV101(Tin input);

        // implemented by router (called by worker)
        [OperationContract]
        void EnlistWorkerV101(Guid nodeId, int initFreeWorkers, string workerAddress);

        [OperationContract]
        void DelistWorkerV101(Guid nodeId);
        // implemented by router (called by client)
        [OperationContract]
        WorkgridOutput<Tout> DispatchToWorkerV101(Guid sourceNode, Tin input);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class WorkgridSwitchSenderV101<Tin, TOut> : CustomClientBase<IWorkgridSwitchV101<Tin, TOut>>, IWorkgridSwitchV101<Tin, TOut>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addressBinding"></param>
        public WorkgridSwitchSenderV101(AddressBinding addressBinding)
            : base(addressBinding)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="initFreeWorkers"></param>
        /// <param name="workerAddress"></param>
        public void EnlistWorkerV101(Guid nodeId, int initFreeWorkers, string workerAddress)
        {
            Channel.EnlistWorkerV101(nodeId, initFreeWorkers, workerAddress);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workerNode"></param>
        public void DelistWorkerV101(Guid workerNode)
        {
            Channel.DelistWorkerV101(workerNode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceNode"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public WorkgridOutput<TOut> DispatchToWorkerV101(Guid sourceNode, Tin input)
        {
            return Channel.DispatchToWorkerV101(sourceNode, input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public WorkflowOutput<TOut> ExecuteLocalCallV101(Tin input)
        {
            return Channel.ExecuteLocalCallV101(input);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class WorkgridSwitchRecverV101<Tin, Tout> : IWorkgridSwitchV101<Tin, Tout>
    {
        private readonly IWorkgridSwitchV101<Tin, Tout> _channel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public WorkgridSwitchRecverV101(IWorkgridSwitchV101<Tin, Tout> channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="initFreeWorkers"></param>
        /// <param name="workerAddress"></param>
        public void EnlistWorkerV101(Guid nodeId, int initFreeWorkers, string workerAddress)
        {
            _channel.EnlistWorkerV101(nodeId, initFreeWorkers, workerAddress);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workerNode"></param>
        public void DelistWorkerV101(Guid workerNode)
        {
            _channel.DelistWorkerV101(workerNode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceNode"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public WorkgridOutput<Tout> DispatchToWorkerV101(Guid sourceNode, Tin input)
        {
            return _channel.DispatchToWorkerV101(sourceNode, input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public WorkflowOutput<Tout> ExecuteLocalCallV101(Tin input)
        {
            return _channel.ExecuteLocalCallV101(input);
        }
    }

    internal class GridWorkerState<Tin, Tout> : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly Guid NodeGuid;

        /// <summary>
        /// 
        /// </summary>
        public readonly string Address;

        /// <summary>
        /// 
        /// </summary>
        public WorkgridSwitchSenderV101<Tin, Tout> ClientBase;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeGuid"></param>
        /// <param name="address"></param>
        /// <param name="clientBase"></param>
        public GridWorkerState(Guid nodeGuid, string address, WorkgridSwitchSenderV101<Tin, Tout> clientBase)
        {
            NodeGuid = nodeGuid;
            Address = address;
            ClientBase = clientBase;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            DisposeHelper.SafeDispose(ref ClientBase);
        }
    }

    internal class GridWorkerPool<Tin, Tout> : IDisposable
    {
        //public int LastWorkerIndex = 0;
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<Guid, GridWorkerState<Tin, Tout>> WorkerNodeMap = new Dictionary<Guid, GridWorkerState<Tin, Tout>>();

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            foreach (GridWorkerState<Tin, Tout> item in WorkerNodeMap.Values)
            {
                item.Dispose();
            }
        }
    }

    internal class NodeCounter
    {
        // readonly state
        public Guid NodeId { get; }

        // managed state
        private long _freeWorkers;

        /// <summary>
        /// 
        /// </summary>
        public long FreeWorkers => Interlocked.Add(ref _freeWorkers, 0);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long IncrementFreeWorkers() { return Interlocked.Increment(ref _freeWorkers); }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long DecrementFreeWorkers() { return Interlocked.Decrement(ref _freeWorkers); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="initFreeWorkers"></param>
        // constructor
        public NodeCounter(Guid nodeId, int initFreeWorkers)
        {
            NodeId = nodeId;
            _freeWorkers = initFreeWorkers;
        }
    }

    internal static class NodeCounterMap
    {
        private static readonly Guarded<Dictionary<Guid, NodeCounter>> Counters =
            new Guarded<Dictionary<Guid, NodeCounter>>(new Dictionary<Guid, NodeCounter>());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="initFreeWorkers"></param>
        /// <returns></returns>
        public static NodeCounter GetCounter(Guid nodeId, int initFreeWorkers)
        {
            NodeCounter counter = null;
            Counters.Locked(counters =>
            {
                if (!counters.TryGetValue(nodeId, out counter))
                {
                    counter = new NodeCounter(nodeId, initFreeWorkers);
                    counters.Add(nodeId, counter);
                }
            });
            return counter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeIds"></param>
        /// <returns></returns>
        public static NodeCounter FindBestNode(IEnumerable<Guid> nodeIds)
        {
            long maxValue = long.MinValue;
            NodeCounter counter = null;
            Counters.Locked(counters =>
            {
                foreach (Guid nodeId in nodeIds)
                {
                    if (counters.TryGetValue(nodeId, out var candidate))
                    {
                        long freeCores = candidate.FreeWorkers;
                        if (freeCores > maxValue)
                        {
                            maxValue = freeCores;
                            counter = candidate;
                        }
                    }
                }
            });
            return counter;
        }
    }
}

