using System;

namespace Highlander.Grpc.Session
{
    public partial class V131SessionReply
    {
        public Guid SessionIdGuid
        {
            get { return Guid.Parse(sessionId_); }
        }

        public V131SessionReply(string message)
        {
            message_ = message;
        }
    }
}
