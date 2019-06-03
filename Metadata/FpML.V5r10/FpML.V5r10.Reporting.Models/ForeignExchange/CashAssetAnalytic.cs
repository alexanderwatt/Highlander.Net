using System;
using FpML.V5r10.Reporting.ModelFramework;

namespace FpML.V5r10.Reporting.Models.ForeignExchange
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class CashAssetAnalytic : ModelAnalyticBase<ICashAssetParameters, FxMetrics>, IFxAssetResults
    {
        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal BaseCcyNPV => NPV;

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ForeignCcyNPV => 0.0m;

        /// <summary>
        /// Gets the derivative with respect to the fx spot.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal SpotDelta => 1.0m;

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal ForwardAtMaturity => EvaluateForwardAtMaturity();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal MarketQuote => 1.0m;

        /// <summary>
        /// Gets the Implied Quote.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the derivative with respect to the fx forward.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal ForwardDelta => 1.0m;

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            return AnalyticParameters.NotionalAmount;

        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedQuote()
        {
            return AnalyticParameters.NotionalAmount;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public virtual Decimal EvaluateForwardAtMaturity()
        {
            return 1.0m;
        }     
    }
}
