#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using Core.Common;
using FpML.V5r3.Codes;
using Orion.Constants;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.LinearAlgebra;
using Orion.Analytics.Options;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.CurveEngine.PricingStructures.SABR;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class CapVolatilitySurface : CurveBase, IStrikeVolatilitySurface
    {
        #region Private Fields

        ///<summary>
        /// Gets and sets the algorithm.
        ///</summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Contains everything you'd want!
        /// </summary>
        public CapletSmileCalibrationEngine CalibrationEngine { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The asset referenced by the cap volaility curve.
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
        /// The underlying vurve to use for interpolation.
        /// </summary>
        public string UnderlyingInterpolatedCurve = "VolatilityCurve";

        /// <summary>
        /// The compounding frequency to use for interpolation.
        /// </summary>
        public CompoundingFrequencyEnum CompoundingFrequency { get; set; }

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        public List<IPriceableRateOptionAssetController> PriceableRateOptionAssets { get; set; }

        /// <summary>
        /// The bootstrapper name.
        /// </summary>
        public String BootstrapperName = "CapFloorBootstrapper";

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
        public InterpolationMethod InterpolationMethodInput = InterpolationMethodHelper.Parse("LinearInterpolation");

        /// <summary>
        /// 
        /// </summary>
        public bool ExtrapolationPermitted = true;

        /// <summary>
        /// 
        /// </summary>
        public bool IsCapletBootstrapSuccessful;

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
        /// 
        /// </summary>
        public InterpolationMethod InterpolationMethod = InterpolationMethodHelper.Parse("LinearInterpolation");

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
        public VolCurveBootstrapResults BootstrapResults { get; set; }

        /// <summary>
        /// Accessor method for the  discount factor offsets.
        /// </summary>
        /// <value>Array of decimals that contains the discount factor
        /// offsets.</value>
        public int[] VolatilityOffsets { get; set; }

        /// <summary>
        /// Accessor method for the  discount factor offsets.
        /// </summary>
        /// <value>Array of decimals that contains the discount factor
        /// offsets.</value>
        public DateTime[] VolatilityDates { get; set; }

        /// <summary>
        /// Accessor method for the  discount factor values.
        /// </summary>
        /// <value>Array of decimals that contains the discount factor
        /// values.</value>
        public Decimal[] VolatilityValues { get; set; }

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

        ///<summary>
        ///</summary>
        public string Handle { get; set; }

        /// <summary>
        /// The daycounter
        /// </summary>
        public IDayCounter DayCounter { get; set; }

        /// <summary>
        /// The underlying numeraire of the volaitility curve.
        /// This would normally be a Xibor type instrument for a cap volatility curve.
        /// </summary>
        public Tuple<Asset, BasicAssetValuation> UnderlyingAssetDetails { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets and sets members of the quoted asset set. Mainly for use the market data server.
        /// </summary>
        public QuotedAssetSet QuotedAssetSet
        {
            get => ((VolatilityMatrix)PricingStructureValuation).inputs;
            set => ((VolatilityMatrix)PricingStructureValuation).inputs = value;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Takes a range of volatilities, an array of tenor expiries and an 
        /// array of strikes to create a VolatilitySurface
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="expiryTenors"></param>
        /// <param name="strikes"></param>
        /// <param name="volSurface"></param>
        /// <param name="surfaceId"></param>
        /// <param name="logger"></param>
        /// <param name="cache">The cache.</param>
        public CapVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, String[] expiryTenors, Double[] strikes, Double[,] volSurface, VolatilitySurfaceIdentifier surfaceId)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates);
            Algorithm = surfaceId.Algorithm;
            PricingStructureIdentifier = surfaceId;
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, surfaceId.PricingStructureType, surfaceId.Algorithm);
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            //_matrixIndexHelper = new SortedList<ExpiryTermStrikeVolatilitySurface.ExpiryTenorStrikeKey, int>(new ExpiryTermStrikeVolatilitySurface.ExpiryTenorStrikeKey());
            var points = ProcessRawSurface(expiryTenors, strikes, volSurface, surfaceId.StrikeQuoteUnits, surfaceId.UnderlyingAssetReference);
            PricingStructure = CreateVolatilityRepresentation(surfaceId);
            PricingStructureValuation = CreateDataPoints(points, surfaceId);
            var expiries = new double[expiryTenors.Length];
            var index = 0;
            foreach (var term in expiryTenors)//TODO include business day holidays and roll conventions.
            {
                expiries[index] = PeriodHelper.Parse(term).ToYearFraction();
                index++;
            }
            // Generate an interpolator to use
            Interpolator = new VolSurfaceInterpolator(expiries, strikes, new Matrix(volSurface), curveInterpolationMethod, true);
            //TODO 
            //Bootstrap a cap/floor volatility surface.
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The data.</param>
        /// <param name="properties">The properties.</param>
        protected CapVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace, Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
        {
            var data = (VolatilityMatrix)fpmlData.Second;
            Algorithm = "Linear";//Add as a propert in the id.
            //Creates the property collection. This should be backward compatable with V1.
            var surfaceId = new VolatilitySurfaceIdentifier(properties);
            PricingStructureIdentifier = surfaceId;
            var holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, surfaceId.PricingStructureType, surfaceId.Algorithm);
            var curveInterpolationMethod = InterpolationMethodHelper.Parse(holder.GetValue("CurveInterpolation"));
            Interpolator = new VolSurfaceInterpolator(data.dataPoints, curveInterpolationMethod, true, PricingStructureIdentifier.BaseDate);
            SetFpmlData(fpmlData);
            //_matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            ProcessVolatilityRepresentation();
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
                string compoundingFrequency = properties.GetString("CompoundingFrequency", null);
                CompoundingFrequency = compoundingFrequency != null
                    ? EnumHelper.Parse<CompoundingFrequencyEnum>(compoundingFrequency)
                    : GetDefaultCompounding(algorithmHolder);
                //If there is an input propoert for extrapolation and interpolation then this overrides the algorithm configuration.
                var extrapolation = properties.GetString("ExtrapolationPermitted", null);
                ExtrapolationPermittedInput = extrapolation != null ? bool.Parse(extrapolation) : GetDefaultExtrapolationFlag(algorithmHolder);
                var interpolation = properties.GetString("BootstrapperInterpolation", null) ?? GetDefaultInterpolationMethod(algorithmHolder);
                InterpolationMethodInput = InterpolationMethodHelper.Parse(interpolation);
                Holder = algorithmHolder;
            }
            else
            {
                Tolerance = 10 ^ -9;
                CompoundingFrequency = CompoundingFrequencyEnum.Quarterly;
            }
        }

        #endregion

        #region Runtime Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CapVolatilitySurface"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing strucuture.</param>
        /// <param name="fixingCalendar">The fixingCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        public CapVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, new VolatilitySurfaceIdentifier(properties ?? PricingStructurePropertyHelper.VolatilityCurve(fpmlData)))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties);
            var curveId = GetCurveId();
            Initialize(properties, Holder);
            //Set the underlying asset information.
            if (properties != null)
            {
                var instrument = properties.GetString("Instrument", false);
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

        private static VolatilityRepresentation CreateVolatilityRepresentation(VolatilitySurfaceIdentifier surfaceId)
        {
            return new VolatilityRepresentation
            {
                name = surfaceId.Name,
                id = surfaceId.Id,
                currency = surfaceId.Currency,
                asset = new AnyAssetReference { href = surfaceId.Instrument },
            };
        }

        private static VolatilityMatrix CreateDataPoints(PricingStructurePoint[] points, VolatilitySurfaceIdentifier surfaceId)
        {
            var datapoints = new MultiDimensionalPricingData
            {
                point = points,
                businessCenter = surfaceId.BusinessCenter,
                timing = surfaceId.QuoteTiming,
                currency = surfaceId.Currency,
                cashflowType = surfaceId.CashflowType,
                informationSource = surfaceId.InformationSources,
                measureType = surfaceId.MeasureType,
                quoteUnits = surfaceId.QuoteUnits
            };
            if (surfaceId.ExpiryTime != null)
            {
                datapoints.expiryTime = (DateTime)surfaceId.ExpiryTime;
                datapoints.expiryTimeSpecified = true;
            }
            if (surfaceId.ValuationDate != null)
            {
                datapoints.valuationDate = (DateTime)surfaceId.ValuationDate;
                datapoints.valuationDateSpecified = true;
            }
            else
            {
                datapoints.valuationDate = surfaceId.BaseDate;
                datapoints.valuationDateSpecified = true;
            }
            if (surfaceId.Time != null)
            {
                datapoints.time = (DateTime)surfaceId.Time;
                datapoints.timeSpecified = true;
            }
            if (surfaceId.QuotationSide != null)
            {
                datapoints.side = (QuotationSideEnum)surfaceId.QuotationSide;
                datapoints.sideSpecified = true;
            }
            var pricingStructureValuation = new VolatilityMatrix
            {
                dataPoints = datapoints,
                objectReference = new AnyAssetReference { href = surfaceId.Instrument },
                baseDate = new IdentifiedDate { Value = surfaceId.BaseDate },
                buildDateTime = surfaceId.BuildDateTime,
                buildDateTimeSpecified = true,
                definitionRef = surfaceId.CapFrequency
            };
            return pricingStructureValuation;
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
            if (underlyingCurve == UnderyingCurveTypes.VolatilityCurve)
            {
                var interpolationMethod = InterpolationMethodInput ?? InterpolationMethod;
                var extrapolationPermitted = ExtrapolationPermittedInput ?? ExtrapolationPermitted;
                Interpolator = new VolCurveInterpolator(timesAndVolatilities, interpolationMethod, extrapolationPermitted);
            }
        }

        protected void SetInterpolator(MultiDimensionalPricingData dataPoints, string algorithm, PricingStructureTypeEnum psType)
        {
            var interpolationMethod = InterpolationMethodInput ?? InterpolationMethod;
            var extrapolationPermitted = ExtrapolationPermittedInput ?? ExtrapolationPermitted;
            Interpolator = new VolCurveInterpolator(dataPoints, interpolationMethod, extrapolationPermitted, GetBaseDate());
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

        private double GetDefaultTolerance(PricingStructureAlgorithmsHolder algorithms)
        {
            string tolerance = algorithms.GetValue(AlgorithmsProp.Tolerance);
            return double.Parse(tolerance);
        }

        private string GetDefaultInterpolationMethod(PricingStructureAlgorithmsHolder algorithms)
        {
            string interpolation = algorithms.GetValue(AlgorithmsProp.BootstrapperInterpolation);
            return interpolation;
        }

        private bool GetDefaultExtrapolationFlag(PricingStructureAlgorithmsHolder algorithms)
        {
            var extrapolation = algorithms.GetValue(AlgorithmsProp.ExtrapolationPermitted);
            return bool.Parse(extrapolation);
        }

        private CompoundingFrequencyEnum GetDefaultCompounding(PricingStructureAlgorithmsHolder algorithms)
        {
            string compounding = algorithms.GetValue(AlgorithmsProp.CompoundingFrequency);
            return EnumHelper.Parse<CompoundingFrequencyEnum>(compounding);
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measuretype to determine which one.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="values">The value to update to.</param>
        /// <param name="measureType">The measureType of the quotation required.</param>
        /// <returns></returns>
        public override bool PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, decimal[] values, string measureType)
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

        #region IPricingStructure Members

        /// <summary>
        /// Gets the curve id.
        /// </summary>
        /// <returns></returns>
        public VolatilitySurfaceIdentifier GetCurveId()
        {
            return (VolatilitySurfaceIdentifier)PricingStructureIdentifier;
        }

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        private void SetFpmlData(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
        }

        /// <summary>
        /// Gets the VolatilityMatrix.
        /// </summary>
        /// <returns></returns>
        public VolatilityMatrix GetVolatilityMatrix()
        {
            return (VolatilityMatrix)PricingStructureValuation;
        }

        /// <summary>
        /// Gets the VolatilityRepresentation.
        /// </summary>
        /// <returns></returns>
        public VolatilityRepresentation GetVolatilityRepresentation()
        {
            return (VolatilityRepresentation)PricingStructure;
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
            //            var result =Interpolator.GetDiscreteSpace().GetClosestValues(pt);

            //var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            //var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));

            IList<IValue> result = new List<IValue>();

            return result;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the algorithm.
        /// </summary>
        /// <returns></returns>
        public override string GetAlgorithm()
        {
            return Algorithm;
        }

        #endregion

        #region Process and Value

        /// <summary>
        /// Record the volatilities in the key helper and counts
        /// </summary>
        private void ProcessVolatilityRepresentation()
        {
            var row = 0;
            var column = 0;
            var columnExtra = 0;
            var strikeCheck = decimal.MinValue;
            var strike = decimal.MinValue;
            var dataPoints = GetVolatilityMatrix().dataPoints;
            var points = dataPoints.point;
            for (var idx = 0; idx < points.Length; idx++)
            {
                var tenor = points[idx].coordinate[0].term != null ? (Period)points[idx].coordinate[0].term[0].Items[0] : null;
                var strikeField = points[idx].coordinate[0].strike;
                if (strikeField != null)
                {
                    strike = strikeField[0];
                }
                if (idx == 0)
                {
                    strikeCheck = strike;
                    // Generate an interpolator to use
                    columnExtra = tenor == null ? 1 : 2;
                }
                else
                {
                    column++;
                    if (strike == strikeCheck)
                    {
                        row++;
                        column = 0;
                    }
                }
                var expiry = points[idx].coordinate[0].expiration[0].Items[0] as Period;
                if (expiry != null)
                {
                    //var expiry = (Period)points[idx].coordinate[0].expiration[0].Items[0];
                    //_matrixIndexHelper.Add(new ExpiryTermStrikeVolatilitySurface.ExpiryTenorStrikeKey(expiry, null, strike), idx);
                }
                var expiryAsDate = points[idx].coordinate[0].expiration[0].Items[0] is DateTime;
                if (expiryAsDate && dataPoints.valuationDateSpecified)
                {
                    var periodInDays = ((DateTime)points[idx].coordinate[0].expiration[0].Items[0] - dataPoints.valuationDate).Days;
                    expiry = new Period { period = PeriodEnum.D, periodMultiplier = periodInDays.ToString(CultureInfo.InvariantCulture), periodSpecified = true };
                    //_matrixIndexHelper.Add(new ExpiryTermStrikeVolatilitySurface.ExpiryTenorStrikeKey(expiry, null, strike), idx);
                }
            }
        }

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
        private PricingStructurePoint[] ProcessRawSurface(String[] expiry, Double[] strike, double[,] volatility, PriceQuoteUnits strikeQuoteUnits, AssetReference underlyingAssetReference)
        {
            var expiryLength = expiry.Length;
            var strikeLength = strike.Length;
            var pointIndex = 0;
            var points = new PricingStructurePoint[expiryLength * strikeLength];
            for (var expiryIndex = 0; expiryIndex < expiryLength; expiryIndex++)
            {
                // extract the current expiry
                var expiryKeyPart = expiry[expiryIndex];
                for (var strikeIndex = 0; strikeIndex < strikeLength; strikeIndex++)
                {
                    // Extract the strike to use in the helper key
                    var strikeKeyPart = strike[strikeIndex];
                    // extract the current tenor (term) of null if there are no tenors
                    // Extract the row,column indexed volatility
                    var vol = (decimal)volatility[expiryIndex, strikeIndex];
                    //var key = new ExpiryTermStrikeVolatilitySurface.ExpiryTenorStrikeKey(expiryKeyPart, strikeKeyPart);
                    //_matrixIndexHelper.Add(key, pointIndex);
                    // Add the value to the points array (dataPoints entry in the matrix)
                    var coordinates = new PricingDataPointCoordinate[1];
                    coordinates[0] = PricingDataPointCoordinateFactory.Create(expiry[expiryIndex], null, (Decimal)strike[strikeIndex]);
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
            }
            return points;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dimension1"></param>
        /// <param name="dimension2"></param>
        /// <returns></returns>
        public double GetValue(double dimension1, double dimension2)
        {
            IPoint point = new Point2D(dimension1, dimension2);
            return Interpolator.Value(point);
        }

        ///<summary>
        /// Returns the underlying surface data
        ///</summary>
        ///<returns></returns>
        public object[,] GetSurface()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="expirationAsDate"></param>
        ///<param name="strike"></param>
        ///<returns></returns>
        public double GetValueByExpiryDateAndStrike(DateTime valuationDate, DateTime expirationAsDate, double strike)
        {
            var value = 0d;

            if (PricingStructureEvolutionType == PricingStructureEvolutionType.ForwardToSpot)
            {
                var time = new DateTimePoint1D(valuationDate, expirationAsDate);
                IPoint point = new Point2D(time.GetX(), strike);
                value = Value(point);
                return value;
            }
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.SpotToForward)
            {
                var time = new DateTimePoint1D(GetBaseDate(), expirationAsDate);
                IPoint point = new Point2D(time.GetX(), strike);
                value = Value(point);
                return value;
            }

            return value;
        }

        ///<summary>
        ///</summary>
        ///<param name="term"></param>
        ///<param name="strike"></param>
        ///<returns></returns>
        public double GetValueByExpiryTermAndStrike(string term, double strike)
        {
            var expiryTerm = PeriodHelper.Parse(term).ToYearFraction();
            IPoint point = new Point2D(expiryTerm, strike);
            return Interpolator.Value(point);
        }

        #endregion
    }
}