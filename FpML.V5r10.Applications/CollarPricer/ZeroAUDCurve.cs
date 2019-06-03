using System;
using System.Collections.Generic;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// Represents an Zero AUD Curve
    /// </summary>
    public class ZeroAUDCurve: ZeroCurveBase
    {
        const string CCurrencyCode = "AUD";

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroAUDCurve"/> class.
        /// </summary>
        /// <param name="curveDate">The curve date.</param>
        /// <param name="tenorDates">The tenor dates.</param>
        /// <param name="zeroRates">The zero rates.</param>
        public ZeroAUDCurve(DateTime curveDate, IEnumerable<DateTime> tenorDates, List<Double> zeroRates)
            : base(curveDate, CCurrencyCode, tenorDates, zeroRates)
        {
        }
    }
}
