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
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.Assets.Rates.Spreads;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Assets;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates
{
    ///<summary>
    ///</summary>
    public abstract class PriceableSimpleRateAsset : PriceableRateSpreadAssetController, IPriceableClearedRateAssetController
    {
        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<ISimpleRateAssetParameters, RateMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ModelIdentifier;

        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime RiskMaturityDate { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => CalculationResults?.DiscountFactorAtMaturity ?? EndDiscountFactor;

        ///<summary>
        /// The business day adjustments.
        ///</summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal YearFraction { get; set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public decimal Notional { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IRateAssetResults CalculationResults { get; protected set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleRateAsset"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        protected PriceableSimpleRateAsset(string id, DateTime baseDate, Decimal amount, BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
        {
            ModelIdentifier = "SimpleAsset";
            Id = id;
            BaseDate = baseDate;
            BusinessDayAdjustments = businessDayAdjustments;
            Notional = amount;
            SetRate(fixedRate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleRateAsset"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="riskMaturityDate">The risk maturity date.</param>
        protected PriceableSimpleRateAsset(string id, Decimal amount, DateTime effectiveDate, DateTime riskMaturityDate)
        {
            Notional = amount;
            ModelIdentifier = "SimpleAsset";
            Id = id;
            AdjustedStartDate = effectiveDate;
            RiskMaturityDate = riskMaturityDate;
            MarketQuote = BasicQuotationHelper.Create(0.0m, "Spread", "DecimalRate");
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
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new RateAssetAnalytic();
                //DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            //
            var bEvalDiscountFactorAtMaturity = false;
            if (metrics.Contains(RateMetrics.DiscountFactorAtMaturity))
            {
                bEvalDiscountFactorAtMaturity = true;
                // Remove all metrics except DFAM
                //
                metrics.RemoveAll(metricItem => metricItem != RateMetrics.DiscountFactorAtMaturity);
            }
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters { YearFraction = YearFraction };
            CalculationResults = new RateAssetResults();
            var metricsToEvaluate = metrics.ToArray();
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
            //2. get start df = curve.getvalue(this._adjustedStartDate);
            //
            analyticModelParameters.StartDiscountFactor = GetDiscountFactor(curve, AdjustedStartDate, modelData.ValuationDate);
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            if (bEvalDiscountFactorAtMaturity)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                //
                CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, metricsToEvaluate);
                EndDiscountFactor = CalculationResults.DiscountFactorAtMaturity;
            }
            else
            {
                analyticModelParameters.NotionalAmount = Notional;
                //3. Get the end discount factor
                //
                EndDiscountFactor = GetDiscountFactor(curve, GetRiskMaturityDate(), modelData.ValuationDate);
                analyticModelParameters.EndDiscountFactor = EndDiscountFactor;
                analyticModelParameters.PaymentDiscountFactor = analyticModelParameters.EndDiscountFactor;
                //4. Set the anaytic input parameters and Calculate the respective metrics
                //
                CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, metricsToEvaluate);
            }
            return GetValue(CalculationResults);
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new RateAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters
                                                                 {
                                                                     YearFraction = YearFraction,
                                                                     StartDiscountFactor =
                                                                         GetDiscountFactor(interpolatedSpace,
                                                                                           AdjustedStartDate, BaseDate)
                                                                 };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            CalculationResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.DiscountFactorAtMaturity });
            return CalculationResults.DiscountFactorAtMaturity;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new RateAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters
            {
                YearFraction = YearFraction,
                StartDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      AdjustedStartDate, BaseDate),
                EndDiscountFactor =
                    GetDiscountFactor(interpolatedSpace,
                                      GetRiskMaturityDate(), BaseDate)
            };
            analyticModelParameters.PaymentDiscountFactor = analyticModelParameters.EndDiscountFactor;
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            CalculationResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return CalculationResults.ImpliedQuote + Spread.value;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new RateAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters
                                                                 {
                                                                     YearFraction = YearFraction,
                                                                     StartDiscountFactor =
                                                                         GetDiscountFactor(interpolatedSpace,
                                                                                           AdjustedStartDate, BaseDate),
                                                                     EndDiscountFactor =
                                                                         GetDiscountFactor(interpolatedSpace,
                                                                                           GetRiskMaturityDate(), BaseDate)
                                                                 };
            analyticModelParameters.PaymentDiscountFactor = analyticModelParameters.EndDiscountFactor;
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            CalculationResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return CalculationResults.ImpliedQuote;
        }

        /// <summary>
        /// Gets the risk maturity date.
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

        #region IPriceableClearedRateAssetController

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <param name="discountedSpace">The OIS Space.</param>
        /// <returns></returns>
        public Decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace, IInterpolatedSpace discountedSpace)
        {
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new RateAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters
            {
                YearFraction = YearFraction,
                StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedStartDate, BaseDate),
                EndDiscountFactor = GetDiscountFactor(interpolatedSpace, GetRiskMaturityDate(), BaseDate),
                PaymentDiscountFactor = GetDiscountFactor(discountedSpace, GetRiskMaturityDate(), BaseDate),
            };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            CalculationResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return CalculationResults.ImpliedQuote;
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
        ///<returns>The spread calculated from the curve provided and the marketquote of the asset.</returns>
        public override decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace)
        {
            return MarketQuote.value - CalculateImpliedQuote(interpolatedSpace);
        }

        #endregion

    }
}