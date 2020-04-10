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
using System.Runtime.Serialization;
using System.ServiceModel;
using Highlander.Metadata.Common;
using Highlander.Utilities;
using Highlander.Utilities.NamedValues;

namespace Highlander.Core.Common
{
    // V3.4 enums
    // Warning: Values are transmitted/persisted externally.
    //          Do not change enumeration order. Ever.
    public enum V341ItemKind
    {
        Undefined,
        Event,
        Object,
        Debug,
        System,
        Local
    }

    public class V341Helpers
    {
        public static V341ItemKind ToV341ItemKind(ItemKind itemKind)
        {
            switch (itemKind)
            {
                case ItemKind.Undefined: return V341ItemKind.Undefined;
                case ItemKind.Object: return V341ItemKind.Object;
                case ItemKind.System: return V341ItemKind.System;
                case ItemKind.Signal: return V341ItemKind.Event;
                case ItemKind.Debug: return V341ItemKind.Debug;
                case ItemKind.Local: return V341ItemKind.Local;
                default: throw new ArgumentException($"Unknown ItemKind: {itemKind}");
            }
        }
        public static ItemKind ToItemKind(V341ItemKind itemKind)
        {
            switch (itemKind)
            {
                case V341ItemKind.Undefined: return ItemKind.Undefined;
                case V341ItemKind.Object: return ItemKind.Object;
                case V341ItemKind.System: return ItemKind.System;
                case V341ItemKind.Event: return ItemKind.Signal;
                case V341ItemKind.Debug: return ItemKind.Debug;
                case V341ItemKind.Local: return ItemKind.Local;
                default: throw new ArgumentException($"Unknown V341ItemKind: {itemKind}");
            }
        }
    }

    [DataContract]
    public class V341Source
    {
        [DataMember]
        public readonly Guid SourceNodeId;
        // constructors
        public V341Source() { }

        public V341Source(Guid source)
        {
            SourceNodeId = source;
        }
    }

    [DataContract]
    public class V341TransportItem
    {
        [DataMember]
        public readonly Guid ItemId;
        [DataMember]
        public readonly V341ItemKind ItemKind;
        [DataMember]
        public readonly bool Transient;
        [DataMember]
        public readonly string ItemName;
        [DataMember]
        public readonly string AppProps;
        [DataMember]
        public readonly string DataType;
        [DataMember]
        public readonly string AppScope;
        [DataMember]
        public readonly DateTimeOffset Created;
        [DataMember]
        public readonly DateTimeOffset Expires;
        [DataMember]
        public readonly string SysProps;
        [DataMember]
        public readonly string NetScope;
        [DataMember]
        public readonly byte[] YData;
        [DataMember]
        public readonly byte[] YSign;
        [DataMember]
        public readonly long SourceUSN;
        // calculated properties
        public int EstimatedSizeInBytes
        {
            get
            {
                // the calculated overhead is actually 1745 bytes, but we are allowing for
                // errors and variations in both the header and body fragments, of up to 250 bytes.
                const int bytesPerStringChar = 2; // 16-bit Unicode strings
                int result = 2000; // overhead
                result += ItemName?.Length * bytesPerStringChar ?? 0;
                result += DataType?.Length * bytesPerStringChar ?? 0;
                result += AppScope?.Length * bytesPerStringChar ?? 0;
                result += NetScope?.Length * bytesPerStringChar ?? 0;
                result += AppProps?.Length * bytesPerStringChar ?? 0;
                result += SysProps?.Length * bytesPerStringChar ?? 0;
                result += YData?.Length ?? 0;
                result += YSign?.Length ?? 0;
                const int bufferBytesPerByte = 2;
                return result * bufferBytesPerByte;
            }
        }

        // constructors
        public V341TransportItem() { }

        public V341TransportItem(CommonItem item, bool excludeDataBody)
        {
            ItemId = item.Id;
            ItemKind = V341Helpers.ToV341ItemKind(item.ItemKind);
            Transient = item.Transient;
            ItemName = item.Name;
            AppProps = item.AppProps.Serialise();
            SysProps = item.SysProps.Serialise();
            DataType = item.DataTypeName;
            AppScope = item.AppScope;
            NetScope = item.NetScope;
            Created = item.Created;
            Expires = item.Expires;
            SourceUSN = item.StoreUSN;
            if (!excludeDataBody)
            {
                YData = item.YData;
                YSign = item.YSign;
            }
        }

        public CommonItem AsCommonItem()
        {
            var result = new CommonItem(
                ItemId, V341Helpers.ToItemKind(ItemKind), Transient, ItemName,
                new NamedValueSet(AppProps), DataType, AppScope,
                new NamedValueSet(SysProps), NetScope, Created, Expires,
                YData, YSign, SourceUSN);
            return result;
        }
    }

    [DataContract]
    public class V341AnswerMultipleItems
    {
        [DataMember]
        public readonly V341TransportItem[] Items;
        // constructors
        public V341AnswerMultipleItems() { }
        public V341AnswerMultipleItems(V341TransportItem item)
        {
            Items = new[] { item };
        }
        public V341AnswerMultipleItems(V341TransportItem[] items)
        {
            Items = items;
        }
    }

    [DataContract]
    public class V341NotifyMultipleItems
    {
        [DataMember]
        public readonly Guid SubscriptionId;
        [DataMember]
        public readonly V341TransportItem[] Items;
        // constructors
        public V341NotifyMultipleItems() { }
        public V341NotifyMultipleItems(Guid subscriptionId, V341TransportItem item)
        {
            SubscriptionId = subscriptionId;
            Items = new[] { item };
        }
        public V341NotifyMultipleItems(Guid subscriptionId, V341TransportItem[] items)
        {
            SubscriptionId = subscriptionId;
            Items = items;
        }
    }

    [DataContract]
    public class V341QueryDefinition
    {
        [DataMember]
        public readonly V341ItemKind ItemKind;
        [DataMember]
        public readonly string DataType;
        [DataMember]
        public readonly string[] AppScopes;
        [DataMember]
        public readonly string[] ItemNames;
        [DataMember]
        public readonly string QueryExpr;
        [DataMember]
        public readonly long MinimumUSN;
        [DataMember]
        public readonly bool ExcludeDeleted;
        [DataMember]
        public readonly DateTimeOffset AsAtTime;
        [DataMember]
        public readonly bool ExcludeExisting;
        [DataMember]
        public readonly bool WaitForExisting;
        [DataMember]
        public readonly bool ExcludeDataBody;
        // constructors
        public V341QueryDefinition() { }
        public V341QueryDefinition(
            ItemKind itemKind,
            string dataType,
            string[] appScopes,
            string[] itemNames,
            string queryExpr,
            DateTimeOffset asAtTime,
            long minimumUSN,
            bool excludeExisting,
            bool waitForExisting,
            bool excludeDeleted,
            bool excludeDataBody)
        {
            ItemKind = V341Helpers.ToV341ItemKind(itemKind);
            DataType = dataType;
            AppScopes = appScopes;
            ItemNames = itemNames;
            QueryExpr = queryExpr;
            AsAtTime = asAtTime;
            MinimumUSN = minimumUSN;
            ExcludeExisting = excludeExisting;
            WaitForExisting = waitForExisting;
            ExcludeDeleted = excludeDeleted;
            ExcludeDataBody = excludeDataBody;
        }
    }

    [DataContract]
    public class V341SelectMultipleItems
    {
        [DataMember]
        public readonly V341QueryDefinition QueryDef;
        [DataMember]
        public readonly Guid[] ItemIds;
        [DataMember]
        public readonly string OrderExpr;
        [DataMember]
        public readonly int StartRow;
        [DataMember]
        public readonly int RowCount;
        // constructors
        public V341SelectMultipleItems() { }
        // selecting by guid(s)
        public V341SelectMultipleItems(
            Guid itemId,
            bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                ItemKind.Undefined, null, null, null, null,
                DateTimeOffset.MinValue, 0, false, true, false, excludeDataBody);
            ItemIds = new[] { itemId };
        }
        public V341SelectMultipleItems(
            Guid[] itemIds,
            bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                ItemKind.Undefined, null, null, null, null,
                DateTimeOffset.MinValue, 0, false, true, false, excludeDataBody);
            ItemIds = itemIds;
        }
        // selecting by names(s)
        public V341SelectMultipleItems(
            string dataTypeName,
            ItemKind itemKind,
            string itemName,
            string[] appScopes,
            long minimumUSN,
            bool includeDeleted,
            DateTimeOffset asAtTime,
            bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                itemKind,
                dataTypeName,
                appScopes,
                new[] { itemName },
                null,
                asAtTime,
                minimumUSN,
                false, true,
                !includeDeleted,
                excludeDataBody);
        }
        // selecting by names(s)
        public V341SelectMultipleItems(
            string dataTypeName,
            ItemKind itemKind,
            string[] itemNames,
            string[] appScopes,
            long minimumUSN,
            bool includeDeleted,
            DateTimeOffset asAtTime,
            bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                itemKind,
                dataTypeName,
                appScopes,
                itemNames,
                null,
                asAtTime,
                minimumUSN,
                false, true,
                !includeDeleted,
                excludeDataBody);
        }
        // selecting by query
        public V341SelectMultipleItems(
            string dataTypeName,
            ItemKind itemKind,
            string queryExprStr,
            string orderExprStr,
            int startRow,
            int rowCount,
            string[] appScopes,
            long minimumUSN,
            bool includeDeleted,
            DateTimeOffset asAtTime,
            bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                itemKind,
                dataTypeName,
                appScopes,
                null,
                queryExprStr,
                asAtTime,
                minimumUSN,
                false, true,
                !includeDeleted,
                excludeDataBody);
            OrderExpr = orderExprStr;
            StartRow = startRow;
            RowCount = rowCount;
        }
    }

    [DataContract]
    public class V341CreateSubscription
    {
        [DataMember]
        public readonly V341QueryDefinition QueryDef;
        [DataMember]
        public readonly Guid SubscriptionId;
        [DataMember]
        public readonly DateTimeOffset ExpiryTime;
        // constructors
        public V341CreateSubscription() { }
        public V341CreateSubscription(
            Guid subscriptionId,
            DateTimeOffset expiryTime,
            ItemKind itemKind,
            string dataTypeName,
            string queryExprStr,
            string[] appScopes,
            long minimumUSN,
            bool excludeDeleted,
            bool excludeExisting,
            bool waitForExisting,
            DateTimeOffset asAtTime,
            bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                itemKind,
                dataTypeName,
                appScopes,
                null,
                queryExprStr,
                asAtTime,
                minimumUSN,
                excludeExisting,
                waitForExisting,
                excludeDeleted,
                excludeDataBody);
            SubscriptionId = subscriptionId;
            ExpiryTime = expiryTime;
        }
    }

    [DataContract]
    public class V341ExtendSubscription
    {
        [DataMember]
        public readonly Guid SubscriptionId;
        [DataMember]
        public readonly DateTimeOffset ExpiryTime;
        // constructors
        public V341ExtendSubscription() { }
        public V341ExtendSubscription(
            Guid subscriptionId,
            DateTimeOffset expiryTime)
        {
            SubscriptionId = subscriptionId;
            ExpiryTime = expiryTime;
        }
    }

    [DataContract]
    public class V341CancelSubscription
    {
        [DataMember]
        public readonly Guid SubscriptionId;
        // constructors
        public V341CancelSubscription() { }
        public V341CancelSubscription(
            Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }
    }

    [DataContract]
    public class V341CompletionResult
    {
        [DataMember]
        public readonly bool Success;
        [DataMember]
        public readonly int Result;
        [DataMember]
        public readonly string Message;
        // constructors
        public V341CompletionResult() { }
        public V341CompletionResult(bool success, int result, string message)
        {
            Success = success;
            Result = result;
            Message = message;
        }
    }

    [DataContract]
    public class V341BeginSession
    {
        [DataMember]
        public readonly Guid Token;
        // constructors
        public V341BeginSession() { }
        public V341BeginSession(Guid token) { Token = token; }
    }

    [DataContract]
    public class V341CloseSession
    {
        [DataMember]
        public readonly int Reserved;
        // constructors
        public V341CloseSession() { }
    }

    [ServiceContract]
    public interface ITransferV341
    {
        // asynchronous operations (1-way over any transport)
        [OperationContract(IsOneWay = true)]
        void TransferV341SelectMultipleItems(V131SessionHeader header, V341SelectMultipleItems body);
        [OperationContract(IsOneWay = true)]
        void TransferV341CompletionResult(V131SessionHeader header, V341CompletionResult body);
        [OperationContract(IsOneWay = true)]
        void TransferV341CreateSubscription(V131SessionHeader header, V341CreateSubscription body);
        [OperationContract(IsOneWay = true)]
        void TransferV341ExtendSubscription(V131SessionHeader header, V341ExtendSubscription body);
        [OperationContract(IsOneWay = true)]
        void TransferV341CancelSubscription(V131SessionHeader header, V341CancelSubscription body);
        [OperationContract(IsOneWay = true)]
        void TransferV341AnswerMultipleItems(V131SessionHeader header, V341AnswerMultipleItems body);
        [OperationContract(IsOneWay = true)]
        void TransferV341NotifyMultipleItems(V131SessionHeader header, V341NotifyMultipleItems body);
    }

    public class TransferRecverV341 : ITransferV341
    {
        private readonly ITransferV341 _channel;
        public TransferRecverV341(ITransferV341 channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public void TransferV341SelectMultipleItems(V131SessionHeader header, V341SelectMultipleItems body)
        {
            _channel.TransferV341SelectMultipleItems(header, body);
        }

        public void TransferV341CompletionResult(V131SessionHeader header, V341CompletionResult body)
        {
            _channel.TransferV341CompletionResult(header, body);
        }

        public void TransferV341CreateSubscription(V131SessionHeader header, V341CreateSubscription body)
        {
            _channel.TransferV341CreateSubscription(header, body);
        }

        public void TransferV341ExtendSubscription(V131SessionHeader header, V341ExtendSubscription body)
        {
            _channel.TransferV341ExtendSubscription(header, body);
        }

        public void TransferV341CancelSubscription(V131SessionHeader header, V341CancelSubscription body)
        {
            _channel.TransferV341CancelSubscription(header, body);
        }

        public void TransferV341AnswerMultipleItems(V131SessionHeader header, V341AnswerMultipleItems body)
        {
            _channel.TransferV341AnswerMultipleItems(header, body);
        }

        public void TransferV341NotifyMultipleItems(V131SessionHeader header, V341NotifyMultipleItems body)
        {
            _channel.TransferV341NotifyMultipleItems(header, body);
        }
    }

    public class TransferSenderV341 : CustomClientBase<ITransferV341>, ITransferV341
    {
        public TransferSenderV341(AddressBinding addressBinding)
            : base(addressBinding)
        { }

        public void TransferV341SelectMultipleItems(V131SessionHeader header, V341SelectMultipleItems body)
        {
            Channel.TransferV341SelectMultipleItems(header, body);
        }

        public void TransferV341CompletionResult(V131SessionHeader header, V341CompletionResult body)
        {
            Channel.TransferV341CompletionResult(header, body);
        }

        public void TransferV341CreateSubscription(V131SessionHeader header, V341CreateSubscription body)
        {
            Channel.TransferV341CreateSubscription(header, body);
        }

        public void TransferV341ExtendSubscription(V131SessionHeader header, V341ExtendSubscription body)
        {
            Channel.TransferV341ExtendSubscription(header, body);
        }

        public void TransferV341CancelSubscription(V131SessionHeader header, V341CancelSubscription body)
        {
            Channel.TransferV341CancelSubscription(header, body);
        }

        public void TransferV341AnswerMultipleItems(V131SessionHeader header, V341AnswerMultipleItems body)
        {
            Channel.TransferV341AnswerMultipleItems(header, body);
        }

        public void TransferV341NotifyMultipleItems(V131SessionHeader header, V341NotifyMultipleItems body)
        {
            Channel.TransferV341NotifyMultipleItems(header, body);
        }
    }
}
