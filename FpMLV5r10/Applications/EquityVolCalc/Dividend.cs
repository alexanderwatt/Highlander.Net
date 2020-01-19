/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace FpML.V5r10.EquityVolatilityCalculator
{
    [Serializable]
    public class Dividend
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        public Dividend()
        {
            PriceUnits = Units.Cents;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        /// <param name="exDate">The ex date.</param>
        /// <param name="amt">The amt.</param>
        /// <param name="units"></param>
        public Dividend(DateTime exDate, decimal amt, Units units)
        {
            ExDate = exDate;
            Amount = amt;
            PriceUnits = units;         
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        /// <param name="exDate">The ex date.</param>
        /// <param name="amt">The amt.</param>
        public Dividend(DateTime exDate, decimal amt)
        {
            PriceUnits = Units.Cents;
            ExDate = exDate;
            Amount = amt;
        }

        /// <summary>
        /// Gets or sets the ex date.
        /// </summary>
        /// <value>The ex date.</value>
        public DateTime ExDate { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public decimal Amount { get; set; }


        /// <summary>
        /// [ERROR: Unknown property access] the price units.
        /// </summary>
        /// <value>The price units.</value>
        public Units PriceUnits { get; set; }
    }


}
