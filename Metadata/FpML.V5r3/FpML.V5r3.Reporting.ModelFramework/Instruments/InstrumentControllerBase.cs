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

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.ModelFramework.Trades;

#endregion

namespace Orion.ModelFramework.Instruments
{
    /// <summary>
    /// Base Model Controller class from which all Instrument controllers/models should be extended
    /// </summary>
    [Serializable]
    public abstract class InstrumentControllerBase : IModelController<IInstrumentControllerData, AssetValuation>, IProduct
    {
        #region Constants

        protected const string CDefaultBucketingInterval = "3M";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets the collateral currency.
        /// </summary>
        /// <value>The id.</value>
        public string CollateralCurrency { get; set; }

        /// <summary>
        /// Gets the multiplier. This is useful particularly for option calculations.
        /// </summary>
        /// <value>The multiplier.</value>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Gets the collateralised flag.
        /// </summary>
        /// <value>The collateralised flag.</value>
        public bool IsCollateralised { get; set; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>The model parameters.</value>
        public IInstrumentControllerData ModelData { get; protected set; }

        /// <summary>
        /// Gets the list of party names, in order.
        /// </summary>
        public List<string> OrderedPartyNames = new List<string>();

        #endregion

        #region Methods And Constructor

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public abstract AssetValuation Calculate(IInstrumentControllerData modelData);

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketing.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        public abstract DateTime[] GetBucketingDates(DateTime baseDate, Period bucketingInterval);

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketing.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        public void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval)
        {
            BucketingInterval = bucketingInterval;
            BucketedDates = GetBucketingDates(baseDate, bucketingInterval);
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public abstract Product BuildTheProduct();

        ///<summary>
        ///</summary>
        protected InstrumentControllerBase()
        {
            Id = string.Empty;
        }

        #endregion

        #region Public members

        // BucketedCouponDates
        public Period BucketingInterval;
        //public IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();
        public DateTime[] BucketedDates = { };

        /// <summary>
        /// Gets the bucketed dates list.
        /// </summary>
        /// <value>The bucketed dates list.</value>
        public string BucketedDatesList => string.Join(",", BucketedDates.Select(date => date.ToString("yyyy-MM-dd")).ToArray());

        /// <summary>
        /// Gets or sets a value indicating whether [calculation performed indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [calculation performed indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool CalculationPerformedIndicator { get; set; }

        ///<summary>
        /// The identifier.
        ///</summary>
        public ITradeIdentifier ProductIdentifier { get; set; }

        ///<summary>
        /// The product type.
        ///</summary>
        public ProductTypeSimpleEnum ProductType { get; set; }

        ///<summary>
        /// Returns the list of payment currencies for that instrument.
        ///</summary>
        public List<string> PaymentCurrencies = new List<string>();

        ///<summary>
        /// The type of curve evolution to use. The default is ForwardToSpot
        ///</summary>
        public PricingStructureEvolutionType PricingStructureEvolutionType { get; set; }

        ///<summary>
        /// The risk maturity date. This is not necessarily the legal maturity date.
        ///</summary>
        public DateTime RiskMaturityDate { get; set; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public IList<string> Metrics
        {
            get => GetMetricsFromQuotes(ModelData.AssetValuation.quote);
            set { }
        }

        #endregion

        #region Private methods 

        /// <summary>
        /// Gets the child valuation.
        /// </summary>
        /// <param name="childController">The child controller.</param>
        /// <param name="childMetrics">The child metrics.</param>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        private static AssetValuation GetChildValuation(IModelController<IInstrumentControllerData, AssetValuation> childController, List<string> childMetrics, IInstrumentControllerData modelData)
        {
            //var valuation = new AssetValuation();
            AssetValuation avInput = AssetValuationHelper.CreateAssetValuation(childMetrics.ToArray(), modelData.ValuationDate);
            IInstrumentControllerData childModelData = new InstrumentControllerData(avInput, modelData.MarketEnvironment, modelData.ValuationDate);
            var valuation = childController.Calculate(childModelData);
            valuation.id = childController.Id;
            return valuation;
        }

        /// <summary>
        /// Convert the model metrics.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="analyticModelMetrics">The analytic model metrics.</param>
        /// <returns></returns>
        protected static List<string> ConvertMetrics<TEnumT>(List<TEnumT> analyticModelMetrics)
        {
            return analyticModelMetrics.Select(element => element.ToString()).ToList();
        }

        /// <summary>
        /// Resolves the model metrics.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="metrics">The metrics.</param>
        /// <param name="analyticModelMetrics">The analytic model metrics.</param>
        /// <returns></returns>
        private static List<TEnumT> ResolveModelMetrics<TEnumT>(ICollection<string> metrics, List<TEnumT> analyticModelMetrics)
        {
            System.Exception exception = null;
            var metricTypes = new List<TEnumT>();
            if (metrics != null)
            {
                try
                {
                    foreach (string metric in metrics)
                    {
                        var matchedMetric = FindEnumFromString(analyticModelMetrics, metric);
                        if (!metricTypes.Contains(matchedMetric) && String.Compare(metric, matchedMetric.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            metricTypes.Add(FindEnumFromString(analyticModelMetrics, metric));
                        }
                    }
                }
                catch
                {
                    exception = new ArgumentException("An Invalid metric type has been supplied");
                }
            }
            else
            {
                exception = new ArgumentException("No metrics to resolve.");
            }
            if (exception != null)
            {
                throw exception;
            }
            return metricTypes;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="analyticResults">The analytic results.</param>
        /// <param name="valuationDate">The valuation date. </param>
        /// <returns></returns>
        protected AssetValuation GetValue<T>(T analyticResults, DateTime valuationDate) where T : class
        {
            AssetValuation av = ModelData.AssetValuation;
            var quotations = new List<Quotation>(av.quote);
            if (analyticResults != null)
            {
                av = new AssetValuation();
                quotations = new List<Quotation>();
                foreach (Quotation quotation in ModelData.AssetValuation.quote)
                {
                    if (ObjectLookupHelper.ObjectPropertyExists(analyticResults, quotation.measureType.Value))
                    {
                        //Firstly get the value
                        var value = ObjectLookupHelper.GetPropertyValue(analyticResults, quotation.measureType.Value);
                        if (value != null)
                        {
                            //check if Item1ChoiceType ia an array
                            if (value.GetType().IsArray)
                            {
                                //TODO
                                //Need to include the case when it is not an array of Decimal
                                var decArray = (Decimal[]) value;
                                Quotation vectorQuotation =
                                    RateInstrumentMetricsHelper.IsLocalCurrencyType(quotation.measureType.Value)
                                        ? CreateQuotation(quotation.measureType.Value, valuationDate,
                                                          PaymentCurrencies[0], decArray.Sum())
                                        : CreateQuotation(quotation.measureType.Value, valuationDate,
                                                          decArray.Sum());
                                vectorQuotation.sensitivitySet = BuildSensitivitySet(quotation.measureType.Value,
                                                                                     decArray, PaymentCurrencies[0]);
                                quotations.Add(vectorQuotation);
                            }
                            else
                            {
                                //if not an array, check if it is a dictionary.
                                if (value is IDictionary<string, decimal> result)
                                {
                                    Quotation vectorQuotation =
                                        RateInstrumentMetricsHelper.IsLocalCurrencyType(quotation.measureType.Value)
                                            ? CreateQuotation(quotation.measureType.Value, valuationDate,
                                                              PaymentCurrencies[0], result.Values.Sum())
                                            : CreateQuotation(quotation.measureType.Value, valuationDate,
                                                              result.Values.Sum());
                                    vectorQuotation.sensitivitySet = BuildSensitivitySet(quotation.measureType.Value,
                                                                                         result);
                                    quotations.Add(vectorQuotation);
                                }
                                else
                                {
                                    //check if it is list of pairs.
                                    if (value is IList<Pair<string, decimal>> otherResult)
                                    {
                                        var sum = 0.0m;
                                        foreach (var pair in otherResult)
                                        {
                                            sum =+ pair.Second;
                                        }
                                        Quotation vectorQuotation = CreateQuotation(quotation.measureType.Value, valuationDate, PaymentCurrencies[0], sum);
                                        vectorQuotation.sensitivitySet = BuildSensitivitySet(quotation.measureType.Value, otherResult);
                                        quotations.Add(vectorQuotation);
                                    }
                                    else
                                    {
                                        Quotation vectorQuotation =
                                        RateInstrumentMetricsHelper.IsLocalCurrencyType(quotation.measureType.Value)
                                            ? CreateQuotation(quotation.measureType.Value, valuationDate,
                                                              PaymentCurrencies[0], (Decimal)value)
                                            : CreateQuotation(quotation.measureType.Value, valuationDate,
                                                              (Decimal)value);
                                        quotations.Add(vectorQuotation);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (RateInstrumentMetricsHelper.IsLocalCurrencyType(quotation.measureType.Value))
                            {
                                quotations.Add(CreateQuotation(quotation.measureType.Value,
                                                               valuationDate,
                                                               PaymentCurrencies[0], 0.0m));
                            }
                            else
                            {
                                quotations.Add(CreateQuotation(quotation.measureType.Value,
                                                               valuationDate,
                                                               0.0m));
                            }
                        }
                    }
                }
            }
            av.quote = quotations.ToArray();
            return av;
        }

        /// <summary>
        /// Evaluates the child metrics.
        /// </summary>
        /// <param name="childControllers">The child controllers.</param>
        /// <param name="modelData">The model data.</param>
        /// <param name="requestedMetrics">The requested metrics.</param>
        /// <returns></returns>
        protected static List<AssetValuation> EvaluateChildMetrics(List<InstrumentControllerBase> childControllers, 
            IInstrumentControllerData modelData, IList<string> requestedMetrics)
        {
            // Get the valuations from the children
            var valuations = new List<AssetValuation>();
            if (requestedMetrics != null && requestedMetrics.Count > 0)
            {
                AssetValuation avInput = AssetValuationHelper.CreateAssetValuation(requestedMetrics, modelData.ValuationDate);
                //modelData.AssetValuation = avInput;
                IInstrumentControllerData childModelData = new InstrumentControllerData(avInput, modelData.MarketEnvironment, modelData.ValuationDate, modelData.ReportingCurrency);
                foreach (var childController in childControllers)
                {
                    var valuation = childController.Calculate(childModelData);
                    valuation.id = childController.Id;
                    valuations.Add(valuation);
                }
            }
            return valuations;
        }

        /// <summary>
        /// Evaluates the child metrics.
        /// </summary>
        /// <typeparam name="FpmLT">The type of the pm LT.</typeparam>
        /// <typeparam name="ParentMetricEnumT">The type of the parent metric enum T.</typeparam>
        /// <typeparam name="ChildModelParamsT">The type of the child model params T.</typeparam>
        /// <typeparam name="ChildMetricEnumT">The type of the child metric enum T.</typeparam>
        /// <param name="childControllers">The child controllers.</param>
        /// <param name="modelData">The model data.</param>
        /// <param name="requestedMetrics">The requested metrics.</param>
        /// <param name="parentMetrics">The parent metrics.</param>
        /// <param name="childMetrics">The child metrics.</param>
        /// <param name="childMetricsList">The child metrics list.</param>
        /// <returns></returns>
        protected static List<AssetValuation> EvaluateChildMetrics<FpmLT, ParentMetricEnumT, ChildModelParamsT, 
            ChildMetricEnumT>(List<IPriceableInstrumentController<FpmLT>> childControllers, 
            IInstrumentControllerData modelData, IList<string> requestedMetrics, List<ParentMetricEnumT> parentMetrics, 
            out List<ChildMetricEnumT> childMetrics, out List<string> childMetricsList)
        {
            const string cEnumError = "{0} must be of type System.Enum";
            // Can't use type constraints on value types, so have to do check like this
            if (typeof(ParentMetricEnumT).BaseType != typeof(Enum))
                throw new ArgumentException(string.Format(cEnumError, "ParentMetricEnumT"));
            if (typeof(ChildMetricEnumT).BaseType != typeof(Enum))
                throw new ArgumentException(string.Format(cEnumError, "ChildMetricEnumT"));
            // Get the valuations from the children
            var valuations = new List<AssetValuation>();
            childMetrics = null;
            childMetricsList = null;
            if (requestedMetrics.Count > 0)
            {
                var parentMetricsList = new List<string>(ConvertEnumToStringArray(parentMetrics.ToArray()));
                childMetricsList = requestedMetrics.Where(requestedMetric => parentMetricsList.Find(item => String.Compare(item, requestedMetric, StringComparison.OrdinalIgnoreCase) == 0) == null).ToList();
            }
            if (childMetricsList != null && childMetricsList.Count > 0)
            {
                valuations = GetChildValuations(childControllers.ToArray(), childMetricsList, modelData);
            }
            return valuations;
        }

        ///// <summary>
        ///// Updates the parent valuation.
        ///// </summary>
        ///// <typeparam name="ParentMetricEnumT">The type of the arent metric enum T.</typeparam>
        ///// <typeparam name="ChildMetricEnumT">The type of the hild metric enum T.</typeparam>
        ///// <typeparam name="RequestMetricT">The type of the equest metric T.</typeparam>
        ///// <param name="parentValuation">The parent valuation.</param>
        ///// <param name="childValuations">The child valuations.</param>
        ///// <param name="parentMetrics">The parent metrics.</param>
        ///// <param name="childMetricsList">The child metrics list.</param>
        ///// <param name="agregateResultInd">if set to <c>true</c> [agregate result ind].</param>
        //protected void UpdateParentValuation<ParentMetricEnumT, ChildMetricEnumT, RequestMetricT>(AssetValuation parentValuation, List<AssetValuation> childValuations, List<ParentMetricEnumT> parentMetrics, List<RequestMetricT> childMetricsList, Boolean agregateResultInd)
        //{
        //    if (childMetricsList != null && childMetricsList.Count > 0)
        //    {
        //        int metricIndex = 0;
        //        foreach (RequestMetricT childMetric in childMetricsList)
        //        {
        //            Boolean bIsBucketingMetric = false;
        //            if (childMetric.ToString().ToLowerInvariant().StartsWith(AggregationType.Bucket.ToString().ToLowerInvariant()))
        //            {
        //                bIsBucketingMetric = true;
        //            }
        //            Boolean existsAsParentMetric = false;
        //            foreach (ParentMetricEnumT parentMetric in parentMetrics)
        //            {
        //                if (string.Compare(parentMetric.ToString(), childMetric.ToString()) == 0)
        //                {
        //                    existsAsParentMetric = true;
        //                    break;
        //                }
        //            }
        //            var results = GetMetricResults(childValuations, childMetric);
        //            Boolean IsVector = false;
        //            foreach (AssetValuation valuation in childValuations)
        //            {
        //                foreach (Quotation quotation in valuation.quote)
        //                {
        //                    if (quotation.sensitivitySet != null)
        //                    {
        //                        IsVector = true;
        //                        break;
        //                    }
        //                }
        //                if (IsVector) break;
        //            }
        //            var aggregatedResults = results;
        //            if (bIsBucketingMetric && IsVector)
        //            {
        //                aggregatedResults = BucketValuationResults(childMetric.ToString(), results);
        //            }
        //            if (!existsAsParentMetric)
        //            {
        //                var aggregatedItems = new List<decimal>();
        //                foreach (string key in aggregatedResults.Keys)
        //                {
        //                    decimal summedResults = SumDecimals(aggregatedResults[key]);
        //                    aggregatedItems.Add(summedResults);
        //                }
        //                if (IsVector)
        //                {
        //                    SensitivitySet[] sensitivitySet = BuildSensitivitySet(childMetric.ToString(), aggregatedItems.ToArray());
        //                    if (agregateResultInd)
        //                    {
        //                        AggregateMetricResult(parentValuation, childMetric, SumDecimals(aggregatedItems.ToArray()), sensitivitySet);
        //                    }
        //                    else
        //                    {
        //                        SetMetricResult(parentValuation, childMetric, SumDecimals(aggregatedItems.ToArray()), sensitivitySet);
        //                    }
        //                }
        //                else
        //                {
        //                    if (agregateResultInd)
        //                    {
        //                        AggregateMetricResult(parentValuation, childMetric, SumDecimals(aggregatedItems.ToArray()));
        //                    }
        //                    else
        //                    {
        //                        SetMetricResult(parentValuation, childMetric, SumDecimals(aggregatedItems.ToArray()));
        //                    }
        //                }
        //            }
        //            metricIndex++;
        //        }
        //    }
        //}

        /// <summary>
        /// Gets the metric value.
        /// </summary>
        /// <typeparam name="TFpML">The type of the fp ML.</typeparam>
        /// <param name="metric">The metric.</param>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        protected Decimal GetMetricValuation<TFpML>(string metric, IInstrumentControllerData modelData)
        {
            Decimal result;
            string[] metricArray = { metric };
            var controller = (IPriceableInstrumentController<TFpML>)this;
            {
                var metricsList = new List<string>(metricArray);
                AssetValuation valuation = GetChildValuation(controller, metricsList, modelData);
                result = GetMetricResult(valuation, metric);
            }
            return result;
        }

        /// <summary>
        /// Gets the metric value.
        /// </summary>
        /// <typeparam name="TFpML">The type of the fp ML.</typeparam>
        /// <param name="metric">The metric.</param>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        protected Decimal GetMetricValue<TFpML>(string metric, IInstrumentControllerData modelData)
        {
            return GetMetricValue((IPriceableInstrumentController<TFpML>)this, metric, modelData);
        }

        /// <summary>
        /// Gets the metric value.
        /// </summary>
        /// <typeparam name="TFpML">The type of the fp ML.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        protected static Decimal GetMetricValue<TFpML>(IPriceableInstrumentController<TFpML> controller, string metric, IInstrumentControllerData modelData)
        {
            Decimal result = 0.0m;
            string[] metricArray = { metric };
            if (controller != null)
            {
                var metricsList = new List<string>(metricArray);
                AssetValuation valuation = GetChildValuation(controller, metricsList, modelData);
                result = GetMetricResult(valuation, metric);
            }
            return result;
        }

        /// <summary>
        /// Gets the child valuations.
        /// </summary>
        /// <typeparam name="TFpmLt">The type of the pm LT.</typeparam>
        /// <param name="childControllers">The child controllers.</param>
        /// <param name="childMetrics">The child metrics.</param>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        protected static List<AssetValuation> GetChildValuations<TFpmLt>(IPriceableInstrumentController<TFpmLt>[] childControllers, List<string> childMetrics, IInstrumentControllerData modelData)
        {
            return childControllers.Select(childController => GetChildValuation(childController, childMetrics, modelData)).ToList();
        }

        /// <summary>
        /// Gets the child valuations.
        /// </summary>
        /// <param name="childControllers">The child controllers.</param>
        /// <param name="childMetrics">The child metrics.</param>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        protected static List<AssetValuation> GetChildValuations(InstrumentControllerBase[] childControllers, List<string> childMetrics, IInstrumentControllerData modelData)
        {
            return childControllers.Select(childController => GetChildValuation(childController, childMetrics, modelData)).ToList();
        }

        /// <summary>
        /// Resolves the model metrics.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="analyticModelMetrics">The analytic model metrics.</param>
        /// <returns></returns>
        protected List<TEnumT> ResolveModelMetrics<TEnumT>(List<TEnumT> analyticModelMetrics)
        {
            return ResolveModelMetrics(Metrics, analyticModelMetrics);
        }

        /// <summary>
        /// Gets the quotation by metric.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="metric">The metric.</param>
        /// <param name="valuation">The valuation.</param>
        /// <returns></returns>
        protected virtual Quotation GetQuotationByMetric<TEnumT>(TEnumT metric, AssetValuation valuation)
        {
            var quotations = new List<Quotation>(valuation.quote);
            return GetQuotationByMetric(metric, quotations);
        }

        /// <summary>
        /// Gets the metric results.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="valuations">The valuations.</param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        protected virtual IDictionary<string, decimal[]> GetMetricResults<TEnumT>(List<AssetValuation> valuations, TEnumT metric)
        {
            IDictionary<string, decimal[]> valuationResults = new Dictionary<string, decimal[]>();
            foreach (AssetValuation valuation in valuations)
            {
                valuationResults.Add(valuation.id, GetMetricResults(valuation, metric));
            }
            return valuationResults;
        }

        /// <summary>
        /// Gets the quotation by metric.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="metric">The metric.</param>
        /// <param name="quotations">The quotations.</param>
        /// <returns></returns>
        protected virtual Quotation GetQuotationByMetric<TEnumT>(TEnumT metric, List<Quotation> quotations)
        {
            return quotations.Find(item => String.Compare(item.measureType.Value, metric.ToString(), StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Updates the valuation.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="metric">The metric.</param>
        /// <param name="sourceValuation">The source valuation.</param>
        /// <param name="targetValuation">The target valuation.</param>
        /// <returns></returns>
        protected virtual void UpdateValuation<TEnumT>(TEnumT metric, AssetValuation sourceValuation, AssetValuation targetValuation)
        {
            Quotation sourceQuotation = GetQuotationByMetric(metric, sourceValuation);
            int matchIndex = 0;
            Boolean bMatchFound = false;
            foreach (Quotation targetQuotation in targetValuation.quote)
            {
                if (String.Compare(targetQuotation.measureType.Value, metric.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    bMatchFound = true;
                    break;
                }
                matchIndex++;
            }
            if (bMatchFound && sourceQuotation != null)
            {
                targetValuation.quote[matchIndex] = sourceQuotation;
            }
        }

        #endregion

        #region Public Methods

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public abstract IList<InstrumentControllerBase> GetChildren();

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Converts the enum to string array.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="enumItems">The enum items.</param>
        /// <returns></returns>
        public static string[] ConvertEnumToStringArray<TEnumT>(TEnumT[] enumItems)
        {
            const string cEnumError = "{0} must be of type System.Enum";

            // Can't use type constraints on value types, so have to do check like this
            if (typeof(TEnumT).BaseType != typeof(Enum))
                throw new ArgumentException(string.Format(cEnumError, "EnumT"));

            return enumItems.Select(enumItem => enumItem.ToString()).ToArray();
        }

        #endregion

        #region Protected Static Methods

        protected static IInstrumentControllerData CreateInstrumentModelData(string[] metrics, DateTime valuationDate, IMarketEnvironment market, string reportingCurrency)
        {
            var bav = new AssetValuation();
            var currency = CurrencyHelper.Parse(reportingCurrency);
            var quotes = new Quotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = QuotationHelper.Create(0.0m, metric);
                index++;
            }
            bav.quote = quotes;
            return new InstrumentControllerData(bav, market, valuationDate, currency);
        }

        /// <summary>
        /// Gets the metric results.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="valuations"> </param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        protected static decimal[] GetMetricArrayResults<TEnumT>(List<AssetValuation> valuations, TEnumT metric)
        {
            return GetMetricArrayResults(valuations, metric.ToString());
        }

        /// <summary>
        /// Gets the metric results.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        protected static decimal[] GetMetricResults<TEnumT>(AssetValuation valuation, TEnumT metric)
        {
            return GetMetricResults(valuation, metric.ToString());
        }

        /// <summary>
        /// Sets the metric result.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">The value.</param>
        protected static void SetMetricResult<TEnumT>(AssetValuation valuation, TEnumT metric, Decimal value)
        {
            var quotes = new List<Quotation>(valuation.quote);
            Quotation matchedQuote = quotes.Find(
                item => String.Compare(item.measureType.Value, metric.ToString(), StringComparison.OrdinalIgnoreCase) == 0
                );
            if (matchedQuote == null) return;
            matchedQuote.value = value;
            matchedQuote.valueSpecified = true;
        }

        /// <summary>
        /// Aggregates the metric result.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">The value.</param>
        protected static void AggregateMetricResult<TEnumT>(AssetValuation valuation, TEnumT metric, Decimal value)
        {
            var quotes = new List<Quotation>(valuation.quote);
            Quotation matchedQuote = quotes.Find(
                item => String.Compare(item.measureType.Value, metric.ToString(), StringComparison.OrdinalIgnoreCase) == 0
                );
            if (matchedQuote == null) return;
            matchedQuote.value += value;
            matchedQuote.valueSpecified = true;
        }

        /// <summary>
        /// Sets the metric result.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">The value.</param>
        /// <param name="sensitivitySet">The sensitivity set.</param>
        protected static void SetMetricResult<TEnumT>(AssetValuation valuation, TEnumT metric, Decimal value, SensitivitySet[] sensitivitySet)
        {
            var quotes = new List<Quotation>(valuation.quote);
            Quotation matchedQuote = quotes.Find(
                item => String.Compare(item.measureType.Value, metric.ToString(), StringComparison.OrdinalIgnoreCase) == 0
                );
            if (matchedQuote == null) return;
            matchedQuote.value = value;
            matchedQuote.valueSpecified = true;
            matchedQuote.sensitivitySet = sensitivitySet;
        }

        /// <summary>
        /// Aggregates the metric result.
        /// </summary>
        /// <typeparam name="TEnumT">The type of the num T.</typeparam>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">The value.</param>
        /// <param name="sensitivitySet">The sensitivity set.</param>
        protected static void AggregateMetricResult<TEnumT>(AssetValuation valuation, TEnumT metric, Decimal value, SensitivitySet[] sensitivitySet)
        {
            var quotes = new List<Quotation>(valuation.quote);
            Quotation matchedQuote = quotes.Find(
                item => String.Compare(item.measureType.Value, metric.ToString(), StringComparison.OrdinalIgnoreCase) == 0
                );
            if (matchedQuote == null) return;
            matchedQuote.value+= value;
            matchedQuote.valueSpecified = true;
            matchedQuote.sensitivitySet = sensitivitySet;
        }

        /// <summary>
        /// Creates the quotation.
        /// </summary>
        /// <param name="measureTypeValue">The measure type value.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected static Quotation CreateQuotation(string measureTypeValue, DateTime valuationDate, Decimal value)
        {
            var newQuotation = new Quotation();
            var measureType = new AssetMeasureType {Value = measureTypeValue};
            newQuotation.measureType = measureType;
            newQuotation.value = value;
            newQuotation.valueSpecified = true;
            newQuotation.valuationDate = valuationDate;
            newQuotation.valuationDateSpecified = true;
            return newQuotation;
        }

        /// <summary>
        /// Creates the quotation.
        /// </summary>
        /// <param name="measureTypeValue">The measure type value.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="currency">The currency </param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected static Quotation CreateQuotation(string measureTypeValue, DateTime valuationDate, string currency, Decimal value)
        {
            var newQuotation = new Quotation();
            var measureType = new AssetMeasureType { Value = measureTypeValue };
            newQuotation.measureType = measureType;
            newQuotation.value = value;
            newQuotation.valueSpecified = true;
            newQuotation.valuationDate = valuationDate;
            newQuotation.valuationDateSpecified = true;
            newQuotation.currency = CurrencyHelper.Parse(currency);
            return newQuotation;
        }

        /// <summary>
        /// Adds the sensitivity set.
        /// </summary>
        /// <param name="measureType">The metrics.</param>
        /// <param name="measureValues">The analytic results.</param>
        /// <param name="currency">The currency. </param>
        /// <returns></returns>
        protected static SensitivitySet[] BuildSensitivitySet(string measureType, Decimal[] measureValues, string currency)
        {
            var sensitivitySetList = new List<SensitivitySet>();
            var sensitivityList = new List<Sensitivity>();
            var sensitivitySet = new SensitivitySet {name = measureType};
            int index = 1;
            foreach (var measureValue in measureValues)
            {
                var sensitivity = new Sensitivity { name = currency + '.' + (index - 1).ToString(CultureInfo.InvariantCulture), Value = measureValue };
                sensitivityList.Add(sensitivity);
                index++;
            }
            sensitivitySet.sensitivity = sensitivityList.ToArray();
            sensitivitySetList.Add(sensitivitySet);
            return sensitivitySetList.ToArray();
        }

        /// <summary>
        /// Adds the sensitivity set.
        /// </summary>
        /// <param name="measureType">The metrics.</param>
        /// <param name="measureValues">The analytic results.</param>
        /// <returns></returns>
        protected static SensitivitySet[] BuildSensitivitySet(string measureType, IDictionary<string, decimal> measureValues)
        {
            var sensitivitySetList = new List<SensitivitySet>();
            var sensitivitySet = new SensitivitySet
                                     {
                                         name = measureType,
                                         sensitivity =
                                             measureValues.Select(
                                                 measureValue =>
                                                 new Sensitivity {name = measureValue.Key, Value = measureValue.Value}).
                                             ToArray()
                                     };
            sensitivitySetList.Add(sensitivitySet);
            return sensitivitySetList.ToArray();
        }

        /// <summary>
        /// Adds the sensitivity set.
        /// </summary>
        /// <param name="measureType">The metrics.</param>
        /// <param name="measureValues">The analytic results.</param>
        /// <returns></returns>
        protected static SensitivitySet[] BuildSensitivitySet(string measureType, IList<Pair<string, decimal>> measureValues)
        {
            var sensitivitySetList = new List<SensitivitySet>();
            var sensitivitySet = new SensitivitySet
            {
                name = measureType,
                sensitivity =
                    measureValues.Select(
                        measureValue =>
                        new Sensitivity { name = measureValue.First, Value = measureValue.Second }).
                    ToArray()
            };
            sensitivitySetList.Add(sensitivitySet);
            return sensitivitySetList.ToArray();
        }
 
        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the metric results. Does not handle valuation sets.
        /// </summary>
        /// <param name="valuations">The valuations.</param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        protected static decimal[] GetMetricArrayResults(List<AssetValuation> valuations, string metric)
        {
            var results = new List<decimal>();
            foreach (var valuation in valuations)
            {
                var quotes = new List<Quotation>(valuation.quote);
                var matchedQuotes = quotes.FindAll(item => String.Compare(item.measureType.Value, metric, StringComparison.OrdinalIgnoreCase) == 0);
                if (matchedQuotes.Count == 1)
                {
                    results.AddRange(matchedQuotes.Select(quotation => quotation.value));
                }
            }
            return results.ToArray();
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
        /// Gets the metric result.
        /// </summary>
        /// <param name="valuation">The valuation.</param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        private static decimal GetMetricResult(AssetValuation valuation, string metric)
        {
            decimal result = 0.0m;
            var quotes = new List<Quotation>(valuation.quote);
            Quotation matchedQuote = quotes.Find(item => String.Compare(item.measureType.Value, metric, StringComparison.OrdinalIgnoreCase) == 0);
            if (matchedQuote != null)
            {
                result = matchedQuote.value;
            }
            return result;
        }

        /// <summary>
        /// Finds the enum from string.
        /// </summary>
        /// <typeparam name="EnumT">The type of the num T.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="findItem">The find item.</param>
        /// <returns></returns>
        private static EnumT FindEnumFromString<EnumT>(List<EnumT> list, string findItem)
        {
            return list.Find(metricItem => String.Compare(metricItem.ToString(), findItem, StringComparison.OrdinalIgnoreCase) == 0
                );
        }

        /// <summary>
        /// Gets the metrics from quotes.
        /// </summary>
        /// <param name="quotes">The quotes.</param>
        /// <returns></returns>
        private static List<string> GetMetricsFromQuotes(IEnumerable<Quotation> quotes)
        {
            var metrics = new List<string>();
            if (quotes != null)
            {
                metrics.AddRange(quotes.Select(quotation => quotation.measureType.Value));
            }
            return metrics;
        }

        #endregion
    }
}