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

namespace Orion.Models.ForeignExchange
{
    public interface IFxOptionAssetParameters
    {
        /// <summary>
        /// Gets or sets the isput flag.
        /// </summary>
        /// <value>The isput flag.</value>
        Boolean isPut { get; set; }

        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        Boolean isDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The start premium.</value>
        Decimal Premium { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal PremiumPaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        Decimal[] DiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the volatilities.
        /// </summary>
        /// <value>The volatilities.</value>
        Decimal[] Volatilities { get; set; }

        /// <summary>
        /// Gets or sets the forward rates.
        /// </summary>
        /// <value>The forward rates.</value>
        Decimal[] ForwardRates { get; set; }

        /// <summary>
        /// Gets or sets the times to expiry.
        /// </summary>
        /// <value>The times to expiry.</value>
        Decimal[] TimesToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        Decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the strike rates.
        /// </summary>
        /// <value>The strike rates.</value>
        Decimal[] StrikeRates { get; set; }

        /// <summary>
        /// Gets or sets the year fractions.
        /// </summary>
        /// <value>The year fractions.</value>
        Decimal[] YearFractions { get; set; }
    }
}