using System;

namespace Highlander.Grpc.Contracts
{
    public partial class V341ExtendSubscription
    {
        public Guid SubscriptionIdGuid
        {
            get { return Guid.Parse(subscriptionId_); }
        }
    }
}
