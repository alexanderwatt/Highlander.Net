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
using System.Threading;
using Orion.Util.Logging;

namespace Orion.Util.Threading
{
    public class AsyncResultNoResult : IAsyncResult
    {
        // Fields set at construction which never change while operation is pending
        private readonly AsyncCallback _mAsyncCallback;

        // Field set at construction which do change after operation completes
        private const Int32 CStatePending = 0;
        private const Int32 CStateCompletedSynchronously = 1;
        private const Int32 CStateCompletedAsynchronously = 2;
        private Int32 _mCompletedState;

        // Field that may or may not get set depending on usage
        private ManualResetEvent _mAsyncWaitHandle;

        // Fields set when operation completes
        private Exception _mException;

        public AsyncResultNoResult(AsyncCallback asyncCallback, Object state)
        {
            _mAsyncCallback = asyncCallback;
            AsyncState = state;
        }

        public void SetAsCompleted(Exception exception, Boolean completedSynchronously)
        {
            // Passing null for exception means no error occurred; this is the common case
            _mException = exception;
            //if (exception != null)
            //{
            //    Trace.WriteLine(String.Format("AsyncResultNoResult.SetAsCompleted: Exception: {0}", exception.GetType().Name));
            //}

            // The m_CompletedState field MUST be set prior calling the callback
            Int32 prevState = Interlocked.Exchange(ref _mCompletedState,
               completedSynchronously ? CStateCompletedSynchronously : CStateCompletedAsynchronously);
            if (prevState != CStatePending)
                throw new InvalidOperationException("You can set a result only once");

            // If the event exists, set it
            _mAsyncWaitHandle?.Set();

            // If a callback method was set, call it
            _mAsyncCallback?.Invoke(this);
        }

        public void EndInvoke()
        {
            // This method assumes that only 1 thread calls EndInvoke for this object
            if (!IsCompleted)
            {
                // If the operation isn't done, wait for it
                AsyncWaitHandle.WaitOne();
                AsyncWaitHandle.Close();
                _mAsyncWaitHandle = null;  // Allow early GC
            }

            // Operation is done: if an exception occured, throw it
            if (_mException != null)
                throw _mException;
        }

        #region Implementation of IAsyncResult
        public Object AsyncState { get; }

        public Boolean CompletedSynchronously => _mCompletedState == CStateCompletedSynchronously;

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_mAsyncWaitHandle == null)
                {
                    Boolean done = IsCompleted;
                    ManualResetEvent mre = new ManualResetEvent(done);
                    if (Interlocked.CompareExchange(ref _mAsyncWaitHandle, mre, null) != null)
                    {
                        // Another thread created this object's event; dispose the event we just created
                        mre.Close();
                    }
                    else
                    {
                        if (!done && IsCompleted)
                        {
                            // If the operation wasn't done when we created 
                            // the event but now it is done, set the event
                            _mAsyncWaitHandle.Set();
                        }
                    }
                }
                return _mAsyncWaitHandle;
            }
        }

        public Boolean IsCompleted => _mCompletedState != CStatePending;

        #endregion
    }

    public class AsyncResult<TResult> : AsyncResultNoResult
    {
        // Field set when operation completes
        private TResult _mResult;

        public AsyncResult(AsyncCallback asyncCallback, Object state) : base(asyncCallback, state) { }

        public void SetAsCompleted(TResult result, Boolean completedSynchronously)
        {
            // Save the asynchronous operation's result
            _mResult = result;
            // Tell the base class that the operation completed sucessfully (no exception)
            base.SetAsCompleted(null, completedSynchronously);
        }

        public new TResult EndInvoke()
        {
            base.EndInvoke(); // Wait until operation has completed 
            return _mResult;  // Return the result (if above didn't throw)
        }
    }

    public enum AsyncClassState
    {
        Initial,
        Running,
        Stopped,
        Faulted
    }
    public delegate void AsyncErrorDelegate<TContext>(Exception excp, TContext context);
    public delegate void AsyncStateDelegate<TContext>(AsyncClassState oldState, AsyncClassState newState, string reason, TContext context);
    public delegate void AsyncEventDelegate<TData, TContext>(TData data, TContext context);
    /// <summary>
    /// Defines methods for managing asynchronously connected classes
    /// </summary>
    public interface IAsyncClass<TData, TContext>
    {
        // control and monitoring
        // - general state management
        // - synchronous (simple stop and wait) methods
        void Start(TimeSpan waitTimeout);
        void Stop(TimeSpan waitTimeout, string reason);
    }
    public interface IAsyncClassEx<TData, TContext> : IAsyncClass<TData, TContext>
    {
        // - asynchronous control methods
        void AsyncStart();
        void AsyncStop(string reason);
        // - asynchronous delegate methods
        AsyncErrorDelegate<TContext> AsyncErrorHandlers { get; set; }
        AsyncStateDelegate<TContext> AsyncStateHandlers { get; set; }
        AsyncEventDelegate<TData, TContext> AsyncEventHandlers { get; set; }
    }
    public class AsyncClass<TData, TContext> : IAsyncClassEx<TData, TContext>
    {
        protected readonly ILogger Logger;
        private readonly TContext _context;
        public object Context => _context;
        private readonly DispatcherHandle _mainDispatcher;
        private readonly DispatcherHandle _userDispatcher;
        private volatile AsyncClassState _state = AsyncClassState.Initial;
        public AsyncClassState State => _state;
        public bool IsRunning => (_state == AsyncClassState.Running);
        public bool HasStarted => (_state >= AsyncClassState.Running);
        public bool HasStopped => (_state >= AsyncClassState.Stopped);
        public bool HasFaulted => (_state == AsyncClassState.Faulted);

        // asynchronous extensions
        public AsyncClass(ILogger logger, TContext context)
        {
            // default constructor
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context;
            _mainDispatcher = new Dispatcher(logger, GetType().Name + "-Main").GetDispatcherHandle(".");
            _userDispatcher = new Dispatcher(logger, GetType().Name + "-User").GetDispatcherHandle(".");
        }
        public AsyncClass(ILogger logger, TContext context, DispatcherHandle mainDispatcher, DispatcherHandle userDispatcher)
        {
            // constructor used when coordinating sequencing with container
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context;
            _mainDispatcher = mainDispatcher ?? throw new ArgumentNullException(nameof(mainDispatcher));
            _userDispatcher = userDispatcher ?? new Dispatcher(logger, GetType().Name + "-User").GetDispatcherHandle(".");
        }
        public AsyncErrorDelegate<TContext> ErrorHandlers { get; set; }

        public AsyncStateDelegate<TContext> StateHandlers { get; set; }

        public AsyncEventDelegate<TData, TContext> EventHandlers { get; set; }

        public AsyncErrorDelegate<TContext> AsyncErrorHandlers { get; set; }

        public AsyncStateDelegate<TContext> AsyncStateHandlers { get; set; }

        public AsyncEventDelegate<TData, TContext> AsyncEventHandlers { get; set; }

        protected virtual void OnAsyncException(Exception excp)
        {
            Logger.Log(excp);
        }
        protected virtual void OnAsyncStateChanged(AsyncClassState oldState, AsyncClassState newState, string reason) { }
        protected virtual void OnAsyncStateInitial(AsyncClassState oldState, string reason) { }
        protected virtual void OnAsyncStateRunning(AsyncClassState oldState, string reason) { }
        protected virtual void OnAsyncStateStopped(AsyncClassState oldState, string reason) { }
        protected virtual void OnAsyncEvent(TData data) { }
        protected virtual void OnAsyncSendOther(object obj) { }

        private class AsyncErrorParams
        {
            public readonly Exception Excp;
            public readonly TContext Context;
            public AsyncErrorParams(Exception excp, TContext context)
            {
                Excp = excp;
                Context = context;
            }
        }
        private void AsyncErrorCallback(AsyncErrorParams args)
        {
            AsyncErrorDelegate<TContext> handlers = AsyncErrorHandlers;
            if (handlers != null)
            {
                try
                {
                    handlers(args.Excp, args.Context);
                }
                catch (Exception excp)
                {
                    // ignore client exceptions
                    Logger.Log(excp);
                }
            }
        }
        private void HandleException(Exception excp)
        {
            // ignore secondary exceptions
            if (_state == AsyncClassState.Faulted)
            {
                Logger.LogDebug("HandleException: Ignoring due to Faulted state: {0}", excp);
                return;
            }

            // notify asynchronous delegates
            if (AsyncErrorHandlers != null)
            {
                _userDispatcher.DispatchCall(new AsyncErrorParams(excp, _context), AsyncErrorCallback);
            }

            // handle the exception
            try
            {
                _state = AsyncClassState.Faulted;
                OnAsyncException(excp);
                ErrorHandlers?.Invoke(excp, _context);
            }
            catch
            {
                // we tried our best - ignore
                Logger.Log(excp);
            }
        }

        private class AsyncStateParams
        {
            public readonly AsyncClassState OldState;
            public readonly AsyncClassState NewState;
            public readonly string Reason;
            public readonly TContext Context;
            public AsyncStateParams(AsyncClassState oldState, AsyncClassState newState, string reason, TContext context)
            {
                OldState = oldState;
                NewState = newState;
                Context = context;
                Reason = reason;
            }
        }
        private void AsyncStateCallback(AsyncStateParams args)
        {
            AsyncStateDelegate<TContext> handlers = AsyncStateHandlers;
            if (handlers != null)
            {
                try
                {
                    handlers(args.OldState, args.NewState, args.Reason, args.Context);
                }
                catch (Exception excp)
                {
                    // ignore client exceptions
                    Logger.Log(excp);
                }
            }
        }
        private void NotifyStateHandlers(AsyncClassState oldState, AsyncClassState newState, string reason)
        {
            // notify asynchronous delegates
            if (AsyncStateHandlers != null)
            {
                _userDispatcher.DispatchCall(new AsyncStateParams(oldState, newState, reason, _context), AsyncStateCallback);
            }

            try
            {
                // call generic state change handler
                OnAsyncStateChanged(oldState, newState, reason);
                // call specific state change handlers
                switch (newState)
                {
                    case AsyncClassState.Initial:
                        OnAsyncStateInitial(oldState, reason);
                        break;
                    case AsyncClassState.Running:
                        OnAsyncStateRunning(oldState, reason);
                        break;
                    case AsyncClassState.Stopped:
                        OnAsyncStateStopped(oldState, reason);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown state: " + newState);
                }
                StateHandlers?.Invoke(oldState, newState, reason, _context);
            }
            catch (Exception excp)
            {
                HandleException(excp);
            }
        }

        // public start/stop methods
        public void Start(TimeSpan waitTimeout)
        {
            if (_state == AsyncClassState.Faulted)
            {
                Logger.LogDebug("Start: Ignoring due to Faulted state!");
                return;
            }
            if (_state != AsyncClassState.Initial)
                throw new InvalidOperationException("Already started!");
            AsyncStart();
            WaitHelper.WaitFor(waitTimeout, () => (_state == AsyncClassState.Running), "Failed to start!");
        }
        public void Stop(TimeSpan waitTimeout, string reason)
        {
            if (_state == AsyncClassState.Faulted)
            {
                Logger.LogDebug("Stop: Ignoring due to Faulted state!");
                return;
            }
            if (_state != AsyncClassState.Running)
            {
                Logger.LogDebug("Stop: Ignoring due to state is not Running!");
                return;
            }
            AsyncStop(reason);
            WaitHelper.WaitFor(waitTimeout, () => (_state == AsyncClassState.Stopped), "Failed to stop!");
        }

        public void AsyncStart()
        {
            _mainDispatcher.DispatchCall<object>(null, StartCallback);
        }
        private void StartCallback(object notUsed)
        {
            if (_state == AsyncClassState.Faulted)
            {
                Logger.LogDebug("StartCallback: Ignoring due to Faulted state!");
                return;
            }
            try
            {
                if (_state != AsyncClassState.Initial)
                    throw new InvalidOperationException("Already started!");
                AsyncClassState oldState = _state;
                _state = AsyncClassState.Running;
                if (_state != oldState)
                {
                    NotifyStateHandlers(oldState, _state, "");
                }
            }
            catch (Exception excp)
            {
                HandleException(excp);
            }
        }

        public void AsyncStop(string reason)
        {
            _mainDispatcher.DispatchCall(reason, StopCallback);
        }

        private void StopCallback(string reason)
        {
            if (_state == AsyncClassState.Faulted)
            {
                Logger.LogDebug("StopCallback: Ignoring due to Faulted state!");
                return;
            }
            if (_state == AsyncClassState.Stopped)
                return;
            try
            {
                if (_state != AsyncClassState.Running)
                {
                    Logger.LogDebug("StopCallback: Ignoring due to state is not Running!");
                    return;
                }
                AsyncClassState oldState = _state;
                _state = AsyncClassState.Stopped;
                if (_state != oldState)
                {
                    NotifyStateHandlers(oldState, _state, "");
                }
            }
            catch (Exception excp)
            {
                HandleException(excp);
            }
        }

        public void AsyncReset()
        {
            _mainDispatcher.DispatchCall<object>(null, ResetCallback);
        }
        private void ResetCallback(object notUsed)
        {
            if (_state == AsyncClassState.Faulted)
            {
                Logger.LogDebug("ResetCallback: Ignoring due to Faulted state!");
                return;
            }
            try
            {
                if (_state != AsyncClassState.Stopped)
                    throw new InvalidOperationException("Not stopped!");
                AsyncClassState oldState = _state;
                _state = AsyncClassState.Initial;
                if (_state != oldState)
                {
                    NotifyStateHandlers(oldState, _state, "");
                }
            }
            catch (Exception excp)
            {
                HandleException(excp);
            }
        }

        private class AsyncEventParams
        {
            public readonly TData Data;
            public readonly TContext Context;
            public AsyncEventParams(TData data, TContext context)
            {
                Data = data;
                Context = context;
            }
        }
        private void AsyncEventCallback(AsyncEventParams args)
        {
            AsyncEventDelegate<TData, TContext> handlers = AsyncEventHandlers;
            if (handlers != null)
            {
                try
                {
                    handlers(args.Data, args.Context);
                }
                catch (Exception excp)
                {
                    // ignore client exceptions
                    Logger.Log(excp);
                }
            }
        }
        private void NotifyEventHandlers(TData data)
        {
            // notify asynchronous delegates
            if (AsyncEventHandlers != null)
            {
                _userDispatcher.DispatchCall(new AsyncEventParams(data, _context), AsyncEventCallback);
            }

            try
            {
                // synchronous handlers
                OnAsyncEvent(data);
                EventHandlers?.Invoke(data, _context);
            }
            catch (Exception excp)
            {
                HandleException(excp);
            }
        }

        public void AsyncPostEvent(TData data)
        {
            // outbound/downstream data
            _mainDispatcher.DispatchCall(data, SendEventCallback);
        }
        private void SendEventCallback(TData data)
        {
            if (_state == AsyncClassState.Faulted)
            {
                return;
            }
            try
            {
                if (_state != AsyncClassState.Running)
                {
                    Logger.LogDebug("SendEventCallback: Ignoring due to state is not Running!");
                    return;
                }
                NotifyEventHandlers(data);
            }
            catch (Exception excp)
            {
                HandleException(excp);
            }
        }
    }

    public delegate bool WaitConditionDelegate();
    public delegate void WaitLoggingDelegate();
    public class WaitHelper
    {
        // move to datetimeprovider
        public static bool WaitFor(
            TimeSpan waitTimeout,
            WaitConditionDelegate exitCondition,
            TimeSpan minSleep, TimeSpan maxSleep, int sleepMult, TimeSpan sleepInc,
            string timeoutExceptionMessage,
            TimeSpan logInterval, WaitLoggingDelegate loggingCallback)
        {
            TimeSpan sleepTimeout = minSleep;
            DateTimeOffset dtNow = DateTimeOffset.Now;
            DateTimeOffset waitExpires = dtNow + waitTimeout;
            DateTimeOffset timeToLog = dtNow + logInterval;
            while (dtNow < waitExpires)
            {
                if (exitCondition())
                    return true;
                if ((loggingCallback != null) && (dtNow > timeToLog))
                {
                    // time to call logging callback
                    loggingCallback();
                    timeToLog = dtNow + logInterval;
                }
                // sanitise timeout
                if (sleepTimeout > maxSleep)
                    sleepTimeout = maxSleep;
                if (sleepTimeout < minSleep)
                    sleepTimeout = minSleep;
                Thread.Sleep(sleepTimeout);
                // calc next sleep timeout
                sleepTimeout = TimeSpan.FromSeconds(sleepTimeout.TotalSeconds * sleepMult).Add(sleepInc);
                dtNow = DateTimeOffset.Now;
            }
            if (timeoutExceptionMessage != null)
                throw new TimeoutException(timeoutExceptionMessage);
            return false;
        }
        public static bool WaitFor(TimeSpan waitTimeout, WaitConditionDelegate exitCondition)
        {
            return WaitFor(waitTimeout, exitCondition, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(100), 2, TimeSpan.Zero, null, TimeSpan.Zero, null);
        }
        public static bool WaitFor(TimeSpan waitTimeout, WaitConditionDelegate exitCondition, string timeoutExceptionMessage)
        {
            return WaitFor(waitTimeout, exitCondition, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(100), 2, TimeSpan.Zero, timeoutExceptionMessage, TimeSpan.Zero, null);
        }
        public static bool WaitFor(TimeSpan waitTimeout, WaitConditionDelegate exitCondition, string timeoutExceptionMessage, TimeSpan logInterval, WaitLoggingDelegate logCallback)
        {
            return WaitFor(waitTimeout, exitCondition, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(100), 2, TimeSpan.Zero, timeoutExceptionMessage, logInterval, logCallback);
        }
    }
}
