using System;
using System.Collections.Generic;

namespace Orion.Equity.VolatilityCalculator.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class DividendHelper
    {
       
        /// <summary>
        /// Dividendses to array.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="dividends">The dividends.</param>
        /// <param name="times">The times.</param>
        /// <param name="divs">The divs.</param>
        public static void DividendsToArray(DateTime baseDate, List<Dividend> dividends, ref double[] times, ref double[] divs)
        {
            int idx = 0;

            foreach (Dividend div in dividends)
            {
                divs[idx] = Convert.ToDouble(div.Amount);
                times[idx] = (div.ExDate - baseDate).Days / 365.0;
                idx++;
            }
        }


        /// <summary>
        /// Sorts the dividends.
        /// </summary>
        /// <param name="divs">The divs.</param>
        /// <returns></returns>
        public static void Sort(List<Dividend> divs)
        {
            var dc = new DivComparer();
            divs.Sort(dc);         
        }


        /// <summary>
        /// Div Comparison
        /// </summary>
        class DivComparer : IComparer<Dividend>
        {
            public int Compare(Dividend x, Dividend y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    return -1;
                }
                if (y == null)
                    return 1;
                int retval = x.ExDate.CompareTo(y.ExDate);
                return retval;
            }
        }
    }

}