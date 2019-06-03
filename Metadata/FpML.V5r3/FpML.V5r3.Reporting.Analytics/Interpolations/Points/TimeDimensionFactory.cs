/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;

#endregion

namespace Orion.Analytics.Interpolations.Points
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
