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


#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.Assets;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IExchangeTradedCurve : ICurve, ICloneable
    {
        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        List<IPriceableFuturesAssetController> PriceableExchangeAssets { get; }

        /// <summary>
        /// Gets the forward value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetForward(DateTime baseDate, DateTime targetDate);

        /// <summary>
        /// Gets the forward value.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        double GetForward(double time);

        /// <summary>
        /// Gets the forward value.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetForward(DateTime targetDate);

        /// <summary>
        /// Gets the asset quotations.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, Decimal> GetAssetQuotations();

        /// <summary>
        /// Gets the assets.
        /// </summary>
        /// <returns></returns>
        Asset[] GetAssets();

        /// <summary>
        /// Gets the days and zero rates.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="compounding">The compounding.</param>
        /// <returns></returns>
        IDictionary<int, Double> GetDaysAndZeroRates(DateTime baseDate, string compounding);

        /// <summary>
        /// Creates the basic rate curve risk set, using the current curve as the base curve.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point pertubation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of pertubed rate curves</returns>
        List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation);
    }
}