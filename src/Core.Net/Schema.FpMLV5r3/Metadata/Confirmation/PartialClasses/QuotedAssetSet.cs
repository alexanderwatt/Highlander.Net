using System;
//using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r3.Confirmation
{
    public partial class QuotedAssetSet
    {
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
            var valuationMap = new Dictionary<string, BasicAssetValuation>();
            if (instrumentSet != null && instrumentSet.Items != null)
            {
                foreach (Asset asset in instrumentSet.Items)//The asset type
                {
                    string assetId = asset.id;
                    //Trace.WriteLine(String.Format("Merge: Existing asset: '{0}'", assetId);
                    instrumentMap[assetId.ToLower()] = asset;
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
                            throw new ApplicationException(String.Format(
                                "Cannot find asset '{0}' for assetQuote", assetId));
                    }
                    // merge the quotes
                    BasicAssetValuation existingValuation;
                    if (valuationMap.TryGetValue(assetId.ToLower(), out existingValuation))
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
                    // append the assetquotes
                    var quotes = new List<BasicQuotation>(existingValuation.quote);
                    foreach (BasicQuotation quote in currentValuation.quote)
                    {
                        if (quote.valueSpecified || includeEmptyCurrentQuotes)
                        {
                            quotes.Add(quote);
                            //if (quote.valueSpecified)
                            //Trace.WriteLine(String.Format("Merge: Asset: '{0}' adding existing quote value '{1}'", assetId, quote.value);
                            //else
                            //Trace.WriteLine(String.Format("Merge: Asset: '{0}' adding existing empty quote", assetId);
                        }
                        else
                        {
                            //Trace.WriteLine(String.Format("Merge; Asset: '{0}' skipping existing empty quote", assetId);
                        }
                    }
                    existingValuation.quote = quotes.ToArray();
                }
            }
            // add extra instruments
            if (additional.instrumentSet != null && additional.instrumentSet.Items != null)
            {
                foreach (Asset asset in additional.instrumentSet.Items)
                {
                    string assetId = asset.id;
                    if (!instrumentMap.ContainsKey(assetId.ToLower()))
                    {
                        instrumentMap[assetId.ToLower()] = asset;
                        //Trace.WriteLine(String.Format("Merge: Additional asset: '{0}'", assetId);
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
                            throw new ApplicationException(String.Format(
                                "Cannot find asset '{0}' for assetQuote", assetId));
                    }
                    // merge the quotes
                    BasicAssetValuation existingValuation;
                    if (valuationMap.TryGetValue(assetId.ToLower(), out existingValuation))
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
                    // append the assetquotes
                    var quotes = new List<BasicQuotation>(existingValuation.quote);
                    foreach (BasicQuotation quote in additionalValuation.quote)
                    {
                        if (quote.valueSpecified || includeEmptyAdditionalQuotes)
                        {
                            quotes.Add(quote);
                            //if (quote.valueSpecified)
                            //Trace.WriteLine(String.Format("Merge: Asset: '{0}' adding additional quote value '{1}'", assetId, quote.value);
                            //else
                            //Trace.WriteLine(String.Format("Merge: Asset: '{0}' adding additional empty quote", assetId);
                        }
                        else
                        {
                            //Trace.WriteLine(String.Format("Merge; Asset: '{0}' skipping additional empty quote", assetId);
                        }
                    }
                    existingValuation.quote = quotes.ToArray();
                }
            }
            var instSet = new InstrumentSet {Items = instrumentMap.Values.ToArray()};
            result.instrumentSet = instSet;
            result.assetQuote = valuationMap.Values.ToArray();
            return result;
        }

        public static List<BasicAssetValuation> GetAssetQuote(QuotedAssetSet quotedAssetSet, string instrumentId)
        {
            return quotedAssetSet.assetQuote.Where(basicAssetValuation => basicAssetValuation.objectReference.href == instrumentId).ToList();
        }
    }
}
