using System;
using System.Collections.Generic;

namespace Orion.Analytics.Utilities
{
    /// <summary>
    /// An aggregator class.
    /// </summary>
    public static class Aggregator
    {
        /// <summary>
        /// Sums the decimals.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static Decimal SumDecimals(Decimal[] list)
        {
            var total = 0.0m;
            var itemsList = new List<Decimal>(list);
            itemsList.ForEach(delegate(Decimal item) { total = total + item; });
            return total;
        }
    }
}