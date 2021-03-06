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
using System.Threading;
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.MDS.Client.V5r3;
using Highlander.MDS.Server.V5r3;
using Highlander.Metadata.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Exception = System.Exception;

#endregion

namespace Highlander.MDS.Server.Cli.V5r3
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new ConsoleLogger("TestMds: ")))
            {
                loggerRef.Target.LogInfo("{0} Started.", DateTime.Now);
                try
                {
                    const MDSProviderId provider = MDSProviderId.Bloomberg;
                    var settings = new NamedValueSet();
                    const int port = 9123;
                    settings.Set(MdsPropName.Port, port);
                    settings.Set(MdsPropName.EnabledProviders, new[] { MDSProviderId.GlobalIB.ToString(), provider.ToString() });
                    using (Reference<ICoreClient> clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(loggerRef).SetEnv(BuildConst.BuildEnv).Create()))
                    using (var mds = new MarketDataServer())
                    {
                        mds.LoggerRef = loggerRef;
                        mds.Client = clientRef;
                        mds.OtherSettings = settings;
                        mds.Start();
                        loggerRef.Target.LogDebug("Waiting...");
                        Thread.Sleep(15000);
                        loggerRef.Target.LogDebug("Continuing...");
                        List<ICoreItem> marketItems;
                        {
                            marketItems = clientRef.Target.LoadItems<Market>(Expr.StartsWith(Expr.SysPropItemName, "Highlander.V5r3.Configuration."));
                        }
                        if (marketItems.Count == 0)
                            throw new ApplicationException("No curve definitions found!");

                        using (IMarketDataClient mdc = MarketDataFactory.Create(loggerRef, null, "localhost:" + port.ToString(CultureInfo.InvariantCulture)))
                        {
                            foreach (ICoreItem marketItem in marketItems)
                            {
                                loggerRef.Target.LogDebug("Curve: {0}", marketItem.Name);
                                var market = (Market)marketItem.Data;
                                //PricingStructure ps = market.Items[0];
                                PricingStructureValuation psv = market.Items1[0];
                                QuotedAssetSet curveDefinition;
                                if (psv is YieldCurveValuation valuation)
                                    curveDefinition = valuation.inputs;
                                else
                                {
                                    if (psv is FxCurveValuation curveValuation)
                                        curveDefinition = new QuotedAssetSet
                                            {
                                                instrumentSet = curveValuation.spotRate.instrumentSet,
                                                assetQuote = curveValuation.spotRate.assetQuote
                                            };
                                    else
                                        throw new NotSupportedException("Unsupported PricingStructureValuation type: " + psv.GetType().Name);
                                }
                                // call MDS
                                MDSResult<QuotedAssetSet> mdsResponse = mdc.GetMarketQuotes(
                                    provider, null, Guid.NewGuid(), false,
                                    null, // caspar-specific parameters
                                    curveDefinition);
                                if (mdsResponse.Error != null)
                                {
                                    throw mdsResponse.Error;
                                }
                                foreach (BasicAssetValuation result in mdsResponse.Result.assetQuote)
                                {                                    string instrId = result.objectReference.href;
                                    foreach (BasicQuotation quote in result.quote)
                                    {
                                        string fieldId = quote.GetStandardFieldName();
                                        loggerRef.Target.LogDebug("{0}/{1} ({2}/{3}) = [{4}]",
                                            instrId, fieldId, quote.measureType.Value, quote.quoteUnits.Value, quote.value);
                                    }
                                }
                            }
                        } // using MDC
                        mds.Stop();
                    } // using MDS
                }
                catch (Exception e)
                {
                    loggerRef.Target.Log(e);
                }
                loggerRef.Target.LogInfo("{0} Completed.", DateTime.Now);
                loggerRef.Target.LogInfo("Press ENTER to exit.");
                Console.ReadLine();
            }
        }
    }
}
