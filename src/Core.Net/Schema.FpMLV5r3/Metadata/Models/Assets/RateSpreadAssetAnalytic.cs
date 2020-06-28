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
using Highlander.Reporting.ModelFramework.V5r3;

#endregion

namespace Highlander.Reporting.Models.V5r3.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class RateSpreadAssetAnalytic : ModelAnalyticBase<ISimpleRateAssetParameters, RateSpreadMetrics>, IRateSpreadAssetResults
    {
        private const Decimal COne = 1.0m;

        #region ISpreadAssetResults Members

        /// <summary>
        /// Gets the discount factor at start.
        /// </summary>
        /// <value>The discount factor at start.</value>
        public decimal DiscountFactorAtStart => EvaluateDiscountFactorAtStart();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Rate;

        /// <summary>
        /// Gets the Implied Quote.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        public decimal AccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity => EvaluateDiscountFactorAtMaturity();

        #endregion

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAccrualFactor()
        {
            return AnalyticParameters.YearFraction *
                   AnalyticParameters.EndDiscountFactor;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateImpliedQuote()
        {
            return (AnalyticParameters.StartDiscountFactor / AnalyticParameters.EndDiscountFactor - COne) /
                   AnalyticParameters.YearFraction;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.StartDiscountFactor /
                   (COne + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDiscountFactorAtStart()
        {
            return AnalyticParameters.EndDiscountFactor *
                   (COne + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
        }
    }
}