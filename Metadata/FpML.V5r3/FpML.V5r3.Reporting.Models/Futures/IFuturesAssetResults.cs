using System;

namespace Orion.Models.Futures
{
    public enum FuturesMetrics
    {
        NPV,
        NPVChange,
        ImpliedQuote, 
        MarketQuote,
        IndexAtMaturity,
        PandL,
        InitialMargin,
        VariationMargin
    }

    public interface IFuturesAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal NPV { get; }

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        Decimal NPVChange { get; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }

        /// <summary>
        /// Gets the Index At Maturity.
        /// </summary>
        Decimal IndexAtMaturity { get; }

        /// <summary>
        /// Gets the PandL.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal PandL { get; }

        /// <summary>
        /// Gets the intial margin.
        /// </summary>
        /// <value>The inital margin.</value>
        Decimal InitialMargin { get; }

        /// <summary>
        /// Gets the variation margin.
        /// </summary>
        /// <value>The variation margin.</value>
        Decimal VariationMargin { get; }
    }
}