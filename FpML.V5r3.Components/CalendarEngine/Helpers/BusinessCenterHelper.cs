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

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.CalendarEngine.Rules;
using Orion.Constants;
using BusinessCentreDateRulesProp = Orion.Constants.BusinessCentreDateRulesProp;
//using LocationCalendarYearProp = Orion.Constants.LocationCalendarYearProp;

#endregion

namespace Orion.CalendarEngine.Helpers
{
    /// <summary>
    /// Helper class for Business Centers
    /// </summary>
    public class BusinessCenterHelper
    {
        /// <summary>
        /// Creates a consolidated business calendar for a given set of business centers
        /// </summary>
        /// <param name="centers">The centers.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <returns></returns>
        public static IBusinessCalendar ToBusinessCalendar(ICoreCache cache, BusinessCenters centers, string nameSpace)
        {
            if (centers != null)
            {
                var calendars = centers.businessCenter.Select(businessCenter => businessCenter.Value).ToArray();
                var dps = GetDateRuleParser(cache, calendars, nameSpace);
                var significantDays = GetBusinessCentreHolidayDates(cache, Dedupe(dps.FpmlName), nameSpace);
                IBusinessCalendar result = new BusinessCalendar(significantDays, dps);
                return result;
            }
            return null;
        }

        /// <summary>
        /// Creates a consolidated business calendar for a given set of business centers
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="centers">The centers.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <returns></returns>
        public static IBusinessCalendar ToBusinessCalendar(ICoreCache cache, string[] centers, string nameSpace)
        {
            if (centers != null)
            {
                var dps = GetDateRuleParser(cache, centers, nameSpace);
                var significantDays = GetBusinessCentreHolidayDates(cache, Dedupe(dps.FpmlName), nameSpace);
                IBusinessCalendar result = new BusinessCalendar(significantDays, dps);
                return result;
            }
            return null;
        }

        /// <summary>
        /// The date rule parser.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="calendars"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static IDateRuleParser GetDateRuleParser(ICoreCache cache, string[] calendars, string nameSpace)
        {
            if (calendars != null)
            {
                var path = nameSpace + "." + BusinessCentreDateRulesProp.GenericName;
                var loadedObject = cache.LoadObject<DateRules>(path);
                var dps = new DateRuleParser(calendars, loadedObject.DateRuleProfile);
                return dps;
            }
            return null;
        }


        /// <summary>
        /// Significant date.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="businessCenters">The city names.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <returns></returns>
        private static List<SignificantDay> GetBusinessCentreHolidayDates(ICoreCache cache, IEnumerable<string> businessCenters, string nameSpace)
        {
            var dateList = new List<SignificantDay>();
            //The new filter with OR on arrays..
            var path = nameSpace + "." + BusinessCenterCalendarProp.GenericName;
            foreach (var centre in businessCenters)
            {
                var identifier = path + '.' + centre;
                var loadedObject = cache.LoadObject<BusinessCenterCalendar>(identifier);
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
        /// De-dupes the specified calendar names.
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
    }
}