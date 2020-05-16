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
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.PricingStructures.Cubes;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.CurveEngine.V5r3.PricingStructures.Surfaces;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Constants;
using Highlander.Metadata.Common;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using FxCurve = Highlander.CurveEngine.V5r3.PricingStructures.Curves.FxCurve;
using Highlander.CurveEngine.V5r3.Assets.Helpers;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Factory
{
    /// <summary>
    /// This duplicates the factory..
    /// </summary>
    public static class PricingStructureFactory
    {
        #region Public Creators

        ///  <summary>
        ///  Create any pricing structure
        ///  </summary>
        ///  <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        ///  <param name="rollCalendar">The rollCalendar.</param>
        ///  <param name="properties"></param>
        ///  <param name="values"></param>
        ///  <returns></returns>
        public static IPricingStructure CreateGenericPricingStructure(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, object[,] values)
        {
            string suppliedPricingStructureType = properties.GetString(CurveProp.PricingStructureType, true);
            EnumHelper.TryParse(suppliedPricingStructureType, false, out PricingStructureTypeEnum psType);
            if (PricingStructureHelper.CurveTypes.Contains(psType))
            {
                return CreateGenericCurveStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, values);
            }
            if (PricingStructureHelper.VolBootstrapperTypes.Contains(psType))
            {
                return CreateGenericCurveStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, values);
            }
            if (PricingStructureHelper.VolSurfaceTypes.Contains(psType))
            {
                throw (new System.Exception($"{suppliedPricingStructureType} cannot be created using this function"));
            }
            throw (new System.Exception($"{suppliedPricingStructureType} cannot be created using this function"));
        }

        ///  <summary>
        ///  Mainly used for RateVolMatrix surfaces that have the inputs sent as additional
        ///  </summary>
        ///  <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        ///  <param name="rollCalendar">The rollCalendar.</param>
        ///  <param name="properties"></param>
        ///  <param name="values"></param>
        ///  <param name="additional"></param>
        ///  <returns></returns>
        public static IPricingStructure CreatePricingStructure(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, object[,] values, object[,] additional)
        {
            string suppliedPricingStructureType = properties.GetString(CurveProp.PricingStructureType, true);
            EnumHelper.TryParse(suppliedPricingStructureType, true, out PricingStructureTypeEnum psType);
            if (psType != PricingStructureTypeEnum.RateVolatilityMatrix)
            {
                return CreatePricingStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, values);
            }
            var expiries = new List<DateTime>();
            var volatilities = new List<double>();
            int rowCount = values.GetUpperBound(0);
            for (int row = 0; row <= rowCount; row++)
            {
                expiries.Add((DateTime)values[row, 0]);
                volatilities.Add((double)values[row, 1]);
            }
            int additionalLength = additional.GetUpperBound(0);
            var inputInstruments = new List<string>();
            var inputSwapRates = new List<double>();
            var inputBlackVolRates = new List<double>();
            for (int row = 0; row <= additionalLength; row++)
            {
                inputInstruments.Add(additional[row, 0].ToString());
                inputSwapRates.Add((double)additional[row, 1]);
                inputBlackVolRates.Add((double)additional[row, 2]);
            }
            return new RateVolatilitySurface(logger, cache, nameSpace, properties, expiries.ToArray(), volatilities.ToArray(), inputInstruments.ToArray(),
                inputSwapRates.ToArray(), inputBlackVolRates.ToArray());
        }

        /// <summary>
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        ///  <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="propertyNames"></param>
        /// <param name="propertyValues"></param>
        /// <param name="instruments"></param>
        /// <param name="rates"></param>
        /// <param name="interpolationFrequency"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static object[][] CreateZeroCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, 
            string[] propertyNames, object[] propertyValues, string[] instruments, double[] rates, string interpolationFrequency)
        {
            //  create NVS from property names and values
            //
            var nvs = new NamedValueSet();
            for (int j = 0; j < propertyNames.Length; j++)
            {
                object obj;
                switch (propertyNames[j])
                {
                    case ("BuildDateTime"):
                        obj = Convert.ToDateTime(propertyValues[j]);
                        break;
                    default:
                        obj = propertyValues[j];
                        break;
                }
                nvs.Set(propertyNames[j], obj);
            }
            var ratesAsDecimals = from rate in rates
                                  select new decimal(rate);
            var asDecimals = ratesAsDecimals as decimal[] ?? ratesAsDecimals.ToArray();
            var rateCurve = (RateCurve)CreateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, nvs, instruments, asDecimals.ToArray(), new decimal[asDecimals.Length]);
            var curveAsArray = rateCurve.GetTermCurve().To2DArray();
            var beginDate = (DateTime)curveAsArray[0, 0];
            var endDate = (DateTime)curveAsArray[curveAsArray.GetUpperBound(0), 0];
            var interpolationFrequencyAsPeriod = PeriodHelper.Parse(interpolationFrequency);
            var targetDate = beginDate;
            var listOfDates = new List<DateTime>();
            var listOfDfs = new List<double>();
            do
            {
                var df = rateCurve.GetDiscountFactor(targetDate);
                listOfDates.Add(targetDate);
                listOfDfs.Add(df);
                targetDate = interpolationFrequencyAsPeriod.Add(targetDate);

            } while (targetDate <= endDate);

            var result = new[]
                             {
                                 (from date in listOfDates select (object)date).ToArray(),
                                 (from df in listOfDfs select (object)df).ToArray()
                             };
            return result;
        }

        /// <summary>
        /// Creates the rate curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public static IPricingStructure CreateCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, string[] instruments, decimal[] adjustedRates, decimal[] additional)
        {
            //  Pricing structure type
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
            QuotedAssetSet quotedAssetSet;
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.RateCurve:
                case PricingStructureTypeEnum.ClearedRateCurve:
                case PricingStructureTypeEnum.InflationCurve:
                case PricingStructureTypeEnum.DiscountCurve:
                case PricingStructureTypeEnum.RateSpreadCurve:
                case PricingStructureTypeEnum.RateBasisCurve:
                case PricingStructureTypeEnum.RateXccyCurve:
                case PricingStructureTypeEnum.XccySpreadCurve:
                case PricingStructureTypeEnum.BondFinancingCurve:
                case PricingStructureTypeEnum.BondFinancingBasisCurve:
                case PricingStructureTypeEnum.BondCurve:
                case PricingStructureTypeEnum.BondDiscountCurve:
                case PricingStructureTypeEnum.CapVolatilityCurve:
                case PricingStructureTypeEnum.ExchangeTradedCurve:
                    quotedAssetSet = AssetHelper.Parse(instruments, adjustedRates, additional);                 
                    break;
                case PricingStructureTypeEnum.GenericVolatilityCurve:
                    quotedAssetSet = AssetHelper.ParseTenorSet(instruments, adjustedRates, additional);
                    break;
                case PricingStructureTypeEnum.FxCurve:
                case PricingStructureTypeEnum.CommodityCurve:
                case PricingStructureTypeEnum.CommoditySpreadCurve:
                case PricingStructureTypeEnum.EquityCurve:
                case PricingStructureTypeEnum.EquitySpreadCurve:
                    quotedAssetSet = AssetHelper.ParseToFxRateSet(instruments, adjustedRates, additional);
                    break;
                default:
                    string message =
                        $"Specified pricing structure type : '{pricingStructureType}' has not been recognized.";
                    throw new ApplicationException(message);
            }
            var structure = CreateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, quotedAssetSet);
            return structure;
        }

        /// <summary>
        /// Creates the rate curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="algorithmProperties">THe curve algorithm properties.</param>
        /// <returns></returns>
        public static IPricingStructure CreateRateCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, string[] instruments, decimal[] adjustedRates, decimal[] additional, Algorithm algorithmProperties)
        {
            //  Pricing structure type
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
            QuotedAssetSet quotedAssetSet;
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.RateCurve:
                case PricingStructureTypeEnum.DiscountCurve:
                    quotedAssetSet = AssetHelper.Parse(instruments, adjustedRates, additional);
                    break;
                default:
                    string message =
                        $"Specified pricing structure type : '{pricingStructureType}' has not been recognized.";
                    throw new ApplicationException(message);
            }
            var structure = CreateRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, quotedAssetSet, algorithmProperties);
            return structure;
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">the nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="quotedAssetSet">The QuotedAssetSet.</param>
        /// <param name="algorithmProperties">The curve algorithm.</param>
        /// <returns></returns>
        public static IPricingStructure CreateRateCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, NamedValueSet properties, QuotedAssetSet quotedAssetSet, Algorithm algorithmProperties)
        {
            //  Pricing structure type
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
            IPricingStructure structure;
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.RateCurve:
                case PricingStructureTypeEnum.DiscountCurve:
                    structure = new RateCurve(logger, cache, nameSpace, properties, quotedAssetSet, algorithmProperties, fixingCalendar, rollCalendar);
                    break;
                default:
                    var message =
                        $"Specified pricing structure type : '{pricingStructureType}' has not been recognized.";
                    throw new ApplicationException(message);
            }
            return structure;
        }


        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">the nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="quotedAssetSet">The QuotedAssetSet.</param>
        /// <returns></returns>
        public static IPricingStructure CreateCurve(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar,
            IBusinessCalendar rollCalendar, NamedValueSet properties, QuotedAssetSet quotedAssetSet)
        {
            //  Pricing structure type
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
            IPricingStructure structure;
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.RateCurve:
                case PricingStructureTypeEnum.DiscountCurve:
                    structure = new RateCurve(logger, cache, nameSpace, properties, quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.InflationCurve:
                    structure = new InflationCurve(logger, cache, nameSpace, properties, quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.BondFinancingCurve:
                    var refId = properties.GetValue<string>(BondProp.ReferenceBond, true);
                    var refItem = cache.LoadItem<Bond>(nameSpace + '.' + "ReferenceData.FixedIncome." + refId);
                    var refAsset = refItem.Data as Bond;
                    structure = new SecuredRateCurve(logger, cache, nameSpace, refAsset, properties, quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.BondFinancingBasisCurve:
                    var ref1Id = properties.GetValue<string>(BondProp.ReferenceBond, true);
                    var ref1Item = cache.LoadItem<Bond>(nameSpace + '.' + "ReferenceData.FixedIncome." + ref1Id);
                    var ref1Asset = ref1Item.Data as Bond;
                    string refCurve2Id = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    var refCurve2 = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, refCurve2Id);
                    structure = new SecuredRateBasisCurve(logger, cache, nameSpace, ref1Asset, refCurve2, quotedAssetSet, properties, fixingCalendar, rollCalendar);  
                    break;
                case PricingStructureTypeEnum.BondDiscountCurve:
                    structure = new BondCurve(logger, cache, nameSpace, properties, quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.BondCurve:
                    var bondId = properties.GetValue<string>(CurveProp.ReferenceBond, true);
                    var currency = properties.GetValue<string>(CurveProp.Currency1, true);
                    var curve = CurveNameHelpers.GetBondCurveNameSimple(currency, bondId);
                    var curveName = properties.GetValue(CurveProp.CurveName, curve);
                    properties.Set(CurveProp.CurveName, curveName);
                    structure = new BondCurve(logger, cache, nameSpace, properties, quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.ExchangeTradedCurve:
                    var futuresCode = properties.GetValue<string>(CurveProp.ContractCode, true);
                    var futuresCurrency = properties.GetValue<string>(CurveProp.Currency1, true);
                    var exchange = properties.GetValue<string>(CurveProp.Exchange, true);
                    var exchangeCurve = CurveNameHelpers.GetExchangeTradedCurveNameSimple(futuresCurrency, exchange, futuresCode);
                    var exchangeCurveName = properties.GetValue(CurveProp.CurveName, exchangeCurve);
                    properties.Set(CurveProp.CurveName, exchangeCurveName);
                    structure = new ExchangeTradedCurve(logger, cache, nameSpace, properties, quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.RateSpreadCurve:
                    string curveId = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    IPricingStructure refCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, curveId);
                    structure = new RateSpreadCurve(logger, cache, nameSpace, properties, refCurve, quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.ClearedRateCurve:
                    string baseDiscountingCurveId = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    var baseDiscountingCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, baseDiscountingCurveId);
                    structure = new ClearedRateCurve(logger, cache, nameSpace, baseDiscountingCurve, quotedAssetSet, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.RateBasisCurve:
                    var refCurveName1 = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    var refCurve1 = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, refCurveName1);
                    structure = new RateBasisCurve(logger, cache, nameSpace, refCurve1, quotedAssetSet, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.RateXccyCurve:
                    string baseCurveName = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    var baseCurve = (RateCurve)CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, baseCurveName);
                    string fxCurveName = PropertyHelper.ExtractReferenceFxCurveUniqueId(properties);
                    var fxCurve = CurveLoader.LoadFxCurve(logger, cache, nameSpace, fxCurveName);
                    string currency2CurveName = PropertyHelper.ExtractReferenceCurrency2CurveId(properties);
                    var currency2Curve = (RateCurve)CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, currency2CurveName);
                    structure = new RateXccySpreadCurve(logger, cache, nameSpace, properties, baseCurve, fxCurve, currency2Curve, quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.XccySpreadCurve:
                    string baseCurveName2 = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    var baseCurve2 = (RateCurve)CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, baseCurveName2);
                    string fxCurveName2 = PropertyHelper.ExtractReferenceFxCurveUniqueId(properties);
                    var fxCurve2 = (FxCurve)CurveLoader.LoadFxCurve(logger, cache, nameSpace, fxCurveName2);
                    string currency2CurveName2 = PropertyHelper.ExtractReferenceCurrency2CurveId(properties);
                    var currency2Curve2 = (RateCurve)CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, currency2CurveName2);
                    structure = new XccySpreadCurve(logger, cache, nameSpace, properties, quotedAssetSet, baseCurve2, currency2Curve2, fxCurve2, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.FxCurve:
                    structure = new FxCurve(logger, cache, nameSpace, properties, (FxRateSet)quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.CommodityCurve:
                    structure = new CommodityCurve(logger, cache, nameSpace, properties, (FxRateSet)quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.CommoditySpreadCurve:
                    var refCommodityCurveName = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    var refCommodityCurve1 = CurveLoader.LoadCommodityCurve(logger, cache, nameSpace, refCommodityCurveName);
                    structure = new CommoditySpreadCurve2(logger, cache, nameSpace, refCommodityCurve1, (FxRateSet)quotedAssetSet, properties, rollCalendar);
                    break;
                case PricingStructureTypeEnum.EquityCurve:
                    structure = new EquityCurve(logger, cache, nameSpace, properties, (FxRateSet)quotedAssetSet, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.CapVolatilityCurve:
                    var discountCurveName = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    var discountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, discountCurveName);
                    var forecastCurveName = PropertyHelper.ExtractReferenceCurrency2CurveId(properties);
                    var forecastCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, forecastCurveName);
                    structure = new CapVolatilityCurve(logger, cache, nameSpace, properties, quotedAssetSet, discountCurve, forecastCurve, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.GenericVolatilityCurve:
                    var discountCurveName1 = PropertyHelper.ExtractReferenceCurveUniqueId(properties);
                    var discountCurve1 = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, discountCurveName1);
                    var forecastCurveName1 = PropertyHelper.ExtractReferenceCurrency2CurveId(properties);
                    var forecastCurve1 = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, forecastCurveName1);
                    structure = new GenericVolatilityCurve(logger, cache, nameSpace, properties, quotedAssetSet, discountCurve1, forecastCurve1, fixingCalendar, rollCalendar);
                    break;
                default:
                    var message =
                        $"Specified pricing structure type : '{pricingStructureType}' has not been recognized.";
                    throw new ApplicationException(message);
            }
            return structure;
        }

        /// <summary>
        /// Creates the volatility surface.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTerms">The expiry terms.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volatilities">The volatilities.</param>
        /// <returns></returns>
        public static IPricingStructure CreateVolatilitySurface(ILogger logger, ICoreCache cache, 
            String nameSpace, NamedValueSet properties, 
            String[] expiryTerms, double[] strikes, Double[,] volatilities)
        {
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
            IPricingStructure structure;
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.RateVolatilityMatrix:
                    structure = new RateVolatilitySurface(logger, cache, nameSpace, properties, expiryTerms, strikes, volatilities);
                    break;
                case PricingStructureTypeEnum.FxVolatilityMatrix:
                    structure = new FxVolatilitySurface(logger, cache, nameSpace, properties, expiryTerms, strikes, volatilities);
                    break;
                case PricingStructureTypeEnum.CommodityVolatilityMatrix:
                    structure = new CommodityVolatilitySurface(logger, cache, nameSpace, properties, expiryTerms, strikes, volatilities);
                    break;
                case PricingStructureTypeEnum.EquityVolatilityMatrix:
                    structure = new EquityVolatilitySurface(logger, cache, nameSpace, properties, expiryTerms, strikes, volatilities);
                    break;
                case PricingStructureTypeEnum.EquityWingVolatilityMatrix:
                    structure = new EquityVolatilitySurface(logger, cache, nameSpace, properties, expiryTerms, strikes, volatilities);
                    break;
                case PricingStructureTypeEnum.SABRSurface:
                    structure = new SABRVolatilitySurface(logger, cache, nameSpace, properties, expiryTerms, strikes, volatilities);
                    break;
                default:
                    {
                        var message = $"Specified PricingStructureType: '{pricingStructureType}' is not supported";
                        throw new ApplicationException(message);
                    }
            }
            return structure;
        }

        /// <summary>
        /// Creates the specified vol curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTerms">An array of expiry tenors..</param>
        /// <param name="strikesOrTenor">An array of strikes or tenors.</param>
        /// <param name="volatilities">A range of volatilities of the correct dimension.</param>
        /// <returns></returns>
        public static IPricingStructure CreateVolatilitySurface(ILogger logger, ICoreCache cache, String nameSpace,
            NamedValueSet properties, String[] expiryTerms, String[] strikesOrTenor, Double[,] volatilities)
        {
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            PricingStructureTypeEnum pricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
            IPricingStructure pricingStructure;
            if (pricingStructureType == PricingStructureTypeEnum.RateATMVolatilityMatrix)
            {
                pricingStructure = new RateATMVolatilitySurface(logger, cache, nameSpace, properties, expiryTerms, strikesOrTenor, volatilities);
            }
            else
            {
                double[] strikes = ConvertStringArrayToDoubleArray(strikesOrTenor);
                pricingStructure = CreateVolatilitySurface(logger, cache, nameSpace, properties, expiryTerms, strikes, volatilities);
            }
            return pricingStructure;
        }

        private static double[] ConvertStringArrayToDoubleArray(string[] strikes)
        {
            var result = new double[strikes.Length];
            for (var i = 0; i < strikes.Length; i++)
            {
                result[i] = Convert.ToDouble(strikes[i]);
            }
            return result;
        }

        /// <summary>
        /// Returns an pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="referenceCurveData">The reference curve data.</param>
        /// <param name="referenceFxCurveData">The Fx reference curve.</param>
        /// <param name="currency2CurveData">The currency2 data.</param>
        /// <param name="spreadCurveData">The spread curve data.</param>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public static IPricingStructure Create(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, 
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceFxCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> currency2CurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData)
        {
            //  Pricing structure type
            //
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(spreadCurveData.Third);
            if (pricingStructureType == PricingStructureTypeEnum.RateXccyCurve)
            {
                return new RateXccySpreadCurve(logger, cache, nameSpace, referenceCurveData, referenceFxCurveData, currency2CurveData, spreadCurveData, fixingCalendar, rollCalendar);
            }
            if (pricingStructureType == PricingStructureTypeEnum.RateBasisCurve)
            {
                return new RateBasisCurve(logger, cache, nameSpace, referenceCurveData, spreadCurveData, fixingCalendar, rollCalendar);
            }
            if (pricingStructureType == PricingStructureTypeEnum.ClearedRateCurve)
            {
                return new ClearedRateCurve(logger, cache, nameSpace, referenceCurveData, spreadCurveData, fixingCalendar, rollCalendar);
            }
            var pricingPair = new Pair<PricingStructure, PricingStructureValuation>(spreadCurveData.First, spreadCurveData.Second);
            return Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, pricingPair, spreadCurveData.Third);
        }

        /// <summary>
        /// Returns an pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="referenceCurveData">The reference curve data.</param>
        /// <param name="derivedCurveData">The derived curve data.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public static IPricingStructure Create(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, 
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> derivedCurveData)
        {
            //  Pricing structure type
            //
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(derivedCurveData.Third);
            if (pricingStructureType == PricingStructureTypeEnum.RateBasisCurve)
            {
                return new RateBasisCurve(logger, cache, nameSpace, referenceCurveData, derivedCurveData, fixingCalendar, rollCalendar);
            }
            if (pricingStructureType == PricingStructureTypeEnum.ClearedRateCurve)
            {
                return new ClearedRateCurve(logger, cache, nameSpace, referenceCurveData, derivedCurveData, fixingCalendar, rollCalendar);
            }
            var pricingPair = new Pair<PricingStructure, PricingStructureValuation>(derivedCurveData.First, derivedCurveData.Second);
            return Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, pricingPair, derivedCurveData.Third);
        }

        /// <summary>
        /// Initialise all valid pricing structures from the FpML.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="rollCalendar">The rollCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        public static IPricingStructure Create(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
        {
            //  Pricing structure type
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
            IPricingStructure curve;
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.RateCurve:
                case PricingStructureTypeEnum.DiscountCurve:
                    curve = new RateCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.BondDiscountCurve:
                    curve = new BondCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.BondCurve:
                    var bondId = properties.GetValue<String>(CurveProp.ReferenceBond, true);
                    var currency = properties.GetValue<String>(CurveProp.Currency1, true);
                    var temp = CurveNameHelpers.GetBondCurveNameSimple(currency, bondId);
                    var curveName = properties.GetValue(CurveProp.CurveName, temp);
                    properties.Set(CurveProp.CurveName, curveName);
                    curve = new BondCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.ExchangeTradedCurve:
                    var futuresCode = properties.GetValue<String>(CurveProp.ContractCode, true);
                    var futuresCurrency = properties.GetValue<String>(CurveProp.Currency1, true);
                    var exchange = properties.GetValue<String>(CurveProp.Exchange, true);
                    var exchangeCurve = CurveNameHelpers.GetExchangeTradedCurveNameSimple(futuresCurrency, exchange, futuresCode);
                    var exchangeCurveName = properties.GetValue(CurveProp.CurveName, exchangeCurve);
                    properties.Set(CurveProp.CurveName, exchangeCurveName);
                    curve = new ExchangeTradedCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.FxCurve:
                    curve = new FxCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.InflationCurve:
                    curve = new InflationCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.BondFinancingCurve:
                    curve = new SecuredRateCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.BondFinancingBasisCurve:
                    curve = new SecuredRateBasisCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);                 
                    break;
                case PricingStructureTypeEnum.RateSpreadCurve:
                    curve = new RateSpreadCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.RateBasisCurve:
                    curve = new RateBasisCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.ClearedRateCurve:
                    curve = new ClearedRateCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.XccySpreadCurve:
                    curve = new XccySpreadCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.CommodityCurve:
                    curve = new CommodityCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.CommoditySpreadCurve:
                    curve = new CommoditySpreadCurve2(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.RateVolatilityMatrix:
                    curve = new RateVolatilitySurface(logger, cache, nameSpace, fpmlData, properties);
                    break;
                case PricingStructureTypeEnum.FxVolatilityMatrix:
                    curve = new FxVolatilitySurface(logger, cache, nameSpace, fpmlData, properties);
                    break;
                case PricingStructureTypeEnum.CommodityVolatilityMatrix:
                    curve = new CommodityVolatilitySurface(logger, cache, nameSpace, fpmlData, properties);
                    break;
                case PricingStructureTypeEnum.EquityVolatilityMatrix:
                    curve = new EquityVolatilitySurface(logger, cache, nameSpace, fpmlData, properties);
                    break;
                case PricingStructureTypeEnum.EquityWingVolatilityMatrix:
                    curve = new ExtendedEquityVolatilitySurface(logger, cache, nameSpace, fpmlData, properties);
                    break;
                case PricingStructureTypeEnum.RateATMVolatilityMatrix:
                    curve = new RateATMVolatilitySurface(logger, cache, nameSpace, fpmlData, properties);
                    break;
                case PricingStructureTypeEnum.RateVolatilityCube:
                    curve = new VolatilityCube(fpmlData, properties);
                    break;
                case PricingStructureTypeEnum.EquityCurve:
                    curve = new EquityCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.CapVolatilityCurve:
                    curve = new CapVolatilityCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.GenericVolatilityCurve:
                    curve = new GenericVolatilityCurve(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                case PricingStructureTypeEnum.SABRSurface:
                    curve = new SABRVolatilitySurface(logger, cache, nameSpace, fpmlData, properties);
                    break;
                case PricingStructureTypeEnum.CapVolatilitySurface:
                    curve = new CapVolatilitySurface(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"PricingStructureType '{pricingStructureType}' not supported.");
            }
            return curve;
        }

        /// <summary>
        /// Create a pricing structure!
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="rollCalendar">The rollCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="properties">The curve properties</param>
        /// <param name="values">A range object that contains the instruments and quotes.</param>
        /// <returns></returns>
        public static IPricingStructure CreatePricingStructure(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, NamedValueSet properties, object[,] values)
        {
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            string suppliedPricingStructureType = properties.GetString(CurveProp.PricingStructureType, true);
            EnumHelper.TryParse(suppliedPricingStructureType, true, out PricingStructureTypeEnum psType);
            if (PricingStructureHelper.CurveTypes.Contains(psType))
            {
                return CreateCurveStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, values);
            }
            if (PricingStructureHelper.VolBootstrapperTypes.Contains(psType))
            {
                return CreateCurveStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, values);
            }
            string[] expiries = ExtractExpiries(values);
            double[,] volatilities = ExtractVolatilities(values);
            if (psType == PricingStructureTypeEnum.RateATMVolatilityMatrix)
            {
                string[] tenors = ExtractTenors(values);
                return new RateATMVolatilitySurface(logger, cache, nameSpace, properties, expiries, tenors, volatilities);
            }
            if (!PricingStructureHelper.VolSurfaceTypes.Contains(psType))
                throw new System.Exception($"{suppliedPricingStructureType} cannot be created using this function");
            double[] strikes = ExtractStrikes(values);
            return CreateVolatilitySurface(logger, cache, nameSpace, properties, expiries, strikes, volatilities);
        }

        /// <summary>
        /// Create a pricing structure!
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="rollCalendar">The rollCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="properties">The curve properties</param>
        /// <param name="additional">To be provided if building a curve.</param>
        /// <param name="expiries">To be provided if building a volatility surface.</param>
        /// <param name="tenors">To be provided if building a volatility surface.</param>
        /// <param name="strikes">To be provided if building a volatility surface.</param>
        /// <param name="volatilities">To be provided if building a volatility surface.</param>
        /// <param name="instruments">To be provided if building a curve.</param>
        /// <param name="rates">To be provided if building a curve.</param>
        /// <returns></returns>
        public static IPricingStructure CreatePricingStructure(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, NamedValueSet properties, string[] instruments, decimal[] rates,
            decimal[] additional, string[] expiries, string[] tenors, double[] strikes, double[,] volatilities)
        {
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            string suppliedPricingStructureType = properties.GetString(CurveProp.PricingStructureType, true);
            EnumHelper.TryParse(suppliedPricingStructureType, true, out PricingStructureTypeEnum psType);
            if (PricingStructureHelper.CurveTypes.Contains(psType))
            {
                return CreateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, instruments, rates, additional);
            }
            if (PricingStructureHelper.VolBootstrapperTypes.Contains(psType))
            {
                return CreateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, instruments, rates, additional);
            }
            if (psType == PricingStructureTypeEnum.RateATMVolatilityMatrix)
            {
                return new RateATMVolatilitySurface(logger, cache, nameSpace, properties, expiries, tenors, volatilities);
            }
            if (!PricingStructureHelper.VolSurfaceTypes.Contains(psType))
                throw new System.Exception($"{suppliedPricingStructureType} cannot be created using this function");
            return CreateVolatilitySurface(logger, cache, nameSpace, properties, expiries, strikes, volatilities);
        }

        /// <summary>
        /// Create a pricing structure!
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="rollCalendar">The rollCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="properties">The curve properties</param>
        /// <param name="values">A range object that contains the instruments and quotes.</param>
        /// <param name="algorithm">The algorithm properties. They had better be correct fot the pricing structure or else!!</param>
        /// <returns></returns>
        public static IPricingStructure CreatePricingStructure(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, NamedValueSet properties, object[,] values, Algorithm algorithm)
        {
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            string suppliedPricingStructureType = properties.GetString(CurveProp.PricingStructureType, true);
            EnumHelper.TryParse(suppliedPricingStructureType, true, out PricingStructureTypeEnum psType);
            return PricingStructureHelper.CurveTypes.Contains(psType) ? CreateRateCurveStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, values, algorithm) : null;
        }

        /// <summary>
        /// Create a pricing structure!
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="rollCalendar">The rollCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="properties">The curve properties</param>
        /// <param name="instruments"></param>
        /// <param name="values">A range object that contains the instruments and quotes.</param>
        /// <param name="additional">Any additional values.</param>
        /// <param name="algorithm">The algorithm properties. They had better be correct fot the pricing structure or else!!</param>
        /// <returns></returns>
        public static IPricingStructure CreatePricingStructure(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, NamedValueSet properties, List<string> instruments, 
            List<decimal> values, List<decimal> additional, Algorithm algorithm)
        {
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            string suppliedPricingStructureType = properties.GetString(CurveProp.PricingStructureType, true);
            EnumHelper.TryParse(suppliedPricingStructureType, true, out PricingStructureTypeEnum psType);
            return PricingStructureHelper.CurveTypes.Contains(psType) ? CreateRateCurveStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, instruments, values, additional, algorithm) : null;
        }

        /// <summary>
        /// Create a pricing structure
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="fixingCalendar">The fixingCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="rollCalendar">The rollCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instrument list.</param>
        /// <param name="values">A range object that contains the instruments and quotes.</param>
        /// <param name="additional">Any additional data.</param>
        /// <returns></returns>
        public static IPricingStructure CreatePricingStructure(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, NamedValueSet properties, List<string> instruments,
            List<decimal> values, List<decimal> additional)
        {
            properties.Set(CurveProp.BaseDate, baseDate);
            properties.Set(CurveProp.BuildDateTime, baseDate);
            properties.Set(CurveProp.MarketDate, baseDate);
            return CreatePricingStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar,
                properties, instruments.ToArray(), values.ToArray(), additional.ToArray(), null, null, null, null);
        }

        /// <summary>
        /// Create a pricing structure
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fixingCalendar">The fixingCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="rollCalendar">The rollCalendar. The calendar is only required if the curve needs to be re-bootstrapped or the priceable assets are required.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="headers">The value headers</param>
        /// <param name="values">A range object that contains the instruments and quotes.</param>
        /// <returns></returns>
        public static List<IPricingStructure> CreatePricingStructures(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, NamedValueSet properties, IList<string> headers, object[,] values)
        {
            var result = new List<IPricingStructure>();
            var numRows = values.GetLength(0);
            var numColumns = values.GetLength(1);
            if (headers.Count <= numColumns)
            {                
                for (int i = 0; i < numRows; i++)
                {
                    //Clears all the added properties.
                    var newProperties = properties.Clone();
                    var baseDate = values[i, 0];
                    newProperties.Set(CurveProp.BaseDate, baseDate);
                    newProperties.Set(CurveProp.BuildDateTime, baseDate);
                    newProperties.Set(CurveProp.MarketDate, baseDate);
                    var newValues = new object[headers.Count, 3];
                    //Map to the instruments using the currency property and others.
                    var index = 0;
                    foreach (var instrument in headers)
                    {
                        newValues[index, 0] = instrument;
                        newValues[index, 1] = values[i, index+1];
                        newValues[index, 2] = 0.0m;
                        index++;
                    }
                    var curve = CreatePricingStructure(logger, cache, nameSpace, fixingCalendar, rollCalendar,
                                                       newProperties, newValues);
                    result.Add(curve);
                }
                return result;
            }
            return null;
        }

        #endregion

        #region Private Creators

        private static CurveBase CreateGenericCurveStructure(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, 
            NamedValueSet properties, object[,] values)
        {
            var instruments = new string[values.GetLength(0)];
            var rates = new decimal[values.GetLength(0)];
            var measures = new string[values.GetLength(0)];
            var units = new string[values.GetLength(0)];
            int lby = values.GetLowerBound(0);
            int uby = values.GetUpperBound(0);
            int lbx = values.GetLowerBound(1);
            int ubx = values.GetUpperBound(1);
            for (var i = lby; i <= uby; ++i)
            {
                instruments[i - lby] = Convert.ToString(values[i, lbx]);
                rates[i - lby] = Convert.ToDecimal(values[i, lbx + 1]);
                if (ubx < lbx + 2) continue;
                if (null != values[i, lbx + 2])
                {
                    measures[i - lby] = Convert.ToString(values[i, lbx + 2]);
                }
                if (null != values[i, lbx + 3])
                {
                    units[i - lby] = Convert.ToString(values[i, lbx + 3]);
                }
            }
            var quotedAssetSet = AssetHelper.Parse(instruments, rates, measures, units);
            var structure = (CurveBase)CreateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, quotedAssetSet);
            return structure;
        }

        private static CurveBase CreateCurveStructure(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, 
            NamedValueSet properties, object[,] values)
        {
            var instruments = new string[values.GetLength(0)];
            var rates = new decimal[values.GetLength(0)];
            var additional = new decimal[values.GetLength(0)];
            int lby = values.GetLowerBound(0);
            int uby = values.GetUpperBound(0);
            int lbx = values.GetLowerBound(1);
            int ubx = values.GetUpperBound(1);
            for (var i = lby; i <= uby; ++i)
            {
                instruments[i - lby] = Convert.ToString(values[i, lbx]);
                rates[i - lby] = Convert.ToDecimal(values[i, lbx + 1]);
                if (ubx < lbx + 2) continue;
                if (null != values[i, lbx + 2])
                {
                    additional[i - lby] = Convert.ToDecimal(values[i, lbx + 2]);
                }
            }
            var structure = (CurveBase)CreateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, instruments, rates, additional);
            return structure;
        }

        private static CurveBase CreateCurveStructure(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, List<string> instruments, List<decimal> values, List<decimal> additional)
        {
            var structure = (CurveBase)CreateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, instruments.ToArray(), values.ToArray(), additional.ToArray());
            return structure;
        }

        public static CurveBase CreateRateCurveStructure(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, List<string> instruments, List<decimal> values, List<decimal> additional, Algorithm algorithmProperties)
        {
            var structure = (CurveBase)CreateRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, instruments.ToArray(), 
                values.ToArray(), additional.ToArray(), algorithmProperties);
            return structure;
        }

        private static CurveBase CreateRateCurveStructure(ILogger logger, ICoreCache cache,
            string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, object[,] values, Algorithm algorithmProperties)
        {
            var instruments = new string[values.GetLength(0)];
            var rates = new decimal[values.GetLength(0)];
            var additional = new decimal[values.GetLength(0)];
            int lby = values.GetLowerBound(0);
            int uby = values.GetUpperBound(0);
            int lbx = values.GetLowerBound(1);
            int ubx = values.GetUpperBound(1);
            for (var i = lby; i <= uby; ++i)
            {
                instruments[i - lby] = Convert.ToString(values[i, lbx]);
                rates[i - lby] = Convert.ToDecimal(values[i, lbx + 1]);
                if (ubx < lbx + 2) continue;
                if (null != values[i, lbx + 2])
                {
                    additional[i - lby] = Convert.ToDecimal(values[i, lbx + 2]);
                }
            }
            var structure = (CurveBase)CreateRateCurve(logger, cache, nameSpace, fixingCalendar, rollCalendar, properties, instruments, rates, additional, algorithmProperties);
            return structure;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Extract the strikes information held in the first row of the array
        /// </summary>
        private static double[] ExtractStrikes(object[,] rawGrid)
        {
            var strikes = new List<double>();
            int maxColumns = rawGrid.GetUpperBound(1) + 1;
            for (int cols = 1; cols < maxColumns; cols++)
            {
                if (!double.TryParse(rawGrid[0, cols].ToString(), out double strike))
                {
                    throw new InvalidCastException($"Cannot cast '{rawGrid[0, cols]}' to strike");
                }
                strikes.Add(strike);
            }
            return strikes.ToArray();
        }

        /// <summary>
        /// Extract the expiry column from the raw data grid
        /// </summary>
        private static string[] ExtractExpiries(object[,] rawGrid)
        {
            int maxRows = rawGrid.GetUpperBound(0) + 1;
            var expiries = new string[maxRows - 1];
            for (int row = 1; row < maxRows; row++)
                expiries[row - 1] = rawGrid[row, 0].ToString();
            return expiries;
        }

        /// <summary>
        /// Extract the expiry column from the raw data grid
        /// </summary>
        private static string[] ExtractTenors(object[,] rawGrid)
        {
            int maxColumns = rawGrid.GetUpperBound(1) + 1;
            var tenors = new string[maxColumns - 1];
            for (int column = 1; column < maxColumns; column++)
                tenors[column - 1] = rawGrid[0, column].ToString();
            return tenors;
        }

        /// <summary>
        /// Extract the volatilities from the raw Data grid (everything except the first column and row)
        /// </summary>
        private static double[,] ExtractVolatilities(object[,] rawGrid)
        {
            int maxRows = rawGrid.GetUpperBound(0);
            int maxColumns = rawGrid.GetUpperBound(1);
            var volatilities = new double[maxRows, maxColumns];
            for (int column = 1; column <= maxColumns; column++)
            {
                for (int row = 1; row <= maxRows; row++)
                {
                    if (!double.TryParse(rawGrid[row, column].ToString(), out double vol))
                    {
                        throw new InvalidCastException($"Cannot cast '{rawGrid[row, column]}' to volatility");
                    }
                    volatilities[row - 1, column - 1] = vol;
                }
            }
            return volatilities;
        }

        #endregion

        #region Old Methods

        internal const string DefaultTenor = "3M";
        private const string CurrencyUsd = "USD";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static NamedValueSet GetInterestRateCurveProperties(ILogger logger, ICoreCache cache, string nameSpace, string marketName, string curveName, string stress)
        {
            // Filter by supplied tenor
            if (curveName != null && marketName != null)
            {
                var identifier = PricingStructureIdentifier.ValidRateCurveIdentifier(marketName, curveName, stress);
                if (identifier == null)
                {
                    MarketEnvironmentHelper.ResolveRateCurveName(curveName, out string currency, out string tenor);
                    var curve = GetInterestRateCurveProperties(logger, cache, nameSpace, marketName, currency, tenor, stress);
                    return curve;
                }
                var result = cache.LoadItem<Market>(nameSpace + "." + identifier);
                return result.AppProps;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public static NamedValueSet GetInterestRateCurveProperties(ILogger logger, ICoreCache cache, string nameSpace, string marketName, string currency, string tenor, string stress)
        {
            IExpression curveTypeFilters = Expr.BoolOR(
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.RateCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.DiscountCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondDiscountCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.BondCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.RateSpreadCurve.ToString()),
                Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.RateBasisCurve.ToString()));

            IExpression currencyFilter = Expr.IsEQU(CurveProp.Currency1, currency);

            IExpression stressFilter = string.IsNullOrEmpty(stress)
                                               ? Expr.IsNull(CurveProp.StressName)
                                               : Expr.IsEQU(CurveProp.StressName, stress);

            // Filter by supplied tenor
            IExpression tenorFilter = string.IsNullOrEmpty(tenor) ? Expr.IsNull(CurveProp.IndexTenor) : Expr.IsEQU(CurveProp.IndexTenor, tenor);
            IExpression filter = Expr.BoolAND(
                Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace),
                curveTypeFilters,
                currencyFilter,
                stressFilter,
                tenorFilter);
            List<ICoreItem> results = cache.LoadItems<Market>(filter);
            if (results.Count == 0)
            {
                if (tenor != DefaultTenor)
                {
                    // try again for 3M tenor
                    tenorFilter = Expr.IsEQU(CurveProp.IndexTenor, DefaultTenor);
                    filter = Expr.BoolAND(
                        Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                        Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace),
                        curveTypeFilters,
                        currencyFilter,
                        stressFilter,
                        tenorFilter);
                    results = cache.LoadItems<Market>(filter);
                }
                else if (!curveTypeFilters.DisplayString().Contains(PricingStructureTypeEnum.XccySpreadCurve.ToString()))
                {
                    // try again for XCcySpreadCurve
                    var newCurveTypeFilters = Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.XccySpreadCurve.ToString());
                    filter = Expr.BoolAND(Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                        Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace),
                                              newCurveTypeFilters,
                                              currencyFilter,
                                              tenorFilter);
                    results = cache.LoadItems<Market>(filter);
                }
                //throw new ArgumentException(string.Format(NoCurvesFound, marketName, currency, tenor));
            }
            if (results.Count < 1)
            {
                return null;
                //throw new ArgumentException(string.Format(NoCurvesFound2, filter));
            }
            return results.Single().AppProps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="marketName"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <returns></returns>
        public static NamedValueSet GetFxCurveProperties(ILogger logger, ICoreCache cache, string nameSpace, string marketName, string currency1, string currency2)
        {
            if (currency1 == currency2) return null;
            if (currency1.Equals(CurrencyUsd, StringComparison.OrdinalIgnoreCase)
             || currency2.Equals(CurrencyUsd, StringComparison.OrdinalIgnoreCase))
            {
                NamedValueSet result = GetSingleCurveProperties(logger, cache, nameSpace, marketName, currency1, currency2);
                return result;
            }
            GetTwoCurvesProperties(logger, cache, nameSpace, marketName, currency1, currency2, out NamedValueSet result1, out NamedValueSet result2);
            NamedValueSet newProperties = null;
            if (result1 != null && result2 != null)
            {
                newProperties = new NamedValueSet();
                newProperties.Set(CurveProp.PricingStructureType, PricingStructureTypeEnum.FxCurve.ToString());
                newProperties.Set(CurveProp.Currency1, currency1);
                newProperties.Set(CurveProp.Currency2, currency2);
                newProperties.Set(CurveProp.MarketAndDate, marketName);
                newProperties.Set(EnvironmentProp.NameSpace, nameSpace);
                newProperties.Set(CurveProp.BaseDate, result1.GetValue<DateTime>(CurveProp.BaseDate));
            }
            return newProperties;
        }

        private static NamedValueSet GetSingleCurveProperties(ILogger logger, ICoreCache cache, string nameSpace, string marketName, string currency1, string currency2)
        {
            if (currency1 == currency2) return null;
            //Use the unique name for all markets.
            var curveId = PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, currency2, null, false);
            var properties = cache.LoadItem<Market>(nameSpace + "." + curveId);
            if (properties == null)
            {
                curveId = PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, currency2, null, true);
                properties = cache.LoadItem<Market>(nameSpace + "." + curveId);
            }
            logger.LogInfo("Properties retrieved.");
            return properties.AppProps;
        }

        private static void GetTwoCurvesProperties(ILogger logger, ICoreCache cache, string nameSpace, string marketName, string currency1, string currency2,
                          out NamedValueSet result1, out NamedValueSet result2)
        {
            //Note this is only called after the single curve call.
            result1 = null;
            result2 = null;
            // Try currency1 per USD
            var currencyId1 = PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, CurrencyUsd, null, false);
            var currencyCurve1 = cache.LoadItem<Market>(nameSpace + "." + currencyId1);
            // Try USD per currency1
            if (currencyCurve1 == null)
            {
                currencyId1 = PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency1, CurrencyUsd, null, true);
                currencyCurve1 = cache.LoadItem<Market>(nameSpace + "." + currencyId1);
                if (currencyCurve1 == null)
                {
                    logger.LogInfo(currencyId1 + " does not exist.");
                }
            }
            // Try currency2 per USD
            var currencyId2 = PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency2, CurrencyUsd, null, false);
            var currencyCurve2 = cache.LoadItem<Market>(nameSpace + "." + currencyId2);
            //Try USD per currency2
            if (currencyCurve2 == null)
            {
                currencyId2 = PricingStructureIdentifier.ValidFxCurveIdentifier(marketName, currency2, CurrencyUsd, null, true);
                currencyCurve2 = cache.LoadItem<Market>(nameSpace + "." + currencyId2);
                if (currencyCurve2 == null)
                {
                    logger.LogInfo(currencyId2 + " does not exist.");
                }
            }
            if (currencyCurve1 != null) result1 = currencyCurve1.AppProps;
            if (currencyCurve2 != null) result2 = currencyCurve2.AppProps;
            logger.LogInfo("Properties retrieved.");
        }

        #endregion
    }
}
