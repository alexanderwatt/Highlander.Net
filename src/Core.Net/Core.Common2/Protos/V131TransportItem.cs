using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Highlander.Core.Common;
using System;

namespace Highlander.Grpc.Contracts
{
    public partial class V341TransportItem
    {
        public Guid ItemIdGuid => Guid.Parse(itemId_);

        public V341TransportItem(CommonItem item, bool excludeDataBody)
        {
            ItemId = item.Id.ToString();
            ItemKind = item.ItemKind.ToV341ItemKind();
            Transient = item.Transient;
            ItemName = item.Name;
            AppProps = item.AppProps.Serialise();
            SysProps = item.SysProps.Serialise();
            DataType = item.DataTypeName;
            AppScope = item.AppScope;
            NetScope = item.NetScope;
            Created = Timestamp.FromDateTimeOffset(item.Created);
            Expires = Timestamp.FromDateTimeOffset(item.Expires);
            SourceUSN = item.StoreUSN;
            if (excludeDataBody) return;
            YData = ByteString.CopyFrom(item.YData);
            YSign = ByteString.CopyFrom(item.YSign);
        }
    }
}
