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

namespace Orion.Models.ForeignExchange.FxSwap
{
    public class FxSwapParameters : IFxSwapParameters
    {
        #region Implementation of IFxSwapParameters

        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        /// <value>The multiplier.</value>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency1.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        public decimal ExchangedStartCurrency1 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency1.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        public decimal ExchangedEndCurrency1 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency2.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        public decimal ExchangedStartCurrency2 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency2.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        public decimal ExchangedEndCurrency2 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency1 discount factor.</value>
        public decimal Currency1DiscountFactor1 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency1 discount factor.</value>
        public decimal Currency1DiscountFactor2 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency2 discount factor.</value>
        public decimal Currency2DiscountFactor1 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency2 discount factor.</value>
        public decimal Currency2DiscountFactor2 { get; set; }

        #endregion
    }
}