/*
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

#region Using directives

using System;
using Orion.ModelFramework.Business;

#endregion

namespace Orion.ModelFramework
{
    /// <summary>
    /// This interface provides methods for determining the length of a time
    /// period according to given market convention, both as a number
    /// of days and as a year fraction.
    /// </summary>
    /// <remarks>
    /// This interface is mainly for COM interop.
    /// </remarks>
    public interface IDayCounter  
    {
        /// <summary>
        /// A string representation of this DayCounter for use with FpML.
        /// </summary>
        /// <remarks>
        /// This method is used for interaction with FpML.
        /// </remarks>
        /// <returns>An FpML String representing the object.</returns>
        string ToFpML();


        /// <summary>
        /// The number of days between two dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The number of days</returns>
        int DayCount(DateTime startDate, DateTime endDate);

        /// <summary>
        /// The period between two dates as a fraction of year.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The period between two dates as a fraction of year.</returns>
        double YearFraction(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns the period between two dates as a fraction of year.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="refPeriodStart">Begin of the reference period or 0.</param>
        /// <param name="refPeriodEnd">End of the reference period or 0.</param>
        /// <returns>The period between two dates as a fraction of year.</returns>
        double YearFraction(DateTime startDate, DateTime endDate, DateTime refPeriodStart, DateTime refPeriodEnd);

        /// <summary>
        /// Day count basis
        /// </summary>
        double Basis { get; }

        /// <summary>
        /// Day count convention enum
        /// </summary>
        DayCountConvention DayCountConvention { get; }
    }
}