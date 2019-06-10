/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Common;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.CurveEngine.Helpers;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    ///<summary>
    ///</summary>
    public class ClearedRateCurve : RateCurve
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public IRateCurve BaseDiscountingCurve { get; set; }

        /// <summary>
        /// A reference rate curve.
        /// </summary>
        public IIdentifier ReferenceDiscountingCurveId { get; set; }

        ///<summary>
        /// The spread controllers that may have been defined.
        ///</summary>
        public List<IPriceableClearedRateAssetController> PriceableClearedRateAssets { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearedRateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instrumentData">The instrument data.</param>
        /// <param name="baseDiscountingCurve">The reference curve.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        [Description("public interface")]
        public ClearedRateCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, 
            QuotedAssetSet instrumentData, IRateCurve baseDiscountingCurve, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new RateCurveIdentifier(properties), fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties);
            BaseDiscountingCurve = baseDiscountingCurve;
            var curveId = GetRateCurveId();
            var termCurve = SetConfigurationData();
            var indexTenor = curveId.ForecastRateIndex?.indexTenor;
            PriceableClearedRateAssets = PriceableAssetFactory.CreatePriceableClearedRateAssetsWithBasisSwaps(logger, cache, nameSpace, indexTenor, instrumentData, GetRateCurveId().BaseDate, fixingCalendar, rollCalendar);
            termCurve.point = ClearedRateBootstrapper.Bootstrap(PriceableClearedRateAssets, BaseDiscountingCurve, curveId.BaseDate, termCurve.extrapolationPermitted, termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, instrumentData);
            // CreatePricingStructure(curveId, termCurve, PriceableAssetFactory.Parse(PriceableClearedRateAssets));
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);                  
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="baseDiscountingCurve">The reference curve.</param>
        /// <param name="clearedRateAssets">The cleared rate asset.</param>
        /// <param name="properties">The properties of the new spread curve.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public ClearedRateCurve(ILogger logger, ICoreCache cache, string nameSpace,
            IRateCurve baseDiscountingCurve, QuotedAssetSet clearedRateAssets, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new RateCurveIdentifier(properties), fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties);
            BaseDiscountingCurve = baseDiscountingCurve;
            ReferenceDiscountingCurveId = BaseDiscountingCurve.GetPricingStructureId();
            if (PricingStructureIdentifier.PricingStructureType != PricingStructureTypeEnum.ClearedRateCurve) return;
            var termCurve = SetConfigurationData();
            var curveId = GetRateCurveId();
            var indexTenor = curveId.ForecastRateIndex?.indexTenor;
            //Set the priceable assets.
            PriceableClearedRateAssets = PriceableAssetFactory.CreatePriceableClearedRateAssetsWithBasisSwaps(logger, cache, nameSpace, indexTenor, clearedRateAssets, PricingStructureIdentifier.BaseDate, fixingCalendar, rollCalendar);
            termCurve.point = ClearedRateBootstrapper.Bootstrap(PriceableClearedRateAssets, BaseDiscountingCurve, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, clearedRateAssets);
            // CreatePricingStructure(curveId, termCurve, PriceableAssetFactory.Parse(PriceableClearedRateAssets));
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearedRateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public ClearedRateCurve(ILogger logger, ICoreCache cache, string nameSpace, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, 
            NamedValueSet properties, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new RateCurveIdentifier(properties), fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties);
            var refCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
            ReferenceDiscountingCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceDiscountingCurveId = null;
            var tempFpml = (YieldCurveValuation)fpmlData.Second;
            var curveId = GetRateCurveId();
            Initialize(properties, Holder);
            FixingCalendar = fixingCalendar;
            PaymentCalendar = rollCalendar;
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);//TODO removed optimisation as it means that partial hedges can not be undertaken.
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(properties);
            var termCurve = SetConfigurationData();
            var qas = tempFpml.inputs;
            var indexTenor = curveId.ForecastRateIndex?.indexTenor;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.discountFactorCurve?.point == null || tempFpml.discountFactorCurve.point.Length == 0;
            //This is an override if the cache is null, as the bootstrapper will not work.
            if (cache == null)
            {
                //optimize = true;
                bootstrap = false;
            }
            bool validAssets = XsdClassesFieldResolver.QuotedAssetSetIsValid(qas);
            //Test to see if a bootstrap is required.
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quoted asset set in order to bootstrap.
                if (!validAssets) return;
                PriceableClearedRateAssets = PriceableAssetFactory.CreatePriceableClearedRateAssetsWithBasisSwaps(logger, cache, nameSpace, indexTenor, qas, GetRateCurveId().BaseDate, fixingCalendar, rollCalendar);
                termCurve.point = RateBootstrapper.Bootstrap(PriceableClearedRateAssets, curveId.BaseDate,
                    termCurve.extrapolationPermitted,
                    termCurve.interpolationMethod,
                    Tolerance);
                CreatePricingStructure(curveId, termCurve, qas);
                SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);
            }
            else
            {
                // the discount curve is already built, so don't rebuild
                SetFpMLData(fpmlData, false);
                SetInterpolator(((YieldCurveValuation)PricingStructureValuation).discountFactorCurve, curveId.Algorithm, curveId.PricingStructureType);
                //Set the priceable assets.
                if (validAssets)//!optimize && 
                {
                    PriceableClearedRateAssets = PriceableAssetFactory.CreatePriceableClearedRateAssetsWithBasisSwaps(logger, cache, nameSpace, indexTenor, qas, GetRateCurveId().BaseDate, fixingCalendar, rollCalendar);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="referenceCurveData">The market data. This must contain both the underlying base curve and the spread curve.
        /// Otherwise the RateBasisInterpolator can not instantiate.</param>
        /// <param name="derivedCurveData">The spread Curve Data</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public ClearedRateCurve(ILogger logger, ICoreCache cache, String nameSpace,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
        Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> derivedCurveData, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(derivedCurveData.Third, GenerateHolder(logger, cache, nameSpace, derivedCurveData.Third))
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, derivedCurveData.Third);
            var curveId = GetRateCurveId();
            //Set the identifier.
            var nvs = derivedCurveData.Third;
            var pricingStructureId = GetRateCurveId();
            var refCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(nvs);
            ReferenceDiscountingCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceDiscountingCurveId = null;
            if (pricingStructureId.PricingStructureType != PricingStructureTypeEnum.ClearedRateCurve) return;
            //Set the reference curve
            var baseCurveFpml = new Pair<PricingStructure, PricingStructureValuation>(referenceCurveData.First, referenceCurveData.Second);
            var baseCurveProps = referenceCurveData.Third;
            BaseDiscountingCurve = (IRateCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, baseCurveFpml, baseCurveProps);
            //Get the spread Data
            var spreadCurveFpml = new Pair<PricingStructure, PricingStructureValuation>(derivedCurveData.First, derivedCurveData.Second);
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(nvs);
            var tempFpml = (YieldCurveValuation)spreadCurveFpml.Second;
            var spreadAssets = tempFpml.inputs;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.discountFactorCurve?.point == null || tempFpml.discountFactorCurve.point.Length == 0;
            var indexTenor = curveId.ForecastRateIndex?.indexTenor;
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quoted asset set in order to bootstrap.
                if (!XsdClassesFieldResolver.QuotedAssetSetIsValid(spreadAssets)) return;
                PriceableClearedRateAssets =
                    PriceableAssetFactory.CreatePriceableClearedRateAssetsWithBasisSwaps(logger, cache, nameSpace, indexTenor, spreadAssets, pricingStructureId.BaseDate, fixingCalendar, rollCalendar);
                GetYieldCurveValuation().zeroCurve = null;
                TermCurve termCurve = tempFpml.discountFactorCurve ?? SetConfigurationData();
                DateTime baseDate = GetYieldCurveValuation().baseDate.Value;
                termCurve.point = ClearedRateBootstrapper.Bootstrap(PriceableClearedRateAssets, BaseDiscountingCurve, baseDate, termCurve.extrapolationPermitted, termCurve.interpolationMethod, Tolerance);
                SetFpMLData(new Pair<PricingStructure, PricingStructureValuation>(PricingStructure, PricingStructureValuation), false);
                SetInterpolator(termCurve, ((RateCurveIdentifier)PricingStructureIdentifier).Algorithm, ((RateCurveIdentifier)PricingStructureIdentifier).PricingStructureType);
            }
            else
            {
                // the discount curve is already built, so don't rebuild
                PriceableClearedRateAssets = PriceableAssetFactory.CreatePriceableClearedRateAssetsWithBasisSwaps(logger, cache, nameSpace, indexTenor, spreadAssets, pricingStructureId.BaseDate, fixingCalendar, rollCalendar);
                //Order the assets.
                PriceableClearedRateAssets = PriceableClearedRateAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
                CreatePricingStructure(pricingStructureId, tempFpml.discountFactorCurve, spreadAssets);
                // CreatePricingStructure(pricingStructureId, tempFpml.discountFactorCurve, PriceableAssetFactory.Parse(PriceableClearedRateAssets));
                SetInterpolator(GetYieldCurveValuation().discountFactorCurve, pricingStructureId.Algorithm, pricingStructureId.PricingStructureType);
            }
        }

        #endregion

        #region Other

        protected static PricingStructureAlgorithmsHolder GenerateHolder(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties)
        {
            var pricingStructureId = new RateCurveIdentifier(properties);
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, pricingStructureId.PricingStructureType, pricingStructureId.Algorithm);
            return holder;
        }

        #endregion
    }
}