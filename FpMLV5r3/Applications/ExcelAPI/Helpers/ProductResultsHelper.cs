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

using System;
using System.Collections.Generic;
using System.Text;
using Highlander.Reporting.V5r3;

namespace HLV5r3.Helpers
{
    ///<summary>
    ///</summary>
    public static class ProductResultsHelper
    {
        /// <summary>
        /// Builds the results.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="productReferenceKeys">The swap product reference keys.</param>
        /// <param name="valuations">The valuations.</param>
        /// <param name="displayHeadersInd">The no of swap products.</param>
        /// <returns></returns>
        public static object BuildResults(string[] metrics, string[] productReferenceKeys, TradeValuationItem[] valuations, Boolean displayHeadersInd)
        {
            int index = 0;
            int noOfSwapProducts = productReferenceKeys.Length;
            int horizontalArrayCellCount = displayHeadersInd ? 1 + (metrics.Length * 2) : metrics.Length;
            int verticalArrayCellCount = noOfSwapProducts;
            var result = new object[verticalArrayCellCount, horizontalArrayCellCount];
            var valuationList = new List<TradeValuationItem>(valuations);
            foreach (string productReferenceKey in productReferenceKeys)
            {
                if (!string.IsNullOrEmpty(productReferenceKey))
                {
                    TradeValuationItem valuation = valuationList.Find(
                        valuationItem => String.Compare(valuationItem.valuationSet.id, productReferenceKey, StringComparison.OrdinalIgnoreCase) == 0
                        );
                    if (valuation != null)
                    {
                        if (displayHeadersInd)
                        {
                            result[index, 0] = valuation.valuationSet.id;
                        }
                        var metricsList = new List<string>(metrics);
                        metricsList.RemoveAll(item => item.Length == 0);
                        int metricIndex = displayHeadersInd ? 1 : 0;
                        foreach (string metric in metricsList)
                        {
                            var quotes = new List<Quotation>(valuation.valuationSet.assetValuation[0].quote);
                            List<Quotation> matchedQuotes = quotes.FindAll(item => String.CompareOrdinal(item.measureType.Value, metric) == 0);
                            if (matchedQuotes.Count > 0)
                            {
                                result[index, metricIndex] = matchedQuotes[0].measureType.Value;

                                int metricValueIndex = displayHeadersInd ? metricIndex + 1 : metricIndex;
                                if (matchedQuotes.Count == 1)
                                {
                                    result[index, metricValueIndex] = matchedQuotes[0].value;
                                }
                                else
                                {
                                    var sb = new StringBuilder();
                                    int quoteIndex = 1;
                                    Boolean maxLimitReached = false;
                                    foreach (Quotation quotation in matchedQuotes)
                                    {
                                        string delimiter = quoteIndex < matchedQuotes.Count ? ", " : string.Empty;
                                        string value = $"{quotation.value}{delimiter}";

                                        if ((sb.Length + value.Length + 8) > 255)
                                        {
                                            maxLimitReached = true;
                                            break;
                                        }
                                        sb.Append(value);
                                        quoteIndex++;
                                    }

                                    string resultValue = maxLimitReached == false ? sb.ToString() : sb + " more...";
                                    result[index, metricValueIndex] = resultValue;
                                }
                            }
                            metricIndex = displayHeadersInd ? metricIndex + 2 : metricIndex + 1;
                        }
                    }
                }
                index++;
            }
            return result;
        }
    }
}