using System;
using Orion.ModelFramework;

namespace Orion.Models.Commodities
{
    public class CommodityFuturesAssetAnalytic : ModelAnalyticBase<ICommodityFuturesAssetParameters, CommodityMetrics>, ICommodityAssetResults
    {
        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV => AnalyticParameters.Position * AnalyticParameters.UnitAmount * (AnalyticParameters.Index - ImpliedQuote) * AnalyticParameters.PointValue;

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the derivative with respect to the fx forward.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal ForwardDelta => EvaluateForwardDelta();

        /// <summary>
        /// Gets the derivative with respect to the fx spot.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal SpotDelta => EvaluateSpotDelta();

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal IndexAtMaturity => AnalyticParameters.Index;

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Index;

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedQuote()
        {
            try
            {
                return AnalyticParameters.Index;
            }
            catch
            {
                throw new Exception("Real solution does not exist");
            }
        }

        /// <summary>
        /// Evaluates the market quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateSpotDelta()
        {
            return .01m * AnalyticParameters.Position;
        }

        /// <summary>
        /// Evaluates the market quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateForwardDelta()
        {
            return .01m * AnalyticParameters.Position;
        }

    }
}