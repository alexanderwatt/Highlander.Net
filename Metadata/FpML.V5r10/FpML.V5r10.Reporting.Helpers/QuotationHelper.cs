using System;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r10.Reporting.Helpers
{
    public static class QuotationHelper
    {
        public  static  bool    AreEqual(Quotation quotation1, Quotation quotation2)
        {
            if (quotation1.value != quotation2.value)
            {
                return false;
            }
            return quotation1.quoteUnits.Value == quotation2.quoteUnits.Value;
        }

        public static Quotation Create(decimal value)
        {
            return Create(value, DateTime.Today, "AUSY", "AUD", "Bloomberg", "Bloomberg", "BBSW", "MarketQuote", "Rate");
        }

        public static Quotation Copy(Quotation baseQuotation)
        {
            Quotation quotation = null;
            if (baseQuotation != null)
            {
                quotation = new Quotation();
                if (baseQuotation.businessCenter!=null)
                {
                    quotation.businessCenter = BusinessCentersHelper.Parse(baseQuotation.businessCenter.Value).businessCenter[0];
                }
                if(baseQuotation.currency!=null)
                {
                    quotation.currency = CurrencyHelper.Parse(baseQuotation.currency.Value);
                }
                if(baseQuotation.valueSpecified)
                {
                    quotation.value = baseQuotation.value;
                    quotation.valueSpecified = true;
                }
                if(baseQuotation.valuationDateSpecified)
                {
                    quotation.valuationDate = baseQuotation.valuationDate; 
                    quotation.valuationDateSpecified = true;
                }
                if (baseQuotation.expiryTimeSpecified)
                {
                    quotation.expiryTime = baseQuotation.expiryTime;
                    quotation.expiryTimeSpecified = true;
                }
                if (baseQuotation.sideSpecified)
                {
                    quotation.side = baseQuotation.side;
                    quotation.sideSpecified = true;
                }
                if (baseQuotation.timeSpecified)
                {
                    quotation.time = baseQuotation.time;
                    quotation.timeSpecified = true;
                }
                if (baseQuotation.sensitivitySet != null)
                {
                    quotation.sensitivitySet = SensitivitySetHelper.Copy(baseQuotation.sensitivitySet);
                }
                if(baseQuotation.measureType!=null)
                {
                    quotation.measureType = AssetMeasureTypeHelper.Copy(baseQuotation.measureType);
                }
                if(baseQuotation.quoteUnits!=null)
                {
                    quotation.quoteUnits = PriceQuoteUnitsHelper.Copy(baseQuotation.quoteUnits);
                }
                if (baseQuotation.informationSource != null)
                {
                    quotation.informationSource = InformationSourceHelper.Copy(baseQuotation.informationSource).ToArray();
                }
                if (baseQuotation.exchangeId!=null)
                {
                    quotation.exchangeId = ExchangeIdHelper.Copy(baseQuotation.exchangeId);
                }
            }
            return quotation;
        }

        public static List<Quotation> Copy(List<Quotation> baseQuotations)
        {
            List<Quotation> quotations = null;
            if (baseQuotations!=null)
            {
                quotations = baseQuotations.Select(Copy).ToList();
            }
            return quotations;
        }

        public static List<Quotation> Copy(Quotation[] baseQuotations)
        {
            List<Quotation> quotations = null;
            if (baseQuotations != null)
            {
                quotations = baseQuotations.Select(Copy).ToList();
            }
            return quotations;
        }

        public static Quotation Create(decimal value, DateTime valuationDate,
            string businessCenter, string currency,
            string rateSourceProvider, string rateSource, string rateSourcePage,
            string assetMeasureType, string quoteUnits)
        {
            var quotation = new Quotation
                                      {
                                          businessCenter = BusinessCentersHelper.Parse(businessCenter).businessCenter[0],
                                          currency = CurrencyHelper.Parse(currency),
                                          informationSource =
                                              InformationSourceHelper.Create(
                                              InformationSourceHelper.Create(rateSourceProvider, rateSource,
                                                                             rateSourcePage)),
                                          measureType = AssetMeasureTypeHelper.Parse(assetMeasureType),
                                          quoteUnits = PriceQuoteUnitsHelper.Create(quoteUnits),
                                          valuationDate = valuationDate,
                                          valuationDateSpecified = true,
                                          value = value,
                                          valueSpecified = true
                                      };

            return quotation;
        }

        public static Quotation Create(decimal parRate, string measureType)
        {
            var quotation = new Quotation
            {
                measureType = new AssetMeasureType { Value = measureType },
                value = parRate,
                valueSpecified = true
            };

            return quotation;
        }

        public static Quotation Create(decimal parRate, string measureType, string priceQuoteUnits)
        {
            var quotation = new Quotation
            {
                quoteUnits = new PriceQuoteUnits { Value = priceQuoteUnits },
                measureType = new AssetMeasureType { Value = measureType },
                value = parRate,
                valueSpecified = true
            };

            return quotation;
        }

        public static Quotation Create(decimal parRate, string measureType, string priceQuoteUnits, DateTime valuationDate)
        {
            var quotation = new Quotation
            {
                quoteUnits = new PriceQuoteUnits { Value = priceQuoteUnits },
                measureType = new AssetMeasureType { Value = measureType },
                value = parRate,
                valueSpecified = true,
                valuationDate = valuationDate,
                valuationDateSpecified = true
            };

            return quotation;
        }

        public static Quotation CreateDepositRate(decimal parRate)
        {
            var quotation = new Quotation
                                      {
                                          quoteUnits = new PriceQuoteUnits {Value = "Rate"},
                                          measureType = new AssetMeasureType {Value = "MarketQuote"},
                                          value = parRate,
                                          valueSpecified = true
                                      };

            return quotation;
        }

        public static Quotation CreateFuturesPrices(decimal futuresPrice)
        {
            var quotation = new Quotation
                                      {
                                          quoteUnits = new PriceQuoteUnits {Value = "IRFuturesPrice"},
                                          measureType = new AssetMeasureType {Value = "MarketQuote"},
                                          value = futuresPrice,
                                          valueSpecified = true
                                      };

            return quotation;
        }
    }
}