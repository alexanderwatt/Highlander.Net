#region Using directives

using System;
using Orion.Analytics.Rates;
using Orion.ModelFramework;

#endregion

namespace Orion.Models.Assets
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