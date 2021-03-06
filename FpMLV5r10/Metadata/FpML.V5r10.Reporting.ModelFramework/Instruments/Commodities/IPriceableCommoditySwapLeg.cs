﻿/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Instruments.Commodities
{
    /// <summary>
    /// IPriceableFxLeg
    /// </summary>
    public interface IPriceableCommoditySwapLeg<TAmp, TAmr> : IMetricsCalculation<TAmp, TAmr>
    {
        /// <summary>
        /// Gets or sets the payment in currency1.
        /// </summary>
        /// <value>The payment in currency1.</value>
        InstrumentControllerBase PriceablePayment { get; }

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketing.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval);
    }
}