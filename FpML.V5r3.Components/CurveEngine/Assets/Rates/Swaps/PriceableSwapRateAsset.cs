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
using System.Linq;
using Core.Common;
using Orion.CurveEngine.Markets;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.ModelFramework;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.Analytics.DayCounters;
using Orion.CurveEngine.Helpers;
using Orion.Models.Assets;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableSwapRateAsset : PriceableRateSpreadAssetController
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        protected const Decimal CDefaultWeightingValue = 1.0m;

        /// <summary>
        /// 
        /// </summary>
        protected const Decimal CDefaultNotional = 1000000.0m;

        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset FixingDateOffset { get; set; }

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
        public IModelAnalytic<ISwapAssetParameters, RateMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IRateAssetResults AnalyticResults { get; protected set; }

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
        public Calculation Calculation { get; set; }

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
        public Decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SimpleIRSwap SimpleIRSwap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DayCountFraction DayCountFraction => Calculation.dayCountFraction;

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
        /// Gets or sets the notional amount.
        /// </summary>
        /// <value>The notional.</value>
        public decimal Notional
        {
            get
            {
                var notional = CDefaultNotional;
                var amount = (Notional) Calculation.Item;
                if (System.Math.Abs(amount.notionalStepSchedule.initialValue) > 0)
                {
                    notional = System.Math.Abs(amount.notionalStepSchedule.initialValue);
                }
                return notional;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="calculation">The calculation class that determines if the swap is discounted or not.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        protected PriceableSwapRateAsset(DateTime baseDate, BusinessDayAdjustments businessDayAdjustments,
                                         Calculation calculation, BasicQuotation fixedRate)
        {
            ModelIdentifier = "SimpleSwapAsset";
            BaseDate = baseDate;
            Calculation = calculation;
            BusinessDayAdjustments = businessDayAdjustments;
            SetRate(fixedRate);
            StartDiscountFactor = 1.0m;
            CurveName = string.Empty;
        }

        #endregion

        #region Calculation

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
                case "SimpleSwapAsset":
                    AnalyticsModel = new SimpleSwapAssetAnalytic();
                    break;
                case "SimpleDiscountSwapAsset":
                    AnalyticsModel = new SimpleDiscountSwapAssetAnalytic();
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
            var analyticModelParameters =
                new IRSwapAssetParameters
                {
                    Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value
                };
            if (bEvalDiscountFactorAtMaturity)
            {
                //3. Set the start discount factor
                analyticModelParameters.StartDiscountFactor =
                    GetDiscountFactor(curve, AdjustedStartDate, modelData.ValuationDate);
                //4. Get the respective year fractions
                analyticModelParameters.YearFractions = YearFractions;
                //5. Set the analytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            else
            {
                analyticModelParameters.NotionalAmount = Notional;
                //2. Get the discount factors
                analyticModelParameters.DiscountFactors =
                    GetDiscountFactors(curve, AdjustedPeriodDates.ToArray(), modelData.ValuationDate);
                //3. Get the respective year fractions
                analyticModelParameters.YearFractions = YearFractions;
                //4. Get the Weightings
                analyticModelParameters.Weightings = Weightings;
                //5. Set the analytic input parameters and Calculate the respective metrics            
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                   metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        #endregion

        #region Interface IAssetController

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace)
        {
            
            if (AnalyticsModel == null)
            {
                switch (ModelIdentifier)
                {
                    case "SwapAsset":
                    case "SimpleSwapAsset":
                    case "ClearedSwapAsset":
                        AnalyticsModel = new SimpleSwapAssetAnalytic();
                        break;
                    case "DiscountSwapAsset":
                    case "SimpleDiscountSwapAsset":
                    case "DiscountClearedSwapAsset":
                        AnalyticsModel = new SimpleDiscountSwapAssetAnalytic();
                        break;
                }
            }
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters()
            {
                YearFractions = YearFractions,
                StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedStartDate, BaseDate)
            };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            AnalyticResults = new RateAssetResults();
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            if (AnalyticsModel == null) return 1.0m;
            AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                new[] {RateMetrics.DiscountFactorAtMaturity});
            return AnalyticResults.DiscountFactorAtMaturity;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="ratecurve"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public virtual decimal[] CalculatePDH(ILogger logger, ICoreCache cache, CurveBase ratecurve, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            if (AnalyticsModel == null)
            {
                switch (ModelIdentifier)
                {
                    case "SimpleSwapAsset":
                    case "ClearedSwapAsset":
                        AnalyticsModel = new SimpleSwapAssetAnalytic();
                        break;
                    case "SimpleDiscountSwapAsset":
                    case "DiscountClearedSwapAsset":
                        AnalyticsModel = new SimpleDiscountSwapAssetAnalytic();
                        break;
                }
            }
            var result = new List<decimal>();
            if (ratecurve != null)
            {
                var riskCurves = ratecurve.CreateCurveRiskSet(1);
                ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters
                                                                   {
                                                                       NotionalAmount = Notional,
                                                                       //2. Get the discount factors
                                                                       DiscountFactors =
                                                                           GetDiscountFactors(ratecurve,
                                                                                              AdjustedPeriodDates.ToArray(),
                                                                                              BaseDate),
                                                                       //3. Get the respective year fractions
                                                                       YearFractions = YearFractions,
                                                                       Rate =
                                                                           MarketQuoteHelper.NormalisePriceUnits(
                                                                           FixedRate, "DecimalRate").value
                                                                   };
                //4. Get the Weightings
                analyticModelParameters.Weightings =
                    CreateWeightings(CDefaultWeightingValue, analyticModelParameters.DiscountFactors.Length - 1);
                //4. Set the analytic input parameters and Calculate the respective metrics
                //
                if (AnalyticsModel != null)
                {
                    var analyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.NPV });
                    var baseNPV = analyticResults.NPV;
                    //Now loop through the risk curves.
                    foreach (var curve in riskCurves)
                    {
                        analyticResults = RiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                        result.Add(analyticResults.NPV - baseNPV);
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ratecurve"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual IRateAssetResults RiskCalculationHelper(IRateCurve ratecurve, ISwapAssetParameters parameters)
        {
            parameters.DiscountFactors =
                GetDiscountFactors(ratecurve,
                                   AdjustedPeriodDates.ToArray(),
                                   BaseDate);
 
                //Set the analytic input parameters and Calculate the respective metrics
                //
            return AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(parameters, new[] { RateMetrics.NPV });
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                switch (ModelIdentifier)
                {
                    case "SwapAsset":
                    case "SimpleSwapAsset":
                    case "ClearedSwapAsset":
                        AnalyticsModel = new SimpleSwapAssetAnalytic();
                        break;
                    case "DiscountSwapAsset":
                    case "SimpleDiscountSwapAsset":
                    case "DiscountClearedSwapAsset":
                        AnalyticsModel = new SimpleDiscountSwapAssetAnalytic();
                        break;
                }
            }
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters
            {
                NotionalAmount = Notional,
                //2. Get the discount factors
                DiscountFactors =
                    GetDiscountFactors(interpolatedSpace,
                                       AdjustedPeriodDates.ToArray(),
                                       BaseDate),
                //3. Get the respective year fractions
                YearFractions = YearFractions,
                Rate =
                    MarketQuoteHelper.NormalisePriceUnits(
                    FixedRate, "DecimalRate").value
            };
            //4. Get the Weightings
            analyticModelParameters.Weightings =
                CreateWeightings(CDefaultWeightingValue, analyticModelParameters.DiscountFactors.Length - 1);
            AnalyticResults = new RateAssetResults();
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            if (AnalyticsModel != null)
                AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote +Spread.value;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The interpolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                switch (ModelIdentifier)
                {
                    case "SwapAsset":
                    case "SimpleSwapAsset":
                    case "ClearedSwapAsset":
                        AnalyticsModel = new SimpleSwapAssetAnalytic();
                        break;
                    case "DiscountSwapAsset":
                    case "SimpleDiscountSwapAsset":
                    case "DiscountClearedSwapAsset":
                        AnalyticsModel = new SimpleDiscountSwapAssetAnalytic();
                        break;
                }
            }
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters
                                                               {
                                                                   NotionalAmount = Notional,
                                                                   //2. Get the discount factors
                                                                   DiscountFactors =
                                                                       GetDiscountFactors(interpolatedSpace,
                                                                                          AdjustedPeriodDates.ToArray(),
                                                                                          BaseDate),
                                                                   //3. Get the respective year fractions
                                                                   YearFractions = YearFractions,
                                                                   Rate =
                                                                       MarketQuoteHelper.NormalisePriceUnits(
                                                                       FixedRate, "DecimalRate").value
                                                               };
            //4. Get the Weightings
            analyticModelParameters.Weightings =
                CreateWeightings(CDefaultWeightingValue, analyticModelParameters.DiscountFactors.Length - 1);
            AnalyticResults = new RateAssetResults();
            //4. Set the analytic input parameters and Calculate the respective metrics
            //
            if (AnalyticsModel != null)
                AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        #endregion

        #region Helper Functions

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
            return periodDates.Select(periodDate => GetDiscountFactor(discountFactorCurve, periodDate, valuationDate)).ToArray();
        }

        /// <summary>
        /// Sets the rate.
        /// </summary>
        /// <param name="fixedRate">The fixed rate.</param>
        private void SetRate(BasicQuotation fixedRate)
        {
            if (String.Compare(fixedRate.measureType.Value, RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                fixedRate.measureType.Value = RateQuotationType;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }
            MarketQuote = fixedRate.measureType.Value == RateQuotationType ? fixedRate : null;
        }

        /// <summary>
        /// Creates the weightings.
        /// </summary>
        /// <param name="weightingValue">The weighting value.</param>
        /// <param name="noOfInstances">The no of instances.</param>
        /// <returns></returns>
        protected static Decimal[] CreateWeightings(Decimal weightingValue, int noOfInstances)
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
            return targetDates.Select(date => GetDiscountFactor(discountFactorCurve, date, valuationDate)).ToArray();
        }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return RiskMaturityDate;
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public decimal[] GetYearFractions()
        {
            return GetYearFractionsForDates(AdjustedPeriodDates, SimpleIRSwap.dayCountFraction);
        }

        /// <summary>
        /// Gets the year fractions for dates.
        /// </summary>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="dayCountFraction">The day count fraction.</param>
        /// <returns></returns>
        protected static decimal[] GetYearFractionsForDates(IList<DateTime> periodDates, DayCountFraction dayCountFraction)
        {
            var yearFractions = new List<decimal>();
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountFraction.Value);
            for (int i = 0; i < periodDates.Count - 1; i++)
            {
                double yearFraction = dayCounter.YearFraction(periodDates[i], periodDates[i + 1]);
                yearFractions.Add((decimal)yearFraction);
            }
            return yearFractions.ToArray();
        }

        #endregion

        #region Implementation of IPriceableSpreadAssetController

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public override IList<decimal> Values => new[] { StartDiscountFactor, DiscountFactorAtMaturity };

        /// <summary>
        /// Gets the value at maturity.
        /// </summary>
        /// <value>The values.</value>
        public override decimal ValueAtMaturity => DiscountFactorAtMaturity;

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public override IList<DateTime> GetRiskDates()
        {
            return new[] { AdjustedStartDate, RiskMaturityDate };
        }

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the market quote of the asset.</returns>
        public override decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace)
        {
            return MarketQuote.value - CalculateImpliedQuote(interpolatedSpace);
        }

        #endregion

    }
}