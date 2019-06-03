#region Usings

using System;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base interface for a priceabl rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analyic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analyic Model Results.</typeparam>
    public interface IPriceableFloatingCashflow<AMP, AMR> : IPriceableCashflow<AMP, AMR>
    {
        /// <summary>
        /// Gets the notional amount.
        /// </summary>
        /// <value>The notional amount.</value>
        Money NotionalAmount { get; }

        /// <summary>
        /// Gets the start index: if there is one, in which case the expected cash flow will be the difference.
        /// </summary>
        /// <value>The start index.</value>
        Decimal? GetStartIndex();

        /// <summary>
        /// Gets the floating index.
        /// </summary>
        /// <value>The floating index.</value>
        Decimal GetFloatingIndex();

        /// <summary>
        /// Gets the HasReset flag.
        /// </summary>
        /// <value>The HasReset flag.</value>
        bool HasReset { get; }

        /// <summary>
        /// Forecast amount at the end date.
        /// </summary>
        /// <returns></returns>
        Money GetForecastAmount();

        ///<summary>
        /// Gts the currency.
        ///</summary>
        ///<returns></returns>
        Currency GetCurrency();
    }
}