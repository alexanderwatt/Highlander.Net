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

#region Using directives

using System;
using Orion.ModelFramework.Business;

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// European Actual/Actual day count convention.
    /// </summary>
    /// <seealso href="http://www.isda.org/c_and_a/pdf/mktc1198.pdf">
    /// ISDA: EMU and market conventions
    /// </seealso>
    public sealed class ActualActualAFB : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly ActualActualAFB Instance = new ActualActualAFB();

        private ActualActualAFB()
            : base("ActualActualAFB") 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "ACT/ACT.AFB";
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
            DateTime newendDate=endDate, temp=endDate;
            double sum = 0.0;
            while (temp > startDate) 
            {
                temp = newendDate.AddYears(-1);
                if (temp.Day==28 && temp.Month==2
                    && DateTime.IsLeapYear(temp.Year)) 
                {
                    temp.AddDays(1);
                }
                if (temp>=startDate) 
                {
                    sum += 1.0;
                    newendDate = temp;
                }
            }
            double den = 365.0;
            if (DateTime.IsLeapYear(newendDate.Year)) 
            {
                temp = new DateTime(newendDate.Year, 2, 29 );
                if (newendDate>temp && startDate<=temp)
                    den += 1.0;
            } 
            else if (DateTime.IsLeapYear(startDate.Year)) 
            {
                temp = new DateTime(startDate.Year, 2, 29 );
                if (newendDate>temp && startDate<=temp)
                    den += 1.0;
            }
            return sum+ActualDays(startDate, newendDate) / den;
        }
    }
}