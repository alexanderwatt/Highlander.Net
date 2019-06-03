#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Analytics.Interpolations.Points;
using Orion.Identifiers;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.Analytics.DayCounters;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    ///<summary>
    ///</summary>
    public class FxCurve : CurveBase, IFxCurve
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public double Tolerance { get; set; }

        ///<summary>
        ///</summary>
        public string Algorithm => ((FxCurveIdentifier) PricingStructureIdentifier).Algorithm;

        /// <summary>
        /// The spot date.
        /// </summary>
        public DateTime SpotDate { get; set; }

        ///<summary>
        ///</summary>
        public string UnderlyingInterpolatedCurve { get; private set; }

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        public List<IPriceableFxAssetController> PriceableFxAssets { get; set; }

        /// <summary>
        /// The bootstrapper name.
        /// </summary>
        public String BootstrapperName { get; set; }

        ///<summary>
        /// A flag that determines the currency convention for spot or forward valuations
        ///</summary>
        private bool Reciprical { get; set; }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override string GetAlgorithm()
        {
            return Algorithm;
        }

        ///<summary>
        /// Gets the QuotedCurrencyPair.
        ///</summary>
        ///<returns>QuotedCurrencyPair</returns>
        public QuotedCurrencyPair GetQuotedCurrencyPair()
        {
            var fxCurve = (FpML.V5r3.Reporting.FxCurve)PricingStructure;
            return fxCurve.quotedCurrencyPair;
        }

        #endregion

        #region NameValueSet-based constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FxCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="assetSet">The assetSet.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public FxCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, 
            FxRateSet assetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            InitialiseInstance(logger, cache, nameSpace, properties, assetSet, fixingCalendar, rollCalendar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        public FxCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Fx, properties); 
            PricingStructureIdentifier = new FxCurveIdentifier(properties);
            Initialize(properties, algorithmHolder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="algorithmHolder"></param>
        protected void Initialize(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            Tolerance = PropertyHelper.ExtractDoubleProperty("Tolerance", properties) ?? GetDefaultTolerance(algorithmHolder);
            Holder = algorithmHolder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// This constructor is used to clone perturbed copies of a base curve.
        /// </summary>
        /// <param name="priceableFxAssets">The priceableFxAssets.</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="curveProperties">The Curve Properties.</param>
        public FxCurve(NamedValueSet curveProperties, List<IPriceableFxAssetController> priceableFxAssets, 
            PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder)
            : this(curveProperties, pricingStructureAlgorithmsHolder)
        {
            var curveId = GetFxCurveId();
            var termCurve = SetConfigurationData();
            PriceableFxAssets = priceableFxAssets;
            termCurve.point = FxBootstrapper.Bootstrap(PriceableFxAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, PriceableFxAssets);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            DateTime baseDate = PricingStructureIdentifier.BaseDate;
            SetInterpolator(baseDate);
        }

        private void InitialiseInstance(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, FxRateSet assetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Fx, properties);
            var curveId = new FxCurveIdentifier(properties);
            PricingStructureIdentifier = curveId;
            Holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, curveId.PricingStructureType, curveId.Algorithm);
            DateTime baseDate = PricingStructureIdentifier.BaseDate;
            //Set the spot date;
            SpotDate = GetSpotDate(logger, cache, nameSpace, fixingCalendar, rollCalendar, baseDate, curveId.QuotedCurrencyPair);
            //TODO
            //FixingCalendar = null;
            //RollCalendar = null;
            // The bootstrapper to use
            BootstrapperName = Holder.GetValue("Bootstrapper");
            Tolerance = double.Parse(Holder.GetValue("Tolerance"));
            bool extrapolationPermitted = bool.Parse(Holder.GetValue("ExtrapolationPermitted"));
            InterpolationMethod interpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("BootstrapperInterpolation"));
            var termCurve = new TermCurve
            {
                extrapolationPermitted = extrapolationPermitted,
                extrapolationPermittedSpecified = true,
                interpolationMethod = interpolationMethod
            };
            PriceableFxAssets = PriceableAssetFactory.CreatePriceableFxAssets(logger, cache, nameSpace, baseDate, assetSet, fixingCalendar, rollCalendar);
            termCurve.point = FxBootstrapper.Bootstrap(PriceableFxAssets, baseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            // Pull out the fx curve and fx curve valuation
            Pair<PricingStructure, PricingStructureValuation> fpmlData
                = CreateFpmlPair(logger, cache, nameSpace, baseDate, (FxCurveIdentifier)PricingStructureIdentifier, assetSet, termCurve, fixingCalendar, rollCalendar);
            SetFpmlData(fpmlData);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(baseDate);
        }

        #endregion

        #region Runtime Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FxCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public FxCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : this(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FxCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="inverted">If true then spot and forward results are reciprated.</param>
        public FxCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, bool inverted)
            : base(logger, cache, nameSpace, fpmlData, new FxCurveIdentifier(properties))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Fx, properties); 
            if (properties == null)
            {
                properties = PricingStructurePropertyHelper.FxCurve(fpmlData);
            }  
            //Set the spot date;
            SpotDate = GetSpotDate(logger, cache, nameSpace, fixingCalendar, rollCalendar, PricingStructureValuation.baseDate.Value);
            //var curveId = new FxCurveIdentifier(properties);
            //PricingStructureIdentifier = curveId;
            //Holder = new PricingStructureAlgorithmsHolder(logger, cache, curveId.PricingStructureType, curveId.Algorithm);
            Reciprical = inverted;
            var fxCurveValuation = (FxCurveValuation)fpmlData.Second;
            // hack - set the base date - todo - is this the right place alex?
            PricingStructureValuation.baseDate = new IdentifiedDate { Value = PricingStructureIdentifier.BaseDate };
            SetFpmlData(fpmlData);
            TermCurve termCurve = SetConfigurationData();
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(properties);
            var buildFlag = (fxCurveValuation.spotRate != null) && (fxCurveValuation.fxForwardCurve == null || fxCurveValuation.fxForwardCurve.point == null);
            // If there's no forward points - do build the curve.
            // - TODO what happened the central bank dates
            if ((bootstrap || buildFlag) && cache!=null)
            {
                // if (BootstrapperName == "FastBootstrapper") //TODO
                PriceableFxAssets = PriceableAssetFactory.CreatePriceableFxAssets(logger, cache, nameSpace,
                    fxCurveValuation.baseDate.Value, fxCurveValuation.spotRate, fixingCalendar, rollCalendar);
                fxCurveValuation.fxForwardCurve = termCurve;
                fxCurveValuation.fxForwardCurve.point = FxBootstrapper.Bootstrap(
                    PriceableFxAssets,
                    fxCurveValuation.baseDate.Value,
                    fxCurveValuation.fxForwardCurve.extrapolationPermitted,
                    fxCurveValuation.fxForwardCurve.interpolationMethod);
            }
            ((FxCurveValuation)PricingStructureValuation).fxForwardCurve = fxCurveValuation.fxForwardCurve;
            SetInterpolator(PricingStructureValuation.baseDate.Value);
        }

        #endregion

        #region FpML and Configuration

        /// <summary>
        /// Sets the interpolator.
        /// </summary>
        private void SetInterpolator(DateTime baseDate)
        {
            // The underlying curve and associated compounding frequency (compounding frequency required when underlying curve is a ZeroCurve)
            InterpolationMethod curveInterpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("CurveInterpolation"));
            IDayCounter dayCounter = DayCounterHelper.Parse(Holder.GetValue("DayCounter"));
            UnderlyingInterpolatedCurve = Holder.GetValue("UnderlyingCurve"); 
            // Retrieve the Discount factor curve and assign the curve interpolation we want to initiate
            // This dependends on the underyling curve type (i.e. rate or discount factor)
            TermCurve termCurve = GetFxCurveValuation().fxForwardCurve;
            termCurve.interpolationMethod = curveInterpolationMethod;
            // interpolate the DiscountFactor curve based on the respective curve interpolation 
            Interpolator = new FxCurveInterpolator(termCurve, baseDate, dayCounter);
        }

        private static Pair<PricingStructure, PricingStructureValuation> CreateFpmlPair(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate,
            FxCurveIdentifier curveId, FxRateSet assetSet, TermCurve termCurve, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            FpML.V5r3.Reporting.FxCurve fxCurve = CreateFxCurve(curveId.Id, curveId.CurveName, curveId.QuotedCurrencyPair, curveId.Currency);
            FxCurveValuation fxCurveValuation = CreateFxCurveValuation(logger, cache, nameSpace, fixingCalendar, rollCalendar, baseDate, assetSet, fxCurve.id, termCurve, curveId.QuotedCurrencyPair);
            return new Pair<PricingStructure, PricingStructureValuation>(fxCurve, fxCurveValuation);
        }


        internal TermCurve SetConfigurationData()
        {
            // The bootstrapper to use
            BootstrapperName = Holder.GetValue("Bootstrapper");

            var termCurve = new TermCurve
            {
                extrapolationPermitted = bool.Parse(Holder.GetValue("ExtrapolationPermitted")),
                extrapolationPermittedSpecified = true,
                interpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("BootstrapperInterpolation"))
            };
            return termCurve;
        }

        //private string GetAlgorithmValue(string key)
        //{
        //    if (Holder == null)
        //    {
        //        Holder = new PricingStructureAlgorithmsHolder();
        //    }
        //    return Holder.GetValue(PricingStructureTypeEnum.FxCurve, Algorithm, key);
        //}

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        private void SetFpmlData(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
        }

        #endregion

        #region Evaluate

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote()
        {
            if (PriceableFxAssets != null)
            {
                return EvaluateImpliedQuote(this, PriceableFxAssets.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (PriceableFxAssets != null)
            {
                return EvaluateImpliedQuote(this, PriceableFxAssets.ToArray());
            }
            FxCurveValuation curveValuation = GetFxCurveValuation();
            PriceableFxAssets = PriceableAssetFactory.CreatePriceableFxAssets(logger, cache, nameSpace, curveValuation.baseDate.Value, curveValuation.spotRate, fixingCalendar, rollCalendar);
            return EvaluateImpliedQuote(this, PriceableFxAssets.ToArray());
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="fxCurve">The rate curve.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        public NamedValueSet EvaluateImpliedQuote(IFxCurve fxCurve, IPriceableFxAssetController[] priceableAssets)
        {
            return EvaluateImpliedQuote(priceableAssets, fxCurve);
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="fxCurve"></param>
        public NamedValueSet EvaluateImpliedQuote(IPriceableFxAssetController[] priceableAssets,
                                                                  IFxCurve fxCurve)
        {
            var result = new NamedValueSet();
            foreach (IPriceableFxAssetController priceableAsset in priceableAssets)
            {
                result.Set(priceableAsset.Id, priceableAsset.CalculateImpliedQuote(fxCurve));
            }
            return result;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            PriceableFxAssets = PriceableAssetFactory.CreatePriceableFxAssets(logger, cache, nameSpace,
                GetFxCurveValuation().baseDate.Value, GetFxCurveValuation().spotRate, fixingCalendar, rollCalendar);
            TermCurve termCurve = ((FxCurveValuation)PricingStructureValuation).fxForwardCurve;
            termCurve.point = FxBootstrapper.Bootstrap(PriceableFxAssets, GetFxCurveValuation().baseDate.Value, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod);
            SetFpmlData(new Pair<PricingStructure, PricingStructureValuation>(PricingStructure, PricingStructureValuation));
            SetInterpolator(GetFxCurveValuation().baseDate.Value);
        }

        #endregion

        #region Value and Forwards

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
            double value = 0d;

            if (PricingStructureEvolutionType == PricingStructureEvolutionType.ForwardToSpot)
            {
                var point = new DateTimePoint1D(valuationDate, targetDate);
                value = Value(point);
                return value;
            }
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.SpotToForward)
            {
                var point1 = new DateTimePoint1D(GetBaseDate(), targetDate);
                double value1 = Value(point1);
                var point2 = new DateTimePoint1D(GetBaseDate(), valuationDate);
                double value2 = Value(point2);
                value = value1 / value2;
                return value;
            }

            return value;
        }

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>The <c>double</c> function value at the point</returns>
        public override double Value(IPoint point)
        {
            double result = Recipricate(Interpolator.Value(point));
            return result;
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

        #endregion

        #region Helpers and Interface Properties

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
            double[] times = GetDiscreteSpace().GetCoordinateArray(1);
            double[] values = GetDiscreteSpace().GetFunctionValueArray();
            int index = Array.BinarySearch(times, pt.Coords[1]);
            if (index >= 0)
            {
                return null;
            }
            int nextIndex = ~index;
            int prevIndex = nextIndex - 1;
            //TODO check for DateTime1D point and return the date.
            var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));
            IList<IValue> result = new List<IValue> { prevValue, nextValue };
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="id">The curve id.</param>
        /// <param name="name">The name.</param>
        /// <param name="quotedCurrencyPair">The quoted currecny pair.</param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private static FpML.V5r3.Reporting.FxCurve CreateFxCurve(string id, string name, QuotedCurrencyPair quotedCurrencyPair,
                                                          Currency currency)
        {
            var fxCurve = new FpML.V5r3.Reporting.FxCurve
            {
                id = id,
                name = name,
                currency = currency,
                quotedCurrencyPair = quotedCurrencyPair
            };
            return fxCurve;
        }

        /// <summary>
        /// Creates the yield curve.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <returns></returns>
        protected static FpML.V5r3.Reporting.FxCurve CreateFxCurve(FxCurveIdentifier curveId)
        {
            var fxCurve = new FpML.V5r3.Reporting.FxCurve
            {
                id = curveId.Id,
                name = curveId.CurveName,
                currency = curveId.Currency,
                quotedCurrencyPair = curveId.QuotedCurrencyPair
            };
            return fxCurve;
        }

        /// <summary>
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="fxRates">The spot rate.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="termCurve">The bootstrapped term curve.</param>
        /// <param name="quotedCurrencyPair"></param>
        /// <returns></returns>
        private static FxCurveValuation CreateFxCurveValuation(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate, FxRateSet fxRates, string curveId,
                                                               TermCurve termCurve, QuotedCurrencyPair quotedCurrencyPair)
        {
            DateTime spotDate = GetSpotDate(logger, cache, nameSpace, fixingCalendar, rollCalendar, baseDate, quotedCurrencyPair);
            var fxCurveValuation = new FxCurveValuation
                                       {
                                           baseDate = IdentifiedDateHelper.Create(baseDate),
                                           buildDateTime = baseDate,
                                           buildDateTimeSpecified = true,
                                           spotRate = fxRates,
                                           id = curveId,
                                           fxForwardCurve = termCurve,
                                           spotDate = IdentifiedDateHelper.Create("SpotDate", spotDate)
                                       };
            return fxCurveValuation;
        }

        /// <summary>
        /// </summary>
        /// <param name="fxRates">The spot rate.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="termCurve">The bootstrapped term curve.</param>
        /// <returns></returns>
        private FxCurveValuation CreateFxCurveValuation(FxCurveIdentifier curveId, FxRateSet fxRates, 
                                                               TermCurve termCurve)
        {
            var fxCurveValuation = new FxCurveValuation
            {
                baseDate = IdentifiedDateHelper.Create(curveId.BaseDate),
                buildDateTime = curveId.BaseDate,
                buildDateTimeSpecified = true,
                spotRate = fxRates,
                id = curveId.UniqueIdentifier,
                fxForwardCurve = termCurve,
                spotDate = IdentifiedDateHelper.Create("SpotDate", SpotDate),
            };
            return fxCurveValuation;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public decimal GetSpotRate()
        {
            var fxVal = (FxCurveValuation)GetFpMLData().Second;
            BasicAssetValuation spotRateAsset = (from spotRateAssets in fxVal.spotRate.assetQuote
                                                 where spotRateAssets.objectReference.href.EndsWith("-FxSpot-SP", StringComparison.InvariantCultureIgnoreCase)
                                                 select spotRateAssets).Single();
            decimal spotRate = spotRateAsset.quote[0].value;        
            return Recipricate(spotRate);
        }

        private decimal Recipricate(decimal value)
        {
            if (value == 0)
            {
                throw new ArgumentException("FX Value cannot be zero");
            }
            return Reciprical ? 1/value : value;
        }

        private double Recipricate(double value)
        {
            if (value == 0)
            {
                throw new ArgumentException("FX Value cannot be zero");
            }
            return Reciprical ? 1/value : value;
        }

        /// <summary>
        /// Gets the spot date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetSpotDate()
        {
            return SpotDate;
        }

        /// <summary>
        /// Gets the spot date.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public DateTime GetSpotDate(DateTime baseDate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the spot date relative to the date provided.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public DateTime GetSpotDate(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate)
        {
            return GetSpotDate(logger, cache, nameSpace, fixingCalendar, rollCalendar, baseDate, GetQuotedCurrencyPair());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <param name="baseDate"></param>
        /// <param name="quotedCurrencyPair"></param>
        /// <returns></returns>
        protected static DateTime GetSpotDate(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate, QuotedCurrencyPair quotedCurrencyPair)
        {
            BasicAssetValuation bav = BasicAssetValuationHelper.Create(BasicQuotationHelper.Create(0, "MarketQuote"));
            string identifier = quotedCurrencyPair.currency1.Value + quotedCurrencyPair.currency2.Value + "-FxSpot-SP";
            var priceableAsset = (IPriceableFxAssetController)PriceableAssetFactory.Create(logger, cache, nameSpace, identifier, baseDate, bav, fixingCalendar, rollCalendar);
            return priceableAsset.GetRiskMaturityDate();
        }

        ///// <summary>
        ///// Generates a derived rate curve from the fx curve by applying the forwards to a reference rate curve.
        ///// No functionality to move the base date currntly available.
        ///// </summary>
        ///// <param name="referenceCurve">The reference Curve</param>
        ///// <param name="isReferenceCurveCurrency2">The isReferenceCurveCurrency2 flag.</param>
        ///// <param name="derivedCurveProperties">The derivedCurve Properties. BaseDate is currently ignored.</param>
        ///// <returns></returns>
        //public IRateCurve GenerateRateCurve(IRateCurve referenceCurve, bool isReferenceCurveCurrency2, NamedValueSet derivedCurveProperties)
        //{
        //    throw new NotImplementedException();
            ////1. Copy across the reference curve quoted asset set.
            ////2. Create the derived curve id using the new properties.
            ////3. Check whether the ref curve is the baseCurve. If yes apply one way conversion
            ////Otherwise do the inverse.
            ////4. Set the discount factors and zero rates.

            //IRateCurve derivedCurve = null;

            //if (referenceCurve != null)
            //{
            //    TermCurve fxTermCurve = GetTermCurve();
            //    DateTime baseDate = referenceCurve.GetBaseDate();
            //    double baseDateValue = GetForward(baseDate);
            //    var quoteBasis = GetQuotedCurrencyPair().quoteBasis;
            //    var points = new List<TermPoint> { TermPointFactory.Create(1.0m, baseDate) };

            //    //TODO need to cover the case where it isn't Currency2PerCurrency1
            //    if ((isReferenceCurveCurrency2 && quoteBasis == QuoteBasisEnum.Currency2PerCurrency1) || (!isReferenceCurveCurrency2 && quoteBasis == QuoteBasisEnum.Currency1PerCurrency2))
            //    {
            //        points.AddRange(from point in fxTermCurve.point
            //                        where point.term.Items[0].GetType() == typeof(DateTime)
            //                        let date = (DateTime)point.term.Items[0]
            //                        where date > baseDate
            //                        let df = referenceCurve.GetDiscountFactor(date)
            //                        let derivedDf = (double)point.mid / baseDateValue * df
            //                        select TermPointFactory.Create((Decimal)derivedDf, date));
            //    }
            //    else
            //    {
            //        points.AddRange(from point in fxTermCurve.point
            //                        where point.term.Items[0].GetType() == typeof(DateTime)
            //                        let date = (DateTime)point.term.Items[0]
            //                        where date > baseDate
            //                        let df = referenceCurve.GetDiscountFactor(date)
            //                        let derivedDf = (double)point.mid / baseDateValue / df
            //                        select TermPointFactory.Create((Decimal)derivedDf, date));
            //    }

            //    derivedCurve = DerivedCurveHelper(derivedCurveProperties, referenceCurve, points.ToArray());

            //}
            //return derivedCurve;
        //}

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(FxCurveIdentifier curveId, TermCurve termCurve, IEnumerable<IPriceableFxAssetController> priceableFxAssets)
        {
            FxRateSet quotedAssetSet = priceableFxAssets != null ? PriceableAssetFactory.Parse(priceableFxAssets) : null;
            CreatePricingStructure(curveId, termCurve, quotedAssetSet);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(FxCurveIdentifier curveId, TermCurve termCurve, FxRateSet quotedAssetSet)
        {
            FpML.V5r3.Reporting.FxCurve fxCurve = CreateFxCurve(curveId);
            FxCurveValuation fxCurveValuation = CreateFxCurveValuation(curveId, quotedAssetSet, termCurve);
            var fpmlData = new Pair<PricingStructure, PricingStructureValuation>(fxCurve, fxCurveValuation);
            SetFpMLData(fpmlData, false);
        }

        /// <summary>
        /// Generates a derived rate curve from the fx curve by applying the forwards to a reference rate curve.
        /// No functionality to move the base date currntly available.
        /// </summary>
        /// <param name="referenceCurve">The reference Curve</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="isReferenceCurveCurrency2">The isReferenceCurveCurrency2 flag.</param>
        /// <param name="derivedCurveProperties">The derivedCurve Properties. BaseDate is currently ignored.</param>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <returns></returns>
        public IRateCurve GenerateRateCurve(ILogger logger, ICoreCache cache, 
            string nameSpace, IRateCurve referenceCurve, bool isReferenceCurveCurrency2, 
            NamedValueSet derivedCurveProperties, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            //1. Copy across the reference curve quoted asset set.
            //2. Create the derived curve id using the new properties.
            //3. Check whether the ref curve is the baseCurve. If yes apply one way conversion
            //Otherwise do the inverse.
            //4. Set the discount factors and zero rates.
            IRateCurve derivedCurve = null;
            if(referenceCurve != null)
            {
                TermCurve fxTermCurve = GetTermCurve();
                DateTime baseDate = referenceCurve.GetBaseDate();
                double baseDateValue = GetForward(baseDate);
                var quoteBasis = GetQuotedCurrencyPair().quoteBasis;
                var points = new List<TermPoint> {TermPointFactory.Create(1.0m, baseDate)}; 
                //This is for debugging only
                var tempDf = new List<double>();
                var tempderivedDf = new List<double>();
                if ((isReferenceCurveCurrency2 && quoteBasis == QuoteBasisEnum.Currency1PerCurrency2))
                {
                    var range = from point in fxTermCurve.point
                                where point.term.Items[0] is DateTime
                                let date = (DateTime)point.term.Items[0]
                                where date > baseDate
                                let df = referenceCurve.GetDiscountFactor(date)
                                let derivedDf = (double)point.mid / baseDateValue * df
                                select TermPointFactory.Create((Decimal)derivedDf, date);
                    points.AddRange(range);
                }
                if ((isReferenceCurveCurrency2 && quoteBasis == QuoteBasisEnum.Currency2PerCurrency1) || (!isReferenceCurveCurrency2 && quoteBasis == QuoteBasisEnum.Currency1PerCurrency2))
                {
                    var range = from point in fxTermCurve.point
                                where point.term.Items[0] is DateTime
                                let date = (DateTime) point.term.Items[0]
                                where date > baseDate
                                let df = referenceCurve.GetDiscountFactor(date)
                                let derivedDf = (double) point.mid/baseDateValue*df
                                select TermPointFactory.Create((Decimal) derivedDf, date);
                    points.AddRange(range);
                    //Debugging only.
                    foreach (var point in fxTermCurve.point)
                    {
                        var date = (DateTime) point.term.Items[0];
                        var df = referenceCurve.GetDiscountFactor(date);
                        tempDf.Add(df);
                        tempderivedDf.Add((double)point.mid / baseDateValue * df);
                    }
                }
                //else
                //{
                //    var range = from point in fxTermCurve.point
                //                where point.term.Items[0] is DateTime
                //                let date = (DateTime) point.term.Items[0]
                //                where date > baseDate
                //                let df = referenceCurve.GetDiscountFactor(date)
                //                let derivedDf = (double) point.mid/baseDateValue/df
                //                select TermPointFactory.Create((Decimal) derivedDf, date);
                //    points.AddRange(range);
                //}
                //TODO
                //At this point the points are zero rates.
                //They need to be converted into discount factors.
                derivedCurve = DerivedCurveHelper(logger, cache, nameSpace, derivedCurveProperties, referenceCurve, points.ToArray(), fixingCalendar, rollCalendar);        
            }
            return derivedCurve;
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
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="values"></param>
        /// <param name="measureType"></param>
        /// <returns></returns>
        public override Boolean PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, Decimal[] values, String measureType)
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
        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)//TODO make proportional perturbations!
        {
            if (PriceableFxAssets == null) return null;
            //Set the parameters and properties.
            decimal perturbation = basisPointPerturbation / 10000.0m;
            NamedValueSet properties = GetPricingStructureId().Properties.Clone();
            properties.Set("PerturbedAmount", basisPointPerturbation);
            string uniqueId = GetPricingStructureId().UniqueIdentifier;
            //Get the assets, BUT REMOVE ALL THE BASIS SWAPS, to prevent double accounting of risks.
            int index = 0;
            var structures = new List<IPricingStructure>();
            //Get the original quotes
            var quotes = GetMarketQuotes(PriceableFxAssets);
            foreach (var instrument in PriceableFxAssets)
            {
                var perturbations = new decimal[quotes.Count];
                quotes.CopyTo(perturbations);
                //Clone the properties.
                NamedValueSet curveProperties = properties.Clone();
                perturbations[index] = quotes[index] + perturbation;
                curveProperties.Set("PerturbedAsset", instrument.Id);
                curveProperties.Set("BaseCurve", uniqueId);
                curveProperties.Set(CurveProp.UniqueIdentifier, uniqueId + "." + instrument.Id);
                curveProperties.Set(CurveProp.Tolerance, Tolerance);
                //Create the new curve.
                //Perturb the quotes
                PerturbedPriceableAssets(PriceableFxAssets, perturbations);
                IPricingStructure fxCurve = new FxCurve(curveProperties, PriceableFxAssets, Holder);
                structures.Add(fxCurve);
                //Set the counter.
                perturbations[index] = 0;
                index++;
            }
            return structures;
        }

        private IRateCurve DerivedCurveHelper(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet curveProperties, IRateCurve baseCurve, TermPoint[] discountFactorPoints, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            //Clone curve
            //
            //var cloneCurve = RateCurve.CloneCurve(baseCurve);
            //Modify the base fpml data.
            //TODO
            //We only allo discount factor curves at this time
            var id = new RateCurveIdentifier(curveProperties);
            Pair<PricingStructure, PricingStructureValuation> fpmlData = RateCurve.CloneCurve(baseCurve.GetFpMLData(), id.UniqueIdentifier);
            if (((YieldCurveValuation)fpmlData.Second).discountFactorCurve!=null)
            {
                ((YieldCurveValuation)fpmlData.Second).discountFactorCurve.point = discountFactorPoints;
            }
            if(((YieldCurveValuation) fpmlData.Second).zeroCurve!=null)
            {
                ((YieldCurveValuation) fpmlData.Second).zeroCurve.rateCurve = null;
            }
            //Add the fxcurve qas
            var qas = ((YieldCurveValuation) fpmlData.Second).inputs;
            //The basic set of assets
            var jointAssetQuotes = qas.assetQuote.ToList();
            //The new fx assets
            var fxAssets = GetQuotedAssetSet().assetQuote;
            jointAssetQuotes.AddRange(fxAssets);
            //The new exended instrument set;
            var tempInstruments = qas.instrumentSet.Items.ToList();
            var tempItemnames = qas.instrumentSet.ItemsElementName.ToList();
            tempInstruments.AddRange(GetQuotedAssetSet().instrumentSet.Items);
            tempItemnames.AddRange(GetQuotedAssetSet().instrumentSet.ItemsElementName);
            qas.assetQuote = jointAssetQuotes.ToArray();
            qas.instrumentSet = new InstrumentSet { Items = tempInstruments.ToArray(), ItemsElementName = tempItemnames.ToArray() };
            //Create the new curve.
            //TODO need to create a class that includes fx forwards.
            var derivedCurve = new RateCurve(logger, cache, nameSpace, fpmlData, curveProperties, fixingCalendar, rollCalendar, false);
            //TODO Add the fxcurve quotedassetset.
            return derivedCurve;
        }

        /// <summary>
        /// Clones the curve.
        /// </summary>
        /// <returns></returns>
        public object Clone()//THis does not instantiate any priceable assets or bootstrap the curve. This means no evaluation of component assets.
        {
            //Clone the properties and set values.
            //
            NamedValueSet properties = GetProperties().Clone();
            string baseCurveId = properties.GetString(CurveProp.UniqueIdentifier, true);
            string id = baseCurveId + ".Clone";
            properties.Set(CurveProp.UniqueIdentifier, id);
            //Clone the curve data.
            //
            Pair<PricingStructure, PricingStructureValuation> fpml = GetFpMLData();
            var fxCurve = new FxCurve(null, null, null, CloneCurve(fpml, id), properties, null, null, false);
            return fxCurve;
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
            if (includeId)
            {
                DateTime baseDate = PricingStructureValuation.baseDate.Value;
                PricingStructureIdentifier = new FxCurveIdentifier(PricingStructureTypeEnum.FxCurve, PricingStructure.id, baseDate);
            }
        }

        /// <summary>
        /// Gets the rate curve id.
        /// </summary>
        /// <returns></returns>
        public FxCurveIdentifier GetFxCurveId()
        {
            return (FxCurveIdentifier)PricingStructureIdentifier;
        }

        private double GetDefaultTolerance(PricingStructureAlgorithmsHolder algorithms)
        {
            string tolerance = algorithms.GetValue(AlgorithmsProp.Tolerance);
            return double.Parse(tolerance);
        }

        private static Pair<PricingStructure, PricingStructureValuation> CloneCurve(Pair<PricingStructure, PricingStructureValuation> fxCurve, string curveId)
        {
            var fxCurveCloned = XmlSerializerHelper.Clone(fxCurve.First);
            var fxvCurveCloned = XmlSerializerHelper.Clone(fxCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var fx = (FpML.V5r3.Reporting.FxCurve)fxCurveCloned;
            fx.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var fxv = (FxCurveValuation)fxvCurveCloned;
            //Dont want to null ther dfs
            //
            //fxv.fxForwardCurve.point = null;
            return new Pair<PricingStructure, PricingStructureValuation>(fx, fxv);
        }

        //Clones a curve, maps the quoted assets specified and then returns an FpML structure back.
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="referenceCurve"></param>
        /// <param name="spreadValues"></param>
        /// <param name="id"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public static Pair<PricingStructure, PricingStructureValuation> ProcessQuotedAssetSet(ILogger logger, ICoreCache cache, 
            string nameSpace, IFxCurve referenceCurve,
            FxRateSet spreadValues, string id, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate)
        {
            //Clone the ref curves.
            //
            var clonedCurve = referenceCurve.Clone();
            var fpml = ((IFxCurve)clonedCurve).GetFpMLData();
            var fxCurveCloned = (FpML.V5r3.Reporting.FxCurve)fpml.First;
            var fxvCurveCloned = (FxCurveValuation)fpml.Second;
            //  assign id to the cloned YieldCurve
            //
            fxCurveCloned.id = id;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            fxvCurveCloned.fxForwardCurve.point = null;
            fxvCurveCloned.fxForwardPointsCurve = null;
            //Manipulate the quated asset set.
            //
            fxvCurveCloned.spotRate = MappedQuotedAssetSet(logger, cache, nameSpace, referenceCurve, spreadValues, fixingCalendar, rollCalendar, baseDate);
            return fpml;
        }


        //Backs out the implied quotes for the asset provided and adds the spread to it.
        //
        private static FxRateSet MappedQuotedAssetSet(ILogger logger, ICoreCache cache, 
            string nameSpace, IInterpolatedSpace referenceCurve, FxRateSet spreadValues,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate)
        {
            var index = 0;
            //Find the backed out implied quote for each asset.
            //
            foreach (var asset in spreadValues.instrumentSet.Items)//TODO aDd in the spread.
            {
                NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, asset.id, baseDate);
                var quote = spreadValues.assetQuote[index];
                //Get the implied quote to use as the input market quote. Make sure it is rate controller.
                var priceableAsset = (PriceableFxAssetController)PriceableAssetFactory.Create(logger, cache, nameSpace, quote, namedValueSet, fixingCalendar, rollCalendar);
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, decimal> GetInputs()
        {
            Pair<PricingStructure, PricingStructureValuation> fpml = GetFpMLData();
            var fxCurve = fpml.Second as FxCurveValuation;
            if (fxCurve == null)
            {
                throw new NotImplementedException("Invalid Fpml for FX Curve");
            }
            BasicAssetValuation[] inputs = fxCurve.spotRate.assetQuote;
            Asset[] instrumentSet = fxCurve.spotRate.instrumentSet.Items;
            IEnumerable<KeyValuePair<string, Decimal>> results
                        = from a in instrumentSet
                          join i in inputs on a.id equals i.objectReference.href
                          where i.quote[0].measureType.Value == "MarketQuote"
                          select new KeyValuePair<string, Decimal>(a.id, i.quote[0].value);

            IDictionary<string, Decimal> data = results.ToDictionary(i => i.Key, i => i.Value);
            return data;
        }

        internal bool ValidateCurrencyMatch(QuotedCurrencyPair outputQuote, out bool isReversed)
        {
            var currencyPair = GetQuotedCurrencyPair();
            bool result = false;
            isReversed = false;
            if (outputQuote.currency1.Value == currencyPair.currency1.Value 
                && outputQuote.currency2.Value == currencyPair.currency2.Value)
            {
                result = true;
            }
            if (outputQuote.currency2.Value == currencyPair.currency1.Value
                && outputQuote.currency1.Value == currencyPair.currency2.Value)
            {
                result = true;
                isReversed = true;
            }
            return result;
        }

        /// <summary>
        /// Creates a perturbed copy of the curve.
        /// </summary>
        /// <returns></returns>
        protected void PerturbedPriceableAssets(List<IPriceableFxAssetController> priceableFxAssets, Decimal[] values)
        {
            var numControllers = priceableFxAssets.Count;
            if ((values.Length != numControllers)) return;//(PriceableRateAssets == null) || 
            var index = 0;
            foreach (var rateController in priceableFxAssets)
            {
                rateController.MarketQuote.value = values[index];
                index++;
            }
        }

        protected List<decimal> GetMarketQuotes(IEnumerable<IPriceableFxAssetController> priceableFxAssets)
        {
            return priceableFxAssets.Select(asset => asset.MarketQuote.value).ToList();
        }

        #endregion
    }
}
