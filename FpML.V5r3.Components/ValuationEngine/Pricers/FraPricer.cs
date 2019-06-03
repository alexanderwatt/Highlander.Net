using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Models.Rates.Fra;
using Orion.Analytics.Helpers;
using Orion.Analytics.Schedulers;
using Orion.Analytics.DayCounters;
using Orion.CalendarEngine.Helpers;
using Orion.Constants;
using Orion.Identifiers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.ValuationEngine.Instruments;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Analytics.Utilities;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Instruments.InterestRates;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Valuation = Orion.ValuationEngine.Valuations.Valuation;
using Orion.ValuationEngine.Helpers;
using Orion.CurveEngine.Markets;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

namespace Orion.ValuationEngine.Pricers
{
    public class FraPricer : InstrumentControllerBase, IPriceableInstrumentController<Fra>, IPriceableFra<IFraInstrumentParameters, IFraInstrumentResults>
    {
        #region Properties

        //public string Buyer { get; set; }

        //public string Seller { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        //Prodcut parameters
        //public FraInputRange FraInputRange { get; set; } 
        //public Fra Fra { get; set; }

        public List<AssetValuation> ChildValuations { get; set; }

        // Analytics
        public IModelAnalytic<IFraInstrumentParameters, FraInstrumentMetrics> AnalyticsModel { get; set; }

        protected const string CModelIdentifier = "Fra";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public Money Notional { get; set; }

        // Requirements for pricing
        public decimal FixedRate { get; set; }

        // Requirements for pricing
        public string DiscountCurveName { get; set; }

        // Requirements for pricing
        public string ForecastCurveName { get; set; }

        // BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();

        // Requirements for pricing
        public FraDiscountingEnum FraDiscounting { get; set; }

        // Requirements for pricing
        public DayCountFraction DayCountFraction { get; set; }

        // Requirements for pricing
        public RelativeDateOffset FixingOffSet { get; set; }

        // Requirements for pricing
        public Period[] IndexTenor { get; set; }

        // Requirements for pricing
        public FloatingRateIndex FloatingRateIndex { get; set; }

        // Requirements for pricing
        public AdjustableDate AdjustablePaymentDate { get; set; }

        // Requirements for pricing
        public int NumberOfDays { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IFraInstrumentParameters AnalyticModelParameters { get; protected set; }

        //public PriceableFixedRateCoupon FixedCoupon { get; set; }

        public PriceableFloatingRateCoupon FloatingCoupon { get; set; }

        /// <summary>
        /// Gets a value indicating whether [base party paying fixed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party paying fixed]; otherwise, <c>false</c>.
        /// </value>
        public bool BasePartyPayingFixed { get; set; }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <value>The effective date.</value>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets the termination date.
        /// </summary>
        /// <value>The termination date.</value>
        public DateTime TerminationDate { get; set; }

        /// <summary>
        /// Gets the termination date.
        /// </summary>
        /// <value>The termination date.</value>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether [adjust calculation dates indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust calculation dates indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool AdjustCalculationDatesIndicator { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public FraPricer()
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="fraFpML"></param>
        /// <param name="nameSpace"></param>
        public FraPricer(ILogger logger, ICoreCache cache,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            Fra fraFpML, String nameSpace)
            : this(logger, cache, fixingCalendar, paymentCalendar, true, fraFpML, nameSpace)
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="fraFpML"></param>
        /// <param name="baseParty"></param>
        /// <param name="nameSpace"></param>
        public FraPricer(ILogger logger, ICoreCache cache,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            Fra fraFpML, String baseParty, String nameSpace)
            : this(logger, cache, fixingCalendar, paymentCalendar, !IsBasePartyBuyer(baseParty, fraFpML), fraFpML, nameSpace)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="isBuyer"></param>
        /// <param name="fraFpML"></param>
        /// <param name="nameSpace"></param>
        public FraPricer(ILogger logger, ICoreCache cache,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, 
            bool isBuyer, Fra fraFpML, String nameSpace)
        {
            OrderedPartyNames = new List<string>();
            Multiplier = 1.0m;
            FraDiscounting = fraFpML.fraDiscounting;
            FixingOffSet = fraFpML.fixingDateOffset;
            FloatingRateIndex = fraFpML.floatingRateIndex;
            DayCountFraction = fraFpML.dayCountFraction;
            Notional = fraFpML.notional;
            IndexTenor = fraFpML.indexTenor;
            AdjustablePaymentDate = fraFpML.paymentDate;
            FixedRate = fraFpML.fixedRate;
            AddCashFlows(logger, cache, fixingCalendar, paymentCalendar, fraFpML, isBuyer, nameSpace);
            BasePartyPayingFixed = !isBuyer;
            RiskMaturityDate = TerminationDate;
            NumberOfDays = (DayCounterHelper.Parse(DayCountFraction.Value)).DayCount(EffectiveDate, TerminationDate);
            //Set the product type.
            ProductType = ProductTypeSimpleEnum.FRA;
            PaymentCurrencies = new List<string> { Notional.currency.Value };
            //Set the default discount curve name.
            DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Notional.currency.Value, true);
            ForecastCurveName = CurveNameHelpers.GetForecastCurveName(fraFpML.floatingRateIndex, fraFpML.indexTenor);
        }

        #endregion

        #region Static Helpers

        private static bool IsBasePartyBuyer(string baseParty, Fra fraFpML)
        {
            return baseParty == fraFpML.buyerPartyReference.href;
        }

        private void AddCashFlows(ILogger logger, ICoreCache cache,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            Fra fraFpML, bool isBuyer, String nameSpace)
        {
            EffectiveDate = fraFpML.adjustedEffectiveDate.Value;
            TerminationDate = fraFpML.adjustedTerminationDate;
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fraFpML.paymentDate.dateAdjustments.businessCenters, nameSpace);
            }
            if (fixingCalendar == null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fraFpML.fixingDateOffset.businessCenters, nameSpace);
            }
            DateTime unadjustedPaymentDate = fraFpML.paymentDate.unadjustedDate.Value;
            var notional = MoneyHelper.GetAmount(fraFpML.notional.amount, fraFpML.notional.currency);
            PaymentDate = paymentCalendar.Roll(unadjustedPaymentDate, BusinessDayConventionHelper.Parse(fraFpML.paymentDate.dateAdjustments.businessDayConvention.ToString()));
            DateTime adjustedFixingDate = GetResetDate(logger, cache, fixingCalendar, fraFpML, nameSpace);
            var interval = fraFpML.indexTenor[0];
            var floatingInterest = new PriceableFloatingRateCoupon(fraFpML.id + "FloatingCoupon_1"
                                                                   , isBuyer
                                                                   , EffectiveDate
                                                                   , TerminationDate
                                                                   , adjustedFixingDate
                                                                   , fraFpML.dayCountFraction
                                                                   , 0.0m
                                                                   , FixedRate
                                                                   , null
                                                                   , isBuyer ? MoneyHelper.Neg(notional) : notional
                                                                   , PaymentDate
                                                                   , new ForecastRateIndex{floatingRateIndex = fraFpML.floatingRateIndex, indexTenor = interval} 
                                                                   , null
                                                                   , null
                                                                   , fraFpML.fraDiscounting
                                                                   , paymentCalendar
                                                                   , fixingCalendar)
                                       {
                                           ForecastRateInterpolation = ForecastRateInterpolation
                                       };
            // Combine two cashflows into one leg
            //
            FloatingCoupon = floatingInterest;//fraFpML.fraDiscounting, 
        }

        /// <summary>
        /// Gets the reset date.
        /// </summary>
        /// <param name="logger"> </param>
        /// <param name="cache"> </param>
        /// <param name="fixingCalendar"> </param>
        /// <param name="fraFpML">The fraFpML.</param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        private static DateTime GetResetDate(ILogger logger, ICoreCache cache,
            IBusinessCalendar fixingCalendar,
            Fra fraFpML, String nameSpace)
        {
            var effectiveDateId = fraFpML.adjustedEffectiveDate.id;
            var fixingdateRef = fraFpML.fixingDateOffset.dateRelativeTo.href;
            DateTime resetDate = fraFpML.adjustedEffectiveDate.Value;
            if (fixingCalendar == null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fraFpML.fixingDateOffset.businessCenters, nameSpace);
            }
            if(fixingdateRef==effectiveDateId)
            {
                resetDate = AdjustedDateHelper.ToAdjustedDate(fixingCalendar, fraFpML.adjustedEffectiveDate.Value, fraFpML.fixingDateOffset);
            }
            logger.LogInfo("Reset date set.");
            return resetDate;
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Builds this instance and retruns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public Fra Build()
        {
            var fra = new Fra
                          {
                              fixingDateOffset = FixingOffSet,
                              fixedRate = FixedRate,
                              fixedRateSpecified = true,
                              adjustedEffectiveDate = new RequiredIdentifierDate {Value = EffectiveDate},
                              floatingRateIndex = FloatingRateIndex,
                              fraDiscounting = FraDiscounting,
                              fraDiscountingSpecified = true,
                              adjustedTerminationDate = TerminationDate,
                              adjustedTerminationDateSpecified = true,
                              dayCountFraction = DayCountFraction,
                              notional = Notional,
                              indexTenor = IndexTenor,
                              calculationPeriodNumberOfDays = NumberOfDays.ToString(CultureInfo.InvariantCulture),
                              buyerPartyReference =
                                  PartyReferenceHelper.Parse("Party1"),
                              sellerPartyReference =
                                  PartyReferenceHelper.Parse("Party2"),
                              paymentDate = AdjustablePaymentDate
                          };
            var product = new ProductType {Value = ProductTypeSimpleEnum.FRA.ToString()};
            fra.Items = new object[] { product };
            fra.ItemsElementName = new[] { ItemsChoiceType2.productType };
            return fra;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            CalculationResults = null;
            UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            var quotes = ModelData.AssetValuation.quote.ToList();
            ModelData.AssetValuation.quote = quotes.ToArray();
            // 1. First derive the analytics to be evaluated via the stream controller model 
            // NOTE: These take precendence of the child model metrics
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new FraInstrumentAnalytic();
            }
            FloatingCoupon.PricingStructureEvolutionType = PricingStructureEvolutionType;
            FloatingCoupon.BucketedDates = BucketedDates;
            FloatingCoupon.Multiplier = Multiplier;
            var childControllers = new List<InstrumentControllerBase>(GetChildren());
            //The assetValuation list.
            ChildValuations = new List<AssetValuation>();
            // 2. Now evaluate only the child specific metrics (if any)
            if (modelData.MarketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                foreach (var cashflow in childControllers)
                {
                    ChildValuations.Add(cashflow.Calculate(modelData));
                }
            }
            else
            {
                ChildValuations = EvaluateChildMetrics(childControllers, modelData, Metrics);
            }
            CalculationPerfomedIndicator = true;
            ChildValuations[0].id = Id;
            return ChildValuations[0];// fraValuation;
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return Build();
        }

        #endregion

        #region Implementation of IMetricsCalculation<IRateCouponParameters,IRateInstrumentResults>

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IFraInstrumentResults CalculationResults { get; protected set; }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            var children = new List<InstrumentControllerBase> {FloatingCoupon};
            return children;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<IPriceableInstrumentController<PaymentCalculationPeriod>> GetInstumentControllers()
        {
            return GetChildren().Cast<IPriceableInstrumentController<PaymentCalculationPeriod>>().ToList();
        }

        #endregion

        #region Helpers

        public static Trade CreateFraTrade(string tradeId, RequiredIdentifierDate adjustedEffectiveDate, DateTime adjustedTerminationDate,
            AdjustableDate paymentDate, RelativeDateOffset fixingDayOffset, DayCountFraction dayCountFraction, decimal notionalAmount, 
            string notionalCurrency, decimal fixedRate, string floatingRateIndex, string indexTenor, FraDiscountingEnum fraDiscounting)
        {
            var trade = new Trade();
            var fra = new Fra
            {
                adjustedEffectiveDate = adjustedEffectiveDate,
                adjustedTerminationDate = adjustedTerminationDate,
                adjustedTerminationDateSpecified = true,
                paymentDate = paymentDate,
                Items = new object[] { new ProductType {Value = ProductTypeSimpleEnum.FRA.ToString()} },
                ItemsElementName = new[] { ItemsChoiceType2.productType }
            };
            if ("resetDate" != fixingDayOffset.dateRelativeTo.href)
            {
                throw new ArgumentException("The fixing date must be specified as 'resetDate'-relative!", nameof(fixingDayOffset));
            }
            fra.fixingDateOffset = fixingDayOffset;
            fra.dayCountFraction = dayCountFraction;
            IDayCounter dayCounter = DayCounterHelper.Parse(fra.dayCountFraction.Value);
            fra.calculationPeriodNumberOfDays = dayCounter.DayCount(fra.adjustedEffectiveDate.Value, fra.adjustedTerminationDate).ToString(CultureInfo.InvariantCulture);
            fra.notional = MoneyHelper.GetAmount(notionalAmount, notionalCurrency);
            fra.fixedRate = fixedRate;
            fra.fixedRateSpecified = true; 
            fra.floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex);
            fra.indexTenor = new[] { PeriodHelper.Parse(indexTenor) };
            fra.fraDiscounting = fraDiscounting;
            fra.fraDiscountingSpecified = true;
            PartyReference party1 = PartyReferenceFactory.Create("party1");
            PartyReference party2 = PartyReferenceFactory.Create("party2");
            fra.sellerPartyReference = party2;
            fra.buyerPartyReference = party1;
            XsdClassesFieldResolver.TradeSetFra(trade, fra);
            trade.id = tradeId;
            return trade;
        }

        public static Trade CreateFraTrade(string tradeId, DateTime adjustedEffectiveDate, DateTime adjustedTerminationDate,
            DateTime unadjustedPaymentDate, string paymentDateBusinessDayConvention, string paymentDateBusinessCenters, 
            string fixingDayOffsetPeriod, string fixingDayOffsetDayType, string fixingDayOffsetBusinessDayConvention, 
            string fixingDayOffsetBusinessCenters, string fixingDayOffsetDateRelativeTo, 
            string dayCountFraction, decimal notionalAmount, string notionalCurrency, decimal fixedRate, string floatingRateIndex,
            string indexTenor, FraDiscountingEnum fraDiscounting)
        {
            var trade = new Trade();
            var fra = new Fra
            {
                adjustedEffectiveDate =
                    DateTypesHelper.ToRequiredIdentifierDate(adjustedEffectiveDate),
                adjustedTerminationDate = adjustedTerminationDate,
                adjustedTerminationDateSpecified = true,
                paymentDate =
                    DateTypesHelper.ToAdjustableDate(unadjustedPaymentDate,
                                                     paymentDateBusinessDayConvention,
                                                     paymentDateBusinessCenters),
                Items = new object[] { new ProductType {Value = ProductTypeSimpleEnum.FRA.ToString()} },
                ItemsElementName = new[] { ItemsChoiceType2.productType }
            };
            if ("resetDate" != fixingDayOffsetDateRelativeTo)
            {
                throw new ArgumentException("The fixing date must be specified as 'resetDate'-relative!", nameof(fixingDayOffsetDateRelativeTo));
            }
            var fixingDayType = EnumHelper.Parse<DayTypeEnum>(fixingDayOffsetDayType);
            fra.fixingDateOffset = RelativeDateOffsetHelper.Create(fixingDayOffsetPeriod, fixingDayType,
                                                                   fixingDayOffsetBusinessDayConvention,
                                                                   fixingDayOffsetBusinessCenters,
                                                                   fixingDayOffsetDateRelativeTo);
            fra.dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction);
            IDayCounter dayCounter = DayCounterHelper.Parse(fra.dayCountFraction.Value);
            fra.calculationPeriodNumberOfDays = dayCounter.DayCount(fra.adjustedEffectiveDate.Value, fra.adjustedTerminationDate).ToString(CultureInfo.InvariantCulture);
            fra.notional = MoneyHelper.GetAmount(notionalAmount, notionalCurrency);
            fra.fixedRate = fixedRate;
            fra.fixedRateSpecified = true; 
            fra.floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex);
            fra.indexTenor = new[] { PeriodHelper.Parse(indexTenor) };
            fra.fraDiscounting = fraDiscounting;
            fra.fraDiscountingSpecified = true;
            PartyReference party1 = PartyReferenceFactory.Create("party1");
            PartyReference party2 = PartyReferenceFactory.Create("party2");
            fra.sellerPartyReference = party2;
            fra.buyerPartyReference = party1;
            XsdClassesFieldResolver.TradeSetFra(trade, fra);
            trade.id = tradeId;
            return trade;
        }

        public static Trade CreateFraTrade(FraInputRange2 fraInputRange)
        {
            var trade = new Trade();
            var fra = new Fra
            {
                adjustedEffectiveDate =
                    DateTypesHelper.ToRequiredIdentifierDate(fraInputRange.AdjustedEffectiveDate),
                adjustedTerminationDate = fraInputRange.AdjustedTerminationDate,
                adjustedTerminationDateSpecified = true,
                paymentDate =
                    DateTypesHelper.ToAdjustableDate(fraInputRange.UnadjustedPaymentDate,
                                                     fraInputRange.PaymentDateBusinessDayConvention,
                                                     fraInputRange.PaymentDateBusinessCenters),
                Items = new object[] { new ProductType {Value = ProductTypeSimpleEnum.FRA.ToString()} },
                ItemsElementName = new[] { ItemsChoiceType2.productType }
            };
            if ("resetDate" != fraInputRange.FixingDayOffsetDateRelativeTo)
            {
                throw new ArgumentException("The fixing date must be specified as 'resetDate'-relative!", nameof(fraInputRange));
            }
            var fixingDayType = EnumHelper.Parse<DayTypeEnum>(fraInputRange.FixingDayOffsetDayType);
            fra.fixingDateOffset = RelativeDateOffsetHelper.Create(fraInputRange.FixingDayOffsetPeriod, fixingDayType,
                                                                   fraInputRange.FixingDayOffsetBusinessDayConvention,
                                                                   fraInputRange.FixingDayOffsetBusinessCenters,
                                                                   fraInputRange.FixingDayOffsetDateRelativeTo);
            fra.dayCountFraction = DayCountFractionHelper.Parse(fraInputRange.DayCountFraction);
            IDayCounter dayCounter = DayCounterHelper.Parse(fra.dayCountFraction.Value);
            fra.calculationPeriodNumberOfDays = dayCounter.DayCount(fra.adjustedEffectiveDate.Value, fra.adjustedTerminationDate).ToString(CultureInfo.InvariantCulture);
            fra.notional = MoneyHelper.GetAmount(fraInputRange.NotionalAmount, fraInputRange.NotionalCurrency);
            fra.fixedRate = (decimal)fraInputRange.FixedRate;
            fra.fixedRateSpecified = true; 
            fra.floatingRateIndex = FloatingRateIndexHelper.Parse(fraInputRange.FloatingRateIndex);
            fra.indexTenor = new[] { PeriodHelper.Parse(fraInputRange.IndexTenor) };
            fra.fraDiscounting = fraInputRange.FraDiscounting;
            fra.fraDiscountingSpecified = true; 
            PartyReference party1 = PartyReferenceFactory.Create("party1");
            PartyReference party2 = PartyReferenceFactory.Create("party2");
            fra.sellerPartyReference = party1;
            fra.buyerPartyReference = party2;
            if (bool.Parse(fraInputRange.IsParty1Buyer))
            {
                fra.sellerPartyReference = party2;
                fra.buyerPartyReference = party1;
            }
            XsdClassesFieldResolver.TradeSetFra(trade, fra);
            trade.id = fraInputRange.TradeId;
            return trade;
        }

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="childValuations">The metrics.</param>
        /// <param name="controllerMetrics">THe controller metrics</param>
        /// <returns></returns>
        protected static AssetValuation AggregateCouponMetrics(List<AssetValuation> childValuations, List<string> controllerMetrics)
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

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        protected virtual Decimal AggregateCouponMetric(InstrumentMetrics metric)
        {
            string[] metrics = { metric.ToString() };
            var childValuations = GetChildValuations(GetInstumentControllers().ToArray(), new List<string>(metrics), ModelData);
            decimal result = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
            return result;
        }

        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            DateTime endDate = FloatingCoupon.AccrualEndDate;
            var bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromTerminationDate(baseDate, endDate, bucketInterval, RollConventionEnum.NONE, out _, out _));
            return bucketDates.ToArray();
        }

        #endregion

        #region Calculation

        /// <summary>
        /// Gets the par rate.
        /// </summary>
        /// <param name="paymentCalendar"> </param>
        /// <param name="fraInputRange">The fra input range.</param>
        /// <param name="logger"> </param>
        /// <param name="cache"> </param>
        /// <param name="forwardCurve"> </param>
        /// <param name="discountCurve"> </param>
        /// <param name="fixingCalendar"> </param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public double GetParRate(ILogger logger, ICoreCache cache, IRateCurve forwardCurve, IRateCurve discountCurve,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, FraInputRange fraInputRange, String nameSpace)
        {
            return GetParRateFromRange(logger, cache, forwardCurve, discountCurve, fixingCalendar, paymentCalendar, fraInputRange, nameSpace);
        }


        public double GetPrice(ILogger logger, ICoreCache cache, IRateCurve forwardCurve, IRateCurve discountCurve,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, FraInputRange fraInputRange, String nameSpace)
        {
            return GetPriceFromRange(logger, cache, forwardCurve, discountCurve, fixingCalendar, paymentCalendar, fraInputRange, nameSpace);
        }

        public string CacheFraTrade(ILogger logger, ICoreCache cache, string fraId, FraInputRange fraInputRange)
        {
            var fra = ProductFactory.GetFpMLFra(fraInputRange);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetFra(trade, fra);
            cache.SaveObject(trade, fraId, null);
            return fraId;
        }

        public string CacheFraTradeWithProperties(ILogger logger, ICoreCache cache, FraInputRange fraInputRange, NamedValueSet properties)
        {
            var fra = ProductFactory.GetFpMLFra(fraInputRange);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetFra(trade, fra);
            //Get the id.
            ProductIdentifier = new Identifiers.TradeIdentifier(properties);
            var fraId = ProductIdentifier.UniqueIdentifier;
            //Cache the trade.
            cache.SaveObject(trade, fraId, properties);
            return fraId;
        }

        public string CreateFraTradeValuation(ILogger logger, ICoreCache cache, 
            IRateCurve forwardCurve, IRateCurve discountCurve,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, 
            FraInputRange fraInputRange, string[] metrics,
            NamedValueSet properties, String nameSpace)
        {
            //get the balues reqired from the property bag.
            var valuationId = new ValuationReportIdentifier(properties);
            var baseParty = properties.GetString("BaseParty", true);
            var reportingCurrency = properties.GetString("ReportingCurrency", true);
            properties.Set("Function", "ValuationReport");
            properties.Set("Domain", "Orion.ValuationReport");
            //TODO add other properties
            //var fra = Cache.GetTrade(fraId);
            var fra = ProductFactory.GetFpMLFra(fraInputRange);
            //Get the curves and store.
            var marketFactory = new MarketFactory();
            var uniqueCurves = new List<IRateCurve>();
            //var forwardCurve = (RateCurve)ObjectCacheHelper.GetPricingStructureFromSerialisable(fraInputRange.ForwardCurveId);
            //var discountCurve = (RateCurve)ObjectCacheHelper.GetPricingStructureFromSerialisable(fraInputRange.DiscountingCurveId);
            var market = CreateMarket(discountCurve, forwardCurve);
            var agreement = new FraPricer(logger, cache, null, null, fra, nameSpace);
            var modelData = CreateInstrumentModelData(metrics, fraInputRange.ValuationDate, market, reportingCurrency);
            var asetValuation = agreement.Calculate(modelData);
            //  Add forward yield curve to the market environment ...
            //
            //var forwardCurve = (IRateCurve)ObjectCacheHelper.GetPricingStructureFromSerialisable(fraInputRange.ForwardCurveId);
            uniqueCurves.Add(forwardCurve);
            //  ... if discount curve is not the same as forward curve - add a discount curve too.
            //
            //if (fraInputRange.ForwardCurveId != fraInputRange.DiscountingCurveId)
            //{
            //    var discountingCurve = (IRateCurve)ObjectCacheHelper.GetPricingStructureFromSerialisable(fraInputRange.DiscountingCurveId);
            //    uniqueCurves.Add(discountingCurve);
            //}
            //TODO Add the FX curve if the reporting currency is different.
            foreach (var rateCurve in uniqueCurves)
            {
                // Add all unique curves into market
                //
                Pair<PricingStructure, PricingStructureValuation> pair = rateCurve.GetFpMLData();
                marketFactory.AddPricingStructure(pair);
            }
            var valuation = new Valuation();
            //  create ValuationReport and add it to in-memory collection.
            //
            valuation.CreateFraValuationReport(cache, nameSpace, valuationId.UniqueIdentifier, baseParty, fra, marketFactory.Create(), asetValuation, properties);
            return valuationId.UniqueIdentifier;
        }

        private static ISwapLegEnvironment CreateMarket(IPricingStructure discountCurve, IPricingStructure forwardCurve)
        {
            var market = new SwapLegEnvironment();
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forwardCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.ReportingCurrencyFxCurve.ToString(), null);
            return market;
        }

        private static double GetPriceFromRange(ILogger logger, ICoreCache cache, IRateCurve forwardCurve, IRateCurve discountCurve,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, FraInputRange fraInputRange, String nameSpace)
        {
            Fra fra = ProductFactory.GetFpMLFra(fraInputRange);
            //var forwardCurve = ObjectCacheHelper.GetPricingStructureFromSerialisable(fraInputRange.ForwardCurveId);
            //var discountCurve = (RateCurve)ObjectCacheHelper.GetPricingStructureFromSerialisable(fraInputRange.DiscountingCurveId);
            var market = CreateMarket(discountCurve, forwardCurve);
            var priceInMoney = GetNPV(logger, cache, fixingCalendar, paymentCalendar, fra, fraInputRange.ValuationDate, market, nameSpace);
            return priceInMoney;
        }

        private static double GetParRateFromRange(ILogger logger, ICoreCache cache, IRateCurve forwardCurve, IRateCurve discountCurve,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, FraInputRange fraInputRange, String nameSpace)
        {
            Fra fra = ProductFactory.GetFpMLFra(fraInputRange);
            //var forwardCurve = ObjectCacheHelper.GetPricingStructureFromSerialisable(fraInputRange.ForwardCurveId);
            //var discountCurve = (RateCurve)ObjectCacheHelper.GetPricingStructureFromSerialisable(fraInputRange.DiscountingCurveId);
            var market = CreateMarket(discountCurve, forwardCurve);
            var priceInMoney = GetParRate(logger, cache, fixingCalendar, paymentCalendar, fra, fraInputRange.ValuationDate, market, nameSpace);
            return priceInMoney;
        }

        #endregion

        #region Implementation of IPriceable

        /// <summary>
        /// Prices the product.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="fra"></param>
        /// <param name="valuationDate"></param>
        /// <param name="market"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static double GetNPV(ILogger logger, ICoreCache cache,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, 
            Fra fra, 
            DateTime valuationDate, 
            IMarketEnvironment market,
            String nameSpace)
        {
            var agreement = new FraPricer(logger, cache, fixingCalendar, paymentCalendar, fra, nameSpace);
            var modelData = CreateInstrumentModelData(new[] { "NPV" }, valuationDate, market, "AUD");
            var av = agreement.Calculate(modelData);
            return (double)av.quote[0].value;
        }

        /// <summary>
        /// Prices the product.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="fra"></param>
        /// <param name="valuationDate"></param>
        /// <param name="market"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static double GetParRate(ILogger logger, ICoreCache cache,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, 
            Fra fra, 
            DateTime valuationDate, 
            IMarketEnvironment market,
            String nameSpace)
        {
            var agreement = new FraPricer(logger, cache, fixingCalendar, paymentCalendar, fra, nameSpace);
            var modelData = CreateInstrumentModelData(new[] { "ImpliedQuote" }, valuationDate, market, "AUD");
            var av = agreement.Calculate(modelData);
            return (double)av.quote[0].value;
        }

        #endregion
    }
}