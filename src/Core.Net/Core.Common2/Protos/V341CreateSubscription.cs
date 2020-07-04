using System;
using Google.Protobuf.WellKnownTypes;
using Highlander.Core.Common;

namespace Highlander.Grpc.Contracts
{
    public partial class V341CreateSubscription
    {
        public Guid SubscriptionIdGuid => Guid.Parse(subscriptionId_);

        public V341CreateSubscription(
            Guid subscriptionId,
            DateTimeOffset expiryTime,
            ItemKind itemKind,
            string dataTypeName,
            string queryExprStr,
            string[] appScopes,
            long minimumUsn,
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
                minimumUsn,
                excludeExisting,
                waitForExisting,
                excludeDeleted,
                excludeDataBody);
            subscriptionId_ = subscriptionId.ToString();
            ExpiryTime = Timestamp.FromDateTimeOffset(expiryTime);
        }
    }
}
