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
using Highlander.Utilities.NamedValues;

namespace Highlander.Core.Common
{
    public interface ICoreItem : ICoreItemInfo
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="ICoreItem"/> is committed.
        /// </summary>
        /// <value><c>true</c> if committed; otherwise, <c>false</c>.</value>
        bool Frozen { get; }
        /// <summary>
        /// Gets/sets the item data appScope.
        /// </summary>
        /// <value>The data appScope.</value>
        void SetNetScope(string netScope);
        /// <summary>
        /// Gets the serialised object.
        /// </summary>
        /// <value>The serialised object.</value>
        string Text { get; }
        /// <summary>
        /// Gets the deserialised data object.
        /// </summary>
        /// <value>The data object.</value>
        object Data { get; }
        /// <summary>
        /// Gets (and deserialises) the data object using the custom type.
        /// </summary>
        /// <value>The data object.</value>
        object GetData(Type dataType, bool binaryClone);
        /// <summary>
        /// Gets (and deserialises) the data object using the custom type.
        /// </summary>
        /// <value>The data object.</value>
        T GetData<T>(bool binaryClone) where T : class;
        /// <summary>
        /// Sets the data object.
        /// </summary>
        /// <value>The data object.</value>
        void SetData(Type dataType, object data);
        /// <summary>
        /// Sets the data object.
        /// </summary>
        /// <value>The data object.</value>
        void SetData<T>(T data) where T : class;
        /// <summary>
        /// Sets the data object.
        /// </summary>
        /// <value>The data object.</value>
        void SetData(object data);
        /// <summary>
        /// Gets the type name of the data object.
        /// </summary>
        /// <value>The data object.</value>
        string DataTypeName { get; }
        /// <summary>
        /// Saves the user-serialised data and its type.
        /// </summary>
        void SetText(string serialisedData, Type dataType);
        /// <summary>
        /// Saves the user-serialised data and its type.
        /// </summary>
        void SetText(string serialisedData, string dataTypeName);
        /// <summary>
        /// Gets the raw (compressed and/or encrypted) data.
        /// </summary>
        /// <value>The raw data.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        byte[] YData { get; }
        /// <summary>
        /// Gets the 128-bit MD5 hash code for the (compressed and/or encrypted) data.
        /// </summary>
        /// <value>The hash code.</value>
        Guid YDataHash { get; }
        /// <summary>
        /// Gets the publishers signature (if any).
        /// </summary>
        /// <value>The publishers signature.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        byte[] YSign { get; }
        /// <summary>
        /// Gets/sets the serialiser format.
        /// </summary>
        /// <value>The serialiser algorithm</value>
        SerialFormat SerialFormat { get; set; }
        /// <summary>
        /// Gets/sets the item lifetime.
        /// </summary>
        /// <value>The lifetime.</value>
        TimeSpan Lifetime { get; set; }
        bool IsCurrent(DateTimeOffset asAtTime);
        bool IsCurrent();
        /// <summary>
        /// Gets the item's system properties.
        /// </summary>
        /// <value>The item property set.</value>
        NamedValueSet SysProps { get; }
        bool IsSigned { get; }
        bool IsSecret { get; }
        string TranspKeyId { get; set; }
        string SenderKeyId { get; set; }
        string RecverKeyId { get; set; }
        /// <summary>
        /// Commits this instance.
        /// </summary>
        void Freeze();
    }

}
