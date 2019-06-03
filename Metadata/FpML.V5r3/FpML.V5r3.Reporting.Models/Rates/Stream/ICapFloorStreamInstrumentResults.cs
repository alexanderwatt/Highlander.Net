using System;

namespace Orion.Models.Rates.Stream
{
    public enum CapFloorStreamInstrumentMetrics
    { 
        BreakEvenStrike 
        , FlatVolatility
        , ImpliedQuote
    }

    public interface ICapFloorStreamInstrumentResults
    {
        /// <summary>
        /// Gets the break even strike.
        /// </summary>
        /// <value>The break even strike.</value>
        Decimal BreakEvenStrike { get; }

        /// <summary>
        /// Gets the flat volatility.
        /// </summary>
        /// <value>The flat volatility.</value>
        Decimal FlatVolatility { get; }

        /// <summary>
        /// Gets the premium value.
        /// </summary>
        /// <value>The premium value.</value>
        Decimal ImpliedQuote { get; }

    }
}