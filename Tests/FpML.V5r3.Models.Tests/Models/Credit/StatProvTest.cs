#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using National.QRSC.AnalyticModels.CreditMetrics;

using CMF = National.QRSC.Analytics.CreditMetrics;

using NUnit.Framework;
using BRA = nabCap.QR.BoundaryRider.DataTransfer;

#endregion

namespace National.QRSC.AnalyticModels.Tests.Models.Credit
{
    [TestFixture]
    public class StatProvTest
    {
        private decimal[] _eeData; //epe (expected positive exposure) vector for matching timebuckets
        private decimal[] _eeDataMonthly;
        private decimal[] _costCapDiscountFactors; //discount factors based on cost of capital
        private int[] _timeBuckets; //int lags used as offsets from base date for buckets
        private int[] _timeBucketsMonthly;
        private decimal _costCapitalRate;

        [SetUp]
        public void Initialisation()
        {
            _costCapitalRate = 0.11M;
            //setup fixed data
            _timeBuckets = new int[] { 0, 90, 180, 270, 360, 450, 540, 630, 730, 820, 910, 1000, 1095, 1460 };
            _timeBucketsMonthly = new int[] { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330, 360, 390, 420, 450, 480, 510, 540, 570, 600, 630, 660, 690, 720, 750, 780, 810, 840, 870, 900, 930, 960, 990, 1020, 1050, 1080, 1120, 1150, 1180, 2210 };
            //this data is generated for a specific trade by calling the stat prov web service on boundary rider
            _eeData = new decimal[] { 211985.89M, 215484.05M, 219417.59M, 221935.30M, 222822.57M, 221751.66M, 220143.99M, 219973.86M, 5117376.44M, 5215793.49M, 5428565.72M, 0.0M, 0.0M, 0.0M };
            _eeDataMonthly = new decimal[] { 211985.89M, 211985.89M, 211985.89M, 215484.05M, 215484.05M, 215484.05M, 219417.59M, 219417.59M, 219417.59M, 221935.30M, 221935.30M, 221935.30M, 222822.57M, 222822.57M, 222822.57M, 221751.66M, 221751.66M, 221751.66M, 220143.99M, 220143.99M, 220143.99M, 219973.86M, 219973.86M, 219973.86M, 5117376.44M, 5117376.44M, 5117376.44M, 5215793.49M, 5215793.49M, 5215793.49M, 5428565.72M, 5428565.72M, 5428565.72M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M };
            //The discount factor is expressed as the reciprocal of 1 plus a cost of capital
            //ie df = 1.0/(1.0 + 0.11) with 11% cost of capital annual rate
            _costCapDiscountFactors = new decimal[_timeBuckets.Length];
            for (int j = 0; j < _timeBuckets.Length; j++)
            {
                //Using flat anually compounded rate
                _costCapDiscountFactors[j] = 1.0M / (decimal)Math.Pow(1.0 + (double)_costCapitalRate, _timeBuckets[j] / 365.0D);
            }
        }

        [Test]
        public void TestStatProv_1()
        {
            StatProvMetrics[] metrics = new StatProvMetrics[] { StatProvMetrics.StatProv };
            IStatProvParameters statprovInput = new StatProvParameters();
            statprovInput.timeBuckets = _timeBucketsMonthly;
            statprovInput.discountFactors = _costCapDiscountFactors;
            statprovInput.epe = _eeDataMonthly;
            statprovInput.FrequencyOfFuturePoints = 1; //3 monthly (used to construct PD data vector)
            statprovInput.CounterpartyRatingID = 0; //no rating
            statprovInput.LendingCategory = "A"; //large corporate
            statprovInput.LGDCounterpartyType = "LARGE CORPORATE";
            StatProvAnalytic statprovCall = new StatProvAnalytic();
            IStatProvResult result = new StatProvResult();
            result = statprovCall.Calculate<IStatProvResult, StatProvResult>(statprovInput, metrics);

            Assert.IsTrue(true);
        }

        [Test]
        public void TestStatProv_2()
        {
            StatProvMetrics[] metrics = new StatProvMetrics[] { StatProvMetrics.StatProv };
            IStatProvParameters statprovInput = new StatProvParameters();
            statprovInput.timeBuckets = _timeBuckets;
            statprovInput.discountFactors = _costCapDiscountFactors;
            statprovInput.epe = _eeData;
            statprovInput.FrequencyOfFuturePoints = 3; //3 monthly (used to construct PD data vector)
            statprovInput.CounterpartyRatingID = 3; //e-CRS rating 3
            statprovInput.LendingCategory = "J"; //large corporate
            statprovInput.LGDCounterpartyType = "LARGE CORPORATE";
            StatProvAnalytic statprovCall = new StatProvAnalytic();
            IStatProvResult result = new StatProvResult();
            result = statprovCall.Calculate<IStatProvResult, StatProvResult>(statprovInput, metrics);

            Assert.IsTrue(true);
        }

        //[Test]
        //public void TestStatProv_TradeWithEECall()
        //{
        //    int[] timeBuckets = _timeBuckets;
        //    decimal[] eeVector = new decimal[timeBuckets.Length];

        //    //First do a call to the StatProv service of boundary rider
        //    //using an IRSwap trade and get the EE vector
        //    BRA.Interface.ITrade trade = BRA.Helpers.BrTradeBlobReaderHelper.LoadTradeBlobFromString(CreditStaticTestData.IRSwap_scenario2)[0];
        //    trade.SourceId = "1";
        //    BRA.Interface.IParameters param = new BRA.Wrappers.Parameters(BRA.Interface.ParametersSetupType.StatProv);

        //    IList<BRA.Interface.ITrade> trades = new List<BRA.Interface.ITrade>();
        //    trades.Add(trade);
        //    param.CalculationDate = new DateTime(2009, 1, 16); ;
        //    param.TimeBuckets = timeBuckets;
        //    BRA.Configuration.ConfigurationData config = BRA.Configuration.ConfigurationData.GetDefaults();
        //    config.System.BoundaryRiderUrl = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
        //    config.System.EndpointConfigurationName = "BoundaryRider.RiskEngineService.AnalyticsSoap";

        //    BRA.Outputs.Result brResult = new BRA.Outputs.Result();
        //    brResult.Status = false;
        //    BRA.Helpers.Client.BaseClientCallSetupHelper helper = new BRA.Helpers.Client.BaseClientCallSetupHelper();

        //    brResult = helper.CalculateStatProv(trades, param, config);
        //    //only get result for nodes that match input time buckets
        //    List<decimal> tmpList = new List<decimal>();
        //    foreach (BRA.Interface.ExposureBucket exp in brResult.Buckets)
        //    {
        //        //use the isNode boolean value to determine if the node matches the timebucket node from input
        //        if (exp.IsNode == true) tmpList.Add(Convert.ToDecimal(exp.EE));
        //    }
        //    eeVector = tmpList.ToArray();

        //    //Now do the Stat Prov calc using the EE vector 
        //    StatProvMetrics[] metrics = new StatProvMetrics[] { StatProvMetrics.StatProv };
        //    IStatProvParameters statprovInput = new StatProvParameters();
        //    statprovInput.timeBuckets = _timeBuckets;
        //    statprovInput.discountFactors = _costCapDiscountFactors;
        //    statprovInput.epe = eeVector;
        //    statprovInput.FrequencyOfFuturePoints = 3; //3 monthly (used to construct PD data vector)
        //    statprovInput.CounterpartyRatingID = 0; //no rating
        //    statprovInput.LendingCategory = "A"; //large corporate
        //    statprovInput.LGDCounterpartyType = "LARGE CORPORATE";
        //    StatProvAnalytic statprovCall = new StatProvAnalytic();
        //    IStatProvResult result = new StatProvResult();
        //    result = statprovCall.Calculate<IStatProvResult, StatProvResult>(statprovInput, metrics);

        //    Assert.IsTrue(true);
        //}
    }
}
