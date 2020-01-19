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

#region Usings

using System;
using Highlander.Reporting.Models.V5r3.ForeignExchange.FxLeg;

#endregion

namespace Highlander.Reporting.Models.V5r3.ForeignExchange.FxOptionLeg
{
    public interface IFxOptionLegParameters : IFxLegParameters
    {
        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        Decimal? Premium { get; set; }

        /// <summary>
        /// The is call flag;
        /// </summary>
        bool IsCall { get; set; }

        ///// <summary>
        ///// Gets or sets the metrics.
        ///// </summary>
        ///// <value>The metrics.</value>
        //string[] Metrics { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exchange currency1.
        ///// </summary>
        ///// <value>The amount of exchange currency1.</value>
        //Decimal ExchangedCurrency1 { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exchange currency2.
        ///// </summary>
        ///// <value>The amount of exchange currency1.</value>
        //Decimal ExchangedCurrency2 { get; set; }

        ///// <summary>
        ///// Gets or sets the currency discount factor.
        ///// </summary>
        ///// <value>The currency1 discount factor.</value>
        //Decimal Currency1DiscountFactor { get; set; }

        ///// <summary>
        ///// Gets or sets the currency discount factor.
        ///// </summary>
        ///// <value>The currency2 discount factor.</value>
        //Decimal Currency2DiscountFactor { get; set; }
    }
}