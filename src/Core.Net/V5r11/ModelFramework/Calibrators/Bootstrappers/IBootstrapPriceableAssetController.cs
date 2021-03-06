﻿using System;
using Orion.ModelFramework.Assets;

namespace Orion.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// A bootstrap controller interface. 
    /// D - Denotes a generic type for bootstrap data
    /// R - Denotes a generic type for the results
    /// </summary>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="R"></typeparam>
    public interface IBootstrapPriceableAssetController<D, R> : IBootstrapController<D, R>
    {
        /// <summary>
        /// Calculates the specified bootstrap data.
        /// </summary>
        /// <param name="bootstrapData">The bootstrap data.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        R Calculate(D bootstrapData, IPriceableAssetController[] priceableAssets);
    }
}