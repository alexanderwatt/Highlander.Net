using System;
using FpML.V5r10.Reporting;

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
        /// <param name="valautionMetricsRequireConvert">The valaution metrics require convert.</param>
        /// <param name="conversionRate">The conversion rate.</param>
        public static void ApplyConversionRate<TEnumT>(AssetValuation[] valuations, TEnumT[] valautionMetricsRequireConvert, Decimal conversionRate)
        {
            foreach (AssetValuation valuation in valuations)
            {
                foreach (TEnumT metric in valautionMetricsRequireConvert)
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