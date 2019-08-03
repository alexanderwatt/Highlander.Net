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