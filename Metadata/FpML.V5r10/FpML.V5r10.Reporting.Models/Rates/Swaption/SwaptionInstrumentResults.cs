#region Usings

using System;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.Swaption
{
    public class SwaptionInstrumentResults : ISwaptionInstrumentResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public Decimal[] PCE { get; set; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public Decimal[] PCETerm { get; set; }

        /// <summary>
        /// Gets the break even stike.
        /// </summary>
        /// <value>The break even strike.</value>
        public decimal BreakEvenStrike { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The vlaue.</value>
        public Decimal NPV { get; set; }

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <value>The delta.</value>
        public Decimal Delta { get; set; }

        /// <summary>
        /// Gets the strike delta.
        /// </summary>
        /// <value>The strike delta.</value>
        public Decimal StrikeDelta { get; set; }

        /// <summary>
        /// Gets the fixed leg accrual factor of a fixed/float swap.
        /// </summary>
        /// <value>The fixed leg accrual factor of a fixed/float swap.</value>
        public Decimal FixedLegAccrualFactor { get; set; }
    }
}