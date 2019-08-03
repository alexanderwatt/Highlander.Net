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

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base interface for a priceable rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analytic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analytic Model Results.</typeparam>
    public interface IPriceableRateCoupon<AMP , AMR> : IPriceableCashflow<AMP, AMR>
    {
        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        decimal Rate { get; set; }

        /// <summary>
        /// Gets a value indicating whether [payment day is adjusted].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [payment day is adjusted]; otherwise, <c>false</c>.
        /// </value>
        Boolean PaymentDayIsAdjusted { get; }

        /// <summary>
        /// Gets a value indicating whether [roll day is adjusted].
        /// </summary>
        /// <value><c>true</c> if [roll day is adjusted]; otherwise, <c>false</c>.</value>
        Boolean EndDayIsAdjusted { get; }

        /// <summary>
        /// Gets a value indicating whether [start day is adjusted].
        /// </summary>
        /// <value><c>true</c> if [start day is adjusted]; otherwise, <c>false</c>.</value>
        Boolean StartDayIsAdjusted { get; }

        /// <summary>
        /// Gets the unadjusted start date.
        /// </summary>
        /// <value>The unadjusted start date.</value>
        DateTime UnadjustedStartDate { get; }

        /// <summary>
        /// Gets the adjusted start date.
        /// </summary>
        /// <value>The adjusted start date.</value>
        DateTime AdjustedStartDate { get; set; }

        /// <summary>
        /// Gets the unadjusted end date.
        /// </summary>
        /// <value>The unadjusted end date.</value>
        DateTime UnadjustedEndDate { get; }

        /// <summary>
        /// Gets the adjusted end date.
        /// </summary>
        /// <value>The adjusted end date.</value>
        DateTime AdjustedEndDate { get; set; }

        /// <summary>
        /// Gets the coupon start date.
        /// </summary>
        /// <value>The coupon start date.</value>
        DateTime CouponStartDate { get; set; }

        /// <summary>
        /// Gets the coupon year fraction.
        /// </summary>
        /// <value>The coupon year fraction.</value>
        Decimal CouponYearFraction { get; }

        /// <summary>
        /// Gets the day count fraction.
        /// </summary>
        /// <value>The day count fraction.</value>
        DayCountFraction DayCountFraction { get; }

        /// <summary>
        /// Gets the day count convention.
        /// </summary>
        /// <value>The day count convention.</value>
        string DayCountConvention { get; }

        /// <summary>
        /// Gets the type of the discounting.
        /// </summary>
        /// <value>The type of the discounting.</value>
        string DiscountingType { get; }

        /// <summary>
        /// Gets the currency.
        /// </summary>
        /// <value>The currency.</value>
        string Currency { get; }

        /// <summary>
        /// Gets the business centers.
        /// </summary>
        /// <value>The business centers.</value>
        string BusinessCenters { get; }

        /// <summary>
        /// Gets the date adjustment convention.
        /// </summary>
        /// <value>The date adjustment convention.</value>
        string DateAdjustmentConvention { get; }

        /// <summary>
        /// Gets or sets the bucketed dates.
        /// </summary>
        /// <value>The bucketed dates.</value>
        IList<DateTime> BucketedDates { get; set; }

        /// <summary>
        /// Gets the bucketed dates list (comma separated)
        /// </summary>
        /// <value>The bucketed dates list.</value>
        string BucketedDatesList { get; }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="newDiscountCurveName">New name of the discount curve.</param>
        void UpdateDiscountCurveName(string newDiscountCurveName);
    }
}