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
using System.Diagnostics;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Interpolations.Points
{
    /// <summary>
    /// A datetime point.
    /// </summary>
    [DebuggerDisplay("Value = {Value}, Name = {Name}")]
    public class DateTimePoint1D : Point1D
    {
        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="termDate"></param>
        public DateTimePoint1D(DateTime baseDate, DateTime termDate)
            : this(baseDate, termDate, 0.0)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        public DateTimePoint1D(DateTime dateTime, double value)
            : this(DateTime.Today, dateTime, value)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="termDate"></param>
        /// <param name="value"></param>
        public DateTimePoint1D(DateTime baseDate, DateTime termDate, double value)
            : base(Actual365.Instance.YearFraction(baseDate, termDate), value)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="tenor"></param>
        public DateTimePoint1D(DateTime baseDate, string tenor)
            : this(baseDate, tenor, 0.0)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="tenor"></param>
        /// <param name="value"></param>
        public DateTimePoint1D(DateTime baseDate, string tenor, double value)
            : this(baseDate, PeriodHelper.Parse(tenor).Add(baseDate), value)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="termDate"></param>
        /// <param name="dayCounter"></param>
        /// <param name="value"></param>
        public DateTimePoint1D(DateTime baseDate, DateTime termDate, IDayCounter dayCounter, double value)
            : base(dayCounter.YearFraction(baseDate, termDate), value)
        {}
    }

}
