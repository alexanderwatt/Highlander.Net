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

using System;
using Highlander.Codes.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using DayCounterHelper=Highlander.Reporting.Analytics.V5r3.DayCounters.DayCounterHelper;

namespace Highlander.CurveEngine.V5r3.Helpers
{
    ///<summary>
    ///</summary>
    public static class CompoundingHelper
    {
        ///<summary>
        ///</summary>
        ///<param name="baseDate"></param>
        ///<param name="frequency"></param>
        ///<param name="dayCountFraction"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public static decimal PeriodFractionFromCompoundingFrequency(DateTime baseDate, CompoundingFrequency frequency, DayCountFraction dayCountFraction)
        {
            return PeriodFractionFromCompoundingFrequency(baseDate, frequency.ToEnum(), dayCountFraction);
        }

        ///<summary>
        ///</summary>
        ///<param name="baseDate"></param>
        ///<param name="frequency"></param>
        ///<param name="dayCountFraction"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public static decimal PeriodFractionFromCompoundingFrequency(DateTime baseDate, CompoundingFrequencyEnum frequency, DayCountFraction dayCountFraction)
        {
            switch (frequency)
            {
                case CompoundingFrequencyEnum.Continuous:
                    return 0.0m;

                case CompoundingFrequencyEnum.Daily:
                    IDayCounter dc = DayCounterHelper.Parse(dayCountFraction.Value);
                    return (decimal) dc.YearFraction(baseDate, baseDate.AddDays(1.0d));

                case CompoundingFrequencyEnum.Weekly:
                    return (decimal) 1/52;

                case CompoundingFrequencyEnum.Monthly:
                    return (decimal) 1/12;

                case CompoundingFrequencyEnum.Quarterly:
                    return (decimal) 1/4;

                case CompoundingFrequencyEnum.SemiAnnual:
                    return (decimal) 1/2;

                case CompoundingFrequencyEnum.Annual:
                    return 1.0m;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}