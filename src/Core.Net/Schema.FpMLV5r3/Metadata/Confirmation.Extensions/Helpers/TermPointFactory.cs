using System;

namespace nab.QDS.FpML.V47
{
    public static class TermPointFactory
    {
        public static TermPoint Create(decimal mid, DateTime term)
        {
            TermPoint termPoint = new TermPoint();

            termPoint.mid = mid;
            termPoint.midSpecified = true;

            TimeDimension timeDimension = new TimeDimension();
            XsdClassesFieldResolver.TimeDimension_SetDate(timeDimension, term);
            termPoint.term = timeDimension;

            return termPoint;
        }
    }
}