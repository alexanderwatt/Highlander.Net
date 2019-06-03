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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orion.Util.Caching;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Threading;

namespace Core.Common
{
    // helper classes
    public class CacheChangeData
    {
        public readonly Guid CacheId;
        public readonly CacheChange Change;
        public readonly ICoreItem OldItem;
        public readonly ICoreItem NewItem;
        public CacheChangeData(Guid cacheId, CacheChange change, ICoreItem oldItem, ICoreItem newItem)
        {
            CacheId = cacheId;
            Change = change;
            OldItem = oldItem;
            NewItem = newItem;
        }
    }

    public delegate void CacheChangeHandler(CacheChangeData update);

    public class CacheState
    {
        public readonly Dictionary<string, ICoreItem> ItemDict = new Dictionary<string, ICoreItem>();
        public int CreateCount;
        public int UpdateCount;
        public int DeleteCount;
    }

    public class CacheSubscription
    {
        public readonly ISubscription Subscription;
        public readonly CoreItemCacheParams CacheParams;
        public readonly SubscriptionCallback UserCallback;
        public readonly object UserContext;
        public CacheSubscription(ISubscription subscription, CoreItemCacheParams cacheParams, SubscriptionCallback userCallback, object userContext)
        {
            Subscription = subscription;
            CacheParams = cacheParams;
            UserCallback = userCallback;
            UserContext = userContext;
        }
    }

    public class CoreItemCacheParams
    {
        public readonly Type DataType; // the type of the core item Data (body)
        public readonly Guid SubscriptionId; // optional subscription id
        public CoreItemCacheParams(Type dataType, Guid subscriptionId)
        {
            DataType = dataType;
            SubscriptionId = subscriptionId;
        }
    }

    internal class WriteBehindQueue
    {
        private readonly int _maxQueueLength;
        private readonly Guarded<Queue<IAsyncResult>> _queue = new Guarded<Queue<IAsyncResult>>(new Queue<IAsyncResult>());

        public WriteBehindQueue(int maxQueueLength)
        {
            _maxQueueLength = maxQueueLength;
        }

        public void Enqueue(IAsyncResult asyncOperation)
        {
            _queue.Locked(queue =>
            {
                queue.Enqueue(asyncOperation);
                while (queue.Count > _maxQueueLength)
                {
                    IAsyncResult ar = queue.Dequeue();
                    ((AsyncResultNoResult)ar).EndInvoke();
                }
            });
        }

        public IAsyncResult Synchronise(IAsyncResult blockingOperation)
        {
            IAsyncResult result = null;
            _queue.Locked(queue =>
            {
                queue.Enqueue(blockingOperation);
                while (queue.Count > 1)
                {
                    IAsyncResult ar = queue.Dequeue();
                    ((AsyncResultNoResult)ar).EndInvoke();
                }
                result = queue.Dequeue();
            });
            return result;
        }
    }

    public class CoreItemCache : CacheBase<string, ICoreItem, CoreItemCacheParams>
    {
        private readonly Guid _cacheId;
        private readonly ICoreClient _client;
        private readonly WriteBehindQueue _writeBehindQueue = new WriteBehindQueue(20);
        private readonly GuardedList<CacheChangeData> _updates = new GuardedList<CacheChangeData>();

        public CoreItemCache(Guid cacheId, ICoreClient client)
        {
            _cacheId = cacheId;
            _client = client;
        }

        protected override string OnGetKey(string userKey)
        {
            return userKey.ToLower();
        }

        protected override ICoreItem OnLoad(string key, CoreItemCacheParams userParams)
        {
            IAsyncResult ar = _writeBehindQueue.Synchronise(_client.LoadItemBegin(null, userParams.DataType, key, true));
             return _client.LoadItemEnd(ar);
        }

        protected override void OnSave(ICoreItem oldItem, ICoreItem newItem, CoreItemCacheParams notUsed)
        {
            // note: cache removal != server store delete
            if (newItem != null)
            {
                _writeBehindQueue.Enqueue(_client.SaveItemBegin(newItem));
            }
        }

        // counters and update tracking
        private int _itemsCreated;
        public int CreateCount => Interlocked.Add(ref _itemsCreated, 0);
        private int _itemsUpdated;
        public int UpdateCount => Interlocked.Add(ref _itemsUpdated, 0);
        private int _itemsDeleted;
        public int DeleteCount => Interlocked.Add(ref _itemsDeleted, 0);
        public int ItemCount => (Interlocked.Add(ref _itemsCreated, 0) - Interlocked.Add(ref _itemsDeleted, 0));

        public List<CacheChangeData> GetUpdates()
        {
            return _updates.Clear();
        }

        protected override void OnUpdate(CacheChange change, string userKey, ICoreItem oldValue, ICoreItem newValue, CoreItemCacheParams userParam)
        {
            switch (change)
            {
                case CacheChange.CacheCleared:
                    if (oldValue != null)
                        throw new ArgumentException("oldValue");
                    if (newValue != null)
                        throw new ArgumentException("newValue");
                    _updates.Add(new CacheChangeData(_cacheId, change, null, null));
                    break;
                case CacheChange.ItemCreated:
                    if (oldValue != null)
                        throw new ArgumentException("oldValue");
                    if (newValue == null)
                        throw new ArgumentException("newValue");
                    _updates.Add(new CacheChangeData(_cacheId, change, null, newValue));
                    Interlocked.Increment(ref _itemsCreated);
                    break;
                case CacheChange.ItemExpired:
                case CacheChange.ItemRemoved:
                    if (oldValue == null)
                        throw new ArgumentException("oldValue");
                    if (newValue != null)
                        throw new ArgumentException("newValue");
                    _updates.Add(new CacheChangeData(_cacheId, change, oldValue, null));
                    Interlocked.Increment(ref _itemsDeleted);
                    break;
                case CacheChange.ItemUpdated:
                    if (oldValue == null)
                        throw new ArgumentException("oldValue");
                    if (newValue == null)
                        throw new ArgumentException("newValue");
                    if (newValue.Id != oldValue.Id)
                    {
                        _updates.Add(new CacheChangeData(_cacheId, change, oldValue, newValue));
                        Interlocked.Increment(ref _itemsUpdated);
                    }
                break;
            }
        }
    }

    public class CoreCache : ICoreCache
    {
        protected readonly Guid CacheId = Guid.NewGuid();
        protected readonly ILogger _Logger;
        protected ICoreClient Client;
        protected readonly SynchronizationContext SyncContext;
        protected readonly AsyncThreadQueue UserThreadQueue;
        protected readonly Timer CacheItemPurgeTimer;
        // managed state
        protected readonly GuardedDictionary<Guid, CacheSubscription> _Subscriptions = new GuardedDictionary<Guid, CacheSubscription>();
        protected readonly CoreItemCache Cache;
        // events
        public event CacheChangeHandler OnDataChange;

        public CoreCache(
            ILogger logger, 
            ICoreClient client,
            CacheChangeHandler dataChangeHandler, 
            SynchronizationContext syncContext)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            SyncContext = syncContext;
            Cache = new CoreItemCache(CacheId, Client);
            UserThreadQueue = new AsyncThreadQueue(_Logger);
            CacheItemPurgeTimer = new Timer(CacheItemPurgeTimeout, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            if (dataChangeHandler != null)
                OnDataChange += dataChangeHandler;
        }

        public void Dispose()
        {
            CacheItemPurgeTimer.Dispose();
            UnsubscribeAll();
            UserThreadQueue.Dispose();
            OnDataChange = null;
        }

        // logger
        public ILogger Logger => _Logger;

        // info properties
        public IModuleInfo ClientInfo => Client.ClientInfo;

        // proxy
        public ICoreClient Proxy => Client;

        public List<ISubscription> Subscriptions
        {
            get
            {
                return _Subscriptions.GetValues().Select(item => item.Subscription).ToList();
            }
        }
        
        // housekeeping
        private void CacheItemPurgeTimeout(object notUsed)
        {
            Cache.Purge();
            NotifyUserDataChange(Cache.GetUpdates());
        }

        // subscriptions
        private ISubscription SubscribePrivate(
            Type dataType, IExpression filter, 
            bool excludeExisting, bool waitForExisting, bool excludeDataBody, 
            SubscriptionCallback userCallback, object userContext)
        {
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            // create subscription
            ISubscription subscription = Client.CreateTypedSubscription(dataType, filter);
            subscription.UserCallback = SubscriptionCallback;
            subscription.UserContext = null;
            subscription.ItemKind = ItemKind.Object;
            subscription.ExcludeExisting = excludeExisting;
            subscription.WaitForExisting = waitForExisting;
            subscription.ExcludeDataBody = excludeDataBody;
            var userParams = new CoreItemCacheParams(dataType, subscription.Id);
            _Subscriptions.Set(subscription.Id, new CacheSubscription(subscription, userParams, userCallback, userContext));
            // hack - todo - do initial load if required (remove when server supports waitForExisting)
            const long minimumUSN = 0;
            if (!excludeExisting)
            {
                List<ICoreItem> items = Client.LoadItems(dataType, ItemKind.Object, filter, 0, true);
                foreach (var newItem in items)
                {
                    Cache.Put(newItem.Name, newItem, LoadSaveType.Avoid, userParams, TimeSpan.MaxValue);
                }
                // notify user
                NotifyUserDataChange(Cache.GetUpdates());
            }
            // start subscription
            subscription.MinimumUSN = minimumUSN;
            //_Logger.LogDebug("Cache: Creating subscription[{0}]: <{1}> {2} ({3})",
            //    filter.GetHashCode(), dataType.FullName,
            //    filter.DisplayString(), subscription.Id);
            subscription.Start();
            return subscription;
        }

        public ISubscription Subscribe(Type dataType, IExpression whereExpr, SubscriptionCallback userCallback, object userContext)
        {
            return SubscribePrivate(dataType, whereExpr, false, true, false, userCallback, userContext);
        }

        public ISubscription Subscribe<T>(IExpression whereExpr, SubscriptionCallback userCallback, object userContext)
        {
            return SubscribePrivate(typeof(T), whereExpr, false, true, false, userCallback, userContext);
        }

        public ISubscription Subscribe<T>(IExpression filter)
        {
            return SubscribePrivate(typeof(T), filter, false, true, false, null, null);
        }

        public ISubscription SubscribeNoWait<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            return SubscribePrivate(typeof(T), filter, false, false, false, userCallback, userContext);
        }

        public ISubscription SubscribeInfoOnly<T>(IExpression filter)
        {
            return SubscribePrivate(typeof(T), filter, false, false, true, null, null);
        }

        public ISubscription SubscribeNewOnly<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            return SubscribePrivate(typeof(T), filter, true, false, false, userCallback, userContext);
        }

        // ICoreClient subscription methods
        public void Unsubscribe(Guid subscriptionId)
        {
            Client.Unsubscribe(subscriptionId);
            _Subscriptions.Remove(subscriptionId);
            var cacheItems = Cache.GetCacheItems();
            foreach (var cacheItem in cacheItems)
            {
                if (cacheItem.UserParam.SubscriptionId == subscriptionId)
                {
                    Cache.Remove(cacheItem.UserKey, cacheItem.UserParam);
                }
            }
        }

        public void UnsubscribeAll()
        {
            foreach (Guid subscriptionId in _Subscriptions.GetKeys())
                Unsubscribe(subscriptionId);
        }

        private void EnsureSubscribed(Type dataType, IExpression whereExpr)
        {
            // check for existing equivalent or global subscription
            // if not found, add it and start a new subscription
            string globalSubscription = Expr.ALL.ToString();
            foreach (CacheSubscription cacheSubs in _Subscriptions.GetValues())
            {
                if (cacheSubs.CacheParams.DataType == dataType)
                {
                    if (cacheSubs.Subscription.WhereExpr.ToString() == globalSubscription)
                        return;
                    if (cacheSubs.Subscription.WhereExpr.GetHashCode() == whereExpr.GetHashCode())
                        return;
                }
            }
            // not found
            SubscribePrivate(dataType, whereExpr, false, true, false, null, null);
        }

        // ------------------------------ load single item methods ------------------------------
        private ICoreItem LoadItem(string itemName, CoreItemCacheParams cacheParams)
        {
            ICoreItem result = Cache.Get(itemName, LoadSaveType.Default, cacheParams);
            if (result != null)
            {
                if (!result.IsCurrent())
                {
                    result = null;
                }
            }
            NotifyUserDataChange(Cache.GetUpdates());
            return result;
        }

        public ICoreItem LoadItem(Type dataType, string itemName)
        {
            return LoadItem(itemName, new CoreItemCacheParams(dataType, Guid.Empty));
        }

        public ICoreItemInfo LoadItemInfo<T>(string name) { return LoadItem(typeof(T), name); }

        public ICoreItemInfo LoadItemInfo(Type dataType, string name) { return LoadItem(dataType, name); }

        public ICoreItem LoadItem<T>(string itemName)
        {
            return LoadItem(itemName, new CoreItemCacheParams(typeof(T), Guid.Empty));
        }

        public T LoadObject<T>(string itemName)
        {
            ICoreItem item = LoadItem(itemName, new CoreItemCacheParams(typeof(T), Guid.Empty));
            if (item != null)
                return (T)item.Data;
            return default(T);
        }

        // ------------------------------ load multiple items methods ------------------------------
        public List<ICoreItem> LoadItems<T>(IEnumerable<string> itemNames)
        {
            var cacheParams = new CoreItemCacheParams(typeof(T), Guid.Empty);
            return itemNames.Select(itemName => LoadItem(itemName, cacheParams)).Where(item => item != null).ToList();
        }

        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr)
        {
            EnsureSubscribed(dataType, whereExpr);
            return GetLocalItems(dataType, whereExpr, false);
        }

        public List<ICoreItem> LoadItems<T>(IExpression whereExpr)
        {
            return LoadItems(typeof(T), whereExpr);
        }

        public List<ICoreItemInfo> LoadItemInfos(Type dataType, IExpression whereExpr)
        {
            return LoadItems(dataType, whereExpr).Cast<ICoreItemInfo>().ToList();
        }

        public List<ICoreItemInfo> LoadItemInfos<T>(IExpression whereExpr)
        {
            return LoadItemInfos(typeof(T), whereExpr);
        }

        public List<T> LoadObjects<T>(IExpression whereExpr)
        {
            return LoadItems(typeof (T), whereExpr).Select(item => (T) item.Data).ToList();
        }

        // ------------------------------ make item methods ------------------------------
        public ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            return Client.MakeItem(data, name, props, transient, expires);
        }

        public ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            return Client.MakeItem(data, name, props, transient, lifetime);
        }

        // ------------------------------ save item methods ------------------------------
        public Guid SaveItem(ICoreItem newItem)
        {
            if (newItem.DataType == null)
                throw new ArgumentNullException("dataType");
            newItem.Freeze();
            Cache.Put(newItem.Name, newItem, new CoreItemCacheParams(newItem.DataType, Guid.Empty));
            NotifyUserDataChange(Cache.GetUpdates());
            return newItem.Id;
        }

        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props)
        {
            ICoreItem item = Client.MakeTypedItem(dataType, data, name, props, false);
            return SaveItem(item);
        }

        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            ICoreItem item = Client.MakeTypedItem(dataType, data, name, props, transient);
            item.Expires = expires;
            return SaveItem(item);
        }

        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            ICoreItem item = Client.MakeTypedItem(dataType, data, name, props, transient);
            item.Lifetime = lifetime;
            return SaveItem(item);
        }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props)
        {
            return SaveTypedObject(typeof(T), data, name, props, false, TimeSpan.MaxValue);
        }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props, DateTimeOffset expires)
        {
            return SaveTypedObject(typeof(T), data, name, props, false, expires);
        }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props, TimeSpan lifetime)
        {
            return SaveTypedObject(typeof(T), data, name, props, false, lifetime);
        }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            return SaveTypedObject(typeof(T), data, name, props, transient, expires);
        }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            return SaveTypedObject(typeof(T), data, name, props, transient, lifetime);
        }

        public Guid SaveObject<T>(T data) where T : ICoreObject
        {
            return SaveObject(data, data.NetworkKey, data.AppProperties, data.IsTransient, data.Lifetime);
        }

        public Guid SaveObject<T>(T data, TimeSpan lifetime) where T : ICoreObject
        {
            return SaveObject(data, data.NetworkKey, data.AppProperties, data.IsTransient, lifetime);
        }

        // delete methods
        public Guid DeleteItem(ICoreItem item)
        {
            return SaveItem(Client.MakeItemFromText(item.DataTypeName, null, item.Name, item.AppProps));
        }

        public int DeleteTypedObjects(Type dataType, IExpression whereExpr)
        {
            List<ICoreItem> items = LoadItems(dataType, whereExpr);
            foreach (var item in items)
            {
                SaveTypedObject(dataType, null, item.Name, item.AppProps, false, TimeSpan.Zero);
            }
            return items.Count;
        }

        public int DeleteObjects<T>(IExpression whereExpr)
        {
            return DeleteTypedObjects(typeof(T), whereExpr);
        }

        // thread-safe methods
        public int ItemCount => Cache.ItemCount;
        public int CreateCount => Cache.CreateCount;
        public int UpdateCount => Cache.UpdateCount;
        public int DeleteCount => Cache.DeleteCount;

        public List<ICoreItem> Items
        {
            get
            {
                var result = Cache.GetCacheItems().Select(item => Cache.Get(item.UserKey)).Where(value => value != null).ToList();
                NotifyUserDataChange(Cache.GetUpdates());
                return result;
            }
        }

        private List<ICoreItem> GetLocalItems(Type dataType, IExpression whereExpr, bool includeDeleted)
        {
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));
            if (whereExpr == null)
                throw new ArgumentNullException(nameof(whereExpr));
            var results = new List<ICoreItem>();
            List<ICoreItem> items = Cache.GetValues();
            foreach (var item in items)
            {
                if (dataType.FullName != null && item != null && (includeDeleted || item.IsCurrent()) && dataType.FullName.Equals(item.DataTypeName) && whereExpr.MatchesProperties(new ExprContext(item.AppProps, item.Name, item.Created, item.Expires)))
                {
                    results.Add(item);
                }
            }
            NotifyUserDataChange(Cache.GetUpdates());
            _Logger.LogDebug("GetLocalItems<{0}>('{1}') returned {2} items.", dataType.Name, whereExpr, results.Count);
            return results;
        }

        public void Clear()
        {
            Cache.Clear();
            NotifyUserDataChange(Cache.GetUpdates());
        }

        //private void ProcessNewItem(List<CacheChangeData> updates, ICoreItem newItem)
        //{
        //    // additions, updates and deletions
        //    _CacheState.Locked((state) =>
        //    {
        //        string key = newItem.Name.ToLower();
        //        ICoreItem oldItem = null;
        //        if (state.ItemDict.TryGetValue(key, out oldItem))
        //        {
        //            // already exists in set
        //            if (newItem.IsCurrent())
        //            {
        //                // update
        //                state.ItemDict[key] = newItem;
        //                // check if really updated
        //                if (newItem.Id != oldItem.Id)
        //                {
        //                    // item updated
        //                    state.UpdateCount++;
        //                    updates.Add(new CacheChangeData(_CacheId, CacheChange.ItemUpdated, oldItem, newItem));
        //                }
        //            }
        //            else
        //            {
        //                // delete
        //                state.ItemDict.Remove(key);
        //                state.DeleteCount++;
        //                updates.Add(new CacheChangeData(_CacheId, CacheChange.ItemDeleted, oldItem, null));
        //            }
        //        }
        //        else
        //        {
        //            // not found in set
        //            if (newItem.IsCurrent())
        //            {
        //                state.CreateCount++;
        //                state.ItemDict[key] = newItem;
        //                updates.Add(new CacheChangeData(_CacheId, CacheChange.ItemCreated, null, newItem));
        //            }
        //            else
        //            {
        //                // ignore - deletion
        //            }
        //        }
        //    });
        //}

        private void SubscriptionCallback(ISubscription subscription, ICoreItem newItem)
        {
            CacheSubscription subs = _Subscriptions.Get(subscription.Id);
            if (subs != null)
            {
                Cache.Put(newItem.Name, newItem, LoadSaveType.Avoid, subs.CacheParams, TimeSpan.MaxValue);
                NotifyUserDataChange(Cache.GetUpdates());

                // call user's subscription callback if provided
                if (subs.UserCallback != null)
                {
                    try
                    {
                        subs.UserCallback(subscription, newItem);
                    }
                    catch (Exception e)
                    {
                        _Logger.LogError("Subscription '{0}' UserCallback() failed: {1}", subscription.Id, e);
                    }
                }
            }
        }

        // data update notifications
        private void NotifyUserDataChange(List<CacheChangeData> updates)
        {
            if (SyncContext != null)
                SyncContext.Post(UserCallbackDataChange, updates);
            else
                UserThreadQueue.Dispatch(updates, UserCallbackDataChange);
        }
        private void UserCallbackDataChange(object state) { UserCallbackDataChange((List<CacheChangeData>)state); }
        private void UserCallbackDataChange(List<CacheChangeData> updates, long notUsed) { UserCallbackDataChange(updates); }
        private void UserCallbackDataChange(List<CacheChangeData> updates)
        {
            foreach (var update in updates)
                try
                {
                    OnDataChange?.Invoke(update);
                }
                catch (Exception e)
                {
                    _Logger.Log(e);
                }
        }

        // private (in-process only) objects
        public Guid SavePrivateObject<T>(T data, string name, NamedValueSet props)
        {
            ICoreItem item = Client.MakeTypedItem(typeof(T), data, name, props, true);
            //item.Lifetime = TimeSpan.MaxValue;
            item.Expires = DateTimeOffset.MaxValue - TimeSpan.FromDays(1);
            Cache.Put(name, item, LoadSaveType.Avoid, new CoreItemCacheParams(typeof(T), Guid.Empty), TimeSpan.MaxValue);
            NotifyUserDataChange(Cache.GetUpdates());
            return item.Id;
        }

        public ICoreItem LoadPrivateItem<T>(string name)
        {
            ICoreItem result = Cache.Get(name, LoadSaveType.Avoid, new CoreItemCacheParams(typeof(T), Guid.Empty));
            if (result != null)
            {
                if (!result.IsCurrent())
                {
                    result = null;
                }
            }
            NotifyUserDataChange(Cache.GetUpdates());
            return result;
        }

        public List<ICoreItem> LoadPrivateItems<T>(IExpression whereExpr)
        {
            return GetLocalItems(typeof(T), whereExpr, false);
        }
    }
}
