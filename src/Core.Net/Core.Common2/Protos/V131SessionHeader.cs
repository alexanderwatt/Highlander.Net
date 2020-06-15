using System;

namespace Highlander.Grpc.Session
{
    public partial class V131SessionHeader
    {
        public Guid SessionIdGuid
        {
            get { return Guid.Parse(sessionId_); }
        }

        public Guid RequestIdGuid
        {
            get { return Guid.Parse(requestId_); }
        }
    }
}
