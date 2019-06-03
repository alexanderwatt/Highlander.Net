/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Orion.Util.Threading
{
    /// <summary>
    /// Delegate method for use with Guarded<T/>.Locked() method.
    /// </summary>
    public delegate void ProtectedSection<T>(T lockedObject);

    /// <summary>
    /// Guards access to an object with a non-blocking spinlock, runs user code in a protected section, 
    /// and disposes the object if it supports IDisposable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Guarded<T> : IDisposable where T : class
    {
        private static readonly bool _IsSingleCore = (Environment.ProcessorCount == 1);
        // debug
        private static long _debugSleepCount;
        private class LocalCounter
        {
            private long _counter;
            public long Value => Interlocked.Add(ref _counter, 0);
            public long Increment() { return Interlocked.Add(ref _counter, 1); }
            public long Decrement() { return Interlocked.Add(ref _counter, -1); }
        }
        private static readonly Dictionary<int, LocalCounter> DebugThreadLockCounter = new Dictionary<int, LocalCounter>();
        // end debug

        private bool _disposed;
        private T _target;
        public Guarded(T target)
        {
            _target = target ?? throw new ArgumentNullException("target");
        }
        private T Enter()
        {
            // check if disposed
            if (_disposed)
                throw new InvalidOperationException(
                    $"Guarded<{typeof(T).Name}>.Locked: Object disposed!");
            // debug temp todo remove - do not allow nested locks (thereby preventing deadlocking)
            LocalCounter threadLockCounter;
            int threadId = Thread.CurrentThread.ManagedThreadId;
            lock(DebugThreadLockCounter)
            {
                if (!DebugThreadLockCounter.TryGetValue(threadId, out threadLockCounter))
                {
                    threadLockCounter = new LocalCounter();
                    DebugThreadLockCounter[threadId] = threadLockCounter;
                }
            }
            long nested = threadLockCounter.Increment();
            // end debug
            try
            {
                if (nested > 1)
                    throw new InvalidOperationException("Attempted secondary lock acquisition!");

                int spinCount = 0;
                T lockedObject;
                while ((lockedObject = Interlocked.Exchange(ref _target, null)) == null)
                {
                    // spin - todo - in .Net 4.0 use Thread.SpinWait()
                    bool sleep = false;
                    if (_IsSingleCore)
                        sleep = true;
                    else
                    {
                        // black magic from JeffreyR@Wintellect.com
                        spinCount++;
                        // debug
                        if (spinCount % 4000 == 0)
                            sleep = true;
                        if (spinCount % 1000000000 == 0) // yes 1 billion, or about 1 second
                        {
                            // if you see this debug message, it is likely you have a deadlock
                            Debug.WriteLine($"Guarded<{typeof(T).Name}>.Enter: Spin={spinCount}");
                        }
                        // end debug
                    }
                    if (sleep)
                    {
                        // debug
                        long sleepCount = Interlocked.Increment(ref _debugSleepCount);
                        if (sleepCount % 100000 == 0)
                        {
                            // if you see this debug message, but not the message above, it is likely there
                            // is another thread locking the target object for very long periods.
                            Debug.WriteLine(String.Format("Guarded<{0}>.Enter: SleepCount={1}", typeof(T).Name, sleepCount));
                        }
                        // end debug
                        Thread.Sleep(0);
                    }
                }
                return lockedObject;
            }
            finally
            {
                threadLockCounter.Decrement();
            }
        }

        private void Leave(T lockedObject)
        {
            Interlocked.Exchange(ref _target, lockedObject);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userCode"></param>
        public void Locked(ProtectedSection<T> userCode)
        {
            //lock (_Lock)
            //{
            //    // check if disposed
            //    if (_Disposed)
            //        throw new InvalidOperationException(
            //            String.Format("Guarded<{0}>.Locked: Object disposed!", typeof(T).Name));
            //    userCode(_Target);
            //}
            T lockedObject = Enter();
            try
            {
                userCode(lockedObject);
            }
            finally
            {
                Leave(lockedObject);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            //lock (_Lock)
            //{
            //    // return if already disposed
            //    if (_Disposed)
            //        return;
            //    _Disposed = true;
            //    IDisposable disposable = _Target as IDisposable;
            //    if (disposable != null)
            //        disposable.Dispose();
            //}
            // acquire the object for the last time
            T lockedObject = Enter();
            try
            {
                _disposed = true;
                if (lockedObject is IDisposable disposable)
                    disposable.Dispose();
            }
            catch (Exception excp)
            {
                Debug.WriteLine($"Guarded<{typeof(T).Name}>.Dispose: {excp.GetType().Name}");
            }
            finally
            {
                Leave(lockedObject);
            }
            _target = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GuardedList<T>
    {
        private readonly Guarded<List<T>> _list = new Guarded<List<T>>(new List<T>());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            _list.Locked(list => list.Add(item));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            var result = new List<T>();
            _list.Locked(result.AddRange);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<T> Clear()
        {
            var result = new List<T>();
            _list.Locked(list => 
            { 
                result.AddRange(list);
                list.Clear();
            });
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class GuardedDictionary<TKey, TValue>
    {
        private readonly Guarded<Dictionary<TKey, TValue>> _dictionary =
            new Guarded<Dictionary<TKey, TValue>>(new Dictionary<TKey, TValue>());

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                int result = 0;
                _dictionary.Locked(dict => result = dict.Count);
                return result;
            }
        }

        /// <summary>
        /// Gets the specified value, or null if it does not exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            // return current value (if any)
            TValue oldValue = default(TValue);
            _dictionary.Locked(dict =>
            {
                TValue tempValue = default(TValue);
                dict.TryGetValue(key, out tempValue);
                oldValue = tempValue;
            });
            return oldValue;
        }

        /// <summary>
        /// Sets the specified value, and returns the old value if it existed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        public TValue Set(TKey key, TValue newValue)
        {
            // set new value, return old value (if any)
            TValue oldValue = default(TValue);
            _dictionary.Locked(dict =>
            {
                TValue tempValue = default(TValue);
                dict.TryGetValue(key, out tempValue);
                dict[key] = newValue;
                oldValue = tempValue;
            });
            return oldValue;
        }

        /// <summary>
        /// Removes the specified value, and returns the old value if it existed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TValue Remove(TKey key)
        {
            TValue oldValue = default(TValue);
            _dictionary.Locked(dict =>
            {
                TValue tempValue = default(TValue);
                if (dict.TryGetValue(key, out tempValue))
                {
                    dict.Remove(key);
                }
                oldValue = tempValue;
            });
            return oldValue;
        }

        /// <summary>
        /// Tries to get the current value, and returns true if it exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public bool TryGet(TKey key, out TValue result)
        {
            bool found = false;
            TValue tempResult = default(TValue);
            _dictionary.Locked(dict =>
            {
                found = dict.TryGetValue(key, out var tempValue);
                tempResult = tempValue;
            });
            result = tempResult;
            return found;
        }

        /// <summary>
        /// Tries to get the current value, and returns true if it existed. If not found, sets the new value.
        /// </summary>
        /// <returns></returns>
        public delegate TValue LazyCreate();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public TValue GetOrSet(TKey key, LazyCreate constructor)
        {
            TValue result = default(TValue);
            _dictionary.Locked(dict =>
            {
                if (!dict.TryGetValue(key, out var tempValue))
                {
                    TValue newValue = constructor();
                    dict[key] = newValue;
                    tempValue = newValue;
                }
                result = tempValue;
            });
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TKey> GetKeys()
        {
            List<TKey> result = null;
            _dictionary.Locked(dict =>
            {
                result = dict.Keys.ToList();
            });
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        public List<TValue> GetValues(bool reset)
        {
            List<TValue> result = null;
            _dictionary.Locked(dict =>
            {
                result = dict.Values.ToList();
                if (reset)
                    dict.Clear();
            });
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TValue> GetValues()
        {
            return GetValues(false);
        }
    }

    // general object spinlock 
    // - less safe than Guarded<T> (inifinite spin possible when target is not initialised)
    // - uncomment if really needed
    public static class ObjectLock<T> where T : class
    {
        private static readonly bool IsSingleCore = (Environment.ProcessorCount == 1);
        // debug
        private static long _debugSleepCount;
        // end debug

        /// <summary>
        /// static patterns
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T Enter(ref T target)
        {
            int spinCount = 0;
            T lockedObject;
            while ((lockedObject = Interlocked.Exchange(ref target, null)) == null)
            {
                // spin - todo - in .Net 4.0 use Thread.SpinWait()
                bool sleep = false;
                if (IsSingleCore)
                    sleep = true;
                else
                {
                    // black magic from JeffreyR@Wintellect.com
                    spinCount++;
                    if (spinCount % 4000 == 0)
                        sleep = true;
                }
                if (sleep)
                {
                    // debug
                    long sleepCount = Interlocked.Increment(ref _debugSleepCount);
                    if (sleepCount % 10000 == 0)
                        Debug.WriteLine($"SleepCount={sleepCount}");
                    // end debug
                    Thread.Sleep(0);
                }
            }
            return lockedObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="lockedObject"></param>
        public static void Leave(ref T target, T lockedObject)
        {
            Interlocked.Exchange(ref target, lockedObject);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="userCode"></param>
        public static void Protect(ref T target, ProtectedSection<T> userCode)
        {

            T lockedObject = Enter(ref target);
            try
            {
                userCode(lockedObject);
            }
            finally
            {
                Leave(ref target, lockedObject);
            }
        }
    }
}
