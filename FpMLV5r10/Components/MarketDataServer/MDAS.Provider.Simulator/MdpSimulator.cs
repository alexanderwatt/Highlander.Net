/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using Metadata.Common;
using Orion.MDAS.Client;
using Orion.Provider;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Exception = System.Exception;

namespace Orion.MDAS.Provider
{
    public class MdpFactorySimulator
    {
        public static IQRMarketDataProvider Create(ILogger logger, ICoreClient client, ConsumerDelegate consumer)
        {
            return new SimulatorPlugin(logger, client, consumer);
        }
    }

    internal class SimulatorPlugin : MdpBaseProvider
    {
        public SimulatorPlugin(ILogger logger, ICoreClient client, ConsumerDelegate consumer)
            : base(logger, client, MDSProviderId.Simulator, consumer) { }

        //public void CancelRequest(Guid subscriptionId)
        //{
        //}

        private class RequestProperties
        {
        }

        private RequestProperties ProcessRequestParams(NamedValueSet requestParams)
        {
            var result = new RequestProperties();

            //NamedValueSet requestSettings = new NamedValueSet(requestParams);
            if (requestParams != null)
            {
                ICollection<NamedValue> coll = requestParams.ToColl();
                foreach (NamedValue nv in coll)
                {
                    try
                    {
                        switch (nv.Name.ToLower())
                        {
                            default:
                                Logger.LogDebug(
                                    "{0}: Parameter {1}='{2}' ignored",
                                    "MdpSimulator", nv.Name, nv.ValueString);
                                break;
                        }
                    }
                    catch (Exception excp)
                    {
                        Logger.Log(excp);
                    }
                }
            }
            return result;
        }

        protected override void OnStart()
        {
            // price quote units (default is Rate)
            // source pattern must be in instrument/fieldname format
            //MarketDataMap.AddUnitsMap(1, MDSRequestType.Undefined, "(.*)-IRFuture-(.*)/(.*)", PriceQuoteUnitsEnum.IRFuturesPrice);
            //MarketDataMap.AddUnitsMap(0, MDSRequestType.Undefined, "(.*)/(.*)", PriceQuoteUnitsEnum.Rate);

            // create instrument id and field name maps
            //MarketDataMap.AddInstrMap(0, "(.*)", "$1");
            //MarketDataMap.AddFieldMap(0, MDSRequestType.Undefined, "(.*)", "$1");
        }

        protected override MDSResult<QuotedAssetSet> OnRequestMarketData(
            IModuleInfo clientInfo, 
            Guid requestId, 
            MDSRequestType requestType, 
            NamedValueSet requestParams, 
            DateTimeOffset subsExpires,
            QuotedAssetSet standardQuotedAssetSet)
        {
            // process request parameters
            RequestProperties requestProperties = ProcessRequestParams(requestParams);

            // process the asset/quote lists to produce 1 or more instrument/field matrices
            RequestContext requestContext = ConvertStandardAssetQuotesToProviderInstrFieldCodes(requestType, standardQuotedAssetSet);

            // process the instr/field code sets
            var results = new List<BasicAssetValuation>();
            foreach (ProviderInstrFieldCodeSet instrFieldCodeSet in requestContext.ProviderInstrFieldCodeSets)
            {
                var providerResults = new Dictionary<string, BasicQuotation>();
                // simulated reply
                var rng = new Random();
                //List<BasicAssetValuation> results = new List<BasicAssetValuation>();
                foreach (string t in instrFieldCodeSet.InstrumentIds)
                {
                    var quote = new BasicAssetValuation
                                    {
                                        objectReference =
                                            new AnyAssetReference {href = t},
                                        quote = new BasicQuotation[instrFieldCodeSet.FieldNames.Count]
                                    };
                    foreach (string t1 in instrFieldCodeSet.FieldNames)
                    {
                        var providerQuote = new BasicQuotation
                                                {
                                                    measureType =
                                                        new AssetMeasureType
                                                            {Value = AssetMeasureEnum.MarketQuote.ToString()},
                                                    quoteUnits =
                                                        new PriceQuoteUnits
                                                            {Value = PriceQuoteUnitsEnum.Rate.ToString()},
                                                    value = Convert.ToDecimal(rng.NextDouble()*10.0),
                                                    valueSpecified = true
                                                };
                        // field value
                        // simulator returns a random decimal value
                        // field done
                        string providerQuoteKey = FormatProviderQuoteKey(t, t1);
                        providerResults[providerQuoteKey] = providerQuote;
                    }
                }

                // process provider results
                //Dictionary<string, BasicQuotation> providerResults = BuildProviderResultsIndex(pages);
                results.AddRange(ConvertProviderResultsToStandardValuations(providerResults, requestContext));
            }
            return new MDSResult<QuotedAssetSet>
                       {
                Result = new QuotedAssetSet { assetQuote = results.ToArray() }
            };
        }
    }
}
