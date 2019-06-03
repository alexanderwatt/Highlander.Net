using System;
using System.Collections.Generic;
using National.QRSC.Runtime.Common;
using National.QRSC.Runtime.Encryption;
using National.QRSC.Runtime.Expressions;
using National.QRSC.ServiceModel;
using National.QRSC.Utility.Logging;
using National.QRSC.Utility.NamedValues;

namespace National.QRSC.Runtime.V32
{
    /// <summary>
    /// The simple client (synchronous) interface of the CoreClient class.
    /// </summary>
    public interface ICoreClient : IDisposable
    {
        // debug support
        bool DebugRequests { get; set; }
        ILogger Logger { get; }
        // info properties
        IModuleInfo ClientInfo { get; }
        // config properties
        TimeSpan DefaultLifetime { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        string[] DefaultAppScopes { get; set; }

        // simple client (synchronous) methods
        // item load methods
        ICoreItem LoadItem<T>(string name);
        ICoreItem LoadItem(Type dataType, string name);
        ICoreItem LoadItem(string name);
        ICoreItem LoadItem<T>(Guid id);
        ICoreItem LoadItem(Type dataType, Guid id);
        ICoreItem LoadItem(Guid id);
        // object load methods
        T LoadObject<T>(Guid id);
        T LoadObject<T>(string name);
        // make item methods
        ICoreItem MakeObject(Type dataType, object data, string name, NamedValueSet props);
        ICoreItem MakeObject<T>(T data, string name, NamedValueSet props) where T : class;
        // save methods
        Guid SaveObject(Type dataType, object data, string name, NamedValueSet props, TimeSpan lifetime);
        Guid SaveObject<T>(T data, string name, NamedValueSet props, TimeSpan lifetime) where T : class;
        Guid SaveItem(ICoreItem item);
        void SaveItems(IEnumerable<ICoreItem> item);

        // multiple object load methods
        // - typed objects
        List<T> LoadObjects<T>(IExpression whereExpr);
        List<T> LoadObjects<T>(IExpression whereExpr, long minimumUSN, bool includeDeleted);
        // - generic items
        List<ICoreItem> LoadItems<T>(IExpression whereExpr);
        List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr);
        List<ICoreItem> LoadItems(IExpression whereExpr);
        // -advanced
        List<ICoreItem> LoadItems<T>(IExpression whereExpr, long minimumUSN, bool includeDeleted);
        List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, long minimumUSN, bool includeDeleted);

        // delete methods
        Guid DeleteItem(ICoreItem item);
        void DeleteObjects(IExpression whereExpr, string dataTypeName);
        void DeleteObjects<T>(IExpression whereExpr);

        // paging methods
        int CountObjects(Type dataType, IExpression whereExpr);
        int CountObjects<T>(IExpression whereExpr);
        List<T> LoadObjects<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);
        List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);
    }

    public interface ICoreClientEx : ICoreClient
    {
        // extended client methods
        // - raw items
        IAsyncResult SaveRawItemBegin(V32StoreItem storeItem);
        void SaveRawItemEnd(IAsyncResult ar);
        void SaveRawItem(V32StoreItem storeItem);
        void SaveRawItems(IEnumerable<V32StoreItem> storeItems);

        // - configuration methods
        NamedValueSet LoadAppSettings();
        NamedValueSet LoadAppSettings(string applName);
        NamedValueSet LoadAppSettings(string applName, string userName, string hostName);
        void SaveAppSettings(NamedValueSet settings);
        void SaveAppSettings(NamedValueSet settings, string applName);
        void SaveAppSettings(NamedValueSet settings, string applName, string userName, string hostName, bool replaceOldSettings);

        // - serialisation control
        SerialFormat DefaultSerialFormat { get; set; }

        // - data privacy
        ICryptoManager CryptoManager { get; }

        // - other item kinds
        List<ICoreItem> LoadItems(Type dataType, ItemKind itemKind, IExpression whereExpr, long minimumUSN, bool includeDeleted);

        // - debug item methods
        void SaveDebug<T>(T data, string name, NamedValueSet props) where T : class;

        // asynchronous methods
        TimeSpan DefaultTimeout { get; set; }
        // - single object load
        IAsyncResult LoadObjectBegin(AsyncCallback callback, string name, string dataTypeName);
        IAsyncResult LoadItemBegin(AsyncCallback callback, Guid id);
        IAsyncResult LoadObjectBegin<T>(AsyncCallback callback, string name);
        IAsyncResult LoadItemBegin<T>(AsyncCallback callback, Guid id);
        ICoreItem LoadObjectEnd(IAsyncResult ar);
        //ICoreItem LoadObjectEnd(IAsyncResult ar, TimeSpan timeout);
        // - multiple object load
        IAsyncResult LoadObjectsBegin(AsyncCallback callback, IExpression whereExpr, string dataTypeName);
        IAsyncResult LoadObjectsBegin<T>(AsyncCallback callback, IExpression whereExpr);
        List<ICoreItem> LoadObjectsEnd(IAsyncResult ar);
        //List<ICoreItem> LoadObjectsEnd(IAsyncResult ar, TimeSpan timeout);
        IAsyncResult SaveObjectBegin(object data, string name, NamedValueSet props);
        IAsyncResult SaveObjectBegin(string data, string name, NamedValueSet props, string dataTypeName);
        void SaveObjectEnd(IAsyncResult ar);

        /// <summary>
        /// Creates a subscription.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="whereExpr">The filter expression</param>
        /// <returns></returns>
        ISubscription StartSubscription<T>(IExpression whereExpr, SubscriptionCallback userCallback);
        /// <summary>
        /// Creates an unstarted subscription.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="whereExpr">The filter expression</param>
        /// <returns></returns>
        ISubscription CreateSubscription<T>(IExpression whereExpr, SubscriptionCallback userCallback);
        /// <summary>
        /// Creates the subscription.
        /// </summary>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="whereExpr">The filter expression</param>
        /// <param name="dataType">The type of the data wanted</param>
        /// <returns></returns>
        ISubscription StartSubscription(Type dataType, IExpression whereExpr, SubscriptionCallback userCallback);
        /// <summary>
        /// Creates an unstarted subscription.
        /// </summary>
        /// <returns></returns>
        ISubscription CreateSubscription();
        /// <summary>
        /// Creates an unstarted subscription.
        /// </summary>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="whereExpr">The filter expression</param>
        /// <param name="dataType">The type of the data wanted</param>
        /// <returns></returns>
        ISubscription CreateSubscription(Type dataType, IExpression whereExpr, SubscriptionCallback userCallback);

        /// <summary>
        /// Cancels all subscriptions currently managed by this client.
        /// </summary>
        void Unsubscribe();

        // paging methods
        //int CountObjects(Type dataType, IExpression whereExpr);
        //int CountObjects<T>(IExpression whereExpr);
        //List<T> LoadObjects<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);
        List<T> LoadObjects<T>(IEnumerable<string> itemNames);
        //List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount);
        List<ICoreItemInfo> LoadItemInfo(Type dataType, IExpression whereExpr);
        List<ICoreItemInfo> LoadItemInfo<T>(IExpression whereExpr);
        List<ICoreItem> LoadItems(Type dataType, IEnumerable<string> itemNames);
        List<ICoreItem> LoadItems(Type dataType, IEnumerable<Guid> itemIds);
    }

    /// <summary>
    /// Client-side data transfer object for core items
    /// </summary>
    [Serializable]
    public class V32StoreItem
    {
        public Guid ItemId { get; private set; }
        public int ItemKind { get; private set; }
        public string AppScope { get; private set; }
        public string NetScope { get; private set; }
        public string DataType { get; private set; }
        public string ItemName { get; private set; }
        public string AppProps { get; private set; }
        public string SysProps { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset Expires { get; private set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] YData { get; private set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] YSign { get; private set; }
        public Int64 StoreUSN { get; private set; }
        // constructors
        public V32StoreItem(ICoreItem item)
        {
            ItemKind = (int)item.ItemKind;
            ItemId = item.Id;
            AppScope = item.AppScope;
            NetScope = item.NetScope;
            ItemName = item.Name;
            AppProps = item.AppProps.Serialise();
            SysProps = item.SysProps.Serialise();
            DataType = item.DataTypeName;
            Created = item.Created;
            Expires = item.Expires;
            YData = item.YData;
            YSign = item.YSign;
            StoreUSN = item.StoreUSN;
        }
    }

}