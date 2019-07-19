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

using System;
using Orion.Util.Helpers;

namespace FpML.V5r10.Reporting.Helpers
{
    public static class IntervalHelper
    {
        public static bool HaveTheSamePeriodType(Period interval1, Period interval2)
        {
            return interval1.period == interval2.period;
        }

        public static Period FromDays(int days)
        {
            return PeriodHelper.Parse(days + "D");
        }

        public static Period FromWeeks(int weeks)
        {
            return PeriodHelper.Parse(weeks + "W");
        }

        public static Period FromMonths(int months)
        {
            return PeriodHelper.Parse(months + "M");
        }

        public static Period FromYears(int years)
        {
            return PeriodHelper.Parse(years + "Y");
        }

        public static Period FromFrequency(Frequency frequency)
        {
            var result
                = new Period
                      {
                          period = EnumHelper.Parse<PeriodEnum>(frequency.period, true),
                          periodMultiplier = frequency.periodMultiplier
                      };
            return result;
        }

        [Obsolete("Use [period].ToString()")]
        public static string ToString(Period interval)
        {
            return interval.ToString();
        }

        [Obsolete("Use [period].Sum()")]
        public static Period Sum(Period period1, Period period2)
        {
            if (period1 == null)
            {
                throw new ArgumentNullException(nameof(period1));
            }
            return period1.Sum(period2);
        }

        [Obsolete("Use [period].Add()")]
        public static DateTime Add(DateTime dateTime, Period periodToAdd)
        {
            if (periodToAdd == null)
            {
                throw new ArgumentNullException(nameof(periodToAdd));
            }
            return periodToAdd.Add(dateTime);
        }

        [Obsolete("Use [period].Subtract()")]
        public static DateTime Sub(DateTime dateTime, Period periodToSubtract)
        {
            if (periodToSubtract == null)
            {
                throw new ArgumentNullException(nameof(periodToSubtract));
            }
            return periodToSubtract.Negative().Add(dateTime);
        }

        [Obsolete("Use [period].Subtract()")]
        public static Period Diff(Period period1, Period period2)
        {
            if (period1 == null)
            {
                throw new ArgumentNullException(nameof(period1));
            }
            return period1.Subtract(period2);
        }

        [Obsolete("Use [period].Negative()")]
        public static Period Neg(Period periodToNegate)
        {
            if (periodToNegate == null)
            {
                throw new ArgumentNullException(nameof(periodToNegate));
            }
            return periodToNegate.Negative();
        }

        [Obsolete("Use [period].Multiply()")]
        public static Period Mul(Period periodToMultiply, int multiplier)
        {
            if (periodToMultiply == null)
            {
                throw new ArgumentNullException(nameof(periodToMultiply));
            }
            return periodToMultiply.Multiply(multiplier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static double Div(Period dividend, Period divisor)
        {
            if (HaveTheSamePeriodType(dividend, divisor))
            {
                double periodsInDividend = dividend.GetPeriodMultiplier();
                double periodsInDivisor = divisor.GetPeriodMultiplier();

                return periodsInDividend / periodsInDivisor;
            }
            // Y DIV M is the only supported div operation as of now.
            //
            if (!(dividend.period == PeriodEnum.Y & divisor.period == PeriodEnum.M))
            {
                throw new NotSupportedException();
            }

            double monthPeriodsInDividend = dividend.GetPeriodMultiplier() * GetMonthMultiplier(dividend.period);
            double monthPeriodsInDivisor = divisor.GetPeriodMultiplier() * GetMonthMultiplier(divisor.period);

            return monthPeriodsInDividend / monthPeriodsInDivisor;
        }

        /// <summary>
        /// Gets the month multiplier.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        private static int GetMonthMultiplier(PeriodEnum period)
        {
            int multiplier = 1;

            switch (period)
            {
                case PeriodEnum.M:
                    break;
                case PeriodEnum.Y:
                    multiplier = 12;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(period), "period does not translate to a whole month");
            }
            return multiplier;
        }

        #region Period Operator

        /// <summary>
        /// Add Period x to Period y
        /// </summary>
        /// <param name="x">Period x</param>
        /// <param name="y">Period y</param>
        /// <returns>A Year fraction representation of the addition of the two intervals</returns>
        public static double Add(Period x, Period y)
        {
            double xAsYF = x.ToYearFraction();
            double yAsYF = y.ToYearFraction();
            return xAsYF + yAsYF;
        }
        
        #endregion

        #region Magnitude Operators

        /// <summary>
        /// Test intervals to determoine x > y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Greater(Period x, Period y)
        {
            // If x is null then this is always false
            // If y is null then this is always true
            if (x == null)
                return false;
            if (y == null)
                return true;

            if (x.period > y.period)
                return true;
            if (x.period == y.period)
            {
                if (double.Parse(x.periodMultiplier) > double.Parse(y.periodMultiplier))
                    return true;
                return false;
            }
            return false;
        }

        public static bool Less(Period x, Period y)
        {
            // If x is null then this is always true
            // If y is null then this is always false
            if (x == null)
                return true;
            if (y == null)
                return false;

            if (x.period < y.period)
                return true;
            if (x.period == y.period)
            {
                if (double.Parse(x.periodMultiplier) < double.Parse(y.periodMultiplier))
                    return true;
                return false;
            }
            return false;
        }

        #endregion
    }
}