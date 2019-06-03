/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Configuration;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;
using Orion.Util.Threading;

#endregion

namespace Orion.Util.Servers
{
    /// <summary>
    /// 
    /// </summary>
    public enum BasicServerState
    {
        Initial,
        Starting,
        Running,
        Stopping,
        Stopped,
        Faulted
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IBasicServer : IDisposable
    {
        Reference<ILogger> LoggerRef { get; set; }
        ILogger Logger { get; }
        NamedValueSet OtherSettings { get; set; }
        void Start();
        void Stop();
        BasicServerState GetState();
        bool IsRunning { get; }
        bool HasStarted { get; }
        bool HasStopped { get; }
        bool HasFaulted { get; }
        bool FirstCallDone { get; }
        bool CloseCallDone { get; }
        bool FinalCallDone { get; }
        long QueueLength { get; }
        bool IsIdle { get; }
    }

    internal class InternalState
    {
        public bool FirstCallDone;
        public bool CloseCallDone;
        public bool FinalCallDone;
        public BasicServerState State = BasicServerState.Initial;
    }

    /// <summary>
    /// A basic server container providing:
    /// - logging support
    /// - start/stop controls
    /// - multi-threaded event dispatcher
    /// - first/close/final event notifications
    /// todo:
    /// - pause/continue controls/notifications
    /// </summary>
    public class BasicServer : IBasicServer
    {
        protected AsyncThreadQueue MainThreadQueue;
        private readonly Guarded<InternalState> _serverState = new Guarded<InternalState>(new InternalState());

        private Reference<ILogger> _loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
        public ILogger Logger => _loggerRef.Target;

        /// <summary>
        /// 
        /// </summary>
        public Reference<ILogger> LoggerRef
        {
            get => _loggerRef.Clone();
            set
            {
                CheckNotStarted();
                if (value == null)
                    Reference<ILogger>.Create(new TraceLogger(true));
                else
                    _loggerRef = value.Clone();
            }
        }

        private NamedValueSet _serverSettings = new NamedValueSet();

        /// <summary>
        /// 
        /// </summary>
        public NamedValueSet OtherSettings
        {
            get => _serverSettings;
            set
            {
                CheckNotStarted();
                _serverSettings = value ?? new NamedValueSet();
            }
        }

        protected void CheckNotStarted()
        {
            if (HasStarted)
                throw new InvalidOperationException("Already started!");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch (Exception e)
            {
                _loggerRef.Target.Log(e);
            }
            _serverState.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public BasicServerState GetState()
        {
            var result = BasicServerState.Initial;
            _serverState.Locked(serverState =>
            {
                result = serverState.State;
            });
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRunning => GetState() == BasicServerState.Running;

        /// <summary>
        /// 
        /// </summary>
        public bool HasStarted => GetState() >= BasicServerState.Running;

        /// <summary>
        /// 
        /// </summary>
        public bool HasStopped => GetState() >= BasicServerState.Stopped;

        /// <summary>
        /// 
        /// </summary>
        public bool HasFaulted => GetState() == BasicServerState.Faulted;

        /// <summary>
        /// 
        /// </summary>
        public long QueueLength
        {
            get
            {
                var queue = MainThreadQueue;
                return queue?.Length ?? 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIdle
        {
            get
            {
                var queue = MainThreadQueue;
                return (queue == null) || queue.IsIdle;
            }
        }

        private BasicServerState SetState(BasicServerState proposedState)
        {
            BasicServerState newState = proposedState;
            _serverState.Locked(serverState =>
            {
                BasicServerState oldState = serverState.State;

                // if the state is faulted, and we are stopping then just return, else fail
                if (oldState == BasicServerState.Faulted)
                {
                    if (newState == BasicServerState.Stopping || newState == BasicServerState.Stopped)
                    {
                        newState = oldState;
                        return;
                    }
                    throw new InvalidOperationException("Server faulted!");
                }

                //proceed
                switch (newState)
                {
                    case BasicServerState.Starting:
                        // allow for restart
                        if (oldState == BasicServerState.Stopped)
                            oldState = BasicServerState.Initial;
                        if (oldState >= BasicServerState.Starting)
                            throw new InvalidOperationException("Already started!");
                        break;
                    case BasicServerState.Running:
                        // only allow starting -> running
                        if (oldState != BasicServerState.Starting)
                            throw new InvalidOperationException("Not starting!");
                        break;
                    case BasicServerState.Stopping:
                        // only set stopping state if running
                        newState = oldState == BasicServerState.Running ? BasicServerState.Stopping : oldState;
                        break;
                    case BasicServerState.Stopped:
                        // only allow stopping -> stopped
                        if (oldState != BasicServerState.Stopping)
                            throw new InvalidOperationException("Not stopping!");
                        break;
                    case BasicServerState.Faulted:
                        break;
                    default:
                        throw new NotSupportedException(
                            $"Unsupported state transition: {oldState} -> {newState}");
                }
                serverState.State = newState;
            }); // end locked
            return newState;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool FirstCallDone
        {
            get
            {
                bool result = false;
                _serverState.Locked(serverState =>
                {
                    result = serverState.FirstCallDone;
                });
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CloseCallDone
        {
            get
            {
                bool result = false;
                _serverState.Locked(serverState =>
                {
                    result = serverState.CloseCallDone;
                });
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool FinalCallDone
        {
            get
            {
                bool result = false;
                _serverState.Locked(serverState =>
                {
                    result = serverState.FinalCallDone;
                });
                return result;
            }
        }

        protected virtual void OnBasicSyncStart() { }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            MainThreadQueue = new AsyncThreadQueue(_loggerRef.Target);
            // determine settings
            // 1 - set directly
            // 2 - via app config
            NamedValueSet directSettings = _serverSettings;
            NamedValueSet appCfgSettings;
            try
            {
                string otherSettingsText = ConfigurationManager.AppSettings["OtherSettings"];
                appCfgSettings = new NamedValueSet(otherSettingsText);
            }
            catch (FormatException fe)
            {
                throw new ConfigurationErrorsException("Configuration value 'OtherSettings': ", fe);
            }
            _serverSettings = new NamedValueSet(appCfgSettings, directSettings);
            SetState(BasicServerState.Starting);
            try
            {
                OnBasicSyncStart();
            }
            catch (Exception e)
            {
                _loggerRef.Target.Log(e);
                SetState(BasicServerState.Faulted);
                throw;
            }
            // dispatch and wait for "start" event to be processed
            MainThreadQueue.Dispatch<object>(null, PostStartup);
            DateTimeOffset waitUntil = DateTimeOffset.Now + TimeSpan.FromSeconds(300); //  5 minutes
            long outstandingCallbacks = MainThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(0));
            while (!FirstCallDone && DateTimeOffset.Now < waitUntil)
            {
                outstandingCallbacks = MainThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(5));
                if (outstandingCallbacks > 0)
                    _loggerRef.Target.LogDebug("Start: Waiting for first callback ({0} callbacks remaining)", outstandingCallbacks);
            }
            if (!FirstCallDone)
                throw new InvalidOperationException(
                    $"Start: Wait for first callback expired ({outstandingCallbacks} callbacks remaining)!");           
            // server is now running
            SetState(BasicServerState.Running);
            _loggerRef.Target.LogInfo("Started.");
        }

        protected virtual void OnFirstCallback() { }
        private void PostStartup(object notUsed)
        {
            try
            {
                OnFirstCallback();
            }
            catch (Exception e)
            {
                _loggerRef.Target.Log(e);
            }
            _serverState.Locked(serverState => serverState.FirstCallDone = true);
        }

        protected virtual void OnBasicSyncStop() { }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            BasicServerState initialState = GetState();
            BasicServerState currentState = SetState(BasicServerState.Stopping);
            if (currentState == initialState ||(currentState != BasicServerState.Stopping))
                return;
            // dispatch and wait for "close" event to be processed
            MainThreadQueue.Dispatch<object>(null, PreCleanup);
            DateTimeOffset waitUntil = DateTimeOffset.Now + TimeSpan.FromSeconds(60); // 1 minute
            long outstandingCallbacks = MainThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(0));
            while (!CloseCallDone && (DateTimeOffset.Now < waitUntil))
            {
                outstandingCallbacks = MainThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(5));
                if (outstandingCallbacks > 0)
                    _loggerRef.Target.LogDebug("Stop: Waiting for close callback ({0} callbacks remaining)", outstandingCallbacks);
            }
            if (!CloseCallDone)
                _loggerRef.Target.LogWarning("Stop: Wait for close callback expired ({0} callbacks remaining)!", outstandingCallbacks);
            // stop the service
            try
            {
                OnBasicSyncStop();
            }
            catch (Exception e)
            {
                _loggerRef.Target.Log(e);
            }
            // dispatch and wait for "final" event to be processed
            MainThreadQueue.Dispatch<object>(null, PostCleanup);
            waitUntil = DateTimeOffset.Now + TimeSpan.FromSeconds(60); // 1 minute
            outstandingCallbacks = MainThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(0));
            while (!FinalCallDone && DateTimeOffset.Now < waitUntil)
            {
                outstandingCallbacks = MainThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(5));
                if (outstandingCallbacks > 0)
                    _loggerRef.Target.LogDebug("Stop: Waiting for final callback ({0} callbacks remaining)", outstandingCallbacks);
            }
            if (!FinalCallDone)
                _loggerRef.Target.LogWarning("Stop: Wait for final callback expired ({0} callbacks remaining)!", outstandingCallbacks);
            // server has stopped
            SetState(BasicServerState.Stopped);
            DisposeHelper.SafeDispose(ref MainThreadQueue);
            _loggerRef.Target.LogInfo("Stopped.");
            _loggerRef.Target.Flush();
            _loggerRef.Dispose();
        }

        protected virtual void OnCloseCallback() { }
        private void PreCleanup(object notUsed)
        {
            try
            {
                OnCloseCallback();
            }
            catch (Exception e)
            {
                _loggerRef.Target.Log(e);
            }
            _serverState.Locked(serverState => serverState.CloseCallDone = true);
        }

        protected virtual void OnFinalCallback() { }
        private void PostCleanup(object notUsed)
        {
            try
            {
                OnFinalCallback();
            }
            catch (Exception e)
            {
                _loggerRef.Target.Log(e);
            }
            _serverState.Locked(serverState => serverState.FinalCallDone = true);
        }
    }
}
