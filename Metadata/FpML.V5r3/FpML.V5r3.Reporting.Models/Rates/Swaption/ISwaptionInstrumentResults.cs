#region Usings

using System;
using Orion.Models.Rates.Swap;

#endregion

namespace Orion.Models.Rates.Swaption
{
    public enum SwaptionInstrumentMetrics { MarketQuote, ImpliedQuote, BreakEvenStrike, PCE, PCETerm, NPV, Delta, StrikeDelta, FixedLegAccrualFactor }

    public interface ISwaptionInstrumentResults
    {
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

        /// <summary>
        /// Gets the break even stike.
        /// </summary>
        /// <value>The break even strike.</value>
        Decimal BreakEvenStrike { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The vlaue.</value>
        Decimal NPV { get; }

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <value>The delta.</value>
        Decimal Delta { get; }

        /// <summary>
        /// Gets the strike delta.
        /// </summary>
        /// <value>The strike delta.</value>
        Decimal StrikeDelta { get; }

        /// <summary>
        /// Gets the fixed leg accrual factor of a fixed/float swap.
        /// </summary>
        /// <value>The fixed leg accrual factor of a fixed/float swap.</value>
        Decimal FixedLegAccrualFactor { get; }
    }
}