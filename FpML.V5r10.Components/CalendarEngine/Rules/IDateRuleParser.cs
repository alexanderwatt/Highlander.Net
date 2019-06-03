using System.Collections.Generic;
namespace Orion.CalendarEngine.Rules
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDateRuleParser
    {
        /// <summary>
        /// 
        /// </summary>
        List<string> InValidCentres { get; }

        /// <summary>
        /// 
        /// </summary>
        List<DateRuleProfile> DateRuleProfiles { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsValidCalendar { get; set; }

        /// <summary>
        /// 
        /// </summary>
        System.Globalization.Calendar[] Calendar { get; }

        /// <summary>
        /// 
        /// </summary>
        string[] CalendarNames { get; }

        /// <summary>
        /// 
        /// </summary>
        System.Globalization.CultureInfo[] Culture { get; }

        /// <summary>
        /// 
        /// </summary>
        string[] FpmlName { get; }

        /// <summary>
        /// 
        /// </summary>
        string[] Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> GetCalendarsSupported();
    }
}
