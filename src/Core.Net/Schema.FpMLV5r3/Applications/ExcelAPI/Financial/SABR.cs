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
using System.Linq;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.PricingStructures.SABR;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Interpolators;
using Highlander.Reporting.Analytics.V5r3.Stochastics.SABR;
using HLV5r3.Helpers;
using Excel = Microsoft.Office.Interop.Excel;

#endregion

namespace HLV5r3.Financial
{
    /// <summary>
    /// Interface layer between ExcelAPI and the underlying CapFloor analytics
    /// </summary>
    public partial class Cache
    {
        #region Optimised SABR Using a Separate Cache

        #region SABR Swaption

        /// <summary>
        /// Calculate the SABR implied volatility for the strike value.
        /// This method uses the calibration engine indexed by the exerciseTime/assetCode pair
        /// When an ATM engine is used then the assetCode is ignored.
        /// </summary>
        /// <param name="engineHandle">The CalibrationEngine to use</param>
        /// <param name="exerciseTime">Option Expiry index</param>
        /// <param name="assetCode">Swap Tenor index</param>
        /// <param name="strike">The strike to calculate Volatility for, as percent</param>
        /// <returns></returns>
        public decimal SabrImpliedVolatility(string engineHandle, string exerciseTime, string assetCode, double strike)
        {
            decimal result = SABRHelper.SabrImpliedVolatility(engineHandle, exerciseTime,
                                                                       assetCode, strike);
            return result;
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
        public string CalibrateSabrATMModel(string engineHandle, string settingsHandle, double nu, double rho,
            double atmVolatility, double assetPrice, string exerciseTime)
        {
            return SABRHelper.CalibrateSabrAtmModel(engineHandle, settingsHandle, nu, rho,
                                                                       atmVolatility, assetPrice, exerciseTime);
        }

        /// <summary>
        /// Generate an ATM (At-The-Money) Swaption Calibration engine using the supplied parameters
        /// This form of engine creates a single cell engine that does not support asset/volatility grid data.
        /// </summary>
        /// <param name="engineHandlesArray">The engine identifiers</param>
        /// <param name="settingsHandlesArray">The settings identifiers</param>
        /// <param name="nuArray">Nu values</param>
        /// <param name="rhoArray">Rho values</param>
        /// <param name="atmVolatilitiesArray">The ATM volatilitys</param>
        /// <param name="assetPricesArray">Asset Prices to use</param>
        /// <param name="exerciseTimesArray">Exercise times for the option</param>
        /// <returns></returns>
        public object[,] CalibrateSabrATMModels(Excel.Range engineHandlesArray, Excel.Range settingsHandlesArray, Excel.Range nuArray, Excel.Range rhoArray,
            Excel.Range atmVolatilitiesArray, Excel.Range assetPricesArray, Excel.Range exerciseTimesArray)
        {
            // Create the parameters used in this engine
            var engineHandles = DataRangeHelper.StripRange(engineHandlesArray);
            var settingsHandles = DataRangeHelper.StripRange(settingsHandlesArray);
            var nu = DataRangeHelper.StripDecimalRange(nuArray);
            var rho = DataRangeHelper.StripDecimalRange(rhoArray);
            var atmVolatilities = DataRangeHelper.StripDecimalRange(atmVolatilitiesArray);
            var assetPrices = DataRangeHelper.StripDecimalRange(assetPricesArray);
            var exerciseTimes = DataRangeHelper.StripRange(exerciseTimesArray);
            var result = SABRHelper.CalibrateSabrAtmModels(engineHandles.ToArray(), settingsHandles.ToArray(), nu.ToArray(), rho.ToArray(),
                                                                       atmVolatilities.ToArray(), assetPrices.ToArray(), exerciseTimes.ToArray());
            return RangeHelper.ConvertArrayToRange(result);
        }

        /// <summary>
        /// Calculate the SABR implied volatilities for the strike values.
        /// This method uses the calibration engine indexed by the exerciseTime/assetCode pair
        /// When an ATM engine is used then the assetCode is ignored.
        /// </summary>
        /// <param name="engineHandle">The CalibrationEngine to use</param>
        /// <param name="exerciseTime">Option Expiry index</param>
        /// <param name="assetCode">Swap Tenor index</param>
        /// <param name="strikeArray">The strike to calculate Volatilities for, as decimal</param>
        /// <returns></returns>
        public object[,] SabrInterpolateVolatilities(string engineHandle, string exerciseTime,
                string assetCode, Excel.Range strikeArray)
        {
            var strikes = DataRangeHelper.StripDoubleRange(strikeArray);
            var result = SABRHelper.SabrInterpolateVolatilities(engineHandle, exerciseTime,
                                                                              assetCode, strikes.ToArray());
            return RangeHelper.ConvertArrayToRange(result);
        }

        /// <summary>
        /// Create a new Calibration Settings object. Each new object holds the instrument, currency and beta
        /// to use for this settings object. Each new object is stored internally using the supplied handle
        /// as an identifying key.
        /// The Settings object is used by a Calibration Engine to define base parameters on which to calibrate.
        /// </summary>
        /// <param name="calibrationHandle">A handle</param>
        /// <param name="calibrationInstrument">The instrument type to use</param>
        /// <param name="calibrationCurrency">The currency setting</param>
        /// <param name="beta">The Beta parameter to use</param>
        /// <returns></returns>
        public string AddSabrCalibrationSettings(string calibrationHandle,
            string calibrationInstrument, string calibrationCurrency, double beta)
        {
            return SABRHelper.AddSabrCalibrationSettings(calibrationHandle,
                    calibrationInstrument, calibrationCurrency, beta);
        }

        /// <summary>
        /// Create a new Calibration Settings object. Each new object holds the instrument, currency and beta
        /// to use for this settings object. Each new object is stored internally using the supplied handle
        /// as an identifying key.
        /// The Settings object is used by a Calibration Engine to define base parameters on which to calibrate.
        /// </summary>
        /// <param name="calibrationHandleArray">A handle</param>
        /// <param name="calibrationInstrument">The instrument type to use</param>
        /// <param name="calibrationCurrency">The currency setting</param>
        /// <param name="betaArray">The Beta parameter to use</param>
        /// <returns></returns>
        public object[,] AddSabrCalibrationSettingArray(Excel.Range calibrationHandleArray,
            string calibrationInstrument, string calibrationCurrency, Excel.Range betaArray)
        {
            var calibrationHandle = DataRangeHelper.StripRange(calibrationHandleArray);
            var beta = DataRangeHelper.StripDecimalRange(betaArray);
            var result = SABRHelper.AddSabrCalibrationSettingArray(calibrationHandle.ToArray(),
                    calibrationInstrument, calibrationCurrency, beta.ToArray());
            return RangeHelper.ConvertArrayToRange(result);
        }

        /// <summary>
        /// Generate a new set of full calibration engines for the supplied data.
        /// Add or overwrite the engine store for the new engine.
        /// Each engineId will point to a set of engines indexed by swap tenor and option expiry
        /// </summary>
        /// <param name="engineHandle">Calibration Engine handle</param>
        /// <param name="settingsHandle">Calibartion settings handle</param>
        /// <param name="rawVolsRange">A grid of volatilities (with row/column labels)</param>
        /// <param name="rawAssetsRange">A grid of asset values</param>
        /// <param name="optionEx">The option expiry to index against</param>
        /// <returns></returns>
        public string CalibrateSabrModel(string engineHandle, string settingsHandle, Excel.Range rawVolsRange, Excel.Range rawAssetsRange, string optionEx)
        {
            var volsRawTable = rawVolsRange.Value[System.Reflection.Missing.Value] as object[,];
            var assetRawTable = rawAssetsRange.Value[System.Reflection.Missing.Value] as object[,];
            return SABRHelper.CalibrateSabrModel(engineHandle, settingsHandle, DataRangeHelper.RangeToMatrix(volsRawTable), DataRangeHelper.RangeToMatrix(assetRawTable), optionEx);
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
        /// <param name="assetCode">The tenor value to use in identifying this calibration engine</param>
        /// <returns></returns>
        public string CalibrateSabrATMModelWithTenor(string engineHandle, string settingsHandle, double nu, double rho,
            double atmVolatility, double assetPrice, string exerciseTime, string assetCode)
        {
            return SABRHelper.CalibrateSabrAtmModelWithTenor(engineHandle, settingsHandle, nu, rho,
                                                                       atmVolatility, assetPrice, exerciseTime, assetCode);
        }

        /// <summary>
        /// Generate an Interpolated Swaption Calibration Engine using the supplied parameters.
        /// The calibrationArray has the handles of the Calibrated Engines to use in the interpolation processs.
        /// The interpolation can only be used in the ExpiryTime dimension to generate a new engine.
        /// If the engine array refers to unknown engines the process will fail (TBD)
        /// </summary>
        /// <param name="engineHandle">The name of the new interpolated calibration engine</param>
        /// <param name="settingsHandle">The settings to use in calibrating the engine</param>
        /// <param name="calibrationRange">The calibration range. </param>
        /// <param name="atmVolatility">The At The Money volatility to use in engine calibration</param>
        /// <param name="assetPrice">The Asset price to use</param>
        /// <param name="exerciseTime">The exercise time to create the new engine for</param>
        /// <param name="tenor">The tenor to create the new engine for. This must be a valid tenor</param>
        /// <returns></returns>
        public string CalibrateSabrInterpolatedModel(string engineHandle, string settingsHandle, Excel.Range calibrationRange,
            double atmVolatility, double assetPrice, string exerciseTime, string tenor)
        {
            var handles = calibrationRange.Value[System.Reflection.Missing.Value] as object[,];
            return SABRHelper.CalibrateSabrInterpolatedModel(engineHandle, settingsHandle, DataRangeHelper.RangeToMatrix(handles),
                                                                                   atmVolatility, assetPrice, exerciseTime, tenor);
        }

        /// <summary>
        /// Get the Alpha value for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public decimal GetSabrParameterAlpha(string engine, string expiry, string tenor)
        {
            return SABRHelper.GetSabrParameterAlpha(engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Beta value for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public decimal GetSabrParameterBeta(string engine, string expiry, string tenor)
        {
            return SABRHelper.GetSabrParameterBeta(engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Nu value for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public decimal GetSabrParameterNu(string engine, string expiry, string tenor)
        {
            return SABRHelper.GetSabrParameterNu(engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Rho value for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public decimal GetSabrParameterRho(string engine, string expiry, string tenor)
        {
            return SABRHelper.GetSabrParameterRho(engine, expiry, tenor);
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
            return SABRHelper.IsModelCalibrated(engine, expiry, tenor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <returns></returns>
        public decimal CalibrationError(string engine, string expiry, string tenor)
        {
            return SABRHelper.CalibrationError(engine, expiry, tenor);
        }

        #endregion

        #endregion

        #region Interpolation Methods

        /// <summary>
        /// Compute a Bilinear interpolation at the ordered pair (ColumnTarget, RowTarget).
        /// Extrapolation is flat-line in both the Column and Row dimensions.
        /// </summary>
        /// <param name="columnLabels">One dimensional array arranged in strict ascending order that governs HORIZONTAL interpolation.</param>
        /// <param name="rowLabels">One dimensional array arranged in strict ascending order that governs VERTICAL interpolation.</param>
        /// <param name="dataTable">Two dimensional array of known values with size equal to RowLabels x ColumnLabels.</param>
        /// <param name="columnTarget">Column (horizontal) target of the interpolation.</param>
        /// <param name="rowTarget">Row (vertical) target of the interpolation.</param>
        /// <returns></returns>
        public object SABRInterpolationBilinear(Excel.Range columnLabels, Excel.Range rowLabels, Excel.Range dataTable,
            double columnTarget, double rowTarget)
        {
            List<double> columns = DataRangeHelper.StripDoubleRange(columnLabels);
            List<double> rows = DataRangeHelper.StripDoubleRange(rowLabels);
            var values = dataTable.Value[System.Reflection.Missing.Value] as object[,];
            var valuesAsDoubles = RangeHelper.RangeToDoubleMatrix(values);
            return SABRInterpolationInterface.BilinearInterpolate(columns.ToArray(), rows.ToArray(), valuesAsDoubles, columnTarget, rowTarget);
        }

        /// <summary>
        /// Compute a one dimensional (piecewise) Linear interpolation. Extrapolation is flat-line.
        /// </summary>
        /// <param name="xArray">One dimensional array of x-values. Array is not required to be in ascending order.</param>
        /// <param name="yArray">One dimensional array of known y-values. Length of the array must be equal to the length of the XArray parameter.</param>
        /// <param name="target">Value at which to compute the interpolation.</param>
        /// <returns></returns>
        public object SABRInterpolationLinear(Excel.Range xArray, Excel.Range yArray, double target)
        {
            List<double> x = DataRangeHelper.StripDoubleRange(xArray);
            List<double> y = DataRangeHelper.StripDoubleRange(yArray);
            return SABRInterpolationInterface.LinearInterpolate(x.ToArray(), y.ToArray(), target);
        }

        /// <summary>
        /// Compute an interpolated value from a set of known (x,y) values by Cubic Hermite Spline interpolation.
        /// Extrapolation is NOT available.
        /// </summary>
        /// <param name="xArray">Array (row/column) of x-values.</param>
        /// <param name="yArray">Array (row/column) of y-values.</param>
        /// <param name="target">Value at which to compute the interpolation.</param>
        /// <returns></returns>
        public object SABRInterpolationCubicHermiteSpline(Excel.Range xArray, Excel.Range yArray, double target)
        {
            List<double> x = DataRangeHelper.StripDoubleRange(xArray);
            List<double> y = DataRangeHelper.StripDoubleRange(yArray);
            return SABRInterpolationInterface.CubicHermiteSplineInterpolate(x.ToArray(), y.ToArray(), target);
        }

        #endregion

        #region SABR - The Generic Case

        #region SABR Calibration - Generic Case

        /// <summary>
        /// Create a settings object to use with SABR Calibration
        /// </summary>, Engine.
        /// <param name="settingsHandle">The settings engine</param>
        /// <param name="beta">The beta value</param>
        /// <param name="interpolationType">Interpolation type to use</param>
        /// <returns></returns>
        public string SABRSettings(string settingsHandle, decimal beta, string interpolationType)
        {
            return SABRInterface.Instance().SABRCalibrationSettings(settingsHandle, beta, interpolationType);
        }

        /// <summary>
        /// Create a SABR CapFloor Calibration Engine
        /// </summary>
        /// <param name="engineHandle">The handle for this engine</param>
        /// <param name="settingsHandle">The handle to the settings object we use for this calibration</param>
        /// <param name="bootstrapFixedStrikeHandle">The fixed strike engines to use as a base</param>
        /// <param name="bootstrapATMHandle">The ATM engine used in Calibration</param>
        /// <returns>The handle of this engine</returns>
        public string SABREngine(string engineHandle, string settingsHandle, string bootstrapFixedStrikeHandle, string bootstrapATMHandle)
        {
            return SABRInterface.Instance().SABRCalibrationEngine(engineHandle, settingsHandle, bootstrapFixedStrikeHandle, bootstrapATMHandle);
        }

        /// <summary>
        /// Compute a Volatility Smile given an engine, a date and a list of strikes
        /// </summary>
        /// <param name="engineHandle">The SABR engine to use</param>
        /// <param name="date">The date to compute for</param>
        /// <param name="strikesAsArray">The strikes that make up the smile. This can be a single strike (ATM)</param>
        /// <returns></returns>
        public object[,] SABRComputeVolatility(string engineHandle, DateTime date, Excel.Range strikesAsArray)
        {
            var strikes = DataRangeHelper.StripDecimalRange(strikesAsArray);
            //TODO this does not work with an decimal[,] conversion to object[,]
            var result = SABRInterface.Instance().SABRComputeVolatility(Engine.Logger, Engine.Cache, NameSpace, engineHandle, date, strikes.ToArray());
            return RangeHelper.ConvertArrayToRange(result);
        }

        #endregion

        #region SABR Volatility Curve Engines - Generic Case

        /// <summary>
        /// Create a series of engines from a raw volatility grid
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdArray">A list of instrumentIds to update of the form AUD-IRCap-3Y</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateVolatilityCurveEngine(Excel.Range propertiesAs2DRange, Excel.Range instrumentIdArray,
            Excel.Range rawVolatilityGrid)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var volatilities = rawVolatilityGrid.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDecimals = RangeHelper.RangeToDecimalMatrix(volatilities);
            List<string> instTypes = DataRangeHelper.StripRange(instrumentIdArray);
            return SABRInterface.Instance().CreateVolatilityCurve(Engine.Logger, Engine.Cache, NameSpace, namedValueSet, instTypes.ToArray(),
                volatilitiesAsDecimals);
        }

        /// <summary>
        /// Create a series of engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdArray">A list of instrumentIds to update of the form AUD-IRCap-3Y</param>
        /// <param name="strikesAsArray">An array of strikes.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateVolatilityCurveEnginesWithStrikes(Excel.Range propertiesAs2DRange, Excel.Range instrumentIdArray,
            Excel.Range strikesAsArray, Excel.Range rawVolatilityGrid)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var volatilities = rawVolatilityGrid.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDecimals = RangeHelper.RangeToDecimalMatrix(volatilities);
            List<string> volTypes = DataRangeHelper.StripRange(instrumentIdArray);
            List<decimal> strikes = DataRangeHelper.StripDecimalRange(strikesAsArray);
            return SABRInterface.Instance().CreateVolatilityCurves(Engine.Logger, Engine.Cache, NameSpace, namedValueSet,
                volTypes.ToArray(), strikes.ToArray(), volatilitiesAsDecimals);
        }

        /// <summary>
        /// Create a series of engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="volTypeAsArray">An array of volatiltiy types.</param>
        /// <param name="volatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateATMVolatilityCurve(Excel.Range propertiesAs2DRange, Excel.Range volTypeAsArray,
            Excel.Range volatilityGrid)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> volTypes = DataRangeHelper.StripRange(volTypeAsArray);
            List<decimal> vols = DataRangeHelper.StripDecimalRange(volatilityGrid);
            return SABRInterface.Instance().CreateATMVolatilityCurve(Engine.Logger, Engine.Cache, NameSpace, namedValueSet, volTypes.ToArray(),
                vols.ToArray());
        }

        /// <summary>
        /// Compute a Volatility using a bootstrapped engine and the set of identifying parameters
        /// </summary>
        /// <param name="engineHandle">The engine handle to the engines group to use</param>
        /// <param name="strike">The strike at which to compute the volatility</param>
        /// <param name="baseDate">The base date to use to perform the calculation</param>
        /// <param name="target">The target date to compute the volatility at</param>
        /// <returns></returns>
        public decimal ComputeVolatility(string engineHandle, decimal strike, DateTime baseDate, DateTime target)
        {
            return SABRInterface.Instance().BootstrapComputeVolatility(engineHandle, strike, baseDate, target);
        }

        /// <summary>
        /// Compute Volatilities using a bootstrapped engine and the set of identifying parameters
        /// for a range of dates
        /// </summary>
        /// <param name="engineHandle">The engine handle to the engines group to use</param>
        /// <param name="strike">The strike at which to compute the volatility</param>
        /// <param name="baseDate">The base date to use to perform the calculation</param>
        /// <param name="targetDateTimeArray">The target dates to compute the volatility at</param>
        /// <returns></returns>
        public object[,] ComputeVolatilities(string engineHandle, decimal strike, DateTime baseDate, Excel.Range targetDateTimeArray)
        {
            List<DateTime> dfDataTimeGrid = DataRangeHelper.StripDateTimeRange(targetDateTimeArray);
            var result = SABRInterface.Instance().BootstrapComputeVolatility(engineHandle, strike, baseDate, dfDataTimeGrid.ToArray()).ToArray();
            return RangeHelper.ConvertArrayToRange(result);
        }

        #endregion

        #region SABR Parameters - Generic Case

        /// <summary>
        /// Get the SABR Calibration engine parameter for the provided options
        /// The exercise/tenor pair form the key to the correct calibration engine
        /// from an underlying store.
        /// </summary>
        /// <param name="param">The parameter type to return</param>
        /// <param name="engine">The engine to use</param>
        /// <returns></returns>
        public decimal GetSABRParameter(string param, string engine)
        {
            var typeofParam = (SABRInterface.CalibrationParameter)Enum.Parse(typeof(SABRInterface.CalibrationParameter), param);
            return SABRInterface.Instance().GetSABRParameter(typeofParam, engine);
        }

        /// <summary>
        /// Get the Calibration status for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <returns></returns>
        public bool IsSABRModelCalibrated(string engine)
        {
            return SABRInterface.Instance().IsSABRModelCalibrated(engine);
        }

        /// <summary>
        /// Get the Calibration Engine Calibration Error the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <returns></returns>
        public decimal SABRCalibrationError(string engine)
        {
            return SABRInterface.Instance().SABRCalibrationError(engine);
        }

        #endregion

        #endregion

        #region Basic Cap Floor Engine Functions

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdArray">A list of instrumentIds to update of the form AUD-IRCap-3Y</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateSimpleCapFloorEngine(Excel.Range propertiesAs2DRange, Excel.Range instrumentIdArray,
            Excel.Range rawVolatilityGrid)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var volatilities = rawVolatilityGrid.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDecimals = RangeHelper.RangeToDecimalMatrix(volatilities);
            List<string> instTypes = DataRangeHelper.StripRange(instrumentIdArray);
            return SABRCapFloorInterface.Instance().CreateCapFloorCurve(Engine.Logger, Engine.Cache, NameSpace, namedValueSet, instTypes.ToArray(),
                volatilitiesAsDecimals);
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdArray">A list of instrumentIds to update of the form AUD-IRCap-3Y</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <param name="dfDataTimeGridAsArray">The discount factor dates array.</param>
        /// <param name="dfGridAsArray">The discount factors required for bootstrapping</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorEngine(Excel.Range propertiesAs2DRange, Excel.Range instrumentIdArray,
                                           Excel.Range rawVolatilityGrid, Excel.Range dfDataTimeGridAsArray, Excel.Range dfGridAsArray)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var volatilities = rawVolatilityGrid.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDecimals = RangeHelper.RangeToDecimalMatrix(volatilities);
            List<string> instTypes = DataRangeHelper.StripRange(instrumentIdArray);
            List<DateTime> dfDataTimeGrid = DataRangeHelper.StripDateTimeRange(dfDataTimeGridAsArray);
            List<decimal> dfGrid = DataRangeHelper.StripDecimalRange(dfGridAsArray);
            return SABRCapFloorInterface.Instance().CreateCapFloorCurve(Engine.Logger, Engine.Cache, NameSpace, namedValueSet, instTypes.ToArray(),
                                            volatilitiesAsDecimals, dfDataTimeGrid.ToArray(), dfGrid.ToArray());
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdArray">A list of instrumentIds to update of the form AUD-IRCap-3Y</param>
        /// <param name="strikesAsArray">An array of strikes.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateSimpleCapFloorEnginesWithStrikes(Excel.Range propertiesAs2DRange, Excel.Range instrumentIdArray,
            Excel.Range strikesAsArray, Excel.Range rawVolatilityGrid)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var volatilities = rawVolatilityGrid.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDecimals = RangeHelper.RangeToDecimalMatrix(volatilities);
            List<string> volTypes = DataRangeHelper.StripRange(instrumentIdArray);
            List<decimal> strikes = DataRangeHelper.StripDecimalRange(strikesAsArray);
            return SABRCapFloorInterface.Instance().CreateCapFloorCurve(Engine.Logger, Engine.Cache, NameSpace, namedValueSet,
                volTypes.ToArray(), strikes.ToArray(), volatilitiesAsDecimals);
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdArray">A list of instrumentIds to update of the form AUD-IRCap-3Y</param>
        /// <param name="strikesAsArray">An array of strikes.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <param name="dfDataTimeGridAsArray">The discount factor dates array.</param>
        /// <param name="dfGridAsArray">The discount factors required for bootstrapping</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorEnginesWithStrikes(Excel.Range propertiesAs2DRange, Excel.Range instrumentIdArray,
                                                       Excel.Range strikesAsArray, Excel.Range rawVolatilityGrid,
            Excel.Range dfDataTimeGridAsArray, Excel.Range dfGridAsArray)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var volatilities = rawVolatilityGrid.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDecimals = RangeHelper.RangeToDecimalMatrix(volatilities);
            List<string> volTypes = DataRangeHelper.StripRange(instrumentIdArray);
            List<DateTime> dfDataTimeGrid = DataRangeHelper.StripDateTimeRange(dfDataTimeGridAsArray);
            List<decimal> dfGrid = DataRangeHelper.StripDecimalRange(dfGridAsArray);
            List<decimal> strikes = DataRangeHelper.StripDecimalRange(strikesAsArray);
            return SABRCapFloorInterface.Instance().CreateCapFloorCurve(Engine.Logger, Engine.Cache, NameSpace, namedValueSet,
                volTypes.ToArray(), strikes.ToArray(), volatilitiesAsDecimals, dfDataTimeGrid.ToArray(), dfGrid.ToArray());
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="volTypeAsArray">An array of volatiltiy types.</param>
        /// <param name="volatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateSimpleCapFloorATMEngines(Excel.Range propertiesAs2DRange, Excel.Range volTypeAsArray,
            Excel.Range volatilityGrid)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> volTypes = DataRangeHelper.StripRange(volTypeAsArray);
            List<decimal> vols = DataRangeHelper.StripDecimalRange(volatilityGrid);
            return SABRCapFloorInterface.Instance().CreateCapFloorATMCurve(Engine.Logger, Engine.Cache, NameSpace, namedValueSet, volTypes.ToArray(),
                vols.ToArray());
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="volTypeAsArray">An array of volatiltiy types.</param>
        /// <param name="volatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <param name="dfDataTimeGridAsArray">The discount factor dates array.</param>
        /// <param name="dfGridAsArray">The discount factors required for bootstrapping</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorATMEngines(Excel.Range propertiesAs2DRange, Excel.Range volTypeAsArray,
                                               Excel.Range volatilityGrid, Excel.Range dfDataTimeGridAsArray, Excel.Range dfGridAsArray)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> volTypes = DataRangeHelper.StripRange(volTypeAsArray);
            List<DateTime> dfDataTimeGrid = DataRangeHelper.StripDateTimeRange(dfDataTimeGridAsArray);
            List<decimal> dfGrid = DataRangeHelper.StripDecimalRange(dfGridAsArray);
            List<decimal> vols = DataRangeHelper.StripDecimalRange(volatilityGrid);
            return SABRCapFloorInterface.Instance().CreateCapFloorATMCurve(Engine.Logger, Engine.Cache, NameSpace, namedValueSet, volTypes.ToArray(),
                                                vols.ToArray(), dfDataTimeGrid.ToArray(), dfGrid.ToArray());
        }

        /// <summary>
        /// Compute a Caplet Volatility using a bootstrapped engine and the set of identifying parameters
        /// </summary>
        /// <param name="engineHandle">The engine handle to the engines group to use</param>
        /// <param name="strike">The strike at which to compute the volatility</param>
        /// <param name="baseDate">The base date to use to perform the calculation</param>
        /// <param name="target">The target date to compute the volatility at</param>
        /// <returns></returns>
        public decimal ComputeCapletVolatility(string engineHandle, decimal strike, DateTime baseDate, DateTime target)
        {
            return SABRCapFloorInterface.Instance().BootstrapComputeCapletVolatility(engineHandle, strike, baseDate, target);
        }

        /// <summary>
        /// Compute Caplet Volatilities using a bootstrapped engine and the set of identifying parameters
        /// for a range of dates
        /// </summary>
        /// <param name="engineHandle">The engine handle to the engines group to use</param>
        /// <param name="strike">The strike at which to compute the volatility</param>
        /// <param name="baseDate">The base date to use to perform the calculation</param>
        /// <param name="targetDateTimeArray">The target dates to compute the volatility at</param>
        /// <returns></returns>
        public object[,] ComputeCapletVolatilities(string engineHandle, decimal strike, DateTime baseDate, Excel.Range targetDateTimeArray)
        {
            List<DateTime> dfDataTimeGrid = DataRangeHelper.StripDateTimeRange(targetDateTimeArray);
            var result = SABRCapFloorInterface.Instance().BootstrapComputeCapletVolatility(engineHandle, strike, baseDate, dfDataTimeGrid.ToArray()).ToArray();
            return RangeHelper.ConvertArrayToRange(result);
        }

        #endregion

        #region CapFloor Engine Utility Methods

        /// <summary>
        /// List all existing bootstrap engines
        /// This method will list any created using Bootstrap methods
        /// and will check for and generate any engines retrieved by subscription
        /// </summary>
        /// <returns></returns>
        public object[,] ListAllCapletBootstrapEngines()
        {
            var result = SABRCapFloorInterface.Instance().ListCapletBootstrapEngines();
            return RangeHelper.ConvertArrayToRange(result);
        }

        #endregion

        #region CapFloor Frequency Conversions

        ///// <summary>
        ///// Create and initialize an instance of the CapletTenorConversion class
        ///// </summary>
        ///// <param name="handle">The engine to attach this tenor converter to</param>
        ///// <param name="interpolation"></param>
        ///// <returns></returns>
        //public string CreateCapFloorFrequency(string handle, string interpolation)
        //{
        //    return SABRCapFloorInterface.Instance().CapFloorFrequencyCreate(Engine.Logger, Engine.Cache, NameSpace, handle, interpolation);
        //}

        ///// <summary>
        ///// Compute the volatility using the tenor frequency conversion
        ///// </summary>
        ///// <param name="expiry"></param>
        ///// <param name="frequency"></param>
        ///// <returns></returns>
        //public Decimal ConvertCapFloorFrequency(DateTime expiry, string frequency)
        //{
        //    return SABRCapFloorInterface.Instance().CapFloorFrequencyConvert(Engine.Logger, Engine.Cache, NameSpace, expiry, frequency);
        //}

        #endregion

        #region SABR CapFloor Calibration

        /// <summary>
        /// Create a settings object to use with SABR Calibration
        /// </summary>, Engine.
        /// <param name="settingsHandle">The settings engine</param>
        /// <param name="beta">The beta value</param>
        /// <param name="interpolationType">Interpolation type to use</param>
        /// <returns></returns>
        public string SABRCapFloorSettings(string settingsHandle, decimal beta, string interpolationType)
        {
            return SABRCapFloorInterface.Instance().SABRCapFloorCalibrationSettings(settingsHandle, beta, interpolationType);
        }

        /// <summary>
        /// Create a SABR CapFloor Calibration Engine
        /// </summary>
        /// <param name="engineHandle">The handle for this engine</param>
        /// <param name="settingsHandle">The handle to the settings object we use for this calibration</param>
        /// <param name="bootstrapFixedStrikeHandle">The fixed strike engines to use as a base</param>
        /// <param name="bootstrapATMHandle">The ATM engine used in Calibration</param>
        /// <returns>The handle of this engine</returns>
        public string SABRCapFloorEngine(string engineHandle, string settingsHandle, string bootstrapFixedStrikeHandle, string bootstrapATMHandle)
        {
            return SABRCapFloorInterface.Instance().SABRCapFloorCalibrationEngine(engineHandle, settingsHandle, bootstrapFixedStrikeHandle, bootstrapATMHandle);
        }

        /// <summary>
        /// Compute a Volatility Smile given an engine, a date and a list of strikes
        /// </summary>
        /// <param name="engineHandle">The SABR engine to use</param>
        /// <param name="date">The date to compute for</param>
        /// <param name="strikesAsArray">The strikes that make up the smile. This can be a single strike (ATM)</param>
        /// <returns></returns>
        public object[,] SABRCapFloorComputeVolatility(string engineHandle, DateTime date, Excel.Range strikesAsArray)
        {
            var strikes = DataRangeHelper.StripDecimalRange(strikesAsArray);
            //TODO this does not work with an decimal[,] conversion to object[,]
            var result = SABRCapFloorInterface.Instance().SABRCapFloorComputeVolatility(Engine.Logger, Engine.Cache, NameSpace, engineHandle, date, strikes.ToArray());
            return RangeHelper.ConvertArrayToRange(result);
        }

        #endregion

        #region Legacy SABR

        #region SABR Swaption Calibration

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
        public string AddSwaptionCalibrationSettings(string handle, string instrument, string ccy, double beta)
        {
            return SABRSwaptionInterface.Instance().SABRAddCalibrationSettings(handle, instrument, ccy, (decimal)beta);
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
        /// <param name="strikesAsArray">An array of strikes.</param>
        /// <param name="volsAsRange">A grid of volatilities (with row/column labels)</param>
        /// <param name="assetsAsRange">A grid of asset values</param>
        /// <param name="assetExpiryAsArray">An array of asset expiries.</param>
        /// <param name="optionEx">The ption expiry to index against</param>
        /// <param name="tenorsAsArray">An array of expiry tenors.</param>
        /// <param name="assetTenorsAsArray">An array of asset tenors.</param>
        /// <returns></returns>
        public string CalibrateSwaptionModel(string engineHandle, string settingsHandle, Excel.Range tenorsAsArray, Excel.Range strikesAsArray,
                                     Excel.Range volsAsRange, Excel.Range assetsAsRange, Excel.Range assetTenorsAsArray, Excel.Range assetExpiryAsArray, string optionEx)
        {
            var tenors = DataRangeHelper.StripRange(tenorsAsArray);
            var assetExpiry = DataRangeHelper.StripRange(assetExpiryAsArray);
            var assetTenors = DataRangeHelper.StripRange(assetTenorsAsArray);
            var strikes = DataRangeHelper.StripDecimalRange(strikesAsArray);
            var volatilities = volsAsRange.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDecimals = RangeHelper.RangeToDecimalMatrix(volatilities);
            var assets = assetsAsRange.Value[System.Reflection.Missing.Value] as object[,];
            var assetsAsDecimals = RangeHelper.RangeToDecimalMatrix(assets);
            return SABRSwaptionInterface.Instance().SABRCalibrateModel(engineHandle, settingsHandle, tenors.ToArray(), strikes.ToArray(), volatilitiesAsDecimals, assetsAsDecimals,
                                                 assetTenors.ToArray(), assetExpiry.ToArray(), optionEx);
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
        public string CalibrateATMSwaptionModel(string engineHandle, string settingsHandle, decimal nu, decimal rho, decimal atmVolatility,
                                        decimal assetPrice, string exerciseTime)
        {
            return SABRSwaptionInterface.Instance().SABRCalibrateATMModel(engineHandle, settingsHandle, nu, rho, atmVolatility,
                                                    assetPrice, exerciseTime);
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
        public string CalibrateSwaptionInterpolatedModel(string engineHandle, string settingsHandle, Excel.Range calibrationArray, 
            decimal atmVolatility, decimal assetPrice, string exerciseTime, string tenor)
        {
            var calibrations = DataRangeHelper.StripRange(calibrationArray);
            return SABRSwaptionInterface.Instance().SABRCalibrateInterpolatedModel(engineHandle, settingsHandle, calibrations.ToArray(), atmVolatility,
                                                             assetPrice, exerciseTime, tenor);
        }

        #endregion

        #region Compute Swaption Volatility

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
        public decimal InterpolateSwaptionVolatility(string engineHandle, string exerciseTime, string assetCode, double strike)
        {
            return SABRSwaptionInterface.Instance().SABRInterpolateVolatility(engineHandle, exerciseTime, assetCode, (decimal)strike);
        }

        #endregion

        #region SABR ATM Swaption Calibration

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
        public string CalibrateSabrATMSwaptionModelWithTenor(string engineHandle, string settingsHandle, decimal nu, decimal rho, decimal atmVolatility,
                                                     decimal assetPrice, string exerciseTime, string assetCode)
        {
            return SABRSwaptionInterface.Instance().SABRCalibrateATMModel(engineHandle, settingsHandle, nu, rho, atmVolatility,
                                                    assetPrice, exerciseTime, assetCode);
        }

        #endregion

        #region SABR Swaption Parameters

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
        public decimal GetSabrSwaptionParameter(string param, string engine, string pExercise, string pTenor)
        {
            var typeofParam = (SABRSwaptionInterface.CalibrationParameter)Enum.Parse(typeof(SABRSwaptionInterface.CalibrationParameter), param);
            //SABRSwaptionInterface.GetSABRParameter
            return SABRSwaptionInterface.Instance().GetSABRParameter(typeofParam, engine, pExercise, pTenor);
        }

        /// <summary>
        /// Get the Calibration status for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public bool IsSwaptionModelCalibrated(string engine, string expiry, string tenor)
        {
            //SABRSwaptionInterface.IsSABRModelCalibrated
            return SABRSwaptionInterface.Instance().IsSABRModelCalibrated(engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Calibration Engine Calibration Error the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public decimal SwaptionCalibrationError(string engine, string expiry, string tenor)
        {
            //SABRSwaptionInterface.CalibrationError
            return SABRSwaptionInterface.Instance().SABRCalibrationError(engine, expiry, tenor);
        }

        #endregion

        #endregion
    }
}