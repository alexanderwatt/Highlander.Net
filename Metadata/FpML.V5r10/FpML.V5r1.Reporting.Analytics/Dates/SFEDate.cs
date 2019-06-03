#region Using Directives

using System;
using System.Collections.Generic;
using National.QRSC.FpML.V47;
using National.QRSC.ModelFramework;

#endregion

namespace Orion.Analytics.Dates
{
    ///<summary>
    ///</summary>
    public class SFEDate : LastTradingDate
    {
        //  SFE prefixed for IR futures
        //
        private static readonly string[] _futureCodesPrefixes = new[] { FutureAssetAnalyticModelIdentifier.IR, FutureAssetAnalyticModelIdentifier.IB };

        /// <summary>
        /// Base consructor, which does not filter for main cycle or not.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public SFEDate(int year, int month)
        {
            ExchangeDate = DateHelper.NthWeekday(2, 5, month, year);
        }

        /// <summary>
        /// returns the SFE date for the given IMM code
        /// (e.g. March 20th, 2013 for H3).
        ///  \warning It raises an exception if the input
        ///          string is not an IMM code
        /// </summary>
        /// <param name="immCode"></param>
        /// <param name="referenceDate"></param>
        /// <param name="mainCycle"></param>
        public SFEDate(string immCode, DateTime referenceDate, bool mainCycle)
        {
            if (IMMDate.isIMMCode(immCode, mainCycle))
            {
                int m = (int)IMMDate.Parse(immCode.ToUpper().Substring(0, 1));    
                int y = Convert.ToInt32(immCode.Substring(1, 1));
                /* year<1900 are not valid QuantLib years: to avoid a run-time
                   exception few lines below we need to add 10 years right away */
                if (y == 0 && referenceDate.Year <= 1909) y += 10;
                int referenceYear = (referenceDate.Year % 10);
                y += referenceDate.Year - referenceYear;
                _sfeDate = nextSFEDate(new DateTime(y, m, 1), mainCycle);
                if (_sfeDate < referenceDate)
                    _sfeDate = nextSFEDate(new DateTime(y + 10, m, 1), mainCycle);
            }
        }

        /// <summary>
        /// whether or not the given date is an IMM date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="mainCycle"></param>
        /// <returns>true/false</returns>
        public static bool isLastTadingDate(DateTime date, bool mainCycle)
        {
            if (date.DayOfWeek != DayOfWeek.Friday)
                return false;

            int d = date.Day;
            if (d<8 || d>14)
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
        public static DateTime nextLastTradingDate(DateTime refDate, bool mainCycle) 
        {
            int d = refDate.Day;
            int y = refDate.Year;
            int m = refDate.Month;

            int offset = mainCycle ? 3 : 1;
            int skipMonths = offset-(m%offset);
            if (skipMonths != offset || d > 14) 
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
            DateTime result = new SFEDate(y, m).Date;
            if (result<=refDate)
                result = nextSFEDate(new DateTime(y, m, 15), mainCycle);
            return result;
        }

        /// <summary>
        /// next IMM date following the given IMM code
        /// returns the 1st delivery date for next contract listed in the
        /// International Money Market section of the Chicago Mercantile
        /// Exchange.
        /// </summary>
        /// <param name="IMMcode"></param>
        /// <param name="mainCycle"></param>
        /// <param name="referenceDate"></param>
        /// <returns></returns>
        public static DateTime nextLastTradingDate(string IMMcode, bool mainCycle, DateTime referenceDate)  
        {
            DateTime sfeDate = new SFEDate(IMMcode, referenceDate, mainCycle).Date;
            return nextSFEDate(sfeDate.AddDays(1), mainCycle);
        }

        //! whether or not the given date is an IMM date
        ///<summary>
        ///</summary>
        ///<param name="d"></param>
        ///<param name="mainCycle"></param>
        ///<returns></returns>
        public static bool isSFEDate(DateTime d, bool mainCycle)
        {
            DateTime date = new SFEDate(d.Year, d.Month).Date;
            return date.Day == d.Day;
        }

        /// <summary>
        /// returns the SFE code for next contract listed in the
        ///SFE.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="mainCycle"></param>
        /// <returns>The imm code string</returns>
        public static string nextFuturesCode(DateTime d, bool mainCycle) 
        {
            DateTime date = nextSFEDate(d, mainCycle);
            return IMMDate.IMMCode(date);
        }

        /// <summary>
        /// The actual date.
        /// </summary>
        public DateTime Date
        {
            get { return _sfeDate; }
        }

        private struct FuturesPrefixImmMonthCodeAndYear
        {
            //public string FuturesPrefix;
            public readonly string ImmMonthCode;
            public readonly int Year;
            public FuturesPrefixImmMonthCodeAndYear(string futuresPrefix, string immMonthCode, string year)
            {
                //FuturesPrefix = futuresPrefix;
                ImmMonthCode = immMonthCode;
                Year = int.Parse(year);
            }
        }

        private static FuturesPrefixImmMonthCodeAndYear BreakCodeIntoPrefixAndYear(string futuresCode)
        {
            foreach (string futureCodesPrefix in _futureCodesPrefixes)
            {
                if (futuresCode.StartsWith(futureCodesPrefix))
                {
                    string immMonthCodeAndYear = futuresCode.Substring(futureCodesPrefix.Length);
                    var result = new FuturesPrefixImmMonthCodeAndYear(futureCodesPrefix, immMonthCodeAndYear[0].ToString(), immMonthCodeAndYear[1].ToString());
                    return result;
                }
            }

            throw new Exception("Futures code prefix has not been identified as such.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="futuresCode">
        /// IRH7,IRZ9 (90 day futures)
        /// 
        /// IBH8,IBU9 (30 day futures)
        /// </param>
        /// <returns></returns>
        /// <param name="referenceYear">
        /// if 2000, IRZ8 is futures that expires in December 2008. 
        /// if 2010, IRZ8 is futures that expires in December 2018. 
        /// </param>
        public static DateTime GetUnadjustedExpirationDate(string futuresCode, int referenceYear)
        {
            FuturesPrefixImmMonthCodeAndYear futuresPrefixImmMonthCodeAndYear = BreakCodeIntoPrefixAndYear(futuresCode);
            var month = (int)IMMDate.Parse(futuresPrefixImmMonthCodeAndYear.ImmMonthCode);
            //  Expiration - 2nd (2) Friday (4) of month.
            //
            //DateTime unadjustedExpirationDate = DateHelper.nthWeekday(2, 5, month, referenceYear + futuresPrefixImmMonthCodeAndYear.Year);
            DateTime unadjustedExpirationDate;
            if (futuresPrefixImmMonthCodeAndYear.Year < referenceYear % 10)
            {
                int realYear = referenceYear - (referenceYear % 10) + futuresPrefixImmMonthCodeAndYear.Year + 10;
                unadjustedExpirationDate = DateHelper.NthWeekday(2, 4, month, realYear);
            }
            else
            {
                int realYear = referenceYear - (referenceYear % 10) + futuresPrefixImmMonthCodeAndYear.Year;
                unadjustedExpirationDate = DateHelper.NthWeekday(2, 4, month, realYear);
            }


            return unadjustedExpirationDate;
        }

        ///<summary>
        ///</summary>
        ///<param name="relativeCode"></param>
        ///<returns></returns>
        ///<exception cref="Exception"></exception>
        public static int GetFuturesPositionByCode(string relativeCode)
        {
            foreach (string futureCodesPrefix in _futureCodesPrefixes)
            {
                if (relativeCode.StartsWith(futureCodesPrefix))
                {
                    string futuresPosition = relativeCode.Substring(futureCodesPrefix.Length);
                    int futuresPositionAsInt = int.Parse(futuresPosition);
                    return futuresPositionAsInt;
                }
            }

            throw new Exception("Futures code prefix has not been identified as such.");
        }

        ///<summary>
        ///</summary>
        ///<param name="relativeCode"></param>
        ///<returns></returns>
        ///<exception cref="Exception"></exception>
        public static bool IsRelativeCode(string relativeCode)
        {
            foreach (string futureCodesPrefix in _futureCodesPrefixes)
            {
                if (relativeCode.StartsWith(futureCodesPrefix))
                {
                    string futuresPosition = relativeCode.Substring(futureCodesPrefix.Length);
                    int dummy;
                    return int.TryParse(futuresPosition, out dummy);
                }
            }

            throw new Exception("Futures code prefix has not been identified as such.");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="absoluteCode">e.g. Z8</param>
        /// <returns>e.g. H9</returns>
        public static string GetFollowingAbsoluteCode(string absoluteCode)
        {
            if (2 != absoluteCode.Length)
            {
                string message = String.Format("{0} is not recognised as absolute futures code. Examples are: 'H8', 'Z9', etc", absoluteCode);
                throw new Exception(message);
            }
            string monthCode = absoluteCode[0].ToString();
            int yearCode = int.Parse(absoluteCode[1].ToString());
            var codesArray = new[] { "H", "M", "U", "Z" };

            if (codesArray.Length - 1 == Array.IndexOf(codesArray, monthCode))
            {
                ++yearCode;
                monthCode = codesArray[0];
            }
            else
            {
                monthCode = codesArray[Array.IndexOf(codesArray, monthCode) + 1];
            }

            return String.Format("{0}{1}", monthCode, yearCode % 10);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>e.g "Z8"</returns>
        public static string GetNextAbsoluteCode(DateTime referenceDate)
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
                DateTime unadjustedExpirationDate = DateHelper.NthWeekday(2, 4, referenceMonth, referenceYear);
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

            return String.Format("{0}{1}", absoluteMonthCode, referenceYear % 10);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeCode">
        /// IR01
        /// IR02
        /// IR03 
        /// etc
        /// </param>
        /// <param name="referenceDate"></param>
        /// <returns></returns>
        public static string ConvertRelativeToAbsoluteCode(string relativeCode, DateTime referenceDate)
        {
            string subCode = relativeCode.Substring(2);
            string nextAbsoluteCode = GetNextAbsoluteCode(referenceDate);
            int numberOfIterations = int.Parse(subCode);
            while (--numberOfIterations > 0)
            {
                nextAbsoluteCode = GetFollowingAbsoluteCode(nextAbsoluteCode);
            }

            return "IR" + nextAbsoluteCode;
        }

    }
}