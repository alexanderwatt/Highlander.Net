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

#region Using Directives

using System;

#endregion

namespace Highlander.Numerics.Dates
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
        XD_Eurobond,
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
        private readonly DateTime _exDivDate;

        /// <summary>
        /// The date.
        /// </summary>
        public DateTime Date => _exDivDate;

        /// <summary>
        /// Converts to FpML.
        /// </summary>
        public DateTime ToFpML => _exDivDate;

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
                case ExDividendEnum.XD_Eurobond: return "Eurobond";
                case ExDividendEnum.XD_Ireland: return "Irish";
                default: return "";
            }
        }


        /// <summary>
        /// Parses the ex div type.
        /// </summary>
        /// <param name="exDivText"></param>
        /// <returns></returns>
        ExDividendEnum ParseExDivDateType(string exDivText)
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
                case "eurobond": return ExDividendEnum.XD_Eurobond;
                case "irish":
                case "ireland": return ExDividendEnum.XD_Ireland;
                default: return ExDividendEnum.XD_Eurobond;
            }
        }
    }
}