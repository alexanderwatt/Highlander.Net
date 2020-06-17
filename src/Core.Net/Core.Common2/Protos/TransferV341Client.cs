using Grpc.Core;
using System;

namespace Highlander.Grpc.Contracts
{
    public static partial class TransferV341
    {
        public partial class TransferV341Client : IDisposable
        {
            private readonly ChannelBase _channel;

            public TransferV341Client(ChannelBase channel, bool unused = false) : base(channel)
            {
                _channel = channel;
            }

            public void Dispose()
            {
                _channel.ShutdownAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}
