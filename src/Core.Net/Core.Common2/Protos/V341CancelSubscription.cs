using System;

namespace Highlander.Grpc.Contracts
{
    public partial class V341CancelSubscription
    {
        public Guid SubscriptionIdGuid
        {
            get { return Guid.Parse(subscriptionId_); }
        }
    }
}
