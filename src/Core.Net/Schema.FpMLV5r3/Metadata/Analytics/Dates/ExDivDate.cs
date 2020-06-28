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

#region Using Directives

using System;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Dates
{
    /// <summary>
    /// Ex-dividend characteristic
    /// </summary>
    public enum ExDividendEnum
    {
        /// <summary>never goes ex-dividend</summary>
        XD_none = 1,
        /// <summary>goes XD when sett date >= coup - 4 biz days</summary>
        XD_4bd,
        /// <summary>"complicated, but normally goes XD 17 days before coupon date"</summary>
        XD_eurobond,
        /// <summary>goes XD when sett date >= coup - 14 cal days</summary>
        XD_14d,
        /// <summary>goes XD when sett date >= coup - 30 cal days</summary>
        XD_30d,
        /// <summary>goes XD when sett date >= coup - 7 cal days</summary>
        XD_7d,
        /// <summary>goes XD when sett date >= coup - 6 biz days</summary>
        XD_6bd,
        /// <summary>goes XD when sett date >= coup - 10 biz days</summary>
        XD_10bd,
        /// <summary>if coup between 10-24th incl then 1st Mon; otherwise 1st Mon after 14th</summary>
        XD_Austria,
        /// <summary>XD on Wed nearest 3 weeks before coup date</summary>
        XD_Ireland,
        /// <summary>goes XD when sett date >= coup - 1 cal month</summary>
        XD_1m,
        /// <summary>goes XD when sett date >= coup - 9 cal days</summary>
        XD_9d
    }

    /// <summary>
    /// ExDivDate 
    /// </summary>
    public class ExDivDate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExDivDate"/> class.
        /// </summary>
        /// <param name="xdt">The XDT.</param>
        /// <param name="nextCoupDate">The next coup date.</param>
        /// <param name="calendar">The calendar.</param>
        public ExDivDate(ExDividendEnum xdt, DateTime nextCoupDate, IBusinessCalendar calendar)
        {
            Date = CalcDate(xdt, nextCoupDate, calendar);
        }

        /// <summary>
        /// Calculates the date.
        /// </summary>
        /// <param name="xdt">The XDT.</param>
        /// <param name="nextCoupDate">The next coup date.</param>
        /// <param name="calendar">The calendar.</param>
        /// <returns></returns>
        public static DateTime CalcDate(ExDividendEnum xdt, DateTime nextCoupDate, IBusinessCalendar calendar)
        {
            switch (xdt)
            {
                case ExDividendEnum.XD_7d: return nextCoupDate.AddDays(-7);
                case ExDividendEnum.XD_9d: return nextCoupDate.AddDays(-9);
                case ExDividendEnum.XD_14d: return nextCoupDate.AddDays(-14);
                case ExDividendEnum.XD_30d: return nextCoupDate.AddDays(-30);
                case ExDividendEnum.XD_1m: return nextCoupDate.AddMonths(-1);
                case ExDividendEnum.XD_4bd:
                    {
                        var minusFourDayInterval = new Offset
                                                       {
                                                           dayType = DayTypeEnum.Business,
                                                           period = PeriodEnum.D,
                                                           periodMultiplier = "-4"
                                                       };

                        return calendar.Advance(nextCoupDate, minusFourDayInterval, BusinessDayConventionEnum.PRECEDING);

                    }
                case ExDividendEnum.XD_6bd:
                    {
                        var minusSixDayInterval = new Offset
                                                      {
                                                          dayType = DayTypeEnum.Business,
                                                          period = PeriodEnum.D,
                                                          periodMultiplier = "-6"
                                                      };

                        return calendar.Advance(nextCoupDate, minusSixDayInterval, BusinessDayConventionEnum.PRECEDING);
                    }
                case ExDividendEnum.XD_10bd:
                    {
                        var minusTenDayInterval = new Offset
                                                      {
                                                          dayType = DayTypeEnum.Business,
                                                          period = PeriodEnum.D,
                                                          periodMultiplier = "-10"
                                                      };

                        return calendar.Advance(nextCoupDate, minusTenDayInterval, BusinessDayConventionEnum.PRECEDING);
                    }

                case ExDividendEnum.XD_Austria:
                    throw new NotImplementedException("ExDividendEnum.XD_Austria");
                    //if (nextCoupDate.Day >= 10 && nextCoupDate.Day <= 24) return nextCoupDate.AddDays(Convert.ToDouble(9 - new DateTime(nextCoupDate.Year, nextCoupDate.Month, 1).DayOfWeek));
                    //return new DateTime(nextCoupDate.Year, nextCoupDate.Month, 22 - Convert.ToInt32(new DateTime(nextCoupDate.Year, nextCoupDate.Month - ((nextCoupDate.Day < 10)?0:1), 14).DayOfWeek));

                case ExDividendEnum.XD_eurobond:
                    int y = nextCoupDate.Year, m = nextCoupDate.Day, d = nextCoupDate.Day;
                    if (d < 3) { d = 16; m--; }
                    else if (d < 17) d = 1;
                    else d = 16;
                    return new DateTime(y, m, d);

                case ExDividendEnum.XD_Ireland:
                    throw new NotImplementedException("ExDividendEnum.XD_Ireland");
                    //return nextCoupDate.AddDays(Convert.ToDouble(-17-nextCoupDate.AddDays(1).DayOfWeek));

                default: return nextCoupDate;
            }
        }

        /// <summary>
        /// The date.
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Converts to FpML.
        /// </summary>
        public DateTime ToFpML => Date;

        /// <summary>
        /// Formats the ex div type.
        /// </summary>
        /// <param name="xdt"></param>
        /// <returns></returns>
        public static string FormatExDiv(ExDividendEnum xdt)
        {
            switch (xdt)
            {
                case ExDividendEnum.XD_none: return "None";
                case ExDividendEnum.XD_7d: return "7d";
                case ExDividendEnum.XD_9d: return "9d";
                case ExDividendEnum.XD_14d: return "14d";
                case ExDividendEnum.XD_30d: return "30d";
                case ExDividendEnum.XD_1m: return "1m";
                case ExDividendEnum.XD_4bd: return "4bd";
                case ExDividendEnum.XD_6bd: return "6bd";
                case ExDividendEnum.XD_10bd: return "10bd";
                case ExDividendEnum.XD_Austria: return "Austrian";
                case ExDividendEnum.XD_eurobond: return "Eurobond";
                case ExDividendEnum.XD_Ireland: return "Irish";
                default: return "";
            }
        }


        /// <summary>
        /// Parses the ex div type.
        /// </summary>
        /// <param name="exDivText"></param>
        /// <returns></returns>
        public static ExDividendEnum ParseXDtype(string exDivText)
        {
            switch (exDivText.ToLower())
            {
                case "none": return ExDividendEnum.XD_none;
                case "7d": return ExDividendEnum.XD_7d;
                case "9d": return ExDividendEnum.XD_9d;
                case "14d": return ExDividendEnum.XD_14d;
                case "30d": return ExDividendEnum.XD_30d;
                case "1m": return ExDividendEnum.XD_1m;
                case "4bd": return ExDividendEnum.XD_4bd;
                case "6bd": return ExDividendEnum.XD_6bd;
                case "10bd": return ExDividendEnum.XD_10bd;
                case "austrian":
                case "austria": return ExDividendEnum.XD_Austria;
                case "ebond":
                case "eurobond": return ExDividendEnum.XD_eurobond;
                case "irish":
                case "ireland": return ExDividendEnum.XD_Ireland;
                default: return ExDividendEnum.XD_eurobond;
            }
        }
    }
}