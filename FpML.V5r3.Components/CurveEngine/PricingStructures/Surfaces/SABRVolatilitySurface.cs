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
using System.ComponentModel;
using Core.Common;
using Orion.Constants;
using Orion.ModelFramework;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.CurveEngine.Factory;
using Orion.ModelFramework.Assets;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class SABRVolatilitySurface : ExpiryTermStrikeVolatilitySurface
    {
        #region Properties

        /// <summary>
        /// Each surface references an underlying asset.
        /// </summary>
        public IList<IPriceableAssetController> UnderlyingPriceableAssets { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="expiryTenors">the expiry tenors.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        [Description("public interface")]
        public SABRVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] expiryTenors, Double[] strikes, 
            Double[,] volSurface)
            : base(logger, cache, nameSpace, properties, expiryTenors, strikes, volSurface)
        {
            var assetClass = properties.GetString(CurveProp.AssetClass, true);
            var type = EnumHelper.Parse<AssetClass>(assetClass);
            var assetId = properties.GetValue<string>(CurveProp.AssetId, true);
            var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
            PricingStructureData = new PricingStructureData(CurveType.Parent, type);
            UnderlyingPriceableAssets = new List<IPriceableAssetController>();
            foreach (var tenor in expiryTenors)
            {
                var expiryTenor = PeriodHelper.Parse(tenor);
                var offset = new Offset
                {
                    period = expiryTenor.period,
                    periodMultiplier = expiryTenor.periodMultiplier,
                    periodSpecified = true,
                    dayType = DayTypeEnum.Calendar,
                    dayTypeSpecified = true
                };
                var expiryDate = offset.Add(baseDate);
                var assetProperties = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, assetId, expiryDate);
                var asset = PriceableAssetFactory.Create(logger, cache, nameSpace, null, assetProperties, null, null);
                UnderlyingPriceableAssets.Add(asset);
            }
        }

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
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        public SABRVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, String[] instrumentIds, 
            String[] expiryTenors, Double[] strikes, Double[,] values)
            : base(logger, cache, nameSpace, properties, expiryTenors, strikes, MapFromRatePremiums(instrumentIds, values))//TODO map into the volsurface.
        {
            var assetClass = properties.GetString(CurveProp.AssetClass, true);
            var type = EnumHelper.Parse<AssetClass>(assetClass);
            PricingStructureData = new PricingStructureData(CurveType.Parent, type);
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The data.</param>
        /// <param name="properties">The properties.</param>
        public SABRVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
            : base(logger, cache, nameSpace, fpmlData, properties)
        {
            var assetClass = properties.GetString(CurveProp.AssetClass, true);
            var type = EnumHelper.Parse<AssetClass>(assetClass);
            PricingStructureData = new PricingStructureData(CurveType.Parent, type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace"></param>
        /// <param name="properties"></param>
        /// <param name="expiries"></param>
        /// <param name="vols"></param>
        /// <param name="inputInstruments"></param>
        /// <param name="inputSwapRates"></param>
        /// <param name="blackVolatilities"></param>
        public SABRVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, DateTime[] expiries, double[] vols,
                                     string[] inputInstruments, double[] inputSwapRates, double[] blackVolatilities)
            : base(logger, cache, nameSpace, properties, expiries, vols, inputInstruments, inputSwapRates, blackVolatilities)
        {
            var assetClass = properties.GetString(CurveProp.AssetClass, true);
            var type = EnumHelper.Parse<AssetClass>(assetClass);
            PricingStructureData = new PricingStructureData(CurveType.Parent, type);
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