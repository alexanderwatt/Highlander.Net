/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace FpML.V5r10.CalendarEngine.Rules
{
    /// <summary>
    /// A container for all the names and FpML identifiers for the business centre holidays.
    /// </summary>
    public class DateRuleParser : IDateRuleParser
    {
        /// <summary>
        /// The date rule profile list.
        /// </summary>
        public List<DateRuleProfile> DateRuleProfiles { get; protected set; }

        /// <summary>
        /// If a single calendar is not valid the flag will be set to false.
        /// </summary>
        public bool IsValidCalendar { get; set; }

        /// <summary>
        /// Any centre that is not a valid calendar.
        /// </summary>
        public List<string> InValidCentres { get; set; }

        /// <summary>
        /// The array of names: e.g. Sydney, Adelaide etc.
        /// </summary>
        public string[] Name { get; set; }

        /// <summary>
        /// The FpNL names of the calendars. THis is what is used for getting the significant dates.
        /// </summary>
        public string[] FpmlName { get; set; }

        /// <summary>
        /// The system cultures.
        /// </summary>
        public CultureInfo[] Culture { get; set; }

        /// <summary>
        /// The system calendars.
        /// </summary>
        public Calendar[] Calendar { get; set; }

        /// <summary>
        /// The calendar names. Used only for reference.
        /// </summary>
        public string[] CalendarNames { get; set; }

        /// <summary>
        /// Resolves the profiles.
        /// </summary>
        /// <param name="dateRuleProfiles">The date rule profile names.</param>
        /// <param name="calendarNames">The calendar names.</param>
        public DateRuleParser(string[] calendarNames, IEnumerable<DateRuleProfile> dateRuleProfiles)
            : this(calendarNames, new List<DateRuleProfile>(dateRuleProfiles))
        {}

        /// <summary>
        /// Resolves the profiles.
        /// </summary>
        /// <param name="dateRuleProfiles">The date rule profile names.</param>
        /// <param name="calendarNames">The calendar names.</param>
        public DateRuleParser(string[] calendarNames, List<DateRuleProfile> dateRuleProfiles)
        {
            DateRuleProfiles = dateRuleProfiles;
            CalendarNames = calendarNames;
            var resolvedNames = new StringCollection();
            var resolvedFpmlNames = new StringCollection();
            var resolvedCultures = new List<CultureInfo>();
            var resolvedCalendars = new List<Calendar>();
            Boolean bExists = true;
            InValidCentres = new List<string>();
            foreach (string calendarName in calendarNames)
            {
                string calendarName1 = calendarName;
                DateRuleProfile dateRuleProfile = dateRuleProfiles.Find(
                    profile => (String.Compare(profile.name, calendarName1, StringComparison.OrdinalIgnoreCase) == 0)
                               || (String.Compare(profile.fPmlIdentifier, calendarName1, StringComparison.OrdinalIgnoreCase) == 0)
                    );
                if (dateRuleProfile == null)
                {
                    InValidCentres.Add(calendarName);
                    bExists = false;
                }
                else
                {
                    CultureInfo resolvedCulture = dateRuleProfile.culture.Length > 0 ? CultureInfo.CreateSpecificCulture(dateRuleProfile.culture) : CultureInfo.InvariantCulture;
                    if (!resolvedNames.Contains(dateRuleProfile.name))
                    {
                        resolvedNames.Add(dateRuleProfile.name);
                        resolvedFpmlNames.Add(dateRuleProfile.fPmlIdentifier);
                        resolvedCultures.Add(resolvedCulture);
                        resolvedCalendars.Add(resolvedCulture.Calendar);
                    }
                }
            }
            IsValidCalendar = bExists;
            Name = new string[resolvedNames.Count];
            FpmlName = new string[resolvedFpmlNames.Count];
            Culture = new CultureInfo[resolvedCultures.Count];
            Calendar = new Calendar[resolvedCalendars.Count];
            resolvedNames.CopyTo(Name, 0);
            resolvedFpmlNames.CopyTo(FpmlName, 0);
            resolvedCultures.CopyTo(Culture, 0);
            resolvedCalendars.CopyTo(Calendar, 0);
        }

        /// <summary>
        /// Calendars that are supported by the date rule set provided.
        /// </summary>
        /// <returns></returns>
        public List<string> GetCalendarsSupported()
        {
            var calendars = new List<string>();
            foreach (DateRuleProfile drp in DateRuleProfiles)
            {
                if (!calendars.Contains(drp.name) && drp.enabled)
                    calendars.Add(drp.name);
            }
            return calendars;
        }
    }
}
