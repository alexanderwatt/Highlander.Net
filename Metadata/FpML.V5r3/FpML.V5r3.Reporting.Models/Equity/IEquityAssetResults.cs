using System;

namespace Orion.Models.Equity
{
    public enum EquityMetrics
    {
        NPV,  
        ImpliedQuote, 
        MarketQuote,
        IndexAtMaturity,
        PandL
    }

    public interface IEquityAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal NPV { get; }

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
    }
}