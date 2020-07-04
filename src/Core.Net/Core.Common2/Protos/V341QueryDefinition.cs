using System;
using Google.Protobuf.WellKnownTypes;
using Highlander.Core.Common;

namespace Highlander.Grpc.Contracts
{
    public partial class V341QueryDefinition
    {
        public V341QueryDefinition(
            ItemKind itemKind,
            string dataType,
            string[] appScopes,
            string[] itemNames,
            string queryExpr,
            DateTimeOffset asAtTime,
            long minimumUsn,
            bool excludeExisting,
            bool waitForExisting,
            bool excludeDeleted,
            bool excludeDataBody)
        {
            ItemKind = itemKind.ToV341ItemKind();
            DataType = dataType;
            var scope = new Google.Protobuf.Collections.RepeatedField<string>();
            scope.AddRange(appScopes);
            appScopes_ = scope;
            var names = new Google.Protobuf.Collections.RepeatedField<string>();
            names.AddRange(itemNames);
            itemNames_ = names;
            QueryExpr = queryExpr;
            AsAtTime = Timestamp.FromDateTimeOffset(asAtTime);
            MinimumUSN = minimumUsn;
            ExcludeExisting = excludeExisting;
            WaitForExisting = waitForExisting;
            ExcludeDeleted = excludeDeleted;
            ExcludeDataBody = excludeDataBody;
        }
    }
}
