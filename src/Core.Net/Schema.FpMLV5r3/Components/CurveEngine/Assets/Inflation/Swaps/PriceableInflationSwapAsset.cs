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
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.Assets.Rates.Cash;
using Highlander.CurveEngine.V5r3.Assets.Rates.Swaps;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Assets;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Inflation.Swaps
{
    ///<summary>
    ///</summary>
    public abstract class PriceableInflationSwapAsset : PriceableInflationAssetController
    {
        private const decimal CDefaultWeightingValue = 1.0m;
        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<ISwapAssetParameters, RateMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IRateAssetResults AnalyticResults { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<DateTime> AdjustedPeriodDates { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal[] YearFractions = { 0.25m };

        /// <summary>
        /// 
        /// </summary>
        public Decimal[] Weightings = { 1.0m };

        /// <summary>
        /// 
        /// </summary>
        public Decimal EndDiscountFactor = 0.9m;

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => AnalyticResults?.DiscountFactorAtMaturity ?? EndDiscountFactor;

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public abstract decimal[] GetYearFractions();

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableDeposit"/> class.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        protected PriceableInflationSwapAsset(DateTime baseDate,
                                              BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
        {
            BaseDate = baseDate;
            BusinessDayAdjustments = businessDayAdjustments;
            SetRate(fixedRate);
            StartDiscountFactor = 1.0m;
            CurveName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableDeposit"/> class.
        /// </summary>
        /// <param name="curveName"></param>
        /// <param name="baseDate"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        protected PriceableInflationSwapAsset(string curveName, DateTime baseDate,
                                              BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
        {
            CurveName = curveName;
            BaseDate = baseDate;
            BusinessDayAdjustments = businessDayAdjustments;
            SetRate(fixedRate);
            StartDiscountFactor = 1.0m;
            CurveName = string.Empty;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new SwapAssetAnalytic();
                //DependencyCreator.Resolve<IModelAnalytic<ISwapAssetParameters, RateMetrics>>("InflationSwapAsset");
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
            //2. Set the rate
            analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            if (bEvalDiscountFactorAtMaturity)
            {
                //3. Set the start diccount factor
                analyticModelParameters.StartDiscountFactor =
                    GetDiscountFactor(curve, AdjustedStartDate, modelData.ValuationDate);
                //4. Get the respective year fractions
                analyticModelParameters.YearFractions = GetYearFractions();
                //5. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            else
            {
                //2. Get the discount factors
                analyticModelParameters.DiscountFactors =
                    GetDiscountFactors(curve, AdjustedPeriodDates.ToArray(), modelData.ValuationDate);
                //3. Get the respective year fractions
                analyticModelParameters.YearFractions = GetYearFractions();
                //4. Get the Weightings
                analyticModelParameters.Weightings =
                    CreateWeightings(CDefaultWeightingValue, analyticModelParameters.DiscountFactors.Length);
                //5. Set the anaytic input parameters and Calculate the respective metrics            
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolatedSpace.</param>
        /// <returns></returns>
        public override decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace)
        {
            AnalyticsModel = new SwapAssetAnalytic();
            //DependencyCreator.Resolve<IModelAnalytic<ISwapAssetParameters, RateMetrics>>("InflationSwapAsset");
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters();
            AnalyticResults = new RateAssetResults();
            //1. instantiate curve
            //var curve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(_discountCurveName);
            //2. Set the rate
            analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            //3. Set the start diccount factor
            analyticModelParameters.StartDiscountFactor =
                GetDiscountFactor(interpolatedSpace, AdjustedStartDate, BaseDate);
            //4. Get the respective year fractions
            analyticModelParameters.YearFractions = GetYearFractions();
            //5. Set the anaytic input parameters and Calculate the respective metrics
            AnalyticResults =
                AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                               new[] {RateMetrics.DiscountFactorAtMaturity});
            return AnalyticResults.DiscountFactorAtMaturity;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolatedSpace.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            AnalyticsModel = new SimpleSwapAssetAnalytic();
            //DependencyCreator.Resolve<IModelAnalytic<ISwapAssetParameters, RateMetrics>>("InflationSwapAsset");
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters();
            AnalyticResults = new RateAssetResults();
            //1. instantiate curve
            //var curve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(_discountCurveName);
            //2. Set the rate
            analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            //2. Get the discount factors
            analyticModelParameters.DiscountFactors =
                GetDiscountFactors(interpolatedSpace, AdjustedPeriodDates.ToArray(), BaseDate);
            //3. Get the respective year fractions
            analyticModelParameters.YearFractions = GetYearFractions();
            //4. Get the Weightings
            analyticModelParameters.Weightings =
                CreateWeightings(CDefaultWeightingValue, analyticModelParameters.DiscountFactors.Length);
            //5. Set the anaytic input parameters and Calculate the respective metrics            
            AnalyticResults =
                AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                               new[] {RateMetrics.ImpliedQuote});
            return AnalyticResults.ImpliedQuote;
        }

        /// <summary>
        /// Gets the discount factors.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal[] GetDiscountFactors(IRateCurve discountFactorCurve, DateTime[] periodDates,
                                            DateTime valuationDate)
        {
            var discountFactors = new List<decimal>();
            foreach (var periodDate in periodDates)
            {
                discountFactors.Add(GetDiscountFactor(discountFactorCurve, periodDate, valuationDate));
            }
            return discountFactors.ToArray();
        }

        /// <summary>
        /// Sets the rate.
        /// </summary>
        /// <param name="fixedRate">The fixed rate.</param>
        private void SetRate(BasicQuotation fixedRate)
        {
            if (String.Compare(fixedRate.measureType.Value, PriceableSwapRateAsset.RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                fixedRate.measureType.Value = PriceableSwapRateAsset.RateQuotationType;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", PriceableSwapRateAsset.RateQuotationType);
            }
            MarketQuote = fixedRate.measureType.Value == PriceableSwapRateAsset.RateQuotationType
                             ? fixedRate
                             : FixedRate;
        }

        /// <summary>
        /// Creates the weightings.
        /// </summary>
        /// <param name="weightingValue">The weighting value.</param>
        /// <param name="noOfInstances">The no of instances.</param>
        /// <returns></returns>
        private static Decimal[] CreateWeightings(Decimal weightingValue, int noOfInstances)
        {
            var weights = new List<decimal>();
            for (var index = 0; index < noOfInstances; index++)
            {
                weights.Add(weightingValue);
            }
            return weights.ToArray();
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDates">The target dates.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal[] GetDiscountFactors(IInterpolatedSpace discountFactorCurve, DateTime[] targetDates,
                                            DateTime valuationDate)
        {
            var discountFactors = new List<decimal>();
            foreach (var date in targetDates)
            {
                discountFactors.Add(GetDiscountFactor(discountFactorCurve, date, valuationDate));
            }
            return discountFactors.ToArray();
        }
    }
}