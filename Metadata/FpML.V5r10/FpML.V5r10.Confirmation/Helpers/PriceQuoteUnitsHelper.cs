using System;
using FpML.V5r3.Codes;

namespace FpML.V5r3.Confirmation
{
    public static class PriceQuoteUnitsHelper
    {
        public static PriceQuoteUnits Parse(string quoteUnitsAsString)
        {
            var quoteUnitsEnum = PriceQuoteUnitsEnum.Undefined;
            if (quoteUnitsAsString != null)
                quoteUnitsEnum = PriceQuoteUnitsScheme.ParseEnumString(quoteUnitsAsString);
            return new PriceQuoteUnits { Value = quoteUnitsEnum.ToString() };
        }

        public static PriceQuoteUnits Create(string quoteUnitsAsString)
        {
            return new PriceQuoteUnits
                       {
                Value = quoteUnitsAsString
            };
        }

        public static PriceQuoteUnits Copy(PriceQuoteUnits quoteUnits)
        {
            return new PriceQuoteUnits
            {
                Value = quoteUnits.Value
            };
        }

        public static PriceQuoteUnits Create(PriceQuoteUnitsEnum quoteUnits)
        {
            return new PriceQuoteUnits
                       {
                Value = quoteUnits.ToString()
            };
        }

        /// <summary>
        /// Converts the price quote value between source/target units.
        /// </summary>
        /// <param name="sourceUnits">The source units.</param>
        /// <param name="targetUnits">The target units.</param>
        /// <param name="sourceValue">The source value.</param>
        /// <returns></returns>
        public static decimal ConvertPriceQuoteUnitsValue(PriceQuoteUnitsEnum sourceUnits, PriceQuoteUnitsEnum targetUnits, decimal sourceValue)
        {
            if (targetUnits == sourceUnits)
            {
                // no conversion required
                return sourceValue;
            }

            // convert source value to normalised value
            decimal normalValue = sourceValue;
            switch (sourceUnits)
            {
                // normalised units that do not need conversion
                case PriceQuoteUnitsEnum.DecimalRate:
                case PriceQuoteUnitsEnum.DecimalSpread:
                    break;
                // other units that require conversion
                case PriceQuoteUnitsEnum.InverseDecimalRate: // to DecimalRate
                    normalValue = (1.0M / sourceValue);
                    break;
                case PriceQuoteUnitsEnum.RateSpread: // to DecimalSpread
                    normalValue = (sourceValue / 10000.0M);
                    break;
                case PriceQuoteUnitsEnum.Rate: // to DecimalRate
                    normalValue = (sourceValue / 100.0M);
                    break;
                case PriceQuoteUnitsEnum.IRFuturesPrice: // to DecimalRate
                    normalValue = ((100.0M - sourceValue) / 100.0M);
                    break;
                default: // conversion not supported/implemented
                    throw new NotSupportedException(String.Format("Cannot convert value ({0}) from units ({1})", sourceValue, sourceUnits));
            }

            // convert normalised value to target value
            decimal targetValue = normalValue;
            switch (targetUnits)
            {
                // normalised units that do not need conversion
                case PriceQuoteUnitsEnum.DecimalRate:
                case PriceQuoteUnitsEnum.DecimalSpread:
                    break;
                // other units that require conversion
                case PriceQuoteUnitsEnum.InverseDecimalRate: // from DecimalRate
                    targetValue = (1.0M / normalValue);
                    break;
                case PriceQuoteUnitsEnum.RateSpread: // from DecimalSpread
                    normalValue = (normalValue * 10000.0M);
                    break;
                case PriceQuoteUnitsEnum.Rate: // from DecimalRate
                    targetValue = (normalValue * 100.0M);
                    break;
                case PriceQuoteUnitsEnum.IRFuturesPrice: // from DecimalRate
                    targetValue = (100.0M - (normalValue * 100.0M));
                    break;
                default: // conversion not supported/implemented
                    throw new NotSupportedException(String.Format("Cannot convert value ({0}) to units ({1})", normalValue, targetUnits));
            }
            return targetValue;
        }
    }
}