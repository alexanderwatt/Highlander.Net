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
using System.Linq;
using Orion.Util.Logging;

namespace Orion.Util.Threading
{
    /// <summary>
    /// Implements an asynchronous callback queue with normal FIFO queueing.
    /// </summary>
    public sealed class AsyncPriorityStack : AsyncQueueBase
    {
        private readonly Dictionary<string, AsyncQueueItemBase>[] _PriorityStack;

        public AsyncPriorityStack(ILogger logger)
            : base(logger)
        {
            int numberOfQueues = (int)(AsyncQueuePriority.Highest - AsyncQueuePriority.Lowest + 1);
            _PriorityStack = new Dictionary<string, AsyncQueueItemBase>[numberOfQueues];
            for (int i = 0; i < _PriorityStack.Length; i++)
                _PriorityStack[i] = new Dictionary<string, AsyncQueueItemBase>();
        }

        public void Dispatch<T>(T data, AsyncQueueCallback<T> callback, AsyncQueuePriority priority, string itemKey)
        {
            base.EnqueueData<T>(data, callback, priority, itemKey);
        }

        protected override long OnGetQueueLength()
        {
            long count = 0;
            int priority = (int)AsyncQueuePriority.Highest;
            do
            {
                count += _PriorityStack[priority].Count;
                priority--;
            }
            while (priority >= (int)AsyncQueuePriority.Lowest);
            return count;
        }

        protected override void OnEnqueueItem(AsyncQueueItemBase item, AsyncQueuePriority priority, string itemKey)
        {
            _PriorityStack[(int)priority][itemKey] = item;
        }

        protected override AsyncQueueItemBase OnDequeueItem()
        {
            int priority = (int)AsyncQueuePriority.Highest;
            do
            {
                // attempt destack in priority order
                Dictionary<string, AsyncQueueItemBase> stack = _PriorityStack[priority];
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
