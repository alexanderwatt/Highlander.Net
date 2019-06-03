#region Usings

using System;
using Orion.Models.Generic.Cashflows;

#endregion

namespace Orion.Models.ForeignExchange
{
    public interface IFxRateCashflowParameters : IFloatingCashflowParameters
    {
        /// <summary>
        /// Gets or sets the first currency.
        /// </summary>
        ///  <value>The first currency.</value>
        String Currency1 { get; set; }

        /// <summary>
        /// Gets or sets the second currency.
        /// </summary>
        ///  <value>The second currency.</value>
        String Currency2 { get; set; }

        /// <summary>
        /// Gets or sets the IsCurrency1Base flag.
        /// </summary>
        ///  <value>The IsCurrency1Base flag.</value>
        bool IsCurrency1Base { get; set; }
    }
}