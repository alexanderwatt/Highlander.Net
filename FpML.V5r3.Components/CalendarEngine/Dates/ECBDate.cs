using System;
using System.Collections.Generic;
using System.Linq;
using Orion.CalendarEngine.Helpers;

namespace Orion.CalendarEngine.Dates
{
    /// <summary>
    /// Implements the ECB calendar interface
    /// </summary>
    public class ECBDate: CentralBankDate
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ECBDate"/> class.
        /// </summary>
        public ECBDate()
            : base(CentralBanks.ECB)
        {}

        /// <summary>
        /// Gets the date rules.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        protected override DateTime? DateRules(int month, int year)
        {
            if (!IsValidMonth(month)) return null;
            var date = RuleHelper.GetNthDayInMonth(month, 2, 2, year, 0, 0);
            return date;
        }

        /// <summary>
        /// Gets the date rules.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        protected override List<DateTime> DateRules(int year)
        {
            return ValidMeetingMonths.Select(month => RuleHelper.GetNthDayInMonth(month, 2, 2, year, 0, 0)).ToList();
        }
    }
}