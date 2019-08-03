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

namespace Orion.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base Interface for Rate Option
    /// </summary>
    /// <typeparam name="AMP">The type of the MP.</typeparam>
    /// <typeparam name="AMR">The type of the MR.</typeparam>
    public interface IPriceableRateOption<AMP, AMR> : IPriceableFloatingRateCoupon<AMP, AMR>
    {
        /// <summary>
        /// Gets the name of the volatility curve.
        /// </summary>
        /// <value>The name of the volatility curve.</value>
        string VolatilityCurveName { get; }

        /// <summary>
        /// Gets the strike price.
        /// </summary>
        /// <value>The strike price.</value>
        Decimal StrikePrice { get; }

        /// <summary>
        /// Updates the name of the volatility curve.
        /// </summary>
        /// <param name="newVolatilityCurveName">New name of the volatility curve.</param>
        void UpdateVolatilityCurveName(string newVolatilityCurveName);
    }
}