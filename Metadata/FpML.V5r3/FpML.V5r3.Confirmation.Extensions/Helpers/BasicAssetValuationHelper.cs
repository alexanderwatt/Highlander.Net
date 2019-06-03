using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Util.Serialisation;

namespace nab.QDS.FpML.V47
{
    public static class BasicAssetValuationHelper
    {
        public static BasicAssetValuation Create(params BasicQuotation[] basicQuotations)
        {
            var result = new BasicAssetValuation {quote = basicQuotations};

            return result;
        }

        /// <summary>
        /// Creates the specified Basic asset valuation with a reference and set of quotations
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="basicQuotations">The basic quotations.</param>
        /// <returns></returns>
        public static BasicAssetValuation Create(string reference, params BasicQuotation[] basicQuotations)
        {
            BasicAssetValuation result = Create(basicQuotations);
            result.objectReference = new AnyAssetReference {href = reference};
            return result;
        }

        public static BasicQuotation GetQuotationByMeasureType(BasicAssetValuation basicAssetValuation, string measureType)
        {
            return basicAssetValuation.quote.FirstOrDefault(basicQuotation => measureType == basicQuotation.measureType.Value);
        }

        public static BasicQuotation GetQuotationByTiming(BasicAssetValuation basicAssetValuation, string timing)
        {
            return basicAssetValuation.quote.FirstOrDefault(basicQuotation => timing == basicQuotation.timing.Value);
        }


        public static BasicAssetValuation Sum(List<BasicAssetValuation> basicAssetValuationList)
        {
            if (0 == basicAssetValuationList.Count)
            {
                throw new ArgumentException("basicAssetValuationList is empty");
            }
            if (1 == basicAssetValuationList.Count)
            {
                return BinarySerializerHelper.Clone(basicAssetValuationList[0]);
            }
// clone collection internally - just to keep invariant of the method.
            //  
            List<BasicAssetValuation> clonedCollection = BinarySerializerHelper.Clone(basicAssetValuationList);

            BasicAssetValuation firstElement = clonedCollection[0];
            clonedCollection.RemoveAt(0);

            BasicAssetValuation sumOfTheTail = Sum(clonedCollection);

            return Add(firstElement, sumOfTheTail);
        }

        public static BasicAssetValuation Add(BasicAssetValuation basicAssetValuation1, BasicAssetValuation basicAssetValuation2)
        {
            BasicAssetValuation result = BinarySerializerHelper.Clone(basicAssetValuation1);

            var proccessedMeasureTypes = new List<string>();

            foreach (BasicQuotation bq1 in result.quote)
            {
                proccessedMeasureTypes.Add(bq1.measureType.Value);

                BasicQuotation bq2 = GetQuotationByMeasureType(basicAssetValuation2, bq1.measureType.Value);
                
                if (null != bq2)
                {
                    bq1.value += bq2.value;
                }
            }

            var bqToAddToList = new List<BasicQuotation>();

            foreach (BasicQuotation bq2 in basicAssetValuation2.quote)
            {
                if (-1 == proccessedMeasureTypes.IndexOf(bq2.measureType.Value))//if hasn't been processed in the first pass
                {
                   bqToAddToList.Add(bq2);
                }
            }
            
            bqToAddToList.AddRange(result.quote);
            
            result.quote = bqToAddToList.ToArray();

            return result;
        }

        
    }
}