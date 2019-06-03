using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class DiscountSwapAssetAnalytic : SwapAssetAnalytic
    {
        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected override Decimal EvaluateAccrualFactor()
        {
            var totalDfsCount = AnalyticParameters.DiscountFactors.Length;
            var accrualFactorTotal = 0.0m;
            for (var index = 1; index < totalDfsCount; index++)
            {
                accrualFactorTotal += AnalyticParameters.DiscountFactors[index] *
                                      AnalyticParameters.YearFractions[index - COne] *
                                      AnalyticParameters.Weightings[index - COne] *
                                      EvaluatePrice(AnalyticParameters.YearFractions[index - COne]);
            }
            return accrualFactorTotal;
        }

        /// <summary>
        /// Gets the discount price.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluatePrice(decimal yearfraction)
        {
            return 1 / (1 + yearfraction * AnalyticParameters.Rate);
        }
    }
}