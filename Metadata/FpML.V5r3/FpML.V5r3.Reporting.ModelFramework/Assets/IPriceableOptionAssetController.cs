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

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableOptionAssetController : IPriceableAssetController
    {
        /// <summary>
        /// Gets the volatility at expiry.
        /// </summary>
        /// <value>The volatility at expiry.</value>
        Decimal VolatilityAtRiskMaturity { get; }

        /// <summary>
        /// Gets the expiry date
        /// </summary>
        /// <returns></returns>
        DateTime GetExpiryDate();

        /// <summary>
        /// The underlying asset reference
        /// </summary>
        string UnderlyingAssetRef { get; }
    }
}