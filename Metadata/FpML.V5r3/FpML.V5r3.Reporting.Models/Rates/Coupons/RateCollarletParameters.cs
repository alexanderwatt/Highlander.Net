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

namespace Orion.Models.Rates.Coupons
{
    public class RateCollarletParameters : RateCouponParameters, IRateCollarletParameters
    {
        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The cap volatility.</value>
        public decimal CapVolatility { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The floor volatility.</value>
        public decimal FloorVolatility { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The cap strike.</value>
        public decimal CapStrike { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The floor strike.</value>
        public decimal FloorStrike { get; set; }

    }
}