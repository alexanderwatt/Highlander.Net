#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.Identifiers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Constants;
using Orion.CurveEngine.Assets.Helpers;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Analytics.Interpolations.Points;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Analytics.DayCounters;
using Orion.Util.Serialisation;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    ///<summary>
    ///</summary>
    public class CommodityCurve : CurveBase, ICommodityCurve
    {
        #region Properties

        private const string DefaultAlgorithName = "LinearForward";

        ///<summary>
        ///</summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// The fixing calendar.
        /// </summary>
        public IBusinessCalendar FixingCalendar { get; set; }

        /// <summary>
        /// The payment calendar.
        /// </summary>
        public IBusinessCalendar PaymentCalendar { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public InterpolationMethod InterpolationMethod = InterpolationMethodHelper.Parse("LinearInterpolation");

        ///<summary>
        ///</summary>
        public string UnderlyingInterpolatedCurve { get; private set; }

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        public List<IPriceableCommodityAssetController> PriceableCommodityAssets { get; set; }

        /// <summary>
        /// The bootstrapper name.
        /// </summary>
        public String BootstrapperName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ExtrapolationPermitted = true;

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override string GetAlgorithm()
        {
            return Algorithm;
        }

        /// <summary>
        /// 
        /// </summary>
        public double Tolerance = 0.00000000001;

        #endregion

        #region NameValueSet-based constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        public CommodityCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity, properties); 
            PricingStructureIdentifier = new CommodityCurveIdentifier(properties);
            Initialize(properties, algorithmHolder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// This constructor is used to clone perturbed copies of a base curve.
        /// </summary>
        /// <param name="priceableCommodityAssets">The priceable Commodity Assets.</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="curveProperties">The Curve Properties.</param>
        public CommodityCurve(NamedValueSet curveProperties, List<IPriceableCommodityAssetController> priceableCommodityAssets, 
            PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder)
            : this(curveProperties, pricingStructureAlgorithmsHolder)
        {
            var curveId = GetCommodityCurveId();
            var termCurve = SetConfigurationData();
            PriceableCommodityAssets = priceableCommodityAssets;
            termCurve.point = CommodityBootstrapper.Bootstrap(PriceableCommodityAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, PriceableCommodityAssets, null);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(curveId.BaseDate, Holder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="algorithmHolder"></param>
        protected void Initialize(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity, properties); 
            if (algorithmHolder != null)
            {
                Tolerance = PropertyHelper.ExtractDoubleProperty("Tolerance", properties) ??
                            GetDefaultTolerance(algorithmHolder);
                Holder = algorithmHolder;
            }
            else
            {
                Tolerance = 10 ^ -9;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        protected CommodityCurve(ILogger logger, ICoreCache cache, String nameSpace, PricingStructureIdentifier curveIdentifier)
            : base(logger, cache, nameSpace, curveIdentifier)
        {
            var properties = curveIdentifier.GetProperties();
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity, properties); 
            Tolerance = PropertyHelper.ExtractDoubleProperty("Tolerance", properties) ?? GetDefaultTolerance(Holder);
        }

        #endregion

        #region Normal Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommodityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="assetSet">The assetSet.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public CommodityCurve(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, FxRateSet assetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : this(logger, cache, nameSpace, properties, assetSet, fixingCalendar, rollCalendar, 0.0000001d)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommodityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="assetSet">The assetSet.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="tolerance">The tolerance for the solver.</param>
        public CommodityCurve(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, FxRateSet assetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, double tolerance)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity, properties);
            var curveId = new CommodityCurveIdentifier(properties);
            PricingStructureIdentifier = curveId;
            Holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, curveId.PricingStructureType, curveId.Algorithm);
            //  Get algo name from the properties
            //
            Algorithm = properties.GetValue<string>("Algorithm", true);
            InitialiseInstance(logger, cache, nameSpace, curveId, curveId.BaseDate, assetSet, fixingCalendar, rollCalendar, tolerance);
        }

        private void InitialiseInstance(ILogger logger, ICoreCache cache, string nameSpace, PricingStructureIdentifier curveId,
            DateTime baseDate, FxRateSet assetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, double tolerance)
        {
            FixingCalendar = fixingCalendar;
            PaymentCalendar = rollCalendar;
            // The bootstrapper to use
            //
            BootstrapperName = Holder.GetValue("Bootstrapper");
            var termCurve = new TermCurve
            {
                extrapolationPermitted =
                    bool.Parse(Holder.GetValue("ExtrapolationPermitted")),
                extrapolationPermittedSpecified = true,
                interpolationMethod =
                    InterpolationMethodHelper.Parse(
                    Holder.GetValue("BootstrapperInterpolation"))
            };
            PriceableCommodityAssets = PriceableAssetFactory.CreatePriceableCommodityAssets(logger, cache, nameSpace, curveId.BaseDate, assetSet, fixingCalendar, rollCalendar);
            termCurve.point = CommodityBootstrapper.Bootstrap(PriceableCommodityAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, tolerance);
            // Pull out the fx curve and fx curve valuation
            //
            var quotedAssetSet = PriceableCommodityAssets != null ? PriceableAssetFactory.Parse(PriceableCommodityAssets) : assetSet;
            var fpmlData = CreateFpMLPair(baseDate, curveId, quotedAssetSet, termCurve);
            //Bootstrapper = Bootstrap(bootstrapperName, fpmlData.First, fpmlData.Second);
            SetFpMLData(fpmlData);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            //
            SetInterpolator(baseDate, Holder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommodityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public CommodityCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, new CommodityCurveIdentifier(properties))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Commodity, properties);
            if (fpmlData == null) return;
            FixingCalendar = fixingCalendar;
            PaymentCalendar = rollCalendar;
            var tempFpML = (FxCurveValuation)fpmlData.Second;
            //  Use default algorithm name
            //
            Algorithm = DefaultAlgorithName;
            var termCurve = SetConfigurationData();
            //SetFpMLData(fpmlData);
            BootstrapperName = Holder.GetValue("Bootstrapper");
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(properties);
            //  If there's no forward points - do build the curve.
            //
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpML.fxForwardCurve == null
                || tempFpML.fxForwardCurve.point == null
                || tempFpML.fxForwardCurve.point.Length == 0;
            if (cache == null)
            {
                //optimize = true;
                bootstrap = false;
            }
            bool doBuild = (tempFpML.spotRate != null) && discountsAbsent;
            if (bootstrap || doBuild)
            {
                var qas = tempFpML.spotRate;
                PriceableCommodityAssets = PriceableAssetFactory.CreatePriceableCommodityAssets(logger, cache, nameSpace, tempFpML.baseDate.Value, qas, fixingCalendar, rollCalendar);
                var point = CommodityBootstrapper.Bootstrap(PriceableCommodityAssets, tempFpML.baseDate.Value, termCurve.extrapolationPermitted,
                                                                                                                   termCurve.interpolationMethod);
                ((FxCurveValuation) fpmlData.Second).fxForwardCurve.point = point;
            }
            SetInterpolator(PricingStructureValuation.baseDate.Value, Holder);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Sets the interpolator.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="holder">The holder.</param>
        private void SetInterpolator(DateTime baseDate, PricingStructureAlgorithmsHolder holder)
        {
            // The underlying curve and associated compounding frequency (compounding frequency required when underlying curve is a ZeroCurve)
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            var dayCounter = DayCounterHelper.Parse(holder.GetValue("DayCounter"));
            UnderlyingInterpolatedCurve = holder.GetValue("UnderlyingCurve"); //TODO this redundant.
            // Retrieve the Discount factor curve and assign the curve interpolation we want to initiate
            // This dependends on the underyling curve type (i.e. rate or discount factor)
            var termCurve = GetFxCurveValuation().fxForwardCurve;
            termCurve.interpolationMethod = curveInterpolationMethod;
            // interpolate the DiscountFactor curve based on the respective curve interpolation 
            Interpolator = new CommodityCurveInterpolator(termCurve, baseDate, dayCounter);
        }

        private double GetDefaultTolerance(PricingStructureAlgorithmsHolder algorithms)
        {
            string tolerance = algorithms.GetValue(AlgorithmsProp.Tolerance);
            return double.Parse(tolerance);
        }

        /// <summary>
        /// Creates the pricing structure with ADDITIONAL
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetSet">The FxRateSet.</param>
        /// <param name="termCurve">The bootstrapped term curve.</param>
        /// <returns></returns>
        private static Pair<PricingStructure, PricingStructureValuation> CreateFpMLPair(DateTime baseDate,
                                                                                        PricingStructureIdentifier curveId,
                                                                                        FxRateSet assetSet, 
                                                                                        TermCurve termCurve)
        {
            var commodityCurve = CreateCommodityCurve(curveId.Id, curveId.CurveName, curveId.Currency);
            var commodityCurveValuation = CreateCommodityCurveValuation(baseDate, assetSet, commodityCurve.id, termCurve);
            return new Pair<PricingStructure, PricingStructureValuation>(commodityCurve, commodityCurveValuation);
        }

        #endregion

        #region Calculations

        /// <summary>
        /// Creates a perturbed copy of the curve.
        /// </summary>
        /// <returns></returns>
        protected void PerturbedPriceableAssets(List<IPriceableCommodityAssetController> priceableCommodityAssets, Decimal[] values)
        {
            var numControllers = priceableCommodityAssets.Count;
            if ((values.Length != numControllers)) return;//(PriceableRateAssets == null) || 
            var index = 0;
            foreach (var rateController in priceableCommodityAssets)
            {
                rateController.MarketQuote.value = values[index];
                index++;
            }
        }

        protected List<decimal> GetMarketQuotes(IEnumerable<IPriceableCommodityAssetController> priceableRateAssets)
        {
            return priceableRateAssets.Select(asset => asset.MarketQuote.value).ToList();
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote()
        {
            return PriceableCommodityAssets != null ? EvaluateImpliedQuote(this, PriceableCommodityAssets.ToArray()) : null;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (PriceableCommodityAssets != null)
            {
                return EvaluateImpliedQuote(this, PriceableCommodityAssets.ToArray());
            }
            var curveValuation = GetFxCurveValuation();
            PriceableCommodityAssets = PriceableAssetFactory.CreatePriceableCommodityAssets(logger, cache, nameSpace, curveValuation.baseDate.Value, curveValuation.spotRate, fixingCalendar, rollCalendar);
            return EvaluateImpliedQuote(this, PriceableCommodityAssets.ToArray());
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="commodityCurve">The rate curve.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        public NamedValueSet EvaluateImpliedQuote(ICommodityCurve commodityCurve, IPriceableCommodityAssetController[] priceableAssets)
        {
            return EvaluateImpliedQuote(priceableAssets, commodityCurve);
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="commodityCurve"></param>
        public NamedValueSet EvaluateImpliedQuote(IPriceableCommodityAssetController[] priceableAssets,
                                                                  ICommodityCurve commodityCurve)
        {
            var result = new NamedValueSet();
            foreach (var priceableAsset in priceableAssets)
            {
                result.Set(priceableAsset.Id, priceableAsset.CalculateImpliedQuote(commodityCurve));
            }
            return result;
        }

        #endregion

        #region Build and Set

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        private void SetFpMLData(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
            try
            {
                if (null == PricingStructureIdentifier)
                {
                    PricingStructureIdentifier = new FxCurveIdentifier(PricingStructure.id);
                }

            }
// ReSharper disable EmptyGeneralCatchClause
            catch // b/c the FxCurveIdentifier ctor throws the exception 
// ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            PriceableCommodityAssets = PriceableAssetFactory.CreatePriceableCommodityAssets(logger, cache, nameSpace, GetFxCurveValuation().baseDate.Value, GetFxCurveValuation().spotRate, fixingCalendar, rollCalendar);
            var termCurve = ((FxCurveValuation)PricingStructureValuation).fxForwardCurve;
            termCurve.point = CommodityBootstrapper.Bootstrap(PriceableCommodityAssets, GetFxCurveValuation().baseDate.Value, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod);
            SetFpMLData(new Pair<PricingStructure, PricingStructureValuation>(PricingStructure, PricingStructureValuation));
            SetInterpolator(GetFxCurveValuation().baseDate.Value, Holder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commodityCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        public static Pair<PricingStructure, PricingStructureValuation> CloneCurve(Pair<PricingStructure, PricingStructureValuation> commodityCurve, string curveId)
        {
            PricingStructure ycCurveCloned = XmlSerializerHelper.Clone(commodityCurve.First);
            PricingStructureValuation ycvCurveCloned = XmlSerializerHelper.Clone(commodityCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var yc = (FpML.V5r10.Reporting.FxCurve)ycCurveCloned;
            yc.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var ycv = (FxCurveValuation)ycvCurveCloned;
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        #endregion

        #region Value

        /// <summary>
        /// DFs the value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public double ForwardIndex(DateTime baseDate, DateTime date)
        {
            IPoint point = new DateTimePoint1D(baseDate, date);
            return Interpolator.Value(point);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetForward(DateTime valuationDate, DateTime targetDate)
        {
            var value = 0d;
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.ForwardToSpot)
            {
                var point = new DateTimePoint1D(valuationDate, targetDate);
                value = Value(point);
                return value;
            }
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.SpotToForward)
            {
                var point1 = new DateTimePoint1D(GetBaseDate(), targetDate);
                var value1 = Value(point1);
                var point2 = new DateTimePoint1D(GetBaseDate(), valuationDate);
                var value2 = Value(point2);
                value = value1 / value2;
                return value;
            }
            return value;
        }

        public double GetForward(double targetTime)
        {
            var point = new Point1D(targetTime);
            var value = Value(point);
            return value;
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetForward(DateTime targetDate)
        {
            return GetForward(GetFxCurveValuation().baseDate.Value, targetDate);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetForward(DateTime[] targetDates)
        {
            return GetForward(GetFxCurveValuation().baseDate.Value, targetDates);
        }

        /// <summary>
        /// Get array of discount factors
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetForward(DateTime baseDate, DateTime[] targetDates)
        {
            return targetDates.Select(targetDate => GetForward(baseDate, targetDate)).ToArray();
        }    

        /// <summary>
        /// Gets the yield curve valuation.
        /// </summary>
        /// <returns></returns>
        public FxCurveValuation GetFxCurveValuation()
        {
            return (FxCurveValuation)PricingStructureValuation;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <returns></returns>
        public override IValue GetValue(IPoint pt)
        {
            return new DoubleValue("dummy", Value(pt), pt);
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override IList<IValue> GetClosestValues(IPoint pt)
        {
            GetDiscreteSpace().GetFunctionValueArray();

            var times = GetDiscreteSpace().GetCoordinateArray(1);
            var values = GetDiscreteSpace().GetFunctionValueArray();
            var index = Array.BinarySearch(times, pt.Coords[1]);
            if (index >= 0)
            {
                return null;
            }
            var nextIndex = ~index;
            var prevIndex = nextIndex - 1;

            //TODO check for DateTime1D point and return the date.
            var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));

            IList<IValue> result = new List<IValue> { prevValue, nextValue };

            return result;
        }

        #endregion

        #region Static Creators

        /// <summary>
        /// Creates the yield curve.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="name">The name.</param>
        /// <param name="currency">The currency.</param>
        /// <returns></returns>
        private static FpML.V5r10.Reporting.FxCurve CreateCommodityCurve(string curveId, string name, Currency currency)
        {
            //            var currencyPair = QuotedCurrencyPairHelper.Parse(currency.Value, currency.Value, QuoteBasisEnum.Currency2PerCurrency1);

            var commodityCurve = new FpML.V5r10.Reporting.FxCurve
            {
                id = curveId,
                name = name,
                currency = currency
                //                                  , quotedCurrencyPair = currencyPair
            };
            return commodityCurve;
        }

        /// <summary>
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="commodityRates">The spot rate.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="termCurve">The bootstrapped term curve.</param>
        /// <returns></returns>
        private static FxCurveValuation CreateCommodityCurveValuation(DateTime baseDate, 
                                                               FxRateSet commodityRates,
                                                               string curveId,
                                                                TermCurve termCurve)
        {
            var commodityCurveValuation = new FxCurveValuation
                                       {
                                           baseDate = IdentifiedDateHelper.Create(baseDate),
                                           buildDateTime = baseDate,
                                           buildDateTimeSpecified = true,
                                           spotRate = commodityRates,
                                           id = curveId,
                                           fxForwardCurve = termCurve 
                                       };
            return commodityCurveValuation;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public decimal GetSpotRate()
        {
            var fxVal = (FxCurveValuation)GetFpMLData().Second;

            var spotRateAsset = (from spotRateAssets in fxVal.spotRate.assetQuote
                                 where spotRateAssets.objectReference.href.EndsWith("-CommoditySpot-SP", StringComparison.InvariantCultureIgnoreCase)
                                 select spotRateAssets).Single();

            decimal spotRate = spotRateAsset.quote[0].value;

            return spotRate;
        }

        /// <summary>
        /// Gets the spot date relative to the date provided.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace"></param>
        /// <param name="baseDate"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <returns></returns>
        public DateTime GetSpotDate(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            //var bq = BasicQuotationHelper.Create(.79m, "MarketQuote"); no magic constants in code
            var bq = BasicQuotationHelper.Create(0, "MarketQuote");
            var bav = BasicAssetValuationHelper.Create(bq);
            //var quotedCurrencyPair = commodityCurveId.QuotedCurrencyPair;
            var quotedCurrencyPair = ((FpML.V5r10.Reporting.FxCurve)GetFpMLData().First).quotedCurrencyPair;
            var identifier = quotedCurrencyPair.currency1.Value + quotedCurrencyPair.currency2.Value + "-CommoditySpot-SP";
            var fxspot = new FxRateAsset
                             {
                                 id = identifier,
                                 currency = new IdentifiedCurrency { Value = quotedCurrencyPair.currency1.Value },
                                 quotedCurrencyPair = quotedCurrencyPair
                             };

            var priceableAsset = (IPriceableCommodityAssetController)PriceableAssetFactory.Create(logger, cache, nameSpace, fxspot.id, baseDate, bav, fixingCalendar, rollCalendar);
            var spot = priceableAsset.GetRiskMaturityDate();

            return spot;
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public override QuotedAssetSet GetQuotedAssetSet()
        {
            return GetFxCurveValuation().spotRate;
        }

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <returns>An array of term points</returns>
        public override TermCurve GetTermCurve()
        {
            return GetFxCurveValuation().fxForwardCurve;
        }

        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace"></param>
        /// <param name="values"></param>
        /// <param name="measureType"></param>
        /// <returns></returns>
        public override bool PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, Decimal[] values, String measureType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the basic rate curve risk set.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point pertubation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of pertubed rate curves</returns>
        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        {
            throw new NotImplementedException();
        }

        //Adds a spread to an asset. The spread value is added to each asset in the original curve.
        //The array of spread values must be the same length as the array of assets.
        // The curve is renamed with the id provided.
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spreadValues"></param>
        /// <param name="commodityCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal[] spreadValues,
                                                                                             Pair<PricingStructure, PricingStructureValuation> commodityCurve, string curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(commodityCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(commodityCurve.Second);
            var yc = (FpML.V5r10.Reporting.FxCurve)ycCurveCloned;
            yc.id = curveId;
            var ycv = (FxCurveValuation)ycvCurveCloned;
            ycv.fxForwardCurve.point = null;
            ycv.fxForwardPointsCurve = null;
            ycv.fxForwardCurve = null;
            //This strips out the basis swaps.
            //
            ycv.spotRate = AssetHelper.RemoveAssetsFromQuotedAssetSet(AssetTypesEnum.BasisSwap, ycv.spotRate);
            if (spreadValues.Length == ycv.spotRate.instrumentSet.Items.Length)
            {
                var index = 0;
                foreach (var bav in ycv.spotRate.assetQuote)
                {
                    bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                      new List<BasicQuotation>(bav.quote),
                                                                                      spreadValues[index]);
                    index++;
                }
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        //Adds a spread to an asset. The spread value is added to each asset in the original curve.
        // The curve is renamed with the id provided.
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spreadValue"></param>
        /// <param name="commodityCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal spreadValue,
                                                                                            Pair<PricingStructure, PricingStructureValuation> commodityCurve, string curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(commodityCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(commodityCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var yc = (FpML.V5r10.Reporting.FxCurve)ycCurveCloned;
            yc.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var ycv = (FxCurveValuation)ycvCurveCloned;
            ycv.fxForwardCurve.point = null;
            ycv.fxForwardPointsCurve = null;
            ycv.fxForwardCurve = null;
            foreach (var bav in ycv.spotRate.assetQuote)
            {
                bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                  new List<BasicQuotation>(bav.quote),
                                                                                  spreadValue);
            }

            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the rate curve id.
        /// </summary>
        /// <returns></returns>
        public CommodityCurveIdentifier GetCommodityCurveId()
        {
            return (CommodityCurveIdentifier)PricingStructureIdentifier;
        }

                /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(CommodityCurveIdentifier curveId, TermCurve termCurve, FxRateSet quotedAssetSet)
        {
            FpML.V5r10.Reporting.FxCurve yieldCurve = CreateCommodityCurve(curveId);
            FxCurveValuation yieldCurveValuation = CreateCommodiyCurveValuation(curveId, quotedAssetSet, yieldCurve.id, termCurve);
            var fpmlData = new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, yieldCurveValuation);
            SetFpMLData(fpmlData, false);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(CommodityCurveIdentifier curveId, TermCurve termCurve, IEnumerable<IPriceableCommodityAssetController> priceableCommodityAssets,
            IEnumerable<IPriceableCommoditySpreadAssetController> priceableCommoditySpreadAssets)
        {
            FxRateSet quotedAssetSet = priceableCommodityAssets != null ? PriceableAssetFactory.Parse(priceableCommodityAssets, priceableCommoditySpreadAssets) : null;
            CreatePricingStructure(curveId, termCurve, quotedAssetSet);
        }

        #endregion

        #region Other Stuff

        internal TermCurve SetConfigurationData()
        {
            // The bootstrapper to use
            if (Holder != null)
            {
                var bootstrapper = Holder.GetValue("Bootstrapper");
                if (bootstrapper != null)
                {
                    BootstrapperName = Holder.GetValue("Bootstrapper");
                }
                var extrapolationPermitted = Holder.GetValue("ExtrapolationPermitted");
                if (extrapolationPermitted != null)
                {
                    ExtrapolationPermitted = bool.Parse(extrapolationPermitted);
                }
                var interpolation = InterpolationMethodHelper.Parse(
                    Holder.GetValue("BootstrapperInterpolation"));
                if (interpolation != null)
                {
                    InterpolationMethod = interpolation;
                }
            }
            var termCurve = new TermCurve
            {
                extrapolationPermitted =
                    ExtrapolationPermitted,
                extrapolationPermittedSpecified = true,
                interpolationMethod = InterpolationMethod
            };
            return termCurve;
        }

        /// <summary>
        /// Creates the yield curve.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <returns></returns>
        protected static FpML.V5r10.Reporting.FxCurve CreateCommodityCurve(CommodityCurveIdentifier curveId)
        {
            var yieldCurve = new FpML.V5r10.Reporting.FxCurve
            {
                id = curveId.Id,
                name = curveId.CurveName,
                currency = curveId.Currency,
            };
            return yieldCurve;
        }

        /// <summary>
        /// Creates the yield curve valuation.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="quotedAssetSet">The quoted asset set.</param>
        /// <param name="commodityurveId">The commodity curve id.</param>
        /// <param name="forwardCurve">The curve</param>
        /// <returns></returns>
        protected static FxCurveValuation CreateCommodiyCurveValuation(PricingStructureIdentifier curveId, FxRateSet quotedAssetSet,
                                                                       string commodityurveId, TermCurve forwardCurve)
        {
            var yieldCurveValuation = new FxCurveValuation
            {
                baseDate = IdentifiedDateHelper.Create(curveId.BaseDate),
                buildDateTime = curveId.BuildDateTime,
                buildDateTimeSpecified = true,
                spotRate = quotedAssetSet,
                id = commodityurveId,
                definitionRef = curveId.PricingStructureType.ToString(),
                fxForwardCurve = forwardCurve
            };
            return yieldCurveValuation;
        }

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="includeId">Include an id check.</param>
        protected void SetFpMLData(Pair<PricingStructure, PricingStructureValuation> fpmlData, Boolean includeId)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
        }

        /// <summary>
        /// Gets the yield curve valuation.
        /// </summary>
        /// <returns></returns>
        public FxCurveValuation GetCommodityCurveValuation()
        {
            return (FxCurveValuation)PricingStructureValuation;
        }

        #endregion
    }
}
