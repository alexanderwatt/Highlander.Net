using System;
using Calendars.Rules;
using National.QRSC.Runtime.Common;

namespace Calendars.Dates
{
    /// <summary>
    /// Evaluates Last Trading Days
    /// </summary>
    public class LastTradingDay: ILastTradingDay
    {
        private LastTradingDayRules LastTradingDayRules;
        private Boolean IsTradingDayRuleInitialised;

        /// <summary>
        /// Initializes a new instance of the <see cref="LastTradingDay"/> class.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity.</param>
        public LastTradingDay(ICoreCache cache, string exchangeCommodityName)
        {
            if (!IsTradingDayRuleInitialised)
            {
                InitialiseTradingDayRules(cache, exchangeCommodityName);
            }
        }

        /// <summary>
        /// Initialises the trading day rules.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity.</param>
        private void InitialiseTradingDayRules(ICoreCache cache, string exchangeCommodityName)
        {
            if (!IsTradingDayRuleInitialised)
            {
                LastTradingDayRules = new LastTradingDayRules(cache, new[] {exchangeCommodityName});
                IsTradingDayRuleInitialised = true;
            }
        }


        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public DateTime GetLastTradingDay(int month, int year)
        {
            DateTime dt = LastTradingDayRules.GetLastTradingDay(month, year);
            return dt;
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        /// <returns></returns>
        public DateTime GetLastTradingDay(bool mainCycle)
        {
            DateTime dt = LastTradingDayRules.GetLastTradingDay(mainCycle);
            return dt;
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        /// <returns></returns>
        public DateTime[] GetLastTradingDay(int year, bool mainCycle)
        {
            return LastTradingDayRules.GetLastTradingDay(year, mainCycle);
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="absoluteIMMCode">The absolute IMM code eg EDZ8.</param>
        /// <returns></returns>
        public DateTime GetLastTradingDay(string absoluteIMMCode)
        {
            DateTime dt = LastTradingDayRules.GetLastTradingDay(absoluteIMMCode);
            return dt;
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="absoluteIMMCode">The absolute IMM code.</param>
        /// <param name="decadeStartYear">The decade start year.</param>
        /// <returns></returns>
        public DateTime GetLastTradingDay(string absoluteIMMCode, int decadeStartYear)
        {
            DateTime dt = LastTradingDayRules.GetLastTradingDay(absoluteIMMCode, decadeStartYear);
            return dt;
        }
    }
}