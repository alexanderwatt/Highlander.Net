using Highlander.Grpc.Discover;

namespace Highlander.Core.Common.CommsInterfaces
{
    public interface IDiscoverV111
    {
        V111DiscoverReply DiscoverServiceV111();
    }
}
