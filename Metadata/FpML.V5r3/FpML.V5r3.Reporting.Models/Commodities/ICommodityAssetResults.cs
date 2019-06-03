using System;

namespace Orion.Models.Commodities
{
    public enum CommodityMetrics { IndexAtMaturity, ImpliedQuote, MarketQuote, NPV, ForwardDelta, SpotDelta}

    public interface ICommodityAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        Decimal NPV { get; }

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
        Decimal IndexAtMaturity { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }
    }
}