#region Usings



#endregion

using System;
using Orion.ModelFramework;

namespace Orion.Models.Property
{
    public class PropertyTransactionAnalytic : ModelAnalyticBase<IPropertyAssetParameters, PropertyMetrics>, IPropertyAssetResults
    {
        #region IEquityAssetResults Members

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote
        {
            get { return AnalyticParameters.Quote; }
        }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        public Decimal NPV
        {
            get { return EvaluateNPV(); }
        }

        /// <summary>
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public Decimal PandL
        {
            get { return EvaluatePandL(); }
        }

        /// <summary>
        /// Gets the index
        /// </summary>
        public decimal IndexAtMaturity
        {
            get { return AnalyticParameters.Quote; }
        }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote
        {
            get { return AnalyticParameters.Quote; }
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            var dp = AnalyticParameters.PurchaseAmount * AnalyticParameters.Quote;
            return AnalyticParameters.Multiplier * dp;
        }

        private Decimal EvaluatePandL()
        {
            //This does not discount the profit.
            var pl = AnalyticParameters.PurchaseAmount * (AnalyticParameters.Quote - AnalyticParameters.Quote);
            return AnalyticParameters.Multiplier * pl;
        }

        #endregion    
    }
}