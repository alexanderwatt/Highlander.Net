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
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Constants;
using Highlander.CurveEngine.V5r3.PricingStructures.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.CurveEngine.V5r3.Assets.Helpers;
using Highlander.CurveEngine.V5r3.Extensions;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.NamedValues;
using Math = System.Math;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.SABR
{
    /// <summary>
    /// Interface layer between ExcelAPI and the underlying CapFloor analytics
    /// </summary>
    public class SABRCapFloorInterface
    {
        #region Singleton Elements

        /// <summary>
        /// A collection of CapFloor engines maintained by a dictionary indexed by the engineHandle
        /// The internal list comprises the individual engines sorted by strike.
        /// </summary>
        private readonly Dictionary<string, SortedList<decimal, CapVolatilityCurve>> _capFloorEngine;

        /// <summary>
        /// A collection of SABR calibration settings
        /// </summary>
        private readonly Dictionary<string, CapletSmileCalibrationSettings> _sabrCapFloorSettings;

        /// <summary>
        /// A collection of SABR Calibrated CapFloor engines
        /// </summary>
        private readonly Dictionary<string, CapletSmileCalibrationEngine> _sabrCapFloorEngines;

        /// <summary>
        /// Singleton instance controlling the interface
        /// </summary>
        private static SABRCapFloorInterface _instance;

        #endregion

        #region Singleton Interface

        /// <summary>
        /// Protect this class from multiple instances
        /// </summary>
        private SABRCapFloorInterface()
        {
            _capFloorEngine = new Dictionary<string, SortedList<decimal, CapVolatilityCurve>>();
            _sabrCapFloorSettings = new Dictionary<string, CapletSmileCalibrationSettings>();
            _sabrCapFloorEngines = new Dictionary<string, CapletSmileCalibrationEngine>();
        }

        /// <summary>
        /// Static Instance method to access this class
        /// </summary>
        /// <returns></returns>
        public static SABRCapFloorInterface Instance()
        {
            return _instance ?? (_instance = new SABRCapFloorInterface());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SABRCapFloorInterface NewInstance()
        {
            return _instance = new SABRCapFloorInterface();
        }

        #endregion

        #region Interface Methods

        #region CapFloor Bootstrap Methods

        /// <summary>
        /// Create a settings object from the supplied settings object
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public NamedValueSet CreateCapFloorProperties(object[,] settings)
        {
            var columns = settings.GetUpperBound(1);
            //This will only process a two column object matrix.
            if (columns != 1) return null;
            var properties = (object[,])TrimNulls(settings);
            var namedValueSet = properties.ToNamedValueSet();
            return namedValueSet;
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <param name="dfDataTimeGrid">The discount factor dates array.</param>
        /// <param name="dfGrid">The discount factors required for bootstrapping</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, 
            String[] instruments, Decimal[] rawVolatilityGrid, DateTime[] dfDataTimeGrid, Decimal[] dfGrid)
        {
            var volatilityCheck = CheckVolatilities(rawVolatilityGrid);
            if (volatilityCheck != null) throw new ArgumentException(volatilityCheck);
            var id = properties.GetString("EngineHandle", true);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var valuationDate = properties.GetValue("ValuationDate", DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set("ValuationDate", baseDate);
            }
            properties.Set("PricingStructureType", PricingStructureTypeEnum.CapVolatilityCurve.ToString());
            var strikeQuoteUnits = properties.GetString("StrikeQuoteUnits", null);
            if (strikeQuoteUnits == null)
            {
                properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString("MeasureType", null);
            if (measureType == null)
            {
                properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString("QuoteUnits", null);
            if (quoteUnits == null)
            {
                properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString("Algorithm", null);
            if (algorithm == null)
            {
                properties.Set("Algorithm", "Default");
            }
            InterpolationMethod interp = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            // Check there are valid strikes
            IRateCurve discountCurve =
                new SimpleDiscountFactorCurve(baseDate, interp, true, dfDataTimeGrid, dfGrid);
            // Create the engines and either add to, or overwrite the existing, collection
            if (_capFloorEngine.ContainsKey(id))
                _capFloorEngine[id] = CreateCurves(logger, cache, nameSpace, properties, discountCurve,
                    discountCurve, instruments, rawVolatilityGrid);
            else
                _capFloorEngine.Add(id,
                    CreateCurves(logger, cache, nameSpace, properties, discountCurve, discountCurve, instruments,
                        rawVolatilityGrid));
            return id;
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <param name="dfDataTimeGrid">The discount factor dates array.</param>
        /// <param name="dfGrid">The discount factors required for bootstrapping</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties,
            String[] instruments, Decimal[,] rawVolatilityGrid, DateTime[] dfDataTimeGrid, Decimal[] dfGrid)
        {
            var id = properties.GetString("EngineHandle", null);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var valuationDate = properties.GetValue("ValuationDate", DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set("ValuationDate", baseDate);
            }
            properties.Set("PricingStructureType", PricingStructureTypeEnum.CapVolatilityCurve.ToString());
            var strikeQuoteUnits = properties.GetString("StrikeQuoteUnits", null);
            if (strikeQuoteUnits == null)
            {
                properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString("MeasureType", null);
            if (measureType == null)
            {
                properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString("QuoteUnits", null);
            if (quoteUnits == null)
            {
                properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString("Algorithm", null);
            if (algorithm == null)
            {
                properties.Set("Algorithm", "Default");
            }
            InterpolationMethod interp = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            // Check there are valid strikes
            IRateCurve discountCurve = new SimpleDiscountFactorCurve(baseDate, interp, true, dfDataTimeGrid, dfGrid);
            var volatilities = GenerateVolatilityMatrix(instruments, 0, rawVolatilityGrid);
            // Create the engines and either add to, or overwrite the existing, collection
            if (_capFloorEngine.ContainsKey(id))
                _capFloorEngine[id] = CreateCurves(logger, cache, nameSpace, properties, discountCurve, discountCurve, instruments, volatilities);
            else
                _capFloorEngine.Add(id, CreateCurves(logger, cache, nameSpace, properties, discountCurve, discountCurve, instruments, volatilities));
            return id;
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties,
            String[] instruments, Decimal[,] rawVolatilityGrid)
        {
            var id = properties.GetString("EngineHandle", null);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var valuationDate = properties.GetValue("ValuationDate", DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set("ValuationDate", baseDate);
            }
            properties.Set("PricingStructureType", PricingStructureTypeEnum.CapVolatilityCurve.ToString());
            var strikeQuoteUnits = properties.GetString("StrikeQuoteUnits", null);
            if (strikeQuoteUnits == null)
            {
                properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString("MeasureType", null);
            if (measureType == null)
            {
                properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString("QuoteUnits", null);
            if (quoteUnits == null)
            {
                properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString("Algorithm", null);
            if (algorithm == null)
            {
                properties.Set("Algorithm", "Default");
            }
            var referenceDiscountCurve = properties.GetString("ReferenceCurveUniqueId", true);
            var referenceForecastCurve = properties.GetString("ReferenceCurrency2CurveId", referenceDiscountCurve);
            var discountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, referenceDiscountCurve);
            var forecastCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, referenceForecastCurve);
            var volatilities = GenerateVolatilityMatrix(instruments, 0, rawVolatilityGrid);
            // Create the engines and either add to, or overwrite the existing, collection
            if (_capFloorEngine.ContainsKey(id))
                _capFloorEngine[id] = CreateCurves(logger, cache, nameSpace, properties, discountCurve, forecastCurve, instruments, volatilities);
            else
                _capFloorEngine.Add(id, CreateCurves(logger, cache, nameSpace, properties, discountCurve, forecastCurve, instruments, volatilities));
            return id;
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of volatility types with instruments.</param>
        /// <param name="strikes">An array of strikes.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <param name="dfDataTimeGrid">The discount factor dates array.</param>
        /// <param name="dfGrid">The discount factors required for bootstrapping</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, 
            String[] instruments, Decimal[] strikes, Decimal[,] rawVolatilityGrid, DateTime[] dfDataTimeGrid, Decimal[] dfGrid)
        {
            var id = properties.GetString("EngineHandle", null);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var valuationDate = properties.GetValue("ValuationDate", DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set("ValuationDate", baseDate);
            }
            properties.Set("PricingStructureType", PricingStructureTypeEnum.CapVolatilityCurve.ToString());
            var strikeQuoteUnits = properties.GetString("StrikeQuoteUnits", null);
            if (strikeQuoteUnits == null)
            {
                properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString("MeasureType", null);
            if (measureType == null)
            {
                properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString("QuoteUnits", null);
            if (quoteUnits == null)
            {
                properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString("Algorithm", null);
            if (algorithm == null)
            {
                properties.Set("Algorithm", "Default");
            }
            InterpolationMethod interp = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            IRateCurve discountCurve = new SimpleDiscountFactorCurve(baseDate, interp, true, dfDataTimeGrid, dfGrid);
            // Create the engines and either add to, or overwrite the existing, collection
            if (_capFloorEngine.ContainsKey(id))
                _capFloorEngine[id] = CreateCurves(logger, cache, nameSpace, properties, discountCurve, discountCurve, instruments, strikes, rawVolatilityGrid);
            else
                _capFloorEngine.Add(id, CreateCurves(logger, cache, nameSpace, properties, discountCurve, discountCurve, instruments, strikes, rawVolatilityGrid));
            return id;
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of volatility types with instruments.</param>
        /// <param name="strikes">An array of strikes.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties,
            String[] instruments, Decimal[] strikes, Decimal[,] rawVolatilityGrid)
        {
            var id = properties.GetString("EngineHandle", null);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var valuationDate = properties.GetValue("ValuationDate", DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set("ValuationDate", baseDate);
            }
            properties.Set("PricingStructureType", PricingStructureTypeEnum.CapVolatilityCurve.ToString());
            var strikeQuoteUnits = properties.GetString("StrikeQuoteUnits", null);
            if (strikeQuoteUnits == null)
            {
                properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString("MeasureType", null);
            if (measureType == null)
            {
                properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString("QuoteUnits", null);
            if (quoteUnits == null)
            {
                properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString("Algorithm", null);
            if (algorithm == null)
            {
                properties.Set("Algorithm", "Default");
            }
            var referenceDiscountCurve = properties.GetString("ReferenceCurveUniqueId", true);
            var referenceForecastCurve = properties.GetString("ReferenceCurrency2CurveId", referenceDiscountCurve);
            var discountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, referenceDiscountCurve);
            var forecastCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, referenceForecastCurve);
            // Create the engines and either add to, or overwrite the existing, collection
            if (_capFloorEngine.ContainsKey(id))
                _capFloorEngine[id] = CreateCurves(logger, cache, nameSpace, properties, discountCurve, forecastCurve, instruments, strikes, rawVolatilityGrid);
            else
                _capFloorEngine.Add(id, CreateCurves(logger, cache, nameSpace, properties, discountCurve, forecastCurve, instruments, strikes, rawVolatilityGrid));
            return id;
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <param name="dfDataTimeGrid">The discount factor dates array.</param>
        /// <param name="dfGrid">The discount factors required for bootstrapping</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorATMCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, 
            String[] instruments, Decimal[] rawVolatilityGrid, DateTime[] dfDataTimeGrid, Decimal[] dfGrid)
        {
            var volatilityCheck = CheckVolatilities(rawVolatilityGrid);
            if (volatilityCheck != null) throw new ArgumentException(volatilityCheck);
            var id = properties.GetString("EngineHandle", true);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var valuationDate = properties.GetValue("ValuationDate", DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set("ValuationDate", baseDate);
            }
            properties.Set("PricingStructureType", PricingStructureTypeEnum.CapVolatilityCurve.ToString());
            var strikeQuoteUnits = properties.GetString("StrikeQuoteUnits", null);
            if (strikeQuoteUnits == null)
            {
                properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString("MeasureType", null);
            if (measureType == null)
            {
                properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString("QuoteUnits", null);
            if (quoteUnits == null)
            {
                properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString("Algorithm", null);
            if (algorithm == null)
            {
                properties.Set("Algorithm", "Default");
            }
            InterpolationMethod interp = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            // Check there are valid strikes
            IRateCurve discountCurve =
                new SimpleDiscountFactorCurve(baseDate, interp, true, dfDataTimeGrid, dfGrid);
            // Create the engines and either add to, or overwrite the existing, collection
            if (_capFloorEngine.ContainsKey(id))
                _capFloorEngine[id] = CreateCurves(logger, cache, nameSpace, properties, discountCurve, discountCurve, instruments, rawVolatilityGrid);
            else
                _capFloorEngine.Add(id, CreateCurves(logger, cache, nameSpace, properties, discountCurve, discountCurve, instruments, rawVolatilityGrid));
            return id;
        }

        /// <summary>
        /// Create a series of CapFloor engines from a raw volatility grid and a DF curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the engine, including the reference handle to access this engine collection</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns>The engine handle or an error message</returns>
        public string CreateCapFloorATMCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties,
            String[] instruments, Decimal[] rawVolatilityGrid)
        {
            var volatilityCheck = CheckVolatilities(rawVolatilityGrid);
            if (volatilityCheck != null) throw new ArgumentException(volatilityCheck);
            var id = properties.GetString("EngineHandle", true);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var valuationDate = properties.GetValue("ValuationDate", DateTime.MinValue);
            if (valuationDate == DateTime.MinValue)
            {
                properties.Set("ValuationDate", baseDate);
            }
            properties.Set("PricingStructureType", PricingStructureTypeEnum.CapVolatilityCurve.ToString());
            var strikeQuoteUnits = properties.GetString("StrikeQuoteUnits", null);
            if (strikeQuoteUnits == null)
            {
                properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            }
            var measureType = properties.GetString("MeasureType", null);
            if (measureType == null)
            {
                properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            }
            var quoteUnits = properties.GetString("QuoteUnits", null);
            if (quoteUnits == null)
            {
                properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            }
            var algorithm = properties.GetString("Algorithm", null);
            if (algorithm == null)
            {
                properties.Set("Algorithm", "Default");
            }
            var referenceDiscountCurve = properties.GetString("ReferenceCurveUniqueId", true);
            var referenceForecastCurve = properties.GetString("ReferenceCurrency2CurveId", referenceDiscountCurve);
            var discountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, referenceDiscountCurve);
            var forecastCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, referenceForecastCurve);
            // Create the engines and either add to, or overwrite the existing, collection
            if (_capFloorEngine.ContainsKey(id))
                _capFloorEngine[id] = CreateCurves(logger, cache, nameSpace, properties, discountCurve, forecastCurve, instruments, rawVolatilityGrid);
            else
                _capFloorEngine.Add(id, CreateCurves(logger, cache, nameSpace, properties, discountCurve, forecastCurve, instruments, rawVolatilityGrid));
            return id;
        }

        /// <summary>
        /// Compute a Caplet Volatility using a bootstrapped engine and the set of identifying parameters
        /// </summary>
        /// <param name="engineHandle">The engine handle to the engines group to use</param>
        /// <param name="strike">The strike at which to compute the volatility</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="target">The target date to compute the volatility at</param>
        /// <returns></returns>
        public decimal BootstrapComputeCapletVolatility(string engineHandle, decimal strike, DateTime baseDate, DateTime target)
        {
            return BootstrapComputeCapletVolatility(engineHandle, strike, baseDate, new[] {target}).Single();
        }

        /// <summary>
        /// Compute a Caplet Volatility using a bootstrapped engine and the set of identifying parameters
        /// </summary>
        /// <param name="engineHandle">The engine handle to the engines group to use</param>
        /// <param name="strike">The strike at which to compute the volatility</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targets">The target dates to compute the volatilities at</param>
        /// <returns></returns>
        public IList<decimal> BootstrapComputeCapletVolatility(string engineHandle, decimal strike, DateTime baseDate, DateTime[] targets)
        {
            if (!_capFloorEngine.ContainsKey(engineHandle))
            {
                throw new ArgumentException(
                    $"The engine: {engineHandle} is not present. The volatility cannot be computed.");
            }
            //// Set up the parameters to use for this Computation
            var engines = _capFloorEngine[engineHandle];
            // Isolate the correct engine from the engine group - fixed strike case
            if (!engines.ContainsKey(strike))
            {
                throw new ArgumentException($"The strike value: {strike} is not valid for this CapletBootstrapper.");
            }
            var result = new List<Decimal>();
            foreach (var date in targets)
            {
                var time = new DateTimePoint1D(baseDate, date);
                var value = (decimal)engines[strike].GetValue(time.GetX());
                result.Add(value);
            }
            return result;
        }

        #endregion

        #region SABR CapFloor Calibration

        /// <summary>
        /// Create a settings object to use with SABR Calibration
        /// </summary>
        /// <param name="settingsHandle">The settings engine</param>
        /// <param name="beta">The beta value</param>
        /// <param name="interpolationType">Interpolation type to use</param>
        /// <returns></returns>
        public string SABRCapFloorCalibrationSettings(string settingsHandle, decimal beta, string interpolationType)
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
            var settings = new CapletSmileCalibrationSettings(beta, volatilityInterpolation, settingsHandle);

            if (_sabrCapFloorSettings.ContainsKey(settingsHandle))
                _sabrCapFloorSettings[settingsHandle] = settings;
            else
                _sabrCapFloorSettings.Add(settingsHandle, settings);
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
        public string SABRCapFloorCalibrationEngine(string engineHandle, string settingsHandle, string bootstrapFixedStrikeHandle, string bootstrapATMHandle)
        {
            // The ATM CapFloor bootstrap engine is the 0th strike CapFloor engine with this handle
            CapVolatilityCurve atmEngine;
            if (_capFloorEngine.ContainsKey(bootstrapATMHandle))
                atmEngine = _capFloorEngine[bootstrapATMHandle][0];
            else
                throw new ArgumentException("ATM CapFloor Bootstrap engine not found.");
            SortedList<decimal, CapVolatilityCurve> fixedStrikeEngines;
            if (_capFloorEngine.ContainsKey(bootstrapFixedStrikeHandle))
                fixedStrikeEngines = _capFloorEngine[bootstrapFixedStrikeHandle];
            else
                throw new ArgumentException("Fixed Strike Bootstrap engines not found.");
            CapletSmileCalibrationSettings settings;
            if (_sabrCapFloorSettings.ContainsKey(settingsHandle))
                settings = _sabrCapFloorSettings[settingsHandle];
            else
                throw new ArgumentException("Caplet Smile Settings not found.");
            var engine = new CapletSmileCalibrationEngine(atmEngine, settings, new List<CapVolatilityCurve>(fixedStrikeEngines.Values), engineHandle);
            if (_sabrCapFloorEngines.ContainsKey(engineHandle))
                _sabrCapFloorEngines[engineHandle] = engine;
            else
                _sabrCapFloorEngines.Add(engineHandle, engine);
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
        public List<Decimal> SABRCapFloorComputeVolatility(ILogger logger, ICoreCache cache, string nameSpace, string engineHandle, DateTime date, decimal[] strikes)
        {
            CapletSmileCalibrationEngine engine;
            if (_sabrCapFloorEngines.ContainsKey(engineHandle))
                engine = _sabrCapFloorEngines[engineHandle];
            else
                throw new ArgumentException("SABR Caplet Smile Calibration engine not found.");
            var strikesList = new List<decimal>(strikes);
            var computedValues = engine.ComputeCapletVolatilitySmile(logger, cache, nameSpace, date, strikesList);
            return computedValues;
        }

        #endregion

        #region CapFloor Engine Utility Methods

        /// <summary>
        /// List all existing bootstrap engines
        /// This method will list any created using Bootstrap methods
        /// and will check for and generate any engines retrieved by subscription
        /// </summary>
        /// <returns></returns>
        public String[] ListCapletBootstrapEngines()
        {
            if (_capFloorEngine == null || _capFloorEngine.Keys.Count == 0)
            {
                return new [] {"No SABR Volatility Surfaces located."};
            }
            return _capFloorEngine.Keys.ToArray();
        }

        #endregion

        #endregion

        #region Private Methods for backward compatability

        /// <summary>
        /// Create a Sorted list of bootstrap engines from the data matrix
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties, including the engine handle.</param>
        /// <param name="forecastCurve">The forecast rate curve.</param>
        /// <param name="discountCurve">The discount rate curve.</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns></returns>
        private SortedList<decimal, CapVolatilityCurve> CreateCurves(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, IRateCurve discountCurve, IRateCurve forecastCurve, String[] instruments,
            Decimal[] rawVolatilityGrid)
        {
            var clonedProperties = properties.Clone();
            // Create engine
            var quotedAssetSet = AssetHelper.Parse(instruments, rawVolatilityGrid, null);
            var engines = new SortedList<decimal, CapVolatilityCurve>();
            // Create a new ATM CapletBootstrap engine. The default decimal should be 0
            var engine = new CapVolatilityCurve(logger, cache, nameSpace, clonedProperties, quotedAssetSet, discountCurve, forecastCurve, null, null);
            // Add engine
            engines.Add(0, engine);
            return engines;
        }

        /// <summary>
        /// Create a Sorted list of bootstrap engines from the data matrix
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties, including the engine handle.</param>
        /// <param name="forecastCurve">The forecast rate curve.</param>
        /// <param name="discountCurve">The discount rate curve.</param>
        /// <param name="instruments">An array of instrument types.</param>
        /// <param name="strikes">The strike array.</param>
        /// <param name="rawVolatilityGrid">The raw grid used to build the engines. Assume that all volatility and strike values are 100x true</param>
        /// <returns></returns>
        private SortedList<decimal, CapVolatilityCurve> CreateCurves(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, IRateCurve discountCurve, IRateCurve forecastCurve, String[] instruments, 
            Decimal[] strikes, Decimal[,] rawVolatilityGrid)
        {
            var clonedProperties = properties.Clone();           
            // Check there are valid strikes
            if (strikes != null && rawVolatilityGrid != null)
            {
                //The matrix is zero based, but the upper bound returns the last column index.
                var volColumns = rawVolatilityGrid.GetUpperBound(1) + 1;
                var columns = Math.Min(strikes.Length, volColumns);           
                var engines = new SortedList<decimal, CapVolatilityCurve>();
                // Loop through the strikes to create each new engine
                for(var i = 0; i < columns; i++)
                {
                    decimal[] adjustedRates = GenerateVolatilityMatrix(instruments, i, rawVolatilityGrid);
                    clonedProperties.Set("Strike", strikes[i]);
                    clonedProperties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.DecimalRate.ToString());
                    // Create engine
                    var quotedAssetSet = AssetHelper.Parse(instruments, adjustedRates, null);
                    var engine = new CapVolatilityCurve(logger, cache, nameSpace, clonedProperties, quotedAssetSet, discountCurve, forecastCurve, null, null);
                    // Add engine
                    engines.Add(strikes[i], engine);
                }
                return engines;
            }
            {
                var engines = new SortedList<decimal, CapVolatilityCurve>();
                // Create a new ATM CapletBootstrap engine. The default decimal should be 0
                var engine = PricingStructureFactory.CreateCurve(logger, cache, nameSpace, null, null, clonedProperties,
                    instruments, null, null) as CapVolatilityCurve;
                // Add engine
                engines.Add(0, engine);
                return engines;
            }
        }

        #endregion

        #region Utility methods

        private string CheckVolatilities(Decimal[] rawVolatilityGrid)
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
        /// <param name="column">The volatility to extract.</param>
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