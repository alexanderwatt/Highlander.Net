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
using System.ComponentModel.Composition;
using System.Linq;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Metadata.Common;
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Identifiers;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;
using Exception = System.Exception;

#endregion

namespace Highlander.Workflow.CurveGeneration.V5r3
{
    [Export(typeof(IRequestHandler<RequestBase, HandlerResponse>))]
    public class WFGenerateStressedCurve : WFGenerateCurveBase, IRequestHandler<RequestBase, HandlerResponse>
    {
        #region IRequestHandler<RequestBase,HandlerResponse> Members

        public void InitialiseRequest(ILogger logger, ICoreCache cache)
        {
            Initialise(new WorkContext(logger, cache, null));
        }

        public void ProcessRequest(RequestBase baseRequest, HandlerResponse response)
        {
            if (baseRequest == null)
                throw new ArgumentNullException(nameof(baseRequest));
            if (!(baseRequest is StressedCurveGenRequest request))
                throw new InvalidCastException(
                    $"{typeof(RequestBase).Name} is not a {typeof(StressedCurveGenRequest).Name}");
            // check for workflow cancellation
            if (Cancelled)
                throw new OperationCanceledException(CancelReason);
            DateTime lastStatusPublishedAt = DateTime.Now;
            CurveSelection[] curveSelectors = request.CurveSelector ?? new List<CurveSelection>().ToArray();

            #region Load stress rules

            //find the uniques namespaces
            var uniquesNameSpaces = new List<string>();
            foreach (CurveSelection curveSelector in curveSelectors)
            {
                if (!uniquesNameSpaces.Contains(curveSelector.NameSpace))
                {
                    uniquesNameSpaces.Add(curveSelector.NameSpace);
                }
            }
            var cachedStressRules = new Dictionary<string, List<CachedStressRule>>();
            {
                IExpression queryExpr = Expr.IsEQU(EnvironmentProp.NameSpace, uniquesNameSpaces[0]);//TODO only does the first namespace....
                List<StressRule> storedStressRules = Context.Cache.LoadObjects<StressRule>(queryExpr);
                foreach (StressRule storedStressRule in storedStressRules)
                {
                    if ((storedStressRule.Disabled) || (storedStressRule.StressId == null)) continue;
                    string key = storedStressRule.StressId.ToLower();
                    if (!cachedStressRules.TryGetValue(key, out var rules))
                    {
                        rules = new List<CachedStressRule>();
                        cachedStressRules[key] = rules;
                    }
                    rules.Add(new CachedStressRule(storedStressRule));
                    rules.Sort();
                }
            }
            #endregion

            response.ItemCount = curveSelectors.Length * cachedStressRules.Count;
            // iterate selected base curves
            foreach (CurveSelection curveSelector in curveSelectors)
            {
                // check for workflow cancellation
                if (Cancelled)
                    throw new OperationCanceledException(CancelReason);
                // publish 'intermediate' in-progress result (throttled)
                if ((DateTime.Now - lastStatusPublishedAt) > TimeSpan.FromSeconds(5))
                {
                    lastStatusPublishedAt = DateTime.Now;
                    response.Status = RequestStatusEnum.InProgress;
                    Context.Cache.SaveObject(response);
                }
                string nameSpace = curveSelector.NameSpace;
                string inputMarketName = curveSelector.MarketName;
                var marketDate = curveSelector.MarketDate;
                if (marketDate != null && marketDate != DateTime.MinValue)
                {
                    inputMarketName += "." + ((DateTime)marketDate).ToString(CurveProp.MarketDateFormat);
                }
                string inputCurveName = curveSelector.CurveName;
                string inputCurveType = curveSelector.CurveType;
                Context.Logger.LogDebug("Building stressed curve(s): {0}.{1}.{2}", inputMarketName, inputCurveType, inputCurveName);

                #region Load base curve

                var curveGenProps = new NamedValueSet();
                curveGenProps.Set(CurveProp.BaseDate, request.BaseDate);
                IPricingStructureIdentifier baseCurveId =
                    PricingStructureIdentifier.CreateMarketCurveIdentifier(curveGenProps, inputMarketName, null, inputCurveType, inputCurveName, null);
                var baseCurveUniqueId = baseCurveId.Properties.GetValue<string>(CurveProp.UniqueIdentifier, true);
                ICoreItem baseCurveItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, baseCurveUniqueId);
                var stressNameProp = baseCurveItem.AppProps.GetValue<string>(CurveProp.StressName, null);
                if (stressNameProp != null)
                    throw new ApplicationException("The Market with name '" + baseCurveUniqueId + "' is NOT a base curve! (Stress name is not null)");
                var baseCurveFpml = (Market)baseCurveItem.Data;
                var baseCurveType = PropertyHelper.ExtractPricingStructureType(baseCurveItem.AppProps);

                #endregion

                #region Load the reference curves - if required

                string fxCurveName = null, refCurveName = null, quoteCurveName = null;
                NamedValueSet fxProperties = null, refProperties = null, quoteProperties = null;
                Market fxMarket = null, refMarket = null, quoteMarket = null;
                if (baseCurveType == PricingStructureTypeEnum.RateBasisCurve
                    || baseCurveType == PricingStructureTypeEnum.RateXccyCurve)
                {
                    // rate basis curves require a reference curve
                    refCurveName = baseCurveItem.AppProps.GetValue<string>(CurveProp.ReferenceCurveUniqueId, true);
                    // load the reference curve
                    var refCurveItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, refCurveName);
                    refMarket = (Market)refCurveItem.Data;
                    refProperties = refCurveItem.AppProps;
                }
                if (baseCurveType == PricingStructureTypeEnum.RateXccyCurve)
                {
                    // rate basis curves require an fx curve
                    fxCurveName = baseCurveItem.AppProps.GetValue<string>(CurveProp.ReferenceFxCurveUniqueId, true);
                    // load the reference curve
                    var fxCurveItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, fxCurveName);
                    fxMarket = (Market)fxCurveItem.Data;
                    fxProperties = fxCurveItem.AppProps;
                    // rate basis curves require a reference curve
                    quoteCurveName = baseCurveItem.AppProps.GetValue<string>(CurveProp.ReferenceCurrency2CurveId, true);
                    // load the reference curve
                    var quoteCurveItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, quoteCurveName);
                    quoteMarket = (Market)quoteCurveItem.Data;
                    quoteProperties = quoteCurveItem.AppProps;
                }
                #endregion

                // process stress rules
                foreach (var kvp in cachedStressRules)
                {
                    CachedStressRule stressRule = kvp.Value.FirstOrDefault(item => (item.FilterExpr == null) || (Expr.CastTo(item.FilterExpr.Evaluate(baseCurveItem.AppProps), false)));
                    // find stress rule that applies
                    if (stressRule == null)
                    {
                        // this stress does not apply to this base curve
                        Context.Logger.LogWarning("Stress '{0}' does not apply to base curve '{1}'!", kvp.Key, baseCurveUniqueId);
                        response.IncrementItemsPassed();
                        continue;
                    }
                    // apply the stress rule
                    //_Context.Logger.LogDebug("Applying stress '{0}' (rule {1}) to base curve '{2}'", stressRule.StressId, stressRule.RuleId, baseCurveUniqueId);
                    var stressDefProps = new NamedValueSet(baseCurveItem.AppProps, curveGenProps);
                    stressDefProps.Set("Identifier", null);//THis is done for backward compatibility with the old rate curves.
                    stressDefProps.Set(CurveProp.BaseCurveType, baseCurveType);
                    IPricingStructureIdentifier stressCurveId = PricingStructureIdentifier.CreateMarketCurveIdentifier(
                        stressDefProps, inputMarketName, null, baseCurveType.ToString(), inputCurveName, stressRule.StressId);
                    NamedValueSet stressCurveProps = stressCurveId.Properties;
                    var stressCurveName = stressCurveProps.GetValue<string>(CurveProp.UniqueIdentifier, true);
                    // from here on a curve will be published (with error details included)
                    var stressCurve = new Market(); // empty
                    try
                    {
                        // clone the base curve and adjust the market quotes
                        var ps = BinarySerializerHelper.Clone(baseCurveFpml.Items[0]);
                        PricingStructureValuation psv = ApplyStress(stressRule, baseCurveFpml.Items1[0]);
                        // hack - supply base date
                        psv.baseDate = new IdentifiedDate { Value = request.BaseDate };
                        Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> refCurveFpmlTriplet = null;
                        if (baseCurveType == PricingStructureTypeEnum.RateBasisCurve
                            || baseCurveType == PricingStructureTypeEnum.RateXccyCurve)
                        {
                            var psRef = BinarySerializerHelper.Clone(refMarket.Items[0]);
                            //var psvRef = BinarySerializerHelper.Clone<PricingStructureValuation>(refcurveFpml.Items1[0]);
                            var psvRef = ApplyStress(stressRule, refMarket.Items1[0]);
                            refCurveFpmlTriplet =
                                new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                                    psRef, psvRef, refProperties);
                        }
                        IPricingStructure ips;
                        switch (baseCurveType)
                        {
                            case PricingStructureTypeEnum.RateBasisCurve:
                                stressCurveProps.Set(CurveProp.ReferenceCurveUniqueId, refCurveName);
                                var basisCurveFpmlTriplet =
                                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                                        ps, psv, stressCurveProps);
                                //create and set the pricing structure
                                ips = CurveLoader.LoadInterestRateCurve(Context.Logger, Context.Cache, nameSpace, refCurveFpmlTriplet, basisCurveFpmlTriplet);
                                //Creator.Create(refCurveFpmlTriplet, basisCurveFpmlTriplet);
                                break;
                            case PricingStructureTypeEnum.RateXccyCurve:
                                stressCurveProps.Set(CurveProp.ReferenceCurveUniqueId, refCurveName);
                                stressCurveProps.Set(CurveProp.ReferenceFxCurveUniqueId, fxCurveName);
                                stressCurveProps.Set(CurveProp.ReferenceCurrency2CurveId, quoteCurveName);
                                var xccyCurveFpmlTriplet =
                                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(ps, psv,
                                                                                                            stressCurveProps);
                                //Format the ref curve data and call the pricing structure helper.
                                var psvFx = ApplyStress(stressRule, fxMarket.Items1[0]);
                                var fxCurveFpmlTriplet
                                    = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                                        fxMarket.Items[0],
                                        psvFx, fxProperties);

                                var psvRef = ApplyStress(stressRule, quoteMarket.Items1[0]);
                                var quoteCurveFpmlTriplet
                                    = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                                        quoteMarket.Items[0],
                                        psvRef, quoteProperties);
                                //create and set the pricing structure
                                ips = CurveLoader.LoadInterestRateCurve(Context.Logger, Context.Cache, nameSpace, refCurveFpmlTriplet, fxCurveFpmlTriplet, quoteCurveFpmlTriplet,
                                                         xccyCurveFpmlTriplet);
                                //Creator.Create(refCurveFpmlTriplet, fxCurveFpmlTriplet, quoteCurveFpmlTriplet, xccyCurveFpmlTriplet);
                                break;
                            default:
                                ips = CurveLoader.LoadCurve(Context.Logger, Context.Cache,
                                        nameSpace, new Pair<PricingStructure, PricingStructureValuation>(ps, psv),
                                        stressCurveProps);
                                //Creator.Create( new Pair<PricingStructure, PricingStructureValuation>(ps, psv), stressCurveProps);
                                break;
                        }
                        var identifier = ips.GetPricingStructureId().UniqueIdentifier;
                        // retrieve curve
                        stressCurve = PricingStructureHelper.CreateMarketFromFpML(
                            identifier,
                            ips.GetFpMLData());
                        // curve done
                        response.IncrementItemsPassed();
                    }
                    catch (Exception innerExcp)
                    {
                        response.IncrementItemsFailed();
                        Context.Logger.Log(innerExcp);
                        stressCurveProps.Set(WFPropName.ExcpName, WFHelper.GetExcpName(innerExcp));
                        stressCurveProps.Set(WFPropName.ExcpText, WFHelper.GetExcpText(innerExcp));
                    }
                    // save stressed curve with same lifetime as base curve
                    stressCurveProps.Set(EnvironmentProp.NameSpace, nameSpace);
                    Context.Cache.SaveObject(stressCurve, nameSpace + "." + stressCurveName, stressCurveProps, true, baseCurveItem.Expires);
                } // foreach stress rule

            } // foreach base curve
            // success
            response.Status = RequestStatusEnum.Completed;
        }
        
        public Type HandledRequestType => typeof(StressedCurveGenRequest);

        #endregion

        //private void PostStressedCurveGenRequest(string marketName, DateTime? marketDate, string curveType, string curveName)
        //{
        //    List<CurveSelection> curveSelectors = new List<CurveSelection>();
        //    curveSelectors.Add(new CurveSelection()
        //    {
        //        MarketName = marketName,
        //        MarketDate = marketDate,
        //        CurveType = curveType,
        //        CurveName = curveName
        //    });
        //    StressedCurveGenRequest curveGenRequest = new StressedCurveGenRequest()
        //    {
        //        RequestId = Guid.NewGuid().ToString(),
        //        RequesterId = new UserIdentity()
        //        {
        //            Name = _Context.Cache.ClientInfo.Name,
        //            DisplayName = "Curve Generator"
        //        },
        //        RequestDescription = String.Format("StressGen [{0}][{1}][{2}]", marketName, curveType, curveName),
        //        BaseDate = DateTime.Now.Date,
        //        CurveSelector = curveSelectors.ToArray()
        //    };
        //    //_WorkerThreadQueue.Dispatch<StressedCurveGenRequest>(curveGenRequest, BackgroundGenerateStressedCurves);
        //    _Context.Cache.SaveObject<StressedCurveGenRequest>(curveGenRequest);
        //    RequestBase.DispatchToManager(_Context.Cache, curveGenRequest);
        //}

        protected override HandlerResponse OnExecute(RequestBase baseRequest)
        {
            if (baseRequest == null)
                throw new ArgumentNullException(nameof(baseRequest));
            var request = baseRequest as StressedCurveGenRequest;
            if (request == null)
                throw new ArgumentNullException(nameof(baseRequest));
            // publish 'initial' status
            var response = new HandlerResponse
                                           {
                NameSpace = EnvironmentProp.DefaultNameSpace,//TODO this will need to be fixed eventually.
                RequestId = request.RequestId,
                RequesterId = request.RequesterId,
                Status = RequestStatusEnum.Commencing,
                CommenceTime = DateTimeOffset.Now.ToString("o"),
                ItemCount = 0,
                ItemsPassed = 0,
                ItemsFailed = 0,
                FaultDetail = null
            };
            Context.Cache.SaveObject(response);
            try
            {
                ProcessRequest(request, response);
            }
            catch (OperationCanceledException cancelExcp)
            {
                response.Status = RequestStatusEnum.Cancelled;
                response.CancelReason = cancelExcp.Message;
            }
            catch (Exception outerExcp)
            {
                response.Status = RequestStatusEnum.Faulted;
                response.FaultDetail = new ExceptionDetail(outerExcp);
                Context.Logger.Log(outerExcp);
            }
            // publish 'completed' status
            response.CompleteTime = DateTimeOffset.Now.ToString("o");
            Context.Cache.SaveObject(response);
            return response;
        }

        private static PricingStructureValuation ApplyStress(CachedStressRule stressRule, PricingStructureValuation psvInput)
        {
            string marketQuote = AssetMeasureScheme.GetEnumString(AssetMeasureEnum.MarketQuote);
            string decimalRate = PriceQuoteUnitsScheme.GetEnumString(PriceQuoteUnitsEnum.DecimalRate);
            var psv = BinarySerializerHelper.Clone(psvInput);
            // extract the market quotes from the cloned base curve
            QuotedAssetSet curveDefinition;
            if (psv is YieldCurveValuation yieldCurveValuation)
            {
                curveDefinition = yieldCurveValuation.inputs;
            }
            else
            {
                if (psv is FxCurveValuation curveValuation)
                {
                    curveDefinition = new QuotedAssetSet
                        {
                            instrumentSet = curveValuation.spotRate.instrumentSet,
                            assetQuote = curveValuation.spotRate.assetQuote
                        };
                }
                else
                    throw new NotSupportedException("Unsupported PricingStructureValuation type: " +
                                                    psv.GetType().Name);
            }
            // stress the market quotes
            foreach (BasicAssetValuation asset in curveDefinition.assetQuote)
            {
                var stressDefQuotes = new List<BasicQuotation>();
                foreach (BasicQuotation quote in asset.quote)
                {
                    if (quote.measureType.Value.Equals(marketQuote)
                        && quote.quoteUnits.Value.Equals(decimalRate))
                    {
                        var exprProps = new NamedValueSet(new NamedValue("MarketQuote", quote.value));
                        quote.valueSpecified = true;
                        quote.value = Convert.ToDecimal(stressRule.UpdateExpr.Evaluate(exprProps));
                    }
                    quote.informationSource = null;
                    quote.timeSpecified = false;
                    quote.valuationDateSpecified = false;
                    stressDefQuotes.Add(quote);
                }
                asset.quote = stressDefQuotes.ToArray();
            }
            // replace the market quotes in the cloned base curve with the stressed values
            if (psv is YieldCurveValuation valuation)
            {
                valuation.inputs = curveDefinition;
                valuation.discountFactorCurve = null;
                valuation.zeroCurve = null;
            }
            else
            {
                ((FxCurveValuation)psv).spotRate
                    = new FxRateSet
                          {
                              instrumentSet = curveDefinition.instrumentSet,
                              assetQuote = curveDefinition.assetQuote
                          };
            }
            return psv;
        }
    }
}
