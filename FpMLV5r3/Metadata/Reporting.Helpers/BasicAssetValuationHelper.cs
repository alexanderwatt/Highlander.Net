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
using System.Linq;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Serialisation;

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class BasicAssetValuationHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="basicQuotations"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basicAssetValuation"></param>
        /// <param name="measureType"></param>
        /// <returns></returns>
        public static BasicQuotation GetQuotationByMeasureType(BasicAssetValuation basicAssetValuation, string measureType)
        {
            return basicAssetValuation.quote.FirstOrDefault(basicQuotation => measureType == basicQuotation.measureType.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basicAssetValuation"></param>
        /// <param name="timing"></param>
        /// <returns></returns>
        public static BasicQuotation GetQuotationByTiming(BasicAssetValuation basicAssetValuation, string timing)
        {
            return basicAssetValuation.quote.FirstOrDefault(basicQuotation => timing == basicQuotation.timing.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basicAssetValuationList"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basicAssetValuation1"></param>
        /// <param name="basicAssetValuation2"></param>
        /// <returns></returns>
        public static BasicAssetValuation Add(BasicAssetValuation basicAssetValuation1, BasicAssetValuation basicAssetValuation2)
        {
            BasicAssetValuation result = BinarySerializerHelper.Clone(basicAssetValuation1);
            var proccessMeasureTypes = new List<string>();
            foreach (BasicQuotation bq1 in result.quote)
            {
                proccessMeasureTypes.Add(bq1.measureType.Value);
                BasicQuotation bq2 = GetQuotationByMeasureType(basicAssetValuation2, bq1.measureType.Value);              
                if (null != bq2)
                {
                    bq1.value += bq2.value;
                }
            }
            var bqToAddToList = new List<BasicQuotation>();
            foreach (BasicQuotation bq2 in basicAssetValuation2.quote)
            {
                if (-1 == proccessMeasureTypes.IndexOf(bq2.measureType.Value))//if hasn't been processed in the first pass
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