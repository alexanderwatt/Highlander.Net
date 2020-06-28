/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Threading;
using Highlander.MDS.Client.V5r3;
using Highlander.MDS.Server.V5r3;
using Highlander.Metadata.Common;
using Highlander.Utilities.Logging;

namespace Highlander.Mdc.V5r3
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = CreateConsoleLogger("TestMdc: ");
            logger.LogInfo("Running...");
            try
            {
                var dataProviderId = MDSProviderId.Bloomberg;
                // create MDS client
                MDSResult<QuotedAssetSet> data = null;
                IMarketDataClient mdc = new MarketDataRealtimeClient(
                    logger, new Sequencer(logger),
                    dataProviderId);

                data = mdc.GetMarketQuotes(
                    MDSProviderId.Undefined,
                    null,
                    Guid.NewGuid(),
                    true,
                    new string[6] { 
                        "AUD-FxSpot-SP", 
                        "NZD-FxSpot-SP",
                        "GBP-FxSpot-SP",
                        "JPY-FxSpot-SP",
                        "CHF-FxSpot-SP",
                        "XYZ-FxSpot-SP"
                    },
                    new[] { "ASK", "BID" });
                Thread.Sleep(1000);
                logger.LogInfo("Results...");
                for (int i = 0; i < data.rows.Length; i++)
                {
                    var row = data.rows[i];
                    logger.LogInfo("[{0}] {1}", i, row.instrumentId);
                    for (int j = 0; j < row.field.Length; j++)
                    {
                        var field = row.field[j];
                        logger.LogInfo("[{0}]     {1} ({2})='[{3}]'", i, field.name, field.type, string.Join(",",field.value as string[]));
                    }
                }
                logger.LogInfo("Completed.");
            }
            catch (Exception e)
            {
                logger.Log(e);
            }
            logger.LogInfo("Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
