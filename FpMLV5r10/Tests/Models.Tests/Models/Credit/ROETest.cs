#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using National.QRSC.AnalyticModels.CreditMetrics;

using CMF = National.QRSC.Analytics.CreditMetrics;
using BRA = nabCap.QR.BoundaryRider.DataTransfer;

using NUnit.Framework;

#endregion

namespace National.QRSC.AnalyticModels.Tests.Models.Credit
{
    [TestFixture]
    public class ROETest
    {
        private IDictionary<StreamFields, List<object>> _fixedStreamData;
        private IDictionary<StreamFields, List<object>> _floatingStreamData;

        [SetUp]
        public void Initialisation()
        {
            //Setup data for an fixed vs floating IR Swap
            //(a) setup fixed stream data
            _fixedStreamData = new SortedDictionary<StreamFields, List<object>>();
            _fixedStreamData.Add(StreamFields.PaymentDate,
                               ConvertToList<string>(CreditStaticTestData.paymentDates));
            _fixedStreamData.Add(StreamFields.Rate,
                                ConvertToList<decimal>(CreditStaticTestData.fixedRate));
            _fixedStreamData.Add(StreamFields.Notional,
                                ConvertToList<decimal>(CreditStaticTestData.notionals));
            _fixedStreamData.Add(StreamFields.AdjustedStartDate,
                               ConvertToList<string>(CreditStaticTestData.adjustedStartDates));
            _fixedStreamData.Add(StreamFields.AdjustedEndDate,
                               ConvertToList<string>(CreditStaticTestData.adjustedEndDates));
            _fixedStreamData.Add(StreamFields.CouponYearFraction,
                               ConvertToList<decimal>(CreditStaticTestData.couponYearFraction));
            _fixedStreamData.Add(StreamFields.DayCountConvention,
                               ConvertToList<string>(CreditStaticTestData.dayCountConventions));
            _fixedStreamData.Add(StreamFields.DiscountingType,
                               ConvertToList<string>(CreditStaticTestData.discountingTypes));
            _fixedStreamData.Add(StreamFields.Currency,
                               ConvertToList<string>(CreditStaticTestData.currency));
            _fixedStreamData.Add(StreamFields.DateAdjustmentConvention,
                               ConvertToList<string>(CreditStaticTestData.dateAdjustmentConventions));
            //(b) setup floating stream data
            _floatingStreamData = new SortedDictionary<StreamFields, List<object>>();

            _floatingStreamData.Add(StreamFields.PaymentDate,
                                ConvertToList<string>(CreditStaticTestData.paymentDates));

            _floatingStreamData.Add(StreamFields.Notional,
                                    ConvertToList<decimal>(CreditStaticTestData.notionals));
            _floatingStreamData.Add(StreamFields.AdjustedStartDate,
                                    ConvertToList<string>(CreditStaticTestData.adjustedStartDates));
            _floatingStreamData.Add(StreamFields.AdjustedEndDate,
                                    ConvertToList<string>(CreditStaticTestData.adjustedEndDates));
            _floatingStreamData.Add(StreamFields.CouponYearFraction,
                                    ConvertToList<decimal>(CreditStaticTestData.couponYearFraction));
            _floatingStreamData.Add(StreamFields.DayCountConvention,
                                ConvertToList<string>(CreditStaticTestData.dayCountConventions));
            _floatingStreamData.Add(StreamFields.DiscountingType,
                                    ConvertToList<string>(CreditStaticTestData.discountingTypes));
            _floatingStreamData.Add(StreamFields.Currency,
                                    ConvertToList<string>(CreditStaticTestData.currency));
            _floatingStreamData.Add(StreamFields.DateAdjustmentConvention,
                                    ConvertToList<string>(CreditStaticTestData.dateAdjustmentConventions));
            _floatingStreamData.Add(StreamFields.RateObservationSpecified,
                                ConvertToList<Boolean>(CreditStaticTestData.rateObservationSpecified));
            _floatingStreamData.Add(StreamFields.ObservedRate,
                                    ConvertToList<decimal>(CreditStaticTestData.observedRates));
            _floatingStreamData.Add(StreamFields.ResetDate,
                                    ConvertToList<string>(CreditStaticTestData.resetDates));
            _floatingStreamData.Add(StreamFields.Margin,
                                ConvertToList<decimal>(CreditStaticTestData.margins));
            _floatingStreamData.Add(StreamFields.Rate,
                                ConvertToList<decimal>(CreditStaticTestData.floatingRate));
            _floatingStreamData.Add(StreamFields.RateIndexName,
                                ConvertToList<string>(CreditStaticTestData.rateIndexNames));
        }

        //[Test]
        //public void TestROEAnalyticsDummyCall()
        //{
        //    var modelMetrics = new [] { ROEMetrics.ROE  }; // what analytics we want to calculate

        //    var modelParameters = new ROEParameters();
            
        //    var model = new ROEAnalytic();
            
        //    var modelResult = model.Calculate<IROEResult, ROEResult>(modelParameters, modelMetrics);

        //    // Dump a content of the modelResult 
        //    //
        //    Dump(modelResult);
        //}

        private void PopulateModelParameters(ROEParameters parameters)
        {

            parameters.BaseCalculationDate = DateTime.Today;
            parameters.CounterpartyRatingID = 2; //Note: must be eCRS 1-23, 98, 99
            parameters.LendingCategory = "J"; //LGD lending category
            parameters.LGDCounterpartyType = "LARGE CORPORATE"; //LGD counterparty type
            parameters.RegCapCounterpartyType = "CORPORATE"; //used to determine risk weights in reg cap calc (Basel I only) 

            parameters.Region = "Australia";
            parameters.CapitalType = "TIER1";

            parameters.Notional = 100000000.0m;
            parameters.Margin = 0.0m;

            parameters.TransactionStartDate = DateTime.Today;
            parameters.TransactionMaturityDate = parameters.TransactionStartDate.AddYears(4);

            parameters.TransactionProductType = "IR";
            parameters.TransactionCurrency = "AUD";
            parameters.FXExchRateStatProvCcyToTransCcy = 0.65m;
            parameters.FrequencyOfFuturePoints = 3; /* used as multiplier for future point set in final ROE calc */

            parameters.DayCountConvention = "ACT365";

            parameters.TraceMode = false;
            //time buckets to use
            parameters.TimeBuckets = new int[] { 0, 90, 180, 270, 360, 450, 540, 630, 730, 820, 910, 1000, 1095, 1460 };


            var cashflowDates = new List<DateTime>();
            foreach (var dayOffset in parameters.TimeBuckets)
            {
                cashflowDates.Add(parameters.TransactionStartDate.AddDays(dayOffset));
            }
            parameters.CashflowDates = cashflowDates.ToArray();


            parameters.CashflowAmounts = new[]
                                             {
                                                 0.0M,
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //0 to 90
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //90 to 180
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //180 to 270
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //270 to 360
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //360 to 450
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //450 to 540
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //540 to 630
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //630 to 730
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //730 to 820
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //820 to 910
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //910 to 1000
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //1000 to 1095
                                                 parameters.Notional * 0.05m * 0.25m * 0.01m, //1095 to 1460
                                             };

            parameters.Costs = new[]
                                   {
                                       0.0M,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m,
                                       parameters.Notional * 0.05m * 0.25m * 0.001m
                                   };

            //should be from rate curve (ie market data) in practice
            parameters.DiscountFactors = new[]
                                             {
                                                 0.0M,
                                                 1.0m / (1m + 0.05m * 0.25m * 1m),
                                                 1.0m / (1m + 0.05m * 0.25m * 2m),
                                                 1.0m / (1m + 0.05m * 0.25m * 3m),
                                                 1.0m / (1m + 0.05m * 0.25m * 4m),
                                                 1.0m / (1m + 0.05m * 0.25m * 5m),
                                                 1.0m / (1m + 0.05m * 0.25m * 6m),
                                                 1.0m / (1m + 0.05m * 0.25m * 7m),
                                                 1.0m / (1m + 0.05m * 0.25m * 8m),
                                                 1.0m / (1m + 0.05m * 0.25m * 9m),
                                                 1.0m / (1m + 0.05m * 0.25m * 10m),
                                                 1.0m / (1m + 0.05m * 0.25m * 11m),
                                                 1.0m / (1m + 0.05m * 0.25m * 12m),
                                                 1.0m / (1m + 0.05m * 0.25m * 13m)
                                             };


            var npvs = new List<decimal>();

            for (int i = 0; i < parameters.CashflowAmounts.Length; ++i)
            {
                var nonDiscountedAmount = parameters.CashflowAmounts[i];
                var discountedAmount = nonDiscountedAmount * parameters.DiscountFactors[i];
                npvs.Add(discountedAmount);
            }

            parameters.NPVs = npvs.ToArray();
            parameters.StatisticalProvisions = new[]
                                                   {  0.0M,
                                                       //just dummy data here
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m,
                                                       100m
                                                   };

        }

        private static IDictionary<int, decimal> LoadTermValueFromFile(string filePath)
        {
            return CMF.Utilities.DataReader.InputData(filePath, (int)CMF.Utilities.DataReader.FileColumn.COL1, (int)CMF.Utilities.DataReader.FileColumn.COL2);
        }

        private static IDictionary<DateTime, decimal> LoadDateTermValueFromFile(string filePath)
        {
            return CMF.Utilities.DataReader.InputDateIndexedData(filePath, (int)CMF.Utilities.DataReader.FileColumn.COL1, (int)CMF.Utilities.DataReader.FileColumn.COL2);
        }


        /*
        [Test]
        public void TestLoadingDatFilesFromEmbeddedResources()
        {
            foreach (var resourceName in new []
                                             {
                                                 "Costs.dat",
                                                 "Curve.dat",
                                                 "EPE.dat",
                                                 "MTM.dat",
                                                 "RevenueArr.dat"
                                             })
            {
                var resourceContent = General.Utilities.ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), resourceName);

                Assert.IsNotEmpty(resourceContent);
            }
        }
         * */


        private static void Dump(IROEResult roeResult)
        {
            Debug.Print("IROEResult.ROE : {0}", roeResult.ROE);
        }

        [Test]
        public void TestROEAnalyticsCall()
        {
            var modelMetrics = new [] { ROEMetrics.ROE  }; // specify metrics to calculate
            var modelParameters = new ROEParameters();
            // Populate input parameters
            PopulateModelParameters(modelParameters);
       
            var model = new ROEAnalytic();      
            var modelResult = model.Calculate<IROEResult, ROEResult>(modelParameters, modelMetrics);
            // Dump a content of the modelResult 
            Dump(modelResult);

            Assert.IsTrue(true);
        }

        private static IDictionary<DateTime, decimal> setupRateCurve()
        {
            IDictionary<DateTime, decimal> result = new SortedDictionary<DateTime, decimal>();
            result.Add(DateTime.Parse("7/08/2008"), 0.0725M);
            result.Add(DateTime.Parse("15/08/2008"), 0.07281M);
            result.Add(DateTime.Parse("22/08/2008"), 0.073122M);
            result.Add(DateTime.Parse("29/08/2008"), 0.073434M);
            result.Add(DateTime.Parse("8/09/2008"), 0.07397M);
            result.Add(DateTime.Parse("8/10/2008"), 0.074138M);
            result.Add(DateTime.Parse("10/11/2008"), 0.074345M);
            result.Add(DateTime.Parse("8/12/2008"), 0.074251M);
            result.Add(DateTime.Parse("8/01/2009"), 0.074104M);
            result.Add(DateTime.Parse("9/02/2009"), 0.074007M);
            result.Add(DateTime.Parse("8/05/2009"), 0.073294M);
            result.Add(DateTime.Parse("10/08/2009"), 0.072489M);
            result.Add(DateTime.Parse("9/11/2009"), 0.071962M);
            result.Add(DateTime.Parse("8/02/2010"), 0.071415M);
            result.Add(DateTime.Parse("10/05/2010"), 0.070912M);
            result.Add(DateTime.Parse("9/08/2010"), 0.070372M);
            result.Add(DateTime.Parse("8/08/2011"), 0.069628M);
            result.Add(DateTime.Parse("8/08/2012"), 0.069593M);
            result.Add(DateTime.Parse("8/08/2013"), 0.069565M);
            result.Add(DateTime.Parse("10/08/2015"), 0.069876M);
            result.Add(DateTime.Parse("8/08/2018"), 0.07019M);
            return result;
        }

        private IDictionary<int, decimal> getZeroSwapCurve(string serverName, string ccy)
        {
            string dbName = "Rider";
            BRA.Controllers.DB.BrDbReader reader = new BRA.Controllers.DB.BrDbReader(serverName, dbName);
            return reader.getCurve("MM", "ZERO", "SWAP", ccy);
        }


        //[Test]
        //public void TestROE_FxForward()
        //{
        //    //No current support in HL for fxforward so will patch-in using BR support
        //    //*** MAIN PARAMETERS ***/
        //    string brServer = "sydwadqur04";
        //    string baseCcy = "AUD";
        //    decimal notional = 0.0M;
        //    DateTime evalDate = new DateTime(2009, 3, 18);
        //    DateTime maturityDate = new DateTime(2010, 3, 18);
        //    string tradeXML = CreditStaticTestData.fxForwardTestTrade;
        //    int[] timeBuckets = CMF.DataUtilities.GenerateTimeBuckets(evalDate,
        //                                                              maturityDate,
        //                                                              1);
            
        //    //get tables from DB
        //    //DB connection string
        //    string dbString = "Data Source=SYDWADBRL01;Initial Catalog=CreditConfig;Integrated Security=True";
        //    CMF.IStressedLGDTable lgdTable = new CMF.Tables.LGDTable(dbString);
        //    CMF.ILGDTable spLGDTable = new CMF.Tables.LGDTable(dbString); //unstressed lgd
        //    CMF.IPDTable pdTable = new CMF.Tables.PDTable(dbString);
        //    CMF.IMDPTable mdpTable = new CMF.Tables.MDPTable(dbString);
        //    CMF.IRegionalSetup paramTable = new CMF.Tables.RegionConfigTable(dbString);
        //    CMF.IGlobalSetup globTable = new CMF.Tables.GlobalConfigTable(dbString);

        //      //setup ROE input data
        //    CMF.ICreditMetricCalc input = new CMF.ROEInputData();
        //    input.CounterpartyRatingID = 5;
        //    input.Region = "Australia";
        //    input.LendingCategory = "J";
        //    input.LGDCounterpartyType = "LARGE CORPORATE";
        //    input.CapitalType = "TIER1";
        //    input.FrequencyOfFuturePoints = 1;
        //    input.TransactionCurrency = baseCcy;
        //    input.Notional = notional;
        //    input.EvaluationDate = evalDate;
        //    input.TransactionStartDate = evalDate;
        //    input.TransactionMaturityDate = maturityDate;
        //    input.TransactionProductType = "FXGOLD";
            
        //    // *** (1) Make call to BR to get EE and MTM profile
        //    BRA.Interface.ITrade trade = BRA.Helpers.BrTradeBlobReaderHelper.LoadTradeBlobFromString(tradeXML)[0];
        //    trade.SourceId = "1";
        //    BRA.Interface.IParameters param = new BRA.Wrappers.Parameters(BRA.Interface.ParametersSetupType.StatProv);
        //    IList<BRA.Interface.ITrade> trades = new List<BRA.Interface.ITrade>();
        //    trades.Add(trade);
        //    param.CalculationDate = evalDate;
        //    //subtract 1 from date due to settlement lag
        //    param.CalculationDate = param.CalculationDate.AddDays(-1);
        //    param.TimeBuckets = timeBuckets;
        //    param.SimulationMethod = "RiderSim";
        //    BRA.Configuration.ConfigurationData config = BRA.Configuration.ConfigurationData.GetDefaults();
        //    config.System.BoundaryRiderUrl = "http://" + brServer + "/RiskEngineService/Analytics.asmx";
        //    config.System.EndpointConfigurationName = "BoundaryRider.RiskEngineService.AnalyticsSoap";
        //    BRA.Outputs.Result brResult = new BRA.Outputs.Result();
        //    brResult.Status = false;
        //    BRA.Helpers.Client.BaseClientCallSetupHelper helper = new BRA.Helpers.Client.BaseClientCallSetupHelper();
        //    brResult = helper.CalculateStatProv(trades, param, config);
        //    List<decimal> eeList = new List<decimal>();
        //    List<decimal> mtmList = new List<decimal>();
        //    for (int i = 0; i < brResult.Buckets.Count; i++)
        //    {
        //        //use the isNode boolean value to determine if the node matches the timebucket node from input
        //        if (brResult.Buckets[i].IsNode == true)
        //        {
        //            eeList.Add(Convert.ToDecimal(brResult.Buckets[i].EE)); //EE
        //            mtmList.Add(Convert.ToDecimal(brResult.Buckets[i].FM)); //MTM
        //        }
        //    }
        //    // *** (2) Calculate market discount factors
        //    IDictionary<int, decimal> zCurve = getZeroSwapCurve(brServer, baseCcy);
        //    decimal[] marketDfs = CMF.CurveUtilities.GenerateDiscountFactorsFromCurve(timeBuckets,
        //                                                    zCurve);
           
        //    // *** (3) Make call to stat prov calculation
        //    decimal costCapital = globTable.GetCostOfCaptial(0);
        //    decimal[] spDFs = CMF.ROECalculator.GetCostOfCapitalDFs(timeBuckets, costCapital); 
        //      //get lgd and pd values
        //    decimal lgdValue = spLGDTable.GetLGD(spLGDTable.TranslateToLendingCategory(input.LendingCategory),
        //                                         spLGDTable.TranslateToLGDCounterpartyType(input.LGDCounterpartyType));
        //    decimal[] mdpRange = CMF.PDTableUtils.SetupPDVector(
        //                                          Convert.ToInt32(input.CounterpartyRatingID),
        //                                          Convert.ToInt32(input.FrequencyOfFuturePoints),
        //                                          timeBuckets.Length,
        //                                          mdpTable);
        //    decimal[] statProv = CMF.StatProvCalculator.CalculateStatProv(timeBuckets,
        //                                                                  mdpRange,
        //                                                                  lgdValue,
        //                                                                  eeList.ToArray(),
        //                                                                  spDFs);


        //    // *** (5) Caculate revenue
        //    decimal[] projRev = new decimal[timeBuckets.Length];
        //    //TODO: here

        //    // *** (6) Make call to ROE calculation
        
        //    //setup logger
        //    CMF.IDiagnostic log = new CMF.DiagnosticLogger();
            
        //    //call roe calc (Basel II)
        //    decimal[] revenue = projRev; //revenue
        //    decimal[] mtm = mtmList.ToArray(); //mtm
        //    decimal[] df = marketDfs; //market discounts
        //    decimal[] costs = new decimal[timeBuckets.Length]; //costs
        //    decimal[] sp = statProv; //stat Prov

        //    /*
        //    CMF.CreditMetricResult roeResult = CMF.ROEDataManager.ManageROEInterim(input,
        //                                    timeBuckets,
        //                                    revenue,
        //                                    mtm,
        //                                    df,
        //                                    costs,
        //                                    sp,
        //                                    1.0M, //fx exchange rate
        //                                    CMF.RegCapitalType.Basel_II,
        //                                    lgdTable,
        //                                    pdTable,
        //                                    mdpTable,
        //                                    paramTable,
        //                                    globTable,
        //                                    log); */

        //    Assert.IsTrue(true);

        //}


        //[Test]
        //public void TestROEAnalyticsCall_withEEandStatProv()
        //{
        //    var modelMetrics = new[] { ROEMetrics.ROE }; // specify metrics to calculate
        //    var modelParameters = new ROEParameters();
        //    DateTime evalDate = new DateTime(2009, 1, 5);
        //    // Populate input parameters
        //    PopulateModelParameters(modelParameters);

        //    //(1) First do EE call
        //    EEMetrics[] eeMetrics = new EEMetrics[] { EEMetrics.EE, EEMetrics.EE_SQ, EEMetrics.MTM, EEMetrics.Term, EEMetrics.IsNode };
        //    IEEParameters eeInput = new EEParameters();
        //    eeInput.PayLegStreamData = _fixedStreamData;
        //    eeInput.ReceiveLegStreamData = _floatingStreamData;
        //    eeInput.PayLegStreamFixed = true;
        //    eeInput.ReceiveLegStreamFixed = false;
        //    eeInput.SimulationMethod = SimulationMethod.RiderNet; //set to RiderNet
        //    eeInput.CalculationTimeBuckets = modelParameters.TimeBuckets;
        //    eeInput.BoundaryRiderProductName = ProductName.IRSwap;
        //    eeInput.BasePartyPays = true;
        //    eeInput.EvaluationDate = evalDate;
        //    eeInput.BoundaryRiderServerURL = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
        //    EEAnalytic eeCall = new EEAnalytic();
        //    IEEResult eeResult = new EEResult();
        //    eeResult = eeCall.Calculate<IEEResult, EEResult>(eeInput, eeMetrics);
        //    List<decimal> eeList = new List<decimal>();
        //    List<decimal> mtmList = new List<decimal>();
        //    for (int i = 0; i < eeResult.IsNode.Length; i++)
        //    {
        //        //use the isNode boolean value to determine if the node matches the timebucket node from input
        //        if (eeResult.IsNode[i] == true)
        //        {
        //            eeList.Add(eeResult.EE[i]);
        //            mtmList.Add(eeResult.MTM[i]);
        //        }
        //    }

        //    //(2) Then do stat prov call
        //    StatProvMetrics[] spMetrics = new StatProvMetrics[] { StatProvMetrics.StatProv };
        //    IStatProvParameters statprovInput = new StatProvParameters();
        //    statprovInput.timeBuckets = modelParameters.TimeBuckets;
        //    statprovInput.discountFactors = CMF.ROECalculator.GetCostOfCapitalDFs(modelParameters.TimeBuckets,
        //                                            0.11M); //based on 11% pa cost capital
        //    statprovInput.epe = eeList.ToArray(); //EE vector
        //    statprovInput.FrequencyOfFuturePoints = 3; //3 monthly (used to construct PD data vector)
        //    statprovInput.CounterpartyRatingID = 0; //no rating
        //    statprovInput.LendingCategory = "A"; //large corporate
        //    statprovInput.LGDCounterpartyType = "LARGE CORPORATE";
        //    StatProvAnalytic statprovCall = new StatProvAnalytic();
        //    IStatProvResult spResult = new StatProvResult();
        //    spResult = statprovCall.Calculate<IStatProvResult, StatProvResult>(statprovInput, spMetrics);
        //    modelParameters.StatisticalProvisions = spResult.StatProv;

        //    //(3) Now do ROE call
        //    modelParameters.NPVs = mtmList.ToArray();
        //    var model = new ROEAnalytic();
        //    var modelResult = model.Calculate<IROEResult, ROEResult>(modelParameters, modelMetrics);

        //    Assert.IsTrue(true);
        //}




        private List<object> ConvertToList<T>(T[] array)
        {
            List<object> result = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                result.Add((object)array[i]);
            }
            return result;
        }

        private T[] CreateArray<T>(T[] theArray, int index, int length)
        {
            T[] newArray = new T[length];
            int j = 0;

            for (int i = index; i < length; ++i)
            {
                newArray[j] = theArray[i];
                ++j;
            }
            return newArray;
        }

    }
}