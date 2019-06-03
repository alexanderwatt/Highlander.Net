using System;
using FpML.V5r10.Reporting.ModelFramework;

namespace FpML.V5r10.Reporting.Models.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class SimpleSwapAssetAnalytic : ModelAnalyticBase<ISwapAssetParameters, RateMetrics>, IRateAssetResults
    {
        protected const Decimal CNegativeMultiplier = -1.0m;
        protected const int COne = 1;
        protected const int CBasisPoint = 10000;

        #region IRateAssetResults Members

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the npv change form a base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public Decimal NPVChange => EvaluateNPVChange();

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        public Decimal AccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the delta wrt the fixed rate R.
        /// </summary>
        public Decimal DeltaR => EvaluateDeltaR();

        /// <summary>
        /// Gets the convexity adjustment. This is zero.
        /// </summary>
        public Decimal ConvexityAdjustment => 0.0m;

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity => EvaluateDiscountFactorAtMaturity();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Rate;

        #endregion

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAccrualFactor()
        {
            var totalDfsCount = AnalyticParameters.DiscountFactors.Length;
            var accrualFactorTotal = 0.0m;
            for (var index = 1; index < totalDfsCount; index++)
            {
                accrualFactorTotal += AnalyticParameters.DiscountFactors[index] *
                                      AnalyticParameters.YearFractions[index - COne] *
                                      AnalyticParameters.Weightings[index - COne];
            }
            return accrualFactorTotal;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateFixedLegNPV()
        {
            //This is the delta per basis point
            var accrualDeltaR = EvaluateDeltaR();
            //This is the npv
            return accrualDeltaR * AnalyticParameters.Rate * CBasisPoint;
        }

        /// <summary>
        /// Evaluates the principal values. The assumption is that all outstanding principal 
        /// is repaid at the maturity date of the swap.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluatePrincipalValues()
        {
            var totalDfsCount = AnalyticParameters.DiscountFactors.Length;
            var initialPrincipal = AnalyticParameters.Weightings[0] * AnalyticParameters.DiscountFactors[0];
            var principalFactorTotal = 0.0m;
            for (var index = 1; index < totalDfsCount - 1; index++)
            {
                var repayment = AnalyticParameters.Weightings[index] - AnalyticParameters.Weightings[index-1];
                principalFactorTotal += AnalyticParameters.DiscountFactors[index] * repayment;
            }
            var finalRepayment = - AnalyticParameters.Weightings[totalDfsCount - 2] * AnalyticParameters.DiscountFactors[totalDfsCount - 1];
            var result = initialPrincipal + principalFactorTotal + finalRepayment;
            return result;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateImpliedQuote()
        {
            var numerator = EvaluatePrincipalValues();
            var denominator = EvaluateAccrualFactor();
            var impliedQuote = numerator / denominator;
            return impliedQuote;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public virtual Decimal EvaluateDiscountFactorAtMaturity()
        {
            var yearFractionTotal = 0.0m;
            var totalYFCount = AnalyticParameters.YearFractions.Length;
            for (var index = 0; index < totalYFCount; index++)
            {
                yearFractionTotal += AnalyticParameters.YearFractions[index];
            }
            return AnalyticParameters.StartDiscountFactor *
                   Convert.ToDecimal(
                       System.Math.Exp(Convert.ToDouble(CNegativeMultiplier * yearFractionTotal * AnalyticParameters.Rate)));
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateNPV()
        {
            //This is the basis point value times the number of basis points in the rate
            var npv = (AnalyticParameters.Rate - EvaluateImpliedQuote()) * EvaluateDeltaR();
            return npv * CBasisPoint;
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
            var deltaR = EvaluateAccrualFactor() * AnalyticParameters.NotionalAmount;
            return deltaR / CBasisPoint;
        }
    }
}