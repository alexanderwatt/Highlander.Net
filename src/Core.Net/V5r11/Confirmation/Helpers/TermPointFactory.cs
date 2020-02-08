using System;

namespace FpML.V5r3.Confirmation
{
    public static class TermPointFactory
    {
        public static TermPoint Create(decimal mid, DateTime term)
        {
            var termPoint = new TermPoint();
            termPoint.mid = mid;
            termPoint.midSpecified = true;
            var timeDimension = new TimeDimension();
            XsdClassesFieldResolver.TimeDimensionSetDate(timeDimension, term);
            termPoint.term = timeDimension;

            return termPoint;
        }
    }
}