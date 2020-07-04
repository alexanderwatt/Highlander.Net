using System;

namespace Highlander.Grpc.Session
{
    public partial class V131SessionReply
    {
        public Guid SessionIdGuid => Guid.Parse(sessionId_);

        public V131SessionReply(string message)
        {
            message_ = message;
        }
    }
}
