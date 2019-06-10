/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Models.Assets;

namespace Orion.Models.Rates.Options
{
    public interface ISimpleOptionAssetParameters : ISimpleRateAssetParameters
    {
        /// <summary>
        /// Gets or sets the isPut flag.
        /// </summary>
        /// <value>The isPut flag.</value>
        Boolean IsPut { get; set; }

        /// <summary>
        /// Gets or sets the isVolatilityQuote flag.
        /// </summary>
        /// <value>The isVolatilityQuote flag.</value>
        Boolean IsVolatilityQuote { get; set; }

        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        Boolean IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        Decimal Volatility { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        Decimal Premium { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        Decimal Strike { get; set; }

        /// <summary>
        /// Gets or sets the time to expiry.
        /// </summary>
        /// <value>The time to expiry.</value>
        Decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the premium payment discount factor..
        /// </summary>
        /// <value>The premium payment discount factor.</value>
        Decimal PremiumPaymentDiscountFactor { get; set; }
    }
}