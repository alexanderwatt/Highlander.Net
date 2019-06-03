using System;

namespace Orion.Models.Rates.Stream
{
    public enum StreamInstrumentMetrics
    { 
        BreakEvenRate 
        , BreakEvenSpread
        , ImpliedQuote
    }

    public interface IStreamInstrumentResults
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

    }
}