using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

using National.QRSC.AnalyticModels.CreditMetrics;

using National.QRSC.AnalyticModels;

namespace National.QRSC.AnalyticModels.Tests.Models.Credit
{
    [TestFixture]
    public class EETest
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
        public void TestEECall1()
        {
            EEMetrics[] metrics = new EEMetrics[] { EEMetrics.EE, EEMetrics.EE_SQ, EEMetrics.MTM, EEMetrics.Term, EEMetrics.IsNode };
            IEEParameters eeInput = new EEParameters();
            eeInput.PayLegStreamData = _fixedStreamData;
            eeInput.ReceiveLegStreamData = _floatingStreamData;
            eeInput.PayLegStreamFixed = true;
            eeInput.ReceiveLegStreamFixed = false;
            eeInput.SimulationMethod = SimulationMethod.RiderSim;
            eeInput.CalculationTimeBuckets = new int[] { 0, 7, 30, 90, 180, 365, 730, 1095, 1460, 1825, 2555, 3650, 36163 };
            eeInput.BoundaryRiderProductName = ProductName.IRSwap;
            eeInput.BasePartyPays = true;
            eeInput.EvaluationDate = new DateTime(2009, 1, 5);
            eeInput.BoundaryRiderServerURL = "http://sydwadqur04/RiskEngineService/Analytics.asmx";
            EEAnalytic eeCall = new EEAnalytic();
            IEEResult result = new EEResult();
            result = eeCall.Calculate<IEEResult, EEResult>(eeInput, metrics);

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
