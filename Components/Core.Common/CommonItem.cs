/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)
 Copyright (C) 2019 Simon Dudley

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
using System.Security.Cryptography;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.Core.Common
{
    public static class AppScopeNames
    {
        public const string Legacy = "Highlander.Global.Legacy";
    }

    public static class AppPropName
    {
        public const string ApplName = "ApplName";
        public const string HostName = "HostName";
        public const string UserName = "UserName";
    }

    public static class SysPropName
    {
        // do not ever alter these constant values
        public const string ApplName = "AN";
        public const string ApplFVer = "AV";
        public const string ApplPTok = "AK";
        public const string HostName = "HN";
        public const string UserName = "UN";
        public const string UserWDom = "UD";
        public const string UserIdentity = "UI"; // domain\username
        public const string UserFullName = "UF";
        public const string OrgEnvId = "OE";
        public const string CoreFVer = "CV";
        public const string CorePTok = "CK";
        public const string NodeGuid = "NI";
        public const string SAlg = "SAlg";
        public const string TLen = "TLen";
        public const string XAlg = "XAlg";
        public const string XLen = "XLen";
        public const string XTKI = "XTKI";
        public const string YAlg = "YAlg";
        public const string YLen = "YLen";
        public const string YRKI = "YRKI";
        public const string YSKI = "YSKI";
        public const string ZAlg = "ZAlg";
        public const string ZLen = "ZLen";
    }

    public enum ItemKind
    {
        Undefined,
        Signal,
        Object,
        Debug,
        System,
        Local
    }

    public static class Converters
    {
        public static ItemKind ToItemKind(int itemType)
        {
            return (ItemKind)itemType;
        }

        public static int ToItemType(ItemKind kind)
        {
            return (int)kind;
        }
    }

    /// <summary>
    /// The serialiser type.
    /// </summary>
    public enum SerialFormat
    {
        /// <summary>
        /// Undefined - Use the default serialisation method.
        /// </summary>
        Undefined,      // 0
        Xml,            // 1
        Soap,           // 2
        Binary,         // 3
        DataContract,   // 4
        Json            // 5
    }

    public class CommonItem
    {
        // immutable properties
        public Guid Id { get; }
        public ItemKind ItemKind { get; }
        public bool Transient { get; }
        public string Name { get; }
        public string AppScope { get; }

        private string _uniqueName;
        public string UniqueName => _uniqueName ?? (_uniqueName = CoreHelper.MakeUniqueName(ItemKind, AppScope, Name));

        // modifiable properties
        public DateTimeOffset Created { get; protected set; }
        public DateTimeOffset Expires { get; protected set; }
        public NamedValueSet AppProps { get; protected set; }
        public NamedValueSet SysProps { get; protected set; }
        public string NetScope { get; protected set; }

        public void SetNetScope(string netScope)
        {
            NetScope = netScope;
        }

        public byte[] YSign { get; protected set; }
        public byte[] YData { get; protected set; }

        public void SetYData(byte[] yData)
        {
            YData = yData;
            YDataHash = CalculateBufferHash(YData);
        }

        public Guid YDataHash { get; protected set; }
        public string DataTypeName { get; set; }
        public long StoreUSN { get; set; }

        public CommonItem(ItemKind itemKind, bool transient, string itemName, string appScope)
        {
            SysProps = new NamedValueSet();
            AppProps = new NamedValueSet();
            ItemKind = itemKind;
            Transient = transient;
            Id = Guid.NewGuid();
            AppScope = appScope ?? AppScopeNames.Legacy;
            Name = itemName;
        }

        public CommonItem(CommonItem item, bool transient, bool expired) // for cloning
        {
            SysProps = new NamedValueSet();
            ItemKind = item.ItemKind;
            Transient = transient;
            Id = Guid.NewGuid();
            AppScope = item.AppScope;
            NetScope = item.NetScope;
            Name = item.Name;
            AppProps = new NamedValueSet(item.AppProps);
            DataTypeName = item.DataTypeName;
            Created = DateTimeOffset.Now;
            StoreUSN = item.StoreUSN;
            if (expired)
            {
                Expires = Created;
                SysProps.Clear();
            }
            else
            {
                Expires = Created.Add(item.Expires - item.Created);
                YData = item.YData;
                YDataHash = CalculateBufferHash(YData);
                YSign = item.YSign;
                SysProps = new NamedValueSet(item.SysProps);
            }
        }

        // for direct loading from stores or bridges
        public CommonItem(
            Guid itemId, ItemKind itemKind, bool transient,
            string itemName, NamedValueSet appProps, string dataTypeName, string appScope,
            NamedValueSet sysProps, string netScope, DateTimeOffset created, DateTimeOffset expires,
            byte[] yData, byte[] ySign, long storeUSN)
        {
            Id = itemId;
            ItemKind = itemKind;
            Transient = transient;
            Name = itemName;
            AppProps = appProps ?? new NamedValueSet();
            SysProps = sysProps ?? new NamedValueSet();
            DataTypeName = dataTypeName;
            AppScope = appScope;
            NetScope = netScope;
            Created = created;
            Expires = expires;
            YData = yData;
            YDataHash = CalculateBufferHash(YData);
            YSign = ySign;
            StoreUSN = storeUSN;
        }

        public bool IsCurrent(DateTimeOffset asAtTime)
        {
            return (asAtTime < Expires);
        }

        public bool IsCurrent()
        {
            return IsCurrent(DateTimeOffset.Now);
        }

        public int EstimatedSizeInBytes(bool excludeDataBody)
        {
            // the calculated overhead is actually 1745 bytes, but we are allowing for
            // errors and variations in both the header and body fragments, of up to 250 bytes.
            const int bytesPerStringChar = 2; // 16-bit Unicode strings
            int result = 2000; // overhead
            result += Name?.Length * bytesPerStringChar ?? 0;
            result += DataTypeName?.Length * bytesPerStringChar ?? 0;
            result += AppScope?.Length * bytesPerStringChar ?? 0;
            result += NetScope?.Length * bytesPerStringChar ?? 0;
            result += (AppProps.Serialise().Length * bytesPerStringChar); // hack - slow - todo - use Text property
            result += (SysProps.Serialise().Length * bytesPerStringChar); // hack - slow - todo - use Text property
            if (!excludeDataBody)
            {
                result += YData?.Length ?? 0;
                result += YSign?.Length ?? 0;
            }
            const int bufferBytesPerByte = 2;
            return result * bufferBytesPerByte;
        }

        protected static Guid CalculateBufferHash(byte[] buffer)
        {
            // An MD5 hash is not a Guid, but there is no other convenient
            // 128-bit value type in .Net 3.x other than Guid that can contain
            // the result. Eg. Int128 would be used if it existed in the CLR.
            if (buffer == null)
                return Guid.Empty;
            HashAlgorithm hash = new MD5CryptoServiceProvider();
            byte[] hashBytes = hash.ComputeHash(buffer);
            return new Guid(hashBytes);
        }
    }
}
