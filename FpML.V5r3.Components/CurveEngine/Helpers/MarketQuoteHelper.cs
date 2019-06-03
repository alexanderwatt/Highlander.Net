using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.CurveEngine.Assets;

namespace Orion.CurveEngine.Helpers
{
    ///<summary>
    ///</summary>
    public static class MarketQuoteHelper
    {
        /// <summary>
        /// Maps from priceunits other than decimals, to a decimal for consumption primarily by a rate curve.
        /// </summary>
        /// <param name="rateQuotationType">The rate quotation type. eg MarketQuote, Spread or Volatility.</param>
        /// <param name="fromQuote">The basic quotation to be mapped from ie normalised.</param>
        /// <param name="convertToType">The type to convert to: only implemented for decimals.</param>
        /// <returns></returns>
        public static BasicQuotation NormaliseGeneralPriceUnits(String rateQuotationType, BasicQuotation fromQuote, String convertToType)
        {
            if (fromQuote.measureType.Value != "MarketQuote" || convertToType != "DecimalRate" && convertToType != "DecimalValue")
            {
                return BasicQuotationHelper.Clone(fromQuote);
            }
            switch (fromQuote.quoteUnits.Value)
            {
                case "IRFuturesPrice"://Format for this is 9500.
                    {
                        return BasicQuotationHelper.Create((100 - fromQuote.value) / 100, rateQuotationType, convertToType);
                    }
                case "Volatility"://Format for this is 7.00
                {
                    return BasicQuotationHelper.Create(fromQuote.value / 100, rateQuotationType, convertToType);
                }
                case "Rate"://Format for this is 7.00
                    {
                        return BasicQuotationHelper.Create(fromQuote.value / 100, rateQuotationType, convertToType);
                    }
                case "DecimalValue":
                case "DecimalVolatility":
                case "LognormalVolatility":
                case "NormalVolatility":
                case "DecimalRate"://Format is .07
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                case "DecimalSpread"://Format is .07
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                case "FuturesPrice"://Format is 95.00
                    {
                        return BasicQuotationHelper.Create((100 - fromQuote.value) / 100, rateQuotationType, convertToType);
                    }
                case "Premium":
                case "DirtyPrice":
                case "Price"://Format is .07
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                case "FowardValue"://Format is .07
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                case "FxRate"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                default:
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
            }
        }

        /// <summary>
        /// Maps from priceunits other than decimals, to a decimal for consumption primarily by a rate curve.
        /// </summary>
        /// <param name="fromQuote">The basic quotation to be mapped from ie normalised.</param>
        /// <param name="convertToType">The type to convert to: only implemented for decimals.</param>
        /// <returns></returns>
        public static BasicQuotation NormalisePriceUnits(BasicQuotation fromQuote, String convertToType)
        {
            const string rateQuotationType = PriceableSimpleRateAsset.RateQuotationType;
            if (fromQuote.measureType.Value != rateQuotationType || (convertToType != "DecimalRate" && convertToType != "DecimalValue"))
            {
                return BasicQuotationHelper.Clone(fromQuote);
            }
            switch (fromQuote.quoteUnits.Value)
            {
                case "IRFuturesPrice"://Format for this is 9500.
                    {
                        return BasicQuotationHelper.Create((100 - fromQuote.value) / 100, rateQuotationType, convertToType);
                    }
                case "Volatility"://Format for this is 7.00
                {
                    return BasicQuotationHelper.Create(fromQuote.value / 100, rateQuotationType, convertToType);
                }
                case "Rate"://Format for this is 7.00
                    {
                        return BasicQuotationHelper.Create(fromQuote.value / 100, rateQuotationType, convertToType);
                    }
                case "DecimalValue":             
                case "DecimalVolatility":
                case "LognormalVolatility":
                case "NormalVolatility":
                case "DecimalRate"://Format is .07
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                case "DecimalSpread"://Format is .07
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                case "FuturesPrice"://Format is 95.00
                    {
                        return BasicQuotationHelper.Create((100 - fromQuote.value) / 100, rateQuotationType, convertToType);
                    }
                case "Premium":
                case "DirtyPrice":
                case "Price"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                case "ForwardValue"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                case "FxRate"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
                default:
                    {
                        return BasicQuotationHelper.Create(fromQuote.value, rateQuotationType, convertToType);
                    }
            }
        }

        /// <summary>
        /// Maps from decimal priceunits to a other types, mainly for display purposes..
        /// </summary>
        /// <param name="toQuote">The basic quotation to be mapped to ie denormalised.</param>
        /// <param name="convertFromType">The type to convert from: only implemented for decimals.</param>
        /// <returns></returns>
        public static BasicQuotation DeNormalisePriceUnits(BasicQuotation toQuote, string convertFromType)
        {
            const string rateQuotationType = PriceableSimpleRateAsset.RateQuotationType;
            if (toQuote.measureType.Value != rateQuotationType || (convertFromType != "DecimalRate" && convertFromType != "DecimalValue"))
            {
                return BasicQuotationHelper.Clone(toQuote);
            }
            switch (toQuote.quoteUnits.Value)
            {
                case "IRFuturesPrice"://Format for this is 9500.
                    {
                        return BasicQuotationHelper.Create(100 - toQuote.value * 100, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "Volatility"://Format for this is 7.00
                {
                    return BasicQuotationHelper.Create(toQuote.value * 100, rateQuotationType, toQuote.quoteUnits.Value);
                }
                case "Rate"://Format for this is 7.00
                    {
                        return BasicQuotationHelper.Create(toQuote.value * 100, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "DecimalValue":
                case "DecimalVolatility":
                case "LognormalVolatility":
                case "NormalVolatility":
                case "DecimalRate"://Format is .07
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "DecimalSpread"://Format is .07
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "FuturesPrice"://Format is 95.00
                    {
                        return BasicQuotationHelper.Create(100 - toQuote.value * 100, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "Premium":
                case "DirtyPrice":
                case "Price"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "ForwardValue"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "FxRate"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                default:
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
            }
        }

        /// <summary>
        /// Maps from decimal priceunits to a other types, mainly for display purposes..
        /// </summary>
        /// <param name="rateQuotationType">The quotation type.</param>
        /// <param name="toQuote">The basic quotation to be mapped to ie denormalised.</param>
        /// <param name="convertFromType">The type to convert from: only implemented for decimals.</param>
        /// <returns></returns>
        public static BasicQuotation DeNormaliseGeneralPriceUnits(String rateQuotationType, BasicQuotation toQuote, string convertFromType)
        {
            if (toQuote.measureType.Value != "MarketQuote" || convertFromType != "DecimalRate" && convertFromType != "DecimalValue")
            {
                return BasicQuotationHelper.Clone(toQuote);
            }
            switch (toQuote.quoteUnits.Value)
            {
                case "IRFuturesPrice"://Format for this is 9500.
                    {
                        return BasicQuotationHelper.Create(100 - toQuote.value * 100, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "Volatility"://Format for this is 7.00
                {
                    return BasicQuotationHelper.Create(toQuote.value * 100, rateQuotationType, toQuote.quoteUnits.Value);
                }
                case "Rate"://Format for this is 7.00
                    {
                        return BasicQuotationHelper.Create(toQuote.value * 100, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "DecimalValue":
                case "DecimalVolatility":
                case "LognormalVolatility":
                case "NormalVolatility":
                case "DecimalRate"://Format is .07
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "DecimalSpread"://Format is .07
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "FuturesPrice"://Format is 95.00
                    {
                        return BasicQuotationHelper.Create(100 - toQuote.value * 100, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "Premium":
                case "DirtyPrice":
                case "Price"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "ForwardValue"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                case "FxRate"://Format is in units.
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
                default:
                    {
                        return BasicQuotationHelper.Create(toQuote.value, rateQuotationType, toQuote.quoteUnits.Value);
                    }
            }
        }

        /// <summary>
        /// Finds the spread, if it exists, and add to the market quote. Otherwise returns 0.
        /// </summary>
        /// <param name="measureType">Type of the measure.</param>
        /// <param name="quotations">The quotations.</param>
        /// <returns></returns>
        public static BasicQuotation GetMarketQuoteAndNormalise(string measureType, IList<BasicQuotation> quotations)
        {
            //Get the market quote.
            var normalisedValue = BasicQuotationHelper.Create(0.0m, measureType, "DecimalRate");
            var value = FindQuotationByMeasureType(measureType, quotations);
            if (value != null)
            {
                normalisedValue = NormalisePriceUnits(value, "DecimalRate");
            }
            return normalisedValue;
        }

        /// <summary>
        /// Finds the spread, if it exists, and add to the market quote. Otherwise returns 0.
        /// </summary>
        /// <param name="quotations">The quotations.</param>
        /// <returns></returns>
        public static BasicQuotation GetInverseMarketQuoteAddSpreadAndNormalise(IList<BasicQuotation> quotations)
        {
            //Get the market quote.
            var normalisedValue = BasicQuotationHelper.Create(0.0m, "MarketQuote", "DecimalRate");
            var value = FindQuotationByMeasureType("MarketQuote", quotations);
            if (value != null)
            {
                normalisedValue = NormalisePriceUnits(value, "DecimalRate");
                normalisedValue.value = -normalisedValue.value;
            }
            //Get the spread.
            var normalisedspread = BasicQuotationHelper.Create(0.0m, "Spread", "DecimalRate");
            var quote = FindQuotationByMeasureType("Spread", quotations);
            if (quote != null)
            {
                normalisedspread = NormalisePriceUnits(quote, "DecimalRate");
            }
            //return the sum.
            normalisedValue.value = normalisedValue.value + normalisedspread.value;
            return normalisedValue;
        }

        /// <summary>//TODO set the quotesideenum.
        /// Finds the spread, if it exists, and add to the market quote. Otherwise returns 0.
        /// </summary>
        /// <param name="quotations">The quotations.</param>
        /// <returns></returns>
        public static BasicQuotation GetMarketQuoteAddSpreadAndNormalise(IList<BasicQuotation> quotations)
        {
            //Get the market quote.
            var normalisedValue = BasicQuotationHelper.Create(0.0m, "MarketQuote", "DecimalRate");
            var value = FindQuotationByMeasureType("MarketQuote", quotations);
            if(value!=null)
            {
                normalisedValue = NormalisePriceUnits(value, "DecimalRate");
            }
            //Get the spread.
            var normalisedspread = BasicQuotationHelper.Create(0.0m, "Spread", "DecimalRate");
            var quote = FindQuotationByMeasureType("Spread", quotations);
            if(quote!=null)
            {
                normalisedspread = NormalisePriceUnits(quote, "DecimalRate");
            }
            //return the sum.
            normalisedValue.value = normalisedValue.value + normalisedspread.value;
            return normalisedValue;
        }

        /// <summary>//TODO set the quotesideenum.
        /// Finds the spread, if it exists, and add to the market quote. Otherwise returns 0.
        /// </summary>
        /// <param name="quotations">The quotations.</param>
        /// <returns></returns>
        public static BasicQuotation GetMarketQuotePriceAddSpreadAndNormalise(IList<BasicQuotation> quotations)
        {
            //Get the market quote.
            var normalisedValue = BasicQuotationHelper.Create(0.0m, "MarketQuote", "Price");
            var value = FindQuotationByMeasureType("MarketQuote", quotations);
            if (value != null)
            {
                normalisedValue = NormalisePriceUnits(value, "Price");
            }
            //Get the spread.
            var normalisedspread = BasicQuotationHelper.Create(0.0m, "Spread", "Price");
            var quote = FindQuotationByMeasureType("Spread", quotations);
            if (quote != null)
            {
                normalisedspread = NormalisePriceUnits(quote, "Price");
            }
            //return the sum.
            normalisedValue.value = normalisedValue.value + normalisedspread.value;
            return normalisedValue;
        }

        /// <summary>//TODO set the quotesideenum.
        /// Finds the spread, if it exists, and add to the market quote. Otherwise returns 0.
        /// </summary>
        /// <param name="quotations">The quotations.</param>
        /// <returns></returns>
        public static BasicQuotation[] MarketQuoteRemoveSpreadAndNormalise(List<BasicQuotation> quotations)
        {
            //Get the market quote.
            var normalisedValue = BasicQuotationHelper.Create(0.0m, "MarketQuote", "DecimalRate");
            var value = FindQuotationByMeasureType("MarketQuote", quotations);
            if (value != null)
            {
                normalisedValue = NormalisePriceUnits(value, "DecimalRate");
            }
            //Get the spread.
            var quote = FindQuotationByMeasureType("Spread", quotations);
            var spreadValue = 0.0m;
            if (quote != null)
            {
                //This is to remove evidence of the spread.
                BasicQuotation normalisedspread = NormalisePriceUnits(quote, "DecimalRate");
                spreadValue = normalisedspread.value;
                ReplaceQuotationByMeasureType("Spread", quotations, 0.0m);
            }          
            //return the sum.
            normalisedValue.value = normalisedValue.value + spreadValue;
            ReplaceQuotationByMeasureType("MarketQuote", quotations, normalisedValue.value);
            return quotations.ToArray();
        }

        /// <summary>
        /// Finds the type of the quotation by measure.
        /// </summary>
        /// <param name="measureType">Type of the measure.</param>
        /// <param name="quotations">The quotations.</param>
        /// <returns></returns>
        public static BasicQuotation FindQuotationByMeasureType(string measureType, IList<BasicQuotation> quotations)
        {
            var matchedQuotes = quotations.Where(q=>q.measureType.Value.Equals(measureType, StringComparison.OrdinalIgnoreCase));
            var basicQuotations = matchedQuotes as IList<BasicQuotation> ?? matchedQuotes.ToList();
            if (basicQuotations.Count > 1)
            {
                throw new ArgumentException($"More than one quote matched the supplied measureType '{measureType}'");
            }
            return basicQuotations.SingleOrDefault();
        }

        /// <summary>
        /// Replaces the type of the quotation by measure.
        /// </summary>
        /// <param name="measureType">Type of the measure.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="value">The value to replace.</param>
        /// <returns></returns>
        public static BasicQuotation[] ReplaceQuotationByMeasureType(string measureType, List<BasicQuotation> quotations, Decimal value)
        {
            var val = quotations.Find(quotationItem => String.Compare(quotationItem.measureType.Value, measureType, StringComparison.OrdinalIgnoreCase) == 0);
            if (val != null)
            {
                var idx =
                    quotations.FindIndex(
                        quotationItem => String.Compare(quotationItem.measureType.Value, measureType, StringComparison.OrdinalIgnoreCase) == 0);
                quotations[idx].value = value;
            }
            else
            {
                var quote = BasicQuotationHelper.Create(value, measureType, "DecimalRate");
                    quotations.Add(quote);
            }
            return quotations.ToArray();
        }

        /// <summary>
        /// Replaces the type of the quotation by measure.
        /// </summary>
        /// <param name="measureType">Type of the measure.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="value">The value to replace.</param>
        /// <returns></returns>
        public static BasicQuotation[] AddAndReplaceQuotationByMeasureType(string measureType, List<BasicQuotation> quotations, Decimal value)
        {
            var idx = quotations.FindIndex(quotationItem => String.Compare(quotationItem.measureType.Value, measureType, StringComparison.OrdinalIgnoreCase) == 0);
            var mid = quotations.Find(quotationItem => String.Compare(quotationItem.measureType.Value, measureType, StringComparison.OrdinalIgnoreCase) == 0);
            if (mid != null && mid.value!=0)
            {
                quotations[idx].value = mid.value + value;
            }
            return quotations.ToArray();
        }
    }
}