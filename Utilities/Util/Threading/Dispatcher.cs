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
using Orion.Util.Logging;

namespace Orion.Util.Threading
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
            DispatcherHandle result;
            DispatcherNode[] nodeList;
            string[] keyParts = null;
            int level = 0;
            if (!string.IsNullOrEmpty(key))
            {
                keyParts = key.Split('/');
                level = keyParts.Length;
            }
            nodeList = new DispatcherNode[level + 1];
            nodeList[0] = _RootNode;
            if (level > 0)
                _RootNode.BuildNodeList(level, nodeList, keyParts);
            result = new DispatcherHandle(this, level, nodeList);
            return result;
        }

        public long Wait(TimeSpan timeout)
        {
            DateTimeOffset WaitExpiry = DateTimeOffset.Now + timeout;
            long counter = _OutstandingCallbacks.Counter;
            while ((counter > 0) && (DateTimeOffset.Now < WaitExpiry))
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
        private readonly Dispatcher _Dispatcher;
        private readonly int _Level; // 0 = root
        private readonly DispatcherNode _Parent; // null if root

        // thread-safe state: index of child nodes, and queue of items
        private readonly ILock _ChildIndexLock = new NonblockingSpinlock();
        private readonly IDictionary<string, DispatcherNode> _ChildIndex = new Dictionary<string, DispatcherNode>();

        private Guarded<Queue<DispatcherItem>> _Queue = new Guarded<Queue<DispatcherItem>>(new Queue<DispatcherItem>());

        private long _ThisNodeRunning; // can only be 0 or 1
        private long _ChildrenRunning;
        private long _ThreadsInTree;

        internal long IncThreadsInTree()
        {
            // increment parent first?
            _Parent?.IncThreadsInTree();
            return Interlocked.Increment(ref _ThreadsInTree);
        }
        internal long DecThreadsInTree()
        {
            // decrement parent first?
            _Parent?.DecThreadsInTree();
            return Interlocked.Decrement(ref _ThreadsInTree);
        }
        internal long DecChildrenRunning()
        {
            // decrement parent first?
            _Parent?.DecChildrenRunning();
            return Interlocked.Decrement(ref _ChildrenRunning);
        }

        // constructors
        internal DispatcherNode(Dispatcher Dispatcher, int level, DispatcherNode parent)
        {
            _Dispatcher = Dispatcher;
            if (level < 0)
                throw new ArgumentException("Invalid", nameof(level));
            _Level = level;
            if ((level == 0) && (parent != null))
                throw new ArgumentException("Invalid", nameof(parent));
            if ((level > 0) && (parent == null))
                throw new ArgumentNullException(nameof(parent));
            _Parent = parent;
        }

        internal void BuildNodeList(int level, DispatcherNode[] nodeList, string[] keyParts)
        {
            // find/create subnode
            nodeList[_Level] = this;
            DispatcherNode subNode;
            _ChildIndexLock.Enter();
            try
            {
                if (!_ChildIndex.TryGetValue(keyParts[_Level], out subNode))
                {
                    subNode = new DispatcherNode(_Dispatcher, _Level + 1, this);
                    _ChildIndex.Add(keyParts[_Level], subNode);
                }
            }
            finally
            {
                _ChildIndexLock.Leave();
            }
            nodeList[_Level + 1] = subNode;
            if (level > (_Level + 1))
            {
                subNode.BuildNodeList(level, nodeList, keyParts);
            }
        }

        internal void Enqueue(DispatcherItem item)
        {
            _Queue.Locked((queue) => queue.Enqueue(item));
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
                _Dispatcher.SetLastException(excp);
                _Dispatcher.Logger.Log(excp);
            }
            _Dispatcher.DecOutstanding();

            // now dispatch any waiting items
            ProcessNode(item);
        }

        internal void AsyncProcessNode(object notUsed)
        {
            ProcessNode(null);
        }

        private void ProcessNode(DispatcherItem completedItem)
        {
            long threadsInTree = IncThreadsInTree();
            try
            {
                if (completedItem != null)
                {
                    // completed item cleanup
                    DecThreadsInTree();
                    long debugN = Interlocked.Decrement(ref _ThisNodeRunning);
                    Debug.Assert(debugN == 0);
                    _Parent?.DecChildrenRunning();
                    completedItem = null;
                }

                bool looping = true;
                while (looping)
                {
                    looping = false;
                    // ensure that at least one pass through the dequeuing logic occurs
                    _Queue.Locked((queue) =>
                    {
                        // we have the queue
                        // we can dispatch the head of the queue if:
                        // a) the item is for this level, AND this tree is not running; 
                        // or b) the item is for a lower level, AND this node is not running.
                        if (queue.Count > 0)
                        {
                            DispatcherItem workItem = queue.Peek();
                            Debug.Assert(workItem.Level >= _Level);
                            if ((workItem.Level == _Level)
                                && (Interlocked.Add(ref _ThisNodeRunning, 0) == 0)
                                && (Interlocked.Add(ref _ChildrenRunning, 0) == 0))
                            {
                                // item is for this level
                                // dispatch it to the threadpool
                                queue.Dequeue();
                                long debugN = Interlocked.Increment(ref _ThisNodeRunning);
                                Debug.Assert(debugN == 1);
                                IncThreadsInTree();
                                ThreadPool.QueueUserWorkItem(CallbackWrapper, workItem);
                            }
                            if ((workItem.Level > _Level)
                                && (Interlocked.Add(ref _ThisNodeRunning, 0) == 0))
                            {
                                // item is for lower level
                                // enqueue it to the child
                                queue.Dequeue();
                                Interlocked.Increment(ref _ChildrenRunning);
                                DispatcherNode childNode = workItem.NodeList[_Level + 1];
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
            if ((threadsInTree == 0) && (_Parent != null))
            {
                // we are the last thread in this tree - handoff to parent
                ThreadPool.QueueUserWorkItem(_Parent.AsyncProcessNode, null);
            }
        }
    }
}
