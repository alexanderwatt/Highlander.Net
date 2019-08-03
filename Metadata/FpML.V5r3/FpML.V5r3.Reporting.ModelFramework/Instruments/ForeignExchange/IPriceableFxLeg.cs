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

#region Usings

using System;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Instruments.ForeignExchange
{
    /// <summary>
    /// IPriceableFxLeg
    /// </summary>
    public interface IPriceableFxLeg<TAmp, TAmr> : IMetricsCalculation<TAmp, TAmr>
    {
        /// <summary>
        /// Gets or sets the payment in currency1.
        /// </summary>
        /// <value>The payment in currency1.</value>
        InstrumentControllerBase Currency1Payment { get; }

        /// <summary>
        /// Gets or sets the payment in currency2.
        /// </summary>
        /// <value>The payment in currency2.</value>
        InstrumentControllerBase Currency2Payment { get; }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="currency1CurveName">New name of the currency discount curve.</param>
        /// <param name="currency2CurveName">New name of the currency discount curve.</param>
        void UpdateDiscountCurveNames(string currency1CurveName, string currency2CurveName);

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketing.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval);
    }
}