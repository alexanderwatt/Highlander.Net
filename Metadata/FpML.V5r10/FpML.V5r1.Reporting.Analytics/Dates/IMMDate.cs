#region Using Directives

using System;
using National.QRSC.FpML.V47;
using National.QRSC.ModelFramework;
using Orion.Analytics.Helpers;

#endregion

namespace Orion.Analytics.Dates
{
    ///<summary>
    ///</summary>
    public class IMMDate : LastTradingDate
    {       
        /// <summary>
        /// Base consructor, which does not filter for main cycle or not.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public IMMDate(int year, int month)
        {
            ExchangeDate = DateHelper.NthWeekday(3, 3, month, year);
        }

        /// <summary>
        /// returns the IMM date for the given IMM code
        /// (e.g. March 20th, 2013 for H3).
        ///  \warning It raises an exception if the input
        ///          string is not an IMM code
        /// </summary>
        /// <param name="immCode"></param>
        /// <param name="referenceDate"></param>
        /// <param name="mainCycle"></param>
        public IMMDate(string immCode, DateTime referenceDate, bool mainCycle)
        {
            if (isIMMCode(immCode, mainCycle))
            {
                int m = (int)Parse(immCode.ToUpper().Substring(0, 1));    
                int y = Convert.ToInt32(immCode.Substring(1, 1));
                /* year<1900 are not valid QuantLib years: to avoid a run-time
                   exception few lines below we need to add 10 years right away */
                if (y == 0 && referenceDate.Year <= 1909) y += 10;
                int referenceYear = (referenceDate.Year % 10);
                y += referenceDate.Year - referenceYear;
                ExchangeDate = nextIMMDate(new DateTime(y, m, 1), mainCycle);
                if (ExchangeDate < referenceDate)
                    ExchangeDate = nextIMMDate(new DateTime(y + 10, m, 1), mainCycle);
            }
        }

        /// <summary>
        /// whether or not the given date is an IMM date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="mainCycle"></param>
        /// <returns>true/false</returns>
        public static bool isIMMdate(DateTime date, bool mainCycle)
        {
            if (date.DayOfWeek != DayOfWeek.Wednesday)
                return false;
            int d = date.Day;
            if (d<15 || d>21)
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
        public static DateTime nextIMMDate(DateTime refDate, bool mainCycle) 
        {
            int d = refDate.Day;
            int y = refDate.Year;
            int m = refDate.Month;

            int offset = mainCycle ? 3 : 1;
            int skipMonths = offset-(m%offset);
            if (skipMonths != offset || d > 21) 
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
            DateTime result = new IMMDate(y, m).Date;
            if (result<=refDate)
                result = nextIMMDate(new DateTime(y, m, 22), mainCycle);
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
        public static DateTime nextIMMDate(string IMMcode, bool mainCycle, DateTime referenceDate)  
        {
            DateTime immDate = new IMMDate(IMMcode, referenceDate, mainCycle).Date;
            return nextIMMDate(immDate.AddDays(1), mainCycle);
        }

        //! whether or not the given date is an IMM date
        ///<summary>
        ///</summary>
        ///<param name="d"></param>
        ///<param name="mainCycle"></param>
        ///<returns></returns>
        public static bool isIMMDate(DateTime d, bool mainCycle)
        {
            DateTime date = new IMMDate(d.Year, d.Month).Date;
            return date.Day == d.Day;
        }

        /// <summary>
        /// returns the IMM code for next contract listed in the
        ///International Money Market section of the Chicago Mercantile
        ///Exchange.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="mainCycle"></param>
        /// <returns>The imm code string</returns>
        public static string nextIMMCode(DateTime d, bool mainCycle) 
        {
            DateTime date = nextIMMDate(d, mainCycle);
            return IMMCode(date);
        }

        /// <summary>
        /// The actual date.
        /// </summary>
        public DateTime Date
        {
            get { return _immDate; }
        }

        /// <summary>
        /// Parses a string representation to am imm month code,
        /// </summary>
        /// <param name="immMonth"></param>
        /// <returns>The imm month code.</returns>
        public static FuturesCodesEnum Parse(string immMonth)
        {

            switch (immMonth)
            {
                case "F": return FuturesCodesEnum.F;
                case "G": return FuturesCodesEnum.G;
                case "H": return FuturesCodesEnum.H;
                case "J": return FuturesCodesEnum.J;
                case "K": return FuturesCodesEnum.K;
                case "M": return FuturesCodesEnum.M;
                case "N": return FuturesCodesEnum.N;
                case "Q": return FuturesCodesEnum.Q;
                case "U": return FuturesCodesEnum.U;
                case "V": return FuturesCodesEnum.V;
                case "X": return FuturesCodesEnum.X;
                case "Z": return FuturesCodesEnum.Z;
                default: return FuturesCodesEnum.Unknown;
            }
        }

        /// <summary>
        /// returns the IMM code for the given date
        ///(e.g. H3 for March 20th, 2013).
        ///\warning It raises an exception if the input
        //date is not an IMM date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string IMMCode(DateTime date)
        {
            string result = "invalid date.";
            if (isIMMDate(date, false))
            {
                int y = date.Year % 10;

                switch (date.Month)
                {
                    case 1:
                        result = "F" + y;
                        break;
                    case 2:
                        result = "G" + y;
                        break;
                    case 3:
                        result = "H" + y;
                        break;
                    case 4:
                        result = "J" + y;
                        break;
                    case 5:
                        result = "K" + y;
                        break;
                    case 6:
                        result = "M" + y;
                        break;
                    case 7:
                        result = "N" + y;
                        break;
                    case 8:
                        result = "Q" + y;
                        break;
                    case 9:
                        result = "U" + y;
                        break;
                    case 10:
                        result = "V" + y;
                        break;
                    case 11:
                        result = "X" + y;
                        break;
                    case 12:
                        result = "Z" + y;
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// whether or not the given string is an IMM code
        /// </summary>
        /// <param name="expiryCode"></param>
        /// <param name="mainCycle"></param>
        /// <returns>true/false</returns>
        public static bool isIMMCode(string expiryCode, bool mainCycle)
        {
            bool result = false;
            const string str2 = "0123456789";
            string str1 = mainCycle ? "hmzuHMZU" : "fghjkmnquvxzFGHJKMNQUVXZ";
            if (expiryCode.Length == 2)
                result = (str1.Contains(expiryCode.Substring(0, 1))) && (str2.Contains(expiryCode.Substring(1, 1)));
            //            else if (expiryCode.Length == 3)
            //                result = (str1.Contains(expiryCode.Substring(0,1)))&& (str2.Contains(expiryCode.Substring(1,1)))&& (str2.Contains(expiryCode.Substring(2,1)));
            return result;
        }

    }
}