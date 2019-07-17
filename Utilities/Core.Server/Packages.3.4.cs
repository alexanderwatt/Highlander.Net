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

#region usings

using System;
using System.Linq;
using Core.Common;
using Orion.Util.NamedValues;

#endregion

namespace Core.Server
{
    internal partial class PackageQueryBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        public PackageQueryBase(V341SelectMultipleItems request)
        {
            DataType = request.QueryDef.DataType;
            ItemKind = V341Helpers.ToItemKind(request.QueryDef.ItemKind);
            ItemIds = request.ItemIds;
            ItemNames = request.QueryDef.ItemNames;
            QueryExpr = request.QueryDef.QueryExpr;
            AppScopes = request.QueryDef.AppScopes;
            MinimumUSN = request.QueryDef.MinimumUSN;
            ExcludeDeleted = request.QueryDef.ExcludeDeleted;
            AsAtTime = request.QueryDef.AsAtTime;
            ExcludeExisting = request.QueryDef.ExcludeExisting;
            ExcludeDataBody = request.QueryDef.ExcludeDataBody;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        public PackageQueryBase(V341CreateSubscription request)
        {
            DataType = request.QueryDef.DataType;
            ItemKind = V341Helpers.ToItemKind(request.QueryDef.ItemKind);
            ItemNames = request.QueryDef.ItemNames;
            QueryExpr = request.QueryDef.QueryExpr;
            AppScopes = request.QueryDef.AppScopes;
            MinimumUSN = request.QueryDef.MinimumUSN;
            ExcludeDeleted = request.QueryDef.ExcludeDeleted;
            AsAtTime = request.QueryDef.AsAtTime;
            ExcludeExisting = request.QueryDef.ExcludeExisting;
            ExcludeDataBody = request.QueryDef.ExcludeDataBody;
        }
    }

    internal partial class PackageSubscriptionQuery
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public PackageSubscriptionQuery(V341CreateSubscription query)
            : base(query)
        {
            SubscriptionId = query.SubscriptionId;
        }
    }

    internal partial class PackageSelectItemsQuery
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public PackageSelectItemsQuery(V341SelectMultipleItems query)
            : base(query)
        {
            OrderExpr = query.OrderExpr;
            StartRow = query.StartRow;
            RowCount = query.RowCount;
        }
    }

    internal partial class PackageAnswerMultipleItems
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public PackageAnswerMultipleItems(Guid clientId, V131SessionHeader header, V341AnswerMultipleItems body)
            : base(clientId, new PackageHeader(header))
        {
            foreach (V341TransportItem item in body.Items)
            {
                _Items.Add(new CommonItem(
                    item.ItemId, V341Helpers.ToItemKind(item.ItemKind),
                    item.Transient, item.ItemName,
                    new NamedValueSet(item.AppProps), item.DataType, item.AppScope,
                    new NamedValueSet(item.SysProps), item.NetScope,
                    item.Created, item.Expires,
                    item.YData, item.YSign, item.SourceUSN));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public V341AnswerMultipleItems ToV341AnswerMultipleItems()
        {
            return new V341AnswerMultipleItems(_Items.Select(item => new V341TransportItem(item, ExcludeDataBody)).ToArray());
        }
    }

    internal partial class PackageNotifyMultipleItems
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public PackageNotifyMultipleItems(Guid clientId, V131SessionHeader header, V341NotifyMultipleItems body)
            : base(clientId, new PackageHeader(header))
        {
            foreach (V341TransportItem item in body.Items)
            {
                _Items.Add(new CommonItem(
                    item.ItemId, V341Helpers.ToItemKind(item.ItemKind),
                    item.Transient, item.ItemName,
                    new NamedValueSet(item.AppProps), item.DataType, item.AppScope,
                    new NamedValueSet(item.SysProps), item.NetScope,
                    item.Created, item.Expires,
                    item.YData, item.YSign, item.SourceUSN));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public V341NotifyMultipleItems ToV341NotifyMultipleItems()
        {
            return new V341NotifyMultipleItems(SubscriptionId, _Items.Select(item => new V341TransportItem(item, false)).ToArray());
        }
    }

    internal partial class PackageCompletionResult
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public V341CompletionResult ToV341CompletionResult()
        {
            return new V341CompletionResult(Success, Result, Message);
        }
    }
}
