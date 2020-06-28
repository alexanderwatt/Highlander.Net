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

namespace Highlander.EquityVolatilityCalculator.V5r3
{
    /// <summary>
    /// Cents or index points
    /// </summary>
    public enum Units
    {
        Cents = 0,
        Dollars = 1,
        Points = 2
    };

    public interface IStock
    {
        /// <summary>
        /// Gets the asset id.
        /// </summary>
        /// <value>The asset id.</value>
        string AssetId { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IStock"/> is liquid.
        /// </summary>
        /// <value><c>true</c> if liquid; otherwise, <c>false</c>.</value>
        Boolean Liquid { get; set; }

        /// <summary>
        /// Gets or sets the volatility surface.
        /// </summary>
        /// <value>The volatility surface.</value>
        IVolatilitySurface VolatilitySurface { get; set; }

        /// <summary>
        /// Gets or sets the _dividends.
        /// </summary>
        /// <value>The _dividends.</value>
        List<Dividend> Dividends { get; set; }

        /// <summary>
        /// Gets or sets the _valuations.
        /// </summary>
        /// <value>The _valuations.</value>
        List<Valuation> Valuations { get; set; }



    }
}
