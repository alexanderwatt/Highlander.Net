using System;

namespace Highlander.Grpc.Session
{
    public partial class V131SessionHeader
    {
        public Guid SessionIdGuid => Guid.Parse(sessionId_);

        public Guid RequestIdGuid => Guid.Parse(requestId_);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="requestId"></param>
        /// <param name="moreFollowing"></param>
        /// <param name="replyRequired"></param>
        /// <param name="replyAddress"></param>
        /// <param name="replyContract"></param>
        /// <param name="debugRequest"></param>
        public V131SessionHeader(Guid clientId, Guid requestId, bool moreFollowing, bool replyRequired, string replyAddress, string replyContract, bool debugRequest)
        {
            sessionId_ = clientId.ToString();
            requestId_ = requestId.ToString();
            MoreFollowing = moreFollowing;
            ReplyRequired = replyRequired;
            ReplyAddress = replyAddress;
            ReplyContract = replyContract;
            DebugRequest = debugRequest;
        }
    }
}
