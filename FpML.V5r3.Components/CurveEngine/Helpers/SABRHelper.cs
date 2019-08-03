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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Swaption = Orion.CurveEngine.PricingStructures.SABR.Swaption;

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// This is a lightweight wrapper that interfaces between the SABRInterface class and the ExcelAPI.
    /// The wrapper acts as a layer that will map Excel specific data/operations to Interface expected types
    /// </summary>
    public class SABRHelper
    {
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
        public static decimal SabrImpliedVolatility(string engineHandle, string exerciseTime, string assetCode, double strike)
        {
            decimal pStrike = Convert.ToDecimal(strike / 100);
            decimal result = Swaption.Instance().SABRInterpolateVolatility(engineHandle, exerciseTime,
                assetCode, pStrike);
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
        public static string CalibrateSabrAtmModel(string engineHandle, string settingsHandle, double nu, double rho,
            double atmVolatility, double assetPrice, string exerciseTime)
        {
            // Create the parameters used in this engine
            decimal pNu = Convert.ToDecimal(nu);
            decimal pRho = Convert.ToDecimal(rho);
            decimal pAtmVolatility = Convert.ToDecimal(atmVolatility) / 100.0m;
            decimal pAssetPrice = Convert.ToDecimal(assetPrice) / 100.0m;
            return
                Swaption.Instance().SABRCalibrateATMModel(engineHandle, settingsHandle, pNu, pRho,
                    pAtmVolatility, pAssetPrice, exerciseTime);
        }

        /// <summary>
        /// Generate an ATM (At-The-Money) Swaption Calibration engine using the supplied parameters
        /// This form of engine creates a single cell engine that does not support asset/volatility grid data.
        /// </summary>
        /// <param name="engineHandles">The engine identifier</param>
        /// <param name="settingsHandles">The settings identifier</param>
        /// <param name="nu">Nu value</param>
        /// <param name="rho">Rho value</param>
        /// <param name="atmVolatilities">The ATM volatility</param>
        /// <param name="assetPrices">Asset Price to use</param>
        /// <param name="exerciseTimes">Exercise time for the option</param>
        /// <returns></returns>
        public static string[] CalibrateSabrAtmModels(string[] engineHandles, string[] settingsHandles, decimal[] nu, decimal[] rho,
            decimal[] atmVolatilities, decimal[] assetPrices, string[] exerciseTimes)
        {
            // Create the parameters used in this engine
            return
                Swaption.Instance().SABRCalibrateATMModels(engineHandles, settingsHandles, nu, rho,
                    atmVolatilities, assetPrices, exerciseTimes);
        }

        /// <summary>
        /// Calculate the SABR implied volatilities for the strike values.
        /// This method uses the calibration engine indexed by the exerciseTime/assetCode pair
        /// When an ATM engine is used then the assetCode is ignored.
        /// </summary>
        /// <param name="engineHandle">The CalibrationEngine to use</param>
        /// <param name="exerciseTime">Option Expiry index</param>
        /// <param name="assetCode">Swap Tenor index</param>
        /// <param name="strikes">The strike to calculate Volatilities for, as decimal</param>
        /// <returns></returns>
        public static decimal[] SabrInterpolateVolatilities(string engineHandle, string exerciseTime,
            string assetCode, double[] strikes)
        {
            decimal[] decimalStrikes = strikes.Select(a => (decimal)a).ToArray();
            return Swaption.Instance().SABRInterpolateVolatilities(engineHandle, exerciseTime,
                assetCode, decimalStrikes);
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
        public static string AddSabrCalibrationSettings(string calibrationHandle,
            string calibrationInstrument, string calibrationCurrency, double beta)
        {
            return Swaption.Instance().SabrAddCalibrationSetting(calibrationHandle,
                calibrationInstrument, calibrationCurrency, (decimal)beta);
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
        public static string[] AddSabrCalibrationSettingArray(string[] calibrationHandle,
            string calibrationInstrument, string calibrationCurrency, decimal[] beta)
        {
            return Swaption.Instance().SabrAddCalibrationSettings(calibrationHandle,
                calibrationInstrument, calibrationCurrency, beta);
        }

        /// <summary>
        /// Generate a new set of full calibration engines for the supplied data.
        /// Add or overwrite the engine store for the new engine.
        /// Each engineId will point to a set of engines indexed by swap tenor and option expiry
        /// </summary>
        /// <param name="engineHandle">Calibration Engine handle</param>
        /// <param name="settingsHandle">Calibration settings handle</param>
        /// <param name="rawVols">A grid of volatilities (with row/column labels)</param>
        /// <param name="rawAssets">A grid of asset values</param>
        /// <param name="optionEx">The option expiry to index against</param>
        /// <returns></returns>
        public static string CalibrateSabrModel(string engineHandle, string settingsHandle, object[,] rawVols, object[,] rawAssets, string optionEx)
        {
            // Create the asset and volatility arrays for the Interface
            object[,] volsRawTable = RedimensionVolatilityGrid(rawVols, true);
            // Create the asset and volatility arrays for the Interface
            object[,] assetRawTable = RedimensionAssetGrid(rawAssets);
            return
                Swaption.Instance().SABRCalibrateModel(engineHandle, settingsHandle, volsRawTable, assetRawTable, optionEx);
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
        public static string CalibrateSabrAtmModelWithTenor(string engineHandle, string settingsHandle, double nu, double rho,
            double atmVolatility, double assetPrice, string exerciseTime, string assetCode)
        {
            // Create the parameters used in this engine
            decimal pNu = Convert.ToDecimal(nu);
            decimal pRho = Convert.ToDecimal(rho);
            decimal pAtmVolatility = Convert.ToDecimal(atmVolatility) / 100.0m;
            decimal pAssetPrice = Convert.ToDecimal(assetPrice) / 100.0m;

            return
                Swaption.Instance().SABRCalibrateATMModel(engineHandle, settingsHandle, pNu, pRho,
                    pAtmVolatility, pAssetPrice, exerciseTime, assetCode);
        }

        /// <summary>
        /// Generate an Interpolated Swaption Calibration Engine using the supplied parameters.
        /// The calibrationArray has the handles of the Calibrated Engines to use in the interpolation process.
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
        public static string CalibrateSabrInterpolatedModel(string engineHandle, string settingsHandle, object[,] calibrationRange,
            double atmVolatility, double assetPrice, string exerciseTime, string tenor)
        {
            string[] handles = ConvertRangeToStringArray(calibrationRange);
            decimal atm = Convert.ToDecimal(atmVolatility) / 100.0m;
            decimal asset = Convert.ToDecimal(assetPrice) / 100.0m;
            return Swaption.Instance().SABRCalibrateInterpolatedModel(engineHandle, settingsHandle, handles,
                atm, asset, exerciseTime, tenor);
        }

        /// <summary>
        /// Get the Alpha value for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public static decimal GetSabrParameterAlpha(string engine, string expiry, string tenor)
        {
            return Swaption.Instance().GetSABRParameter(Swaption.CalibrationParameter.Alpha, engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Beta value for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public static decimal GetSabrParameterBeta(string engine, string expiry, string tenor)
        {
            return Swaption.Instance().GetSABRParameter(Swaption.CalibrationParameter.Beta, engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Nu value for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public static decimal GetSabrParameterNu(string engine, string expiry, string tenor)
        {
            return Swaption.Instance().GetSABRParameter(Swaption.CalibrationParameter.Nu, engine, expiry, tenor);
        }

        /// <summary>
        /// Get the Rho value for the calibration engine using
        /// the expiry/tenor pair
        /// </summary>
        /// <param name="engine">The calibration engine handle</param>
        /// <param name="expiry">The exercise time of the option</param>
        /// <param name="tenor">The asset Code of the swap (tenor)</param>
        /// <returns></returns>
        public static decimal GetSabrParameterRho(string engine, string expiry, string tenor)
        {
            return Swaption.Instance().GetSABRParameter(Swaption.CalibrationParameter.Rho, engine, expiry, tenor);
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
            return Swaption.Instance().IsModelCalibrated(engine, expiry, tenor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <returns></returns>
        public static decimal CalibrationError(string engine, string expiry, string tenor)
        {
            return Swaption.Instance().CalibrationError(engine, expiry, tenor);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// When Excel Ranges are converted to arrays they are 1 based...
        /// We need to reconstruct the arrays as 0 based
        /// </summary>
        /// <param name="rawTable"></param>
        /// <returns>0 based 2D array</returns>
        public static object[,] RedimensionVolatilityGrid(object[,] rawTable)
        {
            return RedimensionVolatilityGrid(rawTable, true);
        }

        /// <summary>
        /// When Excel Ranges are converted to arrays they are 1 based...
        /// We need to reconstruct the arrays as 0 based
        /// </summary>
        /// <param name="rawTable"></param>
        /// <param name="convertPercentage">This is defaulted to be true. If false assume no conversion</param>
        /// <returns>0 based 2D array</returns>
        public static object[,] RedimensionVolatilityGrid(object[,] rawTable, bool convertPercentage)
        {
            // Set the upper/lower bounds of the converted array and adjust the array we write to accordingly
            var hi1D = rawTable.GetUpperBound(0);
            var hi2D = rawTable.GetUpperBound(1);
            //var zeroBasedArray = new object[hi1D, hi2D];
            //for (var row = 0; row < hi1D; row++)
            //{
            //    for (var col = 0; col < hi2D; col++)
            //    {
            //        if (rawTable[row + 1, col + 1] != null && rawTable[row + 1, col + 1] is double)
            //        {
            //            if (convertPercentage)
            //                zeroBasedArray[row, col] = Convert.ToDecimal(rawTable[row + 1, col + 1]) / 100.0m;
            //            else
            //                zeroBasedArray[row, col] = Convert.ToDecimal(rawTable[row + 1, col + 1]);
            //        }
            //        else
            //            zeroBasedArray[row, col] = rawTable[row + 1, col + 1];
            //    }
            //}
            //return zeroBasedArray;
            for (int row = 0; row <= hi1D; row++)
            {
                for (int col = 0; col <= hi2D; col++)
                {
                    if (rawTable[row, col] != null && rawTable[row, col] is double && convertPercentage)
                        rawTable[row, col] = Convert.ToDecimal((double)rawTable[row, col] / 100);
                    else
                        rawTable[row, col] = rawTable[row, col];
                }
            }
            return rawTable;
        }

        /// <summary>
        /// When Excel Ranges are converted to arrays they are 1 based...
        /// We need to reconstruct the arrays as 0 based
        /// </summary>
        /// <param name="rawTable"></param>
        /// <returns>0 based 2D array</returns>
        private static object[,] RedimensionAssetGrid(object[,] rawTable)
        {
            // Set the upper/lower bounds of the converted array and adjust the array we write to accordingly
            int rowCount = rawTable.GetUpperBound(0);
            int columnCount = rawTable.GetUpperBound(1);
            // Check whether there is a header row
            int startRow = 0;
            if (rawTable.GetUpperBound(0) > 0 && rawTable.GetUpperBound(1) > 1 && (rawTable[0, 2] == null
                                                                                   || !(rawTable[0, 2] is double || rawTable[0, 2] is int)))
            {
                startRow = 1;
            }
            // We will also strip out the additional column that has no purpose (if it still exists)
            // This requires a cell by cell copy (it will be slow)
            if (rawTable[startRow, 1] == null || rawTable[startRow, 1].ToString() == "Swap Tenor")
            {
                var trimmedArray = new object[rowCount + 1, columnCount];
                for (int i = 0; i <= rowCount; i++)
                {
                    int k = 0;
                    for (int j = 0; j <= columnCount; j++)
                    {
                        if (j != 1)
                        {
                            trimmedArray[i, k] = rawTable[i, j];
                            k++;
                        }
                    }
                }
                rawTable = trimmedArray;
                rowCount = rawTable.GetUpperBound(0);
                columnCount = rawTable.GetUpperBound(1);
            }
            var zeroBasedArray = new object[rowCount - startRow + 1, columnCount + 1];
            for (int row = startRow; row <= rowCount; row++)
            {
                for (int col = 0; col <= columnCount; col++)
                {
                    if (row == startRow)
                        zeroBasedArray[row - startRow, col] = rawTable[row, col];
                    else
                    {
                        if (rawTable[row, col] != null && rawTable[row, col] is double)
                            zeroBasedArray[row - startRow, col] = Convert.ToDecimal(rawTable[row, col]) / 100.0m; // / 100.0m;
                        else
                            zeroBasedArray[row - startRow, col] = rawTable[row, col];
                    }
                }
            }
            return zeroBasedArray;
        }

        /// <summary>
        /// A range will devolve to a 2d array of object. This method will take this and generate
        /// an array of strings. This is required by the Interpolated calibration engine routines.
        /// </summary>
        private static string[] ConvertRangeToStringArray(object[,] objHandles)
        {
            var handleList = new List<string>();
            // object[,] objHandles = (object[,])((Range)calibrationRange).get_Value(Type.Missing);
            int lo1D = objHandles.GetLowerBound(0);
            int hi1D = objHandles.GetUpperBound(0);
            int lo2D = objHandles.GetLowerBound(1);
            for (int idx = lo1D; idx <= hi1D; idx++)
                if (objHandles[idx, lo2D] != null)
                    handleList.Add(objHandles[idx, lo2D].ToString());
            return handleList.ToArray();
        }

        /// <summary>
        /// Search the supplied array for a given name. The companion cell is the value
        /// We must assume that the array has data laid out as rows with names as col[0] and values as col[1]
        /// </summary>
        /// <param name="defs"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object FindNamedObject(object[,] defs, string name)
        {
            var rowUpperBound = defs.GetUpperBound(0) + 1;
            var colUpperBound = defs.GetUpperBound(1);
            var colLowerBound = defs.GetLowerBound(0);
            for (var i = 0; i < rowUpperBound; i++)
            {
                if (defs[i, colLowerBound].ToString().ToLower().Contains(name.ToLower()))
                {
                    return defs[i, colUpperBound];
                }
            }
            return null;
        }

        /// <summary>
        /// Given a name search for a value
        /// </summary>
        /// <param name="table">The source grid holding name/value pairs</param>
        /// <param name="search">The search term</param>
        /// <returns>A matched value or null if no match</returns>
        public static object FindValueFromName(object[,] table, string search)
        {
            object found = null;
            // Get the number of rows to search through
            var maxRows = table.GetUpperBound(0) + 1;
            for (var row = 0; row < maxRows; row++)
            {
                if (table[row, 0].ToString() != search) continue;
                found = table[row, 1];
                break;
            }
            return found;
        }

        /// <summary>
        /// Method to split and store the alpha and numeric parts of a label
        /// This method should be private to the element but is defined here for convenience
        /// </summary>
        /// <param name="label">The source label to split</param>
        /// <param name="alpha">The alpha part of the label</param>
        /// <param name="numeric">The number part of the label</param>
        public static void LabelSplitter(string label, ref string alpha, ref decimal numeric)
        {
            if (alpha == null) throw new ArgumentNullException(nameof(alpha));
            // Remove all spaces from the label.
            var tempLabel = label.Replace(" ", "");
            // Initialise the regular expressions that will be used to match
            // the necessary format.
            const string alphaPattern = "[a-zA-Z]+";
            const string numericPattern = "-*[0-9.]+";
            var alphaRegex = new Regex(alphaPattern, RegexOptions.IgnoreCase);
            var numericRegex = new Regex(numericPattern, RegexOptions.IgnoreCase);
            // Match the alpha and numeric components.
            var alphaMatches = alphaRegex.Matches(tempLabel);
            var numericMatches = numericRegex.Matches(tempLabel);
            alpha = alphaMatches.Count > 0 ? alphaMatches[0].Value.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) : "M";
            numeric = numericMatches.Count > 0 ? Convert.ToDecimal(numericMatches[0].Value, CultureInfo.CurrentCulture) : 0;
        }

        /// <summary>
        /// Generate a day value from a term
        /// </summary>
        /// <param name="term">A term to convert to a value</param>
        /// <param name="dayCountConvention">day count convention to use</param>
        /// <returns></returns>
        public static double GenerateDayValue(string term, double dayCountConvention)
        {
            var i = PeriodHelper.Parse(term);
            var num = Convert.ToDouble(i.periodMultiplier);
            double yearFraction = 0;
            switch (i.period)
            {
                case PeriodEnum.D:
                    yearFraction = num / dayCountConvention;
                    break;
                case PeriodEnum.W:
                    yearFraction = num / 52.0d;
                    break;
                case PeriodEnum.M:
                    yearFraction = num / 12.0d;
                    break;
                case PeriodEnum.Y:
                    yearFraction = num;
                    break;
            }
            return yearFraction;
        }

        /// <summary>
        /// Test the vols data for values of 0 or less (an error)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool ValidateData(IEnumerable<decimal> data)
        {
            return data.All(d => d > 0);
        }


        /// <summary>
        /// Convert a raw label value to a formatted label
        /// This will allow inputs such as 3.0 to default to 3yr
        /// </summary>
        /// <param name="rawLabel"></param>
        /// <returns></returns>
        public static string GenerateTenorLabel(string rawLabel)
        {
            var alpha = string.Empty;
            decimal num = 0;
            LabelSplitter(rawLabel, ref alpha, ref num);
            var i = PeriodHelper.Parse(rawLabel);
            return i.ToString();
        }

        #endregion

        #region Extract Methods

        /// <summary>
        /// Extract the Volatilities from the raw grid.
        /// If we are dealing with ATM vols there will be only a single column of values
        /// </summary>
        /// <param name="rawGrid"></param>
        /// <returns></returns>
        public static decimal[][] ExtractVolatilities(Decimal[] rawGrid)
        {
            return ExtractATMVolatilities(rawGrid);
        }

        /// <summary>
        /// Extract the Volatilities from the raw grid.
        /// If we are dealing with ATM vols there will be only a single column of values
        /// </summary>
        /// <param name="rawGrid"></param>
        /// <returns></returns>
        public static decimal[][] ExtractVolatilities(Decimal[,] rawGrid)
        {
            return ExtractStrikeVolatilities(rawGrid);
        }

        /// <summary>
        /// Extract the ATM volatilities
        /// </summary>
        /// <param name="rawGrid"></param>
        /// <returns></returns>
        public static decimal[][] ExtractATMVolatilities(Decimal[] rawGrid)
        {
            // We can assume that if it reached this method then it is an ATM grid
            var maxRows = rawGrid.GetUpperBound(0) + 1;
            var volatilities = new decimal[maxRows][];
            // Loop through the grid rows at the ATM column and convert values
            for (var row = 0; row < maxRows; row++)
            {
                volatilities[row] = new decimal[1];
                volatilities[row][0] = rawGrid[row];
            }
            return volatilities;
        }

        /// <summary>
        /// Extract the Strike volatilities from the grid
        /// This method will check the range of strikes to determine how many columns
        /// will be in the final array
        /// </summary>
        /// <param name="rawGrid"></param>
        /// <returns></returns>
        public static decimal[][] ExtractStrikeVolatilities(Decimal[,] rawGrid)
        {
            var maxRows = rawGrid.GetUpperBound(0) + 1;
            var maxColumns = rawGrid.GetUpperBound(1) + 1;
            var volatilities = new decimal[maxRows][];
            // Loop through the rows and columns of the grid extracting vols as we go
            for (var row = 0; row < maxRows; row++)
            {
                volatilities[row] = new decimal[maxColumns];
                for (var col = 0; col < maxColumns; col++)
                {
                    // Fancy convert and shift routine to place the vols in the correct array elements
                    volatilities[row][col] = rawGrid[row, col];
                }
            }
            return volatilities;
        }

        #endregion
    }
}