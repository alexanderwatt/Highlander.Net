using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting;
using Orion.Analytics.Utilities;

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// MetricsHelper
    /// </summary>
    public static class MetricsHelper
    {
        #region Generic Helpers

        /// <summary>
        /// Builds the evaluation results.
        /// </summary>
        /// <param name="assetReferenceKey">The asset identifier</param>
        /// <param name="valuations">The valuations.</param>
        /// <returns></returns>
        public static object[,] BuildEvaluationResults(string assetReferenceKey,
            IEnumerable<BasicAssetValuation> valuations)
        {
            object[,] result = { };
            if (assetReferenceKey != null)
            {
                var verticalArrayCellCount = 1;
                var valuationList = new List<BasicAssetValuation>(valuations);
                var assetIdentifier = assetReferenceKey;
                var valuation = valuationList.Find(
                    valuationItem => String.Compare(valuationItem.objectReference.href, assetIdentifier,
                                            StringComparison.OrdinalIgnoreCase) == 0
                );
                //TODO loop through and aggregate
                //var basicAssetValuation = new List<BasicAssetValuation> {valuation};
                //var sumValuations = BasicAssetValuationHelper.Sum(basicAssetValuation);
                if (valuation != null)
                {
                    var horizontalArrayCellCount = 1 + valuation.quote.Length * 2;
                    result = new object[verticalArrayCellCount, horizontalArrayCellCount];
                    result[0, 0] = valuation.objectReference.href;
                    var metricIndex = 1;
                    foreach (var quotation in valuation.quote)
                    {
                        result[0, metricIndex] = quotation.measureType.Value;
                        result[0, metricIndex + 1] = quotation.value;
                        metricIndex = metricIndex + 2;
                    }
                }           
            }
            return result;
        }

        /// <summary>
        /// Builds the evaluation results.
        /// </summary>
        /// <param name="assetReferenceKeys"></param>
        /// <param name="valuations">The valuations.</param>
        /// <returns></returns>
        public static object[,] BuildEvaluationResults(IList<string> assetReferenceKeys,
            IList<BasicAssetValuation> valuations)
        {
            var tempList = new List<object[,]>();
            var assetReference = 0;
            var verticalArrayCellCount = assetReferenceKeys.Count;
            var horizontalArrayCellCount = 1;
            foreach (var assetReferenceKey in assetReferenceKeys)
            {
                var referenceKeyMetrics = BuildEvaluationResults(assetReferenceKey, valuations);
                if (horizontalArrayCellCount < referenceKeyMetrics.GetUpperBound(1))
                {
                    horizontalArrayCellCount = referenceKeyMetrics.GetUpperBound(1);
                }               
                tempList.Add(referenceKeyMetrics);
            }
            var result = new object[verticalArrayCellCount, horizontalArrayCellCount];
            foreach (var asset in tempList)
            {
                var index = 0;
                var width = asset.GetUpperBound(1);
                for (var i = 0; i < width; i++)
                {
                    result[assetReference, index] = asset[0, index];
                    index++;
                }
                assetReference++;
            }
            return result;
        }

        ///// <summary>
        ///// Builds the evaluation results.
        ///// </summary>
        ///// <param name="assetReferenceKeys"></param>
        ///// <param name="valuations">The valuations.</param>
        ///// <param name="metrics">The metrics.</param>
        ///// <param name="noOfAssets">The no of assets.</param>
        ///// <returns></returns>
        //public static object[,] BuildEvaluationResults(ICollection<string> metrics, IEnumerable<string> assetReferenceKeys,
        //    IEnumerable<BasicAssetValuation> valuations, int noOfAssets)
        //{
        //    var index = 0;
        //    var horizontalArrayCellCount = 1 + metrics.Count * 2;
        //    var verticalArrayCellCount = noOfAssets;
        //    var result = new object[verticalArrayCellCount, horizontalArrayCellCount];
        //    var valuationList = new List<BasicAssetValuation>(valuations);
        //    foreach (var assetReferenceKey in assetReferenceKeys)
        //    {
        //        if (assetReferenceKey.Length > 0)
        //        {
        //            var assetIdentifier = assetReferenceKey;
        //            var valuation = valuationList.Find(
        //                valuationItem => String.Compare(valuationItem.objectReference.href, assetIdentifier, StringComparison.OrdinalIgnoreCase) == 0
        //                );
        //            if (valuation != null)
        //            {
        //                result[index, 0] = valuation.objectReference.href;
        //                var metricIndex = 1;
        //                foreach (var quotation in valuation.quote)
        //                {
        //                    result[index, metricIndex] = quotation.measureType.Value;
        //                    result[index, metricIndex + 1] = quotation.value;
        //                    metricIndex = metricIndex + 2;
        //                }
        //            }
        //        }
        //        index++;
        //    }
        //    return result;
        //}

        /// <summary>
        /// Builds the evaluation results.
        /// </summary>
        /// <param name="assetReferenceKeys"></param>
        /// <param name="valuations">The valuations.</param>
        /// <param name="metrics">The metrics.</param>
        /// <param name="noOfAssets">The no of assets.</param>
        /// <returns></returns>
        public static object[,] BuildEvaluationResultsClean(ICollection<string> metrics, IEnumerable<string> assetReferenceKeys,
            IEnumerable<BasicAssetValuation> valuations, int noOfAssets)
        {
            var valuationList = new List<BasicAssetValuation>(valuations);
            var resultList = new List<Tuple<string, string, decimal>>();
            var index = 0;
            foreach (var assetReferenceKey in assetReferenceKeys)
            {
                if (assetReferenceKey.Length > 0)
                {
                    var assetIdentifier = assetReferenceKey;
                    var valuation = valuationList.Find(
                        valuationItem => String.Compare(valuationItem.objectReference.href, assetIdentifier, StringComparison.OrdinalIgnoreCase) == 0
                        );

                    if (valuation != null)
                    {
                        //                        result[index, 0] = valuation.objectReference.href;
                        foreach (var quotation in valuation.quote)
                        {
                            //                            result[index, metricIndex] = quotation.measureType.Value;
                            var tulple = new Tuple<string, string, decimal> (assetIdentifier, quotation.measureType.Value, quotation.value);
                            resultList.Add(tulple);
                        }
                    }
                }
            }
            var horizontalArrayCellCount = resultList.Count;
            var verticalArrayCellCount = 3;
            var result = new object[horizontalArrayCellCount, verticalArrayCellCount];
            foreach (var tuple in resultList)
            {
                result[index, 0] = tuple.Item1;
                result[index, 1] = tuple.Item2;
                result[index, 2] = tuple.Item3;
                index++;
            }
            return result;
        }

        ///<summary>
        /// Check that on those values in the metrics list are valid in the rate metrics list.
        ///</summary>
        ///<param name="metrics"></param>
        ///<param name="rateMetrics"></param>
        ///<typeparam name="TEnumT"></typeparam>
        ///<returns></returns>
        ///<exception cref="System.Exception"></exception>
        public static List<TEnumT> GetMetricsToEvaluate<TEnumT>(IList<string> metrics, List<TEnumT> rateMetrics) where TEnumT : struct
        {
            System.Exception exception = null;
            var metricTypes = new List<TEnumT>();
            if (metrics != null && metrics.Count > 0)
            {
                try
                {
                    foreach(var metric in metrics)
                    {
                        if (!Enum.TryParse(metric, out TEnumT result)) continue;
                        metricTypes.Add(result);
                    }
                    //This is unstable.
                    //metricTypes.AddRange(metrics.Select(metric => FindEnumFromString(rateMetrics, metric)));
                }
                catch
                {
                    exception = new ArgumentException("Invalid Quotation metric type has been supplied");
                }
            }
            else
            {
                exception = new ArgumentNullException();
            }

            if (exception != null)
            {
                throw exception;
            }
            return metricTypes.Distinct().ToList();
        }

        ///// <summary>
        ///// Finds the enum from string.
        ///// </summary>
        ///// <param name="list">The list.</param>
        ///// <param name="findItem">The find item.</param>
        ///// <returns></returns>
        //private static TEnumT FindEnumFromString<TEnumT>(List<TEnumT> list, string findItem)
        //{
        //    var result = list.Find(metricItem => String.Compare(metricItem.ToString(), findItem, StringComparison.OrdinalIgnoreCase) == 0);
        //    return result;
        //}

        #endregion

        #region Metric Helpers - moved from AssetHelper.

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="valuations">The metrics calculation results.</param>
        /// <param name="controllerMetrics">THe controller metrics</param>
        /// <returns></returns>
        public static Dictionary<string, decimal> AggregateMetrics(BasicAssetValuation valuations, List<string> controllerMetrics)
        {
            return controllerMetrics.ToDictionary(metric => metric, metric => Aggregator.SumDecimals(GetMetricResults(valuations, metric)));
        }

        /// <summary>
        /// Gets the metric results.
        /// </summary>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        public static decimal[] GetMetricResults(BasicAssetValuation valuation, string metric)
        {
            var results = new List<decimal>();
            var quotes = new List<BasicQuotation>(valuation.quote);
            var matchedQuotes = quotes.FindAll(item => String.Compare(item.measureType.Value, metric, StringComparison.OrdinalIgnoreCase) == 0);
            if (matchedQuotes.Count > 0)
            {
                results.AddRange(matchedQuotes.Select(quotation => quotation.value));
            }
            return results.ToArray();
        }

        #endregion
    }
}