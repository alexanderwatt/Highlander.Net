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

namespace Highlander.Core.Common
{
    /// <summary>
    /// Client-side data transfer object for core items
    /// </summary>
    [Serializable]
    public sealed class RawItem
    {
        public Guid ItemId { get; }
        public int ItemKind { get; }
        public bool Transient { get; }
        public string AppScope { get; } // private set
        public string NetScope { get; }
        public string DataType { get; }
        public string ItemName { get; }
        public string AppProps { get; } 
        public string SysProps { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset Expires { get; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] YData { get; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] YSign { get; }
        public Int64 StoreUSN { get; }
        // constructors
        public RawItem(ICoreItem item, string newItemName)
        {
            ItemKind = (int)item.ItemKind;
            Transient = item.Transient;
            ItemId = item.Id;
            AppScope = item.AppScope;
            NetScope = item.NetScope;
            ItemName = newItemName ?? item.Name;
            AppProps = item.AppProps.Serialise();
            SysProps = item.SysProps.Serialise();
            DataType = item.DataTypeName;
            Created = item.Created;
            Expires = item.Expires;
            YData = item.YData;
            YSign = item.YSign;
            StoreUSN = item.StoreUSN;
        }
        public RawItem(ICoreItem item) : this(item, null) { }
    }

}
