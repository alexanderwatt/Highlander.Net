#region Using Directives

using System;
using System.Collections.Generic;
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CalendarEngine.Dates
{
    ///<summary>
    ///</summary>
    public class ThirdWednesday : LastTradingDate
    {
        /// <summary>
        /// Base consructor, which does not filter for main cycle or not.
        /// </summary>
        public ThirdWednesday()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdWednesday"/> class.
        /// </summary>
        /// <param name="exchangeCommodityCode">Name of the exchange commodity.</param>
        ///  <param name="expiryMonthAndYear">Name of the expiryMonthAndYeary.</param>
        public ThirdWednesday(string exchangeCommodityCode, string expiryMonthAndYear)
            : base(exchangeCommodityCode, expiryMonthAndYear)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdWednesday"/> class.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity e.g. EDZ8.</param>
        public ThirdWednesday(string exchangeCommodityName)
            : base(exchangeCommodityName)
        {}

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public override DateTime GetLastTradingDay(int month, int year)
        {
            var significantDate = RuleHelper.GetNthDayInMonth(month, 3, 3, year, 0, 0);
            return significantDate;
        }

        /// <summary>
        /// whether or not the given date is an IMM date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="mainCycle"></param>
        /// <returns>true/false</returns>
        public override bool isLastTradingDate(DateTime date, bool mainCycle)
        {
            if (date.DayOfWeek != DayOfWeek.Friday)
                return false;

            int d = date.Day;
            if (d<17 || d>23)
                return false;

            if (!mainCycle) return true;

            int m = date.Month;
            return (m == 3 || m == 6 || m == 9 || m == 12);
        }

        /// <summary>
        /// next IMM date following the given date
        /// returns the 1st delivery date for next contract listed in the
        /// International Money Market section of the Chicago Mercantile
        /// Exchange.
        /// </summary>
        /// <param name="refDate"></param>
        /// <param name="mainCycle"></param>
        /// <returns></returns>
        public override DateTime nextLastTradingDate(DateTime refDate, bool mainCycle) 
        {
            int d = refDate.Day;
            int y = refDate.Year;
            int m = refDate.Month;

            int offset = mainCycle ? 3 : 1;
            int skipMonths = offset-(m%offset);
            if (skipMonths != offset || d > 23) 
            {
                skipMonths += m;
                if (skipMonths<=12) 
                {
                    m = skipMonths;
                } 
                else 
                {
                    m = skipMonths-12;
                    y += 1;
                }
            }
            DateTime result = GetLastTradingDay(m, y);
            if (result<=refDate)
                result = nextLastTradingDate(new DateTime(y, m, 24), mainCycle);
            return result;
        }

        /// <summary>
        /// IRH7,IRZ9 (90 day futures)
        /// 
        /// IBH8,IBU9 (30 day futures)
        /// </summary>
        /// <returns></returns>
        /// <param name="referenceDate">
        /// if 2000, IRZ8 is futures that expires in December 2008. 
        /// if 2010, IRZ8 is futures that expires in December 2018. 
        /// </param>
        public override DateTime GetLastTradingDay(DateTime referenceDate)
        {
            //FuturesPrefixImmMonthCodeAndYear futuresPrefixImmMonthCodeAndYear = BreakCodeIntoPrefixAndYear(futuresCode);
            var month = (int)LastTradingDayHelper.ParseToCode(CodeAndExpiryMonth.ImmMonthCode);
            //  Expiration - 3rd (3) Wednesday (3) of month.
            //
            //DateTime unadjustedExpirationDate = DateHelper.nthWeekday(2, 5, month, referenceYear + futuresPrefixImmMonthCodeAndYear.Year);
            DateTime unadjustedExpirationDate;
            if (CodeAndExpiryMonth.Year < referenceDate.Year % 10)
            {
                int realYear = referenceDate.Year - (referenceDate.Year % 10) + CodeAndExpiryMonth.Year + 10;
                unadjustedExpirationDate = RuleHelper.GetNthDayInMonth(month, 3, 3, realYear, 0, 0);
            }
            else
            {
                int realYear = referenceDate.Year - (referenceDate.Year % 10) + CodeAndExpiryMonth.Year;
                unadjustedExpirationDate = RuleHelper.GetNthDayInMonth(month, 3, 3, realYear, 0, 0);
            }
            return unadjustedExpirationDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>e.g "Z8"</returns>
        public override string GetNextAbsoluteMainCycleCode(DateTime referenceDate)
        {
            int referenceYear = referenceDate.Year;
            var table = new Dictionary<int, string>
                            {
                                {1, "H"},
                                {2, "H"},
                                {3, "H"},
                                {4, "M"},
                                {5, "M"},
                                {6, "M"},
                                {7, "U"},
                                {8, "U"},
                                {9, "U"},
                                {10, "Z"},
                                {11, "Z"},
                                {12, "Z"}
                            };

            int referenceMonth = referenceDate.Month;
            string absoluteMonthCode = table[referenceMonth];
            //  check if adjustment required (if it is already e.g. 20th of the March - the March futures has already expired)
            //
            if (referenceMonth % 3 == 0)//check if need to advance to next code
            {
                DateTime unadjustedExpirationDate = RuleHelper.GetNthDayInMonth(referenceMonth, 3, 3, referenceYear, 0, 0);
                if (referenceDate >= unadjustedExpirationDate)//if option expires today - move to next code
                {
                    if (referenceMonth == 12)
                    {
                        referenceMonth = 1;
                        ++referenceYear;
                    }
                    else
                    {
                        ++referenceMonth;
                    }
                    absoluteMonthCode = table[referenceMonth];
                }
            }

            return $"{absoluteMonthCode}{referenceYear % 10}";
        }
    }
}