using System;
using System.Collections.Generic;

namespace nab.QDS.FpML.V47
{
    /// <summary>
    /// A very simple class that can be used for generic SortedLists and Dictionaries
    /// that require a way to test equality
    /// </summary>
    public class PeriodComparer : IEqualityComparer<Period>, IComparer<Period>
    {
        #region IEqualityComparer<Period> Members

        public bool Equals(Period x, Period y)
        {
            if (x.period == y.period)
                if (Convert.ToInt32(x.periodMultiplier) == Convert.ToInt32(y.periodMultiplier))
                    return true;
                else
                    return false;
            else
                return false;
        }

        public int GetHashCode(Period obj)
        {
            return obj.GetHashCode();
        }

        #endregion

        #region IComparer<Period> Members

        private static int PeriodDays(Period p)
        {
            switch (p.period)
            {
                case PeriodEnum.W:
                    return 7;
                case PeriodEnum.M:
                    return 30;
                case PeriodEnum.Y:
                    return 360;
                default:
                    return 1;
            }
        }

        public int Compare(Period x, Period y)
        {
            // nothing == nothing, nothing < something
            if ((x == null) && (y == null))
                return 0;
            if ((x != null) && (y == null))
                return 1;
            if ((x == null) && (y != null))
                return -1;
            if (x == y)
                return 0;
            int xPeriods = Int32.Parse(x.periodMultiplier) * PeriodDays(x);
            int yPeriods = Int32.Parse(y.periodMultiplier) * PeriodDays(y);
            if (xPeriods > yPeriods)
                return 1;
            else if (yPeriods > xPeriods)
                return -1;
            else
                return 0;
        }

        #endregion
    }
}
