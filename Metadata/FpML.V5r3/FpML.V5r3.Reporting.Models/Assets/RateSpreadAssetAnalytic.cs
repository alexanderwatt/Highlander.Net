#region Using directives

using System;
using Orion.ModelFramework;

#endregion

namespace Orion.Models.Assets
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