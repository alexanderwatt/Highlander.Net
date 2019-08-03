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
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Rates;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    /// <summary>
    /// A rate spread curve.
    /// </summary>
    public class RateSpreadCurve : RateCurve, ISpreadCurve
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public IRateCurve BaseCurve { get; set; }

        /// <summary>
        /// A reference rate curve.
        /// </summary>
        public IIdentifier ReferenceCurveId { get; set; }

        ///<summary>
        /// The set of controllers to calibrate to.
        ///</summary>
        public List<IPriceableRateSpreadAssetController> PriceableRateSpreadAssets { get; set; }

        #endregion

        #region Runtime Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        protected RateSpreadCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
            : base(properties, algorithmHolder)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        protected RateSpreadCurve(ILogger logger, ICoreCache cache, String nameSpace,
                                  PricingStructureIdentifier curveIdentifier)
            : base(logger, cache, nameSpace, curveIdentifier)
        {
            var properties = curveIdentifier.GetProperties();
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="referenceCurve">The reference parent curve id.</param>
        /// <param name="spreadAssets">The spreads by asset.</param>
        /// <param name="properties">The properties of the new spread curve.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateSpreadCurve(ILogger logger, ICoreCache cache, string nameSpace, 
            IRateCurve referenceCurve, QuotedAssetSet spreadAssets, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, ProcessQuotedAssetSet(logger, cache, nameSpace, referenceCurve, spreadAssets, properties, fixingCalendar, rollCalendar), properties, fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            BaseCurve = referenceCurve;
            ReferenceCurveId = BaseCurve.GetPricingStructureId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="spreadAssets">The spreads by asset.</param>
        /// <param name="properties">The properties of the new spread curve.</param>
        /// <param name="algorithm">The algorithm holder. </param>
        public RateSpreadCurve(NamedValueSet properties, IRateCurve referenceCurve, 
            List<IPriceableRateSpreadAssetController> spreadAssets, PricingStructureAlgorithmsHolder algorithm)
            : base(properties, algorithm)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties);
            PriceableSpreadAssets = spreadAssets;
            BaseCurve = referenceCurve;
        }

        #endregion

        #region Simplified Constuctors

        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curve id.</param>
        /// <param name="values">The values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateSpreadCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, IPricingStructure refCurve, Decimal[] values,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, AdjustMarketQuotes(values, refCurve.GetFpMLData(), PropertyHelper.ExtractUniqueCurveIdentifier(properties)), properties,
            fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            ReferenceCurveId = refCurve.GetPricingStructureId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curve id.</param>
        /// <param name="spreadValueSet">The spread ValueSet.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateSpreadCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, IPricingStructure refCurve, QuotedAssetSet spreadValueSet,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, AdjustMarketQuotes(spreadValueSet, refCurve.GetFpMLData(), PropertyHelper.ExtractUniqueCurveIdentifier(properties)), properties,
            fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties);
            ReferenceCurveId = refCurve.GetPricingStructureId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curve id.</param>
        /// <param name="value">The values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateSpreadCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, IPricingStructure refCurve, Decimal value,
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
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="spreadFpmlData">The FPML data.</param>
        /// <param name="properties">The properties</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateSpreadCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> spreadFpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, spreadFpmlData, properties, fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            var refCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
            ReferenceCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceCurveId = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="spreadCurveData">The spread Curve Data</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateSpreadCurve(ILogger logger, ICoreCache cache, String nameSpace,
        Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData, 
        IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(spreadCurveData.Third, GenerateHolder(logger, cache, nameSpace, spreadCurveData.Third))
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, spreadCurveData.Third);
            FixingCalendar = fixingCalendar;
            PaymentCalendar = rollCalendar;
            //Set the identifier.
            var nvs = spreadCurveData.Third;
            var refCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(nvs);
            ReferenceCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceCurveId = null;
        }

        #endregion

        #region Value and risk Functions

        /// <summary>
        /// Gets the spread adjusted value.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        public double GetSpreadAdjustedValue(DateTime baseDate, DateTime targetDate)
        {
            return GetDiscountFactor(baseDate, targetDate);
        }

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
                    IPricingStructure rateCurve = new RateSpreadCurve(curveProperties, baseCurve,
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
                    IPricingStructure rateCurve = new RateSpreadCurve(curveProperties, BaseCurve,
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

        //Clones a curve, maps the quoted assets specified and then returns an FpML structure back.
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="referenceCurve"></param>
        /// <param name="spreadValues"></param>
        /// <param name="properties"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <returns></returns>
        public static Pair<PricingStructure, PricingStructureValuation> ProcessQuotedAssetSet(ILogger logger, ICoreCache cache, 
            string nameSpace, IRateCurve referenceCurve,
            QuotedAssetSet spreadValues, NamedValueSet properties, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var pricingStructureId = new RateCurveIdentifier(properties);
            Pair<PricingStructure, PricingStructureValuation> fpml = null;
            if (pricingStructureId.PricingStructureType != PricingStructureTypeEnum.RateBasisCurve)
            {
                //Clone the ref curves.
                //
                fpml = CloneCurve(referenceCurve.GetFpMLData(), pricingStructureId.UniqueIdentifier);
                //var ycCurveCloned = (YieldCurve)fpml.First;
                var ycvCurveCloned = (YieldCurveValuation)fpml.Second;
                //  assign id to the cloned YieldCurve
                //
                ycvCurveCloned.discountFactorCurve.point = null;
                ycvCurveCloned.zeroCurve = null;
                ycvCurveCloned.forwardCurve = null;
                //Manipulate the quoted asset set.
                //
                ycvCurveCloned.inputs = MappedQuotedAssetSet(logger, cache, nameSpace, referenceCurve, spreadValues, properties, fixingCalendar, rollCalendar);
            }
            return fpml;
        }

        //Backs out the implied quotes for the asset provided and adds the spread to it.
        //
        protected static QuotedAssetSet MappedQuotedAssetSet(ILogger logger, ICoreCache cache, 
            string nameSpace, IInterpolatedSpace referenceCurve, QuotedAssetSet spreadValues,
            NamedValueSet properties, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var index = 0;
            var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate);
            //Find the backed out implied quote for each asset.
            //
            foreach (var asset in spreadValues.instrumentSet.Items)
            {
                NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, asset.id, baseDate);
                var quote = spreadValues.assetQuote[index];
                //Get the implied quote to use as the input market quote. Make sure it is rate controller.
                var priceableAsset = (PriceableRateAssetController)PriceableAssetFactory.Create(logger, cache, nameSpace, quote, namedValueSet, fixingCalendar, rollCalendar);
                var value = priceableAsset.CalculateImpliedQuote(referenceCurve);
                //Replace the market quote in the bav and remove the spread.
                var quotes = new List<BasicQuotation>(quote.quote);
                var impliedQuote = MarketQuoteHelper.ReplaceQuotationByMeasureType("MarketQuote", quotes, value);
                var marketQuote = new List<BasicQuotation>(impliedQuote);
                spreadValues.assetQuote[index].quote = MarketQuoteHelper.MarketQuoteRemoveSpreadAndNormalise(marketQuote);
                index++;
            }
            return spreadValues;
        }

        protected static PricingStructureAlgorithmsHolder GenerateHolder(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties)
        {
            var pricingStructureId = new RateCurveIdentifier(properties);
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, pricingStructureId.PricingStructureType, pricingStructureId.Algorithm);
            return holder;
        }

        #endregion

        #region Configuration and Interpolation

        //TODO this is not yet implemented. FIX IT!
        internal void SetSpreadInterpolator(TermCurve discountFactorCurve, string algorithm, PricingStructureTypeEnum psType)
        {
            var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
            // The underlying curve and associated compounding frequency (compounding frequency required when underlying curve is a ZeroCurve)
            CompoundingFrequency = EnumHelper.Parse<CompoundingFrequencyEnum>(Holder.GetValue( "CompoundingFrequency"));
            UnderlyingInterpolatedCurve = Holder.GetValue("UnderlyingCurve"); //TODO this redundant.
            var interpDayCounter = Actual365.Instance;
            var underlyingCurve = ParseUnderlyingCurve(UnderlyingInterpolatedCurve);
            // interpolate the DiscountFactor curve based on the respective curve interpolation 
            if (underlyingCurve != UnderlyingCurveTypes.ZeroCurve)
            {
                Interpolator = new TermCurveInterpolator(discountFactorCurve, GetBaseDate(), interpDayCounter);
            }
            // otherwise interpolate our underlying curve will be a zero curve
            else
            {
                var zeroCurve = new ZeroRateCurve
                {
                    rateCurve = YieldCurveAnalytics.ToZeroCurve(discountFactorCurve,
                                                                GetBaseDate(),
                                                                CompoundingFrequency, DayCounter)
                };
                zeroCurve.rateCurve.interpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("CurveInterpolation"));
                zeroCurve.compoundingFrequency = FpML.V5r3.Reporting.CompoundingFrequency.Create(CompoundingFrequency);
                GetYieldCurveValuation().zeroCurve = zeroCurve;
                if (curveId.Algorithm == "SpreadInterpolation")
                {

                }
                else
                {
                    Interpolator =
                        new TermCurveInterpolator(GetYieldCurveValuation().zeroCurve.rateCurve, GetBaseDate(), interpDayCounter);
                }//TODO this is where to add the spread stuff.
            }
        }

        #endregion
    }
}
