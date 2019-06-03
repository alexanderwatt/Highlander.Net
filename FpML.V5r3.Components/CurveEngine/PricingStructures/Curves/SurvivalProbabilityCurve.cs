#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Rates;
using Orion.Analytics.Helpers;
using Orion.Analytics.DayCounters;
using Orion.CalendarEngine.Helpers;
using National.QRSC.Constants;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using National.QRSC.FpML.V47;
using National.QRSC.FpML.V47.Codes;
using National.QRSC.Identifiers;
using National.QRSC.ModelFramework;
using National.QRSC.ModelFramework.Assets;
using National.QRSC.ModelFramework.Business;
using National.QRSC.ModelFramework.PricingStructures;
using National.QRSC.ModelFramework.Identifiers;
using Orion.Numerics.Solvers;
using National.QRSC.Utility.Helpers;
using National.QRSC.Utility.NamedValues;
using National.QRSC.Utility.Serialisation;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    /// <summary>
    /// The advanced ratecurve class.
    /// </summary>
    public class SurvivalProbabilityCurve : CurveBase, ICreditCurve
    {

        /// <summary>
        /// The curve type.
        /// </summary>
        public string CurveType = "SurvivalProbabilityCurve";

        /// <summary>
        /// The underlying vurve to use for interpolation.
        /// </summary>
        public string UnderlyingInterpolatedCurve { get; set; }

        /// <summary>
        /// The compounding frequency to use for interpolation.
        /// </summary>
        public CompoundingFrequencyEnum CompoundingFrequency { get; set; }

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        public List<IPriceableCreditAssetController> PriceableCreditAssets { get; set; }

        /// <summary>
        /// The bootstrapper name.
        /// </summary>
        public String BootstrapperName { get; set; }

        /// <summary>
        /// Holds the algorithm  information.
        /// </summary>
        public PricingStructureAlgorithmsHolder Holder { get; set; }

        ///<summary>
        /// The spread controllers that may have been defined.
        ///</summary>
        public List<IPriceableRateSpreadAssetController> PriceableSpreadAssets { get; set; }

        /// <summary>
        /// Gets and sets members of the quoted asset set. Mainly for use the market data server.
        /// </summary>
        protected QuotedAssetSet QuotedAssetSet
        {
            get { return GetQuotedAssetSet(); }
            set { ((CreditCurveValuation)PricingStructureValuation).inputs = value; }
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="curveId">The curveid.</param>
        ///// <param name="algorithm">The algorithm.</param>
        ///// <param name="instruments">The instruments.</param>
        ///// <param name="values">The values.</param>
        //[Description("public interface")]
        //public SurvivalProbabilityCurve(DateTime baseDate, RateCurveIdentifier curveId, string algorithm, string[] instruments, decimal[] values)
        //    :this(baseDate, curveId, algorithm, instruments, values, (decimal[])null)
        //{
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="curveId">The curve id.</param>
        ///// <param name="algorithm">The algorithm.</param>
        ///// <param name="instruments">The instruments.</param>
        ///// <param name="priceQuoteUnits">The additional price quote units info.</param>
        //public SurvivalProbabilityCurve(DateTime baseDate, RateCurveIdentifier curveId, string algorithm, string[] instruments, string[] priceQuoteUnits)
        //    : this(baseDate, curveId, algorithm, instruments, BuildDecimalArray(0.001m, instruments.Length), BuildStringArray("MarketQuote", instruments.Length), priceQuoteUnits)
        //{ }

        #region NameValueSet-based constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="values">The values.</param>
        [Description("public interface")]
        public SurvivalProbabilityCurve(NamedValueSet properties, string[] instruments, decimal[] values)
            : this(properties, instruments, values, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="values">The values.</param>
        /// <param name="additional">The additional.</param>
        /// <param name="tolerance">The solver tolerance.</param>
        [Description("public interface")] // actual implementation
        public SurvivalProbabilityCurve(NamedValueSet properties, string[] instruments, decimal[] values,
                         decimal[] additional, double tolerance)
        {
            PricingStructureIdentifier = new RateCurveIdentifier(properties);

            if (additional != null)
            {
                additional = new decimal[values.Length];
            }

            var curveId = GetCreditCurveId();

            var termCurve = SetConfigurationData();

            PriceableCreditAssets = AssetHelper.CreatePriceableCreditAssets(instruments, curveId.BaseDate, values, additional);

            termCurve.point = CreditBootstrapper.Bootstrap(PriceableCreditAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                           termCurve.interpolationMethod, tolerance);

            var fpmlData = CreatePricingStructure(curveId.BaseDate, curveId, curveId.Algorithm, instruments, values, additional, termCurve);

            SetFpMLData(fpmlData, false);

            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            //
            SetInterpolator(termCurve);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="values">The values.</param>
        /// <param name="additional">The additional.</param>
        [Description("public interface")] // actual implementation
        public SurvivalProbabilityCurve(NamedValueSet properties, string[] instruments, decimal[] values, decimal[] additional)
        {
            PricingStructureIdentifier = new RateCurveIdentifier(properties);

            var curveId = GetCreditCurveId();

            var termCurve = SetConfigurationData();

            var priceableCreditAssets = AssetHelper.CreatePriceableCreditAssets(instruments, curveId.BaseDate, values, additional);

            termCurve.point = CreditBootstrapper.Bootstrap(priceableCreditAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod);

            var fpmlData = CreatePricingStructure(curveId.BaseDate, curveId, curveId.Algorithm, instruments, values, additional, termCurve);

            PriceableCreditAssets = priceableCreditAssets;

            SetFpMLData(fpmlData, false);

            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            //
            SetInterpolator(termCurve);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="values">The values.</param>
        /// <param name="measureTypes">The measure types.</param>
        /// <param name="priceQuoteUnits">The additional price quote units info.</param>
        [Description("public interface")]
        public SurvivalProbabilityCurve(NamedValueSet properties, string[] instruments,
                         decimal[] values, string[] measureTypes, string[] priceQuoteUnits)
        {
            PricingStructureIdentifier = new RateCurveIdentifier(properties);

            var curveId = GetCreditCurveId();

            var termCurve = SetConfigurationData();

            PriceableSpreadAssets = AssetHelper.CreatePriceableSpreadFraAssets(instruments, curveId.BaseDate, values, measureTypes,
                                                                               priceQuoteUnits);

            var priceableCreditAssets = AssetHelper.CreatePriceableCreditAssets(instruments, curveId.BaseDate, values, measureTypes,
                                                                        priceQuoteUnits);

            termCurve.point = CreditBootstrapper.Bootstrap(priceableCreditAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod);
            var fpmlData = CreatePricingStructure(curveId.BaseDate, curveId, curveId.Algorithm, instruments, values, measureTypes,
                                                  priceQuoteUnits, termCurve);//TODO may want to separate FpML creation from bootstrapping.

            PriceableCreditAssets = priceableCreditAssets;

            SetFpMLData(fpmlData, false);

            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            //
            SetInterpolator(termCurve);

        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="algorithm">The algorithm.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="values">The values.</param>
        /// <param name="measureTypes">The measure types.</param>
        /// <param name="priceQuoteUnits">The additional price quote units info.</param>
        [Description("public interface")]
        public SurvivalProbabilityCurve(DateTime baseDate, RateCurveIdentifier curveId, string algorithm, string[] instruments,
                         decimal[] values, string[] measureTypes, string[] priceQuoteUnits)
        {
            PricingStructureIdentifier = curveId;

            PriceableSpreadAssets = AssetHelper.CreatePriceableSpreadFraAssets(instruments, baseDate, values, measureTypes,
                                                                               priceQuoteUnits);

            var termCurve = SetConfigurationData();

            PriceableCreditAssets = AssetHelper.CreatePriceableCreditAssets(instruments, baseDate, values, measureTypes,
                                                                        priceQuoteUnits);

            termCurve.point = CreditBootstrapper.Bootstrap(PriceableCreditAssets, baseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod);

            var fpmlData = CreatePricingStructure(baseDate, curveId, algorithm, instruments, values, measureTypes,
                                                  priceQuoteUnits, termCurve);//TODO may want to separate FpML creation from bootstrapping.

            SetFpMLData(fpmlData, false);

            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            //
            SetInterpolator(termCurve);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public SurvivalProbabilityCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData)
            : this(fpmlData, PricingStructurePropertyHelper.RateCurve(fpmlData))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing strucuture.</param>
        public SurvivalProbabilityCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
            : base(fpmlData)
        {
            PricingStructureIdentifier = new RateCurveIdentifier(properties);

            var curveId = GetCreditCurveId();

            var termCurve = SetConfigurationData();

            var tempFpML = (CreditCurveValuation)fpmlData.Second;

            BootstrapperName = Holder.GetValue(curveId.PricingStructureType, curveId.Algorithm, "Bootstrapper");

            var doBuild = false;

            if (tempFpML.inputs != null)
            {
                var qas = tempFpML.inputs;

                PriceableCreditAssets = AssetHelper.CreatePriceableCreditAssets(tempFpML.baseDate.Value, qas);

                if (tempFpML.defaultProbabilityCurve.defaultProbabilities == null)
                {
                    doBuild = true;
                }
            }

            if (doBuild)//TODO what happened the central bank dates
            {
                termCurve.point = CreditBootstrapper.Bootstrap(PriceableCreditAssets, tempFpML.baseDate.Value, tempFpML.defaultProbabilityCurve.defaultProbabilities.extrapolationPermitted,
                                                                                                                   tempFpML.defaultProbabilityCurve.defaultProbabilities.interpolationMethod);

                ((CreditCurveValuation)fpmlData.Second).defaultProbabilityCurve.defaultProbabilities = termCurve;
            }

            SetFpMLData(fpmlData, false);

            SetInterpolator(termCurve);
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SurvivalProbabilityCurve"/> class.
        ///// </summary>
        ///// <param name="baseDate"></param>
        ///// <param name="curveId">The curve id.</param>
        ///// <param name="algorithm">The algorithm.</param>
        ///// <param name="priceableRateAssets">The priceable rate assets.</param>
        //public SurvivalProbabilityCurve(DateTime baseDate, RateCurveIdentifier curveId, string algorithm,
        //                 IPriceableRateAssetController[] priceableRateAssets)
        //{
        //    PricingStructureIdentifier = curveId;

        //    var yieldCurve = CreateYieldCurve(curveId.Id, curveId.CurveName, algorithm, curveId.Currency, curveId.ForecastRateIndex);

        //    var quotedAssetSet = RatesQuotedAssetSetHelper.Parse(priceableRateAssets);

        //    Holder = new PricingStructureAlgorithmsHolder();
        //    //string bootstrapperName = holder.GetValue(algorithm, "Bootstrapper");

        //    // The interpolators to use
        //    var bootstrapInterpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue(curveId.PricingStructureType, algorithm, "BootstrapperInterpolation"));

        //    // Get the extrapolation indicator
        //    var extrapolation = bool.Parse(Holder.GetValue(curveId.PricingStructureType, algorithm, "ExtrapolationPermitted"));

        //    var termCurve = new TermCurve
        //                        {
        //                            extrapolationPermitted = extrapolation,
        //                            extrapolationPermittedSpecified = true,
        //                            interpolationMethod = bootstrapInterpolationMethod
        //                        };

        //    var yieldCurveValuation = CreateYieldCurveValuation(baseDate, quotedAssetSet, yieldCurve.id);

        //    yieldCurveValuation.id = yieldCurve.id;

        //    yieldCurveValuation.discountFactorCurve = termCurve;

        //    //IBootstrapController<IBootstrapControllerData, IPricingStructure> bootstrapper = ClassFactory<IBootstrapController<IBootstrapControllerData, IPricingStructure>>.Create(bootstrapperName);
        //    //IBootstrapControllerData controllerData = new BootstrapControllerData(yieldCurve, yieldCurveValuation);

        //    var priceableAssetList = new List<IPriceableRateAssetController>(priceableRateAssets);

        //    priceableAssetList.Sort(
        //        (priceableAssetController1, priceableAssetController2) 
        //        => 
        //        priceableAssetController1.GetRiskMaturityDate().CompareTo(priceableAssetController2.GetRiskMaturityDate())
        //        );

        //    //TODO set the FpML????

        //    //// Run the calculate ????
        //    //IPricingStructure pricingStructure = bootstrapper.Calculate(controllerData);
        //}

        internal TermCurve SetConfigurationData()
        {
            Holder = new PricingStructureAlgorithmsHolder();

            var curveId = GetCreditCurveId();

            // The bootstrapper to use
            //
            BootstrapperName = Holder.GetValue(curveId.PricingStructureType, curveId.Algorithm, "Bootstrapper");

            //if (BootstrapperName == "FastBootstrapper")
            //{
            var termCurve = new TermCurve
            {
                extrapolationPermitted =
                    bool.Parse(Holder.GetValue(curveId.PricingStructureType,
                                               curveId.Algorithm,
                                               "ExtrapolationPermitted")),
                extrapolationPermittedSpecified = true,
                interpolationMethod =
                    InterpolationMethodHelper.Parse(
                    Holder.GetValue(curveId.PricingStructureType, curveId.Algorithm,
                                    "BootstrapperInterpolation"))
            };

            CompoundingFrequency = EnumHelper.Parse<CompoundingFrequencyEnum>(Holder.GetValue(curveId.PricingStructureType, curveId.Algorithm, "CompoundingFrequency"));

            UnderlyingInterpolatedCurve = Holder.GetValue(curveId.PricingStructureType, curveId.Algorithm, "UnderlyingCurve"); //TODO this redundant.

            return termCurve;
        }

        internal void SetInterpolator(TermCurve defaultProbabilityCurve)
        {
            Holder = new PricingStructureAlgorithmsHolder();

            var curveId = (RateCurveIdentifier)PricingStructureIdentifier;

            // The underlying curve and associated compounding frequency (compounding frequency required when underlying curve is a ZeroCurve)
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue(curveId.PricingStructureType, curveId.Algorithm, "CurveInterpolation"));

            var dayCounter = DayCounterHelper.Parse(Holder.GetValue(curveId.PricingStructureType, curveId.Algorithm, "DayCounter"));

            var underlyingCurve = ParseUnderlyingCurve(UnderlyingInterpolatedCurve);

            // interpolate the DiscountFactor curve based on the respective curve interpolation 
            if (underlyingCurve != UnderyingCurveTypes.ZeroCurve)
            {
                Interpolator = new TermCurveInterpolator(defaultProbabilityCurve, GetBaseDate(), dayCounter);
            }
            // otherwise interpolate our underlying curve will be a zero curve
            else
            {
                var zeroCurve = new ZeroRateCurve
                {
                    rateCurve = YieldCurveAnalytics.ToZeroCurve(defaultProbabilityCurve,
                                                                GetBaseDate(),
                                                                CompoundingFrequency, dayCounter),
                    compoundingFrequency = National.QRSC.FpML.V47.CompoundingFrequency.Create(CompoundingFrequency)
                };

                zeroCurve.rateCurve.interpolationMethod = curveInterpolationMethod;

                Interpolator =
                        new TermCurveInterpolator(defaultProbabilityCurve, GetBaseDate(), dayCounter);
            }
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="algorithm">The algorithm.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="values">The values.</param>
        /// <param name="additional">The additonal values:mainly for vols</param>
        /// <param name="termCurve">The term curve.</param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> CreatePricingStructure(DateTime baseDate,
                                                                                                  RateCurveIdentifier curveId,
                                                                                                  string algorithm,
                                                                                                  string[] instruments,
                                                                                                  decimal[] values,
                                                                                                  decimal[] additional,
                                                                                                  TermCurve termCurve)
        {
            var quotedAssetSet = AssetHelper.Parse(instruments, values, additional);
            var creditCurve = CreateCreditCurve(curveId.Id, curveId.CurveName, algorithm, curveId.Currency);
            var creditCurveValuation = CreateCreditCurveValuation(baseDate, quotedAssetSet, creditCurve.id);
            creditCurveValuation.defaultProbabilityCurve.defaultProbabilities = termCurve;

            return new Pair<PricingStructure, PricingStructureValuation>(creditCurve, creditCurveValuation);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="algorithm">The algorithm.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="values">The values.</param>
        /// <param name="measureTypes">The measure types.</param>
        /// <param name="priceQuoteUnits">The additional.</param>
        /// <param name="termCurve">The term curve.</param>
        /// <returns></returns>
        private static Pair<PricingStructure, PricingStructureValuation> CreatePricingStructure(DateTime baseDate,
                                                                                                  RateCurveIdentifier curveId,
                                                                                                  string algorithm,
                                                                                                  string[] instruments,
                                                                                                  decimal[] values,
                                                                                                  string[] measureTypes,
                                                                                                  string[] priceQuoteUnits,
                                                                                                  TermCurve termCurve)
        {
            //var forecastRateIndex = ForecastRateIndexHelper.Parse(indexName, indexTenor);
            var quotedAssetSet = AssetHelper.CreateQuotedAssetSet(instruments, values, measureTypes, priceQuoteUnits);

            var creditCurve = CreateCreditCurve(curveId.Id, curveId.CurveName, algorithm, curveId.Currency);

            var creditCurveValuation = CreateCreditCurveValuation(baseDate, quotedAssetSet, creditCurve.id);

            creditCurveValuation.defaultProbabilityCurve.defaultProbabilities = termCurve;

            return new Pair<PricingStructure, PricingStructureValuation>(creditCurve, creditCurveValuation);
        }

        ///// <summary>
        ///// Creates the pricing structure.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="curveId">The curve id.</param>
        ///// <param name="algorithm">The algorithm.</param>
        ///// <param name="instruments">The instruments.</param>
        ///// <param name="values">The values.</param>
        ///// <returns></returns>
        //protected static Pair<PricingStructure, PricingStructureValuation> CreatePricingStructure(DateTime baseDate,
        //                                                                                          RateCurveIdentifier curveId,
        //                                                                                          string algorithm,
        //                                                                                          string[] instruments,
        //                                                                                          decimal[] values)
        //{
        //    var vols = new decimal[instruments.Length];

        //    return CreatePricingStructure(baseDate, curveId, algorithm, instruments, values, vols);
        //}

        ///// <summary>
        ///// Creates the pricing structure.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="curveId">The curve id.</param>
        ///// <param name="algorithm">The algorithm.</param>
        ///// <param name="instruments">The instruments.</param>
        ///// <param name="values">The values.</param>
        ///// <param name="additional">The additional.</param>
        ///// <returns></returns>
        //protected static Pair<PricingStructure, PricingStructureValuation> CreatePricingStructure(DateTime baseDate,
        //                                                                                          RateCurveIdentifier curveId,
        //                                                                                          string algorithm,
        //                                                                                          string[] instruments,
        //                                                                                          decimal[] values,
        //                                                                                          decimal[] additional)
        //{
        //    var holder = new PricingStructureAlgorithmsHolder();

        //    // The interpolators to use
        //    var bootstrapInterpolation = InterpolationMethodHelper.Parse(holder.GetValue(curveId.PricingStructureType, algorithm, "BootstrapperInterpolation"));

        //    // Get the extrapolation indicator
        //    var extrapolationPermitted = bool.Parse(holder.GetValue(curveId.PricingStructureType, algorithm, "ExtrapolationPermitted"));

        //    //var forecastRateIndex = ForecastRateIndexHelper.Parse(indexName, indexTenor);
        //    var quotedAssetSet = RatesQuotedAssetSetHelper.Parse(instruments, values, additional);

        //    var yieldCurve = CreateYieldCurve(curveId.Id, curveId.CurveName, algorithm, curveId.Currency, curveId.ForecastRateIndex);

        //    var yieldCurveValuation = CreateYieldCurveValuation(baseDate, quotedAssetSet, yieldCurve.id);

        //    var termCurve = new TermCurve
        //                        {
        //                            extrapolationPermittedSpecified = true,
        //                            extrapolationPermitted = extrapolationPermitted,
        //                            interpolationMethod = bootstrapInterpolation
        //                        };
        //    yieldCurveValuation.discountFactorCurve = termCurve;

        //    return new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, yieldCurveValuation);
        //}

        ///// <summary>
        ///// Creates the pricing structure.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="curveId">The curve id.</param>
        ///// <param name="algorithm">The algorithm.</param>
        ///// <param name="instruments">The instruments.</param>
        ///// <param name="values">The values.</param>
        ///// <param name="measureTypes">The measure types.</param>
        ///// <param name="priceQuoteUnits">The additional.</param>
        ///// <returns></returns>
        //protected static Pair<PricingStructure, PricingStructureValuation> CreatePricingStructure(DateTime baseDate,
        //                                                                                          RateCurveIdentifier curveId,
        //                                                                                          string algorithm,
        //                                                                                          string[] instruments,
        //                                                                                          decimal[] values,
        //                                                                                          string[] measureTypes,
        //                                                                                          string[] priceQuoteUnits)
        //{
        //    var holder = new PricingStructureAlgorithmsHolder();

        //    // The interpolators to use
        //    var bootstrapInterpolation = InterpolationMethodHelper.Parse(holder.GetValue(curveId.PricingStructureType, algorithm, "BootstrapperInterpolation"));

        //    // Get the extrapolation indicator
        //    var extrapolationPermitted = bool.Parse(holder.GetValue(curveId.PricingStructureType, algorithm, "ExtrapolationPermitted"));

        //    //var forecastRateIndex = ForecastRateIndexHelper.Parse(indexName, indexTenor);
        //    var quotedAssetSet = AssetHelper.CreateQuotedAssetSet(instruments, values, measureTypes, priceQuoteUnits);

        //    var yieldCurve = CreateYieldCurve(curveId.Id, curveId.CurveName, algorithm, curveId.Currency, curveId.ForecastRateIndex);

        //    var yieldCurveValuation = CreateYieldCurveValuation(baseDate, quotedAssetSet, yieldCurve.id);

        //    var termCurve = new TermCurve
        //                        {
        //                            extrapolationPermittedSpecified = true,
        //                            extrapolationPermitted = extrapolationPermitted,
        //                            interpolationMethod = bootstrapInterpolation
        //                        };
        //    yieldCurveValuation.discountFactorCurve = termCurve;

        //    return new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, yieldCurveValuation);
        //}

        //        /// <summary>
        //        /// Bootstraps the specified bootstrapper name.
        //        /// </summary>
        //        /// <param name="bootstrapperName">Name of the bootstrapper.</param>
        //        /// <param name="yieldCurve">The yield curve.</param>
        //        /// <param name="yieldCurveValuation">The yield curve valuation.</param>
        //        /// <returns></returns>
        //        protected static IBootstrapController<IBootstrapControllerData, IPricingStructure> Bootstrap(string bootstrapperName, PricingStructure yieldCurve,
        //                                                                                                     PricingStructureValuation yieldCurveValuation)
        //        {
        //            // Create the Bootstrapper and then initiate the bootstrapping by invoking the Calculate
        ////            var bootstrapper = ClassFactory<IBootstrapController<IBootstrapControllerData, IPricingStructure>>.Create("National.QRSC.Bootstrappers.RateCurves", bootstrapperName);

        //            //var bootstrapper = BootstrapperFactory.Create<IBootstrapController<IBootstrapControllerData, IPricingStructure>>(bootstrapperName);

        //            var bootstrapper = new SimpleRateBootstrapper();

        //            IBootstrapControllerData controllerData = new BootstrapControllerData(yieldCurve, yieldCurveValuation);

        //            bootstrapper.Calculate(controllerData);

        //            return bootstrapper;
        //        }

        ///// <summary>
        ///// Bootstraps the specified bootstrapper name.
        ///// </summary>
        ///// <param name="bootstrapperName">Name of the bootstrapper.</param>
        ///// <param name="yieldCurve">The yield curve.</param>
        ///// <param name="yieldCurveValuation">The yield curve valuation.</param>
        ///// <returns></returns>
        //protected static IBootstrapController<IBootstrapControllerData, IPricingStructure> SpreadBootstrap(string bootstrapperName, PricingStructure yieldCurve,
        //                                                                                                   PricingStructureValuation yieldCurveValuation)
        //{
        //    // Create the Bootstrapper and then initiate the bootstrapping by invoking the Calculate
        //    var bootstrapper = ClassFactory<IBootstrapController<IBootstrapControllerData, IPricingStructure>>.Create("National.QRSC.Bootstrappers.RateCurves", bootstrapperName);

        //    //var bootstrapper = BootstrapperFactory.Create<IBootstrapController<IBootstrapControllerData, IPricingStructure>>(bootstrapperName);

        //    IBootstrapControllerData controllerData = new BootstrapControllerData(yieldCurve, yieldCurveValuation);

        //    bootstrapper.Calculate(controllerData);

        //    return bootstrapper;
        //}

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote()
        {
            if (PriceableCreditAssets != null)//Bootstrapper
            {
                return EvaluateImpliedQuote(this, PriceableCreditAssets.ToArray());
            }

            var curveValuation = GetCreditCurveValuation();

            PriceableCreditAssets = AssetHelper.CreatePriceableCreditAssets(curveValuation.baseDate.Value, curveValuation.inputs);

            return EvaluateImpliedQuote(this, PriceableCreditAssets.ToArray());

        }

        /// <summary>
        /// Evaluates the implied quotes.
        /// </summary>
        /// <param name="rateCurve">The rate curve.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        public NamedValueSet EvaluateImpliedQuote(ICreditCurve rateCurve, IPriceableCreditAssetController[] priceableAssets)
        {
            return EvaluateImpliedQuote(priceableAssets, rateCurve);
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="creditCurve"></param>
        public NamedValueSet EvaluateImpliedQuote(IPriceableCreditAssetController[] priceableAssets,
                                                                    ICreditCurve creditCurve)
        {
            var result = new NamedValueSet();

            foreach (var priceableAsset in priceableAssets)
            {
                result.Set(priceableAsset.Id, priceableAsset.CalculateImpliedQuote(creditCurve));
                //var marketQuote = MarketQuoteHelper.NormalisePriceUnits(priceableAsset.MarketQuote, "DecimalRate").value;
            }
            return result;
        }

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        protected void SetFpMLData(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            SetFpMLData(fpmlData, true);
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
                PricingStructureIdentifier = new RateCurveIdentifier(PricingStructure.id, baseDate);
            }
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetSurvivalProbability(DateTime baseDate, DateTime targetDate)
        {
            IPoint point = new DateTimePoint1D(baseDate, targetDate);

            return Value(point);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetSurvivalProbability(DateTime targetDate)
        {
            return GetSurvivalProbability(PricingStructureValuation.baseDate.Value, targetDate);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetSurvivalProbability(DateTime baseDate, DateTime[] targetDates)
        {
            var discountFactors = new List<double>();

            foreach (var targetDate in targetDates)
            {
                discountFactors.Add(GetSurvivalProbability(baseDate, targetDate));
            }
            return discountFactors.ToArray();
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetSurvivalProbability(DateTime[] targetDates)
        {
            return GetSurvivalProbability(PricingStructureValuation.baseDate.Value, targetDates);
        }

        /// <summary>
        /// DFs the value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public double DiscountFactorValue(DateTime baseDate, DateTime date)
        {
            IPoint point = new DateTimePoint1D(baseDate, date);

            return UnderlyingInterpolatedCurve == "HazardCurve" 
                ? RateAnalytics.ZeroRateToDiscountFactor(Interpolator.Value(point), (double)point.Coords[0], CompoundingFrequency) 
                : Interpolator.Value(point);
        }

        /// <summary>
        /// Gets the yield curve valuation.
        /// </summary>
        /// <returns></returns>
        public CreditCurveValuation GetCreditCurveValuation()
        {
            return (CreditCurveValuation)PricingStructureValuation;
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public override QuotedAssetSet GetQuotedAssetSet()
        {
            return GetCreditCurveValuation().inputs;
        }

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <returns>An array of term points</returns>
        public override TermCurve GetTermCurve()
        {
            return GetCreditCurveValuation().defaultProbabilityCurve.defaultProbabilities;
        }

        /// <summary>
        /// Gets the quoted asset item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Asset GetQuotedAsset(int item)
        {
            return GetQuotedAssetSet().instrumentSet[item];
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

                var quote = MarketQuoteHelper.FindQuotationByMeasureType(measureType, quotes);

                if (quote != null)
                {
                    result = quote.value;
                }
            }
            return result;
        }

        /// <summary>
        /// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measuretype to determine which one.
        /// </summary>
        /// <param name="assetItemIndex">The item to return. Must be zero based</param>
        /// <param name="value">The value to update to.</param>
        /// <param name="measureType">The measureType of the quotation required.</param>
        /// <returns></returns>
        public Boolean PerturbCurve(int assetItemIndex, Decimal value, String measureType)//TODO need to finish this.
        {
            var quotes = ((CreditCurveValuation)PricingStructureValuation).inputs.assetQuote[assetItemIndex].quote;

            var val = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType(measureType, new List<BasicQuotation>(quotes), value);

            ((CreditCurveValuation)PricingStructureValuation).inputs.assetQuote[assetItemIndex].quote = val;

            Build();

            return true;
        }

        /// <summary>
        /// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measuretype to determine which one.
        /// </summary>
        /// <param name="values">The value to update to.</param>
        /// <param name="measureType">The measureType of the quotation required.</param>
        /// <returns></returns>
        public override Boolean PerturbCurve(Decimal[] values, String measureType)//TODO need to finish this.
        {
            var quotes = ((CreditCurveValuation)PricingStructureValuation).inputs.assetQuote;

            if (values.Length == quotes.Length)
            {
                var index = 0;

                foreach (var quote in quotes)
                {
                    var val = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType(measureType, new List<BasicQuotation>(quote.quote), values[index]);

                    quote.quote = val;

                    index++;
                }
            }
            Build();

            return true;
        }

        /// <summary>
        /// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measuretype to determine which one.
        /// </summary>
        /// <param name="indices">An array of indices of assets to perturb.</param>
        /// <param name="values">The value to update to.</param>
        /// <param name="measureType">The measureType of the quotation required.</param>
        /// <returns></returns>
        public Boolean PerturbCurve(int[] indices, Decimal[] values, String measureType)//TODO need to finish this.
        {
            var quotes = ((CreditCurveValuation)PricingStructureValuation).inputs.assetQuote;

            if (values.Length == indices.Length)
            {
                var index = 0;

                foreach (var idx in indices)
                {
                    if (idx > quotes.Length) continue;

                    var val = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType(measureType, new List<BasicQuotation>(quotes[idx].quote),
                                                                                    values[index]);
                    quotes[idx].quote = val;

                    index++;
                }
            }
            Build();

            return true;
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public override void Build()
        {
            PriceableCreditAssets = AssetHelper.CreatePriceableCreditAssets(GetCreditCurveValuation().baseDate.Value, GetCreditCurveValuation().inputs);

            var termCurve = GetTermCurve();

            termCurve.point = CreditBootstrapper.Bootstrap(PriceableCreditAssets, GetCreditCurveValuation().baseDate.Value, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod);

            SetFpMLData(new Pair<PricingStructure, PricingStructureValuation>(PricingStructure, PricingStructureValuation));

            SetInterpolator(termCurve);
        }

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
            return UnderlyingInterpolatedCurve == "HazardCurve" 
                ? RateAnalytics.ZeroRateToDiscountFactor(Interpolator.Value(point), (double)point.Coords[0], CompoundingFrequency) 
                : Interpolator.Value(point);
        }

        /// <summary>
        /// Gets the pricing structure type date.
        /// </summary>
        /// <returns></returns>
        public PricingStructureTypeEnum GetPricingStructureType()
        {
            return ((IPricingStructureIdentifier)PricingStructureIdentifier).PricingStructureType;
        }

        /// <summary>
        /// Gets the rate curve id.
        /// </summary>
        /// <returns></returns>
        public RateCurveIdentifier GetCreditCurveId()
        {
            return (RateCurveIdentifier)PricingStructureIdentifier;
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
            return ((RateCurveIdentifier)PricingStructureIdentifier).Algorithm;
        }

        ///// <summary>
        ///// The curve algorithm.
        ///// </summary>
        //public string Algorithm
        //{
        //    get
        //    {
        //        var curve = (YieldCurve) PricingStructure;
        //        return curve.algorithm;
        //    }
        //}

        /// <summary>
        /// Creates the yield curve.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="name">The name.</param>
        /// <param name="algorithm">The algorithm.</param>
        /// <param name="currency">The currency.</param>
        /// <returns></returns>
        protected static CreditCurve CreateCreditCurve(string curveId, string name, string algorithm, Currency currency)//TODO add the other parameters.
        {
            var creditCurve = new CreditCurve
            {
                id = curveId,
                name = name,
                currency = currency
            };
            return creditCurve;
        }

        /// <summary>
        /// Creates the yield curve valuation.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotedAssetSet">The quoted asset set.</param>
        /// <param name="creditCurveId">The yield curve id.</param>
        /// <returns></returns>
        protected static CreditCurveValuation CreateCreditCurveValuation(DateTime baseDate, QuotedAssetSet quotedAssetSet,
                                                                       string creditCurveId)
        {
            var creditCurveValuation = new CreditCurveValuation
            {
                baseDate = IdentifiedDateHelper.Create(baseDate),
                buildDateTimeSpecified = false,
                inputs = quotedAssetSet,
                id = creditCurveId
            };
            return creditCurveValuation;
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

        /// <summary>
        /// Builds a string array of length with value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string[] BuildStringArray(string value, int length)
        {
            var result = new string[length];

            for (var i = 0; i < length; i++)
            {
                result[i] = value;
            }
            return result;
        }

        /// <summary>
        /// Builds a decimal array of length with value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static decimal[] BuildDecimalArray(decimal value, int length)
        {
            var result = new decimal[length];

            for (var i = 0; i < length; i++)
            {
                result[i] = value;
            }
            return result;
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public void ReCalculate(Decimal[] values)
        {
            if (PriceableCreditAssets != null)
            {
                var numControllers = PriceableCreditAssets.Count;

                var valuesArray = new decimal[numControllers];

                if (values.Length == numControllers)
                {
                    valuesArray = values;
                }
                if (values.Length < numControllers)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        valuesArray[i] = values[i];
                    }
                    for (var i = values.Length; i < numControllers; i++)
                    {
                        valuesArray[i] = values[values.Length - 1];
                    }

                }
                else
                {
                    for (var i = 0; i < numControllers; i++)
                    {
                        valuesArray[i] = values[i];
                    }
                }
                var index = 0;

                foreach (var rateController in PriceableCreditAssets)
                {
                    rateController.MarketQuote.value = valuesArray[index];

                    index++;
                }
            }

            //TODO update the qas.
            ((CreditCurveValuation)PricingStructureValuation).defaultProbabilityCurve.defaultProbabilities.point =
                CreditBootstrapper.Bootstrap(PriceableCreditAssets, PricingStructureValuation.baseDate.Value,
                                                GetTermCurve().extrapolationPermitted,
                                                GetTermCurve().interpolationMethod);

            SetInterpolator(GetTermCurve());
        }
    }
}
