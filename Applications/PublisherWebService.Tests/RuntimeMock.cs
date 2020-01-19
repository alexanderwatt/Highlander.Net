using System;
using System.Collections.Generic;
using System.Threading;
using Core.Common;
using Core.Common.Encryption;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;

namespace PublisherWebService.Tests
{
    internal class RuntimeMock : ICoreClient
    {
        public string PublishedData { get; private set; }
        public string PublishedName { get; private set; }
        public NamedValueSet PublishedProperties { get; private set; }
        public TimeSpan PublishedTimeSpan { get; private set; }
        //public DateTimeOffset PublishedExpiry { get; private set; }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props, TimeSpan expires)
        {
            PublishedData = XmlSerializerHelper.SerializeToString(data);
            PublishedName = name;
            PublishedProperties = props;
            PublishedTimeSpan = expires;
            return new Guid();
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of ICoreCache

        public ILogger Logger
        {
            get { throw new NotImplementedException(); }
        }

        public IModuleInfo ClientInfo
        {
            get { throw new NotImplementedException(); }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public ICoreItem LoadItem<T>(string name)
        {
            throw new NotImplementedException();
        }

        public ICoreItem LoadItem(Type dataType, string name)
        {
            throw new NotImplementedException();
        }

        public ICoreItemInfo LoadItemInfo<T>(string name)
        {
            throw new NotImplementedException();
        }

        public ICoreItemInfo LoadItemInfo(Type dataType, string name)
        {
            throw new NotImplementedException();
        }

        public T LoadObject<T>(string name)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems<T>(IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems<T>(IEnumerable<string> itemNames)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItemInfo> LoadItemInfos(Type dataType, IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItemInfo> LoadItemInfos<T>(IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public List<T> LoadObjects<T>(IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            throw new NotImplementedException();
        }

        public ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            throw new NotImplementedException();
        }

        public Guid SaveItem(ICoreItem item)
        {
            throw new NotImplementedException();
        }

        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props)
        {
            throw new NotImplementedException();
        }

        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            throw new NotImplementedException();
        }

        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            throw new NotImplementedException();
        }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props)
        {
            throw new NotImplementedException();
        }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props, DateTimeOffset expires)
        {
            throw new NotImplementedException();
        }

        //public Guid SaveObject<T>(T data, string name, NamedValueSet props, TimeSpan lifetime)
        //{
        //    throw new NotImplementedException();
        //}

        public Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            throw new NotImplementedException();
        }

        public Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            throw new NotImplementedException();
        }

        public Guid SaveObject<T>(T data) where T : ICoreObject
        {
            throw new NotImplementedException();
        }

        public Guid SaveObject<T>(T data, TimeSpan lifetime) where T : ICoreObject
        {
            throw new NotImplementedException();
        }

        public Guid SavePrivateObject<T>(T data, string name, NamedValueSet props)
        {
            throw new NotImplementedException();
        }

        public ICoreItem LoadPrivateItem<T>(string name)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadPrivateItems<T>(IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public Guid DeleteItem(ICoreItem item)
        {
            throw new NotImplementedException();
        }

        public int DeleteTypedObjects(Type dataType, IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public int DeleteObjects<T>(IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancels one subscription.
        /// </summary>
        public void Unsubscribe(Guid subscriptionId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancels all subscriptions.
        /// </summary>
        public void UnsubscribeAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and starts a subscription.
        /// </summary>
        /// <param name="dataType">The type of the data wanted</param>
        /// <param name="filter">The filter expression</param>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="userContext">An optional user context.</param>
        /// <returns></returns>
        public ISubscription Subscribe(Type dataType, IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and starts a subscription.
        /// </summary>
        /// <typeparam name="T">The type of the data wanted</typeparam>
        /// <param name="filter">The filter expression</param>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="userContext">An optional user context.</param>
        /// <returns></returns>
        public ISubscription Subscribe<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotImplementedException();
        }

        public ISubscription Subscribe<T>(IExpression filter)
        {
            throw new NotImplementedException();
        }

        public ISubscription SubscribeNoWait<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotImplementedException();
        }

        public ISubscription SubscribeNewOnly<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotImplementedException();
        }

        public ISubscription SubscribeInfoOnly<T>(IExpression filter)
        {
            throw new NotImplementedException();
        }

        public List<ISubscription> Subscriptions
        {
            get { throw new NotImplementedException(); }
        }

        public List<ICoreItem> Items
        {
            get { throw new NotImplementedException(); }
        }

        public int ItemCount
        {
            get { throw new NotImplementedException(); }
        }

        public int CreateCount
        {
            get { throw new NotImplementedException(); }
        }

        public int UpdateCount
        {
            get { throw new NotImplementedException(); }
        }

        public int DeleteCount
        {
            get { throw new NotImplementedException(); }
        }

        public ICoreClient Proxy
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Implementation of ICoreClient

        public bool DebugRequests
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ServiceAddress ServerAddress
        {
            get { throw new NotImplementedException(); }
        }

        public TimeSpan DefaultLifetime
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string[] DefaultAppScopes
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ICoreItem LoadItem(string name)
        {
            throw new NotImplementedException();
        }

        public ICoreItem LoadItem<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICoreItem LoadItem(Type dataType, Guid id)
        {
            throw new NotImplementedException();
        }

        public ICoreItem LoadItem(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICoreItem LoadItem(Type dataType, string name, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public T LoadObject<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICoreItem MakeObject<T>(T data, string name, NamedValueSet props)
        {
            throw new NotImplementedException();
        }

        public ICoreItem MakeTypedItem(Type dataType, object data, string name, NamedValueSet props, bool transient)
        {
            throw new NotImplementedException();
        }

        public ICoreItem MakeItemFromText(string dataTypeName, string serialisedData, string name, NamedValueSet props)
        {
            throw new NotImplementedException();
        }

        public void SaveItems(IEnumerable<ICoreItem> item)
        {
            throw new NotImplementedException();
        }

        public Guid SaveObject<T>(T data, bool transient, TimeSpan lifetime) where T : ICoreObject
        {
            throw new NotImplementedException();
        }

        public Guid SaveObject<T>(T data, bool transient, DateTimeOffset expires) where T : ICoreObject
        {
            throw new NotImplementedException();
        }

        public Guid SaveUntypedObject(object data, string name, NamedValueSet props)
        {
            throw new NotImplementedException();
        }

        public List<T> LoadObjects<T>(IExpression whereExpr, long minimumUSN, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems(IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems<T>(IExpression whereExpr, long minimumUSN, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, long minimumUSN, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadUntypedItems(string dataTypeName, ItemKind itemKind, IExpression whereExpr, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public int DeleteUntypedObjects(string dataTypeName, IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public int CountObjects(Type dataType, IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public int CountObjects<T>(IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public List<T> LoadObjects<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            throw new NotImplementedException();
        }

        public int MaxRequestCount
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public TimeSpan RequestTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public NamedValueSet LoadAppSettings()
        {
            throw new NotImplementedException();
        }

        public NamedValueSet LoadAppSettings(string applName)
        {
            throw new NotImplementedException();
        }

        public NamedValueSet LoadAppSettings(string applName, string userName, string hostName)
        {
            throw new NotImplementedException();
        }

        public void SaveAppSettings(NamedValueSet settings)
        {
            throw new NotImplementedException();
        }

        public void SaveAppSettings(NamedValueSet settings, string applName)
        {
            throw new NotImplementedException();
        }

        public void SaveAppSettings(NamedValueSet settings, string applName, string userName, string hostName, bool replaceOldSettings)
        {
            throw new NotImplementedException();
        }

        public void SaveAppSettings(NamedValueSet settings, string applName, string userName, string hostName, bool replaceOldSettings, EnvId envId)
        {
            throw new NotImplementedException();
        }

        public SerialFormat DefaultSerialFormat
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ICryptoManager CryptoManager
        {
            get { throw new NotImplementedException(); }
        }

        public List<ICoreItem> LoadItems(Type dataType, ItemKind itemKind, IExpression whereExpr, long minimumUSN, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public void SaveDebug<T>(T data, string name, NamedValueSet props) where T : class
        {
            throw new NotImplementedException();
        }

        public IAsyncResult LoadObjectBegin(AsyncCallback callback, string name, string dataTypeName)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult LoadItemBegin(AsyncCallback callback, Guid id)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult LoadObjectBegin<T>(AsyncCallback callback, string name)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult LoadItemBegin(AsyncCallback callback, Type dataType, string name, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult LoadItemBegin<T>(AsyncCallback callback, Guid id)
        {
            throw new NotImplementedException();
        }

        public ICoreItem LoadItemEnd(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult LoadObjectsBegin(AsyncCallback callback, IExpression whereExpr, string dataTypeName)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult LoadObjectsBegin<T>(AsyncCallback callback, IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItemsEnd(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult SaveTypedObjectBegin(Type dataType, object data, string name, NamedValueSet props)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult SaveObjectBegin<T>(T data, bool transient, TimeSpan lifetime) where T : ICoreObject
        {
            throw new NotImplementedException();
        }

        public IAsyncResult SaveObjectBegin<T>(T data, bool transient, DateTimeOffset expires) where T : ICoreObject
        {
            throw new NotImplementedException();
        }

        public IAsyncResult SaveObjectBegin<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime) where T : class
        {
            throw new NotImplementedException();
        }

        public IAsyncResult SaveObjectBegin<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires) where T : class
        {
            throw new NotImplementedException();
        }

        public IAsyncResult SaveItemBegin(ICoreItem item)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult SaveItemsBegin(IEnumerable<ICoreItem> item)
        {
            throw new NotImplementedException();
        }

        public void SaveEnd(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult SaveRawItemBegin(RawItem storeItem)
        {
            throw new NotImplementedException();
        }

        public void SaveRawItemEnd(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        public void SaveRawItem(RawItem storeItem)
        {
            throw new NotImplementedException();
        }

        public void SaveRawItems(IEnumerable<RawItem> storeItems)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts a subscription.
        /// </summary>
        /// <param name="dataTypeName">The full type name of the data wanted.</param>
        /// <param name="whereExpr">The filter expression</param>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="userContext">An optional user context.</param>
        /// <returns></returns>
        public ISubscription StartUntypedSubscription(string dataTypeName, IExpression whereExpr, SubscriptionCallback userCallback, object userContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an unstarted subscription.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr">The filter expression</param>
        /// <returns></returns>
        public ISubscription CreateSubscription<T>(IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an unstarted subscription.
        /// </summary>
        /// <param name="whereExpr">The filter expression</param>
        /// <param name="dataType">The type of the data wanted</param>
        /// <returns></returns>
        public ISubscription CreateTypedSubscription(Type dataType, IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an unstarted subscription.
        /// </summary>
        /// <param name="dataTypeName">The type of the data wanted</param>
        /// <param name="whereExpr">The filter expression</param>
        /// <returns></returns>
        public ISubscription CreateUntypedSubscription(string dataTypeName, IExpression whereExpr)
        {
            throw new NotImplementedException();
        }

        public List<T> LoadObjects<T>(IEnumerable<string> itemNames)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems(Type dataType, IEnumerable<string> itemNames)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems(Type dataType, IEnumerable<Guid> itemIds)
        {
            throw new NotImplementedException();
        }

        public List<ICoreItem> LoadItems<T>(IEnumerable<Guid> itemIds)
        {
            throw new NotImplementedException();
        }

        public event CoreStateHandler OnStateChange;

        public CoreModeEnum CoreMode
        {
            get { throw new NotImplementedException(); }
        }

        public CoreStateEnum CoreState
        {
            get { throw new NotImplementedException(); }
        }

        public ICoreCache CreateCache()
        {
            throw new NotImplementedException();
        }

        public ICoreCache CreateCache(CacheChangeHandler changeHandler, SynchronizationContext syncContext)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
