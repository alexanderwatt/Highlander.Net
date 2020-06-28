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
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using HLV5r3.Helpers;
using Excel = Microsoft.Office.Interop.Excel;

#endregion

namespace HLV5r3.Financial
{
    /// <summary>
    /// A general calendar class.
    /// </summary>
    //[ClassInterface(ClassInterfaceType.AutoDual)]
    public partial class Cache
    {
        #region Calendar Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertiesRange"></param>
        /// <param name="dateArray"></param>
        /// <returns></returns>
        public string CreateCalendar(Excel.Range propertiesRange, Excel.Range dateArray)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var dates = DataRangeHelper.StripDateTimeRange(dateArray);
            var identifier = namedValueSet.GetValue<string>("UniqueIdentifier", true);
            var calendar = BusinessCalendarHelper.CreateCalendar(namedValueSet, dates);
            Engine.Cache.SaveObject(calendar, NameSpace + "." + identifier, namedValueSet);
            Engine.Logger.LogDebug("Loaded business center holiday dates: {0}", identifier);
            return identifier;
        }

        /// <summary>
        /// A function to return the list of valid FpML calendars.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedBusinessCenters()
        {
            var cals = (from BusinessCenterEnum item in Enum.GetValues(typeof (BusinessCenterEnum)) select BusinessCenterScheme.GetEnumString(item) into bc where bc != null select bc).ToList();
            //var names = Enum.GetNames(typeof(BusinessCenterEnum));
            var result = RangeHelper.ConvertArrayToRange(cals);
            return result;
        }

        /// <summary>
        /// Calendars that are supported.
        /// </summary>
        /// <returns>A vertical range of calendars supported.</returns>
        public object[,] CalendarsSupported()
        {
            var calsList = CalendarService.CalendarsSupported();
            var unqVals = new List<object>();
            foreach (var obj in calsList)
            {
                if (!unqVals.Contains(obj))
                    unqVals.Add(obj);
            }
            var resVals = new object[unqVals.Count, 1];
            for (int idx = 0; idx < resVals.Length; ++idx)
                resVals[idx, 0] = unqVals[idx];
            return resVals;
        }

        /// <summary>
        /// Determines whether [is valid calendar] [the specified rule profile].
        /// </summary>
        /// <param name="locationsAsArray">The locations as an array.</param>
        /// <returns></returns>
        public object[,] IsValidCalendar(Excel.Range locationsAsArray)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            IDictionary<string, Boolean> calendars = CalendarService.IsValidCalendar(unqVals.ToArray());
            var collectionResult = calendars.Values;
            var resVals = new object[collectionResult.Count, 1];
            var idx = 0;
            foreach (var result in collectionResult)
            {
                resVals[idx, 0] = result;
                idx++;
            }
            return resVals;
        }

        /// <summary>
        /// Holidays between a period
        /// </summary>
        /// <param name="locationsAsArray">The locations range as an array.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public object[,] HolidaysBetween(Excel.Range locationsAsArray, DateTime startDate, DateTime endDate)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            //Get the holidays.
            var list = CalendarService.HolidaysBetween(unqVals.ToArray(), startDate, endDate);
            var resVals = new object[list.Count, 1];
            for (int idx = 0; idx < resVals.Length; ++idx)
                resVals[idx, 0] = list[idx];
            return resVals;
        }

        /// <summary>
        /// Business days between a period.
        /// </summary>
        /// <param name="locationsAsArray">The locations range as an array.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public object[,] BusinessDaysBetween(Excel.Range locationsAsArray, DateTime startDate, DateTime endDate)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            var list = CalendarService.BusinessDaysBetween(unqVals.ToArray(), startDate, endDate);
            var resVals = new object[list.Count, 1];
            for (int idx = 0; idx < resVals.Length; ++idx)
                resVals[idx, 0] = list[idx];
            return resVals;
        }

        /// <summary>
        /// Determines whether the specified date is a holiday.
        /// </summary>
        /// <param name="locationsAsArray">The locations range as an array.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public Boolean IsHoliday(Excel.Range locationsAsArray, DateTime date)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            var result = CalendarService.IsHoliday(unqVals.ToArray(), date);
            return result;
        }

        /// <summary>
        /// Determines whether the specified date is a business day.
        /// </summary>
        /// <param name="locationsAsArray">The locations range as an array.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public Boolean IsBusinessDay(Excel.Range locationsAsArray, DateTime date)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            var result = CalendarService.IsBusinessDay(unqVals.ToArray(), date);
            return result;
        }

        /// <summary>
        /// Advances the specified date using the underlying calendar locations.
        /// </summary>
        /// <param name="locationsAsArray">The locations range as an array.</param>
        /// <param name="date">The date.</param>
        /// <param name="dayType">The day type: calendar or business.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>The adjusted date.</returns>
        public DateTime Advance(Excel.Range locationsAsArray, DateTime date, string dayType, string periodInterval, string businessDayConvention)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            var result = CalendarService.Advance(unqVals.ToArray(), date, dayType, periodInterval, businessDayConvention);
            return result;
        }

        /// <summary>
        /// Advances the specified date using the underlying calendar locations.
        /// </summary>
        /// <param name="locationsAsArray">The locations range as an array.</param>
        /// <param name="dateRange">The date.</param>
        /// <param name="dayType">The day type: calendar or business.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>The vertical range of adjusted dates.</returns>
        public object[,] AdvanceDateRange(Excel.Range locationsAsArray, Excel.Range dateRange, string dayType, string periodInterval, string businessDayConvention)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            List<DateTime> unqDates = DataRangeHelper.StripDateTimeRange(dateRange);
            List<DateTime> result = unqDates.Select(element => CalendarService.Advance(unqVals.ToArray(), element, dayType, periodInterval, businessDayConvention)).ToList();
            var resVals = new object[result.Count, 1];
            for (int idx = 0; idx < resVals.Length; ++idx)
                resVals[idx, 0] = result[idx];
            return resVals;
        }
 
        /// <summary>
        /// Rolls the specified date using the underlying calendar locations.
        /// </summary>
        /// <param name="locationsAsArray">The locations range as an array.</param>
        /// <param name="date">The date.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns></returns>
        public DateTime Roll(Excel.Range locationsAsArray, DateTime date, string businessDayConvention)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            return CalendarService.Roll(unqVals.ToArray(), date, businessDayConvention);
        }

        #endregion

        #region Central Bank Date Functions

        /// <summary>
        /// A function to return the list of valid indices, which may or may not have been implemented.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedCentralBanks()
        {
            //var names = Enum.GetNames(typeof(FloatingRateIndexEnum));
            var indices = Enum.GetNames(typeof(CentralBanks));
            var result = RangeHelper.ConvertArrayToRange(indices);
            return result;
        }

        /// <summary>
        /// Creates the datatime vector for the request Central Bank.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="validCentralBank">The requested central bank.</param>
        /// <param name="centralBankDateRuleMonths">The rules.</param>
        /// <param name="lastDate">The last date required.</param>
        /// <returns>An aray of relevant dates.</returns>
        public object[,] GetCentralBankDates(DateTime baseDate, string validCentralBank,
                                                      int centralBankDateRuleMonths, DateTime lastDate)
        {
            var result = CentralBanksHelper.GetCBDates(baseDate, validCentralBank, centralBankDateRuleMonths, lastDate);
            var resVals = RangeHelper.ConvertArrayToRange(result);
            return resVals;
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
            string result = LastTradingDayHelper.GetNextFuturesCode(exchangeCommodityName, date, mainCycle);
            return result;
        }

        /// <summary>
        /// next IMM date following the given date
        /// returns the 1st delivery date for next contract listed in the
        /// International Money Market section of the Chicago Mercantile
        /// Exchange.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity.</param>
        /// <param name="refDate">THe refernce date to use.</param>
        /// <param name="mainCycle">Is the contract a main cycle type.</param>
        /// <returns></returns>
        public DateTime GetNextLastTradingDate(string exchangeCommodityName, DateTime refDate, bool mainCycle)
        {
            DateTime date = LastTradingDayHelper.GetNextLastTradingDate(exchangeCommodityName, refDate, mainCycle);
            return date;
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
            bool result = LastTradingDayHelper.IsLastTradingDate(exchangeCommodityName, date, mainCycle);
            return result;
        }

        /// <summary>
        /// Gets the Last trading based on a given month.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity.</param>
        /// <param name="monthsArray">The array of months.</param>
        /// <param name="yearsArray">The array of years as integers.</param>
        /// <returns></returns>
        public object[,] LastTradingDayByMonth(string exchangeCommodityName, Excel.Range monthsArray, Excel.Range yearsArray)
        {
            List<int> unqMonths = DataRangeHelper.StripIntRange(monthsArray);
            List<int> unqYears = DataRangeHelper.StripIntRange(yearsArray);
            List<DateTime> dates = LastTradingDayHelper.GetLastTradingDays(exchangeCommodityName, unqMonths.ToArray(), unqYears.ToArray());
            var resVals = RangeHelper.ConvertArrayToRange(dates);
            return resVals;
        }

        /// <summary>
        /// Gets the Last trading day by year. This takes an array of years.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity.</param>
        /// <param name="yearsArray">The array of years as integers.</param>
        /// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        /// <returns>A range of dates.</returns>
        public object[,] LastTradingDayByYear(string exchangeCommodityName, Excel.Range yearsArray, Boolean mainCycle)
        {
            List<int> unqVals = DataRangeHelper.StripIntRange(yearsArray);
            var dates = LastTradingDayHelper.GetLastTradingDays(exchangeCommodityName, unqVals.ToArray(), mainCycle);
            var resVals = RangeHelper.ConvertArrayToRange(dates);
            return resVals;
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="absoluteIMMCode">The absolute IMM code eg EDZ8.</param>
        /// <returns></returns>
        public DateTime GetLastFuturesTradingDay(DateTime referenceDate, string absoluteIMMCode)
        {
            var lastTradingDay = CalendarService.GetLastTradingDay(referenceDate, absoluteIMMCode);
            return lastTradingDay;
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="absoluteIMMCode">The absolute IMM code.</param>
        /// <param name="decadeStartYear">The decade start year, in the form 2010</param>
        /// <returns></returns>
        public DateTime GetLastTradingDayWithDecade(string absoluteIMMCode, int decadeStartYear)
        {
            var baseDate = new DateTime(decadeStartYear, 1, 1);
            var lastTradingDay = CalendarService.GetLastTradingDay(baseDate, absoluteIMMCode);
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
        public int AcccrualDays(DateTime startDate, DateTime endDate, string dayCounter)
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
        /// <param name="dateArray">The dates as an array.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="location">The calendar. </param>
        /// <returns>A range of dates.</returns>
        public object[,] AddPeriods1(Excel.Range dateArray, string dayType, string periodInterval, string businessDayConvention, string location)
        {
            List<DateTime> dateVals = DataRangeHelper.StripDateTimeRange(dateArray);
            List<DateTime> periods = CalendarService.AddPeriods(dateVals.ToArray(), dayType, periodInterval, businessDayConvention, new[] { location });
            var resVals = RangeHelper.ConvertArrayToRange(periods);
            return resVals;
        }

        /// <summary>
        /// Adds the period to an array of dates.
        /// </summary>
        /// <param name="dateArray">The dates as an array.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="locationsAsArray">The calendars.</param>
        /// <returns>A range of dates.</returns>
        public object[,] AddPeriods(Excel.Range dateArray, string dayType, string periodInterval, string businessDayConvention, Excel.Range locationsAsArray)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            List<DateTime> dateVals = DataRangeHelper.StripDateTimeRange(dateArray);
            List<DateTime> periods = CalendarService.AddPeriods(dateVals.ToArray(), dayType, periodInterval, businessDayConvention, unqVals.ToArray());
            var resVals = RangeHelper.ConvertArrayToRange(periods);
            return resVals;
        }

        /// <summary>
        /// Adds the period to an array of dates.
        /// </summary>
        /// <param name="baseDate">The base dates.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodArray">The period interval array.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="location">The calendar. </param>
        /// <returns>A range of dates.</returns>
        public object[,] AddManyPeriods(DateTime baseDate, string dayType, Excel.Range periodArray, string businessDayConvention, string location)
        {
            List<string> periodVals = DataRangeHelper.StripRange(periodArray);
            List<DateTime> periods = CalendarService.AddPeriods(baseDate, dayType, periodVals.ToArray(), businessDayConvention, new[] { location });
            var resVals = RangeHelper.ConvertArrayToRange(periods);
            return resVals;
        }

        /// <summary>
        /// Adds the period to an array of dates.
        /// </summary>
        /// <param name="baseDate">The base dates.</param>
        /// <param name="dayType">The day type.</param>
        /// <param name="periodArray">The period interval array.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="locationsAsArray">The calendars.</param>
        /// <returns>A range of dates.</returns>
        public object[,] AddManyPeriods2(DateTime baseDate, string dayType, Excel.Range periodArray, string businessDayConvention, Excel.Range locationsAsArray)
        {
            List<string> unqVals = DataRangeHelper.StripRange(locationsAsArray);
            List<string> periodVals = DataRangeHelper.StripRange(periodArray);
            List<DateTime> periods = CalendarService.AddPeriods(baseDate, dayType, periodVals.ToArray(), businessDayConvention, unqVals.ToArray());
            var resVals = RangeHelper.ConvertArrayToRange(periods);
            return resVals;
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
        /// <param name="startDateArray">The start date array.</param>
        /// <param name="endDateArray">The end date arrray.</param>
        /// <param name="dayCountType">The day count fraction type, eg Actual/365.</param>
        /// <returns>The year fractions as a range of decimals.</returns>
        public object[,] YearFractions(Excel.Range startDateArray, Excel.Range endDateArray, string dayCountType)
        {
            var unqStartDateVals = DataRangeHelper.StripDateTimeRange(startDateArray);
            var unqEndDateVals = DataRangeHelper.StripDateTimeRange(endDateArray);
            int count = unqStartDateVals.Count;
            if (count != unqEndDateVals.Count)
            {
                throw new ArgumentException("Size of startDates and endDates arrays must match");
            }
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountType);
            var yearFractions = new List<double>();
            for (int i = 0; i < count; i++)
            {
                yearFractions.Add(dayCounter.YearFraction(unqStartDateVals[i], unqEndDateVals[i]));
            }
            return RangeHelper.ConvertArrayToRange(yearFractions);
        }

        /// <summary>
        /// The supported day counters as a list.
        /// </summary>
        /// <returns></returns>
        public object[,] SupportedDayCounters()
        {
            var indices = Enum.GetValues(typeof (DayCountFractionEnum)).Cast<DayCountFractionEnum>().Select(DayCountFractionScheme.GetEnumString).Where(index => index != null).ToList();
            var result = RangeHelper.ConvertArrayToRange(indices);
            return result;
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
        public object[,] GetSpreadSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            var spreadSchedule = SpreadScheduleGenerator.GetSpreadSchedule(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows);
            var result = RangeHelper.ConvertArrayToRange(spreadSchedule);
            return result;
        }

        /// <summary>
        /// Generates a strike schedule.
        /// </summary>
        /// <param name="initialValue">The initial value of the schedule to be generated.</param>
        /// <param name="step">The step value.</param>
        /// <param name="applyStepToEachNthCashflow">The frequency to apply the step.</param>
        /// <param name="totalNumberOfCashflows">The number of cashflows in the array.</param>
        /// <returns>A vertical array of strikes.</returns>
        public object[,] GetStrikeSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            var strikeSchedule = StrikeScheduleGenerator.GetStrikeSchedule(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows);
            var result = RangeHelper.ConvertArrayToRange(strikeSchedule);
            return result;
        }

        /// <summary>
        /// Generates a notional schedule.
        /// </summary>
        /// <param name="initialValue">The initial value of the schedule to be generated.</param>
        /// <param name="step">The step value.</param>
        /// <param name="applyStepToEachNthCashflow">The frequency to apply the step.</param>
        /// <param name="totalNumberOfCashflows">The number of cashflows in the array.</param>
        /// <returns>A vertical array of notionals.</returns>
        public object[,] GetNotionalSchedule(double initialValue, double step, int applyStepToEachNthCashflow, int totalNumberOfCashflows)
        {
            var notionalSchedule = NotionalScheduleGenerator.GetNotionalSchedule(initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows);
            var result = RangeHelper.ConvertArrayToRange(notionalSchedule);
            return result;
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
        /// <returns>A vertical arrray of dates.</returns>
        public object[,] UnadjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, 
            string rollConvention, out DateTime firstRegularPeriodStartDate, out DateTime lastRegularPeriodEndDate)
        {
            var dates = DateScheduler.UnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollConvention, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);
            var result = RangeHelper.ConvertArrayToRange(dates);
            return result;
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
        /// <returns>A vertical arrray of dates.</returns>
        public object[,] AdjustedDatesFromEffectiveDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, 
            string businessCenters, string dateAdjustmentConvention)
        {
            BusinessCenters centers = BusinessCentersHelper.Parse(businessCenters);
            IBusinessCalendar calendar = Engine.ToBusinessCalendar(centers);
            var adjustments = EnumHelper.Parse<BusinessDayConventionEnum>(dateAdjustmentConvention);
            var adjustedDateSchedule = AdjustedDateScheduler.AdjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollConvention, calendar, adjustments);
            var result = RangeHelper.ConvertArrayToRange(adjustedDateSchedule);
            return result;
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
        /// <returns>A vertical arrray of dates.</returns>
        public object[,] AdjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, 
            string rollConvention, string businessCenters, string dateAdjustmentConvention)
        {
            BusinessCenters centers = BusinessCentersHelper.Parse(businessCenters);
            IBusinessCalendar calendar = Engine.ToBusinessCalendar(centers);
            var adjustments = EnumHelper.Parse<BusinessDayConventionEnum>(dateAdjustmentConvention);
            var adjustedDateSchedule = AdjustedDateScheduler.AdjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval,
                rollConvention, calendar, adjustments);
            var result = RangeHelper.ConvertArrayToRange(adjustedDateSchedule);
            return result;
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
        /// <returns>A vertical arrray of dates.</returns>
        public object[,] UnadjustedDatesFromEffectiveDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, out DateTime firstRegularPeriodStartDate, out DateTime lastRegularPeriodEndDate)
        {
            var dates = DateScheduler.UnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollConvention, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);
            var result = RangeHelper.ConvertArrayToRange(dates);
            return result;
        }

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinitionRange">This must have 3 columns: interval, interval, rollconventionenum.</param>
        /// <param name="startDate">The start date of the schedule to be generated.</param>
        /// <param name="calendar">The relevant calendar.</param>
        /// <param name="businessDayAdjustment">The business day adjustments.</param>
        /// <returns>A vertical arrray of dates.</returns>
        public object[,] GetMetaDatesSchedule(Excel.Range metaScheduleDefinitionRange,
                                                       DateTime startDate,
                                                       string calendar,
                                                       string businessDayAdjustment)
        {
            var values = metaScheduleDefinitionRange.Value[System.Reflection.Missing.Value] as object[,];
            List<ThreeStringsRangeItem> metaScheduleDefinition = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<ThreeStringsRangeItem>(values);
            BusinessCenters centers = BusinessCentersHelper.Parse(calendar);
            IBusinessCalendar businessCalendar = Engine.ToBusinessCalendar(centers);
            var metaSchedule = AdjustedDatesMetaSchedule.GetMetaDatesSchedule(metaScheduleDefinition, startDate, businessCalendar, calendar, businessDayAdjustment);
            var result = RangeHelper.ConvertArrayToRange(metaSchedule);
            return result;
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
        public object[,] GetAdjustedCalculationPeriodDates(DateTime effectiveDate, DateTime terminationDate,
                                                                    string periodInterval, string rollConvention, DateTime firstRegularPeriodDate, string stubPeriodType,
                                                                    Excel.Range calendars, string businessDayConvention)
        {
            List<string> unqVals = DataRangeHelper.StripRange(calendars);
            IBusinessCalendar businessCalendar = CalendarService.GetCalendar(unqVals.ToArray());
            var dates = AdjustedDateScheduler.GetAdjustedCalculationPeriodDates(effectiveDate, terminationDate,
                                                                     periodInterval, rollConvention, firstRegularPeriodDate, stubPeriodType,
                                                                     businessCalendar, businessDayConvention);
            var result = RangeHelper.ConvertArrayToRange(dates);
            return result;
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
        public object[,] GetUnadjustedCalculationPeriodDates(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, 
            DateTime firstRegularPeriodDate, string stubPeriodType)
        {
            var dates = DateScheduler.GetUnadjustedCalculationPeriodDates(effectiveDate, terminationDate,
                                                                              periodInterval, rollConvention,
                                                                              firstRegularPeriodDate, stubPeriodType);
            var result = RangeHelper.ConvertArrayToRange(dates);
            return result;
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
        public object[,] UnadjustedCalculationDatesFromFirstRegularInterval(DateTime effectiveDate, DateTime terminationDate, 
            string intervalToFirstRegularPeriodStart, string periodInterval, string rollConvention, string stubPeriodType)
        {
            var dates = DateScheduler.UnadjustedCalculationDatesFromFirstRegularInterval(effectiveDate,
                                                                                             terminationDate,
                                                                                             intervalToFirstRegularPeriodStart,
                                                                                             periodInterval,
                                                                                             rollConvention,
                                                                                             stubPeriodType);
            var result = RangeHelper.ConvertArrayToRange(dates);
            return result;
        }

        #endregion
    }
}