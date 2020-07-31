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
using System.Threading;
using Highlander.Utilities.Encryption;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.NamedValues;

namespace Highlander.Core.Common
{
    /// <summary>
    /// Core modes
    /// </summary>
    public enum CoreModeEnum
    {
        /// <summary>
        /// Default mode designed for transient use (eg. within web service request handlers).
        /// - All exceptions are propagated to the caller.
        /// - Subscriptions are not supported.
        /// Automatic recovery mode. Transient mode plus:
        /// - communication exceptions are managed;
        /// - subscriptions supported;
        /// - offline state not allowed.
        /// </summary>
        Standard,

        /// <summary>
        /// Not supported yet.
        /// Offline/reliable mode. Auto-recovery mode where Offline state is allowed. Uses a reliable
        /// transport (MSMQ) to communicate with servers.
        /// </summary>
        Reliable
    }

    public enum CoreStateEnum
    {
        Initial,
        Connecting,
        Connected,
        Offline,
        Disposed,
        Faulted
    }

    public class CoreStateChange
    {
        public readonly CoreStateEnum OldState;

        public readonly CoreStateEnum NewState;

        public CoreStateChange(CoreStateEnum oldState, CoreStateEnum newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    public delegate void CoreStateHandler(CoreStateChange update);

    public interface ICoreClient : ICoreCache
    {
        // debug support
        bool DebugRequests { get; set; }

        ServiceAddress ServerAddress { get; }

        // config properties
        TimeSpan DefaultLifetime { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        string[] DefaultAppScopes { get; set; }

        // simple client (synchronous) methods
        // item load methods
        ICoreItem LoadItem(string name);
        ICoreItem LoadItem<T>(Guid id);
        ICoreItem LoadItem(Type dataType, Guid id);
        ICoreItem LoadItem(Guid id);
        ICoreItem LoadItem(Type dataType, string name, bool includeDeleted);
        // object load methods
        T LoadObject<T>(Guid id);
        // make item methods
        ICoreItem MakeObject<T>(T data, string name, NamedValueSet props);
        ICoreItem MakeTypedItem(Type dataType, object data, string name, NamedValueSet props, bool transient);
        ICoreItem MakeItemFromText(string dataTypeName, string serialisedData, string name, NamedValueSet props);
        // save pre-built items
        void SaveItems(IEnumerable<ICoreItem> item);
        // save object methods
        Guid SaveObject<T>(T data, bool transient, TimeSpan lifetime) where T : ICoreObject;
        Guid SaveObject<T>(T data, bool transient, DateTimeOffset expires) where T : ICoreObject;
        Guid SaveUntypedObject(object data, string name, NamedValueSet props);

        // multiple object load methods
        // - typed objects
        List<T> LoadObjects<T>(IExpression whereExpr, long minimumUsn, bool includeDeleted);
        // - generic items
        List<ICoreItem> LoadItems(IExpression whereExpr);
        // - advanced
        List<ICoreItem> LoadItems<T>(IExpression whereExpr, long minimumUsn, bool includeDeleted);
        List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, long minimumUsn, bool includeDeleted);
        List<ICoreItem> LoadUntypedItems(string dataTypeName, ItemKind itemKind, IExpression whereExpr, bool includeDeleted);

        // delete methods
        int DeleteUntypedObjects(string dataTypeName, IExpression whereExpr);

        // paging methods
        int CountObjects(Type dataType, IExpression whereExpr);
        int CountObjects<T>(IExpression whereExpr);
        List<T> LoadObjects<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);
        List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);
        List<ICoreItem> LoadItems<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);

        // connection control
        int MaxRequestCount { get; set; }
        TimeSpan RequestTimeout { get; set; }

        // extended client methods
        // - configuration methods
        NamedValueSet LoadAppSettings();
        NamedValueSet LoadAppSettings(string applName);
        NamedValueSet LoadAppSettings(string applName, string userName, string hostName);
        void SaveAppSettings(NamedValueSet settings);
        void SaveAppSettings(NamedValueSet settings, string applName);
        void SaveAppSettings(NamedValueSet settings, string applName, string userName, string hostName, bool replaceOldSettings);
        void SaveAppSettings(NamedValueSet settings, string applName, string userName, string hostName, bool replaceOldSettings, EnvId envId);

        // - serialisation control
        SerialFormat DefaultSerialFormat { get; set; }

        // - data privacy
        ICryptoManager CryptoManager { get; }

        // - other item kinds
        List<ICoreItem> LoadItems(Type dataType, ItemKind itemKind, IExpression whereExpr, long minimumUsn, bool includeDeleted);

        // - debug item methods
        void SaveDebug<T>(T data, string name, NamedValueSet props) where T : class;

        // asynchronous methods
        // - single object load
        IAsyncResult LoadObjectBegin(AsyncCallback callback, string name, string dataTypeName);
        IAsyncResult LoadItemBegin(AsyncCallback callback, Guid id);
        IAsyncResult LoadObjectBegin<T>(AsyncCallback callback, string name);
        IAsyncResult LoadItemBegin(AsyncCallback callback, Type dataType, string name, bool includeDeleted);
        IAsyncResult LoadItemBegin<T>(AsyncCallback callback, Guid id);
        ICoreItem LoadItemEnd(IAsyncResult ar);
        // - multiple object load
        IAsyncResult LoadObjectsBegin(AsyncCallback callback, IExpression whereExpr, string dataTypeName);
        IAsyncResult LoadObjectsBegin<T>(AsyncCallback callback, IExpression whereExpr);
        List<ICoreItem> LoadItemsEnd(IAsyncResult ar);
        IAsyncResult SaveTypedObjectBegin(Type dataType, object data, string name, NamedValueSet props);
        IAsyncResult SaveObjectBegin<T>(T data, bool transient, TimeSpan lifetime) where T : ICoreObject;
        IAsyncResult SaveObjectBegin<T>(T data, bool transient, DateTimeOffset expires) where T : ICoreObject;
        IAsyncResult SaveObjectBegin<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime) where T : class;
        IAsyncResult SaveObjectBegin<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires) where T : class;
        IAsyncResult SaveItemBegin(ICoreItem item);
        IAsyncResult SaveItemsBegin(IEnumerable<ICoreItem> item);
        void SaveEnd(IAsyncResult ar);

        // - raw items
        IAsyncResult SaveRawItemBegin(RawItem storeItem);
        void SaveRawItemEnd(IAsyncResult ar);
        void SaveRawItem(RawItem storeItem);
        void SaveRawItems(IEnumerable<RawItem> storeItems);

        // - subscriptions
        /// <summary>
        /// Starts a subscription.
        /// </summary>
        /// <param name="dataTypeName">The full type name of the data wanted.</param>
        /// <param name="whereExpr">The filter expression</param>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="userContext">An optional user context.</param>
        /// <returns></returns>
        ISubscription StartUntypedSubscription(string dataTypeName, IExpression whereExpr, SubscriptionCallback userCallback, object userContext);
        /// <summary>
        /// Creates an un-started subscription.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr">The filter expression</param>
        /// <returns></returns>
        ISubscription CreateSubscription<T>(IExpression whereExpr); //, SubscriptionCallback userCallback);
        /// <summary>
        /// Creates an un-started subscription.
        /// </summary>
        /// <param name="whereExpr">The filter expression</param>
        /// <param name="dataType">The type of the data wanted</param>
        /// <returns></returns>
        ISubscription CreateTypedSubscription(Type dataType, IExpression whereExpr); //, SubscriptionCallback userCallback);
        /// <summary>
        /// Creates an un-started subscription.
        /// </summary>
        /// <param name="dataTypeName">The type of the data wanted</param>
        /// <param name="whereExpr">The filter expression</param>
        /// <returns></returns>
        ISubscription CreateUntypedSubscription(string dataTypeName, IExpression whereExpr); //, SubscriptionCallback userCallback);

        // paging methods
        //int CountObjects(Type dataType, IExpression whereExpr);
        //int CountObjects<T>(IExpression whereExpr);
        //List<T> LoadObjects<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);
        List<T> LoadObjects<T>(IEnumerable<string> itemNames);
        //List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);
        List<ICoreItem> LoadItems(Type dataType, IEnumerable<string> itemNames);
        List<ICoreItem> LoadItems(Type dataType, IEnumerable<Guid> itemIds);
        List<ICoreItem> LoadItems<T>(IEnumerable<Guid> itemIds);

        // mode change notifications
        event CoreStateHandler OnStateChange;
        CoreModeEnum CoreMode { get; }
        CoreStateEnum CoreState { get; }

        // cache constructors
        ICoreCache CreateCache();
        ICoreCache CreateCache(CacheChangeHandler changeHandler, SynchronizationContext syncContext);
    }
}
