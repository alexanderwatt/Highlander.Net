#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.Assets;
using FpML.V5r10.Reporting.Models.Rates.Options;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Interpolations.Points;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Markets;

#endregion

namespace Orion.CurveEngine.Assets.Rates.CapsFloors
{
    ///<summary>
    ///</summary>
    public abstract class PriceableCapRateAsset : PriceableRateOptionAssetController
    {
        ///<summary>
        ///</summary>
        public const string VolatilityQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        public string ModelIdentifier { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime RiskMaturityDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime OptionsExpiryDate { get; set; }

        ///<summary>
        ///</summary>
        public Boolean IsCap { get; set; }

        ///<summary>
        ///</summary>
        public Boolean IsVolatilityQuote { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IncludeFirstPeriod { get; set; }

        /// <summary>
        /// Gets and sets the volatility.
        /// </summary>
        public decimal Volatility { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public StrikeQuoteUnitsEnum StrikeQuoteUnits { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public MeasureTypesEnum MeasureType { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public QuoteUnitsEnum QuoteUnits { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<IRateOptionAssetParameters, RateOptionMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IRateOptionAssetResults AnalyticResults { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime AdjustedStartDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<DateTime> AdjustedPeriodDates { get; set; }

        /// <summary>
        /// The fixing dates.
        /// </summary>
        public List<DateTime> ExpiryDates { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BusinessDayAdjustments PaymentBusinessDayAdjustments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset FixingBusinessDayOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Calculation Calculation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<double> YearFractions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<double> ResetRates { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal VolatilityAtExpiry => Volatility;

        /// <summary>
        /// 
        /// </summary>
        public Decimal ParRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SimpleIRSwap SimpleIRCap { get; set; }

        /// <summary>
        /// Gets the index of the underlying rate.
        /// </summary>
        /// <value>The index of the underlying rate.</value>
        public FloatingRateIndex UnderlyingRateIndex => (FloatingRateIndex)Calculation.Items[0];

        /// <summary>
        /// Returns the strikes.
        /// </summary>
        public List<double> Strikes { get; set; }

        /// <summary>
        /// Returns the strikes.
        /// </summary>
        public decimal? Strike { get; set; }

        /// <summary>
        /// Returns the notionals.
        /// </summary>
        public List<double> Notionals { get; set; }

        /// <summary>
        /// Returns the times to expiry.
        /// </summary>
        public List<double> TimesToExpiry { get; set; }

        /// <summary>
        /// Returns the forward rates.
        /// </summary>
        public List<double> ForwardRates { get; set; }

        /// <summary>
        /// Returns the volatilties.
        /// </summary>
        public List<double> Volatilities { get; set; }

        /// <summary>
        /// Returns the hasSet flag.
        /// </summary>
        public List<bool> HasSetFlag { get; set; }

        /// <summary>
        /// Returns the last expiry date.
        /// </summary>
        public override DateTime GetExpiryDate() => OptionsExpiryDate;

        /// <summary>
        /// Gets the vol at maturity.
        /// </summary>
        /// <value>The vol at maturity.</value>
        public override decimal VolatilityAtRiskMaturity => AnalyticResults == null ? VolatilityAtExpiry : 
            (decimal)AnalyticResults.VolatilityAtExpiry[AnalyticResults.VolatilityAtExpiry.Count - 1];

        /// <summary>
        /// Gets or sets the name of the forecast curve.
        /// </summary>
        /// <value>The name of the forecast curve.</value>
        public string ForecastCurveName { get; set; }

        /// <summary>
        /// Gets or sets the forecast curve.
        /// </summary>
        /// <value>The forecast curve.</value>
        public IRateCurve ForecastCurve { get; set; }

        /// <summary>
        /// Gets or sets the name of the discount curve.
        /// </summary>
        /// <value>The name of the discount curve.</value>
        public string DiscountCurveName { get; set; }

        /// <summary>
        /// Gets or sets the discount curve.
        /// </summary>
        /// <value>The discount curve.</value>
        public IRateCurve DiscountCurve { get; set; }

        /// <summary>
        /// Gets or sets the discount curve.
        /// </summary>
        /// <value>The discount curve.</value>
        public IVolatilitySurface VolatilityCurve { get; set; }

        /// <summary>
        /// Gets or sets the name of the volatility curve.
        /// </summary>
        /// <value>The name of the volatility curve.</value>
        public string VolatilityCurveName { get; set; }

        /// <summary>
        /// Gets the initial notional amount.
        /// </summary>
        /// <value>The notional.</value>
        public decimal InitialNotional
        {
            get
            {
                var amount = (Notional) Calculation.Item;
                return amount.notionalStepSchedule.initialValue;
            }
        }

        /// <summary>
        /// Gets the premium.
        /// </summary>
        /// <value>The rate.</value>
        public decimal? Premium { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCapRateAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="paymentBusinessDayAdjustments">The payment business day adjustments.</param>
        /// <param name="resetBusinessDayAdjustments">The fixing business day adsjustments.</param>
        /// <param name="calculation">The calculation class that determines if the swap is discounted or not.</param>
        /// <param name="marketQuotes">The market Quote. This could be premium, normal volatility or lognormalvolatility as defined by the price quote units.</param>
        protected PriceableCapRateAsset(DateTime baseDate, BusinessDayAdjustments paymentBusinessDayAdjustments,
                                        RelativeDateOffset resetBusinessDayAdjustments, Calculation calculation,
                                        BasicAssetValuation marketQuotes)
        {
            ModelIdentifier = "CapAsset";
            BaseDate = baseDate;
            Calculation = calculation;
            PaymentBusinessDayAdjustments = paymentBusinessDayAdjustments;
            FixingBusinessDayOffset = resetBusinessDayAdjustments;
            if (marketQuotes != null)
            {
                var quotes = new List<BasicQuotation>(marketQuotes.quote);
                SetQuote(VolatilityQuotationType, quotes);
            }
        }

        /// <summary>
        /// Sets the marketQuote.
        /// </summary>
        /// <param name="quotes">The quotes.</param>
        /// <param name="measureType">The measure type.</param>
        protected void SetQuote(String measureType, IList<BasicQuotation> quotes)
        {
            BasicQuotation normalisedQuote = MarketQuoteHelper.GetMarketQuoteAndNormalise(measureType, quotes);
            IsVolatilityQuote = true;
            if (normalisedQuote != null)
            {
                if (normalisedQuote.measureType.Value == VolatilityQuotationType)
                {
                    MarketQuote = normalisedQuote;
                    Volatility = normalisedQuote.value;
                }
                if (measureType == "SpotRate")
                {
                    ParRate = normalisedQuote.value;
                }
                if (measureType == "Strike")
                {
                    Strike = normalisedQuote.value;
                }
                if (measureType == "Premium")
                {
                    Premium = normalisedQuote.value;
                }
            }
        }

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
                case "CapAsset":
                    AnalyticsModel = new RateOptionAssetAnalytic();
                    break;
                case "DiscountCapAsset":
                    AnalyticsModel = new RateOptionAssetAnalytic();
                    break;
            }
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalLastForewardVolatiltiy = false;
            if (metrics.Contains(RateOptionMetrics.VolatilityAtExpiry))
            {
                bEvalLastForewardVolatiltiy = true;
                metrics.RemoveAll(
                    metricItem => metricItem != RateOptionMetrics.VolatilityAtExpiry);
            }
            var metricsToEvaluate = metrics.ToArray();
            IRateOptionAssetParameters analyticModelParameters = new RateOptionAssetParameters();
            AnalyticResults = new RateOptionAssetResults();
            var marketEnvironment = modelData.MarketEnvironment;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                DiscountCurve = (IRateCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                DiscountCurveName = DiscountCurve.GetPricingStructureId().UniqueIdentifier;
                ForecastCurve = DiscountCurve;
                ForecastCurveName = DiscountCurveName;
                var volatilities = CreateList((double)Volatility, TimesToExpiry.Count);
                analyticModelParameters.Volatilities = volatilities;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                DiscountCurve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
                DiscountCurveName = DiscountCurve.GetPricingStructureId().UniqueIdentifier;
                ForecastCurve = ((ISwapLegEnvironment)marketEnvironment).GetForecastRateCurve();
                ForecastCurveName = ForecastCurve.GetPricingStructureId().UniqueIdentifier;
                VolatilityCurve = ((ISwapLegEnvironment)marketEnvironment).GetVolatilitySurface();
                VolatilityCurveName = VolatilityCurve.GetPricingStructureId().UniqueIdentifier;
                analyticModelParameters.Volatilities = GetVolatilties(VolatilityCurve, TimesToExpiry, Strikes);

            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                if(DiscountCurveName != null)
                { DiscountCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(DiscountCurveName);}
                if (ForecastCurveName != null)
                { ForecastCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(ForecastCurveName);}
                if (VolatilityCurveName != null)
                { VolatilityCurve = (IVolatilitySurface)modelData.MarketEnvironment.GetPricingStructure(VolatilityCurveName);}
                analyticModelParameters.Volatilities = GetVolatilties(VolatilityCurve, TimesToExpiry, Strikes);
            }
            analyticModelParameters.FlatVolatility = Volatility;
            //2. Set the premium
            if (Premium == null)
            {
                Premium = CalculatePremium();
            }
            if (Premium != null) analyticModelParameters.Premium = (double) Premium;
            if (bEvalLastForewardVolatiltiy)
            {
                //3. Set the start diccount factor and vol.
                analyticModelParameters.PremiumPaymentDiscountFactor =
                    GetDiscountFactor(DiscountCurve, AdjustedStartDate, modelData.ValuationDate);          
                //4. Get the respective year fractions
                analyticModelParameters.YearFractions = GetYearFractions();
                //5. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateOptionAssetResults, RateOptionAssetResults>(analyticModelParameters,
                                                                                               metricsToEvaluate);
            }
            else
            {
                analyticModelParameters.IsDiscounted = false;
                analyticModelParameters.IsPut = !IsCap;
                analyticModelParameters.Notionals = Notionals;
                analyticModelParameters.ForwardRates = ResetRates;
                //2. Get the discount factors
                analyticModelParameters.PaymentDiscountFactors =
                    GetDiscountFactors(DiscountCurve, AdjustedPeriodDates.ToArray(), modelData.ValuationDate);
                analyticModelParameters.ForecastDiscountFactors =
                    GetDiscountFactors(ForecastCurve, AdjustedPeriodDates.ToArray(), modelData.ValuationDate);
                //3. Get the respective year fractions
                analyticModelParameters.YearFractions = GetYearFractions();
                analyticModelParameters.Strikes = Strikes;
                analyticModelParameters.Rate = (double)CalculateImpliedParRate(modelData.ValuationDate);
                analyticModelParameters.TimesToExpiry = TimesToExpiry;
                //5. Set the anaytic input parameters and Calculate the respective metrics            
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateOptionAssetResults, RateOptionAssetResults>(analyticModelParameters,
                                                                                               metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Calculates the premium using previously set curves</returns>
        public Decimal? CalculatePremium()
        {
            //Check the curves. If they are null use the market quote.
            if (DiscountCurve == null || ForecastCurve == null) return null;
            //1. Copy the vols.
            var volatilities = CreateList((double) Volatility, TimesToExpiry.Count);
            //Currently onlt non-discounted caps are avialable.
            var notionals = CreateList(InitialNotional, TimesToExpiry.Count);
            //2.The strikes 
            if (Strike != null)
            {
                var strikes = CreateList((decimal) Strike, TimesToExpiry.Count);
                //3.The premium of the flat volatility must be calculated.
                var analyticsModel = new RateOptionAssetAnalytic();
                var analyticModelParameters = new RateOptionAssetParameters
                {
                    PaymentDiscountFactors = GetDiscountFactors(DiscountCurve, AdjustedPeriodDates.ToArray(), BaseDate),
                    ForecastDiscountFactors =
                        GetDiscountFactors(ForecastCurve, AdjustedPeriodDates.ToArray(), BaseDate),
                    Volatilities = volatilities,
                    YearFractions = GetYearFractions(),
                    Strikes = strikes,
                    TimesToExpiry = TimesToExpiry,
                    IsDiscounted = false,
                    IsPut = !IsCap,
                    Notionals = notionals
                };
                //4. Solve for the forward volatility that has the same premium.
                var metrics = new[] {RateOptionMetrics.NPV};
                var analyticResults =
                    analyticsModel.Calculate<IRateOptionAssetResults, RateOptionAssetResults>(analyticModelParameters,
                        metrics);
                if (IncludeFirstPeriod)
                {
                    Premium = (decimal) SumDoubleList(analyticResults.NPV, 0);
                }
                else
                {
                    Premium = (decimal) SumDoubleList(analyticResults.NPV, 1);
                }
                return Premium;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The swap par rate</returns>
        public decimal CalculateImpliedParRate(DateTime baseDate)
        {
            //Check the curves. If they are null use the market quote.
            if (ForecastCurve == null) return 0.0m;
            var analyticsModel = new SimpleSwapAssetAnalytic();
            var analyticModelParameters = new IRSwapAssetParameters
            {
                NotionalAmount = InitialNotional,
                DiscountFactors = GetDiscountFactors(ForecastCurve, AdjustedPeriodDates, baseDate),
                YearFractions = Convert(GetYearFractions()),
                Weightings = CreateList(AdjustedPeriodDates.Count - 1, 1.0m)
            };
            //2. Get the discount factors
            //3. Get the respective year fractions
            //4. Get the Weightings
            //3. Solve for the forward volatility that has the same premium.
            var metrics = new[] { RateMetrics.ImpliedQuote };
            var analyticResults =
                analyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                    metrics);
            return analyticResults.ImpliedQuote;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns>The premium</returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            //Check the curves. If they are null use the market quote.
            if (DiscountCurve != null && ForecastCurve != null)
            {
                //1. Copy the vols.
                var volatilities = GetCurveVolatilties(interpolatedSpace, TimesToExpiry);
                //Currently onlt non-discounted caps are avialable.
                var notionals = CreateList(InitialNotional, TimesToExpiry.Count);
                //2.The premium of the flat volatility must be calculated.
                var analyticsModel = new RateOptionAssetAnalytic();
                if (Strike != null)
                {
                    var strikes = CreateList((decimal) Strike, TimesToExpiry.Count);
                    var analyticModelParameters = new RateOptionAssetParameters
                    {
                        PaymentDiscountFactors = GetDiscountFactors(DiscountCurve, AdjustedPeriodDates.ToArray(), BaseDate),
                        ForecastDiscountFactors = GetDiscountFactors(ForecastCurve, AdjustedPeriodDates.ToArray(), BaseDate),
                        Volatilities = volatilities,
                        YearFractions = GetYearFractions(),
                        Strikes = strikes,
                        TimesToExpiry = TimesToExpiry,
                        IsDiscounted = false,
                        IsPut = !IsCap,
                        Notionals = notionals
                    };
                    //3. Solve for the forward volatility that has the same premium.
                    var metrics = new[] { RateOptionMetrics.NPV };
                    var analyticResults =
                        analyticsModel.Calculate<IRateOptionAssetResults, RateOptionAssetResults>(analyticModelParameters,
                            metrics);
                    decimal calculatedPremium;
                    if (IncludeFirstPeriod)
                    {
                        calculatedPremium = (decimal)SumDoubleList(analyticResults.NPV, 0);
                    }
                    else
                    {
                        calculatedPremium = (decimal)SumDoubleList(analyticResults.NPV, 1);
                    }
                    return calculatedPremium;
                }
            }
            return 0.0m;
        }

        /// <summary>
        /// Gets the times to expiry.
        /// </summary>
        /// <param name="expiryDates">The expiry Dates.</param>
        /// <param name="baseDate">The base Date.</param>
        /// <returns>A list of expity times.</returns>
        public List<double> GetTimesToExpiry(List<DateTime> expiryDates,
                                            DateTime baseDate)
        {
            IDayCounter cDefaultDayCounter = Actual365.Instance;
            return expiryDates.Select(date => cDefaultDayCounter.YearFraction(baseDate, date)).ToList();
        }

        private static decimal[] Convert(List<double> doubleList)
        {
            var decimalList = new List<decimal>();
            foreach (var element in doubleList)
            {
                decimalList.Add((decimal)element);
            }
            return decimalList.ToArray();
        }

        /// <summary>
        /// Gets the vols.
        /// </summary>
        /// <param name="volCurve">The vol curve.</param>
        /// <param name="timesToExpiry">The times To Expiry.</param>
        /// <returns></returns>
        public List<double> GetCurveVolatilties(IInterpolatedSpace volCurve, List<double> timesToExpiry)
        {
            var vol = new List<double>();
            foreach (var time in timesToExpiry)
            {
                vol.Add(GetCurveVolatility(volCurve, time));
            }
            return vol;
        }

        /// <summary>
        /// Gets the vol.
        /// </summary>
        /// <param name="volCurve">The vol curve.</param>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        public double GetCurveVolatility(IInterpolatedSpace volCurve, Double expiry)
        {
            IPoint point = new Point1D(expiry);
            var vol = volCurve.Value(point);
            return vol;
        }

        /// <summary>
        /// Gets the vols.
        /// </summary>
        /// <param name="volCurve">The vol curve.</param>
        /// <param name="timesToExpiry">The times To Expiry.</param>
        /// <param name="strikes">The strikes.</param>
        /// <returns></returns>
        public List<double> GetVolatilties(IVolatilitySurface volCurve, List<double> timesToExpiry,
                                            List<double> strikes)
        {
            var vol = new List<double>();
            if (timesToExpiry.Count == strikes.Count)
            {
                var index = 0;
                foreach (var time in timesToExpiry)
                {
                    vol.Add(GetVolatility(volCurve, time, strikes[index]));
                    index++;
                }
            }
            return vol;
        }

        /// <summary>
        /// Gets the vol.
        /// </summary>
        /// <param name="volCurve">The vol curve.</param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="strike">The strike.</param>
        /// <returns></returns>
        public double GetVolatility(IVolatilitySurface volCurve, Double expiry,
                                         Double strike)
        {
            IPoint point = new Point2D(expiry, strike);
            var vol = volCurve.Value(point);
            return vol;
        }

        /// <summary>
        /// Gets the discount factors.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public Decimal[] GetDiscountFactors(IRateCurve discountFactorCurve, IList<DateTime> periodDates,
            DateTime valuationDate)
        {
            return periodDates.Select(periodDate => GetDiscountFactor(periodDate, valuationDate, discountFactorCurve)).ToArray();
        }

        /// <summary>
        /// Gets the discount factors.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public List<double> GetDiscountFactors(IRateCurve discountFactorCurve, DateTime[] periodDates,
                                            DateTime valuationDate)
        {
            return periodDates.Select(periodDate => GetDiscountFactor(discountFactorCurve, periodDate, valuationDate)).ToList();
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public Decimal GetDiscountFactor(DateTime targetDate,
            DateTime valuationDate, IRateCurve discountFactorCurve)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = discountFactorCurve.Value(point);
            return (Decimal)discountFactor;
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public double GetDiscountFactor(IRateCurve discountFactorCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = discountFactorCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Gets the year fractions for dates.
        /// </summary>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="dayCountFraction">The day count fraction.</param>
        /// <returns></returns>
        protected static List<double> GetYearFractionsForDates(IList<DateTime> periodDates, DayCountFraction dayCountFraction)
        {
            var yearFractions = new List<double>();
            var index = 0;
            var periodDatesLastIndex = periodDates.Count - 1;
            foreach (var periodDate in periodDates)
            {
                if (index == periodDatesLastIndex)
                    break;
                var yearFraction =
                    DayCounterHelper.Parse(dayCountFraction.Value).YearFraction(periodDate,
                                                                                       periodDates[index + 1]);
                yearFractions.Add(yearFraction);
                index++;
            }
            return yearFractions;
        }

        /// <summary>
        /// Gets the reset date.
        /// </summary>
        /// <param name="startDates">The reset relative to start Dates.</param>
        /// <returns></returns>
        /// <param name="fixingDateRelativeOffset"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="excludeLastDate"></param>
        protected static List<DateTime> GetResetDates(List<DateTime> startDates, IBusinessCalendar fixingCalendar,
            RelativeDateOffset fixingDateRelativeOffset, Boolean excludeLastDate)
        {
            var resetDates = new List<DateTime>();
            var tail = 0;
            if (excludeLastDate)
            {
                tail = 1;
            }
            for (var index = 0; index < startDates.Count - tail; index++)
            {
                var resetDate = GetFixingDate(startDates[index], fixingCalendar, fixingDateRelativeOffset);
                resetDates.Add(resetDate);
            }

            return resetDates;
        }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return ExpiryDates[ExpiryDates.Count - 1];
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public List<double> GetYearFractions()
        {
            return GetYearFractionsForDates(AdjustedPeriodDates, SimpleIRCap.dayCountFraction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        protected static List<Boolean> GenerateOptionTypeFlag(Boolean callFlag, int number)
        {
            var result = new List<Boolean>();
            for (int i = 0; i < number; i++)
            {
                result.Add(callFlag);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        protected static List<double> CreateList(decimal value, int number)
        {
            var result = new List<double>();
            for (int i = 0; i < number; i++)
            {
                result.Add((double)value);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        protected static Decimal[] CreateList(int number, decimal value)
        {
            var result = new List<Decimal>();
            for (int i = 0; i < number; i++)
            {
                result.Add(value);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        protected static List<double> CreateList(double value, int number)
        {
            var result = new List<double>();
            for (int i = 0; i < number; i++)
            {
                result.Add(value);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        protected static List<bool> CreateList(bool value, int number)
        {
            var result = new List<bool>();
            for (int i = 0; i < number; i++)
            {
                result.Add(value);
            }
            return result;
        }

        protected static double SumDoubleList(List<double> doubleList, int startElement)
        {
            var newList = new List<double>();
            for (var i = startElement; i < doubleList.Count; i++)
            {
                newList.Add(doubleList[i]);
            }
            return newList.Sum();
        }
    }
}