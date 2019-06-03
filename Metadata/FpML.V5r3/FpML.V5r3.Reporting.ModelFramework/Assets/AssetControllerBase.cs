#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Assets
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
        /// The valuations; quotes, vols etc
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
                //Added to handle the case where the metric is not a valid metric for that asset.
                if (value == null) continue;
                //Need to handle array data as well. This will only work for decimal arrays.
                if (value is IList list)
                {
                    var index = 1;
                    var valArray = list;
                    //This handles the already existing quotation.
                    object decimalValue = Convert.ToDecimal(valArray[0]);
                    ObjectLookupHelper.SetPropertyValue(quotation, cValuePropertyName, decimalValue);
                    quotation.informationSource = InformationSourceHelper.CreateArray("Item.0");
                    //Set the appropriate values. Should really srilaise the object.
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
            if (extraQuotations.Count > 0)
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

        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <param name="other">The first object to compare. </param>
        /// <returns>A signed integer that indicates the relative values of <paramref name="other" /></returns>
        
        public int CompareTo(IPriceableAssetController other)
        {
            int compare = DateTime.Compare(GetRiskMaturityDate(), other.GetRiskMaturityDate());
            return compare;
        }
    }
}