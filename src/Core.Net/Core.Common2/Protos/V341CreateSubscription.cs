using System;

namespace Highlander.Grpc.Contracts
{
    public partial class V341CreateSubscription
    {
        public Guid SubscriptionIdGuid
        {
            get { return Guid.Parse(subscriptionId_); }
        }
    }
}
