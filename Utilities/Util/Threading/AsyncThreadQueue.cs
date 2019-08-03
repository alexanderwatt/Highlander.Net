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
using Orion.Util.Logging;

namespace Orion.Util.Threading
{
    /// <summary>
    /// Implements an asynchronous callback queue with normal FIFO queueing.
    /// </summary>
    public sealed class AsyncThreadQueue : AsyncQueueBase
    {
        private readonly Queue<AsyncQueueItemBase> _queue = new Queue<AsyncQueueItemBase>();

        public AsyncThreadQueue(ILogger logger)
            : base(logger)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        public void Dispatch<T>(T data, AsyncQueueCallback<T> callback)
        {
            EnqueueData(data, callback, AsyncQueuePriority.Normal, null);
        }

        protected override long OnGetQueueLength()
        {
            return _queue.Count;
        }

        protected override void OnEnqueueItem(AsyncQueueItemBase item, AsyncQueuePriority priority, string itemKey)
        {
            _queue.Enqueue(item);
        }

        protected override AsyncQueueItemBase OnDequeueItem()
        {
            if (_queue.Count > 0)
                return _queue.Dequeue();
            return null;
        }
    }
}
