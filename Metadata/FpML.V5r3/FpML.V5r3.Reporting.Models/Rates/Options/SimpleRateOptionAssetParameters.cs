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

namespace Orion.Models.Rates.Options
{
    public class SimpleRateOptionAssetParameters : ISimpleOptionAssetParameters
    {
        #region IRateOptionAssetParameters Members

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the base NPV.
        /// </summary>
        /// <value>The notional.</value>
        public decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal YearFraction { get; set; }

        /// <summary>
        /// Gets or sets the isPut flag.
        /// </summary>
        /// <value>The isPut flag.</value>
        public bool IsPut { get; set; }

        /// <summary>
        /// Gets or sets the isVolatilityQuote flag.
        /// </summary>
        /// <value>The isVolatilityQuote flag.</value>
        public bool IsVolatilityQuote { get; set; }

        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        public bool IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal Volatility { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The start premium.</value>
        public decimal Premium { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public decimal Strike { get; set; }

        /// <summary>
        /// Gets or sets the time to expiry.
        /// </summary>
        /// <value>The time to expiry.</value>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public decimal PremiumPaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the payment discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public Decimal PaymentDiscountFactor { get; set; }

        #endregion
    }
}