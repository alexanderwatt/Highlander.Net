using System;

namespace Orion.Models.Rates.Stream
{
    public class CapFloorStreamInstrumentResults : ICapFloorStreamInstrumentResults
    {
        /// <summary>
        /// Gets the break even strike.
        /// </summary>
        /// <value>The break even strike.</value>
        public Decimal BreakEvenStrike { get; set; }

        /// <summary>
        /// Gets the flat volatility.
        /// </summary>
        /// <value>The flat volatility.</value>
        public Decimal FlatVolatility { get; set; }

        /// <summary>
        /// Gets the premium value.
        /// </summary>
        /// <value>The premium value.</value>
        public Decimal ImpliedQuote { get; set; }
    }
}