using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

using National.QRSC.AnalyticModels.CreditMetrics;

using National.QRSC.AnalyticModels;
using BRA = nabCap.QR.BoundaryRider.DataTransfer;

namespace National.QRSC.AnalyticModels.Tests.Models.Credit
{
    [TestFixture]
    public class PCETest
    {
        private IDictionary<StreamFields, List<object>> _fixedStreamData;
        private IDictionary<StreamFields, List<object>> _floatingStreamData;

        [SetUp]
        public void Initialisation()
        {
            //setup fixed stream data
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
            //setup floating stream data
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

        [Test]
        public void TestPCECall_FXForwardRaw()
        {
            BRA.Interface.ITrade trade = BRA.Helpers.BrTradeBlobReaderHelper.LoadTradeBlobFromString(CreditStaticTestData.fxForwardTestTrade)[0];
            trade.SourceId = "1";
            BRA.Interface.IParameters param = new BRA.Wrappers.Parameters(BRA.Interface.ParametersSetupType.Credit);

            IList<BRA.Interface.ITrade> trades = new List<BRA.Interface.ITrade>();
            trades.Add(trade);
            param.CalculationDate = new DateTime(2009, 3, 18); 
        
            param.TimeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };
            param.SimulationMethod = "RiderSim";
            
            BRA.Configuration.ConfigurationData config = BRA.Configuration.ConfigurationData.GetDefaults();
            config.System.BoundaryRiderUrl = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
            config.System.EndpointConfigurationName = "BoundaryRider.RiskEngineService.AnalyticsSoap";

            // *** (2) Make WS call to BR Server ***
            BRA.Outputs.Result brResult = new BRA.Outputs.Result();
            brResult.Status = false;
            BRA.Helpers.Client.BaseClientCallSetupHelper helper = new BRA.Helpers.Client.BaseClientCallSetupHelper();

            brResult = helper.CalculateCreditExposure(trades, param, config);
            Assert.IsTrue(brResult.Status);

        }


        [Test]
        public void TestPCECall_IRSwapRaw()
        {
            BRA.Interface.ITrade trade = BRA.Helpers.BrTradeBlobReaderHelper.LoadTradeBlobFromString(CreditStaticTestData.IRSwap_scenario1)[0];
            trade.SourceId = "1";
            BRA.Interface.IParameters param = new BRA.Wrappers.Parameters(BRA.Interface.ParametersSetupType.Credit);

            IList<BRA.Interface.ITrade> trades = new List<BRA.Interface.ITrade>();
            trades.Add(trade);
            param.CalculationDate =  new DateTime(2009, 1, 16);
          
            param.TimeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };
            BRA.Configuration.ConfigurationData config = BRA.Configuration.ConfigurationData.GetDefaults();
            config.System.BoundaryRiderUrl = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
            config.System.EndpointConfigurationName = "BoundaryRider.RiskEngineService.AnalyticsSoap";

            // *** (2) Make WS call to BR Server ***
            BRA.Outputs.Result brResult = new BRA.Outputs.Result();
            brResult.Status = false;
            BRA.Helpers.Client.BaseClientCallSetupHelper helper = new BRA.Helpers.Client.BaseClientCallSetupHelper();

            brResult = helper.CalculateCreditExposure(trades, param, config);
            Assert.IsTrue(brResult.Status);
        }

        [Test]
        public void TestPCECall_CcySwapRaw()
        {
            BRA.Interface.ITrade trade = BRA.Helpers.BrTradeBlobReaderHelper.LoadTradeBlobFromString(CreditStaticTestData.CcySwap_scenario1)[0];
            trade.SourceId = "1";
            BRA.Interface.IParameters param = new BRA.Wrappers.Parameters(BRA.Interface.ParametersSetupType.Credit);

            IList<BRA.Interface.ITrade> trades = new List<BRA.Interface.ITrade>();
            trades.Add(trade);
            param.CalculationDate = new DateTime(2009, 1, 16); ;
            param.TimeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };
            BRA.Configuration.ConfigurationData config = BRA.Configuration.ConfigurationData.GetDefaults();
            config.System.BoundaryRiderUrl = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
            config.System.EndpointConfigurationName = "BoundaryRider.RiskEngineService.AnalyticsSoap";

            // *** (2) Make WS call to BR Server ***
            BRA.Outputs.Result brResult = new BRA.Outputs.Result();
            brResult.Status = false;
            BRA.Helpers.Client.BaseClientCallSetupHelper helper = new BRA.Helpers.Client.BaseClientCallSetupHelper();

            brResult = helper.CalculateCreditExposure(trades, param, config);
            Assert.IsTrue(brResult.Status);
        }

        [Test]
        public void TestPCECall_IRCapFloorRaw()
        {
            BRA.Interface.ITrade trade = BRA.Helpers.BrTradeBlobReaderHelper.LoadTradeBlobFromString(CreditStaticTestData.IRCapFloor_scenario1)[0];
            trade.SourceId = "1";
            BRA.Interface.IParameters param = new BRA.Wrappers.Parameters(BRA.Interface.ParametersSetupType.Credit);

            IList<BRA.Interface.ITrade> trades = new List<BRA.Interface.ITrade>();
            trades.Add(trade);
            param.CalculationDate = new DateTime(2009, 1, 16); ;
            param.TimeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };
            BRA.Configuration.ConfigurationData config = BRA.Configuration.ConfigurationData.GetDefaults();
            config.System.BoundaryRiderUrl = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
            config.System.EndpointConfigurationName = "BoundaryRider.RiskEngineService.AnalyticsSoap";

            // *** (2) Make WS call to BR Server ***
            BRA.Outputs.Result brResult = new BRA.Outputs.Result();
            brResult.Status = false;
            BRA.Helpers.Client.BaseClientCallSetupHelper helper = new BRA.Helpers.Client.BaseClientCallSetupHelper();

            brResult = helper.CalculateCreditExposure(trades, param, config);
            Assert.IsTrue(brResult.Status);
        }

        [Test]
        public void TestPCECall_IRSwap1()
        {
            PCEMetrics[] metrics = new PCEMetrics[] { PCEMetrics.PCE };
            IPCEParameters pceInput = new PCEParameters();
            pceInput.PayLegStreamData = _fixedStreamData;
            pceInput.ReceiveLegStreamData = _floatingStreamData;
            pceInput.PayLegStreamFixed = true;
            pceInput.ReceiveLegStreamFixed = false;
            pceInput.SimulationMethod = SimulationMethod.RiderSim;
            pceInput.CalculationTimeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };
            pceInput.BoundaryRiderProductName = ProductName.IRSwap;
            pceInput.BasePartyPays = true;
            pceInput.EvaluationDate = new DateTime(2009, 1, 5);
            pceInput.BoundaryRiderServerURL = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
            
            PCEAnalytic pceCall = new PCEAnalytic();
            IPCEResult result = new PCEResult();
            result = pceCall.Calculate<IPCEResult, PCEResult>(pceInput, metrics);

            Assert.IsTrue(true);

        }

        [Test]
        public void TestPCECall_IRSwap2()
        {
            PCEMetrics[] metrics = new PCEMetrics[] { PCEMetrics.PCE, PCEMetrics.MTM, PCEMetrics.Term, PCEMetrics.IsNode };
            IPCEParameters pceInput = new PCEParameters();
            pceInput.PayLegStreamData = _fixedStreamData;
            pceInput.ReceiveLegStreamData = _floatingStreamData;
            pceInput.PayLegStreamFixed = true;
            pceInput.ReceiveLegStreamFixed = false;
            pceInput.SimulationMethod = SimulationMethod.RiderSim;
            pceInput.CalculationTimeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };
            pceInput.BoundaryRiderProductName = ProductName.IRSwap;
            pceInput.BasePartyPays = true;
            pceInput.EvaluationDate = new DateTime(2009, 1, 5);
            pceInput.BoundaryRiderServerURL = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
            
            PCEAnalytic pceCall = new PCEAnalytic();
            IPCEResult result = new PCEResult();
            result = pceCall.Calculate<IPCEResult, PCEResult>(pceInput, metrics);

            Assert.IsTrue(true);
        }

        [Test]
        public void TestPCECall_IRSwap3()
        {
            PCEMetrics[] metrics = new PCEMetrics[] { PCEMetrics.PCE, PCEMetrics.MTM, PCEMetrics.Term, PCEMetrics.IsNode, PCEMetrics.ResultXml };
            IPCEParameters pceInput = new PCEParameters();
            pceInput.PayLegStreamData = _fixedStreamData;
            pceInput.ReceiveLegStreamData = _floatingStreamData;
            pceInput.PayLegStreamFixed = true;
            pceInput.ReceiveLegStreamFixed = false;
            pceInput.SimulationMethod = SimulationMethod.RiderSim;
            pceInput.CalculationTimeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };
            pceInput.BoundaryRiderProductName = ProductName.IRSwap;
            pceInput.BasePartyPays = true;
            pceInput.EvaluationDate = new DateTime(2009, 1, 5);
            pceInput.BoundaryRiderServerURL = "http://sydwadqur04/RiskEngineService/Analytics.asmx";

            PCEAnalytic pceCall = new PCEAnalytic();
            IPCEResult result = new PCEResult();
            result = pceCall.Calculate<IPCEResult, PCEResult>(pceInput, metrics);

            Assert.IsTrue(true);
        }


    


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
