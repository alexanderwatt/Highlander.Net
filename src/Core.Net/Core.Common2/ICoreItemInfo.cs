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
    /// <summary>
    /// Item "header" properties.
    /// </summary>
    public interface ICoreItemInfo
    {
        /// <summary>
        /// Gets the item id object.
        /// </summary>
        /// <value>The item id.</value>
        Guid Id { get; }
        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        /// <value>The type of the item.</value>
        ItemKind ItemKind { get; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="ICoreItemInfo"/> is transient.
        /// </summary>
        /// <value><c>true</c> if transient; otherwise, <c>false</c>.</value>
        bool Transient { get; }
        /// <summary>
        /// Gets/sets the name of the item.
        /// </summary>
        /// <value>The name of the item.</value>
        string Name { get; }
        /// <summary>
        /// Gets the fully qualified unique name of the item.
        /// </summary>
        /// <value>The unique name of the item.</value>
        string UniqueName { get; }
        /// <summary>
        /// Gets/sets the item props.
        /// </summary>
        /// <value>The item property set.</value>
        NamedValueSet AppProps { get; }
        /// <summary>
        /// Gets the type of the data object.
        /// </summary>
        /// <value>The data object.</value>
        Type DataType { get; }
        /// <summary>
        /// Gets the item create (published) time (in local time).
        /// </summary>
        /// <value>The create time.</value>
        DateTimeOffset Created { get; }
        /// <summary>
        /// Gets the item expiry time (in local time).
        /// </summary>
        /// <value>The create time.</value>
        DateTimeOffset Expires { get; set; }
        /// <summary>
        /// Gets/sets the item data appScope.
        /// </summary>
        /// <value>The data appScope.</value>
        string AppScope { get; }
        /// <summary>
        /// Gets/sets the item data appScope.
        /// </summary>
        /// <value>The data appScope.</value>
        string NetScope { get; }
        /// <summary>
        /// Gets the store USN.
        /// </summary>
        /// <value>The store USN.</value>
        Int64 StoreUSN { get; }
    }

}
