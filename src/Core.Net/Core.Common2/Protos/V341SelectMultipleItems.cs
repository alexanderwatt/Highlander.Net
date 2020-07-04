using System;
using Highlander.Core.Common;

namespace Highlander.Grpc.Contracts
{
    public partial class V341SelectMultipleItems
    {

        public V341SelectMultipleItems(Guid itemId, bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                ItemKind.Undefined, null, null, null, null,
                DateTimeOffset.MinValue, 0, false, true, false, excludeDataBody);
            var items = new Google.Protobuf.Collections.RepeatedField<string> {itemId.ToString()};
            itemIds_ = items;
        }

        public V341SelectMultipleItems(
            Guid[] itemIds,
            bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                ItemKind.Undefined, null, null, null, null,
                DateTimeOffset.MinValue, 0, false, true, false, excludeDataBody);
            var items = new Google.Protobuf.Collections.RepeatedField<string>();
            foreach (var itemId in itemIds)
            {
                items.Add(itemId.ToString());
            }
            itemIds_ = items;
        }

        // selecting by names(s)
        public V341SelectMultipleItems(
            string dataTypeName,
            ItemKind itemKind,
            string itemName,
            string[] appScopes,
            long minimumUsn,
            bool includeDeleted,
            DateTimeOffset asAtTime,
            bool excludeDataBody)
        {
            QueryDef = new V341QueryDefinition(
                itemKind,
                dataTypeName,
                appScopes,
                new[] {itemName},
                null,
                asAtTime,
                minimumUsn,
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
            long minimumUsn,
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
                minimumUsn,
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
            long minimumUsn,
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
                minimumUsn,
                false, true,
                !includeDeleted,
                excludeDataBody);
            OrderExpr = orderExprStr;
            StartRow = startRow;
            RowCount = rowCount;
        }
    }
}
