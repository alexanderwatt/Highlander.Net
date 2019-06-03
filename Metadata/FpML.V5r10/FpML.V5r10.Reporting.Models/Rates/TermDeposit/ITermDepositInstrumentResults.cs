using System;

namespace FpML.V5r10.Reporting.Models.Rates.TermDeposit
{
    public enum TermDepositInstrumentMetrics { MarketQuote, BreakEvenRate, BreakEvenSpread, ImpliedQuote, PCE, PCETerm }

    public interface ITermDepositInstrumentResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenRate { get; }

        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        Decimal BreakEvenSpread { get; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the market rate.
        /// </summary>
        /// <value>The market rate.</value>
        Decimal MarketQuote { get;}

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