using System.Threading.Tasks;
using Grpc.Core;
using Highlander.Core.Common.CommsInterfaces;
using Highlander.Grpc.Discover;
using Microsoft.Extensions.Logging;
using static Highlander.Grpc.Discover.DiscoverV111;

namespace Highlander.Core.Common.Services
{
    public class DiscoverReceiverV111 : DiscoverV111Base
    {
        private readonly ILogger<DiscoverReceiverV111> _logger;
        private readonly IDiscoverV111 discoverV111Interface;

        public DiscoverReceiverV111(ILogger<DiscoverReceiverV111> logger, IDiscoverV111 discoverV111Interface)
        {
            _logger = logger;
            this.discoverV111Interface = discoverV111Interface;
        }

        public override Task<V111DiscoverReply> DiscoverServiceV111(DiscoverServiceV111Request request, ServerCallContext context)
        {
            discoverV111Interface.DiscoverServiceV111();
            return Task.FromResult(new V111DiscoverReply());
        }
    }
}
