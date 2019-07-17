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

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableFuturesAssetController : IPriceableAssetController
    {
        /// <summary>
        /// Gets the exchange initial margin.
        /// </summary>
        /// <value>The exchange initial margin.</value>
        Decimal InitialMargin { get; }

        /// <summary>
        /// Gets the last settled value.
        /// </summary>
        /// <value>The last settled value.</value>
        Decimal LastSettledValue { get; }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>The current value.</value>
        Decimal IndexAtMaturity { get; }

        /// <summary>
        /// Gets the last trading date.
        /// </summary>
        /// <value>The last trading dat.</value>
        DateTime LastTradeDate { get; }

        /// <summary>
        /// Returns the futures expiry or the option expiry date.
        /// </summary>
        /// <returns></returns>
        DateTime GetBootstrapDate();
    }
}