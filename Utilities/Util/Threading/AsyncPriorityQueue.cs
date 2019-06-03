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

using System.Collections.Generic;
using Orion.Util.Logging;

namespace Orion.Util.Threading
{
    /// <summary>
    /// Implements an asynchronous callback queue with normal FIFO queueing.
    /// </summary>
    public sealed class AsyncPriorityQueue : AsyncQueueBase
    {
        private readonly Queue<AsyncQueueItemBase>[] _PriorityQueue;

        public AsyncPriorityQueue(ILogger logger)
            : base(logger)
        {
            int numberOfQueues = (int)(AsyncQueuePriority.Highest - AsyncQueuePriority.Lowest + 1);
            _PriorityQueue = new Queue<AsyncQueueItemBase>[numberOfQueues];
            for (int i = 0; i < _PriorityQueue.Length; i++)
                _PriorityQueue[i] = new Queue<AsyncQueueItemBase>();
        }

        public void Dispatch<T>(T data, AsyncQueueCallback<T> callback, AsyncQueuePriority priority)
        {
            base.EnqueueData<T>(data, callback, priority, null);
        }

        protected override long OnGetQueueLength()
        {
            long count = 0;
            int priority = (int)AsyncQueuePriority.Highest;
            do
            {
                count += _PriorityQueue[priority].Count;
                priority--;
            }
            while (priority >= (int)AsyncQueuePriority.Lowest);
            return count;
        }

        protected override void OnEnqueueItem(AsyncQueueItemBase item, AsyncQueuePriority priority, string itemKey)
        {
            _PriorityQueue[(int)priority].Enqueue(item);
        }

        protected override AsyncQueueItemBase OnDequeueItem()
        {
            int priority = (int)AsyncQueuePriority.Highest;
            do
            {
                // attempt dequeue in priority order
                if (_PriorityQueue[priority].Count > 0)
                {
                    return _PriorityQueue[priority].Dequeue();
                }
                //next
                priority--;
            }
            while (priority >= (int)AsyncQueuePriority.Lowest);
            return null;
        }
    }
}
