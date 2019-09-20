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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Utilities.Threading;

#endregion

namespace Highlander.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public enum CacheChange
    {
        Undefined,
        CacheCleared,
        ItemCreated,
        ItemUpdated,
        ItemRemoved,
        ItemExpired
    }

    /// <summary>
    /// 
    /// </summary>
    public enum LoadSaveType
    {
        Default,    // load/save if required
        Avoid,      // skip any load/save operation
        Force       // force the load/save operation
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class CacheItem<K, V, U>
    {
        public readonly K UserKey;
        public readonly V Value;
        public readonly U UserParam;
        public readonly DateTimeOffset Expires;
        public CacheItem(K userKey, V value, U userParam, DateTimeOffset expires)
        {
            UserKey = userKey;
            Value = value;
            UserParam = userParam;
            Expires = expires;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class CacheState<K, V, U> where V : class
    {
        public readonly Dictionary<K, CacheItem<K, V, U>> Cache = new Dictionary<K, CacheItem<K, V, U>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class CacheBase<K, V, U> where V : class
    {
        // K the userKey type
        // V the reference type to be cached
        // U user-defined parameter type

        private TimeSpan _defaultLoadCacheDuration = TimeSpan.FromSeconds(120);
        private TimeSpan _defaultSaveCacheDuration = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the default duration for loaded items to be cached. Must be >= zero. Initial value is 2 minutes.
        /// </summary>
        /// <value>The default duration of the cache.</value>
        public TimeSpan DefaultLoadCacheDuration
        {
            get => _defaultLoadCacheDuration;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentException("DefaultLoadCacheDuration");
                _defaultLoadCacheDuration = value;
            }
        }

        /// <summary>
        /// Gets or sets the default duration for saved items to be cached. Must be >= zero. Initial value is 10 seconds.
        /// </summary>
        /// <value>The default duration of the cache.</value>
        public TimeSpan DefaultSaveCacheDuration
        {
            get => _defaultSaveCacheDuration;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentException("DefaultSaveCacheDuration");
                _defaultSaveCacheDuration = value;
            }
        }

        private DateTimeOffset CalculateExpiry(DateTimeOffset now, TimeSpan cacheDuration)
        {
            if (cacheDuration <= TimeSpan.Zero)
                throw new ArgumentException("cacheDuration");
            TimeSpan maxDuration = DateTimeOffset.MaxValue - now - TimeSpan.FromDays(1);
            if (cacheDuration > maxDuration)
                return now.Add(maxDuration);
            return now.Add(cacheDuration);
        }

        protected Guarded<CacheState<K, V, U>> CacheState =
            new Guarded<CacheState<K, V, U>>(new CacheState<K, V, U>());

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<V> GetValues()
        {
            return GetValues(LoadSaveType.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadOption"></param>
        /// <returns></returns>
        public List<V> GetValues(LoadSaveType loadOption)
        {
            var items = new List<CacheItem<K, V, U>>();
            CacheState.Locked(state =>
            {
                items = state.Cache.Values.ToList();
            });
            // call get on each item to check expiry
            return items.Select(item => Get(item.UserKey, loadOption, item.UserParam)).Where(value => value != null).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<K> GetKeys()
        {
            var items = new List<CacheItem<K, V, U>>();
            CacheState.Locked(state =>
            {
                items = state.Cache.Values.ToList();
            });
            return items.Select(item => item.UserKey).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CacheItem<K, V, U>> GetCacheItems()
        {
            var result = new List<CacheItem<K, V, U>>();
            CacheState.Locked(state =>
            {
                result = state.Cache.Values.ToList();
            });
            return result;
        }

        /// <summary>
        /// Clears the cache of all items, both current and expired.
        /// </summary>
        public void Clear()
        {
            CacheState.Locked(state => state.Cache.Clear());
            OnUpdate(CacheChange.CacheCleared, default(K), null, null, default(U));
        }

        /// <summary>
        /// Purges the cache of expired items.
        /// </summary>
        public void Purge()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            var purgedKVPs = new List<KeyValuePair<K,CacheItem<K,V,U>>>();
            CacheState.Locked(state =>
                                   {
                                       purgedKVPs.AddRange(state.Cache.Where(kvpItem => now > kvpItem.Value.Expires));
                                       foreach (var kvpItem in purgedKVPs)
                                       {
                                           state.Cache.Remove(kvpItem.Key);
                                       }
                                   });
            foreach (var kvpItem in purgedKVPs)
            {
                CacheItem<K, V, U> cacheItem = kvpItem.Value;
                if (cacheItem.Value != null)
                    OnUpdate(CacheChange.ItemExpired, cacheItem.UserKey, cacheItem.Value, null, cacheItem.UserParam);
            }
        }

        /// <summary>
        /// Called when the cache needs to convert a user-defined key into an internal
        /// key. (eg. to implement case-insensitive keys). If no override is provided,
        /// the user-defined key is returned unconverted.
        /// </summary>
        /// <param name="userKey">The user key.</param>
        /// <returns></returns>
        protected virtual K OnGetKey(K userKey) { return userKey; }

        /// <summary>
        /// Called after the cache has updated an item.
        /// </summary>
        /// <param name="change"> </param>
        /// <param name="userKey"> </param>
        /// <param name="oldValue">The old item.</param>
        /// <param name="newValue">The new item.</param>
        /// <param name="userParam">The userParam.</param>
        protected virtual void OnUpdate(CacheChange change, K userKey, V oldValue, V newValue, U userParam) { }
        private void ProcessUpdate(K userKey, V oldValue, V newValue, bool expired, U userParam)
        {
            if (newValue != oldValue)
            {
                CacheChange change = CacheChange.ItemUpdated;
                if (oldValue == null)
                    change = CacheChange.ItemCreated;
                if (newValue == null)
                {
                    change = expired ? CacheChange.ItemExpired : CacheChange.ItemRemoved;
                }
                OnUpdate(change, userKey, oldValue, newValue, userParam);
            }
        }
        /// <summary>
        /// Called when the cache needs to load an item.
        /// </summary>
        /// <param name="userKey">The userKey.</param>
        /// <param name="userParam">The userParam.</param>
        /// <returns></returns>
        protected virtual V OnLoad(K userKey, U userParam) { return null; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <returns></returns>
        public V Get(K userKey)
        {
            return Get(userKey, LoadSaveType.Default, default(U));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="loadOption"></param>
        /// <returns></returns>
        public V Get(K userKey, LoadSaveType loadOption)
        {
            return Get(userKey, loadOption, default(U));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="loadOption"></param>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public V Get(K userKey, LoadSaveType loadOption, U userParam)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset expires = CalculateExpiry(now, _defaultLoadCacheDuration);
            if (userKey == null)
                throw new ArgumentNullException(nameof(userKey));
            K cacheKey = OnGetKey(userKey);
            V newValue = null;
            V oldValue = null;
            bool current = false;
            bool expired = false;
            CacheState.Locked(state =>
            {
                if (state.Cache.TryGetValue(cacheKey, out var oldItem))
                {
                    // found but may have expired
                    userParam = oldItem.UserParam;
                    oldValue = oldItem.Value;
                    if (now > oldItem.Expires)
                    {
                        // expired
                        state.Cache.Remove(cacheKey);
                        expired = true;
                    }
                    else
                    {
                        // not expired
                        newValue = oldValue;
                        current = true;
                        // keep original expiry if longer
                        if (oldItem.Expires > expires)
                            expires = oldItem.Expires;
                    }
                }
            });
            if ((loadOption != LoadSaveType.Avoid) &&
                ((loadOption == LoadSaveType.Force) || !current))
            {
                // not current or forced - call load
                newValue = OnLoad(userKey, userParam);
                CacheState.Locked(state =>
                {
                    state.Cache[cacheKey] = new CacheItem<K, V, U>(userKey, newValue, userParam, expires);
                });
            }
            ProcessUpdate(userKey, oldValue, newValue, expired, userParam);
            return newValue;
        }
        /// <summary>
        /// Called when the cache needs to save an updated item.
        /// </summary>
        /// <param name="oldValue">The old item.</param>
        /// <param name="newValue">The new item.</param>
        /// <param name="userParam">The userParam.</param>
        protected virtual void OnSave(V oldValue, V newValue, U userParam) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public V Put(K userKey, V newValue)
        {
            return Put(userKey, newValue, LoadSaveType.Default, default(U), _defaultSaveCacheDuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="newValue"></param>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public V Put(K userKey, V newValue, U userParam)
        {
            return Put(userKey, newValue, LoadSaveType.Default, userParam, _defaultSaveCacheDuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="newValue"></param>
        /// <param name="saveOption"></param>
        /// <returns></returns>
        public V Put(K userKey, V newValue, LoadSaveType saveOption)
        {
            return Put(userKey, newValue, saveOption, default(U), _defaultSaveCacheDuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="newValue"></param>
        /// <param name="saveOption"></param>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public V Put(K userKey, V newValue, LoadSaveType saveOption, U userParam)
        {
            return Put(userKey, newValue, saveOption, userParam, _defaultSaveCacheDuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="newValue"></param>
        /// <param name="saveOption"></param>
        /// <param name="userParam"></param>
        /// <param name="cacheDuration"></param>
        /// <returns></returns>
        public V Put(K userKey, V newValue, LoadSaveType saveOption, U userParam, TimeSpan cacheDuration)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset expires = CalculateExpiry(now, cacheDuration);
            if (userKey == null)
                throw new ArgumentNullException(nameof(userKey));
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));
            K cacheKey = OnGetKey(userKey);
            V oldValue = null;
            bool expired = false;
            CacheState.Locked(state =>
            {
                if (state.Cache.TryGetValue(cacheKey, out var oldItem))
                {
                    // found but may have expired
                    oldValue = oldItem.Value;
                    if (now > oldItem.Expires)
                    {
                        // expired
                        expired = true;
                    }
                    else
                    {
                        // keep original expiry if longer
                        if (oldItem.Expires > expires)
                            expires = oldItem.Expires;
                    }
                }
                state.Cache[cacheKey] = new CacheItem<K, V, U>(userKey, newValue, userParam, expires);
            });
            if ((saveOption != LoadSaveType.Avoid) &&
                ((saveOption == LoadSaveType.Force) || (newValue != oldValue)))
            {
                OnSave(oldValue, newValue, userParam);
            }
            ProcessUpdate(userKey, oldValue, newValue, expired, userParam);
            return oldValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <returns></returns>
        public bool Remove(K userKey)
        {
            return Remove(userKey, default(U));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public bool Remove(K userKey, U userParam)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            const bool found = false;
            K cacheKey = OnGetKey(userKey);
            V newValue = null;
            V oldValue = null;
            bool expired = false;
            CacheState.Locked(state =>
            {
                if (state.Cache.TryGetValue(cacheKey, out var oldItem))
                {
                    // found but may have expired
                    oldValue = oldItem.Value;
                    if (now > oldItem.Expires)
                    {
                        // expired
                        expired = true;
                    }
                }
                state.Cache.Remove(cacheKey);
            });
            ProcessUpdate(userKey, oldValue, newValue, expired, userParam);
            return found;
        }
    }
}
