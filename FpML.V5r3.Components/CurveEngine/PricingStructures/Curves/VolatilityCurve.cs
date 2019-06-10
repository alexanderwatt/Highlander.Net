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
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Analytics.Options;
using Orion.Analytics.Utilities;
using Orion.CurveEngine.Assets.Helpers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{

    /// <summary>
    /// The advanced ratecurve class.
    /// </summary>
    public abstract class VolatilityCurve : CurveBase, IVolatilitySurface
    {
        #region Properties

        /// <summary>
        /// The asset referenced by the cap volatility curve.
        /// </summary>
        public AnyAssetReference Asset { get; set; }

        /// <summary>
        /// The fixing calendar.
        /// </summary>
        public IBusinessCalendar FixingCalendar { get; set; }

        /// <summary>
        /// The payment calendar.
        /// </summary>
        public IBusinessCalendar PaymentCalendar { get; set; }

        /// <summary>
        /// The underlying curve to use for interpolation.
        /// </summary>
        public string UnderlyingInterpolatedCurve = "VolatilityCurve";

        /// <summary>
        /// The compounding frequency to use for interpolation.
        /// </summary>
        public CompoundingFrequencyEnum CompoundingFrequency { get; set; }

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        public List<IPriceableOptionAssetController> PriceableOptionAssets { get; set; }

        /// <summary>
        /// The bootstrapper name.
        /// </summary>
        public String BootstrapperName = "VolatilityCurveBootstrapper";

        /// <summary>
        /// 
        /// </summary>
        public double Tolerance = 0.00000000001;

        /// <summary>
        /// 
        /// </summary>
        public bool? ExtrapolationPermittedInput = true;

        /// <summary>
        /// 
        /// </summary>
        public InterpolationMethod InterpolationMethod = InterpolationMethodHelper.Parse("LinearInterpolation");

        /// <summary>
        /// 
        /// </summary>
        public bool ExtrapolationPermitted = true;

        /// <summary>
        /// 
        /// </summary>
        public bool IsBootstrapSuccessful;

        /// <summary>
        /// 
        /// </summary>
        public StrikeQuoteUnitsEnum StrikeQuoteUnits { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public MeasureTypesEnum MeasureType { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public QuoteUnitsEnum QuoteUnits { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? Strike { get; protected set; }

        /// <summary>
        /// The date array.
        /// </summary>
        public DateTime[] Dates => VolatilityDates;

        /// <summary>
        /// The discount curve
        /// </summary>
        public IRateCurve DiscountCurve { get; set; }

        /// <summary>
        /// The forecast curve
        /// </summary>
        public IRateCurve ForecastCurve { get; set; }

        /// <summary>
        /// Accessor method for the container that stores the bootstrap Caplet
        /// volatilities.
        /// </summary>
        /// <value>Data structure that stores the results of the Caplet
        /// bootstrap.</value>
        public VolCurveBootstrapResults BootstrapResults { get; protected set; }

        /// <summary>
        /// Accessor method for the  discount factor offsets.
        /// </summary>
        /// <value>Array of decimals that contains the discount factor
        /// offsets.</value>
        public int[] VolatilityOffsets { get; private set; }

        /// <summary>
        /// Accessor method for the  discount factor offsets.
        /// </summary>
        /// <value>Array of decimals that contains the discount factor
        /// offsets.</value>
        public DateTime[] VolatilityDates { get; private set; }

        /// <summary>
        /// Accessor method for the  discount factor values.
        /// </summary>
        /// <value>Array of decimals that contains the discount factor
        /// values.</value>
        public Decimal[] VolatilityValues { get; private set; }

        /// <summary>
        /// Accessor method for the field that indicates if the instantiated
        /// engine is for an ATM bootstrap.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is ATM bootstrap; otherwise,
        /// 	<c>false</c>.
        /// </value>
        public bool IsATMBootstrap { get; set; }

        /// <summary>
        /// Accessor method for the field that indicates if the instantiated
        /// engine is for a fixed strike bootstrap.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a fixed strike bootstrap; 
        /// 	otherwise, <c>false</c>.
        /// </value>
        public bool IsFixedStrikeBootstrap { get; set; }

        /// <summary>
        /// The day counter
        /// </summary>
        public IDayCounter DayCounter { get; set; }

        /// <summary>
        /// The underlying numeraire of the volatility curve.
        /// This would normally be a Xibor type instrument for a cap volatility curve.
        /// </summary>
        public Tuple<Asset, BasicAssetValuation> UnderlyingAssetDetails { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets and sets members of the quoted asset set. Mainly for use the market data server.
        /// </summary>
        protected QuotedAssetSet QuotedAssetSet
        {
            get => GetQuotedAssetSet();
            set => ((VolatilityMatrix)PricingStructureValuation).inputs = value;
        }

        #endregion

        #region NameValueSet-based constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        protected VolatilityCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            if (properties != null)
            {
                var assetClass = properties.GetString(CurveProp.AssetClass, "Rates");
                var asset = EnumHelper.Parse<AssetClass>(assetClass);
                PricingStructureData = new PricingStructureData(CurveType.Parent, asset, properties);
                IsFixedStrikeBootstrap = properties.GetValue(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness) == StrikeQuoteUnitsEnum.ATMFlatMoneyness;
                if(!IsFixedStrikeBootstrap) IsATMBootstrap = true;
            }
            PricingStructureIdentifier = new VolatilitySurfaceIdentifier(properties);
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
                Tolerance = PropertyHelper.ExtractDoubleProperty(CurveProp.Tolerance, properties) ??
                            GetDefaultTolerance(algorithmHolder);
                string compoundingFrequency = properties.GetString(CurveProp.CompoundingFrequency, null);
                CompoundingFrequency = compoundingFrequency != null
                                           ? EnumHelper.Parse<CompoundingFrequencyEnum>(compoundingFrequency)
                                           : GetDefaultCompounding(algorithmHolder);
                //If there is an input property for extrapolation and interpolation then this overrides the algorithm configuration.
                var extrapolation = properties.GetString(CurveProp.ExtrapolationPermitted, null);
                ExtrapolationPermittedInput = extrapolation != null ? bool.Parse(extrapolation) : GetDefaultExtrapolationFlag(algorithmHolder);                
                var interpolation = properties.GetString(CurveProp.BootstrapperInterpolation, null) ?? GetDefaultInterpolationMethod(algorithmHolder);
                InterpolationMethod = InterpolationMethodHelper.Parse(interpolation);
                Holder = algorithmHolder;
            }
            else
            {
                Tolerance = 10 ^ -9;
                CompoundingFrequency = CompoundingFrequencyEnum.Quarterly;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CapVolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        protected VolatilityCurve(ILogger logger, ICoreCache cache, String nameSpace, PricingStructureIdentifier curveIdentifier)
            : base(logger, cache, nameSpace, curveIdentifier)
        {
            var properties = curveIdentifier.GetProperties();
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            Tolerance = PropertyHelper.ExtractDoubleProperty(CurveProp.Tolerance, properties) ?? GetDefaultTolerance(Holder);
            var compoundingFrequency = properties.GetString(CurveProp.CompoundingFrequency, null);
            CompoundingFrequency = compoundingFrequency != null
                                       ? EnumHelper.Parse<CompoundingFrequencyEnum>(compoundingFrequency)
                                       : GetDefaultCompounding(Holder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CapVolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        protected VolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace,
            VolatilitySurfaceIdentifier curveIdentifier, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : this(logger, cache, nameSpace, curveIdentifier)
        {
            FixingCalendar = fixingCalendar;
            PaymentCalendar = rollCalendar;
        }

        #endregion

        #region Runtime Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing structure.</param>
        /// <param name="fixingCalendar">The fixingCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        protected VolatilityCurve(ILogger logger, ICoreCache cache, String nameSpace, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, new VolatilitySurfaceIdentifier(properties ?? PricingStructurePropertyHelper.VolatilityCurve(fpmlData)))
        {
            if (properties != null)
            {
                var assetClass = properties.GetString(CurveProp.AssetClass, "Rates");
                var asset = EnumHelper.Parse<AssetClass>(assetClass);
                PricingStructureData = new PricingStructureData(CurveType.Parent, asset, properties);
                var quoteUnits = properties.GetValue(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
                IsFixedStrikeBootstrap = EnumHelper.Parse<StrikeQuoteUnitsEnum>(quoteUnits) == StrikeQuoteUnitsEnum.ATMFlatMoneyness;
                if (!IsFixedStrikeBootstrap) IsATMBootstrap = true;
                //Set the underlying asset information.
                var instrument = properties.GetString(CurveProp.Instrument, true);
                Asset = new AnyAssetReference { href = instrument };
                UnderlyingAssetDetails = CreateUnderlyingAssetWithProperties();
                QuoteUnits = EnumHelper.Parse<QuoteUnitsEnum>(properties.GetValue(CurveProp.QuoteUnits, QuoteUnitsEnum.LogNormalVolatility.ToString()));
                MeasureType = EnumHelper.Parse<MeasureTypesEnum>(properties.GetValue(CurveProp.MeasureType, MeasureTypesEnum.Volatility.ToString()));
                StrikeQuoteUnits = EnumHelper.Parse<StrikeQuoteUnitsEnum>(properties.GetValue(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()));
                IsFixedStrikeBootstrap = true;
                IsATMBootstrap = false;
                if (StrikeQuoteUnits == StrikeQuoteUnitsEnum.ATMFlatMoneyness || StrikeQuoteUnits == StrikeQuoteUnitsEnum.ATMMoneyness)
                {
                    IsFixedStrikeBootstrap = false;
                    IsATMBootstrap = true;
                }
            }            
            var curveId = GetCurveId();
            Initialize(properties, Holder);
            //Set the underlying asset information.
            if (properties != null)
            {
                var instrument = properties.GetString(CurveProp.Instrument, false);
                Asset = new AnyAssetReference { href = instrument };
            }
            if (fpmlData == null) return;
            FixingCalendar = fixingCalendar;
            PaymentCalendar = rollCalendar;
            if (DayCounter == null)
            {
                DayCounter = DayCounterHelper.Parse("ACT/365.FIXED");
                if (Holder != null)
                {
                    string dayCountBasis = Holder.GetValue("DayCounter");
                    DayCounter = DayCounterHelper.Parse(dayCountBasis);
                }
            }
            // the curve is already built, so don't rebuild
            //Process the matrix
            SetInterpolator(((VolatilityMatrix)PricingStructureValuation).dataPoints, curveId.Algorithm, curveId.PricingStructureType);
            SetFpMLData(fpmlData, false);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the libor rate using the forecast curve.
        /// </summary>
        /// <returns></returns>
        public Tuple<Asset, BasicAssetValuation> CreateUnderlyingAssetWithProperties()
        {
            //1. Find the underlying asset of the volatility curve. This should be a Xibor type of 1M, 1M or 6M tenor.
            var assetId = Asset?.href;         
            //3. Create the asset-basic asset valuation pair.
            var asset = AssetHelper.Parse(assetId, .05m, 0.0m);
            var result = new Tuple<Asset, BasicAssetValuation>(asset.First, asset.Second) ;
            return result;
        }

        /// <summary>
        /// Returns the libor rate using the forecast curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public decimal ComputeForwardPrice(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate)
        {
            //2. Create the properties.
            var properties = PriceableAssetFactory.BuildPropertiesForAssets(NameSpace, UnderlyingAssetDetails.Item1.id, baseDate);
            //4. Create the priceable asset.
            var priceableAsset = PriceableAssetFactory.Create(logger, cache, nameSpace, UnderlyingAssetDetails.Item2, properties, null, null);
            return priceableAsset.CalculateImpliedQuote(ForecastCurve);
        }

        /// <summary>
        /// Helper function used by the master validation function to validate
        /// the strike.
        /// </summary>
        /// <param name="strike">Cap strike.</param>
        protected static void ValidateStrike(decimal strike)
        {
            const string strikeErrorMessage = "Strike must be positive";
            DataQualityValidator.ValidatePositive
                (strike, strikeErrorMessage, true);
        }

        /// <summary>
        /// Helper method used by the master initialisation method to 
        /// initialise the discount factors.
        /// Precondition: Caplet Bootstrap Settings object has been
        /// initialised.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="volatilities">The (raw) volatilities.</param>
        protected void InitialiseVolatilities(DateTime baseDate, SortedList<DateTime, Decimal> volatilities)
        {
            var count = volatilities.Count;
            VolatilityOffsets = new int[count];
            VolatilityDates = new DateTime[count];
            VolatilityValues = new Decimal[count];
            var idx = 0;
            foreach (var date in volatilities.Keys)
            {
                VolatilityDates[idx] = date;
                var offset = date - baseDate;
                VolatilityOffsets[idx] = offset.Days;
                VolatilityValues[idx] = volatilities[date];
                ++idx;
            }
        }

        internal MultiDimensionalPricingData SetConfigurationData()
        {
            if (DayCounter == null)
            {
                DayCounter = DayCounterHelper.Parse("ACT/365.FIXED");
            }
            // The bootstrapper to use
            if (Holder != null)
            {
                string dayCountBasis = Holder.GetValue("DayCounter");
                DayCounter = DayCounterHelper.Parse(dayCountBasis);
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
            var volCurve = new MultiDimensionalPricingData();
            var quoteUnits = PricingStructureIdentifier.Properties.GetValue<string>("QuoteUnits", null);
            if (quoteUnits != null)
            {
                volCurve.quoteUnits = PriceQuoteUnitsHelper.Create(quoteUnits);
            }
            var quotationSide = PricingStructureIdentifier.Properties.GetValue<string>("QuotationSide", null);
            if (quotationSide != null)
            {
                volCurve.side = EnumHelper.Parse<QuotationSideEnum>(quotationSide);
                volCurve.sideSpecified = true;
            }
            var currency = PricingStructureIdentifier.Properties.GetValue<string>("Currency", null);
            if (currency != null)
            {
                volCurve.currency = CurrencyHelper.Parse(currency);
            }
            var timing = PricingStructureIdentifier.Properties.GetValue<string>("Timing", null);
            if (timing != null)
            {
                volCurve.timing = new QuoteTiming {Value = timing};
            }
            var measureType = PricingStructureIdentifier.Properties.GetValue<string>("MeasureType", null);
            if (measureType != null)
            {
                volCurve.measureType = AssetMeasureTypeHelper.Parse(measureType);
            }
            return volCurve;
        }

        protected double GetDefaultTolerance(PricingStructureAlgorithmsHolder algorithms)
        {
            string tolerance = algorithms.GetValue(AlgorithmsProp.Tolerance);
            return double.Parse(tolerance);
        }

        protected string GetDefaultInterpolationMethod(PricingStructureAlgorithmsHolder algorithms)
        {
            string interpolation = algorithms.GetValue(AlgorithmsProp.BootstrapperInterpolation);
            return interpolation;
        }

        protected bool GetDefaultExtrapolationFlag(PricingStructureAlgorithmsHolder algorithms)
        {
            var extrapolation = algorithms.GetValue(AlgorithmsProp.ExtrapolationPermitted);
            return bool.Parse(extrapolation);
        }

        protected CompoundingFrequencyEnum GetDefaultCompounding(PricingStructureAlgorithmsHolder algorithms)
        {
            string compounding = algorithms.GetValue(AlgorithmsProp.CompoundingFrequency);
            return EnumHelper.Parse<CompoundingFrequencyEnum>(compounding);
        }

        protected void SetInterpolator(IList<Tuple<double, double>> timesAndVolatilities, string algorithm, PricingStructureTypeEnum psType)
        {
            // The underlying curve and associated compounding frequency (compounding frequency required when underlying curve is a ZeroCurve)
            var underlyingInterpolatedCurve = Holder?.GetValue("UnderlyingCurve");
            if (underlyingInterpolatedCurve != null)
            {
                UnderlyingInterpolatedCurve = underlyingInterpolatedCurve;
            }
            var underlyingCurve = ParseUnderlyingCurve(UnderlyingInterpolatedCurve);
            // interpolate the curve based on the respective curve interpolation 
            if (underlyingCurve == UnderlyingCurveTypes.VolatilityCurve)
            {
                var interpolationMethod = InterpolationMethod;
                var extrapolationPermitted = ExtrapolationPermittedInput ?? ExtrapolationPermitted;
                Interpolator = new VolCurveInterpolator(timesAndVolatilities, interpolationMethod, extrapolationPermitted);
            }
        }

        protected void SetInterpolator(MultiDimensionalPricingData dataPoints, string algorithm, PricingStructureTypeEnum psType)
        {
            var interpolationMethod = InterpolationMethod;
            var extrapolationPermitted = ExtrapolationPermittedInput ?? ExtrapolationPermitted;
            Interpolator = new VolCurveInterpolator(dataPoints, interpolationMethod, extrapolationPermitted, GetBaseDate());
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(VolatilitySurfaceIdentifier curveId, MultiDimensionalPricingData volatilityCurve, QuotedAssetSet quotedAssetSet)
        {
            VolatilityRepresentation volCurve = CreateVolatilityRepresentation(curveId, Asset);
            VolatilityMatrix volCurveValuation = CreateVolatilityMatrix(curveId, quotedAssetSet, volCurve.id, volatilityCurve);
            var fpmlData = new Pair<PricingStructure, PricingStructureValuation>(volCurve, volCurveValuation);
            SetFpMLData(fpmlData, false);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(VolatilitySurfaceIdentifier curveId, MultiDimensionalPricingData volatilityCurve, 
            IEnumerable<IPriceableOptionAssetController> priceableRateOptionAssets)
        {
            QuotedAssetSet quotedAssetSet = priceableRateOptionAssets != null ? PriceableAssetFactory.Parse(priceableRateOptionAssets) : null;
            CreatePricingStructure(curveId, volatilityCurve, quotedAssetSet);
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
                //DateTime baseDate = PricingStructureValuation.baseDate.Value;
                PricingStructureIdentifier = new VolatilitySurfaceIdentifier(PricingStructure.id);
            }
        }

        #endregion

        #region Curve Properties

        /// <summary>
        /// Gets the yield curve valuation.
        /// </summary>
        /// <returns></returns>
        public VolatilityMatrix GetVolatilityMatrix()
        {
            return (VolatilityMatrix)PricingStructureValuation;
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public override QuotedAssetSet GetQuotedAssetSet()
        {
            return GetVolatilityMatrix().inputs;
        }

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <returns>An array of term points</returns>
        public override TermCurve GetTermCurve()
        {
            //This only works for volatility curves.
            var vm = GetVolatilityMatrix();
            if (vm != null)
            {
                var points = vm.dataPoints?.point;
                var termPoints = new List<TermPoint>();
                if (points != null && vm.baseDate?.Value is DateTime baseDate)
                {
                    foreach (var point in points)
                    {
                        var tenor = point.coordinate[0]?.expiration[0];
                        var period = tenor?.Items[0];
                        if (period is Period numDaysPeriod)
                        {
                            var newDate = baseDate.AddDays(Convert.ToInt16(numDaysPeriod.periodMultiplier));
                            var time = new TimeDimension { Items = new object[1] };
                            time.Items[0] = newDate;
                            var termPoint = new TermPoint
                            {
                                term = time,
                                mid = point.value,
                                midSpecified = true
                            };
                            termPoints.Add(termPoint);
                        }
                    }
                }
                var termCurve = new TermCurve { point = termPoints.ToArray() };
                return termCurve;
            }
            return null;
        }

        /// <summary>
        /// Gets the quoted assets.
        /// </summary>
        /// <returns></returns>
        public Asset[] GetAssets()
        {
            return GetVolatilityMatrix().inputs.instrumentSet.Items;
        }

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <returns>An array of term points</returns>
        public MultiDimensionalPricingData GetPricingData()
        {
            return GetVolatilityMatrix().dataPoints;
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

        #region Light Perturbation Without Cache or BusinessCalendars

        /// <summary>
        /// Creates a perturbed copy of the curve.
        /// </summary>
        /// <returns></returns>
        protected void PerturbedPriceableAssets(List<IPriceableOptionAssetController> priceableRateAssets, Decimal[] values)
        {
            var numControllers = priceableRateAssets.Count;
            if (values.Length != numControllers) return; 
            var index = 0;
            foreach (var rateController in priceableRateAssets)
            {
                rateController.MarketQuote.value = values[index];
                index++;
            }
        }

        protected List<decimal> GetMarketQuotes(IEnumerable<IPriceableOptionAssetController> priceableRateOptionAssets)
        {
            return priceableRateOptionAssets.Select(asset => asset.MarketQuote.value).ToList();
        }

        //TODO Get RId of This!!!!!
        /// <summary>
        /// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measure type to determine which one.
        /// The original curve is modified. This does not create a copy!!
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="values">The value to update to.</param>
        /// <param name="measureType">The measureType of the quotation required.</param>
        /// <returns></returns>
        public override Boolean PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, Decimal[] values, String measureType)
        {
            var quotes = ((VolatilityMatrix)PricingStructureValuation).inputs.assetQuote;
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
            Build(logger, cache, nameSpace, FixingCalendar, PaymentCalendar);
            return true;
        }

        #endregion

        #region Build and Clone

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        public static Pair<PricingStructure, PricingStructureValuation> CloneCurve(Pair<PricingStructure, PricingStructureValuation> volCurve, string curveId)
        {
            PricingStructure volCurveCloned = XmlSerializerHelper.Clone(volCurve.First);
            PricingStructureValuation volvCurveCloned = XmlSerializerHelper.Clone(volCurve.Second);
            //  assign id to the cloned VolatilityRepresentation
            //
            var vc = (VolatilityRepresentation)volCurveCloned;
            vc.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var vcv = (VolatilityMatrix)volvCurveCloned;
            return new Pair<PricingStructure, PricingStructureValuation>(vc, vcv);
        }

        #endregion

        #region IPricingStructure

        /// <summary>
        /// Gets the curve id.
        /// </summary>
        /// <returns></returns>
        public VolatilitySurfaceIdentifier GetCurveId()
        {
            return (VolatilitySurfaceIdentifier)PricingStructureIdentifier;
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
        /// 
        /// </summary>
        /// <param name="dimension1">The expiry dimension.</param>
        /// <returns></returns>
        public double GetValue(double dimension1)
        {
            IPoint point = new Point1D(dimension1);
            return Interpolator.Value(point);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dimension1">The expiry dimension.</param>
        /// <param name="dimension2">the strike dimension. This is ignored</param>
        /// <returns></returns>
        public double GetValue(double dimension1, double dimension2)
        {
            IPoint point = new Point1D(dimension1);
            return Interpolator.Value(point);
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
            return ((VolatilitySurfaceIdentifier)PricingStructureIdentifier).Algorithm;
        }

        #endregion

        #region Other Stuff

        /// <summary>
        /// Creates the yield curve.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="asset">The asset reference</param>
        /// <returns></returns>
        protected static VolatilityRepresentation CreateVolatilityRepresentation(VolatilitySurfaceIdentifier curveId, AnyAssetReference asset)
        {
            var volatilityMatrix = new VolatilityRepresentation
            {
                id = curveId.Id,
                asset = asset
            };
            return volatilityMatrix;
        }

        /// <summary>
        /// Creates the yield curve valuation.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="quotedAssetSet">The quoted asset set.</param>
        /// <param name="volatilityCurveId">The vol curve id.</param>
        /// <param name="volatilityData"></param>
        /// <returns></returns>
        protected static VolatilityMatrix CreateVolatilityMatrix(PricingStructureIdentifier curveId, QuotedAssetSet quotedAssetSet,
        string volatilityCurveId, MultiDimensionalPricingData volatilityData)
        {
            var volatilityValuation = new VolatilityMatrix
            {
                baseDate = IdentifiedDateHelper.Create(curveId.BaseDate),
                buildDateTime = curveId.BuildDateTime,
                buildDateTimeSpecified = true,
                inputs = quotedAssetSet,
                id = volatilityCurveId,
                definitionRef = curveId.PricingStructureType.ToString(),
                dataPoints = volatilityData
            };
            return volatilityValuation;
        }

        /// <summary>
        /// Parses the underlying curve.
        /// </summary>
        /// <param name="underlyingCurveAsString">The underlying curve.</param>
        /// <returns></returns>
        protected static UnderlyingCurveTypes ParseUnderlyingCurve(string underlyingCurveAsString)
        {
            return EnumHelper.Parse<UnderlyingCurveTypes>(underlyingCurveAsString);
        }

        #endregion

        #region Process and Value

        /// <summary>
        /// Generate an array of PricingStructurePoint from a set of input arrays
        /// The array can then be added to the Matrix
        /// </summary>
        /// <param name="expiry">Expiry values to use</param>
        /// <param name="strike">Strike values to use</param>
        /// <param name="volatility">An array of volatility values</param>
        /// <param name="strikeQuoteUnits">The strike quote units.</param>
        /// <param name="underlyingAssetReference">The underlying asset.</param>
        /// <returns></returns>
        protected PricingStructurePoint[] ProcessRawSurface(IList<String> expiry, Decimal? strike, IList<Decimal> volatility, PriceQuoteUnits strikeQuoteUnits, AssetReference underlyingAssetReference)
        {
            var expiryLength = expiry.Count;
            var pointIndex = 0;
            var points = new PricingStructurePoint[expiryLength];
            for (var expiryIndex = 0; expiryIndex < expiryLength; expiryIndex++)
            {
                // extract the current tenor (term) of null if there are no tenors
                // Extract the volatility
                var vol = volatility[expiryIndex];
                // Add the value to the points array (dataPoints entry in the matrix)
                var coordinates = new PricingDataPointCoordinate[1];
                if (strike != null)
                {
                    coordinates[0] =
                        PricingDataPointCoordinateFactory.Create(expiry[expiryIndex], null, (Decimal) strike);
                }
                else
                {
                    coordinates[0] = PricingDataPointCoordinateFactory.Create(expiry[expiryIndex], "ATMSurface");
                }
                var pt = new PricingStructurePoint
                {
                    value = vol,
                    valueSpecified = true,
                    coordinate = coordinates,
                    underlyingAssetReference = underlyingAssetReference,
                    quoteUnits = strikeQuoteUnits
                };
                points[pointIndex++] = pt;
            }
            return points;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public object[,] GetSurface()
        {
            // Assign the matrix size
            var points = GetVolatilityMatrix().dataPoints.point;
            var rawSurface = new object[points.Length + 1, 1];
            // Zeroth row will be the column headers
            rawSurface[0, 0] = "Option Expiry";
            // Add the data rows to the matrix
            var idx = 0;
            for (var row = 1; row < points.Length + 1; row++)
            {
                rawSurface[row, 0] = points[idx].coordinate.ToString();//Not sure about this.
                idx++;
            }
            return rawSurface;
        }

        #endregion        
    }
}
