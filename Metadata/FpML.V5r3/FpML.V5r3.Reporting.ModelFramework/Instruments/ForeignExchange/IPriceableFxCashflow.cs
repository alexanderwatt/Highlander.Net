#region Usings

using System;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;

#endregion

namespace Orion.ModelFramework.Instruments.ForeignExchange
{
    /// <summary>
    /// Base interface for a priceable fx rate
    /// </summary>
    /// <typeparam name="AMP">The type of the Analyic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analyic Model Results.</typeparam>
    public interface IPriceableFxCashflow<AMP, AMR> : IPriceableCashflow<AMP, AMR>
    {
        // Requirements for pricing
        /// <summary>
        /// The base currency amount.
        /// </summary>
        Money BaseCurrencyAmount { get; }

        // Requirements for pricing
        /// <summary>
        /// The reference currency amount.
        /// </summary>
        Money ReferenceCurrencyAmount { get; }

        ///<summary>
        /// Gets the base currency.
        ///</summary>
        ///<returns></returns>
        Currency GetBaseCurrency();

        ///<summary>
        /// Gets the base currency.
        ///</summary>
        ///<returns></returns>
        Currency GetReferenceCurrency();
    }
}