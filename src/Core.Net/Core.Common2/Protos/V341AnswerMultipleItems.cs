using System;
using System.Collections;
using System.Collections.Generic;

namespace Highlander.Grpc.Contracts
{
    public partial class V341AnswerMultipleItems
    {
        //public Guid SubscriptionIdGuid
        //{
        //    get { return Guid.Parse(subscriptionId_); }
        //}

        public V341AnswerMultipleItems(IEnumerable<V341TransportItem> items)
        {
            items_ = new Google.Protobuf.Collections.RepeatedField<V341TransportItem>();
            items_.AddRange(items);
        }
    }
}
