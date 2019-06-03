#region Using

using System;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using FpML.V5r10.Reporting.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using FpML.V5r10.Reporting;
using Orion.Constants;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Schedulers;
using Orion.Analytics.Options;
using Orion.CalendarEngine.Helpers;
using Orion.CalendarEngine.Schedulers;
using Orion.UnitTestEnv;
using Orion.TestHelpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.CurveEngine.Markets;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.CurveEngine.Helpers;
using Orion.ValuationEngine.Generators;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Pricers.Products;
using Orion.ValuationEngine.Tests.Helpers;
using Math = System.Math;
using XsdClassesFieldResolver = FpML.V5r10.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Tests 
{
    [TestClass]
    public class ValuationEngineTests2
    {
        #region Internal Classes

        public class SwapExcelParameters
        {
            public SwapExcelParameters(SwapLegParametersRange_Old payLeg, SwapLegParametersRange_Old receiveLeg)
            {
                PayLeg = payLeg;
                ReceiveLeg = receiveLeg;
            }

            public SwapLegParametersRange_Old PayLeg { get; }

            public SwapLegParametersRange_Old ReceiveLeg { get; }
        }

        #endregion

        #region Properties

        //private static ILogger Logger_obs { get; set; }
        private static UnitTestEnvironment UTE { get; set; }
        private static CurveEngine.CurveEngine CurveEngine { get; set; }
        private static CalendarEngine.CalendarEngine CalendarEngine { get; set; }
        //private static TimeSpan Retention { get; set; }
        private static IBusinessCalendar FixingCalendar { get; set; }
        private static IBusinessCalendar PaymentCalendar { get; set; }
        private static IMarketEnvironment Market { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        //public static void Setup()
        //{
        //    UTE = new UnitTestEnvironment();
        //    //Set the calendar engine
        //    CurveEngine = new CurveEngine.CurveEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
        //    CalendarEngine = new CalendarEngine.CalendarEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
        //    // Set the Retention
        //    //Retention = TimeSpan.FromHours(1);
        //    const string center = "AUSY";
        //    FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(UTE.Cache, BusinessCentersHelper.Parse(center), UTE.NameSpace);
        //    PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(UTE.Cache, BusinessCentersHelper.Parse(center), UTE.NameSpace);
        //    Market = GetMarket();
        //}

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            //Set the calendar engine
            CurveEngine = new CurveEngine.CurveEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
            CalendarEngine = new CalendarEngine.CalendarEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
            // Set the Retention
            //Retention = TimeSpan.FromHours(1);
            const string center = "AUSY";
            FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(UTE.Cache, BusinessCentersHelper.Parse(center), UTE.NameSpace);
            PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(UTE.Cache, BusinessCentersHelper.Parse(center), UTE.NameSpace);
            Market = GetMarket();
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            UTE.Dispose();
        }

        #endregion

        #region Test Swap Modifying Cash Flows

        #region Helper functions

        public void HelperGenerateSwapChangeTwoFloatCashflowsIntoFixed(decimal rateChange)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            //  Update first two cashflows in float stream
            //
            foreach (int periodNumber in new[] { 0, 1 })
            {
                PaymentCalculationPeriod paymentCalculationPeriod = floatingStream.cashflows.paymentCalculationPeriod[periodNumber];
                //CalculationPeriod calcPeriod = PaymentCalculationPeriodHelper.GetCalculationPeriods(paymentCalculationPeriod)[0];

                decimal newRate = PaymentCalculationPeriodHelper.GetRate(paymentCalculationPeriod) + rateChange;

                PaymentCalculationPeriodHelper.ReplaceFloatingRateWithFixedRate(paymentCalculationPeriod, newRate);
            }
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            Debug.Print("Modified swap:");
            Debug.Print(XmlSerializerHelper.SerializeToString(generateSwapStructure.Swap));
            if (rateChange > 0) //that is we RECEIVE MORE
            {
                decimal originalSwapPV = SwapHelper.GetPresentValue(originalSwap, fixedStream.payerPartyReference.href).amount;
                decimal modifiedSwapPV = SwapHelper.GetPresentValue(generateSwapStructure.Swap, fixedStream.payerPartyReference.href).amount;
                Assert.IsTrue(modifiedSwapPV > originalSwapPV);
                decimal originalSwapFV = SwapHelper.GetFutureValue(originalSwap, fixedStream.payerPartyReference.href).amount;
                decimal modifiedSwapFV = SwapHelper.GetFutureValue(generateSwapStructure.Swap, fixedStream.payerPartyReference.href).amount;
                Assert.IsTrue(modifiedSwapFV > originalSwapFV);
            }
            else
            {
                decimal originalSwapPV = SwapHelper.GetPresentValue(originalSwap, fixedStream.payerPartyReference.href).amount;
                decimal modifiedSwapPV = SwapHelper.GetPresentValue(generateSwapStructure.Swap, fixedStream.payerPartyReference.href).amount;
                Assert.IsTrue(modifiedSwapPV < originalSwapPV);
                decimal originalSwapFV = SwapHelper.GetFutureValue(originalSwap, fixedStream.payerPartyReference.href).amount;
                decimal modifiedSwapFV = SwapHelper.GetFutureValue(generateSwapStructure.Swap, fixedStream.payerPartyReference.href).amount;
                Assert.IsTrue(modifiedSwapFV < originalSwapFV);
            }
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }

        private static SwapLegParametersRange_Old GetFloatingReceiveLeg(string curveId)
        {
            var receiveLeg = new SwapLegParametersRange_Old
            {
                Payer = "Counterparty",
                Receiver = "NAB",
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = curveId,
                ForecastCurve = curveId,
                DiscountingType = "None"
            };

            return receiveLeg;
        }

        private static SwapLegParametersRange_Old GetFixedPayLeg(string curveId)
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                Payer = "NAB",
                Receiver = "Counterparty",
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                FloatingRateSpread = 0,
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = curveId,
                DiscountingType = "None"
            };

            return payLeg;
        }

        #endregion

        #region Assert methods

        public void AssertPVAndFVIncreased(Swap originalSwap, Swap modifiedSwap)
        {
            Money payPresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[0].cashflows);
            Money payPresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[0].cashflows);

            Money receivePresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[1].cashflows);
            Money receivePresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[1].cashflows);

            Money payFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[0].cashflows);
            Money payFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[0].cashflows);

            Money receiveFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[1].cashflows);
            Money receiveFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[1].cashflows);

            //  Check PV
            //
            Money swapPresentValueOriginal = MoneyHelper.Sub(receivePresentValueOriginal, payPresentValueOriginal);
            Money swapPresentValueModified = MoneyHelper.Sub(receivePresentValueModified, payPresentValueModified);
            AssertExtension.Greater(swapPresentValueModified.amount, swapPresentValueOriginal.amount);

            //  Check FV
            //
            Money swapFutureValueOriginal = MoneyHelper.Sub(receiveFutureValueOriginal, payFutureValueOriginal);
            Money swapFutureValueModified = MoneyHelper.Sub(receiveFutureValueModified, payFutureValueModified);
            AssertExtension.Greater(swapFutureValueModified.amount, swapFutureValueOriginal.amount);
        }

        public void AssertPVAndFVDecreased(Swap originalSwap, Swap modifiedSwap)
        {
            Money payPresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[0].cashflows);
            Money payPresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[0].cashflows);

            Money receivePresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[1].cashflows);
            Money receivePresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[1].cashflows);

            Money payFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[0].cashflows);
            Money payFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[0].cashflows);

            Money receiveFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[1].cashflows);
            Money receiveFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[1].cashflows);

            //  Check PV
            //
            Money swapPresentValueOriginal = MoneyHelper.Sub(receivePresentValueOriginal, payPresentValueOriginal);
            Money swapPresentValueModified = MoneyHelper.Sub(receivePresentValueModified, payPresentValueModified);
            AssertExtension.Less(swapPresentValueModified.amount, swapPresentValueOriginal.amount);

            //  Check FV
            //
            Money swapFutureValueOriginal = MoneyHelper.Sub(receiveFutureValueOriginal, payFutureValueOriginal);
            Money swapFutureValueModified = MoneyHelper.Sub(receiveFutureValueModified, payFutureValueModified);
            AssertExtension.Less(swapFutureValueModified.amount, swapFutureValueOriginal.amount);
        }

        public void AssertPVOfStreamsDecreased(Swap originalSwap, Swap modifiedSwap)
        {
            //  Pay PV
            //
            Money payPresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[0].cashflows);
            Money payPresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[0].cashflows);
            AssertExtension.Less(payPresentValueModified.amount, payPresentValueOriginal.amount);

            //  Receive PV
            //
            Money receivePresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[1].cashflows);
            Money receivePresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[1].cashflows);
            AssertExtension.Less(receivePresentValueModified.amount, receivePresentValueOriginal.amount);
        }

        public void AssertPVOfStreamsIncreased(Swap originalSwap, Swap modifiedSwap)
        {
            //  Pay PV
            //
            Money payPresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[0].cashflows);
            Money payPresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[0].cashflows);
            AssertExtension.Greater(payPresentValueModified.amount, payPresentValueOriginal.amount);

            //  Receive PV
            //
            Money receivePresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[1].cashflows);
            Money receivePresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[1].cashflows);
            AssertExtension.Greater(receivePresentValueModified.amount, receivePresentValueOriginal.amount);
        }

        public void AssertAmountsAreIncreased(Swap originalSwap, Swap modifiedSwap)
        {
            Money payPresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[0].cashflows);
            Money payPresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[0].cashflows);
            AssertExtension.Greater(payPresentValueModified.amount, payPresentValueOriginal.amount);


            Money payFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[0].cashflows);
            Money payFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[0].cashflows);
            AssertExtension.Greater(payFutureValueModified.amount, payFutureValueOriginal.amount);

            Money receivePresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[1].cashflows);
            Money receivePresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[1].cashflows);
            AssertExtension.Greater(receivePresentValueModified.amount, receivePresentValueOriginal.amount);


            Money receiveFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[1].cashflows);
            Money receiveFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[1].cashflows);
            AssertExtension.Greater(receiveFutureValueModified.amount, receiveFutureValueOriginal.amount);
        }

        public void AssertAmountsAreDecreased(Swap originalSwap, Swap modifiedSwap)
        {
            Money payPresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[0].cashflows);
            Money payPresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[0].cashflows);
            AssertExtension.Less(payPresentValueModified.amount, payPresentValueOriginal.amount);


            Money payFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[0].cashflows);
            Money payFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[0].cashflows);
            AssertExtension.Less(payFutureValueModified.amount, payFutureValueOriginal.amount);

            Money receivePresentValueOriginal = CashflowsHelper.GetPresentValue(originalSwap.swapStream[1].cashflows);
            Money receivePresentValueModified = CashflowsHelper.GetPresentValue(modifiedSwap.swapStream[1].cashflows);
            AssertExtension.Less(receivePresentValueModified.amount, receivePresentValueOriginal.amount);


            Money receiveFutureValueOriginal = CashflowsHelper.GetForecastValue(originalSwap.swapStream[1].cashflows);
            Money receiveFutureValueModified = CashflowsHelper.GetForecastValue(modifiedSwap.swapStream[1].cashflows);
            AssertExtension.Less(receiveFutureValueModified.amount, receiveFutureValueOriginal.amount);

            //            Money swapPresentValue = MoneyHelper.Sub(receivePresentValue, payPresentValue);
            //            Money swapFutureValue = MoneyHelper.Sub(receiveFutureValue, payFutureValue);
        }

        #endregion

        #region Helper Set/Get methods

        private static void SetNotional(PaymentCalculationPeriod paymentCalculationPeriod, decimal value)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            XsdClassesFieldResolver.CalculationPeriodSetNotionalAmount(calculationPeriodArray[0], value);
        }

        private static decimal GetNotional(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            return XsdClassesFieldResolver.CalculationPeriodGetNotionalAmount(calculationPeriodArray[0]);
        }

        private static void SetFixedRate(PaymentCalculationPeriod paymentCalculationPeriod, decimal value)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            XsdClassesFieldResolver.SetCalculationPeriodFixedRate(calculationPeriodArray[0], value);
        }

        private static decimal GetFixedRate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            return XsdClassesFieldResolver.CalculationPeriodGetFixedRate(calculationPeriodArray[0]);
        }


        private static void SetFloatRate(PaymentCalculationPeriod paymentCalculationPeriod, decimal value)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriodArray[0]);
            floatingRateDefinition.calculatedRate = value;
        }

        private static decimal GetFloatRate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriodArray[0]);
            return floatingRateDefinition.calculatedRate;
        }


        private static void SetSpread(PaymentCalculationPeriod paymentCalculationPeriod, decimal spreadValue)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriodArray[0]);
            floatingRateDefinition.spread = spreadValue;
        }

        private static decimal GetSpread(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriodArray[0]);
            return floatingRateDefinition.spread;
        }

        #endregion

        #region Methods

        [Serializable]
        public class GenerateSwapStructure
        {
            public SwapLegParametersRange_Old PayLeg;
            public SwapLegParametersRange_Old ReceiveLeg;
            public DateTime ValuationDate;
            public string CurveId;
            public string MarketEnvironmentId;
            public Swap Swap;
            public InterestRateStream PayFixedStream;
            public InterestRateStream ReceiveFloatingStream;
        }

        private static GenerateSwapStructure GenerateSwap()
        {
            string curveId = "AUD-LIBOR-3M" + Guid.NewGuid();
            SwapLegParametersRange_Old payLeg = GetFixedPayLeg(curveId);
            SwapLegParametersRange_Old receiveLeg = GetFloatingReceiveLeg(curveId);
            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add(marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 20);
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            var result = new GenerateSwapStructure
            {
                PayLeg = payLeg,
                ReceiveLeg = receiveLeg,
                ValuationDate = valuationDT,
                CurveId = curveId,
                MarketEnvironmentId = marketEnvironmentId,
                Swap = swap,
                PayFixedStream = swap.swapStream[0],
                ReceiveFloatingStream = swap.swapStream[1]
            };

            return result;
        }

        private static void CleanUpMarketEnvironment(string marketEnvironmentId)
        {
            //  clean up market environment
            //
            MarketEnvironmentRegistry.Remove(marketEnvironmentId);
        }

        public void HelperGenerateSwapAddBulletPayments(bool payerPaysAdditional, NonNegativeMoney futureAmount, DateTime paymentDate)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            var payment = new Payment
            {
                payerPartyReference =
                    (payerPaysAdditional
                         ? fixedStream.payerPartyReference
                         : floatingStream.payerPartyReference),
                receiverPartyReference =
                    (payerPaysAdditional
                         ? fixedStream.receiverPartyReference
                         : floatingStream.receiverPartyReference),
                discountFactor = 0.93m,
                discountFactorSpecified = true,
                paymentAmount = futureAmount
            };
            payment.presentValueAmount = MoneyHelper.Mul(payment.paymentAmount, payment.discountFactor);
            payment.paymentDate = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(paymentDate);
            generateSwapStructure.Swap.additionalPayment = new[] { payment };
            //Debug.Print("With bullet payments {0}", XmlSerializerHelper.SerializeToString(generateSwapStructure.swap));
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            string basePartyIsPayerOfTheFixed = fixedStream.payerPartyReference.href;
            if (payerPaysAdditional)
            {
                decimal originalSwapPV = SwapHelper.GetPresentValue(originalSwap, basePartyIsPayerOfTheFixed).amount;
                decimal modifiedSwapPV = SwapHelper.GetPresentValue(generateSwapStructure.Swap, basePartyIsPayerOfTheFixed).amount;
                Assert.IsTrue(modifiedSwapPV < originalSwapPV);
                decimal originalSwapFV = SwapHelper.GetFutureValue(originalSwap, basePartyIsPayerOfTheFixed).amount;
                decimal modifiedSwapFV = SwapHelper.GetFutureValue(generateSwapStructure.Swap, basePartyIsPayerOfTheFixed).amount;
                Assert.IsTrue(modifiedSwapFV < originalSwapFV);
            }
            else
            {
                decimal originalSwapPV = SwapHelper.GetPresentValue(originalSwap, basePartyIsPayerOfTheFixed).amount;
                decimal modifiedSwapPV = SwapHelper.GetPresentValue(generateSwapStructure.Swap, basePartyIsPayerOfTheFixed).amount;
                Assert.IsTrue(modifiedSwapPV > originalSwapPV);
                decimal originalSwapFV = SwapHelper.GetFutureValue(originalSwap, basePartyIsPayerOfTheFixed).amount;
                decimal modifiedSwapFV = SwapHelper.GetFutureValue(generateSwapStructure.Swap, basePartyIsPayerOfTheFixed).amount;
                Assert.IsTrue(modifiedSwapFV > originalSwapFV);
            }
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }

        public void HelperGenerateSwapAddPrincipalExchangePayments()
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            fixedStream.principalExchanges.initialExchange =
                fixedStream.principalExchanges.finalExchange =
                fixedStream.principalExchanges.intermediateExchange = true;
            floatingStream.principalExchanges.initialExchange =
                floatingStream.principalExchanges.finalExchange =
                floatingStream.principalExchanges.intermediateExchange = true;
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            decimal originalSwapPV = SwapHelper.GetPresentValue(originalSwap, fixedStream.payerPartyReference.href).amount;
            decimal modifiedSwapPV = SwapHelper.GetPresentValue(generateSwapStructure.Swap, fixedStream.payerPartyReference.href).amount;
            Assert.AreEqual((double)modifiedSwapPV, (double)originalSwapPV);//comparison with a double precision
            decimal originalSwapFV = SwapHelper.GetFutureValue(originalSwap, fixedStream.payerPartyReference.href).amount;
            decimal modifiedSwapFV = SwapHelper.GetFutureValue(generateSwapStructure.Swap, fixedStream.payerPartyReference.href).amount;
            Assert.AreEqual((double)modifiedSwapFV, (double)originalSwapFV);//comparison with a double precision
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }

        #endregion

        #region Update Spreads

        [TestMethod]
        public void GenerateSwapUpdateSpreadFixedRateNotinalInFirstTwoCFs()
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            // Update spread in a first two cashflows
            // 
            SetSpread(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[0], 0.0002m);
            SetSpread(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[1], 0.0003m);
            // Update notional 
            //
            SetNotional(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[0], 1000000);
            SetNotional(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[1], 900000);
            SetNotional(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[2], 500000);
            SetNotional(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[3], 1500000);
            SetNotional(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[0], 1000001);
            SetNotional(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[1], 900001);
            SetNotional(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[2], 500001);
            SetNotional(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[3], 1500001);
            // Update fixed rate
            //
            SetFixedRate(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[0], 0.071m);
            SetFixedRate(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[1], 0.072m);
            // Update amounts - this call should not change the manually update values
            //
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            // Update fixed stream
            //
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(generateSwapStructure.payFixedStream, payStreamDiscountingCurve, generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(generateSwapStructure.PayFixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(generateSwapStructure.ReceiveFloatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            // Check spread
            //
            Assert.AreEqual(GetSpread(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[0]), 0.0002m);
            Assert.AreEqual(GetSpread(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[1]), 0.0003m);
            // Check notional
            //
            Assert.AreEqual(GetNotional(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[0]), 1000000);
            Assert.AreEqual(GetNotional(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[1]), 900000);
            Assert.AreEqual(GetNotional(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[2]), 500000);
            Assert.AreEqual(GetNotional(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[3]), 1500000);
            Assert.AreEqual(GetNotional(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[0]), 1000001);
            Assert.AreEqual(GetNotional(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[1]), 900001);
            Assert.AreEqual(GetNotional(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[2]), 500001);
            Assert.AreEqual(GetNotional(generateSwapStructure.ReceiveFloatingStream.cashflows.paymentCalculationPeriod[3]), 1500001);
            // Check fixed rate
            //
            Assert.AreEqual(GetFixedRate(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[0]), 0.071m);
            Assert.AreEqual(GetFixedRate(generateSwapStructure.PayFixedStream.cashflows.paymentCalculationPeriod[1]), 0.072m);
            //Debug.WriteLine(XmlSerializerHelper.SerializeToString(generateSwapStructure.swap));
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }

        #endregion

        #region Increase/Decrease notional on both streams

        [TestMethod]
        public void GenerateSwapChangeNotionalBothStreamsFirstTwoCFs()
        {
            foreach (decimal changeInNotional in new[] { 1.1m, 0.9m })
            {
                HelperGenerateSwapChangeNotionalBothStreamsFirstTwoCFs(changeInNotional);
            }
        }

        public void HelperGenerateSwapChangeNotionalBothStreamsFirstTwoCFs(decimal change)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            // Increase notional (both streams)
            //
            decimal newFixedStreamNotional = generateSwapStructure.PayLeg.NotionalAmount * change;
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            SetNotional(fixedStream.cashflows.paymentCalculationPeriod[0], newFixedStreamNotional);
            SetNotional(fixedStream.cashflows.paymentCalculationPeriod[1], newFixedStreamNotional);
            decimal newFloatStreamNotional = generateSwapStructure.ReceiveLeg.NotionalAmount * change;
            SetNotional(floatingStream.cashflows.paymentCalculationPeriod[0], newFloatStreamNotional);
            SetNotional(floatingStream.cashflows.paymentCalculationPeriod[1], newFloatStreamNotional);
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            // Check notional
            //
            Assert.AreEqual(GetNotional(fixedStream.cashflows.paymentCalculationPeriod[0]), newFixedStreamNotional);
            Assert.AreEqual(GetNotional(fixedStream.cashflows.paymentCalculationPeriod[1]), newFixedStreamNotional);
            Assert.AreEqual(GetNotional(floatingStream.cashflows.paymentCalculationPeriod[0]), newFloatStreamNotional);
            Assert.AreEqual(GetNotional(floatingStream.cashflows.paymentCalculationPeriod[1]), newFloatStreamNotional);
            if (change > 1)
            {
                AssertAmountsAreIncreased(originalSwap, generateSwapStructure.Swap);
            }
            else
            {
                AssertAmountsAreDecreased(originalSwap, generateSwapStructure.Swap);
            }
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }


        #endregion

        #region Change payment days on both streams

        [TestMethod]
        public void GenerateSwapChangePaymentDateBothStreamsFirstTwoCFs()
        {
            foreach (int movePaymentDate in new[] { 10, -10, 5, -5 })
            {
                HelperGenerateSwapChangePaymentDateBothStreamsFirstTwoCFs(movePaymentDate);
            }
        }

        public void HelperGenerateSwapChangePaymentDateBothStreamsFirstTwoCFs(int days)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            //fixedStream.cashflows.principalExchange[0].
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            //  Move payments days further or closer
            //
            foreach (InterestRateStream streamToUpdate in new[] { fixedStream, floatingStream })
            {
                streamToUpdate.cashflows.paymentCalculationPeriod[0].adjustedPaymentDate = streamToUpdate.cashflows.paymentCalculationPeriod[0].adjustedPaymentDate.AddDays(days);
                streamToUpdate.cashflows.paymentCalculationPeriod[1].adjustedPaymentDate = streamToUpdate.cashflows.paymentCalculationPeriod[1].adjustedPaymentDate.AddDays(days);
            }
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            if (days > 1)//increase PV and FV
            {
                AssertPVOfStreamsDecreased(originalSwap, generateSwapStructure.Swap);
            }
            else
            {
                AssertPVOfStreamsIncreased(originalSwap, generateSwapStructure.Swap);
            }
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }

        #endregion

        #region Change roll dates
        [TestMethod]
        public void GenerateSwapChangeRollDatesBothStreamsFirstTwoCFs()
        {
            foreach (int movePaymentDate in new[] { 10, -10, 5, -5 })
            {
                HelperGenerateSwapChangeRollDatesBothStreamsFirstTwoCFs(movePaymentDate, new[] { 0, 1 });
                HelperGenerateSwapChangeRollDatesBothStreamsFirstTwoCFs(movePaymentDate, new[] { 8, 9 });
            }
        }

        public void HelperGenerateSwapChangeRollDatesBothStreamsFirstTwoCFs(int days, int[] periodNumbers)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            //  Move payments days further or closer
            //
            foreach (InterestRateStream streamToUpdate in new[] { fixedStream, floatingStream })
            {
                foreach (int periodNumber in periodNumbers)
                {
                    CalculationPeriod calculationPeriod = PaymentCalculationPeriodHelper.GetFirstCalculationPeriod(streamToUpdate.cashflows.paymentCalculationPeriod[periodNumber]);
                    calculationPeriod.adjustedEndDate = calculationPeriod.adjustedEndDate.AddDays(days);
                }
            }
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            //  change in accrual periods should affect FV of the swap
            //
            string payerOfSwap = fixedStream.payerPartyReference.href;

            decimal originalSwapFV = SwapHelper.GetFutureValue(originalSwap, payerOfSwap).amount;
            decimal modifiedSwapFV = SwapHelper.GetFutureValue(generateSwapStructure.Swap, payerOfSwap).amount;
            Assert.AreNotEqual(modifiedSwapFV, originalSwapFV);
            //            if (days > 1)//increase PV and FV
            //            {
            //                AssertPVOfStreamsDecreased(originalSwap, generateSwapStructure.swap);
            //            }
            //            else
            //            {
            //                AssertPVOfStreamsIncreased(originalSwap, generateSwapStructure.swap);
            //            }

            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }

        #endregion

        #region Remove cashflows

        [TestMethod]
        public void GenerateSwapRemoveCashflowsFromOneStream()
        {
            foreach (int numberOfCashflowsToRemove in new[] { 1, 2, 3, 4, 5, 9 })
            {
                foreach (bool fromTheStart in new[] { true, false })
                {
                    foreach (bool paySide in new[] { true, false })
                    {
                        HelperGenerateSwapRemoveCashflowsFromOneStreamStreams(numberOfCashflowsToRemove, fromTheStart, paySide);
                    }
                }
            }
        }

        public void HelperGenerateSwapRemoveCashflowsFromOneStreamStreams(int numberOfCashflows, bool fromTheStart, bool paySide)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            InterestRateStream streamToModify = paySide ? fixedStream : floatingStream;
            var listOfCashflowsToModify = new List<PaymentCalculationPeriod>(streamToModify.cashflows.paymentCalculationPeriod);
            while (numberOfCashflows-- > 0)
            {
                if (fromTheStart)
                {
                    listOfCashflowsToModify.RemoveAt(0);
                }
                else
                {
                    listOfCashflowsToModify.RemoveAt(listOfCashflowsToModify.Count - 1);
                }
            }
            streamToModify.cashflows.paymentCalculationPeriod = listOfCashflowsToModify.ToArray();
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //            FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            if (paySide)//increase PV and FV
            {
                AssertPVAndFVIncreased(originalSwap, generateSwapStructure.Swap);
            }
            else
            {
                AssertPVAndFVDecreased(originalSwap, generateSwapStructure.Swap);
            }
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }

        #endregion

        #region Add one cashflow

        [TestMethod]
        public void GenerateSwapAddOneCashflowsAtEndOfOneStreamStreams()
        {
            foreach (bool paySide in new[] { true, false })
            {
                HelperGenerateSwapAddOneCashflowsAtEndOfOneStreamStreams(paySide);
            }
        }

        public void HelperGenerateSwapAddOneCashflowsAtEndOfOneStreamStreams(bool paySide)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            InterestRateStream streamToModify = paySide ? fixedStream : floatingStream;
            var listOfCashflowsToModify = new List<PaymentCalculationPeriod>(streamToModify.cashflows.paymentCalculationPeriod);
            PaymentCalculationPeriod lastCashflow = listOfCashflowsToModify[listOfCashflowsToModify.Count - 1];
            PaymentCalculationPeriod additionalCashflow = BinarySerializerHelper.Clone(lastCashflow);
            // updateStart & End of the period & payment dates + roll period length.
            //
            const int monthsToAdd = 6;
            additionalCashflow.adjustedPaymentDate = additionalCashflow.adjustedPaymentDate.AddMonths(monthsToAdd);
            CalculationPeriod calculationPeriod = PaymentCalculationPeriodHelper.GetCalculationPeriods(additionalCashflow)[0];
            calculationPeriod.adjustedEndDate = calculationPeriod.adjustedEndDate.AddMonths(monthsToAdd);
            calculationPeriod.adjustedStartDate = calculationPeriod.adjustedStartDate.AddMonths(monthsToAdd);
            calculationPeriod.unadjustedEndDate = calculationPeriod.unadjustedEndDate.AddMonths(monthsToAdd);
            calculationPeriod.unadjustedStartDate = calculationPeriod.unadjustedStartDate.AddMonths(monthsToAdd);
            //  add one more cashflow
            //
            listOfCashflowsToModify.Add(additionalCashflow);
            streamToModify.cashflows.paymentCalculationPeriod = listOfCashflowsToModify.ToArray();
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);

            if (paySide)
            {
                AssertPVAndFVDecreased(originalSwap, generateSwapStructure.Swap);
            }
            else
            {
                AssertPVAndFVIncreased(originalSwap, generateSwapStructure.Swap);
            }
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }


        #endregion

        #region Increase/Decrease fixed rate

        [TestMethod]
        public void GenerateSwapChangeFixedRateFixedPayStreamFirstTwoCFs()
        {
            foreach (decimal changeOfRate in new[] { 0.005m, -0.005m })
            {
                HelperGenerateSwapModifyRateFixedPayStreamFirstTwoCFs(changeOfRate);//50bp

            }
        }

        public void HelperGenerateSwapModifyRateFixedPayStreamFirstTwoCFs(decimal change)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            // Increase fixed rate (pay stream)
            //
            decimal newFixedRate = generateSwapStructure.PayLeg.CouponOrLastResetRate + change;
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            SetFixedRate(fixedStream.cashflows.paymentCalculationPeriod[0], newFixedRate);
            SetFixedRate(fixedStream.cashflows.paymentCalculationPeriod[1], newFixedRate);
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            // Check rate
            //
            Assert.AreEqual(newFixedRate, GetFixedRate(fixedStream.cashflows.paymentCalculationPeriod[0]));
            Assert.AreEqual(newFixedRate, GetFixedRate(fixedStream.cashflows.paymentCalculationPeriod[1]));
            if (change > 0)
            {
                AssertPVAndFVDecreased(originalSwap, generateSwapStructure.Swap);
            }
            else
            {
                AssertPVAndFVIncreased(originalSwap, generateSwapStructure.Swap);
            }
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }


        [TestMethod]
        public void GenerateSwapAddPrincipalExchangePayments()
        {
            HelperGenerateSwapAddPrincipalExchangePayments();
        }


        #endregion

        #region Increase/Decrease floating prev. calculated rate

        [TestMethod]
        public void GenerateSwapModifyRateFloatReceiveStreamFirstTwoCFs()
        {
            foreach (decimal change in new[] { -0.005m, 0.005m })
            {
                HelperGenerateSwapModifyRateFloatReceiveStreamFirstTwoCFs(change);
            }
        }

        public void HelperGenerateSwapModifyRateFloatReceiveStreamFirstTwoCFs(decimal change)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();
            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);
            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;
            //decimal newFloatRate = GetFloatRate(floatingStream.cashflows.paymentCalculationPeriod[0]) + change;
            SetSpread(floatingStream.cashflows.paymentCalculationPeriod[0], change);
            SetSpread(floatingStream.cashflows.paymentCalculationPeriod[1], change);
            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);
            // Check rate
            //
            var floatRate1 = GetSpread(floatingStream.cashflows.paymentCalculationPeriod[0]);
            Assert.AreEqual(change, floatRate1);
            var floatRate2 = GetSpread(floatingStream.cashflows.paymentCalculationPeriod[1]);
            Assert.AreEqual(change, floatRate2);
            if (change > 0)
            {
                AssertPVAndFVIncreased(originalSwap, generateSwapStructure.Swap);
            }
            else
            {
                AssertPVAndFVDecreased(originalSwap, generateSwapStructure.Swap);
            }
            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }

        #endregion

        #region Add Payment

        [TestMethod]
        public void GenerateSwapAddBulletPayments()
        {
            foreach (bool payerPaysAdditional in new[] { true, false })
            {
                foreach (decimal paymentAmount in new[] { 1m, 2m, 100000m, 500000m })
                {
                    foreach (DateTime paymentDate in new[] { DateTime.Today, DateTime.Today.AddYears(3) })
                    {
                        var amount = new NonNegativeMoney { amount = paymentAmount, currency = CurrencyHelper.Parse("AUD") };
                        HelperGenerateSwapAddBulletPayments(payerPaysAdditional, amount, paymentDate);
                    }
                }
            }
        }

        #endregion

        #region Increase/Decrease float spread

        [TestMethod]
        public void GenerateSwapChangeSpreadFloatingReceiveStreamFirstTwoCFs()
        {
            foreach (decimal spreadChange in new[] { 0.0010m, -0.0010m })
            {
                HelperGenerateSwapModifySpreadFloatingReceiveStreamFirstTwoCFs(spreadChange);
            }

        }

        private void HelperGenerateSwapModifySpreadFloatingReceiveStreamFirstTwoCFs(decimal change)
        {
            GenerateSwapStructure generateSwapStructure = GenerateSwap();

            Swap originalSwap = BinarySerializerHelper.Clone(generateSwapStructure.Swap);

            // Increase spread (receive stream)
            //
            decimal newFloatingRateSpread = generateSwapStructure.ReceiveLeg.FloatingRateSpread + change;

            InterestRateStream fixedStream = generateSwapStructure.PayFixedStream;
            InterestRateStream floatingStream = generateSwapStructure.ReceiveFloatingStream;


            SetSpread(floatingStream.cashflows.paymentCalculationPeriod[0], newFloatingRateSpread);
            SetSpread(floatingStream.cashflows.paymentCalculationPeriod[1], newFloatingRateSpread);

            // Update fixed stream
            IRateCurve payStreamDiscountingCurve = ((SwapLegEnvironment)MarketEnvironmentRegistry.Get(generateSwapStructure.MarketEnvironmentId)).GetDiscountRateCurve();

            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, MarketEnvironmentRegistry.Get(generateSwapStructure.marketEnvironmentId).GetRateCurve(generateSwapStructure.curveId), generateSwapStructure.valuationDate);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, generateSwapStructure.ValuationDate);

            IRateCurve receiveStreamForecastCurve = payStreamDiscountingCurve;
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;
            // Update float stream
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, generateSwapStructure.ValuationDate);

            // Check rate
            //
            Assert.AreEqual(newFloatingRateSpread, GetSpread(floatingStream.cashflows.paymentCalculationPeriod[0]));
            Assert.AreEqual(newFloatingRateSpread, GetSpread(floatingStream.cashflows.paymentCalculationPeriod[1]));

            if (change > 0)
            {
                AssertPVAndFVIncreased(originalSwap, generateSwapStructure.Swap);
            }
            else
            {
                AssertPVAndFVDecreased(originalSwap, generateSwapStructure.Swap);
            }


            //  clean up market environment
            //
            CleanUpMarketEnvironment(generateSwapStructure.MarketEnvironmentId);
        }



        #endregion

        #region Cash Flows

        [TestMethod]
        public void GenerateSwapChangeTwoFloatCashflowsIntoFixed()
        {
            foreach (decimal rateChange in new[] { 0.01m, -0.01m, 0.001m, -0.001m })
            {
                HelperGenerateSwapChangeTwoFloatCashflowsIntoFixed(rateChange);
            }
        }

        #endregion

        #endregion

        #region Test Swap Parametric Representation

        #region Methods

        public static SwapExcelParameters GetSwapInputParametesForVanillaSwap(string discountingType)
        {
            var fixedLeg = new SwapLegParametersRange_Old
            {
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 50000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.058m,
                PaymentFrequency = "1Y",
                DayCount = "30E/360",
                PaymentCalendar = "Sydney",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "London",
                FixingBusinessDayAdjustments = "FOLLOWING",
                DiscountCurve = "AUD-LIBOR-BBA.3M",
                ForecastCurve = "AUD-LIBOR-BBA.3M",
                DiscountingType = discountingType
            };


            var floatingLeg = new SwapLegParametersRange_Old
            {
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 50000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                PaymentFrequency = "3M",
                DayCount = "ACT/360",
                PaymentCalendar = "Sydney",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "London",
                FixingBusinessDayAdjustments = "FOLLOWING",
                DiscountCurve = "AUD-LIBOR-BBA.3M",
                ForecastCurve = "AUD-LIBOR-BBA.3M",
                ForecastIndexName = "AUD-LIBOR-BBA",
                DiscountingType = discountingType
            };

            var swapExcelParameters = new SwapExcelParameters(fixedLeg, floatingLeg);

            return swapExcelParameters;
        }


        private static IEnumerable<string> GetDiscountingTypes()
        {
            return new[] { "Standard", "FRA" };
        }

        #endregion

        #region Tests

        #region To list of properties

        [TestMethod]
        public void TestGenerateFixedStreamCashflowsToListOfProperties()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.PayLeg;
                legParametersRange.AdjustedType = AdjustedType.Unadjusted;

                InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                var listOfExcelObjects = new List<CashflowScheduleRangeItem>();

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);
                foreach (PaymentCalculationPeriod paymentCalculationPeriod in cashflows.paymentCalculationPeriod)
                {
                    CalculationPeriod calcPeriod = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod)[0];

                    var rateCashflowScheduleRangeItem = new CashflowScheduleRangeItem();
                    listOfExcelObjects.Add(rateCashflowScheduleRangeItem);

                    //rateCashflowScheduleRangeItem.Rate = XsdClassesFieldResolver.CalculationPeriod_GetFixedRate(calcPeriod);
                    //rateCashflowScheduleRangeItem.Notional = XsdClassesFieldResolver.CalculationPeriod_GetNotionalAmount(calcPeriod);
                    rateCashflowScheduleRangeItem.StartDate = calcPeriod.adjustedStartDate;
                    rateCashflowScheduleRangeItem.EndDate = calcPeriod.adjustedEndDate;
                    rateCashflowScheduleRangeItem.PaymentDate = paymentCalculationPeriod.adjustedPaymentDate;

                }

                Debug.WriteLine("Xml dump (with present/forecastvalue/discountfactor) cashflows");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));

#pragma warning disable 168
                object[,] listOfProperties = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(listOfExcelObjects);
#pragma warning restore 168
            }
        }

        [TestMethod]
        public void TestUpdateFixedStreamCashflowsToListOfProperties()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.PayLeg;
                legParametersRange.AdjustedType = AdjustedType.Unadjusted;
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
                var listOfExcelObjects = new List<CashflowScheduleRangeItem>();
                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);
                foreach (PaymentCalculationPeriod paymentCalculationPeriod in cashflows.paymentCalculationPeriod)
                {
                    CalculationPeriod calcPeriod =
                        XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(
                            paymentCalculationPeriod)[0];

                    var rateCashflowScheduleRangeItem = new CashflowScheduleRangeItem();
                    listOfExcelObjects.Add(rateCashflowScheduleRangeItem);
                    //rateCashflowScheduleRangeItem.Rate = XsdClassesFieldResolver.CalculationPeriod_GetFixedRate(calcPeriod);
                    //rateCashflowScheduleRangeItem.Notional = XsdClassesFieldResolver.CalculationPeriod_GetNotionalAmount(calcPeriod);
                    rateCashflowScheduleRangeItem.StartDate = calcPeriod.adjustedStartDate;
                    rateCashflowScheduleRangeItem.EndDate = calcPeriod.adjustedEndDate;
                    rateCashflowScheduleRangeItem.PaymentDate = paymentCalculationPeriod.adjustedPaymentDate;
                    //FixedRateCashflow cashflow = new FixedRateCashflow(paymentCalculationPeriod);
                    //Money couponAmount = cashflow.GetAmount();
                    //rateCashflowScheduleRangeItem.Amount = couponAmount.amount;
                }
                // Dump original cf
                //
                object[,] listOfProperties =
                    ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(listOfExcelObjects);
                // After the user have altered: 
                // "rate" of the first coupon and 
                // "payment day" of the second.
                //
                //listOfProperties[0, 0] = 0.04;
                listOfProperties[1, 0] = ((DateTime)listOfProperties[1, 0]).AddDays(5);
                List<CashflowScheduleRangeItem> listOfExcelObjects2 =
                    ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<CashflowScheduleRangeItem>(
                        listOfProperties);
                Cashflows cashflowsCopy = BinarySerializerHelper.Clone(cashflows);
                cashflowsCopy.cashflowsMatchParameters = false; //!more smarter! 
                foreach (CashflowScheduleRangeItem excelCFModified in listOfExcelObjects2)
                {
                    int cfIndex = listOfExcelObjects2.IndexOf(excelCFModified);
                    PaymentCalculationPeriod paymentCalculationPeriod = cashflowsCopy.paymentCalculationPeriod[cfIndex];
                    paymentCalculationPeriod.adjustedPaymentDate = excelCFModified.PaymentDate;
                    CalculationPeriod calcPeriod =
                        XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(
                            paymentCalculationPeriod)[0];
                    calcPeriod.adjustedStartDate = excelCFModified.StartDate;
                    calcPeriod.adjustedEndDate = excelCFModified.EndDate;
                    //                XsdClassesFieldResolver.SetCalculationPeriod_FixedRate(calcPeriod, excelCFModified.Rate);
                }
                Debug.WriteLine("Xml dump (altered) cashflows");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflowsCopy));
            }
        }

        #endregion

        #region Tests ParametricRepresentation of fixed stream

        [TestMethod]
        public void TestGenerateParametricRepresentationOfUnadjustedFixedStream()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.PayLeg;
                legParametersRange.AdjustedType = AdjustedType.Unadjusted;

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfAdjustedFixedStream()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.PayLeg;
                legParametersRange.AdjustedType = AdjustedType.Adjusted;

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }

        #endregion

        #region Tests ParametricRepresentation of floating stream

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFloatingStream()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFloatingStreamEomroll()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
                legParametersRange.EffectiveDate = new DateTime(1994, 12, 31);
                legParametersRange.FirstRegularPeriodStartDate = new DateTime(1995, 03, 31); //3m (float leg) later
                legParametersRange.MaturityDate = new DateTime(1999, 12, 31);
                legParametersRange.RollConvention = "EOM";
                legParametersRange.InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString();
                legParametersRange.FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString();

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFloatingStreamEomrollCashflows()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
                legParametersRange.EffectiveDate = new DateTime(1994, 12, 31);
                legParametersRange.FirstRegularPeriodStartDate = new DateTime(); //3m (float leg) later
                legParametersRange.MaturityDate = new DateTime(1999, 12, 31);
                legParametersRange.RollConvention = "EOM";

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
                Assert.AreEqual(20, cashflows.paymentCalculationPeriod.Length);
            }
        }
        //        [TestMethod]
        //        public void TestGenerateParametricRepresentationOf_FloatingStream_EOMROLL_Cashflows()
        //        {
        //            foreach (string discountingType in GetDiscountingTypes())
        //            {
        //                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
        //
        //                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
        //
        //
        //                legParametersRange.EffectiveDate = new DateTime(1994, 12, 31);
        //                legParametersRange.FirstRegularPeriodStartDate = new DateTime(1995, 03, 31); //3m (float leg) later
        //                legParametersRange.MaturityDate = new DateTime(1999, 12, 31);
        //
        //                legParametersRange.RollConvention = "EOM";
        //                legParametersRange.InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString();
        //                legParametersRange.FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString();
        //
        //
        //                InterestRateStream stream =
        //                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
        //
        //                Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
        //
        //                Debug.WriteLine("Xml dump:");
        //                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
        //                Assert.AreEqual(20, cashflows.paymentCalculationPeriod.Length);
        //            }
        //        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFloatingStreamEomrollInitialShortCashflows()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 01, 15);
                legParametersRange.FirstRegularPeriodStartDate = new DateTime(1995, 03, 31); //3m (float leg) later
                legParametersRange.MaturityDate = new DateTime(1999, 12, 31);
                legParametersRange.RollConvention = "EOM";

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
                Assert.AreEqual(20, cashflows.paymentCalculationPeriod.Length);
            }
        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFloatingStreamLongInitialStub()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 01, 14);
                legParametersRange.InitialStubType = StubPeriodTypeEnum.LongInitial.ToString();

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFloatingStreamShortInitialStub()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 01, 14);
                legParametersRange.InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString();

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }

        [TestMethod]
        public void TestGenerateFloatingStreamCashflows() // vanilla swap
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);
                Debug.WriteLine("Xml dump:");
                string serializeToString1 = XmlSerializerHelper.SerializeToString(cashflows);
                Debug.WriteLine(serializeToString1);
            }
        }

        [TestMethod]
        public void TestGenerateFloatingStreamLongInitialStubCashflows()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 01, 14);
                legParametersRange.InitialStubType = StubPeriodTypeEnum.LongInitial.ToString();

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
            }
        }

        [TestMethod]
        public void TestGenerateFloatingStreamShortInitialStubCashflows()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.ReceiveLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 01, 14);
                legParametersRange.InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString();

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
            }
        }

        #endregion

        #region Tests ParametricRepresentation of fixed stream

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFixedStreamShortInitial2()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap2 = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap2.PayLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 10, 14);
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }


        [TestMethod]
        public void TestGenerateFixedStreamCashflowsFixedStreamShortInitial2()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap2 = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap2.PayLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 10, 16);
                //legParametersRange.LastRegularPeriodEndDate = new DateTime(1999, 10, 14);
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);

                Assert.AreEqual(legParametersRange.EffectiveDate,
                                PaymentCalculationPeriodHelper.GetCalculationPeriodStartDate(
                                    cashflows.paymentCalculationPeriod[0]));

                Debug.WriteLine("Xml dump:");
                string serializeToString1 = XmlSerializerHelper.SerializeToString(cashflows);
                Debug.WriteLine(serializeToString1);
            }
        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFixedStream2()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap2 = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap2.PayLeg;
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }


        [TestMethod]
        public void TestGenerateFixedStreamCashflows()//fixed stream cashflows...
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap2 = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap2.PayLeg;
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);
                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
            }
        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFixedStreamLongInitialStub()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.PayLeg;
                legParametersRange.InitialStubType = StubPeriodTypeEnum.LongInitial.ToString();
                legParametersRange.EffectiveDate = new DateTime(1994, 11, 14);

                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }

        [TestMethod]
        public void TestGenerateParametricRepresentationOfFixedStreamShortInitialStub()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.PayLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 01, 14);
                legParametersRange.InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString();
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream));
            }
        }


        [TestMethod]
        public void TestGenerateFixedStreamLongInitialStubCashflows()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);

                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.PayLeg;
                legParametersRange.EffectiveDate = new DateTime(1994, 11, 14);
                //legParametersRange.InitialStubType = StubPeriodTypeEnum.LongInitial.ToString();
                legParametersRange.FirstRegularPeriodStartDate = new DateTime(1995, 12, 14);
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
                Assert.AreEqual(5, cashflows.paymentCalculationPeriod.Length);
            }
        }

        [TestMethod]
        public void TestGenerateFixedStreamShortInitialStubCashflows()
        {
            foreach (string discountingType in GetDiscountingTypes())
            {
                SwapExcelParameters excelParametesForVanillaSwap = GetSwapInputParametesForVanillaSwap(discountingType);
                SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap.PayLeg;
                legParametersRange.EffectiveDate = new DateTime(1995, 01, 14);
                legParametersRange.InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString();
                InterestRateStream stream =
                    InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

                //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
                Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);

                Debug.WriteLine("Xml dump:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
            }
        }

        #endregion

        #endregion

        #endregion

        #region Test Bill Swap Pricer

        #region Methods

        private List<BillSwapPricerDateNotional> GetData2(DateTime today)
        {
            var list = new List<BillSwapPricerDateNotional>();

            var billSwapPricerCashflowRow = new BillSwapPricerDateNotional {DateTime = today, MaturityValue = 500000};
            list.Add(billSwapPricerCashflowRow);
            var billSwapPricerCashflowRow2 = new BillSwapPricerDateNotional
                {
                    DateTime = today.AddMonths(3*1),
                    MaturityValue = 500000
                };
            list.Add(billSwapPricerCashflowRow2);
            var billSwapPricerCashflowRow3 = new BillSwapPricerDateNotional
                {
                    DateTime = today.AddMonths(3*2),
                    MaturityValue = 500000
                };
            list.Add(billSwapPricerCashflowRow3);
            return list;
        }

        #endregion

        #region Tests

        [TestMethod]
        public void BuildBillPricerDates()
        {
            DateTime today = DateTime.Today;
            var range = new BillSwapPricerDatesRange
            {
                StartDate = today,
                EndDate = today.AddYears(3),
                FirstRegularPeriodStartDate = today.AddMonths(3),
                RollConvention = ((today.Day == 31) ? "EOM" : today.Day.ToString(CultureInfo.InvariantCulture)),
                RollFrequency = "3m",
                Calendar = "AUSY",
                BusinessDayConvention = "MODFOLLOWING"
            };
            Debug.Print("Start Date: {0}", range.StartDate);
            Debug.Print("End Date: {0}", range.EndDate);
            Debug.Print("FirstRegularPeriodStartDate: {0}", range.FirstRegularPeriodStartDate);
            Debug.Print("RollConvention: {0}", range.RollConvention);
            Debug.Print("RollFrequency: {0}", range.RollFrequency);
            Debug.Print("Calendar: {0}", range.Calendar);
            Debug.Print("BusinessDayConvention: {0}", range.BusinessDayConvention);
            var billSwapPricer = new BillSwapPricer();
            List<DateTime> dates = billSwapPricer.BuildDates(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, range, null);
            foreach (DateTime date in dates)
            {
                Debug.Print("RollDate: {0}", date);
            }
        }

        [TestMethod]
        public void BuildDates2_3MFrequency()
        {
            var range = new BillSwapPricerDatesRange
            {
                StartDate = DateTime.Parse("31/01/2008 12:00:00 AM"),
                EndDate = DateTime.Parse("28/01/2011 12:00:00 AM"),
                FirstRegularPeriodStartDate = DateTime.Parse("30/04/2008 12:00:00 AM"),
                RollConvention = "30",
                RollFrequency = "3m",
                Calendar = "AUSY",
                BusinessDayConvention = "MODFOLLOWING"
            };
            //range.EndDate = DateTime.Parse("30/01/2011 12:00:00 AM"); - falls on Sat.
            Debug.Print("Start Date: {0}", range.StartDate);
            Debug.Print("End Date: {0}", range.EndDate);
            Debug.Print("FirstRegularPeriodStartDate: {0}", range.FirstRegularPeriodStartDate);
            Debug.Print("RollConvention: {0}", range.RollConvention);
            Debug.Print("RollFrequency: {0}", range.RollFrequency);
            Debug.Print("Calendar: {0}", range.Calendar);
            Debug.Print("BusinessDayConvention: {0}", range.BusinessDayConvention);
            var billSwapPricer = new BillSwapPricer();
            List<DateTime> dates = billSwapPricer.BuildDates(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, range, null);
            foreach (DateTime date in dates)
            {
                Debug.Print("RollDate: {0}", date);
            }
        }

        [TestMethod]
        public void BuildDates390DaysFrequency()
        {
            var range = new BillSwapPricerDatesRange
                {
                    StartDate = DateTime.Parse("31/01/2008 12:00:00 AM"),
                    EndDate = DateTime.Parse("28/01/2011 12:00:00 AM"),
                    FirstRegularPeriodStartDate = DateTime.Parse("30/04/2008 12:00:00 AM"),
                    RollConvention = "30",
                    RollFrequency = "90d",
                    Calendar = "AUSY",
                    BusinessDayConvention = "MODFOLLOWING"
                };
            //range.EndDate = DateTime.Parse("30/01/2011 12:00:00 AM"); - fails on w/e
            Debug.Print("Start Date: {0}", range.StartDate);
            Debug.Print("End Date: {0}", range.EndDate);
            Debug.Print("FirstRegularPeriodStartDate: {0}", range.FirstRegularPeriodStartDate);
            Debug.Print("RollConvention: {0}", range.RollConvention);
            Debug.Print("RollFrequency: {0}", range.RollFrequency);
            Debug.Print("Calendar: {0}", range.Calendar);
            Debug.Print("BusinessDayConvention: {0}", range.BusinessDayConvention);
            var billSwapPricer = new BillSwapPricer();
            List<DateTime> dates = billSwapPricer.BuildDates(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, range, PaymentCalendar);
            foreach (DateTime date in dates)
            {
                Debug.Print("RollDate: {0}", date);
            }
        }

        [TestMethod]
        public void PopulateForwardRates()
        {
            foreach (string dayCount in new[] { "ACT/360", "ACT/365.FIXED" })
            {
                DateTime today = DateTime.Today;
                var rc = CreateInterestRateStreamTestEnvironment(today);//.CreateAudLiborBba3MonthFlat8PercentCurve(today, today);
                var curve = (RateCurve)rc.GetDiscountRateCurve();
                var billSwapPricer = new BillSwapPricer();
                List<BillSwapPricerDateNotional> data1 = GetData2(today);
                IDayCounter dayCounter = DayCounterHelper.Parse(dayCount);
                List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(data1, dayCounter, curve);
                object[,] array = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(forwardRates);
                Debug.Print("With forward rates: (DayCount = {0})\n\r", dayCount);
                Debug.Print(ParameterFormatter.FormatObject(array));
            }
        }

        [TestMethod]
        public void PopulatePurchaseCosts()
        {
            foreach (string dayCount in new[] { "ACT/360", "ACT/365.FIXED" })
            {
                DateTime today = DateTime.Today;
                var rc = CreateInterestRateStreamTestEnvironment(today);//.CreateAudLiborBba3MonthFlat8PercentCurve(today, today);
                var curve = (RateCurve)rc.GetDiscountRateCurve();
                var billSwapPricer = new BillSwapPricer();
                List<BillSwapPricerDateNotional> data1 = GetData2(today);
                IDayCounter dayCounter = DayCounterHelper.Parse(dayCount);
                List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(data1, dayCounter, curve);
                List<BillSwapPricerCashflowRow> purchasePrices = billSwapPricer.PopulatePurchaseCosts(forwardRates, dayCounter, curve);
                object[,] array = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(purchasePrices);
                Debug.Print("With purchasePrices: (DayCount = {0})\n\r", dayCount);
                Debug.Print(ParameterFormatter.FormatObject(array));
            }
        }

        [TestMethod]
        public void SimpleYield()
        {
            foreach (string dayCount in new[] { "ACT/360", "ACT/365.FIXED" })
            {
                DateTime today = DateTime.Today;
                var rc = CreateInterestRateStreamTestEnvironment(today);//.CreateAudLiborBba3MonthFlat8PercentCurve(today, today);
                var curve = (RateCurve)rc.GetDiscountRateCurve();
                var billSwapPricer = new BillSwapPricer();
                List<BillSwapPricerDateNotional> data1 = GetData2(today);
                IDayCounter dayCounter = DayCounterHelper.Parse(dayCount);
                List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(data1, dayCounter, curve);
                double simpleYield = billSwapPricer.GetSimpleYield(forwardRates, dayCounter, curve);
                Debug.Print("Simple (over a period) yield : {0}, DayCount({1})", simpleYield, dayCount);
            }
        }

        [TestMethod]
        public void AnnuallyCompoundingYield()
        {
            foreach (string dayCount in new[] { "ACT/360", "ACT/365.FIXED" })
            {
                DateTime today = DateTime.Today;
                var rc = CreateInterestRateStreamTestEnvironment(today);//.CreateAudLiborBba3MonthFlat8PercentCurve(today, today);
                var curve = (RateCurve)rc.GetDiscountRateCurve();
                var billSwapPricer = new BillSwapPricer();
                List<BillSwapPricerDateNotional> data1 = GetData2(today);
                IDayCounter dayCounter = DayCounterHelper.Parse(dayCount);
                List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(data1, dayCounter, curve);
                double annYield = billSwapPricer.GetAnnualYield(forwardRates, dayCounter, curve);
                Debug.Print("Ann yield : {0} DayCount({1})", annYield, dayCount);
            }
        }

        [TestMethod]
        public void SettlementValue()
        {
            foreach (string dayCount in new[] { "ACT/360", "ACT/365.FIXED" })
            {
                DateTime today = DateTime.Today;
                var rc = CreateInterestRateStreamTestEnvironment(today);//.CreateAudLiborBba3MonthFlat8PercentCurve(today, today);
                var curve = (RateCurve)rc.GetDiscountRateCurve();
                var billSwapPricer = new BillSwapPricer();
                List<BillSwapPricerDateNotional> data1 = GetData2(today);
                IDayCounter dayCounter = DayCounterHelper.Parse(dayCount);
                List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(data1, dayCounter, curve);
                List<BillSwapPricerCashflowRow> purchasePrices = billSwapPricer.PopulatePurchaseCosts(forwardRates, dayCounter, curve);
                double yearFractionOverWholePeriod = dayCounter.YearFraction(today, purchasePrices[purchasePrices.Count - 1].DateTime);
                double annYield = billSwapPricer.GetSimpleYield(purchasePrices, dayCounter, curve);
                double settValue = 500000 / (Math.Pow(annYield, yearFractionOverWholePeriod));
                Debug.Print("Settlement value : {0}, DayCount : {1}", settValue, dayCount);
            }
        }

        #endregion

        #endregion

        #region Test Bill Swap Pricer2

        //[TestMethod]
        public List<DateTime> GetAdjustedDates2Backward()
        {
            var startDate = new DateTime(2008, 04, 10);
            var endDate = new DateTime(2009, 04, 24);
            Period interval = IntervalHelper.FromMonths(3);
            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(endDate.Day.ToString(CultureInfo.InvariantCulture));
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("MODFOLLOWING", "AUSY");
            List<DateTime> unadjustedDates = AdjustedDatesMetaSchedule.GetAdjustedDates2(startDate, endDate, interval, rollConventionEnum, true, businessDayAdjustments, PaymentCalendar);

            return unadjustedDates;
            //PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void ConvertActualToEffectiveRate()
        {
            const double actualRate = 0.08;
            const double actualRateFrequency = 0.5;
            const double freq12M = 1.0;
            const double freq6M = 0.5;
            const double freq4M = 1.0 / 3.0;
            const double freq3M = 1.0 / 4.0;
            const double freq1M = 1.0 / 12.0;

            Debug.Print("Actual rate: {0}, Actual rate freq : {1}", actualRate, actualRateFrequency);
            Debug.Print("freq12M(1Y)(calculated): {0}", BillsSwapPricer2.ConvertActualToEffectiveRate(actualRate, actualRateFrequency, freq12M));
            Debug.Print("freq6M(calculated): {0}", BillsSwapPricer2.ConvertActualToEffectiveRate(actualRate, actualRateFrequency, freq6M));
            Debug.Print("freq4M(calculated): {0}", BillsSwapPricer2.ConvertActualToEffectiveRate(actualRate, actualRateFrequency, freq4M));
            Debug.Print("freq3M(calculated): {0}", BillsSwapPricer2.ConvertActualToEffectiveRate(actualRate, actualRateFrequency, freq3M));
            Debug.Print("freq1M(calculated): {0}", BillsSwapPricer2.ConvertActualToEffectiveRate(actualRate, actualRateFrequency, freq1M));
        }

        [TestMethod]
        public void TestBillSwapPricer2()
        {
            List<DateTime> dates2Backward = GetAdjustedDates2Backward();
            dates2Backward.RemoveAt(0);
            var date2BackwardWithWasRolled = dates2Backward.Select(time => new Pair<DateTime, string>(time, "No")).ToList();
            const double initialNotional = 500000;
            var amortSchedule = new List<AmortScheduleItem>();
            var item = new AmortScheduleItem
                {
                    StartDate = dates2Backward[0],
                    EndDate = dates2Backward[1],
                    ApplyEveryNthRoll = 1,
                    AmortAmount = -100000
                };
            amortSchedule.Add(item);
            var item2 = new AmortScheduleItem
                {
                    StartDate = dates2Backward[2],
                    EndDate = dates2Backward[3],
                    ApplyEveryNthRoll = 1,
                    AmortAmount = 50000
                };
            amortSchedule.Add(item2);
            var amortizationSchedule = BillsSwapPricer2.GetAmortizationSchedule(date2BackwardWithWasRolled, initialNotional, amortSchedule);
            object[,] arrayRange = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(amortizationSchedule);

            Debug.Print(ParameterFormatter.FormatObject(arrayRange));
        }

        #endregion

        #region Test Swaption Parameteric Representation

        [TestMethod]
        public void SwaptionTestGenerateFixedStreamCashflows()
        {
            SwapExcelParameters excelParametesForVanillaSwap2 = GetSwapInputParametesForVanillaSwap("None");
            SwapLegParametersRange_Old legParametersRange = excelParametesForVanillaSwap2.PayLeg;
            var swaptionTerms = new SwaptionParametersRange
                                    {
                                        StrikeRate = legParametersRange.CouponOrLastResetRate,
                                        Volatility = 0.2m
                                    };

            InterestRateStream stream =
                InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
       
            DateTime curveRefDate = legParametersRange.EffectiveDate;
            //RateCurve rateCurve = RateCurveTests.CreateAUD_LIBOR_BBA_3MonthFlat8PercentCurve_WithBankBillFutures(curveRefDate, curveRefDate, "AUD-LIBOR-BBA");
            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(curveRefDate);

            Guid marketEnvironmentId = Guid.NewGuid();
            marketEnvironment.Id = marketEnvironmentId.ToString();
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);

            DateTime valuationDt = legParametersRange.EffectiveDate;

//            stream.cashflows = FixedRateStreamCashflowGenerator.GetCashflows(stream);
//            FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(stream, rateCurve, valuationDT);
            stream.cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(stream, null, marketEnvironment.GetDiscountRateCurve(), valuationDt);
            MarketEnvironmentRegistry.Remove(marketEnvironmentId.ToString());
            Money fv = CashflowsHelper.GetForecastValue(stream.cashflows);
            Money pv = CashflowsHelper.GetPresentValue(stream.cashflows);

            Debug.Print("Future value :{0}", fv.amount);
            Debug.Print("Present value :{0}", pv.amount);
           
            // get swaption price
            //
            double coeff = BlackModel.GetSwaptionValue((double)legParametersRange.CouponOrLastResetRate, (double)swaptionTerms.StrikeRate, (double)swaptionTerms.Volatility, 0.25);//3m to expiry

            Debug.Print("3m Swaption price :{0}", (double)pv.amount * coeff);
            Debug.WriteLine("Xml dump:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(stream.cashflows));
        }     

        #endregion

        #region Test CalculationPeriod Generator

        private static void InfoToDebug(CalculationPeriodsPrincipalExchangesAndStubs periods)
        {
            int numberOfPeriods = periods.CalculationPeriods.Count;
            if (null != periods.InitialStubCalculationPeriod)
            {
                ++numberOfPeriods;
            }
            if (null != periods.FinalStubCalculationPeriod)
            {
                ++numberOfPeriods;
            }
            Trace.WriteLine($"Number of periods : {numberOfPeriods}");
            if (null != periods.InitialStubCalculationPeriod)
            {
                Trace.WriteLine(String.Format("Initial Stub"));
            }
            if (null != periods.FinalStubCalculationPeriod)
            {
                Trace.WriteLine(String.Format("Final Stub"));
            }

            Debug.WriteLine("Xml dump:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(periods));
        }

        [TestMethod]
        public void TestVanila()
        {
            var startDate = new DateTime(2007, 01, 5);
            var endDate = new DateTime(2008, 01, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"), 
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.ShortFinal
                , PaymentCalendar);

            InfoToDebug(periods);
        }

        [TestMethod]
        public void TestVanilaRollDayEom()
        {
            var startDate = new DateTime(2007, 01, 31);
            var endDate = new DateTime(2008, 01, 31);
            var firstRollDate = new DateTime(2007, 04, 30);
            const RollConventionEnum rollConvention = RollConventionEnum.EOM;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.ShortFinal, PaymentCalendar);

            InfoToDebug(periods);          //            Assert.AreEqual(4, periods.Count);
        }

        [TestMethod]
        public void TestVanilaStartEndRollDayDifferentEomShortInitialShortFinal()
        {
            var startDate = new DateTime(2007, 01, 4);
            var endDate = new DateTime(2008, 01, 4);
            var firstRollDate = new DateTime(2007, 04, 30);
            const RollConventionEnum rollConvention = RollConventionEnum.EOM;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.ShortFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(5, periods.Count);
        }

        [TestMethod]
        public void TestVanillaStartEndRollDayDifferentShortInitialShortFinal()
        {
            var startDate = new DateTime(2007, 01, 5);
            var endDate = new DateTime(2008, 01, 5);
            var firstRollDate = new DateTime(2007, 04, 15);
            const RollConventionEnum rollConvention = RollConventionEnum.Item15;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.ShortFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(5, periods.Count);
        }

        [TestMethod]
        public void TestVanillaStartEndRollDayDifferentLongInitialLongFinal()
        {
            var startDate = new DateTime(2007, 01, 5);
            var endDate = new DateTime(2008, 01, 5);
            var firstRollDate = new DateTime(2007, 04, 15);
            const RollConventionEnum rollConvention = RollConventionEnum.Item15;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.LongInitial, StubPeriodTypeEnum.LongFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(3, periods.Count);
        }

        [TestMethod]
        public void TestShortFinal()
        {
            var startDate = new DateTime(2007, 01, 5);
            var endDate = new DateTime(2007, 12, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.ShortFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(4, periods.Count);
        }

        [TestMethod]
        public void TestLongFinal()
        {
            var startDate = new DateTime(2007, 01, 5);
            var endDate = new DateTime(2008, 02, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.LongFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(4, periods.Count);
        }

        [TestMethod]
        public void TestShortInitial()
        {
            var startDate = new DateTime(2007, 02, 5);
            var endDate = new DateTime(2008, 01, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.ShortFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(4, periods.Count);
        }

        [TestMethod]
        public void TestShortInitialShortFinal()
        {
            var startDate = new DateTime(2007, 02, 5);
            var endDate = new DateTime(2007, 12, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.ShortFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(4, periods.Count);
        }

        [TestMethod]
        public void TestShortInitialLongFinal()
        {
            var startDate = new DateTime(2007, 02, 5);
            var endDate = new DateTime(2008, 02, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.ShortInitial, StubPeriodTypeEnum.LongFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(4, periods.Count);
        }

        [TestMethod]
        public void TestLongInitialShortFinal()
        {
            var startDate = new DateTime(2006, 12, 5);
            var endDate = new DateTime(2007, 12, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.LongInitial, StubPeriodTypeEnum.ShortFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(4, periods.Count);
        }

        [TestMethod]
        public void TestLongInitialLongFinal()
        {
            var startDate = new DateTime(2006, 12, 5);
            var endDate = new DateTime(2008, 02, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.LongInitial, StubPeriodTypeEnum.LongFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(4, periods.Count);
        }

        [TestMethod]
        public void TestLongInitial()
        {
            var startDate = new DateTime(2006, 12, 5);
            var endDate = new DateTime(2008, 01, 5);
            var firstRollDate = new DateTime(2007, 04, 05);
            const RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodsPrincipalExchangesAndStubs periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                startDate, endDate, firstRollDate, frequency, 
                BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY"),
                StubPeriodTypeEnum.LongInitial, StubPeriodTypeEnum.ShortFinal, PaymentCalendar);

            InfoToDebug(periods);
            //            Assert.AreEqual(4, periods.Count);
        }

        #endregion

        #region Test Credit Foncier Schedule

        [TestMethod]
        public void Test1()
        {
            DateTime today = DateTime.Today;
            DateTime firstRoll = DateTime.Today.AddMonths(3);
            DateTime secondRoll = DateTime.Today.AddMonths(6);
            DateTime timehirdRoll = DateTime.Today.AddMonths(9);
            DateTime fourthRoll = DateTime.Today.AddMonths(12);
            var listOfRollDates = new List<DateTime> {today, firstRoll, secondRoll, timehirdRoll, fourthRoll};
            const string dayCounterAsString = "ACT/365.FIXED";
            const double initialPrincipal = 100000.0;
            const double annualRate = 0.08;
            List<CreditFoncierScheduleItem> schedule = CreditFoncierSchedule.GenerateSchedule(listOfRollDates, dayCounterAsString, initialPrincipal, annualRate);
            object[,] array = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(schedule);

            Debug.Print(ParameterFormatter.FormatObject(array));

        }

        #endregion

        #region Test Trade Pricer

        private const string MarketName = "TestTradePricer";
        private readonly DateTime _baseDate = new DateTime(1998, 2, 20);
        private readonly string[] _metrics = ValuationEngineTests1.GetCouponMetrics();

        #region Initialisation Method

        public static ISwapLegEnvironment CreateInterestRateStreamTestEnvironment(DateTime baseDate)
        {
            var market = new SwapLegEnvironment();
            var curve = TestRateCurve(baseDate);
            var fxcurve = TestFxCurve(baseDate);
            market.AddPricingStructure("DiscountCurve", curve);
            market.AddPricingStructure("ForecastCurve", curve);
            market.AddPricingStructure("ReportingCurrencyFxCurve", fxcurve);
            return market;
        }

        public static IFxCurve TestFxCurve(DateTime baseDate)
        {
            const string curveName = "AUD-USD";
            const string algorithm = "LinearForward";
            //const double tolerance = 0.00000001;
            var fxProperties = new NamedValueSet();
            fxProperties.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties.Set("BuildDateTime", baseDate);
            fxProperties.Set(CurveProp.BaseDate, baseDate);
            fxProperties.Set(CurveProp.Market, "LIVE");
            fxProperties.Set("Identifier", "FxCurve.AUD-USD");
            fxProperties.Set(CurveProp.Currency1, "AUD");
            fxProperties.Set(CurveProp.CurrencyPair, curveName);
            fxProperties.Set(CurveProp.CurveName, curveName);
            fxProperties.Set("Algorithm", algorithm);
            fxProperties.Set(CurveProp.OptimizeBuild, false);
            string[] instruments =  
                {   "AUDUSD-FxForward-0D", "AUDUSD-FxSpot-SP", "AUDUSD-FxForward-1M", "AUDUSD-FxForward-2M", "AUDUSD-FxForward-3M"
                };

            decimal[] rates = { 0.90m, 0.90m, 0.90m, 0.90m, 0.90m };
            var curve = CurveEngine.CreateCurve(fxProperties, instruments, rates, null, null, null) as IFxCurve;
            return curve;
        }


        public static IRateCurve TestRateCurve(DateTime baseDate)
        {
            const string curveName = "AUD-LIBOR-BBA-3M";
            const string indexTenor = "3M";
            const string algorithm = "FastLinearZero";
            const string marketEnvironment = "Bob";
            const double tolerance = 0.00000001;
            var props = new object[11, 2];
            props[0, 0] = CurveProp.CurveName;
            props[0, 1] = curveName;
            props[1, 0] = "Algorithm";
            props[1, 1] = algorithm;
            props[2, 0] = CurveProp.PricingStructureType;
            props[2, 1] = "RateCurve";
            props[3, 0] = "BuildDateTime";
            props[3, 1] = baseDate;
            props[4, 0] = CurveProp.IndexName;
            props[4, 1] = "AUD-LIBOR-BBA";
            props[5, 0] = CurveProp.IndexTenor;
            props[5, 1] = indexTenor;
            props[6, 0] = "Identifier";
            props[6, 1] = "Alex";
            props[7, 0] = CurveProp.Market;
            props[7, 1] = marketEnvironment;
            props[8, 0] = "BaseDate";
            props[8, 1] = baseDate;
            props[9, 0] = "Tolerance";
            props[9, 1] = tolerance;
            props[10, 0] = CurveProp.OptimizeBuild;
            props[10, 1] = false;
            var namevalues = new NamedValueSet(props);
            string[] instruments =  
                {   "AUD-Deposit-1D", "AUD-Deposit-1M", "AUD-Deposit-2M", "AUD-Deposit-3M",
                    "AUD-IRFuture-IR-0", "AUD-IRFuture-IR-1", "AUD-IRFuture-IR-2", "AUD-IRFuture-IR-3", 
                    "AUD-IRFuture-IR-4", "AUD-IRFuture-IR-5", "AUD-IRFuture-IR-6", "AUD-IRFuture-IR-7",
                    "AUD-IRSwap-3Y", "AUD-IRSwap-4Y", "AUD-IRSwap-5Y", "AUD-IRSwap-6Y", 
                    "AUD-IRSwap-7Y", "AUD-IRSwap-8Y", "AUD-IRSwap-9Y", "AUD-IRSwap-10Y", 
                    "AUD-IRSwap-12Y", "AUD-IRSwap-15Y", "AUD-IRSwap-20Y", "AUD-IRSwap-25Y", "AUD-IRSwap-30Y"
                };
            decimal[] rates =      {0.0725m,    0.0755m,    0.0766m,    0.07755m, 
                                    0.0781m,    0.07865m,   0.0794m,    0.07862m, 
                                    0.07808m,   0.07745m,   0.07752m,   0.0764m,
                                    0.06915m,   0.06745m,   0.06745m,   0.0785m,
                                    0.0786m,    0.0795m,    0.0725m,    0.0785m,
                                    0.0785m,    0.0785m,    0.0786m,    0.0787m, 0.0788m
                                   };
            decimal[] additional = {      
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m
                                   };
            var curve = CurveEngine.CreateCurve(namevalues, instruments, rates, additional, null, null);
            return curve as IRateCurve;
        }

        private static double[,] GenerateVols(double vol)
        {
            var result = new double[12, 9];
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    result[i, j] = vol;
                }
            }
            return result;
        }

        public static IMarketEnvironment GetMarket()//ILogger _logger, ICoreCache _cache, String nameSpace
        {
            var baseDate = new DateTime(1998, 2, 20);
            var volproperties = new NamedValueSet();
            string[] expiries =      {"1D",    "1W",    "1M",    "2M", 
                                                  "3M",    "6M",   "1Y", 
                                                  "2Y",   "3Y",   "5Y",   "7Y",   "10Y"};

            string[] strikes =      {"3.50",    "4.00",    "4.50",    "5.00", 
                                                  "5.50",    "6.00",   "6.50",    "7.00", 
                                                  "7.50"};


            // build the market environment
            var marketEnvironment = new MarketEnvironment();
            //The vol surface.
            volproperties.Set(CurveProp.PricingStructureType, "RateVolatilityMatrix");
            volproperties.Set("BuildDateTime", baseDate);
            volproperties.Set("BaseDate", baseDate);
            volproperties.Set(CurveProp.Market, "LIVE");
            volproperties.Set(CurveProp.MarketDate, baseDate);
            volproperties.Set("Instrument", "AUD-Xibor-3M");
            volproperties.Set("Source", "SydneySwapDesk");
            volproperties.Set(CurveProp.CurveName, "AUD-Xibor-3M-SydneySwapDesk");
            volproperties.Set("Identifier", "RateVolatilityMatrix.AUD-Xibor-3M");
            volproperties.Set("Algorithm", "Linear");
            volproperties.Set("Currency", "AUD");
            volproperties.Set("QuoteUnits", "LogNormalVolatility");
            volproperties.Set("InformationSource", "SwapDesk");
            volproperties.Set("MeasureType", "Volatility");
            volproperties.Set("StrikeQuoteUnits", "Rate");
            volproperties.Set("QuotationSide", "Mid");
            volproperties.Set("Timing", "Close");
            volproperties.Set("ValuationDate", baseDate);
            volproperties.Set("BusinessCenter", "Sydney");
            var volatilities = GenerateVols(0.12);
            var volcurve = (IVolatilitySurface)CurveEngine.CreateVolatilitySurface(volproperties, expiries, strikes, volatilities);
            marketEnvironment.AddPricingStructure("RateVolatilityMatrix.EUR-EURIBOR-Telerate-6M", volcurve);
            marketEnvironment.AddPricingStructure("RateVolatilityMatrix.EUR-IRSwap-5Y", volcurve);
            //Build the AUD Curves
            const string disIdn = "DiscountCurve." + "AUD-LIBOR-SENIOR";
            var discountCurven = TestRateCurve(baseDate);
            marketEnvironment.AddPricingStructure(disIdn, discountCurven);
            //Build the EUR curves.
            var discountCurveProperties = new NamedValueSet();
            discountCurveProperties.Set(CurveProp.PricingStructureType, "DiscountCurve");
            discountCurveProperties.Set("BuildDateTime", baseDate);
            discountCurveProperties.Set(CurveProp.BaseDate, baseDate);
            discountCurveProperties.Set(CurveProp.Market, MarketName);
            discountCurveProperties.Set(CurveProp.MarketAndDate, MarketName);
            const string disId = "DiscountCurve.EUR-LIBOR-SENIOR";
            discountCurveProperties.Set("Identifier", disId);
            discountCurveProperties.Set(CurveProp.Currency1, "EUR");
            discountCurveProperties.Set("CreditInstrumentId", "LIBOR");
            discountCurveProperties.Set("CreditSeniority", "SENIOR");
            discountCurveProperties.Set(CurveProp.CurveName, "EUR-LIBOR-SENIOR");
            discountCurveProperties.Set("Algorithm", "LinearZero");
            //Set the required test curves.Firstly the discount curve.
            var discountCurve = TestRateCurve(baseDate);
            marketEnvironment.AddPricingStructure(disId, discountCurve);
            marketEnvironment.AddPricingStructure("DiscountCurve.GBP-LIBOR-SENIOR", discountCurve);
            var curvePropertiesa = new NamedValueSet();
            curvePropertiesa.Set(CurveProp.PricingStructureType, "RateCurve");
            curvePropertiesa.Set("BuildDateTime", baseDate);
            curvePropertiesa.Set(CurveProp.BaseDate, baseDate);
            curvePropertiesa.Set(CurveProp.Market, MarketName);
            curvePropertiesa.Set(CurveProp.MarketAndDate, MarketName);
            const string forecastCurveIda = "RateCurve.EUR-EURIBOR-Telerate-6M";
            curvePropertiesa.Set("Identifier", forecastCurveIda);
            curvePropertiesa.Set(CurveProp.Currency1, "EUR");
            curvePropertiesa.Set(CurveProp.IndexName, "EUR-EURIBOR-Telerate");
            curvePropertiesa.Set(CurveProp.IndexTenor, "6M");
            curvePropertiesa.Set(CurveProp.CurveName, "EUR-EURIBOR-Telerate-6M");
            curvePropertiesa.Set("Algorithm", "LinearZero");
            //THe forecast ratecurve
            marketEnvironment.AddPricingStructure(forecastCurveIda, discountCurve);
            var curvePropertiesb = new NamedValueSet();
            curvePropertiesb.Set(CurveProp.PricingStructureType, "RateCurve");
            curvePropertiesb.Set("BuildDateTime", baseDate);
            curvePropertiesb.Set(CurveProp.BaseDate, baseDate);
            curvePropertiesb.Set(CurveProp.Market, MarketName);
            curvePropertiesb.Set(CurveProp.MarketAndDate, MarketName);
            curvePropertiesb.Set("Identifier", "RateCurve." + "EUR-LIBOR-BBA");
            curvePropertiesb.Set(CurveProp.Currency1, "EUR");
            curvePropertiesb.Set(CurveProp.IndexName, "EUR-LIBOR-BBA");
            curvePropertiesb.Set(CurveProp.IndexTenor, "6M");
            curvePropertiesb.Set(CurveProp.CurveName, "EUR-LIBOR-BBA-6M");
            curvePropertiesb.Set("Algorithm", "LinearZero");
            //THe forecast ratecurve
            const string forecastCurveIdb = "RateCurve.EUR-LIBOR-BBA-6M";
            marketEnvironment.AddPricingStructure(forecastCurveIdb, discountCurve);
            var fxProperties = new NamedValueSet();
            fxProperties.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties.Set("BuildDateTime", baseDate);
            fxProperties.Set(CurveProp.BaseDate, baseDate);
            fxProperties.Set(CurveProp.Market, MarketName);
            fxProperties.Set(CurveProp.MarketAndDate, MarketName);
            const string fxCurveId = "FxCurve.EUR-USD";
            fxProperties.Set("Identifier", fxCurveId);
            fxProperties.Set(CurveProp.Currency1, "EUR");
            fxProperties.Set(CurveProp.Currency2, "USD");
            fxProperties.Set(CurveProp.CurrencyPair, "EUR-USD");
            fxProperties.Set(CurveProp.CurveName, "EUR-USD");
            fxProperties.Set("Algorithm", "LinearForward");
            //The fx curve.
            var fxCurve = TestFxCurve(baseDate);
            marketEnvironment.AddPricingStructure(fxCurveId, fxCurve);
            marketEnvironment.AddPricingStructure("FxCurve.EUR-AUD", fxCurve);
            //Build the USD Curves
            var discountCurveProperties2 = new NamedValueSet();
            discountCurveProperties2.Set(CurveProp.PricingStructureType, "DiscountCurve");
            discountCurveProperties2.Set("BuildDateTime", baseDate);
            discountCurveProperties2.Set(CurveProp.BaseDate, baseDate);
            discountCurveProperties2.Set(CurveProp.Market, MarketName);
            discountCurveProperties2.Set(CurveProp.MarketAndDate, MarketName);
            const string disId2 = "DiscountCurve." + "USD-LIBOR-SENIOR";
            discountCurveProperties2.Set("Identifier", disId2);
            discountCurveProperties2.Set(CurveProp.Currency1, "USD");
            discountCurveProperties2.Set("CreditInstrumentId", "LIBOR");
            discountCurveProperties2.Set("CreditSeniority", "SENIOR");
            discountCurveProperties2.Set(CurveProp.CurveName, "USD-LIBOR-SENIOR");
            discountCurveProperties2.Set("Algorithm", "LinearZero");
            //Set the required test curves.Firstly the discount curve.
            var discountCurve2 = TestRateCurve(baseDate);
            marketEnvironment.AddPricingStructure(disId2, discountCurve2);
            var curveProperties2 = new NamedValueSet();
            curveProperties2.Set(CurveProp.PricingStructureType, "RateCurve");
            curveProperties2.Set("BuildDateTime", baseDate);
            curveProperties2.Set(CurveProp.BaseDate, baseDate);
            curveProperties2.Set(CurveProp.Market, MarketName);
            curveProperties2.Set(CurveProp.MarketAndDate, MarketName);
            const string forecastCurveId2 = "RateCurve.USD-LIBOR-BBA-6M";
            curveProperties2.Set("Identifier", forecastCurveId2);
            curveProperties2.Set(CurveProp.Currency1, "USD");
            curveProperties2.Set(CurveProp.IndexName, "USD-LIBOR-BBA");
            curveProperties2.Set(CurveProp.IndexTenor, "6M");
            curveProperties2.Set(CurveProp.CurveName, "USD-LIBOR-BBA-6M");
            curveProperties2.Set("Algorithm", "LinearZero");
            //THe forecast ratecurve
            marketEnvironment.AddPricingStructure(forecastCurveId2, discountCurve);
            var fxProperties2 = new NamedValueSet();
            fxProperties2.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties2.Set("BuildDateTime", baseDate);
            fxProperties2.Set(CurveProp.BaseDate, baseDate);
            fxProperties2.Set(CurveProp.Market, MarketName);
            fxProperties2.Set("Identifier", "FxCurve.USD-AUD");
            fxProperties2.Set(CurveProp.Currency1, "AUD");
            fxProperties2.Set(CurveProp.Currency2, "USD");
            fxProperties2.Set(CurveProp.CurrencyPair, "USD-AUD");
            fxProperties2.Set(CurveProp.CurveName, "USD-AUD");
            fxProperties2.Set("Algorithm", "LinearForward");
            //The fx curve.
            const string fxCurveId2 = "FxCurve.USD-AUD";
            marketEnvironment.AddPricingStructure(fxCurveId2, fxCurve);
            //Build the JPY Curves
            var discountCurveProperties3 = new NamedValueSet();
            discountCurveProperties3.Set(CurveProp.PricingStructureType, "DiscountCurve");
            discountCurveProperties3.Set("BuildDateTime", baseDate);
            discountCurveProperties3.Set(CurveProp.BaseDate, baseDate);
            discountCurveProperties3.Set(CurveProp.Market, MarketName);
            discountCurveProperties3.Set(CurveProp.MarketAndDate, MarketName);
            const string disId3 = "DiscountCurve.JPY-LIBOR-SENIOR";
            discountCurveProperties3.Set("Identifier", disId3);
            discountCurveProperties3.Set(CurveProp.Currency1, "JPY");
            discountCurveProperties3.Set("CreditInstrumentId", "LIBOR");
            discountCurveProperties3.Set("CreditSeniority", "SENIOR");
            discountCurveProperties3.Set(CurveProp.CurveName, "JPY-LIBOR-SENIOR");
            discountCurveProperties3.Set("Algorithm", "LinearZero");
            //Set the required test curves.Firstly the discount curve.
            marketEnvironment.AddPricingStructure(disId3, discountCurve);
            var curveProperties3 = new NamedValueSet();
            curveProperties3.Set(CurveProp.PricingStructureType, "RateCurve");
            curveProperties3.Set("BuildDateTime", baseDate);
            curveProperties3.Set(CurveProp.BaseDate, baseDate);
            curveProperties3.Set(CurveProp.Market, MarketName);
            curveProperties3.Set(CurveProp.MarketAndDate, MarketName);
            const string forecastCurveId3 = "RateCurve.JPY-LIBOR-BBA-6M";
            curveProperties3.Set("Identifier", forecastCurveId3);
            curveProperties3.Set(CurveProp.Currency1, "JPY");
            curveProperties3.Set(CurveProp.IndexName, "JPY-LIBOR-BBA");
            curveProperties3.Set(CurveProp.IndexTenor, "6M");
            curveProperties3.Set(CurveProp.CurveName, "JPY-LIBOR-BBA-6M");
            curveProperties3.Set("Algorithm", "LinearZero");
            //THe forecast ratecurve
            marketEnvironment.AddPricingStructure(forecastCurveId3, discountCurve);
            var fxProperties3 = new NamedValueSet();
            fxProperties3.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties3.Set("BuildDateTime", baseDate);
            fxProperties3.Set(CurveProp.BaseDate, baseDate);
            fxProperties3.Set(CurveProp.Market, MarketName);
            fxProperties3.Set(CurveProp.MarketAndDate, MarketName);
            const string fxCurveId3 = "FxCurve.JPY-USD";
            fxProperties3.Set("Identifier", fxCurveId3);
            fxProperties3.Set(CurveProp.Currency1, "JPY");
            fxProperties3.Set(CurveProp.Currency2, "USD");
            fxProperties3.Set(CurveProp.CurrencyPair, "JPY-USD");
            fxProperties3.Set(CurveProp.CurveName, "JPY-USD");
            fxProperties3.Set("Algorithm", "LinearForward");
            //The fx curve.
            marketEnvironment.AddPricingStructure(fxCurveId3, fxCurve);
            marketEnvironment.AddPricingStructure("FxCurve.JPY-AUD", fxCurve);
            marketEnvironment.AddPricingStructure("FxCurve.GBP-AUD", fxCurve);
            marketEnvironment.AddPricingStructure("FxCurve.GBP-USD", fxCurve);
            //Build the CHF curves.
            var discountCurveProperties4 = new NamedValueSet();
            discountCurveProperties4.Set(CurveProp.PricingStructureType, "DiscountCurve");
            discountCurveProperties4.Set("BuildDateTime", baseDate);
            discountCurveProperties4.Set(CurveProp.BaseDate, baseDate);
            discountCurveProperties4.Set(CurveProp.Market, MarketName);
            discountCurveProperties4.Set(CurveProp.MarketAndDate, MarketName);
            const string disId4 = "DiscountCurve.CHF-LIBOR-SENIOR";
            discountCurveProperties4.Set("Identifier", disId4);
            discountCurveProperties4.Set(CurveProp.Currency1, "CHF");
            discountCurveProperties4.Set("CreditInstrumentId", "LIBOR");
            discountCurveProperties4.Set("CreditSeniority", "SENIOR");
            discountCurveProperties4.Set(CurveProp.CurveName, "CHF-LIBOR-SENIOR");
            discountCurveProperties4.Set("Algorithm", "LinearZero");
            //Set the required test curves.Firstly the discount curve.
            marketEnvironment.AddPricingStructure(disId4, discountCurve);
            var curveProperties4 = new NamedValueSet();
            curveProperties4.Set(CurveProp.PricingStructureType, "RateCurve");
            curveProperties4.Set("BuildDateTime", baseDate);
            curveProperties4.Set(CurveProp.BaseDate, baseDate);
            curveProperties4.Set(CurveProp.Market, MarketName);
            curveProperties4.Set(CurveProp.MarketAndDate, MarketName);
            const string forecastCurveId4 = "RateCurve.CHF-LIBOR-BBA-6M";
            curveProperties4.Set("Identifier", forecastCurveId4);
            curveProperties4.Set(CurveProp.Currency1, "CHF");
            curveProperties4.Set(CurveProp.IndexName, "CHF-LIBOR-BBA");
            curveProperties4.Set(CurveProp.IndexTenor, "6M");
            curveProperties4.Set(CurveProp.CurveName, "CHF-LIBOR-BBA-6M");
            curveProperties4.Set("Algorithm", "LinearZero");
            //THe forecast ratecurve
            marketEnvironment.AddPricingStructure(forecastCurveId4, discountCurve);
            var fxProperties4 = new NamedValueSet();
            fxProperties4.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties4.Set("BuildDateTime", baseDate);
            fxProperties4.Set(CurveProp.BaseDate, baseDate);
            fxProperties4.Set(CurveProp.Market, MarketName);
            fxProperties4.Set(CurveProp.MarketAndDate, MarketName);
            const string fxCurveId4 = "FxCurve.CHF-AUD";
            fxProperties4.Set("Identifier", fxCurveId4);
            fxProperties4.Set(CurveProp.Currency1, "AUD");
            fxProperties4.Set(CurveProp.Currency2, "CHF");
            fxProperties4.Set(CurveProp.CurrencyPair, "CHF-AUD");
            fxProperties4.Set(CurveProp.CurveName, "CHF-AUD");
            fxProperties4.Set("Algorithm", "LinearForward");
            //The fx curve.
            marketEnvironment.AddPricingStructure(fxCurveId4, fxCurve);
            var fxProperties5 = new NamedValueSet();
            fxProperties5.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties5.Set("BuildDateTime", baseDate);
            fxProperties5.Set(CurveProp.BaseDate, baseDate);
            fxProperties5.Set(CurveProp.Market, MarketName);
            fxProperties5.Set(CurveProp.MarketAndDate, MarketName);
            const string fxCurveId5 = "FxCurve.AUD-USD";
            fxProperties5.Set("Identifier", fxCurveId5);
            fxProperties5.Set(CurveProp.Currency1, "AUD");
            fxProperties5.Set(CurveProp.Currency2, "USD");
            fxProperties5.Set(CurveProp.CurrencyPair, "AUD-USD");
            fxProperties5.Set(CurveProp.CurveName, "AUD-USD");
            fxProperties5.Set("Algorithm", "LinearForward");
            //The fx curve.
            marketEnvironment.AddPricingStructure(fxCurveId5, fxCurve);
            return marketEnvironment;
        }

        #endregion

        [TestMethod]
        public void TradeIRSwapPricerWithReportingCurrency1()
        {
            var swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            //Price.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "EUR", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }


        [TestMethod]
        public void TradeIRSwapPricerWithReportingCurrency3()
        {
            var swap = FpMLTestsSwapHelper.GetSwap02StubAmort();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            trade.id = "TestTrade001";
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "EUR", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }


        [TestMethod]
        public void TradeIRSwapPricerWithReportingCurrency4()
        {
            var swap = FpMLTestsSwapHelper.GetSwap05ExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            trade.id = "TestTrade001";
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "EUR", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        [Ignore]
        public void TradeIRSwapPricerWithReportingCurrency5()
        {
            //Contains complex cashflows. This functionality is not yet available.
            var swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            trade.id = "TestTrade001";
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "EUR", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }


        [TestMethod]
        public void TradeIRSwaptionPricerWithReportingCurrency1()
        {
            var swap = FpMLTestsSwapHelper.GetIrSwaptionExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwaption(trade, swap);
            trade.id = "TestTrade001";
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwaption.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeIRSwaptionPricerWithReportingCurrency2()
        {
            var swap = FpMLTestsSwapHelper.GetIrSwaption2ExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwaption(trade, swap);
            trade.id = "TestTrade001";
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwaption.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeXccySwapPricerWithReportingCurrency()
        {
            var swap = FpMLTestsSwapHelper.GetSwap06ExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            trade.id = "TestTrade002";
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleXccySwap.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeAssetSwapPricerWithReportingCurrency()
        {
            var swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            trade.id = "TestTrade003";
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleAssetSwap.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }


        [TestMethod]
        public void TradeFraPricerWithReportingCurrency()
        {
            var swap = FpMLTestsFraHelper.GetFra08ExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetFra(trade, swap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFra.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "CHF", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeFxForwardPricerWithReportingCurrency()
        {
            var swap = FpMLTestsSwapHelper.GetSwap07ExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetFxSingleLeg(trade, swap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxForward.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            //Calculate the metrics.
            //Price.
            //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeFxSpotPricerWithReportingCurrency()
        {
            var swap = FpMLTestsSwapHelper.GetSwap08ExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetFxSingleLeg(trade, swap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxSpot.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            //Price.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);

        }

        [TestMethod]
        public void TradeBulletPaymentPricerWithReportingCurrency()
        {
            var swap = FpMLTestsSwapHelper.GetBulletPaymentExampleObject();
            var trade = new Trade();
            FpMLFieldResolver.TradeSetBulletPayment(trade, swap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleBullet.nvs"));

            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);

            Debug.Print(result);

        }

        [TestMethod]
        public void TradeTermDepositPricerWithReportingCurrency()
        {
            var depo = FpMLTestsSwapHelper.GetSimpleTermDepositExampleObject();
            var trade = new Trade();
            FpMLFieldResolver.TradeSetTermDeposit(trade, depo);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleTermDeposit.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeSimpleTermDepositPricerWithReportingCurrency()
        {
            var depo = FpMLTestsSwapHelper.GetTermDepositExampleObject();
            var trade = new Trade();
            FpMLFieldResolver.TradeSetTermDeposit(trade, depo);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleTermDeposit.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeCapPricerWithReportingCurrency()
        {
            var cap = FpMLTestsSwapHelper.GetCapExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, cap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleCapFloor.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeFloorPricerWithReportingCurrency()
        {
            var cap = FpMLTestsSwapHelper.GetFloorExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, cap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleCapFloor.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeCollarPricerWithReportingCurrency()
        {
            var cap = FpMLTestsSwapHelper.GetCollarExampleObject();
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, cap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleCapFloor.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeFxSwapPricerWithReportingCurrency()
        {
            var cap = FpMLTestsSwapHelper.GetFxSwapExampleObject();
            var trade = new Trade();
            FpMLFieldResolver.TradeSetFxSwap(trade, cap);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxSwap.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        [TestMethod]
        public void TradeFxOptionPricerWithReportingCurrency()
        {
            var fxoption = FpMLTestsSwapHelper.GetFxOptionLegExampleObject();
            var trade = new Trade();
            FpMLFieldResolver.TradeSetFxOptionLeg(trade, fxoption);
            var tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxOptionLeg.nvs"));
            //Create the tradepricer.
            var tradePricer = new TradePricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, trade, tradeProps);
            var baseParty = tradeProps.GetValue<string>("Party1", true);
            trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
            //Calculate the metrics.
            var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", baseParty);
            var calcresult = tradePricer.Price(modelData, ValuationReportType.Summary);
            var result = XmlSerializerHelper.SerializeToString(calcresult);
            Debug.Print(result);
        }

        #endregion

        #region Test Portfolio Pricer

        // needs rewrite
        //[TestMethod]
        //public void TradeIRSwapPricerWithReportingCurrency1()
        //{
        //    var trade1 = FpMLTestsSwapHelper.GetTrade01_ExampleObject();

        //    //TODO Add the other trade examples.

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));

        //    var item = UTE.Cache.MakeItem(trade1, "trade00001", tradeProps, false, new TimeSpan(1000));
        //    var tradeList = new List<ICoreItem> { item };
        //    var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    //Calculate the metrics.
        //    //Price.
        //    //var logger = new TraceLogger(false);
        //    var modelData = ValuationEngineTests1.CreateInstrumentModelData(_metrics, _baseDate, Market, "AUD", "X-1131063516");
        //    //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
        //    var calcresult = portfolioPricer.Price(modelData, ValuationReportType.Summary, false);//This no longer works.
        //    var result = XmlSerializerHelper.SerializeToString(calcresult);
        //    Debug.Print(result);
        //}

        // needs rewrite
        //[TestMethod]
        //public void TradeIRSwapPricerWithReportingCurrency1WithDetail()
        //{
        //    var trade1 = FpMLTestsSwapHelper.GetTrade01_ExampleObject();

        //    //TODO Add the other trade examples.

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    //trade.ItemElementName = ItemChoiceType16.swap;
        //    //XsdClassesFieldResolver.Trade_SetSwap(trade, swap);
        //    //NamedValueSet tradeProps = new NamedValueSet();
        //    //Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
        //    //    ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));

        //    //Create the tradepricer.
        //    //var tradePricer = new TradePricer(trade, tradeProps);
        //    //trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    //var tradePair = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    //var tradeList = new List<Pair<Trade, NamedValueSet>> { tradePair };

        //    var item = ServerStore.Client.MakeItem(trade1, "trade00001", tradeProps, false, new TimeSpan(1000));
        //    var tradeList = new List<ICoreItem> { item };
        //    var portfolioPricer = new PortfolioPricer(tradeList);
        //    //Calculate the metrics.
        //    //Price.
        //    var logger = new TraceLogger(false);
        //    //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
        //    var calcresult = portfolioPricer.Price(logger, tradeList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Full, false);
        //    var result = XmlSerializerHelper.SerializeToString(calcresult);
        //    Debug.Print(result);
        //}


        // needs rewrite
        //[TestMethod]
        //public void TradeFxSwapPricerWithReportingCurrency1WithDetail()
        //{
        //    var trade1 = FpMLTestsSwapHelper.GetTrade08_ExampleObject();

        //    //TODO Add the other trade examples.

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    //trade.ItemElementName = ItemChoiceType16.swap;
        //    //XsdClassesFieldResolver.Trade_SetSwap(trade, swap);
        //    //NamedValueSet tradeProps = new NamedValueSet();
        //    //Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
        //    //    ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxSpot.nvs"));

        //    //Create the tradepricer.
        //    //var tradePricer = new TradePricer(trade, tradeProps);
        //    //trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    //var tradePair = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    //var tradeList = new List<Pair<Trade, NamedValueSet>> { tradePair };
        //    var item = ServerStore.Client.MakeItem(trade1, "trade00001", tradeProps, false, new TimeSpan(1000));
        //    var tradeList = new List<ICoreItem> { item };
        //    var portfolioPricer = new PortfolioPricer(tradeList);
        //    //Calculate the metrics.
        //    //Price.
        //    var logger = new TraceLogger(false);
        //    //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
        //    var calcresult = portfolioPricer.Price(logger, tradeList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Full, false);
        //    var result = XmlSerializerHelper.SerializeToString(calcresult);
        //    Debug.Print(result);
        //    //summary
        //    var calcresult2 = portfolioPricer.Price(logger, tradeList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Summary, false);
        //    var result2 = XmlSerializerHelper.SerializeToString(calcresult2);
        //    Debug.Print(result2);
        //}

        // needs rewrite
        //[TestMethod]
        //public void PortfolioPricerWithReportingCurrency()
        //{
        //    //ird-ex01-vanilla-swap.xml
        //    var trade1 = FpMLTestsSwapHelper.GetTrade01_ExampleObject();

        //    //ird-ex02-stub-amort-swap.xml
        //    var trade2 = FpMLTestsSwapHelper.GetTrade02_ExampleObject();

        //    //ird-ex03-compound-swap.xml
        //    //var trade3 = FpMLTestsSwapHelper.GetTrade03_ExampleObject();

        //    //ird-ex04-arrears-stepup-fee-swap.xml
        //    var trade4 = FpMLTestsSwapHelper.GetTrade04_ExampleObject();

        //    //ird-ex05-long-stub-swap.xml
        //    var trade5 = FpMLTestsSwapHelper.GetTrade05_ExampleObject();

        //    //ird-ex06-xccy-swap.xml
        //    var trade6 = FpMLTestsSwapHelper.GetTrade06_ExampleObject();

        //    //fx-ex03-fx-fwd.xml
        //    var trade7 = FpMLTestsSwapHelper.GetTrade07_ExampleObject();

        //    //fx-ex01-fx-spot.xml
        //    var trade8 = FpMLTestsSwapHelper.GetTrade08_ExampleObject();

        //    //ird-ex07-ois-swap.xml
        //    //var trade9 = FpMLTestsSwapHelper.GetTrade09_ExampleObject();

        //    //ird-ex08-fra.xml
        //    var trade10 = FpMLTestsSwapHelper.GetTrade10_ExampleObject();

        //    //ird-ex28-bullet-payments.xml
        //    var trade11 = FpMLTestsSwapHelper.GetTradeBullet_ExampleObject();

        //    //TODO Add the other trade examples.

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    //trade.ItemElementName = ItemChoiceType16.swap;
        //    //XsdClassesFieldResolver.Trade_SetSwap(trade, swap);
        //    //NamedValueSet tradeProps = new NamedValueSet();
        //    //Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
        //    //    ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
        //    var tradePropsxccy = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleXccySwap.nvs"));
        //    var tradePropsfxfwd = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxForward.nvs"));
        //    var tradePropsfxspot = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxSpot.nvs"));
        //    var tradePropsfra = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFra.nvs"));
        //    var tradePropsbullet = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleBullet.nvs"));

        //    //Create the tradepricer.
        //    //var tradePricer = new TradePricer(trade, tradeProps);
        //    //trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    var tradePair1 = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    var tradePair2 = new Pair<Trade, NamedValueSet>(trade2, tradeProps);
        //    //var tradePair3 = new Pair<Trade, NamedValueSet>(trade3, tradeProps);
        //    var tradePair4 = new Pair<Trade, NamedValueSet>(trade4, tradeProps);
        //    var tradePair5 = new Pair<Trade, NamedValueSet>(trade5, tradeProps);
        //    var tradePair6 = new Pair<Trade, NamedValueSet>(trade6, tradePropsxccy);
        //    var tradePair7 = new Pair<Trade, NamedValueSet>(trade7, tradePropsfxfwd);
        //    var tradePair8 = new Pair<Trade, NamedValueSet>(trade8, tradePropsfxspot);
        //    //var tradePair9 = new Pair<Trade, NamedValueSet>(trade9, tradeProps);
        //    var tradePair10 = new Pair<Trade, NamedValueSet>(trade10, tradePropsfra);
        //    var tradePair11 = new Pair<Trade, NamedValueSet>(trade11, tradePropsbullet);
        //    var tradeList = new List<Pair<Trade, NamedValueSet>>
        //                        {
        //                            tradePair1,
        //                            tradePair2,
        //                            //tradePair3,
        //                            tradePair4,
        //                            tradePair5,
        //                            tradePair6,
        //                            tradePair7,
        //                            tradePair8,
        //                            //tradePair9,
        //                            tradePair10,
        //                            tradePair11
        //                        };
        //    var coreList = tradeList.Select(trade => ServerStore.Client.MakeItem(trade.First, "trade00001", trade.Second, false, new TimeSpan(1000))).ToList();
        //    var portfolioPricer = new PortfolioPricer(coreList);
        //    //Calculate the metrics.
        //    //Price.
        //    var logger = new TraceLogger(false);
        //    //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
        //    var calcresult = portfolioPricer.Price(logger, coreList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Summary, false);
        //    foreach (var report in calcresult)
        //    {
        //        var result = XmlSerializerHelper.SerializeToString(report);
        //        Debug.Print(result);
        //    }
        //}


        // needs rewrite
        //[TestMethod]
        //public void TradeSetIRSwapPricerWithReportingCurrency1()
        //{
        //    var trade1 = FpMLTestsSwapHelper.GetTrade01_ExampleObject();

        //    //TODO Add the other trade examples.
        //    var stopwatch = new Stopwatch();

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));

        //    //Create the tradepricer.
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    //var tradePair = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    //var tradeList = new List<Pair<Trade, NamedValueSet>>();
        //    var item = ServerStore.Client.MakeItem(trade1, "trade00001", tradeProps, false, new TimeSpan(1000));
        //    var coreList = new List<ICoreItem> { item };
        //    stopwatch.Start();

        //    for(var i = 0; i < 100; i++)
        //    {
        //        coreList.Add(item);
        //    }
        //    var portfolioPricer = new PortfolioPricer(coreList);
        //    Debug.Print("Load Trades : {0}", stopwatch.Elapsed);
        //    //Calculate the metrics.
        //    //Price.
        //    //var logger = new TraceLogger(false);
        //    var calcresult = portfolioPricer.Price(null, coreList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Summary, false);
        //    Debug.Print("Calculate Trades : {0}", stopwatch.Elapsed);
        //    var result = XmlSerializerHelper.SerializeToString(calcresult);
        //    Debug.Print(result);
        //}

        #endregion
    }
}
