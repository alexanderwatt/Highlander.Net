/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using FpML.V5r3.Reporting;

namespace Orion.ValuationEngine.Helpers
{
    public static class InterestRateQuote
    {
        /// <summary>
        /// Creates the asset valuation.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="baseDate">The base date.</param>
        /// <returns></returns>
        public static AssetValuation CreateAssetValuation(string[] metrics, DateTime baseDate)
        {
            var av = new AssetValuation();
            var quotes = new Quotation[metrics.Length];
            int index = 0;
            foreach (string metric in metrics)
            {
                var quotation = new Quotation();
                var measureType = new AssetMeasureType {Value = metric};
                quotation.value = 0.0m;
                quotation.measureType = measureType;
                quotation.valuationDate = baseDate;
                quotation.valuationDateSpecified = true;
                quotes[index] = quotation;
                index++;
            }
            av.quote = quotes;
            return av;
        }

        /// <summary>
        /// Applies the conversion rate.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="valuations">The valuations.</param>
        /// <param name="valuationMetricsRequireConvert">The valuation metrics require convert.</param>
        /// <param name="conversionRate">The conversion rate.</param>
        public static void ApplyConversionRate<TEnumT>(AssetValuation[] valuations, TEnumT[] valuationMetricsRequireConvert, Decimal conversionRate)
        {
            foreach (AssetValuation valuation in valuations)
            {
                foreach (TEnumT metric in valuationMetricsRequireConvert)
                {
                    foreach (Quotation quotation in valuation.quote)
                    {
                        if (String.Compare(quotation.measureType.Value, metric.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            quotation.value = quotation.value / conversionRate;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the conversion rate.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rate">The rate.</param>
        /// <returns></returns>
        public static Decimal ApplyConversionRate(Decimal value, Decimal rate)
        {
            Decimal result = value;
            if (rate != 0)
                result = result * rate;

            return result;
        }
    }
}