#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.Rates.Swaption;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.Util.Logging;
//Remove this and replace with a cap

#endregion

namespace Orion.ValuationEngine.Pricers.Products
{
    public class InterestRateSwaptionPricer : SwaptionPricer, IPriceableInterestRateSwaption<ISwaptionInstrumentParameters, ISwaptionInstrumentResults>
    {
        #region Members

        protected IDayCounter CDefaultDayCounter = Actual365.Instance;

        #endregion

        #region Constructors

        public InterestRateSwaptionPricer()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="swaptionFpML"> </param>
        /// <param name="basePartyReference"></param>
        /// <param name="underlyingSwap"> </param>
        public InterestRateSwaptionPricer(ILogger logger, ICoreCache cache,
            String nameSpace, Swaption swaptionFpML, string basePartyReference, 
            InterestRateSwapPricer underlyingSwap)
            : base(logger, cache, nameSpace, swaptionFpML, basePartyReference)
        {
            ProductType = ProductTypeSimpleEnum.InterestRateSwaption;
            Swap = underlyingSwap;
            //Add the currencies for the trade pricer.
            foreach (var ccy in Swap.PaymentCurrencies)
            {
                if (!PaymentCurrencies.Contains(ccy))
                {
                    PaymentCurrencies.Add(ccy);
                }
            }
            if (underlyingSwap.SwapType == SwapType.FixedFloat && underlyingSwap.ProductType == ProductTypeSimpleEnum.InterestRateSwap)
            {
                if (Swap.BasePartyPayingFixed && underlyingSwap.PayLeg.Strike != null)
                {
                    StrikeRate = (decimal)underlyingSwap.PayLeg.Strike;
                    if (IsBasePartyBuyer)
                    {
                        IsCall = true;
                    }
                }
                if (!Swap.BasePartyPayingFixed && underlyingSwap.ReceiveLeg.Strike != null)//
                {
                    StrikeRate = (decimal)underlyingSwap.ReceiveLeg.Strike;
                    if (!IsBasePartyBuyer)
                    {
                        IsCall = true;
                    }
                }
                VolatilitySurfaceName = CurveNameHelpers.GetRateVolatilityMatrixName(swaptionFpML.swap);
            }
            BucketedDates = new DateTime[] { };
            RiskMaturityDate = Swap.RiskMaturityDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="swaptionFpML"> </param>
        /// <param name="basePartyReference"></param>
        public InterestRateSwaptionPricer(ILogger logger, ICoreCache cache, String nameSpace,
            //List<Pair<IBusinessCalendar, IBusinessCalendar>> swapCalendars,
            Swaption swaptionFpML, string basePartyReference)
            : this(logger, cache, nameSpace, swaptionFpML, basePartyReference, false)
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="swaptionFpML"> </param>
        /// <param name="basePartyReference"></param>
        /// <param name="forecastRateInterpolation"></param>
        public InterestRateSwaptionPricer(ILogger logger, ICoreCache cache, String nameSpace,
            Swaption swaptionFpML, string basePartyReference, Boolean forecastRateInterpolation)
            : base(logger, cache, nameSpace, swaptionFpML, basePartyReference, forecastRateInterpolation)
        {
            ProductType = ProductTypeSimpleEnum.InterestRateSwaption;
            var underlyingSwap = new InterestRateSwapPricer(logger, cache, nameSpace, null, swaptionFpML.swap, basePartyReference, forecastRateInterpolation);
            Swap = underlyingSwap;
            //Add the currencies for the trade pricer.
            foreach (var ccy in Swap.PaymentCurrencies)
            {
                if (!PaymentCurrencies.Contains(ccy))
                {
                    PaymentCurrencies.Add(ccy);
                }
            }
            if (underlyingSwap.SwapType == SwapType.FixedFloat && underlyingSwap.ProductType == ProductTypeSimpleEnum.InterestRateSwap)
            {
                if (Swap.BasePartyPayingFixed && underlyingSwap.PayLeg.Strike != null)
                {
                    StrikeRate = (decimal)underlyingSwap.PayLeg.Strike;
                    if (!IsBasePartyBuyer)
                    {
                        IsCall = true;
                    }
                }
                if (!Swap.BasePartyPayingFixed && underlyingSwap.ReceiveLeg.Strike != null)//
                {
                    StrikeRate = (decimal)underlyingSwap.ReceiveLeg.Strike;
                    if (IsBasePartyBuyer)
                    {
                        IsCall = true;
                    }
                }
                VolatilitySurfaceName = CurveNameHelpers.GetRateVolatilityMatrixName(swaptionFpML.swap);
            }
            BucketedDates = new DateTime[] { };
            RiskMaturityDate = Swap.RiskMaturityDate;
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>


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
            // 1. First derive the analytics to be evaluated via the stream controller model 
            // NOTE: These take precendence of the child model metrics
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new SimpleIRSwaptionInstrumentAnalytic();
            }
            var swaptionControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            var quotes = ModelData.AssetValuation.quote.ToList();
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.BreakEvenRate.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.BreakEvenRate.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.NPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.NPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            ModelData.AssetValuation.quote = quotes.ToArray();
            AssetValuation swaptionValuation;
            //Sets the evolution type for calculations.
            Swap.PricingStructureEvolutionType = PricingStructureEvolutionType;
            Swap.BucketedDates = BucketedDates;
            if (PremiumPayments != null)
            {
                foreach (var payment in PremiumPayments)
                {
                    payment.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    payment.BucketedDates = BucketedDates;
                }
            }
            //The assetValuation list.
            var childValuations = new List<AssetValuation>();
            // 2. Now evaluate only the child specific metrics (if any)
            if (PremiumPayments != null)
            {
                var paymentControllers = new List<InstrumentControllerBase>(PremiumPayments);
                childValuations.AddRange(paymentControllers.Select(payment => payment.Calculate(modelData)));
            }
            var swapMetrics = Swap.Calculate(modelData);
            //We assume the fixed leg is always the first leg!
            var fixedLeg = Swap.GetLegs()[0].Calculate(modelData);
            var breakEvenRate = AssetValuationHelper.GetQuotationByMeasureType(swapMetrics, InstrumentMetrics.BreakEvenRate.ToString()).value;
            var timeToIndex = (Swap.EffectiveDate - ModelData.ValuationDate).Days / 365.0;
            //This is European only.
            var expiryTime = (ExerciseDates[0] - ModelData.ValuationDate).Days / 365.0;
            IVolatilitySurface indexVolSurface = null;
            if (modelData.MarketEnvironment is ISwapLegEnvironment streamMarket1)
            {
                indexVolSurface = streamMarket1.GetVolatilitySurface();
                indexVolSurface.PricingStructureEvolutionType = PricingStructureEvolutionType;
                VolatilitySurfaceName = indexVolSurface.GetPricingStructureId().UniqueIdentifier;
            }
            else
            {
                if (!string.IsNullOrEmpty(VolatilitySurfaceName))
                {
                    indexVolSurface = (IVolatilitySurface)modelData.MarketEnvironment.GetPricingStructure(VolatilitySurfaceName);
                }
            }
            //Calculate the delta
            var delta = SimpleIRSwaptionInstrumentAnalytic.CalculateOptionDelta(IsCall, breakEvenRate, StrikeRate, expiryTime, timeToIndex, indexVolSurface);
            //Set the multiplier using the delta of the option.
            //Multiplier = delta;?
            Swap.Multiplier = System.Math.Abs(delta);
            //New function that converts the metrics by multiplying be the delta.
            var swapCalculations = Swap.Calculate(modelData);
            //childValuations.Add(swapCalculations);
            var childControllerValuations = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            var streamAccrualFactor = AssetValuationHelper.GetQuotationByMeasureType(fixedLeg, InstrumentMetrics.AccrualFactor.ToString());//TODO This is not correct!
            var npv = AssetValuationHelper.GetQuotationByMeasureType(childControllerValuations, InstrumentMetrics.NPV.ToString());
            childValuations.Add(swapCalculations);
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (swaptionControllerMetrics.Count > 0)
            {
                //Get the market data.
                IFxCurve fxCurve = null;
                ISwaptionInstrumentParameters analyticModelParameters = new SwaptionInstrumentParameters
                {
                    IsBought = IsBasePartyBuyer,
                    IsCall = IsCall,
                    SwapAccrualFactor = System.Math.Abs(streamAccrualFactor.value),
                    Strike = StrikeRate,
                    OtherNPV = npv.value,
                    TimeToExpiry = (decimal)expiryTime,
                    SwapBreakEvenRate = breakEvenRate,
                    //OtherNPV = 
                };
                // Curve Related
                if (modelData.MarketEnvironment is ISwapLegEnvironment streamMarket)
                {
                    analyticModelParameters.VolatilitySurface = indexVolSurface;
                    //Check for currency.
                    if (ModelData.ReportingCurrency != null)
                    {
                        if (ModelData.ReportingCurrency.Value != PaymentCurrencies[0])//This is an interest rate swap and so only has one currency.
                        {
                            fxCurve = streamMarket.GetReportingCurrencyFxCurve();
                            fxCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                        }
                    }
                }
                var analyticsModel = new SimpleIRSwaptionInstrumentAnalytic(ModelData.ValuationDate,  (decimal)timeToIndex,
                    StrikeRate, fxCurve, indexVolSurface);
                AnalyticsModel = analyticsModel;
                Volatility = analyticsModel.Volatility;
                AnalyticModelParameters = analyticModelParameters;
                CalculationResults = AnalyticsModel.Calculate<ISwaptionInstrumentResults, SwaptionInstrumentResults>(analyticModelParameters, swaptionControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var swapControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                //childValuations.Add(swapControllerValuation);
                childControllerValuations = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
                swaptionValuation = AssetValuationHelper.UpdateValuation(swapControllerValuation,
                                                                     childControllerValuations, ConvertMetrics(swaptionControllerMetrics), new List<string>(Metrics));
                //swaptionValuation = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), modelData.ValuationDate);

            }
            else
            {
                swaptionValuation = childControllerValuations;
            }
            CalculationPerfomedIndicator = true;
            swaptionValuation.id = Id;
            return swaptionValuation;
        }

        #endregion

        #region Metrics for Valuation

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        protected virtual Decimal GetPaymentYearFraction(DateTime startDate, DateTime endDate)
        {
            return (Decimal)CDefaultDayCounter.YearFraction(startDate, endDate);
        }

        /// <summary>
        /// Gets the bucketed discount factors.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="bucketDates">The bucket dates.</param>
        /// <returns></returns>
        protected static Decimal[] GetBucketedDiscountFactors(IRateCurve curve, DateTime valuationDate,
                                                              IList<DateTime> bucketDates)
        {
            // Bucketed Delta

            return Array.ConvertAll(bucketDates.Select((t, index) => index == 0 ? 1.0 : curve.GetDiscountFactor(valuationDate, t)).ToArray(), Convert.ToDecimal);
        }

/*
        private static AssetValuation MultiplyByDelta(AssetValuation swapMetrics, decimal delta)
        {
            var result = new AssetValuation
                             {
                                 quote = (from metric in swapMetrics.quote
                                          where
                                              metric.measureType.Value != InstrumentMetrics.BreakEvenRate.ToString() ||
                                              metric.measureType.Value !=
                                              InstrumentMetrics.BreakEvenStrike.ToString() ||
                                              metric.measureType.Value != InstrumentMetrics.ImpliedQuote.ToString() ||
                                              metric.measureType.Value != InstrumentMetrics.MarketQuote.ToString()
                                          select
                                              QuotationHelper.Create(metric.value*System.Math.Abs(delta),
                                                                     metric.measureType.Value)).ToArray()
                             };
            return result;
        }
*/

        #endregion
    }
}