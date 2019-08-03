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
using System.Linq;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.Helpers
{
    ///<summary>
    /// A helper class to create term points.
    ///</summary>
    public class TermPointsFactory
    {
        private const string TermPointConstructorArgumentError =
            "Dates array length {0} and discount factors array length {1} must be equal";

        ///<summary>
        /// Creates a term point array.
        ///</summary>
        ///<param name="dates"></param>
        ///<param name="discountFactors"></param>
        ///<returns></returns>
        public static TermPoint[] Create(IList<DateTime> dates, IList<double> discountFactors)
        {
            return dates.Select((t, i) => TermPointFactory.Create((decimal) discountFactors[i], t)).ToArray();
        }

        ///<summary>
        /// Creates a term point array.
        ///</summary>
        ///<param name="dates"></param>
        ///<param name="discountFactors"></param>
        ///<returns></returns>
        public static TermPoint[] Create(IList<DateTime> dates, IList<decimal> discountFactors)
        {
            if (dates.Count != discountFactors.Count)
            {
                throw new ArgumentException(string.Format(TermPointConstructorArgumentError, dates.Count, discountFactors.Count));
            }
            return dates.Select((t, i) => TermPointFactory.Create(discountFactors[i], t)).ToArray();
        }

        ///<summary>
        /// Creates a term point array.
        ///</summary>
        public static TermPoint[] Create(IList<TimeDimension> timeDimensions, IList<decimal> termPointValues)
        {
            var termPoints = new List<TermPoint>();
            var length = Math.Min(timeDimensions.Count, termPointValues.Count);
            for(var i = 0; i < length; i++)
            {
                var point = TermPointFactory.Create(termPointValues[i], timeDimensions[i]);
                termPoints.Add(point);
            }
            return termPoints.ToArray();
        }

        ///<summary>
        /// Creates a term point array.
        ///</summary>
        public static TermPoint[] Create(IDictionary<DateTime, Pair<string, decimal>> items)
        {
            var termPoints = new List<TermPoint>();
            foreach (var item in items)
            {
                var point = TermPointFactory.Create(item.Value.Second, item.Key);
                if (!string.IsNullOrEmpty(item.Value.First))
                {
                    point.id = item.Value.First;
                }
                termPoints.Add(point);
            }
            return termPoints.ToArray();
        }
    }
}