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
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace FpML.V5r10.Reporting.Analytics.DayCounters
{
    /// <summary>
    /// Helper class which allows various day counters to be created
    /// </summary>
    public static class DayCounterHelper
    {
        /// <summary>
        /// Parses the specified day counter as string.
        /// </summary>
        /// <param name="dayCounterAsString">The day counter as string.</param>
        /// <returns></returns>
        public static IDayCounter Parse(string dayCounterAsString)
        {
            DayCountFractionEnum dayCountFractionEnum = EnumParse.ToDayCountFractionEnum(dayCounterAsString);
            return ToDayCounter(dayCountFractionEnum);
        }

        /// <summary>
        /// Converts to a day counter class.
        /// </summary>
        /// <param name="dayCountFractionAsEnum"></param>
        /// <returns></returns>
        public static IDayCounter ToDayCounter(DayCountFractionEnum dayCountFractionAsEnum)
        {
            switch (dayCountFractionAsEnum)
            {
                case DayCountFractionEnum._1_1: // "1/1"
                    return OneOne.Instance;
                case DayCountFractionEnum.ACT_ACT_ICMA: // "ACT/ACT.ICMA"
                case DayCountFractionEnum.ACT_ACT_ISDA: // "ACT/ACT.ISDA"
                    return ActualActualISDA.Instance;
                case DayCountFractionEnum.ACT_ACT_ISMA: // "ACT/ACT.ISMA"
                    return ActualActualISMA.Instance;
                case DayCountFractionEnum.ACT_ACT_AFB: // "ACT/ACT.AFB"
                    return ActualActualAFB.Instance;
                case DayCountFractionEnum.ACT_365_FIXED: // "ACT/365.FIXED"
                    return Actual365.Instance;
                case DayCountFractionEnum.ACT_360: // "ACT/360"
                    return Actual360.Instance;
                case DayCountFractionEnum._30_360: // "30/360"
                    return Thirty360US.Instance;
                case DayCountFractionEnum._30E_360: // "30E/360"
                    return Thirty360EU.Instance;
                case DayCountFractionEnum.BUS_252: // "BUS/252"
                    return Business252.Instance;
                default:
                    throw new ArgumentOutOfRangeException($"DayCountFraction {dayCountFractionAsEnum} not supported");
            }
        }
    }
}