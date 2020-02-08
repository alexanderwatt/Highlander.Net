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
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.ModelFramework.V5r3.Assets
{
    /// <summary>
    /// The Priceable base asset controller
    /// </summary>
    public interface IPriceableAssetController : IModelController<IAssetControllerData, BasicAssetValuation>, IComparable<IPriceableAssetController>
    {
        /// <summary>
        /// Gets and sets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        BasicQuotation MarketQuote { get; set; }

        /// <summary>
        /// Calculates the implied quote. For use with the fast bootstrapper.
        /// </summary>
        /// <value>The implied quote.</value>
        decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace);

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        DateTime GetRiskMaturityDate();

        /// <summary>
        /// Store the original valuations
        /// </summary>
        BasicAssetValuation BasicAssetValuation { get; set; }
    }
}