using System;

namespace Orion.Models.Rates.Stream
{
    public class StreamInstrumentResults: IStreamInstrumentResults
    {
        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        public Decimal BreakEvenSpread { get; set; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenRate { get; set; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal ImpliedQuote { get; set; }
    }
}