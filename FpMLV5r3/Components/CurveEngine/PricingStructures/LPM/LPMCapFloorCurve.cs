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
using System.Text.RegularExpressions;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.PricingStructures.Helpers;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Constants;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Highlander.CurveEngine.V5r3.Assets.Helpers;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.NamedValues;
using VolatilitySurface = Highlander.CurveEngine.V5r3.PricingStructures.Surfaces.VolatilitySurface;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.LPM
{

    /// <summary>
    /// LPMCapFloorCurve
    /// </summary>
    public static class LPMCapFloorCurve
    {
        private const string ExpiryKeys = "0D,2D,1M,2M,3M,4M,5M,6M,9M,12M,15M,18M,21M,24M,27M,30M,33M,36M,39M,42M,45M,48M,51M,54M,57M,60M,63M,66M,69M,72M,75M,78M,81M,84M,87M,90M,93M,96M,99M,102M,105M,108M,111M,114M,117M,120M,123M,126M,129M,132M,135M,138M,141M,144M,147M,150M,153M,156M,159M,162M,165M,168M,171M,174M,177M,180M,183M,186M,189M,192M,195M,198M,201M,204M,207M,210M,213M,216M,219M,222M,225M,228M,231M,234M,237M,240M,243M,246M,249M,252M,255M,258M,261M,264M,267M,270M,273M,276M,279M,282M,285M,288M,291M,294M,297M,300M,303M,306M,309M,312M,315M,318M,321M,324M,327M,330M,333M,336M,339M,342M,345M,348M,351M,354M,357M,360M";

        #region Static Public Mathods

        /// <summary>
        /// Process a capfloor curve. If it is an ATM curve bootstrap and publish
        /// If it is a PPD curve convert to ATM and process as an ATM curve
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="cache">The cache.</param>
        /// <param name="rateCurve"></param>
        /// <param name="capFloor"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static Market ProcessCapFloor(ILogger logger, ICoreCache cache, string nameSpace, Market rateCurve, CapFloorATMMatrix capFloor)
        {
            return capFloor.MatrixType == "ATM" ? ProcessCapFloorATM(logger, cache, nameSpace, rateCurve, capFloor) : ProcessCapFloorPpd(logger, cache, nameSpace, rateCurve, capFloor);
        }

        ///<summary>
        ///</summary>
        ///<param name="offsetAsInt"></param>
        ///<returns></returns>
        private static double IntToDouble(int offsetAsInt)
        {
            return Convert.ToDouble(offsetAsInt);
        }

        #endregion

        #region Private Processing Mathods

        /// <summary>
        /// Convert the cap/floor PPD values to an ATM parVols structure
        /// This method then calls the bootstrapper <see cref="ProcessCapFloorPpd"/>
        /// to generate the cap vol curve
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="cache">The cache.</param>
        /// <param name="rateCurve"></param>
        /// <param name="capFloor"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        private static Market ProcessCapFloorPpd(ILogger logger, ICoreCache cache, string nameSpace, Market rateCurve, CapFloorATMMatrix capFloor)
        {
            var expiry = capFloor.GetExpiries();
            var vols = capFloor.GetVolatilities();
            var mkt = rateCurve;
            var curve = new SimpleRateCurve(mkt);
            var rawOffsets = curve.GetDiscountFactorOffsets();
            var offsets = Array.ConvertAll(rawOffsets, IntToDouble);
            var discountFactors = curve.GetDiscountFactors();
            var volType = capFloor.GetVolatilityTypes();
            var atmVols = new Dictionary<string, decimal>();
            var settings = CreateCapFloorProperties(capFloor.GetVolatilitySettings());
            var bc = BusinessCenterHelper.ToBusinessCalendar(cache, new[] { "AUSY" }, nameSpace);
            // Use some logic to get the spot date to use
            // Step through each vol and convert ppd to ATM vol
            for (var i = 0; i < expiry.Length; i++)
            {
                // Create a Swap rate for each expiry
                // Assume frequency = 4 months until 4 years tenor is reached
                Period tv = PeriodHelper.Parse(expiry[i]);
                //double tvYearFraction = tv.ToYearFraction();
                //int frequency = tvYearFraction < 4 ? 4 : 2;
                const int frequency = 4;
                var rates = new SwapRate(logger, cache, nameSpace, "AUSY", curve.GetBaseDate(), "ACT/365.FIXED", discountFactors, curve.GetDiscountFactorOffsets(), frequency, BusinessDayConventionEnum.MODFOLLOWING);
                switch (volType[i])
                {
                    case "ETO":
                        {
                            DateTime spotDate = settings.GetValue("Calculation Date", DateTime.MinValue);
                            var rollConvention = settings.GetValue("RollConvention", BusinessDayConventionEnum.MODFOLLOWING);
                            DateTime etoDate = bc.Roll(tv.Add(spotDate), rollConvention);
                            atmVols[expiry[i]] = CalculateATMVolatility(settings, spotDate, etoDate, offsets, discountFactors, vols[i][0]);
                        }
                        break;
                    case "Cap/Floor":
                        {
                            DateTime spotDate = settings.GetValue("Calculation Date", DateTime.MinValue);
                            var rollConvention = settings.GetValue("RollConvention", BusinessDayConventionEnum.MODFOLLOWING);
                            DateTime expiryDate = bc.Roll(tv.Add(spotDate), rollConvention);
                            string tenor = DateToTenor(spotDate, expiryDate);
                            double tenorYearFraction = PeriodHelper.Parse(tenor).ToYearFraction();
                            // Add the caplet maturity to the expiry and then calculate the vol
                            atmVols[expiry[i]] = CalculateCAPATMVolatility(rates, spotDate, tenorYearFraction, vols[i][0]);
                        }
                        break;
                }
            }
            // Fudge to switch the PPD header to ATM
            // We've converted so we want the new name
            var headers = capFloor.GetHeaders();
            for (var i = 0; i < headers.Length; i++)
            {
                if (!headers[i].Contains("PPD")) continue;
                headers[i] = "ATM";
                break;
            }
            // Convert our lovely dictionary to a grubby array of arrays
            var volatilities = new object[atmVols.Keys.Count][];
            var row = 0;
            foreach (var key in atmVols.Keys)
            {
                volatilities[row] = new object[3];
                volatilities[row][0] = key;
                volatilities[row][1] = atmVols[key];
                volatilities[row][2] = volType[row++];
            }
            var convertedCapFloor = new CapFloorATMMatrix(headers, volatilities, capFloor.GetVolatilitySettings(),
                                                          capFloor.baseDate.Value, capFloor.id);
            return ProcessCapFloorATM(logger, cache, nameSpace, rateCurve, convertedCapFloor);
        }

        /// <summary>
        /// Process a CapFloor ATM par Vols structure.
        /// The process Bootstraps the parVols using the supplied ratecurve
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace"></param>
        /// <param name="rateCurve"></param>
        /// <param name="capFloor"></param>
        /// <returns></returns>
        private static Market ProcessCapFloorATM(ILogger logger, ICoreCache cache, string nameSpace, Market rateCurve, CapFloorATMMatrix capFloor)
        {
            var id = capFloor.id;
            var expiry = capFloor.GetExpiries();
            var vols = capFloor.GetVolatilities();
            var mkt = rateCurve;
            var curve = new SimpleRateCurve(mkt);
            var discountFactorDates = curve.GetDiscountFactorDates();
            var discountFactors = curve.GetDiscountFactors();
            var volType = capFloor.GetVolatilityTypes();
            var atmVols = new Dictionary<string, decimal>();
            var settings = CreateCapFloorProperties(capFloor.GetVolatilitySettings());
            // Create an internal matrix object from the raw data
            var matrix = new CapFloorVolatilityMatrix(id, expiry, volType, vols, null,
                                                      discountFactorDates, ArrayUtilities.ArrayToDecimal(discountFactors));
            // Create an ATM engine from the matrix
            var engines = CreateEngines(logger, cache, nameSpace, id, matrix, settings);
            // Add the default interpolation to use
            const ExpiryInterpolationType volatilityInterpolation = ExpiryInterpolationType.Linear;
            if (engines != null)
            {
                var vol = new CapletExpiryInterpolatedVolatility(engines[0], volatilityInterpolation);
                // List the values so we can build our ATM vols
                var keys = ExpiryKeys.Split(',');
                // Create a calendar to use to modify the date
                var businessCalendar = settings.GetValue("BusinessCalendar", "AUSY");
                var bc = BusinessCenterHelper.ToBusinessCalendar(cache, new[] { businessCalendar }, nameSpace);
                // Use some logic to get the spot date to use
                // LPM Spot lag is 2 days (mod following)
                var spotDate = curve.GetSpotDate();
                var rollConvention = settings.GetValue("RollConvention", BusinessDayConventionEnum.FOLLOWING);
                spotDate = spotDate == curve.GetBaseDate() ? bc.Roll(spotDate.Add(new TimeSpan(2, 0, 0, 0)), rollConvention) : spotDate;
                //// Extract each surface and build an ATM engine therefrom
                //// Build a list of all possible engines
                foreach (var key in keys)
                {
                    // Calculate the volatility for each target key
                    var tv = PeriodHelper.Parse(key);
                    var target = tv.period == PeriodEnum.D ? bc.Roll(spotDate.AddDays(Convert.ToInt32(tv.periodMultiplier)), rollConvention) : 
                        bc.Roll(spotDate.AddMonths(Convert.ToInt32(tv.periodMultiplier)), rollConvention);
                    atmVols.Add(key, vol.ComputeCapletVolatility(target));
                }
            }
            var outputVols = new object[atmVols.Count + 1, 2];
            var i = 1;
            //Expiry	0
            outputVols[0, 0] = "Expiry";
            outputVols[0, 1] = "0";
            foreach (var key in atmVols.Keys)
            {
                outputVols[i, 0] = key;
                outputVols[i, 1] = atmVols[key];
                i++;
            }
            DateTime buildDateTime = rateCurve.Items1[0].buildDateTime;
            var volSurface = new VolatilitySurface(outputVols, new VolatilitySurfaceIdentifier(id), curve.BaseDate, buildDateTime);
            return CreateMarketDocument(volSurface.GetFpMLData());
        }

        #endregion

        #region Build Engines

        /// <summary>
        /// Create a Sorted list of bootstrap engines from the data matrix
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace"></param>
        /// <param name="engineHandle">The engine collection name</param>
        /// <param name="matrix">The internal data structure used in conversion</param>
        /// <param name="properties">The properties object to use</param>
        /// <returns></returns>
        private static SortedList<decimal, CapVolatilityCurve> CreateEngines(ILogger logger, ICoreCache cache,
            string nameSpace, string engineHandle, CapFloorVolatilityMatrix matrix, NamedValueSet properties)
        {
            var engines = new SortedList<decimal, CapVolatilityCurve>();
            //  Check there is a valid settings object
            if (properties == null)
                return null;
            var currency = properties.GetString("Currency", true);
            var baseDate = properties.GetValue("Calculation Date", DateTime.MinValue);
            properties.Set("EngineHandle", engineHandle);
            matrix.ValuationDate = baseDate;
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
            var dfs = matrix.DiscountFactorsAsDoubles();
            // Check there are valid strikes
            IRateCurve discountCurve =
                new SimpleDiscountFactorCurve(baseDate, interp, true, dfs);
            //TODO
            //The quoted asset set
            var volTypes = matrix.CapVolatilities();
            var instruments = new List<string>();
            var volatilities = new List<decimal>();
            foreach(var volType in volTypes)
            {
                string tempName;
                string tenor;
                var type = volType.VolatilityType;
                if (type == VolatilityDataType.ETO)
                {
                    tempName = currency + "-Caplet-";
                    tenor = volType.Expiry + "D-90D";
                }
                else
                {
                    tempName = currency + "-IRCap-";
                    tenor = volType.Expiry + "Y";
                }               
                instruments.Add(tempName + tenor);
                volatilities.Add(volType.Volatility);
            }
            var qas = AssetHelper.Parse(instruments.ToArray(), volatilities.ToArray());
            //The volatilities
            // Create a new ATM CapletBootstrap engine. The default decimal should be 0
            var engine = new CapVolatilityCurve(logger, cache, nameSpace, properties, qas, discountCurve, discountCurve,
                                                   null, null);
            // Add engine
            engines.Add(0, engine);
            return engines;
        }

        /// <summary>
        /// Create a settings object from the supplied settings object
        /// Settings Names::
        ///     Calculation Date
        ///     Cap Frequency
        ///     Cap Start Lag
        ///     Currency
        ///     Handle
        ///     Par Volatility Interpolation (Optional)
        ///     Roll Convention (Optional)
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static NamedValueSet CreateCapFloorProperties(object[][] settings)
        {
            var settingsHandle = Convert.ToString(FindValueFromName(settings, "Handle"));
            var calculationDate = Convert.ToDateTime(FindValueFromName(settings, "Calculation Date"));
            var baseDate = calculationDate;//For backward compatibility
            var capFrequency = CalculateCapFrequency(Convert.ToString(FindValueFromName(settings, "Cap Frequency")));
            var capStartLag = Convert.ToInt32(FindValueFromName(settings, "Cap Start Lag"));
            var currency = Convert.ToString(FindValueFromName(settings, "Currency"));
            var instrument = Convert.ToString(FindValueFromName(settings, "Instrument"));
            var indexTenor = Convert.ToString(FindValueFromName(settings, "IndexTenor"));
            var interp = Convert.ToString(FindValueFromName(settings, "Par Volatility Interpolation"));
            var parVolatilityInterpolation = ParVolatilityInterpolationType.CubicHermiteSpline;
            if (!string.IsNullOrEmpty(interp))
            {
                var found = false;
                var interps = Enum.GetValues(typeof(ParVolatilityInterpolationType));
                foreach (ParVolatilityInterpolationType interpType in interps)
                {
                    if (interpType.ToString() != interp) continue;
                    parVolatilityInterpolation = interpType;
                    found = true;
                    break;
                }
                if (!found)
                    throw new ArgumentException($"Unknown interpolation method {interp} specified.");
            }
            string roll = Convert.ToString(FindValueFromName(settings, "Roll Convention"));
            BusinessDayConventionEnum rollConvention;
            if (!string.IsNullOrEmpty(roll))
            {
                if (!EnumHelper.TryParse(roll, true, out rollConvention))
                {
                    throw new ArgumentException($"Unknown roll convention {roll} specified.");
                }
            }
            else
            {
                rollConvention = BusinessDayConventionEnum.MODFOLLOWING;
            }
            // Create a settings object
            var settingsObj = new NamedValueSet();
            settingsObj.Set("Handle", settingsHandle);
            settingsObj.Set("Calculation Date", calculationDate);
            settingsObj.Set("BaseDate", baseDate);
            settingsObj.Set("Cap Frequency", capFrequency);
            settingsObj.Set("Cap Start Lag", capStartLag);
            settingsObj.Set("Currency", currency);
            settingsObj.Set("Par Volatility Interpolation", parVolatilityInterpolation);
            settingsObj.Set("Roll Convention", rollConvention);
            settingsObj.Set("Instrument", instrument);
            settingsObj.Set("IndexTenor", indexTenor);
            var dayCount = InitialiseDayCount(settingsObj);
            settingsObj.Set("DayCount", dayCount);
            return settingsObj;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper function used by the master initialisation function to
        /// initialise the Day Count.
        /// Precondition: InitialiseBusinessCalendarAndCurrency method has
        /// been called.
        /// </summary>
        private static string InitialiseDayCount(NamedValueSet properties)
        {
            // Load the day count converter.
            var dayCountConverter =
                new SortedList<string, string>
                {
                    {"AUD", "ACT/365.FIXED"},
                    {"USD", "ACT/360"},
                    {"GBP", "ACT/365.FIXED"},
                    {"EUR", "ACT/360"},
                    {"NZD", "ACT/365.FIXED"}
                };

            var currency = properties.GetValue("Currency", "AUD");
            var dayCount = dayCountConverter[currency];
            return dayCount;
        }

        /// <summary>
        /// Given a name search for a value
        /// </summary>
        /// <param name="table">The source grid holding name/value pairs</param>
        /// <param name="search">The search term</param>
        /// <returns>A matched value or null if no match</returns>
        private static object FindValueFromName(object[][] table, string search)
        {
            object found = null;
            search = search.Replace(" ", "");
            // Get the number of rows to search through
            var maxRows = table.Length;
            for (var row = 0; row < maxRows; row++)
            {
                if (table[row][0].ToString().Replace(" ", "") != search) continue;
                found = table[row][1];
                break;
            }
            return found;
        }

        /// <summary>
        /// Create a Market to wrap the Pricing Structure/Valuation data
        /// </summary>
        /// <param name="marketData"></param>
        /// <returns></returns>
        private static Market CreateMarketDocument(Pair<PricingStructure, PricingStructureValuation> marketData)
        {
            var market = new Market
                             {
                                 id = "Market - " + marketData.First.id,
                                 Items = new[] {marketData.First},
                                 Items1 = new[] {marketData.Second}
                             };

            return market;
        }

        /// <summary>
        /// Calculate the CapFrequency to use from the term structure expressed as a string
        /// </summary>
        /// <param name="frequency">The term structure to convert</param>
        /// <returns>The capFrequency equivalent of the term</returns>
        private static CapFrequency CalculateCapFrequency(string frequency)
        {
            CapFrequency capFrequency;

            // Process the term string to give us the frequency
            decimal multiplier = 0;
            var period = "";
            LabelSplitter(frequency, ref period, ref multiplier);

            // Try to match the term structure to an allowable frequency
            switch (period)
            {
                case "M":
                    switch ((int)multiplier)
                    {
                        case 1:
                            capFrequency = CapFrequency.Monthly;
                            break;
                        case 3:
                            capFrequency = CapFrequency.Quarterly;
                            break;
                        case 6:
                            capFrequency = CapFrequency.SemiAnnually;
                            break;
                        default:
                            throw new ArgumentException("Cap Frequency not a valid value.");
                    }
                    break;
                case "Y":
                    capFrequency = CapFrequency.Yearly;
                    break;
                default:
                    throw new ArgumentException("Cap Frequency not a valid value.");
            }
            return capFrequency;
        }

        /// <summary>
        /// Method to split and store the alpha and numeric parts of a label
        /// This method should be private to the element but is defined here for convenience
        /// </summary>
        /// <param name="label">The source label to split</param>
        /// <param name="alpha">The alpha part of the label</param>
        /// <param name="numeric">The number part of the label</param>
        private static void LabelSplitter(string label, ref string alpha, ref decimal numeric)
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
            alpha = alphaMatches.Count > 0 ? alphaMatches[0].Value.Substring(0, 1).ToUpper() : "D";
            numeric = numericMatches.Count > 0 ? Convert.ToDecimal(numericMatches[0].Value) : 0;
        }

        /// <summary>
        /// Calculate an ATM volatility given a Price Per Day (PPD) value and a swap curve
        /// The curve provides forward rates for the calculation
        /// </summary>
        /// <param name="settings">The settings object used to build the fwd rate</param>
        /// <param name="spotDate">The spot date used to calculate fwd rate from</param>
        /// <param name="etoDate">The calculation date to calculate fwd rate to</param>
        /// <param name="offsets">The offset for each discount factor</param>
        /// <param name="df">The discount factor at each offset</param>
        /// <param name="ppd">The ppd indicative vol</param>
        /// <returns></returns>
        private static decimal CalculateATMVolatility(NamedValueSet settings, DateTime spotDate, DateTime etoDate, double[] offsets, double[] df, decimal ppd)
        {
            // Extract the forward rate
            decimal rate = LPMCapFloorHelper.ComputeForwardRate(settings, offsets, df, spotDate, etoDate);
            ppd /= 100.0m;
            // Calculate the volatility from the parameters
            var atmVolatility = ppd * (decimal)System.Math.Sqrt(250.0) / rate;
            return atmVolatility;
        }

        /// <summary>
        /// Calculate an ATM volatility given a Price Per Day (PPD) value and a swap curve
        /// The curve provides forward rates for the calculation
        /// </summary>
        /// <returns></returns>
        private static decimal CalculateCAPATMVolatility(SwapRate rates, DateTime spotDate, double tenorYearFraction, decimal ppd)
        {
            // Extract the forward rate
            var swapRate = rates.ComputeSwapRate(spotDate, tenorYearFraction);
            ppd /= 100.0m;
            // Calculate the volatility from the parameters
            var atmVolatility = ppd * (decimal)System.Math.Sqrt(250.0) / swapRate;
            return atmVolatility;
        }

        /// <summary>
        /// A method to convert a date to a month based tenor (nearest)
        /// </summary>
        /// <param name="spot"></param>
        /// <param name="date">The date</param>
        /// <returns>A string representation of a month based tenor</returns>
        private static string DateToTenor(DateTime spot, DateTime date)
        {
            var months = date.Subtract(spot).Days/30;
            return $"{months}M";
        }

        #endregion
    }
}