#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.CurveEngine.Markets;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Schedulers;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.Models.Assets;
using IRSwapAssetParameters=Orion.Models.Assets.IRSwapAssetParameters;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.CurveEngine.Assets
{
    public enum CurvePerturbation
    {
        /// <summary>
        /// The forecast curve.
        /// </summary>
        ForecastCurve,

        /// <summary>
        /// The discount curve
        /// </summary>
        DiscountCurve,

        /// <summary>
        /// Both curves
        /// </summary>
        Both
    }

    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableIRSwap : PriceableSimpleIRSwap, IPriceableClearedRateAssetController
    {
        #region Properties

        ///<summary>
        ///</summary>
        public const string QuotationType = "MarketQuote";

        /// <summary>
        /// The spread quotation
        /// </summary>
        public BasicQuotation FloatingLegSpread { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal[] FloatingLegYearFractions = { 0.25m };

        /// <summary>
        /// 
        /// </summary>
        public Decimal[] FloatingLegWeightings = { 1.0m };

        /// <summary>
        /// 
        /// </summary>
        public Decimal[] ForwardRates { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<DateTime> FloatingLegAdjustedPeriodDates { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DayCountFraction FloatingLegDayCountFraction => Calculation.dayCountFraction;

        /// <summary>
        /// 
        /// </summary>
        public FloatingRateCalculation FloatingRateCalculation => XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(Calculation);

        /// <summary>
        /// The forecast curve, normally LIBOR
        /// </summary>
        public String ForecastCurveName { get; set; }

        #endregion

        #region Constructors - Dual legs

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="amount">The ammount.</param>
        /// <param name="discountingType">The discounting type.</param>
        /// <param name="effectiveDate">The base date.</param>
        /// <param name="tenor">The maturity tenor.</param>
        /// <param name="fxdDayFraction">The fixed leg day fraction.</param>
        /// <param name="businessCenters">The payment business centers.</param>
        /// <param name="businessDayConvention">The payment business day convention.</param>
        /// <param name="fxdFrequency">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="currency">THe currency.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableIRSwap(string id, DateTime baseDate, string currency,
            decimal amount, DiscountingTypeEnum? discountingType,
            DateTime effectiveDate, string tenor, string fxdDayFraction,
            string businessCenters, string businessDayConvention, string fxdFrequency,
            RateIndex underlyingRateIndex, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar,
            BasicQuotation fixedRate)
            : base(baseDate, SimpleIrsHelper.Parse(id, currency, fxdDayFraction, tenor, fxdFrequency, id),
                effectiveDate,
                CalculationFactory.CreateFixed(fixedRate.value, MoneyHelper.GetAmount(amount, currency),
                    DayCountFractionHelper.Parse(fxdDayFraction), discountingType),
                BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters), underlyingRateIndex,
                fixingCalendar, paymentCalendar, fixedRate)
        {
            ModelIdentifier = DiscountingType == null ? "SwapAsset" : "DiscountSwapAsset";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="simpleIRSwap">This contains the minimal information for the swap.</param>
        /// <param name="spotDateOffset">All necessary data related to the spot period for the swap type.</param>
        /// <param name="calculation">Contians information related to the floating leg.</param>
        /// <param name="businessDayAdjustments">The business day adjustments for all payments.</param>
        /// <param name="underlyingRateIndex">Index of the floating leg.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableIRSwap(DateTime baseDate, SimpleIRSwap simpleIRSwap, RelativeDateOffset spotDateOffset,
                                    Calculation calculation, BusinessDayAdjustments businessDayAdjustments, RateIndex underlyingRateIndex,
                                      IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, simpleIRSwap, spotDateOffset, calculation, businessDayAdjustments, underlyingRateIndex, fixingCalendar, paymentCalendar, fixedRate)
        {
            ModelIdentifier = DiscountingType == null ? "SwapAsset" : "DiscountSwapAsset";
            //The floating leg which is now non-zero.
            var unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleIRSwap.term, UnderlyingRateIndex.term);
            FloatingLegAdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, BusinessDayAdjustments.businessDayConvention, fixingCalendar);
            FloatingLegWeightings = CreateWeightings(CDefaultWeightingValue, FloatingLegAdjustedPeriodDates.Count - 1);
            FloatingLegYearFractions = GetFloatingLegYearFractions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct;</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableIRSwap(DateTime baseDate, SimpleIRSwapNodeStruct nodeStruct,
                                     IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, nodeStruct, fixingCalendar, paymentCalendar, fixedRate)
        {
            ModelIdentifier = DiscountingType == null ? "SwapAsset" : "DiscountSwapAsset";
            //The floating leg which is now non-zero.
            var unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleIRSwap.term, UnderlyingRateIndex.term);
            FloatingLegAdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, BusinessDayAdjustments.businessDayConvention, fixingCalendar);
            FloatingLegWeightings = CreateWeightings(CDefaultWeightingValue, FloatingLegAdjustedPeriodDates.Count - 1);
            FloatingLegYearFractions = GetFloatingLegYearFractions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="simpleIRSwap"></param>
        /// <param name="spotDate"></param>
        /// <param name="calculation"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableIRSwap(DateTime baseDate, SimpleIRSwap simpleIRSwap,
                                     DateTime spotDate, Calculation calculation,
                                     BusinessDayAdjustments businessDayAdjustments, RateIndex underlyingRateIndex,
                                     IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, simpleIRSwap, spotDate, calculation, businessDayAdjustments, underlyingRateIndex,
                                     fixingCalendar, paymentCalendar, fixedRate)
        {
            ModelIdentifier = DiscountingType == null ? "SwapAsset" : "DiscountSwapAsset";
            //The floating leg which is now non-zero.
            var unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleIRSwap.term, UnderlyingRateIndex.term);
            FloatingLegAdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, BusinessDayAdjustments.businessDayConvention, fixingCalendar);
            FloatingLegWeightings = CreateWeightings(CDefaultWeightingValue, FloatingLegAdjustedPeriodDates.Count - 1);
            FloatingLegYearFractions = GetFloatingLegYearFractions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="simpleIRSwap">THe FpML swap.</param>
        /// <param name="spotDate">The spot date.</param>
        /// <param name="calculation">A calculation.</param>
        /// <param name="stringRollConvention">The roll convention.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableIRSwap(DateTime baseDate, SimpleIRSwap simpleIRSwap,
                                     DateTime spotDate, Calculation calculation, String stringRollConvention,
                                     BusinessDayAdjustments businessDayAdjustments, RateIndex underlyingRateIndex,
                                     IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, simpleIRSwap, spotDate, calculation, stringRollConvention,
                                      businessDayAdjustments,  underlyingRateIndex, fixingCalendar,  paymentCalendar, fixedRate)
        {
            ModelIdentifier = DiscountingType == null ? "SwapAsset" : "DiscountSwapAsset";
            //The floating leg which is now non-zero.
            var unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleIRSwap.term, UnderlyingRateIndex.term);
            FloatingLegAdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, BusinessDayAdjustments.businessDayConvention, fixingCalendar);
            FloatingLegWeightings = CreateWeightings(CDefaultWeightingValue, FloatingLegAdjustedPeriodDates.Count - 1);
            FloatingLegYearFractions = GetFloatingLegYearFractions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="fixedLegSwap">The fixed leg details.</param>
        /// <param name="spotDate">the spot date.</param>
        /// <param name="notional">The notional amount.</param>
        /// <param name="paymentBusinessDayAdjustments">The business day adjustments.</param>
        /// <param name="floatingLegSwap">The floating leg details.</param>
        /// <param name="floatingLegcalculation">The floatingLegcalculation.</param>
        /// <param name="fixingDateOffset">The fixing date business day adjustments.</param>
        /// <param name="resetRates">The reset rates of the floating leg - if any.</param>
        /// <param name="fixingCalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="spread">The spread on the floating leg.</param>
        public PriceableIRSwap(DateTime baseDate, SimpleIRSwap fixedLegSwap, DateTime spotDate, MoneyBase notional,
                               BusinessDayAdjustments paymentBusinessDayAdjustments, SimpleIRSwap floatingLegSwap, 
                               Calculation floatingLegcalculation, RelativeDateOffset fixingDateOffset, List<Decimal> resetRates, 
                               IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate, BasicQuotation spread)
            : base(baseDate, fixedLegSwap, fixingDateOffset, floatingLegcalculation, paymentBusinessDayAdjustments, null, fixingCalendar, paymentCalendar, fixedRate)
        {
            ModelIdentifier = DiscountingType == null ? "SwapAsset" : "DiscountSwapAsset";
            FloatingLegSpread = GetSpread(spread);
            ForwardRates = resetRates?.ToArray();
            UnderlyingRateIndex = RateIndexHelper.Parse(FloatingRateCalculation.floatingRateIndex.Value, notional.currency.Value, Calculation.dayCountFraction.Value);
            var unadjustedFloatingDates = DateScheduler.GetUnadjustedDateSchedule(spotDate, floatingLegSwap.term, floatingLegSwap.paymentFrequency);
            FloatingLegAdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedFloatingDates, paymentBusinessDayAdjustments.businessDayConvention, paymentCalendar);
            FloatingLegWeightings = CreateWeightings(CDefaultWeightingValue, FloatingLegAdjustedPeriodDates.Count-1);
            FloatingLegYearFractions = GetFloatingLegYearFractions();
        }
      
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="fixedDates">The fixed side dates.</param>
        /// <param name="floatingDates">The floating side dates.</param>
        /// <param name="resetRates">An array of override reset dates.</param>
        /// <param name="fixedCalculation">The fixed side calculation.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="floatingCalculation">The floating leg calculation.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="floatingSpread">The floating leg spread.</param>
        public PriceableIRSwap(DateTime baseDate, List<DateTime> fixedDates,
            List<DateTime> floatingDates, List<Decimal> resetRates,
            Calculation fixedCalculation, BusinessDayAdjustments businessDayAdjustments, 
            Calculation floatingCalculation, RateIndex underlyingRateIndex, 
            BasicQuotation fixedRate, BasicQuotation floatingSpread)
            : base(baseDate, null, fixedDates.ToArray(), null, floatingCalculation, 
                  businessDayAdjustments, underlyingRateIndex, fixedRate)
        {
            ModelIdentifier = DiscountingType == null ? "SwapAsset" : "DiscountSwapAsset";
            Id = "Unidentified";
            ForwardRates = resetRates?.ToArray();
            FloatingLegSpread = floatingSpread;
            FloatingLegAdjustedPeriodDates = floatingDates;
            //Exract the notional weights from the fixed calculation.
            var fixedNotionalSchedule = (Notional)fixedCalculation.Item;
            Weightings = CreateWeightingsFromNonNegativeNotionalSchedule(fixedNotionalSchedule.notionalStepSchedule, fixedDates);
            //Extract the floating weights from the floating calculation.
            var floatingNotionalSchedule = (Notional)Calculation.Item;
            FloatingLegWeightings = CreateWeightingsFromNonNegativeNotionalSchedule(floatingNotionalSchedule.notionalStepSchedule, floatingDates);
            FloatingLegYearFractions = GetFloatingLegYearFractions();
        }

        #endregion

        #region IAssetController Interface

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            switch (ModelIdentifier)
            {
                case "SwapAsset":
                    AnalyticsModel = new SwapAssetAnalytic();
                    break;
                case "DiscountSwapAsset":
                    AnalyticsModel = new DiscountSwapAssetAnalytic();
                    break;
            }
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalDiscountFactorAtMaturity = false;
            if (metrics.Contains(RateMetrics.DiscountFactorAtMaturity))
            {
                bEvalDiscountFactorAtMaturity = true;
                metrics.RemoveAll(
                    metricItem => metricItem != RateMetrics.DiscountFactorAtMaturity);
            }
            var metricsToEvaluate = metrics.ToArray();
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters();
            AnalyticResults = new RateAssetResults();
            var marketEnvironment = modelData.MarketEnvironment;
            IRateCurve curve = null;
            IRateCurve forecastCurve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                curve = (IRateCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
                ForecastCurveName = CurveName;
            }
            if (marketEnvironment.GetType() == typeof(SimpleRateMarketEnvironment))
            {
                curve = ((ISimpleRateMarketEnvironment)marketEnvironment).GetRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
                ForecastCurveName = CurveName;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                curve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
                forecastCurve = ((ISwapLegEnvironment)marketEnvironment).GetForecastRateCurve();
                ForecastCurveName = forecastCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                curve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(CurveName);
                if (ForecastCurveName != null)
                {
                    forecastCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(ForecastCurveName);
                }
            }
            if (forecastCurve == null)
            {
                forecastCurve = curve;
            }
            //2. Set the rate
            analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            if (bEvalDiscountFactorAtMaturity)
            {
                //3. Set the start diccount factor
                analyticModelParameters.StartDiscountFactor =
                    GetDiscountFactor(curve, AdjustedStartDate, modelData.ValuationDate);
                //4. Get the respective year fractions
                analyticModelParameters.YearFractions = YearFractions;
                //5. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            else
            {
                analyticModelParameters.NotionalAmount = Notional;
                //1. Get any rates that have reset.
                if(ForwardRates!=null)
                {
                    analyticModelParameters.FloatingLegForwardRates = ForwardRates;
                }
                //2. Get the discount factors
                analyticModelParameters.DiscountFactors =
                    GetDiscountFactors(curve, AdjustedPeriodDates.ToArray(), modelData.ValuationDate);
                //3. Get the respective year fractions
                analyticModelParameters.YearFractions = YearFractions;
                //4. Get the Weightings
                analyticModelParameters.Weightings = Weightings;
                //5. Get the respective year fractions
                analyticModelParameters.FloatingLegYearFractions = FloatingLegYearFractions;
                //6. Get the Weightings
                analyticModelParameters.FloatingLegWeightings = FloatingLegWeightings;
                //7. Get the floating discount factors
                analyticModelParameters.FloatingLegDiscountFactors =
                    GetDiscountFactors(curve, FloatingLegAdjustedPeriodDates.ToArray(), modelData.ValuationDate);
                //8. Get the forecast curve discount factors.
                analyticModelParameters.FloatingLegForecastDiscountFactors =
                            GetDiscountFactors(forecastCurve, FloatingLegAdjustedPeriodDates.ToArray(), modelData.ValuationDate);
                //9. Get the Spread
                analyticModelParameters.FloatingLegSpread = FloatingLegSpread?.value ?? 0.0m;
                //10. Set the anaytic input parameters and Calculate the respective metrics            
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <param name="discountedSpace">The OIS Space.</param>
        /// <returns></returns>
        public Decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace, IInterpolatedSpace discountedSpace)
        {
            switch (ModelIdentifier)
            {
                case "SwapAsset":
                    AnalyticsModel = new SwapAssetAnalytic();
                    break;
                case "DiscountSwapAsset":
                    AnalyticsModel = new DiscountSwapAssetAnalytic();
                    break;
            }
            //1. Set the basic parameters.
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters
            {
                NotionalAmount = Notional,
                //2. Get the discount factors
                DiscountFactors = GetDiscountFactors(discountedSpace, AdjustedPeriodDates.ToArray(), BaseDate),
                //3. Get the respective year fractions
                YearFractions = YearFractions,
                Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value
            };
            //2. Get any rates that have reset.
            if (ForwardRates != null)
            {
                analyticModelParameters.FloatingLegForwardRates = ForwardRates;
            }
            //4. Get the Weightings
            analyticModelParameters.Weightings =
                CreateWeightings(CDefaultWeightingValue, analyticModelParameters.DiscountFactors.Length - 1);
            analyticModelParameters.FloatingLegDiscountFactors =
                GetDiscountFactors(discountedSpace, FloatingLegAdjustedPeriodDates.ToArray(), BaseDate);
            //6. Get the respective fixed leg year fractions
            analyticModelParameters.FloatingLegYearFractions = FloatingLegYearFractions;
            //7. Get the Fixed Leg Weightings
            analyticModelParameters.FloatingLegWeightings = FloatingLegWeightings;
            //8. Get the forecast curve discount factors.
            analyticModelParameters.FloatingLegForecastDiscountFactors =
                GetDiscountFactors(interpolatedSpace, FloatingLegAdjustedPeriodDates.ToArray(), BaseDate);
            AnalyticResults = new RateAssetResults();
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            if (AnalyticsModel != null)
                AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="discountCurve">The discount curve.</param>
        /// <param name="forecastCurve">The forecast curve</param>
        /// <param name="curveToPerturb">The curve to perturb: the discount curve, the forecast curve or both.</param>
        /// <returns></returns>
        public IDictionary<string, Decimal> CalculatePDH(CurveBase discountCurve, CurveBase forecastCurve, CurvePerturbation curveToPerturb)
        {
            if (AnalyticsModel == null)
            {
                switch (ModelIdentifier)
                {
                    case "SwapAsset":
                        AnalyticsModel = new SwapAssetAnalytic();
                        break;
                    case "DiscountSwapAsset":
                        AnalyticsModel = new DiscountSwapAssetAnalytic();
                        break;
                }
            }
            var result = new Dictionary<string, Decimal>();
            if (discountCurve != null && forecastCurve != null)
            {
                ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters
                {
                    NotionalAmount = Notional,
                    //2. Get the discount factors
                    DiscountFactors =
                        GetDiscountFactors(discountCurve,
                                           AdjustedPeriodDates.ToArray(),
                                           BaseDate),
                    //3. Get the respective year fractions
                    YearFractions = YearFractions,
                    Weightings = Weightings,
                    Rate =
                        MarketQuoteHelper.NormalisePriceUnits(
                        FixedRate, "DecimalRate").value,
                    FloatingLegDiscountFactors =
                    GetDiscountFactors(discountCurve,
                                       FloatingLegAdjustedPeriodDates.ToArray(),
                                       BaseDate),
                    FloatingLegForecastDiscountFactors =
                        GetDiscountFactors(forecastCurve, FloatingLegAdjustedPeriodDates.ToArray(), BaseDate),
                    FloatingLegYearFractions = FloatingLegYearFractions,
                    FloatingLegWeightings = FloatingLegWeightings,
                    FloatingLegSpread =
                        MarketQuoteHelper.NormalisePriceUnits(
                        FloatingLegSpread, "DecimalRate").value
                };
                if (ForwardRates != null)
                {
                    analyticModelParameters.FloatingLegForwardRates = ForwardRates;
                }
                //4. Set the analytic input parameters and Calculate the respective metrics
                //
                if (AnalyticsModel != null)
                {
                    var analyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.NPV });
                    AnalyticResults = analyticResults;
                    analyticModelParameters.BaseNPV = analyticResults.NPV;
                    //Now loop through the risk curves.
                    if(curveToPerturb == CurvePerturbation.DiscountCurve)
                    {
                        var riskCurves = discountCurve.CreateCurveRiskSet(1);
                        foreach (var curve in riskCurves)
                        {
                            var perturbedAsset = curve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset");
                            analyticResults = RiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                            result.Add("DiscountCurve:" + perturbedAsset, analyticResults.NPVChange);
                        }
                    }
                    if (curveToPerturb == CurvePerturbation.ForecastCurve)
                    {
                        var riskCurves = forecastCurve.CreateCurveRiskSet(1);
                        foreach (var curve in riskCurves)
                        {
                            var perturbedAsset = curve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset");
                            analyticResults = ForecastRiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                            result.Add("ForecastCurve:" + perturbedAsset, analyticResults.NPVChange);
                        }
                    }
                    if (curveToPerturb == CurvePerturbation.Both)
                    {
                        var riskCurves1 = discountCurve.CreateCurveRiskSet(1);
                        foreach (var curve in riskCurves1)
                        {
                            var perturbedAsset = curve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset");
                            analyticResults = RiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                            result.Add("DiscountCurve:" + perturbedAsset, analyticResults.NPVChange);
                        }
                        var riskCurves2 = forecastCurve.CreateCurveRiskSet(1);
                        foreach (var curve in riskCurves2)
                        {
                            var perturbedAsset = curve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset");
                            analyticResults = ForecastRiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                            result.Add("ForecastCurve:" + perturbedAsset, analyticResults.NPVChange);
                        }
                    }
                }
            }
            return result;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Sets the rate.
        /// </summary>
        /// <param name="spread">The spread.</param>
        private static BasicQuotation GetSpread(BasicQuotation spread)
        {
            if (String.Compare(spread.measureType.Value, "Spread", StringComparison.OrdinalIgnoreCase) == 0)
            {
                spread.measureType.Value = "Spread";
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }
            return spread.measureType.Value == "Spread" ? spread : null;
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public decimal[] GetFloatingLegYearFractions()
        {
            return GetYearFractionsForDates(FloatingLegAdjustedPeriodDates, Calculation.dayCountFraction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="discountCurve"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected override IRateAssetResults RiskCalculationHelper(IRateCurve discountCurve, ISwapAssetParameters parameters)
        {
            parameters.FloatingLegDiscountFactors =
                GetDiscountFactors(discountCurve,
                                   AdjustedPeriodDates.ToArray(),
                                   BaseDate);
            parameters.DiscountFactors =
                GetDiscountFactors(discountCurve,
                                   FloatingLegAdjustedPeriodDates.ToArray(),
                                   BaseDate);
            //Set the analytic input parameters and Calculate the respective metrics
            //
            return AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(parameters, new[] { RateMetrics.NPVChange });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forecastcurve"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected IRateAssetResults ForecastRiskCalculationHelper(IRateCurve forecastcurve, ISwapAssetParameters parameters)
        {
            parameters.FloatingLegForecastDiscountFactors = GetDiscountFactors(forecastcurve,
                AdjustedPeriodDates.ToArray(),
                BaseDate);
            //Set the anaytic input parameters and Calculate the respective metrics
            //
            return AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(parameters, new[] { RateMetrics.NPVChange });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notionalStepSchedule"></param>
        /// <param name="dates"></param>
        /// <returns></returns>
        protected static decimal[] CreateWeightingsFromNonNegativeNotionalSchedule(NonNegativeAmountSchedule notionalStepSchedule, IList<DateTime> dates)
        {
            var result = new List<Decimal>();
            var initalNotional = notionalStepSchedule.initialValue;
            if (null != notionalStepSchedule.step)//there should be steps - otherwise NO interm. exchanges.
            {
                result.AddRange(dates.Select(date => ScheduleHelper.GetValue(notionalStepSchedule, date)).Select(stepAmount => stepAmount/initalNotional));
            }
            return result.ToArray();
        }

        #endregion
    }
}