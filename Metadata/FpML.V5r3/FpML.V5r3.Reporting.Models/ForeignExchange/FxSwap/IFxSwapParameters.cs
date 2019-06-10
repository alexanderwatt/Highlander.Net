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

namespace Orion.Models.ForeignExchange.FxSwap
{
    public interface IFxSwapParameters //: IRateInstrumentParameters
    {
        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        /// <value>The multiplier.</value>
        Decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency1.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        Decimal ExchangedStartCurrency1 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency1.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        Decimal ExchangedEndCurrency1 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency2.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        Decimal ExchangedStartCurrency2 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency2.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        Decimal ExchangedEndCurrency2 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency1 discount factor.</value>
        Decimal Currency1DiscountFactor1 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency1 discount factor.</value>
        Decimal Currency1DiscountFactor2 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency2 discount factor.</value>
        Decimal Currency2DiscountFactor1 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency2 discount factor.</value>
        Decimal Currency2DiscountFactor2 { get; set; }
    }
}