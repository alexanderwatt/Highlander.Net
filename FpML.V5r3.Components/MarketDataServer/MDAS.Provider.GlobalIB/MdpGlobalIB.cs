/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting;
using Metadata.Common;
using Orion.MDAS.Client;
using Orion.Provider;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.NamedValues;

#endregion

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
