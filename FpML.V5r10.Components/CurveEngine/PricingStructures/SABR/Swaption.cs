#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FpML.V5r10.Reporting;
using Orion.Analytics.Interpolations;
using Orion.Analytics.PricingEngines;
using Orion.Analytics.Rates;
using Orion.Analytics.Stochastics.SABR;
using Orion.Analytics.Stochastics.Volatilities;
using Orion.Analytics.Utilities;

#endregion

namespace Orion.CurveEngine.PricingStructures.SABR
{
    public class Swaption
    {
        #region Private Enumerations

        public enum CalibrationParameter
        {
            Alpha,
            Beta,
            Nu,
            Rho
        }

        #endregion

        #region Static Collections

        /// <summary>
        /// Persistent storage for engines created by spreadsheets
        /// </summary>
        private readonly Dictionary<string, SortedDictionary<SABRKey, SABRCalibrationEngine>> _sabrEngines;

        /// <summary>
        /// Convenience storage for Forward Rates where one is stored with the Calibration engine during creation
        /// </summary>
        private readonly Dictionary<string, ForwardRatesMatrix> _engineRatesGrid;

        /// <summary>
        /// Persistent storage for SABR settings objects used by SABR engines
        /// </summary>
        private readonly Dictionary<string, SABRCalibrationSettings> _sabrSettings;

        /// <summary>
        /// Singleton instance controlling the interface
        /// </summary>
        private static Swaption _instance;

        #endregion

        #region Singleton Interface

        /// <summary>
        /// Protect this class from multiple instances
        /// </summary>
        private Swaption()
        {
            _sabrEngines = new Dictionary<string, SortedDictionary<SABRKey, SABRCalibrationEngine>>();
            _engineRatesGrid = new Dictionary<string, ForwardRatesMatrix>();
            _sabrSettings = new Dictionary<string, SABRCalibrationSettings>();
        }

        /// <summary>
        /// Static Instance method to access this class
        /// </summary>
        /// <returns></returns>
        public static Swaption Instance()
        {
            return _instance ?? (_instance = new Swaption());
        }

        #endregion

        #region Interface Methods

        #region Calibration Settings

        /// <summary>
        /// Create a new Calibration Settings object. Each new object holds the instrument, currency and beta
        /// to use for this settings object. Each new object is stored internally using the supplied handle
        /// as an identifying key.
        /// The Settings object is used by a Calibration Engine to define base parameters on which to calibrate.
        /// </summary>
        /// <param name="handle">A <see cref="SABRCalibrationSettings"/> handle</param>
        /// <param name="instrument">The instrument type to use</param>
        /// <param name="ccy">The currency setting</param>
        /// <param name="beta">The Beta parameter to use</param>
        /// <returns></returns>
        public string SabrAddCalibrationSetting(string handle, string instrument, string ccy, decimal beta)
        {
            // Setup some defaults for the currency and instrument settings
            var calibrationInstrument = InstrumentType.Instrument.Swaption;
            // Identify the instrument used in this settings object
            Array instruments = Enum.GetValues(typeof(InstrumentType.Instrument));
            foreach (InstrumentType.Instrument s in instruments)
            {
                if (s.ToString() == instrument)
                {
                    calibrationInstrument = s;
                    break;
                }
            }
            // Create a new calibration settings object using the instrument/ccy/beta grouping
            var newCalibration =
                new SABRCalibrationSettings(handle, calibrationInstrument, ccy, beta);
            // Test whether the CalibrationSettings is already present
            // If not already present then add it
            if (_sabrSettings.ContainsKey(handle))
                _sabrSettings[handle] = newCalibration;
            else
                _sabrSettings.Add(handle, newCalibration);
            return handle;
        }

        /// <summary>
        /// Create a new Calibration Settings object. Each new object holds the instrument, currency and beta
        /// to use for this settings object. Each new object is stored internally using the supplied handle
        /// as an identifying key.
        /// The Settings object is used by a Calibration Engine to define base parameters on which to calibrate.
        /// </summary>
        /// <param name="handles">A handle</param>
        /// <param name="instrument">The instrument type to use</param>
        /// <param name="ccy">The currency setting</param>
        /// <param name="betas">The Beta parameter to use</param>
        /// <returns></returns>
        public string[] SabrAddCalibrationSettings(string[] handles, string instrument, string ccy, decimal[] betas)
        {
            // Setup some defaults for the currency and instrument settings
            var calibrationInstrument = InstrumentType.Instrument.Swaption;
            // Identify the instrument used in this settings object
            Array instruments = Enum.GetValues(typeof(InstrumentType.Instrument));
            foreach (InstrumentType.Instrument s in instruments)
            {
                if (s.ToString() == instrument)
                {
                    calibrationInstrument = s;
                    break;
                }
            }

            for (int i = 0; i < handles.Length; i++)
            {
                string handle = handles[i];
                // Create a new calibration settings object using the instrument/ccy/beta grouping
                var newCalibration =
                    new SABRCalibrationSettings(handle, calibrationInstrument, ccy, betas[i]);
                // Test whether the CalibrationSettings is already present
                // If not already present then add it
                if (_sabrSettings.ContainsKey(handle))
                    _sabrSettings[handle] = newCalibration;
                else
                    _sabrSettings.Add(handle, newCalibration);
            }
            return handles;
        }

        #endregion

        #region Calibration Engines

        /// <summary>
        /// Generate a new set of full calibration engines for the supplied data.
        /// Add or overwrite the engine store for the new engine.
        /// Each engineId will point to a set of engines indexed by swap tenor and option expiry
        /// Calibration assumes that the format of the grid data is as follows:
        /// 
        ///     + XXXX |  lbl0  |  lbl1  | ... |  lbln  +
        ///     | lbl0 | d[0,0] | d[0,1] | ... | d[0,n] |
        ///     | lbl1 | d[1,0] | d[1,1] | ... | d[1,n] |
        ///     | ...  |   ...  |   ...  | ... |   ...  |
        ///     + lbln | d[n,0] | d[n,1] | ... | d[n,n] +
        /// 
        /// </summary>
        /// <param name="engineHandle">Calibration Engine handle</param>
        /// <param name="settingsHandle">Calibartion settings handle</param>
        /// <param name="rawVols">A grid of volatilities (with row/column labels)</param>
        /// <param name="rawAssets">A grid of asset values</param>
        /// <param name="optionEx">The ption expiry to index against</param>
        /// <returns></returns>
        public string SABRCalibrateModel(string engineHandle, string settingsHandle, object[,] rawVols, object[,] rawAssets, string optionEx)
        {
            // Create the asset and volatility data grids
            SwaptionDataMatrix volatilityGrid = ParseVolatilityInput(rawVols, optionEx);
            ForwardRatesMatrix assetGrid = ParseAssetInputWithInterpolation(rawAssets);
            // Retrieve the calibration settings to use with this calibration engine
            if (!_sabrSettings.ContainsKey(settingsHandle))
            {
                throw new ArgumentException($"Configuration '{settingsHandle}' has not been set up.");
            }
            SABRCalibrationSettings settings = _sabrSettings[settingsHandle];
            // Generate the CalibrationEngine Id
            string calibrationEngineId = engineHandle;
            string optionExpiry = GenerateTenorLabel(optionEx);
            // Create a new engine holder object
            SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngine =
                BuildEngineCollection(volatilityGrid, assetGrid, settings, calibrationEngineId, optionExpiry);
            // We have an asset grid (forward rates) with this engine type so we should keep it
            // for future reference (that is during the lifetime of this session)
            if (_engineRatesGrid.ContainsKey(engineHandle))
                _engineRatesGrid[engineHandle] = assetGrid;
            else
                _engineRatesGrid.Add(engineHandle, assetGrid);

            // Add the SABREngine to the persistent store
            if (_sabrEngines.ContainsKey(calibrationEngineId))
                _sabrEngines[calibrationEngineId] = sabrEngine;
            else
                _sabrEngines.Add(calibrationEngineId, sabrEngine);
            return engineHandle;
        }

        /// <summary>
        /// Generate an ATM (At-The-Money) Swaption Calibration engine using the supplied parameters
        /// This form of engine creates a single cell engine that does not support asset/volatility grid data.
        /// </summary>
        /// <param name="engineHandle">The engine identifier</param>
        /// <param name="settingsHandle">The settings identifier</param>
        /// <param name="nu">Nu value</param>
        /// <param name="rho">Rho value</param>
        /// <param name="atmVolatility">The ATM volatility</param>
        /// <param name="assetPrice">Asset Price to use</param>
        /// <param name="exerciseTime">Exercise time for the option</param>
        /// <returns></returns>
        public string SABRCalibrateATMModel(string engineHandle, string settingsHandle, decimal nu, decimal rho, decimal atmVolatility, decimal assetPrice, string exerciseTime)
        {
            // Create the parameters used in this engine
            if (!_sabrSettings.ContainsKey(settingsHandle))
            {
                throw new ArgumentException($"Configuration '{settingsHandle}' has not been set up.");
            }
            SABRCalibrationSettings settings = _sabrSettings[settingsHandle];
            // Create the engine
            SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngine = BuildEngineCollection(settings, engineHandle, nu, rho,
                                                                                                atmVolatility, assetPrice, GenerateTenorLabel(exerciseTime), null);
            // Add the SABREngine to the persistent store
            if (_sabrEngines.ContainsKey(engineHandle))
                _sabrEngines[engineHandle] = sabrEngine;
            else
                _sabrEngines.Add(engineHandle, sabrEngine);
            return engineHandle;
        }

        /// <summary>
        /// Generate an ATM (At-The-Money) Swaption Calibration engine using the supplied parameters
        /// This form of engine creates a single cell engine that does not support asset/volatility grid data.
        /// </summary>
        /// <param name="engineHandle">The engine identifier</param>
        /// <param name="settingsHandle">The settings identifier</param>
        /// <param name="nu">Nu value</param>
        /// <param name="rho">Rho value</param>
        /// <param name="atmVolatility">The ATM volatility</param>
        /// <param name="assetPrice">Asset Price to use</param>
        /// <param name="exerciseTime">Exercise time for the option</param>
        /// <returns></returns>
        public string[] SABRCalibrateATMModels(string[] engineHandle, string[] settingsHandle, decimal[] nu, decimal[] rho, 
            decimal[] atmVolatility, decimal[] assetPrice, string[] exerciseTime)
        {
            for (int i = 0; i < engineHandle.Length; i++)
            {
                // Create the parameters used in this engine
                if (!_sabrSettings.ContainsKey(settingsHandle[i]))
                {
                    throw new ArgumentException(string.Format("Configuration '{0}' has not been set up.", settingsHandle));
                }
                SABRCalibrationSettings settings = _sabrSettings[settingsHandle[i]];
                string tenor = GenerateTenorLabel(exerciseTime[i]);
                // Create the engine
                SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngine 
                    = BuildEngineCollection(settings, engineHandle[i], nu[i], rho[i], atmVolatility[i], assetPrice[i], tenor, null);
                // Add the SABREngine to the persistent store
                if (_sabrEngines.ContainsKey(engineHandle[i]))
                    _sabrEngines[engineHandle[i]] = sabrEngine;
                else
                    _sabrEngines.Add(engineHandle[i], sabrEngine);
            }

            return engineHandle;
        }

        /// <summary>
        /// Generate an ATM (At-The-Money) Swaption Calibration engine using the supplied parameters
        /// This form of engine creates a single cell engine that does not support asset/volatility grid data.
        /// </summary>
        /// <param name="engineHandle">The engine identifier</param>
        /// <param name="settingsHandle">The settings identifier</param>
        /// <param name="nu">Nu value</param>
        /// <param name="rho">Rho value</param>
        /// <param name="atmVolatility">The ATM volatility</param>
        /// <param name="assetPrice">Asset Price to use</param>
        /// <param name="exerciseTime">Exercise time for the option</param>
        /// <param name="assetCode">The asset code. </param>
        /// <returns></returns>
        public string SABRCalibrateATMModel(string engineHandle, string settingsHandle, decimal nu, decimal rho, decimal atmVolatility, decimal assetPrice, string exerciseTime, string assetCode)
        {
            // Create the parameters used in this engine
            if (!_sabrSettings.ContainsKey(settingsHandle))
            {
                throw new ArgumentException($"Configuration '{settingsHandle}' has not been set up.");
            }
            SABRCalibrationSettings settings = _sabrSettings[settingsHandle];
            // Create the engine
            SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngine = BuildEngineCollection(settings, engineHandle, nu, rho, atmVolatility,
                                                                                                assetPrice, GenerateTenorLabel(exerciseTime), GenerateTenorLabel(assetCode));
            // Add the SABREngine to the persistent store
            if (_sabrEngines.ContainsKey(engineHandle))
                _sabrEngines[engineHandle] = sabrEngine;
            else
                _sabrEngines.Add(engineHandle, sabrEngine);
            return engineHandle;
        }

        /// <summary>
        /// Create a CalibrationEngine using an interpolation across existing CalibrationEngines.
        /// The calibrationArray has the handles of the Calibrated Engines to use in the interpolation processs.
        /// The interpolation can only be used in the ExpiryTime dimension to generate a new engine.
        /// If the engine array refers to unknown engines the process will fail (TBD)
        /// </summary>
        /// <param name="engineHandle">The name of the new interpolated calibration engine</param>
        /// <param name="settingsHandle">The settings to use in calibrating the engine</param>
        /// <param name="calibrationArray">The array of engine handles to use</param>
        /// <param name="atmVolatility">The At The Money volatility to use in engine calibration</param>
        /// <param name="assetPrice">The Asset price to use</param>
        /// <param name="exerciseTime">The exercise time to create the new engine for</param>
        /// <param name="tenor">The tenor to create the new engine for. This must be a valid tenor</param>
        /// <returns>The engine handle</returns>
        public string SABRCalibrateInterpolatedModel(string engineHandle, string settingsHandle, string[] calibrationArray, decimal atmVolatility, decimal assetPrice, string exerciseTime, string tenor)
        {
            SortedDictionary<SABRKey, SABRCalibrationEngine> engines = null;
            if (!_sabrSettings.ContainsKey(settingsHandle))
            {
                throw new ArgumentException($"Configuration '{settingsHandle}' has not been set up.");
            }
            SABRCalibrationSettings settings = _sabrSettings[settingsHandle];
            foreach (string handle in calibrationArray)
            {
                ExtractAllEngines(ref engines, handle);
            }
            decimal pATM = atmVolatility;
            decimal pAsset = assetPrice;
            // Create the new InterpolatedCalibrationEngine and add to the SABREngineCollection
            SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngine = BuildEngineCollection(settings, engineHandle,
                                                                                                ref engines, pATM, pAsset, exerciseTime, tenor);
            // Add the SABREngine to the persistent store
            if (_sabrEngines.ContainsKey(engineHandle))
                _sabrEngines[engineHandle] = sabrEngine;
            else
                _sabrEngines.Add(engineHandle, sabrEngine);
            return engineHandle;
        }

        #endregion

        #region Compute Volatility

        /// <summary>
        /// Calculate the SABR implied volatility for the strike value.
        /// This method uses the calibration engine indexed by the exerciseTime/assetCode pair
        /// When an ATM engine is used then the assetCode is ignored.
        /// </summary>
        /// <param name="engineHandle">The CalibrationEngine to use</param>
        /// <param name="exerciseTime">Option Expiry index</param>
        /// <param name="assetCode">Swap Tenor index</param>
        /// <param name="strike">The strike to calculate Volatility for</param>
        /// <returns></returns>
        public decimal SABRInterpolateVolatility(string engineHandle, string exerciseTime, string assetCode, decimal strike)
        {
            decimal result = 0;
            string errmsg = "";
            // Only process if the engine object holder holds the engine for this exercise/asset key
            if (_sabrEngines.ContainsKey(engineHandle))
            {
                // Extract the information we need from this engine holder
                SortedDictionary<SABRKey, SABRCalibrationEngine> engine = _sabrEngines[engineHandle];
                var key = new SABRKey(GenerateTenorLabel(exerciseTime), GenerateTenorLabel(assetCode));
                // Check that the engine for this exercise/asset key exists
                if (engine.ContainsKey(key))
                {
                    SABRCalibrationEngine calibrationEngine = engine[key];
                    if (!calibrationEngine.IsSABRModelCalibrated)
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "SABR Engine with key: {0}:{1} is not calibrated", new object[] { key.Expiry, key.Tenor }));
                    // Create SABRImpliedVolatility parameters
                    decimal assetPrice = _engineRatesGrid.ContainsKey(engineHandle) ? _engineRatesGrid[engineHandle].GetAssetPrice(exerciseTime, assetCode) : calibrationEngine.AssetPrice;
                    decimal strikeValue = strike;
                    decimal expiry = GenerateDayValue(exerciseTime, 365.0m);
                    // build an ImpliedVolatility object
                    SABRParameters parameters = calibrationEngine.GetSABRParameters;
                    var vol = new SABRImpliedVolatility(parameters, true);
                    // value the strike (interpolate as necessary)
                    if (!vol.SABRInterpolatedVolatility(assetPrice, expiry, strikeValue, ref errmsg, ref result, true))
                        throw new NotSupportedException(errmsg);
                }
                else
                    throw new ArgumentException("The Calibration Engine with Key(" + exerciseTime + "," + assetCode + ") not found.", nameof(exerciseTime));
            }
            else
                throw new ArgumentException("Calibration Engine " + engineHandle + " not found.", nameof(engineHandle));
            return result;
        }

        /// <summary>
        /// Calculate the SABR implied volatility for the strike value.
        /// This method uses the calibration engine indexed by the exerciseTime/assetCode pair
        /// When an ATM engine is used then the assetCode is ignored.
        /// </summary>
        /// <param name="engineHandle">The CalibrationEngine to use</param>
        /// <param name="exerciseTime">Option Expiry index</param>
        /// <param name="assetCode">Swap Tenor index</param>
        /// <param name="strikes">The strikes to calculate Volatility for</param>
        /// <returns></returns>
        public decimal[] SABRInterpolateVolatilities(string engineHandle, string exerciseTime, string assetCode,
                                                     decimal[] strikes)
        {
            // Only process if the engine object holder holds the engine for this exercise/asset key
            if (!_sabrEngines.ContainsKey(engineHandle))
            {
                throw new System.Exception("Calibration Engine " + engineHandle + " not found.");
            }
            // Extract the information we need from this engine holder
            SortedDictionary<SABRKey, SABRCalibrationEngine> engine = _sabrEngines[engineHandle];
            var key = new SABRKey(GenerateTenorLabel(exerciseTime), GenerateTenorLabel(assetCode));
            // Check that the engine for this exercise/asset key exists
            if (!engine.ContainsKey(key))
            {
                throw new System.Exception("The Calibration Engine with Key(" + exerciseTime + "," + assetCode + ") not found.");
            }
            SABRCalibrationEngine calibrationEngine = engine[key];
            if (!calibrationEngine.IsSABRModelCalibrated)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                          "SABR Engine with key: {0}:{1} is not calibrated",
                                                          new object[] { key.Expiry, key.Tenor }));
            // Create SABRImpliedVolatility parameters
            decimal assetPrice = _engineRatesGrid.ContainsKey(engineHandle)
                                     ? _engineRatesGrid[engineHandle].GetAssetPrice(exerciseTime, assetCode)
                                     : calibrationEngine.AssetPrice;
            decimal expiry = GenerateDayValue(exerciseTime, 365.0m);
            // build an ImpliedVolatility object
            SABRParameters parameters = calibrationEngine.GetSABRParameters;
            var vol = new SABRImpliedVolatility(parameters, true);
            // value the strike (interpolate as necessary)
            var results = new List<decimal>(strikes.Length);
            foreach (decimal strike in strikes)
            {
                string errmsg = "";
                decimal result = 0;
                if (!vol.SABRInterpolatedVolatility(assetPrice, expiry, strike, ref errmsg, ref result, true))
                    throw new System.Exception(errmsg);
                results.Add(result);
            }
            return results.ToArray();
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
        /// <param name="pExercise">The exercise (option expiry) part of the key</param>
        /// <param name="pTenor">The tenor (asset code) part of the key</param>
        /// <returns></returns>
        public decimal GetSABRParameter(CalibrationParameter param, string engine, string pExercise, string pTenor)
        {
            string exercise = GenerateTenorLabel(pExercise);
            string tenor = GenerateTenorLabel(pTenor);
            if (!_sabrEngines.ContainsKey(engine))
            {
                throw new InvalidOperationException("Calibration Engine " + engine + " not found.");
            }
            SortedDictionary<SABRKey, SABRCalibrationEngine> engines = _sabrEngines[engine];
            var key = new SABRKey(exercise, tenor);
            if (!engines.ContainsKey(key))
            {
                throw new InvalidOperationException("The Calibration Engine with Key(" + exercise + "," + tenor + ") not found.");
            }
            SABRCalibrationEngine calibrationEngine = engines[key];
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
                    throw new InvalidOperationException("Unknown Calibration type: " + param);
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
        public bool IsModelCalibrated(string engine, string expiry, string tenor)
        {
            if (_sabrEngines.ContainsKey(engine))
            {
                //SABREngineCollection SABREngines = _SABREngines[engine];
                SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngines = _sabrEngines[engine];
                var key = new SABRKey(GenerateTenorLabel(expiry), GenerateTenorLabel(tenor));
                if (sabrEngines.ContainsKey(key))
                {
                    SABRCalibrationEngine calibrationEngine = sabrEngines[key];
                    return calibrationEngine.IsSABRModelCalibrated;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the Calibration Engine Calibration Error the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public decimal CalibrationError(string engine, string expiry, string tenor)
        {
            if (!_sabrEngines.ContainsKey(engine))
            {
                throw new InvalidOperationException("Calibration Engine " + engine + " not found.");
            }
            SortedDictionary<SABRKey, SABRCalibrationEngine> engines = _sabrEngines[engine];
            var key = new SABRKey(GenerateTenorLabel(expiry), GenerateTenorLabel(tenor));
            if (!engines.ContainsKey(key))
            {
                throw new InvalidOperationException("The Calibration Engine with Key(" + expiry + "," + tenor +") not found.");
            }
            SABRCalibrationEngine calibrationEngine = engines[key];
            return calibrationEngine.CalibrationError;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a full calibration model.This version uses the volatility grid to generate an engine
        /// for each swap tenor (row values)
        /// </summary>
        /// <param name="volatilityGrid">The vols grid</param>
        /// <param name="assetGrid">The asset grid</param>
        /// <param name="settings">The SABR settings</param>
        /// <param name="calibrationEngineId">The id of this engine</param>
        /// <param name="optionExpiry">The ATM pointer</param>
        private SortedDictionary<SABRKey, SABRCalibrationEngine> BuildEngineCollection(SwaptionDataMatrix volatilityGrid,
                                                                                       ForwardRatesMatrix assetGrid, SABRCalibrationSettings settings, 
                                                                                       string calibrationEngineId, string optionExpiry)
        {
            var engineCollection = new SortedDictionary<SABRKey, SABRCalibrationEngine>(new SABRKey());
            // Generate a new entry in the engineCollection for each row in the volatility grid
            var tenors = volatilityGrid.GetTenors();
            foreach (string tenor in tenors)
            {
                decimal assetPrice = assetGrid.GetAssetPrice(optionExpiry, tenor);
                decimal exerciseTime = GenerateDayValue(optionExpiry, 365.0m);
                // Generate the Vols and Strikes lists for the engine
                var vols = volatilityGrid.GetVolatility(tenor).ToList();
                var strikes = volatilityGrid.GetStrikes().Select(strike => assetPrice + strike).ToList();
                // Only add a new Calibration Engine (and Calibrate it) if the vols are greater than 0
                if (ValidateData(vols))
                {
                    // Create a new instance of the engine
                    var calibrationEngine =
                        new SABRCalibrationEngine(calibrationEngineId, settings, strikes, vols, assetPrice, exerciseTime);
                    // Calibrate the engine
                    calibrationEngine.CalibrateSABRModel();
                    // Add the new engine to our collection
                    var key = new SABRKey(optionExpiry, tenor);
                    engineCollection.Add(key, calibrationEngine);
                }
            }
            return engineCollection;
        }

        /// <summary>
        /// Create an ATM calibration engine. Such an engine is designed for a single value
        /// </summary>
        /// <param name="settings">The settings object to use</param>
        /// <param name="calibrationEngineId">The id of this engine</param>
        /// <param name="nu">Nu value</param>
        /// <param name="rho">Rho value</param>
        /// <param name="atmVolatility">The ATM volatility</param>
        /// <param name="assetPrice">Asset Price to use</param>
        /// <param name="optionExpiry">The ATM pointer</param>
        /// <param name="assetCode">The ATM identifiier (if used)</param>
        private static SortedDictionary<SABRKey, SABRCalibrationEngine> BuildEngineCollection(SABRCalibrationSettings settings,
                                                                                       string calibrationEngineId, decimal nu, decimal rho, decimal atmVolatility, decimal assetPrice,
                                                                                       string optionExpiry, string assetCode)
        {
            // basic setup
            var engineCollection = new SortedDictionary<SABRKey, SABRCalibrationEngine>(new SABRKey());
            decimal exerciseTime = GenerateDayValue(optionExpiry, 365.0m);
            // Create a new instance of the engine
            var calibrationEngine =
                new SABRCalibrationEngine(calibrationEngineId, settings, nu, rho, atmVolatility, assetPrice,
                                          exerciseTime);
            // Calibrate the engine
            calibrationEngine.CalibrateATMSABRModel();
            // Add the new engine to our collection
            SABRKey key = assetCode != null ? new SABRKey(optionExpiry, assetCode) : new SABRKey(optionExpiry);
            engineCollection.Add(key, calibrationEngine);
            return engineCollection;
        }

        /// <summary>
        /// Create an Interpolated calibration engine. Such an engine is designed for a single value
        /// derived from a set of base engines.
        /// </summary>
        /// <param name="settings">The settings object to use</param>
        /// <param name="calibrationEngineId">The id of this engine</param>
        /// <param name="engines">The array of engine handles to use</param>
        /// <param name="atmVolatility">The ATM volatility</param>
        /// <param name="assetPrice">Asset Price to use</param>
        /// <param name="optionExpiry">The ATM pointer</param>
        /// <param name="tenor">The tenor to create the new engine for. This must be a valid tenor</param>
        private SortedDictionary<SABRKey, SABRCalibrationEngine> BuildEngineCollection(SABRCalibrationSettings settings,
                                                                                       string calibrationEngineId, ref SortedDictionary<SABRKey, SABRCalibrationEngine> engines,
                                                                                       decimal atmVolatility, decimal assetPrice, string optionExpiry, string tenor)
        {
            var engineCollection = new SortedDictionary<SABRKey, SABRCalibrationEngine>(new SABRKey());
            decimal exerciseTime = GenerateDayValue(optionExpiry, 365.0m);
            decimal indexTenor = GenerateDayValue(tenor, 365.0m);
            // Create a new instance of the engine
            //(string handle, SABRCalibrationSettings calibrationSettings,            
            var calibrationEngine =
                new SABRCalibrationEngine(calibrationEngineId, settings, engines, 
                    atmVolatility, assetPrice, exerciseTime, indexTenor);
            // Calibrate the engine
            calibrationEngine.CalibrateInterpSABRModel();
            // Add the new engine to our collection
            var key = new SABRKey(optionExpiry, tenor);
            engineCollection.Add(key, calibrationEngine);
            return engineCollection;
        }

        /// <summary>
        /// Parse the raw volatilities to create the labels and values arrays
        /// required by a SwaptionDataGrid{T,U} used to store the data
        /// for use by the SABR Calibration routines
        /// </summary>
        /// <param name="rawVolatility">The raw volatility data</param>
        /// <param name="expiry">The expiry for this grid</param>
        /// <returns>the rawdata as a SwaptiondataGrid</returns>
        private static SwaptionDataMatrix ParseVolatilityInput(object[,] rawVolatility, string expiry)
        {
            // Set the upper/lower bounds of the converted array
            int hi1D = rawVolatility.GetUpperBound(0);
            int hi2D = rawVolatility.GetUpperBound(1);
            // Create and populate the data arrays used to build the SwaptionDataGrid
            // The columns represent tenors, the rows option expiries
            var strikes = new decimal[hi2D];
            var tenors = new string[hi1D];
            for (int idx = 1; idx <= hi2D; idx++)
            {
                string strike = rawVolatility[0, idx].ToString();
                // If contains ATM then parse and assume it is in BP
                if (strike.Contains("ATM"))
                {
                    strike = strike.Replace("ATM", "").Replace(" ", "");
                    if (strike == "")
                    {
                        strike = "0";
                    }
                    strikes[idx - 1] = decimal.Parse(strike)/10000;
                }
                else // otherwise just take the numbers as they are
                {
                    strikes[idx - 1] = (decimal)rawVolatility[0, idx];
                }
            }
            for (int idx = 1; idx <= hi1D; idx++)
                tenors[idx - 1] = rawVolatility[idx, 0].ToString();
            // Add arrays for the values
            var values = new decimal[hi1D][];
            for (int idx = 1; idx <= hi1D; idx++)
            {
                values[idx - 1] = new decimal[hi2D];
                for (int jdx = 1; jdx <= hi2D; jdx++)
                    values[idx - 1][jdx - 1] = decimal.Parse(rawVolatility[idx, jdx].ToString());
            }
            var grid = new SwaptionDataMatrix(tenors, strikes, expiry, values);
            return grid;
        }

        /// <summary>
        /// Parse the raw assets grid to create the labels and values arrays
        /// required by a SwaptionDataGrid{T,U} used to store the data
        /// for use by the SABR calibration/calulation routines
        /// This version uses Interpolation to include Swap Tenors that are not
        /// included as part of the grid but fall within the minimum and maximum tenor
        /// values of the grid.
        /// </summary>
        /// <param name="rawAsset">The raw volatility data</param>
        /// <returns>The rawdata as a SwaptionDataGrid</returns>
        private static ForwardRatesMatrix ParseAssetInputWithInterpolation(object[,] rawAsset)
        {
            // Set the upper/lower bounds of the converted array
            int hi1D = rawAsset.GetUpperBound(0);
            int hi2D = rawAsset.GetUpperBound(1);
            // Create and populate the data arrays used to build the SwaptionDataGrid
            // The columns represent tenors, the rows option expiries
            var tenors = new string[hi2D];
            var expiry = new string[hi1D];
            for (int idx = 1; idx <= hi2D; idx++)
                tenors[idx - 1] = rawAsset[0, idx].ToString();
            for (int idx = 1; idx <= hi1D; idx++)
                expiry[idx - 1] = rawAsset[idx, 0].ToString();
            // Add arrays for the values
            var values = new decimal[hi1D][];
            for (int idx = 1; idx <= hi1D; idx++)
            {
                values[idx - 1] = new decimal[hi2D];
                for (int jdx = 1; jdx <= hi2D; jdx++)
                    values[idx - 1][jdx - 1] = decimal.Parse(rawAsset[idx, jdx].ToString());
            }
            // Set up the lists we shall use to insert any interpolated column entries
            var fullTenors = new List<string>();
            var fullValues = new List<List<decimal>>();
            // Convert the tenors to doubles
            var tenorsAsDouble = new decimal[tenors.Length];
            for (int idx = 0; idx < tenors.Length; idx++)
                tenorsAsDouble[idx] = decimal.Parse(tenors[idx]);
            // Set up the min and max column (tenor) values
            int min = int.Parse(tenors[0]);
            int max = int.Parse(tenors[tenors.Length - 1]);
            // Loop through the expiry values testing the tenor values
            // Copy existing tenors or add new tenor values using interpolation
            for (int expiryIdx = 0; expiryIdx < expiry.Length; expiryIdx++)
            {
                int tenorIdx = 0;
                var inner = new List<decimal>();
                var lin = new LinearInterpolation(tenorsAsDouble, values[expiryIdx]);
                // Loop from the min to the max testing for valid tenor entries
                // and inserting new values where necessary
                for (int idx = min; idx <= max; idx++)
                {
                    if (idx == int.Parse(tenors[tenorIdx]))
                    {
                        // Add an existing tenor and its values array - Remember to check we haven't already added the tenor
                        if (!fullTenors.Contains(tenors[tenorIdx]))
                            fullTenors.Add(tenors[tenorIdx]);
                        inner.Add(values[expiryIdx][tenorIdx++]);
                    }
                    else
                    {
                        // Add the interpolated tenor and its value - Remember to check we haven't already added the tenor
                        if (!fullTenors.Contains(tenors[tenorIdx]))
                            fullTenors.Add(idx.ToString(CultureInfo.InvariantCulture));
                        inner.Add(lin.ValueAt((decimal)idx));
                    }
                }
                fullValues.Add(inner);
            }
            // Regenerate the arrays to use in the grid
            string[] interpolatedTenors = fullTenors.Select(a=>a.Last() == 'y' ? a : a + "y").ToArray();
            var interpolatedValues = new decimal[fullValues.Count][];
            for (int idx = 0; idx < fullValues.Count; idx++)
                interpolatedValues[idx] = fullValues[idx].ToArray();
            // Create the grid
            var grid = new ForwardRatesMatrix(expiry, interpolatedTenors, interpolatedValues);
            return grid;
        }

        /// <summary>
        /// Convert a raw label value to a formatted label
        /// This will allow inputs such as 3.0 to default to 3yr
        /// </summary>
        /// <param name="rawLabel"></param>
        /// <returns></returns>
        private static string GenerateTenorLabel(string rawLabel)
        {
            string alpha = string.Empty;
            decimal numero = 0;
            LabelSplitter(rawLabel, ref alpha, ref numero);
            Period i = CreateInterval(rawLabel);
            return i.ToString();
        }

        /// <summary>
        /// Extract a series of CalibrationEngine objects indexed by a given OptionExpiry value
        /// Add these to a sorted dictionary.
        /// If the sorted dictionary is not set (that is null) then create a new instance.
        /// </summary>
        /// <param name="engines">The dictionary to add the CalibrationEngines to</param>
        /// <param name="engineHandle">The handle of a CalibrationEngineCollection</param>
        private void ExtractAllEngines(ref SortedDictionary<SABRKey, SABRCalibrationEngine> engines, string engineHandle)
        {
            if (engines == null)
                engines = new SortedDictionary<SABRKey, SABRCalibrationEngine>(new SABRKey());
            // Loop through the engines in the dictionary adding to the Sorted Dictionary
            //SABREngineCollection collection = _SABREngines[engineHandle];
            SortedDictionary<SABRKey, SABRCalibrationEngine> collection = _sabrEngines[engineHandle];
            foreach (KeyValuePair<SABRKey, SABRCalibrationEngine> pair in collection)
                engines.Add(pair.Key, pair.Value);
        }


        /// <summary>
        /// Splits a generic grid label into its numeric and alpha parts.
        /// White space at the start and end of the grid label are ignored. 
        /// </summary>
        /// <param name="label">The source label to split</param>
        /// <param name="alpha">The alpha part of the label</param>
        /// <param name="numeric">The number part of the label</param>
        private static void LabelSplitter(string label, ref string alpha, ref decimal numeric)
        {
            if (alpha == null) throw new ArgumentNullException(nameof(alpha));
            // Trim label at both ends.
            string tempLabel = label.Trim();
            // Initialise the regular expressions that will be used to match
            // the necessary format.
            const string alphaPattern = "[a-zA-Z]+";
            const string numericPattern = "[0-9.]+";
            var alphaRegex =
                new Regex(alphaPattern, RegexOptions.IgnoreCase);
            var numericRegex =
                new Regex(numericPattern, RegexOptions.IgnoreCase);

            // Match the alpha and numeric components.
            MatchCollection alphaMatches = alphaRegex.Matches(tempLabel);
            MatchCollection numericMatches = numericRegex.Matches(tempLabel);
            alpha = (alphaMatches.Count > 0) ? alphaMatches[0].Value : "Y";
            numeric = (numericMatches.Count > 0) ? Convert.ToDecimal(numericMatches[0].Value) : 0;
        }

        /// <summary>
        /// Convert the term structure string representation to an interval
        /// </summary>
        /// <param name="p">a string representing a term structure</param>
        /// <returns></returns>
        private static Period CreateInterval(string p)
        {
            string alphaPart = string.Empty;
            decimal numPart = 0;
            // Split the term string into its parts
            LabelSplitter(p, ref alphaPart, ref numPart);
            var i = new Period {periodMultiplier = numPart.ToString(CultureInfo.InvariantCulture)};
            // Convert the alpha to a PeriodEnum
            switch (alphaPart.Substring(0, 1).ToUpperInvariant())
            {
                case "D":
                    i.period = PeriodEnum.D;
                    break;
                case "M":
                    i.period = PeriodEnum.M;
                    break;
                case "Y":
                    i.period = PeriodEnum.Y;
                    break;
                default:
                    i.period = PeriodEnum.M;
                    break;
            }
            return i;
        }

        /// <summary>
        /// Generate a day value from a term
        /// </summary>
        /// <param name="term">A term to convert to a value</param>
        /// <param name="dayCountConvention">day count convention to use</param>
        /// <returns></returns>
        private static decimal GenerateDayValue(string term, decimal dayCountConvention)
        {
            Period i = CreateInterval(term);
            decimal numero = Convert.ToDecimal(i.periodMultiplier);
            decimal yearFraction = 0;
            switch (i.period)
            {
                case PeriodEnum.D:
                    yearFraction = numero / dayCountConvention;
                    break;
                case PeriodEnum.W:
                    yearFraction = (numero / 52.0m);
                    break;
                case PeriodEnum.M:
                    yearFraction = (numero / 12.0m);
                    break;
                case PeriodEnum.Y:
                    yearFraction = numero;
                    break;
            }
            return yearFraction;
        }

        /// <summary>
        /// Test the vols data for values of 0 or less (an error)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool ValidateData(IEnumerable<decimal> data)
        {
            return data.All(d => d > 0);
        }

        #endregion
    }
}