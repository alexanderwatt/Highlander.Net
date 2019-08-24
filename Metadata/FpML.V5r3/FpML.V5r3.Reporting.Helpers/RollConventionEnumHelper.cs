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
using System.Globalization;
using Orion.Util.Helpers;

namespace FpML.V5r3.Reporting.Helpers
{
    public static class RollConventionEnumHelper
    {
        public static string GetRollConventionAsString(int day)
        {
            return (day == 31) ? RollConventionEnum.EOM.ToString() : day.ToString(CultureInfo.InvariantCulture);
        }

        public static RollConventionEnum   Parse(string rollConventionAsString)
        {
            if (int.TryParse(rollConventionAsString, out var result))
            {
                if (31 == result)
                {
                    return RollConventionEnum.EOM;
                }
                return EnumHelper.Parse<RollConventionEnum>("Item" + rollConventionAsString);
            }
            return EnumHelper.Parse<RollConventionEnum>(rollConventionAsString);
        }

        public static RollConventionEnum GetRollDayConvention(int directionDateGeneration, DateTime effectiveDate, DateTime terminationDate)
        {
            switch (directionDateGeneration)
            {
                case 1: return Parse(effectiveDate.Day.ToString(CultureInfo.InvariantCulture));
                case 2: return Parse(terminationDate.Day.ToString(CultureInfo.InvariantCulture));
                default:
                    {
                        const string message = "Argument value is out of range. Only 1 and 2 are the valid values for this argument";
                        throw new ArgumentOutOfRangeException(nameof(directionDateGeneration), directionDateGeneration, message);
                    }
            }
        }

        public static bool IsAdjusted(RollConventionEnum rollConvention, DateTime inputDate)
        {
            return (AdjustDate(rollConvention, inputDate) == inputDate);
        }


        public static DateTime AdjustDate(RollConventionEnum rollConvention, DateTime inputDate)
        {
            if (rollConvention == RollConventionEnum.NONE)
                return inputDate;
            if (rollConvention == RollConventionEnum.EOM)
            {
                int currentMonth = inputDate.Month;
                while (currentMonth == inputDate.Month)
                {
                    inputDate = inputDate.AddDays(1);
                }
                return inputDate.AddDays(-1);
            }
            if (rollConvention >= RollConventionEnum.Item1 && rollConvention <= RollConventionEnum.Item28)
            {
                int day = (rollConvention - RollConventionEnum.Item1) + 1;
                return new DateTime(inputDate.Year, inputDate.Month, day);
            }
            if (rollConvention > RollConventionEnum.Item28 && rollConvention <= RollConventionEnum.Item30)
            {
                int day = (rollConvention - RollConventionEnum.Item1) + 1;

                if (inputDate.Month == 2)//This is a check to make sure that a valid February date is created.
                {
                    var startDate = new DateTime(inputDate.Year, inputDate.Month, 1).Date;
                    var endDate = new DateTime(inputDate.Year, inputDate.Month + 1, 1).Date;
                    var days = (endDate - startDate).Days;
                    return new DateTime(inputDate.Year, inputDate.Month, System.Math.Min(day, days));
                }
                return new DateTime(inputDate.Year, inputDate.Month, day);
            }
            throw new ArgumentOutOfRangeException(nameof(rollConvention), rollConvention, "supplied value is not supported.");
        }
    }
}