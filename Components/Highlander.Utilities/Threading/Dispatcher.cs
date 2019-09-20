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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Highlander.Utilities.Logging;

namespace Highlander.Utilities.Threading
{
    public interface IDispatcher
    {
        DispatcherHandle GetDispatcherHandle(string key);
        long Wait(TimeSpan timeout);
        ILogger Logger { get; set; }
    }

    internal class DispatcherItem
    {
        internal int Level { get; }
        internal DispatcherNode[] NodeList { get; }
        internal SendOrPostCallback UserCallback { get; }
        internal object UserContext { get; }

        internal DispatcherItem(int level, DispatcherNode[] nodeList, SendOrPostCallback userCallback, object userContext)
        {
            Level = level;
            NodeList = nodeList;
            UserCallback = userCallback;
            UserContext = userContext;
        }
    }

    public delegate void DispatcherCallback<T>(T data);
    public class DispatcherHandle
    {
        private readonly Dispatcher _dispatcher;
        public int Level { get; }
        private readonly DispatcherNode[] _nodeList;
        internal DispatcherHandle(Dispatcher dispatcher, int level, DispatcherNode[] nodeList)
        {
            _dispatcher = dispatcher;
            Level = level;
            _nodeList = nodeList;
        }
        // traditional type-less callback
        public void DispatchObject(SendOrPostCallback callback, object state)
        {
            _dispatcher.IncOutstanding();
            _nodeList[0].Enqueue(new DispatcherItem(Level, _nodeList, callback, state));
        }
        // generic type-safe callback
        public void DispatchCall<T>(T data, DispatcherCallback<T> callback)
        {
            DispatchObject((state) => callback((T)state), data);
        }
    }

    /// <summary>
    /// Implements a top-down callback Dispatcher.
    /// </summary>
    public class Dispatcher : IDispatcher
    {
        private ILogger _Logger;
        public ILogger Logger
        {
            get => _Logger;
            set => _Logger = value ?? throw new ArgumentNullException("logger");
        }
        private readonly DispatcherNode _RootNode;
        private readonly LoggedCounter _OutstandingCallbacks;
        private int _UserExceptionCount;

        public Dispatcher(ILogger logger, string debugName)
        {
            _Logger = logger ?? throw new ArgumentNullException("logger");
            _OutstandingCallbacks = new LoggedCounter(logger, debugName, 0, 250); // todo 1000
            _RootNode = new DispatcherNode(this, 0, null);
        }
        public Dispatcher(ILogger logger) : this(logger, null) { }

        internal void IncOutstanding()
        {
            _OutstandingCallbacks.Increment();
        }

        internal void DecOutstanding()
        {
            _OutstandingCallbacks.Decrement();
        }

        internal void SetLastException(Exception excp)
        {
            Interlocked.Increment(ref _UserExceptionCount);
        }

        public DispatcherHandle GetDispatcherHandle(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            string[] keyParts = null;
            int level = 0;
            if (!string.IsNullOrEmpty(key))
            {
                keyParts = key.Split('/');
                level = keyParts.Length;
            }
            var nodeList = new DispatcherNode[level + 1];
            nodeList[0] = _RootNode;
            if (level > 0)
                _RootNode.BuildNodeList(level, nodeList, keyParts);
            var result = new DispatcherHandle(this, level, nodeList);
            return result;
        }

        public long Wait(TimeSpan timeout)
        {
            DateTimeOffset waitExpiry = DateTimeOffset.Now + timeout;
            long counter = _OutstandingCallbacks.Counter;
            while ((counter > 0) && (DateTimeOffset.Now < waitExpiry))
            {
                Thread.Sleep(50);
                counter = _OutstandingCallbacks.Counter;
            }
            return counter;
        }
    }

    /// <summary>
    /// A node in the Dispatcher tree hierarchy which contains a queue of unexecuted
    /// callback items. This class implements its own thread synchronisation and is 
    /// designed to be used in multi-threaded applications.
    /// </summary>
    internal class DispatcherNode
    {
        // readonly state
        private readonly Dispatcher _dispatcher;
        private readonly int _level; // 0 = root
        private readonly DispatcherNode _parent; // null if root

        // thread-safe state: index of child nodes, and queue of items
        private readonly ILock _childIndexLock = new NonblockingSpinlock();
        private readonly IDictionary<string, DispatcherNode> _childIndex = new Dictionary<string, DispatcherNode>();

        private readonly Guarded<Queue<DispatcherItem>> _queue = new Guarded<Queue<DispatcherItem>>(new Queue<DispatcherItem>());

        private long _thisNodeRunning; // can only be 0 or 1
        private long _childrenRunning;
        private long _threadsInTree;

        internal long IncThreadsInTree()
        {
            // increment parent first?
            _parent?.IncThreadsInTree();
            return Interlocked.Increment(ref _threadsInTree);
        }
        internal long DecThreadsInTree()
        {
            // decrement parent first?
            _parent?.DecThreadsInTree();
            return Interlocked.Decrement(ref _threadsInTree);
        }
        internal long DecChildrenRunning()
        {
            // decrement parent first?
            _parent?.DecChildrenRunning();
            return Interlocked.Decrement(ref _childrenRunning);
        }

        // constructors
        internal DispatcherNode(Dispatcher dispatcher, int level, DispatcherNode parent)
        {
            _dispatcher = dispatcher;
            if (level < 0)
                throw new ArgumentException("Invalid", nameof(level));
            _level = level;
            if ((level == 0) && (parent != null))
                throw new ArgumentException("Invalid", nameof(parent));
            if ((level > 0) && (parent == null))
                throw new ArgumentNullException(nameof(parent));
            _parent = parent;
        }

        internal void BuildNodeList(int level, DispatcherNode[] nodeList, string[] keyParts)
        {
            // find/create subnode
            nodeList[_level] = this;
            DispatcherNode subNode;
            _childIndexLock.Enter();
            try
            {
                if (!_childIndex.TryGetValue(keyParts[_level], out subNode))
                {
                    subNode = new DispatcherNode(_dispatcher, _level + 1, this);
                    _childIndex.Add(keyParts[_level], subNode);
                }
            }
            finally
            {
                _childIndexLock.Leave();
            }
            nodeList[_level + 1] = subNode;
            if (level > (_level + 1))
            {
                subNode.BuildNodeList(level, nodeList, keyParts);
            }
        }

        internal void Enqueue(DispatcherItem item)
        {
            _queue.Locked((queue) => queue.Enqueue(item));
            ThreadPool.QueueUserWorkItem(AsyncProcessNode, null);
        }

        private void CallbackWrapper(object state)
        {
            DispatcherItem item = (DispatcherItem)state;
            SendOrPostCallback userCallback = item.UserCallback;
            object userContext = item.UserContext;
            try
            {
                userCallback(userContext);
            }
            catch (Exception excp)
            {
                _dispatcher.SetLastException(excp);
                _dispatcher.Logger.Log(excp);
            }
            _dispatcher.DecOutstanding();

            // now dispatch any waiting items
            ProcessNode(item);
        }

        internal void AsyncProcessNode(object notUsed)
        {
            ProcessNode(null);
        }

        private void ProcessNode(DispatcherItem completedItem)
        {
            long threadsInTree;
            try
            {
                if (completedItem != null)
                {
                    // completed item cleanup
                    DecThreadsInTree();
                    long debugN = Interlocked.Decrement(ref _thisNodeRunning);
                    Debug.Assert(debugN == 0);
                    _parent?.DecChildrenRunning();
                    completedItem = null;
                }
                bool looping = true;
                while (looping)
                {
                    looping = false;
                    // ensure that at least one pass through the dequeuing logic occurs
                    _queue.Locked((queue) =>
                    {
                        // we have the queue
                        // we can dispatch the head of the queue if:
                        // a) the item is for this level, AND this tree is not running; 
                        // or b) the item is for a lower level, AND this node is not running.
                        if (queue.Count > 0)
                        {
                            DispatcherItem workItem = queue.Peek();
                            Debug.Assert(workItem.Level >= _level);
                            if ((workItem.Level == _level)
                                && (Interlocked.Add(ref _thisNodeRunning, 0) == 0)
                                && (Interlocked.Add(ref _childrenRunning, 0) == 0))
                            {
                                // item is for this level
                                // dispatch it to the threadpool
                                queue.Dequeue();
                                long debugN = Interlocked.Increment(ref _thisNodeRunning);
                                Debug.Assert(debugN == 1);
                                IncThreadsInTree();
                                ThreadPool.QueueUserWorkItem(CallbackWrapper, workItem);
                            }
                            if ((workItem.Level > _level)
                                && (Interlocked.Add(ref _thisNodeRunning, 0) == 0))
                            {
                                // item is for lower level
                                // enqueue it to the child
                                queue.Dequeue();
                                Interlocked.Increment(ref _childrenRunning);
                                DispatcherNode childNode = workItem.NodeList[_level + 1];
                                childNode.Enqueue(workItem);
                                // dequeue next
                                looping = true;
                            }
                        }
                    });
                } // while looping
            }
            finally
            {
                threadsInTree = DecThreadsInTree();
            }
            if ((threadsInTree == 0) && (_parent != null))
            {
                // we are the last thread in this tree - handoff to parent
                ThreadPool.QueueUserWorkItem(_parent.AsyncProcessNode, null);
            }
        }
    }
}
