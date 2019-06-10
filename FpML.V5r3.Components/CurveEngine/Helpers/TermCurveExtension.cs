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

#region Using directives

using System.Collections.Generic;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;

#endregion

namespace Orion.CurveEngine.Helpers
{
    ///<summary>
    ///</summary>
    public static class TermCurveExtension
    {
        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<returns></returns>
        public static TermCurve Sort(this TermCurve termCurve)
        {
            if (null != termCurve.point)
            {
                var termPoints = new List<TermPoint>(termCurve.point);
                termPoints.Sort((x, y) => TermPointHelper.GetDate(x).CompareTo(TermPointHelper.GetDate(y)));
                termCurve.point = termPoints.ToArray();
            }
            return termCurve;
        }

        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<returns></returns>
        public static object[,] To2DArray(this TermCurve termCurve)
        {
            if (null != termCurve.point)
            {
                var result = new object[termCurve.point.Length, 2];
                for(int i = 0; i < termCurve.point.Length; ++i)
                {
                    var termPoint = termCurve.point[i];

                    result[i, 0] = TermPointHelper.GetDate(termPoint);
                    result[i, 1] = termPoint.mid;
                }
                return result;
            }
// ReSharper disable RedundantIfElseBlock
            else
// ReSharper restore RedundantIfElseBlock
            {
                return null;
            }
        }
    }
}