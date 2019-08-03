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

namespace Orion.Models.Commodities
{
    public class CommodityFuturesAssetParameters : ICommodityFuturesAssetParameters
    {
        public CommodityFuturesAssetParameters()
        {
            Position = 1;
        }

        /// <summary>
        /// Gets or position.
        /// </summary>
        /// <value>The position.</value>
        public int Position { get; set; }

        /// <summary>
        /// Gets or amount.
        /// </summary>
        /// <value>The number of underlying units per contract.</value>
        public int UnitAmount { get; set; }

        /// <summary>
        /// Gets or sets the point value.
        /// </summary>
        /// <value>The point value.</value>
        public decimal PointValue { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public Decimal Index { get; set; }

        /// <summary>
        /// The time to expiry of the future.
        /// </summary>
        /// <value>The time to expiry of the future.</value>
        public Decimal TimeToExpiry { get; set; }
    }
}