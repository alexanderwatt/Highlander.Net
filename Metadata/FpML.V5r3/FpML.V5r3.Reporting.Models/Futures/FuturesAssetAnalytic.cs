using System;
using Orion.ModelFramework;

namespace Orion.Models.Futures
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class FuturesAssetAnalytic : ModelAnalyticBase<IFuturesAssetParameters, FuturesMetrics>, IFuturesAssetResults
    {
        public FuturesAssetAnalytic()
        {}

        public FuturesAssetAnalytic(decimal initialMargin, decimal variationMargin)
        {
            InitialMargin = initialMargin;
            VariationMargin = variationMargin;
        }

        #region IFuturesAssetResults Members

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public decimal NPVChange => EvaluateNPVChange();

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => AnalyticParameters.Quote;

        /// <summary>
        /// Gets the forward delta.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public Decimal PandL => EvaluatePandL();

        /// <summary>
        /// 
        /// </summary>
        public decimal InitialMargin { get; }

        /// <summary>
        /// 
        /// </summary>
        public decimal VariationMargin { get; }

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
        private Decimal EvaluateNPVChange()
        {
            var dp = EvaluateNPV();
            return AnalyticParameters.BaseNPV - dp;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            var dp = AnalyticParameters.NumberOfContracts * AnalyticParameters.AccrualPeriod * (AnalyticParameters.TradePrice - AnalyticParameters.Quote);
            return AnalyticParameters.Multiplier * dp * AnalyticParameters.SettlementDiscountFactor * AnalyticParameters.ContractNotional;
        }

        private Decimal EvaluatePandL()
        {
            //This does not discount the profit.
            var pl = AnalyticParameters.NumberOfContracts * AnalyticParameters.AccrualPeriod * (AnalyticParameters.TradePrice - AnalyticParameters.Quote);
            return AnalyticParameters.Multiplier * pl * AnalyticParameters.SettlementDiscountFactor * AnalyticParameters.ContractNotional;
        }

        #endregion      

    }
}