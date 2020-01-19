/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;

namespace Highlander.Equities
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
        public ZeroAUDCurve(DateTime curveDate, IEnumerable<DateTime> tenorDates, List<double> zeroRates)
            : base(curveDate, CCurrencyCode, tenorDates, zeroRates)
        {
        }
    }
}
