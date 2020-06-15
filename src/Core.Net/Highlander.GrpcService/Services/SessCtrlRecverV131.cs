using System.Threading.Tasks;
using Grpc.Core;
using Highlander.Grpc.Session;
using Microsoft.Extensions.Logging;
using static Highlander.Grpc.Session.SessCtrlV131;

namespace Highlander.GrpcService.Services
{
    public class SessCtrlReceiverV131 : SessCtrlV131Base
    {
        private readonly ILogger<SessCtrlReceiverV131> _logger;
        public SessCtrlReceiverV131(ILogger<SessCtrlReceiverV131> logger)
        {
            _logger = logger;
        }

        public override Task<V131SessionReply> BeginSessionV131(BeginSessionV131Request request, ServerCallContext context)
        {
            return base.BeginSessionV131(request, context);
        }

        public override Task<CloseSessionV131Reply> CloseSessionV131(CloseSessionV131Request request, ServerCallContext context)
        {
            return base.CloseSessionV131(request, context);
        }
    }
}
