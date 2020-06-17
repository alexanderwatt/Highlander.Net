using System.Threading.Tasks;
using Grpc.Core;
using Highlander.Core.Common.CommsInterfaces;
using Highlander.Grpc.Session;
using Microsoft.Extensions.Logging;
using static Highlander.Grpc.Session.SessCtrlV131;

namespace Highlander.Core.Common.Services
{
    public class SessCtrlReceiverV131 : SessCtrlV131Base
    {
        private readonly ILogger<SessCtrlReceiverV131> _logger;
        private readonly ISessCtrlV131 sessCtrlV131Interface;

        public SessCtrlReceiverV131(ILogger<SessCtrlReceiverV131> logger, ISessCtrlV131 sessCtrlV131Interface)
        {
            _logger = logger;
            this.sessCtrlV131Interface = sessCtrlV131Interface;
        }

        public override Task<V131SessionReply> BeginSessionV131(BeginSessionV131Request request, ServerCallContext context)
        {
            return Task.FromResult(sessCtrlV131Interface.BeginSessionV131(request.Header, request.ClientInfo));
        }

        public override Task<CloseSessionV131Reply> CloseSessionV131(CloseSessionV131Request request, ServerCallContext context)
        {
            sessCtrlV131Interface.CloseSessionV131(request.Header);
            return Task.FromResult(new CloseSessionV131Reply());
        }
    }
}
