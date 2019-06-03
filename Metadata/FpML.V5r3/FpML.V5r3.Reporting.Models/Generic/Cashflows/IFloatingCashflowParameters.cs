#region Usings

using System;

#endregion

namespace Orion.Models.Generic.Cashflows
{
    public interface IFloatingCashflowParameters : ICashflowParameters
    {
        /// <summary>
        /// Gets or sets the start index.
        /// </summary>
        ///  <value>The start index.</value>
        Decimal? StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the floating index.
        /// </summary>
        ///  <value>The floating index.</value>
        Decimal? FloatingIndex { get; set; }

        /// <summary>
        /// Gets or sets the IsReset flag.
        /// </summary>
        ///  <value>The IsReset flag.</value>
        bool IsReset { get; set; }

        /// <summary>
        /// The ExpiryYearFraction.
        /// </summary>
        Decimal ExpiryYearFraction { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        ///  <value>The premium.</value>
        Decimal? Premium { get; set; }
    }
}