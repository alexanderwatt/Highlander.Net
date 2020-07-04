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
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Highlander.Core.Common;
using Highlander.Core.Common.CommsInterfaces;
using Highlander.Grpc.Contracts;
using Highlander.Grpc.Session;
using Highlander.Utilities.Compression;
using Highlander.Utilities.Encryption;
using Highlander.Utilities.Exception;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Serialisation;
using Highlander.Utilities.Threading;
using static System.String;
using static Highlander.Grpc.Contracts.TransferV341;
using ArgumentException = System.ArgumentException;
using InvalidOperationException = System.InvalidOperationException;

#endregion

namespace Highlander.Core.V1
{
    internal class ClientItem : CommonItem, ICoreItem
    {
        private readonly ICoreClient _proxy;
        private bool _useExpiry;
        private TimeSpan _lifetime;
        // mutable until frozen
        private int _ySignedState;
        // data buffers
        private byte[] _xData;
        private byte[] _zData;
        private string _text;
        private object _data;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="itemKind"></param>
        /// <param name="appScope"></param>
        /// <param name="transient"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <param name="serialFormat"></param>
        /// <param name="lifetime"></param>
        public ClientItem(
            ICoreClient proxy,
            ItemKind itemKind,
            string appScope,
            bool transient,
            string name,
            NamedValueSet props,
            object data,
            Type dataType,
            SerialFormat serialFormat,
            TimeSpan lifetime)
            : base(itemKind, transient, name, appScope)
        {
            _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            SysProps.Set(SysPropName.SAlg, (int)serialFormat);
            AppProps.Add(props);
            _data = data;
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            DataTypeName = dataType.FullName;
            _lifetime = lifetime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="itemKind"></param>
        /// <param name="appScope"></param>
        /// <param name="transient"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="serialisedData"></param>
        /// <param name="dataTypeName"></param>
        /// <param name="lifetime"></param>
        public ClientItem(
            ICoreClient proxy,
            ItemKind itemKind,
            string appScope,
            bool transient,
            string name,
            NamedValueSet props,
            string serialisedData,
            string dataTypeName,
            TimeSpan lifetime)
            : base(itemKind, transient, name, appScope)
        {
            _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            AppProps.Add(props);
            _text = serialisedData;
            SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Undefined);
            DataTypeName = dataTypeName;
            _lifetime = lifetime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="item"></param>
        /// <param name="dataType"></param>
        public ClientItem(
            ICoreClient proxy,
            V341TransportItem item,
            Type dataType)
            : base(new Guid(item.ItemId), Converters.ToItemKind((int)item.ItemKind), item.Transient,
                item.ItemName, new NamedValueSet(item.AppProps),
                item.DataType, item.AppScope,
                new NamedValueSet(item.SysProps), item.NetScope,
                item.Created.ToDateTimeOffset(), item.Expires.ToDateTimeOffset(),
                item.YData.ToByteArray(), item.YSign.ToByteArray(),
                item.SourceUSN)
        {
            _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            Frozen = true;
            DataType = dataType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="item"></param>
        public ClientItem(
            ICoreClient proxy,
            RawItem item)
            : base(item.ItemId, (ItemKind)item.ItemKind, item.Transient,
                item.ItemName, new NamedValueSet(item.AppProps),
                item.DataType, item.AppScope,
                new NamedValueSet(item.SysProps), item.NetScope,
                item.Created, item.Expires,
                item.YData, item.YSign,
                item.StoreUSN)
        {
            _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            Frozen = true;
        }

        /// <summary>
        /// </summary>
        public bool Frozen { get; private set; }

        /// <summary>
        /// </summary>
        public new DateTimeOffset Expires
        {
            get
            {
                if (Frozen || _useExpiry)
                    return base.Expires;
                return DateTimeOffset.Now + _lifetime;
            }
            set
            {
                CheckNotFrozen();
                _useExpiry = true;
                base.Expires = value;
            }
        }

        /// <summary>
        /// </summary>
        public TimeSpan Lifetime
        {
            get
            {
                if (Frozen || _useExpiry)
                    return (base.Expires - DateTimeOffset.Now);
                return _lifetime;
            }
            set
            {
                CheckNotFrozen();
                _useExpiry = false;
                _lifetime = value;
            }
        }

        private void CheckNotFrozen()
        {
            if (Frozen)
                throw new ApplicationException("Item already frozen/saved!");
        }

        /// <summary>
        /// </summary>
        public void Freeze()
        {
            if (Frozen)
                return;
            if (Name == null)
                throw new ApplicationException("Item name not set!");
            TimeSpan maxLifetime = DateTimeOffset.MaxValue - DateTimeOffset.Now - TimeSpan.FromDays(1);
            Created = DateTimeOffset.Now;
            if (_useExpiry)
            {
                if (base.Expires < Created)
                    base.Expires = Created;
            }
            else
            {
                if (_lifetime > maxLifetime)
                    _lifetime = maxLifetime;
                if (_lifetime < TimeSpan.Zero)
                    _lifetime = TimeSpan.Zero;
                base.Expires = Created.Add(_lifetime);
            }
            // serialise the data if required
            Serialise();
            DataTypeName ??= "";
            if (_text == null)
            {
                //_Text = "";
                SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Undefined);
            }
            SysProps.Set(SysPropName.TLen, _text?.Length ?? 0);
            // compress the data
            _zData = CompressionHelper.CompressToBuffer(_text);
            SysProps.Set(SysPropName.ZAlg, 1);
            SysProps.Set(SysPropName.ZLen, _zData?.Length ?? 0);
            // do symmetric encryption 1st, if required
            var xtki = SysProps.GetValue<String>(SysPropName.Xtki, null);
            if (xtki != null)
            {
                _xData = _proxy.CryptoManager.EncryptWithTransportKey(xtki, _zData);
                SysProps.Set(SysPropName.XAlg, 1);
            }
            else
                _xData = _zData;
            SysProps.Set(SysPropName.XLen, _xData?.Length ?? 0);
            // do asymmetric encryption 2nd, if required
            var yrki = SysProps.GetValue<String>(SysPropName.Yrki, null);
            if (yrki != null)
            {
                SysProps.Set(SysPropName.YAlg, 1);
                YData = _proxy.CryptoManager.EncryptWithPublicKey(yrki, _xData);
            }
            else
                YData = _xData;
            YDataHash = CalculateBufferHash(YData);
            SysProps.Set(SysPropName.YLen, YData?.Length ?? 0);
            // do public signature 3rd, if required
            var yski = SysProps.GetValue<String>(SysPropName.Yski, null);
            if (yski != null)
            {
                SysProps.Set(SysPropName.YAlg, 1);
                YSign = _proxy.CryptoManager.CreateSignature(yski, YData);
            }
            // add other publisher properties
            SysProps.Set(SysPropName.ApplName, _proxy.ClientInfo.ApplName);
            SysProps.Set(SysPropName.ApplFVer, _proxy.ClientInfo.ApplFVer);
            SysProps.Set(SysPropName.ApplPTok, _proxy.ClientInfo.ApplPTok);
            SysProps.Set(SysPropName.CoreFVer, _proxy.ClientInfo.CoreFVer);
            SysProps.Set(SysPropName.CorePTok, _proxy.ClientInfo.CorePTok);
            SysProps.Set(SysPropName.HostName, _proxy.ClientInfo.HostName);
            SysProps.Set(SysPropName.UserName, _proxy.ClientInfo.UserName);
            SysProps.Set(SysPropName.UserWDom, _proxy.ClientInfo.UserWDom);
            SysProps.Set(SysPropName.UserIdentity, _proxy.ClientInfo.Name);
            SysProps.Set(SysPropName.UserFullName, _proxy.ClientInfo.UserFullName);
            SysProps.Set(SysPropName.OrgEnvId, EnvHelper.EnvName(_proxy.ClientInfo.ConfigEnv));
            SysProps.Set(SysPropName.NodeGuid, _proxy.ClientInfo.NodeGuid);
            // done
            Frozen = true;
        }

        // mutable props
        private void Authenticate()
        {
            if (_ySignedState == 0)
            {
                var yAlg = SysProps.GetValue(SysPropName.YAlg, 0);
                var yski = SysProps.GetValue<string>(SysPropName.Yski, null);
                if (yAlg > 0 && yski != null)
                {
                    if (_proxy.CryptoManager.VerifySignature(yski, YData, YSign))
                        _ySignedState = 1; // success
                }
                else
                {
                    // cannot authenticate
                    _ySignedState = -1;
                }
            }
        }

        /// <summary>
        /// </summary>
        public bool IsSigned
        {
            get
            {
                // authenticate if required
                Authenticate();
                return (_ySignedState > 0);
            }
        }

        /// <summary>
        /// </summary>
        public bool IsSecret
        {
            get
            {
                var yAlg = SysProps.GetValue(SysPropName.YAlg, 0);
                var yrki = SysProps.GetValue<string>(SysPropName.Yrki, null);
                return (yAlg > 0 && yrki != null);
            }
        }

        private void Decrypt()
        {
            if (_zData == null)
            {
                // do asymmetric decryption 1st, if required
                if (_xData == null)
                {
                    var yAlg = SysProps.GetValue(SysPropName.YAlg, 0);
                    var yrki = SysProps.GetValue<string>(SysPropName.Yrki, null);
                    if (yAlg > 0 && yrki != null)
                        _xData = _proxy.CryptoManager.DecryptWithSecretKey(yrki, YData);
                    else
                        _xData = YData;
                    //_YData = null;
                }
                // now do symmetric decryption 2nd, if required
                var xAlg = SysProps.GetValue(SysPropName.XAlg, 0);
                var xtki = SysProps.GetValue<string>(SysPropName.Xtki, null);
                if (xAlg > 0 && xtki != null)
                    _zData = _proxy.CryptoManager.DecryptWithTransportKey(xtki, _xData);
                else
                    _zData = _xData;
                _xData = null;
            }
        }

        private void Decompress()
        {
            if (_text == null)
            {
                // decrypt 1st if required
                Decrypt();
                // now decompress
                _text = CompressionHelper.DecompressToString(_zData);
            }
        }

        private void Serialise()
        {
            if (_text == null && _data != null)
            {
                var serialFormat = (SerialFormat)SysProps.GetValue(SysPropName.SAlg, 0);
                switch (serialFormat)
                {
                    case SerialFormat.Binary:
                        // try Binary serialiser
                        _text = BinarySerializerHelper.SerializeToString(_data);
                        SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Binary);
                        break;
                    //case SerialFormat.Soap:
                    //    // try Soap serialiser
                    //    _text = SoapSerializerHelper.SerializeToString(_data);
                    //    SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Soap);
                    //    break;
                    case SerialFormat.Json:
                        // try Json serialiser
                        _text = JsonSerializerHelper.SerializeToString(_data);
                        SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Json);
                        break;
                    case SerialFormat.Xml:              
                        try
                        {
                            _text = XmlSerializerHelper.SerializeToString(DataType, _data);
                            SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Xml);
                        }
                        catch (Exception excp)
                        {
                            throw new ApplicationException(
                                "The XmlSerializer has thrown an exception: '" + excp.GetType().Name + "'. " +
                                "If your intent was to use the BinaryFormatter or SoapFormatter for serialisation, " +
                                "then you should set the SerialFormat property appropriately.",
                                excp);
                        }
                        break;
                    default:
                        // use default xml serialiser
                        try
                        {
                            _text = XmlSerializerHelper.SerializeToString(DataType, _data);
                            SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Xml);
                        }
                        catch (Exception excp)
                        {
                            throw new ApplicationException(
                                "The XmlSerializer has thrown an exception: '" + excp.GetType().Name + "'. " +
                                "If your intent was to use the BinaryFormatter or SoapFormatter for serialisation, " +
                                "then you should set the SerialFormat property appropriately.",
                                excp);
                        }
                        break;
                }
            }
        }

        private void Deserialise(Type userDataType)
        {
            if (_data == null)
            {
                // decompress 1st if required
                Decompress();
                // now deserialise
                Type dataTypeType = userDataType ?? DataType;
                if (dataTypeType == null && !IsNullOrEmpty(DataTypeName))
                {
                    dataTypeType = Type.GetType(DataTypeName);
                }
                var serialFormat = (SerialFormat)SysProps.GetValue(SysPropName.SAlg, (int)SerialFormat.Undefined);
                if (IsNullOrEmpty(_text))
                {
                    // null data object
                    _data = null;
                }
                else if (serialFormat == SerialFormat.Binary)
                {
                    // use Binary deserializer
                    _data = BinarySerializerHelper.DeserializeFromString(_text);
                }
                //else if (serialFormat == SerialFormat.Soap)
                //{
                //    // use Soap deserializer
                //    _data = SoapSerializerHelper.DeserializeFromString(_text);
                //}
                else if (serialFormat == SerialFormat.Json)
                {
                    // use Json deserializer
                    _data = JsonSerializerHelper.DeserializeFromString(dataTypeType, _text);
                }
                else if (dataTypeType != null)
                {
                    // try default xml serialiser
                    _data = XmlSerializerHelper.DeserializeFromString(dataTypeType, _text);
                }
                else
                {
                    throw new ApplicationException("Cannot deserialise!");
                }
                // clear text
                _text = null;
            }
        }

        /// <summary>
        /// </summary>
        public string Text
        {
            get
            {
                // serialise if required
                Serialise();
                // decompress if required
                if (_text == null)
                {
                    Decompress();
                }
                return _text;
            }
        }

        /// <summary>
        /// </summary>
        public object Data
        {
            get
            {
                // deserialise if required
                Deserialise(DataType);
                return _data;
            }
        }

        /// <summary>
        /// </summary>
        public object GetData(Type dataType, bool binaryClone)
        {
            // deserialise if required
            Deserialise(dataType);
            if (binaryClone)
                return BinarySerializerHelper.Clone(_data);
            return _data;
        }

        /// <summary>
        /// </summary>
        public T GetData<T>(bool binaryClone) where T : class
        {
            return (T)GetData(typeof(T), binaryClone);
        }

        /// <summary>
        /// </summary>
        public Type DataType { get; private set; }

        /// <summary>
        /// </summary>
        public void SetData(Type dataType, object data)
        {
            CheckNotFrozen();
            _data = data;
            _text = null;
            DataType = dataType;
            DataTypeName = dataType.FullName;
        }

        /// <summary>
        /// </summary>
        public void SetData<T>(T data) where T : class
        {
            SetData(typeof(T), data);
        }

        /// <summary>
        /// </summary>
        public void SetData(object data)
        {
            SetData(data.GetType(), data);
        }

        /// <summary>
        /// </summary>
        public void SetText(string text, string dataTypeName)
        {
            CheckNotFrozen();
            _text = text;
            SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Undefined);
            _data = null;
            DataType = null;
            DataTypeName = dataTypeName;
        }

        /// <summary>
        /// </summary>
        public void SetText(string text, Type dataType)
        {
            CheckNotFrozen();
            _text = text;
            SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Undefined);
            _data = null;
            DataType = dataType;
            DataTypeName = dataType.FullName;
        }

        /// <summary>
        /// </summary>
        public string TranspKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.Xtki, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.Xtki, value); }
        }

        /// <summary>
        /// </summary>
        public string SenderKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.Yski, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.Yski, value); }
        }

        /// <summary>
        /// </summary>
        public string RecverKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.Yrki, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.Yrki, value); }
        }

        /// <summary>
        /// </summary>
        public SerialFormat SerialFormat
        {
            get => (SerialFormat)SysProps.GetValue(SysPropName.SAlg, 0);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.SAlg, (int)value); }
        }
    }

    internal class RequestBase
    {
        public readonly Guid RequestId;
        public readonly bool DebugRequest;
        public readonly AsyncResult<GuardedList<ClientItem>> AsyncResult;
        public readonly Type DataTypeType;
        public readonly GuardedList<ClientItem> PartialResults;
        public readonly DateTimeOffset SubmitTime;
        public readonly DateTimeOffset ExpiryTime;
        public Exception Fault;
        public bool Completed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <param name="dataType"></param>
        /// <param name="expiryTimeout"></param>
        /// <param name="debugRequest"></param>
        public RequestBase(AsyncResult<GuardedList<ClientItem>> asyncResult, Type dataType, TimeSpan expiryTimeout, bool debugRequest)
        {
            RequestId = Guid.NewGuid();
            DataTypeType = dataType;
            AsyncResult = asyncResult;
            SubmitTime = DateTimeOffset.Now;
            ExpiryTime = SubmitTime.Add(expiryTimeout);
            PartialResults = new GuardedList<ClientItem>();
            DebugRequest = debugRequest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="clientBase"></param>
        /// <param name="header"></param>
        public void Transmit(ILogger logger, TransferV341Client clientBase, V131SessionHeader header)
        {
            OnTransmit(logger, clientBase, header);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="clientBase"></param>
        /// <param name="header"></param>
        protected virtual void OnTransmit(ILogger logger, TransferV341Client clientBase, V131SessionHeader header)
        {
            throw new NotImplementedException();
        }
    }

    internal class RequestSelectMultipleItems : RequestBase
    {
        public readonly V341SelectMultipleItems Body;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <param name="dataType"></param>
        /// <param name="expiryTimeout"></param>
        /// <param name="debugRequest"></param>
        /// <param name="body"></param>
        public RequestSelectMultipleItems(
            AsyncResult<GuardedList<ClientItem>> asyncResult, Type dataType, TimeSpan expiryTimeout, bool debugRequest,
            V341SelectMultipleItems body)
            : base(asyncResult, dataType, expiryTimeout, debugRequest)
        {
            Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="clientBase"></param>
        /// <param name="header"></param>
        protected override void OnTransmit(ILogger logger, TransferV341Client clientBase, V131SessionHeader header)
        {
            if (DebugRequest)
            {
                logger.LogDebug("Request '{0}' sending SelectMultipleItems", RequestId);
                //logger.LogDebug("  AppScopes: {0}", (Body.AppScopes == null) ? "*" : String.Join(",", Body.AppScopes));
                if (Body.QueryDef.ItemKind != V341ItemKind.Object)
                    logger.LogDebug("  ItemKind : {0}", Body.QueryDef.ItemKind == V341ItemKind.Undefined ? "(any)" : Body.QueryDef.ItemKind.ToString());
                if (Body.ItemIds != null)
                {
                    if (Body.ItemIds.Count == 1)
                        logger.LogDebug("  ItemId   : {0}", Body.ItemIds[0]);
                    else
                        logger.LogDebug("  ItemIds  : Multiple({0})", Body.ItemIds.Count);
                }
                if (Body.QueryDef.ItemNames != null)
                {
                    if (Body.QueryDef.ItemNames.Count == 1)
                        logger.LogDebug("  ItemName : '{0}'", Body.QueryDef.ItemNames[0]);
                    else
                        logger.LogDebug("  ItemNames: Multiple({0})", Body.QueryDef.ItemNames.Count);
                }
                logger.LogDebug("  DataType : {0}", Body.QueryDef.DataType ?? "(any)");
                if (Body.QueryDef.MinimumUSN > 0)
                    logger.LogDebug("  USN >    : {0}", Body.QueryDef.MinimumUSN);
                if (Body.QueryDef.QueryExpr != null)
                    logger.LogDebug("  Query    : {0}", Expr.Create(Body.QueryDef.QueryExpr).DisplayString());
                if (Body.OrderExpr != null)
                {
                    logger.LogDebug("  OrderBy  : {0}", Expr.Create(Body.OrderExpr).DisplayString());
                }
                if (Body.RowCount > 0)
                {
                    logger.LogDebug("  StartRow : {0}", Body.StartRow);
                    logger.LogDebug("  RowCount : {0}", Body.RowCount);
                }
                logger.LogDebug("  Excl.Old?: {0}", Body.QueryDef.ExcludeDeleted);
                logger.LogDebug("  Excl.Data: {0}", Body.QueryDef.ExcludeDataBody);
            }
            clientBase.TransferV341SelectMultipleItems(new TransferV341SelectMultipleItemsRequest { Header = header, Body = Body });
        }
    }

    internal class RequestCreateSubscription : RequestBase
    {
        /// <summary>
        /// </summary>
        public readonly V341CreateSubscription Body;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <param name="dataType"></param>
        /// <param name="expiryTimeout"></param>
        /// <param name="debugRequest"></param>
        /// <param name="body"></param>
        public RequestCreateSubscription(
            AsyncResult<GuardedList<ClientItem>> asyncResult, Type dataType, TimeSpan expiryTimeout, bool debugRequest,
            V341CreateSubscription body)
            : base(asyncResult, dataType, expiryTimeout, debugRequest)
        {
            Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="clientBase"></param>
        /// <param name="header"></param>
        protected override void OnTransmit(ILogger logger, TransferV341Client clientBase, V131SessionHeader header)
        {
            clientBase.TransferV341CreateSubscription(new TransferV341CreateSubscriptionRequest { Header = header, Body = Body });
        }
    }

    internal class RequestExtendSubscription : RequestBase
    {
        /// <summary>
        /// </summary>
        public readonly V341ExtendSubscription Body;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <param name="dataType"></param>
        /// <param name="expiryTimeout"></param>
        /// <param name="debugRequest"></param>
        /// <param name="body"></param>
        public RequestExtendSubscription(
            AsyncResult<GuardedList<ClientItem>> asyncResult, Type dataType, TimeSpan expiryTimeout, bool debugRequest,
            V341ExtendSubscription body)
            : base(asyncResult, dataType, expiryTimeout, debugRequest)
        {
            Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="clientBase"></param>
        /// <param name="header"></param>
        protected override void OnTransmit(ILogger logger, TransferV341Client clientBase, V131SessionHeader header)
        {
            clientBase.TransferV341ExtendSubscription(new TransferV341ExtendSubscriptionRequest { Header = header, Body = Body });
        }
    }

    internal class RequestCancelSubscription : RequestBase
    {
        /// <summary>
        /// </summary>
        public readonly V341CancelSubscription Body;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <param name="dataType"></param>
        /// <param name="expiryTimeout"></param>
        /// <param name="debugRequest"></param>
        /// <param name="body"></param>
        public RequestCancelSubscription(
            AsyncResult<GuardedList<ClientItem>> asyncResult, Type dataType, TimeSpan expiryTimeout, bool debugRequest,
            V341CancelSubscription body)
            : base(asyncResult, dataType, expiryTimeout, debugRequest)
        {
            Body = body;
        }

        /// <summary>
        /// </summary>
        protected override void OnTransmit(ILogger logger, TransferV341Client clientBase, V131SessionHeader header)
        {
            clientBase.TransferV341CancelSubscription(new TransferV341CancelSubscriptionRequest { Header = header, Body = Body });
        }
    }

    internal class RequestNotifyMultipleItems : RequestBase
    {
        /// <summary>
        /// </summary>
        public readonly V341NotifyMultipleItems Body;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <param name="dataType"></param>
        /// <param name="expiryTimeout"></param>
        /// <param name="debugRequest"></param>
        /// <param name="body"></param>
        public RequestNotifyMultipleItems(
            AsyncResult<GuardedList<ClientItem>> asyncResult, Type dataType, TimeSpan expiryTimeout, bool debugRequest,
            V341NotifyMultipleItems body)
            : base(asyncResult, dataType, expiryTimeout, debugRequest)
        {
            Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="clientBase"></param>
        /// <param name="lastHeader"></param>
        protected override void OnTransmit(ILogger logger, TransferV341Client clientBase, V131SessionHeader lastHeader)
        {
            var page = new List<V341TransportItem>();
            foreach (V341TransportItem item in Body.Items)
            {
                page.Add(item);
                if (page.Count >= 50) // todo const
                {
                    // page limit - create intermediate page header
                    var pageHeader = new V131SessionHeader(
                        lastHeader.SessionIdGuid, lastHeader.RequestIdGuid, true, false,
                        lastHeader.ReplyAddress, lastHeader.ReplyContract, lastHeader.DebugRequest);
                    clientBase.TransferV341NotifyMultipleItems(new TransferV341NotifyMultipleItemsRequest{Header = pageHeader, Body = new V341NotifyMultipleItems(Guid.Empty, page.ToArray())});
                    page.Clear();
                }
            }
            clientBase.TransferV341NotifyMultipleItems(new TransferV341NotifyMultipleItemsRequest { Header = lastHeader, Body = new V341NotifyMultipleItems(Guid.Empty, page.ToArray()) });
        }
    }

    internal class IncompleteRequests
    {
        /// <summary>
        /// </summary>
        public readonly Queue<Guid> OutgoingRequestQueue = new Queue<Guid>();

        /// <summary>
        /// </summary>
        public readonly Dictionary<Guid, RequestBase> RequestCache = new Dictionary<Guid, RequestBase>();
    }

    internal class CoreClient : ICoreClient, ITransferV341
    {
        private readonly TimeSpan _cDebugMsgLifetime = TimeSpan.FromDays(1);

        // readonly state
        private readonly AsyncThreadQueue _mainThreadQueue;
        private readonly AsyncThreadQueue _userThreadQueue;

        public ILogger Logger { get; }

        //private readonly AddressBinding _sessCtrlAddressBinding;
        //private readonly AddressBinding _transferAddressBinding;
        private readonly V131ClientInfo _clientInfoV131;
        private readonly string _instanceName;

        public ServiceAddress ServerAddress { get; }

        public ICoreClient Proxy => this;

        public void Clear() { throw new NotSupportedException(); }

        public List<ISubscription> Subscriptions => throw new NotSupportedException();

        public List<ICoreItem> Items => throw new NotSupportedException();

        public int ItemCount => throw new NotSupportedException();

        public int CreateCount => throw new NotSupportedException();

        public int UpdateCount => throw new NotSupportedException();

        public int DeleteCount => throw new NotSupportedException();

        // heartbeat state
        private Timer _heartbeatTimer;
        private int _heartbeatCalls;
        // managed state
        public IModuleInfo ClientInfo { get; }

        //private CustomServiceHost<ITransferV341, TransferRecverV341> _replyServiceHost;
        private string _replyAddress;
        private readonly string _replyContract = typeof(ITransferV341).FullName;
        private Guid _sessionId = Guid.Empty;
        private TransferV341Client _clientBase;
        private Timer _housekeepingTimer;
        //private Timer _KeepAliveTimer;
        //private ISubscription _DefaultSubscription;
        private readonly Guarded<IncompleteRequests> _incompleteRequests = new Guarded<IncompleteRequests>(new IncompleteRequests());
        private long _asyncCallProcessOutgoingRequestsCount;

        // default serialisation format

        /// <summary>
        /// </summary>
        public SerialFormat DefaultSerialFormat { get; set; } = SerialFormat.Undefined;

        // default object lifetime
        private TimeSpan _defaultLifetime = TimeSpan.MaxValue;

        /// <summary>
        /// </summary>
        public TimeSpan DefaultLifetime
        {
            get => _defaultLifetime;
            set
            {
                if (value < TimeSpan.Zero || (value > TimeSpan.MaxValue))
                    throw new ArgumentOutOfRangeException($"DefaultLifetime",
                        $"Must be between {TimeSpan.Zero} and {TimeSpan.MaxValue}");
                _defaultLifetime = value;
            }
        }

        // default app scopes
        private string[] _defaultAppScopes = { AppScopeNames.Legacy };

        /// <summary>
        /// </summary>
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

        // client core mode

        /// <summary>
        /// </summary>
        public CoreModeEnum CoreMode { get; }

        /// <summary>
        /// </summary>
        private CoreStateEnum _coreState = CoreStateEnum.Initial;

        /// <summary>
        /// </summary>
        public CoreStateEnum CoreState
        {
            get => _coreState;
            private set
            {
                CoreStateEnum oldState = _coreState;
                _coreState = value;
                if (value != oldState)
                {
                    NotifyUserStateChange(new CoreStateChange(oldState, value));
                }
            }
        }

        private void NotifyUserStateChange(CoreStateChange update)
        {
            _userThreadQueue.Dispatch(update, UserCallbackStateChange);
        }

        // range helper
        private T CheckRange<T>(T value, T minValue, T maxValue, string paramName) where T : IComparable
        {
            if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
                throw new ArgumentOutOfRangeException(paramName,
                    $"Must be between {minValue} and {maxValue}");
            return value;
        }

        // request timeout
        private readonly TimeSpan _minRequestTimeout = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _maxRequestTimeout = TimeSpan.FromSeconds(3600);
        private TimeSpan _requestTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan RequestTimeout
        {
            get => _requestTimeout;
            set
            {
                _requestTimeout = CheckRange(value, _minRequestTimeout, _maxRequestTimeout, "RequestTimeout");
                Logger.LogDebug("New request timeout is: {0}", value);
            }
        }

        // offline timeout
        private readonly TimeSpan _minOfflineTimeout = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _maxOfflineTimeout = TimeSpan.FromMinutes(60);
        private TimeSpan _offlineTimeout = TimeSpan.FromMinutes(15);
        //private bool _OutageInProgress = true;
        private DateTimeOffset _offlineCommenced = DateTimeOffset.Now;

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan OfflineTimeout
        {
            get => _offlineTimeout;
            set
            {
                _offlineTimeout = CheckRange(value, _minOfflineTimeout, _maxOfflineTimeout, "OfflineTimeout");
                Logger.LogDebug("New offline timeout is: {0}", value);
            }
        }

        // request count
        private const int MinMaxRequestCount = 1;

        private const int MaxMaxRequestCount = 100;

        private int _maxRequestCount = 20;

        /// <summary>
        /// 
        /// </summary>
        public int MaxRequestCount
        {
            get => _maxRequestCount;
            set
            {
                if (value < MinMaxRequestCount || value > MaxMaxRequestCount)
                    throw new ArgumentOutOfRangeException($"MaxRequestCount",
                        $"Must be between {MinMaxRequestCount} and {MaxMaxRequestCount}");
                _maxRequestCount = value;
                Logger.LogDebug("New max request count is: {0}", value);
            }
        }

        private void EnqueueOutgoingRequest(RequestBase request)
        {
            // fail if client has faulted
            if (_coreState == CoreStateEnum.Faulted)
                throw new InvalidOperationException();
            // block until request count falls below maximum
            DateTime lastLogged = DateTime.Now;
            bool logged = false;
            int requestCount = 0;
            _incompleteRequests.Locked(requests => requestCount = requests.OutgoingRequestQueue.Count);
            while (requestCount > _maxRequestCount)
            {
                if (DateTime.Now - lastLogged > TimeSpan.FromSeconds(5))
                {
                    Logger.LogDebug("Request '{0}' blocked until request count <= {1}", request.RequestId, _maxRequestCount);
                    lastLogged = DateTime.Now;
                    logged = true;
                }
                Thread.Sleep(10); // about 1 time slice
                _incompleteRequests.Locked(requests => requestCount = requests.OutgoingRequestQueue.Count);
            }
            if (logged)
                Logger.LogDebug("Request '{0}' unblocked.", request.RequestId);
            _incompleteRequests.Locked(requests =>
            {
                requests.OutgoingRequestQueue.Enqueue(request.RequestId);
                requests.RequestCache.Add(request.RequestId, request);
            });
            // dispatch call the process requests
            AsyncCallProcessOutgoingRequests();
        }

        // connection core mode notifications
        /// <summary>
        /// </summary>
        public event CoreStateHandler OnStateChange;

        private void UserCallbackStateChange(CoreStateChange update)
        {
            try
            {
                string message = $"Core state change: {update.OldState} -> {update.NewState}";
                int severity;
                switch (update.NewState)
                {
                    case CoreStateEnum.Faulted:
                        severity = LogSeverity.Error;
                        break;
                    case CoreStateEnum.Offline:
                        severity = LogSeverity.Warning;
                        break;
                    case CoreStateEnum.Disposed:
                    case CoreStateEnum.Connected:
                        severity = LogSeverity.Info;
                        break;
                    default:
                        severity = LogSeverity.Debug;
                        break;
                }
                Logger.Log(severity, "", message);
                OnStateChange?.Invoke(update);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        // debug requests flag

        /// <summary>
        /// </summary>
        public bool DebugRequests { get; set; }

        // security
        /// <summary>
        /// </summary>
        public ICryptoManager CryptoManager { get; } = new DefaultCryptoManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerRef"></param>
        /// <param name="instanceName"></param>
        /// <param name="env"></param>
        /// <param name="hostList"></param>
        /// <param name="protocols"></param>
        /// <param name="useFallbackServers"></param>
        /// <param name="debugRequests"></param>
        /// <param name="requestTimeout"></param>
        /// <param name="offlineTimeout"></param>
        /// <param name="coreMode"></param>
        /// <param name="applAssembly"></param>
        public CoreClient(Reference<ILogger> loggerRef, string instanceName, string env, string hostList, string protocols,
            bool useFallbackServers, bool debugRequests,
            TimeSpan requestTimeout,
            TimeSpan offlineTimeout,
            CoreModeEnum coreMode,
            Assembly applAssembly)
        {
            if (env == null)
                throw new ArgumentNullException(nameof(env));
            EnvId envId = EnvHelper.CheckEnv(EnvHelper.ParseEnvName(env));
            if (loggerRef == null)
                throw new ArgumentNullException(nameof(loggerRef));
            var loggerRef1 = Reference<ILogger>.Create(new FilterLogger(loggerRef, instanceName != null ? instanceName + ": " : "Proxy: "));
            Logger = loggerRef1.Target;
            _mainThreadQueue = new AsyncThreadQueue(Logger);
            _userThreadQueue = new AsyncThreadQueue(Logger);
            Guid clientId = Guid.NewGuid();
            // get user identity and full name
            string userLoginName;
            string userFullName = null;
            using (WindowsIdentity winIdent = WindowsIdentity.GetCurrent())
            {
                userLoginName = winIdent.Name;
            }
            try
            {
                using (var principalContext = new PrincipalContext(ContextType.Domain))
                {
                    using (
                        UserPrincipal principal = UserPrincipal.FindByIdentity(principalContext,
                            IdentityType.SamAccountName,
                            userLoginName))
                    {
                        if (principal != null)
                            userFullName = principal.GivenName + " " + principal.Surname;
                    }
                }
            }
            catch (PrincipalException principalException)
            {
                // swallow - can occur on machines not connected to domain controller
                Logger.LogWarning("UserPrincipal.FindByIdentity failed. User name: {0}. {1}: {2}", userLoginName, principalException.GetType().Name, principalException.Message);
            }
            catch (COMException e)
            {
                // Can occur when trying to run as a scheduled task
                Logger.LogWarning("UserPrincipal.FindByIdentity COM failure: User name: {0}. {1}: {2}", userLoginName, e.GetType().Name, e.Message);
            }
            Assembly coreAssembly = Assembly.GetExecutingAssembly();
            ClientInfo = new ModuleInfo(env, clientId, userLoginName, userFullName, applAssembly, coreAssembly);
            _clientInfoV131 = new V131ClientInfo(
                clientId,
                CoreHelper.ToV131EnvId(envId),
                coreAssembly,
                applAssembly,
                ClientInfo.Name,
                ClientInfo.UserFullName);
            _instanceName = instanceName;
            var protocols1 = (protocols ?? WcfConst.NetTcp).Split(';');
            DebugRequests = debugRequests;
            if (requestTimeout != TimeSpan.Zero)
                _requestTimeout = CheckRange(requestTimeout, _minRequestTimeout, _maxRequestTimeout, "RequestTimeout");
            if (offlineTimeout != TimeSpan.Zero)
                _offlineTimeout = CheckRange(offlineTimeout, _minOfflineTimeout, _maxOfflineTimeout, "OfflineTimeout");
            // set behaviour based on core mode
            CoreMode = coreMode;
            switch (CoreMode)
            {
                case CoreModeEnum.Standard:
                    break;
                //case CoreModeEnum.Reliable:
                //    break;
                default:
                    throw new NotSupportedException("CoreMode: " + CoreMode);
            }
            // create server connection
            const SvcId svc = SvcId.CoreServer;
            string svcName = EnvHelper.SvcPrefix(svc);
            string[] serviceAddress = EnvHelper.GetServiceAddrs(envId, svc, useFallbackServers);
            if (hostList != null)
                serviceAddress = hostList.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int defaultPort = EnvHelper.SvcPort(envId, svc);
            //ServiceAddress resolvedServer = V111Helpers.ResolveServer(Logger, svcName, new ServiceAddresses(protocols1, serviceAddress, defaultPort),
            //    new[] { typeof(ISessCtrlV131).FullName, typeof(ITransferV341).FullName });
            //_sessCtrlAddressBinding = WcfHelper.CreateAddressBinding(
            //    WcfConst.NetTcp, resolvedServer.Host, resolvedServer.Port, svcName, typeof(ISessCtrlV131).Name);
            //_transferAddressBinding = WcfHelper.CreateAddressBinding(
            //    resolvedServer.Protocol, resolvedServer.Host, resolvedServer.Port, svcName, typeof(ITransferV341).Name);
            //ServerAddress = resolvedServer;
            // initialise session
            OpenClientBaseAndSendRequest(false, null);
            // start housekeeping timer
            _housekeepingTimer = new Timer(
                HousekeepingRequestsTimeout, null,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1));
            // start heartbeat timer
            _heartbeatTimer = new Timer(
                HeartbeatTimeout, null,
                TimeSpan.FromSeconds(60),
                TimeSpan.FromSeconds(60));
            // successfully started
            //Logger.LogInfo("Client connected to: {0}", _transferAddressBinding.Address.Uri.AbsoluteUri);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SafeDisconnect()
        {
            // close connection
            try
            {
                //using (var session = new SessCtrlSenderV131(_sessCtrlAddressBinding))
                //{
                //    var header = new V131SessionHeader(_sessionId, Guid.NewGuid(), false, false, _replyAddress, _replyContract, DebugRequests);
                //    session.CloseSessionV131(header);
                //}
            }
            catch (CommunicationException wcfExcp)
            {
                Logger.LogWarning("Close session attempt failed: {0}: {1}", wcfExcp.GetType().Name, wcfExcp.Message);
            }
            _sessionId = Guid.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriptionId"></param>
        public void Unsubscribe(Guid subscriptionId)
        {
            // cancel one subscription
            IAsyncResult ar = null;
            _subscriptions.Locked(subscriptions =>
            {
                if (subscriptions.TryGetValue(subscriptionId, out var subscription))
                {
                    // send cancel request to server
                    try
                    {
                        ar = SendCancelSubsBegin(null, subscription.DataType,
                            new V341CancelSubscription(subscription.Id));
                        Logger.LogDebug("Cancelling subscription: <{0}> {1} ({2})", subscription.DataTypeName, subscription.WhereExpr.DisplayString(), subscription.Id);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Failed to cancel subscription: {0}: {1}", e.GetType().Name, e.Message);
                    }
                }
            });
            // wait for requests to complete
            if (ar != null)
            {
                try
                {
                    SendCancelSubsEnd(ar);
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to cancel subscription: {0}: {1}", e.GetType().Name, e.Message);
                }
            }
        }

        /// <summary>
        /// </summary>
        public void UnsubscribeAll()
        {
            // cancel all subscriptions
            var arList = new List<IAsyncResult>();
            _subscriptions.Locked(subscriptions =>
            {
                foreach (Subscription subscription in subscriptions.Values)
                {
                    // send cancel request to server
                    try
                    {
                        arList.Add(SendCancelSubsBegin(null, subscription.DataType,
                            new V341CancelSubscription(subscription.Id)));
                        Logger.LogDebug("Cancelling subscription: <{0}> {1} ({2})", subscription.DataTypeName, subscription.WhereExpr.DisplayString(), subscription.Id);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Failed to cancel subscription: {0}: {1}", e.GetType().Name, e.Message);
                    }
                }
            });
            // wait for requests to complete
            if (arList.Count > 0)
            {
                foreach (IAsyncResult ar in arList)
                {
                    try
                    {
                        SendCancelSubsEnd(ar);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Failed to cancel subscription: {0}: {1}", e.GetType().Name, e.Message);
                    }
                }
                Logger.LogDebug("Cancelled {0} subscriptions.", arList.Count);
            }
        }

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            _requestTimeout = TimeSpan.FromSeconds(15);
            // stop all timers
            DisposeHelper.SafeDispose(ref _housekeepingTimer);
            DisposeHelper.SafeDispose(ref _heartbeatTimer);
            // cancel all subscriptions
            UnsubscribeAll();
            DisposeHelper.SafeDispose(ref _subscriptions);
            // cancel all requests
            _mainThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(30));
            _mainThreadQueue.Dispose();
            CoreState = CoreStateEnum.Disposed;
            _userThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(30));
            _userThreadQueue.Dispose();
            SafeDisconnect();
            _incompleteRequests.Dispose();
            //DisposeHelper.SafeDispose(ref _IncompleteRequests);
            // close connections
            DisposeHelper.SafeDispose(ref _clientBase);
            //DisposeHelper.SafeDispose(ref _replyServiceHost);
            Logger.LogDebug("Closed");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        public void SaveDebug<T>(T data, string name, NamedValueSet props) where T : class
        {
            var item = new ClientItem(this, ItemKind.Debug, _defaultAppScopes[0], true, name, props, data, typeof(T), DefaultSerialFormat,
                (data != null) ? _cDebugMsgLifetime : TimeSpan.Zero);
            IAsyncResult ar = SaveItemsBegin(null, item);
            // non-blocking version - do async completion
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    SaveEnd((IAsyncResult)state);
                }
                catch (Exception e)
                {
                    Logger.LogError("SaveDebug completion failed: {0}", e.GetType().Name);
                }
            }, ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props)
        {
            ICoreItem item = MakeTypedItem(dataType, data, name, props, false);
            IAsyncResult ar = SaveItemBegin(item);
            SaveEnd(ar);
            return item.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            ICoreItem item = MakeTypedItem(dataType, data, name, props, transient);
            item.Expires = expires;
            IAsyncResult ar = SaveItemBegin(item);
            SaveEnd(ar);
            return item.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            ICoreItem item = MakeTypedItem(dataType, data, name, props, transient);
            item.Lifetime = lifetime;
            IAsyncResult ar = SaveItemBegin(item);
            SaveEnd(ar);
            return item.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public Guid SaveUntypedObject(object data, string name, NamedValueSet props)
        {
            ICoreItem item = MakeTypedItem(data.GetType(), data, name, props, false);
            IAsyncResult ar = SaveItemBegin(item);
            SaveEnd(ar);
            return item.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data, string name, NamedValueSet props)
        {
            return SaveTypedObject(typeof(T), data, name, props, false, (data == null) ? TimeSpan.Zero : TimeSpan.MaxValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data, string name, NamedValueSet props, DateTimeOffset expires)
        {
            return SaveTypedObject(typeof(T), data, name, props, false, expires);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data, string name, NamedValueSet props, TimeSpan lifetime)
        {
            return SaveTypedObject(typeof(T), data, name, props, false, lifetime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            return SaveTypedObject(typeof(T), data, name, props, transient, expires);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            ICoreItem item = MakeItem(data, name, props, transient, lifetime);
            return SaveItem(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, data.IsTransient);
            item.Lifetime = data.Lifetime;
            return SaveItem(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data, TimeSpan lifetime) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, data.IsTransient);
            item.Lifetime = lifetime;
            return SaveItem(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="transient"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data, bool transient, DateTimeOffset expires) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, transient);
            item.Expires = expires;
            return SaveItem(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="transient"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public Guid SaveObject<T>(T data, bool transient, TimeSpan lifetime) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, transient);
            item.Lifetime = lifetime;
            return SaveItem(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Guid SaveItem(ICoreItem item)
        {
            IAsyncResult ar = SaveItemBegin(item);
            SaveEnd(ar);
            return item.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private ClientItem ConvertToClientItem(ICoreItem item)
        {
            var result = item as ClientItem ??
                         new ClientItem(this, item.ItemKind, item.AppScope, item.Transient, item.Name, item.AppProps, item.Data, item.DataType, DefaultSerialFormat, item.Lifetime);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public void SaveItems(IEnumerable<ICoreItem> items)
        {
            IAsyncResult ar = SaveObjectsBegin(items);
            SaveObjectsEnd(ar);
        }

        private IAsyncResult SaveObjectsBegin(IEnumerable<ICoreItem> items)
        {
            var internalItems = items.Select(ConvertToClientItem).ToList(); // ClientItem[items.Length];
            return SaveItemsBegin(null, internalItems);
        }

        private void SaveObjectsEnd(IAsyncResult ar)
        {
            SaveItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IAsyncResult SaveItemBegin(ICoreItem item)
        {
            return SaveItemsBegin(null, ConvertToClientItem(item));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public IAsyncResult SaveItemsBegin(IEnumerable<ICoreItem> items)
        {
            var internalItems = items.Select(ConvertToClientItem).ToList();
            return SaveItemsBegin(null, internalItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public IAsyncResult SaveTypedObjectBegin(Type dataType, object data, string name, NamedValueSet props)
        {
            ICoreItem item = MakeTypedItem(dataType, data, name, props, false);
            return SaveItemBegin(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public IAsyncResult SaveObjectBegin<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime) where T : class
        {
            ICoreItem item = MakeItem(data, name, props, transient, lifetime);
            return SaveItemBegin(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public IAsyncResult SaveObjectBegin<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires) where T : class
        {
            ICoreItem item = MakeItem(data, name, props, transient, expires);
            return SaveItemBegin(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="transient"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public IAsyncResult SaveObjectBegin<T>(T data, bool transient, TimeSpan lifetime) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, transient);
            item.Lifetime = lifetime;
            return SaveItemBegin(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="transient"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public IAsyncResult SaveObjectBegin<T>(T data, bool transient, DateTimeOffset expires) where T : ICoreObject
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, data.NetworkKey, data.AppProperties, transient);
            item.Expires = expires;
            return SaveItemBegin(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public void SaveEnd(IAsyncResult ar)
        {
            SaveItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeItem"></param>
        /// <returns></returns>
        public IAsyncResult SaveRawItemBegin(RawItem storeItem)
        {
            return SaveItemsBegin(null, new ClientItem(this, storeItem));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public void SaveRawItemEnd(IAsyncResult ar)
        {
            SaveItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeItem"></param>
        public void SaveRawItem(RawItem storeItem)
        {
            IAsyncResult ar = SaveRawItemBegin(storeItem);
            SaveItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeItems"></param>
        public void SaveRawItems(IEnumerable<RawItem> storeItems)
        {
            var items = storeItems.Select(storeItem => new ClientItem(this, storeItem)).ToList(); // = new ClientItem[storeItems.Count<V31StoreItem>()];
            IAsyncResult ar = SaveItemsBegin(null, items);
            SaveItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTypeName"></param>
        /// <param name="text"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public ICoreItem MakeItemFromText(string dataTypeName, string text, string name, NamedValueSet props)
        {
            // note: (text == null) implies a deleted object
            if (dataTypeName == null)
                throw new ArgumentNullException(nameof(dataTypeName));
            return new ClientItem(this, ItemKind.Object, _defaultAppScopes[0], false, name, props, text, dataTypeName,
                (text != null) ? _defaultLifetime : TimeSpan.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <returns></returns>
        public ICoreItem MakeTypedItem(Type dataType, object data, string name, NamedValueSet props, bool transient)
        {
            // note: (data == null) implies a deleted object
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));
            if (dataType.IsInterface)
                throw new ArgumentException("Cannot be an interface type!", nameof(dataType));
            return new ClientItem(this, ItemKind.Object, _defaultAppScopes[0], transient, name, props, data, dataType, DefaultSerialFormat,
                (data != null) ? _defaultLifetime : TimeSpan.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires)
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, name, props, transient);
            item.Expires = expires;
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="transient"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime)
        {
            ICoreItem item = MakeTypedItem(typeof(T), data, name, props, transient);
            item.Lifetime = lifetime;
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public ICoreItem MakeObject<T>(T data, string name, NamedValueSet props)
        {
            return MakeItem(data, name, props, false, TimeSpan.MaxValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Guid DeleteItem(ICoreItem item)
        {
            ICoreItem newItem = MakeItemFromText(item.DataTypeName, null, item.Name, item.AppProps);
            IAsyncResult ar = SaveItemsBegin(null, ConvertToClientItem(newItem));
            SaveItemsEnd(ar);
            return newItem.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="name"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public ICoreItem LoadItem(Type dataType, string name, bool includeDeleted)
        {
            IAsyncResult ar = LoadItemsBegin(null, dataType,
                new V341SelectMultipleItems((dataType != null) ? dataType.FullName : null, ItemKind.Object, name, _defaultAppScopes, 0, includeDeleted, DateTimeOffset.Now, false));
            return LoadItemEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ICoreItem LoadItem(Type dataType, string name)
        {
            IAsyncResult ar = LoadItemsBegin(null, dataType,
                new V341SelectMultipleItems((dataType != null) ? dataType.FullName : null, ItemKind.Object, name, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
            return LoadItemEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public ICoreItem LoadItem<T>(string name)
        {
            return LoadItem<T>(name, ItemKind.Object);
        }

        private ICoreItem LoadItem<T>(string name, ItemKind itemKind)
        {
            IAsyncResult ar = LoadItemsBegin(null, typeof(T),
                new V341SelectMultipleItems(typeof(T).FullName, itemKind, name, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
            return LoadItemEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ICoreItem LoadItem(string name)
        {
            return LoadItem(name, ItemKind.Object);
        }

        private ICoreItem LoadItem(string name, ItemKind itemKind)
        {
            IAsyncResult ar = LoadItemsBegin(null, null,
                new V341SelectMultipleItems(null, itemKind, name, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
            return LoadItemEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ICoreItem LoadItem(Type dataType, Guid id)
        {
            IAsyncResult ar = LoadItemsBegin(null, dataType, new V341SelectMultipleItems(id, false));
            return LoadItemEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public ICoreItem LoadItem<T>(Guid id)
        {
            IAsyncResult ar = LoadItemsBegin(null, typeof(T), new V341SelectMultipleItems(id, false));
            return LoadItemEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ICoreItem LoadItem(Guid id)
        {
            IAsyncResult ar = LoadItemsBegin(null, null, new V341SelectMultipleItems(id, false));
            return LoadItemEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T LoadObject<T>(string name)
        {
            IAsyncResult ar = LoadItemsBegin(null, typeof(T),
                new V341SelectMultipleItems(typeof(T).FullName, ItemKind.Object, name, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
            ICoreItem item = LoadItemEnd(ar);
            return (T) item?.Data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T LoadObject<T>(Guid id)
        {
            IAsyncResult ar = LoadItemsBegin(null, typeof(T),
                new V341SelectMultipleItems(id, false));
            ICoreItem item = LoadItemEnd(ar);
            if (item.ItemKind != ItemKind.Object)
                throw new ApplicationException($"Item id ({id}) does not refer to an object!");
            return (T)item.Data;
        }

        // get multiple items
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public List<T> LoadObjects<T>(IExpression whereExpr)
        {
            List<ICoreItem> items = LoadItems(typeof(T), whereExpr);
            return items.Select(item => (T)item.Data).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTypeName"></param>
        /// <param name="itemKind"></param>
        /// <param name="whereExpr"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadUntypedItems(string dataTypeName, ItemKind itemKind, IExpression whereExpr, bool includeDeleted)
        {
            if (whereExpr == null)
                throw new ArgumentNullException(nameof(whereExpr), "If you really want to load ALL objects then pass 'Expr.ALL'");
            IAsyncResult ar = LoadItemsBegin(null, null,
                new V341SelectMultipleItems(dataTypeName, itemKind, whereExpr.Serialise(),
                    null, 0, 0, _defaultAppScopes, 0, includeDeleted, DateTimeOffset.Now, false));
            return LoadItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="itemKind"></param>
        /// <param name="whereExpr"></param>
        /// <param name="minimumUsn"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems(Type dataType, ItemKind itemKind, IExpression whereExpr, long minimumUsn, bool includeDeleted)
        {
            if (whereExpr == null)
                throw new ArgumentNullException(nameof(whereExpr), "If you really want to load ALL objects then pass 'Expr.ALL'");
            IAsyncResult ar = LoadItemsBegin(null, dataType,
                new V341SelectMultipleItems((dataType != null) ? dataType.FullName : null, itemKind, whereExpr.Serialise(),
                    null, 0, 0, _defaultAppScopes, minimumUsn, includeDeleted, DateTimeOffset.Now, false));
            return LoadItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="whereExpr"></param>
        /// <param name="minimumUsn"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, long minimumUsn, bool includeDeleted)
        {
            return LoadItems(dataType, ItemKind.Object, whereExpr, minimumUsn, includeDeleted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <param name="minimumUsn"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems<T>(IExpression whereExpr, long minimumUsn, bool includeDeleted)
        {
            return LoadItems(typeof(T), ItemKind.Object, whereExpr, minimumUsn, includeDeleted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems(IExpression whereExpr)
        {
            return LoadItems(null, ItemKind.Object, whereExpr, 0, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr)
        {
            return LoadItems(dataType, ItemKind.Object, whereExpr, 0, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems<T>(IExpression whereExpr)
        {
            return LoadItems(typeof(T), ItemKind.Object, whereExpr, 0, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <param name="minimumUsn"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public List<T> LoadObjects<T>(IExpression whereExpr, long minimumUsn, bool includeDeleted)
        {
            List<ICoreItem> items = LoadItems(typeof(T), whereExpr, minimumUsn, includeDeleted);
            return items.Select(item => (T)item.Data).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <param name="orderExpr"></param>
        /// <param name="startRow"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public List<T> LoadObjects<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            var items = LoadItems(typeof(T), whereExpr, orderExpr, startRow, rowCount);
            return items.Select(item => (T)item.Data).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemNames"></param>
        /// <returns></returns>
        public List<T> LoadObjects<T>(IEnumerable<string> itemNames)
        {
            List<ICoreItem> items = LoadItems(typeof(T), itemNames);
            return items.Select(item => (T)item.Data).ToList();
        }

        // delete methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTypeName"></param>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public int DeleteUntypedObjects(string dataTypeName, IExpression whereExpr)
        {
            if (whereExpr == null)
                throw new ArgumentNullException(nameof(whereExpr), "If you really want to delete ALL objects then pass Expr.ALL");
            IAsyncResult ar1 = LoadItemsBegin(null, null,
                new V341SelectMultipleItems(dataTypeName, ItemKind.Object, whereExpr.Serialise(),
                    null, 0, 0, _defaultAppScopes, 0, false, DateTimeOffset.Now, true));
            List<ICoreItem> currentItems = LoadItemsEnd(ar1);
            var deleteCompletions = new List<IAsyncResult>();
            foreach (ICoreItem currentItem in currentItems)
            {
                ICoreItem deletedItem = MakeItemFromText(currentItem.DataTypeName, null, currentItem.Name, currentItem.AppProps);
                deleteCompletions.Add(SaveItemBegin(deletedItem));
                if (deleteCompletions.Count >= 50)
                {
                    foreach (IAsyncResult ar in deleteCompletions)
                    {
                        SaveEnd(ar);
                    }
                    deleteCompletions.Clear();
                }
            }
            foreach (IAsyncResult ar in deleteCompletions)
                SaveEnd(ar);
            return currentItems.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public int DeleteTypedObjects(Type dataType, IExpression whereExpr)
        {
            return DeleteUntypedObjects(dataType != null ? dataType.FullName : null, whereExpr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public int DeleteObjects<T>(IExpression whereExpr)
        {
            return DeleteUntypedObjects(typeof(T).FullName, whereExpr);
        }

        // paging support methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public int CountObjects(Type dataType, IExpression whereExpr)
        {
            return LoadItemInfos(dataType, whereExpr).Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public int CountObjects<T>(IExpression whereExpr)
        {
            return LoadItemInfos<T>(whereExpr).Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="whereExpr"></param>
        /// <param name="orderExpr"></param>
        /// <param name="startRow"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            IAsyncResult ar = LoadItemsBegin(null, dataType,
                new V341SelectMultipleItems((dataType != null) ? dataType.FullName : null, ItemKind.Object, whereExpr?.Serialise(),
                    orderExpr?.Serialise(), startRow, rowCount, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
            return LoadItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <param name="orderExpr"></param>
        /// <param name="startRow"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems<T>(IExpression whereExpr, IExpression orderExpr, int startRow, int rowCount)
        {
            return LoadItems(typeof(T), whereExpr, orderExpr, startRow, rowCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public ICoreItemInfo LoadItemInfo<T>(string name) { return LoadItem(typeof(T), name); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ICoreItemInfo LoadItemInfo(Type dataType, string name) { return LoadItem(dataType, name); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public List<ICoreItemInfo> LoadItemInfos(Type dataType, IExpression whereExpr)
        {
            // get item info (without data)
            IAsyncResult ar = LoadItemsBegin(null, dataType,
                new V341SelectMultipleItems(dataType != null ? dataType.FullName : null, ItemKind.Object, whereExpr?.Serialise(),
                    null, 0, 0, _defaultAppScopes, 0, false, DateTimeOffset.Now, true));
            return ToInfoList(EndRequest(ar));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public List<ICoreItemInfo> LoadItemInfos<T>(IExpression whereExpr)
        {
            return LoadItemInfos(typeof(T), whereExpr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="itemNames"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems(Type dataType, IEnumerable<string> itemNames)
        {
            IAsyncResult ar = LoadItemsBegin(null, dataType,
                new V341SelectMultipleItems(dataType != null ? dataType.FullName : null, ItemKind.Object, itemNames.ToArray(),
                    _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
            return LoadItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems(Type dataType, IEnumerable<Guid> itemIds)
        {
            IAsyncResult ar = LoadItemsBegin(null, dataType, new V341SelectMultipleItems(itemIds.ToArray(), false));
            return LoadItemsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemNames"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems<T>(IEnumerable<string> itemNames)
        {
            return LoadItems(typeof(T), itemNames);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItems<T>(IEnumerable<Guid> itemIds)
        {
            return LoadItems(typeof(T), itemIds);
        }

        // asynchronous methods
        private IEnumerable<ClientItem> EndRequest(IAsyncResult ar)
        {
            bool debugRequests = DebugRequests;
            if (debugRequests)
                Logger.LogDebug("Waiting for completion...");
            GuardedList<ClientItem> source = ((AsyncResult<GuardedList<ClientItem>>)ar).EndInvoke();
            if (debugRequests)
                Logger.LogDebug("Request completed.");
            // note: don't cast the result - copy it to prevent enumerator multi-threading issues
            return source.ToList();
        }

        private List<ICoreItem> ToItemList(IEnumerable<ClientItem> clientItems)
        {
            return clientItems.Cast<ICoreItem>().ToList();
        }

        private List<ICoreItemInfo> ToInfoList(IEnumerable<ClientItem> clientItems)
        {
            return clientItems.Cast<ICoreItemInfo>().ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public ICoreItem LoadItemEnd(IAsyncResult ar)
        {
            List<ICoreItem> results = ToItemList(EndRequest(ar));
            if (results == null)
                throw new ApplicationException("Invalid results!");
            if (results.Count > 1)
                throw new ApplicationException("Too many results!");
            if (results.Count == 0)
                return null;
            return results[0];
        }

        private IAsyncResult LoadItemsBegin(AsyncCallback callback, Type dataTypeType, V341SelectMultipleItems selectRequest)
        {
            var ar = new AsyncResult<GuardedList<ClientItem>>(callback, null);
            var request = new RequestSelectMultipleItems(ar, dataTypeType, _requestTimeout, DebugRequests, selectRequest);
            EnqueueOutgoingRequest(request);
            return ar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadItemsEnd(IAsyncResult ar)
        {
            List<ICoreItem> results = ToItemList(EndRequest(ar));
            if (results == null)
                throw new ApplicationException("Invalid results!");
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="name"></param>
        /// <param name="dataTypeName"></param>
        /// <returns></returns>
        public IAsyncResult LoadObjectBegin(AsyncCallback callback, string name, string dataTypeName)
        {
            return LoadItemsBegin(callback, null,
                new V341SelectMultipleItems(dataTypeName, ItemKind.Object, name, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IAsyncResult LoadItemBegin(AsyncCallback callback, Guid id)
        {
            return LoadItemsBegin(callback, null, new V341SelectMultipleItems(id, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IAsyncResult LoadObjectBegin<T>(AsyncCallback callback, string name)
        {
            return LoadItemsBegin(callback, typeof(T),
                new V341SelectMultipleItems(typeof(T).FullName, ItemKind.Object, name, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IAsyncResult LoadItemBegin<T>(AsyncCallback callback, Guid id)
        {
            return LoadItemsBegin(callback, typeof(T), new V341SelectMultipleItems(id, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="dataType"></param>
        /// <param name="name"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public IAsyncResult LoadItemBegin(AsyncCallback callback, Type dataType, string name, bool includeDeleted)
        {
            return LoadItemsBegin(callback, dataType,
                new V341SelectMultipleItems(dataType.FullName, ItemKind.Object, name, _defaultAppScopes, 0, includeDeleted, DateTimeOffset.Now, false));
        }

        private IAsyncResult SaveItemsBegin(AsyncCallback callback, ClientItem item)
        {
            item.Freeze();
            var ar = new AsyncResult<GuardedList<ClientItem>>(callback, null);
            var request = new RequestNotifyMultipleItems(ar, null, _requestTimeout, DebugRequests,
                new V341NotifyMultipleItems(Guid.Empty, new V341TransportItem(item, false)));
            EnqueueOutgoingRequest(request);
            return ar;
        }

        private IAsyncResult SaveItemsBegin(AsyncCallback callback, IEnumerable<ClientItem> items)
        {
            var transportItems = new List<V341TransportItem>();
            foreach (ClientItem item in items)
            {
                item.Freeze();
                transportItems.Add(new V341TransportItem(item, false));
            }
            var ar = new AsyncResult<GuardedList<ClientItem>>(callback, null);
            var request = new RequestNotifyMultipleItems(ar, null, _requestTimeout, DebugRequests,
                new V341NotifyMultipleItems(Guid.Empty, transportItems.ToArray()));
            EnqueueOutgoingRequest(request);
            return ar;
        }

        private List<ICoreItem> SaveItemsEnd(IAsyncResult ar)
        {
            List<ICoreItem> results = ToItemList(EndRequest(ar));
            if (results == null)
                throw new ApplicationException("Invalid results!");
            return results;
        }

        // subscriptions
        private IAsyncResult SendCreateSubsBegin(AsyncCallback callback, Type dataTypeType, V341CreateSubscription createSubsRequest)
        {
            var ar = new AsyncResult<GuardedList<ClientItem>>(callback, null);
            var request = new RequestCreateSubscription(ar, dataTypeType, _requestTimeout, DebugRequests, createSubsRequest);
            EnqueueOutgoingRequest(request);
            return ar;
        }

        private List<ICoreItem> SendCreateSubsEnd(IAsyncResult ar)
        {
            List<ICoreItem> results = ToItemList(EndRequest(ar));
            if (results == null)
                throw new ApplicationException("Invalid results!");
            return results;
        }

        private IAsyncResult SendKeepaliveBegin(AsyncCallback callback, Type dataTypeType, V341ExtendSubscription extendSubsRequest)
        {
            var ar = new AsyncResult<GuardedList<ClientItem>>(callback, null);
            var request = new RequestExtendSubscription(ar, dataTypeType, _requestTimeout, DebugRequests, extendSubsRequest);
            EnqueueOutgoingRequest(request);
            return ar;
        }

        private List<ICoreItem> SendKeepAliveEnd(IAsyncResult ar)
        {
            List<ICoreItem> results = ToItemList(EndRequest(ar));
            if (results == null)
                throw new ApplicationException("Invalid results!");
            return results;
        }

        private IAsyncResult SendCancelSubsBegin(AsyncCallback callback, Type dataTypeType, V341CancelSubscription cancelSubsRequest)
        {
            var ar = new AsyncResult<GuardedList<ClientItem>>(callback, null);
            var request = new RequestCancelSubscription(ar, dataTypeType, _requestTimeout, DebugRequests, cancelSubsRequest);
            EnqueueOutgoingRequest(request);
            return ar;
        }

        private List<ICoreItem> SendCancelSubsEnd(IAsyncResult ar)
        {
            List<ICoreItem> results = ToItemList(EndRequest(ar));
            if (results == null)
                throw new ApplicationException("Invalid results!");
            return results;
        }

        private void AsyncCallProcessOutgoingRequests()
        {
            Interlocked.Increment(ref _asyncCallProcessOutgoingRequestsCount);
            _mainThreadQueue.Dispatch<object>(null, AsyncExecProcessOutgoingRequests);
        }

        private void HousekeepingRequestsTimeout(object notUsed)
        {
            AsyncCallProcessOutgoingRequests();
        }

        private void HeartbeatTimeout(object notUsed)
        {
            // (not on main thread) build and publish a heartbeat message
            int count = Interlocked.Increment(ref _heartbeatCalls);
            try
            {
                // return if still busy sending last heartbeat
                if (count > 1)
                    return;
                // don't bother sending if faulted
                if (_coreState == CoreStateEnum.Faulted)
                    return;
                // extend subscriptions
                IAsyncResult ar = SendKeepaliveBegin(null, null, new V341ExtendSubscription());
                SendKeepAliveEnd(ar);
                // send heartbeat message
                string itemName =
                    $"System.Heartbeat.{ClientInfo.HostName}.{ClientInfo.UserName}.{ClientInfo.ApplName}.{ClientInfo.NodeGuid}";
                ICoreItem item = MakeItem("Heartbeat", itemName, null, true, TimeSpan.FromMinutes(10));
                SaveItem(item);
            }
            catch (Exception e)
            {
                Logger.LogError("Heartbeat failed: {0}: {1}", e.GetType().Name, e.Message);
            }
            finally
            {
                Interlocked.Decrement(ref _heartbeatCalls);
            }
        }

        #region ITransferV341 Members

        private void CommenceHeaderProcessing(V131SessionHeader header, out RequestBase request)
        {
            // data received - only accept messages with a valid request id
            RequestBase result = null;
            _incompleteRequests.Locked(requests => requests.RequestCache.TryGetValue(header.RequestIdGuid, out result));
            request = result;
            if (request == null)
            {
                // unknown request
                Logger.LogWarning("Request '{0}' unknown - ignoring data", header.RequestId);
            }

        }

        private void CompleteHeaderProcessing(V131SessionHeader header, RequestBase request, Exception excp)
        {
            if (request != null)
            {
                if (excp != null)
                {
                    request.Fault = excp;
                    //request.AsyncResult.SetAsCompleted(excp, false);
                    AsyncCallProcessOutgoingRequests();
                    Logger.LogError("Request '{0}' failed: {1}", header.RequestId, excp);
                }
                else if (!header.MoreFollowing)
                {
                    request.Completed = true;
                    //request.AsyncResult.SetAsCompleted(request.PartialResults, false);
                    AsyncCallProcessOutgoingRequests();
                    if (header.DebugRequest)
                        Logger.LogDebug("Request '{0}' completed normally ({1}ms).", header.RequestId, (DateTimeOffset.Now - request.SubmitTime).TotalMilliseconds);
                }
            }
        }

        public void TransferV341AnswerMultipleItems(V131SessionHeader header, V341AnswerMultipleItems body)
        {
            _mainThreadQueue.Dispatch(new PackageAnswerMultipleItems(header, body), PerformSyncRecvAnswerMultipleItems);
        }
        private void PerformSyncRecvAnswerMultipleItems(PackageAnswerMultipleItems package)
        {
            // the received data is a response to a select request
            RequestBase request = null;
            try
            {
                if (DebugRequests || package.Header.DebugRequest)
                    Logger.LogDebug("Request '{0}' received {1} items",
                        package.Header.RequestId,
                        package.Body.Items?.Count ?? 0);
                CommenceHeaderProcessing(package.Header, out request);
                if (request != null)
                {
                    // add the received items to the results
                    if (package.Body.Items != null)
                        foreach (V341TransportItem item in package.Body.Items)
                        {
                            //if (_DebugRequests || package.Header.DebugRequest)
                            //    _Logger.LogDebug("Request '{0}' received item: {1}", package.Header.RequestId, item.ItemName);
                            request.PartialResults.Add(new ClientItem(this, item, request.DataTypeType));
                        }
                    CompleteHeaderProcessing(package.Header, request, null);
                }
            }
            catch (Exception excp)
            {
                CompleteHeaderProcessing(package.Header, request, excp);
            }
        }

        public void TransferV341NotifyMultipleItems(V131SessionHeader header, V341NotifyMultipleItems body)
        {
            _mainThreadQueue.Dispatch(new PackageNotifyMultipleItems(header, body), PerformSyncRecvNotifyMultipleItems);
        }
        private void PerformSyncRecvNotifyMultipleItems(PackageNotifyMultipleItems package)
        {
            // the received data is a match for a current subscription.
            if (DebugRequests || package.Header.DebugRequest)
            {
                if (package.Body.SubscriptionIdGuid != Guid.Empty)
                {
                    Logger.LogDebug("Subscription '{0}' received {1} items",
                    package.Body.SubscriptionId,
                    package.Body.Items?.Count ?? 0);
                }
            }
            Subscription subscription = null;
            _subscriptions.Locked(subscriptions =>
            {
                subscriptions.TryGetValue(package.Body.SubscriptionIdGuid, out var temp);
                subscription = temp;
            });
            if (subscription != null)
            {
                // notify the subscriber
                if (package.Body.Items != null)
                    foreach (V341TransportItem item in package.Body.Items)
                    {
                        if (DebugRequests || package.Header.DebugRequest)
                            Logger.LogDebug("Subscription '{0}' received item: {1}", package.Body.SubscriptionId, item.ItemName);
                        subscription.ProcessUpdate(new ClientItem(this, item, subscription.DataType));
                    }
            }
            else
            {
                // unknown subscription
                Logger.LogWarning("Subscription '{0}' unknown - ignoring data", package.Body.SubscriptionId);
            }
        }

        public void TransferV341CompletionResult(V131SessionHeader header, V341CompletionResult body)
        {
            _mainThreadQueue.Dispatch(new PackageCompletionResult(header), PerformSyncRecvCompletionResult);
        }
        private void PerformSyncRecvCompletionResult(PackageCompletionResult package)
        {
            RequestBase request = null;
            try
            {
                if (DebugRequests || package.Header.DebugRequest)
                    Logger.LogDebug("Request '{0}' received CompletionResult", package.Header.RequestId);
                CommenceHeaderProcessing(package.Header, out request);
                if (request != null)
                {
                    // nothing to do - just complete the request
                    CompleteHeaderProcessing(package.Header, request, null);
                }
            }
            catch (Exception excp)
            {
                CompleteHeaderProcessing(package.Header, request, excp);
            }
        }

        public void TransferV341SelectMultipleItems(V131SessionHeader header, V341SelectMultipleItems body)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void OpenServerHostForReplies()
        {
            //const int minPort = 10000;
            //const int maxPort = 65535;
            //const int maxAttempts = 10;
            //var random = new Random(Environment.TickCount);
            //int attempt = 0;
            //string protocol = ServerAddress.Protocol;
            //while (_replyServiceHost == null)
            //{
            //    if (attempt >= maxAttempts)
            //        throw new ApplicationException(
            //            $"Aborting - open host attempt limit ({maxAttempts}) reached!");
            //    // attempt to start a new host on a new port
            //    attempt++;
            //    int port = random.Next(minPort, maxPort);
            //    // clear session id to force client reconnect
            //    _sessionId = Guid.Empty;
            //    try
            //    {
            //        string endpoint = ServiceHelper.FormatEndpoint(protocol, port);
            //        _replyServiceHost = new CustomServiceHost<ITransferV341, TransferRecverV341>(
            //            Logger, new TransferRecverV341(this), endpoint,
            //            ClientInfo.ApplName, typeof(ITransferV341).Name, false);
            //    }
            //    catch (AddressAlreadyInUseException e1)
            //    {
            //        // expected often
            //        Logger.LogDebug("Failed to open port {0}: {1}: {2}", port, e1.GetType().Name, e1.Message);
            //        DisposeHelper.SafeDispose(ref _replyServiceHost);
            //    }
            //    catch (InvalidOperationException e2)
            //    {
            //        // expected but not often
            //        Logger.LogDebug("Failed to open port {0}: {1}: {2}", port, e2.GetType().Name, e2.Message);
            //        DisposeHelper.SafeDispose(ref _replyServiceHost);
            //    }
            //    catch (CommunicationException e3)
            //    {
            //        // unexpected communications error
            //        Logger.LogWarning("Failed to open port {0}: {1}: {2}", port, e3.GetType().Name, e3.Message);
            //        DisposeHelper.SafeDispose(ref _replyServiceHost);
            //        throw;
            //    }
            //} // while
            //_replyAddress = _replyServiceHost.GetIpV4Addresses(protocol).ToArray()[0];
        }

        private void BackoffRetryAlgorithm(Exception excp, Guid requestId, DateTimeOffset expiryTime, RequestBase request, int attempt)
        {
            // backoff (between 0 and 15 seconds) if max timeout not reached
            const double minBackoff = 0.125;
            const double maxBackoff = 15.0;
            double backoff = minBackoff + 0.01 * (attempt - 1) * (attempt - 1) + 0.1 * (attempt - 1);
            if (backoff < minBackoff)
                backoff = minBackoff;
            if (backoff > maxBackoff)
                backoff = maxBackoff;
            // start outage (if not started)
            CoreState = CoreStateEnum.Connecting;
            // check request timeouts
            if (request != null)
            {
                if (DateTimeOffset.Now > request.ExpiryTime)
                {
                    // request expired
                    request.AsyncResult.SetAsCompleted(new TimeoutException("RequestTimeout expired!"), false);
                    return;
                }
                if ((DateTimeOffset.Now - _offlineCommenced) > _offlineTimeout)
                {
                    // outage exceeded maximum
                    request.AsyncResult.SetAsCompleted(new TimeoutException("OfflineTimeout expired!"), false);
                    // goto faulted or offline state
                    CoreState = CoreMode == CoreModeEnum.Standard ? CoreStateEnum.Faulted : CoreStateEnum.Offline;
                    return;
                }
            }
            DisposeHelper.SafeDispose(ref _clientBase);
            if (attempt <= 1)
            {
                // initial catch
                //Logger.LogWarning("Request '{0}' to server at '{1}' failed: {2}",
                //    requestId, _transferAddressBinding.Address.Uri.AbsoluteUri, excp.GetType().Name);
                Logger.LogDebug("Request '{0}': connection attempts will continue until {1}", requestId, expiryTime);
            }
            else
            {
                // repeat catch
                Logger.LogDebug("Request '{0}': connection attempt ({1}) failed: {2}",
                    requestId, attempt, excp.GetType().Name);
                // 2nd or subsequent failure - could be a problem at our end
                // - force restart of the reply host
                //DisposeHelper.SafeDispose(ref _replyServiceHost);
            }
            if (DateTimeOffset.Now < expiryTime)
                Thread.Sleep(TimeSpan.FromSeconds(backoff));
            else
            {
                //Logger.LogError(
                //    "Request '{0}' to server at '{1}' failed: {2}",
                //    requestId,
                //    _transferAddressBinding.Address.Uri.AbsoluteUri,
                //    excp.GetType().Name);
                throw excp;
            }
        }

        private void OpenClientBaseAndSendRequest(bool keepClientOpen, RequestBase request)
        {
            Guid requestId = Guid.Empty;
            DateTimeOffset expiryTime = DateTime.Now.Add(_requestTimeout);
            if (request != null)
            {
                requestId = request.RequestId;
                expiryTime = request.ExpiryTime;
            }
            try
            {
                // attempt to send
                int attempt = 0;
                bool sent = false;
                while (!sent && _coreState != CoreStateEnum.Faulted)
                {
                    attempt++;
                    // (re)open reply service host if required
                    OpenServerHostForReplies();
                    try
                    {
                        // reconnected if required
                        if (_clientBase == null)
                        {
                            //if (attempt > 1)
                            //    Logger.LogDebug("Reconnect attempt ({0}) to server at: {1}", attempt, _sessCtrlAddressBinding.Address.Uri.AbsoluteUri);
                            //if (_sessionId == Guid.Empty)
                            //{
                            //    CoreState = CoreStateEnum.Connecting;
                            //    using (var session = new SessCtrlSenderV131(_sessCtrlAddressBinding))
                            //    {
                            //        var sessionHeader = new V131SessionHeader(Guid.Empty, Guid.NewGuid(), false, false, _replyAddress, _replyContract, DebugRequests);
                            //        var sessionReply = session.BeginSessionV131(sessionHeader, _clientInfoV131);
                            //        if (!sessionReply.Success)
                            //        {
                            //            throw new ApplicationException("Connection rejected: " + sessionReply.Message);
                            //        }
                            //        _sessionId = sessionReply.SessionId;
                            //    }
                            //}
                            ////instantiate a channel
                            //_clientBase = new TransferV341Client(_transferAddressBinding);
                        }
                        if (request != null)
                        {
                            if (request.DebugRequest)
                                Logger.LogDebug("Request '{0}': sending {1} ...", requestId, request.GetType().Name);
                            var header = new V131SessionHeader(_sessionId, request.RequestId, false, true, _replyAddress, _replyContract, request.DebugRequest);
                            request.Transmit(Logger, _clientBase, header);
                        }
                        sent = true;
                        if (request != null && request.DebugRequest)
                            Logger.LogDebug("Request '{0}': sent.", requestId);
                        if (attempt > 1)
                            Logger.LogDebug("Request '{0}': Connect attempt ({1}) succeeded.", requestId, attempt);
                        // end outage (if any)
                        _offlineCommenced = DateTimeOffset.Now;
                        CoreState = CoreStateEnum.Connected;
                    }
                    catch (CommunicationException commsExcp)
                    {
                        // expected - retry
                        BackoffRetryAlgorithm(commsExcp, requestId, expiryTime, request, attempt);
                    }
                    catch (TimeoutException timeoutExcp)
                    {
                        // expected - retry
                        BackoffRetryAlgorithm(timeoutExcp, requestId, expiryTime, request, attempt);
                    }
                    catch (Exception unexpectedExcp)
                    {
                        // unexpected - fault immediately
                        CoreState = CoreStateEnum.Faulted;
                        Logger.Log(unexpectedExcp);
                        throw;
                    }
                } // while
            }
            finally
            {
                if (!keepClientOpen)
                    DisposeHelper.SafeDispose(ref _clientBase);
            }
        }

        private void AsyncExecProcessOutgoingRequests(object notUsed1)
        {
            if (Interlocked.Decrement(ref _asyncCallProcessOutgoingRequestsCount) > 0)
                return;
            // dequeue request (if any)
            RequestBase unsentRequest = null;
            bool moreFollowing = false;
            _incompleteRequests.Locked(requests =>
            {
                int queueLength = requests.OutgoingRequestQueue.Count;
                if (queueLength > 0)
                {
                    Guid requestId = requests.OutgoingRequestQueue.Dequeue();
                    unsentRequest = requests.RequestCache[requestId];
                    moreFollowing = (queueLength > 1);
                }
            });
            while (unsentRequest != null)
            {
                // do not send if client has faulted
                if (_coreState != CoreStateEnum.Faulted)
                {
                    // attempt send
                    try
                    {
                        OpenClientBaseAndSendRequest(moreFollowing, unsentRequest);
                    }
                    catch (CommunicationException e)
                    {
                        unsentRequest.Fault = e;
                    }
                }
                // dequeue next request (if any)
                unsentRequest = null;
                _incompleteRequests.Locked(requests =>
                {
                    if (requests.OutgoingRequestQueue.Count > 0)
                    {
                        Guid requestId = requests.OutgoingRequestQueue.Dequeue();
                        unsentRequest = requests.RequestCache[requestId];
                    }
                });
            } // while
            // housekeeping
            try
            {
                var completedRequests = new List<RequestBase>();
                _incompleteRequests.Locked(requests =>
                {
                    // scan all the pending requests
                    foreach (RequestBase request in requests.RequestCache.Values)
                    {
                        if (request.Completed)
                        {
                            // request completed
                            request.AsyncResult.SetAsCompleted(request.PartialResults, false);
                            completedRequests.Add(request);
                        }
                        else
                        {
                            if (request.Fault != null)
                            {
                                // comms exception
                                request.AsyncResult.SetAsCompleted(request.Fault, false);
                                completedRequests.Add(request);
                            }
                            else if (_coreState == CoreStateEnum.Faulted)
                            {
                                // general fault
                                request.AsyncResult.SetAsCompleted(new TimeoutException("General fault!"), false);
                                completedRequests.Add(request);
                            }
                        }
                    }
                    // now remove all completed requests
                    foreach (RequestBase request in completedRequests)
                        requests.RequestCache.Remove(request.RequestId);
                });
            }
            catch (Exception e)
            {
                Logger.LogError("HousekeepingRequests: Unhandled exception: {0}", e);
            }

        }

        internal class PackageBase
        {
            public readonly V131SessionHeader Header;
            public PackageBase(V131SessionHeader header)
            {
                Header = header;
            }
        }
        internal class PackageAnswerMultipleItems : PackageBase
        {
            public readonly V341AnswerMultipleItems Body;
            public PackageAnswerMultipleItems(V131SessionHeader header, V341AnswerMultipleItems body)
                : base(header)
            {
                Body = body;
            }
        }

        private class PackageNotifyMultipleItems : PackageBase
        {
            public readonly V341NotifyMultipleItems Body;
            public PackageNotifyMultipleItems(V131SessionHeader header, V341NotifyMultipleItems body)
                : base(header)
            {
                Body = body;
            }
        }

        private class PackageCompletionResult : PackageBase
        {
            public PackageCompletionResult(V131SessionHeader header)
                : base(header)
            {
            }
        }

        public IAsyncResult LoadObjectsBegin(AsyncCallback callback, IExpression whereExpr, string dataTypeName)
        {
            return LoadItemsBegin(callback, null,
                new V341SelectMultipleItems(dataTypeName, ItemKind.Object, whereExpr?.Serialise(),
                    null, 0, 0, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
        }
        public IAsyncResult LoadObjectsBegin<T>(AsyncCallback callback, IExpression whereExpr)
        {
            return LoadItemsBegin(callback, typeof(T),
                new V341SelectMultipleItems(typeof(T).FullName, ItemKind.Object, whereExpr?.Serialise(),
                    null, 0, 0, _defaultAppScopes, 0, false, DateTimeOffset.Now, false));
        }

        // configuration methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newSettings"></param>
        /// <param name="applName"></param>
        /// <param name="userName"></param>
        /// <param name="hostName"></param>
        /// <param name="replaceOldSettings"></param>
        /// <param name="env"></param>
        public void SaveAppSettings(NamedValueSet newSettings, string applName, string userName, string hostName, bool replaceOldSettings, EnvId env)
        {
            // get old record (if any)
            var oldSettings = new NamedValueSet();
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
                    foreach (var rule in rules)
                    {
                        // config values are cumulative
                        if (!rule.Disabled)
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
            var appCfg = new AppCfgRuleV2();
            var settings = new NamedValueSet();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newSettings"></param>
        /// <param name="applName"></param>
        /// <param name="userName"></param>
        /// <param name="hostName"></param>
        /// <param name="replaceOldSettings"></param>
        public void SaveAppSettings(NamedValueSet newSettings, string applName, string userName, string hostName, bool replaceOldSettings)
        {
            SaveAppSettings(newSettings, applName, userName, hostName, replaceOldSettings, ClientInfo.ConfigEnv);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public void SaveAppSettings(NamedValueSet settings)
        {
            SaveAppSettings(settings, ClientInfo.ApplName, ClientInfo.UserName, ClientInfo.HostName, false, ClientInfo.ConfigEnv);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="applName"></param>
        public void SaveAppSettings(NamedValueSet settings, string applName)
        {
            SaveAppSettings(settings, applName, ClientInfo.UserName, ClientInfo.HostName, false, ClientInfo.ConfigEnv);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applName"></param>
        /// <param name="userName"></param>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public NamedValueSet LoadAppSettings(string applName, string userName, string hostName)
        {
            var result = new NamedValueSet();
            IExpression query = Expr.BoolAND(
                Expr.BoolOR(Expr.IsEQU(AppPropName.ApplName, "*"),
                    (applName == null ? Expr.IsEQU(AppPropName.ApplName, ClientInfo.ApplName) : Expr.StartsWith(AppPropName.ApplName, applName))),
                Expr.BoolOR(Expr.IsEQU(AppPropName.UserName, "*"),
                    (userName == null ? Expr.IsEQU(AppPropName.UserName, ClientInfo.UserName) : Expr.IsEQU(AppPropName.UserName, userName))),
                Expr.BoolOR(Expr.IsEQU(AppPropName.HostName, "*"),
                    (hostName == null ? Expr.IsEQU(AppPropName.HostName, ClientInfo.HostName) : Expr.IsEQU(AppPropName.HostName, hostName))),
                Expr.BoolOR(Expr.IsEQU("Env", EnvHelper.EnvName(ClientInfo.ConfigEnv)), Expr.IsEQU("Env", "*")));
            Logger.LogDebug("LoadAppSettings: Query={0}", query.DisplayString());
            List<AppCfgRuleV2> rules = LoadObjects<AppCfgRuleV2>(query);
            if (rules.Count > 0)
            {
                rules.Sort();
                int n = 0;
                foreach (AppCfgRuleV2 rule in rules)
                {
                    // config values are cumulative
                    Logger.LogDebug("LoadAppSettings: Result[{0}]:{1},{2},{3},{4},{5},{6},{7}",
                        n, rule.Priority, rule.Disabled, rule.Env, rule.ApplName, rule.UserName, rule.HostName, rule.Settings);
                    if (!rule.Disabled)
                        result.Add(new NamedValueSet(rule.Settings));
                    n++;
                }
            }
            Logger.LogDebug("LoadAppSettings: Results:");
            result.LogValues((text) => Logger.LogDebug("  " + text));
            return result;
        }

        /// <summary>
        /// </summary>
        public NamedValueSet LoadAppSettings(string applName)
        {
            return LoadAppSettings(applName, null, null);
        }

        /// <summary>
        /// </summary>
        public NamedValueSet LoadAppSettings()
        {
            return LoadAppSettings(null, null, null);
        }

        // subscription management
        private Guarded<Dictionary<Guid, Subscription>> _subscriptions = new Guarded<Dictionary<Guid, Subscription>>(new Dictionary<Guid, Subscription>());

        /// <summary>
        /// Creates but does not start a subscription.
        /// </summary>
        /// <returns></returns>
        private ISubscription CreateSubscription()
        {
            var result = new Subscription(Logger, this);
            _subscriptions.Locked(subscriptions => subscriptions[result.Id] = result);
            return result;
        }
        
        internal void StartSubscription(Guid subscriptionId)
        {
            IAsyncResult ar = null;
            _subscriptions.Locked(subscriptions =>
            {
                subscriptions.TryGetValue(subscriptionId, out var subscription);
                if (subscription == null)
                    throw new ArgumentException("Invalid subscriptionId: " + subscriptionId.ToString());
                // send create request to server
                ar = SendCreateSubsBegin(null, subscription.DataType,
                    new V341CreateSubscription(
                        subscription.Id,
                        DateTimeOffset.Now.Add(_maxOfflineTimeout), // ignored
                        subscription.ItemKind,
                        subscription.DataTypeName,
                        subscription.WhereExpr?.Serialise(),
                        subscription.AppScopes, subscription.MinimumUSN,
                        subscription.ExcludeDeleted,
                        subscription.ExcludeExisting,
                        subscription.WaitForExisting,
                        subscription.AsAtTime, subscription.ExcludeDataBody));
                if (subscription.WhereExpr != null)
                    Logger.LogDebug("Started subscription: <{0}> {1} ({2})", subscription.DataTypeName,
                        subscription.WhereExpr.DisplayString(), subscription.Id);
            });
            // wait for request to complete
            SendCreateSubsEnd(ar);
        }

        internal IAsyncResult CancelSubscriptionBegin(Guid subscriptionId)
        {
            // stops and removes a subscription
            IAsyncResult ar = null;
            _subscriptions.Locked(subscriptions =>
            {
                if (subscriptions.TryGetValue(subscriptionId, out var subscription))
                {
                    subscriptions.Remove(subscriptionId);
                    if (subscription.Started)
                    {
                        ar = SendCancelSubsBegin(null, null, new V341CancelSubscription(subscriptionId));
                        Logger.LogDebug("Subscription '{0}' cancelled", subscription.Id);
                    }
                    else
                    {
                        Logger.LogDebug("Subscription '{0}' not started!", subscription.Id);
                    }
                }
                else
                {
                    Logger.LogDebug("Subscription '{0}' missing!", subscriptionId.ToString());
                }
            });
            return ar;
        }

        internal void CancelSubscriptionEnd(IAsyncResult ar)
        {
            // wait for request to complete
            if (ar != null)
                SendCancelSubsEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public ISubscription CreateSubscription<T>(IExpression whereExpr) //, SubscriptionCallback userCallback)
        {
            ISubscription result = CreateSubscription();
            result.DataType = typeof(T);
            result.WhereExpr = whereExpr;
            //result.UserCallback = userCallback;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public ISubscription CreateTypedSubscription(Type dataType, IExpression whereExpr) //, SubscriptionCallback userCallback)
        {
            ISubscription result = CreateSubscription();
            result.DataType = dataType;
            result.WhereExpr = whereExpr;
            //result.UserCallback = userCallback;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTypeName"></param>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public ISubscription CreateUntypedSubscription(string dataTypeName, IExpression whereExpr) //, SubscriptionCallback userCallback)
        {
            ISubscription result = CreateSubscription();
            result.DataTypeName = dataTypeName;
            result.WhereExpr = whereExpr;
            //result.UserCallback = userCallback;
            return result;
        }

        private ISubscription SubscribePrivate(
            Type dataType, IExpression filter,
            bool excludeExisting, bool waitForExisting, bool excludeDataBody,
            SubscriptionCallback userCallback, object userContext)
        {
            ISubscription subscription = CreateSubscription();
            subscription.DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            subscription.WhereExpr = filter ?? throw new ArgumentNullException(nameof(filter));
            subscription.UserCallback = userCallback;
            subscription.UserContext = userContext;
            subscription.ItemKind = ItemKind.Object;
            subscription.ExcludeExisting = excludeExisting;
            subscription.WaitForExisting = waitForExisting;
            subscription.ExcludeDataBody = excludeDataBody;
            subscription.Start();
            return subscription;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="filter"></param>
        /// <param name="userCallback"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        public ISubscription Subscribe(Type dataType, IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            return SubscribePrivate(dataType, filter, false, true, false, userCallback, userContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ISubscription Subscribe<T>(IExpression filter)
        {
            return SubscribePrivate(typeof(T), filter, false, true, false, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="userCallback"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        public ISubscription SubscribeNoWait<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            return SubscribePrivate(typeof(T), filter, false, false, false, userCallback, userContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="userCallback"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        public ISubscription SubscribeNewOnly<T>(IExpression filter, SubscriptionCallback userCallback, object userContext)
        {
            return SubscribePrivate(typeof(T), filter, true, false, false, userCallback, userContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ISubscription SubscribeInfoOnly<T>(IExpression filter)
        {
            return SubscribePrivate(typeof(T), filter, false, false, true, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <param name="userCallback"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        public ISubscription Subscribe<T>(IExpression whereExpr, SubscriptionCallback userCallback, object userContext)
        {
            return Subscribe(typeof(T), whereExpr, userCallback, userContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTypeName"></param>
        /// <param name="whereExpr"></param>
        /// <param name="userCallback"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        public ISubscription StartUntypedSubscription(string dataTypeName, IExpression whereExpr, SubscriptionCallback userCallback, object userContext)
        {
            ISubscription result = CreateSubscription();
            result.DataTypeName = dataTypeName;
            result.WhereExpr = whereExpr ?? Expr.ALL;
            result.UserCallback = userCallback;
            result.UserContext = userContext;
            result.Start();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ICoreCache CreateCache()
        {
            return new CoreCache(Logger, this, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changeHandler"></param>
        /// <param name="syncContext"></param>
        /// <returns></returns>
        public ICoreCache CreateCache(CacheChangeHandler changeHandler, SynchronizationContext syncContext)
        {
            return new CoreCache(Logger, this, changeHandler, syncContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341CreateSubscription(V131SessionHeader header, V341CreateSubscription body)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341ExtendSubscription(V131SessionHeader header, V341ExtendSubscription body)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341CancelSubscription(V131SessionHeader header, V341CancelSubscription body)
        {
            throw new NotSupportedException();
        }

        // private (in-process only) objects
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public Guid SavePrivateObject<T>(T data, string name, NamedValueSet props)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public ICoreItem LoadPrivateItem<T>(string name)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpr"></param>
        /// <returns></returns>
        public List<ICoreItem> LoadPrivateItems<T>(IExpression whereExpr)
        {
            throw new NotSupportedException();
        }
    }
}

