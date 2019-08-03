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

#region Using Directives

using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;

#endregion

namespace Orion.CurveEngine.PricingStructures.Helpers
{
    /// <summary>
    /// A special interpolator for spreads.
    /// </summary>
    public static class SpreadSplicer
    {
        /// <summary>
        /// Adds the extra points defined using the spread controllers.
        /// </summary>
        /// <param name="yieldCurveValuation"></param>
        /// <param name="priceableSpreadAssets"></param>
        /// <returns></returns>
        public static YieldCurveValuation SpreadController(PricingStructureValuation yieldCurveValuation, IPriceableSpreadAssetController[] priceableSpreadAssets)
        {
            var curveValuation = (YieldCurveValuation)yieldCurveValuation;
            var curve = curveValuation.discountFactorCurve;
            return curveValuation;
        }

        /// <summary>
        /// Gets the term points from a priceable spread asset controller.
        /// </summary>
        /// <param name="spreadAsset"></param>
        /// <returns></returns>
        public static TermPoint[] SpreadAssetMapper(IPriceableSpreadAssetController spreadAsset)
        {
            var points = new TermPoint[1];
            return points;
        }
    }
}