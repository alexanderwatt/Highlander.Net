using System;
using Orion.ModelFramework;

namespace Orion.Models.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class InflationAssetAnalytic : ModelAnalyticBase<ISimpleDualAssetParameters, RateMetrics>, IRateAssetResults
    {
        private const Decimal COne = 1.0m;

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
        private Decimal EvaluateNPV() //TODO correct for inflation. add AnalyticParameters.PaymentDiscountFactor
        {
            return AnalyticParameters.YearFraction * (AnalyticParameters.Rate - EvaluateImpliedQuote()) *
                   AnalyticParameters.EndDiscountFactor;
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
        private Decimal EvaluateDeltaR()
        {
            return AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * AnalyticParameters.EndDiscountFactor / 10000;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateAccrualFactor()
        {
            return AnalyticParameters.YearFraction * AnalyticParameters.EndDiscountFactor;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedQuote()
        {
            //var result = Math.Pow(
            //                 (double)AnalyticParameters.StartDiscountFactor / (double)AnalyticParameters.EndDiscountFactor,
            //                 1 / (double)AnalyticParameters.YearFraction) - 1;
            var result = (AnalyticParameters.StartDiscountFactor / AnalyticParameters.EndDiscountFactor - 1) / AnalyticParameters.YearFraction;
            return result;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public virtual Decimal EvaluateDiscountFactorAtMaturity()
        {
            var result = AnalyticParameters.StartDiscountFactor / (COne + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
            return result;
        }
    }
}