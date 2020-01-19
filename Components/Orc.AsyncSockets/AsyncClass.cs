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

using System;
using System.Diagnostics;

namespace Highlander.Orc.AsyncSockets
{
    public enum AsyncClassState
    {
        Initial,
        Starting,
        Active,
        Stopping,
        Stopped
    }
    public delegate void AsyncErrorCallback(object context, Exception excp);
    public delegate void AsyncStateCallback(object context, AsyncClassState oldState, AsyncClassState newState);
    public delegate void AsyncEventCallback(object context, object data);
    public interface IAsyncClass
    {
        AsyncClassState AsyncState { get; }
        AsyncErrorCallback ErrorCallback { get; set; }
        AsyncStateCallback StateCallback { get; set; }
        AsyncEventCallback EventCallback { get; set; }
        void AsyncStart();
        void AsyncPost(object data);
        void AsyncStop(string reason);
    }
    public class AsyncClass : IAsyncClass
    {
        private volatile AsyncClassState _state = AsyncClassState.Initial;

        public AsyncClassState AsyncState => _state;

        private string _stopReason = "";

        public string AsyncStopReason => _stopReason;

        public bool AsyncStateIsActive
        {
            get
            {
                AsyncClassState state = _state;
                return state == AsyncClassState.Active || state == AsyncClassState.Starting;
            }
        }

        public object AsyncContext { get; }

        private readonly SequencerHandle _sequencerHandle;
        private AsyncErrorCallback _errorCallback;
        private AsyncStateCallback _stateCallback;
        private AsyncEventCallback _eventCallback;

        public AsyncClass(object context)
        {
            // default constructor
            AsyncContext = context;
            ISequencer sequencer = new Sequencer();
            _sequencerHandle = sequencer.GetSequencerHandle(".");
        }

        public AsyncClass(object context, ISequencer sequencer, string sequencerKey)
        {
            AsyncContext = context;
            _sequencerHandle = sequencer.GetSequencerHandle(sequencerKey);
        }

        public AsyncErrorCallback ErrorCallback 
        {
            get => _errorCallback;
            set
            {
                Debug.Assert(_state == AsyncClassState.Initial);
                _errorCallback = value;
            }
        }

        public AsyncStateCallback StateCallback
        {
            get => _stateCallback;
            set
            {
                Debug.Assert(_state == AsyncClassState.Initial);
                _stateCallback = value;
            }
        }

        public AsyncEventCallback EventCallback
        {
            get => _eventCallback;
            set
            {
                Debug.Assert(_state == AsyncClassState.Initial);
                _eventCallback = value;
            }
        }

        protected virtual void OnAsyncException(Exception excp)
        {
            try
            {
                _errorCallback?.Invoke(AsyncContext, excp);
            }
            catch
            {
                // ignore - we tried our best
            }
        }

        protected virtual void OnAsyncStateChange(AsyncClassState oldState, AsyncClassState newState)
        {
            _stateCallback?.Invoke(AsyncContext, oldState, newState);
        }

        protected virtual void OnAsyncProcessData(object data)
        {
            _eventCallback?.Invoke(AsyncContext, data);
        }

        public void AsyncStart()
        {
            try
            {
                AsyncClassState oldState = _state;
                AsyncClassState newState;
                lock (this)
                {
                    if (_state != AsyncClassState.Initial)
                        throw new ApplicationException("Already started!");
                    _state = AsyncClassState.Starting;
                    newState = _state;
                }
                if (newState == oldState) return;
                OnAsyncStateChange(oldState, newState);
                _sequencerHandle.SequenceCallback(StartCallback, null);
            }
            catch (Exception excp)
            {
                OnAsyncException(excp);
            }
        }

        private void StartCallback(object state)
        {
            try
            {
                AsyncClassState oldState = _state;
                AsyncClassState newState;
                lock (this)
                {
                    _state = AsyncClassState.Active;
                    newState = _state;
                }
                if (newState != oldState)
                {
                    OnAsyncStateChange(oldState, newState);
                }
            }
            catch (Exception excp)
            {
                this.OnAsyncException(excp);
            }
        }

        public void AsyncStop(string reason)
        {
            try
            {
                AsyncClassState oldState = _state;
                AsyncClassState newState;
                lock (this)
                {
                    if (_state == AsyncClassState.Initial)
                        throw new ApplicationException("Not started!");
                    if ((_state != AsyncClassState.Stopped))
                        _state = AsyncClassState.Stopping;
                    newState = _state;
                }
                if (newState != oldState)
                {
                    _stopReason = reason;
                    OnAsyncStateChange(oldState, newState);
                    _sequencerHandle.SequenceCallback(StopCallback, null);
                }
            }
            catch (Exception excp)
            {
                OnAsyncException(excp);
            }
        }

        private void StopCallback(object state)
        {
            try
            {
                AsyncClassState oldState = _state;
                AsyncClassState newState;
                lock (this)
                {
                    _state = AsyncClassState.Stopped;
                    newState = _state;
                }
                if (newState != oldState)
                {
                    OnAsyncStateChange(oldState, newState);
                }
            }
            catch (Exception excp)
            {
                OnAsyncException(excp);
            }
        }

        public void AsyncPost(object data)
        {
            if (_state != AsyncClassState.Active
                && _state != AsyncClassState.Starting)
                throw new ApplicationException("Not started!");
            _sequencerHandle.SequenceCallback(DataCallback, data);
        }

        private void DataCallback(object data)
        {
            // notify client 
            try
            {
                OnAsyncProcessData(data);
            }
            catch (Exception excp)
            {
                OnAsyncException(excp);
            }
        }
    }
}
