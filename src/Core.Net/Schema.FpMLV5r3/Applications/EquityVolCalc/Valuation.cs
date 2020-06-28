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
using Highlander.Equities;

namespace Highlander.Equity.Calculator.V5r3
{
    [Serializable]
    public class Valuation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Valuation"/> class.
        /// </summary>
        public Valuation()
        {
            PriceUnits = Units.Cents;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Valuation"/> class.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="price">The price.</param>
        /// <param name="units">The units.</param>
        public Valuation(DateTime date, decimal price, Units units)
        {
            Date = date;
            Price = price;
            PriceUnits = units;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Valuation"/> class.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="price">The price.</param>
        public Valuation(DateTime date, decimal price)
        {
            PriceUnits = Units.Cents;
            Date = date;
            Price = price;            
        }

        /// <summary>
        /// Gets or sets a value indicating whether [ex date].
        /// </summary>
        /// <value><c>true</c> if [ex date]; otherwise, <c>false</c>.</value>
        public bool ExDate { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>The price.</value>
        public decimal Price { get; set; }


        /// <summary>
        /// [ERROR: Unknown property access] the price units.
        /// </summary>
        /// <value>The price units.</value>
        public Units PriceUnits { get; set; }
    }
}
