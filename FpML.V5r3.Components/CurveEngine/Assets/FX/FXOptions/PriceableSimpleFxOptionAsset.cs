/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using FpML.V5r3.Reporting;
using Orion.Analytics.Interpolations.Points;
using Orion.CurveEngine.Helpers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.Models.Rates.Options;

#endregion

namespace Orion.CurveEngine.Assets.FX.FXOptions
{
    ///<summary>
    ///</summary>
    public abstract class PriceableSimpleFxOptionAsset : PriceableFxOptionAssetController
    {
        ///<summary>
        ///</summary>
        public const string FxQuotationType = "MarketQuote";

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
        public DateTime ExpiryDate { get; set; }

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
        public Boolean IsPut { get; set; }

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

        ///// <summary>
        ///// The strike of the expiry time.
        ///// </summary>
        //public double ExpiryTime { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal YearFraction { get; set; }

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
        public override decimal VolatilityAtRiskMaturity => AnalyticResults?.VolatilityAtExpiry ?? Volatility;

        ///// <summary>
        ///// Gets or sets the start discount factor.
        ///// </summary>
        ///// <value>The start discount factor.</value>
        //public Decimal StartDiscountFactor { get; set; }

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

        ///// <summary>
        ///// Gets or sets the end discount factor.
        ///// </summary>
        ///// <value>The end discount factor.</value>
        //public Decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public BasicQuotation Quote
        {
            get => MarketQuote;
            set
            {
                MarketQuote = value;
                if (value.quoteUnits.Value == "DecimalVolatility")
                {
                    IsVolatilityQuote = true;
                    Volatility = value.value;
                }
                if (value.quoteUnits.Value == "Volatility")
                {
                    IsVolatilityQuote = true;
                    Volatility = value.value/100.0m;
                }

                IsVolatilityQuote = false;

                if (value.quoteUnits.Value == "Premium")
                {
                    Premium = value.value;
                }
            }
        }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ISimpleRateOptionAssetResults CalculationResults => AnalyticResults;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="resetDates">The reset date information.</param>
        /// <param name="rateOption">A rateOption.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="volMarketQuote">The marketQuote, with must be a volatility.</param>
        protected PriceableSimpleFxOptionAsset(DateTime baseDate, RelativeDateOffset resetDates, SimpleFra rateOption,
                                               Decimal amount, Decimal strike, BusinessDayAdjustments businessDayAdjustments, BasicQuotation volMarketQuote)
        {
            Strike = strike;
            ResetDateOffset = resetDates;
            RateOption = rateOption;
            ModelIdentifier = "SimpleFxOptionAsset";
            BaseDate = baseDate;
            BusinessDayAdjustments = businessDayAdjustments;
            Notional = amount;
            SetQuote(volMarketQuote);
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="simpleFra">A simple fra.</param>
        /// <param name="resetDates">The reset date information.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="marketQuote">The marketQuote.</param>
        protected PriceableSimpleFxOptionAsset(DateTime baseDate, RelativeDateOffset resetDates, SimpleFra simpleFra,
                                               Decimal strike, BusinessDayAdjustments businessDayAdjustments,
                                               BasicQuotation marketQuote)
            : this(baseDate, resetDates, simpleFra, 1.0m, strike, businessDayAdjustments, marketQuote)
        {}

        /// <summary>
        /// Sets the marketQuote.
        /// </summary>
        /// <param name="marketQuote">The marketQuote.</param>
        private void SetQuote(BasicQuotation marketQuote)
        {
            if (String.Compare(marketQuote.measureType.Value, FxQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                Quote = marketQuote;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", FxQuotationType);
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

            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
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
            var curve = (IRateCurve) modelData.MarketEnvironment.GetPricingStructure(CurveName);
            analyticModelParameters.IsPut = IsPut;
            analyticModelParameters.Rate = GetRate(curve, modelData.ValuationDate, AdjustedStartDate,
                                                   MaturityDate, YearFraction);
            analyticModelParameters.Premium = Premium;

            //2. get start df = curve.getvalue(this._adjustedStartDate);
            //
            analyticModelParameters.StartDiscountFactor = GetDiscountFactor(curve, AdjustedStartDate,
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
                var volCurve = (IVolatilitySurface) modelData.MarketEnvironment.GetPricingStructure(VolCurveName);
                analyticModelParameters.Volatility =
                    Convert.ToDecimal(volCurve.GetValue((double) TimeToExpiry, (double) Strike));
            }

            //4. Set the anaytic input parameters and Calculate the respective metrics
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
        protected abstract Decimal GetYearFraction();

    }
}