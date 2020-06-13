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
using Highlander.Core.Common;

#endregion

namespace Highlander.Core.Server
{
    internal partial class PackageHeader
    {
        public readonly Guid ClientId;
        public readonly Guid RequestId;
        public readonly string ReplyAddress;
        public readonly string ReplyContract;
        public readonly bool MoreFollowing;
        public readonly bool ReplyRequired;
        public readonly bool DebugRequest;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        public PackageHeader(Guid clientId)
        {
            ClientId = clientId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="requestId"></param>
        /// <param name="moreFollowing"></param>
        /// <param name="replyRequired"></param>
        /// <param name="replyAddress"></param>
        /// <param name="replyContract"></param>
        /// <param name="debugRequest"></param>
        public PackageHeader(
            Guid clientId, Guid requestId, bool moreFollowing,
            bool replyRequired, string replyAddress, string replyContract,
            bool debugRequest)
        {
            ClientId = clientId;
            RequestId = requestId;
            MoreFollowing = moreFollowing;
            ReplyRequired = replyRequired;
            ReplyAddress = replyAddress;
            ReplyContract = replyContract;
            DebugRequest = debugRequest;
        }

        // V1.3
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        public PackageHeader(V131SessionHeader header)
        {
            ClientId = header.SessionId;
            RequestId = header.RequestId;
            MoreFollowing = header.MoreFollowing;
            ReplyRequired = header.ReplyRequired;
            ReplyAddress = header.ReplyAddress;
            ReplyContract = header.ReplyContract;
            DebugRequest = header.DebugRequest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public V131SessionHeader ToV131Header()
        {
            return new V131SessionHeader(ClientId, RequestId, MoreFollowing, ReplyRequired, ReplyAddress, ReplyContract, DebugRequest);
        }
    }

    internal class PackageBase
    {
        public readonly PackageHeader Header;
        public readonly Guid ClientId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        public PackageBase(Guid clientId, PackageHeader header)
        {
            ClientId = clientId;
            Header = header;
        }
    }

    internal partial class PackageAnswerMultipleItems : PackageBase
    {
        public readonly List<CommonItem> _Items = new List<CommonItem>();
        public IEnumerable<CommonItem> Items => _Items;
        public readonly bool ExcludeDataBody;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="items"></param>
        /// <param name="excludeDataBody"></param>
        public PackageAnswerMultipleItems(Guid clientId, PackageHeader header, IEnumerable<CommonItem> items, bool excludeDataBody)
            : base(clientId, header)
        {
            _Items.AddRange(items);
            ExcludeDataBody = excludeDataBody;
        }
    }

    internal partial class PackageNotifyMultipleItems : PackageBase
    {
        public readonly Guid SubscriptionId;
        public readonly List<CommonItem> _Items = new List<CommonItem>();
        public IEnumerable<CommonItem> Items => _Items;
        public readonly bool ExcludeDataBody;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="items"></param>
        /// <param name="excludeDataBody"></param>
        public PackageNotifyMultipleItems(Guid clientId, PackageHeader header, Guid subscriptionId, IEnumerable<CommonItem> items, bool excludeDataBody)
            : base(clientId, header)
        {
            SubscriptionId = subscriptionId;
            _Items.AddRange(items);
            ExcludeDataBody = excludeDataBody;
        }
    }

    internal partial class PackageSubscriptionQuery : PackageQueryBase
    {
        public Guid SubscriptionId;
    }

    internal partial class PackageSelectItemsQuery : PackageQueryBase
    {
        public string OrderExpr;
        public int StartRow;
        public int RowCount;
    }

    internal class PackageSelectMultipleItems : PackageBase
    {
        public readonly PackageSelectItemsQuery Query;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="query"></param>
        public PackageSelectMultipleItems(Guid clientId, PackageHeader header, PackageSelectItemsQuery query)
            : base(clientId, header)
        {
            Query = query;
        }
    }

    internal partial class PackageCompletionResult : PackageBase
    {
        public readonly bool Success;
        public readonly int Result;
        public readonly string Message;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="success"></param>
        /// <param name="result"></param>
        /// <param name="message"></param>
        public PackageCompletionResult(Guid clientId, PackageHeader header, bool success, int result, string message)
            : base(clientId, header)
        {
            Success = success;
            Result = result;
            Message = message;
        }
    }

    internal class PackageCreateSubscription : PackageBase
    {
        public readonly PackageSubscriptionQuery Query;
        public readonly DateTimeOffset ExpiryTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="query"></param>
        /// <param name="expiryTime"></param>
        public PackageCreateSubscription(Guid clientId, PackageHeader header, PackageSubscriptionQuery query, DateTimeOffset expiryTime)
            : base(clientId, header)
        {
            Query = query;
            ExpiryTime = expiryTime;
        }
    }

    internal class PackageExtendSubscription : PackageBase
    {
        public readonly Guid SubscriptionId;
        public readonly DateTimeOffset ExpiryTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="expiryTime"></param>
        public PackageExtendSubscription(Guid clientId, PackageHeader header, Guid subscriptionId, DateTimeOffset expiryTime)
            : base(clientId, header)
        {
            SubscriptionId = subscriptionId;
            ExpiryTime = expiryTime;
        }
    }

    internal class PackageCancelSubscription : PackageBase
    {
        public readonly Guid SubscriptionId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="subscriptionId"></param>
        public PackageCancelSubscription(Guid clientId, PackageHeader header, Guid subscriptionId)
            : base(clientId, header)
        {
            SubscriptionId = subscriptionId;
        }
    }

    internal class PackageSubscriptionCheck
    {
        public readonly ClientSubscription ClientSubs;
        public readonly CommonItem Candidate;
        public readonly bool DebugRequest;
        public readonly Guid RequestId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSubs"></param>
        /// <param name="candidate"></param>
        /// <param name="debugRequest"></param>
        /// <param name="requestId"></param>
        public PackageSubscriptionCheck(ClientSubscription clientSubs, CommonItem candidate, bool debugRequest, Guid requestId)
        {
            ClientSubs = clientSubs;
            Candidate = candidate;
            DebugRequest = debugRequest;
            RequestId = requestId;
        }
    }

    internal partial class PackageQueryBase
    {
        public string DataType;
        public ItemKind ItemKind;
        public Guid[] ItemIds;
        public string[] ItemNames;
        public string QueryExpr;
        public string[] AppScopes;
        public long MinimumUSN;
        public bool ExcludeDeleted;
        public DateTimeOffset AsAtTime;
        public bool ExcludeExisting;
        public bool ExcludeDataBody;
        public PackageQueryBase() { }
    }
}
