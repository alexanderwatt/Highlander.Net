using System;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base interface for a priceabl rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analyic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analyic Model Results.</typeparam>
    public interface IPriceableCoupon<AMP, AMR> : IPriceableCashflow<AMP, AMR>
    {
        /// <summary>
        /// Gets the notional amount.
        /// </summary>
        /// <value>The notional amount.</value>
        Money NotionalAmount { get; }

        /// <summary>
        /// Start of the accrual period.
        /// </summary>
        DateTime AccrualStartDate
        {
            get;
        }

        /// <summary>
        /// End of the accrual period.
        /// </summary>
        DateTime AccrualEndDate
        {
            get;
        }

        /// <summary>
        /// Accrued amount at the given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Money GetAccruedAmount(DateTime date);

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