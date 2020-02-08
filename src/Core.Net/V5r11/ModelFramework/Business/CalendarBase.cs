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

using System.Collections.Generic;
using System;
using System.Globalization;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.Reporting.ModelFramework.V5r3.Business
{
    /// <summary>
    /// This class provides methods for determining whether a date is a business 
    /// day or a holiday for a given market, and for incrementing/decrementing a 
    /// date of a given number of business days.
    /// </summary>
    public abstract class CalendarBase : IBusinessCalendar 
    {
        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public abstract string ToFpML();

        /// <summary>
        /// Name(s) of the calendar(s)
        /// </summary>
        public string NameList { get; }

        /// <summary>
        /// Gets the deduped name list.
        /// </summary>
        /// <value>The deduped name list.</value>
        public string DedupedNameList => NameList.Trim();

        #region Constructors

        /// <summary>
        /// Construct a new BusinessCalendar based on a localized 
        /// culture and its default calendar.
        /// </summary>
        /// <param name="culture">The associated <see cref="System.Globalization.CultureInfo"/>.</param>
        /// <param name="name">The name of this business calendar.</param>
        protected CalendarBase(String name, CultureInfo culture) 
            : this(name, culture, culture.Calendar) 
        {}
			
        /// <summary>
        /// Construct a new BusinessCalendar based on a localized 
        /// culture and non-default calendar.
        /// </summary>
        /// <param name="name">The name of this business calendar.</param>
        /// <param name="culture">The associated <see cref="System.Globalization.CultureInfo"/>.</param>
        /// <param name="calendar">The underlying <see cref="System.Globalization.Calendar"/>.</param>
        protected CalendarBase( String name, CultureInfo culture, Calendar calendar )
        {
            NameList = name;
            Calendar = calendar;
            _culture = culture;
        }

        #endregion

        #region Properties

        /// <summary>
        /// A string representation of the Calendar.
        /// </summary>
        /// <returns>A String representing the object.</returns>
        public override string ToString() 
        { 
            return NameList; 
        }

        /// <summary>
        /// The underlying <see cref="System.Globalization.Calendar"/>.
        /// </summary>
        public Calendar Calendar { get; }

        /// <summary>
        /// The associated <see cref="System.Globalization.CultureInfo"/>.
        /// </summary>
        private readonly CultureInfo _culture;

        /// <summary>
        /// The associated <see cref="System.Globalization.CultureInfo"/>.
        /// </summary>
        public CultureInfo Culture => _culture;

        /// <summary>
        /// The associated <see cref="System.Globalization.RegionInfo"/>.
        /// </summary>
        public RegionInfo Region => new RegionInfo(_culture.LCID);

        #endregion

        #region Business days

        /// <summary>
        /// Returns <c>true</c> iff the date is a business day for the given market.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to check.</param>
        /// <returns><c>true</c> iff the date is a business day for the given market.</returns>
        public abstract bool IsBusinessDay(DateTime date);

        /// <summary>
        /// Returns <c>true</c> iff the date is a holiday for the given market.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to check.</param>
        /// <returns><c>true</c> iff the date is a holiday for the given market.</returns>
        public bool IsHoliday(DateTime date) 
        {
            return !IsBusinessDay(date);
        }


        /// <summary>
        /// Holidays between.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> HolidaysBetweenDates(DateTime startDate, DateTime endDate)
        {
            throw new InvalidOperationException("Not supported");
        }

        /// <summary>
        /// Businesses the days between dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> BusinessDaysBetweenDates(DateTime startDate, DateTime endDate)
        {
            throw new InvalidOperationException("Not supported");
        }


        #endregion

        #region Rolling and advancing

        /// <summary>
        /// Find the next business day with respect to the given date and
        /// rolling convention.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>Returns the resulting business day<see cref="DateTime"/>.</returns>
        public DateTime Roll(DateTime date, BusinessDayConventionEnum businessDayConvention) 
        {
            // QL_REQUIRE(d!=Date(), "Calendar::roll : null date");
            DateTime rolledDate = date;

            if (businessDayConvention == BusinessDayConventionEnum.FOLLOWING || businessDayConvention == BusinessDayConventionEnum.MODFOLLOWING) 
            {
                while (IsHoliday(rolledDate))
                {
                    rolledDate = Calendar.AddDays(rolledDate, 1);
                }

                if (businessDayConvention == BusinessDayConventionEnum.MODFOLLOWING && rolledDate.Month != date.Month)
                {
                    return Roll(date, BusinessDayConventionEnum.PRECEDING);
                }
            }
            else if (businessDayConvention == BusinessDayConventionEnum.PRECEDING || businessDayConvention == BusinessDayConventionEnum.MODPRECEDING) 
            {
                while (IsHoliday(rolledDate))
                {
                    rolledDate = Calendar.AddDays(rolledDate, -1);
                }

                if (businessDayConvention == BusinessDayConventionEnum.MODPRECEDING && rolledDate.Month != date.Month)
                {
                    return Roll(date, BusinessDayConventionEnum.FOLLOWING);
                }
            } 
            else if ( businessDayConvention != BusinessDayConventionEnum.NONE ) 
            {
                throw new ArgumentOutOfRangeException(nameof(businessDayConvention), "Unknown rolling convention.");
            }

            return rolledDate;
        }

        /// <summary>
        /// Advances the given <see cref="DateTime"/> the given <see cref="Offset"/>.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> that is to be advanced.</param>
        /// <param name="offset">The <see cref="Offset"/> that the given date is advanced.</param>
        /// <param name="businessDayConvention">The <see cref="BusinessDayConventionEnum"/> to use.</param>
        /// <returns>Returns the resulting <see cref="DateTime"/>.</returns>
        public DateTime Advance(DateTime date, Offset offset, BusinessDayConventionEnum businessDayConvention)
        {
            return Advance(date, int.Parse(offset.periodMultiplier), offset.period, offset.dayType, businessDayConvention);
        }

        /// <summary>
        /// Advances the given <see cref="DateTime"/> the given
        /// <paramref name="periodMultiplier"/> of time <paramref name="period"/>.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> that is to be advanced.</param>
        /// <param name="periodMultiplier">The number of time period to advance.</param>
        /// <param name="period">The <see cref="PeriodEnum"/> used.</param>
        /// <param name="dayType">Type of the day.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>
        /// Returns the resulting <see cref="DateTime"/>.
        /// </returns>
        /// <overloads>
        /// Advances the given <see cref="DateTime"/>.
        /// </overloads>
        private DateTime Advance(DateTime date, int periodMultiplier, PeriodEnum period, DayTypeEnum dayType, BusinessDayConventionEnum businessDayConvention)
        {
            #region Assert validity of parameters

            if ((dayType != DayTypeEnum.Business) & (dayType != DayTypeEnum.Calendar))
            {
                throw new ArgumentOutOfRangeException(nameof(dayType), dayType, "Only 'DayTypeEnum.Business' and 'DayTypeEnum.Calendar' day types are currently supported.");
            }

            //  We can only use Business dayType for days intervals.
            //
            if ((dayType == DayTypeEnum.Business) & (period != PeriodEnum.D))
            {
                throw new NotSupportedException();
            }

            #endregion

            if (periodMultiplier == 0)
            {
                return Roll(date, businessDayConvention);
            }


            if ((period == PeriodEnum.D) & DayTypeEnum.Business == dayType) //Business days
            {
                DateTime returnValue = date;

                if (periodMultiplier > 0)
                {
                    while (periodMultiplier > 0)
                    {
                        returnValue = returnValue.AddDays(1);

                        while (IsHoliday(returnValue))
                        {
                            returnValue = returnValue.AddDays(1);
                        }

                        periodMultiplier--;
                    }
                }
                else
                {
                    while (periodMultiplier < 0)
                    {
                        returnValue = returnValue.AddDays(-1);

                        while (IsHoliday(returnValue))
                        {
                            returnValue = returnValue.AddDays(-1);
                        }

                        periodMultiplier++;
                    }
                }

                return returnValue;
            }
            var interval = new Period {period = period, periodMultiplier = periodMultiplier.ToString()};

            return Roll(Add(date, interval), businessDayConvention);
        }

        /// <summary>
        /// TODO - remove this function from this class
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private static DateTime Add(DateTime dateTime, Period interval)
        {
            int periodMultiplierAsInt = int.Parse(interval.periodMultiplier);

            switch (interval.period)
            {
                case PeriodEnum.D:
                    {
                        return dateTime.AddDays(periodMultiplierAsInt);
                    }

                case PeriodEnum.W:
                    // This may not be correct for some calendars
                    // where a week has not always 7 days.
                    // Calendar.AddWeeks() is the right way to go
                    //
                    {
                        return dateTime.AddDays(7 * periodMultiplierAsInt);
                    }

                case PeriodEnum.M:
                    // ditto with months resp. Calendar.AddMonths()
                    //
                    {
                        return dateTime.AddMonths(periodMultiplierAsInt);
                    }
                case PeriodEnum.Y:
                    {
                        return dateTime.AddYears(periodMultiplierAsInt);
                    }
                default:
                    {
                        string intervalAsString = IntervalToString(interval);

                        throw new ArgumentOutOfRangeException(nameof(interval), intervalAsString, "specified period type is not supported by this function");
                    }
            }
        }

        /// <summary>
        /// Converts an Interval to a string.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        public static string IntervalToString(Period interval)
        {
            return $"{interval.periodMultiplier}{interval.period}";
        }

        #endregion

    }
}