/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Globalization;
using Core.Common;
using Core.V34;
using FpML.V5r3.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.MDAS.Server;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;
using Orion.Build;
using Orion.MDAS.Client;
using Orion.V5r3.Configuration;

#endregion

namespace TestMarketDataClient
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class IntegrationTests//TODO THese will only work if the quaotedassetsets have been loaded into the cache.
    {
       //public static UnitTestEnvironment UTE { get; set; }

        public IntegrationTests()
        {
            //UTE = new UnitTestEnvironment();
            //Set the calendar engine
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private static ILogger _logger;
        //private static UnitTestEnvironment TestEnvironment;
        //private static IWorkContext _WorkContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _logger = new TraceLogger(true);
            //TestEnvironment = new UnitTestEnvironment(Logger);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            //TestEnvironment.Dispose();
            _logger.Dispose();
        }

        private NamedValueSet BuildRequestParams(MDSProviderId dataProviderId)
        {
            var result = new NamedValueSet();
            switch (dataProviderId)
            {
                case MDSProviderId.Bloomberg:
                //using (var key = new PrivateKey("MarketData", "Bloomberg", true))
                //{
                //    result.Set(MdpConfigName.BloombergCustName, key.GetValue(MdpConfigName.BloombergCustName, null));
                //    result.Set(MdpConfigName.BloombergUUID, Int32.Parse(key.GetValue(MdpConfigName.BloombergUUID, null)));
                //    result.Set(MdpConfigName.BloombergSID, Int32.Parse(key.GetValue(MdpConfigName.BloombergSID, null)));
                //    result.Set(MdpConfigName.BloombergSidN, Int32.Parse(key.GetValue(MdpConfigName.BloombergSidN, null)));
                //    result.Set(MdpConfigName.BloombergTsid, Int32.Parse(key.GetValue(MdpConfigName.BloombergTsid, null)));
                //    result.Set(MdpConfigName.BloombergTSidN, Int32.Parse(key.GetValue(MdpConfigName.BloombergTSidN, null)));
                //}
                break;
                case MDSProviderId.ReutersIDN:
                case MDSProviderId.ReutersDTS:
                    result.Set(MdpConfigName.ReutersIdleTimeoutSecs, 10);
                    break;
                case MDSProviderId.GlobalIB:
                    break;
                case MDSProviderId.Simulator:
                    break;
            }
            return result;
        }

        //[TestMethod]
        [Ignore]
        public void TestMDSBloomberg()
        {
            // test basic start, request snapshot, and stop functions
            // - create a MDS client with direct connection to provider
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                IModuleInfo clientInfo = new V131ModuleInfo(new V131ClientInfo());
                using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv(BuildConst.BuildEnv).Create())
                {
                    const string curveName = "Orion.V5r3.Configuration.PricingStructures.QR_LIVE.RateCurve.AUD-BBR-BBSW-3M";
                    ICoreItem marketItem = client.LoadItem<Market>(curveName);
                    if (marketItem == null)
                        throw new ApplicationException("Market '" + curveName + "' not found!");
                    var market = (Market)marketItem.Data;
                    //PricingStructure ps = market.Items[0];
                    PricingStructureValuation psv = market.Items1[0];
                    QuotedAssetSet quotedAssetSet = ((YieldCurveValuation)psv).inputs;
                    using (IMarketDataClient mdc = new MarketDataRealtimeClient(
                        loggerRef, null, client,
                        MDSProviderId.Bloomberg))//MDSProviderId.GlobalIB
                    {
                        QuotedAssetSet data = mdc.GetMarketQuotes(
                            MDSProviderId.Bloomberg, clientInfo, Guid.NewGuid(), true,
                            null,
                            quotedAssetSet).Result;
                        Assert.IsNotNull(data);
                    }
                }
            }
        }

        private void LogResults(ILogger logger, QuotedAssetSet results)
        {
            foreach (BasicAssetValuation instrument in results.assetQuote)
            {
                logger.LogDebug("{0}:", instrument.objectReference.href);
                foreach (BasicQuotation field in instrument.quote)
                {
                    logger.LogDebug("  '{0}' ({1}/{2}) = {3} (as at {4}) (received {5})",
                        (field.timing != null) ? field.timing.Value : "Last", // side (null=last)
                        field.measureType.Value,    // usually "MarketQuote" unless error
                        field.quoteUnits.Value,     // usually "Rate" unless error
                        field.valueSpecified ? field.value.ToString(CultureInfo.InvariantCulture) : "#N/A",
                        field.valuationDateSpecified ? field.valuationDate.ToShortTimeString() : "???",
                        field.timeSpecified ? field.time.ToShortTimeString() : "???");
                }
            }
        }

        internal delegate void IterationTarget(string currency, string index, string market, string tenor);
        private void IterateCurveProperties(IterationTarget target)
        {
            foreach (string ccy in "AUD;USD;EUR;NZD;GBP".Split(';'))
            {
                foreach (string index in "BBR-BBSW;LIBOR-BBA".Split(';'))
                {
                    foreach (string marketName in "QR_LIVE".Split(';'))
                    {
                        foreach (string tenor in "3M".Split(';'))
                        {
                            target(ccy, index, marketName, tenor);
                        }
                    }
                }
            }
        }

        private void RemoveMarketQuoteValues(QuotedAssetSet marketData)
        {
            // strip out market quote values
            foreach (var item in marketData.assetQuote)
            {
                foreach (var quote in item.quote)
                {
                    if (quote.measureType.Value != "MarketQuote") continue;
                    quote.valueSpecified = false;
                    quote.value = 0.0m;
                }
            }
        }

        [TestMethod]
        public void ConvertQuotedAssetSetsToRateCurveDefinition()
        {
            const string curveType = "RateCurve";
            using (var logger = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (ICoreClient client = (new CoreClientFactory(logger)).SetEnv(BuildConst.BuildEnv).Create())
                {
                    IterateCurveProperties((ccy, index, marketName, tenor) =>
                    {
                        string indexName = $"{ccy}-{index}";
                        string curveName = $"{indexName}-{tenor}";
                        string qasItemName = String.Format("Orion.V5r3.QuotedAssetSet.{0}.{4}.{1}-{2}-{3}",
                            marketName, ccy, index, tenor, curveType);
                        // outputs
                        string curveItemName = String.Format("Orion.V5r3.Configuration.PricingStructures.{0}.{4}.{1}-{2}-{3}",
                            marketName, ccy, index, tenor, curveType);
                        var marketData = client.LoadObject<QuotedAssetSet>(qasItemName);
                        if (marketData != null)
                        {
                            RemoveMarketQuoteValues(marketData);
                            var curveDef = new Market
                                {
                                id = curveName,
                                Items = new PricingStructure[] { new YieldCurve { id = curveName } },
                                Items1 = new PricingStructureValuation[] {
                                    new YieldCurveValuation { id = curveName, inputs = marketData } }
                            };
                            var curveProps = new NamedValueSet();
                            curveProps.Set(CurveProp.Market, marketName);
                            curveProps.Set(CurveProp.PricingStructureType, curveType);
                            curveProps.Set(CurveProp.IndexName, indexName);
                            curveProps.Set(CurveProp.IndexTenor, tenor);
                            curveProps.Set(CurveProp.CurveName, curveName);
                            curveProps.Set("Algorithm", "FastLinearZero");
                            curveProps.Set(CurveProp.Currency1, ccy);
                            curveProps.Set(CurveProp.DataGroup, "Orion.V5r3.Configuration.PricingStructureType");
                            curveProps.Set("SourceSystem", "Orion");
                            curveProps.Set("Function", "Configuration");
                            curveProps.Set("Type", CurveProp.PricingStructureType);
                            curveProps.Set(CurveProp.UniqueIdentifier, curveItemName);
                            client.SaveObject(curveDef, curveItemName, curveProps, TimeSpan.MaxValue);
                        }
                    });
                }
            }
        }

        [TestMethod]
        public void ConvertQuotedAssetSetsToFxCurveDefinition()
        {
            const string curveType = "FxCurve";
            using (var logger = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (ICoreClient client = (new CoreClientFactory(logger)).SetEnv(BuildConst.BuildEnv).Create())
                {
                    IterateCurveProperties((ccy, index, marketName, tenor) =>
                    {
                        string indexName = $"{ccy}-{index}";
                        string curveName = $"{indexName}-{tenor}";
                        string qasItemName = String.Format("Orion.V5r3.MarketData.{0}.{4}.{1}-{2}-{3}",
                            marketName, ccy, index, tenor, curveType);

                        // outputs
                        string curveItemName = String.Format("Orion.V5r3.Configuration.PricingStructures.{0}.{4}.{1}-{2}-{3}",
                            marketName, ccy, index, tenor, curveType);

                        var marketData = client.LoadObject<QuotedAssetSet>(qasItemName);
                        if (marketData != null)
                        {
                            RemoveMarketQuoteValues(marketData);

                            var curveDef = new Market
                                               {
                                id = curveName,
                                Items = new PricingStructure[] { new FxCurve { id = curveName } },
                                Items1 = new PricingStructureValuation[] {
                                    new FxCurveValuation
                                        {
                                        id = curveName,
                                        spotRate = new FxRateSet
                                                       {
                                             instrumentSet = marketData.instrumentSet,
                                             assetQuote = marketData.assetQuote
                                        }
                                    }
                            }
                            };
                            var curveProps = new NamedValueSet();
                            curveProps.Set(CurveProp.Market, marketName);
                            curveProps.Set(CurveProp.PricingStructureType, curveType);
                            curveProps.Set(CurveProp.IndexName, indexName);
                            curveProps.Set(CurveProp.IndexTenor, tenor);
                            curveProps.Set(CurveProp.CurveName, curveName);
                            curveProps.Set("Algorithm", "FastLinearZero");
                            curveProps.Set(CurveProp.Currency1, ccy);
                            curveProps.Set(CurveProp.DataGroup, "Orion.V5r3.Configuration.PricingStructureType");
                            curveProps.Set("SourceSystem", "Orion");
                            curveProps.Set("Function", "Configuration");//TODO add the namespace
                            curveProps.Set("Type", CurveProp.PricingStructureType);
                            curveProps.Set(CurveProp.UniqueIdentifier, curveItemName);
                            client.SaveObject(curveDef, curveItemName, curveProps, TimeSpan.MaxValue);
                        }
                    });
                }
            }
        }

        [TestMethod]
        public void ConvertQuotedAssetSetsToDiscountCurveDefinition()
        {
            using (var logger = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (ICoreClient client = (new CoreClientFactory(logger)).SetEnv(BuildConst.BuildEnv).Create())
                {
                    const string instrument = "LIBOR";
                    const string seniority = "SENIOR";
                    foreach (string ccy in "AUD;USD;EUR;NZD;GBP".Split(';'))
                    {
                        foreach (string marketName in CurveConst.QR_LIVE.Split(';'))
                        {
                            //foreach (string tenor in "3M".Split(';'))
                            {
                                //string indexName = String.Format("{0}-{1}", ccy, index);
                                string curveName = $"{ccy}-{instrument}-{seniority}";
                                string qasItemName = $"Orion.V5r3.MarketData.{marketName}.DiscountCurve.{curveName}";
                                // outputs
                                string curveItemName =
                                    $"Orion.V5r3.Configuration.PricingStructures.{marketName}.DiscountCurve.{curveName}";
                                var marketData = client.LoadObject<QuotedAssetSet>(qasItemName);
                                if (marketData != null)
                                {
                                    // strip out market quote values
                                    foreach (var tradeItem in marketData.assetQuote)
                                    {
                                        foreach (var quote in tradeItem.quote)
                                        {
                                            if (quote.measureType.Value == "MarketQuote")
                                            {
                                                quote.valueSpecified = false;
                                                quote.value = 0.0m;
                                            }
                                        }
                                    }
                                    var curveDef = new Market
                                                       {
                                        id = curveName,
                                        Items = new PricingStructure[] { new YieldCurve { id = curveName } },
                                        Items1 = new PricingStructureValuation[] {
                                    new YieldCurveValuation { id = curveName, inputs = marketData } }
                                    };

                                    var curveProps = new NamedValueSet();
                                    curveProps.Set(CurveProp.Market, marketName);
                                    curveProps.Set(CurveProp.PricingStructureType, "DiscountCurve");
                                    //curveProps.Set(CurveProp.IndexName, indexName);
                                    //curveProps.Set(CurveProp.IndexTenor, tenor);
                                    curveProps.Set("CreditInstrumentId", instrument);
                                    curveProps.Set("CreditSeniority", seniority);
                                    curveProps.Set(CurveProp.CurveName, curveName);
                                    curveProps.Set("Algorithm", "FastLinearZero");
                                    curveProps.Set(CurveProp.Currency1, ccy);
                                    curveProps.Set(CurveProp.DataGroup, "Orion.V5r3.Configuration.PricingStructureType");
                                    curveProps.Set("SourceSystem", "Orion");
                                    curveProps.Set("Function", "Configuration");
                                    curveProps.Set("Type", CurveProp.PricingStructureType);
                                    curveProps.Set(CurveProp.UniqueIdentifier, curveItemName);
                                    client.SaveObject(curveDef, curveItemName, curveProps, TimeSpan.MaxValue);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
