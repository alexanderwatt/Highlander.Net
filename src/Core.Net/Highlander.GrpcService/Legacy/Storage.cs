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
using Highlander.GrpcService.Data;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Threading;

#endregion

namespace Highlander.Core.Server
{
    /// <summary>
    /// The store interface
    /// </summary>
    public interface IStoreEngine : IServerPart
    {
        List<CommonItem> SyncSelectAllItems();
        void AsyncRecvDeleteItem(Guid itemId);
        void AsyncRecvInsertItem(CommonItem item);
    }

    internal class StoreEngineState
    {
        public readonly ServerCfg ServerCfg;
        public readonly Queue<CommonItem> InsertQueue = new Queue<CommonItem>();
        public readonly Queue<Guid> DeleteQueue = new Queue<Guid>();
        public HighlanderContext DbContext { get; set; }
        public int ExceptionCount;
        public int CompletedCount;
        public StoreEngineState(ServerCfg serverCfg, HighlanderContext dbContext)
        {
            ServerCfg = serverCfg;
            DbContext = dbContext;
        }
    }

    // class that manages the object store table in SQL Server
    internal class StoreEngine : ServerPart, IStoreEngine
    {
        private readonly AsyncThreadQueue _inboundCallQueue;
        private readonly Guarded<StoreEngineState> _state;
        private CacheEngine _cacheEngine;
        private Timer _dbRetryTimer;

        public StoreEngine(
            ILogger logger,
            ServerCfg serverCfg,
            HighlanderContext dbContext
            )
            : base(PartNames.Store, logger)
        {
            _inboundCallQueue = new AsyncThreadQueue(Logger);
            string connectionString = EnvHelper.FormatDbCfgStr(serverCfg.ModuleInfo.ConfigEnv, serverCfg.DbServer, serverCfg.DbPrefix);
            Logger.LogDebug("Connection String: {0}", connectionString);
            _state = new Guarded<StoreEngineState>(new StoreEngineState(serverCfg, dbContext));
        }

        protected override void OnAttached(string name, IServerPart part)
        {
            switch (name)
            {
                case PartNames.Cache:
                    _cacheEngine = (CacheEngine)part;
                    break;
                default:
                    throw new NotImplementedException($"Unknown part: '{name}'");
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            _dbRetryTimer = new Timer(OnDbRetryTimeout, null, ServerCfg.StoreDatabaseRetryInterval, ServerCfg.StoreDatabaseRetryInterval);
        }

        private void OnDbRetryTimeout(object notUsed)
        {
            _inboundCallQueue.Dispatch<object>(null, AsyncDequeueDbOperations);
        }

        protected override void OnStop()
        {
            long remaining = _inboundCallQueue.WaitUntilEmpty(ServerCfg.ShutdownDisposeDelay);
            if (remaining > 0)
                Logger.LogWarning("Stopping with {0} operations enqueued!", remaining);
            _dbRetryTimer.Dispose();
            base.OnStop();
        }

        protected override void  OnStatsTimeoutReportDeltas()
        {
            base.OnStatsTimeoutReportDeltas();
            int pendingInserts = 0;
            int pendingDeletes = 0;
            long completedOps = 0;
            long dbExceptions = 0;
            _state.Locked(state =>
            {
                pendingInserts = state.InsertQueue.Count;
                pendingDeletes = state.DeleteQueue.Count;
                dbExceptions = state.ExceptionCount;
                completedOps = state.CompletedCount;
            });
            Logger.LogDebug("Enqueued operations : {0}", _inboundCallQueue.Length);
            Logger.LogDebug("Pending  insert ops : {0}", pendingInserts);
            Logger.LogDebug("Pending  delete ops : {0}", pendingDeletes);
            Logger.LogDebug("Completed operations: {0}", completedOps);
            Logger.LogDebug("Database exceptions : {0}", dbExceptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CommonItem> SyncSelectAllItems()
        {
            List<ItemData> sqlList = null;
            _state.Locked(state => sqlList = state.DbContext.Items.ToList());
            var results = new List<CommonItem>();
            foreach (ItemData row in sqlList)
            {
                results.Add(new CommonItem(
                    row.ItemId, (ItemKind)row.ItemType, false, row.ItemName, 
                    new NamedValueSet(row.AppProps), row.DataType, row.AppScope, 
                    new NamedValueSet(row.SysProps), row.NetScope, 
                    DateTimeOffset.Parse(row.Created), DateTimeOffset.Parse(row.Expires),
                    row.YData?.ToArray(), 
                    row.YSign?.ToArray(),
                    row.StoreUSN));
            }
            return results;
        }

        private void DequeueDbOperations()
        {
            bool faulted = false;
            int pendingOps = 0;
            do
            {
                _state.Locked(state =>
                {
                    string action = null;
                    try
                    {
                        // attempt insert (if any)
                        if (state.InsertQueue.Count > 0)
                        {
                            CommonItem item = state.InsertQueue.Peek();
                            action = $"create {item.ItemKind}: '{item.Name}' {item.Id} ({item.AppScope})";
                            state.DbContext.Items.Add(new ItemData(item));
                            state.DbContext.SaveChanges(true);
                            // insert done
                            state.CompletedCount++;
                            state.InsertQueue.Dequeue();
                            StatsCountersDelta.AddToHierarchy($"Inserted.{item.ItemKind}.{item.DataTypeName}");
                        }
                        else
                        {
                            // attempt delete (if any)
                            if (state.DeleteQueue.Count > 0)
                            {
                                Guid itemId = state.DeleteQueue.Peek();
                                action = $"delete {itemId}";
                                var itemToBeRemoved = state.DbContext.Items.Find(itemId);
                                state.DbContext.Remove(itemToBeRemoved);
                                state.DbContext.SaveChanges(true);
                                // delete done
                                state.CompletedCount++;
                                state.DeleteQueue.Dequeue();
                                StatsCountersDelta.Add("Deleted", 1);
                            }
                        }
                        pendingOps = state.InsertQueue.Count + state.DeleteQueue.Count;

                    }
                    catch (Exception e)
                    {
                        faulted = true;
                        state.ExceptionCount++;
                        Logger.LogDebug("Failed to {0}", action);
                        Logger.Log(e);
                    }
                });
            }
            while(pendingOps > 0 && !faulted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void AsyncRecvInsertItem(CommonItem item)
        {
            _inboundCallQueue.Dispatch(item, AsyncInsertItem); //AsyncQueuePriority.Normal, item.UniqueName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        public void AsyncRecvDeleteItem(Guid itemId)
        {
            _inboundCallQueue.Dispatch(itemId, AsyncDeleteItem); // AsyncQueuePriority.BelowNormal, itemId.ToString());
        }

        private void AsyncDeleteItem(Guid itemId)
        {
            _state.Locked(state => state.DeleteQueue.Enqueue(itemId));
            // now do outstanding db actions
            DequeueDbOperations();
        }

        private void AsyncInsertItem(CommonItem item)
        {
            _state.Locked(state => state.InsertQueue.Enqueue(item));
            // now do outstanding db actions
            DequeueDbOperations();
        }

        private void AsyncDequeueDbOperations(object notUsed)
        {
            DequeueDbOperations();
        }
    }

    /// <summary>
    /// A unit test helper class that simulates a store engine
    /// class that manages the QRStore database in SQL Server
    /// </summary>
    public class UnitTestStoreEngine : ServerPart, IStoreEngine
    {
        private readonly AsyncThreadQueue _databaseQueue;
        private CacheEngine _cacheEngine;
        private readonly Dictionary<Guid, CommonItem>  _itemDataTable = new Dictionary<Guid,CommonItem>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public UnitTestStoreEngine(ILogger logger)
            : base(PartNames.Store, logger)
        {
            _databaseQueue = new AsyncThreadQueue(Logger);
        }

        protected override void OnAttached(string name, IServerPart part)
        {
            switch (name)
            {
                case PartNames.Cache:
                    _cacheEngine = (CacheEngine)part;
                    break;
                default:
                    throw new NotImplementedException($"Unknown part: '{name}'");
            }
        }

        protected override void OnStart()
        {
            Logger.LogDebug("StoreEngine: UnitTest");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CommonItem> SyncSelectAllItems()
        {
            return _itemDataTable.Values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        public void AsyncRecvDeleteItem(Guid itemId)
        {
            _databaseQueue.Dispatch(itemId, AsyncExecDeleteItem); //, AsyncQueuePriority.BelowNormal, itemId.ToString());
        }

        private void AsyncExecDeleteItem(Guid itemId)
        {
            SyncDeleteItem(itemId);
        }

        private void SyncDeleteItem(Guid itemId)
        {
            _itemDataTable.Remove(itemId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void AsyncRecvInsertItem(CommonItem item)
        {
            _databaseQueue.Dispatch(item, AsyncExecInsertItem); //, AsyncQueuePriority.Normal, item.UniqueName);
        }

        private void AsyncExecInsertItem(CommonItem item)
        {
            SyncInsertItem(item);
        }

        private void SyncInsertItem(CommonItem item)
        {
            _itemDataTable.Add(item.Id, item);
        }
    }
}
