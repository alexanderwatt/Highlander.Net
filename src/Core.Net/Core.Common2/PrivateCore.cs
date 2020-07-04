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
using System.Linq;
using System.Threading;
using Highlander.Build;
using Highlander.Core.Common.Encryption;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;
using Highlander.Utilities.Threading;

namespace Highlander.Core.Common
{
    public class PrivateCore : ICoreClient
    {
        private readonly TimeSpan _cDebugMsgLifetime = TimeSpan.FromDays(1);

        public IModuleInfo ClientInfo { get; } = new ModuleInfo(BuildConst.BuildEnv, Guid.NewGuid(), "localhost\\user", null, null, null);

        public ServiceAddress ServerAddress => null;

        public ILogger Logger { get; }

        private PrivateEngine _cacheEngine;

        public ICoreClient Proxy => throw new InvalidOperationException();

        public PrivateCore(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheEngine = new PrivateEngine(Logger);
        }

        public void Dispose()
        {
            if (_cacheEngine == null) return;
            _cacheEngine.Dispose();
            _cacheEngine = null;
        }

        // ICoreClient methods
        // debug requests flag
        public bool DebugRequests { get; set; }

        // default object lifetime
        private TimeSpan _defaultLifetime = TimeSpan.MaxValue;

        public TimeSpan DefaultLifetime
        {
            get => _defaultLifetime; set
            {
                if (value < TimeSpan.Zero || value > TimeSpan.MaxValue)
                    throw new ArgumentOutOfRangeException($"DefaultLifetime",
                        $"Must be between {TimeSpan.Zero} and {TimeSpan.MaxValue}");
                _defaultLifetime = value;
            }
        }
        // default app scopes
        private string[] _defaultAppScopes = { AppScopeNames.Legacy };
        public string[] DefaultAppScopes
        {
            get => _defaultAppScopes;
            set
            {
                if (value != null && value.Length > 0)
                    _defaultAppScopes = value;
                else
                    _defaultAppScopes = new[] { AppScopeNames.Legacy };
            }
        }

        // simple client (synchronous) methods
        public ICoreItem LoadItem(Type dataType, string name, bool includeDeleted)
        {
            return _cacheEngine.SelectItem(Guid.Empty, null, name, ItemKind.Object, dataType.FullName, includeDeleted, DateTimeOffset.Now, DebugRequests);
        }

        public ICoreItem LoadItem(Type dataType, string name)
        {
            return _cacheEngine.SelectItem(Guid.Empty, null, name, ItemKind.Object, dataType.FullName, false, DateTimeOffset.Now, DebugRequests);
        }

        public ICoreItem LoadItem(string name)
        {
            return LoadItem(name, ItemKind.Object);
        }

        private ICoreItem LoadItem(string name, ItemKind itemKind)
        {
            return _cacheEngine.SelectItem(Guid.Empty, null, name, itemKind, null, false, DateTimeOffset.Now, DebugRequests);
        }
        public ICoreItem LoadItem<T>(string name)
        {
            return LoadItem<T>(name, ItemKind.Object);
        }

        private ICoreItem LoadItem<T>(string name, ItemKind itemKind)
        {
            return _cacheEngine.SelectItem(Guid.Empty, null, name, itemKind, typeof(T).FullName, false, DateTimeOffset.Now, DebugRequests);
        }

        public ICoreItem LoadItem(Type dataType, Guid id)
        {
            return _cacheEngine.SelectItem(id, null, null, ItemKind.Object, dataType.FullName, false, DateTimeOffset.Now, DebugRequests);
        }

        public ICoreItem LoadItem<T>(Guid id)
        {
            return _cacheEngine.SelectItem(id, null, null, ItemKind.Object, typeof(T).FullName, false, DateTimeOffset.Now, DebugRequests);
        }

        public ICoreItem LoadItem(Guid id)
        {
            return _cacheEngine.SelectItem(id, null, null, ItemKind.Object, null, false, DateTimeOffset.Now, DebugRequests);
        }

        public T LoadObject<T>(string name)
        {
            T result = default;
            PrivateItem item = _cacheEngine.SelectItem(Guid.Empty, null, name, ItemKind.Object, typeof(T).FullName, false, DateTimeOffset.Now, DebugRequests);
            if (item != null)
            {
                result = (T)item.Data;
            }
            return result;
        }

        public T LoadObject<T>(Guid id)
        {
            T result = default;
            PrivateItem item = _cacheEngine.SelectItem(id, null, null, ItemKind.Object, typeof(T).FullName, false, DateTimeOffset.Now, DebugRequests);
            if (item != null)
            {
                if (item.ItemKind != ItemKind.Object)
                    throw new ApplicationException($"Item id ({id}) does not refer to an object!");
                result = (T)item.Data;
            }
            return result;
        }

        public Guid DeleteItem(ICoreItem item)
        {
            PrivateItem item2 = MakePrivateItem(item.ItemKind, item.DataType, null, item.Name, item.AppProps, TimeSpan.Zero, item.Transient);
            _cacheEngine.UpdateItem(item2);
            return item2.Id;
        }

        public ICoreItem MakeItemFromText(string dataTypeName, string text, string name, NamedValueSet props)
        {
            throw new NotSupportedException();
        }

        private static PrivateItem MakePrivateItem(ItemKind itemKind, Type dataType, object data, string name, NamedValueSet props, TimeSpan lifetime, bool transient)
        {
            // note: data == null implies a deleted object
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));
            if (dataType.IsInterface)
                throw new ArgumentException("Cannot be an interface type!", nameof(dataType));
            return new PrivateItem(itemKind, transient, null, name, props, data, dataType, lifetime);
        }

        public ICoreItem MakeTypedItem(Type dataType, object data, string name, NamedValueSet props, bool transient)
        {
            return MakePrivateItem(ItemKind.Object, dataType, data, name, props, (data != null) ? _defaultLifetime : TimeSpan.Zero, transient);
        }

        public ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            ICoreItem item = MakePrivateItem(ItemKind.Object, typeof(T), data, name, props, _defaultLifetime, transient);
            item.Expires = expires;
            return item;
        }
        public ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            ICoreItem item = MakePrivateItem(ItemKind.Object, typeof(T), data, name, props, _defaultLifetime, transient);
            item.Lifetime = lifetime;
            return item;
        }
        public ICoreItem MakeObject<T>(T data, string name, NamedValueSet props)
        {
            return MakeItem(data, name, props, false, TimeSpan.MaxValue);
        }
        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props)
        {
            // note: data == null implies a deleted object 
            PrivateItem item = MakePrivateItem(ItemKind.Object, dataType, data, name, props, (data != null) ? _defaultLifetime : TimeSpan.Zero, false);
            _cacheEngine.UpdateItem(item);
            return item.Id;
        }
        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            // note: data == null implies a deleted object (data != null) ? _DefaultLifetime : TimeSpan.Zero
            PrivateItem item = MakePrivateItem(ItemKind.Object, dataType, data, name, props, TimeSpan.MaxValue, transient);
            item.Expires = expires;
            _cacheEngine.UpdateItem(item);
            return item.Id;
        }
        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            // note: data == null implies a deleted object (data != null) ? _DefaultLifetime : TimeSpan.Zero
            PrivateItem item = MakePrivateItem(ItemKind.Object, dataType, data, name, props, lifetime, transient);
            _cacheEngine.UpdateItem(item);
            return item.Id;
        }
        public Guid SaveUntypedObject(object data, string name, NamedValueSet props)
        {
            ICoreItem item = MakeTypedItem(data.GetType(), data, name, props, false);
            return SaveItem(item);
        }
        public Guid SaveObject<T>(T data, string name, NamedValueSet props, DateTimeOffset expires)
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, name, props, false);
            item.Expires = expires;
            return SaveItem(item);
        }
        public Guid SaveObject<T>(T data, string name, NamedValueSet props, TimeSpan lifetime)
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, name, props, false);
            item.Lifetime = lifetime;
            return SaveItem(item);
        }
        public Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            return SaveTypedObject(typeof(T), data, name, props, transient, expires);
        }
        public Guid SaveObject<T>(T data, string name, NamedValueSet props)
        {
            return SaveTypedObject(typeof(T), data, name, props, false, TimeSpan.MaxValue);
        }
        public Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            return SaveTypedObject(typeof(T), data, name, props, transient, lifetime);
        }
        public Guid SaveObject<T>(T data) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, data.IsTransient);
            item.Lifetime = data.Lifetime;
            return SaveItem(item);
        }
        public Guid SaveObject<T>(T data, TimeSpan lifetime) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, data.IsTransient);
            item.Lifetime = lifetime;
            return SaveItem(item);
        }
        public Guid SaveObject<T>(T data, bool transient, DateTimeOffset expires) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, transient);
            item.Expires = expires;
            return SaveItem(item);
        }
        public Guid SaveObject<T>(T data, bool transient, TimeSpan lifetime) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, transient);
            item.Lifetime = lifetime;
            return SaveItem(item);
        }
        public Guid SaveItem(ICoreItem item)
        {
            PrivateItem item2 = item as PrivateItem ?? MakePrivateItem(item.ItemKind, item.DataType, item.Data, item.Name, item.AppProps, item.Lifetime, item.Transient);
            _cacheEngine.UpdateItem(item2);
            return item2.Id;
        }

        public void SaveItems(IEnumerable<ICoreItem> items)
        {
            foreach (ICoreItem item in items)
                SaveItem(item);
        }

        // multiple object load methods
        public List<ICoreItem> LoadUntypedItems(string dataTypeName, ItemKind itemKind, IExpression whereExpr, bool includeDeleted)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, itemKind, dataTypeName, whereExpr, 0, includeDeleted, DateTimeOffset.Now, DebugRequests);
            List<ICoreItem> results = new List<ICoreItem>();
            foreach (PrivateItem item in items)
                results.Add(item);
            return results;
        }
        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, dataType.FullName, whereExpr, 0, false, DateTimeOffset.Now, DebugRequests);
            List<ICoreItem> results = new List<ICoreItem>();
            foreach (PrivateItem item in items)
                results.Add(item);
            return results;
        }
        public List<ICoreItem> LoadItems<T>(IExpression whereExpr)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, typeof(T).FullName, whereExpr, 0, false, DateTimeOffset.Now, DebugRequests);
            List<ICoreItem> results = new List<ICoreItem>();
            foreach (PrivateItem item in items)
                results.Add(item);
            return results;
        }
        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, long minimumUsn, bool includeDeleted)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, dataType.FullName, whereExpr, minimumUsn, includeDeleted, DateTimeOffset.Now, DebugRequests);
            List<ICoreItem> results = new List<ICoreItem>();
            foreach (PrivateItem item in items)
                results.Add(item);
            return results;
        }
        public List<ICoreItem> LoadItems<T>(IExpression whereExpr, long minimumUsn, bool includeDeleted)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, typeof(T).FullName, whereExpr, minimumUsn, includeDeleted, DateTimeOffset.Now, DebugRequests);
            List<ICoreItem> results = new List<ICoreItem>();
            foreach (PrivateItem item in items)
                results.Add(item);
            return results;
        }

        public List<ICoreItem> LoadItems(IExpression whereExpr)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, null, whereExpr, 0, false, DateTimeOffset.Now, DebugRequests);
            List<ICoreItem> results = new List<ICoreItem>();
            foreach (PrivateItem item in items)
                results.Add(item);
            return results;
        }
        
        public List<T> LoadObjects<T>(IExpression whereExpr)
        {
            List<T> results = new List<T>();
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, typeof(T).FullName, whereExpr, 0, false, DateTimeOffset.Now, DebugRequests);
            foreach (PrivateItem item in items)
                results.Add((T)item.Data);
            return results;
        }
        public List<T> LoadObjects<T>(IExpression whereExpr, long minimumUsn, bool includeDeleted)
        {
            List<T> results = new List<T>();
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, typeof(T).FullName, whereExpr, minimumUsn, includeDeleted, DateTimeOffset.Now, DebugRequests);
            foreach (PrivateItem item in items)
                results.Add((T)item.Data);
            return results;
        }

        // multiple object delete methods
        public int DeleteUntypedObjects(string dataTypeName, IExpression whereExpr)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, dataTypeName, whereExpr, 0, false, DateTimeOffset.Now, DebugRequests);
            foreach (PrivateItem item in items)
            {
                PrivateItem item2 = MakePrivateItem(item.ItemKind, item.DataType, null, item.Name, item.AppProps, TimeSpan.Zero, item.Transient);
                _cacheEngine.UpdateItem(item2);
            }
            return items.Count;
        }
        public int DeleteTypedObjects(Type dataType, IExpression whereExpr)
        {
            return DeleteUntypedObjects(dataType != null ? dataType.FullName : null, whereExpr);
        }
        public int DeleteObjects<T>(IExpression whereExpr)
        {
            return DeleteUntypedObjects(typeof(T).FullName, whereExpr);
        }

        // Paging methods
        public int CountObjects(Type dataType, IExpression whereExpr)
        {
            int result = _cacheEngine.SelectItems(null, ItemKind.Object, dataType.FullName, whereExpr, 0, false, DateTimeOffset.Now, DebugRequests).Count;
            return result;
        }

        public int CountObjects<T>(IExpression whereExpr)
        {
            return CountObjects(typeof(T), whereExpr);
        }

        public List<T> LoadObjects<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, typeof(T).FullName, whereExpr, 0, false, DateTimeOffset.Now, DebugRequests);
            IEnumerable<T> results = items.Select(item => (T)item.Data);
            results = results.Skip(startRow).Take(rowCount);
            return results.ToList();
        }

        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, ItemKind.Object, dataType.FullName, whereExpr, 0, false, DateTimeOffset.Now, DebugRequests);
            IEnumerable<PrivateItem> results = items.Skip(startRow).Take(rowCount);
            return results.Select(item => (ICoreItem)item).ToList();
        }
        public List<ICoreItem> LoadItems<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            return LoadItems(typeof(T), whereExpr, orderExpr, startRow, rowCount);
        }

        // configuration methods
        public void SaveAppSettings(NamedValueSet newSettings, string applName, string userName, string hostName, bool replaceOldSettings)
        {
            SaveAppSettings(newSettings, applName, userName, hostName, replaceOldSettings, ClientInfo.ConfigEnv);
        }

        public void SaveAppSettings(NamedValueSet newSettings, string applName, string userName, string hostName, bool replaceOldSettings, EnvId env)
        {
            // get old record (if any)
            NamedValueSet oldSettings = new NamedValueSet();
            if (!replaceOldSettings)
            {
                // get the old settings
                IExpression query = Expr.BoolAND(
                    Expr.IsEQU(AppPropName.ApplName, applName ?? "*"),
                    Expr.IsEQU(AppPropName.UserName, userName ?? "*"),
                    Expr.IsEQU(AppPropName.HostName, hostName ?? "*"),
                    Expr.IsEQU("Env", (env == EnvId.Undefined) ? "*" : EnvHelper.EnvName(env)));
                List<AppCfgRuleV2> rules = LoadObjects<AppCfgRuleV2>(query);
                // sort rules from lowest to highest priority, then accumulate
                if (rules.Count > 0)
                {
                    rules.Sort();
                    foreach (var rule in rules.Where(rule => !rule.Disabled))
                    {
                        oldSettings.Add(new NamedValueSet(rule.Settings));
                    }
                }
            }
            // now create/update new record
            int priority = 0;
            if (env != EnvId.Undefined)
                priority += 8;
            if (applName != null)
                priority += 4;
            if (userName != null)
                priority += 2;
            if (hostName != null)
                priority += 1;
            AppCfgRuleV2 appCfg = new AppCfgRuleV2();
            NamedValueSet settings = new NamedValueSet();
            settings.Add(oldSettings);
            settings.Add(newSettings);
            appCfg.Settings = settings.Serialise();
            appCfg.Env = (env == EnvId.Undefined) ? "*" : EnvHelper.EnvName(env);
            appCfg.ApplName = applName ?? "*";
            appCfg.UserName = userName ?? "*";
            appCfg.HostName = hostName ?? "*";
            appCfg.Disabled = false;
            appCfg.Priority = priority;
            SaveObject(appCfg, appCfg.ItemName, appCfg.ItemProps, TimeSpan.MaxValue);
        }

        public void SaveAppSettings(NamedValueSet settings)
        {
            SaveAppSettings(settings, ClientInfo.ApplName, ClientInfo.UserName, ClientInfo.HostName, false);
        }

        public void SaveAppSettings(NamedValueSet settings, string applName)
        {
            SaveAppSettings(settings, applName, ClientInfo.UserName, ClientInfo.HostName, false);
        }

        public NamedValueSet LoadAppSettings(string applName, string userName, string hostName)
        {
            NamedValueSet result = new NamedValueSet();
            IExpression query = Expr.BoolAND(
                Expr.BoolOR(Expr.IsEQU(AppPropName.ApplName, "*"),
                    (applName == null ? Expr.IsEQU(AppPropName.ApplName, ClientInfo.ApplName) : Expr.StartsWith(AppPropName.ApplName, applName))),
                Expr.BoolOR(Expr.IsEQU(AppPropName.UserName, "*"),
                    (userName == null ? Expr.IsEQU(AppPropName.UserName, ClientInfo.UserName) : Expr.IsEQU(AppPropName.UserName, userName))),
                Expr.BoolOR(Expr.IsEQU(AppPropName.HostName, "*"),
                    (hostName == null ? Expr.IsEQU(AppPropName.HostName, ClientInfo.HostName) : Expr.IsEQU(AppPropName.HostName, hostName))),
                Expr.BoolOR(Expr.IsEQU("Env", EnvHelper.EnvName(ClientInfo.ConfigEnv)), Expr.IsEQU("Env", "*")));
            //if (DebugRequests)
            //    _Logger.LogDebug("LoadAppSettings: Query={0}", query.DisplayString());
            List<AppCfgRuleV2> rules = LoadObjects<AppCfgRuleV2>(query);
            if (rules.Count > 0)
            {
                rules.Sort();
                int n = 0;
                foreach (AppCfgRuleV2 rule in rules)
                {
                    // config values are cumulative
                    //if (DebugRequests)
                    //    _Logger.LogDebug("LoadAppSettings: Result[{0}]:{1},{2},{3},{4},{5},{6},{7}",
                    //    n, rule.Priority, rule.Disabled, rule.Env, rule.ApplName, rule.UserName, rule.HostName, rule.Settings);
                    if (!rule.Disabled)
                        result.Add(new NamedValueSet(rule.Settings));
                    n++;
                }
            }
            //if(DebugRequests)
            //    _Logger.LogDebug("LoadAppSettings: Results:");
            result.LogValues(delegate(string text)
            {
                if (DebugRequests)
                    Logger.LogDebug("  " + text);
            });
            return result;
        }

        public NamedValueSet LoadAppSettings(string applName)
        {
            return LoadAppSettings(applName, null, null);
        }

        public NamedValueSet LoadAppSettings()
        {
            return LoadAppSettings(null, null, null);
        }

        // default async timeout
        private readonly TimeSpan _minTimeout = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _maxTimeout = TimeSpan.FromSeconds(3600);
        private TimeSpan _defaultTimeout = TimeSpan.FromSeconds(60);
        public TimeSpan RequestTimeout
        {
            get => _defaultTimeout;
            set
            {
                if (value < _minTimeout || value > _maxTimeout)
                    throw new ArgumentOutOfRangeException($"RequestTimeout",
                        $"Must be between {_minTimeout} and {_maxTimeout}");
                _defaultTimeout = value;
            }
        }

        // request count
        private int _MaxRequestCount = 10;
        public int MaxRequestCount
        {
            get => _MaxRequestCount;
            set => throw new NotSupportedException();
        }

        // default serialisation format - not used
        public SerialFormat DefaultSerialFormat { get; set; } = SerialFormat.Undefined;

        public List<ICoreItem> LoadItems(Type dataType, IEnumerable<string> itemNames)
        {
            List<ICoreItem> results = new List<ICoreItem>();
            foreach (string itemName in itemNames)
            {
                PrivateItem item = _cacheEngine.SelectItem(Guid.Empty, null, itemName, ItemKind.Object, dataType.FullName, false, DateTimeOffset.Now, DebugRequests);
                if (item != null)
                    results.Add(item);
            }
            return results;
        }

        public List<ICoreItem> LoadItems(Type dataType, IEnumerable<Guid> itemIds)
        {
            return itemIds.Select(itemId => _cacheEngine.SelectItem(itemId, null, null, ItemKind.Object, dataType.FullName, false, DateTimeOffset.Now, DebugRequests)).Where(item => item != null).Cast<ICoreItem>().ToList();
        }
        public List<ICoreItem> LoadItems<T>(IEnumerable<string> itemNames)
        {
            return LoadItems(typeof(T), itemNames);
        }

        public List<ICoreItem> LoadItems<T>(IEnumerable<Guid> itemIds)
        {
            return LoadItems(typeof(T), itemIds);
        }

        public ICoreItemInfo LoadItemInfo<T>(string name) { return LoadItem(typeof(T), name); }

        public ICoreItemInfo LoadItemInfo(Type dataType, string name) { return LoadItem(dataType, name); }

        public List<ICoreItemInfo> LoadItemInfos(Type dataType, IExpression whereExpr)
        {
            List<ICoreItemInfo> results = new List<ICoreItemInfo>();
            List<ICoreItem> items = LoadItems(dataType, whereExpr);
            foreach (var item in items)
                results.Add(item);
            return results;
        }

        public List<ICoreItemInfo> LoadItemInfos<T>(IExpression whereExpr)
        {
            return LoadItemInfos(typeof(T), whereExpr);
        }

        public List<T> LoadObjects<T>(IEnumerable<string> itemNames)
        {
            List<T> results = new List<T>();
            List<ICoreItem> items = LoadItems(typeof(T), itemNames);
            foreach (ICoreItem item in items)
                results.Add((T)item.Data);
            return results;
        }

        public void SaveDebug<T>(T data, string name, NamedValueSet props) where T : class
        {
            SaveItem(MakePrivateItem(ItemKind.Debug, typeof(T), data, name, props, (data != null) ? _cDebugMsgLifetime : TimeSpan.Zero, true));
        }

        public List<ICoreItem> LoadItems(Type dataType, ItemKind itemKind, IExpression whereExpr, long minimumUsn, bool includeDeleted)
        {
            List<PrivateItem> items = _cacheEngine.SelectItems(null, itemKind, dataType.FullName, whereExpr, minimumUsn, includeDeleted, DateTimeOffset.Now, DebugRequests);
            return items.Cast<ICoreItem>().ToList();
        }

        public IAsyncResult SaveObjectBegin<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires) where T : class
        {
            SaveObject(data, name, props, transient, expires);
            return CompletedResult();
        }

        public void SaveEnd(IAsyncResult ar)
        {
            if (!ar.IsCompleted)
            {
                throw new NotSupportedException("Should be completed.");
            }
        }

        private static IAsyncResult CompletedResult()
        {
            AsyncResult<GuardedList<PrivateItem>> ar = new AsyncResult<GuardedList<PrivateItem>>(null, null);
            ar.SetAsCompleted(null, true);
            return ar;
        }

        public ICoreCache CreateCache()
        {
            return new CoreCache(Logger, this, null, null);
        }
        public ICoreCache CreateCache(CacheChangeHandler changeHandler, SynchronizationContext syncContext)
        {
            return new CoreCache(Logger, this, changeHandler, syncContext);
        }

        // private (in-process only) objects
        public Guid SavePrivateObject<T>(T data, string name, NamedValueSet props)
        {
            return SaveObject(data, name, props);
        }
        public ICoreItem LoadPrivateItem<T>(string name)
        {
            return LoadItem<T>(name);
        }
        public List<ICoreItem> LoadPrivateItems<T>(IExpression whereExpr)
        {
            return LoadItems<T>(whereExpr);
        }

        #region Unsupported ICoreClient methods

        public void SaveRawItem(RawItem storeItem)
        {
            throw new NotSupportedException();
        }
        public void SaveRawItems(IEnumerable<RawItem> storeItems)
        {
            throw new NotSupportedException();
        }
        public ICryptoManager CryptoManager => throw new NotSupportedException();

        public IAsyncResult LoadObjectBegin(AsyncCallback callback, string name, string dataTypeName)
        {
            throw new NotSupportedException();
        }
        public IAsyncResult LoadItemBegin(AsyncCallback callback, Guid id)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult LoadItemBegin(AsyncCallback callback, Type dataType, string name, bool includeDeleted)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult LoadObjectBegin<T>(AsyncCallback callback, string name)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult LoadItemBegin<T>(AsyncCallback callback, Guid id)
        {
            throw new NotSupportedException();
        }
        public ICoreItem LoadItemEnd(IAsyncResult ar)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult LoadObjectsBegin(AsyncCallback callback, IExpression whereExpr, string dataTypeName)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult LoadObjectsBegin<T>(AsyncCallback callback, IExpression whereExpr)
        {
            throw new NotSupportedException();
        }

        public List<ICoreItem> LoadItemsEnd(IAsyncResult ar)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult SaveTypedObjectBegin(Type dataType, object data, string name, NamedValueSet props)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult SaveObjectBegin<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime) where T : class
        {
            throw new NotSupportedException();
        }

        public IAsyncResult SaveObjectBegin<T>(T data, bool transient, TimeSpan lifetime) where T : ICoreObject
        {
            throw new NotSupportedException();
        }

        public IAsyncResult SaveObjectBegin<T>(T data, bool transient, DateTimeOffset expires) where T : ICoreObject
        {
            throw new NotSupportedException();
        }

        public IAsyncResult SaveItemBegin(ICoreItem item)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult SaveItemsBegin(IEnumerable<ICoreItem> item)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult SaveRawItemBegin(RawItem storeItem)
        {
            throw new NotSupportedException();
        }

        public void SaveRawItemEnd(IAsyncResult ar)
        {
            throw new NotSupportedException();
        }

        public ISubscription CreateSubscription<T>(IExpression whereExpr) //, SubscriptionCallback userCallback)
        {
            throw new NotSupportedException();
        }

        public ISubscription CreateTypedSubscription(Type dataType, IExpression whereExpr) //, SubscriptionCallback userCallback)
        {
            throw new NotSupportedException();
        }

        public ISubscription CreateUntypedSubscription(string dataTypeName, IExpression whereExpr)
        {
            throw new NotSupportedException();
        }

        public ISubscription Subscribe<T>(IExpression whereExpr, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotSupportedException();
        }

        public ISubscription Subscribe(Type dataType, IExpression whereExpr, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotSupportedException();
        }

        public ISubscription Subscribe<T>(IExpression filter)
        {
            throw new NotSupportedException();
        }

        public ISubscription SubscribeNoWait<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotSupportedException();
        }

        public ISubscription SubscribeNewOnly<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotSupportedException();
        }

        public ISubscription SubscribeInfoOnly<T>(IExpression filter)
        {
            throw new NotSupportedException();
        }

        public ISubscription StartUntypedSubscription(string dataTypeName, IExpression whereExpr, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotSupportedException();
        }

        public void Unsubscribe(Guid subscriptionId)
        {
            // ignored
        }

        public void UnsubscribeAll()
        {
            // ignored
        }

        public void Clear() { throw new NotSupportedException(); }

        public List<ISubscription> Subscriptions => throw new NotSupportedException();

        public List<ICoreItem> Items => throw new NotSupportedException();

        public int ItemCount => throw new NotSupportedException();

        public int CreateCount => throw new NotSupportedException();

        public int UpdateCount => throw new NotSupportedException();

        public int DeleteCount => throw new NotSupportedException();

        // connection mode notifications
        public event CoreStateHandler OnStateChange;

        private void UserCallbackStateChange(CoreStateChange update)
        {
            try
            {
                OnStateChange?.Invoke(update);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public CoreModeEnum CoreMode => CoreModeEnum.Standard;

        public CoreStateEnum CoreState => CoreStateEnum.Connected;

        #endregion

    }

    internal class PrivateEngine : IDisposable
    {
        private readonly ILogger _logger;
        // item cache
        private long _lastStoreUsn;
        private readonly Guarded<Dictionary<string, PrivateItem>> _itemNameIndex =
            new Guarded<Dictionary<string, PrivateItem>>(new Dictionary<string, PrivateItem>());
        private readonly IScopeManager _appScopeManager = new ScopeManager();

        public PrivateEngine(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose()
        {
            _itemNameIndex.Dispose();
        }

        private void ProcessReceivedItem(PrivateItem item)
        {
            // process a received item
            // - assign a new USN
            item.StoreUSN = Interlocked.Increment(ref _lastStoreUsn);

            try
            {
                // retain if name already exists
                // - except if out-of-date
                string uniqueItemName = item.UniqueName;
                _itemNameIndex.Locked((nameIndex) =>
                {
                    // update the item name index
                    if (nameIndex.TryGetValue(uniqueItemName, out var oldItem))
                    {
                        if (item.Created >= oldItem.Created)
                            nameIndex[uniqueItemName] = item;
                    }
                    else
                        nameIndex[uniqueItemName] = item;
                });
            }
            catch (Exception excp)
            {
                _logger.Log(excp);
            }
        }

        internal void UpdateItem(PrivateItem item)
        {
            item.Freeze();
            ProcessReceivedItem(item);
        }

        public List<PrivateItem> SelectItems(
            string[] appScopes, 
            ItemKind itemKind, 
            string dataType, 
            IExpression query,
            long minimumUsn, 
            bool includeDeleted, 
            DateTimeOffset asAtTime, 
            bool debugRequests)
        {
            if (debugRequests)
            {
                _logger.LogDebug("Local: Selecting...");
                _logger.LogDebug("Local:   AppScopes: {0}", appScopes == null ? "*" : string.Join(",", appScopes));
                _logger.LogDebug("Local:   ItemKind : {0}", itemKind == ItemKind.Undefined ? "(any)" : itemKind.ToString());
                _logger.LogDebug("Local:   DataType : {0}", dataType ?? "(any)");
                if (minimumUsn > 0)
                    _logger.LogDebug("Local:   USN >    : {0}", minimumUsn);
                if (query != null)
                    _logger.LogDebug("Local:   Query    : {0}", query.DisplayString());
                _logger.LogDebug("Local:   Deleted? : {0}", includeDeleted);
            }
            List<PrivateItem> results =
                GetCacheItems(appScopes, itemKind, dataType, query, minimumUsn, includeDeleted, asAtTime);
                if (debugRequests)
                    _logger.LogDebug("Local: Found {0} items.", results.Count);
            return results;
        }

        private static bool ItemMatchesSubscription(
            PrivateItem item, ItemKind itemKind, string[] appScopes, IExpression queryExpr, string dataTypeName, DateTimeOffset asAtTime)
        {
            bool matched = (itemKind == ItemKind.Undefined || item.ItemKind == itemKind)
                           && (string.IsNullOrEmpty(dataTypeName) || item.DataTypeName == dataTypeName);

            if (!matched)
                return false;
            // check item matches appScope list
            if (appScopes != null)
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
                return false;
            // check item matches query
            if (queryExpr != null)
            {
                matched = queryExpr.MatchesProperties(new ExprContext(item.AppProps, item.Name, item.Created, item.Expires));
            }
            return matched;
        }

        private List<PrivateItem> GetCacheItems(string[] appScopes, ItemKind itemKind, string dataType, 
            IExpression query, long minimumUsn, bool includeDeleted, DateTimeOffset asAtTime)
        {
            List<PrivateItem> results = new List<PrivateItem>();
            query ??= Expr.ALL;
            // match subscription to existing cached Events
            _itemNameIndex.Locked(nameIndex =>
            {
                foreach (PrivateItem item in nameIndex.Values)
                {
                    if (ItemMatchesSubscription(item, itemKind, appScopes, query, dataType ?? "", asAtTime))
                    {
                        if (item != null && (item.StoreUSN > minimumUsn) && (includeDeleted || item.IsCurrent(asAtTime)))
                        {
                            results.Add(item);
                        }
                    }
                }
            });
            return results;
        }

        // get single item
        private static PrivateItem GetCacheItem(Guid itemId, bool includeDeleted, DateTimeOffset asAtTime)
        {
            throw new NotSupportedException();
        }

        private PrivateItem GetCacheItem(string itemName, ItemKind itemKind, string appScope, string dataTypeName, bool includeDeleted, DateTimeOffset asAtTime)
        {
            appScope ??= _appScopeManager.DefaultAppScope;
            string uniqueItemName = CoreHelper.MakeUniqueName(itemKind, appScope, itemName);
            PrivateItem result = null;
            _itemNameIndex.Locked(nameIndex =>
            {
                if (nameIndex.TryGetValue(uniqueItemName, out var item))
                {
                    if (dataTypeName == null || item.DataTypeName == dataTypeName)
                    {
                        if (includeDeleted || item.IsCurrent(asAtTime))
                        {
                            result = item;
                        }
                    }
                }
            });
            return result;
        }

        public PrivateItem SelectItem(Guid itemId, string appScope, string itemName, ItemKind itemKind, string dataType, bool includeDeleted, DateTimeOffset asAtTime, bool debugRequests)
        {
            if (debugRequests)
            {
                _logger.LogDebug("Local: SelectItem: Running query");
                if (itemId == Guid.Empty)
                {
                    _logger.LogDebug("Local:   ItemName: {0}", itemName);
                    _logger.LogDebug("Local:   ItemType: {0}", itemKind == ItemKind.Undefined ? "(null)" : itemKind.ToString());
                    _logger.LogDebug("Local:   AppScope: {0}", appScope ?? "(null)");
                    _logger.LogDebug("Local:   DataType: {0}", dataType ?? "(null)");
                }
                else
                {
                    _logger.LogDebug("Local:   ItemId  : {0}", itemId == Guid.Empty ? "(null)" : itemId.ToString());
                }
            }
            var result = itemId == Guid.Empty ? GetCacheItem(itemName, itemKind, appScope, dataType, includeDeleted, asAtTime) : GetCacheItem(itemId, includeDeleted, asAtTime);
            return result;
        }

    }

    internal class PrivateItem : ICoreItem
    {
        // managed state
        // immutable state

        public ItemKind ItemKind { get; }

        public bool Transient { get; }

        public Guid Id { get; }

        public string Name { get; }

        private string _uniqueName;

        public string UniqueName => _uniqueName ??= CoreHelper.MakeUniqueName(ItemKind, AppScope, Name);

        // mutable until frozen (committed)
        private Type _dataTypeType;

        private DateTimeOffset _created;

        public DateTimeOffset Created => _created;

        private bool _useExpiry;

        private TimeSpan _lifetime;

        private DateTimeOffset _expires;

        public NamedValueSet AppProps { get; } = new NamedValueSet();

        public NamedValueSet SysProps { get; } = new NamedValueSet();

        public string DataTypeName { get; protected set; }

        public Int64 StoreUSN { get; set; }

        // constructors
        public PrivateItem(ItemKind itemKind, bool transient, string appScope, string itemName, NamedValueSet props, object data, Type dataType, TimeSpan lifetime)
        {
            ItemKind = itemKind;
            Transient = transient;
            Id = Guid.NewGuid();
            AppScope = appScope ?? AppScopeNames.Legacy;
            Name = itemName;
            AppProps.Add(props);
            _dataTypeType = dataType;
            DataTypeName = dataType.FullName;
            Data = data;
            _lifetime = lifetime;
        }

        public string AppScope { get; }

        public string NetScope { get; protected set; }

        public void SetNetScope(string netScope)
        {
            NetScope = netScope;
        }

        public bool IsCurrent(DateTimeOffset asAtTime)
        {
            DateTimeOffset expires = Frozen ? _expires : _useExpiry ? _expires : DateTimeOffset.Now + _lifetime;
            return (asAtTime < expires);
        }

        public bool IsCurrent()
        {
            return IsCurrent(DateTimeOffset.Now);
        }

        public bool Frozen { get; private set; }

        protected void CheckNotFrozen()
        {
            if (Frozen)
                throw new ApplicationException("Item already frozen/saved!");
        }
        public void Freeze()
        {
            if (Frozen)
                return;
            if (Name == null)
                throw new ApplicationException("Item name not set!");
            TimeSpan maxLifetime = DateTimeOffset.MaxValue - DateTimeOffset.Now - TimeSpan.FromDays(1);
            _created = DateTimeOffset.Now;
            if (_useExpiry)
            {
                if (_expires < _created)
                    _expires = _created;
            }
            else
            {
                if (_lifetime > maxLifetime)
                    _lifetime = maxLifetime;
                if (_lifetime < TimeSpan.Zero)
                    _lifetime = TimeSpan.Zero;
                _expires = _created.Add(_lifetime);
            }
            DataTypeName ??= "";
            // done
            Frozen = true;
        }
        // mutable props
        public bool IsSigned => false;

        public bool IsSecret => false;

        public string Text => null;

        public object Data { get; private set; }

        public object GetData(Type dataType, bool binaryClone)
        {
            return binaryClone ? BinarySerializerHelper.Clone(Data) : Data;
        }

        public T GetData<T>(bool binaryClone) where T : class
        {
            return (T)GetData(typeof(T), binaryClone);
        }

        public Type DataType => _dataTypeType;

        public void SetData(Type dataType, object data)
        {
            CheckNotFrozen();
            Data = data;
            _dataTypeType = dataType;
            DataTypeName = dataType.FullName;
        }
        public void SetData<T>(T data) where T : class
        {
            SetData(typeof(T), data);
        }
        public void SetData(object data)
        {
            SetData(data.GetType(), data);
        }
        public void SetText(string text, string dataTypeName)
        {
            throw new NotSupportedException();
        }
        public void SetText(string text, Type dataType)
        {
            throw new NotSupportedException();
        }
        public byte[] YData => throw new NotSupportedException();

        public Guid YDataHash => throw new NotSupportedException();

        public byte[] YSign => throw new NotSupportedException();

        public DateTimeOffset Expires
        {
            get
            {
                if (Frozen || _useExpiry)
                    return _expires;
                return (DateTimeOffset.Now + _lifetime);
            }
            set
            {
                CheckNotFrozen();
                _useExpiry = true;
                _expires = value;
            }
        }
        public TimeSpan Lifetime
        {
            get
            {
                if (Frozen || _useExpiry)
                    return (_expires - DateTimeOffset.Now);
                return _lifetime;
            }
            set
            {
                CheckNotFrozen();
                _useExpiry = false;
                _lifetime = value;
            }
        }
        public string TranspKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.Xtki, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.Xtki, value); }
        }
        public string SenderKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.Yski, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.Yski, value); }
        }
        public string RecverKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.Yrki, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.Yrki, value); }
        }
        public SerialFormat SerialFormat
        {
            get => SerialFormat.Undefined;
            set => throw new NotSupportedException();
        }
    }
}
