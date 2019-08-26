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
    /// IPriceableInterestRateStream
    /// </summary>
    public interface IPriceableBondCouponRateStream<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets the Issuer.
        /// </summary>
        /// <value>The Issuer.</value>
        string Issuer { get; }

        /// <summary>
        /// Gets the Buyer.
        /// </summary>
        /// <value>The Buyer.</value>
        string Buyer { get; }

        /// <summary>
        /// The bond identifier for discounting.
        /// </summary>
        string BondId { get; set; }

        /// <summary>
        /// Gets the coupon steam type.
        /// </summary>
        /// <value>The coupon stream type.</value>
        CouponStreamType BondCouponStreamType { get; set; }

        /// <summary>
        /// Gets the priceable coupons.
        /// </summary>
        /// <value>The priceable coupons.</value>
        List<InstrumentControllerBase> PriceableCoupons { get;}

        /// <summary>
        /// Gets the stream start dates.
        /// </summary>
        /// <value>The stream start dates.</value>
        IList<DateTime> StreamStartDates { get; }

        /// <summary>
        /// Gets the stream end dates.
        /// </summary>
        /// <value>The stream end dates.</value>
        IList<DateTime> StreamEndDates { get; }

        /// <summary>
        /// Gets the stream payment dates.
        /// </summary>
        /// <value>The stream payment dates.</value>
        IList<DateTime> StreamPaymentDates { get; }

        /// <summary>
        /// Gets the adjusted stream dates indicators.
        /// </summary>
        /// <value>The adjusted stream dates indicators.</value>
        IList<bool> AdjustedStreamDatesIndicators { get; }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="newDiscountCurveName">New name of the discount curve.</param>
        void UpdateDiscountCurveName(string newDiscountCurveName);

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketing.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval);
    }
}