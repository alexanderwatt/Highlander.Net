using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class DiscountRateAssetAnalytic : RateAssetAnalytic
    {
        private const Decimal cOne = 1.0m; //TODO not done this model yet.
        
        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateNPV()
        {
            var disc1 = 1 / (1 + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
            var result = AnalyticParameters.NotionalAmount *
                         (EvaluatePrice() - disc1);
            return result * AnalyticParameters.StartDiscountFactor;
        }

        /// <summary>
        /// Gets the discount price.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluatePrice()
        {
            return AnalyticParameters.NotionalAmount / (1 + AnalyticParameters.YearFraction * EvaluateImpliedQuote());
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDeltaR()
        {
            return EvaluatePrice() * AnalyticParameters.YearFraction /
                   (1 + AnalyticParameters.YearFraction * AnalyticParameters.Rate) / 10000;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateAccrualFactor()
        {
            return AnalyticParameters.YearFraction *
                   AnalyticParameters.EndDiscountFactor;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateImpliedQuote()
        {
            return ((AnalyticParameters.StartDiscountFactor / AnalyticParameters.EndDiscountFactor) - cOne) /
                   AnalyticParameters.YearFraction;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.StartDiscountFactor /
                   (cOne + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
        }
    }
}