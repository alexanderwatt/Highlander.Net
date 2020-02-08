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

using System;
using Highlander.Reporting.ModelFramework.V5r3;

namespace Highlander.Reporting.Models.V5r3.Commodities
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class CommodityAverageAssetAnalytic : ModelAnalyticBase<ICommodityAverageAssetParameters, CommodityMetrics>, ICommodityAssetResults
    {
        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal BaseCcyNPV => EvaluateCcy1NPV();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ForeignCcyNPV => EvaluateCcy2NPV();

        /// <summary>
        /// Gets the derivative with respect to the fx spot.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal SpotDelta => EvaluateSpotDelta();

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal IndexAtMaturity => EvaluateIndexAtMaturity();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal MarketQuote => EvaluateMarketQuote();

        /// <summary>
        /// Gets the Implied Quote.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the derivative with respect to the fx forward.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal ForwardDelta => EvaluateForwardDelta();

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            var df = AnalyticParameters.Ccy2SpotDiscountFactor;
            if (AnalyticParameters.Currency1PerCurrency2)
            {
                df = AnalyticParameters.Ccy1SpotDiscountFactor;
            }
            return AnalyticParameters.NotionalAmount * (EvaluateImpliedQuote() - MarketQuote) * df;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateIndexAtMaturity()
        {
            var indexAtMaturity = 2 * AnalyticParameters.AverageIndex - AnalyticParameters.StartIndex;
            return indexAtMaturity;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateCcy1NPV()
        {
            var fx = 1.0m;
            if (AnalyticParameters.Currency1PerCurrency2)
            {
                fx = 1 / EvaluateImpliedQuote();
            }
            return AnalyticParameters.NotionalAmount * fx * AnalyticParameters.Ccy1SpotDiscountFactor;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateCcy2NPV()
        {
            var fx = -EvaluateImpliedQuote();
            if (AnalyticParameters.Currency1PerCurrency2)
            {
                fx = 1.0m;
            }
            return AnalyticParameters.NotionalAmount * fx * AnalyticParameters.Ccy1SpotDiscountFactor;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedQuote()
        {
            return AnalyticParameters.CommodityCurveForward;
        }

        /// <summary>
        /// Evaluates the market quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateMarketQuote()
        {
            return AnalyticParameters.CommodityForward;
        }

        /// <summary>
        /// Evaluates the market quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateSpotDelta()
        {
            return .01m * AnalyticParameters.NotionalAmount;
        }

        /// <summary>
        /// Evaluates the market quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateForwardDelta()
        {
            return .01m * AnalyticParameters.NotionalAmount;
        }
    }
}