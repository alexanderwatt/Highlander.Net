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
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface ICommodityCurve : ICurve
    {
        /// <summary>
        /// Gets the commodity forward.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetForward(DateTime baseDate, DateTime targetDate);

        /// <summary>
        /// Gets the commodity forward.
        /// </summary>
        /// <param name="targetTime">The target time interval.</param>
        /// <returns></returns>
        double GetForward(Double targetTime);

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        List<IPriceableCommodityAssetController> PriceableCommodityAssets { get; }
    }
}