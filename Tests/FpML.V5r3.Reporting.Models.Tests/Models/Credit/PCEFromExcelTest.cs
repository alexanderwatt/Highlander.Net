using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using nabCap.QR.General.Utilities;
using nabCap.QR.General.NamedValues;
using nabCap.QR.General.Web;

using nabCap.QR.AnalyticModels.CreditMetrics;
using CMF = nabCap.QR.Analytics.CreditMetrics;

using nabCap.QR.BoundaryRider.DataTransfer.Configuration;
using nabCap.QR.BoundaryRider.DataTransfer.Helpers.Client;
using nabCap.QR.BoundaryRider.DataTransfer.Helpers;
using nabCap.QR.BoundaryRider.DataTransfer.Interface;
using nabCap.QR.BoundaryRider.DataTransfer.Wrappers;

using NUnit.Framework;

namespace nabCap.QR.AnalyticModels.Tests.Models.Credit
{
    [TestFixture]
    public class PCEFromExcelTest
    {

        [SetUp]
        public void Initialisation()
        {


        }

        private static string LoadXMLFileToString(string filePath)
        {
            StringBuilder result = new StringBuilder();
            string input = null;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                TextReader r = new StreamReader(fs);
                while ((input = r.ReadLine()) != null)
                {
                    result.Append(input);
                }
            }
            return result.ToString();
        }

        [Test]
        public void PCEFromClientCall()
        {
            string filePath = "C:\\workspace\\FX_Project\\MTM_test\\strip_test\\Strip2\\xxx.xml";
            string outPath = "C:\\workspace\\FX_Project\\MTM_test\\strip_test\\Strip2\\blob.xml";
            string result = null;
            ArgumentContainer<string, object> argumentList = new ArgumentContainer<string, object>();
            try
            {
                //deserialize from file
                argumentList = ArgumentSerializer.Deserialize(LoadXMLFileToString(filePath));

                var settleDates = ArrayOrItemToList<DateTime>((Array)argumentList["SettleDate"]);
                var recAmounts = ArrayOrItemToList<decimal>((Array)argumentList["RecAmount"]);
                var recCurrencies = ArrayOrItemToList<string>((Array)argumentList["RecCurrency"]);
                var payAmounts = ArrayOrItemToList<decimal>((Array)argumentList["PayAmount"]);
                var payCurrencies = ArrayOrItemToList<string>((Array)argumentList["PayCurrency"]);

                DateTime evaluationDate = (DateTime)argumentList["EvaluationDate"];
                String pceCalculationCurrency = (string)argumentList["PCECalculationCurrency"];

                ITrade[] trades = ToMultipleFxForwardTrades(settleDates.ToArray(), recAmounts.ToArray(), recCurrencies.ToArray(), payAmounts.ToArray(),
                                                                              payCurrencies.ToArray());
                result = BrTradeBlobReaderHelper.SaveTradeBlob(trades);

                //save result
                 //write output
                using (FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(result);
                    }
                }
            }
            catch (Exception e)
            {
                //do nothing
            }

            Assert.IsTrue(true);
        }

        private static ArgumentContainer<string, object> GetFxForwardPCE(ArgumentContainer<string, object> argumentList)
        {
            ArgumentContainer<string, object> result = new ArgumentContainer<string, object>();
            try
            {
                var settleDate = ArrayOrItemToList<DateTime>((Array)argumentList["SettleDate"]);
                var recAmount = ArrayOrItemToList<decimal>((Array)argumentList["RecAmount"]);
                var recCurrency = ArrayOrItemToList<string>((Array)argumentList["RecCurrency"]);
                var payAmount = ArrayOrItemToList<decimal>((Array)argumentList["PayAmount"]);
                var payCurrency = ArrayOrItemToList<string>((Array)argumentList["PayCurrency"]);

                DateTime evaluationDate = (DateTime)argumentList["EvaluationDate"];
                String pceCalculationCurrency = (string)argumentList["PCECalculationCurrency"];

                Result brResult = GetFxForwardPCE(settleDate.ToArray(), recAmount.ToArray(), recCurrency.ToArray(),
                                                    payAmount.ToArray(), payCurrency.ToArray(), evaluationDate,
                                                    pceCalculationCurrency);

                List<int> resultTimeBuckets = new List<int>();
                List<double> resultPCE = new List<double>();

                foreach (ExposureBucket eb in brResult.Buckets)
                {
                    resultTimeBuckets.Add(eb.Term);
                    resultPCE.Add(eb.PE);
                }
                result.Add("TimeBuckets", resultTimeBuckets.ToArray());
                result.Add("PCE", resultPCE.ToArray());
            }
            catch (Exception e)
            {
                result.Add("exception.Message", e.Message);
                result.Add("exception.StackTrace", e.StackTrace);
            }
            return result;
        }


        private static Result GetFxForwardPCE(DateTime[] settleDates,
                                              decimal[] recAmounts,
                                              string[] recCurrencies,
                                              decimal[] payAmounts,
                                              string[] payCurrencies,
                                              DateTime evaluationDate,
                                              string pceCalculationCurrency)
        {
            var trades = ToMultipleFxForwardTrades(settleDates, recAmounts, recCurrencies, payAmounts,
                                                                          payCurrencies);


            IParameters param = new Parameters(ParametersSetupType.Credit) { CalculationDate = evaluationDate };

            //subtract 1 from date due to settlement lag
            //
            param.CalculationDate = param.CalculationDate.AddDays(-1);

            var default_time_buckets = new[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };

            param.BaseCurrency = pceCalculationCurrency;
            param.SimulationMethod = "RiderNet";
            param.TimeBuckets = default_time_buckets;

            var config = ConfigurationData.GetDefaults();
            config.System.BoundaryRiderUrl = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
            config.System.EndpointConfigurationName = "BoundaryRider.RiskEngineService.AnalyticsSoap";
            config.System.Database = "Rider";

            var helper = new BaseClientCallSetupHelper();

            Result result = helper.CalculateCreditExposure(trades, param, config);

            return result;
        }


        private static ITrade[] ToMultipleFxForwardTrades(DateTime[] settleDates,
                                                decimal[] recAmounts,
                                                string[] recCurrencies,
                                                decimal[] payAmounts,
                                                string[] payCurrencies)
        {
            var trades = new List<ITrade>();

            for (int i = 0; i < settleDates.Length; ++i)
            {
                trades.Add(ToFxForwardTrade(settleDates[i],
                                            recAmounts[i],
                                            recCurrencies[i],
                                            payAmounts[i],
                                            payCurrencies[i],
                                            i));
            }

            return trades.ToArray();
        }

     
        private static ITrade ToFxForwardTrade(DateTime settleDate,
                                                decimal recAmount,
                                                string recCurrency,
                                                decimal payAmount,
                                                string payCurrency, int count)
        {
            var result = new Trade
            {
                Product = "FX Forward",
                SourceSystem = "QR",
                SourceId = count.ToString()
            };

            result.AddTradeField("SettleDate", settleDate.ToShortDateString());

            result.AddTradeField("RecAmount", recAmount.ToString());
            result.AddTradeField("RecCurrency", recCurrency);

            result.AddTradeField("PayAmount", payAmount.ToString());
            result.AddTradeField("PayCurrency", payCurrency);

            return result;
        }


        private static List<T> ArgumentContainerToList<T>(ArgumentContainer<string, object> argumentList, string itemName)
        {
            if (!argumentList.ContainsKey(itemName))
            {
                return new List<T>();
            }

            return ArrayOrItemToList<T>(argumentList[itemName]);
        }


        private static List<T> ArrayOrItemToList<T>(object arrayOrItem)
        {
            var result = new List<T>();

            if (null != arrayOrItem)
            {
                if (arrayOrItem is string && String.IsNullOrEmpty(arrayOrItem as string))
                {
                }
                else
                {
                    if (arrayOrItem is Array)
                    {
                        foreach (Array item in arrayOrItem as Array)
                        {
                            var converted = Convert.ChangeType(item.GetValue(0), typeof(T));

                            result.Add((T)converted);
                        }

                    }
                    else//single item
                    {
                        var converted = Convert.ChangeType(arrayOrItem, typeof(T));

                        result.Add((T)converted);
                    }
                }
            }
            return result;
        }
    }
}
