using System;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r3.Confirmation
{
    public partial class TermCurve
    {
        #region Object Methods
        public List<DateTime> GetListTermDates()
        {
            return point.Select(eachPoint => XsdClassesFieldResolver.TimeDimensionGetDate(eachPoint.term)).ToList();
        }

        public List<decimal> GetListMidValues()
        {
            return point.Select(eachPoint => eachPoint.mid).ToList();
        }

        public void Add(TermPoint pointToAdd)
        {
            var list = new List<TermPoint>(point) { pointToAdd };
            point = list.ToArray();
        }

        #endregion

        #region Static Constructors

        public static TermCurve Create(List<TermPoint> points)
        {
            var result = new TermCurve { point = points.ToArray() };

            return result;
        }

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