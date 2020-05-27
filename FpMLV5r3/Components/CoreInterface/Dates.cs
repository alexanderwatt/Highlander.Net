/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.CalendarEngine.V5r3.Dates;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.CalendarEngine.V5r3.Schedulers;
using Highlander.Codes.V5r3;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.Core.Interface.V5r3
{
    /// <summary>
    /// A general calendar class.
    /// </summary>
    public partial class PricingCache
    {
        #region Calendar Functions

        public string CreateCalendar(NamedValueSet properties, List<DateTime> dates)
        {
            var identifier = properties.GetValue<string>("UniqueIdentifier", true);
            var calendar = BusinessCalendarHelper.CreateCalendar(properties, dates);
            Engine.Cache.SaveObject(calendar, NameSpace + "." + identifier, properties);
            Engine.Logger.LogDebug("Loaded business center holiday dates: {0}", identifier);
            return identifier;
        }

        /// <summary>
        /// A function to return the list of valid FpML calendars.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedBusinessCenters()
        {
            var calendars = (from BusinessCenterEnum item in Enum.GetValues(typeof (BusinessCenterEnum)) select BusinessCenterScheme.GetEnumString(item) into bc where bc != null select bc).ToList();
            return calendars;
        }

        /// <summary>
        /// Calendars that are supported.
        /// </summary>
        /// <returns>A vertical range of calendars supported.</returns>
        public List<string> CalendarsSupported()
        {
            var calendarsList = CalendarService.CalendarsSupported();
            var uniqueValues = new List<string>();
            foreach (var obj in calendarsList)
            {
                if (!uniqueValues.Contains(obj))
                    uniqueValues.Add(obj);
            }
            return uniqueValues;
        }

        /// <summary>
        /// Determines whether [is valid calendar] [the specified rule profile].
        /// </summary>
        /// <param name="locations">The locations as a unique value list.</param>
        /// <returns></returns>
        public IDictionary<string, bool> IsValidCalendar(List<string> locations)
        {
            return CalendarService.IsValidCalendar(locations.ToArray());
        }

        /// <summary>
        /// Holidays between a period
        /// </summary>
        /// <param name="locations">The locations as a unique value list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> HolidaysBetween(List<string> locations, DateTime startDate, DateTime endDate)
        {
            //Get the holidays.
            return CalendarService.HolidaysBetween(locations.ToArray(), startDate, endDate);
        }

        /// <summary>
        /// Business days between a period.
        /// </summary>
        /// <param name="locations">The locations as a unique value list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> BusinessDaysBetween(List<string> locations, DateTime startDate, DateTime endDate)
        {
            return CalendarService.BusinessDaysBetween(locations.ToArray(), startDate, endDate);
        }

        /// <summary>
        /// Determines whether the specified date is a holiday.
        /// </summary>
        /// <param name="locations">The locations as a unique value list.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public bool IsHoliday(List<string> locations, DateTime date)
        {
            return CalendarService.IsHoliday(locations.ToArray(), date);
        }

        /// <summary>
        /// Determines whether the specified date is a business day.
        /// </summary>
        /// <param name="locations">The locations as a unique value list.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public bool IsBusinessDay(List<string> locations, DateTime date)
        {
            var result = CalendarService.IsBusinessDay(locations.ToArray(), date);
            return result;
        }

        /// <summary>
        /// Advances the specified date using the underlying calendar locations.
        /// </summary>
        /// <param name="locations">The locations as a unique value list.</param>
        /// <param name="date">The date.</param>
        /// <param name="dayType">The day type: calendar or business.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>The adjusted date.</returns>
        public DateTime Advance(List<string> locations, DateTime date, string dayType, string periodInterval, string businessDayConvention)
        {
            return CalendarService.Advance(locations.ToArray(), date, dayType, periodInterval, businessDayConvention);
        }

        /// <summary>
        /// Advances the specified date using the underlying calendar locations.
        /// </summary>
        /// <param name="locations">The locations as a unique value list.</param>
        /// <param name="dates">The dates.</param>
        /// <param name="dayType">The day type: calendar or business.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>The vertical range of adjusted dates.</returns>
        public List<DateTime> AdvanceDateRange(List<string> locations, List<DateTime> dates, string dayType, string periodInterval, string businessDayConvention)
        {
            return dates.Select(element => CalendarService.Advance(locations.ToArray(), element, dayType, periodInterval, businessDayConvention)).ToList();
        }

        /// <summary>
        /// Rolls the specified date using the underlying calendar locations.
        /// </summary>
        /// <param name="locations">The locations as a unique value list.</param>
        /// <param name="date">The date.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns></returns>
        public DateTime Roll(List<string> locations, DateTime date, string businessDayConvention)
        {
            return CalendarService.Roll(locations.ToArray(), date, businessDayConvention);
        }

        #endregion

        #region Central Bank Date Functions

        /// <summary>
        /// A function to return the list of valid indices, which may or may not have been implemented.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedCentralBanks()
        {
            return Enum.GetNames(typeof(CentralBanks)).ToList();
        }

        /// <summary>
        /// Creates the dataTime vector for the request Central Bank.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="validCentralBank">The requested central bank.</param>
        /// <param name="centralBankDateRuleMonths">The rules.</param>
        /// <param name="lastDate">The last date required.</param>
        /// <returns>A list of relevant dates.</returns>
        public List<DateTime> GetCentralBankDates(DateTime baseDate, string validCentralBank,
                                                      int centralBankDateRuleMonths, DateTime lastDate)
        {
            return CentralBanksHelper.GetCBDates(baseDate, validCentralBank, centralBankDateRuleMonths, lastDate).ToList();
        }

        #endregion

        #region Futures Dates

        /// <summary>
        /// returns the IMM code for next contract listed in the
        /// relevant Exchange.
        /// </summary>
        /// <param name="exchangeCommodityName">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="date">The date.</param>
        /// <param name="mainCycle">Is the contract a main cycle type?</param>
        /// <returns>The futures code string</returns>
        public string GetNextFuturesCode(string exchangeCommodityName, DateTime date, bool mainCycle)
        {
            return LastTradingDayHelper.GetNextFuturesCode(exchangeCommodityName, date, mainCycle);
        }

        /// <summary>
        /// next IMM date following the given date
        /// returns the 1st delivery date for next contract listed in the
        /// International Money Market section of the Chicago Mercantile
        /// Exchange.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity.</param>
        /// <param name="refDate">THe reference date to use.</param>
        /// <param name="mainCycle">Is the contract a main cycle type.</param>
        /// <returns></returns>
        public DateTime GetNextLastTradingDate(string exchangeCommodityName, DateTime refDate, bool mainCycle)
        {
            return LastTradingDayHelper.GetNextLastTradingDate(exchangeCommodityName, refDate, mainCycle);
        }

        /// <summary>
        /// whether or not the given date is an IMM date
        /// </summary>
        /// <param name="exchangeCommodityName">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="date">THe date to verify.</param>
        /// <param name="mainCycle">Is the contract a main cycle type?</param>
        /// <returns>true/false</returns>
        public bool IsLastTradingDate(string exchangeCommodityName, DateTime date, bool mainCycle)
        {
            return LastTradingDayHelper.IsLastTradingDate(exchangeCommodityName, date, mainCycle);
        }

        /// <summary>
        /// Gets the Last trading based on a given month.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity.</param>
        /// <param name="months">The list of months.</param>
        /// <param name="years">The list of years as integers.</param>
        /// <returns></returns>
        public List<DateTime> LastTradingDayByMonth(string exchangeCommodityName, List<int> months, List<int> years)
        {
            return LastTradingDayHelper.GetLastTradingDays(exchangeCommodityName, months.ToArray(), years.ToArray());
        }

        /// <summary>
        /// Gets the Last trading day by year. This takes an array of years.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity.</param>
        /// <param name="years">The list of years as integers.</param>
        /// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        /// <returns>A range of dates.</returns>
        public List<DateTime> LastTradingDayByYear(string exchangeCommodityName, List<int> years, bool mainCycle)
        {
            return LastTradingDayHelper.GetLastTradingDays(exchangeCommodityName, years.ToArray(), mainCycle);
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="absoluteImmCode">The absolute IMM code eg EDZ8.</param>
        /// <returns></returns>
        public DateTime GetLastFuturesTradingDay(DateTime referenceDate, string absoluteImmCode)
        {
            var lastTradingDay = CalendarService.GetLastTradingDay(referenceDate, absoluteImmCode);
            return lastTradingDay;
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="absoluteImmCode">The absolute IMM code.</param>
        /// <param name="decadeStartYear">The decade start year, in the form 2010</param>
        /// <returns></returns>
        public DateTime GetLastTradingDayWithDecade(string absoluteImmCode, int decadeStartYear)
        {
            var baseDate = new DateTime(decadeStartYear, 1, 1);
            var lastTradingDay = CalendarService.GetLastTradingDay(baseDate, absoluteImmCode);
            return lastTradingDay;
        }

        #endregion

        #region Periods and Year Fractions

        /// <summary>
        /// Calculates the number of days between the two dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end Date.</param>
        /// <param name="dayCounter">THe dayCounter.</param>
        /// <returns></returns>
        public int AccrualDays(DateTime startDate, DateTime endDate, string dayCounter)
        {
            IDayCounter dc = DayCounterHelper.Parse(dayCounter);
            int days = dc.DayCount(startDate, endDate);
            return days;
        }

        /// <summary>
        /// Adds the period to an array of dates.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="calendar">The calendar.</param>
        /// <returns>A range of dates.</returns>
        public DateTime AddPeriod(DateTime date, string periodInterval, string calendar, string businessDayConvention, string dayType)
        {
            var period = CalendarService.AddPeriod(date, periodInterval, calendar, businessDayConvention, dayType);
            return period;
        }

        /// <summary>
        /// Adds the period to an array of dates.
        /// </summary>
        /// <param name="dates">The dates as a list.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="location">The calendar. </param>
        /// <returns>A range of dates.</returns>
        public List<DateTime> AddPeriods1(List<DateTime> dates, string dayType, string periodInterval, string businessDayConvention, string location)
        {
            return CalendarService.AddPeriods(dates.ToArray(), dayType, periodInterval, businessDayConvention, new[] { location });
        }

        /// <summary>
        /// Adds the period to an array of dates.
        /// </summary>
        /// <param name="dates">The dates as a list.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="locations">The calendars.</param>
        /// <returns>A range of dates.</returns>
        public List<DateTime> AddPeriods(List<DateTime> dates, string dayType, string periodInterval, string businessDayConvention, List<string> locations)
        {
            return CalendarService.AddPeriods(dates.ToArray(), dayType, periodInterval, businessDayConvention, locations.ToArray()); ;
        }

        /// <summary>
        /// Adds the period to an array of dates.
        /// </summary>
        /// <param name="baseDate">The base dates.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periods">The period interval list.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="location">The calendar. </param>
        /// <returns>A range of dates.</returns>
        public List<DateTime> AddManyPeriods(DateTime baseDate, string dayType, List<string> periods, string businessDayConvention, string location)
        {
            return CalendarService.AddPeriods(baseDate, dayType, periods.ToArray(), businessDayConvention, new[] { location });
        }

        /// <summary>
        /// Adds the period to an array of dates.
        /// </summary>
        /// <param name="baseDate">The base dates.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periods">The period interval list.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="locations">The calendars.</param>
        /// <returns>A range of dates.</returns>
        public List<DateTime> AddManyPeriods2(DateTime baseDate, string dayType, List<string> periods, string businessDayConvention, List<string> locations)
        {
            return CalendarService.AddPeriods(baseDate, dayType, periods.ToArray(), businessDayConvention, locations.ToArray());
        }

        /// <summary>
        /// Returns the calculated year fraction for the dates provided.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="dayCountType">The day count fraction type, eg Actual/365.</param>
        /// <returns>The year fraction as a decimal.</returns>
        public double YearFraction(DateTime startDate, DateTime endDate, string dayCountType)
        {
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountType);
            return dayCounter.YearFraction(startDate, endDate);
        }

        /// <summary>
        /// Returns the calculated year fraction for the dates provided.
        /// </summary>
        /// <param name="startDates">The start date list.</param>
        /// <param name="endDates">The end date list.</param>
        /// <param name="dayCountType">The day count fraction type, eg Actual/365.</param>
        /// <returns>The year fractions as a range of decimals.</returns>
        public List<double> YearFractions(List<DateTime> startDates, List<DateTime> endDates, string dayCountType)
        {
            int count = startDates.Count;
            if (count != endDates.Count)
            {
                throw new ArgumentException("Size of startDates and endDates arrays must match");
            }
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountType);
            var yearFractions = new List<double>();
            for (int i = 0; i < count; i++)
            {
                yearFractions.Add(dayCounter.YearFraction(startDates[i], endDates[i]));
            }
            return yearFractions;
        }

        /// <summary>
        /// The supported day counters as a list.
        /// </summary>
        /// <returns></returns>
        public List<string> SupportedDayCounters()
        {
            return Enum.GetValues(typeof (DayCountFractionEnum)).Cast<DayCountFractionEnum>().Select(DayCountFractionScheme.GetEnumString).Where(index => index != null).ToList();
        }

        #endregion

        #region Schedulers

        /// <summary>
        /// Generates a spread schedule.
        /// </summary>
        /// <param name="initialValue">The initial value of the schedule to be generated.</param>
        /// <param name="step">The step value.</param>
        /// <param name="applyStepToEachNthCashflow">The frequency to apply the step.</param>
        /// <param name="totalNumberOfCashflows">The number of cashflows in the array.</param>
        /// <returns>A vertical array of spreads.</returns>
        public List<double> GetSpreadSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            return SpreadScheduleGenerator.GetSpreadSchedule(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows).ToList();
        }

        /// <summary>
        /// Generates a strike schedule.
        /// </summary>
        /// <param name="initialValue">The initial value of the schedule to be generated.</param>
        /// <param name="step">The step value.</param>
        /// <param name="applyStepToEachNthCashflow">The frequency to apply the step.</param>
        /// <param name="totalNumberOfCashflows">The number of cashflows in the array.</param>
        /// <returns>A vertical array of strikes.</returns>
        public List<double> GetStrikeSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            return StrikeScheduleGenerator.GetStrikeSchedule(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows).ToList();
        }

        /// <summary>
        /// Generates a notional schedule.
        /// </summary>
        /// <param name="initialValue">The initial value of the schedule to be generated.</param>
        /// <param name="step">The step value.</param>
        /// <param name="applyStepToEachNthCashflow">The frequency to apply the step.</param>
        /// <param name="totalNumberOfCashflows">The number of cashflows in the array.</param>
        /// <returns>A vertical list of notional values.</returns>
        public List<double> GetNotionalSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            return NotionalScheduleGenerator.GetNotionalSchedule(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows).ToList(); ;
        }

        /// <summary>
        /// Gets the unadjusted dates from termination date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodStartDate">The first regular period start date.</param>
        /// <param name="lastRegularPeriodEndDate">The last regular period end date.</param>
        /// <returns>A vertical array of dates.</returns>
        public List<DateTime> UnadjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, 
            string rollConvention, out DateTime firstRegularPeriodStartDate, out DateTime lastRegularPeriodEndDate)
        {
            return DateScheduler.UnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollConvention, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate).ToList();
        }

        /// <summary>
        /// Adjusted dates from effective date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="businessCenters">The business centers.</param>
        /// <param name="dateAdjustmentConvention">The date adjustment convention.</param>
        /// <returns>A vertical array of dates.</returns>
        public List<DateTime> AdjustedDatesFromEffectiveDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, 
            string businessCenters, string dateAdjustmentConvention)
        {
            var centers = BusinessCentersHelper.Parse(businessCenters);
            var calendar = Engine.ToBusinessCalendar(centers);
            var adjustments = EnumHelper.Parse<BusinessDayConventionEnum>(dateAdjustmentConvention);
            return AdjustedDateScheduler.AdjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollConvention, calendar, adjustments).ToList();
        }

        /// <summary>
        /// Adjusted dates from termination date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="businessCenters">The business centers.</param>
        /// <param name="dateAdjustmentConvention">The date adjustment convention.</param>
        /// <returns>A vertical array of dates.</returns>
        public List<DateTime> AdjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, 
            string rollConvention, string businessCenters, string dateAdjustmentConvention)
        {
            var centers = BusinessCentersHelper.Parse(businessCenters);
            var calendar = Engine.ToBusinessCalendar(centers);
            var adjustments = EnumHelper.Parse<BusinessDayConventionEnum>(dateAdjustmentConvention);
            return AdjustedDateScheduler.AdjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval,
                rollConvention, calendar, adjustments).ToList();
        }

        /// <summary>
        /// Gets the unadjusted dates from effective date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodStartDate">The first regular period start date.</param>
        /// <param name="lastRegularPeriodEndDate">The last regular period end date.</param>
        /// <returns>A vertical array of dates.</returns>
        public List<DateTime> UnadjustedDatesFromEffectiveDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, out DateTime firstRegularPeriodStartDate, out DateTime lastRegularPeriodEndDate)
        {
            return DateScheduler.UnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollConvention, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate).ToList();
        }

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinition">This must have 3 columns: interval, interval, rollConventionEnum.</param>
        /// <param name="startDate">The start date of the schedule to be generated.</param>
        /// <param name="calendar">The relevant calendar.</param>
        /// <param name="businessDayAdjustment">The business day adjustments.</param>
        /// <returns>A vertical array of dates.</returns>
        public List<DateTime> GetMetaDatesSchedule(List<ThreeStringsRangeItem> metaScheduleDefinition,
                                                       DateTime startDate,
                                                       string calendar,
                                                       string businessDayAdjustment)
        {
            var centers = BusinessCentersHelper.Parse(calendar);
            var businessCalendar = Engine.ToBusinessCalendar(centers);
            return AdjustedDatesMetaSchedule.GetMetaDatesSchedule(metaScheduleDefinition, startDate, businessCalendar, calendar, businessDayAdjustment).ToList();
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
        /// <returns>A vertical range of dates.</returns>
        public List<DateTime> GetAdjustedCalculationPeriodDates(DateTime effectiveDate, DateTime terminationDate,
                                                                    string periodInterval, string rollConvention, DateTime firstRegularPeriodDate, string stubPeriodType,
                                                                    List<string> calendars, string businessDayConvention)
        {
            var businessCalendar = CalendarService.GetCalendar(calendars.ToArray());
            return AdjustedDateScheduler.GetAdjustedCalculationPeriodDates(effectiveDate, terminationDate,
                                                                     periodInterval, rollConvention, firstRegularPeriodDate, stubPeriodType,
                                                                     businessCalendar, businessDayConvention).ToList();
        }

        /// <summary>
        /// Gets the unadjusted calculation period start dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodDate">The first regular period date.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns>A vertical range of dates.</returns>
        public List<DateTime> GetUnadjustedCalculationPeriodDates(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, 
            DateTime firstRegularPeriodDate, string stubPeriodType)
        {
            return DateScheduler.GetUnadjustedCalculationPeriodDates(effectiveDate, terminationDate,
                                                                              periodInterval, rollConvention,
                                                                              firstRegularPeriodDate, stubPeriodType).ToList();
        }

        /// <summary>
        /// Returns the unadjusted the calculation dates
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="intervalToFirstRegularPeriodStart">The interval to first regular period start.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns>A vertical array of dates.</returns>
        public List<DateTime> UnadjustedCalculationDatesFromFirstRegularInterval(DateTime effectiveDate, DateTime terminationDate, 
            string intervalToFirstRegularPeriodStart, string periodInterval, string rollConvention, string stubPeriodType)
        {
            return DateScheduler.UnadjustedCalculationDatesFromFirstRegularInterval(effectiveDate,
                                                                                             terminationDate,
                                                                                             intervalToFirstRegularPeriodStart,
                                                                                             periodInterval,
                                                                                             rollConvention,
                                                                                             stubPeriodType).ToList();
        }

        #endregion
    }
}