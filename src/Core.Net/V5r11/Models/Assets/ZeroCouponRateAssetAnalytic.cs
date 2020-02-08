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
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.ModelFramework.V5r3;

#endregion

namespace Highlander.Reporting.Models.V5r3.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class ZeroCouponRateAssetAnalytic : ModelAnalyticBase<IZeroRateAssetParameters, RateMetrics>, IRateAssetResults
    {
        #region IRateAssetResults Members

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the npv change form a base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public Decimal NPVChange => EvaluateNPVChange();

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
        /// Gets the delta wrt the fixed rate R.
        /// </summary>
        public decimal DeltaR => EvaluateDeltaR();

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        public decimal AccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the convexity adjustment. This is zero.
        /// </summary>
        public decimal ConvexityAdjustment => 0.0m;

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity => EvaluateDiscountFactorAtMaturity();

        #endregion

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateNPV()
        {
            return AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
                   (AnalyticParameters.Rate - EvaluateImpliedQuote()) * AnalyticParameters.EndDiscountFactor;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateNPVChange()
        {
            return EvaluateNPV() - AnalyticParameters.BaseNPV;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDeltaR()
        {
            return AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
                   AnalyticParameters.EndDiscountFactor / 10000;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAccrualFactor()
        {
            return AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
                   AnalyticParameters.EndDiscountFactor;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateImpliedQuote()//TODO this won't work.
        {
            return RateAnalytics.DiscountFactorToZeroRate(AnalyticParameters.StartDiscountFactor, AnalyticParameters.EndDiscountFactor, AnalyticParameters.YearFraction, AnalyticParameters.PeriodAsTimesPerYear);
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.StartDiscountFactor * RateAnalytics.ZeroRateToDiscountFactor(AnalyticParameters.Rate, AnalyticParameters.YearFraction, AnalyticParameters.PeriodAsTimesPerYear);
        }
    }
}