#region Using directives

using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;

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