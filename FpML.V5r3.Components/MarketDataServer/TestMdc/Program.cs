using System;
//
using System.Threading;
using Orion.MDAS.Server;
using Orion.Util.Logging;

//using nabCap.QR.Runtime.V25;

namespace TestMdc
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = Logger.CreateConsoleLogger("TestMdc: ");

            logger.LogInfo("Running...");
            try
            {
                QRDataProviderId dataProviderId = QRDataProviderId.Bloomberg; // QRDataProviderId.ReutersIDN;

                // create MDS client
                MarketDataSet data = null;
                IMarketDataClient mdc = new MarketDataRealtimeClient(
                    logger, new Sequencer(logger),
                    dataProviderId);

                data = mdc.GetCurrentData(
                    QRDataProviderId.Undefined,
                    Guid.NewGuid(), 
                    null, 
                    QRAssetIdType.Undefined,
                    new string[6] { 
                        "AUD-FxSpot-SP", 
                        "NZD-FxSpot-SP",
                        "GBP-FxSpot-SP",
                        "JPY-FxSpot-SP",
                        "CHF-FxSpot-SP",
                        "XYZ-FxSpot-SP"
                    },
                    new string[2] { "ASK", "BID" });

                Thread.Sleep(1000);

                logger.LogInfo("Results...");
                for (int i = 0; i < data.rows.Length; i++)
                {
                    MarketDataRow row = data.rows[i];
                    logger.LogInfo("[{0}] {1}", i, row.instrumentId);
                    for (int j = 0; j < row.field.Length; j++)
                    {
                        MarketDataFld field = row.field[j];
                        logger.LogInfo("[{0}]     {1} ({2})='[{3}]'", i, field.name, field.type, String.Join(",",field.value));
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
