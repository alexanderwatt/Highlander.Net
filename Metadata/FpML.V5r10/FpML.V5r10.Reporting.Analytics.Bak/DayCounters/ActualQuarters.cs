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
using FpML.V5r10.Reporting.ModelFramework.Business;

namespace Orion.Analytics.DayCounters
{
    ///<summary>
    /// A class representing a daycount used in AUD CPI for the fixed leg
    /// 
    /// it is calculated as Year(date2) - Year(date1) + (Quarter(date2) - Quarter(date1)) / 4
    /// 
    /// Quarters are 
    /// 1-Jan to 31-March
    /// 1 April to 30 Jun
    /// 1 July to 30 September
    /// 1 October to 31 December    
    ///</summary>
    public sealed class ActualQuarters: DayCounterBase
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly ActualQuarters Instance = new ActualQuarters();

        ///<summary>
        /// Basic constructor
        ///</summary>
        private ActualQuarters() : base("ActualQuarters")
        {
        }

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the period between two dates as a fraction of year.
        /// </summary>
        /// <remarks>
        /// This is the default implementation, which implements year fractions
        /// on a fixed basis. act/act day counters need to override this method.
        /// </remarks>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="refPeriodStart"></param>
        /// <param name="refPeriodEnd"></param>
        /// <returns></returns>
        protected override double YearFractionImpl(DateTime startDate, DateTime endDate, DateTime refPeriodStart,
                                                   DateTime refPeriodEnd)
        {
            double years = endDate.Year - startDate.Year;
            double quarters = Quarter(endDate.Month) - Quarter(startDate.Month);
            return years + quarters/4;
        }

        /// <summary>
        /// Calculate which quarter the mont belongs to
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        private double Quarter(int month)
        {
            return ((month-1)/3) + 1;
        }
    }
}