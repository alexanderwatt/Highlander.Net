using System;

namespace Orion.CalendarEngine.Helpers
{
    /// <summary>
    /// A significant day in a calendar created as a resulkt of a date rule
    /// </summary>
    public class SignificantDay : IComparable
    {
        /// <summary>
        /// Represnts the actual date of a given holiday
        /// 
        /// </summary>
        public DateTime Date = DateTime.MinValue;

        /// <summary>
        /// Represnts the name of a given holiday
        /// </summary>
        public string Name;


        /// <summary>
        /// Represents the associated business day holiday date
        /// </summary>
        public DateTime ObservedSignificantDayDate = DateTime.MinValue;


        /// <summary>
        /// Gets a value indicating whether this instance is weekend.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is weekend; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsWeekend => RuleHelper.IsWeekend(Date);

        /// <summary>
        /// Gets a value indicating whether this instance is business holiday date A weekend.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is business holiday date A weekend; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsSignificantDayDateAWeekend => RuleHelper.IsWeekend(ObservedSignificantDayDate);

        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than obj. Zero This instance is equal to obj. Greater than zero This instance is greater than obj.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">obj is not the same type as this instance. </exception>
        public int CompareTo(object obj)
        {
            if (obj is SignificantDay)
            {
                var s = (SignificantDay)obj;
                return Date.CompareTo(s.Date);
            }
            throw new ArgumentException("Object is not a Significant Day");
        }
        #endregion
    }
}