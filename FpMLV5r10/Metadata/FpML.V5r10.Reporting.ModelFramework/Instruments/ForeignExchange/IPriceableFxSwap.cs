/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/


using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.ModelFramework.Instruments.ForeignExchange
{
    /// <summary>
    /// IPriceableFxLeg
    /// </summary>
    public interface IPriceableFxSwap<TAmp, TAmr> : IMetricsCalculation<TAmp, TAmr>
    {
        /// <summary>
        /// Gets the payments in currency1.
        /// </summary>
        /// <value>The payment in currency1.</value>
        IList<InstrumentControllerBase> Currency1Payments { get; }

        /// <summary>
        /// Gets the payment in currency2.
        /// </summary>
        /// <value>The payment in currency2.</value>
        IList<InstrumentControllerBase> Currency2Payments { get; }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="currency1CurveName">New name of the currecny discount curve.</param>
        /// <param name="currency2CurveName">New name of the currecny discount curve.</param>
        void UpdateDiscountCurveNames(string currency1CurveName, string currency2CurveName);

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketting.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval);
    }
}