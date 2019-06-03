using System;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Constants;

namespace Orion.Identifiers
{
    /// <summary>
    /// PricingStructureIdHelper
    /// </summary>
    public class PricingStructureIdHelper
    {
        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static DateTime? ExtractExpiryTime(NamedValueSet propertyCollection)
        {
            DateTime? expiryTime = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("ExpiryTime"))
            {
                expiryTime = propertyCollection.Get("ExpiryTime").AsValue<DateTime>();
            }
            return expiryTime;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static DateTime? ExtractTime(NamedValueSet propertyCollection)
        {
            DateTime? time = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("Time"))
            {
                time = propertyCollection.Get("Time").AsValue<DateTime>();
            }
            return time;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static DateTime? ExtractValuationDate(NamedValueSet propertyCollection)
        {
            DateTime? valuationDate = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("ValuationDate"))
            {
                valuationDate = propertyCollection.Get("ValuationDate").AsValue<DateTime>();
            }
            return valuationDate;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static QuoteTiming ExtractQuoteTiming(NamedValueSet propertyCollection)
        {
            QuoteTiming quoteTiming = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("QuoteTiming"))
            {
                var result = propertyCollection.Get("QuoteTiming").AsValue<string>();
                quoteTiming = new QuoteTiming { Value = result };
            }
            return quoteTiming;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static AssetMeasureType ExtractMeasureType(NamedValueSet propertyCollection)
        {
            AssetMeasureType measureType = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("MeasureType"))
            {
                var result = propertyCollection.Get("MeasureType").AsValue<string>();
                measureType = new AssetMeasureType { Value = result };
            }
            return measureType;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static PriceQuoteUnits ExtractQuoteUnits(NamedValueSet propertyCollection)
        {
            PriceQuoteUnits priceQuoteUnits = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("QuoteUnits"))
            {
                var result = propertyCollection.Get("QuoteUnits").AsValue<string>();
                priceQuoteUnits = new PriceQuoteUnits { Value = result };
            }
            return priceQuoteUnits;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static PriceQuoteUnits ExtractStrikeQuoteUnits(NamedValueSet propertyCollection)
        {
            var priceQuoteUnits = new PriceQuoteUnits { Value = "Absolute" };
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("StrikeQuoteUnits"))
            {
                var result = propertyCollection.Get("StrikeQuoteUnits").AsValue<string>();
                priceQuoteUnits.Value = result;
            }
            return priceQuoteUnits;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static CashflowType ExtractCashflowType(NamedValueSet propertyCollection)
        {
            CashflowType cashflowType = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("CashflowType"))
            {
                var result = propertyCollection.Get("CashflowType").AsValue<string>();
                cashflowType = new CashflowType { Value = result };
            }
            return cashflowType;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static QuotationSideEnum? ExtractSide(NamedValueSet propertyCollection)
        {
            QuotationSideEnum? quotationSideEnum = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("Side"))
            {
                var result = propertyCollection.Get("Side").AsValue<string>();
                quotationSideEnum = (QuotationSideEnum)Enum.Parse(typeof(QuotationSideEnum), result, true);
            }
            return quotationSideEnum;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static InformationSource[] ExtractInformationSources(NamedValueSet propertyCollection)
        {
            InformationSource[] informationSource = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("InformationSource"))
            {
                var result = propertyCollection.Get("InformationSource").AsValue<string>();
                informationSource = new InformationSource[1];
                informationSource[0] = InformationSourceHelper.Create(result);
            }
            return informationSource;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static BusinessCenters ExtractBusinessCenters(NamedValueSet propertyCollection)
        {
            BusinessCenters businessCentres = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("BusinessCenters"))
            {
                var centres = propertyCollection.Get("BusinessCenters").AsValue<string[]>();
                businessCentres = BusinessCentersHelper.Parse(centres);
            }
            return businessCentres;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static BusinessCenter ExtractBusinessCenter(NamedValueSet propertyCollection)
        {
            BusinessCenter businessCenter = null;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("BusinessCenter"))
            {
                var center = propertyCollection.Get("BusinessCenter").AsValue<string>();
                businessCenter = new BusinessCenter { Value = center };
            }
            return businessCenter;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static BusinessDayConventionEnum ExtractBusinessDayConvention(NamedValueSet propertyCollection)
        {
            var businessDayConvention = BusinessDayConventionEnum.MODFOLLOWING;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("BusinessDayConvention"))
            {
                var convention = propertyCollection.Get("BusinessDayConvention").AsValue<string>();
                businessDayConvention =
                    (BusinessDayConventionEnum)Enum.Parse(typeof(BusinessDayConventionEnum), convention, true);
            }
            return businessDayConvention;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static BusinessDayAdjustments ExtractBusinessDayAdjustments(NamedValueSet propertyCollection)
        {
            var businessDayConvention = ExtractBusinessDayConvention(propertyCollection);
            var businessCenters = ExtractBusinessCenters(propertyCollection);
            var businessDayAdjustments = new BusinessDayAdjustments
                                             {
                                                 businessDayConvention = businessDayConvention,
                                                 businessCenters = businessCenters
                                             };

            return businessDayAdjustments;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractCommodityAsset(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();
            string commodityAsset = "Uknown";
            if (dictionaryKeys.ContainsKey("CommodityAsset"))
            {
                commodityAsset = propertyCollection.Get("CommodityAsset").AsValue<string>();
            }
            else
            {
                if (dictionaryKeys.ContainsKey(CurveProp.CurveName))
                {
                    var curveName = ExtractCurveName(propertyCollection);
                    commodityAsset = curveName.Split('-')[1];
                }
            }

            return commodityAsset;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static QuotedCurrencyPair ExtractQuotedCurrencyPair(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();
            
            string currencyPair = "AUD-USD";

            if (dictionaryKeys.ContainsKey(CurveProp.CurrencyPair))
            {
                currencyPair = propertyCollection.Get(CurveProp.CurrencyPair).AsValue<string>();
            }
            else
            {
                if (dictionaryKeys.ContainsKey(CurveProp.CurveName))
                {
                    var curveName = ExtractCurveName(propertyCollection);
                    
                    try
                    {
                        currencyPair = curveName;

                    }
                    catch (System.Exception)
                    {
                        throw new System.Exception("CurveName property is not valid.");
                    }

                }
            }
            var pair = currencyPair.Split('-');

            var quoteBasis = ExtractQuoteBasis(propertyCollection);

            return QuotedCurrencyPair.Create(pair[0], pair[1], quoteBasis);
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static QuoteBasisEnum ExtractQuoteBasis(NamedValueSet propertyCollection)
        {
            var quoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("QuoteBasis"))
            {
                var quoteBasisString = propertyCollection.Get("QuoteBasis").AsValue<string>();
                quoteBasis =
                    (QuoteBasisEnum)Enum.Parse(typeof(QuoteBasisEnum), quoteBasisString, true);
            }
            return quoteBasis;
        }
       
        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractCurveName(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();
            
            if (dictionaryKeys.ContainsKey(CurveProp.CurveName))
            {
                return propertyCollection.Get(CurveProp.CurveName).AsValue<string>();
            }

            throw new System.Exception("Mandatory 'CurveName' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static PricingStructureTypeEnum ExtractPricingStructureType(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey(CurveProp.PricingStructureType))
            {
                var pricingStructureTypeAsString = propertyCollection.Get(CurveProp.PricingStructureType).AsValue<string>();
                return (PricingStructureTypeEnum)Enum.Parse(typeof(PricingStructureTypeEnum), pricingStructureTypeAsString, true);
            }

            throw new System.Exception("Mandatory 'PricingStructureType' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static DateTime ExtractBuildDateTime(NamedValueSet propertyCollection)
        {
            var buildDateTime = new DateTime();
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("BuildDateTime"))
            {
                buildDateTime = propertyCollection.Get("BuildDateTime").AsValue<DateTime>();
            }
            else
            {
                if (dictionaryKeys.ContainsKey(CurveProp.CurveName))
                {
                    var curveName = ExtractCurveName(propertyCollection);
                    buildDateTime = DateTime.Parse(curveName.Split('-')[2]);
                }
            }
            return buildDateTime;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractIdentifier(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey("Identifier"))
            {
                return propertyCollection.Get("Identifier").AsValue<string>();
            }

            throw new System.Exception("Mandatory 'Identifier' property has not been specified. Please specify.");
        }
        
        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractAlgorithm(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey("Algorithm"))
            {
                return propertyCollection.Get("Algorithm").AsValue<string>();
            }

            throw new System.Exception("Mandatory 'Algorithm' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractSource(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey("Source"))
            {
                return propertyCollection.Get("Source").AsValue<string>();
            }

            throw new System.Exception("Mandatory 'Source' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractInstrument(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();
            
            if (dictionaryKeys.ContainsKey("Instrument"))
            {
                return propertyCollection.Get("Instrument").AsValue<string>();
            }

            throw new System.Exception("Mandatory 'Instrument' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractCurrency(NamedValueSet propertyCollection)
        {
            var currency = "AUD";
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("Currency"))
            {
                currency = propertyCollection.Get("Currency").AsValue<string>();
            }
            return currency;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static Boolean ExtractLPMCopy(NamedValueSet propertyCollection)
        {
            var lpmCopy = false;
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey("LPMCopy"))
            {
                lpmCopy = propertyCollection.Get("LPMCopy").AsValue<Boolean>();
            }

            return lpmCopy;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractMarketEnvironment(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey("MarketEnvironment"))
            {
                return propertyCollection.Get("MarketEnvironment").AsValue<string>();
            }

            throw new System.Exception("Mandatory 'MarketEnvironment' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractIndexName(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey(CurveProp.IndexName))
            {
                return propertyCollection.Get(CurveProp.IndexName).AsValue<string>();
            }

            throw new System.Exception("Mandatory 'IndexName' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractIndexTenor(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey(CurveProp.IndexTenor))
            {
                return propertyCollection.Get(CurveProp.IndexTenor).AsValue<string>();
            }

            throw new System.Exception("Mandatory 'IndexTenor' property has not been specified. Please specify.");
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractCreditSeniority(NamedValueSet propertyCollection)
        {
            var creditSeniority = "Unknown";
            var dictionaryKeys = propertyCollection.ToDictionary();

            if (dictionaryKeys.ContainsKey("CreditSeniority"))
            {
                creditSeniority = propertyCollection.Get("CreditSeniority").AsValue<string>();
            }

            var pst = ExtractPricingStructureType(propertyCollection).ToString();
            if(pst=="DiscountCurve")
            {
                var curveName = ExtractCurveName(propertyCollection);
                creditSeniority = curveName.Split('-')[2];
            }
            return creditSeniority;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractCreditInstrumentId(NamedValueSet propertyCollection)
        {
            var creditInstrumentId = "Unknown";
            var dictionaryKeys = propertyCollection.ToDictionary();
            if (dictionaryKeys.ContainsKey("CreditInstrumentId"))
            {
                creditInstrumentId = propertyCollection.Get("CreditInstrumentId").AsValue<string>();
            }
            var pst = ExtractPricingStructureType(propertyCollection).ToString();
            if (pst == "DiscountCurve")
            {
                var curveName = ExtractCurveName(propertyCollection);
                creditInstrumentId = curveName.Split('-')[1];
            }
            return creditInstrumentId;
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static string ExtractInflationLag(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();
            
            if (dictionaryKeys.ContainsKey("InflationLag"))
            {
                return propertyCollection.Get("InflationLag").AsValue<string>();
            }

            //throw new Exception("Mandatory 'InflationLag' property has not been specified. Please specify.");
            return "Unknown";
        }

        /// <summary>
        /// A helper to extract properties from a namedvalueset..
        /// </summary>
        /// <param name="propertyCollection">The collection of properties.</param>
        public static DateTime ExtractBaseDate(NamedValueSet propertyCollection)
        {
            var dictionaryKeys = propertyCollection.ToDictionary();
            DateTime baseDate = dictionaryKeys.ContainsKey(CurveProp.BaseDate) ? propertyCollection.Get(CurveProp.BaseDate).AsValue<DateTime>() : ExtractBuildDateTime(propertyCollection);
            return baseDate;
        }
    }
}