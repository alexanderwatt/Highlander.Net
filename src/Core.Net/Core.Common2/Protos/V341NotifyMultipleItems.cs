using System;
using System.Collections;
using System.Collections.Generic;

namespace Highlander.Grpc.Contracts
{
    public partial class V341NotifyMultipleItems
    {
        public Guid SubscriptionIdGuid => Guid.Parse(subscriptionId_);

        public V341NotifyMultipleItems(Guid subscriptionId, V341TransportItem item)
        {
            subscriptionId_ = subscriptionId.ToString();
            items_ = new Google.Protobuf.Collections.RepeatedField<V341TransportItem> {item};
        }

        public V341NotifyMultipleItems(Guid subscriptionId, IEnumerable<V341TransportItem> items)
        {
            subscriptionId_ = subscriptionId.ToString();
            items_ = new Google.Protobuf.Collections.RepeatedField<V341TransportItem>();
            items_.AddRange(items);
        }
    }
}
