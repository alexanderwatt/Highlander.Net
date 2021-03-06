﻿/*
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting.Helpers;
using Orion.Util.Helpers;


namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base Model Controller class from which all Asset controllers/models should be extended
    /// </summary>
    public abstract class AssetControllerBase : IPriceableAssetController
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public BasicQuotation MarketQuote { get; set; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>The model parameters.</value>
        public IAssetControllerData ModelData { get; protected set; }

        /// <summary>
        /// The valuations; quotes, volatility etc
        /// </summary>
        public BasicAssetValuation BasicAssetValuation { get; set; }
        
        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public IList<string> Metrics
        {
            get => GetMetricsFromQuotes(ModelData.BasicAssetValuation.quote);
            set { }
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public abstract BasicAssetValuation Calculate(IAssetControllerData modelData);


        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetRiskMaturityDate();


        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns></returns>
        public abstract decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="analyticResults">The analytic results.</param>
        /// <returns></returns>
        protected BasicAssetValuation GetValue<T>(T analyticResults)
        {
            const string cValuePropertyName = "value";
            var extraQuotations = new List<BasicQuotation>();
            foreach (BasicQuotation quotation in ModelData.BasicAssetValuation.quote)
            {
                object value = ObjectLookupHelper.GetPropertyValue(analyticResults, quotation.measureType.Value);
                //Need to handle array data as well. This will only work for decimal arrays.
                if (value is IList list)
                {
                    var index = 1;
                    var valArray = list;
                    //This handles the already existing quotation.
                    object decimalValue = Convert.ToDecimal(valArray[0]);
                    ObjectLookupHelper.SetPropertyValue(quotation, cValuePropertyName, decimalValue);
                    quotation.informationSource = InformationSourceHelper.CreateArray("Item.0");
                    //Set the appropriate values. Should really serialize the object.
                    var measureType = quotation.measureType;
                    var quoteUnits = quotation.quoteUnits;//TODO add any other properties or do automatically.
                    //Add all extra quotations.
                    for(int i = 1; i < valArray.Count; i++)
                    {
                        var extraQuote = new BasicQuotation
                                             {
                                                 measureType = measureType,
                                                 quoteUnits = quoteUnits,
                                                 value = Convert.ToDecimal(valArray[index]),
                                                 informationSource =
                                                     InformationSourceHelper.CreateArray("Item." + index)
                                             };
                        extraQuotations.Add(extraQuote);
                        index++;
                    }
                }
                else
                {
                    ObjectLookupHelper.SetPropertyValue(quotation, cValuePropertyName, value);
                }                
            }
            //Merge the quotes.
            if (extraQuotations.Count>0)
            {
                var mergedQuotes = new List<BasicQuotation>(ModelData.BasicAssetValuation.quote);
                mergedQuotes.AddRange(extraQuotations);
                ModelData.BasicAssetValuation.quote = mergedQuotes.ToArray();
            }           
            return ModelData.BasicAssetValuation;
        }

        /// <summary>
        /// Gets the metrics from quotes.
        /// </summary>
        /// <param name="quotes">The quotes.</param>
        /// <returns></returns>
        private static List<string> GetMetricsFromQuotes(IEnumerable<BasicQuotation> quotes)
        {
            var metrics = new List<string>();
            if (quotes != null)
            {
                metrics.AddRange(quotes.Select(basicQuotation => basicQuotation.measureType.Value));
            }
            return metrics;
        }

        /// <summary>
        /// Gets the forward spot date.
        /// </summary>
        /// <returns></returns>
        protected static DateTime GetSpotDate(DateTime baseDate, IBusinessCalendar fixingCalendar, RelativeDateOffset spotDateOffset)
        {
            return fixingCalendar.Advance(baseDate, spotDateOffset, spotDateOffset.businessDayConvention);
        }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <returns></returns>
        protected static DateTime GetEffectiveDate(DateTime startDate, IBusinessCalendar paymentCalendar, Period term, BusinessDayConventionEnum businessDayConvention)
        {
            return
                paymentCalendar.Advance(startDate,
                                        OffsetHelper.FromInterval(term, DayTypeEnum.Calendar),
                                        businessDayConvention);
        }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <returns></returns>
        protected static DateTime GetFixingDate(DateTime startDate, IBusinessCalendar fixingCalendar, RelativeDateOffset fixingOffset)
        {
            return fixingCalendar.Advance(startDate, fixingOffset, fixingOffset.businessDayConvention);
        }
    }
}