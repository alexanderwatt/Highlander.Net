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

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.PricingEngines;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.Analytics.V5r3.Stochastics.SABR;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.SABR
{
    ///<summary>
    ///</summary>
    public class SABRSwaptionInterface
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
        } ;

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
        private static SABRSwaptionInterface _instance;

        #endregion

        #region Singleton Interface

        /// <summary>
        /// Protect this class from multiple instances
        /// </summary>
        public SABRSwaptionInterface()
        {
            _sabrEngines = new Dictionary<string, SortedDictionary<SABRKey, SABRCalibrationEngine>>();
            _engineRatesGrid = new Dictionary<string, ForwardRatesMatrix>();
            _sabrSettings = new Dictionary<string, SABRCalibrationSettings>();
        }

        /// <summary>
        /// Static Instance method to access this class
        /// </summary>
        /// <returns></returns>
        public static SABRSwaptionInterface Instance()
        {
            return _instance ?? (_instance = new SABRSwaptionInterface());
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
        public static string AddCalibrationSettings(string handle, string instrument, string ccy, decimal beta)
        {
            return Instance().SABRAddCalibrationSettings(handle, instrument, ccy, beta);
        }

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
        public string SABRAddCalibrationSettings(string handle, string instrument, string ccy, decimal beta)
        {
            // Setup some defaults for the currency and instrument settings
            const string calibrationCcy = "AUD";
            var calibrationInstrument = InstrumentType.Instrument.Swaption;
            // Identify the instrument used in this settings object
            Array instruments = Enum.GetValues(typeof(InstrumentType.Instrument));
            foreach (InstrumentType.Instrument s in instruments)
            {
                if (s.ToString() != instrument) continue;
                calibrationInstrument = s;
                break;
            }
            // Create a new calibration settings object using the instrument/ccy/beta grouping
            var newCalibration =
                new SABRCalibrationSettings(handle, calibrationInstrument, calibrationCcy, beta);
            // Test whether the CalibrationSettings is already present
            // If not already present then add it
            if (_sabrSettings.ContainsKey(handle))
                _sabrSettings[handle] = newCalibration;
            else
                _sabrSettings.Add(handle, newCalibration);
            return handle;
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
        /// <param name="settingsHandle">Calibration settings handle</param>
        /// <param name="strikes">An array of strikes.</param>
        /// <param name="vols">A grid of volatilities (with row/column labels)</param>
        /// <param name="assets">A grid of asset values</param>
        /// <param name="assetExpiry">An array of asset expiries.</param>
        /// <param name="optionEx">The ption expiry to index against</param>
        /// <param name="tenors">An array of expiry tenors.</param>
        /// <param name="assetTenors">An array of asset tenors.</param>
        /// <returns></returns>
        public static string CalibrateModel(string engineHandle, string settingsHandle, String[] tenors, decimal[] strikes,
                                     Decimal[,] vols, Decimal[,] assets, String[] assetTenors, String[] assetExpiry, string optionEx)
        {
            return Instance().SABRCalibrateModel(engineHandle, settingsHandle, tenors, strikes, vols, assets,
                                                 assetTenors, assetExpiry, optionEx);
        }

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
        /// <param name="strikes">An array of strikes.</param>
        /// <param name="rawVols">A grid of volatilities (with row/column labels)</param>
        /// <param name="rawAssets">A grid of asset values</param>
        /// <param name="assetExpiry">An array of asset expiries.</param>
        /// <param name="optionEx">The ption expiry to index against</param>
        /// <param name="tenors">An array of expiry tenors.</param>
        /// <param name="assetTenors">An array of asset tenors.</param>
        /// <returns></returns>
        public string SABRCalibrateModel(string engineHandle, string settingsHandle, String[] tenors, decimal[] strikes,
                                         Decimal[,] rawVols, Decimal[,] rawAssets, String[] assetTenors, String[] assetExpiry, string optionEx)
        {
            // Create the asset and volatility data grids
            SwaptionDataMatrix volatilityGrid = ParseVolatilityInput(tenors, strikes, rawVols, optionEx);
            ForwardRatesMatrix assetGrid = ParseAssetInputWithInterpolation(assetTenors, assetExpiry, rawAssets);
            // Retrieve the calibration settings to use with this calibration engine
            SABRCalibrationSettings settings = _sabrSettings[settingsHandle];
            // Generate the CalibrationEngine Id
            string calibrationEngineId = engineHandle;
            string optionExpiry = SABRHelper.GenerateTenorLabel(optionEx);
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
        public static string CalibrateATMModel(string engineHandle, string settingsHandle, decimal nu, decimal rho, decimal atmVolatility,
                                        decimal assetPrice, string exerciseTime)
        {
            return Instance().SABRCalibrateATMModel(engineHandle, settingsHandle, nu, rho, atmVolatility,
                                                    assetPrice, exerciseTime);
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
        public string SABRCalibrateATMModel(string engineHandle, string settingsHandle, decimal nu, decimal rho, decimal atmVolatility,
                                            decimal assetPrice, string exerciseTime)
        {
            // Create the parameters used in this engine
            SABRCalibrationSettings settings = _sabrSettings[settingsHandle];
            // Create the engine
            SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngine = BuildEngineCollection(settings, engineHandle, nu, rho, atmVolatility, assetPrice, SABRHelper.GenerateTenorLabel(exerciseTime), null);
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
        /// <param name="assetCode">The asset code.</param>
        /// <returns></returns>
        public static string CalibrateSABRATMModelWithTenor(string engineHandle, string settingsHandle, decimal nu, decimal rho, decimal atmVolatility,
                                                     decimal assetPrice, string exerciseTime, string assetCode)
        {
            return Instance().SABRCalibrateATMModel(engineHandle, settingsHandle, nu, rho, atmVolatility,
                                                    assetPrice, exerciseTime, assetCode);
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
        /// <param name="assetCode">The asset code.</param>
        /// <returns></returns>
        public string SABRCalibrateATMModel(string engineHandle, string settingsHandle, decimal nu, decimal rho, decimal atmVolatility,
                                            decimal assetPrice, string exerciseTime, string assetCode)
        {
            // Create the parameters used in this engine
            SABRCalibrationSettings settings = _sabrSettings[settingsHandle];
            // Create the engine
            SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngine = BuildEngineCollection(settings, engineHandle, nu, rho, atmVolatility,
                                                   assetPrice, SABRHelper.GenerateTenorLabel(exerciseTime), SABRHelper.GenerateTenorLabel(assetCode));
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
        /// <param name="handles">The array of engine handles to use</param>
        /// <param name="atmVolatility">The At The Money volatility to use in engine calibration</param>
        /// <param name="assetPrice">The Asset price to use</param>
        /// <param name="exerciseTime">The exercise time to create the new engine for</param>
        /// <param name="tenor">The tenor to create the new engine for. This must be a valid tenor</param>
        /// <returns>The engine handle</returns>
        public static string CalibrateInterpolatedModel(string engineHandle, string settingsHandle, string[] handles, decimal atmVolatility, decimal assetPrice, string exerciseTime, string tenor)
        {
            return Instance().SABRCalibrateInterpolatedModel(engineHandle, settingsHandle, handles, atmVolatility,
                                                             assetPrice, exerciseTime, tenor);
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
            SABRCalibrationSettings settings = _sabrSettings[settingsHandle];
            foreach (string handle in calibrationArray)
            {
                ExtractAllEngines(ref engines, handle);
            }
            decimal pATM = atmVolatility;
            decimal pAsset = assetPrice;
            // Create the new InterpolatedCalibrationEngine and add to the SABREngineCollection
            SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngine = BuildEngineCollection(settings, engineHandle, engines, pATM, pAsset, exerciseTime, tenor);
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
        public static decimal InterpolateVolatility(string engineHandle, string exerciseTime, string assetCode, decimal strike)
        {
            return Instance().SABRInterpolateVolatility(engineHandle, exerciseTime, assetCode, strike);
        }

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
                var engine = _sabrEngines[engineHandle];
                var key = new SABRKey(SABRHelper.GenerateTenorLabel(exerciseTime), SABRHelper.GenerateTenorLabel(assetCode));
                // Check that the engine for this exercise/asset key exists
                if (engine.ContainsKey(key))
                {
                    SABRCalibrationEngine calibrationEngine = engine[key];
                    if (!calibrationEngine.IsSABRModelCalibrated)
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "SABR Engine with key: {0}:{1} is not calibrated", new object[] { key.Expiry, key.Tenor }));
                    // Create SABRImpliedVolatility parameters
                    decimal assetPrice = _engineRatesGrid.ContainsKey(engineHandle) ? _engineRatesGrid[engineHandle].GetAssetPrice(exerciseTime, assetCode) : calibrationEngine.AssetPrice;
                    decimal strikeValue = strike;
                    var expiry = (decimal)SABRHelper.GenerateDayValue(exerciseTime, 365.0d);
                    //decimal strikeValue = strike / 100.0m;
                    // build an ImpliedVolatility object
                    SABRParameters parameters = calibrationEngine.GetSABRParameters;
                    var vol = new SABRImpliedVolatility(parameters, true);
                    // value the strike (interpolate as necessary)
                    if (!vol.SABRInterpolatedVolatility(assetPrice, expiry, strikeValue, ref errmsg, ref result, true))
                        throw new ArgumentException(errmsg);
                }
                else
                    throw new ArgumentException("The Calibration Engine with Key(" + exerciseTime + "," + assetCode + ") not found.");
            }
            else
                throw new ArgumentException("Calibration Engine " + engineHandle + " not found.");
            return result;
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
        public static decimal GetSABRParameter(string param, string engine, string pExercise, string pTenor)
        {
            var typeofParam = (CalibrationParameter)Enum.Parse(typeof(CalibrationParameter), param);
            return Instance().GetSABRParameter(typeofParam, engine, pExercise, pTenor);
        }

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
            string exercise = SABRHelper.GenerateTenorLabel(pExercise);
            string tenor = SABRHelper.GenerateTenorLabel(pTenor);

            if (!_sabrEngines.ContainsKey(engine))
                throw new ArgumentException("Calibration Engine " + engine + " not found.");
            SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngines = _sabrEngines[engine];
            var key = new SABRKey(exercise, tenor);
            if (!sabrEngines.ContainsKey(key))
                throw new ArgumentException("The Calibration Engine with Key(" + exercise + "," + tenor + ") not found.");
            SABRCalibrationEngine calibrationEngine = sabrEngines[key];
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
            return Instance().IsSABRModelCalibrated(engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Calibration status for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public bool IsSABRModelCalibrated(string engine, string expiry, string tenor)
        {
            if (_sabrEngines.ContainsKey(engine))
            {
                //SABREngineCollection SABREngines = _SABREngines[engine];
                SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngines = _sabrEngines[engine];
                var key = new SABRKey(SABRHelper.GenerateTenorLabel(expiry), SABRHelper.GenerateTenorLabel(tenor));
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
        public static decimal CalibrationError(string engine, string expiry, string tenor)
        {
            return Instance().SABRCalibrationError(engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Calibration Engine Calibration Error the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public decimal SABRCalibrationError(string engine, string expiry, string tenor)
        {
            if (_sabrEngines.ContainsKey(engine))
            {
                //SABREngineCollection SABREngines = _SABREngines[engine];
                SortedDictionary<SABRKey, SABRCalibrationEngine> sabrEngines = _sabrEngines[engine];
                var key = new SABRKey(SABRHelper.GenerateTenorLabel(expiry), SABRHelper.GenerateTenorLabel(tenor));
                if (sabrEngines.ContainsKey(key))
                {
                    SABRCalibrationEngine calibrationEngine = sabrEngines[key];
                    return calibrationEngine.CalibrationError;
                }
                throw new ArgumentException("The Calibration Engine with Key(" + expiry + "," + tenor + ") not found.");
            }
            throw new ArgumentException("Calibration Engine " + engine + " not found.");
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
        private static SortedDictionary<SABRKey, SABRCalibrationEngine> BuildEngineCollection(SwaptionDataMatrix volatilityGrid,
                                                                                              ForwardRatesMatrix assetGrid, SABRCalibrationSettings settings, string calibrationEngineId, string optionExpiry)
        {
            var engineCollection = new SortedDictionary<SABRKey, SABRCalibrationEngine>(new SABRKey());
            // Generate a new entry in the engineCollection for each row in the volatility grid
            foreach (string tenor in volatilityGrid.GetTenors())
            {
                var assetPrice = assetGrid.GetAssetPrice(optionExpiry, tenor);
                var exerciseTime = (decimal)SABRHelper.GenerateDayValue(optionExpiry, 365.0d);
                // Generate the Vols and Strikes lists for the engine
                List<decimal> vols = volatilityGrid.GetVolatility(tenor).ToList();
                List<decimal> strikes = volatilityGrid.GetStrikes().Select(strike => assetPrice + strike).ToList();
                // Only add a new Calibration Engine (and Calibrate it) if the vols are greater than 0
                if (!SABRHelper.ValidateData(vols)) continue;
                // Create a new instance of the engine
                var calibrationEngine =
                    new SABRCalibrationEngine(calibrationEngineId, settings, strikes, vols, assetPrice, exerciseTime);
                // Calibrate the engine
                calibrationEngine.CalibrateSABRModel();
                // Add the new engine to our collection
                var key = new SABRKey(optionExpiry, tenor);
                engineCollection.Add(key, calibrationEngine);
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
            var exerciseTime = (decimal)SABRHelper.GenerateDayValue(optionExpiry, 365.0d);
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
        private static SortedDictionary<SABRKey, SABRCalibrationEngine> BuildEngineCollection(SABRCalibrationSettings settings, string calibrationEngineId, IEnumerable<KeyValuePair<SABRKey, SABRCalibrationEngine>> engines, decimal atmVolatility, decimal assetPrice, string optionExpiry, string tenor)
        {
            var engineCollection = new SortedDictionary<SABRKey, SABRCalibrationEngine>(new SABRKey());
            var exerciseTime = (decimal)SABRHelper.GenerateDayValue(optionExpiry, 365.0d);
            var indexTenor = (decimal)SABRHelper.GenerateDayValue(tenor, 365.0d);
            // Create a new instance of the engine
            var calibrationEngine =
                new SABRCalibrationEngine(calibrationEngineId, settings, engines, atmVolatility, assetPrice, exerciseTime, indexTenor);
            // Calibrate the engine
            calibrationEngine.CalibrateInterpSABRModel();
            // Add the new engine to our collection
            var key = new SABRKey(optionExpiry, tenor);
            engineCollection.Add(key, calibrationEngine);
            return engineCollection;
        }

        /// <summary>
        /// Parse the raw volatilities to create the labels and values arrays
        /// required by a <see cref="SwaptionDataMatrix"/> used to store the data
        /// for use by the SABR Calibration routines
        /// </summary>
        /// <param name="strikes">An array of strikes.</param>
        /// <param name="rawVolatility">The raw volatility data</param>
        /// <param name="expiry">The expiry for this grid</param>
        /// <param name="tenors">An array of expiry tenors.</param>
        /// <returns>the rawdata as a SwaptiondataGrid</returns>
        private static SwaptionDataMatrix ParseVolatilityInput(String[] tenors, decimal[] strikes, Decimal[,] rawVolatility, string expiry)
        {
            // Set the upper/lower bounds of the converted array
            int hi1D = rawVolatility.GetUpperBound(0) + 1;
            int hi2D = rawVolatility.GetUpperBound(1) + 1;
            // Create and populate the data arrays used to build the SwaptionDataGrid
            // The columns represent tenors, the rows option expiries
            // Add arrays for the values
            var values = new decimal[hi1D][];
            for (int idx = 0; idx < hi1D; idx++)
            {
                values[idx] = new decimal[hi2D];
                for (int jdx = 0; jdx < hi2D; jdx++)
                    values[idx][jdx] = rawVolatility[idx, jdx];
            }
            var grid = new SwaptionDataMatrix(tenors, strikes, expiry, values);
            return grid;
        }

        /// <summary>
        /// Parse the raw assets grid to create the labels and values arrays
        /// required by a <see cref="SwaptionDataMatrix"/> used to store the data
        /// for use by the SABR calibration/calulation routines
        /// This version uses Interpolation to include Swap Tenors that are not
        /// included as part of the grid but fall within the minimum and maximum tenor
        /// values of the grid.
        /// </summary>
        /// <param name="expiry">An array of expiries.</param>
        /// <param name="rawAsset">The raw volatility data</param>
        /// <param name="tenors">An array of tenors.</param>
        /// <returns>The rawdata as a SwaptionDataGrid</returns>
        private static ForwardRatesMatrix ParseAssetInputWithInterpolation(String[] tenors, String[] expiry,
                                                                           Decimal[,] rawAsset)
        {
            // Set the upper/lower bounds of the converted array
            //var lo1D = rawAsset.GetLowerBound(0);
            int hi1D = rawAsset.GetUpperBound(0) + 1;
            //var lo2D = rawAsset.GetLowerBound(1);
            int hi2D = rawAsset.GetUpperBound(1) + 1;

            // Add arrays for the values
            var values = new decimal[hi1D][];
            for (int idx = 0; idx < hi1D; idx++)
            {
                values[idx] = new decimal[hi2D];
                for (int jdx = 0; jdx < hi2D; jdx++)
                    values[idx][jdx] = rawAsset[idx, jdx];
            }
            // Set up the lists we shall use to insert any interpolated column entries
            var fullTenors = new List<string>();
            var fullValues = new List<List<decimal>>();
            // Convert the tenors to doubles
            var tenorsAsDouble = new decimal[tenors.Length];//Tenor must be years.
            for (int idx = 0; idx < tenors.Length; idx++)
                tenorsAsDouble[idx] = (decimal)(PeriodHelper.Parse(tenors[idx]).ToYearFraction());
            // Set up the min and max column (tenor) values
            var min = (int)(tenorsAsDouble[0]);
            var max = (int)(tenorsAsDouble[tenorsAsDouble.Length - 1]);

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
                    if (idx == int.Parse(PeriodHelper.Parse(tenors[tenorIdx]).periodMultiplier))
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
                            fullTenors.Add(idx.ToString(CultureInfo.InvariantCulture) + 'Y');
                        inner.Add(lin.ValueAt((decimal)idx));
                    }
                }
                fullValues.Add(inner);
            }

            // Regenerate the arrays to use in the grid
            string[] interpolatedTenors = fullTenors.ToArray();
            var interpolatedValues = new decimal[fullValues.Count][];
            for (int idx = 0; idx < fullValues.Count; idx++)
                interpolatedValues[idx] = fullValues[idx].ToArray();
            // Create the grid
            var grid = new ForwardRatesMatrix(expiry, interpolatedTenors, interpolatedValues);
            return grid;
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

        #endregion
    }
}