#region Usings

using System;

#endregion

namespace Orion.Models.Generic.Cashflows
{
    public class FloatingCashflowParameters : CashflowParameters, IFloatingCashflowParameters
    {
        /// <summary>
        /// Gets the start index.
        /// </summary>
        /// <value>The start index.</value>
        public Decimal? StartIndex { get; set;  }

        /// <summary>
        /// Gets or sets the floating index.
        /// </summary>
        ///  <value>The floating index.</value>
        public Decimal? FloatingIndex { get; set; }

        /// <summary>
        /// Gets or sets the IsReset flag.
        /// </summary>
        ///  <value>The IsReset flag.</value>
        public bool IsReset { get; set; }
        
        /// <summary>
        /// The ExpiryYearFraction.
        /// </summary>
        public Decimal ExpiryYearFraction { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        ///  <value>The premium.</value>
        public decimal? Premium { get; set; }
    }
}