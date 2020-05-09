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
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.ModelFramework.V5r3.Instruments.Lease
{
    /// <summary>
    /// IPriceableInterestRateStream
    /// </summary>
    public interface IPriceableLeasePaymentStream<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets the Tenant.
        /// </summary>
        /// <value>The Tenant.</value>
        string Tenant { get; }

        /// <summary>
        /// The lease identifier for discounting.
        /// </summary>
        string LeaseId { get; set; }

        /// <summary>
        /// Gets the priceable coupons.
        /// </summary>
        /// <value>The priceable coupons.</value>
        List<InstrumentControllerBase> PriceablePayments { get;}

        /// <summary>
        /// Gets the stream payment dates.
        /// </summary>
        /// <value>The stream payment dates.</value>
        IList<DateTime> StreamPaymentDates { get; }

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