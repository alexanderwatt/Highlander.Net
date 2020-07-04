/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Highlander.Core.Common;
using Highlander.Utilities;
using Highlander.Utilities.Encryption;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Threading;

#endregion

namespace Highlander.Core.Server
{
    internal class CacheIndexSet
    {
        public readonly Dictionary<string, Dictionary<string, InnerItemRef>> DataTypes = new Dictionary<string, Dictionary<string, InnerItemRef>>();

        public readonly Dictionary<Guid, InnerItemRef> InnerGuids = new Dictionary<Guid, InnerItemRef>();

        public readonly Dictionary<string, InnerItemRef> OuterNames = new Dictionary<string, InnerItemRef>();
    }

    internal class CommonItemComparer : IComparer<CommonItem>
    {
        private readonly IExpression _getValueExpr;

        private readonly IExpression _comparerExpr;

        public CommonItemComparer(IExpression orderExpr)
        {
            _getValueExpr = orderExpr;
            _comparerExpr = Expr.Compare(Expr.Prop("x"), Expr.Prop("y"));
        }

        public int Compare(CommonItem x, CommonItem y)
        {
            var values = new NamedValueSet();
            if (x != null) values.Set("x", _getValueExpr.Evaluate(x.AppProps));
            if (y != null) values.Set("y", _getValueExpr.Evaluate(y.AppProps));
            return Convert.ToInt32(_comparerExpr.Evaluate(values));
        }
    }

    internal delegate void CodeSection();

    internal class CacheEngine : ServerPart
    {
        // readonly state
        private readonly ServerCfg _serverCfg;
        private readonly ICryptoManager _cryptoManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly AsyncThreadQueue _mainCacheDispatcher;
        // item cache and indexes
        private readonly Guarded<CacheIndexSet> _indexes = new Guarded<CacheIndexSet>(new CacheIndexSet());
        // subscriptions collection
        private readonly GuardedDictionary<Guid, ClientSubscription> _clientSubscriptions = new GuardedDictionary<Guid, ClientSubscription>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        public void RestoreSubscription(ClientSubscription subscription)
        {
            _clientSubscriptions.Set(subscription.SubscriptionId, subscription);
        }

        private void SaveInternalItem<T>(object data, string name, bool transient, TimeSpan lifetime)
        {
            ServerItem item = new ServerItem(
                _serverCfg.ModuleInfo, _cryptoManager,
                ItemKind.Local, transient, null, name, null,
                data, typeof(T),
                SerialFormat.Xml, lifetime);
            item.Freeze();
            ProcessNewItem(item, ItemSource.Client, false, Guid.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        public void UpdateSubscriptionState(ClientSubscription subscription)
        {
            ClientSubscriptionState state =
                new ClientSubscriptionState
                {
                    ConnectionId = subscription.ClientId.ToString(),
                    SubscriptionId = subscription.SubscriptionId.ToString(),
                    DataTypeName = subscription.DataTypeName,
                    ItemKind = (int) subscription.ItemKind,
                    Expression = subscription.Expression?.Serialise(),
                    AppScopes = subscription.AppScopes,
                    MinimumUSN = subscription.MinimumUSN,
                    ExcludeExisting = subscription.ExcludeExisting,
                    IncludeDeleted = !subscription.ExcludeDeleted,
                    AsAtTime = subscription.AsAtTime.ToString("o"),
                    ExcludeDataBody = subscription.ExcludeDataBody,
                    DebugRequest = subscription.DebugRequest
                };
            //state.ExpiryTime = subscription.ExpiryTime.ToString("o");
            string name = $"Subscription.State.{subscription.SubscriptionId}";
            SaveInternalItem<ClientSubscriptionState>(state, name, false, TimeSpan.MaxValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriptionId"></param>
        public void DeleteSubscriptionState(Guid subscriptionId)
        {
            string name = $"Subscription.State.{subscriptionId}";
            SaveInternalItem<ClientSubscriptionState>(null, name, false, TimeSpan.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="contractName"></param>
        /// <param name="replyAddress"></param>
        public void UpdateConnectionState(Guid clientId, string contractName, string replyAddress)
        {
            ClientConnectionState connState = new ClientConnectionState
            {
                SourceId = clientId.ToString(),
                Contract = contractName,
                ReplyAddress = replyAddress
            };
            string name = $"Connection.State.{clientId}";
            SaveInternalItem<ClientConnectionState>(connState, name, false, ServerCfg.CommsConnectionExtension);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        public void DeleteConnectionState(Guid clientId)
        {
            string name = $"Connection.State.{clientId}";
            SaveInternalItem<ClientConnectionState>(null, name, false, TimeSpan.Zero);
        }

        // start-time state
        private IStoreEngine _storeEngine;
        private CommsEngine _commsEngine;
        private Timer _housekeepTimer;
        //private Timer _HeartbeatTimer;

        // run-time state
        private long _lastStoreUsn;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serverCfg"></param>
        /// <param name="cryptoManager"></param>
        public CacheEngine(ILogger logger, ServerCfg serverCfg, ICryptoManager cryptoManager)
            : base(PartNames.Cache, logger)
        {
            _serverCfg = serverCfg;
            _cryptoManager = cryptoManager;
            _mainCacheDispatcher = new AsyncThreadQueue(Logger);
            _dateTimeProvider = new StandardDateTimeProvider(TimeZoneInfo.Local);
        }

        protected override void OnAttached(string name, IServerPart part)
        {
            switch (name)
            {
                case PartNames.Store:
                    _storeEngine = (IStoreEngine)part;
                    break;
                case PartNames.Comms:
                    _commsEngine = (CommsEngine)part;
                    break;
                default:
                    throw new NotSupportedException($"Unknown part: '{name}'");
            }
        }

        protected override void OnStart()
        {
            // load objects from store
            if (_storeEngine != null)
            {
                // synchronously load all existing data
                Logger.LogDebug("Querying store...");
                List<CommonItem> items = _storeEngine.SyncSelectAllItems();
                Logger.LogDebug("Loading {0} items...", items.Count);
                foreach (CommonItem item in items)
                {
                    ProcessNewItem(item, ItemSource.LocalStore, false, Guid.Empty);
                    if (item.StoreUSN > _lastStoreUsn)
                        _lastStoreUsn = item.StoreUSN;
                }
                Logger.LogDebug("Loaded {0} items (StoreUSN={1})", items.Count, _lastStoreUsn);
            }
            // start housekeeping timer
            _housekeepTimer = new Timer(DispatchHousekeepTimeout, null, ServerCfg.CacheHousekeepInterval, ServerCfg.CacheHousekeepInterval);
            //_HeartbeatTimer = new Timer(DispatchHeartbeatTimeout, null, ServerCfg.CacheHeartbeatInterval, ServerCfg.CacheHeartbeatInterval);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // final housekeep cycle
                DisposeHelper.SafeDispose(ref _housekeepTimer);
                DispatchHousekeepTimeout(null);
                _mainCacheDispatcher.WaitUntilEmpty(ServerCfg.ShutdownDisposeDelay);
                // clean up managed resources
                //DisposeHelper.SafeDispose(ref _HeartbeatTimer);
                _indexes.Dispose();
            }
            // no unmanaged resources to clean up
            base.Dispose(disposing);
        }

        //private void DispatchHeartbeatTimeout(object notUsed)
        //{
        //    todo;
        //}

        // housekeeping chain
        // parts:
        // 1) cleanup item name index
        //    - removed expired items from index
        // 2) cleanup item guid index
        //    - delete expired items from store
        //    - remove ancient items from index
        // 3) consolidate data in memory (buffer interning)
        // 4) remove expired connections todo in comms manager
        // 5) remove expired subscriptions
        //
        private int _housekeepCallsPart1;

        private void DispatchHousekeepTimeout(object notUsed)
        {
            Interlocked.Increment(ref _housekeepCallsPart1);
            _mainCacheDispatcher.Dispatch<object>(null, HousekeeperPart1);
        }

        private void HousekeeperPart1(object notUsed)
        {
            // calls are accumulative, not discrete
            if (Interlocked.Decrement(ref _housekeepCallsPart1) > 0) return;

            // Part1
            string step = "initialising";
            try
            {
                DateTimeOffset dtCommenced = DateTimeOffset.Now;
                int itemsInCache = 0;
                int itemsExpired = 0;
                int itemsStored = 0;
                _indexes.Locked(indexes =>
                {
                    step = "scanning names";
                    itemsInCache = indexes.OuterNames.Count;
                    List<string> oldNames = new List<string>();
                    foreach (KeyValuePair<string, InnerItemRef> kvp in indexes.OuterNames)
                    {
                        InnerItemRef itemRef = kvp.Value;
                        if (itemRef.Item != null)
                        {
                            // persist if required
                            if ((_storeEngine != null) && (!itemRef.Item.Transient) && (!itemRef.Persisted))
                            {
                                itemsStored++;
                                itemRef.Persist();
                                _storeEngine.AsyncRecvInsertItem(itemRef.Item);
                            }
                            if (!itemRef.Item.IsCurrent(dtCommenced))
                                oldNames.Add(kvp.Key); // expired - remove it
                        }
                        else
                        {
                            oldNames.Add(kvp.Key); // deleted - remove it
                        }
                    }
                    step = "deleting old names";
                    foreach (string oldName in oldNames)
                    {
                        itemsExpired++;
                        indexes.OuterNames.Remove(oldName);
                    }
                });
                step = "completing";
                DateTimeOffset dtCompleted = DateTimeOffset.Now;
                TimeSpan duration = (dtCompleted - dtCommenced);
                Logger.LogDebug("---------- housekeep Part1 ----------");
                Logger.LogDebug("Items");
                Logger.LogDebug("  Total   : {0}", itemsInCache);
                Logger.LogDebug("  Expired : {0}", itemsExpired);
                Logger.LogDebug("  Stored  : {0}", itemsStored);
                Logger.LogDebug("Duration  : {0}s", duration.TotalSeconds);
                Logger.LogDebug("---------- housekeep Part1 ----------");
            }
            catch (Exception e)
            {
                Logger.LogError("HousekeeperPart1 failed while '{0}': {1}", step, e);
            }

            // ---------------------------------------- next part ----------------------------------------
            Interlocked.Increment(ref _housekeepCallsPart2);
            _mainCacheDispatcher.Dispatch<object>(null, HousekeeperPart2);
        }

        private int _housekeepCallsPart2;
        private void HousekeeperPart2(object notUsed)
        {
            // calls are accumulative, not discrete
            if (Interlocked.Decrement(ref _housekeepCallsPart2) > 0) return;

            // Part2
            string step = "initialising";
            try
            {
                DateTimeOffset dtCommenced = DateTimeOffset.Now;
                int guidsInCache = 0;
                int guidsExpired = 0;
                int guidsDeleted = 0;
                _indexes.Locked(indexes =>
                {
                    step = "finding deleted item guids";
                    guidsInCache = indexes.InnerGuids.Count;
                    IList<CommonItem> deletedItems = new List<CommonItem>();
                    IList<Guid> ancientGuids = new List<Guid>();
                    Dictionary<Guid, byte[]> uniqueBuffers = new Dictionary<Guid, byte[]>();
                    foreach (KeyValuePair<Guid, InnerItemRef> kvp in indexes.InnerGuids)
                    {
                        string substep = "checking itemref state";
                        try
                        {
                            InnerItemRef itemref = kvp.Value;
                            CommonItem item = itemref.Item;
                            if (item != null)
                            {
                                substep = "checking current itemref";
                                if (!item.IsCurrent(dtCommenced))
                                {
                                    substep = "recording as deleted";
                                    // expired - remove it
                                    deletedItems.Add(item);
                                }
                                else
                                {
                                    substep = "checking if superceded";
                                    // check unexpired item has a matching name index entry
                                    if (indexes.OuterNames.TryGetValue(CoreHelper.MakeUniqueName(item.ItemKind, item.AppScope, item.Name), out var namedItemRef))
                                    {
                                        // found - check if superceded
                                        if (namedItemRef.Item != null 
                                            && namedItemRef.Item.Id != item.Id
                                            && (dtCommenced - namedItemRef.Item.Created) > ServerCfg.CacheDeletedItemsRetention)
                                        {
                                            // item has been superceded by more than 1 housekeep interval
                                            // - remove it
                                            deletedItems.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        // matching named item not found - remove it
                                        deletedItems.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                substep = "checking deleted itemref";
                                // already deleted - ancient?
                                if ((dtCommenced - itemref.Changed) > ServerCfg.CacheAncientGuidsRetention)
                                    ancientGuids.Add(kvp.Key);
                            }
                        }
                        catch (Exception excp)
                        {
                            Logger.LogError("Failed while '{0}({1})/{2}': {3}", step, kvp.Key, substep, excp);
                        }
                    }
                    step = "removing deleted item guids";
                    foreach (var item in deletedItems)
                    {
                        guidsExpired++;
                        indexes.InnerGuids[item.Id].Delete();
                        // for persistent items send delete request to store
                        if (_storeEngine != null && !item.Transient)
                            _storeEngine.AsyncRecvDeleteItem(item.Id);
                    }
                    step = "removing ancient item guids";
                    foreach (Guid id in ancientGuids)
                    {
                        guidsDeleted++;
                        indexes.InnerGuids.Remove(id);
                    }
                });
                step = "completing";
                DateTimeOffset dtCompleted = DateTimeOffset.Now;
                TimeSpan duration = (dtCompleted - dtCommenced);
                Logger.LogDebug("---------- housekeep Part2 ----------");
                Logger.LogDebug("Guids");
                Logger.LogDebug("  Total   : {0}", guidsInCache);
                Logger.LogDebug("  Expired : {0}", guidsExpired);
                Logger.LogDebug("  Deleted : {0}", guidsDeleted);
                Logger.LogDebug("Duration    : {0}s", duration.TotalSeconds);
                Logger.LogDebug("---------- housekeep Part2 ----------");
            }
            catch (Exception e)
            {
                Logger.LogError("HousekeeperPart2 failed while '{0}': {1}", step, e);
            }

            // ---------------------------------------- next part ----------------------------------------
            Interlocked.Increment(ref _housekeepCallsPart3);
            _mainCacheDispatcher.Dispatch<object>(null, HousekeeperPart3);
        }

        private int _housekeepCallsPart3;
        private void HousekeeperPart3(object notUsed)
        {
            // calls are accumulative, not discrete
            if (Interlocked.Decrement(ref _housekeepCallsPart3) > 0) return;

            // Part3
            string step = "initialising";
            try
            {
                DateTimeOffset dtCommenced = DateTimeOffset.Now;
                int totalBytes = 0;
                int totalItems = 0;
                int uniqueBytes = 0;
                int uniqueItems = 0;
                //int internBytes = 0;
                //int internItems = 0;
                _indexes.Locked((indexes) =>
                {
                    step = "consolidating buffers";
                    Dictionary<Guid, byte[]> uniqueBuffers = new Dictionary<Guid, byte[]>();
                    foreach (KeyValuePair<Guid, InnerItemRef> kvp in indexes.InnerGuids)
                    {
                        InnerItemRef itemref = kvp.Value;
                        CommonItem item = itemref.Item;
                        if (item?.YData != null)
                        {
                            totalItems++;
                            totalBytes += item.YData.Length;
                            Guid hash = item.YDataHash;
                            if (uniqueBuffers.TryGetValue(hash, out var buffer))
                            {
                                // already exists - not unique
                                int buflen = buffer.Length;
                                if (buflen > 0
                                    && buflen == item.YData.Length
                                    && buffer[0] == item.YData[0]
                                    && buffer[buflen - 1] == item.YData[buflen - 1])
                                {
                                    item.SetYData(buffer);
                                }
                                else
                                {
                                    Logger.LogError("Hash collision!");
                                }
                            }
                            else
                            {
                                // not found - new unique buffer
                                uniqueItems++;
                                uniqueBytes += item.YData.Length;
                                uniqueBuffers[hash] = item.YData;
                            }
                        }
                    }
                });
                step = "completing";
                DateTimeOffset dtCompleted = DateTimeOffset.Now;
                TimeSpan duration = (dtCompleted - dtCommenced);
                Logger.LogDebug("---------- housekeep Part3 ----------");
                Logger.LogDebug("Memory");
                //_Logger.LogDebug("  Interned: {0} ({1} bytes)", internItems.ToString("N0"), internBytes.ToString("N0"));
                Logger.LogDebug("  Total    : {0} ({1} bytes)", totalItems.ToString("N0"), totalBytes.ToString("N0"));
                Logger.LogDebug("  Unique   : {0} ({1} bytes)", uniqueItems.ToString("N0"), uniqueBytes.ToString("N0"));
                Logger.LogDebug("Duration   : {0}s", duration.TotalSeconds);
                Logger.LogDebug("---------- housekeep Part3 ----------");
            }
            catch (Exception e)
            {
                Logger.LogError("HousekeeperPart3 failed while '{0}': {1}", step, e);
            }

            // ---------------------------------------- next part ----------------------------------------
            Interlocked.Increment(ref _housekeepCallsPart4);
            _mainCacheDispatcher.Dispatch<object>(null, HousekeeperPart4);
        }

        private int _housekeepCallsPart4;
        private void HousekeeperPart4(object notUsed)
        {
            // calls are accumulative, not discrete
            if (Interlocked.Decrement(ref _housekeepCallsPart4) > 0) return;

            // Part4
            string step = "initialising";
            try
            {
                DateTimeOffset dtCommenced = DateTimeOffset.Now;
                _indexes.Locked((indexes) =>
                {
                    step = "cleaning data type indexes";
                    foreach (Dictionary<string, InnerItemRef> dataTypeSubIndex in indexes.DataTypes.Values)
                    {
                        IList<string> oldNames2 = new List<string>();
                        foreach (string key in dataTypeSubIndex.Keys)
                        {
                            InnerItemRef itemref = dataTypeSubIndex[key];
                            if ((itemref.Item == null) || (!itemref.Item.IsCurrent(dtCommenced)))
                            {
                                // expired - remove it
                                oldNames2.Add(key);
                            }
                        }
                        foreach (string oldName in oldNames2)
                        {
                            dataTypeSubIndex.Remove(oldName);
                        }
                        // note: we don't remove empty sub-indexes
                    }
                });
                step = "completing";
                DateTimeOffset dtCompleted = DateTimeOffset.Now;
                TimeSpan duration = (dtCompleted - dtCommenced);
                Logger.LogDebug("---------- housekeep Part4 ----------");
                //_Logger.LogDebug(todo);
                Logger.LogDebug("Duration    : {0}s", duration.TotalSeconds);
                Logger.LogDebug("---------- housekeep Part4 ----------");
            }
            catch (Exception e)
            {
                Logger.LogError("HousekeeperPart4 failed while '{0}': {1}", step, e);
            }

            // ---------------------------------------- next part ----------------------------------------
            Interlocked.Increment(ref _housekeepCallsPart5);
            _mainCacheDispatcher.Dispatch<object>(null, HousekeeperPart5);
        }

        private int _housekeepCallsPart5;
        private void HousekeeperPart5(object notUsed)
        {
            // calls are accumulative, not discrete
            if (Interlocked.Decrement(ref _housekeepCallsPart5) > 0) return;

            // Part5
            string step = "initialising";
            try
            {
                DateTimeOffset dtCommenced = DateTimeOffset.Now;
                step = "expiring old subscriptions";
                int activeCount = 0;
                List<ClientSubscription> expiredSubs = new List<ClientSubscription>();
                foreach (ClientSubscription clientSub in _clientSubscriptions.GetValues(false))
                {
                    IConnection connection = _commsEngine.GetValidConnection(clientSub.ClientId);
                    if (connection == null)
                    {
                        // client not found or faulted or expired
                        expiredSubs.Add(clientSub);
                    }
                    else
                    {
                        activeCount++;
                    }
                }
                foreach (ClientSubscription clientSub in expiredSubs)
                {
                    _clientSubscriptions.Remove(clientSub.SubscriptionId);
                    DeleteSubscriptionState(clientSub.SubscriptionId);
                    Logger.LogDebug("Subscription: '{0}' expired ({1})", clientSub.SubscriptionId, clientSub.ClientId);
                }
                step = "completing";
                DateTimeOffset dtCompleted = DateTimeOffset.Now;
                TimeSpan duration = dtCompleted - dtCommenced;
                Logger.LogDebug("---------- housekeep Part5 ----------");
                Logger.LogDebug("Subscriptions");
                Logger.LogDebug("  Active    : {0}", activeCount);
                Logger.LogDebug("  Expired   : {0}", expiredSubs.Count);
                Logger.LogDebug("Duration    : {0}s", duration.TotalSeconds);
                Logger.LogDebug("---------- housekeep Part5 ----------");
            }
            catch (Exception e)
            {
                Logger.LogError("HousekeeperPart5 failed while '{0}': {1}", step, e);
            }

            // ---------------------------------------- next part ----------------------------------------
            Interlocked.Increment(ref _housekeepCallsPart6);
            _mainCacheDispatcher.Dispatch<object>(null, HousekeeperPart6);
        }

        private int _housekeepCallsPart6;
        private void HousekeeperPart6(object notUsed)
        {
            // calls are accumulative, not discrete
            if (Interlocked.Decrement(ref _housekeepCallsPart6) > 0) return;

            // Part6
            string step = "initialising";
            try
            {
                DateTimeOffset dtCommenced = DateTimeOffset.Now;
                _indexes.Locked((indexes) =>
                {
                    step = "garbage collecting";
                    GC.Collect();
                });
                step = "completing";
                DateTimeOffset dtCompleted = DateTimeOffset.Now;
                TimeSpan duration = (dtCompleted - dtCommenced);
                Logger.LogDebug("---------- housekeep Part6 ----------");
                //_Logger.LogDebug(todo);
                Logger.LogDebug("Duration    : {0}s", duration.TotalSeconds);
                Logger.LogDebug("---------- housekeep Part6 ----------");
            }
            catch (Exception e)
            {
                Logger.LogError("HousekeeperPart6 failed while '{0}': {1}", step, e);
            }

            // ---------------------------------------- next part ----------------------------------------
        }

        public void PerformSyncRecvSelectMultipleItems(PackageSelectMultipleItems package)
        {
            PerformSyncRecvGenericRequest(package, () =>
            {
                string[] appScopes = { AppScopeNames.Legacy };
                if ((package.Query.AppScopes != null) && (package.Query.AppScopes.Length != 0))
                    appScopes = package.Query.AppScopes;
                IExpression whereExpr = package.Query.QueryExpr != null ? Expr.Create(package.Query.QueryExpr) : Expr.ALL;
                IExpression orderExpr = package.Query.OrderExpr != null ? Expr.Create(package.Query.OrderExpr) : null;
                if (package.Header.DebugRequest)
                {
                    Logger.LogDebug("Running query...");
                    Logger.LogDebug("  Request  : {0}", package.Header.RequestId);
                    Logger.LogDebug("  Client Id: {0}", package.ClientId);
                    Logger.LogDebug("  AppScopes: {0}", (appScopes == null) ? "*" : string.Join(",", appScopes));
                    Logger.LogDebug("  ItemKind : {0}", (package.Query.ItemKind == ItemKind.Undefined) ? "(any)" : package.Query.ItemKind.ToString());
                    if (package.Query.ItemIds != null)
                    {
                        //string[] itemIds = new string[request.ItemIds.Length];
                        //for (int i = 0; i < request.ItemIds.Length; i++)
                        //    itemIds[i] = request.ItemIds[i].ToString();
                        if (package.Query.ItemIds.Length == 1)
                            Logger.LogDebug("  ItemId   : {0}", package.Query.ItemIds[0].ToString());
                        else
                            Logger.LogDebug("  ItemIds  : Multiple({0})", package.Query.ItemIds.Length);
                    }
                    if (package.Query.ItemNames != null)
                    {
                        if (package.Query.ItemNames.Length == 1)
                            Logger.LogDebug("  ItemName : '{0}'", package.Query.ItemNames[0]);
                        else
                            Logger.LogDebug("  ItemNames: Multiple({0})", package.Query.ItemNames.Length);
                    }
                    Logger.LogDebug("  DataType : {0}", package.Query.DataType ?? "(any)");
                    if (package.Query.MinimumUSN > 0)
                        Logger.LogDebug("  USN >    : {0}", package.Query.MinimumUSN);
                    if (package.Query.QueryExpr != null)
                        Logger.LogDebug("  Query    : {0}", whereExpr.DisplayString());
                    if (orderExpr != null)
                        Logger.LogDebug("  OrderBy  : {0}", orderExpr.DisplayString());
                    if (package.Query.RowCount > 0)
                    {
                        Logger.LogDebug("  StartRow : {0}", package.Query.StartRow);
                        Logger.LogDebug("  RowCount : {0}", package.Query.RowCount);
                    }
                    Logger.LogDebug("  Excl.Deleted?: {0}", package.Query.ExcludeDeleted);
                    Logger.LogDebug("  Excl.DataBody: {0}", package.Query.ExcludeDataBody);
                }
                List<CommonItem> results = new List<CommonItem>();
                if (package.Query.ItemIds != null)
                {
                    foreach (Guid id in package.Query.ItemIds)
                    {
                        CommonItem item = SelectItem(
                            id, appScopes[0], null, package.Query.ItemKind,
                            package.Query.DataType, package.Query.MinimumUSN, package.Query.AsAtTime, package.Query.ExcludeDeleted);
                        if (item != null)
                            results.Add(item);
                    }
                }
                else if (package.Query.ItemNames != null)
                {
                    foreach (string name in package.Query.ItemNames)
                    {
                        CommonItem item = SelectItem(
                            new Guid(), appScopes[0], name, package.Query.ItemKind,
                            package.Query.DataType, package.Query.MinimumUSN, package.Query.AsAtTime, package.Query.ExcludeDeleted);
                        if (item != null)
                            results.Add(item);
                    }
                }
                else
                {
                    var interimResults = GetCacheItems(
                        appScopes, package.Query.ItemKind, package.Query.DataType, whereExpr,
                        package.Query.MinimumUSN, package.Query.AsAtTime, package.Query.ExcludeDeleted, package.Header.DebugRequest);
                    // sort results if required for paging
                    if (orderExpr != null)
                    {
                        if (package.Header.DebugRequest)
                            Logger.LogDebug("Sorting {0} items", interimResults.Count);
                        CommonItemComparer comparer = new CommonItemComparer(orderExpr);
                        interimResults.Sort(comparer);
                    }
                    if (package.Query.RowCount > 0)
                    {
                        // return the requested page
                        for (int i = package.Query.StartRow; i < (package.Query.StartRow + package.Query.RowCount); i++)
                        {
                            if (i < interimResults.Count)
                                results.Add(interimResults[i]);
                            else
                                break;
                        }
                    }
                    else
                    {
                        // no paging
                        foreach (CommonItem item in interimResults)
                            results.Add(item);
                    }
                }
                if (package.Header.DebugRequest)
                    Logger.LogDebug("Found {0} items.", results.Count);
                // send large item sets in multiple pages
                PackageHeader header2 = new PackageHeader(package.ClientId, package.Header.RequestId, true, false, null, null, package.Header.DebugRequest);
                List<CommonItem> currentPage = new List<CommonItem>();
                int currentPageSize = 0;
                foreach (CommonItem item in results)
                {
                    int estimatedItemSize = item.EstimatedSizeInBytes(package.Query.ExcludeDataBody);
                    if ((currentPage.Count >= 50) || ((currentPageSize + estimatedItemSize) >= WcfConst.MaxMessageSize))
                    {
                        _commsEngine.DispatchAsyncSendAnswerMultipleItems(new PackageAnswerMultipleItems(
                            package.ClientId, header2, currentPage, package.Query.ExcludeDataBody));
                        currentPage = new List<CommonItem>();
                        currentPageSize = 0;
                    }
                    currentPage.Add(item);
                    currentPageSize += estimatedItemSize;
                }
                _commsEngine.DispatchAsyncSendAnswerMultipleItems(new PackageAnswerMultipleItems(
                        package.ClientId, header2, currentPage, package.Query.ExcludeDataBody));
            });
        }

        // subscriptions
        private void PerformSyncRecvGenericRequest(PackageBase package, CodeSection codeSection)
        {
            int debugFlags = 0;
            string debugId = package.Header.RequestId.ToString();
            try
            {
                string requestTypeName = package.GetType().Name;
                PackageHeader header = package.Header;
                if (header.DebugRequest)
                    Logger.LogDebug("Request '{0}' ({1})", header.RequestId, requestTypeName);
                try
                {
                    debugFlags += 1;
                    codeSection();
                    debugFlags += 2;
                }
                catch (Exception excp)
                {
                    Logger.LogError("Request '{0}' ({1}) failed: {2}", header.RequestId, requestTypeName, excp);
                    // todo - send exception details
                }
                finally
                {
                    // send response if requested
                    if ((header.RequestId != Guid.Empty) && (header.ReplyRequired) && (!header.MoreFollowing))
                    {
                        debugFlags += 4;
                        _commsEngine.DispatchAsyncSendCompletionResult(new PackageCompletionResult(
                            package.ClientId,
                            new PackageHeader(package.ClientId, header.RequestId, false, false, null, null, header.DebugRequest),
                            true, 0, null));
                        debugFlags += 8;
                    }
                }
                debugFlags += 256;
            }
            finally
            {
                Logger.LogDebug("GenReq(" + package.GetType().Name + ") '" + debugId + "' recving [" + debugFlags.ToString() + "] ...");
            }
        }

        public void PerformSyncRecvCreateSubscription(PackageCreateSubscription package)
        {
            int debugFlags = 0;
            string debugId = package.Header.RequestId.ToString();
            try
            {
                //PackageHeader header = package.Header;
                ClientSubscription subscription = null;
                PerformSyncRecvGenericRequest(package, () =>
                {
                    // create subscription
                    subscription = new ClientSubscription(package.ClientId, package, package.Header.DebugRequest);
                    _clientSubscriptions.Set(subscription.SubscriptionId, subscription);
                    // persist updated subscription state
                    UpdateSubscriptionState(subscription);
                    Logger.LogDebug("Subscription: '{0}' created ({1})", subscription.SubscriptionId, subscription.ClientId);
                    if (package.Header.DebugRequest)
                    {
                        Logger.LogDebug("  Request  : {0}", package.Header.RequestId);
                        Logger.LogDebug("  AppScopes: {0}", String.Join(",", subscription.AppScopes ?? new[] { AppScopeNames.Legacy }));
                        Logger.LogDebug("  ItemKind : {0}", subscription.ItemKind == ItemKind.Undefined ? "(any)" : subscription.ItemKind.ToString());
                        Logger.LogDebug("  DataType : {0}", subscription.DataTypeName ?? "(any)");
                        Logger.LogDebug("  Query    : {0}", subscription.Expression.DisplayString());
                        Logger.LogDebug("  USN >    : {0}", subscription.MinimumUSN);
                        Logger.LogDebug("  Excl.Existing?: {0}", subscription.ExcludeExisting);
                        Logger.LogDebug("  Excl.Deleted? : {0}", subscription.ExcludeDeleted);
                        Logger.LogDebug("  Excl.DataBody?: {0}", subscription.ExcludeDataBody);
                    }
                });
                // now send existing items (if required) after subscription created (not during)
                if (!subscription.ExcludeExisting)
                {
                    string[] appScopes = subscription.AppScopes ?? new[] { AppScopeNames.Legacy };
                    debugFlags += 1;
                    var items = GetCacheItems(
                        appScopes, subscription.ItemKind, subscription.DataTypeName, subscription.Expression,
                        subscription.MinimumUSN, subscription.AsAtTime, (subscription.ExcludeDeleted), subscription.DebugRequest).ToArray();
                    debugFlags += 2;
                    if (package.Header.DebugRequest)
                        Logger.LogDebug("Found {0} existing items.", items.Length);
                    // send large item sets in multiple pages
                    PackageHeader header2 = new PackageHeader(package.ClientId, package.Header.RequestId, false, false, null, null, package.Header.DebugRequest);
                    List<CommonItem> currentPage = new List<CommonItem>();
                    int currentPageSize = 0;
                    foreach (CommonItem item in items)
                    {
                        int itemSize = item.EstimatedSizeInBytes(package.Query.ExcludeDataBody);
                        if ((currentPage.Count >= 50) || ((currentPageSize + itemSize) >= WcfConst.MaxMessageSize))
                        {
                            if (package.Header.DebugRequest)
                                Logger.LogDebug("Sending {0} items (est. {1} bytes) in page", currentPage.Count, currentPageSize);
                            debugFlags += 4;
                            _commsEngine.DispatchAsyncSendNotifyMultipleItems(new PackageNotifyMultipleItems(
                                package.ClientId, header2, subscription.SubscriptionId, currentPage, subscription.ExcludeDataBody));
                            debugFlags += 8;
                            currentPage = new List<CommonItem>();
                            currentPageSize = 0;
                        }
                        currentPage.Add(item);
                        currentPageSize += itemSize;
                    }
                    if (package.Header.DebugRequest)
                        Logger.LogDebug("Sending {0} items (est. {1} bytes) in last page", currentPage.Count, currentPageSize);
                    debugFlags += 16;
                    _commsEngine.DispatchAsyncSendNotifyMultipleItems(new PackageNotifyMultipleItems(
                            package.ClientId, header2, subscription.SubscriptionId, currentPage, subscription.ExcludeDataBody));
                    debugFlags += 32;
                }
                debugFlags += 256;
            }
            finally
            {
                Logger.LogDebug("CreSub '" + debugId + "' recving [" + debugFlags + "] ...");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void PerformSyncRecvExtendSubscription(PackageExtendSubscription package)
        {
            PerformSyncRecvGenericRequest(package, () =>
            {
                // do nothing
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void PerformSyncRecvCancelSubscription(PackageCancelSubscription package)
        {
            PerformSyncRecvGenericRequest(package, () =>
            {
                // cancel subscription
                _clientSubscriptions.Remove(package.SubscriptionId);
                DeleteSubscriptionState(package.SubscriptionId);
                Logger.LogDebug("Subscription: '{0}' removed ({1})", package.SubscriptionId, package.ClientId);
            });
        }

        private void DispatchAsyncSendSubscriptionCheck(ClientSubscription clientSubs, CommonItem item, bool debugRequest, Guid requestId)
        {
            IConnection connection = _commsEngine.GetValidConnection(clientSubs.ClientId);
            connection?.DispatchToIncomingThreadQueue(
                new PackageSubscriptionCheck(clientSubs, item, debugRequest, requestId),
                PerformSyncSendSubscriptionCheck);
        }

        private void PerformSyncSendSubscriptionCheck(PackageSubscriptionCheck package)
        {
            ClientSubscription subscription = package.ClientSubs;
            CommonItem candidate = package.Candidate;
            try
            {
                IConnection connection = _commsEngine.GetValidConnection(subscription.ClientId);
                if (candidate != null 
                    && connection != null
                    && (!subscription.ExcludeDeleted || candidate.IsCurrent(subscription.AsAtTime)))
                {
                    if (ItemMatchesQuery(candidate, subscription.ItemKind, subscription.AppScopes, subscription.Expression, subscription.DataTypeName, 
                        package.DebugRequest))
                    {
                        List<CommonItem> items = new List<CommonItem> {candidate};
                        _commsEngine.DispatchAsyncSendNotifyMultipleItems(
                            new PackageNotifyMultipleItems(
                                subscription.ClientId,
                                new PackageHeader(subscription.ClientId, subscription.SubscriptionId, false, false, null, null, subscription.DebugRequest),
                                subscription.SubscriptionId,
                                items,
                                subscription.ExcludeDataBody));
                    }
                }
            }
            catch (Exception excp)
            {
                // subscription expression evaluation failed
                // - cancel subscription
                _clientSubscriptions.Remove(subscription.SubscriptionId);
                Logger.LogError("Subscription '{0}' failed: {1}", subscription.SubscriptionId, excp);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void PerformSyncRecvAnswerMultipleItems(PackageAnswerMultipleItems package)
        {
            int count = 0;
            PerformSyncRecvGenericRequest(package, () =>
            {
                foreach (CommonItem item in package.Items)
                {
                    ProcessNewItem(item, ItemSource.PeerServer, package.Header.DebugRequest, package.Header.RequestId);
                    count++;
                }
            });
            if (package.Header.DebugRequest)
            {
                Logger.LogDebug("Request '{0}' received {1} items",
                    package.Header.RequestId, count);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void PerformSyncRecvNotifyMultipleItems(PackageNotifyMultipleItems package)
        {
            int count = 0;
            PerformSyncRecvGenericRequest(package, () =>
            {
                foreach (CommonItem item in package.Items)
                {
                    ProcessNewItem(item, ItemSource.PeerServer, package.Header.DebugRequest, package.Header.RequestId);
                    count++;
                }
            });
            if (package.Header.DebugRequest)
            {
                Logger.LogDebug("Request '{0}' received {1} items",
                    package.Header.RequestId, count);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void PerformSyncRecvCompletionResult(PackageCompletionResult package)
        {
            PerformSyncRecvGenericRequest(package, () =>
            {
                // nothing extra to do
            });
        }

        private void ProcessNewItem(
            CommonItem item,
            ItemSource itemSource,
            bool debugRequest, Guid requestId)
        {
            if (debugRequest)
                Logger.LogDebug("Request '{0}' processing item: '{1}' ({2})", requestId, item.Name, item.Id);

            switch (itemSource)
            {
                case ItemSource.Client:
                case ItemSource.LocalStore:
                case ItemSource.PeerServer:
                    break;
                default:
                    throw new ArgumentException("Unknown ItemSource: " + itemSource.ToString());
            }
            // we ignore the item if:
            // - the item is a duplicate (currently in or was in the guid index)
            // - we already have a newer item with the same name (in the name index)
            // note: we don't ignore expired items - housekeeping will catch these
            DateTimeOffset dtoNow = DateTimeOffset.Now;
            string step = null;
            try
            {
                step = "A";
                // ignore item if already in guid index
                bool duplicate = false;
                _indexes.Locked((indexes) =>
                {
                    step = "A.0";
                    if (indexes.InnerGuids.ContainsKey(item.Id))
                    {
                        duplicate = true;
                    }
                    step = "A.9";
                });
                if (duplicate)
                {
                    if (debugRequest)
                        Logger.LogDebug("Request '{0}' ignoring duplicate item", requestId);
                    if (itemSource == ItemSource.LocalStore)
                        return;
                    else
                        return;
                }
                step = "B";
                // retain if name already exists
                // - except if out-of-date
                string uniqueItemName = item.UniqueName;
                //bool itemExists = false;
                _indexes.Locked((indexes) =>
                {
                    step = "B.0";
                    // ----- cache locked -----
                    bool outOfDate = false;
                    if (indexes.OuterNames.TryGetValue(uniqueItemName, out var oldItemRef))
                    {
                        step = "B.0.0";
                        //itemExists = true;
                        if (item.Created < oldItemRef.Item.Created)
                        {
                            // received item is older - ignore
                            outOfDate = true;
                        }
                    }
                    step = "B.1";
                    // assign a new USN if not load from store
                    if (itemSource != ItemSource.LocalStore)
                        item.StoreUSN = Interlocked.Increment(ref _lastStoreUsn);
                    // update indexes
                    InnerItemRef itemRef = new InnerItemRef(item, (itemSource == ItemSource.LocalStore));
                    // - outer name index
                    step = "B.2";
                    if (!outOfDate)
                    {
                        step = "B.2.0";
                        // update who stats: domain/name/host/app
                        //_StatsCounters.AddToHierarchy(String.Format("SaveUser.{0}.{1}.{2}.{3}",
                        //    clientInfo.UserName,
                        //    clientInfo.UserWDom,
                        //    clientInfo.HostName,
                        //    clientInfo.ApplName));
                        // update where stats: ip/host/app
                        //_StatsCounters.AddToHierarchy(String.Format("SaveHost.{0}.{1}.{2}",
                        //    clientInfo.HostIpV4,
                        //    clientInfo.HostName,
                        //    clientInfo.ApplName));
                        step = "B.2.1";
                        // update data type stats:
                        StatsCountersDelta.AddToHierarchy($"SaveType.{item.ItemKind}.{item.DataTypeName}");
                        // update item time stats:
                        StatsCountersDelta.AddToHierarchy($"SaveTime.{dtoNow:ddd}.{dtoNow:HH}");
                        // update item time stats:
                        StatsCountersDelta.AddToHierarchy($"SaveDate.{dtoNow:MMM}.{dtoNow:dd}");
                        if (debugRequest)
                            Logger.LogDebug("Request '{0}' item '{1}' updated.", requestId, item.Name);
                        step = "B.2.2";
                        indexes.OuterNames[uniqueItemName] = itemRef;
                    }
                    else
                    {
                        step = "B.2.3";
                        if (debugRequest)
                            Logger.LogDebug("Request '{0}' item '{1}' out-of-date.", requestId, item.Name);
                    }
                    step = "B.3";
                    // - inner guid index
                    indexes.InnerGuids[item.Id] = itemRef;
                    // - typed name index
                    step = "B.4";
                    if (item.DataTypeName != null)
                    {
                        step = "B.4.0";
                        if (!indexes.DataTypes.TryGetValue(item.DataTypeName, out var dataTypeSubIndex))
                        {
                            step = "B.4.0.0";
                            dataTypeSubIndex = new Dictionary<string, InnerItemRef>();
                            step = "B.4.0.1";
                            indexes.DataTypes[item.DataTypeName] = dataTypeSubIndex;
                            step = "B.4.0.9";
                        }
                        step = "B.4.1";
                        dataTypeSubIndex[uniqueItemName] = itemRef;
                        step = "B.4.9";
                    }
                    // ----- cache unlocked -----
                    step = "B.9";
                });
                // send to subscribers (if any)
                int subscriberCount = 0;
                foreach (ClientSubscription clientSub in _clientSubscriptions.GetValues(false))
                {
                    //if (clientSub.ExpiryTime >= dtoNow)
                    //{
                        subscriberCount++;
                        DispatchAsyncSendSubscriptionCheck(clientSub, item, debugRequest, requestId);
                    //}
                }
                if (debugRequest)
                    Logger.LogDebug("Request '{0}' checked with {1} current subscribers.", requestId, subscriberCount);

                step = "Z";
            }
            catch (Exception excp)
            {
                Logger.LogError("Step={0}: {1}", step, excp);
            }
            finally
            {
                //Logger.LogDebug("ProcessNewItem: {0}: '{1}' ({2}) {3}", item.ItemKind, item.Name, item.Id, debugInfo);
            }
        }

        //public void DeleteItems(string[] appScopes, ItemKind itemKind, string dataType, string query, DateTimeOffset asAtTime, bool debugRequest)
        //{
        //    // for each item matching - delete it
        //    List<CommonItem> items = GetCacheItems(
        //        appScopes, itemKind, dataType,
        //        (query != null) ? Expr.Create(query) : Expr.ALL,
        //        0, asAtTime, true, debugRequest);
        //    foreach (CommonItem oldItem in items)
        //    {
        //        CommonItem newItem = new CommonItem(oldItem, oldItem.Transient, true);
        //        ProcessNewItem(newItem, ItemSource.Client, false, Guid.Empty);
        //    }
        //}

        private bool ItemMatchesQuery(
            CommonItem item, ItemKind itemKind, string[] appScopes, IExpression queryExpr, string dataTypeName,
            bool debugRequest)
        {
            bool matched = (
                ((itemKind == ItemKind.Undefined) || (item.ItemKind == itemKind))
                && (string.IsNullOrEmpty(dataTypeName) || (item.DataTypeName == dataTypeName))
                );
            if (!matched)
            {
                return false;
            }
            // check item matches appScope list
            if (appScopes == null)
                matched = true;
            else
            {
                matched = false;
                foreach (string appScope in appScopes)
                {
                    if (appScope == null || string.Compare(item.AppScope, appScope, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        matched = true;
                        break;
                    }
                }
            }
            if (!matched)
            {
                return false;
            }
            // check item matches query
            if (queryExpr == null)
                matched = true; // null query - matches all
            else
            {
                try
                {
                    matched = queryExpr.MatchesProperties(new ExprContext(item.AppProps, item.Name, item.Created, item.Expires));
                    //if (debugIsLevel2)
                    //{
                    //    _Logger.LogDebug("{0} '{1}' {2} match.", item.ItemKind, item.Name, Matched ? "DOES" : "does NOT");
                    //}
                }
                catch (Exception excp)
                {
                    if (debugRequest)
                    {
                        //_Logger.LogDebug("Query  : {0}", queryExpr.DisplayString());
                        Logger.LogDebug("Item   : '{0}' ({1}) {2}", item.Name, item.ItemKind, item.Id);
                        Logger.LogDebug("Props  :");
                        item.AppProps.LogValues(delegate(string text) { Logger.LogDebug("  " + text); });
                        Logger.LogDebug("Expression failed: {0}", excp);
                    }
                    return false;
                }
            }
            return matched;
        }

        internal List<CommonItem> GetCacheItems(
            string[] appScopes, ItemKind itemKind, string dataType, IExpression queryExpr,
            long minimumUSN, DateTimeOffset asAtTime, bool excludeDeleted, bool debugRequest)
        {
            // match subscription to existing cached items
            List<CommonItem> candidates = new List<CommonItem>();
            _indexes.Locked(indexes =>
            {
                if (dataType != null)
                {
                    // use data type index to find items quickly
                    if (indexes.DataTypes.TryGetValue(dataType, out var dataTypeSubIndex))
                    {
                        foreach (InnerItemRef itemref in dataTypeSubIndex.Values)
                        {
                            if (itemref.Item != null)
                                candidates.Add(itemref.Item);
                        }
                    }
                }
                else
                {
                    candidates.AddRange(indexes.OuterNames.Values.Select(itemRef => itemRef.Item));
                }
            });
            List<CommonItem> results = new List<CommonItem>();
            foreach (CommonItem item in candidates)
            {
                if (item != null && item.StoreUSN > minimumUSN && (!excludeDeleted || item.IsCurrent(asAtTime)))
                {
                    if (ItemMatchesQuery(item, itemKind, appScopes, queryExpr, (dataType ?? ""), debugRequest))
                    {
                        results.Add(item);
                    }
                }
            }
            return results;
        }

        // get single item
        private CommonItem GetCacheItem(Guid itemId)
        {
            return GetCacheItem(itemId, 0, DateTimeOffset.Now, true);
        }

        private CommonItem GetCacheItem(Guid itemId, long minimumUsn, DateTimeOffset asAtTime, bool excludeDeleted)
        {
            InnerItemRef itemref;
            CommonItem result = null;
            _indexes.Locked(indexes =>
            {
                if (indexes.InnerGuids.TryGetValue(itemId, out itemref))
                {
                    CommonItem item = itemref.Item;
                    if (item != null && item.StoreUSN > minimumUsn && (!excludeDeleted || item.IsCurrent(asAtTime)))
                    {
                        result = item;
                    }
                }
            });
            return result;
        }

        private CommonItem GetCacheItem(string itemName, ItemKind itemKind, string appScope, string dataType)
        {
            return GetCacheItem(itemName, itemKind, appScope, dataType, 0, DateTimeOffset.Now, true);
        }

        private CommonItem GetCacheItem(string itemName, ItemKind itemKind, string appScope, string dataTypeName, long minimumUSN, DateTimeOffset asAtTime, bool excludeDeleted)
        {
            string uniqueItemName = CoreHelper.MakeUniqueName(itemKind, appScope, itemName);

            CommonItem result = null;
            _indexes.Locked((indexes) =>
            {
                if (indexes.OuterNames.TryGetValue(uniqueItemName, out var itemRef))
                {
                    CommonItem item = itemRef.Item;
                    if (item.StoreUSN > minimumUSN
                        && (dataTypeName == null || (item.DataTypeName == dataTypeName))
                        && (!excludeDeleted || (item.IsCurrent(asAtTime))))
                    {
                        result = item;
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="appScope"></param>
        /// <param name="itemName"></param>
        /// <param name="itemKind"></param>
        /// <param name="dataType"></param>
        /// <param name="minimumUSN"></param>
        /// <param name="asAtTime"></param>
        /// <param name="excludeDeleted"></param>
        /// <returns></returns>
        public CommonItem SelectItem(Guid itemId, string appScope, string itemName, ItemKind itemKind, string dataType,
            long minimumUSN, DateTimeOffset asAtTime, bool excludeDeleted)
        {
            var result = itemId != Guid.Empty ? GetCacheItem(itemId, minimumUSN, asAtTime, excludeDeleted) : GetCacheItem(itemName, itemKind, appScope, dataType, minimumUSN, asAtTime, excludeDeleted);
            return result;
        }
    }
}
