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
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;

namespace Highlander.Core.Common
{
    /// <summary>
    /// The core object interface.
    /// </summary>
    public interface ICoreObject
    {
        string NetworkKey { get; }
        string PrivateKey { get; }
        NamedValueSet AppProperties { get; }
        bool IsTransient { get; }
        TimeSpan Lifetime { get; }
    }

    /// <summary>
    /// The simplest (synchronous) interface of the CoreClient class.
    /// </summary>
    public interface ICoreCache : IDisposable
    {
        ILogger Logger { get; }
        IModuleInfo ClientInfo { get; }
        void Clear();
        // single load methods
        ICoreItem LoadItem<T>(string name);
        ICoreItem LoadItem(Type dataType, string name);
        ICoreItemInfo LoadItemInfo<T>(string name);
        ICoreItemInfo LoadItemInfo(Type dataType, string name);
        T LoadObject<T>(string name);
        // multiple load methods
        List<ICoreItem> LoadItems(Type dataType, IExpression whereExpr);
        List<ICoreItem> LoadItems<T>(IExpression whereExpr);
        List<ICoreItem> LoadItems<T>(IEnumerable<string> itemNames);
        List<ICoreItemInfo> LoadItemInfos(Type dataType, IExpression whereExpr);
        List<ICoreItemInfo> LoadItemInfos<T>(IExpression whereExpr);
        List<T> LoadObjects<T>(IExpression whereExpr);
        // make item methods
        ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires);
        ICoreItem MakeItem<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime);
        // save methods
        Guid SaveItem(ICoreItem item);
        Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props);
        Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, DateTimeOffset expires);
        Guid SaveTypedObject(Type dataType, object data, string name, NamedValueSet props, bool transient, TimeSpan lifetime);
        Guid SaveObject<T>(T data, string name, NamedValueSet props);
        Guid SaveObject<T>(T data, string name, NamedValueSet props, DateTimeOffset expires);
        Guid SaveObject<T>(T data, string name, NamedValueSet props, TimeSpan lifetime);
        Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, DateTimeOffset expires);
        Guid SaveObject<T>(T data, string name, NamedValueSet props, bool transient, TimeSpan lifetime);
        Guid SaveObject<T>(T data) where T : ICoreObject;
        Guid SaveObject<T>(T data, TimeSpan lifetime) where T : ICoreObject;

        // private (in-process only) objects
        Guid SavePrivateObject<T>(T data, string name, NamedValueSet props);

        ICoreItem LoadPrivateItem<T>(string name);

        List<ICoreItem> LoadPrivateItems<T>(IExpression whereExpr);

        // delete methods
        Guid DeleteItem(ICoreItem item);

        int DeleteTypedObjects(Type dataType, IExpression whereExpr);

        int DeleteObjects<T>(IExpression whereExpr);
        // subscription methods
        /// <summary>
        /// Cancels one subscription.
        /// </summary>
        void Unsubscribe(Guid subscriptionId);

        /// <summary>
        /// Cancels all subscriptions.
        /// </summary>
        void UnsubscribeAll();
        /// <summary>
        /// Creates and starts a subscription.
        /// </summary>
        /// <param name="dataType">The type of the data wanted</param>
        /// <param name="filter">The filter expression</param>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="userContext">An optional user context.</param>
        /// <returns></returns>
        ISubscription Subscribe(Type dataType, IExpression filter, SubscriptionCallback userCallback, object userContext);

        /// <summary>
        /// Creates and starts a subscription.
        /// </summary>
        /// <typeparam name="T">The type of the data wanted</typeparam>
        /// <param name="filter">The filter expression</param>
        /// <param name="userCallback">The callback for received items</param>
        /// <param name="userContext">An optional user context.</param>
        /// <returns></returns>
        ISubscription Subscribe<T>(IExpression filter, SubscriptionCallback userCallback, object userContext);
        ISubscription Subscribe<T>(IExpression filter);
        ISubscription SubscribeNoWait<T>(IExpression filter, SubscriptionCallback userCallback, object userContext);
        ISubscription SubscribeNewOnly<T>(IExpression filter, SubscriptionCallback userCallback, object userContext);
        ISubscription SubscribeInfoOnly<T>(IExpression filter);

        // cache properties
        List<ISubscription> Subscriptions { get; }
        List<ICoreItem> Items { get; }
        int ItemCount { get; }
        int CreateCount { get; }
        int UpdateCount { get; }
        int DeleteCount { get; }

        // access to client
        ICoreClient Proxy { get; }
    }
}
