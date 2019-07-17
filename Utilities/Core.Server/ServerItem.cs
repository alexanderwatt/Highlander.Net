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
using Core.Common;
using Core.Common.Encryption;
using Orion.Util.Compression;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;

#endregion

namespace Core.Server
{
    internal class ServerItem : CommonItem
    {
        private readonly IModuleInfo _moduleInfo;
        private readonly ICryptoManager _cryptoManager;
        private bool _frozen;
        private TimeSpan _lifetime;
        // mutable until frozen
        private int _ySignedState;
        // data buffers
        private byte[] _xData;
        private byte[] _zData;
        private string _text;
        private object _data;
        private Type _dataTypeType;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleInfo"></param>
        /// <param name="cryptoManager"></param>
        /// <param name="itemKind"></param>
        /// <param name="transient"></param>
        /// <param name="appScope"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <param name="serialFormat"></param>
        /// <param name="lifetime"></param>
        public ServerItem(
            IModuleInfo moduleInfo,
            ICryptoManager cryptoManager,
            ItemKind itemKind,
            bool transient,
            string appScope,
            string name,
            NamedValueSet props,
            object data,
            Type dataType,
            SerialFormat serialFormat,
            TimeSpan lifetime)
            : base(itemKind, transient, name, appScope)
        {
            _moduleInfo = moduleInfo ?? throw new ArgumentNullException(nameof(moduleInfo));
            _cryptoManager = cryptoManager ?? throw new ArgumentNullException(nameof(cryptoManager));
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));
            SysProps.Set(SysPropName.SAlg, (int)serialFormat);
            AppProps.Add(props);
            _data = data;
            _dataTypeType = dataType;
            DataTypeName = dataType.FullName;
            _lifetime = lifetime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemKind"></param>
        /// <param name="transient"></param>
        /// <param name="appScope"></param>
        /// <param name="name"></param>
        /// <param name="props"></param>
        /// <param name="serialisedData"></param>
        /// <param name="dataTypeName"></param>
        /// <param name="lifetime"></param>
        public ServerItem(
            ItemKind itemKind,
            bool transient,
            string appScope,
            string name,
            NamedValueSet props,
            string serialisedData,
            string dataTypeName,
            TimeSpan lifetime)
            : base(itemKind, transient, name, appScope)
        {
            AppProps.Add(props);
            _text = serialisedData;
            SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Undefined);
            DataTypeName = dataTypeName;
            _lifetime = lifetime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public ServerItem(
            RawItem item)
            : base(item.ItemId, (ItemKind)item.ItemKind, item.Transient,
                item.ItemName, new NamedValueSet(item.AppProps),
                item.DataType, item.AppScope,
                new NamedValueSet(item.SysProps), item.NetScope,
                item.Created, item.Expires,
                item.YData, item.YSign,
                item.StoreUSN)
        {
            _frozen = true;
        }

        public bool Frozen => _frozen;

        private void CheckNotFrozen()
        {
            if (_frozen)
                throw new ApplicationException("Item already frozen/saved!");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Freeze()
        {
            if (_frozen)
                return;
            if (Name == null)
                throw new ApplicationException("Item name not set!");
            TimeSpan maxLifetime = DateTimeOffset.MaxValue - DateTimeOffset.Now - TimeSpan.FromDays(1);
            if (_lifetime > maxLifetime)
                _lifetime = maxLifetime;
            if (_lifetime < TimeSpan.Zero)
                _lifetime = TimeSpan.Zero;
            Created = DateTimeOffset.Now;
            Expires = Created.Add(_lifetime);
            // serialise the data if required
            Serialise();
            if (DataTypeName == null)
                DataTypeName = "";
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
            var xtki = SysProps.GetValue<String>(SysPropName.XTKI, null);
            if (xtki != null)
            {
                _xData = _cryptoManager.EncryptWithTranspKey(xtki, _zData);
                SysProps.Set(SysPropName.XAlg, 1);
            }
            else
                _xData = _zData;
            SysProps.Set(SysPropName.XLen, _xData?.Length ?? 0);
            // do asymmetric encryption 2nd, if required
            var yrki = SysProps.GetValue<String>(SysPropName.YRKI, null);
            if (yrki != null)
            {
                SysProps.Set(SysPropName.YAlg, 1);
                YData = _cryptoManager.EncryptWithPublicKey(yrki, _xData);
            }
            else
                YData = _xData;
            YDataHash = CalculateBufferHash(YData);
            SysProps.Set(SysPropName.YLen, YData?.Length ?? 0);
            // do public signature 3rd, if required
            var yski = SysProps.GetValue<String>(SysPropName.YSKI, null);
            if (yski != null)
            {
                SysProps.Set(SysPropName.YAlg, 1);
                YSign = _cryptoManager.CreateSignature(yski, YData);
            }
            // add other publisher properties
            SysProps.Set(SysPropName.ApplName, _moduleInfo.ApplName);
            SysProps.Set(SysPropName.ApplFVer, _moduleInfo.ApplFVer);
            SysProps.Set(SysPropName.ApplPTok, _moduleInfo.ApplPTok);
            SysProps.Set(SysPropName.CoreFVer, _moduleInfo.CoreFVer);
            SysProps.Set(SysPropName.CorePTok, _moduleInfo.CorePTok);
            SysProps.Set(SysPropName.HostName, _moduleInfo.HostName);
            SysProps.Set(SysPropName.UserName, _moduleInfo.UserName);
            SysProps.Set(SysPropName.UserWDom, _moduleInfo.UserWDom);
            SysProps.Set(SysPropName.UserIdentity, _moduleInfo.Name);
            SysProps.Set(SysPropName.UserFullName, _moduleInfo.UserFullName);
            SysProps.Set(SysPropName.OrgEnvId, EnvHelper.EnvName(_moduleInfo.ConfigEnv));
            SysProps.Set(SysPropName.NodeGuid, _moduleInfo.NodeGuid);

            // done
            _frozen = true;
        }

        // mutable props
        private void Authenticate()
        {
            if (_ySignedState == 0)
            {
                var yAlg = SysProps.GetValue(SysPropName.YAlg, 0);
                var yski = SysProps.GetValue<string>(SysPropName.YSKI, null);
                if ((yAlg > 0) && (yski != null))
                {
                    if (_cryptoManager.VerifySignature(yski, YData, YSign))
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
        /// 
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
        /// 
        /// </summary>
        public bool IsSecret
        {
            get
            {
                var yAlg = SysProps.GetValue(SysPropName.YAlg, 0);
                var yrki = SysProps.GetValue<string>(SysPropName.YRKI, null);
                return ((yAlg > 0) && (yrki != null));
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
                    var yrki = SysProps.GetValue<string>(SysPropName.YRKI, null);
                    if ((yAlg > 0) && (yrki != null))
                        _xData = _cryptoManager.DecryptWithSecretKey(yrki, YData);
                    else
                        _xData = YData;
                    //_YData = null;
                }
                // now do symmetric decryption 2nd, if required
                var xAlg = SysProps.GetValue(SysPropName.XAlg, 0);
                var xtki = SysProps.GetValue<string>(SysPropName.XTKI, null);
                if ((xAlg > 0) && (xtki != null))
                    _zData = _cryptoManager.DecryptWithTranspKey(xtki, _xData);
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
                    case SerialFormat.Soap:
                        // try Soap serialiser
                        _text = SoapSerializerHelper.SerializeToString(_data);
                        SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Soap);
                        break;
                    case SerialFormat.Json:
                        // try Json serialiser
                        _text = JsonSerializerHelper.SerializeToString(_data);
                        SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Json);
                        break;
                    case SerialFormat.Xml:
                        try
                        {
                            _text = XmlSerializerHelper.SerializeToString(_dataTypeType, _data);
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
                            _text = XmlSerializerHelper.SerializeToString(_dataTypeType, _data);
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
                Type dataTypeType = userDataType ?? _dataTypeType;
                if ((dataTypeType == null) && !String.IsNullOrEmpty(DataTypeName))
                {
                    dataTypeType = Type.GetType(DataTypeName);
                }
                var serialFormat = (SerialFormat)SysProps.GetValue(SysPropName.SAlg, (int)SerialFormat.Undefined);
                if (String.IsNullOrEmpty(_text))
                {
                    // null data object
                    _data = null;
                }
                else if (serialFormat == SerialFormat.Binary)
                {
                    // use Binary deserialiser
                    _data = BinarySerializerHelper.DeserializeFromString(_text);
                }
                else if (serialFormat == SerialFormat.Soap)
                {
                    // use Soap deserialiser
                    _data = SoapSerializerHelper.DeserializeFromString(_text);
                }
                else if (serialFormat == SerialFormat.Json)
                {
                    // use Json deserialiser
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
            }
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public object Data
        {
            get
            {
                // deserialise if required
                Deserialise(_dataTypeType);
                return _data;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="binaryClone"></param>
        /// <returns></returns>
        public object GetData(Type dataType, bool binaryClone)
        {
            // deserialise if required
            Deserialise(dataType);
            if (binaryClone)
                return BinarySerializerHelper.Clone(_data);
            return _data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binaryClone"></param>
        /// <returns></returns>
        public T GetData<T>(bool binaryClone) where T : class
        {
            return (T)GetData(typeof(T), binaryClone);
        }

        /// <summary>
        /// 
        /// </summary>
        public Type DataType => _dataTypeType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        public void SetData(Type dataType, object data)
        {
            CheckNotFrozen();
            _data = data;
            _text = null;
            _dataTypeType = dataType;
            DataTypeName = dataType.FullName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void SetData<T>(T data) where T : class
        {
            SetData(typeof(T), data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void SetData(object data)
        {
            SetData(data.GetType(), data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="dataTypeName"></param>
        public void SetText(string text, string dataTypeName)
        {
            CheckNotFrozen();
            _text = text;
            SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Undefined);
            _data = null;
            _dataTypeType = null;
            DataTypeName = dataTypeName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="dataType"></param>
        public void SetText(string text, Type dataType)
        {
            CheckNotFrozen();
            _text = text;
            SysProps.Set(SysPropName.SAlg, (int)SerialFormat.Undefined);
            _data = null;
            _dataTypeType = dataType;
            DataTypeName = dataType.FullName;
        }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Lifetime
        {
            get
            {
                if (_frozen)
                    return (Expires - DateTimeOffset.Now);
                return _lifetime;
            }
            set { CheckNotFrozen(); _lifetime = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TranspKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.XTKI, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.XTKI, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SenderKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.YSKI, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.YSKI, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string RecverKeyId
        {
            get => SysProps.GetValue<string>(SysPropName.YRKI, null);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.YRKI, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public SerialFormat SerialFormat
        {
            get => (SerialFormat)SysProps.GetValue(SysPropName.SAlg, 0);
            set { CheckNotFrozen(); SysProps.Set(SysPropName.SAlg, (int)value); }
        }
    }

}