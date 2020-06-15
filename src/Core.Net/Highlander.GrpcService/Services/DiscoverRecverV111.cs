using System.Threading.Tasks;
using Grpc.Core;
using Highlander.Grpc.Discover;
using Microsoft.Extensions.Logging;
using static Highlander.Grpc.Discover.DiscoverV111;

namespace Highlander.GrpcService.Services
{
    public class DiscoverReceiverV111 : DiscoverV111Base
    {
        private readonly ILogger<DiscoverReceiverV111> _logger;
        public DiscoverReceiverV111(ILogger<DiscoverReceiverV111> logger)
        {
            _logger = logger;
        }

        public override Task<V111DiscoverReply> DiscoverServiceV111(DiscoverServiceV111Request request, ServerCallContext context)
        {
            return base.DiscoverServiceV111(request, context);
        }
    }
}
