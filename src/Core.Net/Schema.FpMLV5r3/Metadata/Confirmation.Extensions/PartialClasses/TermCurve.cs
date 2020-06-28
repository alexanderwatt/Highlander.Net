using System;
using System.Collections.Generic;

namespace nab.QDS.FpML.V47
{
    public partial class TermCurve
    {
        #region Object Methods
        public List<DateTime> GetListTermDates()
        {
            List<DateTime> result = new List<DateTime>();

            foreach (TermPoint eachPoint in point)
            {
                DateTime dateTime = XsdClassesFieldResolver.TimeDimension_GetDate(eachPoint.term);
                result.Add(dateTime);
            }

            return result;
        }

        public List<decimal> GetListMidValues()
        {
            List<decimal> result = new List<decimal>();

            foreach (TermPoint eachPoint in point)
            {
                result.Add(eachPoint.mid);
            }

            return result;
        }

        public void Add(TermPoint pointToAdd)
        {
            List<TermPoint> list = new List<TermPoint>(this.point) { pointToAdd };

            this.point = list.ToArray();
        }

        #endregion

        #region Static Constructors

        public static TermCurve Create(List<TermPoint> points)
        {
            TermCurve result = new TermCurve { point = points.ToArray() };

            return result;
        }

        public static TermCurve Create(DateTime baseDate, InterpolationMethod interpolationMethod, bool extrapolationTrue, List<TermPoint> points)
        {
            TermCurve result = new TermCurve
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