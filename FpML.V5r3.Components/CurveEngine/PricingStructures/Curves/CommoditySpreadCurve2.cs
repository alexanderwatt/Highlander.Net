#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using AssetClass = Orion.Constants.AssetClass;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    /// <summary>
    /// A rate spreadcurve.
    /// </summary>
    public class CommoditySpreadCurve2 : CommodityCurve, ISpreadCurve
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ICommodityCurve BaseCurve { get; set; }

        /// <summary>
        /// A reference rate curve.
        /// </summary>
        public IIdentifier ReferenceCurveId { get; set; }

        ///<summary>
        /// The set of controllers to calibrate to.
        ///</summary>
        public List<IPriceableCommoditySpreadAssetController> PriceableCommoditySpreadAssets { get; set; }

        #endregion

        #region Runtime Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        protected CommoditySpreadCurve2(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
            : base(properties, algorithmHolder)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Commodity, properties); 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommoditySpreadCurve2"/> class.
        /// </summary>CommoditySpreadCurve
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        protected CommoditySpreadCurve2(ILogger logger, ICoreCache cache, String nameSpace,
                                       PricingStructureIdentifier curveIdentifier)
            : base(logger, cache, nameSpace, curveIdentifier)
        {
            var properties = curveIdentifier.GetProperties();
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Commodity, properties); 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommoditySpreadCurve2"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="referenceCurve">The reference parent curveid.</param>
        /// <param name="spreadAssets">The spreads by asset.</param>
        /// <param name="properties">The properties of the new spread curve.</param>
        /// <param name="calendar">The calendar.</param>
        public CommoditySpreadCurve2(ILogger logger, ICoreCache cache, string nameSpace,
            ICommodityCurve referenceCurve, FxRateSet spreadAssets, NamedValueSet properties,
            IBusinessCalendar calendar)
            : base(logger, cache, nameSpace, ProcessQuotedAssetSet(logger, cache, nameSpace, referenceCurve, spreadAssets, properties, calendar), properties, calendar, calendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Commodity, properties); 
            BaseCurve = referenceCurve;
            ReferenceCurveId = BaseCurve.GetPricingStructureId();
            if (PricingStructureIdentifier.PricingStructureType != PricingStructureTypeEnum.CommoditySpreadCurve) return;
            //Set the spread sets.
            PriceableCommoditySpreadAssets = PriceableAssetFactory.CreatePriceableCommoditySpreadAssets(logger, cache, nameSpace, PricingStructureIdentifier.BaseDate, spreadAssets, calendar);
            Build(logger, cache, nameSpace, calendar, calendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommoditySpreadCurve2"/> class.
        /// </summary>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="spreadAssets">The spreads by asset.</param>
        /// <param name="properties">The properties of the new spread curve.</param>
        /// <param name="algorithm">The alogorithm holder. </param>
        public CommoditySpreadCurve2(NamedValueSet properties, ICommodityCurve referenceCurve, 
            List<IPriceableCommoditySpreadAssetController> spreadAssets, PricingStructureAlgorithmsHolder algorithm)
            : base(properties, algorithm)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Commodity, properties); 
            //Set the identifier.
            var pricingStructureId = GetCommodityCurveId();
            if (pricingStructureId.PricingStructureType != PricingStructureTypeEnum.CommoditySpreadCurve) return;
            //Set the reference curve
            BaseCurve = referenceCurve;
            ReferenceCurveId = BaseCurve.GetPricingStructureId();
            PriceableCommoditySpreadAssets = spreadAssets;
            //Order the assets.
            PriceableCommoditySpreadAssets = PriceableCommoditySpreadAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            var termCurve = SetConfigurationData();
            //Get the reference interpolated curve.
            IList<Double> xArray = new List<double>();
            IList<Double> yArray = new List<double>(); 
            termCurve.point = CommoditySpreadBootstrapper2.Bootstrap(PriceableCommoditySpreadAssets,
                                                                    BaseCurve,
                                                                    pricingStructureId.BaseDate,
                                                                    termCurve.extrapolationPermitted,
                                                                    Tolerance, ref xArray, ref yArray);
            CreatePricingStructure(pricingStructureId, termCurve, PriceableAssetFactory.Parse(PriceableCommoditySpreadAssets));
            SetInterpolator(BaseCurve, xArray, yArray, pricingStructureId.PricingStructureType);
        }

        #endregion

        #region Simplified Constuctors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommoditySpreadCurve2"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curveid.</param>
        /// <param name="values">The values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public CommoditySpreadCurve2(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, IPricingStructure refCurve, Decimal[] values,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, AdjustMarketQuotes(values, refCurve.GetFpMLData(), PropertyHelper.ExtractUniqueCurveIdentifier(properties)), properties,
            fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            ReferenceCurveId = refCurve.GetPricingStructureId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommoditySpreadCurve2"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curveid.</param>
        /// <param name="value">The values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public CommoditySpreadCurve2(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, IPricingStructure refCurve, Decimal value,
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
        /// Initializes a new instance of the <see cref="CommoditySpreadCurve2"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="spreadFpmlData">The FPML data.</param>
        /// <param name="properties">The properties</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public CommoditySpreadCurve2(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> spreadFpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, spreadFpmlData, properties, fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, properties); 
            var refCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
            ReferenceCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceCurveId = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommoditySpreadCurve2"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="referenceCurveData">The market data. This must contain both the underlying base curve and the spread curve.
        /// Otherwise the RateBasisInterpolator can not instantiate.</param>
        /// <param name="spreadCurveData">The spread Curve Data</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public CommoditySpreadCurve2(ILogger logger, ICoreCache cache, String nameSpace,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
        Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData, IBusinessCalendar rollCalendar)
            : base(spreadCurveData.Third, GenerateHolder(logger, cache, nameSpace, spreadCurveData.Third))
        {
            PricingStructureData = new PricingStructureData(CurveType.Child, AssetClass.Rates, spreadCurveData.Third); 
            //Set the identifier.
            var nvs = spreadCurveData.Third;
            var pricingStructureId = GetCommodityCurveId();
            var refCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(nvs);
            ReferenceCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceCurveId = null;
            if (pricingStructureId.PricingStructureType != PricingStructureTypeEnum.CommoditySpreadCurve) return;
            //Set the reference curve
            var baseCurveFpml = new Pair<PricingStructure, PricingStructureValuation>(referenceCurveData.First, referenceCurveData.Second);
            var baseCurveProps = referenceCurveData.Third;
            BaseCurve = (ICommodityCurve)PricingStructureFactory.Create(logger, cache, nameSpace, rollCalendar, rollCalendar, baseCurveFpml, baseCurveProps);
            //Get the spread Data
            var spreadCurveFpml = new Pair<PricingStructure, PricingStructureValuation>(spreadCurveData.First, spreadCurveData.Second);
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(nvs);
            var tempFpml = (FxCurveValuation)spreadCurveFpml.Second;
            var spreadAssets = tempFpml.spotRate;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.fxForwardCurve == null
                                  || tempFpml.fxForwardCurve.point == null
                                  || tempFpml.fxForwardCurve.point.Length == 0;
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quotedassetset in order to bootstrap.
                if (!XsdClassesFieldResolver.QuotedAssetSetIsValid(spreadAssets)) return;
                PriceableCommoditySpreadAssets =
                    PriceableAssetFactory.CreatePriceableCommoditySpreadAssets(logger, cache, nameSpace, pricingStructureId.BaseDate, spreadAssets, rollCalendar);
                Build(logger, cache, nameSpace, rollCalendar, rollCalendar);
            }
            else
            {
                // the discount curve is already built, so don't rebuild
                PriceableCommoditySpreadAssets = PriceableAssetFactory.CreatePriceableCommoditySpreadAssets(logger, cache, nameSpace, pricingStructureId.BaseDate, spreadAssets, rollCalendar);
                //Order the assets.
                PriceableCommoditySpreadAssets = PriceableCommoditySpreadAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
                CreatePricingStructure(pricingStructureId, tempFpml.fxForwardCurve, PriceableAssetFactory.Parse(PriceableCommoditySpreadAssets));
                //SetInterpolator(BaseCurve, pricingStructureId.PricingStructureType);
            }
        }

        #endregion

        #region Value and risk Functions

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
            var value = Interpolator.Value(point);
            //if (UnderlyingInterpolatedCurve == "ZeroCurve")
            //{
            //    if (DayCounter.DayCountConvention != DayCountConvention.Actual)
            //    {
            //        throw new NotImplementedException("ZeroCurve not implemented for day counts other than ACT/365 and ACT/360");
            //    }
            //    // It is assumed that point is on an ACT/365 basis
            //    var yearFraction = (double)point.Coords[0];
            //    yearFraction *= Actual365.Instance.Basis / DayCounter.Basis;
            //    value = RateAnalytics.ZeroRateToDiscountFactor(value, yearFraction, CompoundingFrequency);
            //}
            return value;
        }

        /// <summary>
        /// Gets the spread sjusted value.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        public double GetSpreadAdjustedValue(DateTime baseDate, DateTime targetDate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the basic rate curve risk set, using the current curve as the base curve.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point pertubation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of pertubed rate curves</returns>
        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        {
            return CreateCurveRiskSet(basisPointPerturbation, PricingStructureRiskSetType.Parent);
        }

        /// <summary>
        /// Creates the basic rate curve risk set, using the current curve as the base curve.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point pertubation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <param name="pricingStructureRiskSetType">This determins which assets to perturb. </param>
        /// <returns>A list of pertubed rate curves</returns>
        public List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation, PricingStructureRiskSetType pricingStructureRiskSetType)
        {
            var structures = new List<IPricingStructure>();
            if (PriceableCommoditySpreadAssets == null) return structures;
            //Add the spread asset perturbed curves.
            //
            //Set the parameters and properties.
            decimal perturbation = basisPointPerturbation / 10000.0m;
            NamedValueSet properties = GetPricingStructureId().Properties.Clone();
            properties.Set("PerturbedAmount", basisPointPerturbation);
            string uniqueId = GetPricingStructureId().UniqueIdentifier;
            int index = 0;
            //Perturb the base curve quote set
            if (BaseCurve.PriceableCommodityAssets != null && pricingStructureRiskSetType != PricingStructureRiskSetType.Child)
            {
                var basequotes = GetMarketQuotes(BaseCurve.PriceableCommodityAssets);
                foreach (var instrument in BaseCurve.PriceableCommodityAssets)
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
                    PerturbedPriceableAssets(BaseCurve.PriceableCommodityAssets, perturbations);
                    //Create the new curve.
                    var baseCurve = new CommodityCurve(curveProperties, BaseCurve.PriceableCommodityAssets, Holder);
                    IPricingStructure rateCurve = new CommoditySpreadCurve2(curveProperties, baseCurve,
                                                                      PriceableCommoditySpreadAssets, Holder);
                    structures.Add(rateCurve);
                    //Set the counter.
                    perturbations[index] = 0;
                    index++;
                }
            }
            //Perturb the spread curve quotess
            if (pricingStructureRiskSetType != PricingStructureRiskSetType.Parent)
            {
                var spreadquotes = GetMarketQuotes(PriceableCommoditySpreadAssets);
                index = 0;
                foreach (var instrument in PriceableCommoditySpreadAssets)
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
                    PerturbedPriceableAssets(new List<IPriceableCommodityAssetController>(PriceableCommoditySpreadAssets),
                                             perturbations);
                    IPricingStructure rateCurve = new CommoditySpreadCurve2(curveProperties, BaseCurve,
                                                                      PriceableCommoditySpreadAssets, Holder);
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
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var pricingStructureId = (CommodityCurveIdentifier)PricingStructureIdentifier;
            //Order the assets.
            PriceableCommoditySpreadAssets = PriceableCommoditySpreadAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            var termCurve = SetConfigurationData();
            //Get the reference interpolated curve.
            IList<Double> xArray = new List<double>();
            IList<Double> yArray = new List<double>(); 
            termCurve.point = CommoditySpreadBootstrapper2.Bootstrap(PriceableCommoditySpreadAssets,//TODO what about the interpoation.
                                                               BaseCurve,
                                                               pricingStructureId.BaseDate,
                                                               termCurve.extrapolationPermitted,
                                                               Tolerance, ref xArray, ref yArray);
            CreatePricingStructure(pricingStructureId, termCurve, PriceableAssetFactory.Parse(PriceableCommoditySpreadAssets));
            SetInterpolator(BaseCurve, xArray, yArray, pricingStructureId.PricingStructureType);//TOTO Modify with the actual spreads....
        }

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
        /// <param name="calendar"></param>
        /// <returns></returns>
        public static Pair<PricingStructure, PricingStructureValuation> ProcessQuotedAssetSet(ILogger logger, ICoreCache cache, 
            string nameSpace, ICommodityCurve referenceCurve,  FxRateSet spreadValues, NamedValueSet properties, IBusinessCalendar calendar)
        {
            var identifier = new CommodityCurveIdentifier(properties);
            //Clone the ref curves.
            //
            Pair<PricingStructure, PricingStructureValuation> fpml = CloneCurve(referenceCurve.GetFpMLData(), identifier.UniqueIdentifier);
            if (identifier.PricingStructureType != PricingStructureTypeEnum.CommoditySpreadCurve)
            {
                var ycvCurveCloned = (FxCurveValuation) fpml.Second;
                //  assign id to the cloned YieldCurve
                //
                ycvCurveCloned.fxForwardCurve.point = null;
                ycvCurveCloned.fxForwardCurve = null;
                //Manipulate the quated asset set.
                ycvCurveCloned.spotRate = MappedQuotedAssetSet(logger, cache, nameSpace, referenceCurve, spreadValues,
                                                               properties, calendar);
            }
            return fpml;
        }

        //Backs out the implied quotes for the asset provided and adds the spread to it.
        //
        private static FxRateSet MappedQuotedAssetSet(ILogger logger, ICoreCache cache, 
            string nameSpace, IInterpolatedSpace referenceCurve, FxRateSet spreadValues,
            NamedValueSet properties, IBusinessCalendar calendar)
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
                var priceableAsset = (PriceableCommodityAssetController)PriceableAssetFactory.Create(logger, cache, nameSpace, quote, namedValueSet, calendar, calendar);
                var value = priceableAsset.CalculateImpliedQuote(referenceCurve);
                //Replace the marketquote in the bav and remove the spread.
                var quotes = new List<BasicQuotation>(quote.quote);
                var impliedQuote = MarketQuoteHelper.ReplaceQuotationByMeasureType("MarketQuote", quotes, value);
                var marketQuote = new List<BasicQuotation>(impliedQuote);
                spreadValues.assetQuote[index].quote = MarketQuoteHelper.MarketQuoteRemoveSpreadAndNormalise(marketQuote);
                index++;
            }
            return spreadValues;
        }

        private static PricingStructureAlgorithmsHolder GenerateHolder(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties)
        {
            var pricingStructureId = new CommodityCurveIdentifier(properties);
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, pricingStructureId.PricingStructureType, pricingStructureId.Algorithm);
            return holder;
        }

        #endregion

        #region Configuration and Interpolation

        internal void SetSpreadInterpolator(TermCurve discountFactorCurve, string algorithm)
        {
            var interpDayCounter = Actual365.Instance;
            Interpolator = new TermCurveInterpolator(discountFactorCurve, GetBaseDate(), interpDayCounter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseCurve"></param>
        /// <param name="spreadXArray"></param>
        /// <param name="spreadYArray"></param>
        /// <param name="psType"></param>
        protected void SetInterpolator(ICommodityCurve baseCurve, IList<double> spreadXArray, IList<double> spreadYArray, PricingStructureTypeEnum psType)
        {
            //var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
            var interpDayCounter = Actual365.Instance;
            // interpolate the DiscountFactor curve based on the respective curve interpolation 
            if (psType == PricingStructureTypeEnum.CommoditySpreadCurve)
            {
                Interpolator = new CommoditySpreadInterpolator2(baseCurve, spreadXArray, spreadYArray, true);
            }
        }

        #endregion
    }
}
