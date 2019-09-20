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
using System.Linq;
using Highlander.Utilities.Logging;

namespace Highlander.Utilities.Threading
{
    /// <summary>
    /// Implements an asynchronous callback queue with normal FIFO queueing.
    /// </summary>
    public sealed class AsyncPriorityStack : AsyncQueueBase
    {
        private readonly Dictionary<string, AsyncQueueItemBase>[] _priorityStack;

        public AsyncPriorityStack(ILogger logger)
            : base(logger)
        {
            int numberOfQueues = (int)(AsyncQueuePriority.Highest - AsyncQueuePriority.Lowest + 1);
            _priorityStack = new Dictionary<string, AsyncQueueItemBase>[numberOfQueues];
            for (int i = 0; i < _priorityStack.Length; i++)
                _priorityStack[i] = new Dictionary<string, AsyncQueueItemBase>();
        }

        public void Dispatch<T>(T data, AsyncQueueCallback<T> callback, AsyncQueuePriority priority, string itemKey)
        {
            EnqueueData<T>(data, callback, priority, itemKey);
        }

        protected override long OnGetQueueLength()
        {
            long count = 0;
            int priority = (int)AsyncQueuePriority.Highest;
            do
            {
                count += _priorityStack[priority].Count;
                priority--;
            }
            while (priority >= (int)AsyncQueuePriority.Lowest);
            return count;
        }

        protected override void OnEnqueueItem(AsyncQueueItemBase item, AsyncQueuePriority priority, string itemKey)
        {
            _priorityStack[(int)priority][itemKey] = item;
        }

        protected override AsyncQueueItemBase OnDequeueItem()
        {
            int priority = (int)AsyncQueuePriority.Highest;
            do
            {
                // attempt destack in priority order
                Dictionary<string, AsyncQueueItemBase> stack = _priorityStack[priority];
                if (stack.Count > 0)
                {
                    var kvp = stack.ElementAt(0);
                    stack.Remove(kvp.Key);
                    return kvp.Value;
                }
                //next
                priority--;
            }
            while (priority >= (int)AsyncQueuePriority.Lowest);
            return null;
        }
    }
}
