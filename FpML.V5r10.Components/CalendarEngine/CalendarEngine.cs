#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting.Helpers;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using Orion.Analytics.BusinessCenters;
using Orion.Analytics.Helpers;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Helpers;
using Orion.CalendarEngine.Rules;
using Orion.Constants;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Util.NamedValues;
using BusinessCentreDateRulesProp = Orion.Constants.BusinessCentreDateRulesProp;

#endregion

namespace Orion.CalendarEngine
{
    /// <summary>
    /// An intantiable calendar engine!.
    /// </summary>
    public class CalendarEngine
    {
        #region Private fields

        public ICoreCache Cache;
        public ILogger Logger;
        public readonly string NameSpace;

        #endregion

        #region Constructor

        /// <summary>
        /// THe main constructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        public CalendarEngine(ILogger logger, ICoreCache cache)
            : this(logger, cache, EnvironmentProp.DefaultNameSpace)
        {
        }

        /// <summary>
        /// THe main constructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        public CalendarEngine(ILogger logger, ICoreCache cache, string nameSpace)
        {
            Logger = logger;
            Cache = cache;
            NameSpace = nameSpace;
            //1. Load up relevant data.
            CacheDateRules();
            //CacheHolidays();
            CacheBusinessCenterHolidays();
        }

        #endregion

        #region Load Configuration Data

        /// <summary>
        /// Caches the instruments from the database.
        /// </summary>
        private void CacheDateRules()
        {
            var requestProperties = new NamedValueSet();
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            List<ICoreItem> items = Cache.LoadItems<DateRules>(queryExpr);
            if (items.Count == 0)
            {
                throw new ApplicationException(
                    $"The search using the query '{queryExpr.DisplayString()}' yielded no results.");
            }
        }

        /// <summary>
        /// Caches the instruments from the database.
        /// </summary>
        private void CacheBusinessCenterHolidays()
        {
            var requestProperties = new NamedValueSet();
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            List<ICoreItem> items = Cache.LoadItems<BusinessCenterCalendar>(queryExpr);
            if (items.Count == 0)
            {
                throw new ApplicationException(
                    $"The search using the query '{queryExpr.DisplayString()}' yielded no results.");
            }
        }

        #endregion

        #region Adjusted Dates

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="adjustableDate"></param>
        /// <returns></returns>
        public DateTime ToAdjustedDate(AdjustableDate adjustableDate)
        {
            if (null == adjustableDate)
                throw new ArgumentNullException(nameof(adjustableDate));
            // handle  BusinessDatConventionEnum is NONE as a special case, since there might be no business centers provided. 
            //
            if ((null == adjustableDate.dateAdjustments) ||
                (BusinessDayConventionEnum.NONE == adjustableDate.dateAdjustments.businessDayConvention))
            {
                return adjustableDate.unadjustedDate.Value;
            }
            IBusinessCalendar businessCalendar = ToBusinessCalendar(adjustableDate.dateAdjustments.businessCenters);
            DateTime result = businessCalendar.Roll(adjustableDate.unadjustedDate.Value, adjustableDate.dateAdjustments.businessDayConvention);
            return result;
        }

        /// <summary>
        /// Converts to an adjusted date.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="relativeDateOffset"></param>
        /// <returns></returns>
        public DateTime ToAdjustedDate(DateTime referenceDate, RelativeDateOffset relativeDateOffset)
        {
            // handle  BusinessDatConventionEnum is NONE as a special case, since there might be no business centers provided. 
            if (BusinessDayConventionEnum.NONE == relativeDateOffset.businessDayConvention)
            {
                return referenceDate;
            }
            //The default daytype.
            if (relativeDateOffset.dayTypeSpecified == false || relativeDateOffset.businessCenters == null)
            {
                relativeDateOffset.dayType = DayTypeEnum.Calendar;
            }
            IBusinessCalendar businessCalendar = relativeDateOffset.businessCenters == null ? new Hell() : ToBusinessCalendar(relativeDateOffset.businessCenters);
            var interval = PeriodHelper.Parse(relativeDateOffset.periodMultiplier + relativeDateOffset.period);
            var offset = OffsetHelper.FromInterval(interval, relativeDateOffset.dayType);
            DateTime result = businessCalendar.Advance(referenceDate, offset, relativeDateOffset.businessDayConvention);
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConventions"></param>
        /// <param name="businessCentersAsString"></param>
        /// <returns></returns>
        public DateTime ToAdjustedDate(DateTime unadjustedDate, string businessDayConventions, string businessCentersAsString)
        {
            AdjustableDate adjustableDate = DateTypesHelper.ToAdjustableDate(unadjustedDate, businessDayConventions, businessCentersAsString);
            DateTime adjustedDate = ToAdjustedDate(adjustableDate);
            return adjustedDate;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <returns></returns>
        public DateTime ToAdjustedDate(DateTime unadjustedDate, BusinessDayAdjustments businessDayAdjustments)
        {
            var adjustableDate = new AdjustableDate
            {
                unadjustedDate = new IdentifiedDate { Value = unadjustedDate },
                dateAdjustments = businessDayAdjustments
            };
            return ToAdjustedDate(adjustableDate);
        }

        /// <summary>
        /// Converts to a date time.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public DateTime ToAdjustedDate(DateTime unadjustedDate, BusinessDayAdjustments businessDayAdjustments, Offset offset)
        {
            if (DayTypeEnum.Business != offset.dayType && DayTypeEnum.Calendar != offset.dayType)
            {
                throw new NotSupportedException(
                    $"Only {DayTypeEnum.Business}, {DayTypeEnum.Calendar} day types supported of Offset type.");
            }
            IBusinessCalendar businessCalendar = ToBusinessCalendar(businessDayAdjustments.businessCenters);
            int periodMultiplier = int.Parse(offset.periodMultiplier);
            // offset using calendar days
            //
            switch (offset.dayType)
            {
                case DayTypeEnum.Business:
                    {
                        switch (offset.period)
                        {
                            case PeriodEnum.D:
                                {
                                    // Advance using given number of business days
                                    //
                                    int periodMultiplierSign = System.Math.Sign(periodMultiplier);
                                    DateTime offsetedDate = unadjustedDate;
                                    while (periodMultiplier-- > 0)
                                    {
                                        offsetedDate = offsetedDate.AddDays(periodMultiplierSign);
                                        offsetedDate = businessCalendar.Roll(offsetedDate, businessDayAdjustments.businessDayConvention);
                                    }

                                    return offsetedDate;
                                }

                            default:
                                throw new NotSupportedException(
                                    $"{offset.period} not supported in conjunction with '{offset.dayType} day type'");

                        }//~switch(offset.period)
                    }

                case DayTypeEnum.Calendar:
                    {
                        // Convert offset to period.
                        DateTime adjustedDate = offset.Add(unadjustedDate);

                        return adjustedDate;
                    }

                default:
                    {
                        throw new NotSupportedException(
                            $"Only {DayTypeEnum.Business}, {DayTypeEnum.Calendar} day types supported of Offset type.");
                    }
            }
        }

        #endregion

        #region Business Calendars and Calendar Related Functions.

        /// <summary>
        /// Creates a consolidated business calendar for a given sset of business centers
        /// </summary>
        /// <param name="centers">The centers.</param>
        /// <returns></returns>
        public IBusinessCalendar ToBusinessCalendar(BusinessCenters centers)
        {
            var calendars = centers.businessCenter.Select(businessCenter => businessCenter.Value).ToArray();
            var dps = GetDateRuleParser(calendars, NameSpace);
            var significantDays = GetSignificantDates(Dedupe(dps.FpmlName));
            IBusinessCalendar result = new BusinessCalendar(significantDays, dps);
            return result;
        }

        /// <summary>
        /// Gets/Creates the calendar.
        /// </summary>
        /// <param name="calendars">The calendars.</param>
        /// <returns></returns>
        public IBusinessCalendar GetCalendar(string[] calendars)
        {
            string[] calendarNames = Dedupe(calendars);
            var dps = GetDateRuleParser(calendarNames, NameSpace);
            var significantDays = GetSignificantDates(Dedupe(dps.FpmlName));
            IBusinessCalendar cal = new BusinessCalendar(significantDays, dps);
            return cal;
        }

        /// <summary>
        /// Gets/Creates the calendar.
        /// </summary>
        /// <param name="years">The years.</param>
        /// <param name="calendars">The calendars.</param>
        /// <returns></returns>
        public IBusinessCalendar GetCalendar(int[] years, string[] calendars)
        {
            string[] calendarNames = Dedupe(calendars);
            var dps = GetDateRuleParser(calendarNames, NameSpace);
            var significantDays = GetSignificantDates(years, Dedupe(dps.FpmlName));
            IBusinessCalendar cal = new BusinessCalendar(significantDays, dps);
            return cal;
        }

        private IDateRuleParser GetDateRuleParser(string[] calendars, string nameSpace)
        {
            var path = nameSpace + "." + BusinessCentreDateRulesProp.GenericName;
            var loadedObject = Cache.LoadObject<DateRules>(path);
            var dps = new DateRuleParser(calendars, loadedObject.DateRuleProfile);
            return dps;
        }

        /// <summary>
        /// Calendars that are supported.
        /// </summary>
        /// <returns></returns>
        public string[] CalendarsSupported()
        {
            var path = NameSpace + "." + BusinessCentreDateRulesProp.GenericName;
            var loadedObject = Cache.LoadObject<DateRules>(path);
            var calendars = new List<string>();
            foreach (DateRuleProfile drp in loadedObject.DateRuleProfile)
            {
                if (!calendars.Contains(drp.name) && drp.enabled)
                    calendars.Add(drp.name);
            }
            string[] calArray = { };
            if (calendars.Count > 0)
            {
                calArray = new string[calendars.Count];
                calendars.CopyTo(calArray, 0);
            }
            return calArray;
        }

        /// <summary>
        /// Gets the supported business centres of the set provided.
        /// </summary>
        /// <param name="businessCenters">The list of centres in the calendar.</param>
        /// <returns></returns>
        public List<string> CalendarsSupported(string[] businessCenters)
        {
            string[] calendarNames = Dedupe(businessCenters);
            var dps = GetDateRuleParser(calendarNames, NameSpace);
            return dps.GetCalendarsSupported();
        }

        /// <summary>
        /// Determines if the list of business centres creates a valid calendar.
        /// </summary>
        /// <param name="businessCenters">The list of business centres to check.</param>
        /// <returns></returns>
        public bool IsValidBusinessCalendar(string[] businessCenters)
        {
            var dps = GetDateRuleParser(businessCenters, NameSpace);
            return dps.IsValidCalendar;
        }

        /// <summary>
        /// Determines whether [is valid calendar] [the specified rule profile].
        /// </summary>
        /// <param name="businessCenters">The list of business centres to check.</param>
        /// <returns></returns>
        public Dictionary<string, Boolean> IsValidCalendar(string[] businessCenters)
        {
            var retVals = new Dictionary<string, bool>();
            if (businessCenters != null)
            {
                var valid = CalendarsSupported(businessCenters);
                foreach (string profile in valid)
                {
                    retVals.Add(profile, !retVals.ContainsKey(profile));
                }
            }
            return retVals;
        }

        /// <summary>
        /// Signficant dates for the requested busuiness centres.
        /// </summary>
        /// <param name="businessCenters">The city names.</param>
        /// <returns></returns>
        public IEnumerable<DateTime> GetBusinessCentreHolidayDates(string[] businessCenters)
        {
            IEnumerable<DateTime> dates = null;
            //The new filter with OR on arrays..
            var path = NameSpace + "." + BusinessCenterCalendarProp.GenericName;
            foreach (var centre in businessCenters)
            {
                var identifier = path + '.' + centre;
                var loadedObject = Cache.LoadObject<BusinessCenterCalendar>(identifier);
                if (loadedObject?.Holidays != null)
                {
                    dates = loadedObject.Holidays.Select(dr => (DateTime)dr.Item);
                }
            }
            return dates;
        }

        /// <summary>
        /// Signficants the date.
        /// </summary>
        /// <param name="years">The years to return.</param>
        /// <param name="businessCenter">The city name.</param>
        /// <returns></returns>
        public IList<DateTime> GetBusinessCentreHolidayDates(int[] years, BusinessCenterEnum businessCenter)
        {
            var dates = new List<DateTime>();
            var path = NameSpace + "." + BusinessCenterCalendarProp.GenericName;
            var identifier = path + '.' + businessCenter;
            var loadedObject = Cache.LoadObject<BusinessCenterCalendar>(identifier);
            if (loadedObject?.Holidays != null)
            {
                dates.AddRange(loadedObject.Holidays.Select(dr => (DateTime)dr.Item));
            }
            return (IList<DateTime>)dates.Distinct();
        }

        /// <summary>
        /// Signficants the date.
        /// </summary>
        /// <param name="years">The years.</param>
        /// <param name="businessCenters">The city names.</param>
        /// <returns></returns>
        public List<SignificantDay> GetSignificantDates(int[] years, string[] businessCenters)//TODO use specific names!
        {
            var dateList = new List<SignificantDay>();
            var path = NameSpace + "." + BusinessCenterCalendarProp.GenericName;
            //REmove the NONE calendar - businessCenters.
            foreach (var centre in businessCenters)
            {
                if (centre == "NONE") return dateList;
                var identifier = path + '.' + centre;
                var loadedObject = Cache.LoadObject<BusinessCenterCalendar>(identifier);
                if (loadedObject?.Holidays != null)
                {
                    var dates = loadedObject.Holidays.Select(dr => (DateTime)dr.Item);
                    // Get the dates as one list
                    var result = new List<DateTime>();
                    foreach (var year in years)
                    {
                        var dateYears = dates.Where(date => date.Year == year).Select(date => date);
                        result.AddRange(dateYears);
                    }
                    var significantDates
                        = result.Select(dr => new SignificantDay { Date = dr, ObservedSignificantDayDate = dr, Name = centre });//result from dates
                    dateList.AddRange(significantDates);
                }
            }
            return dateList;
        }

        /// <summary>
        /// Signficant dates for the requested busuiness centres. This uses a query and will be slower than using specific years.
        /// </summary>
        /// <param name="businessCenters">The city names.</param>
        /// <returns></returns>
        public List<SignificantDay> GetSignificantDates(string[] businessCenters)
        {
            var dateList = new List<SignificantDay>();
            //The new filter with OR on arrays..
            var path = NameSpace + "." + BusinessCenterCalendarProp.GenericName;
            foreach (var centre in businessCenters)
            {
                var identifier = path + '.' + centre;
                var loadedObject = Cache.LoadObject<BusinessCenterCalendar>(identifier);
                if (loadedObject?.Holidays != null)
                {
                    var dates = loadedObject.Holidays.Select(dr => (DateTime)dr.Item);
                    // Get the dates as one list
                    var significantDates
                        = dates.Select(dr => new SignificantDay { Date = dr, ObservedSignificantDayDate = dr, Name = centre });
                    dateList.AddRange(significantDates);
                }
            }
            return dateList;
        }

        /// <summary>
        /// Signficants the date.
        /// </summary>
        /// <param name="years">The years to return.</param>
        /// <param name="businessCenter">The city name.</param>
        /// <returns></returns>
        public List<SignificantDay> GetSignificantDates(int[] years, BusinessCenterEnum businessCenter)//TODO FIX THIS!
        {
            var dateList = new List<SignificantDay>();
            var path = NameSpace + "." + BusinessCenterCalendarProp.GenericName;
            var identifier = path + '.' + businessCenter;
            var loadedObject = Cache.LoadObject<BusinessCenterCalendar>(identifier);
            if (loadedObject?.Holidays != null)
            {
                var dates = loadedObject.Holidays.Select(dr => (DateTime)dr.Item);
                // Filter the dates down to the required years.
                var result = new List<DateTime>();
                foreach(var year in years)
                {
                    var dateYears = dates.Where(date => date.Year == year).Select(date => date);
                    result.AddRange(dateYears);
                }
                var significantDates
                    = result.Select(dr => new SignificantDay { Date = dr, ObservedSignificantDayDate = dr, Name = businessCenter.ToString() });
                dateList.AddRange(significantDates);
            }
            return dateList;
        }

        /// <summary>
        /// Determines whether [is business day] [the specified calendars].
        /// </summary>
        /// <param name="calendars">The calendars.</param>
        /// <param name="date">The date.</param>
        /// <returns>
        /// 	<c>true</c> if [is business day] [the specified calendars]; otherwise, <c>false</c>.
        /// </returns>
        public Boolean IsBusinessDay(string[] calendars, DateTime date)
        {
            IBusinessCalendar calendar = GetCalendar(calendars);
            return calendar.IsBusinessDay(date);
        }

        /// <summary>
        /// Determines whether the specified date is holiday in the supplied calendar
        /// </summary>
        /// <param name="calendars">The calendars.</param>
        /// <param name="date">The date.</param>
        /// <returns>
        /// 	<c>true</c> if the specified calendars is holiday; otherwise, <c>false</c>.
        /// </returns>
        public Boolean IsHoliday(string[] calendars, DateTime date)
        {
            IBusinessCalendar calendar = GetCalendar(calendars);
            return calendar.IsHoliday(date);
        }

        /// <summary>
        /// Holidays between the supplied dates
        /// </summary>
        /// <param name="calendars">The calendars.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> HolidaysBetween(string[] calendars, DateTime startDate, DateTime endDate)
        {
            IBusinessCalendar calendar = GetCalendar(calendars);
            return calendar.HolidaysBetweenDates(startDate, endDate);
        }

        /// <summary>
        /// Businesses days between the supplied dates
        /// </summary>
        /// <param name="calendars">The calendars.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> BusinessDaysBetween(string[] calendars, DateTime startDate, DateTime endDate)
        {
            IBusinessCalendar calendar = GetCalendar(calendars);
            return calendar.BusinessDaysBetweenDates(startDate, endDate);
        }

        /// <summary>
        /// Advances the specified calendars.
        /// </summary>
        /// <param name="calendars">The calendars.</param>
        /// <param name="date">The date.</param>
        /// <param name="dayTypeString">The day type string.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns></returns>
        public DateTime Advance(string[] calendars, DateTime date, string dayTypeString, string periodInterval, string businessDayConvention)
        {
            IBusinessCalendar calendar = GetCalendar(calendars);
            Period interval = PeriodHelper.Parse(periodInterval);

            var dayType = DayTypeEnum.Calendar;
            if (dayTypeString.Length > 0)
                dayType = (DayTypeEnum)Enum.Parse(typeof(DayTypeEnum), dayTypeString, true);

            Offset offset = OffsetHelper.FromInterval(interval, dayType);
            BusinessDayConventionEnum dayConvention = BusinessDayConventionHelper.Parse(businessDayConvention);
            return calendar.Advance(date, offset, dayConvention);
        }

        /// <summary>
        /// Rolls the specified date using the underlying calendars.
        /// </summary>
        /// <param name="calendars">The calendars.</param>
        /// <param name="date">The date.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns></returns>
        public DateTime Roll(string[] calendars, DateTime date, string businessDayConvention)
        {
            IBusinessCalendar calendar = GetCalendar(calendars);
            BusinessDayConventionEnum dayConvention = BusinessDayConventionHelper.Parse(businessDayConvention);
            return calendar.Roll(date, dayConvention);
        }

        #endregion

        #region Last Trading Days

        /// <summary>
        /// Gets the last trading dates for the contract and year specified.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="months">The months.</param>
        /// <param name="years">The years.</param>
        /// <returns></returns>
        public static List<DateTime> GetLastTradingDays(string futuresCode, int[] months, int[] years)
        {
            var lastTradingDays = LastTradingDayHelper.GetLastTradingDays(futuresCode, months, years);
            return lastTradingDays;
        }

        /// <summary>
        /// Gets the last trading date for the contract specified.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="years">The years.</param>
        /// <param name="mainCycle">The mainCycle flag.</param>
        /// <returns></returns>
        public static List<DateTime> GetLastTradingDays(string futuresCode, int[] years, bool mainCycle)
        {
            var lastTradingDays = LastTradingDayHelper.GetLastTradingDays(futuresCode, years, mainCycle);
            return lastTradingDays;
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="referenceDate">The reference Date.</param>
        /// <param name="exchangeCommodityName">The exchange Commodity Name.</param>
        /// <returns></returns>
        public DateTime GetLastTradingDay(DateTime referenceDate, string exchangeCommodityName)
        {
            var lastTradingDay = LastTradingDayHelper.GetLastTradingDay(referenceDate, exchangeCommodityName);
            return lastTradingDay;
        }

        #endregion
        
        #region Central Bank Dates

        ///// <summary>
        ///// Signficants the date.
        ///// </summary>
        ///// <param name="years">The years to return.</param>
        ///// <param name="centralBank">The central Bank.</param>
        ///// <returns></returns>
        //private List<DateTime> GetCentralBankDates(int[] years, string centralBank)
        //{
        //    List<DateTime> dates = new List<DateTime>();
        //    foreach(var year in years)
        //    {
        //        var result = CentralBanksHelper.GetCBDates(new DateTime(year, 1, 1), centralBank, 12, new DateTime(year, 12, 31));
        //        dates.AddRange(result);
        //    }
        //    return dates;
        //}

        /// <summary>
        /// Creates the datatime vector for the request Central Bank.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="validCentralBank">The requested central bank.</param>
        /// <param name="centralBankDateRuleMonths">The rules.</param>
        /// <param name="lastDate">The last date required.</param>
        /// <returns>An aray of relevant dates.</returns>
        public DateTime[] GetCBDates(DateTime baseDate, string validCentralBank,
                                             int centralBankDateRuleMonths, DateTime lastDate)//TODO splice in the quarterly dates afterwards..
        {
            var result = CentralBanksHelper.GetCBDates(baseDate, validCentralBank, centralBankDateRuleMonths, lastDate);
            return result;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Arrays to sorted string list.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static string ArrayToSortedStringList(ICollection<string> items)
        {
            var itemsArray = new string[items.Count];
            var list = new List<string>(items);
            list.Sort();
            list.CopyTo(itemsArray, 0);
            return string.Join(",", itemsArray).ToLowerInvariant();
        }

        /// <summary>
        /// Dedupes the specified calendar names.
        /// </summary>
        /// <param name="calendarNames">The calendar names.</param>
        /// <returns></returns>
        public static string[] Dedupe(IEnumerable<string> calendarNames)
        {
            var names = new List<string>();
            foreach (string name in calendarNames)
            {
                if (!names.Contains(name))
                {
                    names.Add(name);
                }
            }
            return names.ToArray();
        }

        #endregion

        #region Schedulers

        /// <summary>
        /// Adjusteds the dates from termination date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="businessCenters">The business centers.</param>
        /// <param name="dateAdjustmentConvention">The date adjustment convention.</param>
        /// <returns></returns>
        public DateTime[] AdjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, BusinessCenters businessCenters, BusinessDayConventionEnum dateAdjustmentConvention)
        {
            DateTime firstRegularPeriodStartDate;
            DateTime lastRegularPeriodEndDate;
            DateTime[] dateSchedule = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);
            DateTime[] adjustedDateSchedule = AdjustDates(dateSchedule, businessCenters, dateAdjustmentConvention);
            return adjustedDateSchedule;
        }

        /// <summary>
        /// Adjusteds the dates from termination date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="businessCenters">The business centers.</param>
        /// <param name="dateAdjustmentConvention">The date adjustment convention.</param>
        /// <returns></returns>
        public DateTime[] AdjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, string businessCenters, string dateAdjustmentConvention)
        {
            DateTime firstRegularPeriodStartDate;
            DateTime lastRegularPeriodEndDate;
            DateTime[] dateSchedule = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);
            DateTime[] adjustedDateSchedule = AdjustDates(dateSchedule, businessCenters, dateAdjustmentConvention);
            return adjustedDateSchedule;
        }

        /// <summary>
        /// Adjusteds the dates from effective date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="businessCenters">The business centers.</param>
        /// <param name="dateAdjustmentConvention">The date adjustment convention.</param>
        /// <returns></returns>
        public DateTime[] AdjustedDatesFromEffectiveDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, string businessCenters, string dateAdjustmentConvention)
        {
            DateTime firstRegularPeriodStartDate;
            DateTime lastRegularPeriodEndDate;
            DateTime[] dateSchedule = DateScheduler.GetUnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);
            DateTime[] adjustedDateSchedule = AdjustDates(dateSchedule, businessCenters, dateAdjustmentConvention);
            return adjustedDateSchedule;
        }

        private DateTime[] AdjustDates(IEnumerable<DateTime> dateSchedule, string businessCenters, string dateAdjustmentConvention)
        {
            string businessCentersString = string.Join("-", businessCenters.Split(','));
            return dateSchedule.Select(t => DateTypesHelper.ToAdjustableDate(t, dateAdjustmentConvention, businessCentersString)).Select(ToAdjustedDate).ToArray();
        }

        private DateTime[] AdjustDates(IEnumerable<DateTime> dateSchedule, BusinessCenters businessCenters, BusinessDayConventionEnum dateAdjustmentConvention)
        {
            string businessCentersString = BusinessCentersHelper.BusinessCentersString(businessCenters.businessCenter);
            return dateSchedule.Select(t => DateTypesHelper.ToAdjustableDate(t, dateAdjustmentConvention, businessCentersString)).Select(ToAdjustedDate).ToArray();
        }

        /// <summary>
        /// Gets the adjusted date schedule.
        /// </summary>
        /// <param name="unadjustedDates">An unadjy=usted date list.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <returns></returns>
        public DateTime[] AdjustedDateSchedule(DateTime[] unadjustedDates, BusinessDayAdjustments businessDayAdjustments)
        {
            return unadjustedDates.Select(unadjustedDate => ToAdjustedDate(unadjustedDate, businessDayAdjustments)).ToArray();
        }

        /// <summary>
        /// Gets the adjusted calculation period start dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodDate">The first regular period date.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <param name="calendars">The holiday array.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>A vertival range of dates.</returns>
        public DateTime[] GetAdjustedCalculationPeriodDates(DateTime effectiveDate, DateTime terminationDate,
                                                                    string periodInterval, string rollConvention, DateTime firstRegularPeriodDate, string stubPeriodType,
                                                                    string[] calendars, string businessDayConvention)
        {
            const string dateToReturn = "unadjustedStartDate";
            StubPeriodTypeEnum? stubType = null;
            if (!string.IsNullOrEmpty(stubPeriodType))
                stubType = (StubPeriodTypeEnum)Enum.Parse(typeof(StubPeriodTypeEnum), stubPeriodType, true);
            var periods = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, firstRegularPeriodDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), stubType);
            var dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(periods, dateToReturn);
            var newDates = new DateTime[dates.Count];
            var index = 0;
            foreach (var date in dates)
            {
                var newDate = Advance(calendars, date, "Calendar", "0D", businessDayConvention);
                newDates[index] = newDate;
                index++;
            }
            var result = newDates;
            return result;
        }

        /// <summary>
        /// Gets the adjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns></returns>
        /// <param name="businessDayAdjustments">The necessary date adjustment details.</param>
        public List<CalculationPeriod> GetAdjustedCalculationDateSchedule(DateTime effectiveDate,
                                                                           Period intervalToTerminationDate, Period periodInterval, BusinessDayAdjustments businessDayAdjustments)
        {
            List<CalculationPeriod> adjustedDateScheduleList =
                new CalculationPeriodSchedule().GetUnadjustedCalculationDateSchedule(effectiveDate, intervalToTerminationDate, periodInterval);
            foreach (CalculationPeriod period in adjustedDateScheduleList)
            {
                period.adjustedStartDateSpecified = true;
                period.adjustedStartDate =
                    ToAdjustedDate(period.unadjustedStartDate, businessDayAdjustments);
                period.adjustedEndDateSpecified = true;
                period.adjustedEndDate =
                    ToAdjustedDate(period.unadjustedEndDate, businessDayAdjustments);
            }
            return adjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the adjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns></returns>
        /// <param name="businessDayAdjustments">The necessary date adjustment details.</param>
        /// <param name="discountingTypeEnum"></param>
        public List<PaymentCalculationPeriod> GetAdjustedPaymentCalculationDateSchedule(DateTime effectiveDate,
                                                                                        Period intervalToTerminationDate, Period periodInterval, BusinessDayAdjustments businessDayAdjustments,
                                                                                        DiscountingTypeEnum discountingTypeEnum)
        {
            var adjustedPaymentCalculationDateScheduleList =
                new List<PaymentCalculationPeriod>();
            List<CalculationPeriod> adjustedDateScheduleList = GetAdjustedCalculationDateSchedule(effectiveDate,
                                                                                                  intervalToTerminationDate,
                                                                                                  periodInterval,
                                                                                                  businessDayAdjustments);
            foreach (CalculationPeriod period in adjustedDateScheduleList)
            {
                var paymentCalculationPeriod = new PaymentCalculationPeriod
                {
                    adjustedPaymentDate =
                        discountingTypeEnum == DiscountingTypeEnum.Standard
                            ? period.adjustedEndDate
                            : period.adjustedStartDate,
                    adjustedPaymentDateSpecified = true
                };
                var calc = new CalculationPeriod[1];
                calc[0] = period;
                paymentCalculationPeriod.Items = calc;
                adjustedPaymentCalculationDateScheduleList.Add(paymentCalculationPeriod);
            }
            return adjustedPaymentCalculationDateScheduleList;
        }

        /// <summary>
        /// Gets the adjusted date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <returns></returns>
        public List<DateTime> GetAdjustedDateSchedule(DateTime effectiveDate, Period intervalToTerminationDate,
                                                             Period periodInterval, BusinessDayAdjustments businessDayAdjustments)
        {
            List<DateTime> unadjustedPeriodDates = DateScheduler.GetUnadjustedDateSchedule(effectiveDate, intervalToTerminationDate, periodInterval);

            IBusinessCalendar businessCalendar = ToBusinessCalendar(businessDayAdjustments.businessCenters);
            IEnumerable<DateTime> adjustedPeriodDates
                = unadjustedPeriodDates.Select(a => businessCalendar.Roll(a, businessDayAdjustments.businessDayConvention));
            return adjustedPeriodDates.Distinct().ToList();
        }

        #endregion

        #region MetaDateScheduler

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinitionRange">This must have 3 columns: interval, interval, rollconventionenum.</param>
        /// <param name="startDate"></param>
        /// <param name="calendar"></param>
        /// <param name="businessDayAdjustment"></param>
        /// <returns></returns>
        public DateTime[] GetMetaDatesSchedule(string[,] metaScheduleDefinitionRange,
                                                       DateTime startDate,
                                                       string calendar,
                                                       string businessDayAdjustment)
        {
            var metaScheduleDefinition = new List<ThreeStringsRangeItem>();
            var index = metaScheduleDefinitionRange.GetLowerBound(1);
            for (var i = metaScheduleDefinitionRange.GetLowerBound(0); i <= metaScheduleDefinitionRange.GetUpperBound(0); i++)
            {
                var output = new ThreeStringsRangeItem
                {
                    Value1 = metaScheduleDefinitionRange[i, index],
                    Value2 = metaScheduleDefinitionRange[i, index + 1],
                    Value3 = metaScheduleDefinitionRange[i, index + 2]
                };
                metaScheduleDefinition.Add(output);
            }
            var resultAsListOfDates = GetDatesSchedule(metaScheduleDefinition,
                                                       startDate,
                                                       calendar,
                                                       businessDayAdjustment);
            var result = resultAsListOfDates;

            return result;
        }

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinitionRange"></param>
        /// <param name="startDate"></param>
        /// <param name="calendar"></param>
        /// <param name="businessDayAdjustment"></param>
        /// <returns></returns>//TODO
        public DateTime[] GetDatesSchedule(List<ThreeStringsRangeItem> metaScheduleDefinitionRange,
                                                  DateTime startDate,
                                                  string calendar,
                                                  string businessDayAdjustment)
        {
            var metaScheduleDefinition = metaScheduleDefinitionRange.Select(item => new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse(item.Value1), PeriodHelper.Parse(item.Value2), RollConventionEnumHelper.Parse(item.Value3))).ToList();
            List<DateTime> resultAsListOfDates;
            if (String.IsNullOrEmpty(calendar) | String.IsNullOrEmpty(businessDayAdjustment))
            {
                resultAsListOfDates = DatesMetaSchedule.GetUnadjustedDates(metaScheduleDefinition, startDate);
            }
            else
            {
                var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayAdjustment, calendar);
                resultAsListOfDates = GetAdjustedDates(metaScheduleDefinition, startDate, businessDayAdjustments);
            }
            var result = resultAsListOfDates.ToArray();

            return result;
        } 

        ///<summary>
        /// Gets the adjusted dates from a provided date schedule.
        ///</summary>
        ///<param name="metaScheduleDefinition"></param>
        ///<param name="startDate"></param>
        ///<param name="businessDayAdjustments"></param>
        ///<returns></returns>
        public List<DateTime> GetAdjustedDates(List<Triplet<Period, Period, RollConventionEnum>> metaScheduleDefinition,
                                                        DateTime startDate, BusinessDayAdjustments businessDayAdjustments)
        {
            var unadjustedDates = DatesMetaSchedule.GetUnadjustedDates(metaScheduleDefinition, startDate);

            return unadjustedDates.Select(date => ToAdjustedDate(date, businessDayAdjustments)).ToList();
        }

        /// <summary>
        /// A simple date scheduler.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="interval"></param>
        /// <param name="rollConventionEnum"></param>
        /// <param name="backwardGeneration"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <returns></returns>
        public List<DateTime> GetAdjustedDates2(DateTime startDate, DateTime endDate,
                                                       Period interval, RollConventionEnum rollConventionEnum,
                                                       bool backwardGeneration, BusinessDayAdjustments businessDayAdjustments)
        {
            var unadjustedDates = DatesMetaSchedule.GetUnadjustedDates2(startDate, endDate, interval, rollConventionEnum, backwardGeneration);

            return unadjustedDates.Select(date => ToAdjustedDate(date, businessDayAdjustments)).ToList();
        }

        /// <summary>
        /// A slightly  ore complicated date scheduler.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="rollsMetaSchedule"></param>
        /// <param name="backwardGeneration"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <returns></returns>
        public List<DateTime> GetAdjustedDates3(DateTime startDate, DateTime endDate,
                                                       List<MetaScheduleItem> rollsMetaSchedule,
                                                       bool backwardGeneration, BusinessDayAdjustments businessDayAdjustments)
        {
            var unadjustedDates = DatesMetaSchedule.GetUnadjustedDates3(startDate, endDate, rollsMetaSchedule, backwardGeneration);

            return unadjustedDates.Select(date => ToAdjustedDate(date, businessDayAdjustments)).ToList();
        }

        #endregion

        #region Fixing Dates

        ///<summary>
        ///</summary>
        ///<param name="interestRateStream"></param>
        ///<param name="listAdjustedResetDates"></param>
        ///<returns></returns>
        ///<exception cref="NotSupportedException"></exception>
        public List<DateTime> GetAdjustedFixingDates(InterestRateStream interestRateStream, List<DateTime> listAdjustedResetDates)
        {
            var result = new List<DateTime>();
            RelativeDateOffset fixingDatesOffset = interestRateStream.resetDates.fixingDates;
            int numberOfDays = int.Parse(fixingDatesOffset.periodMultiplier);
            // Only NON-POSITIVE offset expressed in BUSINESS DAYS is supported.
            //
            if (!(fixingDatesOffset.dayType == DayTypeEnum.Business & fixingDatesOffset.period == PeriodEnum.D & numberOfDays <= 0))
            {
                string exceptionMessage =
                    $"[{fixingDatesOffset.dayType} {fixingDatesOffset.period} {numberOfDays} days] fixing day offset is not supported.";
                throw new NotSupportedException(exceptionMessage);
            }
            var businessDayAdjustments = new BusinessDayAdjustments
            {
                businessCenters = fixingDatesOffset.businessCenters,
                businessDayConvention =
                    fixingDatesOffset.businessDayConvention
            };
            IBusinessCalendar businessCalendar = ToBusinessCalendar(businessDayAdjustments.businessCenters);
            foreach (DateTime adjustredResetDate in listAdjustedResetDates)
            {
                int offsetInBusinessDays = numberOfDays;
                DateTime adjustedFixingDate = adjustredResetDate;
                while (offsetInBusinessDays++ < 0)
                {
                    // Adjust fixing date for one days back.
                    //
                    do
                    {
                        adjustedFixingDate = adjustedFixingDate.AddDays(-1);

                    } while (!businessCalendar.IsBusinessDay(adjustedFixingDate));
                    // adjustedFixingDate has been adjusted for 1 BUSINESS DAY
                }
                result.Add(adjustedFixingDate);
            }
            return result;
        }

        #endregion

        #region Reset Dates

        ///<summary>
        ///</summary>
        ///<param name="interestRateStream"></param>
        ///<param name="listCalculationPeriods"></param>
        ///<returns></returns>
        ///<exception cref="NotSupportedException"></exception>
        ///<exception cref="NotImplementedException"></exception>
        public List<DateTime> GetAdjustedResetDates(InterestRateStream interestRateStream, List<CalculationPeriod> listCalculationPeriods)
        {
            var adjustedResetDates = new List<DateTime>();
            ResetDates resetDates = interestRateStream.resetDates;
            Period resetFrequency = IntervalHelper.FromFrequency(resetDates.resetFrequency);
            Period calculationPeriodFrequency = IntervalHelper.FromFrequency(interestRateStream.calculationPeriodDates.calculationPeriodFrequency);
            if (resetFrequency.period != calculationPeriodFrequency.period)
            {
                throw new NotSupportedException(
                    $"Reset period type ({resetFrequency.period}) and calculation period type ({calculationPeriodFrequency.period}) are different. This is not supported.");
            }
            if (int.Parse(resetFrequency.periodMultiplier) != int.Parse(calculationPeriodFrequency.periodMultiplier))
            {
                throw new NotSupportedException(
                    $"Reset period frequency ({resetFrequency.period}) is not equal to calculation period frequency ({calculationPeriodFrequency.period}). This is not supported.");
            }
            BusinessDayAdjustments resetDatesAdjustments = resetDates.resetDatesAdjustments;
            ResetRelativeToEnum resetRelativeTo = resetDates.resetRelativeTo;
            foreach (CalculationPeriod calculationPeriodsInPamentPeriod in listCalculationPeriods)
            {
                switch (resetRelativeTo)
                {
                    case ResetRelativeToEnum.CalculationPeriodStartDate:
                        {
                            DateTime unadjustedResetDate = calculationPeriodsInPamentPeriod.unadjustedStartDate;
                            DateTime adjustedResetDate = ToAdjustedDate(unadjustedResetDate, resetDatesAdjustments);
                            adjustedResetDates.Add(adjustedResetDate);
                            break;
                        }
                    case ResetRelativeToEnum.CalculationPeriodEndDate:
                        {
                            DateTime unadjustedResetDate = calculationPeriodsInPamentPeriod.unadjustedEndDate;
                            DateTime adjustedResetDate = ToAdjustedDate(unadjustedResetDate, resetDatesAdjustments);
                            adjustedResetDates.Add(adjustedResetDate);
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException("resetRelativeTo");
                        }
                }
            }

            return adjustedResetDates;
        }

        #endregion

        #region Period Helpers

        /// <summary>
        /// Adds the periods.
        /// </summary>
        /// <param name="dates">The dates.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="calendars">The calendars.</param>
        /// <returns></returns>
        public List<DateTime> AddPeriods(DateTime[] dates, string dayType, string periodInterval, string businessDayConvention, string[] calendars)
        {
            string calendarsStr = string.Join("-", calendars);
            BusinessCenters businessCenters = BusinessCentersHelper.Parse(calendarsStr);
            IBusinessCalendar calendar = ToBusinessCalendar(businessCenters);
            return dates.Select(t => AddPeriod(t, periodInterval, calendar, businessDayConvention, dayType)).ToList();
        }

        /// <summary>
        /// Adds the periods.
        /// </summary>
        /// <param name="date">The base date.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodIntervals">The period intervals.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="calendars">The calendars.</param>
        /// <returns></returns>
        public List<DateTime> AddPeriods(DateTime date, string dayType, string[] periodIntervals, string businessDayConvention, string[] calendars)
        {
            string calendarsStr = string.Join("-", calendars);
            BusinessCenters businessCenters = BusinessCentersHelper.Parse(calendarsStr);
            IBusinessCalendar calendar = ToBusinessCalendar(businessCenters);
            return periodIntervals.Select(t => AddPeriod(date, t, calendar, businessDayConvention, dayType)).ToList();
        }

        /// <summary>
        /// Adds the period.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="tenorString">The tenor string.</param>
        /// <param name="calendarAsString">The calendar as string.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="dayType">Type of the day.</param>
        /// <returns></returns>
        public DateTime AddPeriod(DateTime startDate, string tenorString, string calendarAsString, string rollConvention, string dayType)
        {
            const string defaultCalendar = "Hell";
            const string defaultRollConvention = "FOLLOWING";
            if (String.IsNullOrEmpty(calendarAsString))
            {
                calendarAsString = defaultCalendar;
            }
            if (String.IsNullOrEmpty(rollConvention))
            {
                rollConvention = defaultRollConvention;
            }
            Period rollPeriod = PeriodHelper.Parse(tenorString);
            if (String.IsNullOrEmpty(dayType))
            {
                dayType = DayTypeStringFromRollPeriodInterval(rollPeriod);
            }
            var dayTypeEnum = (DayTypeEnum)Enum.Parse(typeof(DayTypeEnum), dayType, true);
            BusinessCenters businessCenters = BusinessCentersHelper.Parse(calendarAsString);
            IBusinessCalendar calendar = ToBusinessCalendar(businessCenters);
            BusinessDayConventionEnum businessDayConvention = BusinessDayConventionHelper.Parse(rollConvention);
            DateTime endDate = calendar.Advance(startDate, OffsetHelper.FromInterval(rollPeriod, dayTypeEnum), businessDayConvention);
            return endDate;
        }

        /// <summary>
        /// Adds the period.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="tenorString">The tenor string.</param>
        /// <param name="calendar">The calendar.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="dayType">Type of the day.</param>
        /// <returns></returns>
        private DateTime AddPeriod(DateTime startDate, string tenorString, IBusinessCalendar calendar, string rollConvention, string dayType)
        {
            const string defaultCalendar = "Hell";
            const string defaultRollConvention = "FOLLOWING";
            if (calendar==null)
            {
                BusinessCenters businessCenters = BusinessCentersHelper.Parse(defaultCalendar);
                calendar = ToBusinessCalendar(businessCenters);
            }
            if (String.IsNullOrEmpty(rollConvention))
            {
                rollConvention = defaultRollConvention;
            }
            Period rollPeriod = PeriodHelper.Parse(tenorString);
            if (String.IsNullOrEmpty(dayType))
            {
                dayType = DayTypeStringFromRollPeriodInterval(rollPeriod);
            }
            var dayTypeEnum = (DayTypeEnum)Enum.Parse(typeof(DayTypeEnum), dayType, true);
            BusinessDayConventionEnum businessDayConvention = BusinessDayConventionHelper.Parse(rollConvention);
            DateTime endDate = calendar.Advance(startDate, OffsetHelper.FromInterval(rollPeriod, dayTypeEnum), businessDayConvention);
            return endDate;
        }

        /// <summary>
        ///  Derives the Day type string from roll period.interval.
        /// </summary>
        /// <param name="rollPeriod">The roll period.</param>
        /// <returns></returns>
        private static string DayTypeStringFromRollPeriodInterval(Period rollPeriod)
        {
            string dayType = rollPeriod.period == PeriodEnum.D ? "Business" : "Calendar";
            return dayType;
        }

        #endregion
    }
}
