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
using Core.Common;
using Orion.Constants;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class CommodityVolatilitySurface : ExpiryTermStrikeVolatilitySurface
    {
        #region Constructors

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="instrumentIds">A set of valid instrumentIds</param>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="values">The values.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="properties">The properties.</param>
        public CommodityVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] instrumentIds, String[] expiryTenors, Double[] strikes, Double[,] values)
            : base(logger, cache, nameSpace, properties, expiryTenors, strikes, MapFromCommodityPremiums(instrumentIds, values))//TODO map into the volsurface.
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity);
        }

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="expiryTenors"></param>
        /// <param name="strikes"></param>
        /// <param name="volSurface"></param>
        /// <param name="surfaceId"></param>
        /// <param name="logger"></param>
        /// <param name="cache">The algorithm for interpolation.</param>
        public CommodityVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, String[] expiryTenors, Double[] strikes, Double[,]
                                                                                       volSurface, VolatilitySurfaceIdentifier surfaceId)
            : base(logger, cache, nameSpace, expiryTenors, strikes, volSurface, surfaceId)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity);
        }

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="properties">The properties.</param>
        public CommodityVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] expiryTenors, Double[] strikes, Double[,] volSurface)
            : base(logger, cache, nameSpace, properties, expiryTenors, strikes, volSurface)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity);
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="fpmlData"></param>
        public CommodityVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData)
            : this(logger, cache, nameSpace, fpmlData, PricingStructurePropertyHelper.CommodityVolatilityMatrix(fpmlData))
        {}

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="fpmlData">The data.</param>
        /// <param name="properties">The properties.</param>
        public CommodityVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
            : base(logger, cache, nameSpace, fpmlData, properties)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity);
        }

        #endregion  

        ///<summary>
        /// Maps from the premium values to implied vols.
        ///</summary>
        ///<param name="instrumentIds">Valid instrument ids.</param>
        ///<param name="values">The matric of values.</param>
        ///<returns></returns>
        public static Double[,] MapFromCommodityPremiums(String[] instrumentIds, Double[,] values)
        {
            return values;
        }
    }
}