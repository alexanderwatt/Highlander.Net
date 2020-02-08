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
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;

#endregion

namespace Highlander.Reporting.Models.V5r3.Rates.Swaption
{
    public class SwaptionInstrumentParameters : ISwaptionInstrumentParameters
    {
        public Decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public Decimal? Premium { get; set; }

        /// <summary>
        /// Gets or sets the IsBought flag.
        /// </summary>
        /// <value>The IsBought flag.</value>
        public Boolean IsBought { get; set; }

        /// <summary>
        /// Gets or sets the is a payers swaption flag.
        /// </summary>
        /// <value>The payers swaption flag.</value>
        public bool IsCall { get; set; }

        /// <summary>
        /// Gets or sets the volatility surface.
        /// </summary>
        /// <value>The volatility surface.</value>
        public IVolatilitySurface VolatilitySurface { get; set; } // VolCube!

        /// <summary>
        /// Gets or sets the other npv.
        /// </summary>
        /// <value>The other npv</value>
        public Decimal OtherNPV { get; set; }

        /// <summary>
        /// Gets or sets the swap delta.
        /// </summary>
        /// <value>The swap delta.</value>
        public decimal SwapAccrualFactor { get; set; }

        /// <summary>
        /// Gets or sets the swap break even rate.
        /// </summary>
        /// <value>The swap break even rate.</value>
        public decimal SwapBreakEvenRate { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal? Volatility { get; set; }

        /// <summary>
        /// Gets or sets the time to expiry.
        /// </summary>
        /// <value>The time to expiry.</value>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public decimal Strike { get; set; }
    }
}