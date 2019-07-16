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
using System.Globalization;

#endregion

namespace FpML.V5r3.Reporting
{
    public partial class Period
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            string s = $"{periodMultiplier}{period}";
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="periodToCompare"></param>
        /// <returns></returns>
        public bool Equals(Period periodToCompare)
        {
            if (periodToCompare == null)
                return false;
            bool result
                = period == periodToCompare.period
                  && double.Parse(periodMultiplier) == double.Parse(periodToCompare.periodMultiplier);
            return result;
        }
    }

    public static class FpMLExtensions
    {
        private static readonly Calendar Calendar = CultureInfo.CurrentCulture.Calendar;

        #region Object Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <returns></returns>
        public static double ToYearFraction(this Period thisPeriod)
        {
            switch (thisPeriod.period)
            {
                case PeriodEnum.D:
                    return double.Parse(thisPeriod.periodMultiplier) / 365d;
                case PeriodEnum.W:
                    return double.Parse(thisPeriod.periodMultiplier) / 52d;
                case PeriodEnum.M:
                    return double.Parse(thisPeriod.periodMultiplier) / 12d;
                case PeriodEnum.Y:
                    return double.Parse(thisPeriod.periodMultiplier);
                default:
                    throw new InvalidOperationException(
                        $"PeriodEnum '{thisPeriod.period}' is not supported in this function");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <returns></returns>
        public static Frequency ToFrequency(this Period thisPeriod)
        {
            var result
                = new Frequency
                      {
                          period = thisPeriod.period.ToString(),
                          periodMultiplier = thisPeriod.periodMultiplier
                      }; 
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <param name="periodToAdd"></param>
        /// <returns></returns>
        public static Period Sum(this Period thisPeriod, Period periodToAdd)
        {
            if (periodToAdd == null)
            {
                throw new ArgumentNullException(nameof(periodToAdd));
            }
            if (thisPeriod.period != periodToAdd.period)
            {
                string message =
                    $"Intervals must be of the same period type and they are not. Interval1 : {thisPeriod}, interval2 : {periodToAdd}";
                throw new System.Exception(message);
            }
            string newPeriodMultiplier = (thisPeriod.GetPeriodMultiplier() + periodToAdd.GetPeriodMultiplier()).ToString(CultureInfo.InvariantCulture);
            var sum
                = new Period
                {
                    period = thisPeriod.period,
                    periodMultiplier = newPeriodMultiplier
                };
            return sum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime Add(this Period thisPeriod, DateTime dateTime)
        {
            int periodMultiplierAsInt = thisPeriod.GetPeriodMultiplier();
            switch (thisPeriod.period)
            {
                case PeriodEnum.D:
                    return Calendar.AddDays(dateTime, periodMultiplierAsInt);
                case PeriodEnum.W:
                    return Calendar.AddWeeks(dateTime, periodMultiplierAsInt);
                case PeriodEnum.M:
                    return Calendar.AddMonths(dateTime, periodMultiplierAsInt);
                case PeriodEnum.Y:
                    return Calendar.AddYears(dateTime, periodMultiplierAsInt);
                default:
                    throw new ArgumentException($"PeriodEnum '{thisPeriod.period}' is not supported in this function");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <returns></returns>
        public static int GetPeriodMultiplier(this Period thisPeriod)
        {
            return int.Parse(thisPeriod.periodMultiplier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <param name="dateToStartAt"></param>
        /// <returns></returns>
        public static DateTime Subtract(this Period thisPeriod, DateTime dateToStartAt)
        {
            return thisPeriod.Negative().Add(dateToStartAt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <param name="periodToSubtract"></param>
        /// <returns></returns>
        public static Period Subtract(this Period thisPeriod, Period periodToSubtract)
        {
            if (periodToSubtract == null)
            {
                throw new ArgumentNullException(nameof(periodToSubtract));
            }
            if (thisPeriod.period != periodToSubtract.period)
            {
                string message =
                    $"Periods must be of the same period type. This period '{thisPeriod}', period to subtract '{periodToSubtract}'";
                throw new ArgumentException(message, nameof(periodToSubtract));
            }
            double multiplier = thisPeriod.GetPeriodMultiplier() - periodToSubtract.GetPeriodMultiplier();
            var difference
                = new Period
                {
                    period = thisPeriod.period,
                    periodMultiplier = multiplier.ToString(CultureInfo.InvariantCulture)
                };
            return difference;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <returns></returns>
        public static Period Negative(this Period thisPeriod)
        {
            return thisPeriod.Multiply(-1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisPeriod"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static Period Multiply(this Period thisPeriod, int multiplier)
        {
            int periodMultiplierAsInt = thisPeriod.GetPeriodMultiplier();
            var result = new Period
                             {
                                 period = thisPeriod.period,
                                 periodMultiplier =
                                     (multiplier*periodMultiplierAsInt).ToString(CultureInfo.InvariantCulture)
                             };
            //Period result = BinarySerializerHelper.Clone(thisPeriod);//TODO there is a problem with the binary serializer!
            return result;
        }

        #endregion
    }
}
