using System;
using Orion.ModelFramework;

namespace Orion.Models.Equity
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class EquityAssetAnalytic : ModelAnalyticBase<IEquityAssetParameters, EquityMetrics>, IEquityAssetResults
    {
        #region IEquityAssetResults Members

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => AnalyticParameters.EquityPrice;

        /// <summary>
        /// Gets the npv.
        /// </summary>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public Decimal PandL => EvaluatePandL();

        /// <summary>
        /// Gets the index
        /// </summary>
        public decimal IndexAtMaturity => AnalyticParameters.Quote;

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Quote;

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            var dp = AnalyticParameters.NotionalAmount * AnalyticParameters.EquityPrice;
            return AnalyticParameters.Multiplier * dp;
        }

        private Decimal EvaluatePandL()
        {
            //This does not discount the profit.
            var pl = AnalyticParameters.NotionalAmount * (AnalyticParameters.EquityPrice - AnalyticParameters.Quote);
            return AnalyticParameters.Multiplier * pl;
        }

        #endregion      

    }
}