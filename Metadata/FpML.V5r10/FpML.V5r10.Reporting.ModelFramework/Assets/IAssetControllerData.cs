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
    /// Base interface defines the data required by all asset controllers (i.e. Type A Models)
    /// </summary>
    public interface IAssetControllerData
    {
        /// <summary>
        /// Gets the basic asset valuation.
        /// </summary>
        /// <value>The basic asset valuation.</value>
        BasicAssetValuation BasicAssetValuation { get; }

        /// <summary>
        /// Gets the valuation date.
        /// </summary>
        /// <value>The valuation date.</value>
        DateTime ValuationDate { get; }

        /// <summary>
        /// Gets the market environment.
        /// </summary>
        /// <value>The market environment.</value>
        IMarketEnvironment MarketEnvironment { get; }

        /// <summary>
        /// Creates the asset controller data.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        IAssetControllerData CreateAssetControllerData(string[] metrics, DateTime baseDate, IMarketEnvironment market);
    }
}