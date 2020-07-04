using System;

namespace Highlander.Grpc.Contracts
{
    public partial class V341CancelSubscription
    {
        public Guid SubscriptionIdGuid => Guid.Parse(subscriptionId_);

        public V341CancelSubscription(
            Guid subscriptionId)
        {
            subscriptionId_ = subscriptionId.ToString();
        }
    }
}
