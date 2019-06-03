#region Usings

using System;

#endregion

namespace Orion.Models.Generic.Cashflows
{
    [Serializable]
    public class FloatingCashflowResults : CashflowResults, IFloatingCashflowResults
    {
        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta0.</value>
        public Decimal Delta0 { get; set; }

        /// <summary>
        /// Gets the LocalCurrencyDelta0.
        /// </summary>
        /// <value>The LocalCurrencyDelta0.</value>
        public Decimal LocalCurrencyDelta0 { get; set; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenStrike { get; set; }

        /// <summary>
        /// Gets the index at maturity.
        /// </summary>
        /// <value>The index at maturity.</value>
        public decimal IndexAtMaturity { get; set; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public Decimal[] PCE { get; set; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public int[] PCETerm { get; set; }

        /// <summary>
        /// Gets the break even index.
        /// </summary>
        /// <value>The break even index.</value>
        public Decimal BreakEvenIndex { get; set; }
    }
}
