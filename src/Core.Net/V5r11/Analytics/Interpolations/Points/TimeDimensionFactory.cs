﻿/*
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
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Interpolations.Points
{
    /// <summary>
    /// A helper to create time dimensions.
    /// </summary>
    public static class TimeDimensionFactory
    {
        /// <summary>
        /// Parses a time dimension from strings.
        /// </summary>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static TimeDimension Create(DateTime expiry)
        {
            var pExpiry = expiry;
            var dimension = new TimeDimension { Items = new object[] { pExpiry } };
            return dimension;
        }

        /// <summary>
        /// Parses a time dimension from strings.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static TimeDimension Create(DateTime expiry, string term)
        {
            var pExpiry = expiry;
            var pTerm = term != null ? PeriodHelper.Parse(term) : null;
            var dimension = pTerm != null ? new TimeDimension { Items = new object[] { pExpiry, pTerm } } : new TimeDimension { Items = new object[] { pExpiry} };
            return dimension;
        }

        /// <summary>
        /// Parses a time dimension from strings.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static TimeDimension Create(DateTime expiry, Period term)
        {
            var pExpiry = expiry;
            var pTerm = term;
            var dimension = pTerm != null ? new TimeDimension { Items = new object[] { pExpiry, pTerm } } : new TimeDimension { Items = new object[] { pExpiry } };
            return dimension;
        }
    }
}
