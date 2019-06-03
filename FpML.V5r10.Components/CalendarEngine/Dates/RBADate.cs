using System;
using System.Collections.Generic;
using System.Linq;
using Orion.CalendarEngine.Helpers;

namespace Orion.CalendarEngine.Dates
{
    /// <summary>
    /// Implements the RBA calendar interface
    /// </summary>
    public class RBADate: CentralBankDate
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RBADate"/> class.
        /// </summary>
        public RBADate()
            : base(CentralBanks.RBA)
        {}

        /// <summary>
        /// Gets the date rules.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        protected override DateTime? DateRules(int month, int year)
        {
            if(!IsValidMonth(month)) return null;
            var date = RuleHelper.GetNthDayInMonth(month, 1, 3, year, 0, 0);
            return date;
        }

        /// <summary>
        /// Gets the date rules.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        protected override List<DateTime> DateRules(int year)
        {
            return ValidMeetingMonths.Select(month => RuleHelper.GetNthDayInMonth(month, 1, 3, year, 0, 0)).ToList();
        }
    }
}