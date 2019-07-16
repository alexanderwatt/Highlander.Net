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

namespace FpML.V5r3.Reporting.Helpers
{
    public static class OffsetHelper
    {
        public  static  Offset FromInterval(Period interval, DayTypeEnum dayType)
        {
            if ((dayType != DayTypeEnum.Business) & (dayType != DayTypeEnum.Calendar))
            {
                throw new ArgumentOutOfRangeException(nameof(dayType), dayType, "Only 'DayTypeEnum.Business' and 'DayTypeEnum.Calendar' day types are currently supported.");
            }
            //  We can only use Business dayType for days intervals.
            //
            if ((dayType == DayTypeEnum.Business) & (interval.period != PeriodEnum.D))
            {
                throw new NotSupportedException();
            }            
            var offset = new Offset
                {
                    period = interval.period,
                    periodMultiplier = interval.periodMultiplier,
                    dayType = dayType,
                    dayTypeSpecified = true
                };
            return offset;
        }
    }
}