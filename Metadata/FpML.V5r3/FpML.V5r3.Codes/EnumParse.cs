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

#region Usings

using System;

#endregion

namespace FpML.V5r3.Codes
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumParse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dayCountFractionAsString"></param>
        /// <returns></returns>
        public static DayCountFractionEnum ToDayCountFractionEnum(string dayCountFractionAsString)
        {
            dayCountFractionAsString = dayCountFractionAsString.Replace("ActualActual", "ACT/ACT.");
            if (!DayCountFractionScheme.TryParseEnumString(dayCountFractionAsString, out var dayCountFractionEnum))
            {
                // Try custom values if standard enum names fail
                switch (dayCountFractionAsString)
                {
                    case "OneOne":
                        dayCountFractionEnum = DayCountFractionEnum._1_1;
                        break;
                    case "Actual365":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_365_FIXED;
                        break;
                    case "Actual360":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_360;
                        break;
                    case "Thirty360EU":
                        dayCountFractionEnum = DayCountFractionEnum._30E_360;
                        break;
                    case "Thirty360US":
                        dayCountFractionEnum = DayCountFractionEnum._30_360;
                        break;
                    case "Business252":
                    case "ACT/252.FIXED":
                        dayCountFractionEnum = DayCountFractionEnum.BUS_252;
                        break;
                    case "ACT/ACT.AFB":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_ACT_AFB;
                        break;
                    case "ACT/ACT.ICMA":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_ACT_ICMA;
                        break;
                    case "ACT/ACT.ISDA":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_ACT_ISDA;
                        break;
                    case "ACT/ACT.ISMA":
                        dayCountFractionEnum = DayCountFractionEnum.ACT_ACT_ISMA;
                        break;
                    default:
                        string message = $"DayCountFraction '{dayCountFractionAsString}' is not valid.";
                        throw new ArgumentOutOfRangeException(nameof(dayCountFractionAsString), message);
                }
            }
            return dayCountFractionEnum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CompoundingFrequencyEnum ToCompoundingFrequencyEnum(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(string.Format("CompoundingFrequencyEnum parsing failed for ''"));
            }
            if (value.Equals("Semi-Annual", StringComparison.InvariantCultureIgnoreCase)
                || value.Equals("Semi", StringComparison.InvariantCultureIgnoreCase))
            {
                value = "SemiAnnual";
            }
            CompoundingFrequencyEnum result;
            try
            {
                result = (CompoundingFrequencyEnum)Enum.Parse(typeof(CompoundingFrequencyEnum), value, true);
            }
            catch (Exception)
            {
                throw new ArgumentException($"CompoundingFrequencyEnum parsing failed for '{value}'");
            }
            return result;
        }
    }
}
