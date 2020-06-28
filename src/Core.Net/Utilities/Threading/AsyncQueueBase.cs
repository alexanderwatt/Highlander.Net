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
using Highlander.Utilities.Logging;

namespace Highlander.Utilities.Threading
{
    /// <summary>
    /// An abstract base class for high-speed asynchronous callback dispatchers, where the
    /// enqueue and dequeue algorithms are provided by derived classes.
    /// </summary>
    public abstract class AsyncQueueBase : IDisposable
    {
        private readonly ILogger _logger;

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
        private bool _closed;
        // the queue state spinlock
        private long _lock;
        // other dynamic state
        private long _enqueueThreads;
        private long _dequeueThreads;

        private void AcquireLock()
        {
            // acquire spinlock
            long locked = Interlocked.Increment(ref _lock);
            while (locked != 1)
            {
                Interlocked.Decrement(ref _lock);
                locked = Interlocked.Increment(ref _lock);
            }
            // spinlock acquired
        }
        private void ReleaseLock()
        {
            // release spinlock
            Interlocked.Decrement(ref _lock);
        }

        protected AsyncQueueBase(ILogger logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _closed = true;
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
                    return ((this.OnGetQueueLength() == 0) && (Interlocked.Add(ref _dequeueThreads, 0) == 0));
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
            while (((this.Length > queueLength) || (Interlocked.Add(ref _dequeueThreads, 0) > 0)) && (DateTimeOffset.Now < waitExpires))
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
            if (_closed)
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
            Interlocked.Increment(ref _enqueueThreads);
            ThreadPool.QueueUserWorkItem(DequeueItems);
        }
        private void DequeueItems(object notUsed)
        {
            if (Interlocked.Decrement(ref _enqueueThreads) > 0)
            {
                // another thread following - exit
                return;
            }

            bool checkDone;
            long threadCount = 0;
            do
            {
                checkDone = false;
                threadCount = Interlocked.Increment(ref _dequeueThreads);
                try
                {
                    if (threadCount == 1)
                    {
                        // we are the dequeue thread - dequeue all items
                        checkDone = true;
                        //new
                        AcquireLock();
                        try
                        {
                            var queueLength = OnGetQueueLength();
                            while (queueLength > 0)
                            {
                                AsyncQueueItemBase item = OnDequeueItem();
                                ReleaseLock();
                                try
                                {
                                    if (item != null)
                                    {
                                        // process item
                                        // - or discard it if closed
                                        if (!_closed)
                                        {
                                            try
                                            {
                                                item.CallUserCallback();
                                            }
                                            catch (System.Exception e)
                                            {
                                                if (_logger != null)
                                                    _logger.Log(e);
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
                                queueLength = OnGetQueueLength();
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
                    threadCount = Interlocked.Decrement(ref _dequeueThreads);
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
            OnUserCallback();
        }
    }
    public class AsyncQueueItemUser<T> : AsyncQueueItemBase
    {
        public AsyncQueueCallback<T> UserCallback { get; }

        public T UserData { get; }

        protected override void OnUserCallback()
        {
            UserCallback(UserData);
        }

        public AsyncQueueItemUser(AsyncQueueCallback<T> userCallback, T userData)
        {
            UserCallback = userCallback;
            UserData = userData;
        }
    }
}
