using System;

namespace FpML.V5r3.Reporting.Helpers
{
    public static class TermPointFactory
    {
        public static TermPoint Create(decimal mid, DateTime term)
        {
            var termPoint = new TermPoint
            {
                mid = mid,
                midSpecified = true
            };
            var timeDimension = new TimeDimension();
            XsdClassesFieldResolver.TimeDimensionSetDate(timeDimension, term);
            termPoint.term = timeDimension;
            return termPoint;
        }

        public static TermPoint Create(decimal mid, TimeDimension term)
        {
            var termPoint = new TermPoint
            {
                mid = mid,
                midSpecified = true,
                term = term
            };
            return termPoint;
        }

        public static TermPoint Create(decimal mid, Period term)
        {
            var termPoint = new TermPoint
            {
                mid = mid,
                midSpecified = true
            };
            var timeDimension = new TimeDimension { Items = new object[] { term } };
            termPoint.term = timeDimension;
            return termPoint;
        }
    }
}