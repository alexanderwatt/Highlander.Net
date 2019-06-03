using System;

namespace FpML.V5r10.Reporting.Models.ForeignExchange
{
    public enum FxMetrics { ForwardAtMaturity, NPV, BaseCcyNPV, ForeignCcyNPV, ImpliedQuote, 
        MarketQuote, ForwardDelta, SpotDelta }
    
    public interface IFxAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        Decimal NPV { get; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal BaseCcyNPV { get; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal ForeignCcyNPV { get; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the derivative with respect to the fx forward.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        Decimal ForwardDelta { get; }

        /// <summary>
        /// Gets the derivative with respect to the fx spot.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        Decimal SpotDelta { get; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal ForwardAtMaturity { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }
    }
}
