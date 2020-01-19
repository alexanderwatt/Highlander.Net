#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.Common;
using Orion.CurveEngine.PricingStructures.Helpers;
using Orion.Util.Logging;
using Orion.Analytics.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Stochastics.SABR;
using Orion.Constants;
using Orion.CurveEngine.Assets.Helpers;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.Util.NamedValues;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.PricingStructures.SABR
{
    /// <summary>
    /// Interface layer between ExcelAPI and the underlying CapFloor analytics
    /// </summary>
    public class SABRInterface
    {
        #region Private Enumerations

        ///<summary>
        ///</summary>
        public enum CalibrationParameter
        {
            ///<summary>
            ///</summary>
            Alpha,
            ///<summary>
            ///</summary>
            Beta,
            ///<summary>
            ///</summary>
            Nu,
            ///<summary>
            ///</summary>
            Rho
        };

        #endregion

        #region Singleton Elements

        /// <summary>
        /// A collection of engines maintained by a dictionary indexed by the engineHandle
        /// The internal list comprises the individual engines sorted by strike.
        /// </summary>
        private readonly Dictionary<string, SortedList<decimal, VolatilityCurve>> _volCurveEngines;

        /// <summary>
        /// A collection of SABR calibration settings
        /// </summary>
        private readonly Dictionary<string, SmileCalibrationSettings> _sabrSettings;

        /// <summary>
        /// A collection of SABR Calibrated CapFloor engines
        /// </summary>
        private readonly Dictionary<string, SmileCalibrationEngine> _sabrEngines;

        /// <summary>
        /// Singleton instance controlling the interface
        /// </summary>
        private static SABRInterface _instance;

        #endregion

        #region Singleton Interface

        /// <summary>
        /// Protect this class from multiple instances
        /// </summary>
        private SABRInterface()
        {
            _volCurveEngines = new Dictionary<string, SortedList<decimal, VolatilityCurve>>();
            _sabrSettings = new Dictionary<string, SmileCalibrationSettings>();
            _sabrEngines = new Dictionary<string, SmileCalibrationEngine>();
        }

        /// <summary>
        /// Static Instance method to access this class
        /// </summary>
        /// <returns></returns>
        public static SABRInterface Instance()
        {
            return _instance ?? (_instance = new SABRInterface());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SABRInterface NewInstance()
        {
            return _instance = new SABRInterface();
        }

        #endregion

        #region Interface Methods

        #region Bootstrap Methods

        /// <summary>
        /// Create a settings object from the supplied settings object
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public NamedValueSet CreateVolatilityEngineProperties(object[,] settings)
        {
            var columns = settings.GetUpperBound(1);
            //This will only process a two column obect matrix.
            if (columns != 1) return null;
            var properties = (object[,])TrimNulls(settings);
            var namedValueSet = properties.ToNamedValueSet();
            return namedValueSet;
        }

        /// <summary>
        /// Create a series of Volatility engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateVolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties,
            String[] instruments, Decimal[,] rawVolatilityGrid)
        {
            var id = properties.GetString(CurveProp.EngineHandle, null);
            var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
            var valuationDate = properties.GetValue(CurveProp.ValuationDate, DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set(CurveProp.ValuationDate, baseDate);
            }
            var strikeQuoteUnits = properties.GetString(CurveProp.StrikeQuoteUnits, null);
            if (strikeQuoteUnits == null)
            {
                properties.Set(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString(CurveProp.MeasureType, null);
            if (measureType == null)
            {
                properties.Set(CurveProp.MeasureType, MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString(CurveProp.QuoteUnits, null);
            if (quoteUnits == null)
            {
                properties.Set(CurveProp.QuoteUnits, QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString(CurveProp.Algorithm, null);
            if (algorithm == null)
            {
                properties.Set(CurveProp.Algorithm, "Default");
            }
            var volatilities = GenerateVolatilityMatrix(instruments, 0, rawVolatilityGrid);
            // Create the engines and either add to, or overwrite the existing, collection
            if (_volCurveEngines.ContainsKey(id))
                _volCurveEngines[id] = VolatilityCurveCreate(logger, cache, nameSpace, properties, instruments, volatilities);
            else
                _volCurveEngines.Add(id, VolatilityCurveCreate(logger, cache, nameSpace, properties, instruments, volatilities));
            return id;
        }

        /// <summary>
        /// Create a series of Volatility engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of volatiltiy types with instruments.</param>
        /// <param name="strikes">An array of strikes.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateVolatilityCurves(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties,
            String[] instruments, Decimal[] strikes, Decimal[,] rawVolatilityGrid)
        {
            var id = properties.GetString(CurveProp.EngineHandle, null);
            var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
            var valuationDate = properties.GetValue(CurveProp.ValuationDate, DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set(CurveProp.ValuationDate, baseDate);
            }
            var strikeQuoteUnits = properties.GetString(CurveProp.StrikeQuoteUnits, null);
            if (strikeQuoteUnits == null)
            {
                properties.Set(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString(CurveProp.MeasureType, null);
            if (measureType == null)
            {
                properties.Set(CurveProp.MeasureType, MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString(CurveProp.QuoteUnits, null);
            if (quoteUnits == null)
            {
                properties.Set(CurveProp.QuoteUnits, QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString(CurveProp.Algorithm, null);
            if (algorithm == null)
            {
                properties.Set(CurveProp.Algorithm, "Default");
            }
            // Create the engines and either add to, or overwrite the existing, collection
            if (_volCurveEngines.ContainsKey(id))
                _volCurveEngines[id] = VolatilityCurvesCreate(logger, cache, nameSpace, properties, instruments, strikes, rawVolatilityGrid);
            else
                _volCurveEngines.Add(id, VolatilityCurvesCreate(logger, cache, nameSpace, properties, instruments, strikes, rawVolatilityGrid));
            return id;
        }

        /// <summary>
        /// Create a series of Volatility engines from a raw volatility grid
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateATMVolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties,
            String[] instruments, Decimal[] rawVolatilityGrid)
        {
            var volatilityCheck = CheckVolatilties(rawVolatilityGrid);
            if (volatilityCheck != null) throw new ArgumentException(volatilityCheck);
            var id = properties.GetString(CurveProp.EngineHandle, true);
            var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
            var valuationDate = properties.GetValue(CurveProp.ValuationDate, DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set(CurveProp.ValuationDate, baseDate);
            }
            var strikeQuoteUnits = properties.GetString(CurveProp.StrikeQuoteUnits, null);
            if (strikeQuoteUnits == null)
            {
                properties.Set(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString(CurveProp.MeasureType, null);
            if (measureType == null)
            {
                properties.Set(CurveProp.MeasureType, MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString(CurveProp.QuoteUnits, null);
            if (quoteUnits == null)
            {
                properties.Set(CurveProp.QuoteUnits, QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString(CurveProp.Algorithm, null);
            if (algorithm == null)
            {
                properties.Set(CurveProp.Algorithm, "Default");
            }
            // Create the engines and either add to, or overwrite the existing, collection
            if (_volCurveEngines.ContainsKey(id))
                _volCurveEngines[id] = VolatilityCurveCreate(logger, cache, nameSpace, properties, instruments, rawVolatilityGrid);
            else
                _volCurveEngines.Add(id, VolatilityCurveCreate(logger, cache, nameSpace, properties, instruments, rawVolatilityGrid));
            return id;
        }

        /// <summary>
        /// Compute a Volatility using a bootstrapped engine and the set of identifying parameters
        /// </summary>
        /// <param name="engineHandle">The engine handle to the engines group to use</param>
        /// <param name="strike">The strike at which to compute the volatility</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="target">The target date to compute the volatility at</param>
        /// <returns></returns>
        public decimal BootstrapComputeVolatility(string engineHandle, decimal strike, DateTime baseDate, DateTime target)
        {
            return BootstrapComputeVolatility(engineHandle, strike, baseDate, new[] {target}).Single();
        }

        /// <summary>
        /// Compute a Volatility using a bootstrapped engine and the set of identifying parameters
        /// </summary>
        /// <param name="engineHandle">The engine handle to the engines group to use</param>
        /// <param name="strike">The strike at which to compute the volatility</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targets">The target dates to compute the volatilities at</param>
        /// <returns></returns>
        public IList<decimal> BootstrapComputeVolatility(string engineHandle, decimal strike, DateTime baseDate, DateTime[] targets)
        {
            if (!_volCurveEngines.ContainsKey(engineHandle))
            {
                throw new ArgumentException(
                    $"The engine: {engineHandle} is not present. The volatility cannot be computed.");
            }
            //// Set up the parameters to use for this Computation
            var engines = _volCurveEngines[engineHandle];
            if (engines == null) return null;
// Isolate the correct engine from the engine group - fixed strike case
            if (!engines.ContainsKey(strike))
            {
                throw new ArgumentException($"The strike value: {strike} is not valid for this Bootstrapper.");
            }
            var result = new List<Decimal>();
            var engine = engines[strike];
            if (engine != null)
            {
                foreach (var date in targets)
                {
                    var time = new DateTimePoint1D(baseDate, date);
                    var value = (decimal) engine.GetValue(time.GetX());
                    result.Add(value);
                }
                return result;
            }
            return null;
        }

        #endregion

        #region SABR Calibration

        /// <summary>
        /// Create a settings object to use with SABR Calibration
        /// </summary>
        /// <param name="settingsHandle">The settings engine</param>
        /// <param name="beta">The beta value</param>
        /// <param name="interpolationType">Interpolation type to use</param>
        /// <returns></returns>
        public string SABRCalibrationSettings(string settingsHandle, decimal beta, string interpolationType)
        {
            // Extract the correct interpolation type from the parameter
            var volatilityInterpolation = ExpiryInterpolationType.CubicHermiteSpline;
            if (!string.IsNullOrEmpty(interpolationType))
            {
                var found = false;
                var interps = Enum.GetValues(typeof(ExpiryInterpolationType));
                foreach (ExpiryInterpolationType interpType in interps)
                {
                    if (interpType.ToString() != interpolationType) continue;
                    volatilityInterpolation = interpType;
                    found = true;
                    break;
                }
                if (!found)
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown interpolation method {0} specified.", new object[] { interpolationType }));
            }
            // Create a new Caplet Smile Calibration Settings object and store it
            var settings = new SmileCalibrationSettings(beta, volatilityInterpolation, settingsHandle);
            if (_sabrSettings.ContainsKey(settingsHandle))
                _sabrSettings[settingsHandle] = settings;
            else
                _sabrSettings.Add(settingsHandle, settings);
            return settingsHandle;
        }

        /// <summary>
        /// Create a SABR CapFloor Calibration Engine
        /// </summary>
        /// <param name="engineHandle">The handle for this engine</param>
        /// <param name="settingsHandle">The handle to the settings object we use for this calibration</param>
        /// <param name="bootstrapFixedStrikeHandle">The fixed strike engines to use as a base</param>
        /// <param name="bootstrapATMHandle">The ATM engine used in Calibration</param>
        /// <returns>The handle of this engine</returns>
        public string SABRCalibrationEngine(string engineHandle, string settingsHandle, string bootstrapFixedStrikeHandle, string bootstrapATMHandle)
        {
            // The ATM CapFloor bootstrap engine is the 0th strike CapFloor engine with this handle
            VolatilityCurve atmEngine;
            if (_volCurveEngines.ContainsKey(bootstrapATMHandle))
                atmEngine = _volCurveEngines[bootstrapATMHandle][0];
            else
                throw new ArgumentException("ATM Bootstrap engine not found.");
            SortedList<decimal, VolatilityCurve> fixedStrikeEngines;
            if (_volCurveEngines.ContainsKey(bootstrapFixedStrikeHandle))
                fixedStrikeEngines = _volCurveEngines[bootstrapFixedStrikeHandle];
            else
                throw new ArgumentException("Fixed Strike Bootstrap engines not found.");
            SmileCalibrationSettings settings;
            if (_sabrSettings.ContainsKey(settingsHandle))
                settings = _sabrSettings[settingsHandle];
            else
                throw new ArgumentException("Smile Settings not found.");
            var engine = new SmileCalibrationEngine(atmEngine, settings, new List<VolatilityCurve>(fixedStrikeEngines.Values), engineHandle);
            if (_sabrEngines.ContainsKey(engineHandle))
                _sabrEngines[engineHandle] = engine;
            else
                _sabrEngines.Add(engineHandle, engine);
            return engineHandle;
        }

        /// <summary>
        /// Compute a Volatility Smile given an engine, a date and a list of strikes
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <param name="engineHandle">The SABR engine to use</param>
        /// <param name="date">The date to compute for</param>
        /// <param name="strikes">The strikes that make up the smile. This can be a single strike (ATM)</param>
        /// <returns></returns>
        public List<Decimal> SABRComputeVolatility(ILogger logger, ICoreCache cache, string nameSpace, string engineHandle, DateTime date, decimal[] strikes)
        {
            SmileCalibrationEngine engine;
            if (_sabrEngines.ContainsKey(engineHandle))
                engine = _sabrEngines[engineHandle];
            else
                throw new ArgumentException("SABR Caplet Smile Calibration engine not found.");
            var strikesList = new List<decimal>(strikes);
            var computedValues = engine.ComputeVolatilitySmile(logger, cache, nameSpace, date, strikesList);
            return computedValues;
        }

        #endregion

        #region Engine Utility Methods

        /// <summary>
        /// List all existing bootstrap engines
        /// This method will list any created using Bootstrap methods
        /// and will check for and generate any engines retrieved by subscription
        /// </summary>
        /// <returns></returns>
        public String[] ListBootstrapEngines()
        {
            if (_volCurveEngines == null || _volCurveEngines.Keys.Count == 0)
            {
                return new [] {"No SABR Volatility Surfaces located."};
            }
            return _volCurveEngines.Keys.ToArray();
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a Sortedlist of volatility curves from the data matrix
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties, including the engine handle.</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns></returns>
        private SortedList<decimal, VolatilityCurve> VolatilityCurveCreate(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, String[] instruments,
            Decimal[] rawVolatilityGrid)
        {
            var clonedProperties = properties.Clone();
            // Create engine
            var quotedAssetSet = AssetHelper.Parse(instruments, rawVolatilityGrid, null);
            var engines = new SortedList<decimal, VolatilityCurve>();
            // Create a new ATM CapletBootstrap engine. The default decimal should be 0
            var volatilityCurve = PricingStructureFactory.CreateCurve(logger, cache, nameSpace, null, null, clonedProperties, quotedAssetSet);
            // Add engine
            if (volatilityCurve is VolatilityCurve vCurve)
            {
                engines.Add(0, vCurve);
            }
            return engines;
        }

        /// <summary>
        /// Create a Sortedlist of bootstrap engines from the data matrix
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties, including the engine handle.</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="strikes">The strike array.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns></returns>
        private SortedList<decimal, VolatilityCurve> VolatilityCurvesCreate(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, String[] instruments, 
            Decimal[] strikes, Decimal[,] rawVolatilityGrid)
        {
            var clonedProperties = properties.Clone();           
            // Check there are valid strikes
            if (strikes != null && rawVolatilityGrid != null)
            {
                //The matrix is zero based, but the upper bound returns the last column index.
                var volColumns = rawVolatilityGrid.GetUpperBound(1) + 1;
                var columns = Math.Min(strikes.Length, volColumns);           
                var engines = new SortedList<decimal, VolatilityCurve>();
                // Loop through the strikes to create each new engine
                for(var i = 0; i < columns; i++)
                {
                    decimal[] adjustedRates = GenerateVolatilityMatrix(instruments, i, rawVolatilityGrid);
                    clonedProperties.Set("Strike", strikes[i]);
                    clonedProperties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.DecimalRate.ToString());
                    // Create engine
                    var quotedAssetSet = AssetHelper.Parse(instruments, adjustedRates, null);
                    var engine = PricingStructureFactory.CreateCurve(logger, cache, nameSpace, null, null, clonedProperties, quotedAssetSet) as VolatilityCurve;
                    // Add engine
                    engines.Add(strikes[i], engine);
                }
                return engines;
            }
            {
                var engines = new SortedList<decimal, VolatilityCurve>();
                // Create a new ATM CapletBootstrap engine. The default decimal should be 0
                var engine = PricingStructureFactory.CreateCurve(logger, cache, nameSpace, null, null, clonedProperties,
                    instruments, null, null) as VolatilityCurve;
                // Add engine
                engines.Add(0, engine);
                return engines;
            }
        }

        #endregion

        #region SABR Parameters

        /// <summary>
        /// Get the SABR Calibration engine parameter for the provided options
        /// The exercise/tenor pair form the key to the correct calibration engine
        /// from an underlying store.
        /// </summary>
        /// <param name="param">The parameter type to return</param>
        /// <param name="engine">The engine to use</param>
        /// <returns></returns>
        public static decimal GetSABRParameter(string param, string engine)
        {
            var typeofParam = (CalibrationParameter)Enum.Parse(typeof(CalibrationParameter), param);
            return Instance().GetSABRParameter(typeofParam, engine);
        }

        /// <summary>
        /// Get the SABR Calibration engine parameter for the provided options
        /// The exercise/tenor pair form the key to the correct calibration engine
        /// from an underlying store.
        /// </summary>
        /// <param name="param">The parameter type to return</param>
        /// <param name="engineHandle">The engine to use</param>
        /// <returns></returns>
        public decimal GetSABRParameter(CalibrationParameter param, string engineHandle)
        {
            SmileCalibrationEngine smileEngine;
            if (_sabrEngines.ContainsKey(engineHandle))
                smileEngine = _sabrEngines[engineHandle];
            else
                throw new ArgumentException("SABR Smile Calibration engine not found.");
            var calibrationEngine = smileEngine.SABRCalibrationEngine;
            SABRParameters parameters = calibrationEngine.GetSABRParameters;
            switch (param)
            {
                case CalibrationParameter.Alpha:
                    return parameters.Alpha;
                case CalibrationParameter.Beta:
                    return parameters.Beta;
                case CalibrationParameter.Nu:
                    return parameters.Nu;
                case CalibrationParameter.Rho:
                    return parameters.Rho;
                default:
                    throw new ArgumentException("Unknown Calibration Parameter request");
            }
        }

        /// <summary>
        /// Get the Calibration status for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public static bool IsModelCalibrated(string engine, string expiry, string tenor)
        {
            return Instance().IsSABRModelCalibrated(engine);
        }

        /// <summary>
        /// Get the Calibration status for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engineHandle">The calibration engine handle</param>
        /// <returns></returns>
        public bool IsSABRModelCalibrated(string engineHandle)
        {
            if (!_sabrEngines.ContainsKey(engineHandle)) return false;
            SmileCalibrationEngine smileEngine = _sabrEngines[engineHandle];
            var calibrationEngine = smileEngine.SABRCalibrationEngine;
            return calibrationEngine.IsSABRModelCalibrated;
        }

        /// <summary>
        /// Get the Calibration Engine Calibration Error the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public static decimal CalibrationError(string engine, string expiry, string tenor)
        {
            return Instance().SABRCalibrationError(engine);
        }

        /// <summary>
        /// Get the Calibration Engine Calibration Error the expiry/tenor pair
        /// </summary>
        /// <param name="engineHandle">The calibration engine handle</param>
        /// <returns></returns>
        public decimal SABRCalibrationError(string engineHandle)
        {
            if (!_sabrEngines.ContainsKey(engineHandle)) throw new ArgumentException("SABR Smile Calibration engine not found.");
            SmileCalibrationEngine smileEngine = _sabrEngines[engineHandle];
            var calibrationEngine = smileEngine.SABRCalibrationEngine;
            return calibrationEngine.CalibrationError;
        }

        #endregion

        #region Utility methods

        private string CheckVolatilties(Decimal[] rawVolatilityGrid)
        {
            foreach (var vol in rawVolatilityGrid)
            {
                if (vol < 0) return "An illegal volatility value";
            }
            return null;
        }

        /// <summary>
        /// Build up the matrix of capfloor volatilities (expiry x strike)
        /// </summary>
        /// <param name="column">The volaility to extract.</param>
        /// <param name="instrument">The type of the volatility surface at each expiry</param>
        /// <param name="volatility">A 2d array of the volatilities</param>
        private decimal[] GenerateVolatilityMatrix(string[] instrument, int column, decimal[,] volatility)
        {
            if (column > volatility.GetLongLength(1)) return null;
            var maxRows = Math.Min(instrument.Length, volatility.GetLongLength(0));
            var result = new List<decimal>();
            for (var row = 0; row < maxRows; row++)
            {
                result.Add(volatility[row, column]);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static Array TrimNulls(Array array)
        {
            //  find the last non null row
            //
            int index1LowerBound = array.GetLowerBound(1);
            int nonNullRows = 0;
            for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); ++i)
            {
                object o = array.GetValue(i, index1LowerBound);
                if (o == null || o.ToString() == String.Empty)
                    break;
                ++nonNullRows;
            }
            //  get the element type
            //
            Type elementType = array.GetType().GetElementType();
            if (elementType == null) return null;
            {
                var result = Array.CreateInstance(elementType, new[] { nonNullRows, array.GetLength(1) },
                    new[] { array.GetLowerBound(0), array.GetLowerBound(1) });
                for (int i = array.GetLowerBound(0); i < array.GetLowerBound(0) + nonNullRows; ++i)
                {
                    for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); ++j)
                    {
                        var value = array.GetValue(i, j);
                        result.SetValue(value, i, j);
                    }
                }
                return result;
            }
        }

        #endregion
    }
}