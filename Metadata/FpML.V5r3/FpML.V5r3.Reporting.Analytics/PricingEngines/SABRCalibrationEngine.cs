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

#region Using Directives

using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.SolverFoundation.Services;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Solvers;
using Orion.Analytics.Statistics;
using Orion.Analytics.Stochastics.SABR;
using Microsoft.SolverFoundation.Solvers;

#endregion

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Class that encapsulates the functionality to calibrate the SABR model
    /// for a particular asset type.
    /// Calibration occurs for a particular row in a table of implied 
    /// volatilities, for example in the case of swaptions this would  
    /// correspond to a particular (expiry, tenor) pair.
    /// </summary>
    public class SABRCalibrationEngine
    {
        #region Constants

        /// <summary>
        /// Multiplier applied to the guess for the SABR parameter alpha to
        /// form the upper bound in the Dekker-Brent solver.
        /// </summary>
        private const decimal AlphaMultiplier = 2.0m;

        /// <summary>
        /// Dimension of each point in the sequence of quasi-random numbers.
        /// </summary>
        private const int Dimension = 2;

        /// <summary>
        /// Constant that is mid way between the upper and lower bounds
        /// for the magnitude (absolute value) of the SABR parameter rho.
        /// </summary>
        private const decimal MidRho = 0.5m;

        /// <summary>
        /// Constant that specifies the lower bound for the SABR parameter
        /// alpha. The constant is used to set the lower bound in the
        /// Dekker-Brent solver used in calculation of the SABR
        /// parameter alpha. 
        /// </summary>
        private const decimal MinimumAlpha = 0.00001m;

        /// <summary>
        /// Number of candidates selected from the list of all randomly
        /// generated initial guesses that will be used in a full calibration.
        /// </summary>
        private const int NumBestCandidates = 5;

        /// <summary>
        /// Constant that specifies the absolute magnitude of the perturbation
        /// in the SABR parameter rho in the event that during the calibration
        /// process the case |rho| = 1 arises. The effect of the constant is to
        /// perturb the SABR parameter rho to |rho| = (1 - RhoPerturbation).
        /// </summary>
        private const decimal RhoPerturbation = 0.01m;

        /// <summary>
        /// Number of points in the sequence of quasi-random numbers.
        /// </summary>
        private const int SequenceLength = 1500;//10000;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Constructor for the class <see cref="SABRCalibrationEngine"/>.
        /// The purpose of this constructor is to initialise the calibration
        /// engine when a full calibration of the SABR model is required.
        /// Post condition: private field _isFullCalibrationPossible is true.
        /// </summary>
        /// <param name="handle">Name that identifies the SABR calibration
        ///  engine object.</param>
        /// <param name="calibrationSettings">Specific settings used for
        ///  calibration of the SABR model.</param>
        /// <param name="strikes">List of strikes arranged in ascending order.
        /// Precondition: Centre of the list will be the asset (ATM) price.
        /// </param>
        /// <param name="volatilities">List that contains the implied 
        ///  volatility at each strike.
        /// Precondition: Number of volatilities is the same as the
        /// number of strikes.</param>
        /// <param name="assetPrice">Price (ATM level) of the relevant
        ///  asset as a decimal.</param>
        /// <param name="exerciseTime">Time to option exercise in years.</param>
        public SABRCalibrationEngine(string handle,
                                     SABRCalibrationSettings calibrationSettings,
                                     IEnumerable<decimal> strikes,
                                     IEnumerable<decimal> volatilities,
                                     decimal assetPrice,
                                     decimal exerciseTime)
        {
            // Map all function arguments into the appropriate private field.
            Handle = handle;
            _calibrationSettings = calibrationSettings;
            _strikes = new List<decimal>();
            foreach(var strike in strikes)
            {
                _strikes.Add(strike);
            }
            _volatilities = new List<decimal>();
            foreach (var volatility in volatilities)
            {
                _volatilities.Add(volatility);
                
            }
            _assetPrice = assetPrice;
            _exerciseTime = exerciseTime;
            // Set the flag that indicates the status of the calibration.
            _isSABRModelCalibrated = false;
            // Set remaining private fields to appropriate default values.
            _atmIndex = -1;
            _atmSlope = 0.0m;
            _atmVolatility = 0.0m;
            _bestCandidates = null;
            _calibrationError = 0.0m;
            _countSABRSurfaceEntries = 0;
            _expiries = null;
            _muGuess = 0.0m;
            _sabrNuSurface = null;
            _sabrRhoSurface = null;
            _tenor = 0.0m;
            _tenors = null;
            _thetaGuess = 0.0m;
            _sabrParameters =
                new SABRParameters(0.0m, _calibrationSettings.Beta, 0.0m, 0.0m);
            // Set flags that indicate the constructor called.
            _isATMCalibrationPossible = false;
            _isFullCalibrationPossible = true;
            _isInterpCalibrationPossible = false;
        }

        /// <summary>
        /// Overload of the constructor for the class
        /// <see cref="SABRCalibrationEngine"/>.
        /// The purpose of this constructor is to initialise the calibration
        /// engine when an ATM (at-the-money) calibration of the SABR model
        /// is required. The SABR parameters beta, nu and rho have been
        /// supplied by the client and the SABR parameter alpha is determined
        /// from the ATM calibration.
        /// Post condition: private field _isATMCalibrationPossible is true.
        /// </summary>
        /// <param name="handle">Name that identifies the SABR calibration
        ///  engine object.</param>
        /// <param name="calibrationSettings">Specific settings used for
        ///  calibration of the SABR model.</param>
        /// <param name="nu">SABR parameter nu.</param>
        /// <param name="rho">SABR parameter rho.</param>
        /// <param name="atmVolatility">The ATM volatility as a decimal.</param>
        /// <param name="assetPrice">Price (ATM level) of the relevant
        ///  asset as decimal.</param>
        /// <param name="exerciseTime">Time to option exercise in years.</param>
        public SABRCalibrationEngine(string handle,
                                     SABRCalibrationSettings calibrationSettings,
                                     decimal nu,
                                     decimal rho,
                                     decimal atmVolatility,
                                     decimal assetPrice,
                                     decimal exerciseTime)
        {
            // Map all function arguments into the appropriate private field.
            Handle = handle;
            _calibrationSettings = calibrationSettings;
            _atmVolatility = atmVolatility;
            _assetPrice = assetPrice;
            _exerciseTime = exerciseTime;
            // Set the object that stores the SABR parameters: first check the
            // validity of the SABR parameters nu and rho.
            var errorMessage = "";
            var isNuValid =
                SABRParameters.CheckSABRParameterNu(nu, ref errorMessage);
            var isRhoValid =
                SABRParameters.CheckSABRParameterRho(rho, ref errorMessage);
            if(!isNuValid || !isRhoValid)
            {
                throw new ArgumentException(errorMessage);                
            }
            _sabrParameters =
                new SABRParameters(0.0m, _calibrationSettings.Beta, nu, rho);
            // Set the flag that indicates the status of the calibration.
            _isSABRModelCalibrated = false;
            // Set the remaining private fields to appropriate default values.
            _atmIndex = -1;
            _atmSlope = 0.0m;
            _bestCandidates = null;
            _calibrationError = 0.0m;
            _countSABRSurfaceEntries = 0;
            _expiries = null;
            _muGuess = 0.0m;
            _sabrNuSurface = null;
            _sabrRhoSurface = null;
            _strikes = null;
            _tenor = 0.0m;
            _tenors = null;
            _thetaGuess = 0.0m;
            _volatilities = null;
            // Set flags that indicate the constructor called.
            _isATMCalibrationPossible = true;
            _isFullCalibrationPossible = false;
            _isInterpCalibrationPossible = false;
        }

        /// <summary>
        /// Overload of the constructor for the class
        /// <see cref="SABRCalibrationEngine"/>.
        /// The purpose of this constructor is to initialise the calibration
        /// engine when a full calibration of the SABR model is required.
        /// Post condition: private field _isInterpCalibrationPossible is true.
        /// </summary>
        /// <param name="handle">Name that identifies the SABR calibration
        /// engine object.</param>
        /// <param name="calibrationSettings">Specific settings used for
        /// calibration of the SABR model.</param>
        /// <param name="engineHandles">Data structure that stores all existing
        /// SABRCalibrationEngine objects that will be used in the interpolation
        /// calibration.</param>
        /// <param name="atmVolatility">The ATM (at-the-money) volatility as
        /// a decimal.</param>
        /// <param name="assetPrice">Price (ATM level) of the relevant
        /// asset as a decimal.</param>
        /// <param name="exerciseTime">Time to option exercise in years.</param>
        /// <param name="tenor">The tenor of the relevant asset in
        /// years.</param>
        public SABRCalibrationEngine
            (string handle, SABRCalibrationSettings calibrationSettings, 
             IEnumerable<KeyValuePair<SABRKey, SABRCalibrationEngine>> engineHandles, 
             decimal atmVolatility, decimal assetPrice, decimal exerciseTime, decimal tenor)
        {
            // Map all function arguments into the appropriate private field.
            Handle = handle;
            _calibrationSettings = calibrationSettings;
            _atmVolatility = atmVolatility;
            _assetPrice = assetPrice;
            _exerciseTime = exerciseTime;
            _tenor = tenor;
            // Set the object that stores the SABR parameters: this will
            // initially store on the SABR parameter beta because this is
            // the only known SABR parameter.
            _sabrParameters =
                new SABRParameters(0.0m, _calibrationSettings.Beta, 0.0m, 0.0m);
            // Set the flag that indicates the status of the calibration.
            _isSABRModelCalibrated = false;
            // Build the SABR Nu and Rho surfaces.
            _countSABRSurfaceEntries = 0;
            BuildSABRNuAndRhoSurfaces(engineHandles);
            // Set the remaining private fields to appropriate default values.
            _atmIndex = -1;
            _atmSlope = 0.0m;
            _bestCandidates = null;
            _calibrationError = 0.0m;
            _muGuess = 0.0m;
            _strikes = null;
            _thetaGuess = 0.0m;
            _volatilities = null;
            // Set flags that indicate the constructor called.
            _isATMCalibrationPossible = false;
            _isFullCalibrationPossible = false;
            _isInterpCalibrationPossible = true;
        }

        #endregion Constructors

        #region Public Calibration Methods

        /// <summary>
        /// Provides a full calibration of the SABR model; the SABR parameters
        /// alpha, nu and rho are determined by calibration to market data.
        /// Precondition: private field _isFullCalibrationPossible is true.
        /// </summary>
        public void CalibrateSABRModel()
        {
            if(!_isFullCalibrationPossible)
            {
                const string errorMessage = "Full calibration of the SABR model is not available";
                throw new Exception(errorMessage);
            }
            // Seed the SABR model for calibration.
            SetInitialGuesses();
            // Calibrate SABR model.
            Optimizer();         
            // Perform enhanced calibration if required.
            if (!_isSABRModelCalibrated)
            {
                PerformEnhancedCalibration();
            }
        }

        /// <summary>
        /// Provides an ATM calibration of the SABR model; only the SABR 
        /// parameter alpha is determined by calibration to market data.
        /// Precondition: private field _isATMCalibrationPossible is true.
        /// Post condition: private field _isSABRModelCalibrated is set.
        /// </summary>
        public void CalibrateATMSABRModel()
        {
            if (!_isATMCalibrationPossible)
            {
                const string errorMessage = "ATM calibration of the SABR model is not available";
                throw new Exception(errorMessage);
            }
            _isSABRModelCalibrated = UpdateAlpha();
        }

        /// <summary>
        /// Provides an interpolation calibration of the SABR model; the SABR
        /// parameters nu and rho are determined from bilinear interpolation
        /// of the SABR nu and rho surfaces, and the SABR parameter alpha is
        /// determined by calibration to market data.
        /// Precondition: private field _isInterpCalibrationPossible is true.
        /// Post condition: private field _isSABRModelCalibrated is set.
        /// </summary>
        public void CalibrateInterpSABRModel()
        {
            // Perform some checks for whether the calibration is possible
            // and if the necessary data is available. 
            CheckCalibrateInterpData();
            // Compute and store the interpolated SABR parameters Nu and Rho.
            ComputeInterpolatedNuAndRho();
            // Compute the SABR parameter Alpha.
            _isSABRModelCalibrated = UpdateAlpha();
        }

        #endregion Public Calibration Methods

        #region Accessor Methods

        /// <summary>
        /// Accessor method for the asset price used in the calibration 
        /// of the SABR model.
        /// </summary>
        /// <value>Asset price.</value>
        public decimal AssetPrice => _assetPrice;

        /// <summary>
        /// Accessor method for the ATM slope of the curve log moneyness
        /// against implied volatility .
        /// </summary>
        /// <value>ATM slope.</value>
        public decimal ATMSlope => _atmSlope;

        /// <summary>
        /// Accessor method for the calibration error associated with a
        /// full calibration of the SABR model.
        /// </summary>
        /// <value>Error from a full calibration of the SABR model.</value>
        public decimal CalibrationError => _calibrationError;

        /// <summary>
        /// Accessor method for the handle that identifies the SABR 
        /// calibration engine object.
        /// </summary>
        /// <value>Handle to the SABR calibration engine object.</value>
        public string Handle { get; }

        /// <summary>
        /// Accessor method for whether an ATM calibration is possible.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if an ATM calibration possible; otherwise,
        /// 	<c>false</c>.
        /// </value>
        public bool IsATMCalibrationPossible => _isATMCalibrationPossible;

        /// <summary>
        /// Accessor method for whether a full calibration is possible.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a full calibration possible; otherwise,
        /// 	<c>false</c>.
        /// </value>
        public bool IsFullCalibrationPossible => _isFullCalibrationPossible;

        /// <summary>
        /// Accessor method for the status of the SABR model calibration.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if SABR model calibrated; otherwise, <c>false</c>.
        /// </value>
        public bool IsSABRModelCalibrated => _isSABRModelCalibrated;

        /// <summary>
        /// Accessor method for the initial guess of the transformed SABR
        /// parameter mu.
        /// </summary>
        /// <value>Guess of the mu value.</value>
        public decimal MuGuess => _muGuess;

        /// <summary>
        /// Accessor method for the object that stores the SABR parameters
        /// associated with the calibration.
        /// </summary>
        /// <value>Current status of the SABR parameters object.</value>
        public SABRParameters GetSABRParameters => _sabrParameters;

        /// <summary>
        /// Accessor method for the two dimensional array that stores all
        /// instances of the SABR parameter Nu that will be used in a
        /// SABR interpolation calibration.
        /// </summary>
        /// <value>SABR Nu surface.</value>
        public decimal[,] SABRNuSurface => _sabrNuSurface;

        /// <summary>
        /// Accessor method for the two dimensional array that stores all
        /// instances of the SABR parameter Rho that will be used in a
        /// SABR interpolation calibration.
        /// </summary>
        /// <value>SABR Rho surface.</value>
        public decimal[,] SABRRhoSurface => _sabrRhoSurface;

        /// <summary>
        /// Accessor method for the initial guess of the transformed SABR
        /// parameter theta.
        /// </summary>
        /// <value>Guess of the theta value.</value>
        public decimal ThetaGuess => _thetaGuess;

        #endregion Accessor Methods

        #region Private Business Logic Methods

        /// <summary>
        /// Helper function used to build the expiry (row) and tenor (column)
        /// labels used to index into the SABR Nu and Rho surfaces.
        /// Post conditions: private fields _expiries and _tenors are set and
        /// sorted into ascending numerical order.
        /// </summary>
        /// <param name="engineHandles">Data structure that stores all existing
        /// SABRCalibrationEngine objects that will be used in the interpolation
        /// calibration.</param>
        private void BuildExpiryTenorLabels
            (IEnumerable<KeyValuePair<SABRKey, SABRCalibrationEngine>> engineHandles)
        {
            // Instantiate the data structures that store the expiry and tenor
            // labels.
            _expiries = new List<decimal>();
            _tenors = new List<decimal>();
            // Add a particular expiry and tenor, only if not present in
            // each list.
            foreach(var kvPair in
                engineHandles)
            {
                // Expiry (row) labels.
                var expiry = kvPair.Key.ExpiryAsDecimal;
                if(!_expiries.Contains(expiry))
                {
                    _expiries.Add(expiry);
                }
                // Tenor (column) labels.
                var tenor = kvPair.Key.TenorAsDecimal;
                if(!_tenors.Contains(tenor))
                {
                    _tenors.Add(tenor);
                }
            }
            // Sort the expiry and tenor labels.
            _expiries.Sort();
            _tenors.Sort();
        }

        /// <summary>
        /// Helper function used by the SABR interpolation constructor
        /// to build the SABR Nu and Rho surfaces.
        /// The SABR Nu and Rho surfaces will in most cases be sparse with
        /// the sentinel "decimal.MinValue" used to designate an unavailable 
        /// SABR Nu and Rho parameter at a particular expiry tenor pair.
        /// Precondition: private field _calibrationSettings has been set.
        /// Post conditions: private fields _expiries and _tenors are set and
        /// sorted into ascending numerical order.
        /// Post conditions: private fields _countSABRSurfaceEntries, 
        /// _sabrNuSurface and _sabrRhoSurface are set.
        /// </summary>
        /// <param name="engineHandles">Data structure that stores all existing
        /// SABRCalibrationEngine objects that will be used in the interpolation
        /// calibration.</param>
        private void BuildSABRNuAndRhoSurfaces
            (IEnumerable<KeyValuePair<SABRKey, SABRCalibrationEngine>> engineHandles)
        {
            // Build the expiry (row) and tenor (column) labels.
            var keyValuePairs = engineHandles as KeyValuePair<SABRKey, SABRCalibrationEngine>[] ?? engineHandles.ToArray();
            BuildExpiryTenorLabels(keyValuePairs);
            // Instantiate and initialise the data structures that will store 
            // the SABR Nu and Rho surfaces.
            var numExpiries = _expiries.Count;
            var numTenors = _tenors.Count;
            _sabrNuSurface = new decimal[numExpiries, numTenors];
            _sabrRhoSurface = new decimal[numExpiries, numTenors];
            for (var rowIndex = 0; rowIndex < numExpiries; ++rowIndex)
            {
                for(var columnIndex = 0; columnIndex < numTenors; ++columnIndex)
                {
                    _sabrNuSurface[rowIndex, columnIndex] = decimal.MinValue;
                    _sabrRhoSurface[rowIndex, columnIndex] = decimal.MinValue;
                }
            }
            // Fill the SABR Nu and Rho surfaces with the available SABR
            // parameter values, provided that the engine is calibrated
            // and the SABR parameter beta in the engine has the same value
            // as in the settings object.
            var beta = _calibrationSettings.Beta;
            foreach (var kvPair in
                keyValuePairs)
            {
                // Determine the location of the expiry tenor pair in the
                // SABR Nu and Rho surfaces.
                var expiry = kvPair.Key.ExpiryAsDecimal;
                var tenor = kvPair.Key.TenorAsDecimal;
                var engine = kvPair.Value;
                var rowIndex = _expiries.IndexOf(expiry);
                var columnIndex = _tenors.IndexOf(tenor);
                // Store available SABR Nu and Rho parameters.
                // Criteria: 1) ATM/Full calibration; 2) Calibrated engine;
                //           3) Beta match.
                var isCorrectCalibrationType =
                engine.IsATMCalibrationPossible ||
                engine.IsFullCalibrationPossible;
                var addSABRParameters = engine.IsSABRModelCalibrated &&
                                        beta == engine.GetSABRParameters.Beta &&
                                        isCorrectCalibrationType;
                if (!addSABRParameters) continue;
                ++_countSABRSurfaceEntries;
                _sabrNuSurface[rowIndex, columnIndex] =
                    engine.GetSABRParameters.Nu;
                _sabrRhoSurface[rowIndex, columnIndex] =
                    engine.GetSABRParameters.Rho;
            }
            // Fill the gaps in the SABR Nu and Rho surfaces.
            FillSABRNuAndRhoSurfaces();
        }

        /// <summary>
        /// Helper function used by CalibrateInterpSABRModel to
        /// check for whether the calibration is possible and if
        /// the necessary data is available..
        /// </summary>
        private void CheckCalibrateInterpData()
        {
            if (!_isInterpCalibrationPossible)
            {
                const string errorMessage = "Interpolation calibration of the SABR model is not available";
                throw new Exception(errorMessage);
            }
            if (_countSABRSurfaceEntries != 0)
            {
            }
            else
            {
                const string errorMessage = "No valid engines found by SABR interpolated calibration";
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Helper function used by the method SetCalibrationSeeds to 
        /// compute the ATM slope of the curve log moneyness against
        /// implied volatility.
        /// Post condition: Private fields _atmIndex, _atmSlope and
        /// _atmVolatility are set.
        /// </summary>
        private void ComputeATMSlope()
        {
            // Compute log moneyness values across the list of strikes.
            var logMoneyness = _strikes.Select(strike => decimal.ToDouble(strike)/decimal.ToDouble(_assetPrice)).Select(ratio => (Decimal) Math.Log(ratio)).ToList();
            // Set the ATM volatility.
            SetATMIndex(logMoneyness);
            _atmVolatility = _volatilities[_atmIndex];
            // Compute ATM slope as the slope of the line segment that joins
            // the points either side of the ATM point.
            var atmRise =
                _volatilities[_atmIndex + 1] - _volatilities[_atmIndex - 1];
            var atmRun =
                logMoneyness[_atmIndex + 1] - logMoneyness[_atmIndex - 1];
            _atmSlope = atmRise / atmRun;
        }

        /// <summary>
        /// Computes the SABR parameters Nu and Rho by interpolation of the
        /// SABR Nu and Rho surfaces, respectively.
        /// </summary>
        private void ComputeInterpolatedNuAndRho()
        {
            // Construct column labels: tenors.
            var columnLabels = _tenors.Select(decimal.ToDouble).ToArray();
            // Construct row labels: expiries.
            var rowLabels = _expiries.Select(decimal.ToDouble).ToArray();
            // Compute and store the interpolated SABR Nu and Rho parameters.
            var numColumns = columnLabels.Length;
            var numRows = rowLabels.Length;
            var nuDataTable = new double[numRows, numColumns];
            var rhoDataTable = new double[numRows, numColumns];
            for (var i = 0; i < numRows; ++i)
            {
                for (var j = 0; j < numColumns; ++j)
                {
                    nuDataTable[i,j] = decimal.ToDouble(_sabrNuSurface[i,j]);
                    rhoDataTable[i,j] = decimal.ToDouble(_sabrRhoSurface[i,j]);
                }
            }
            var nuInterpObj = new BilinearInterpolation
                (ref columnLabels, ref rowLabels, ref nuDataTable);
            _sabrParameters.Nu = (decimal)nuInterpObj.Interpolate                
                                              (decimal.ToDouble(_tenor), decimal.ToDouble(_exerciseTime));
            var rhoInterpObj = new BilinearInterpolation
                (ref columnLabels, ref rowLabels, ref rhoDataTable);
            _sabrParameters.Rho = (decimal)rhoInterpObj.Interpolate
                                               (decimal.ToDouble(_tenor), decimal.ToDouble(_exerciseTime));
        }

        /// <summary>
        /// Helper function used by the method BuildSABRNuAndRhoSurfaces
        /// to fill the missing elements of the SABR Nu and Rho surfaces.
        /// Linear interpolation, with flat extrapolation, in expiry is used 
        /// to compute the SABR parameters Nu and Rho at a fixed tenor.
        /// Precondition: private method BuildSABRNuAndRhoSurfaces has been
        /// called.
        /// </summary>
        private void FillSABRNuAndRhoSurfaces()
        {
            // Determine the number of expiries and tenors available.
            var numExpiries = _expiries.Count;
            var numTenors = _tenors.Count;
            // Loop through SABR Nu and Rho surfaces by column.
            for (var columnIndex = 0; columnIndex < numTenors; ++columnIndex)
            {
                // Instantiate clean instances of the data structures used
                // to determine the locations to be filled.
                var targets = new List<decimal>();
                var knownExpiries = new List<decimal>();
                var knownSABRNus = new List<decimal>();
                var knownSABRRhos = new List<decimal>(); 
                // Locate the missing expiries.
                for (var rowIndex = 0; rowIndex < numExpiries; ++rowIndex)
                {
                    if (_sabrNuSurface[rowIndex, columnIndex] != decimal.MinValue)
                    {
                        // Known SABR Nu and Rho parameters: record.
                        knownExpiries.Add(_expiries[rowIndex]);
                        knownSABRNus.Add(_sabrNuSurface[rowIndex, columnIndex]);
                        knownSABRRhos.Add(_sabrRhoSurface[rowIndex, columnIndex]);
                    } 
                    else
                    {
                        // Missing expiry: record.
                        targets.Add(_expiries[rowIndex]);
                    }
                }
                // Fill-in gaps by linear interpolation.
                var xValues = knownExpiries.ToArray();
                var nuValues = knownSABRNus.ToArray();
                var rhoValues = knownSABRRhos.ToArray();
                var nuInterpObj =
                    new LinearInterpolation(xValues, nuValues);
                var rhoInterpObj =
                    new LinearInterpolation(xValues, rhoValues);
                foreach (var target in targets)
                {
                    var nu = nuInterpObj.ValueAt(target);
                    var rho = rhoInterpObj.ValueAt(target);
                    var row = _expiries.IndexOf(target);
                    _sabrNuSurface[row, columnIndex] = nu;
                    _sabrRhoSurface[row, columnIndex] = rho;
                }
            }
        }

        /// <summary>
        /// Helper function used by the CalibrateSABRModel method to call the
        /// optimization routine.
        /// Precondition: private fields _thetaGuess and _muGuess have been
        /// set.
        /// Post conditions: private fields _isSABRModelCalibrated and 
        /// _calibrationError are set.
        /// </summary>
        private void Optimizer()
        {
            var initalGuess = new[] {decimal.ToDouble(_thetaGuess), decimal.ToDouble(_muGuess)};
            // Calibrate SABR model.
            //var objectiveFunction = new MultiObjectiveFunction(ObjectiveFunction);
            //var optimizer = new NelderMeadSolver();
            var solution = NelderMeadSolver.Solve(ObjectiveFunction, initalGuess);
            //optimizer.FindExtremum();
            // Set the flag that indicates whether the SABR model is calibrated
            // and also set the calibration error.
            _isSABRModelCalibrated = solution.Result == NonlinearResult.LocalOptimal;
            var tolerance = 1.0E-05;
            var result = NelderMeadSolver.IsValidTolerance(tolerance);
            if (result)
            {
                _calibrationError = (decimal)tolerance;
            }
        }

        /// <summary>
        /// Performs an enhanced full SABR calibration.
        /// The enhanced calibration is triggered whenever the initial full
        /// calibration fails, and attempts to search for a feasible solution
        /// by using random initial guesses drawn from a sequence of quasi-random
        /// numbers.
        /// </summary>
        private void PerformEnhancedCalibration()
        {
            // Select the candidates that will participate in the enhanced
            // calibration.
            SelectCandidatesForEnhancedCalibration();         
            // Perform a full SABR calibration on the best candidates available.
            foreach (var kvPair in
                _bestCandidates)
            {
                // Set the initial guess.
                // Note: Initial guess is set in the optimization variable
                // coordinates.
                _muGuess = 
                    (decimal) Math.Sqrt(decimal.ToDouble(kvPair.Value.Nu));
                _thetaGuess = 
                    (decimal) Math.Acos(decimal.ToDouble(kvPair.Value.Rho));
                // Call the optimization routine to perform the calibration.
                Optimizer();
                if (_isSABRModelCalibrated)
                {
                    // Calibration successful: stop enhanced calibration.
                    break;
                }
            }
        }

        /// <summary>
        /// Helper function used by PerformEnhancedCalibration to select
        /// the candidates for enhanced calibration.
        /// Post condition: private field _bestCandidates is set.
        /// </summary>
        private void SelectCandidatesForEnhancedCalibration()
        {
            // Generate the sequence of quasi-random numbers.
            // The first dimension will map to the SABR parameter Nu, and
            // the second dimension will map to the SABR parameter Rho.
            var point = HaltonSequence.Sequence(0, SequenceLength, Dimension);
            // Initialise the data structure that will store the collection of best
            // candidates for a full calibration.
            _bestCandidates = new SortedList<Decimal, SABRParameters>();
            for (var i = 1; i <= SequenceLength; ++i)
            {
                // Configure the SABR parameters object, based on the current
                // random draw.
                _sabrParameters.Nu = (decimal) point[0];
                _sabrParameters.Rho = (decimal) (2.0 * point[1] - 1.0);
                UpdateAlpha();
                // Evaluate the objective function at the current point.
                var residual = ComputeObjectiveFunctionResidual();
                // Create the temporary object that will store the
                // SABR parameters.
                var temp = new SABRParameters(_sabrParameters.Alpha,
                                              _sabrParameters.Beta,
                                              _sabrParameters.Nu,
                                              _sabrParameters.Rho);
                // Select the best candidates for a full calibration.
                if (_bestCandidates.Count < NumBestCandidates)
                {
                    // Check if the current draw should be added
                    // to the initial collection that stores the
                    // candidates for a full calibration.
                    if (!_bestCandidates.ContainsKey(residual))
                    {
                        _bestCandidates.Add(residual, temp);
                    }
                }
                else
                {
                    // Check if the current draw should be added to the
                    // list of best candidates.
                    // Note: the residual is a positive number.
                    var lastKey =
                        _bestCandidates.Keys[NumBestCandidates - 1];
                    var addCurrentDraw = residual < lastKey;
                    if (addCurrentDraw)
                    {
                        // Pop the worst stored candidate and then add the 
                        // current draw.
                        _bestCandidates.Remove(lastKey);
                        _bestCandidates.Add(residual, temp);
                    }
                }
                // Move to the next point in  the sequence.
                point = HaltonSequence.Sequence(i, SequenceLength, Dimension);//sequence[i];
            }
        }

        /// <summary>
        /// Sets the initial guesses for the SABR parameters rho and nu.
        /// Helper function used by the method CalibrateSABRModel to
        /// seed the calibration of rho and nu.
        /// </summary>
        private void SetInitialGuesses()
        {
            ComputeATMSlope();
            SetThetaGuess();
            SetMuGuess();
        }
        
        /// <summary>
        /// Sets the (array) index of the ATM (at-the-money) volatility.
        /// The ATM index is the array index that corresponds to a zero log
        /// moneyness value; if there is no zero log moneyness value, then
        /// it is the zero-based position of the element with the minimum
        /// magnitude. 
        /// Post condition: Private field _atmIndex is set.
        /// </summary>
        /// <param name="logMoneyness">List that contains the log moneyness,
        /// where log moneyness is Log(strike/_assetPrice)</param>
        private void SetATMIndex(IList<decimal> logMoneyness)
        {
            // Find the index of the ATM strike - this will be the index
            // at which there is zero log moneyness; if there is no zero
            // log moneyness value, then find the position of the element
            // with the minimum magnitude.
            _atmIndex = 0;           
            var minValue = Math.Abs(logMoneyness[0]);
            var pos = 0; // zero based index position
            foreach(var logMoneynessValue in logMoneyness)
            {
                if(Math.Abs(logMoneynessValue) < minValue)
                {
                    // Replace current minimum.
                    minValue = Math.Abs(logMoneynessValue);
                    _atmIndex = pos;
                }
                ++pos; // increment position marker
            }
            // Check that the ATM index is not at the left or right ends of
            // the array.
            if (_atmIndex == 0 || _atmIndex == (logMoneyness.Count - 1))
            {
                // Invalid array of strikes - stop calibration procedure.
                const string errorMessage =
                    "SABR calibration found invalid strikes: ATM strike missing.";
                throw new ArgumentException(errorMessage);
            }
        }

        /// <summary>
        /// Helper function used by the method SetInitialGuesses to set the
        /// guess for the transformed SABR parameter mu.
        /// Preconditions: ComputeATMSlope and SetThetaGuess have been called.
        /// Post condition: private field _muGuess is set.
        /// </summary>
        private void SetMuGuess()
        {
            decimal magnitudeATMSlope = Math.Abs(_atmSlope);
            decimal convexity =
                _rhoGuess*(1.0m - _calibrationSettings.Beta)*_atmVolatility;

            decimal nuGuess = 4.0m * Math.Abs(magnitudeATMSlope + convexity);
            _muGuess = (decimal) Math.Sqrt(decimal.ToDouble(nuGuess));
        }

        /// <summary>
        /// Helper function used by the method SetInitialGuesses to set the
        /// guess for the transformed SABR parameter theta.
        /// Precondition: ComputeATMSlope has been called.
        /// Post condition: private fields _rhoGuess and _thetaGuess are set.
        /// </summary>
        private void SetThetaGuess()
        {
            _rhoGuess = MidRho*Math.Sign(_atmSlope);
            _thetaGuess = (decimal) Math.Acos(decimal.ToDouble(_rhoGuess));
        }

        /// <summary>
        /// Master function used by the objective function to update the
        /// SABR parameters alpha, nu and rho to values that are consistent
        /// with the current status of the optimization variable.
        /// </summary>
        /// <param name="x">Optimization variable.</param>
        private void UpdateSABRParameters(IList<double> x)
        {
            // Update the SABR parameters in the order rho, nu and
            // finally alpha.
            UpdateRho(x);
            UpdateNu(x);
            UpdateAlpha(); 
        }

        /// <summary>
        /// Updates the SABR parameter alpha based on the status of the
        /// optimization variable.
        /// Precondition: the functions UpdateNu and UpdateRho have been
        /// called and the private field _atmVolatility has been set.
        /// </summary>
        /// <returns>true if the root finding algorithm converged,
        /// otherwise false</returns>
        private bool UpdateAlpha()
        {
            var beta = _sabrParameters.Beta;
            var alpha = _atmVolatility *
                        (decimal) Math.Pow(decimal.ToDouble(_assetPrice),
                                           decimal.ToDouble(1.0m - beta));
            var upperBound = AlphaMultiplier*alpha;
            // Instantiate and initialise the equation solver.
            //var solver = new Brent();
            // Solve for SABR parameter alpha and store result.
            try
            {
                _sabrParameters.Alpha = (decimal)Brent.FindRootExpand(TargetFunction, 
                    decimal.ToDouble(Math.Min(MinimumAlpha, alpha / AlphaMultiplier)), 
                    decimal.ToDouble(upperBound));
                return true;
            }
            catch (Exception)
            {
                //double accuracy = 1e-8;
                //Func<double, double> function = TargetFunction;
                //var result = solver.Solve(function, accuracy, decimal.ToDouble(alpha),
                //    decimal.ToDouble(Math.Min(MinimumAlpha, alpha / AlphaMultiplier)), decimal.ToDouble(upperBound));
                //_sabrParameters.Alpha = (decimal) result;
                _sabrParameters.Alpha = alpha;
                return false;
            }                        
            //// Check the status of the convergence.
            //var success = solver.Status == AlgorithmStatus.Converged;
            //if(!success)
            //{
            //    // Algorithm failed: use the approximate solution of the
            //    // ATM equation stored in the local variable alpha.
            //    _sabrParameters.Alpha = alpha;
            //}
            //return success;
        }

        /// <summary>
        /// Updates the SABR parameter nu based on the status of the
        /// optimization variable.
        /// </summary>
        /// <param name="x">Optimization variable.</param>
        private void UpdateNu(IList<double> x)
        {
            // Map the optimization variable x[1] to the SABR parameter nu.
            _sabrParameters.Nu = (decimal) x[1]*(decimal) x[1];
        }

        /// <summary>
        /// Updates the SABR parameter rho based on the status of the 
        /// optimization variable.
        /// Logic is added to cater for the boundary case |rho| = 1.0.
        /// </summary>
        /// <param name="x">Optimization variable.</param>
        private void UpdateRho(IList<double> x)
        {
            // Map the optimization variable x[0] to the SABR parameter rho.
            _sabrParameters.Rho = (decimal) Math.Cos(x[0]);
            if(Math.Abs(_sabrParameters.Rho) == 1.0m)
            {
                // Perturb the SABR parameter rho away from the
                // boundary |rho| = 1.0.
                _sabrParameters.Rho +=
                    -Math.Sign(_sabrParameters.Rho)*RhoPerturbation;
            }
        }

        #endregion Private Business Logic Methods

        #region Objective and Target Functions

        /// <summary>
        /// Helper function used by the private method ObjectiveFunction
        /// to compute actual value (residual) of the objective function.
        /// </summary>
        /// <returns></returns>
        private decimal ComputeObjectiveFunctionResidual()
        {
            // Initialise variables needed in the main body.
            var errorMessage = "";
            var index = 0;
            var modelVolatility = 0.0m;
            var residual = 0.0m;
            var volatilityObj =
                new SABRImpliedVolatility(_sabrParameters, false);
            // Compute the value of the objective function.
            foreach (var strike in _strikes)
            {
                // Compute the volatility from the SABR model.
                volatilityObj.SABRInterpolatedVolatility(_assetPrice,
                                                         _exerciseTime,
                                                         strike,
                                                         ref errorMessage,
                                                         ref modelVolatility,
                                                         false);
                var volDiff =
                    decimal.ToDouble(modelVolatility - _volatilities[index]);
                residual += (decimal) Math.Pow(volDiff, 2);
                // Move to the next market volatility.
                ++index;
            }
            return residual;
        }

        /// <summary>
        /// Objective function supplied to the optimization routine used in the
        /// calibration of the SABR model.
        /// Function signature and return type has been designed to be 
        /// compatible with the Microsoft Solver class.
        /// </summary>
        /// <param name="x">Vector that contains the optimization variables.
        /// Contents are: x[0] stores the transformed SABR parameter theta;
        /// x[1] stores the transformed SABR parameter mu.</param>
        /// Post condition: SABR parameters alpha, nu and rho are updated to
        /// their current values based on the status of the optimization
        /// variable.
        /// <returns>Least squares error.</returns>
        private double ObjectiveFunction(double[] x)
        {
            // Update the SABR parameters.
            UpdateSABRParameters(x);
            // Compute the residual from the evaluation of the objective
            // function in the current state of the SABR parameters object.
            decimal residual = ComputeObjectiveFunctionResidual();
            return decimal.ToDouble(residual);
        }

        /// <summary>
        /// Implementation of the business logic for the target function
        /// used by the Microsoft Solver class. 
        /// </summary>
        /// <param name="alpha">Current value of SABR parameter alpha. </param>
        /// <returns>Residual.</returns>
        private decimal TargetFunction(decimal alpha)
        {
            // Get current SABR parameters.
            decimal beta = _sabrParameters.Beta;
            decimal nu = _sabrParameters.Nu;
            decimal rho = _sabrParameters.Rho;
            // Construct the coefficients of the cubic equation that
            // determines the SABR parameter alpha.
            decimal numerator3 = (1.0m - beta)*(1.0m - beta)*_exerciseTime;
            decimal denominator3 = 24.0m*
                                   (decimal) Math.Pow(decimal.ToDouble(_assetPrice),
                                                      decimal.ToDouble(2.0m - 2.0m*beta));
            decimal cubicTerm = numerator3/denominator3*
                                (decimal) Math.Pow(decimal.ToDouble(alpha), 3.0);
            decimal numerator2 = rho*beta*nu*_exerciseTime;
            decimal denominator2 = 4.0m*
                                   (decimal) Math.Pow(decimal.ToDouble(_assetPrice),
                                                      decimal.ToDouble(1.0m - beta));
            decimal quadraticTerm = numerator2/denominator2*alpha*alpha;
            decimal linearTerm =
                (1.0m + nu*nu/24.0m*(2.0m - 3.0m*rho*rho)*_exerciseTime)*alpha;
            decimal constant = -_atmVolatility * 
                               (decimal) Math.Pow(decimal.ToDouble(_assetPrice),
                                                  decimal.ToDouble(1.0m - beta));
            // Construct and return the residual value.
            decimal residual = cubicTerm + quadraticTerm + linearTerm + constant;
            return residual;
        }

        /// <summary>
        /// Target function supplied to the root bracketing solver used in the
        /// ATM calibration of the SABR model.
        /// Function signature and return type has been designed to be
        /// compatible with the RealFunction delegate in the Extreme
        /// Optimization library. 
        /// </summary>
        /// <param name="alpha">Current value of SABR parameter alpha. </param>
        /// <returns>Residual.</returns>
        private double TargetFunction(double alpha)
        {
            // Convert the argument to the Decimal data type, and then call
            // the Decimal overload of the function.
            var dAlpha = Convert.ToDecimal(alpha);
            return decimal.ToDouble(TargetFunction(dAlpha));
        }

        #endregion Objective and Target Functions

        #region Private Fields

        /// <summary>
        /// Price (ATM level) of the relevant asset.
        /// </summary>
        private readonly decimal _assetPrice;

        /// <summary>
        /// Zero based index for the location of the ATM strike in the
        /// list of strikes.
        /// </summary>
        private int _atmIndex;

        /// <summary>
        /// ATM slope of the curve log moneyness against implied volatility.
        /// </summary>
        private decimal _atmSlope;

        /// <summary>
        /// ATM volatility.
        /// </summary>
        private decimal _atmVolatility;

        /// <summary>
        /// Data structure that stores the collection of best
        /// candidates that will participate in an enhanced calibration.
        /// </summary>
        private SortedList<Decimal, SABRParameters> _bestCandidates;

        /// <summary>
        /// Specific settings used for calibration of the SABR model.
        /// </summary>
        private readonly SABRCalibrationSettings _calibrationSettings;

        /// <summary>
        /// Calibration error for a full calibration of the SABR model.
        /// The full calibration error is measured by the value of the
        /// objective function at the extremum.
        /// </summary>
        private decimal _calibrationError;

        /// <summary>
        /// Variable used to store the number of valid entries in the
        /// (sparse) SABR Nu and Rho surfaces.
        /// </summary>
        private int _countSABRSurfaceEntries;

        /// <summary>
        /// Time, in years, to option exercise.
        /// </summary>
        private readonly decimal _exerciseTime;

        /// <summary>
        /// Private field used by the interpolation constructor to store
        /// the list of expiries in ascending numerical order.
        /// </summary>
        private List<decimal> _expiries;

        /// <summary>
        /// Flag that indicates whether an ATM calibration of the SABR model
        /// is available. A true flag indicates that the second of the class
        /// constructors has been called. 
        /// </summary>
        private readonly bool _isATMCalibrationPossible;

        /// <summary>
        /// Flag that indicates whether a full calibration of the SABR model
        /// is available. A true flag indicates that the first of the class
        /// constructors has been called.
        /// </summary>
        private readonly bool _isFullCalibrationPossible;

        /// <summary>
        /// Flag that indicates whether an interp (interpolation) calibration
        /// of the SABR model is available. A true flag indicates that the
        /// third of the class constructors has been called.
        /// </summary>
        private readonly bool _isInterpCalibrationPossible;

        /// <summary>
        /// Flag that indicates whether the SABR model is calibrated; the
        /// calibration referred to here can be a full or ATM calibration.
        /// The flag is set to true only at the end of a successful calibration.
        /// </summary>
        private bool _isSABRModelCalibrated;

        /// <summary>
        /// Initial guess for the transformed SABR parameter mu. 
        /// </summary>
        private decimal _muGuess;

        /// <summary>
        /// Initial guess for the SABR parameter rho.
        /// </summary>
        private decimal _rhoGuess;

        /// <summary>
        /// Data structure used to store at a particular expiry (row),
        /// tenor (column) the SABR parameter Nu from a successful calibration.
        /// </summary>
        private decimal[,] _sabrNuSurface;

        /// <summary>
        /// Storage place for all SABR parameters. Object is updated
        /// as the calibration proceeds.
        /// </summary>
        private readonly SABRParameters _sabrParameters;

        /// <summary>
        /// Data structure used to store at a particular expiry (row),
        /// tenor (column) the SABR parameter Rho from a successful calibration.
        /// </summary>
        private decimal[,] _sabrRhoSurface;

        /// <summary>
        /// List of strikes arranged in ascending order.
        /// The centre of the list will be the asset (ATM) price.
        /// </summary>
        private readonly List<decimal> _strikes;

        /// <summary>
        /// Tenor, in years, for the underlying swap associated with
        /// a swaption.
        /// </summary>
        private readonly decimal _tenor;

        /// <summary>
        /// Private field used by the interpolation constructor to store
        /// the list of tenors in ascending numerical order.
        /// </summary>
        private List<decimal> _tenors;

        /// <summary>
        /// Initial guess for the transformed SABR parameter theta.
        /// </summary>
        private decimal _thetaGuess;

        /// <summary>
        /// List that contains the implied volatility at each strike.
        /// </summary>
        private readonly List<decimal> _volatilities;

        #endregion Private Fields
    }
}