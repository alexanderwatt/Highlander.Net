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
using Highlander.Utilities.Helpers;

namespace Highlander.Equities
{
    /// <summary>
    /// A Zero Rate
    /// </summary>
    public class ZeroRate
    {
        /// <summary>
        /// Gets the tenor date.
        /// </summary>
        /// <value>The tenor date.</value>
        public DateTime TenorDate { get; }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public double Rate { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroRate"/> class.
        /// </summary>
        /// <param name="tenorDate">The tenor date.</param>
        /// <param name="rate">The rate.</param>
        public ZeroRate(DateTime tenorDate, Double rate)
        {
            InputValidator.NotNull("Tenor Date", rate, true);
            InputValidator.NotZero("Rate", rate, true);
            TenorDate = tenorDate;
            Rate = rate;
        }
    }
}
