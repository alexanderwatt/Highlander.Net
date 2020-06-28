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

namespace Highlander.Utilities.Caching
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
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    /// <typeparam name="TU"></typeparam>
    public class CacheItem<TK, TV, TU>
    {
        public readonly TK UserKey;
        public readonly TV Value;
        public readonly TU UserParam;
        public readonly DateTimeOffset Expires;
        public CacheItem(TK userKey, TV value, TU userParam, DateTimeOffset expires)
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
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    /// <typeparam name="TU"></typeparam>
    public class CacheState<TK, TV, TU> where TV : class
    {
        public readonly Dictionary<TK, CacheItem<TK, TV, TU>> Cache = new Dictionary<TK, CacheItem<TK, TV, TU>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    /// <typeparam name="TU"></typeparam>
    public class CacheBase<TK, TV, TU> where TV : class
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
            return now.Add(cacheDuration > maxDuration ? maxDuration : cacheDuration);
        }

        protected Guarded<CacheState<TK, TV, TU>> CacheState =
            new Guarded<CacheState<TK, TV, TU>>(new CacheState<TK, TV, TU>());

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TV> GetValues()
        {
            return GetValues(LoadSaveType.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadOption"></param>
        /// <returns></returns>
        public List<TV> GetValues(LoadSaveType loadOption)
        {
            var items = new List<CacheItem<TK, TV, TU>>();
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
        public List<TK> GetKeys()
        {
            var items = new List<CacheItem<TK, TV, TU>>();
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
        public List<CacheItem<TK, TV, TU>> GetCacheItems()
        {
            var result = new List<CacheItem<TK, TV, TU>>();
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
            OnUpdate(CacheChange.CacheCleared, default, null, null, default);
        }

        /// <summary>
        /// Purges the cache of expired items.
        /// </summary>
        public void Purge()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            var purgedKVPs = new List<KeyValuePair<TK,CacheItem<TK,TV,TU>>>();
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
                CacheItem<TK, TV, TU> cacheItem = kvpItem.Value;
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
        protected virtual TK OnGetKey(TK userKey) { return userKey; }

        /// <summary>
        /// Called after the cache has updated an item.
        /// </summary>
        /// <param name="change"> </param>
        /// <param name="userKey"> </param>
        /// <param name="oldValue">The old item.</param>
        /// <param name="newValue">The new item.</param>
        /// <param name="userParam">The userParam.</param>
        protected virtual void OnUpdate(CacheChange change, TK userKey, TV oldValue, TV newValue, TU userParam) { }
        private void ProcessUpdate(TK userKey, TV oldValue, TV newValue, bool expired, TU userParam)
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
        protected virtual TV OnLoad(TK userKey, TU userParam) { return null; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <returns></returns>
        public TV Get(TK userKey)
        {
            return Get(userKey, LoadSaveType.Default, default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="loadOption"></param>
        /// <returns></returns>
        public TV Get(TK userKey, LoadSaveType loadOption)
        {
            return Get(userKey, loadOption, default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="loadOption"></param>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public TV Get(TK userKey, LoadSaveType loadOption, TU userParam)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset expires = CalculateExpiry(now, _defaultLoadCacheDuration);
            if (userKey == null)
                throw new ArgumentNullException(nameof(userKey));
            TK cacheKey = OnGetKey(userKey);
            TV newValue = null;
            TV oldValue = null;
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
                    state.Cache[cacheKey] = new CacheItem<TK, TV, TU>(userKey, newValue, userParam, expires);
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
        protected virtual void OnSave(TV oldValue, TV newValue, TU userParam) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public TV Put(TK userKey, TV newValue)
        {
            return Put(userKey, newValue, LoadSaveType.Default, default, _defaultSaveCacheDuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="newValue"></param>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public TV Put(TK userKey, TV newValue, TU userParam)
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
        public TV Put(TK userKey, TV newValue, LoadSaveType saveOption)
        {
            return Put(userKey, newValue, saveOption, default, _defaultSaveCacheDuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="newValue"></param>
        /// <param name="saveOption"></param>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public TV Put(TK userKey, TV newValue, LoadSaveType saveOption, TU userParam)
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
        public TV Put(TK userKey, TV newValue, LoadSaveType saveOption, TU userParam, TimeSpan cacheDuration)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset expires = CalculateExpiry(now, cacheDuration);
            if (userKey == null)
                throw new ArgumentNullException(nameof(userKey));
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));
            TK cacheKey = OnGetKey(userKey);
            TV oldValue = null;
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
                state.Cache[cacheKey] = new CacheItem<TK, TV, TU>(userKey, newValue, userParam, expires);
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
        public bool Remove(TK userKey)
        {
            return Remove(userKey, default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public bool Remove(TK userKey, TU userParam)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            const bool found = false;
            TK cacheKey = OnGetKey(userKey);
            TV newValue = null;
            TV oldValue = null;
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
