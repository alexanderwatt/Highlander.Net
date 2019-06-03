using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting;
using Metadata.Common;
using Orion.Provider;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.V5r10.Reporting.Common;

namespace Orion.MDAS.Provider
{
    public class MdpFactoryGlobalIB
    {
        public static IQRMarketDataProvider Create(ILogger logger, ICoreClient client, ConsumerDelegate consumer)
        {
            return new MdpGlobalIB(logger, client, consumer);
        }
    }

    internal class MdpGlobalIB : MdpBaseProvider
    {
        public MdpGlobalIB(ILogger logger, ICoreClient client, ConsumerDelegate consumer)
            : base(logger, client, MDSProviderId.GlobalIB, consumer) { }

        protected override MDSResult<QuotedAssetSet> OnRequestPricingStructure(
            IModuleInfo clientInfo, 
            Guid requestId, 
            MDSRequestType requestType, 
            NamedValueSet requestParams, 
            DateTimeOffset subsExpires, 
            NamedValueSet structureParams)
        {
            var combinedResult = new QuotedAssetSet();
            var debugRequest = requestParams.GetValue<bool>("DebugRequest", false);
            Client.DebugRequests = debugRequest;
            IExpression query = Expr.BoolAND(structureParams);
            List<QuotedAssetSet> providerResults = Client.LoadObjects<QuotedAssetSet>(query);
            Client.DebugRequests = false;
            // merge multiple results
            combinedResult = providerResults.Aggregate(combinedResult, (current, providerResult) => current.Merge(providerResult, false, true, true));
            return new MDSResult<QuotedAssetSet>
                       {
                Result = combinedResult
            };
        }
    }
}
