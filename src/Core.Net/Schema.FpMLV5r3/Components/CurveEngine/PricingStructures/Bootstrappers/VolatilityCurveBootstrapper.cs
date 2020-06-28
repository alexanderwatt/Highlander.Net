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

#region Using directives

using System;
using System.Collections.Generic;
using Highlander.Reporting.ModelFramework.V5r3.Assets;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public static class VolatilityCurveBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static SortedList<DateTime, Decimal> Bootstrap(IEnumerable<IPriceableOptionAssetController> priceableAssets)
        {
            var points = new SortedList<DateTime, Decimal>();
            // Add the rest
            foreach (var priceableAsset in priceableAssets)
            {
                //TODO Replace with the expiry date.
                DateTime assetMaturityDate = priceableAsset.GetExpiryDate();
                if (points.Keys.Contains(assetMaturityDate)) continue;
                points[assetMaturityDate] = priceableAsset.VolatilityAtRiskMaturity;
            }
            return points;
        }
    }
}