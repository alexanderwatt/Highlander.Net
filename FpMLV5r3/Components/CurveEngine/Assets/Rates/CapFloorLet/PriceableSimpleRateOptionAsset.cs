/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using Highlander.Codes.V5r3;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Rates.Options;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.CapFloorLet
{
    ///<summary>
    ///</summary>
    public abstract class PriceableSimpleRateOptionAsset : PriceableRateOptionAssetController
    {
        ///<summary>
        ///</summary>
        public const string VolatilityQuotationType = "MarketQuote";

        ///<summary>
        ///</summary>
        public IModelAnalytic<ISimpleOptionAssetParameters, RateOptionMetrics> AnalyticsModel { get; set; }

        ///<summary>
        ///</summary>
        public ISimpleRateOptionAssetResults AnalyticResults { get; set; }

        ///<summary>
        ///</summary>
        public string ModelIdentifier { get; set; }

        ///<summary>
        ///</summary>
        public DateTime BaseDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime AdjustedStartDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime AdjustedEffectiveDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime OptionsExpiryDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime MaturityDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime PaymentDate { get; set; }

        ///<summary>
        ///</summary>
        public Boolean IsVolatilityQuote { get; set; }

        ///<summary>
        ///</summary>
        public Boolean IsDiscounted { get; set; }

        ///<summary>
        ///</summary>
        public Boolean IsCap { get; set; }

        ///<summary>
        ///</summary>
        public SimpleFra RateOption { get; set; }
        
        /// <summary>
        /// Gets and sets the volatility.
        /// </summary>
        public decimal Volatility { get; set; }

        /// <summary>
        /// Gets and sets the premium.
        /// </summary>
        public decimal Premium { get; set; }

        /// <summary>
        /// The strike of the simple options. There is only one.
        /// </summary>
        public decimal Strike { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal YearFraction { get; set; }

        /// <summary>
        /// Returns the forward rate, if supplied.
        /// </summary>
        public decimal ForwardRate { get; set; }

        /// <summary>
        /// Returns the time to expiry.
        /// </summary>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// The business day adjustments.
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; set; }

        /// <summary>
        /// The reset date offsets.
        /// </summary>
        public RelativeDateOffset ResetDateOffset { get; set; }

        /// <summary>
        /// The spot date offsets.
        /// </summary>
        public RelativeDateOffset SpotDateOffset { get; set; }

        /// <summary>
        /// Gets the volatility at maturity.
        /// </summary>
        /// <value>The volatility at maturity.</value>
        public override decimal VolatilityAtRiskMaturity => Volatility;

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets or sets the name of the vol curve.
        /// </summary>
        /// <value>The name of the vol curve.</value>
        public string VolCurveName { get; set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public decimal Notional { get; set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ISimpleRateOptionAssetResults CalculationResults => AnalyticResults;

        /// <summary>
        /// Gets the expiry date
        /// </summary>
        /// <returns></returns>
        public override DateTime GetExpiryDate() => OptionsExpiryDate;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="resetDates">The reset date information.</param>
        /// <param name="rateOption">A rateOption.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="marketQuotes">The market Quotes. This must include the volatility.</param>
        protected PriceableSimpleRateOptionAsset(DateTime baseDate, RelativeDateOffset resetDates, SimpleFra rateOption,
        Decimal amount, Decimal strike, BusinessDayAdjustments businessDayAdjustments, BasicAssetValuation marketQuotes)
        {
            Strike = strike;
            ResetDateOffset = resetDates;
            RateOption = rateOption;
            ModelIdentifier = "SimpleRateOptionAsset";
            BaseDate = baseDate;
            BusinessDayAdjustments = businessDayAdjustments;
            Notional = amount;
            var quotes = new List<BasicQuotation>(marketQuotes.quote);
            SetQuote(VolatilityQuotationType, quotes);
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
            if (normalisedQuote.measureType.Value == VolatilityQuotationType)
            {
                MarketQuote = normalisedQuote;
                Volatility = MarketQuote.value;
            }
            if (measureType == "ForwardRate")
            {
                ForwardRate = normalisedQuote.value;
            }
            if (measureType == "Strike")
            {
                Strike = normalisedQuote.value;
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
            if (AnalyticsModel==null)
            {
                //AnalyticsModel =
                //    DependencyCreator.Resolve<IModelAnalytic<ISimpleOptionAssetParameters, RateOptionMetrics>>(
                //        ModelIdentifier);
                AnalyticsModel = new SimpleRateOptionAssetAnalytic();
            }
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so that all we evaluate - every other metric is ignored
            //
            var bEvalVolatilityAtRiskMaturity = false;
            if (metrics.Contains(RateOptionMetrics.VolatilityAtExpiry))
            {
                bEvalVolatilityAtRiskMaturity = true;
                // Remove all metrics except DFAM
                //
                metrics.RemoveAll(metricItem => metricItem != RateOptionMetrics.VolatilityAtExpiry);
            }
            ISimpleOptionAssetParameters analyticModelParameters = new SimpleRateOptionAssetParameters { YearFraction = YearFraction };
            AnalyticResults = new SimpleRateOptionAssetResults();
            var metricsToEvaluate = metrics.ToArray();
            if (IsVolatilityQuote)
            {
                analyticModelParameters.IsVolatilityQuote = true;
                analyticModelParameters.Volatility = Volatility;
            }
            if (bEvalVolatilityAtRiskMaturity && IsVolatilityQuote)
            {
                analyticModelParameters.IsVolatilityQuote = true;
                AnalyticResults = AnalyticsModel.Calculate<ISimpleRateOptionAssetResults, SimpleRateOptionAssetResults>(analyticModelParameters, metricsToEvaluate);
                return GetValue(AnalyticResults);
            }
            var marketEnvironment = modelData.MarketEnvironment;
            IRateCurve curve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                curve = (IRateCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleRateMarketEnvironment))
            {
                curve = ((ISimpleRateMarketEnvironment)marketEnvironment).GetRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                curve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                curve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(CurveName);
            }  
            analyticModelParameters.IsPut = !IsCap;
            //A cap is a call option on the rate. This rate should reflect the underlying Xibor
            analyticModelParameters.Rate = GetRate(curve, modelData.ValuationDate, AdjustedEffectiveDate,
                                                   MaturityDate, YearFraction);
            analyticModelParameters.Premium = Premium;
            //2. get start df = curve.getvalue(this._adjustedStartDate);
            //
            analyticModelParameters.StartDiscountFactor = GetDiscountFactor(curve, AdjustedEffectiveDate,
                                                                            modelData.ValuationDate);
            analyticModelParameters.PremiumPaymentDiscountFactor = GetDiscountFactor(curve, PaymentDate,
                                                                                     modelData.ValuationDate);
            analyticModelParameters.NotionalAmount = Notional;
            analyticModelParameters.IsDiscounted = IsDiscounted;
            //3. Get the end discount factor
            //
            analyticModelParameters.EndDiscountFactor = GetDiscountFactor(curve, MaturityDate,
                                                                          modelData.ValuationDate);
            analyticModelParameters.Strike = Strike;
            analyticModelParameters.TimeToExpiry = TimeToExpiry;
            if (bEvalVolatilityAtRiskMaturity && !IsVolatilityQuote)
            {
                AnalyticResults =
                    AnalyticsModel.Calculate<ISimpleRateOptionAssetResults, SimpleRateOptionAssetResults>(
                        analyticModelParameters, metricsToEvaluate);
                return GetValue(AnalyticResults);
            }
            if (!IsVolatilityQuote)
            {
                IVolatilitySurface volCurve;
                if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
                {
                    var me = (ISwapLegEnvironment)marketEnvironment;
                    volCurve = me.GetVolatilitySurface();
                    VolCurveName = volCurve.GetPricingStructureId().UniqueIdentifier;
                }
                else
                {
                    volCurve = (IVolatilitySurface)modelData.MarketEnvironment.GetPricingStructure(VolCurveName);
                } 
                analyticModelParameters.Volatility =
                    Convert.ToDecimal(volCurve.GetValue((double) TimeToExpiry, (double) Strike));
            }
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            AnalyticResults =
                AnalyticsModel.Calculate<ISimpleRateOptionAssetResults, SimpleRateOptionAssetResults>(analyticModelParameters,
                                                                                          metricsToEvaluate);
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        protected static decimal GetDiscountFactor(IRateCurve discountFactorCurve, DateTime targetDate,
                                                   DateTime valuationDate)
        {
            if(targetDate==valuationDate)
            {
                return 1.0m;
            }
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal) discountFactorCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="endDate">The end date</param>
        /// <param name="yearFraction">The yearFraction</param>
        /// <returns>The rate</returns>
        protected static decimal GetRate(IRateCurve discountFactorCurve, DateTime valuationDate, DateTime startDate, 
                                         DateTime endDate, Decimal yearFraction)
        {
            var startDF = GetDiscountFactor(discountFactorCurve, startDate, valuationDate);
            var endDF = GetDiscountFactor(discountFactorCurve, endDate, valuationDate);
            var rate = (startDF/endDF - 1)/yearFraction;
            return rate;
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public decimal GetYearFraction(string dayCountFraction, DateTime adjustedStartDate, DateTime maturityDate)
        {
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountFraction);
            var yearFraction = (decimal)dayCounter.YearFraction(adjustedStartDate, maturityDate);
            if (yearFraction == 0)
            {
                throw new NotSupportedException("YearFraction cannot be zero");
            }
            return yearFraction;
        }

        /// <summary>
        /// Gets the year fraction to maturity.
        /// </summary>
        /// <returns></returns>
        protected decimal GetTimeToExpiry(DateTime baseDate, DateTime expiryDate)
        {
            return (decimal)DayCounterHelper.ToDayCounter(DayCountFractionEnum.ACT_365_FIXED).YearFraction(baseDate, expiryDate);
        }
    }
}