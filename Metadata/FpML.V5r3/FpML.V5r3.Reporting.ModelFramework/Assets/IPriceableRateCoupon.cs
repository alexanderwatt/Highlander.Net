/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base interface for a priceable rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analytic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analytic Model Results.</typeparam>
    public interface IPriceableRateCoupon<AMP , AMR> : IPriceableCoupon<AMP, AMR>
    {
        ///<summary>
        /// Gets the paymentCalculationPeriod
        ///</summary>
        PaymentCalculationPeriod GetPaymentCalculationPeriod();

        ///<summary>
        /// Gets the discounting type enum.
        ///</summary>
        ///<returns></returns>
        DiscountingTypeEnum? GetDiscountingTypeEnum();

        ///<summary>
        /// Gets the discounting type enum.
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