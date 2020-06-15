using Highlander.Grpc.Session;

namespace Highlander.Core.Common.CommsInterfaces
{
    public interface ISessCtrlV131
    {
        V131SessionReply BeginSessionV131(V131SessionHeader header, V131ClientInfo clientInfo);
        void CloseSessionV131(V131SessionHeader header);
    }
}
