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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.CurveEngine.V5r3.PricingStructures.Helpers;
using Highlander.CurveEngine.V5r3.PricingStructures.Surfaces;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using FxCurve = Highlander.CurveEngine.V5r3.PricingStructures.Curves.FxCurve;
using InflationCurve = Highlander.CurveEngine.V5r3.PricingStructures.Curves.InflationCurve;

#endregion

namespace Highlander.CurveEngine.V5r3.Helpers
{
    /// <summary>
    /// Creates a pricing structure directly.
    /// </summary>
    public static class PricingStructureHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly List<PricingStructureTypeEnum> VolSurfaceTypes =
            new List<PricingStructureTypeEnum>
                  {
                      PricingStructureTypeEnum.FxVolatilityMatrix,
                      PricingStructureTypeEnum.CommodityVolatilityMatrix,
                      PricingStructureTypeEnum.RateVolatilityMatrix,
                      PricingStructureTypeEnum.RateATMVolatilityMatrix,
                      PricingStructureTypeEnum.EquityVolatilityMatrix,
                      PricingStructureTypeEnum.EquityWingVolatilityMatrix,
                      PricingStructureTypeEnum.VolatilitySurface,
                      PricingStructureTypeEnum.VolatilitySurface2,
                      PricingStructureTypeEnum.SABRSurface
                  };

        /// <summary>
        /// 
        /// </summary>
        public static readonly List<PricingStructureTypeEnum> VolBootstrapperTypes =
            new List<PricingStructureTypeEnum>
            {
                PricingStructureTypeEnum.CapVolatilityCurve
            };

        /// <summary>
        /// 
        /// </summary>
        public static readonly List<PricingStructureTypeEnum> CurveTypes =
            new List<PricingStructureTypeEnum>
                 {
                     PricingStructureTypeEnum.RateCurve,
                     PricingStructureTypeEnum.RateSpreadCurve,
                     PricingStructureTypeEnum.RateBasisCurve,
                     PricingStructureTypeEnum.RateXccyCurve,
                     PricingStructureTypeEnum.XccySpreadCurve,
                     PricingStructureTypeEnum.DiscountCurve,
                     PricingStructureTypeEnum.DiscountSpreadCurve,
                     PricingStructureTypeEnum.BondCurve,
                     PricingStructureTypeEnum.BondDiscountCurve,
                     PricingStructureTypeEnum.InflationCurve,
                     PricingStructureTypeEnum.FxCurve,
                     PricingStructureTypeEnum.FxDerivedCurve,
                     PricingStructureTypeEnum.SurvivalProbabilityCurve,
                     PricingStructureTypeEnum.CommodityCurve,
                     PricingStructureTypeEnum.CommoditySpreadCurve,
                     PricingStructureTypeEnum.LPMCapFloorCurve,
                     PricingStructureTypeEnum.LPMSwaptionCurve,
                     PricingStructureTypeEnum.BondFinancingCurve,
                     PricingStructureTypeEnum.BondFinancingBasisCurve,                    
                     PricingStructureTypeEnum.EquityCurve,
                     PricingStructureTypeEnum.EquitySpreadCurve,
                     PricingStructureTypeEnum.ClearedRateCurve,
                     PricingStructureTypeEnum.ExchangeTradedCurve,
                     PricingStructureTypeEnum.CapVolatilityCurve,
                     PricingStructureTypeEnum.GenericVolatilityCurve
                 };

        /// <summary>
        /// Returns properties from the pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="marketEnvironmentId">The market id.</param>
        /// <param name="fpmlData">The FPML data.</param>
        public static Market CreateMarketFromFpML(string marketEnvironmentId, List<IPricingStructure> fpmlData)
        {
            var data = fpmlData.Select(curve => curve.GetFpMLData()).ToList();
            var market = CreateMarketFromFpML(marketEnvironmentId, data);
            return market;
        }

        /// <summary>
        /// Returns properties from the pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="marketEnvironmentId">The market id.</param>
        /// <param name="fpmlData">The FPML data.</param>
        public static Market CreateMarketFromFpML(string marketEnvironmentId, List<Pair<PricingStructure, PricingStructureValuation>> fpmlData)
        {
            var market = new Market
            {
                id = marketEnvironmentId,
            };
            var curves = new List<PricingStructure>();
            var curveValuations = new List<PricingStructureValuation>();
            foreach (var pair in fpmlData)
            {
                curves.Add(pair.First);
                curveValuations.Add(pair.Second);
            }
            market.Items = curves.ToArray();
            market.Items1 = curveValuations.ToArray();
            return market;
        }

        /// <summary>
        /// Returns properties from the pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="marketEnvironmentId">The market id.</param>
        /// <param name="fpmlData">The FPML data.</param>
        public static Market CreateMarketFromFpML(string marketEnvironmentId, Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var market = new Market
                             {
                                 id = marketEnvironmentId,
                                 Items = new PricingStructure[1],
                                 Items1 = new PricingStructureValuation[1]
                             };
            market.Items[0] = fpmlData.First;
            market.Items1[0] = fpmlData.Second;

            return market;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fpMLPair"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public delegate IPricingStructure GetFunctionDelegate(ILogger logger, ICoreCache cache, string nameSpace, Pair<PricingStructure, PricingStructureValuation> fpMLPair, NamedValueSet properties);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fpMLPair"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IPricingStructure GetRateCurve(ILogger logger, ICoreCache cache, string nameSpace, Pair<PricingStructure, PricingStructureValuation> fpMLPair, NamedValueSet properties)
        {
            return new RateCurve(logger, cache, nameSpace, fpMLPair, properties, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fpMLPair"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IPricingStructure GetInflationCurve(ILogger logger, ICoreCache cache, string nameSpace, Pair<PricingStructure, PricingStructureValuation> fpMLPair, NamedValueSet properties)
        {
            return new InflationCurve(logger, cache, nameSpace, fpMLPair, properties, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fpMLPair"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IPricingStructure GetFxCurve(ILogger logger, ICoreCache cache, string nameSpace, Pair<PricingStructure, PricingStructureValuation> fpMLPair, NamedValueSet properties)
        {
            return new FxCurve(logger, cache, nameSpace, fpMLPair, properties, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fpMLPair"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IPricingStructure GetVolatilitySurface2(ILogger logger, ICoreCache cache, string nameSpace, Pair<PricingStructure, PricingStructureValuation> fpMLPair, NamedValueSet properties)
        {
            return new VolatilitySurface2(fpMLPair);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fpMLPair"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IPricingStructure GetRateVolatilityMatrix(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpMLPair, NamedValueSet properties)
        {
            return new RateVolatilitySurface(logger, cache, nameSpace, fpMLPair, properties);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fpMLPair"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IPricingStructure GetRateATMVolatilityMatrix(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpMLPair, NamedValueSet properties)
        {
            return new RateATMVolatilitySurface(logger, cache, nameSpace, fpMLPair, properties);
        }

        #region Helpers

        public static object[,] FpMLPairTo2DArray(Pair<PricingStructure, PricingStructureValuation> fpMLPair)
        {
            var displayFunctions = new Dictionary<Type, DisplayFunctionDelegate>
                                       {
                                           {typeof(YieldCurveValuation), DisplayRateOrInflationCurve},
                                           {typeof(FxCurveValuation), DisplayFxCurve},
                                           {typeof(VolatilityMatrix), DisplayVolatilitySurface2}
                                       };
            Type pricingStructureDOTNETType = fpMLPair.Second.GetType();
            if (!displayFunctions.ContainsKey(pricingStructureDOTNETType))
            {
                string exceptionMessage = $"Display function does not support {pricingStructureDOTNETType} yet.";
                throw new ApplicationException(exceptionMessage);
            }
            return displayFunctions[pricingStructureDOTNETType](fpMLPair);
        }

        delegate object[,] DisplayFunctionDelegate(Pair<PricingStructure, PricingStructureValuation> fpMLPair);

        private static object[,] DisplayRateOrInflationCurve(Pair<PricingStructure, PricingStructureValuation> fpMLPair)
        {
            // ReSharper disable PossibleNullReferenceException
            var to2DArray = TermCurveTo2DArray((fpMLPair.Second as YieldCurveValuation).discountFactorCurve);
            // ReSharper restore PossibleNullReferenceException
            return to2DArray;
        }

        private static object[,] DisplayFxCurve(Pair<PricingStructure, PricingStructureValuation> fpMLPair)
        {
            // ReSharper disable PossibleNullReferenceException
            var to2DArray = TermCurveTo2DArray((fpMLPair.Second as FxCurveValuation).fxForwardCurve);
            // ReSharper restore PossibleNullReferenceException
            return to2DArray;
        }

        private static object[,] DisplayVolatilitySurface2(Pair<PricingStructure, PricingStructureValuation> fpMLPair)
        {
            var points = ((VolatilityMatrix)fpMLPair.Second).dataPoints.point;
            var matrixAsTwoDimArray = VolatilitySurfaceHelper.GetExpirationByStikeVolatilityMatrixWithDimensions(points);

            return matrixAsTwoDimArray;
        }

        private static object[,] TermCurveTo2DArray(TermCurve termCurve)
        {
            if (null != termCurve.point)
            {
                var result = new object[termCurve.point.Length, 2];
                for (int i = 0; i < termCurve.point.Length; ++i)
                {
                    var termPoint = termCurve.point[i];
                    result[i, 0] = TermPointHelper.GetDate(termPoint);
                    result[i, 1] = termPoint.mid;
                }
                return result;
            }
            // ReSharper disable RedundantIfElseBlock
            else
            // ReSharper restore RedundantIfElseBlock
            {
                return null;
            }
        }

        #endregion
    }
}