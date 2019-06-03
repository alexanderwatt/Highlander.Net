#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Assets.Helpers;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
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
    /// The advanced ratecurve class.
    /// </summary>
    public class ExchangeTradedCurve : CurveBase, IExchangeTradedCurve
    {
        #region Properties

        /// <summary>
        /// The fixing calendar.
        /// </summary>
        public IBusinessCalendar SettlementCalendar { get; set; }

        /// <summary>
        /// The payment calendar.
        /// </summary>
        public IBusinessCalendar PaymentCalendar { get; set; }

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        public List<IPriceableFuturesAssetController> PriceableExchangeAssets { get; set; }

        /// <summary>
        /// The bootstrapper name.
        /// </summary>
        public String BootstrapperName = "FastBootstrapper";

        /// <summary>
        /// 
        /// </summary>
        public double Tolerance = 0.00000000001;

        /// <summary>
        /// 
        /// </summary>
        public bool ExtrapolationPermitted = true;

        /// <summary>
        /// 
        /// </summary>
        public InterpolationMethod InterpolationMethod = InterpolationMethodHelper.Parse("LinearRateInterpolation");

        /// <summary>
        /// The date array.
        /// </summary>
        public DateTime[] Dates
        {
            get
            {
                var pointCurve = GetTermCurve().point;
                return pointCurve.Select(point => (DateTime)point.term.Items[0]).ToArray();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets and sets members of the quoted asset set. Mainly for use the market data server.
        /// </summary>
        protected QuotedAssetSet QuotedAssetSet
        {
            get => GetQuotedAssetSet();
            set => ((YieldCurveValuation)PricingStructureValuation).inputs = value;
        }

        #endregion

        #region NameValueSet-based constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        public ExchangeTradedCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            PricingStructureIdentifier = new ExchangeCurveIdentifier(properties);
            Initialize(properties, algorithmHolder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="algorithmHolder"></param>
        protected void Initialize(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
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
        protected ExchangeTradedCurve(ILogger logger, ICoreCache cache, String nameSpace, PricingStructureIdentifier curveIdentifier)
            : base(logger, cache, nameSpace, curveIdentifier)
        {
            var properties = curveIdentifier.GetProperties();
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            Tolerance = PropertyHelper.ExtractDoubleProperty("Tolerance", properties) ?? GetDefaultTolerance(Holder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        /// <param name="instrumentData">The instrument data.</param>
        /// <param name="settlementCalendar">The settlementCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        [Description("public interface")]
        public ExchangeTradedCurve(ILogger logger, ICoreCache cache, string nameSpace,
            ExchangeCurveIdentifier curveIdentifier, QuotedAssetSet instrumentData, 
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
            : this(logger, cache, nameSpace, curveIdentifier)
        {
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            var termCurve = SetConfigurationData();
            PriceableExchangeAssets = PriceableAssetFactory.CreatePriceableFuturesAssets(logger, cache, nameSpace, curveIdentifier.BaseDate, instrumentData, settlementCalendar, rollCalendar);
            termCurve.point = SimpleExchangeBootstrapper.Bootstrap(PriceableExchangeAssets, curveIdentifier.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveIdentifier, termCurve, PriceableExchangeAssets);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(termCurve, curveIdentifier.Algorithm, curveIdentifier.PricingStructureType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instrumentData">The instrument data.</param>
        /// <param name="settlementCalendar">The settlementCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        [Description("public interface")]
        public ExchangeTradedCurve(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, QuotedAssetSet instrumentData, 
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
            : this(logger, cache, nameSpace, new ExchangeCurveIdentifier(properties))
        {
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            var curveId = GetExchangeCurveId();
            var termCurve = SetConfigurationData();
            PriceableExchangeAssets = PriceableAssetFactory.CreatePriceableFuturesAssets(logger, cache, nameSpace, curveId.BaseDate, instrumentData, settlementCalendar, rollCalendar);
            termCurve.point = SimpleExchangeBootstrapper.Bootstrap(PriceableExchangeAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, PriceableExchangeAssets);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// This constructor is used to clone perturbed copies of a base curve.
        /// </summary>
        /// <param name="priceableRateAssets">The priceableRateAssets.</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="curveProperties">The Curve Properties.</param>
        public ExchangeTradedCurve(NamedValueSet curveProperties, List<IPriceableFuturesAssetController> priceableRateAssets, 
            PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder)
            : this(curveProperties, pricingStructureAlgorithmsHolder)
        {
            var curveId = GetExchangeCurveId();
            var termCurve = SetConfigurationData();
            PriceableExchangeAssets = priceableRateAssets;
            termCurve.point = SimpleExchangeBootstrapper.Bootstrap(PriceableExchangeAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, PriceableExchangeAssets);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);
        }

        #endregion

        #region Runtime Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing strucuture.</param>
        /// <param name="settlementCalendar">The settlementCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        public ExchangeTradedCurve(String nameSpace, PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
            : base(null, null, nameSpace, fpmlData, new ExchangeCurveIdentifier(properties ?? PricingStructurePropertyHelper.RateCurve(fpmlData)))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            var curveId = GetExchangeCurveId();
            Initialize(properties, pricingStructureAlgorithmsHolder);
            if (fpmlData == null) return;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            // the discount curve is already built, so don't rebuild
            SetFpMLData(fpmlData, false);
            SetInterpolator(((YieldCurveValuation)PricingStructureValuation).discountFactorCurve, curveId.Algorithm, curveId.PricingStructureType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing strucuture.</param>
        /// <param name="settlementCalendar">The settlementCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="buildAssets">This is a flag which allows no assets to be built. Mainly for dervived rate curve from fx curve. </param>
        public ExchangeTradedCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar, bool buildAssets)
            : base(logger, cache, nameSpace, fpmlData, new ExchangeCurveIdentifier(properties ?? PricingStructurePropertyHelper.RateCurve(fpmlData)))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties);
            var curveId = GetExchangeCurveId();
            Initialize(properties, Holder);
            if (fpmlData == null) return;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);//TODO removed optimisation as it means that partial hedges can not be undertaken.
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(properties);
            var tempFpml = (YieldCurveValuation)fpmlData.Second;
            var termCurve = SetConfigurationData();
            var qas = tempFpml.inputs;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.discountFactorCurve?.point == null || tempFpml.discountFactorCurve.point.Length == 0;
            //This is an overrice if the cache is null, as the bootstrapper will not work.
            if (cache == null)
            {
                //optimize = true;
                bootstrap = false;
            }
            //This ensures only building if the asset flag is true.
            bootstrap = bootstrap && buildAssets;
            bool validAssets = XsdClassesFieldResolver.QuotedAssetSetIsValid(qas);
            //Test to see if a bootstrap is required.
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quotedassetset in order to bootstrap.
                if (!validAssets) return;
                PriceableExchangeAssets = PriceableAssetFactory.CreatePriceableFuturesAssets(logger, cache, nameSpace, curveId.BaseDate, qas, settlementCalendar, rollCalendar);
                termCurve.point = SimpleExchangeBootstrapper.Bootstrap(PriceableExchangeAssets, curveId.BaseDate,
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
                if (validAssets && buildAssets)//!optimize && 
                {
                    PriceableExchangeAssets = PriceableAssetFactory.CreatePriceableFuturesAssets(logger, cache, nameSpace, curveId.BaseDate, qas, settlementCalendar, rollCalendar);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing strucuture.</param>
        /// <param name="settlementCalendar">The fixingCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        public ExchangeTradedCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, new ExchangeCurveIdentifier(properties ?? PricingStructurePropertyHelper.RateCurve(fpmlData)))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            var curveId = GetExchangeCurveId();
            Initialize(properties, Holder);
            if (fpmlData == null) return;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);//TODO removed optimisation as it means that partial hedges can not be undertaken.
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(properties);           
            var tempFpml = (YieldCurveValuation)fpmlData.Second;          
            var termCurve = SetConfigurationData();
            var qas = tempFpml.inputs;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.discountFactorCurve?.point == null || tempFpml.discountFactorCurve.point.Length == 0;
            //This is an overrice if the cache is null, as the bootstrapper will not work.
            if (cache == null)
            {
                //optimize = true;
                bootstrap = false;
            }
            bool validAssets = XsdClassesFieldResolver.QuotedAssetSetIsValid(qas);
            //Test to see if a bootstrap is required.
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quotedassetset in order to bootstrap.
                if (!validAssets) return;
                PriceableExchangeAssets = PriceableAssetFactory.CreatePriceableFuturesAssets(logger, cache, nameSpace, curveId.BaseDate, qas, settlementCalendar, rollCalendar);
                termCurve.point = SimpleExchangeBootstrapper.Bootstrap(PriceableExchangeAssets, curveId.BaseDate,
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
                    PriceableExchangeAssets = PriceableAssetFactory.CreatePriceableFuturesAssets(logger, cache, nameSpace, curveId.BaseDate, qas, settlementCalendar, rollCalendar);
                }
            }
        }

        #endregion

        #region Helpers

        internal TermCurve SetConfigurationData()
        {
            //GetRateCurveId();
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
                interpolationMethod =InterpolationMethod
            };
            return termCurve;
        }

        private double GetDefaultTolerance(PricingStructureAlgorithmsHolder algorithms)
        {
            string tolerance = algorithms.GetValue(AlgorithmsProp.Tolerance);
            return double.Parse(tolerance);
        }

        private IDayCounter _dayCounter;

        public IDayCounter DayCounter
        {
            get
            {
                if (_dayCounter == null)
                {
                    _dayCounter = DayCounterHelper.Parse("ACT/365.FIXED");
                    if (Holder != null)
                    {
                        string dayCountBasis = Holder.GetValue("DayCounter");
                        _dayCounter = DayCounterHelper.Parse(dayCountBasis);
                    }
                }
                return _dayCounter;
            }
        }

        internal void SetInterpolator(TermCurve yieldToMaturityCurve, string algorithm, PricingStructureTypeEnum psType)
        {
            IDayCounter interpolateDayCounter = Actual365.Instance;
            Interpolator = new TermCurveInterpolator(yieldToMaturityCurve, GetBaseDate(), interpolateDayCounter);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(ExchangeCurveIdentifier curveId, TermCurve termCurve, QuotedAssetSet quotedAssetSet)
        {
            YieldCurve yieldCurve = CreateYieldCurve(curveId);
            YieldCurveValuation yieldCurveValuation = CreateYieldCurveValuation(curveId, quotedAssetSet, yieldCurve.id, termCurve);
            var fpmlData = new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, yieldCurveValuation);
            SetFpMLData(fpmlData, false);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(ExchangeCurveIdentifier curveId, TermCurve termCurve, IEnumerable<IPriceableFuturesAssetController> priceableExchangeAssets)
        {
            QuotedAssetSet quotedAssetSet = priceableExchangeAssets != null ? PriceableAssetFactory.Parse(priceableExchangeAssets) : null;
            CreatePricingStructure(curveId, termCurve, quotedAssetSet);
        }

        #endregion

        #region Evaluations

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote()
        {
            if (PriceableExchangeAssets != null)
            {
                return EvaluateImpliedQuote(this, PriceableExchangeAssets.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (PriceableExchangeAssets != null)//Bootstrapper
            {
                return EvaluateImpliedQuote(this, PriceableExchangeAssets.ToArray());
            }
            var curveValuation = GetYieldCurveValuation();
            PriceableExchangeAssets = PriceableAssetFactory.CreatePriceableFuturesAssets(logger, cache, nameSpace, curveValuation.baseDate.Value, curveValuation.inputs, fixingCalendar, rollCalendar);
            return EvaluateImpliedQuote(this, PriceableExchangeAssets.ToArray());

        }

        /// <summary>
        /// Evaluates the implied quotes.
        /// </summary>
        /// <param name="exchangeCurve">The exchange curve.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        public NamedValueSet EvaluateImpliedQuote(IExchangeTradedCurve exchangeCurve, IPriceableFuturesAssetController[] priceableAssets)
        {
            return EvaluateImpliedQuote(priceableAssets, exchangeCurve);
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="exchangeCurve">The exchange Curve</param>
        public NamedValueSet EvaluateImpliedQuote(IPriceableFuturesAssetController[] priceableAssets,
            IExchangeTradedCurve exchangeCurve)
        {
            var result = new NamedValueSet();
            foreach (var priceableAsset in priceableAssets)
            {
                result.Set(priceableAsset.Id, priceableAsset.CalculateImpliedQuote(exchangeCurve));
                //var marketQuote = MarketQuoteHelper.NormalisePriceUnits(priceableAsset.MarketQuote, "DecimalRate").value;
            }
            return result;
        }

        #endregion

        #region FpML

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
                PricingStructureIdentifier = new ExchangeCurveIdentifier(PricingStructure.id, baseDate);
            }
        }

        #endregion

        #region Forward Rate Methods

        /// <summary>
        /// Gets the forward.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetForward(DateTime valuationDate, DateTime targetDate)
        {
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.ForwardToSpot)
            {
                var point = new DateTimePoint1D(valuationDate, targetDate);
                return Value(point);
            }
            var value1 = new DateTimePoint1D(GetBaseDate(), targetDate);
            var value2 = new DateTimePoint1D(valuationDate, valuationDate);
            return Value(value1) / Value(value2);
        }


        /// <summary>
        /// Gets the forward.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public double GetForward(double time)
        {
            var point = new Point1D(time);
            var value = Value(point);
            return value;
        }

        /// <summary>
        /// Gets the forward.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetForward(DateTime targetDate)
        {
            return GetForward(GetBaseDate(), targetDate);
        }

        /// <summary>
        /// Gets the forward.
        /// </summary>
        /// <param name="valuationDate">THe valuation Date</param>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetForward(DateTime valuationDate, DateTime[] targetDates)
        {
            return targetDates.Select(targetDate => GetForward(valuationDate, targetDate)).ToArray();
        }

        /// <summary>
        /// Gets the forward.
        /// </summary>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetForward(DateTime[] targetDates)
        {
            return GetForward(GetBaseDate(), targetDates);
        }

        #endregion

        #region Yield Curve Properties

        /// <summary>
        /// Gets the yield curve valuation.
        /// </summary>
        /// <returns></returns>
        public YieldCurveValuation GetYieldCurveValuation()
        {
            return (YieldCurveValuation)PricingStructureValuation;
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public override QuotedAssetSet GetQuotedAssetSet()
        {
            return GetYieldCurveValuation().inputs;
        }

        /// <summary>
        /// Gets the quoted assets.
        /// </summary>
        /// <returns></returns>
        public Asset[] GetAssets()
        {
            return GetYieldCurveValuation().inputs.instrumentSet.Items;
        }

        /// <summary>
        /// Gets the days and zero rates.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="compounding">The compounding.</param>
        /// <returns></returns>
        public IDictionary<int, double> GetDaysAndZeroRates(DateTime baseDate, string compounding)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <returns>An array of term points</returns>
        public override TermCurve GetTermCurve()
        {
            return GetYieldCurveValuation().discountFactorCurve;
        }

        public override bool PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, decimal[] values, string measureType)
        {
            throw new NotImplementedException();
        }

        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the quoted asset item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Asset GetQuotedAsset(int item)
        {
            return GetQuotedAssetSet().instrumentSet.Items[item];
        }

        /// <summary>
        /// Gets the quoted asset valuation for a specific asset.
        /// </summary>
        /// <param name="item">The item to return. Must be zero based</param>
        /// <param name="measureType">The measureType of the quotation required.</param>
        /// <returns></returns>
        public Decimal GetAssetQuotation(int item, String measureType)
        {
            var numQuotes = GetQuotedAssetSet().assetQuote.Length;
            var result = 0m;
            if (item < numQuotes)
            {
                var quotes = GetQuotedAssetSet().assetQuote[item].quote;
                var quote = MarketQuoteHelper.FindQuotationByMeasureType(measureType, new List<BasicQuotation>(quotes));
                if (quote != null)
                {
                    result = quote.value;
                }
            }
            return result;
        }

        #endregion

        #region Build and Clone

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="settlementCalendar">The settlementCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
        {
            PriceableExchangeAssets = PriceableAssetFactory.CreatePriceableFuturesAssets(logger, cache, nameSpace, GetYieldCurveValuation().baseDate.Value, GetYieldCurveValuation().inputs, settlementCalendar, rollCalendar);
            GetYieldCurveValuation().zeroCurve = null;
            TermCurve termCurve = GetTermCurve();
            DateTime baseDate = GetYieldCurveValuation().baseDate.Value;
            termCurve.point = SimpleExchangeBootstrapper.Bootstrap(PriceableExchangeAssets, baseDate, termCurve.extrapolationPermitted, termCurve.interpolationMethod, Tolerance);
            SetFpMLData(new Pair<PricingStructure, PricingStructureValuation>(PricingStructure, PricingStructureValuation), false);
            SetInterpolator(termCurve, ((ExchangeCurveIdentifier)PricingStructureIdentifier).Algorithm, ((ExchangeCurveIdentifier)PricingStructureIdentifier).PricingStructureType);
        }

        /// <summary>
        /// Clones the curve.
        /// </summary>
        /// <returns></returns>
        public object Clone()//TODO there may be a problem with this. The df curve is set to null.
        {
            //Clone the properties ande set values.
            //
            NamedValueSet properties = GetProperties().Clone();
            var baseCurveId = properties.GetString(CurveProp.UniqueIdentifier, true);
            var id = baseCurveId + ".Clone";
            properties.Set(CurveProp.UniqueIdentifier, id);
            //Clone the curve data.
            //
            var fpml = GetFpMLData();
            var rateCurve = new ExchangeTradedCurve(NameSpace, Holder, CloneCurve(fpml, id), properties, null, null);
            return rateCurve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        public static Pair<PricingStructure, PricingStructureValuation> CloneCurve(Pair<PricingStructure, PricingStructureValuation> bondCurve, string curveId)
        {
            PricingStructure ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            PricingStructureValuation ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            //Dont want to null ther dfs
            //
            //ycv.discountFactorCurve.point = null;
            //ycv.zeroCurve = null;
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        #endregion

        #region IPricingStructure

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
            return value;
        }

        /// <summary>
        /// Gets the rate curve id.
        /// </summary>
        /// <returns></returns>
        public ExchangeCurveIdentifier GetExchangeCurveId()
        {
            return (ExchangeCurveIdentifier)PricingStructureIdentifier;
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
            var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));
            IList<IValue> result = new List<IValue> { prevValue, nextValue };
            return result;
        }

        /// <summary>
        /// Gets the curve algorithm.
        /// </summary>
        /// <returns></returns>
        public override string GetAlgorithm()
        {
            return ((BondCurveIdentifier)PricingStructureIdentifier).Algorithm;
        }

        #endregion

        #region Other Stuff

        /// <summary>
        /// Creates the yield curve.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <returns></returns>
        protected static YieldCurve CreateYieldCurve(ExchangeCurveIdentifier curveId)
        {
            var yieldCurve = new YieldCurve
            {
                id = curveId.Id,
                name = curveId.CurveName,
                algorithm = curveId.Algorithm,
                currency = curveId.Currency,
            };
            return yieldCurve;
        }

        /// <summary>
        /// Creates the yield curve valuation.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="quotedAssetSet">The quoted asset set.</param>
        /// <param name="yieldCurveId">The yield curve id.</param>
        /// <param name="discountFactorCurve"></param>
        /// <returns></returns>
        protected static YieldCurveValuation CreateYieldCurveValuation(PricingStructureIdentifier curveId, QuotedAssetSet quotedAssetSet,
                                                                       string yieldCurveId, TermCurve discountFactorCurve)
        {
            var yieldCurveValuation = new YieldCurveValuation
            {
                baseDate = IdentifiedDateHelper.Create(curveId.BaseDate),
                buildDateTime = curveId.BuildDateTime,
                buildDateTimeSpecified = true,
                inputs = quotedAssetSet,
                id = yieldCurveId,
                definitionRef = curveId.PricingStructureType.ToString(),
                discountFactorCurve = discountFactorCurve
            };
            return yieldCurveValuation;
        }

        /// <summary>
        /// Parses the underlying curve.
        /// </summary>
        /// <param name="underlyingCurveAsString">The underlying curve.</param>
        /// <returns></returns>
        protected static UnderyingCurveTypes ParseUnderlyingCurve(string underlyingCurveAsString)
        {
            return EnumHelper.Parse<UnderyingCurveTypes>(underlyingCurveAsString);
        }


        #endregion

        #region QuotedAssetSet Helpers

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The array of spread values must be the same length as the array of assets.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValues"></param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal[] spreadValues,
                                                                                     Pair<PricingStructure, PricingStructureValuation> bondCurve, BondCurveIdentifier curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId.Id;
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            if (spreadValues.Length == ycv.inputs.instrumentSet.Items.Length)
            {
                var index = 0;
                foreach (var bav in ycv.inputs.assetQuote)
                {
                    bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                      new List<BasicQuotation>(bav.quote),
                                                                                      spreadValues[index]);
                    index++;
                }
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The array of spread values must be the same length as the array of assets.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValueSet">THe assets must be the same as thoe in the reference curve. Any asset not the same the spread value will be excluded.
        /// and the base value reapplied. Also any basis swaps will be removed.</param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(QuotedAssetSet spreadValueSet,
                                                                                             Pair<PricingStructure, PricingStructureValuation> bondCurve, string curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId;
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            //This strips out the basis swaps.
            //
            ycv.inputs = AssetHelper.RemoveAssetsFromQuotedAssetSet(AssetTypesEnum.BasisSwap, ycv.inputs);
            var instruments = ycv.inputs.instrumentSet.Items.ToList();
            var index = 0;
            foreach (var asset in spreadValueSet.instrumentSet.Items)
            {                
                var idx = instruments.FindIndex(quotationItem => String.Compare(quotationItem.id, asset.id, StringComparison.OrdinalIgnoreCase) == 0);
                ycv.inputs.assetQuote[idx].quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                      new List<BasicQuotation>(ycv.inputs.assetQuote[idx].quote),
                                                                                      spreadValueSet.assetQuote[index].quote[0].value);
                index++;
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The array of spread values must be the same length as the array of assets.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValues"></param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal[] spreadValues,
                                                                                             Pair<PricingStructure, PricingStructureValuation> bondCurve, string curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId;
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            //This strips out the basis swaps.
            //
            ycv.inputs = AssetHelper.RemoveAssetsFromQuotedAssetSet(AssetTypesEnum.BasisSwap, ycv.inputs);
            if (spreadValues.Length == ycv.inputs.instrumentSet.Items.Length)
            {
                var index = 0;
                foreach (var bav in ycv.inputs.assetQuote)
                {
                    bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                      new List<BasicQuotation>(bav.quote),
                                                                                      spreadValues[index]);
                    index++;
                }
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValue"></param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal spreadValue,
                                                                                            Pair<PricingStructure, PricingStructureValuation> bondCurve, IIdentifier curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId.Id;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            foreach (var bav in ycv.inputs.assetQuote)
            {
                bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                  new List<BasicQuotation>(bav.quote),
                                                                                  spreadValue);
            }

            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValue"></param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal spreadValue,
                                                                                            Pair<PricingStructure, PricingStructureValuation> bondCurve, string curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            foreach (var bav in ycv.inputs.assetQuote)
            {
                bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                  new List<BasicQuotation>(bav.quote),
                                                                                  spreadValue);
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Clones a curve, sets the quoted assetset specified and then returns an FpML structure back.
        /// </summary>
        /// <param name="referenceCurve"></param>
        /// <param name="cleanedAssetSet"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> ProcessCurveRiskSet(IBondCurve referenceCurve, QuotedAssetSet cleanedAssetSet)
        {
            //Clone the ref curves.
            //
            var clonedCurve = referenceCurve.Clone();
            var fpml = ((IBondCurve)clonedCurve).GetFpMLData();
            var ycvCurveCloned = (YieldCurveValuation)fpml.Second;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            ycvCurveCloned.discountFactorCurve.point = null;
            ycvCurveCloned.zeroCurve = null;
            ycvCurveCloned.forwardCurve = null;
            //Manipulate the quated asset set.
            //
            ycvCurveCloned.inputs = cleanedAssetSet;
            return fpml;
        }

        #endregion

        #region Factory methods

        internal static double CalculateImpliedQuote(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime baseDate, NamedValueSet properties,
            string[] fraInstruments, decimal[] fraRates, decimal[] adjustments,
            string spreadAssetId, BasicAssetValuation spreadAssetValuation, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            properties.Set("Usage", "Solver");
            // create the pricing structure & put into Cache
            var quotedAssetSet = AssetHelper.Parse(fraInstruments, fraRates, adjustments);
            IPricingStructure fraPricingStructure = new RateCurve(logger, cache, nameSpace, properties, quotedAssetSet, fixingCalendar, rollCalendar);
            //Solve 
            var assetProperties = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, spreadAssetId, baseDate);
            IPriceableAssetController priceableAsset = PriceableAssetFactory.Create(logger, cache, nameSpace, spreadAssetValuation, assetProperties, fixingCalendar, rollCalendar);
            var value = (double)priceableAsset.CalculateImpliedQuote(fraPricingStructure);

            return value;
        }

        #endregion
    }
}
