#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting.Helpers;
using Orion.Analytics.Schedulers;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using FpML.V5r10.Reporting;
using Orion.Analytics.Utilities;
using FpML.V5r10.Reporting.Models.Rates;
using FpML.V5r10.Reporting.Models.Rates.Coupons;
using FpML.V5r10.Reporting.Models.Rates.Stream;
using Orion.ValuationEngine.Generators;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates;
using Orion.CalendarEngine.Helpers;
using Orion.ValuationEngine.Factory;
using XsdClassesFieldResolver = FpML.V5r10.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableInterestRateStream : InstrumentControllerBase, IPriceableInterestRateStream<IStructuredStreamParameters, IStreamInstrumentResults>, IPriceableInstrumentController<InterestRateStream>
    {
        #region Member Fields

        // Analytics
        public IModelAnalytic<IStructuredStreamParameters, StreamInstrumentMetrics> AnalyticsModel { get; set; }

        protected const string CModelIdentifier = "StructuredInterestRateStream";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public Currency Currency { get; set; }
        public string BasePartyCurveName { get; set; }
        public string CounterPartyCurveName { get; set; }
        public string DiscountCurveName { get; set; }
        public string ForecastCurveName { get; set; }
        public bool IsDiscounted { get; set; }

        // Requirements for pricing
        public string ReportingCurrencyFxCurveName { get; set; }

        //// BucketedCouponDates
        //public Period BucketingInterval { get; set; }
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();
        //public DateTime[] BucketedDates { get; set; }

        //// PaymentDates
        public PaymentDates PaymentDates { get; set; }
        public ResetDates ResetDates { get; set; }
        public CalculationPeriodDates CalculationPeriodDates { get; set; }

        //The amount info.
        public CalculationPeriodAmount CalculationPeriodAmount { get; set; }
        public StubCalculationPeriodAmount StubCalculationPeriodAmount { get; set; }

        //Extra stuff
        public SettlementProvision SettlementProvision { get; set; }

        //The cash flows.
        public Cashflows Cashflows { get; set; }

        // Children
        public List<PriceableRateCoupon> Coupons { get; set; }

        public List<PriceablePrincipalExchange> Exchanges { get; set; }

        #endregion

        #region Public Fields

        /// <summary>
        /// The strike for use in Swaptions.
        /// </summary>
        public decimal? Strike { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        /// <summary>
        /// Gets a value indicating whether [paying fixed rate].
        /// </summary>
        /// <value><c>true</c> if [paying fixed rate]; otherwise, <c>false</c>.</value>
        public bool PayingFixedRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool PayerIsBaseParty { get; set; }

        /// <summary>
        /// Gets the payer.
        /// </summary>
        /// <value>The payer.</value>
        public string Payer { get; protected set; }

        /// <summary>
        /// Gets the receiver.
        /// </summary>
        /// <value>The receiver.</value>
        public string Receiver { get; protected set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IStructuredStreamParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IStreamInstrumentResults CalculationResults { get; protected set; }

        /// <summary>
        /// Gets the coupon steam type.
        /// </summary>
        /// <value>The coupon stream type.</value>
        public CouponStreamType CouponStreamType { get; set; }

        /// <summary>
        /// Gets the calculation.
        /// </summary>
        /// <value>The calculation.</value>
        public Calculation Calculation { get; protected set; }

        /// <summary>
        /// Gets the principal type.
        /// </summary>
        /// <value>The principal type.</value>
        public PrincipalExchanges PrincipalExchanges { get; protected set; }

        /// <summary>
        /// Gets the priceable coupons.
        /// </summary>
        /// <value>The priceable coupons.</value>
        public List<InstrumentControllerBase> PriceableCoupons
        {
            get 
            {
                List<InstrumentControllerBase> result = null;
                if (Coupons != null && Coupons.Count > 0)
                {
                    result = new List<InstrumentControllerBase>(Coupons); 
                }
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the priceable principal exchanges.
        /// </summary>
        /// <value>The priceable principal exchanges.</value>
        public List<InstrumentControllerBase> PriceablePrincipalExchanges
        {
            get 
            { 
                List<InstrumentControllerBase> result = null;
                if (Exchanges != null && Exchanges.Count > 0)
                {
                    result = new List<InstrumentControllerBase>(Exchanges);
                }
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the priceable principal exchanges.
        /// </summary>
        /// <value>The priceable principal exchanges.</value>
        public PrincipalExchange[] GetCashflowPrincipalExchanges()
        {
            return Cashflows?.principalExchange;
        }

        /// <summary>
        /// Gets or sets the priceable principal exchanges.
        /// </summary>
        /// <value>The priceable principal exchanges.</value>
        public PaymentCalculationPeriod[] GetCashflowPaymentCalculationPeriods()
        {
            return Cashflows?.paymentCalculationPeriod;
        }

        /// <summary>
        /// Gets the stream start dates.
        /// </summary>
        /// <value>The stream start dates.</value>
        public IList<DateTime> StreamStartDates
        {
            get
            {
                var datesList = Coupons.Select(coupon => coupon.AccrualStartDate).ToList();
                datesList.Sort();
                return datesList;
            }
        }

        /// <summary>
        /// Gets the stream end dates.
        /// </summary>
        /// <value>The stream end dates.</value>
        public IList<DateTime> StreamEndDates
        {
            get
            {
                var datesList = Coupons.Select(coupon => coupon.AccrualEndDate).ToList();
                datesList.Sort();
                return datesList;
            }
        }

        /// <summary>
        /// Gets the stream payment dates.
        /// </summary>
        /// <value>The stream payment dates.</value>
        public IList<DateTime> StreamPaymentDates
        {
            get
            {
                var datesList = Coupons.Select(coupon => coupon.PaymentDate).ToList();
                datesList.Sort();
                return datesList;
            }
        }

        /// <summary>
        /// Gets the adjusted stream dates indicators.
        /// </summary>
        /// <value>The adjusted stream dates indicators.</value>
        public IList<Boolean> AdjustedStreamDatesIndicators => GetStreamAdjustedIndicators();

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public PriceableInterestRateStream()
        {
            Multiplier = 1.0m;
            AnalyticsModel = new StructuredStreamAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableInterestRateStream"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="swapId">The swap Id.</param>
        /// <param name="payerPartyReference">The payer party reference.</param>
        /// <param name="receiverPartyReference">The receiver party reference.</param>
        /// <param name="payerIsBase">The flag for whether the payerreference is the base party.</param>
        /// <param name="calculationPeriodDates">The caluclation period date information.</param>
        /// <param name="paymentDates">The payment dates of the swap leg.</param>
        /// <param name="resetDates">The reset dates of the swap leg.</param>
        /// <param name="principalExchanges">The principal Exchange type.</param>
        /// <param name="calculationPeriodAmount">The calculation period amount data.</param>
        /// <param name="stubCalculationPeriodAmount">The stub calculation information.</param>
        /// <param name="cashflows">The FpML cashflows for that stream.</param>
        /// <param name="settlementProvision">The settlement provision data.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableInterestRateStream
            (
            ILogger logger
            , ICoreCache cache
            , String nameSpace
            , string swapId
            , string payerPartyReference
            , string receiverPartyReference 
            , bool payerIsBase
            , CalculationPeriodDates calculationPeriodDates
            , PaymentDates paymentDates
            , ResetDates resetDates
            , PrincipalExchanges principalExchanges
            , CalculationPeriodAmount calculationPeriodAmount
            , StubCalculationPeriodAmount stubCalculationPeriodAmount
            , Cashflows cashflows
            , SettlementProvision settlementProvision
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : this(logger, cache, nameSpace, swapId, payerPartyReference, receiverPartyReference, payerIsBase, calculationPeriodDates, paymentDates, resetDates, principalExchanges,
            calculationPeriodAmount, stubCalculationPeriodAmount, cashflows, settlementProvision, true, fixingCalendar, paymentCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableInterestRateStream"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namesspace.</param>
        /// <param name="swapId">The swap Id.</param>
        /// <param name="payerPartyReference">The payer party reference.</param>
        /// <param name="receiverPartyReference">The receiver party reference.</param>
        /// <param name="payerIsBase">The flag for whether the payerreference is the base party.</param>
        /// <param name="calculationPeriodDates">The caluclation period date information.</param>
        /// <param name="paymentDates">The payment dates of the swap leg.</param>
        /// <param name="resetDates">The reset dates of the swap leg.</param>
        /// <param name="principalExchanges">The principal Exchange type.</param>
        /// <param name="calculationPeriodAmount">The calculation period amount data.</param>
        /// <param name="stubCalculationPeriodAmount">The stub calculation information.</param>
        /// <param name="cashflows">The FpML cashflows for that stream.</param>
        /// <param name="settlementProvision">The settlement provision data.</param>
        /// <param name="forecastRateInterpolation">ForwardEndDate = forecastRateInterpolation ? AccrualEndDate 
        /// : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableInterestRateStream
            (
            ILogger logger
            , ICoreCache cache
            , String nameSpace
            , string swapId
            , string payerPartyReference
            , string receiverPartyReference 
            , bool payerIsBase
            , CalculationPeriodDates calculationPeriodDates
            , PaymentDates paymentDates
            , ResetDates resetDates
            , PrincipalExchanges principalExchanges
            , CalculationPeriodAmount calculationPeriodAmount
            , StubCalculationPeriodAmount stubCalculationPeriodAmount
            , Cashflows cashflows
            , SettlementProvision settlementProvision
            , bool forecastRateInterpolation
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
        {
            Multiplier = 1.0m;
            Payer = payerPartyReference;
            Receiver = receiverPartyReference;
            PayerIsBaseParty = payerIsBase;
            CalculationPeriodDates = calculationPeriodDates;
            PaymentDates = paymentDates;
            PaymentCurrencies = new List<string>();
            ResetDates = resetDates;
            PrincipalExchanges = principalExchanges;
            CalculationPeriodAmount = calculationPeriodAmount;
            AnalyticsModel = new StructuredStreamAnalytic();
            Calculation = (Calculation)CalculationPeriodAmount.Item;
            if (Calculation.Items?[0] is Schedule strikeSchedule)
            {
                Strike = strikeSchedule.initialValue;//Only picks up the first fixed rate for the swaption calculation.
            }
            StubCalculationPeriodAmount = stubCalculationPeriodAmount;
            Cashflows = cashflows;         
            CouponStreamType = CouponTypeFromCalculation(Calculation);
            Id = BuildId(swapId, CouponStreamType);
            ForecastRateInterpolation = forecastRateInterpolation;
            var isThereDiscounting = XsdClassesFieldResolver.CalculationHasDiscounting(Calculation);
            if (isThereDiscounting)
            {
                IsDiscounted = true; //TODO need to include rate logic for the correct solved answers. What about reset cashflows??
            }
            //Get the currency.
            var currency = XsdClassesFieldResolver.CalculationGetNotionalSchedule(Calculation);
            Currency = currency.notionalStepSchedule.currency;
            if (!PaymentCurrencies.Contains(Currency.Value))
            {
                PaymentCurrencies.Add(Currency.Value);
            }
            //The calendars
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, PaymentDates.paymentDatesAdjustments.businessCenters, nameSpace);
            }
            SettlementProvision = settlementProvision;
            //Set the default discount curve name.
            DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency.Value, true);
            //Set the forecast curve name.//TODO extend this to the other types.
            if (CouponStreamType != CouponStreamType.GenericFixedRate)
            {
                if (fixingCalendar == null)
                {
                    fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, ResetDates.resetDatesAdjustments.businessCenters, nameSpace);
                }
                ForecastCurveName = null;
                if (Calculation.Items != null)
                {
                    var floatingRateCalculation = Calculation.Items;
                    var floatingRateIndex = (FloatingRateCalculation) floatingRateCalculation[0];
                    ForecastCurveName = CurveNameHelpers.GetForecastCurveName(floatingRateIndex);
                }
            }
            //Build the coupons and principal exchanges.
            if (GetCashflowPaymentCalculationPeriods() != null)
            {
                Coupons = PriceableInstrumentsFactory.CreatePriceableCoupons(PayerIsBaseParty,
                                                                             GetCashflowPaymentCalculationPeriods(),
                                                                             Calculation, ForecastRateInterpolation, fixingCalendar, paymentCalendar);//TODO add the stubcalculation.
                UpdateCouponIds();
            }
            if (GetCashflowPrincipalExchanges() != null)
            {
                var exchanges = GetCashflowPrincipalExchanges();
                Exchanges = PriceableInstrumentsFactory.CreatePriceablePrincipalExchanges(PayerIsBaseParty, exchanges, Currency.Value, paymentCalendar);
                UpdateExchangeIds();
            }            
            RiskMaturityDate = LastDate();
            logger.LogInfo("Stream built");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableInterestRateStream"/> class.  All the cashflows must be signed.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="payerIsBase">The flag for whether the payerreference is the base party.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableInterestRateStream(ILogger logger, ICoreCache cache
            , String nameSpace, bool payerIsBase, InterestRateStream stream
            , IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
            : this(logger, cache, nameSpace, payerIsBase, stream, true, fixingCalendar, paymentCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableInterestRateStream"/> class.  All the cashflows must be signed.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="payerIsBase">The flag for whether the payerreference is the base party.</param>
        /// <param name="forecastRateInterpolation">ForwardEndDate = forecastRateInterpolation ? AccrualEndDate 
        /// : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableInterestRateStream(ILogger logger, ICoreCache cache, String nameSpace
            , bool payerIsBase, InterestRateStream stream, bool forecastRateInterpolation
            , IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
            : this(logger
            , cache
            , nameSpace
            , stream.id
            , stream.payerPartyReference.href
            , stream.receiverPartyReference.href 
            , payerIsBase
            , stream.calculationPeriodDates
            , stream.paymentDates
            , stream.resetDates
            , stream.principalExchanges
            , stream.calculationPeriodAmount
            , stream.stubCalculationPeriodAmount
            , BuildCashflow(stream, fixingCalendar, paymentCalendar)
            , stream.settlementProvision
            , forecastRateInterpolation
            , fixingCalendar
            , paymentCalendar)
        {}

        #endregion

        #region Coupons

        /// <summary>
        /// Gets the stream dates.
        /// </summary>
        /// <returns></returns>
        public DateTime[] GetStreamDates()
        {
            var couponDates = new List<DateTime>();
            for (var index = 1; index < Coupons.Count; index++)
            {
                var firstCoupon = Coupons[index - 1];
                var secondCoupon = Coupons[index];
                if (!couponDates.Contains(firstCoupon.AccrualStartDate))
                {
                    couponDates.Add(firstCoupon.AccrualStartDate);
                }
                if (!couponDates.Contains(secondCoupon.AccrualStartDate))
                {
                    couponDates.Add(secondCoupon.AccrualStartDate);
                }
                if (index == Coupons.Count - 1)
                {
                    couponDates.Add(secondCoupon.AccrualEndDate);
                }
            }
            couponDates.Sort();
            return couponDates.ToArray();
        }

        /// <summary>
        /// Gets the stream adjusted indicators.
        /// </summary>
        /// <returns></returns>
        public IList<Boolean> GetStreamAdjustedIndicators()
        {
            var adjusted = new List<bool>();
            var lastCouponIndex = Coupons.Count - 1;
            var index = 0;
            foreach (var coupon in Coupons)
            {
                adjusted.Add(coupon.AdjustCalculationDatesIndicator);
                if (lastCouponIndex == index)
                {
                    adjusted.Add(coupon.AdjustCalculationDatesIndicator);
                }
                index++;
            }
            return adjusted;
        }

        /// <summary>
        /// Gets the bucketed coupon dates.
        /// </summary>
        /// <param name="bucketInterval">The bucket interval.</param>
        /// <returns></returns>
        protected IDictionary<string, DateTime[]> GetBucketedCouponDates(Period bucketInterval)
        {
            IDictionary<string, DateTime[]> bucketCouponDates = new Dictionary<string, DateTime[]>();
            var bucketDates = new List<DateTime>();
            foreach (IPriceableInstrumentController<PaymentCalculationPeriod> coupon in Coupons)
            {
                var priceableRateCoupon = (IPriceableRateCoupon<IRateCouponParameters, IRateInstrumentResults>)coupon;
                DateTime firstRegularPeriodStartDate;
                DateTime lastRegularPeriodEndDate;
                bucketDates.AddRange(new List<DateTime>(DateScheduler.GetUnadjustedDatesFromTerminationDate(priceableRateCoupon.UnadjustedStartDate, priceableRateCoupon.UnadjustedEndDate, BucketingInterval, RollConventionEnum.NONE, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate)));
                bucketDates = RemoveDuplicates(bucketDates);
                bucketCouponDates.Add(coupon.Id, bucketDates.ToArray());
            }
            return bucketCouponDates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="bucketInterval"></param>
        /// <returns></returns>
        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            var bucketDates = new List<DateTime>();
            if (Coupons.Count > 1)
            {
                DateTime firstRegularPeriodStartDate;
                DateTime lastRegularPeriodEndDate;
                bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate));
            }
            return bucketDates.ToArray();
        }

        /// <summary>
        /// A mapping function to label a stream correctly.
        /// </summary>
        /// <param name="calculation"></param>
        /// <returns>The couponstreamtype</returns>
        private static CouponStreamType CouponTypeFromCalculation(Calculation calculation)
        {
            var cpt = CouponStreamType.GenericFloatingRate;
            // assume fixed if Items is null, alex!? todo
            if ((null == calculation.Items) || (calculation.Items[0] is Schedule))
            {
                cpt = CouponStreamType.GenericFixedRate;
            }
            var frc = calculation.Items?[0] as FloatingRateCalculation;
            if (frc != null)
            {
                if (frc.capRateSchedule != null && frc.floorRateSchedule == null)
                {
                    return CouponStreamType.CapRate;
                }
                if (frc.capRateSchedule == null && frc.floorRateSchedule != null)
                {
                    return CouponStreamType.FloorRate;
                }
                if (frc.capRateSchedule != null && frc.floorRateSchedule != null)
                {
                    return CouponStreamType.CollarRate;
                }
                cpt = CouponStreamType.GenericFloatingRate;
            }
            return cpt;
        }


        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public InterestRateStream Build()
        {
            var irstream = new InterestRateStream
                               {
                                   cashflows = BuildCashflows(),
                                   payerPartyReference = PartyReferenceFactory.Create(Payer),
                                   receiverPartyReference = PartyReferenceFactory.Create(Receiver),
                                   calculationPeriodDates = CalculationPeriodDates,
                                   calculationPeriodAmount = CalculationPeriodAmount,
                                   id = Id,
                                   paymentDates = PaymentDates,
                                   principalExchanges = PrincipalExchanges,
                                   resetDates = ResetDates,
                                   settlementProvision = SettlementProvision,
                                   stubCalculationPeriodAmount = StubCalculationPeriodAmount
                               };
            
            return irstream;
        }

        /// <summary>
        /// Builds the cashflows.
        /// </summary>
        /// <returns></returns>
        protected Cashflows BuildCashflows()
        {
            //TODO what about the cash flow match parameter?
            var cashflows = new Cashflows
                                {
                                    paymentCalculationPeriod = Coupons.Select(priceableCoupon => priceableCoupon.Build()).ToArray()
                                    
                                };
            if (Exchanges != null)
            {
                var principalExchanges = Exchanges.Select(principalExchange => principalExchange.Build()).ToArray();
                if (principalExchanges.Any())
                {
                    cashflows.principalExchange = principalExchanges;
                }
            }
            return cashflows;
        }

        #endregion

        #region Metrics for Valuation

        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            CalculationResults = null;
            UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            // 1. First derive the analytics to be evaluated via the stream controller model 
            // NOTE: These take precendence of the child model metrics
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new StructuredStreamAnalytic();
            }
            var streamControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation streamValuation;
            // 2. Now evaluate only the child specific metrics (if any)
            foreach (var coupon in Coupons)
            {
                coupon.PricingStructureEvolutionType = PricingStructureEvolutionType;
                coupon.BucketedDates = BucketedDates;
                coupon.Multiplier = Multiplier;
            }
            var childControllers = new List<InstrumentControllerBase>(Coupons.ToArray());
            //Now the stream analytics can be completed.
            var childValuations = EvaluateChildMetrics(childControllers, modelData, Metrics);
            var couponValuation = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            var childControllerValuations = new List<AssetValuation> {couponValuation};
            if (Exchanges != null && Exchanges.Count > 0)
            {
                foreach (var exchange in Exchanges)
                {
                    exchange.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    exchange.BucketedDates = BucketedDates;
                    exchange.Multiplier = Multiplier;
                }
                // Roll-Up and merge child valuations into parent Valuation
                var childPrincipalControllers = new List<InstrumentControllerBase>(Exchanges.ToArray());
                var childPrincipalValuations = EvaluateChildMetrics(childPrincipalControllers, modelData, Metrics);
                var principalValuation = AssetValuationHelper.AggregateMetrics(childPrincipalValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
                childControllerValuations.Add(principalValuation);
            }
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (streamControllerMetrics.Count > 0)
            {
                var reportingCurrency = ModelData.ReportingCurrency == null ? Currency.Value : ModelData.ReportingCurrency.Value;
                var notionals = GetCouponNotionals();
                var accrualFactors = GetCouponAccrualFactors();
                var discountFactors = GetPaymentDiscountFactors();
                var floatingNPV = AggregateMetric(InstrumentMetrics.FloatingNPV, childControllerValuations);
                var accrualFactor = AggregateMetric(InstrumentMetrics.AccrualFactor, childControllerValuations);
                //TODO need to  set the notional amount and the weighting. Also amortisation??
                IStructuredStreamParameters analyticModelParameters = new StructuredStreamParameters
                                                                          {   Multiplier = Multiplier,
                                                                              IsDiscounted = IsDiscounted,
                                                                              CouponNotionals = notionals,
                                                                              Currency = Currency.Value,
                                                                              ReportingCurrency = reportingCurrency,
                                                                              AccrualFactor = accrualFactor,
                                                                              FloatingNPV = floatingNPV,
                                                                              NPV = AggregateMetric(InstrumentMetrics.NPV, childControllerValuations),
                                                                              CouponYearFractions = accrualFactors,
                                                                              PaymentDiscountFactors = discountFactors,
                                                                              TargetNPV = floatingNPV
                                                                          };
                CalculationResults = AnalyticsModel.Calculate<IStreamInstrumentResults, StreamInstrumentResults>(analyticModelParameters, streamControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var streamControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                streamValuation = AssetValuationHelper.UpdateValuation(streamControllerValuation,
                                                                       childControllerValuations, ConvertMetrics(streamControllerMetrics), new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
                AnalyticModelParameters = analyticModelParameters;
            }
            else
            {
                streamValuation = AssetValuationHelper.AggregateMetrics(childControllerValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            }
            CalculationPerfomedIndicator = true;
            streamValuation.id = Id;
            return streamValuation;
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return null;
        }

        #region Obsolete - Don'e use as the child controllers are recalculated

        /// <summary>
        /// Aggregates the metric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <param name="valuationDate">The vluation date. </param>
        /// <returns></returns>
        public Decimal AggregateMetric(string metric, DateTime valuationDate)
        {
            var enumMetric = EnumHelper.Parse<InstrumentMetrics>(metric);
            var childControllerValuations = new List<AssetValuation>();
            if (Coupons != null)
            {
                childControllerValuations.AddRange(Coupons.Select(cashflow => GetValue(cashflow.CalculationResults, valuationDate)));
            }
            if (Exchanges != null)
            {
                childControllerValuations.AddRange(Exchanges.Select(principal => GetValue(principal.CalculationResults, valuationDate)));
            }
            var result = AggregateMetric(enumMetric, childControllerValuations);
            return result;
        }

        ///// <summary>
        ///// Aggregates the metric.
        ///// </summary>
        ///// <param name="metric">The metric.</param>
        ///// <returns></returns>
        //virtual protected Decimal AggregateCashflowMetric(InstrumentMetrics metric)
        //{
        //    decimal result = AggregateCouponMetric(metric) + AggregateExchangeMetric(metric);
        //    return result;
        //}

        ///// <summary>
        ///// Aggregates the coupon metric.
        ///// </summary>
        ///// <param name="metric">The metric.</param>
        ///// <returns></returns>
        //public Decimal AggregateCouponMetric(string metric)
        //{
        //    var enumMetric = EnumHelper.Parse<InstrumentMetrics>(metric);
        //    decimal result = AggregateCouponMetric(enumMetric);
        //    return result;
        //}

        ///// <summary>
        ///// Aggregates the coupon metric.
        ///// </summary>
        ///// <param name="metric">The metric.</param>
        ///// <returns></returns>
        //virtual protected Decimal AggregateCouponMetric(InstrumentMetrics metric)
        //{
        //    var result = 0.0m;
        //    if (Coupons != null && Coupons.Count > 0)
        //    {
        //        string[] metrics = { metric.ToString() };
        //        var childValuations = GetChildValuations(Coupons.ToArray(), new List<string>(metrics), ModelData);
        //        result = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// Aggregates the exchange metric.
        ///// </summary>
        ///// <param name="metric">The metric.</param>
        ///// <returns></returns>
        //public Decimal AggregateExchangeMetric(string metric)
        //{
        //    var enumMetric = EnumHelper.Parse<InstrumentMetrics>(metric);
        //    decimal result = AggregateExchangeMetric(enumMetric);
        //    return result;
        //}

        ///// <summary>
        ///// Aggregates the exchange metric.
        ///// </summary>
        ///// <param name="metric">The metric.</param>
        ///// <returns></returns>
        //virtual protected Decimal AggregateExchangeMetric(InstrumentMetrics metric)
        //{
        //    var result = 0.0m;
        //    if (Exchanges != null && Exchanges.Count > 0)
        //    {
        //        string[] metrics = { metric.ToString() };
        //        var childValuations = GetChildValuations(Exchanges.ToArray(), new List<string>(metrics), ModelData);
        //        result = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
        //    }
        //    return result;
        //}

        #endregion

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <param name="childValuations"> </param>
        /// <returns></returns>
        protected Decimal AggregateMetric(InstrumentMetrics metric, List<AssetValuation> childValuations)
        {
            var result = 0.0m;
            if (childValuations != null && childValuations.Count > 0)
            {
                result = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
            }
            return result;
        }

        public void AddCoupon(PriceableRateCoupon newFlow)
        {
            Coupons.Add(newFlow);
        }

        public void AddPrincipalExchange(PriceablePrincipalExchange newFlow)
        {
            Exchanges.Add(newFlow);
        }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="newCurveName">New name of the curve.</param>
        public void UpdateDiscountCurveName(string newCurveName)
        {
            foreach (PriceableRateCoupon coupon in Coupons)
            {
                coupon.UpdateDiscountCurveName(newCurveName);
            }
            DiscountCurveName = newCurveName;
        }

        #endregion

        #region Helper Methods

        public decimal[] GetCouponNotionals()
        {
            var result = new List<decimal>();
            Coupons.ForEach(cashflow => result.Add(cashflow.Notional));
            return result.ToArray();
        }

        public decimal[] GetCouponAccrualFactors()
        {
            var result = new List<decimal>();
            Coupons.ForEach(cashflow => result.Add(cashflow.CouponYearFraction));
            return result.ToArray();
        }

        public decimal[] GetPaymentDiscountFactors()
        {
            var result = new List<decimal>();
            Coupons.ForEach(cashflow => result.Add(cashflow.PaymentDiscountFactor));
            return result.ToArray();
        }

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="childValuations">The metrics.</param>
        /// <param name="controllerMetrics">The controller metrics</param>
        /// <returns></returns>
        protected static AssetValuation AggregateMetrics(List<AssetValuation> childValuations, List<string> controllerMetrics)
        {
            var result = new AssetValuation();
            var quotes = new List<Quotation>();
            foreach (var metric in controllerMetrics)
            {
                var quote = new Quotation();
                var measure = new AssetMeasureType { Value = metric };
                quote.measureType = measure;
                quote.value = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
                quote.valueSpecified = true;
                quotes.Add(quote);
            }
            result.quote = quotes.ToArray();
            return result;
        }

        #endregion

        #region Static Helpers

        protected static Cashflows BuildCashflow(InterestRateStream stream, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var cashflows = stream.cashflows;
            if (stream.cashflows == null || stream.cashflows.cashflowsMatchParameters == false)
            {
                cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            }
            return cashflows;
        }

        protected void  UpdateCouponIds()
        {
            var index = 1;
            foreach (var coupon in Coupons)
            {
                coupon.Id = Id + "." + coupon.CashflowType.Value + "." + index;
                index++;
            }
        }

        protected void UpdateExchangeIds()
        {
            var index = 1;
            foreach (var exchange in Exchanges)
            {
                exchange.Id = Id + "." + exchange.CashflowType.Value + "." + index;
                index++;
            }
        }

        protected static string BuildId(string swapId, CouponStreamType couponStreamType)//, string payerPartyReference)
        {
            const string cUnknown = "UNKNOWN";
            string swapIdentifier = string.IsNullOrEmpty(swapId) ? cUnknown : swapId;
            //return string.Format("{0}.{1}.{2}", swapIdentifier, couponStreamType, payerPartyReference);
            return $"{swapIdentifier}.{couponStreamType}";
        }

        internal static List<IPriceableInstrumentController<PaymentCalculationPeriod>> MapCoupons(List<PriceableRateCoupon> coupons)
        {
            return coupons.Cast<IPriceableInstrumentController<PaymentCalculationPeriod>>().ToList();
        }

        internal static List<IPriceableInstrumentController<PrincipalExchange>> MapExchanges(List<PriceablePrincipalExchange> exchanges)
        {
            return exchanges.Cast<IPriceableInstrumentController<PrincipalExchange>>().ToList();
        }

        public DateTime LastDate()
        {
            if (LastExchangeDate() == null) return LastCouponDate();
            var lastExchangeDate = LastExchangeDate();
            return lastExchangeDate != null && LastCouponDate() <= (DateTime)lastExchangeDate ? (DateTime)lastExchangeDate : LastCouponDate();
        }

        //It is assumed that all coupons are ordered and there is at least one.
        public DateTime LastCouponDate()
        {
            return Coupons[Coupons.Count - 1].AccrualEndDate;
        }

        //There may be no exchanges..
        internal DateTime? LastExchangeDate()
        {
            if (Exchanges == null) return null;
            var length = Exchanges.Count;//System.Math.Max(1, _priceablePrincipalExchanges.Count);
            if (length<1) return null;
            return Exchanges[length - 1].PaymentDate;
        }

        /// <summary>
        /// Intervals the string.
        /// </summary>
        /// <param name="periodLength">Length of the period.</param>
        /// <param name="periodTimeUnit">The period time unit.</param>
        /// <returns></returns>
        protected static string IntervalString(string periodLength, PeriodEnum periodTimeUnit)
        {
            return $"{periodLength}{periodTimeUnit}";
        }

        /// <summary>
        /// Businesses the centers string.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        protected static string BusinessCentersString(List<string> businessCenters)
        {
            return BusinessCentersString(businessCenters.ToArray());
        }

        /// <summary>
        /// Businesses the centers as list.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        internal static List<string> BusinessCentersAsList(BusinessCenter[] businessCenters)
        {
            return businessCenters.Select(bc => bc.Value).ToList();
        }

        /// <summary>
        /// Businesses the centers string.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        protected static string BusinessCentersString(string[] businessCenters)
        {
            return string.Join("-", businessCenters);
        }

        /// <summary>
        /// Updates the rate.
        /// </summary>
        /// <param name="newRate">The new rate.</param>
        public void UpdateRate(Decimal newRate)
        {
            foreach (var priceableCoupon in Coupons)
            {
                if (priceableCoupon.GetType() == typeof(PriceableFixedRateCoupon))
                {
                    var coupon = (PriceableFixedRateCoupon) priceableCoupon;
                    coupon.Rate = newRate;
                }
            }
        }

        //static private Boolean CouponDatesHaveChanged(IEnumerable<DateTime> existingStreamDates, IEnumerable<DateTime> newStreamDates)
        //{
        //    var baseDates = new List<DateTime>(existingStreamDates);
        //    var referenceDates = new List<DateTime>(newStreamDates);
        //    baseDates.Sort();
        //    referenceDates.Sort();

        //    var bChangedIndicator = false;
        //    if (baseDates.Count != referenceDates.Count)
        //    {
        //        bChangedIndicator = true;
        //    }

        //    if (!bChangedIndicator)
        //    {
        //        for (var index = 0; index < baseDates.Count; index++)
        //        {
        //            if (DateTime.Compare(baseDates[index], referenceDates[index]) == 0) continue;
        //            bChangedIndicator = true;
        //            break;
        //        }
        //    }
        //    return bChangedIndicator;
        //}

        /// <summary>
        /// Removes the duplicates
        /// </summary>
        /// <param name="inputList">The input list.</param>
        /// <returns></returns>
        protected static List<T> RemoveDuplicates<T>(List<T> inputList)
        {
            var uniqueStore = new List<T>();
            foreach (T currValue in inputList)
            {
                if (!uniqueStore.Contains(currValue))
                {
                    uniqueStore.Add(currValue);
                }
            }
            return uniqueStore;
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            var children = Coupons.Cast<InstrumentControllerBase>().ToList();
            if (Exchanges!=null)
            {
                children.AddRange(Exchanges);
            }
            return children;
        }

        #endregion

    }
}