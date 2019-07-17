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

using System;

namespace Orion.CalendarEngine.Helpers
{
    /// <summary>
    /// A significant day in a calendar created as a result of a date rule
    /// </summary>
    public class SignificantDay : IComparable
    {
        /// <summary>
        /// Represents the actual date of a given holiday
        /// 
        /// </summary>
        public DateTime Date = DateTime.MinValue;

        /// <summary>
        /// Represents the name of a given holiday
        /// </summary>
        public string Name;


        /// <summary>
        /// Represents the associated business day holiday date
        /// </summary>
        public DateTime ObservedSignificantDayDate = DateTime.MinValue;


        /// <summary>
        /// Gets a value indicating whether this instance is weekend.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is weekend; otherwise, <c>false</c>.
        /// </value>
        public bool IsWeekend => RuleHelper.IsWeekend(Date);

        /// <summary>
        /// Gets a value indicating whether this instance is business holiday date A weekend.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is business holiday date A weekend; otherwise, <c>false</c>.
        /// </value>
        public bool IsSignificantDayDateAWeekend => RuleHelper.IsWeekend(ObservedSignificantDayDate);

        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than obj. Zero This instance is equal to obj. Greater than zero This instance is greater than obj.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">obj is not the same type as this instance. </exception>
        public int CompareTo(object obj)
        {
            if (obj is SignificantDay s)
            {
                return Date.CompareTo(s.Date);
            }
            throw new ArgumentException("Object is not a Significant Day");
        }

        #endregion
    }
}