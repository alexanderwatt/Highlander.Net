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

using System.Collections.Generic;
using Highlander.Utilities.Logging;

namespace Highlander.Utilities.Threading
{
    /// <summary>
    /// Implements an asynchronous callback queue with normal FIFO queueing.
    /// </summary>
    public sealed class AsyncPriorityQueue : AsyncQueueBase
    {
        private readonly Queue<AsyncQueueItemBase>[] _priorityQueue;

        public AsyncPriorityQueue(ILogger logger)
            : base(logger)
        {
            int numberOfQueues = AsyncQueuePriority.Highest - AsyncQueuePriority.Lowest + 1;
            _priorityQueue = new Queue<AsyncQueueItemBase>[numberOfQueues];
            for (int i = 0; i < _priorityQueue.Length; i++)
                _priorityQueue[i] = new Queue<AsyncQueueItemBase>();
        }

        public void Dispatch<T>(T data, AsyncQueueCallback<T> callback, AsyncQueuePriority priority)
        {
            EnqueueData(data, callback, priority, null);
        }

        protected override long OnGetQueueLength()
        {
            long count = 0;
            int priority = (int)AsyncQueuePriority.Highest;
            do
            {
                count += _priorityQueue[priority].Count;
                priority--;
            }
            while (priority >= (int)AsyncQueuePriority.Lowest);
            return count;
        }

        protected override void OnEnqueueItem(AsyncQueueItemBase item, AsyncQueuePriority priority, string itemKey)
        {
            _priorityQueue[(int)priority].Enqueue(item);
        }

        protected override AsyncQueueItemBase OnDequeueItem()
        {
            int priority = (int)AsyncQueuePriority.Highest;
            do
            {
                // attempt dequeue in priority order
                if (_priorityQueue[priority].Count > 0)
                {
                    return _priorityQueue[priority].Dequeue();
                }
                //next
                priority--;
            }
            while (priority >= (int)AsyncQueuePriority.Lowest);
            return null;
        }
    }
}
