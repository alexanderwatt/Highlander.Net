using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Highlander.Core.Common;
using System;

namespace Highlander.Grpc.Session
{
    public partial class V131ClientInfo
    {
        public Guid NodeGuidAsGuid
        {
            get { return Guid.Parse(nodeGuid_); }
        }
    }
}
