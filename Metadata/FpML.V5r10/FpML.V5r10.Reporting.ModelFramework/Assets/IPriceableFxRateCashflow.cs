#region Usings

using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base interface for a priceabl rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analyic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analyic Model Results.</typeparam>
    public interface IPriceableFxRateCashflow<AMP, AMR> : IPriceableFloatingCashflow<AMP, AMR>
    {
        /// <summary>
        /// Gets the start fx rate.
        /// </summary>
        /// <value>The start index.</value>
        FxRate GetDealtFxRate();

        /// <summary>
        /// Gets the floating fx rate.
        /// </summary>
        /// <value>The floating fx rate.</value>
        FxRate GetFloatingFxRate();

        ///<summary>
        /// Gts the risk currencies: there may be more than one.
        ///</summary>
        ///<returns></returns>
        List<Currency> GetRiskCurrencies();
    }
}