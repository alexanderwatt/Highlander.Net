/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

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

namespace Highlander.Orc.AsyncSockets
{
    public interface ISequencer
    {
        SequencerHandle GetSequencerHandle(string key);
        void SequenceCallbackWithKey(string key, WaitCallback callback, object state);
        void PostUnsequencedCallback(WaitCallback callback, object state);
        int Wait(TimeSpan timeout);
    }

    internal class SequencerTask
    {
        internal int Level { get; }
        internal SequencerNode[] NodeList { get; }
        internal WaitCallback UserCallback { get; }
        internal object UserContext { get; }

        internal SequencerTask(int level, SequencerNode[] nodeList, WaitCallback userCallback, object userContext)
        {
            Level = level;
            NodeList = nodeList;
            UserCallback = userCallback;
            UserContext = userContext;
        }
    }

    public class SequencerHandle
    {
        private readonly Sequencer _sequencer;
        public int Level { get; }
        internal SequencerNode[] NodeList { get; }

        internal SequencerHandle(Sequencer sequencer, int level, SequencerNode[] nodeList)
        {
            _sequencer = sequencer;
            Level = level;
            NodeList = nodeList;
        }
        public void SequenceCallback(WaitCallback callback, object state)
        {
            _sequencer.IncOutstanding();
            NodeList[0].Enqueue(new SequencerTask(Level, NodeList, callback, state));
        }
    }

    /// <summary>
    /// Implements a top-down callback sequencer.
    /// </summary>
    public class Sequencer : ISequencer
    {
        private readonly SequencerNode _rootNode;
        private volatile int _outstandingCallbacks;
        private int _userExceptionCount;
        private Exception _lastException;

        public Sequencer()
        {
            _rootNode = new SequencerNode(this, 0, null);
        }

#pragma warning disable 420
        internal void IncOutstanding()
        {
            Interlocked.Increment(ref _outstandingCallbacks);
        }

        internal void DecOutstanding()
        {
            Interlocked.Decrement(ref _outstandingCallbacks);
        }
#pragma warning restore 420

        internal void SetLastException(Exception excp)
        {
            Interlocked.Increment(ref _userExceptionCount);
            _lastException = excp;
        }

        public void PostUnsequencedCallback(WaitCallback callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            SequencerTask task = new SequencerTask(-1, null, callback, state);
            IncOutstanding();
            ThreadPool.QueueUserWorkItem(UnsequencedCallback, task);
        }

        public SequencerHandle GetSequencerHandle(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            string[] keyParts = null;
            int level = 0;
            if (key != "")
            {
                keyParts = key.Split('/');
                level = keyParts.Length;
            }
            var nodeList = new SequencerNode[level + 1];
            nodeList[0] = _rootNode;
            if (level > 0)
                _rootNode.BuildNodeList(level, nodeList, keyParts);
            var result = new SequencerHandle(this, level, nodeList);
            return result;
        }

        public void SequenceCallbackWithKey(string key, WaitCallback callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            SequencerHandle handle = GetSequencerHandle(key);
            SequencerTask task = new SequencerTask(handle.Level, handle.NodeList, callback, state);
            IncOutstanding();
            _rootNode.Enqueue(task);
        }

        public int Wait(TimeSpan timeout)
        {
            DateTime waitExpiry = DateTime.UtcNow + timeout;
            while (_outstandingCallbacks > 0 && DateTime.UtcNow < waitExpiry)
            {
                Thread.Sleep(50);
            }
            return _outstandingCallbacks;
        }

        private void UnsequencedCallback(object state)
        {
            DecOutstanding();
            SequencerTask task = (SequencerTask)state;
            WaitCallback userCallback = task.UserCallback;
            object userContext = task.UserContext;
            try
            {
                userCallback(userContext);
            }
            catch (Exception excp)
            {
                Interlocked.Increment(ref _userExceptionCount);
                _lastException = excp;
            }
            // note: no cleanup required for unsequenced tasks
        }
    }

    /// <summary>
    /// A node in the sequencer tree hierarchy which contains a queue of unexecuted
    /// callback tasks. This class implements its own thread synchronisation and is 
    /// designed to be used in multi-threaded applications.
    /// </summary>
    internal class SequencerNode
    {
        // algorithms
        // - enqueue task
        // - dequeue task

        // lock management
        // - there is only one lock per node which is used for protecting both 
        //   enqueuing and dequeuing data structures. Node locks must be acquired
        //   in the direction of root to leaf (parent to children).
        // - Following this rule will prevent deadlock from occurring.

        // readonly state
        private readonly Sequencer _sequencer;
        private readonly int _level; // 0 = root
        internal SequencerNode Parent { get; }
        private readonly IDictionary<string, SequencerNode> _childIndex;
        private readonly Queue<SequencerTask> _queue;

        // turnlock
        //private int _TurnlockHead = 0;
        //private int _TurnlockTail = 1;

        //private void Lock()
        //{
        //    int thisTurn = Interlocked.Increment(ref _TurnlockHead);
        //    int lastTurn = _TurnlockTail;
        //    while (lastTurn != thisTurn)
        //    {
        //        Thread.Sleep(0);
        //        Interlocked.Decrement(ref _TurnlockTail);
        //        lastTurn = Interlocked.Increment(ref _TurnlockTail);
        //    }
        //}

        //private void Unlock()
        //{
        //    Interlocked.Increment(ref _TurnlockTail);
        //}

        // protected state
        private int _activeTasks; // count of this and child tasks queued/running

        // constructors
        internal SequencerNode(Sequencer sequencer, int level, SequencerNode parent)
        {
            _sequencer = sequencer;
            if (level < 0)
                throw new ArgumentException("Invalid", nameof(level));
            _level = level;
            if (level == 0 && parent != null)
                throw new ArgumentException("Invalid", nameof(parent));
            if (level > 0 && parent == null)
                throw new ArgumentNullException(nameof(parent));
            Parent = parent;
            _queue = new Queue<SequencerTask>();
            _childIndex = new Dictionary<string, SequencerNode>();
        }

        internal void BuildNodeList(int level, SequencerNode[] nodeList, string[] keyParts)
        {
            //Lock();
            //try
            lock(this)
            {
                // find/create sub node
                nodeList[_level] = this;
                if (!_childIndex.TryGetValue(keyParts[_level], out var subNode))
                {
                    subNode = new SequencerNode(_sequencer, _level + 1, this);
                    _childIndex.Add(keyParts[_level], subNode);
                }
                nodeList[_level + 1] = subNode;
                if (level > (_level + 1))
                {
                    subNode.BuildNodeList(level, nodeList, keyParts);
                }
                for (int i = 1; i <= level; i++)
                {
                    Debug.Assert(nodeList[i].Parent == nodeList[i - 1]);
                }
            }
            //finally
            //{
            //    Unlock();
            //}
        }

        internal void Enqueue(SequencerTask task)
        {
            //Lock();
            //try
            lock(this)
            {
                if (_queue.Count > 0)
                {
                    // queue not empty - enqueue and get out 
                    // - another thread will dequeue us later
                    _queue.Enqueue(task);
                    return;
                }
                if ((task.Level == _level) && (_activeTasks > 0))
                {
                    // cannot run yet - enqueue and get out 
                    // - another thread will dequeue us later
                    _queue.Enqueue(task);
                    return;
                }

                // node quiet - dispatch task 
                DispatchTask(task);
            }
            //finally
            //{
            //    Unlock();
            //}
        }

        private void SequencedCallback(object state)
        {
            _sequencer.DecOutstanding();
            SequencerTask task = (SequencerTask)state;
            WaitCallback userCallback = task.UserCallback;
            object userContext = task.UserContext;
            try
            {
                userCallback(userContext);
            }
            catch(Exception excp)
            {
                _sequencer.SetLastException(excp);
            }
            Debug.Assert(task.NodeList != null);
            Debug.Assert(task.Level == _level);
            Debug.Assert(task.NodeList.Length == (task.Level + 1));
            Debug.Assert(task.NodeList[task.Level] == this);
            // task cleanup
            DecActiveTasks();
            // now dispatch any waiting tasks
            Dequeue();
        }

        internal void DecActiveTasks()
        {
            Interlocked.Decrement(ref _activeTasks);
            Parent?.DecActiveTasks();
        }

        internal void Dequeue()
        {
            // we can dispatch the head of the queue if
            // there are no active tasks
            if (_activeTasks > 0)
            {
                // node busy - get out 
                // - another thread will come by later
                return;
            }
            // node quiet - we can dequeue some tasks
            int dispatchCount = 0;
            //Lock();
            //try
            lock(this)
            {
                while (_queue.Count > 0)
                {
                    SequencerTask task = _queue.Peek();
                    if (task.Level == _level)
                    {
                        // we can only dispatch if no active tasks already
                        if (_activeTasks > 0)
                            return;
                    }
                    DispatchTask(task);
                    dispatchCount += 1;
                    _queue.Dequeue();
                }
            }
            //finally
            //{
            //    Unlock();
            //}
            // if nothing dispatched - try the parent
            if (dispatchCount == 0)
                Parent?.Dequeue();

        }

        private void DispatchTask(SequencerTask task)
        {
            Interlocked.Increment(ref _activeTasks);
            if (task.Level == _level)
            {
                // dispatch
                ThreadPool.QueueUserWorkItem(SequencedCallback, task);
            }
            else
            {
                // enqueue to child
                Debug.Assert(task.Level > _level);
                SequencerNode subNode = task.NodeList[_level + 1];
                subNode.Enqueue(task);
            }
        }
    }
}
