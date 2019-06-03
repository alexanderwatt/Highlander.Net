#region Using directives

using System;
using System.Linq;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.Analytics.Helpers;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Helpers;
using Orion.ValuationEngine.Generators;
using Orion.ValuationEngine.Helpers;
//Remove this and replace with a cap
using Orion.Models.Rates.Swaption;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.PricingStructures;
using Orion.ValuationEngine.Valuations;
using Orion.ValuationEngine.Instruments;
using Orion.CurveEngine.Factory;
using Orion.ValuationEngine.Factory;
using Orion.Analytics.Options;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    public abstract class SwaptionPricer :  InstrumentControllerBase, IPriceableInstrumentController<Swaption>
    {
        #region Properties

        public string VolatilitySurfaceName { get; set; }

        /// <summary>
        /// Gets the seller party reference.
        /// </summary>
        /// <value>The seller party reference.</value>
        public string SellerPartyReference { get; set; }

        /// <summary>
        /// Gets the buyer party reference.
        /// </summary>
        /// <value>The buyer party reference.</value>
        public string BuyerPartyReference { get; set; }

        /// <summary>
        /// THe swap
        /// </summary>
        public SwapPricer Swap { get; set; }

        /// <summary>
        /// Gets or sets the priceable swap instrument.
        /// </summary>
        /// <value>The priceable swap instrument.</value>
        public IPriceableInstrumentController<Swap> PriceableSwapInstrument => Swap;

        /// <summary>
        /// Gets a value indicating whether [base party selling the option].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party selling the option]; otherwise, <c>false</c>.
        /// </value>
        public bool IsBasePartyBuyer { get; set; }

        /// <summary>
        /// Gets the premium payment dates.
        /// </summary>
        /// <value>The termination dates.</value>
        public DateTime[] PremiumPaymentDates { get; set; }

        /// <summary>
        /// Gets the premium payment amounts.
        /// </summary>
        /// <value>The payment amounts.</value>
        public Decimal[] PremiumPaymentAmounts { get; set; }

        /// <summary>
        /// Gets the exercise dates.
        /// </summary>
        /// <value>The exercise dates.</value>
        public DateTime[] ExerciseDates { get; set; }

        /// <summary>
        /// Gets a value indicating whether [adjust calculation dates indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust calculation dates indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool AdjustCalculationDatesIndicator { get; set; }

        // Analytics
        public IModelAnalytic<ISwaptionInstrumentParameters, SwaptionInstrumentMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        protected const string CModelIdentifier = "Swaption";
        //protected const string CDefaultBucketingInterval = "3M";

        //// Requirements for pricing
        //public string DiscountCurveName { get; set; }

        //// BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public ISwaptionInstrumentParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// The premium payments.
        /// </summary>
        public List<PriceablePayment> PremiumPayments { get; set; }

        /// <summary>
        /// The call  flag
        /// </summary>
        public bool IsCall { get; set; }

        /// <summary>
        /// The has been exercised  flag
        /// </summary>
        public bool HasBeenExercised { get; set; }

        /// <summary>
        /// THe cash settled flag
        /// </summary>
        public bool IsCashSettled { get; set; }

        /// <summary>
        /// THe exercise type
        /// </summary>
        public ExerciseType ExerciseType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public EuropeanExercise Exercise { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AutomaticExcercise { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal StrikeRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? Volatility { get; set; }

        #endregion

        #region Implementation of IMetricsCalculation<IRateCouponParameters,IRateInstrumentResults>

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ISwaptionInstrumentResults CalculationResults { get; protected set; }

        #endregion

        #region Constructors

        protected SwaptionPricer()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="swaptionFpML"> </param>
        /// <param name="basePartyReference"></param>
        protected SwaptionPricer(ILogger logger, ICoreCache cache,
            String nameSpace, Swaption swaptionFpML, string basePartyReference)
        {
            ProductType =ProductTypeSimpleEnum.InterestRateSwaption;
            if (swaptionFpML == null) return;
            BuyerPartyReference = swaptionFpML.buyerPartyReference.href;
            SellerPartyReference = swaptionFpML.sellerPartyReference.href;
            IsBasePartyBuyer = basePartyReference == BuyerPartyReference;
            PaymentCurrencies = new List<string>();
            if (swaptionFpML.Item is EuropeanExercise exercise)
            {
                ExerciseType = ExerciseType.European;
                Exercise = exercise;
                var exerciseDate = AdjustedDateHelper.ToAdjustedDate(cache, null, exercise.expirationDate, nameSpace);
                ExerciseDates = new[] { exerciseDate };//TODO Only does adjustabledate exercise.
            }

            if (swaptionFpML.exerciseProcedure?.Item is AutomaticExercise)
            {
                AutomaticExcercise = true;
            }
            if (swaptionFpML.premium != null)
            {
                PremiumPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, swaptionFpML.premium, null);
                var paymentAmounts = new List<Decimal>();
                var paymentDates = new List<DateTime>();
                foreach (var premium in PremiumPayments)
                {
                    paymentAmounts.Add(premium.PaymentAmount.amount);
                    paymentDates.Add(premium.PaymentDate);
                    //Add the currencies for the trade pricer.
                    if (!PaymentCurrencies.Contains(premium.PaymentAmount.currency.Value))
                    {
                        PaymentCurrencies.Add(premium.PaymentAmount.currency.Value);
                    }
                }
                PremiumPaymentAmounts = paymentAmounts.ToArray();
                PremiumPaymentDates = paymentDates.ToArray();
            }

            if (swaptionFpML.Item1 is CashSettlement)
            {
                IsCashSettled = true;//TODO extend to the type of calculation.
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="swaptionFpML"> </param>
        /// <param name="basePartyReference"></param>
        /// <param name="forecastRateInterpolation"></param>
        protected SwaptionPricer(ILogger logger, ICoreCache cache,
            String nameSpace, Swaption swaptionFpML, string basePartyReference, Boolean forecastRateInterpolation)
        {
            ProductType = ProductTypeSimpleEnum.InterestRateSwaption;
            if (swaptionFpML == null) return;
            BuyerPartyReference = swaptionFpML.buyerPartyReference.href;
            SellerPartyReference = swaptionFpML.sellerPartyReference.href;
            IsBasePartyBuyer = basePartyReference == BuyerPartyReference;
            if (swaptionFpML.Item is EuropeanExercise exercise)
            {
                ExerciseType = ExerciseType.European;
                Exercise = exercise;
                var exerciseDate = AdjustedDateHelper.ToAdjustedDate(cache, null, exercise.expirationDate, nameSpace);
                ExerciseDates = new[] { exerciseDate };//TODO Only does adjustabledate exercise
            }

            if (swaptionFpML.exerciseProcedure?.Item is AutomaticExercise)
            {
                AutomaticExcercise = true;
            }
            if (swaptionFpML.premium != null)
            {
                PremiumPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, swaptionFpML.premium, null);
            }
            var paymentAmounts = new List<Decimal>();
            var paymentDates = new List<DateTime>();
            foreach (var premium in PremiumPayments)
            {
                paymentAmounts.Add(premium.PaymentAmount.amount);
                paymentDates.Add(premium.PaymentDate);
            }
            PremiumPaymentAmounts = paymentAmounts.ToArray();
            PremiumPaymentDates = paymentDates.ToArray();
            if (swaptionFpML.Item1 is CashSettlement)
            {
                IsCashSettled = true;
            }
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        public Payment[] MapToPayments(List<PriceablePayment> payments)
        {
            return payments?.Select(payment => payment.Build()).ToArray();
        }

        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            var bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out _, out _));
            return bucketDates.ToArray();
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return Build();
        }

        /// <summary>
        /// Builds this instance and retruns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public Swaption Build()
        {
            var premium = MapToPayments(PremiumPayments); //MoneyHelper.GetAmount(Premium, PremiumCurrency);
            var swaption = SwaptionFactory.Create(Swap.Build(), premium, Exercise, AutomaticExcercise);
            return swaption;
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            var children = GetUnderlying();
            var payments = GetPremiumPayments();
            if (children != null)
            {
                if (payments != null)
                {
                    children.AddRange(GetPremiumPayments());
                }
                return children;
            }
            return payments;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public List<InstrumentControllerBase> GetUnderlying()
        {
            return Swap != null ? new List<InstrumentControllerBase> {Swap} : null;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<InstrumentControllerBase> GetPremiumPayments()
        {
            return PremiumPayments?.Cast<InstrumentControllerBase>().ToList();
        }

        #endregion

        #region Static Helpers

        public static Trade CreateSwaptionTrade(SwaptionParametersRange swaptionParametersRange, IBusinessCalendar paymentCalendar, Swap underlyingSwap)
        {
            var premium = MoneyHelper.GetNonNegativeAmount(swaptionParametersRange.Premium, swaptionParametersRange.PremiumCurrency);
            AdjustableDate expirationDate = DateTypesHelper.ToAdjustableDate(swaptionParametersRange.ExpirationDate, swaptionParametersRange.ExpirationDateBusinessDayAdjustments, swaptionParametersRange.ExpirationDateCalendar);
            AdjustableOrAdjustedDate paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(swaptionParametersRange.PaymentDate, swaptionParametersRange.PaymentDateBusinessDayAdjustments, swaptionParametersRange.PaymentDateCalendar);
            TimeSpan earliestExerciseTimeAsTimeSpan = TimeSpan.FromDays(swaptionParametersRange.EarliestExerciseTime);
            DateTime earliestExerciseTime = DateTime.MinValue.Add(earliestExerciseTimeAsTimeSpan);
            TimeSpan expirationTimeAsTimeSpan = TimeSpan.FromDays(swaptionParametersRange.ExpirationTime);
            DateTime expirationTime = DateTime.MinValue.Add(expirationTimeAsTimeSpan);
            var swaption = SwaptionFactory.Create(underlyingSwap, premium, swaptionParametersRange.PremiumPayer, swaptionParametersRange.PremiumReceiver,
                                                                              paymentDate, expirationDate,
                                                                              earliestExerciseTime, expirationTime, swaptionParametersRange.AutomaticExcercise);
            swaption.Items = new object[] {new ProductType {Value = ProductTypeSimpleEnum.InterestRateSwaption.ToString()}};
            swaption.ItemsElementName = new[] {ItemsChoiceType2.productType};
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwaption(trade, swaption);
            return trade;
        }

        public string CreateValuation(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            SwaptionParametersRange swaptionParametersRange,
            List<StringObjectRangeItem> valuationSet,
            ValuationRange valuationRange,
            TradeRange tradeRange,
            SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange,
            List<InputCashflowRangeItem> leg1DetailedCashflowsListArray,
            List<InputCashflowRangeItem> leg2DetailedCashflowsListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentListArray,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentListArray,
            List<PartyIdRangeItem> partyIdList,//optional
            List<OtherPartyPaymentRangeItem> otherPartyPaymentList,//opt
            List<FeePaymentRangeItem> feePaymentList//opt
            )
        {
            var swap = GetPriceAndGeneratedFpMLSwap(logger, cache, nameSpace,
                fixingCalendar, paymentCalendar, valuationRange,
                tradeRange, leg1ParametersRange, leg2ParametersRange,
                leg1DetailedCashflowsListArray, leg2DetailedCashflowsListArray,
                leg1PrincipalExchangeCashflowListArray, leg2PrincipalExchangeCashflowListArray,
                leg1AdditionalPaymentListArray, leg2AdditionalPaymentListArray).Second;
            string baseParty = valuationRange.BaseParty;
            List<IRateCurve> uniqueCurves = GetUniqueCurves(logger, cache, nameSpace, leg1ParametersRange, leg2ParametersRange);
            Market fpMLMarket = InterestRateProduct.CreateFpMLMarketFromCurves(uniqueCurves);
            //  TODO: add Trade Id & Trade data into valuation. (Trade.Id & Trade.TradeHeader.TradeDate)
            //     
            //  create ValuationReport and add it to in-memory collection.
            //  Add methods!
            AssetValuation assetValuation = InterestRateProduct.CreateAssetValuationFromValuationSet(valuationSet);
            NonNegativeMoney premium = MoneyHelper.GetNonNegativeAmount(swaptionParametersRange.Premium, swaptionParametersRange.PremiumCurrency);
            AdjustableDate expirationDate = DateTypesHelper.ToAdjustableDate(swaptionParametersRange.ExpirationDate, swaptionParametersRange.ExpirationDateBusinessDayAdjustments, swaptionParametersRange.ExpirationDateCalendar);
            AdjustableOrAdjustedDate paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(swaptionParametersRange.PaymentDate, swaptionParametersRange.PaymentDateBusinessDayAdjustments, swaptionParametersRange.PaymentDateCalendar);
            TimeSpan earliestExerciseTimeAsTimeSpan = TimeSpan.FromDays(swaptionParametersRange.EarliestExerciseTime);
            DateTime earliestExerciseTime = DateTime.MinValue.Add(earliestExerciseTimeAsTimeSpan);
            TimeSpan expirationTimeAsTimeSpan = TimeSpan.FromDays(swaptionParametersRange.ExpirationTime);
            DateTime expirationTime = DateTime.MinValue.Add(expirationTimeAsTimeSpan);
            var swaption = SwaptionFactory.Create(swap, premium, swaptionParametersRange.PremiumPayer, swaptionParametersRange.PremiumReceiver,
                                                                              paymentDate, expirationDate,
                                                                              earliestExerciseTime, expirationTime, swaptionParametersRange.AutomaticExcercise);
            // overrides the premium created by SwaptionFactort.Create
            //
            var feeList = new List<Payment>();
            if (null != feePaymentList)
            {
                feeList.AddRange(feePaymentList.Select(feePaymentRangeItem => new Payment
                {
                    paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(feePaymentRangeItem.PaymentDate), 
                    paymentAmount = MoneyHelper.GetNonNegativeAmount(feePaymentRangeItem.Amount), 
                    payerPartyReference = PartyReferenceFactory.Create(feePaymentRangeItem.Payer), 
                    receiverPartyReference = PartyReferenceFactory.Create(feePaymentRangeItem.Receiver)
                }));
            }
            swaption.premium = feeList.ToArray();
            string valuationReportAndProductId = tradeRange.Id ?? Guid.NewGuid().ToString();
            swaption.id = valuationReportAndProductId;
            ValuationReport valuationReport = ValuationReportGenerator.Generate(valuationReportAndProductId, baseParty, valuationReportAndProductId, tradeRange.TradeDate, swaption, fpMLMarket, assetValuation);
            cache.SaveObject(valuationReport, valuationReportAndProductId, null);
            InterestRateProduct.ReplacePartiesInValuationReport(valuationReport, partyIdList);
            InterestRateProduct.AddOtherPartyPayments(valuationReport, otherPartyPaymentList);
            return valuationReportAndProductId;
        }

        internal static Pair<ValuationResultRange, Swap> GetPriceAndGeneratedFpMLSwap(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            ValuationRange valuationRange,
            TradeRange tradeRange,
            SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange,
            List<InputCashflowRangeItem> leg1DetailedCashflowsList,
            List<InputCashflowRangeItem> leg2DetailedCashflowsList,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowList,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowList,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentList,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentList
            )
        {
            //Check if the calendars are null. If not build them!
            //
            //
            //
            //
            //
            InterestRateStream stream1 = GetCashflowsSchedule(fixingCalendar, paymentCalendar, leg1ParametersRange);//parametric definiton + cashflows schedule
            InterestRateStream stream2 = GetCashflowsSchedule(fixingCalendar, paymentCalendar, leg2ParametersRange);//parametric definiton + cashflows schedule
            var swap = SwapFactory.Create(stream1, stream2);
            // Update FpML cashflows
            //
            InterestRateSwapPricer.UpdateCashflowsWithDetailedCashflows(stream1.cashflows, leg1DetailedCashflowsList);
            InterestRateSwapPricer.UpdateCashflowsWithDetailedCashflows(stream2.cashflows, leg2DetailedCashflowsList);
            //  Update PE
            //
            CreatePrincipalExchangesFromListOfRanges(stream1.cashflows, leg1PrincipalExchangeCashflowList);
            CreatePrincipalExchangesFromListOfRanges(stream2.cashflows, leg2PrincipalExchangeCashflowList);
            //  Add bullet payments...
            //
            var bulletPaymentList = new List<Payment>();
            if (null != leg1AdditionalPaymentList)
            {
                bulletPaymentList.AddRange(leg1AdditionalPaymentList.Select(bulletPaymentRangeItem => new Payment
                {
                    payerPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Payer), receiverPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Receiver), paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount), paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                }));
            }
            if (null != leg2AdditionalPaymentList)
            {
                bulletPaymentList.AddRange(leg2AdditionalPaymentList.Select(bulletPaymentRangeItem => new Payment
                {
                    payerPartyReference = PartyReferenceFactory.Create(leg2ParametersRange.Payer), receiverPartyReference = PartyReferenceFactory.Create(leg2ParametersRange.Receiver), paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount), paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                }));
            }
            swap.additionalPayment = bulletPaymentList.ToArray();
            // Update FpML cashflows with DF,FV,PV, etc (LegParametersRange needed to access curve functionality)
            //
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream1, leg1ParametersRange, valuationRange);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream2, leg2ParametersRange, valuationRange);
            //  Update additional payments
            //
            var leg1DiscountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, leg1ParametersRange.DiscountCurve);
            var leg2DiscountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, leg2ParametersRange.DiscountCurve);
            SwapGenerator.UpdatePaymentsAmounts(logger, cache, nameSpace, swap, leg1ParametersRange, leg2ParametersRange, leg1DiscountCurve, leg2DiscountCurve, valuationRange.ValuationDate, paymentCalendar);
            //~  Update additional payments
            string baseParty = valuationRange.BaseParty;
            return new Pair<ValuationResultRange, Swap>(CreateValuationRange(swap, baseParty), swap);
        }

        private static void CreatePrincipalExchangesFromListOfRanges(
            Cashflows cashflows,
            IEnumerable<InputPrincipalExchangeCashflowRangeItem> principalExchangeRangeList)
        {
            cashflows.principalExchange = principalExchangeRangeList.Select(item => PrincipalExchangeHelper.Create(item.PaymentDate, (decimal) item.Amount)).ToArray();
        }

        private static InterestRateStream GetCashflowsSchedule(
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            SwapLegParametersRange legParametersRange)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            stream.cashflows = cashflows;
            return stream;
        }

        private static ValuationResultRange CreateValuationRange(Swap swap, string baseParty)
        {
            Money payPresentValue = SwapHelper.GetPayPresentValue(swap, baseParty);
            Money payFutureValue = SwapHelper.GetPayFutureValue(swap, baseParty);
            Money receivePresentValue = SwapHelper.GetReceivePresentValue(swap, baseParty);
            Money receiveFutureValue = SwapHelper.GetReceiveFutureValue(swap, baseParty);
            Money swapPresentValue = SwapHelper.GetPresentValue(swap, baseParty);
            Money swapFutureValue = SwapHelper.GetFutureValue(swap, baseParty);
            var resultRange = new ValuationResultRange
            {
                PresentValue = swapPresentValue.amount,
                FutureValue = swapFutureValue.amount,
                PayLegPresentValue = payPresentValue.amount,
                PayLegFutureValue = payFutureValue.amount,
                ReceiveLegPresentValue = receivePresentValue.amount,
                ReceiveLegFutureValue = receiveFutureValue.amount
            };
            return resultRange;
        }

        public static double GetPremiumImpl(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            SwapLegParametersRange payLegParametersRange,
            SwapLegParametersRange receiveLegParametersRange,
            SwaptionParametersRange swaptionTermsRange,
            ValuationRange valuationRange)
        {
            InterestRateStream payStream = null;
            InterestRateStream receiveStream = null;
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, payStream, payLegParametersRange, valuationRange);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, receiveStream, receiveLegParametersRange, valuationRange);
            Money fv = CashflowsHelper.GetForecastValue(payStream.cashflows);
            Money pv = CashflowsHelper.GetPresentValue(payStream.cashflows);
            double tillExpiry = (swaptionTermsRange.ExpirationDate - valuationRange.ValuationDate).TotalDays / 365.0;
            //Debug.Print("Future value :{0}", fv.amount);
            //Debug.Print("Present value :{0}", pv.amount);
            // get swaption price
            //
            double pricePerDollar = BlackModel.GetSwaptionValue((double)payLegParametersRange.CouponOrLastResetRate, (double)swaptionTermsRange.StrikeRate, (double)swaptionTermsRange.Volatility, tillExpiry);
            double premium = System.Math.Abs((double)payLegParametersRange.NotionalAmount * pricePerDollar);
            return premium;
        }

        private static void UpdateCashflowsWithAmounts(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            InterestRateStream stream,
            SwapLegParametersRange legParametersRange,
            ValuationRange valuationRange)
        {
            //  Get a forecast curve
            //
            IRateCurve forecastCurve = null;
            if (!String.IsNullOrEmpty(legParametersRange.ForecastCurve))
            {
                forecastCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, legParametersRange.ForecastCurve);
            }
            //  Get a discount curve
            //
            var discountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, legParametersRange.DiscountCurve);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(stream, forecastCurve, discountCurve, valuationRange.ValuationDate);
        }

        private static List<IRateCurve> GetUniqueCurves(
            ILogger logger,
            ICoreCache cache,
            String nameSpace,
            SwapLegParametersRange payLegParametersRange,
            SwapLegParametersRange receiveLegParametersRange)
        {
            var uniqueCurves = new List<IRateCurve>();
            var curveNames = new[]
                                 {
                                     payLegParametersRange.ForecastCurve,
                                     payLegParametersRange.DiscountCurve,
                                     receiveLegParametersRange.ForecastCurve,
                                     receiveLegParametersRange.DiscountCurve
                                 };
            foreach (string curveName in curveNames)
            {
                if (!String.IsNullOrEmpty(curveName) && curveName.ToLower() != "none")
                {
                    var curve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, curveName);
                    if (!uniqueCurves.Contains(curve))
                    {
                        uniqueCurves.Add(curve);
                    }
                }
            }
            return uniqueCurves;
        }

        #endregion
    }
}