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
using Highlander.Core.Common;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Constants;
using Highlander.Reporting.V5r3;
using AssetClass = Highlander.Constants.AssetClass;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class RateATMVolatilitySurface : ExpiryTermTenorATMVolatilitySurface
    {
        #region Constructors

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="expiryTenors"></param>
        /// <param name="tenors"></param>
        /// <param name="volSurface"></param>
        /// <param name="surfaceId"></param>
        /// <param name="baseDate"></param>
        /// <param name="algorithm">The algorithm for interpolation.</param>
        public RateATMVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, String[] expiryTenors, String[] tenors, Double[,] volSurface, VolatilitySurfaceIdentifier surfaceId, DateTime baseDate, 
                                        string algorithm)
            : base(logger, cache, nameSpace, expiryTenors, tenors, volSurface, surfaceId, baseDate, algorithm)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
        }

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="tenors">The tenors.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        public RateATMVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] expiryTenors, String[] tenors, Double[,] volSurface)
            : base(logger, cache, nameSpace, properties, expiryTenors, tenors, volSurface)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
        }

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="instrumentIds">A set of valid instrumentIds</param>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="tenors">The v.</param>
        /// <param name="values">The values.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        public RateATMVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] instrumentIds, String[] expiryTenors, String[] tenors, Double[,] values)
            : base(logger, cache, nameSpace, properties, expiryTenors, tenors, MapFromRatePremiums(instrumentIds, values))//TODO map into the volsurface.
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The data.</param>
        /// <param name="properties">The properties.</param>
        public RateATMVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
            : base(logger, cache, nameSpace, fpmlData, properties)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
        }

        #endregion  

        ///<summary>
        /// Maps from the premium values to implied vols.
        ///</summary>
        ///<param name="instrumentIds">Valid instrument ids.</param>
        ///<param name="values">The matrix of values.</param>
        ///<returns></returns>
        public static Double[,] MapFromRatePremiums(String[] instrumentIds, Double[,] values)
        {
            return values;
        }
    }
}