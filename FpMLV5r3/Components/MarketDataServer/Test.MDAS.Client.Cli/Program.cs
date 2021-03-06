﻿/*
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
using System.Globalization;
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.MDS.Client.V5r3;
using Highlander.Metadata.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Exception = System.Exception;

#endregion

namespace Highlander.WebMdc.Test.V5r3
{
    class Program
    {
        private static NamedValueSet BuildRequestParamsNotUsed(MDSProviderId dataProviderId)
        {
            var result = new NamedValueSet();
            switch (dataProviderId)
            {
                case MDSProviderId.Bloomberg:
                    //using (PrivateKey key = new PrivateKey("MarketData", "Bloomberg", true))
                    //{
                    //    result.Set(MdpConfigName.Bloomberg_CustName, key.GetValue(MdpConfigName.Bloomberg_CustName, "unknown"));
                    //    result.Set(MdpConfigName.Bloomberg_UUID, Int32.Parse(key.GetValue(MdpConfigName.Bloomberg_UUID, null)));
                    //    result.Set(MdpConfigName.Bloomberg_SID, Int32.Parse(key.GetValue(MdpConfigName.Bloomberg_SID, null)));
                    //    result.Set(MdpConfigName.Bloomberg_SidN, Int32.Parse(key.GetValue(MdpConfigName.Bloomberg_SidN, null)));
                    //    result.Set(MdpConfigName.Bloomberg_TSID, Int32.Parse(key.GetValue(MdpConfigName.Bloomberg_TSID, null)));
                    //    result.Set(MdpConfigName.Bloomberg_TSidN, Int32.Parse(key.GetValue(MdpConfigName.Bloomberg_TSidN, null)));
                    //}
                    break;
                //case MDSProviderId.ReutersIDN:
                //case MDSProviderId.ReutersDTS:
                //    result.Set(MdpConfigName.Reuters_IdleTimeoutSecs, 10);
                //    break;
                //case MDSProviderId.nabCapital:
                //    break;
                //case MDSProviderId.Simulator:
                //    break;
            }
            return result;
        }

        private static string ProviderName(IEnumerable<InformationSource> sources)
        {
            if (sources != null)
            {
                foreach (InformationSource source in sources)
                {
                    if (source?.rateSource?.Value != null)
                    {
                        return source.rateSource.Value;
                    }
                }
            }
            return "???";
        }

        private static void LogResults(ILogger logger, QuotedAssetSet results)
        {
            foreach (BasicAssetValuation instrument in results.assetQuote)
            {
                logger.LogDebug("{0}:", instrument.objectReference.href);
                foreach (BasicQuotation field in instrument.quote)
                {
                    logger.LogDebug("  '{0}' ({1}/{2}) = {3} [Source:{4},Recd:{5}]",
                        field.GetStandardFieldName(),         // field name
                        field.measureType.Value,    // usually "MarketQuote" unless error
                        field.quoteUnits.Value,     // usually "Rate" unless error
                        field.valueSpecified ? field.value.ToString(CultureInfo.InvariantCulture) : "#N/A",
                        ProviderName(field.informationSource),
                        //field.valuationDateSpecified ? field.valuationDate.ToShortTimeString() : "???",
                        field.timeSpecified ? field.time.ToShortTimeString() : "???");
                }
            }
        }

        static void Main(string[] args)
        {
            Reference<ILogger> loggerRef = Reference<ILogger>.Create(new ConsoleLogger("TestWebMdc: "));
            loggerRef.Target.LogInfo("Running...");
            try
            {
                // get some market quotes from for a Highlander FX curve
                // and get a Highlander volatility matrix
                const string curveName = "Highlander.V5r3.Configuration.PricingStructures.QR_LIVE.FxCurve.AUD-USD";
                QuotedAssetSet quotedAssetSet;
                using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv(BuildConst.BuildEnv).Create())
                {
                    ICoreItem marketItem = client.LoadItem<Market>(curveName);
                    if (marketItem == null)
                        throw new ApplicationException("Market '" + curveName + "' not found!");
                    var market = (Market)marketItem.Data;
                    //PricingStructure ps = market.Items[0];
                    PricingStructureValuation psv = market.Items1[0];
                    if (psv is YieldCurveValuation valuation)
                        quotedAssetSet = valuation.inputs;
                    else
                    {
                        if (psv is FxCurveValuation curveValuation)
                            quotedAssetSet = new QuotedAssetSet
                                {
                                    instrumentSet = curveValuation.spotRate.instrumentSet,
                                    assetQuote = curveValuation.spotRate.assetQuote
                                };
                        else
                            throw new NotSupportedException("Unsupported PricingStructureValuation type: " + psv.GetType().Name);
                    }
                }
                //Copied from the working version
                const int port = 9123;
                // create MDS client
                using (IMarketDataClient mdc = MarketDataFactory.Create(loggerRef, null, "localhost:" + port.ToString(CultureInfo.InvariantCulture)))//This was null in the 3rd parameter.
                {
                    {
                       
                        const MDSProviderId providerId = MDSProviderId.Bloomberg;
                        loggerRef.Target.LogInfo("----- {0} Market Quotes -----", providerId);
                        QuotedAssetSet quotes = mdc.GetMarketQuotes(
                            providerId, null, Guid.NewGuid(), true, null,
                            quotedAssetSet).Result;
                        LogResults(loggerRef.Target, quotes);
                    }
                    {
                        const MDSProviderId providerId = MDSProviderId.GlobalIB;
                        loggerRef.Target.LogInfo("----- {0} Volatility Matrix -----", providerId);
                        var matrixProps = new NamedValueSet();
                        matrixProps.Set("Function", "MarketData");
                        matrixProps.Set("Market", "EOD");
                        matrixProps.Set("CurveName", "AUD-Swap");
                        matrixProps.Set("PricingStructureType", "RateATMVolatilityMatrix");
                        QuotedAssetSet matrix = mdc.GetPricingStructure(
                            providerId, null, Guid.NewGuid(), true, null,
                            matrixProps).Result;
                        LogResults(loggerRef.Target, matrix);
                    }
                }
            }
            catch (Exception e)
            {
                loggerRef.Target.Log(e);
            }
            loggerRef.Target.LogInfo("Completed.");
            loggerRef.Target.LogInfo("Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
