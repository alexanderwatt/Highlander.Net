#region Using directives

using System;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace FpML.V5r10.Reporting.Models.Commodities
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class CommoditySpreadAssetAnalytic : ModelAnalyticBase<ICommodityAssetParameters, CommoditySpreadMetrics>, ICommoditySpreadAssetResults
    {
        //private const Decimal COne = 1.0m;

        #region ISpreadAssetResults Members

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Spread;

        /// <summary>
        /// Gets the Implied Quote.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal IndexAtMaturity => EvaluateIndexAtMaturity();

        #endregion

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateImpliedQuote()
        {
            return AnalyticParameters.Spread;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateIndexAtMaturity()
        {
            return AnalyticParameters.CommodityForward + AnalyticParameters.Spread;
        }
    }
}