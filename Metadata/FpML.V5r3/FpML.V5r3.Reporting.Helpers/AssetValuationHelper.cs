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

#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public static class AssetValuationHelper
    {
        public static AssetValuation Copy(AssetValuation baseValuation)
        {
            AssetValuation result = null;
            if (baseValuation!=null)
            {
                result = new AssetValuation();
                if (baseValuation.quote!=null)
                {
                    result.quote = QuotationHelper.Copy(baseValuation.quote).ToArray();
                }
                if (baseValuation.fxRate!=null)
                {
                    var fxRate = new List<FxRate>();
                    foreach (var rate in baseValuation.fxRate)
                    {
                        var newRate = new FxRate
                            {
                                quotedCurrencyPair = new QuotedCurrencyPair
                                    {
                                        currency1 =
                                            CurrencyHelper.Copy(
                                                rate.quotedCurrencyPair.
                                                        currency1),
                                        currency2 =
                                            CurrencyHelper.Copy(
                                                rate.quotedCurrencyPair.
                                                        currency2),
                                        quoteBasis =
                                            rate.quotedCurrencyPair.
                                                    quoteBasis
                                    }
                            };
                        fxRate.Add(newRate);
                    }
                    result.fxRate = fxRate.ToArray();
                }
                if(baseValuation.valuationScenarioReference!=null)
                {
                    result.valuationScenarioReference = new ValuationScenarioReference
                                                            {href = baseValuation.valuationScenarioReference.href};
                }
                if(baseValuation.id != null)
                {
                    result.id = baseValuation.id;
                }
                if(baseValuation.objectReference != null)
                {
                    result.objectReference = new AnyAssetReference {href = baseValuation.objectReference.href};
                }
            }
            return result;
        }

        public static List<AssetValuation> Copy(List<AssetValuation> baseValuations)
        {
            List<AssetValuation> result = null;
            if (baseValuations != null)
            {
                result = baseValuations.Select(Copy).ToList();
            }
            return result;
        }

        public static AssetValuation Create(params Quotation[] quotations)
        {
            var result = new AssetValuation { quote = quotations };
            return result;
        }

        /// <summary>
        /// Creates the specified Basic asset valuation with a reference and set of quotations
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="quotations">The quotations.</param>
        /// <returns></returns>
        public static AssetValuation Create(string reference, params Quotation[] quotations)
        {
            AssetValuation result = Create(quotations);
            result.objectReference = new AnyAssetReference { href = reference };
            return result;
        }

        public static Sensitivity GetSensitivityByName(SensitivitySet sensitivitySet, string name)
        {
            return sensitivitySet.sensitivity.FirstOrDefault(basicQuotation => name == basicQuotation.name);
        }

        public static Quotation GetQuotationByMeasureType(AssetValuation assetValuation, string measureType)
        {
            return assetValuation.quote.FirstOrDefault(basicQuotation => measureType == basicQuotation.measureType.Value);
        }

        public static Quotation GetQuotationByTiming(AssetValuation basicAssetValuation, string timing)
        {
            return basicAssetValuation.quote.FirstOrDefault(basicQuotation => timing == basicQuotation.timing.Value);
        }

        public static AssetValuation Sum(List<AssetValuation> assetValuationList)
        {
            if (0 == assetValuationList.Count)
            {
                throw new ArgumentException("basicAssetValuationList is empty");
            }
            if (1 == assetValuationList.Count)
            {
                return Copy(assetValuationList[0]);//BinarySerializerHelper.Clone(assetValuationList[0]);//TODO problem with the binary serializer
            }
            // clone collection internally - just to keep invariant of the method.
            //  
            List<AssetValuation> clonedCollection = Copy(assetValuationList);//BinarySerializerHelper.Clone(assetValuationList);//TODO problem with the binary serializer
            AssetValuation firstElement = clonedCollection[0];
            clonedCollection.RemoveAt(0);
            AssetValuation sumOfTheTail = Sum(clonedCollection);
            return Add(firstElement, sumOfTheTail);
        }

        public static AssetValuation Add(AssetValuation assetValuation1, AssetValuation assetValuation2)
        {
            AssetValuation result = Copy(assetValuation1);//BinarySerializerHelper.Clone(assetValuation1);//TODO problem with the binary serializer
            var proccessedMeasureTypes = new List<string>();
            foreach (Quotation bq1 in result.quote)
            {
                proccessedMeasureTypes.Add(bq1.measureType.Value);
                Quotation bq2 = GetQuotationByMeasureType(assetValuation2, bq1.measureType.Value);
                if (null != bq2)
                {
                    bq1.value += bq2.value;
                }
            }
            var bqToAddToList = assetValuation2.quote.Where(bq2 => -1 == proccessedMeasureTypes.IndexOf(bq2.measureType.Value)).ToList();
            bqToAddToList.AddRange(result.quote);
            result.quote = bqToAddToList.ToArray();
            return result;
        }

        public static SensitivitySet Add(SensitivitySet sensitivitySet1, SensitivitySet sensitivitySet2)
        {
            SensitivitySet result = SensitivitySetHelper.Copy(sensitivitySet1);//BinarySerializerHelper.Clone(sensitivitySet1);//TODO problem with the binary serializer
            var proccessedMeasureTypes = new List<string>();
            foreach (Sensitivity bq1 in result.sensitivity)
            {
                proccessedMeasureTypes.Add(bq1.name);
                //var sensitivity = new Sensitivity {name = bq1.name, Value = bq1.Value};
                Sensitivity bq2 = GetSensitivityByName(sensitivitySet2, bq1.name);
                if (null != bq2)
                {
                    bq1.Value += bq2.Value;
                }
            }
            var bqToAddToList = sensitivitySet2.sensitivity.Where(bq2 => -1 == proccessedMeasureTypes.IndexOf(bq2.name)).ToList();
            bqToAddToList.AddRange(result.sensitivity);
            result.sensitivity = bqToAddToList.ToArray();
            return result;
        }

        /// <summary>
        /// Sums all quotations. They are assumed to be the same metric.
        /// </summary>
        /// <param name="quotationList">The quotation list contains all quotations of the same metric.</param>
        /// <param name="currency">THe currency of the quotation. This may be null.</param>
        /// <returns></returns>
        public static Quotation Sum(List<Quotation> quotationList, Currency currency)
        {
            var result = new List<Quotation>();//quotationList.FindAll(quotation => currency == quotation.currency);
            foreach(var quote in quotationList)
            {
                if(currency == null && quote.currency == null)
                {
                    result.Add(quote);
                }
                if (currency != null && quote.currency != null)
                {
                    if (currency.Value == quote.currency.Value)
                    {
                        result.Add(quote);
                    }
                }
            }
            if(0 == result.Count)
            {
                return null;
            }
            if (1 == result.Count)
            {
                return QuotationHelper.Copy(result[0]);//BinarySerializerHelper.Clone(result[0]);//TODO problem with the binary serializer
            }
            // clone collection internally - just to keep invariant of the method.
            //  
            List<Quotation> clonedCollection = QuotationHelper.Copy(quotationList);//BinarySerializerHelper.Clone(quotationList);//TODO problem with the binary serializer
            Quotation firstElement = clonedCollection[0];
            clonedCollection.RemoveAt(0);
            Quotation sumOfTheTail = Sum(clonedCollection, currency);
            return Add(firstElement, sumOfTheTail);
        }

        public static Quotation Add(Quotation quotation1, Quotation quotation2)
        {
            var result = QuotationHelper.Copy(quotation1);
                //new Quotation
                //             {
                //                 businessCenter = quotation1.businessCenter,
                //                 cashFlowType = quotation1.cashFlowType,
                //                 currency = quotation1.currency,
                //                 exchangeId = quotation1.exchangeId
                //             };
            //BinarySerializerHelper.Clone(quotation1);//TODO problem with the binary serializer
            if (null != quotation2)
            {
                result.value += quotation2.value;
                if (quotation2.sensitivitySet == null)
                {
                    return result;
                }
                if (quotation1.sensitivitySet == null)
                {
                    result.sensitivitySet = quotation2.sensitivitySet;
                    return result;
                }
                //Add the sensitivity set values.
                result.sensitivitySet = new[] {Add(quotation1.sensitivitySet[0], quotation2.sensitivitySet[0])};
            }
            return result;
        }

        /// <summary>
        /// Updates the parent valuation with the sum of child valuations, where there is no specific parent valuation.
        /// </summary>
        /// <param name="parentValuation">The parent valuation.</param>
        /// <param name="childValuations">The child valuations.</param>
        /// <param name="parentMetrics">A list of the parent metrics.</param>
        /// <param name="childMetrics">A list iof the child netrics to aggregate.</param>
        /// <param name="currencies">The currencies.</param>
        public static AssetValuation UpdateValuation(AssetValuation parentValuation, List<AssetValuation> childValuations,
            List<string> parentMetrics, List<string> childMetrics, List<string> currencies)
        {
            AssetValuation childQuotesSum = AggregateMetrics(childValuations, childMetrics, currencies);
            var parentQuotes = new List<Quotation>(parentValuation.quote);
            var childQuotes = new List<Quotation>(childQuotesSum.quote);
            parentQuotes.AddRange(childQuotes.Where(quote => !parentMetrics.Contains(quote.measureType.Value)));
            parentValuation.quote = parentQuotes.ToArray();
            return parentValuation;
        }

        /// <summary>
        /// Updates the parent valuation with the sum of child valuations, where there is no specific parent valuation.
        /// </summary>
        /// <param name="parentValuation">The parent valuation.</param>
        /// <param name="childValuation">The child valuations.</param>
        /// <param name="parentMetrics">A list of the parent metrics.</param>
        /// <param name="childMetrics">A list iof the child netrics to aggregate.</param>
        public static AssetValuation UpdateValuation(AssetValuation parentValuation, AssetValuation childValuation,
            List<string> parentMetrics, List<string> childMetrics)
        {
            var parentQuotes = new List<Quotation>(parentValuation.quote);
            var childQuotes = new List<Quotation>(childValuation.quote);
            parentQuotes.AddRange(childQuotes.Where(quote => !parentMetrics.Contains(quote.measureType.Value)));
            parentValuation.quote = parentQuotes.ToArray();
            return parentValuation;
        }

        /// <summary>
        /// Updates the parent valuation with the sum of child valuations, where there is no specific parent valuation.
        /// </summary>
        /// <param name="parentValuation">The parent valuation.</param>
        /// <param name="childValuation">The child valuation.</param>
        public static AssetValuation UpdateValuation(AssetValuation parentValuation, AssetValuation childValuation)
        {
            var parentMetrics = ExtractMetricsFromAssetValuations(parentValuation);
            var childMetrics = ExtractMetricsFromAssetValuations(childValuation);
            var parentValuationMerged = UpdateValuation(parentValuation, childValuation, parentMetrics, childMetrics);
            return parentValuationMerged;
        }

        /// <summary>
        /// Aggregates the metrics.
        /// </summary>
        /// <param name="childValuation">The valuation.</param>
        /// <returns></returns>
        public static List<string> ExtractMetricsFromAssetValuations(AssetValuation childValuation)
        {
            //initialise the metrics list.
            return childValuation.quote.Select(quotation => quotation.measureType.Value).ToList();
        }

        /// <summary>
        /// Extract the sub metrics in the sensitivity sets.
        /// </summary>
        /// <param name="childValuationSensitivities">The valuation.</param>
        /// <returns></returns>
        public static List<string> ExtractSubMetricsFromSensitivities(List<Sensitivity> childValuationSensitivities)
        {
            //initialise the sub metrics list.
            return childValuationSensitivities.Select(quotation => quotation.name).ToList();
        }

        /// <summary>
        /// Aggregates the metrics.
        /// </summary>
        /// <param name="childValuations">The metrics.</param>
        /// <param name="controllerMetrics">THe controller metrics</param>
        /// <param name="currencies">The possible currencies. </param>
        /// <returns></returns>
        public static AssetValuation AggregateMetrics(List<AssetValuation> childValuations, List<string> controllerMetrics, List<string> currencies )
        {
            var result = new AssetValuation();
            var quotes = new List<Quotation>();
            foreach (var quote in controllerMetrics.Select(metric => SumMetricQuotations(childValuations, metric, currencies)))
            {
                quotes.AddRange(quote);
            }
            result.quote = quotes.ToArray();
            return result;
        }

        /// <summary>
        /// Gets the metric results.
        /// </summary>
        /// <param name="valuations">The valuations.</param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        public static IDictionary<string, decimal[]> GetMetricResults(List<AssetValuation> valuations, string metric)
        {
            IDictionary<string, decimal[]> valuationResults = new Dictionary<string, decimal[]>();
            foreach (AssetValuation valuation in valuations)
            {
                valuationResults.Add(valuation.id, GetMetricResults(valuation, metric));
            }
            return valuationResults;
        }

        /// <summary>
        /// Gets the metric results.
        /// </summary>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        private static decimal[] GetMetricResults(AssetValuation valuation, string metric)
        {
            var results = new List<decimal>();
            var quotes = new List<Quotation>(valuation.quote);
            var matchedQuotes = quotes.FindAll(item => String.Compare(item.measureType.Value, metric, StringComparison.OrdinalIgnoreCase) == 0);
            if (matchedQuotes.Count > 0)
            {
                foreach (Quotation quotation in matchedQuotes)
                {
                    if (quotation.sensitivitySet != null)
                    {
                        results.AddRange(quotation.sensitivitySet[0].sensitivity.Select(sensitivity => sensitivity.Value));
                    }
                    else
                    {
                        results.Add(quotation.value);
                    }
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// Gets the metric results.
        /// </summary>
        /// <param name="childValuations">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="currencies">The possible currencies in the quote set. </param>
        /// <returns></returns>
        private static List<Quotation> SumMetricQuotations(IEnumerable<AssetValuation> childValuations, string metric, IEnumerable<string> currencies)
        {
            var quotations = new List<Quotation>();
            foreach (var childValuation in childValuations)
            {
                var quotes = new List<Quotation>(childValuation.quote);
                var matchedQuotes =
                    quotes.FindAll(
                        item => String.Compare(item.measureType.Value, metric, StringComparison.OrdinalIgnoreCase) == 0);
                quotations.AddRange(matchedQuotes);
            }
            //Add Up the results with no currency.
            var result = new List<Quotation>();
            var quotes2 = quotations.FindAll(item => item.currency == null);
            if (quotes2.Count > 0)//TODO RiskNPV has multiple quotes with sensitivity sets but all with no currency
            {
                result.Add(Sum(quotes2, null));
            }
            var ccyQuotes = currencies.Select(CurrencyHelper.Parse).Select(ccy => Sum(quotations, ccy));
            result.AddRange(ccyQuotes.Where(quote => quote != null));
            return result;
        }

        /// <summary>
        /// Sums the decimals.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static Decimal SumDecimals(Decimal[] list)
        {
            var total = 0.0m;
            var itemsList = new List<Decimal>(list);
            itemsList.ForEach(delegate(Decimal item) { total = total + item; });
            return total;
        }

        /// <summary>
        /// Buckets the valuation results.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <param name="valuationResults">The valuation results.</param>
        /// <returns></returns>
        public static IDictionary<string, decimal[]> BucketValuationResults(string metric, IDictionary<string, decimal[]> valuationResults)
        {
            IDictionary<string, decimal[]> bucketedResults = new Dictionary<string, decimal[]>();
            int maxArrayCount = valuationResults.Keys.Aggregate(0, (current, key) => current < valuationResults[key].Length ? valuationResults[key].Length : current);
            for (int index = 0; index < maxArrayCount; index++)
            {
                string indexStr = index == 0 ? string.Empty : index.ToString(CultureInfo.InvariantCulture);
                bucketedResults.Add($"{metric}{indexStr}", (from key in valuationResults.Keys
                                                                                where valuationResults[key].Length > 0 && index < valuationResults[key].Length
                                                                                select valuationResults[key][index]).ToArray());
            }
            return bucketedResults;
        }

        /// <summary>
        /// Adds the sensitivity set.
        /// </summary>
        /// <param name="measureType">The metrics.</param>
        /// <param name="measureValues">The analytic results.</param>
        /// <returns></returns>
        public static SensitivitySet[] BuildSensitivitySet(string measureType, Decimal[] measureValues)
        {
            var sensitivitySetList = new List<SensitivitySet>();
            var sensitivitySet = new SensitivitySet
                                     {
                                         sensitivity =
                                             measureValues.Select(
                                                 measureValue =>
                                                 new Sensitivity {name = measureType, Value = measureValue}).ToArray()
                                     };
            //int index = 1;
            sensitivitySetList.Add(sensitivitySet);
            return sensitivitySetList.ToArray();
        }

        /// <summary>
        /// Aggregates the metric result.
        /// </summary>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">The value.</param>
        /// <param name="sensitivitySet">The sensitivity set.</param>
        public static void AggregateMetricResult(AssetValuation valuation, string metric, Decimal value, SensitivitySet[] sensitivitySet)
        {
            var quotes = new List<Quotation>(valuation.quote);
            Quotation matchedQuote = quotes.Find(
                item => String.Compare(item.measureType.Value, metric, StringComparison.OrdinalIgnoreCase) == 0
                );

            if (matchedQuote == null) return;
            matchedQuote.value += value;
            matchedQuote.valueSpecified = true;
            matchedQuote.sensitivitySet = sensitivitySet;
        }

        /// <summary>
        /// Sets the metric result.
        /// </summary>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">The value.</param>
        /// <param name="sensitivitySet">The sensitivity set.</param>
        public static void SetMetricResult(AssetValuation valuation, string metric, Decimal value, SensitivitySet[] sensitivitySet)
        {
            var quotes = new List<Quotation>(valuation.quote);
            Quotation matchedQuote = quotes.Find(
                item => String.Compare(item.measureType.Value, metric, StringComparison.OrdinalIgnoreCase) == 0
                );
            if (matchedQuote == null) return;
            matchedQuote.value = value;
            matchedQuote.valueSpecified = true;
            matchedQuote.sensitivitySet = sensitivitySet;
        }

        /// <summary>
        /// Determines whether the specified results is vector.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <returns>
        /// 	<c>true</c> if the specified results is vector; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsVector(IDictionary<string, decimal[]> results)
        {
            return results.Keys.Any(resultKey => results[resultKey].Length > 1);
        }

        /// <summary>
        /// Creates the asset valuation.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="baseDate">The base date.</param>
        /// <returns></returns>
        public static AssetValuation CreateAssetValuation(IEnumerable<string> metrics, DateTime baseDate)
        {
            var av = new AssetValuation();
            var quotes = new List<Quotation>();
            foreach (string metric in metrics)
            {
                var quotation = new Quotation();
                var measureType = new AssetMeasureType { Value = metric };
                quotation.value = 0.0m;
                quotation.valueSpecified = true;
                quotation.measureType = measureType;
                quotation.valuationDate = baseDate;
                quotation.valuationDateSpecified = true;
                quotes.Add(quotation);
            }
            av.quote = quotes.ToArray();
            return av;
        }

        /// <summary>
        /// Does what it says.
        /// </summary>
        /// <param name="jaggedArray">The jagged array.</param>
        /// <returns></returns>
        public static decimal[] AggregateJaggedArray(IEnumerable<decimal[]> jaggedArray)
        {
            var result = new List<decimal>();
            foreach (var record in jaggedArray)
            {
                for (int j = 0; j < record.Length; j++)
                {
                    if (j > result.Count - 1)
                    {
                        result.Add(record[j]);
                    }
                    else
                    {
                        result[j] = result[j] + record[j];
                    }
                }
            }
            return result.ToArray();
        }
    }
}