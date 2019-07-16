/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
//using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Orion.Util.Serialisation;

namespace FpML.V5r11.Reporting
{
    public partial class QuotedAssetSet
    {
        /// <summary>
        /// Replaces a QuotedAssetSet with this. Note that the referential integrity between the assets
        /// and valuations is checked and maintained.
        /// </summary>
        /// <param name="additional">The additional QuotedAssetSet.</param>
        /// <returns></returns>
        public void Replace(QuotedAssetSet additional)
        {
            var qas = XmlSerializerHelper.Clone(additional);
            instrumentSet = qas.instrumentSet;
            assetQuote = qas.assetQuote;
        }

        /// <summary>
        /// Merges a QuotedAssetSet with this. Note that the referential integrity between the assets
        /// and valuations is checked and maintained.
        /// </summary>
        /// <param name="additional">The additional QuotedAssetSet.</param>
        /// <param name="checkValuationAssetReferences">if set to <c>true</c> [check valuation asset references].</param>
        /// <param name="includeEmptyCurrentQuotes">if set to <c>true</c> [include empty current quotes].</param>
        /// <param name="includeEmptyAdditionalQuotes">if set to <c>true</c> [include empty additional quotes].</param>
        /// <returns></returns>
        public QuotedAssetSet Merge(QuotedAssetSet additional,
            bool checkValuationAssetReferences, 
            bool includeEmptyCurrentQuotes, 
            bool includeEmptyAdditionalQuotes)
        {
            var result = new QuotedAssetSet();
            // build unique instrumentSet and valuation quote lists
            var instrumentMap = new Dictionary<string, Asset>();
            var itemsMap = new Dictionary<string, ItemsChoiceType45>();
            var valuationMap = new Dictionary<string, BasicAssetValuation>();
            if (instrumentSet?.Items != null && instrumentSet.ItemsElementName != null)
            {
                if (instrumentSet.Items.Length == instrumentSet.ItemsElementName.Length)
                {
                    var index = 0;
                    foreach (Asset asset in instrumentSet.Items) //The asset type
                    {
                        string assetId = asset.id;
                        instrumentMap[assetId.ToLower()] = asset;
                        itemsMap[assetId.ToLower()] = instrumentSet.ItemsElementName[index];
                        index++;
                    }
                }
            }
            if (assetQuote != null)
            {
                foreach (BasicAssetValuation currentValuation in assetQuote)
                {
                    string assetId = currentValuation.objectReference.href;
                    if (checkValuationAssetReferences)
                    {
                        Asset asset;
                        if (!instrumentMap.TryGetValue(assetId.ToLower(), out asset))
                            throw new ApplicationException($"Cannot find asset '{assetId}' for assetQuote");
                    }
                    // merge the quotes
                    if (valuationMap.TryGetValue(assetId.ToLower(), out var existingValuation))
                    {
                        //Trace.WriteLine(String.Format("Merge: Asset: '{0}' updating existing valuation", assetId);
                    }
                    else
                    {
                        // not found - create a new valuation
                        //Trace.WriteLine(String.Format("Merge: Asset: '{0}' creating existing valuation", assetId);
                        existingValuation = new BasicAssetValuation
                                                {
                            objectReference = new AnyAssetReference { href = assetId },
                            quote = new List<BasicQuotation>().ToArray()
                        };
                        valuationMap[assetId.ToLower()] = existingValuation;
                    }
                    // append the asset quotes
                    var quotes = new List<BasicQuotation>(existingValuation.quote);
                    quotes.AddRange(currentValuation.quote.Where(quote => quote.valueSpecified || includeEmptyCurrentQuotes));
                    existingValuation.quote = quotes.ToArray();
                }
            }
            // add extra instruments
            if (additional.instrumentSet?.Items != null && additional.instrumentSet.ItemsElementName != null)
            {
                if (additional.instrumentSet.Items.Length == additional.instrumentSet.ItemsElementName.Length)
                {
                    var index = 0;
                    foreach (Asset asset in additional.instrumentSet.Items)
                    {
                        string assetId = asset.id;
                        if (!instrumentMap.ContainsKey(assetId.ToLower()))
                        {
                            instrumentMap[assetId.ToLower()] = asset;
                            itemsMap[assetId.ToLower()] = additional.instrumentSet.ItemsElementName[index];
                            //Trace.WriteLine(String.Format("Merge: Additional asset: '{0}'", assetId);
                        }
                        index++;
                    }
                }
            }
            // append valuation quotes
            if (additional.assetQuote != null)
            {
                foreach (BasicAssetValuation additionalValuation in additional.assetQuote)
                {
                    string assetId = additionalValuation.objectReference.href;
                    if (checkValuationAssetReferences)
                    {
                        Asset asset;
                        if (!instrumentMap.TryGetValue(assetId.ToLower(), out asset))
                            throw new ApplicationException($"Cannot find asset '{assetId}' for assetQuote");
                    }
                    // merge the quotes
                    if (valuationMap.TryGetValue(assetId.ToLower(), out var existingValuation))
                    {
                        //Trace.WriteLine(String.Format("Merge: Asset: '{0}' updating additional valuation", assetId);
                    }
                    else
                    {
                        // not found - just add the valuation
                        existingValuation = new BasicAssetValuation
                                                {
                            objectReference = new AnyAssetReference { href = assetId },
                            quote = new List<BasicQuotation>().ToArray()
                        };
                        valuationMap[assetId.ToLower()] = existingValuation;
                        //Trace.WriteLine(String.Format("Merge: Asset: '{0}' creating additional valuation", assetId);
                    }
                    // append the asset quotes
                    var quotes = new List<BasicQuotation>(existingValuation.quote);
                    quotes.AddRange(additionalValuation.quote.Where(quote => quote.valueSpecified || includeEmptyAdditionalQuotes));
                    existingValuation.quote = quotes.ToArray();
                }
            }
            var instSet = new InstrumentSet {Items = instrumentMap.Values.ToArray(), ItemsElementName = itemsMap.Values.ToArray()};
            result.instrumentSet = instSet;
            result.assetQuote = valuationMap.Values.ToArray();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quotedAssetSet"></param>
        /// <param name="instrumentId"></param>
        /// <returns></returns>
        public static List<BasicAssetValuation> GetAssetQuote(QuotedAssetSet quotedAssetSet, string instrumentId)
        {
            return quotedAssetSet.assetQuote.Where(basicAssetValuation => basicAssetValuation.objectReference.href == instrumentId).ToList();
        }
    }
}
