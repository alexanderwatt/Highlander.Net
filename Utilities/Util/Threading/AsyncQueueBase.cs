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
using System.Diagnostics;
using System.Threading;
using Orion.Util.Logging;

namespace Orion.Util.Threading
{
    /// <summary>
    /// An abstract base class for high-speed asynchronous callback dispatchers, where the
    /// enqueue and dequeue algorithms are provided by derived classes.
    /// </summary>
    public abstract class AsyncQueueBase : IDisposable
    {
        private readonly ILogger _Logger;

        // virtual methods the derived class must implement
        protected virtual long OnGetQueueLength()
        {
            throw new NotImplementedException();
        }
        protected virtual void OnEnqueueItem(AsyncQueueItemBase item, AsyncQueuePriority priority, string itemKey)
        {
            throw new NotImplementedException();
        }
        protected virtual AsyncQueueItemBase OnDequeueItem()
        {
            throw new NotImplementedException();
        }

        // state used only during shutdown
        private bool _Closed;
        // the queue state spinlock
        private long _Lock;
        // other dynamic state
        private long _EnqueueThreads;
        private long _DequeueThreads;

        private void AcquireLock()
        {
            // acquire spinlock
            long locked = Interlocked.Increment(ref _Lock);
            while (locked != 1)
            {
                Interlocked.Decrement(ref _Lock);
                locked = Interlocked.Increment(ref _Lock);
            }
            // spinlock acquired
        }
        private void ReleaseLock()
        {
            // release spinlock
            Interlocked.Decrement(ref _Lock);
        }

        public AsyncQueueBase(ILogger logger)
        {
            _Logger = logger;
        }

        public void Dispose()
        {
            _Closed = true;
        }

        public long Length
        {
            get
            {
                AcquireLock();
                try
                {
                    return this.OnGetQueueLength();
                }
                finally
                {
                    ReleaseLock();
                }
            }
        }
        public bool IsIdle
        {
            get
            {
                AcquireLock();
                try
                {
                    return ((this.OnGetQueueLength() == 0) && (Interlocked.Add(ref _DequeueThreads, 0) == 0));
                }
                finally
                {
                    ReleaseLock();
                }
            }
        }

        public long WaitUntilEmpty(TimeSpan timeout)
        {
            return WaitUntilLengthLEQ(0, timeout);
        }

        public long WaitUntilLengthLEQ(long queueLength, TimeSpan timeout)
        {
            DateTimeOffset waitExpires = DateTimeOffset.Now + timeout;
            while (((this.Length > queueLength) || (Interlocked.Add(ref _DequeueThreads, 0) > 0)) && (DateTimeOffset.Now < waitExpires))
            {
                Thread.Sleep(1);
            }
            long length = this.Length;
            //if (length > queueLength)
            //{
            //    _Logger.LogDebug("Items remaining in queue: {0}", length);
            //}
            return length;
        }

        protected void EnqueueData<T>(T data, AsyncQueueCallback<T> callback, AsyncQueuePriority priority, string itemKey)
        {
            if (_Closed)
                return; // throw new InvalidOperationException("Cannot enqueue while closing!");
            AsyncQueueItemUser<T> item = new AsyncQueueItemUser<T>(callback, data);
            AcquireLock();
            try
            {
                this.OnEnqueueItem(item, priority, itemKey);
            }
            finally
            {
                ReleaseLock();
            }

            // dispatch a dequeue thread if needed
            Interlocked.Increment(ref _EnqueueThreads);
            ThreadPool.QueueUserWorkItem(DequeueItems);
        }
        private void DequeueItems(object notUsed)
        {
            if (Interlocked.Decrement(ref _EnqueueThreads) > 0)
            {
                // another thread following - exit
                return;
            }

            bool checkDone;
            long threadCount = 0;
            do
            {
                checkDone = false;
                threadCount = Interlocked.Increment(ref _DequeueThreads);
                try
                {
                    if (threadCount == 1)
                    {
                        // we are the dequeue thread - dequeue all items
                        checkDone = true;
                        long queueLength;
                        //new
                        AcquireLock();
                        try
                        {
                            queueLength = this.OnGetQueueLength();
                            while (queueLength > 0)
                            {
                                AsyncQueueItemBase item = this.OnDequeueItem();
                                ReleaseLock();
                                try
                                {
                                    if (item != null)
                                    {
                                        // process item
                                        // - or discard it if closed
                                        if (!_Closed)
                                        {
                                            try
                                            {
                                                item.CallUserCallback();
                                            }
                                            catch (Exception e)
                                            {
                                                if (_Logger != null)
                                                    _Logger.Log(e);
                                                else
                                                    Debug.WriteLine("AsyncQueueBase: " + e.ToString());
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    AcquireLock();
                                }
                                // next
                                queueLength = this.OnGetQueueLength();
                            }
                        }
                        finally
                        {
                            ReleaseLock();
                        }
                    }
                }
                finally
                {
                    threadCount = Interlocked.Decrement(ref _DequeueThreads);
                }
            } while ((threadCount == 0) && (!checkDone));
        }
    }

    public enum AsyncQueuePriority
    {
        Lowest,
        //VeryLow,
        //Low,
        BelowNormal,
        Normal,
        AboveNormal,
        //High,
        //VeryHigh,
        Highest
    }

    public delegate void AsyncQueueCallback<T>(T data);

    public class AsyncQueueItemBase
    {
        protected virtual void OnUserCallback()
        {
            throw new NotImplementedException();
        }
        public void CallUserCallback()
        {
            this.OnUserCallback();
        }
    }
    public class AsyncQueueItemUser<T> : AsyncQueueItemBase
    {
        private readonly AsyncQueueCallback<T> _UserCallback;
        public AsyncQueueCallback<T> UserCallback { get { return _UserCallback; } }
        private readonly T _UserData;
        public T UserData { get { return _UserData; } }
        protected override void OnUserCallback()
        {
            _UserCallback(_UserData);
        }
        public AsyncQueueItemUser(AsyncQueueCallback<T> userCallback, T userData)
        {
            _UserCallback = userCallback;
            _UserData = userData;
        }
    }
}
