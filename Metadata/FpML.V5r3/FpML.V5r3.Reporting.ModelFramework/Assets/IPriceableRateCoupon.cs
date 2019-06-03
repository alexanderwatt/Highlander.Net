using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base interface for a priceabl rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analyic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analyic Model Results.</typeparam>
    public interface IPriceableRateCoupon<AMP , AMR> : IPriceableCoupon<AMP, AMR>
    {
        ///<summary>
        /// Gets the paymentCalculationPeriod
        ///</summary>
        PaymentCalculationPeriod GetPaymentCalculationPeriod();

        ///<summary>
        /// Gets the discountingtypeenum.
        ///</summary>
        ///<returns></returns>
        DiscountingTypeEnum? GetDiscountingTypeEnum();

        ///<summary>
        /// Gets the discountingtypeenum.
        ///</summary>
        ///<returns></returns>
        FraDiscountingEnum? GetFraDiscountingType();

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        decimal GetRate();

        /// <summary>
        /// Accrual period as fraction of year.
        /// </summary>
        decimal GetAccrualYearFraction();
    }
}