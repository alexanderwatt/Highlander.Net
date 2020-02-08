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
using System.Linq;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.NamedValues;

namespace Highlander.Reporting.Identifiers.V5r3
{
    /// <summary>
    /// Generic property helper class.
    /// </summary>
    public static class PropertyHelper
    {
        /// <summary>
        /// Adds the message properties.
        /// </summary>
        /// <param name="baseProperties">The properties to add.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">the value</param>
        public static void Update(NamedValueSet baseProperties, string name, object value)
        {
            if (baseProperties.ToDictionary().ContainsKey(name))
                baseProperties.ToDictionary()[name] = value;
            else
            {
                baseProperties.Set(name, value);
            }
        }

        /// <summary>
        /// Adds the message properties.
        /// </summary>
        /// <param name="baseProperties">The properties to add.</param>
        /// <param name="addProperties">The message properties.</param>
        public static void Update(NamedValueSet baseProperties, IDictionary<string, object> addProperties)
        {
            foreach (var keyName in addProperties.Keys)
            {
                addProperties.TryGetValue(keyName, out var value);
                if (baseProperties.ToDictionary().ContainsKey(keyName))
                    baseProperties.ToDictionary()[keyName] = value;
                else
                {
                    baseProperties.Set(keyName, value);
                }
            }
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="propertyName">THe property name.</param>
        /// <param name="properties">The collection of properties.</param>
        public static DateTime? ExtractDateTimeProperty(string propertyName, NamedValueSet properties)
        {
            DateTime? date = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey(propertyName))
            {
                date = properties.Get(propertyName).AsValue<DateTime>();
            }
            return date;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="propertyName">THe property name.</param>
        /// <param name="properties">The collection of properties.</param>
        public static int? ExtractIntegerProperty(string propertyName, NamedValueSet properties)
        {
            int? result = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey(propertyName))
            {
                result = properties.Get(propertyName).AsValue<int>();
            }
            return result;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="propertyName">THe property name.</param>
        /// <param name="properties">The collection of properties.</param>
        public static double? ExtractDoubleProperty(string propertyName, NamedValueSet properties)
        {
            double? result = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey(propertyName))
            {
                result = properties.Get(propertyName).AsValue<double>();
            }
            return result;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="propertyName">THe property name.</param>
        /// <param name="properties">The collection of properties.</param>
        public static decimal? ExtractDecimalProperty(string propertyName, NamedValueSet properties)
        {
            decimal? result = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey(propertyName))
            {
                result = properties.Get(propertyName).AsValue<decimal>();
            }
            return result;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="propertyName">THe property name.</param>
        /// <param name="properties">The collection of properties.</param>
        public static bool? ExtractBooleanProperty(string propertyName, NamedValueSet properties)
        {
            bool? boolean = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey(propertyName))
            {
                boolean = properties.Get(propertyName).AsValue<bool>();
            }
            return boolean;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="propertyName">THe property name.</param>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractStringProperty(string propertyName, NamedValueSet properties)
        {
            const string error = "Unknown Property.";
            var dictionaryKeys = properties.ToDictionary();
            return dictionaryKeys.ContainsKey(propertyName) ? properties.Get(propertyName).AsValue<string>() : error;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractMarketAndDate(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.TryGetValue(CurveProp.MarketAndDate, out var value))
            {
                return value.ToString();
            }
            var marketName = ExtractMarket(properties);
            var marketDate = ExtractMarketDate(properties);
            if (marketDate != null)
            {
                return $"{marketName}.{((DateTime) marketDate):yyyy-MM-dd}";
            }
            return marketName;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractAssetIdentifier(NamedValueSet properties)
        {
            const string assetRef = "Unspecified";
            var dictionaryKeys = properties.ToDictionary();
            return dictionaryKeys.ContainsKey("AssetIdentifier") ? properties.Get("AssetIdentifier").AsValue<string>() : assetRef;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractAssetType(NamedValueSet properties)
        {
            const string assetRef = "Unspecified";
            var dictionaryKeys = properties.ToDictionary();
            return dictionaryKeys.ContainsKey("AssetType") ? properties.Get("AssetType").AsValue<string>() : assetRef;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractAssetRef(NamedValueSet properties)
        {
            const string assetRef = "Unspecified";
            var dictionaryKeys = properties.ToDictionary();
            return dictionaryKeys.ContainsKey("AssetRef") ? properties.Get("AssetRef").AsValue<string>() : assetRef;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractMarket(NamedValueSet properties)
        {
            string market = properties.GetValue(CurveProp.Market, "");
            if (string.IsNullOrEmpty(market))
            {
                market = properties.GetValue(CurveProp.MarketAndDate, "Unspecified");
            }
            return market;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static DateTime? ExtractMarketDate(NamedValueSet properties)
        {
            return properties.GetNullable<DateTime>(CurveProp.MarketDate);
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractSourceSystem(NamedValueSet properties)
        {
            const string sourceSystem = "Highlander";
            var dictionaryKeys = properties.ToDictionary();
            return dictionaryKeys.ContainsKey("SourceSystem") ? properties.Get("SourceSystem").AsValue<string>() : sourceSystem;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractDataType(NamedValueSet properties)
        {
            const string dataType = "Market";
            var dictionaryKeys = properties.ToDictionary();
            return dictionaryKeys.ContainsKey("DataType") ? properties.Get("DataType").AsValue<string>() : dataType;
        }

        /// <summary>
        /// A helper to extract properties from a named value set..
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractUniqueCurveIdentifier(NamedValueSet properties)
        {
            var domain = ExtractDomain(properties);
            var market = ExtractMarketAndDate(properties);
            var pst = ExtractPricingStructureType(properties);
            var curveName = ExtractCurveName(properties);
            var identifier = domain + '.' + market + '.' + pst + '.' + curveName;
            var dictionaryKeys = properties.ToDictionary();
            return dictionaryKeys.ContainsKey(CurveProp.UniqueIdentifier) ? properties.Get(CurveProp.UniqueIdentifier).AsValue<string>() : identifier;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractDomain(NamedValueSet properties)
        {
            var sourceSystem = ExtractSourceSystem(properties);
            var dataType = ExtractDataType(properties);
            return sourceSystem + '.' + dataType;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static Decimal? ExtractStrike(NamedValueSet properties)
        {
            Decimal? strike = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("Strike"))
            {
                strike = properties.Get("Strike").AsValue<Decimal>();
            }
            return strike;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static DateTime? ExtractExpiryTime(NamedValueSet properties)
        {
            DateTime? expiryTime = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("ExpiryTime"))
            {
                expiryTime = properties.Get("ExpiryTime").AsValue<DateTime>();
            }
            return expiryTime;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static DateTime? ExtractTime(NamedValueSet properties)
        {
            DateTime? time = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("Time"))
            {
                time = properties.Get("Time").AsValue<DateTime>();
            }
            return time;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static DateTime? ExtractValuationDate(NamedValueSet properties)
        {
            DateTime? valuationDate = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("ValuationDate"))
            {
                valuationDate = properties.Get("ValuationDate").AsValue<DateTime>();
            }
            return valuationDate;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static QuoteTiming ExtractQuoteTiming(NamedValueSet properties)
        {
            QuoteTiming quoteTiming = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("QuoteTiming"))
            {
                var result = properties.Get("QuoteTiming").AsValue<string>();
                quoteTiming = new QuoteTiming { Value = result };
            }
            return quoteTiming;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static Boolean ExtractBootStrapOverrideFlag(NamedValueSet properties)
        {
            bool flag = false;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("BootStrap"))
            {
                flag = properties.Get("BootStrap").AsValue<bool>();
            }
            return flag;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static Boolean ExtractOptimizeBuildFlag(NamedValueSet properties)
        {
            bool flag = true;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("OptimizeBuild"))
            {
                flag = properties.Get("OptimizeBuild").AsValue<bool>();
            }
            return flag;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static AssetMeasureType ExtractMeasureType(NamedValueSet properties)
        {
            AssetMeasureType measureType = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("MeasureType"))
            {
                var result = properties.Get("MeasureType").AsValue<string>();
                measureType = new AssetMeasureType { Value = result };
            }
            return measureType;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static PriceQuoteUnits ExtractQuoteUnits(NamedValueSet properties)
        {
            string value = properties.GetString("QuoteUnits", false);
            var priceQuoteUnitsEnum = PriceQuoteUnitsEnum.DecimalRate;
            if (!string.IsNullOrEmpty(value))
            {
                if (!PriceQuoteUnitsScheme.TryParseEnumString(value, out priceQuoteUnitsEnum))
                    throw new InvalidCastException($"Cannot cast {value} to PriceQuoteUnitsEnum");
            }
            return new PriceQuoteUnits { Value = PriceQuoteUnitsScheme.GetEnumString(priceQuoteUnitsEnum) };
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static PriceQuoteUnits ExtractStrikeQuoteUnits(NamedValueSet properties)
        {
            string value = properties.GetValue("StrikeQuoteUnits", "ATMFlatMoneyness");
            return new PriceQuoteUnits { Value = value };
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractCapFrequency(NamedValueSet properties)
        {
            // 3M by default
            string value = properties.GetString("CapFrequency", "3M");
            return value;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static CashflowType ExtractCashflowType(NamedValueSet properties)
        {
            CashflowType cashflowType = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("CashflowType"))
            {
                var result = properties.Get("CashflowType").AsValue<string>();
                cashflowType = new CashflowType { Value = result };
            }
            return cashflowType;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static QuotationSideEnum? ExtractQuotationSide(NamedValueSet properties)
        {
            QuotationSideEnum? quotationSideEnum = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("QuotationSide"))
            {
                var result = properties.Get("QuotationSide").AsValue<string>();
                quotationSideEnum = (QuotationSideEnum)Enum.Parse(typeof(QuotationSideEnum), result, true);
            }
            return quotationSideEnum;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static InformationSource[] ExtractInformationSources(NamedValueSet properties)
        {
            InformationSource[] informationSource = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("InformationSource"))
            {
                var result = properties.Get("InformationSource").AsValue<string>();
                informationSource = new InformationSource[1];
                informationSource[0] = InformationSourceHelper.Create(result);
            }
            return informationSource;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static BusinessCenters ExtractBusinessCenters(NamedValueSet properties)
        {
            BusinessCenters businessCentres = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("BusinessCenters"))
            {
                var centres = properties.Get("BusinessCenters").AsValue<string[]>();
                businessCentres = BusinessCentersHelper.Parse(centres);
            }
            return businessCentres;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static BusinessCenter ExtractBusinessCenter(NamedValueSet properties)
        {
            BusinessCenter businessCenter = null;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("BusinessCenter"))
            {
                var center = properties.Get("BusinessCenter").AsValue<string>();
                businessCenter = new BusinessCenter { Value = center };
            }
            return businessCenter;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static BusinessDayConventionEnum ExtractBusinessDayConvention(NamedValueSet properties)
        {
            var businessDayConvention = BusinessDayConventionEnum.MODFOLLOWING;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("BusinessDayConvention"))
            {
                var convention = properties.Get("BusinessDayConvention").AsValue<string>();
                businessDayConvention =
                    (BusinessDayConventionEnum)Enum.Parse(typeof(BusinessDayConventionEnum), convention, true);
            }
            return businessDayConvention;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static BusinessDayAdjustments ExtractBusinessDayAdjustments(NamedValueSet properties)
        {
            var businessDayConvention = ExtractBusinessDayConvention(properties);
            var businessCenters = ExtractBusinessCenters(properties);
            var businessDayAdjustments = new BusinessDayAdjustments
                                             {
                                                 businessDayConvention = businessDayConvention,
                                                 businessCenters = businessCenters
                                             };
            return businessDayAdjustments;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractEquityAsset(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();
            string equityAsset = "Unknown";
            if (dictionaryKeys.ContainsKey("EquityAsset"))
            {
                equityAsset = properties.Get("EquityAsset").AsValue<string>();
            }
            else
            {
                if (dictionaryKeys.ContainsKey(CurveProp.CurveName))
                {
                    var curveName = ExtractCurveName(properties);
                    equityAsset = curveName.Split('-')[1];
                }
            }
            return equityAsset;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractCommodityAsset(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();
            string commodityAsset = "Unknown";
            if (dictionaryKeys.ContainsKey("CommodityAsset"))
            {
                commodityAsset = properties.Get("CommodityAsset").AsValue<string>();
            }
            else
            {
                if (dictionaryKeys.ContainsKey(CurveProp.CurveName))
                {
                    var curveName = ExtractCurveName(properties);
                    commodityAsset = curveName.Split('-')[1];
                }
            }
            return commodityAsset;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static QuotedCurrencyPair ExtractQuotedCurrencyPair(NamedValueSet properties)
        {
            string currency = ExtractCurrency(properties);
            string quoteCurrency = ExtractCurrency2(properties);
            if (string.IsNullOrEmpty(currency) || string.IsNullOrEmpty(quoteCurrency))
            {
                string currencyPair = properties.GetString(CurveProp.CurrencyPair, false);
                if (!string.IsNullOrEmpty(currencyPair))
                {
                    if (!GetCurrencyPair(currencyPair, out currency, out quoteCurrency))
                    {
                        throw new ArgumentException("Currency pair is invalid, it should be of the type XXX-YYY");
                    }
                }
                else
                {
                    currencyPair = properties.GetString("CurveName", false);
                    if (!string.IsNullOrEmpty(currencyPair))
                    {
                        if (!GetCurrencyPair(currencyPair, out currency, out quoteCurrency))
                        {
                            throw new ArgumentException("CurveName is invalid, it should be of the type XXX-YYY");
                        }
                    }
                    else
                    {
                        string id = properties.GetString("Identifier", false);
                        if (!string.IsNullOrEmpty(currencyPair) && id.Split('.').Length < 2)
                        {
                            currencyPair = id.Split('.').Last();
                            if (!GetCurrencyPair(currencyPair, out currency, out quoteCurrency))
                            {
                                throw new ArgumentException("Identifier is invalid, it should be of the type XXX-YYY");
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Mandatory field CurrencyPair or QuoteCurrency, not set.");
                        }
                    }
                }
            }
            QuoteBasisEnum quoteBasis = ExtractQuoteBasis(properties);
            return QuotedCurrencyPair.Create(currency, quoteCurrency, quoteBasis);
        }

        private static bool GetCurrencyPair(string input, out string baseCurrency, out string quoteCurrency)
        {
            string[] pair = input.Split('-');
            if (pair.Length != 2 || pair[0].Length != 3 || pair[1].Length != 3)
            {
                baseCurrency = null;
                quoteCurrency = null;
                return false;
            }
            baseCurrency = pair[0];
            quoteCurrency = pair[1];
            return true;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static QuoteBasisEnum ExtractQuoteBasis(NamedValueSet properties)
        {
            var quoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("QuoteBasis"))
            {
                var quoteBasisString = properties.Get("QuoteBasis").AsValue<string>();
                quoteBasis =
                    (QuoteBasisEnum)Enum.Parse(typeof(QuoteBasisEnum), quoteBasisString, true);
            }
            return quoteBasis;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractCurveName(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.TryGetValue(CurveProp.CurveName, out var value))
            {
                return value.ToString();
            }
            if (dictionaryKeys.TryGetValue("PricingStructureName", out value))
            {
                return value.ToString();
            }
            string currency = ExtractCurrency(properties);
            string indexTenor = properties.GetString(CurveProp.IndexTenor, false);
            string index = ExtractIndex(properties);
            if (!string.IsNullOrEmpty(currency) && !string.IsNullOrEmpty(index))
            {
                if (!string.IsNullOrEmpty(indexTenor))
                {
                    return $"{currency}-{index}-{indexTenor}";
                }
                string instrument = ExtractInstrument(properties);
                if (!string.IsNullOrEmpty(instrument))
                {
                    return $"{currency}-{index}-{instrument}";
                }
                return $"{currency}-{index}";
            }
            return null;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static Boolean ExtractIsBaseCurveFlag(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();

            if (dictionaryKeys.ContainsKey("IsBaseCurve"))
            {
                var isBaseCurve = properties.Get("IsBaseCurve").AsValue<bool>();
                return isBaseCurve;
            }
            return true;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static PricingStructureTypeEnum ExtractPricingStructureType(NamedValueSet properties)
        {
            var pricingStructureType = properties.GetValue<string>(CurveProp.PricingStructureType, true);
            if (!EnumHelper.TryParse(pricingStructureType, true, out PricingStructureTypeEnum pricingStructureTypeEnum))
            {
                throw new ArgumentException($"PricingStructureType '{pricingStructureType}' not recognized.");
            }
            return pricingStructureTypeEnum;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static DateTime ExtractBuildDateTime(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();
            DateTime buildDateTime = dictionaryKeys.ContainsKey("BuildDateTime") ? properties.Get("BuildDateTime").AsValue<DateTime>() : DateTime.Now;
            return buildDateTime;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractPropertyIdentifier(NamedValueSet properties)
        {
            return properties.GetValue<string>("PropertyIdentifier", true);
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractIdentifier(NamedValueSet properties)
        {
            return properties.GetValue<string>("Identifier", true);
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractTradeIdentifier(NamedValueSet properties)
        {
            string identifier = properties.GetString(TradeProp.TradeId, false);
            if (string.IsNullOrEmpty(identifier))
            {
                identifier = "Unknown Trade Identifier.";
            }
            return identifier;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractAlgorithm(NamedValueSet properties)
        {
            string algorithm = properties.GetString("Algorithm", "Default");
            return algorithm;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractSource(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("Source"))
            {
                return properties.Get("Source").AsValue<string>();
            }
            if (!dictionaryKeys.ContainsKey(CurveProp.CurveName))
            {
                throw new System.Exception("Mandatory 'Source' property has not been specified. Please specify.");
            }
            var curveName = ExtractCurveName(properties).Split('-');

            if (curveName.Length > 2)
            {
                return curveName[2];
            }
            throw new System.Exception("Mandatory 'Source' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractInstrument(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();           
            if (dictionaryKeys.ContainsKey("Instrument"))
            {
                return properties.Get("Instrument").AsValue<string>();
            }
            if (dictionaryKeys.ContainsKey(CurveProp.CurveName))
            {
                var curveName = ExtractCurveName(properties).Split('-');

                if (curveName.Length > 1)
                {
                    return curveName[1];
                }
            }
            throw new System.Exception("Mandatory property 'Instrument' (or 'CurveName') has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractCurrency(NamedValueSet properties)
        {
            Dictionary<string, object> dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.TryGetValue(CurveProp.Currency1, out var value))
            {
                return value.ToString();
            }
            if (dictionaryKeys.TryGetValue("Currency1", out value))
            {
                return value.ToString();
            }
            if (dictionaryKeys.TryGetValue("BaseCurrency", out value))
            {
                return value.ToString();
            }
            if (dictionaryKeys.TryGetValue(CurveProp.CurveName, out value))
            {
                return value.ToString().Split('-')[0];
            }
            throw new System.Exception("Mandatory 'Currency' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractCurrency2(NamedValueSet properties)
        {
            Dictionary<string, object> dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.TryGetValue(CurveProp.Currency2, out var value))
            {
                return value.ToString();
            }
            if (dictionaryKeys.TryGetValue("QuoteCurrency", out value))
            {
                return value.ToString();
            }
            if (dictionaryKeys.TryGetValue(CurveProp.CurveName, out value))
            {
                return value.ToString().Split('-')[1];
            }
            throw new System.Exception("Mandatory 'Currency' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static bool ExtractLpmCopy(NamedValueSet properties)
        {
            var lpmCopy = false;
            var dictionaryKeys = properties.ToDictionary();

            if (dictionaryKeys.ContainsKey("LPMCopy"))
            {
                lpmCopy = properties.Get("LPMCopy").AsValue<bool>();
            }

            return lpmCopy;
        }
        
        /// <summary>
        /// Index is a short name such as Swap or LIBOR-BBA
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractIndex(NamedValueSet properties)
        {
            string index = properties.GetString("Index", "");
            if (string.IsNullOrEmpty(index))
            {
                string indexName = ExtractIndexName(properties);
                if (!string.IsNullOrEmpty(indexName))
                {
                    List<string> parts = indexName.Split('-').ToList();
                    if (parts.Count > 1)
                    {
                        parts.RemoveAt(0);
                        index = string.Join("-", parts.ToArray());
                    }
                    else if (parts.Count == 1)
                    {
                        index = indexName;
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// IndexName is an amalgamation of Currency and Index, such as AUD-Swap or AUD-LIBOR-BBA
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractIndexName(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();
            object value;
            if (dictionaryKeys.TryGetValue(CurveProp.IndexName, out value))
            {
                return value.ToString();
            }
            object currency;
            if (dictionaryKeys.TryGetValue("Index", out value)
                && dictionaryKeys.TryGetValue(CurveProp.Currency1, out currency))
            {
                return $"{currency}-{value}";
            }
            if (dictionaryKeys.TryGetValue(CurveProp.CurveName, out value))
            {
                List<string> curveNameParts = value.ToString().Split('-').ToList();
                if (curveNameParts.Count > 1)
                {
                    // Remove the last part - the tenor
                    curveNameParts.RemoveAt(curveNameParts.Count - 1);
                }
                return string.Join("-", curveNameParts.ToArray());
            }
            return null;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractIndexTenor(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();
            object value;
            if (dictionaryKeys.TryGetValue(CurveProp.IndexTenor, out value))
            {
                return value.ToString();
            }
            if (dictionaryKeys.TryGetValue(CurveProp.CurveName, out value))
            {
                string[] curveNameParts = value.ToString().Split('-');
                if (curveNameParts.Length > 2)
                {
                    return curveNameParts.Last();
                }
            }
            return null;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractCreditSeniority(NamedValueSet properties)
        {
            string creditSeniority;
            Dictionary<string, object> dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey(CurveProp.CreditSeniority))
            {
                creditSeniority = properties.Get(CurveProp.CreditSeniority).AsValue<string>();
            }
            else
            {
                string pst = ExtractPricingStructureType(properties).ToString();
                creditSeniority = "Senior";
                if (pst == PricingStructureTypeEnum.DiscountCurve.ToString())
                {
                    var curveName = properties.Get("CurveName").AsValue<string>();
                    List<string> parts = curveName.Split('-').ToList();
                    if (parts.Count > 2)
                    {
                        creditSeniority = parts[2];
                    }
                }
            }
            return creditSeniority;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractCreditInstrumentId(NamedValueSet properties)
        {
            var creditInstrumentId = "Unknown";
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey(CurveProp.CreditInstrumentId))
            {
                creditInstrumentId = properties.Get(CurveProp.CreditInstrumentId).AsValue<string>();
            }
            var pst = ExtractPricingStructureType(properties).ToString();
            if (pst == PricingStructureTypeEnum.DiscountCurve.ToString())
            {
                creditInstrumentId = ExtractIndex(properties);
            }
            return creditInstrumentId;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractInflationLag(NamedValueSet properties)
        {
            var dictionaryKeys = properties.ToDictionary();           
            return dictionaryKeys.ContainsKey("InflationLag") ? properties.Get("InflationLag").AsValue<string>() : "Unknown";
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceCurveName(NamedValueSet properties)
        {
            return properties.GetValue<string>(CurveProp.ReferenceCurveName, false) ??
                   properties.GetValue(CurveProp.BaseCurve, "Unknown");
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceCurveUniqueId(NamedValueSet properties)
        {
            return properties.GetValue<string>(CurveProp.ReferenceCurveUniqueId, null);
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceCurrency2CurveName(NamedValueSet properties)
        {
            return properties.GetValue<string>(CurveProp.ReferenceCurrency2CurveName, false);
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceCurrency2CurveId(NamedValueSet properties)
        {
            return properties.GetValue<string>(CurveProp.ReferenceCurrency2CurveId, null);
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceFxCurveName(NamedValueSet properties)
        {
            return properties.GetValue<string>(CurveProp.ReferenceFxCurveName, false);
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceFxCurveUniqueId(NamedValueSet properties)
        {
            return properties.GetValue<string>(CurveProp.ReferenceFxCurveUniqueId, null);
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceFxCurve2Name(NamedValueSet properties)
        {
            return properties.GetValue<string>(CurveProp.ReferenceFxCurve2Name, false);
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceFxCurve2UniqueId(NamedValueSet properties)
        {
            return properties.GetValue<string>(CurveProp.ReferenceFxCurve2UniqueId, null);
        }
        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static string ExtractReferenceVolCurveName(NamedValueSet properties)
        {
            var curveName = "Unknown";
            var dictionaryKeys = properties.ToDictionary();
            if (dictionaryKeys.ContainsKey("ReferenceVolCurveName"))
            {
                curveName = properties.Get("ReferenceVolCurveName").AsValue<string>();
            }
            return curveName;
        }

        /// <summary>
        /// A helper to extract properties from a named value set.
        /// </summary>
        /// <param name="properties">The collection of properties.</param>
        public static DateTime ExtractBaseDate(NamedValueSet properties)
        {
            DateTime baseDate = properties.GetValue(CurveProp.BaseDate, DateTime.MinValue);
            if (baseDate == DateTime.MinValue)
            {
                baseDate = ExtractBuildDateTime(properties);
            }
            return baseDate.Date;
        }
    }
}
