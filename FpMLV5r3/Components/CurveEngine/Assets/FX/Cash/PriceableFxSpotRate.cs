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
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.ForeignExchange;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.FX.Cash
{
    ///<summary>
    ///</summary>
    public class PriceableFxSpotRate : PriceableFxForwardRate
    {
        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public BasicQuotation FxSpotRate => FxRate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxSpotRate"/> class.
        /// </summary>
        /// <param name="notionalAmount">The notional.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fxRateAsset">The asset itself</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fxForward">The forward points.</param>
        public PriceableFxSpotRate(DateTime baseDate, decimal notionalAmount, FxSpotNodeStruct nodeStruct,
                                      FxRateAsset fxRateAsset, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fxForward)
            : base(baseDate, "2D", notionalAmount, nodeStruct, fxRateAsset, fixingCalendar, paymentCalendar, fxForward)
        {
            AdjustedStartDate = baseDate;//GetSpotDate();
            AdjustedEffectiveDate = AdjustedStartDate;
            RiskMaturityDate = GetSpotDate(AdjustedStartDate, fixingCalendar, nodeStruct.SpotDate);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new FxRateAssetAnalytic();
                //DependencyCreator.Resolve<IModelAnalytic<IFxRateAssetParameters, FxMetrics>>(_modelIdentifier);
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalForwardAtMaturity = false;
            if (metrics.Contains(FxMetrics.ForwardAtMaturity))
            {
                bEvalForwardAtMaturity = true;
                metrics.RemoveAll(
                    metricItem => metricItem != FxMetrics.ForwardAtMaturity);
            }
            IFxRateAssetParameters analyticModelParameters = new FxRateAssetParameters();
            AnalyticResults = new FxAssetResults();
            var metricsToEvaluate = metrics.ToArray();
            var marketEnvironment = modelData.MarketEnvironment;
            //1. instantiate curve
            //var fxCurve = (IFxCurve)modelData.MarketEnvironment.GetPricingStructure(FxCurveName);
            IFxCurve fxCurve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                fxCurve = (IFxCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                FxCurveName = fxCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleFxMarketEnvironment))
            {
                fxCurve = ((ISimpleFxMarketEnvironment)marketEnvironment).GetFxCurve();
                FxCurveName = fxCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                fxCurve = ((ISwapLegEnvironment)marketEnvironment).GetReportingCurrencyFxCurve();
                FxCurveName = fxCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                fxCurve = (IFxCurve)modelData.MarketEnvironment.GetPricingStructure(FxCurveName);
            } 

            //3. Get the Rate
            if (FxRate != null)
            {
                analyticModelParameters.FxRate = FxRate.value;
            }

            if (bEvalForwardAtMaturity)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IFxAssetResults, FxAssetResults>(analyticModelParameters,
                                                                               metricsToEvaluate);
            }
            else
            {
                analyticModelParameters.NotionalAmount = NotionalAmount;
                IDayCounter dc = Actual365.Instance;
                analyticModelParameters.YearFraction = (decimal)dc.YearFraction(modelData.ValuationDate, RiskMaturityDate);
                //3. Get the end discount factor - Need to fix this.
                analyticModelParameters.FxCurveSpotRate =
                    GetForwardAtMaturity(fxCurve, GetRiskMaturityDate(), modelData.ValuationDate);
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IFxAssetResults, FxAssetResults>(analyticModelParameters,
                                                                               metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="dfCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal GetDiscountFactor(IRateCurve dfCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            var point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)dfCurve.Value(point);
            return discountFactor;
        }
    }
}