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

#endregion

namespace Highlander.Reporting.ModelFramework.V5r3.Business
{
    /// <summary>
    /// Conventions for act/n and 30/n types of day counters.
    /// </summary>
    public enum DayCountConvention
    {
        /// <summary>act/basis convention</summary>
        Actual,
        /// <summary>30/basis convention (US)</summary>
        US,
        /// <summary>30/basis convention (EU)</summary>
        EU,
        /// <summary>30/basis convention (Italy)</summary>
        Italian
    };

    /// <summary>
    /// This class provides methods for determining the length of a time
    /// period according to given market convention, both as a number
    /// of days and as a year fraction.
    /// </summary>
    /// <unittest>Highlander.Business.Tests.TestDayCounters.TestActualActual</unittest>
    public abstract class DayCounterBase : IDayCounter 
    {
        private readonly String _name;

        public DayCountConvention DayCountConvention { get; set; }
        public double Basis { get; }

        /// <summary>
        /// Constructor for act/n and 30/n types of day counters.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="countConvention"></param>
        /// <param name="basis"></param>
        protected DayCounterBase(String name, DayCountConvention countConvention, int basis) 
        {
            _name = name;
            DayCountConvention = countConvention;
            Basis = basis;
        }

        /// <summary>
        /// Constructor for act/act types of day counters.
        /// </summary>
        /// <param name="name"></param>
        protected DayCounterBase(String name) 
        {
            _name = name;
            DayCountConvention = DayCountConvention.Actual;
            Basis = 365.25; // just to have it initialized somehow
        }

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return _name;
        }


        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public abstract string ToFpML();


        /// <summary>
        /// The number of actual days between two dates.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        protected internal static int ActualDays(DateTime startDate, DateTime endDate) 
        {
            return (endDate.Date - startDate.Date).Days;
        }

        /// <summary>
        /// Returns the number of days between two dates.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int DayCount(DateTime startDate, DateTime endDate) 
        {
            if (DayCountConvention.Actual == DayCountConvention)
            {
                return ActualDays(startDate, endDate);
            }
            // otherwise we count 30 somethings
            int dd1 = startDate.Day;
            int dd2 = endDate.Day;
            int mm1 = startDate.Month; 
            int mm2 = endDate.Month;
            switch (DayCountConvention) 
            {
                case DayCountConvention.US:
                    if (dd2 == 31 && dd1 < 30) 
                    { 
                        dd2 = 1; 
                        ++mm2; 
                    }
                    break;
                case DayCountConvention.Italian:
                    if (mm1 == 2 && dd1 > 27) dd1 = 30;
                    if (mm2 == 2 && dd2 > 27) dd2 = 30;
                    break;
                case DayCountConvention.EU:
                    break;
                default:
                    //Trace.Fail("Unknown Option " + _countConvention, "Assuming EU convention");
                    throw new ArgumentOutOfRangeException("Unknown Option " + DayCountConvention);
                    //break;
            }
            return 360 * (endDate.Year - startDate.Year) + 30*(mm2 - mm1 -1) + Math.Max(0, 30 - dd1) + Math.Min(30, dd2);
        }

        /// <summary>
        /// Returns the period between two dates as a fraction of year. Start date after end date generates negative year fraction.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public double YearFraction(DateTime start, DateTime end) 
        {
            return YearFraction(start, end, 
                                // TODO: can we replace zero dates with start, end ??
                                // see ActualActualISMA for a test on these 0 values
                                // use CalendarBase.NullDate here???
                                new DateTime(0), new DateTime(0));
        }
		
        /// <summary>
        /// Returns the period between two dates as a fraction of year.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="refPeriodStart"></param>
        /// <param name="refPeriodEnd"></param>
        /// <returns></returns>
        public double YearFraction(
            DateTime startDate, DateTime endDate, 
            DateTime refPeriodStart, DateTime refPeriodEnd) 
        {
            // normalize to 00:00 am
            startDate = startDate.Date;
            endDate = endDate.Date;

            if (startDate > endDate) return -YearFractionImpl(endDate, startDate,
                                                              refPeriodEnd.Date, refPeriodStart.Date);
/*				throw new ArgumentException(
					// Invalid dates: startDate={0}, endDate={1}.
					Strings.Format(GetType(), "DcInvDates", startDate, endDate) 
				); */

            if (startDate == endDate) return 0.0;//should we remove it ?

            return YearFractionImpl(startDate, endDate, refPeriodStart.Date, refPeriodEnd.Date);
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
        protected virtual double YearFractionImpl(DateTime startDate, DateTime endDate, 
                                                  DateTime refPeriodStart, DateTime refPeriodEnd) 
        {
            return DayCount(startDate, endDate) / Basis;
        }
    }
}