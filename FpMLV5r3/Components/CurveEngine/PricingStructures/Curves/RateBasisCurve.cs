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
using System.Linq;
using Highlander.Codes.V5r3;
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.CurveEngine.V5r3.PricingStructures.Bootstrappers;
using Highlander.CurveEngine.V5r3.PricingStructures.Interpolators;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Constants;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.V5r3;
using AssetClass = Highlander.Constants.AssetClass;
using XsdClassesFieldResolver = Highlander.Reporting.Helpers.V5r3.XsdClassesFieldResolver;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Curves
{
    /// <summary>
    /// A rate spread curve.
    /// </summary>
    public class RateBasisCurve : RateSpreadCurve
    {
        #region Runtime Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        protected RateBasisCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
            : base(properties, algorithmHolder)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        protected RateBasisCurve(ILogger logger, ICoreCache cache, String nameSpace,
                                  PricingStructureIdentifier curveIdentifier)
            : base(logger, cache, nameSpace, curveIdentifier)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="referenceCurve">The reference parent curve id.</param>
        /// <param name="spreadAssets">The spreads by asset.</param>
        /// <param name="properties">The properties of the new spread curve.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateBasisCurve(ILogger logger, ICoreCache cache, string nameSpace, 
            IRateCurve referenceCurve, QuotedAssetSet spreadAssets, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, ProcessQuotedAssetSet(logger, cache, nameSpace, referenceCurve, spreadAssets, properties, fixingCalendar, rollCalendar), properties, fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            BaseCurve = referenceCurve;
            ReferenceCurveId = BaseCurve.GetPricingStructureId();
            if (PricingStructureIdentifier.PricingStructureType != PricingStructureTypeEnum.RateBasisCurve) return;
            //Set the spread sets.
            PriceableRateSpreadAssets = PriceableAssetFactory.CreatePriceableRateSpreadAssets(logger, cache, nameSpace, PricingStructureIdentifier.BaseDate, spreadAssets, fixingCalendar, rollCalendar);
            Build(logger, cache, nameSpace, fixingCalendar, rollCalendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="spreadAssets">The spreads by asset.</param>
        /// <param name="properties">The properties of the new spread curve.</param>
        /// <param name="algorithm">The algorithm holder. </param>
        public RateBasisCurve(NamedValueSet properties, IRateCurve referenceCurve, 
            List<IPriceableRateSpreadAssetController> spreadAssets, PricingStructureAlgorithmsHolder algorithm)
            : base(properties, algorithm)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            //Set the identifier.
            var pricingStructureId = GetRateCurveId();
            if (pricingStructureId.PricingStructureType != PricingStructureTypeEnum.RateBasisCurve) return;
            //Set the reference curve
            BaseCurve = referenceCurve;
            ReferenceCurveId = BaseCurve.GetPricingStructureId();
            PriceableRateSpreadAssets = spreadAssets;
            //Order the assets.
            PriceableRateSpreadAssets = PriceableRateSpreadAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            var termCurve = SetConfigurationData();
            //Get the reference interpolated curve.
            termCurve.point = RateSpreadBootstrapper.Bootstrap(PriceableRateSpreadAssets,
                                                               BaseCurve,
                                                               pricingStructureId.BaseDate,
                                                               termCurve.extrapolationPermitted,
                                                               Tolerance);
            CreatePricingStructure(pricingStructureId, termCurve, PriceableAssetFactory.Parse(PriceableRateSpreadAssets));
            SetInterpolator(BaseCurve, pricingStructureId.PricingStructureType);
            // Set the Zero curve, just for reference
            YieldCurveValuation yieldCurveValuation = GetYieldCurveValuation();
            if (yieldCurveValuation.zeroCurve?.rateCurve == null)
            {
                //var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
                //var psType = PricingStructureIdentifier.PricingStructureType;
                TermCurve curve = YieldCurveAnalytics.ToZeroCurve(termCurve, GetBaseDate(), CompoundingFrequency, DayCounter);
                curve.interpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("CurveInterpolation"));
                var zeroCurve = new ZeroRateCurve
                    {
                        rateCurve = curve,
                        compoundingFrequency = Reporting.V5r3.CompoundingFrequency.Create(CompoundingFrequency)
                    };
                yieldCurveValuation.zeroCurve = zeroCurve;
            }
        }

        #endregion

        #region Simplified Constuctors

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curve id.</param>
        /// <param name="values">The values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateBasisCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, IPricingStructure refCurve, Decimal[] values,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, AdjustMarketQuotes(values, refCurve.GetFpMLData(), PropertyHelper.ExtractUniqueCurveIdentifier(properties)), properties,
            fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            ReferenceCurveId = refCurve.GetPricingStructureId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curve id.</param>
        /// <param name="spreadValueSet">The spread ValueSet.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateBasisCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, IPricingStructure refCurve, QuotedAssetSet spreadValueSet,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, AdjustMarketQuotes(spreadValueSet, refCurve.GetFpMLData(), PropertyHelper.ExtractUniqueCurveIdentifier(properties)), properties,
            fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties);
            ReferenceCurveId = refCurve.GetPricingStructureId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curve id.</param>
        /// <param name="value">The values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateBasisCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, IPricingStructure refCurve, Decimal value,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, AdjustMarketQuotes(value, refCurve.GetFpMLData(), PropertyHelper.ExtractUniqueCurveIdentifier(properties)), properties,
            fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            ReferenceCurveId = refCurve.GetPricingStructureId();
        }

        #endregion

        #region FpML Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="spreadFpmlData">The FPML data.</param>
        /// <param name="properties">The properties</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateBasisCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> spreadFpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, spreadFpmlData, properties, fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            var refCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
            ReferenceCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceCurveId = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="referenceCurveData">The market data. This must contain both the underlying base curve and the spread curve.
        /// Otherwise the RateBasisInterpolator can not instantiate.</param>
        /// <param name="spreadCurveData">The spread Curve Data</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateBasisCurve(ILogger logger, ICoreCache cache, String nameSpace,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
        Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(spreadCurveData.Third, GenerateHolder(logger, cache, nameSpace, spreadCurveData.Third))
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, spreadCurveData.Third); 
            //Set the identifier.
            var nvs = spreadCurveData.Third;
            var pricingStructureId = GetRateCurveId();
            var refCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(nvs);
            ReferenceCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceCurveId = null;
            if (pricingStructureId.PricingStructureType != PricingStructureTypeEnum.RateBasisCurve) return;
            //Set the reference curve
            var baseCurveFpml = new Pair<PricingStructure, PricingStructureValuation>(referenceCurveData.First, referenceCurveData.Second);
            var baseCurveProps = referenceCurveData.Third;
            BaseCurve = (IRateCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, baseCurveFpml, baseCurveProps);
            //Get the spread Data
            var spreadCurveFpml = new Pair<PricingStructure, PricingStructureValuation>(spreadCurveData.First, spreadCurveData.Second);
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(nvs);
            var tempFpml = (YieldCurveValuation)spreadCurveFpml.Second;
            var spreadAssets = tempFpml.inputs;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.discountFactorCurve?.point == null || tempFpml.discountFactorCurve.point.Length == 0;
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quoted asset set in order to bootstrap.
                if (!XsdClassesFieldResolver.QuotedAssetSetIsValid(spreadAssets)) return;
                PriceableRateSpreadAssets =
                    PriceableAssetFactory.CreatePriceableRateSpreadAssets(logger, cache, nameSpace, pricingStructureId.BaseDate, spreadAssets, fixingCalendar, rollCalendar);
                Build(logger, cache, nameSpace, fixingCalendar, rollCalendar);
            }
            else
            {
                // the discount curve is already built, so don't rebuild
                PriceableRateSpreadAssets = PriceableAssetFactory.CreatePriceableRateSpreadAssets(logger, cache, nameSpace, pricingStructureId.BaseDate, spreadAssets, fixingCalendar, rollCalendar);
                //Order the assets.
                PriceableRateSpreadAssets = PriceableRateSpreadAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
                CreatePricingStructure(pricingStructureId, tempFpml.discountFactorCurve, PriceableAssetFactory.Parse(PriceableRateSpreadAssets));
                SetInterpolator(BaseCurve, pricingStructureId.PricingStructureType);
            }
        }

        #endregion

        #region Value and Risk Functions

        /// <summary>
        /// Creates the basic rate curve risk set, using the current curve as the base curve.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point perturbation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of perturbed rate curves</returns>
        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        {
            return CreateCurveRiskSet(basisPointPerturbation, PricingStructureRiskSetType.Parent);
        }

        /// <summary>
        /// Creates the basic rate curve risk set, using the current curve as the base curve.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point perturbation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <param name="pricingStructureRiskSetType">This determine which assets to perturb. </param>
        /// <returns>A list of perturbed rate curves</returns>
        private List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation, PricingStructureRiskSetType pricingStructureRiskSetType)
        {
            var structures = new List<IPricingStructure>();
            if (PriceableRateSpreadAssets == null) return structures;
            //Add the spread asset perturbed curves.
            //
            //Set the parameters and properties.
            decimal perturbation = basisPointPerturbation / 10000.0m;
            NamedValueSet properties = GetPricingStructureId().Properties.Clone();
            properties.Set("PerturbedAmount", basisPointPerturbation);
            string uniqueId = GetPricingStructureId().UniqueIdentifier;
            int index = 0;
            //Perturb the base curve quote set
            if (BaseCurve.PriceableRateAssets != null && pricingStructureRiskSetType != PricingStructureRiskSetType.Child)
            {
                var basequotes = GetMarketQuotes(BaseCurve.PriceableRateAssets);
                foreach (var instrument in BaseCurve.PriceableRateAssets)
                {
                    var perturbations = new decimal[basequotes.Count];
                    basequotes.CopyTo(perturbations);
                    //Clone the properties.
                    NamedValueSet curveProperties = properties.Clone();
                    perturbations[index] = basequotes[index] + perturbation;
                    curveProperties.Set("PerturbedAsset", instrument.Id);
                    curveProperties.Set("BaseCurve", uniqueId);
                    curveProperties.Set(CurveProp.UniqueIdentifier, uniqueId + "." + instrument.Id);
                    curveProperties.Set(CurveProp.Tolerance, Tolerance);
                    //Perturb the quotes
                    PerturbedPriceableAssets(BaseCurve.PriceableRateAssets, perturbations);
                    //Create the new curve.
                    var baseCurve = new RateCurve(curveProperties, BaseCurve.PriceableRateAssets, Holder);
                    IPricingStructure rateCurve = new RateBasisCurve(curveProperties, baseCurve,
                                                                      PriceableRateSpreadAssets, Holder);
                    structures.Add(rateCurve);
                    //Set the counter.
                    perturbations[index] = 0;
                    index++;
                }
            }
            //Perturb the spread curve quotes
            if (pricingStructureRiskSetType != PricingStructureRiskSetType.Parent)
            {
                var spreadquotes = GetMarketQuotes(PriceableRateSpreadAssets);
                index = 0;
                foreach (var instrument in PriceableRateSpreadAssets)
                {
                    var perturbations = new decimal[spreadquotes.Count];
                    spreadquotes.CopyTo(perturbations);
                    //Clone the properties.
                    NamedValueSet curveProperties = properties.Clone();
                    perturbations[index] = spreadquotes[index] + perturbation;
                    curveProperties.Set("PerturbedAsset", instrument.Id);
                    curveProperties.Set("BaseCurve", uniqueId);
                    curveProperties.Set(CurveProp.UniqueIdentifier, uniqueId + "." + instrument.Id);
                    curveProperties.Set(CurveProp.Tolerance, Tolerance);
                    //Perturb the quotes
                    PerturbedPriceableAssets(new List<IPriceableRateAssetController>(PriceableRateSpreadAssets),
                                             perturbations);
                    IPricingStructure rateCurve = new RateBasisCurve(curveProperties, BaseCurve,
                                                                      PriceableRateSpreadAssets, Holder);
                    structures.Add(rateCurve);
                    //Set the counter.
                    perturbations[index] = 0;
                    index++;
                }
            }
            return structures;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public sealed override void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var pricingStructureId = (RateCurveIdentifier)PricingStructureIdentifier;
            //Order the assets.
            PriceableRateSpreadAssets = PriceableRateSpreadAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            var termCurve = SetConfigurationData();
            //Get the reference interpolated curve.
            termCurve.point = RateSpreadBootstrapper.Bootstrap(PriceableRateSpreadAssets,//TODO what about the interpoation.
                                                               BaseCurve,
                                                               pricingStructureId.BaseDate,
                                                               termCurve.extrapolationPermitted,
                                                               Tolerance);
            CreatePricingStructure(pricingStructureId, termCurve, PriceableAssetFactory.Parse(PriceableRateSpreadAssets));
            SetInterpolator(BaseCurve, pricingStructureId.PricingStructureType);
            // Set the Zero curve, just for reference
            YieldCurveValuation yieldCurveValuation = GetYieldCurveValuation();
            if (yieldCurveValuation.zeroCurve?.rateCurve == null)
            {
                //var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
                //var psType = PricingStructureIdentifier.PricingStructureType;
                TermCurve curve = YieldCurveAnalytics.ToZeroCurve(termCurve, GetBaseDate(), CompoundingFrequency, DayCounter);
                curve.interpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("CurveInterpolation"));
                var zeroCurve = new ZeroRateCurve
                {
                    rateCurve = curve,
                    compoundingFrequency = Reporting.V5r3.CompoundingFrequency.Create(CompoundingFrequency)
                };
                yieldCurveValuation.zeroCurve = zeroCurve;
            }
        }

        #endregion

        #region Configuration and Interpolation

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>
        /// The <c>double</c> function value at the point
        /// </returns>
        public override double Value(IPoint point)
        {
            return Interpolator.Value(point);        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseCurve"></param>
        /// <param name="psType"></param>
        protected void SetInterpolator(IRateCurve baseCurve, PricingStructureTypeEnum psType)
        {
            //var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
            var interpDayCounter = Actual365.Instance;
            // The underlying curve and associated compounding frequency (compounding frequency required when underlying curve is a ZeroCurve)
            CompoundingFrequency = EnumHelper.Parse<CompoundingFrequencyEnum>(Holder.GetValue("CompoundingFrequency"));
            // interpolate the DiscountFactor curve based on the respective curve interpolation 
            var discountFactorCurve = GetYieldCurveValuation().discountFactorCurve;
            Interpolator = new RateSpreadInterpolator(baseCurve, discountFactorCurve, GetBaseDate(), interpDayCounter);
        }

        #endregion
    }
}
