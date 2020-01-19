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

using System.Collections.Generic;

namespace FpML.V5r10.CalendarEngine.Rules
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
