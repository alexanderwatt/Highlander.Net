using System;

namespace Orion.Models.Rates.Swap
{
    public enum SwapInstrumentMetrics
    {
        MarketQuote
        , BreakEvenRate
        , PCE
        , PCETerm
        , BreakEvenSpread 
        , ImpliedQuote
    }

    public interface IIRSwapInstrumentResults
    {
        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        Decimal BreakEvenSpread { get; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenRate { get; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the market rate/spread, depending on the swaptype.
        /// </summary>
        /// <value>The market rate/spread, depending on the swaptype.</value>
        Decimal MarketQuote { get; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        Decimal[] PCE { get; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        Decimal[] PCETerm { get; }
  
    }
}