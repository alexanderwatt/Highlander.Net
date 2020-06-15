namespace Highlander.Grpc.Discover
{
    public partial class V111DiscoverReply
    {
        public V111DiscoverReply(params string[] supportedContracts)
        {
            supportedContracts_.AddRange(supportedContracts);
        }
    }
}
