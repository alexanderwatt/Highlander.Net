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

#region Using directives

using System;
using Orion.ModelFramework.Business;

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// Actual/Actual (ISMA/Bond) day count convention.
    /// </summary>
    /// <seealso href="http://www.isda.org/c_and_a/pdf/mktc1198.pdf">
    /// ISDA: EMU and market conventions
    /// </seealso>
    public sealed class ActualActualISMA : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly ActualActualISMA Instance = new ActualActualISMA();

        private ActualActualISMA()
            : base("ActualActualISMA") 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "ACT/ACT.ISMA";
        }

        /// <summary>
        /// Returns the period between two dates as a fraction of year.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="refPeriodStart"></param>
        /// <param name="refPeriodEnd"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is the default implementation, which implements year fractions
        /// on a fixed basis. act/act day counters need to override this method.
        /// </remarks>
        protected override double YearFractionImpl(
            DateTime startDate, DateTime endDate, 
            DateTime refPeriodStart, DateTime refPeriodEnd) 
        {
            if( refPeriodStart.Ticks == 0) refPeriodStart = startDate;
            if( refPeriodEnd.Ticks == 0) refPeriodEnd = endDate;
            // normalize reference period
            refPeriodStart = refPeriodStart.Date;
            refPeriodEnd = refPeriodEnd.Date;
            if( ! ( refPeriodEnd > refPeriodStart && refPeriodEnd > startDate ) )
                throw new ArgumentException(
                    $"Invalid reference period: startDate: {startDate}, endDate: {endDate}, Reference period Start: {refPeriodStart}, Reference period end: {refPeriodEnd}."
                );
            // Estimate roughly the length in months of a period
            //
            int months = (int)(0.5 + 12 * ActualDays(refPeriodStart, refPeriodEnd) / 365.0);
            if( months == 0 )
                throw new ArgumentException(
                    // TODO: i do not understand this error message
                    // number of months does not divide 12 exactly
                    "Number of months does not divide 12 exactly.");
            double period = months / 12.0;
            if (endDate <= refPeriodEnd) 
            {
                // here refPeriodEnd is a future (notional?) payment date
                if (startDate >= refPeriodStart)
                    // here refPeriodStart is the last (maybe notional)
                    // payment date.
                    // refPeriodStart <= startDate <= endDate <= refPeriodEnd
                    // [maybe the equality should be enforced, since
                    // refPeriodStart < startDate <= endDate < refPeriodEnd
                    // could give wrong results] ???
                    return period * ActualDays(startDate,endDate) / ActualDays(refPeriodStart,refPeriodEnd);
                // here refPeriodStart is the next (maybe notional)
                // payment date and refPeriodEnd is the second next
                // (maybe notional) payment date.
                // startDate < refPeriodStart < refPeriodEnd
                // AND endDate <= refPeriodEnd
                // this case is long first coupon
                // the last notional payment date
                DateTime previousRef = refPeriodStart.AddMonths(-months);
                if (endDate > refPeriodStart)
                    return YearFractionImpl(startDate, refPeriodStart, previousRef,
                                            refPeriodStart) +
                           YearFractionImpl(refPeriodStart, endDate, refPeriodStart,
                                            refPeriodEnd);
                return YearFractionImpl(startDate,endDate,previousRef,refPeriodStart);
            }
            // here refPeriodEnd is the last (notional?) payment date
            // startDate < refPeriodEnd < endDate AND refPeriodStart < refPeriodEnd
            if( refPeriodStart > startDate )
                throw new ArgumentException(
                    // TODO: now it is: refPeriodStart <= startDate < refPeriodEnd < endDate
                    // Invalid dates: startDate < refPeriodStart < refPeriodEnd < endDate.
                    "Invalid dates:");
            // the part from startDate to refPeriodEnd
            //
            double sum = YearFractionImpl(startDate, refPeriodEnd, refPeriodStart, refPeriodEnd);
            // the part from refPeriodEnd to endDate
            // count how many regular periods are in [refPeriodEnd, endDate],
            // then add the remaining time
            int i = 0;
            DateTime newRefStart, newRefEnd;
            do 
            {
                newRefStart = refPeriodEnd.AddMonths(months * i );
                newRefEnd   = refPeriodEnd.AddMonths(months * (i+1) );
                if (endDate < newRefEnd) 
                {
                    break;
                }
                sum += period;
                i++;
            } while (true);
            sum += YearFractionImpl(newRefStart, endDate, newRefStart, newRefEnd);
            return sum;
        }
    }
}