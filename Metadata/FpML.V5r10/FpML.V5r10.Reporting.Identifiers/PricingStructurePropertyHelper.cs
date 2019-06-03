using System;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The FxCurveIdentifier.
    /// </summary>
    public static class PricingStructurePropertyHelper
    {
        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="pricingStructureType"></param>
        public static NamedValueSet GeneralCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData, PricingStructureTypeEnum pricingStructureType)
        {
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.RateCurve:
                    return RateCurve(fpmlData);
                //case PricingStructureTypeEnum.RateSpreadCurve:
                //    break;
                //case PricingStructureTypeEnum.RateBasisCurve:
                //    break;
                case PricingStructureTypeEnum.DiscountCurve:
                    return DiscountCurve(fpmlData);
                case PricingStructureTypeEnum.InflationCurve:
                    return InflationCurve(fpmlData);
                case PricingStructureTypeEnum.FxCurve:
                    return FxCurve(fpmlData);
                case PricingStructureTypeEnum.FxVolatilityMatrix:
                    return FxVolatilityMatrix(fpmlData);
                //case PricingStructureTypeEnum.SurvivalProbabilityCurve:
                //    break;
                case PricingStructureTypeEnum.CommodityCurve:
                    return CommodityCurve(fpmlData);
                case PricingStructureTypeEnum.CommodityVolatilityMatrix:
                    return CommodityVolatilityMatrix(fpmlData);
                case PricingStructureTypeEnum.RateVolatilityMatrix:
                    return RateVolatilityMatrix(fpmlData);
                case PricingStructureTypeEnum.RateATMVolatilityMatrix:
                    return RateATMVolatilityMatrix(fpmlData);
                //case PricingStructureTypeEnum.RateVolatilityCube:
                //    return RateVolatilityCube(fpmlData);
                case PricingStructureTypeEnum.LPMCapFloorCurve:
                    return RateVolatilityMatrix(fpmlData);
                //case PricingStructureTypeEnum.LPMSwaptionCurve:
                //    break;
                //case PricingStructureTypeEnum.VolatilitySurface:
                //    break;
                //case PricingStructureTypeEnum.VolatilityCube:
                //    break;
                //case PricingStructureTypeEnum.VolatilitySurface2:
                //    break;
                case PricingStructureTypeEnum.EquityVolatilityMatrix:
                    return EquityVolatilityMatrix(fpmlData);
                //case PricingStructureTypeEnum.EquityWingVolatilityMatrix:
                //    return EquityWingVolatilityMatrix(fpmlData);
                default:
                    throw new ArgumentOutOfRangeException(nameof(pricingStructureType));
            }
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet RateCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (YieldCurveValuation)fpmlData.Second;
            var curve = (YieldCurve)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneySwapDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, CurveProp.IndexName, CurveProp.IndexTenor,
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var indexName = curve.forecastRateIndex.floatingRateIndex.Value;
            var indexTenor = curve.forecastRateIndex.indexTenor.ToString();
            var values = new object[]
                             {
                                 curve.id, curveName, curve.algorithm, "RateCurve",
                                 indexName, indexTenor, buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet DiscountCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (YieldCurveValuation)fpmlData.Second;
            var curve = (YieldCurve)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneySwapDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var curveData = curveName.Split('-');
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "CreditInstrumentId", "CreditInstrumentId",
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var values = new object[]
                             {
                                 curve.id, curveName, curve.algorithm, "DiscountCurve",
                                 curveData[1], curveData[2], buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet InflationCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var yieldCurveValuation = (YieldCurveValuation)fpmlData.Second;
            var curve = (YieldCurve)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            if (string.IsNullOrEmpty(curve.name))
            {
                curve.name = "AUD-Dummy-SydneySwapDesk";
            }
            NamedValueSet properties = new NamedValueSet();
            properties.Set(CurveProp.UniqueIdentifier, curve.id);
            properties.Set(CurveProp.CurveName, curve.name);
            properties.Set(CurveProp.Algorithm, curve.algorithm);
            properties.Set(CurveProp.PricingStructureType, PricingStructureTypeEnum.InflationCurve.ToString());
            properties.Set(CurveProp.IndexName, curve.forecastRateIndex.floatingRateIndex.Value);
            properties.Set(CurveProp.IndexTenor, curve.forecastRateIndex.indexTenor.ToString());
            properties.Set(CurveProp.BuildDateTime, yieldCurveValuation.buildDateTime);
            properties.Set(CurveProp.BaseDate, yieldCurveValuation.baseDate.Value);
            properties.Set(CurveProp.Currency1, curve.currency.Value);
            properties.Set(CurveProp.Tolerance, 0.0000001d);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstructure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet VolatilityCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (VolatilityMatrix)fpmlData.Second;
            var curve = (VolatilityRepresentation)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneySwapDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
            {
                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "Instrument", CurveProp.IndexName, CurveProp.IndexTenor,
                "BuildDateTime", "BaseDate", "Currency"
            };
            var instrument = curve.asset.href;
            String indexTenor = null;
            String indexName = null;
            var temp = instrument.Split('-');
            if (temp.Length > 1)
            {
                indexTenor = temp[temp.Length - 1];
                indexName = string.Join("-", temp, 0, temp.Length - 1);
            }
            var values = new object[]
            {
                curve.id, curveName, "Default", PricingStructureTypeEnum.CapVolatilityCurve.ToString(),
                instrument, indexName, indexTenor, buildDateTime, baseDate, currency
            };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet ExpiryStrikeSurface(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (VolatilityMatrix)fpmlData.Second;
            var curve = (VolatilityRepresentation)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneySwapDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "Instrument",
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var values = new object[]
                             {
                                 curve.id, curveName, "Linear", "RateVolatilityMatrix",
                                 curve.asset.href, buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;

        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet ExpiryTenorSurface(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (VolatilityMatrix)fpmlData.Second;
            var curve = (VolatilityRepresentation)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneySwapDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "Instrument",
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var values = new object[]
                             {
                                 curve.id, curveName, "Linear", "RateATMVolatilityMatrix",
                                 curve.asset.href, buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet RateVolatilityMatrix(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (VolatilityMatrix)fpmlData.Second;
            var curve = (VolatilityRepresentation)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneySwapDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "Instrument",
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var values = new object[]
                             {
                                 curve.id, curveName, "Linear", "RateVolatilityMatrix",
                                 curve.asset.href, buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet EquityVolatilityMatrix(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (VolatilityMatrix)fpmlData.Second;
            var curve = (VolatilityRepresentation)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneyFxDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "Instrument",
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var values = new object[]
                             {
                                 curve.id, curveName, "Linear", "EquityVolatilityMatrix",
                                 curve.asset.href, buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet FxVolatilityMatrix(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (VolatilityMatrix)fpmlData.Second;
            var curve = (VolatilityRepresentation)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneyFxDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "Instrument",
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var values = new object[]
                             {
                                 curve.id, curveName, "Linear", "FxVolatilityMatrix",
                                 curve.asset.href, buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet CommodityVolatilityMatrix(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (VolatilityMatrix)fpmlData.Second;
            var curve = (VolatilityRepresentation)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneyCommodityDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "Instrument",
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var values = new object[]
                             {
                                 curve.id, curveName, "Linear", "CommodityVolatilityMatrix",
                                 curve.asset.href, buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet FxCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var valuation = (FxCurveValuation)fpmlData.Second;
            var curve = (FxCurve)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var names = new[] { "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, CurveProp.CurrencyPair, "QuoteBasis", "BuildDateTime", "BaseDate", "Currency" };
            var currencyPair = curve.quotedCurrencyPair.currency1.Value + "-" + curve.quotedCurrencyPair.currency2.Value;
            var quoteBasis = curve.quotedCurrencyPair.quoteBasis.ToString();
            var buildDateTime = valuation.buildDateTime;
            var curveName = "AUD-Dummy-SydneyFxDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = valuation.baseDate.Value;
            var currency = curve.currency.Value;
            //Get the values.
            var values = new object[]
                             {
                                 curve.id, curveName, "LinearForward", "FxCurve", 
                                 currencyPair, quoteBasis, buildDateTime, baseDate, currency
                             };
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet CommodityCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (FxCurveValuation)fpmlData.Second;
            var curve = (FxCurve)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var names = new[] { "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "CommodityAsset", 
                                "BuildDateTime", "BaseDate", "Currency" };
            var commodityAsset = curve.name.Split('-')[1];
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = "AUD-Dummy-SydneyCommodityDesk";
            if (curve.name != null)
            {
                curveName = curve.name;
            }
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            //Get the values.
            var values = new object[]
                             {
                                 curve.id, curveName, "LinearForward", "CommodityCurve",
                                 commodityAsset, buildDateTime, baseDate, currency
                             };
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }

        /// <summary>
        /// Returns properties from the pricingstrucutre. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        public static NamedValueSet RateATMVolatilityMatrix(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var tempFpML = (VolatilityMatrix)fpmlData.Second;
            var curve = (VolatilityRepresentation)fpmlData.First;
            //Creates the property collection. This should be backward compatable with V1.
            var buildDateTime = tempFpML.buildDateTime;
            var curveName = curve.name;
            var baseDate = tempFpML.baseDate.Value;
            var currency = curve.currency.Value;
            var names = new[]
                            {
                                "Identifier", CurveProp.CurveName, "Algorithm", CurveProp.PricingStructureType, "Instrument",
                                "BuildDateTime", "BaseDate", "Currency"
                            };
            var values = new object[]
                             {
                                 curve.id, curveName, "Linear", "RateATMVolatilityMatrix",
                                 curve.asset.href, buildDateTime, baseDate, currency
                             };
            //Get the values.
            var properties = NamedValueSetHelper.Build(names, values);
            return properties;
        }
    }
}