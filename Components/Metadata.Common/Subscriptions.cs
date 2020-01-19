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
using Highlander.Utilities;
using Highlander.Utilities.Expressions;

namespace Highlander.Metadata.Common
{
    public delegate void SubscriptionCallback(ISubscription subscription, ICoreItem item);

    /// <summary>
    /// The publicly visible interface of a subscription.
    /// </summary>
    public interface ISubscription : IDisposable
    {
        /// <summary>
        /// Gets the unique subscription id.
        /// </summary>
        /// <value>The id.</value>
        Guid Id { get; }
        /// <summary>
        /// Gets or sets the subscription query expression.
        /// </summary>
        /// <value>The query expr.</value>
        IExpression WhereExpr { get; set; }
        /// <summary>
        /// Gets or sets the subscription data type. Updates DataTypeName.
        /// </summary>
        /// <value>The type of the data.</value>
        Type DataType { get; set; }
        /// <summary>
        /// Gets or sets the subscription data type name. Setting this will reset DataType.
        /// </summary>
        /// <value>The type of the data.</value>
        string DataTypeName { get; set; }
        /// <summary>
        /// Gets or sets the kind of the item (default is Object).
        /// </summary>
        /// <value>The kind of the item.</value>
        ItemKind ItemKind { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to exclude deleted items (default is false).
        /// </summary>
        /// <value><c>true</c> if [exclude deleted]; otherwise, <c>false</c>.</value>
        bool ExcludeDeleted { get; set; }
        /// <summary>
        /// Gets or sets the minimum USN.
        /// </summary>
        /// <value>The minimum USN.</value>
        long MinimumUSN { get; set; }
        /// <summary>
        /// Gets or sets the app scopes.
        /// </summary>
        /// <value>The app scopes.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        string[] AppScopes { get; set; }
        /// <summary>
        /// Gets or sets the time reference used to determine if items are deleted.
        /// </summary>
        /// <value>As at time.</value>
        DateTimeOffset AsAtTime { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to exclude existing items (default is false).
        /// </summary>
        /// <value><c>true</c> if [exclude existing]; otherwise, <c>false</c>.</value>
        bool ExcludeExisting { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to wait for existing items to be returned on start (default is true).
        /// </summary>
        /// <value><c>true</c> if [wait for existing]; otherwise, <c>false</c>.</value>
        bool WaitForExisting { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to exclude the data body from returned items (default is false).
        /// </summary>
        /// <value><c>true</c> if [include current]; otherwise, <c>false</c>.</value>
        bool ExcludeDataBody { get; set; }
        /// <summary>
        /// Gets or sets the user callback.
        /// </summary>
        /// <value>The user callback.</value>
        SubscriptionCallback UserCallback { get; set; }
        /// <summary>
        /// Gets or sets the user context.
        /// </summary>
        /// <value>The user context.</value>
        object UserContext { get; set; }
        /// <summary>
        /// Starts this subscription.
        /// </summary>
        void Start();
        /// <summary>
        /// Returns whether this subscription is running.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this subscription is running; otherwise, <c>false</c>.
        /// </returns>
        bool Started { get; }
        /// <summary>
        /// Cancels this subscription.
        /// </summary>
        void Cancel();
        IAsyncResult CancelBegin();
        void CancelEnd(IAsyncResult ar);

        // mode change notifications
        event CoreStateHandler OnStateChange;
    }

}
