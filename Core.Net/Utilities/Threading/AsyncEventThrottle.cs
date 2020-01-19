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

namespace Highlander.Utilities.Threading
{
    public class AsyncEventThrottle<T> : IDisposable
    {
        private readonly AsyncQueueCallback<T> _Callback;

        // the state
        private T _CallbackData;
        private bool _CallbackDone;

        // closing flag used only during shutdown
        private bool _Closed;
        // the state spinlock
        private int _Lock;
        // other dynamic state
        private long _EnqueueThreads;
        private long _DequeueThreads;

        private void AcquireLock()
        {
            // acquire spinlock
            int locked = Interlocked.Increment(ref _Lock);
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

        public void Dispose()
        {
            _Closed = true;
        }

        public AsyncEventThrottle(AsyncQueueCallback<T> callback)
        {
            _Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public void Dispatch(T data)
        {
            if (_Closed)
                return;
            AcquireLock();
            try
            {
                _CallbackData = data;
                _CallbackDone = false;
            }
            finally
            {
                ReleaseLock();
            }

            // dispatch a dequeue thread
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
            long threadCount;
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
                        //new
                        AcquireLock();
                        try
                        {
                            while (!_CallbackDone)
                            {
                                T callbackData = _CallbackData;
                                _CallbackDone = true;
                                ReleaseLock();
                                try
                                {
                                    if (!_Closed)
                                    {
                                        try
                                        {
                                            _Callback(callbackData);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("AsyncEventThrottle: " + e);
                                        }
                                    }
                                }
                                finally
                                {
                                    AcquireLock();
                                }
                                // next
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
}
