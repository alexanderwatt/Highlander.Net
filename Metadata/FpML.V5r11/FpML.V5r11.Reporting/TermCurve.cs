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
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r3.Reporting
{
    public partial class TermCurve
    {
        #region Object Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<DateTime> GetListTermDates()
        {
            return point.Select(eachPoint => XsdClassesFieldResolver.TimeDimensionGetDate(eachPoint.term)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<decimal> GetListMidValues()
        {
            return point.Select(eachPoint => eachPoint.mid).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointToAdd"></param>
        public void Add(TermPoint pointToAdd)
        {
            var list = new List<TermPoint>(point) { pointToAdd };
            point = list.ToArray();
        }

        #endregion

        #region Static Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static TermCurve Create(List<TermPoint> points)
        {
            var result = new TermCurve { point = points.ToArray() };

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="extrapolationTrue"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static TermCurve Create(DateTime baseDate, InterpolationMethod interpolationMethod, bool extrapolationTrue, List<TermPoint> points)
        {
            var result = new TermCurve
            {
                extrapolationPermitted = extrapolationTrue,
                extrapolationPermittedSpecified = true,
                interpolationMethod = interpolationMethod,
                point = points.ToArray()
            };

            return result;
        }

        #endregion
    }
}