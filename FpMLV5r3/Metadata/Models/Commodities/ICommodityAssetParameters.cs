﻿/*
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

namespace Highlander.Reporting.Models.V5r3.Commodities
{
    public interface ICommodityAssetParameters
    {
        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or relevant fx spot rate.
        /// </summary>
        /// <value>The relevant fx spot rate.</value>
        Decimal CommodityForward { get; set; }

        /// <summary>
        /// Gets or relevant spread.
        /// </summary>
        /// <value>The relevant spread.</value>
        Decimal Spread { get; set; }

        /// <summary>
        /// Gets the conversion units..
        /// </summary>
        /// <value>The currency denominator flag.</value>
        bool Currency1PerCurrency2 { get; set; }

        /// <summary>
        /// Gets or relevant fx curve spot rate.
        /// </summary>
        /// <value>The relevant fx curve spot rate.</value>
        Decimal CommodityCurveForward { get; set; }

        /// <summary>
        /// Gets or sets the spot discount factor for currency1.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal Ccy1SpotDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the spot discount factor for currency1.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal Ccy2SpotDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        Decimal YearFraction { get; set; }
    }
}