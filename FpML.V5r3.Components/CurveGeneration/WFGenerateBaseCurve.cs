/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Common;
using FpML.V5r3.Reporting;
using Metadata.Common;
using Orion.Constants;
using Orion.Contracts;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Factory;
using Orion.MDAS.Client;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Identifiers;
using Orion.Util.RefCounting;
using Exception = System.Exception;

#endregion

namespace Orion.Workflow.CurveGeneration
{
    public class WFGenerateOrdinaryCurve : WFGenerateCurveBase, IRequestHandler<RequestBase, HandlerResponse>
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
            var request = baseRequest as OrdinaryCurveGenRequest;
            if (request == null)
                throw new InvalidCastException(
                    $"{typeof(RequestBase).Name} is not a {typeof(OrdinaryCurveGenRequest).Name}");
            CurveSelection[] curveSelectors = request.CurveSelector ?? new List<CurveSelection>().ToArray();
            response.ItemCount = curveSelectors.Length;
            DateTime lastStatusPublishedAt = DateTime.Now;
            // check for workflow cancellation
            if (Cancelled)
                throw new OperationCanceledException(CancelReason);
            // iterate selected curves
            foreach (CurveSelection curveSelector in curveSelectors)
            {
                // publish 'intermediate' in-progress result (throttled)
                if ((DateTime.Now - lastStatusPublishedAt) > TimeSpan.FromSeconds(5))
                {
                    lastStatusPublishedAt = DateTime.Now;
                    response.Status = RequestStatusEnum.InProgress;
                    Context.Cache.SaveObject(response);
                }
                string nameSpace = curveSelector.NameSpace;
                string inputMarketName = curveSelector.MarketName;
                string inputCurveName = curveSelector.CurveName;
                string inputCurveType = curveSelector.CurveType;
                // given a curve definition, this workflow generates:
                // - a live base curve using current market data
                // load curve definition
                Context.Logger.LogDebug("Building ordinary curve: {0}.{1}.{2}", inputMarketName, inputCurveType, inputCurveName);
                string curveUniqueId =
                    $"Configuration.PricingStructures.{inputMarketName}.{inputCurveType}.{inputCurveName}";
                //TODO This does not work for MArket=Test_EOD because the market date propeerty 
                //is not included in the identifier and unique identifier!
                ICoreItem marketItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, curveUniqueId);
                // check data is not mutated
                //AssertNotModified<Market>(marketItem);
                // note: we must clone the definition to avoid updating it in the cache!
                var market = marketItem.GetData<Market>(true);
                //AssertSomeQuotesMissing(((YieldCurveValuation)(cachedMarket.Items1[0])).inputs);
                //Market clonedMarket = BinarySerializerHelper.Clone<Market>(cachedMarket);
                PricingStructure ps = market.Items[0];
                PricingStructureValuation psv = market.Items1[0];
                // supply base data and  build datetime
                psv.baseDate = new IdentifiedDate { Value = request.BaseDate };
                QuotedAssetSet curveDefinition;
                if (psv is YieldCurveValuation curveValuation)
                {
                    curveDefinition = curveValuation.inputs;
                }
                else
                {
                    if (psv is FxCurveValuation valuation)
                    {
                        curveDefinition = valuation.spotRate;
                    }
                    else
                        throw new NotSupportedException("Unsupported PricingStructureValuation type: " + psv.GetType().Name);
                }
                //AssertSomeQuotesMissing(curveDefinition);
                // default outputs
                var curveDefProps = new NamedValueSet(marketItem.AppProps);
                var curveType = PropertyHelper.ExtractPricingStructureType(curveDefProps);//.GetValue<string>(CurveProp.PricingStructureType, true));
                var curveName = curveDefProps.GetValue<string>(CurveProp.CurveName, true);
                string marketDataItemName = String.Format(FunctionProp.QuotedAssetSet.ToString() + ".{0}.{1}.{2}", inputMarketName, curveType, curveName);
                curveDefProps.Set("BootStrap", true);
                curveDefProps.Set(CurveProp.BaseDate, request.BaseDate);
                IPricingStructureIdentifier liveCurveId = PricingStructureIdentifier.CreateMarketCurveIdentifier(curveDefProps, inputMarketName, null, null, null, null);
                NamedValueSet liveCurveProps = liveCurveId.Properties;
                var liveCurveItemName = liveCurveProps.GetValue<string>(CurveProp.UniqueIdentifier, true);
                var liveCurve = new Market(); // empty
                try
                {
                    // build a request/response map (indexed by instrument id)
                    var instrumentMap = new Dictionary<string, Asset>();
                    foreach (Asset asset in curveDefinition.instrumentSet.Items)
                    {
                        instrumentMap[asset.id.ToLower()] = asset;
                    }
                    int bavNum = 0;
                    foreach (BasicAssetValuation quoteInstr in curveDefinition.assetQuote)
                    {
                        if (quoteInstr.objectReference?.href == null)
                            throw new ApplicationException($"Missing objectReference in BasicAssetValuation[{bavNum}]");
                        string instrId = quoteInstr.objectReference.href;
                        Asset asset;
                        if (!instrumentMap.TryGetValue(instrId.ToLower(), out asset))
                            throw new ApplicationException($"Cannot find instrument '{instrId}' for assetQuote");
                        bavNum++;
                    }
                    // request market data from MDS
                    QuotedAssetSet marketData;
                    if (request.UseSavedMarketData)
                    {
                        // get saved market data
                        marketData = Context.Cache.LoadObject<QuotedAssetSet>(nameSpace + "." + marketDataItemName);
                        if (marketData == null)
                            throw new ApplicationException(
                                $"Could not load saved market data with name: '{marketDataItemName}'");
                    }
                    else
                    {
                        //throw new NotImplementedException();
                        using (var mdc = MarketDataFactory.Create(Reference<ILogger>.Create(Context.Logger), Assembly.GetExecutingAssembly(), null))
                        {
                            // call MDS
                            //AssertSomeQuotesMissing(curveDefinition);
                            Guid mdsRequestId = Guid.NewGuid();
                            MDSResult<QuotedAssetSet> mdsResponse = mdc.GetMarketQuotes(
                                MDSProviderId.Bloomberg, null, mdsRequestId, true, null,
                                curveDefinition);
                            if (mdsResponse.Error != null)
                            {
                                throw mdsResponse.Error;
                            }
                            marketData = mdsResponse.Result;
                            if ((marketData.assetQuote == null) || marketData.assetQuote.Length < 1)
                            {
                                throw new ApplicationException($"MDS response contains no quotes! ({mdsRequestId})");
                            }

                            // save transient market data for later offline use
                            if (request.SaveMarketData)
                            {
                                var marketDataProps = new NamedValueSet();
                                marketDataProps.Set(liveCurveProps.Get(EnvironmentProp.NameSpace));//TODO Added to filter on client namespace!
                                marketDataProps.Set(liveCurveProps.Get(CurveProp.Market));
                                marketDataProps.Set(liveCurveProps.Get(CurveProp.PricingStructureType));
                                marketDataProps.Set(liveCurveProps.Get(CurveProp.CurveName));
                                marketDataProps.Set(liveCurveProps.Get(CurveProp.Currency1));
                                Context.Cache.SaveObject(marketData, marketDataItemName, marketDataProps, true, TimeSpan.FromDays(7));
                            }
                        }
                    }
                    // check market data for undefined/invalid quotes
                    foreach (BasicAssetValuation asset in marketData.assetQuote)
                    {
                        if (asset.quote.Any(quote => quote.measureType.Value.Equals("undefined", StringComparison.OrdinalIgnoreCase)))
                        {
                            throw new ApplicationException(
                                $"Market quote undefined/missing for asset '{asset.objectReference.href}'");
                        }
                    }
                    // merge MDS results with stored quotes in the curve definition
                    curveDefinition.Replace(marketData);//Merge(marketData, true, false, true);
                    // generate ordinary base curve
                    var valuation = psv as YieldCurveValuation;
                    if (valuation != null)
                    {
                        valuation.inputs = curveDefinition;
                    }
                    else
                    {
                        ((FxCurveValuation)psv).spotRate = new FxRateSet
                                                               {
                                                                   instrumentSet = curveDefinition.instrumentSet,
                                                                   assetQuote = curveDefinition.assetQuote
                                                               };
                    }
                    // hack - if rate basis curve then call new triplet fn, else call old pair fn.
                    IPricingStructure ips;
                    switch (curveType)
                    {
                        case PricingStructureTypeEnum.RateBasisCurve:
                            {
                                // rate basis curves require a reference curve
                                string refCurveUniqueId =
                                    $"Market.{inputMarketName}.{curveDefProps.GetValue<string>(CurveProp.ReferenceCurveName, true)}";
                                // load the reference curve
                                ICoreItem refCurveItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, refCurveUniqueId);
                                var refCurve = (Market)refCurveItem.Data;
                                //Format the ref curve data and call the pricing structure helper.
                                var refCurveFpMLTriplet = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(refCurve.Items[0],
                                                                                                 refCurve.Items1[0], refCurveItem.AppProps);
                                liveCurveProps.Set(CurveProp.ReferenceCurveUniqueId, refCurveUniqueId);
                                var spreadCurveFpMLTriplet = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(ps, psv, liveCurveProps);
                                //create and set the pricingstructure
                                ips = CurveLoader.LoadInterestRateCurve(Context.Logger, Context.Cache, nameSpace, refCurveFpMLTriplet, spreadCurveFpMLTriplet);
                                //Creator.Create(refCurveFpMLTriplet, spreadCurveFpMLTriplet);
                            }
                            break;
                        case PricingStructureTypeEnum.RateXccyCurve:
                            {
                                // rate basis curves require a base curve
                                string baseCurveUniqueId = String.Format(nameSpace + ".Market.{0}.{1}",
                                    inputMarketName, curveDefProps.GetValue<string>(CurveProp.ReferenceCurveName, true));
                                // load the reference curve
                                ICoreItem baseCurveItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, baseCurveUniqueId);
                                var baseCurve = (Market)baseCurveItem.Data;
                                // rate basis curves require an fx curve
                                string fxCurveUniqueId = String.Format(nameSpace + ".Market.{0}.{1}",
                                    inputMarketName, curveDefProps.GetValue<string>(CurveProp.ReferenceFxCurveName, true));
                                // load the reference curve
                                ICoreItem fxCurveItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, fxCurveUniqueId);
                                var fxCurve = (Market)fxCurveItem.Data;
                                // rate basis curves require a reference curve
                                string refCurveUniqueId = String.Format(nameSpace + ".Market.{0}.{1}",
                                    inputMarketName, curveDefProps.GetValue<string>(CurveProp.ReferenceCurrency2CurveName, true));
                                // load the reference curve
                                ICoreItem refCurveItem = LoadAndCheckMarketItem(Context.Cache, nameSpace, refCurveUniqueId);
                                var refCurve = (Market)refCurveItem.Data;
                                //Format the ref curve data and call the pricing structure helper.
                                var baseCurveFpMLTriplet = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(baseCurve.Items[0],
                                                                                                 baseCurve.Items1[0], baseCurveItem.AppProps);
                                var fxCurveFpMLTriplet = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(fxCurve.Items[0],
                                                                                                 fxCurve.Items1[0], fxCurveItem.AppProps);
                                var refCurveFpMLTriplet = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(refCurve.Items[0],
                                                                                                 refCurve.Items1[0], refCurveItem.AppProps);
                                liveCurveProps.Set(CurveProp.ReferenceCurveUniqueId, baseCurveUniqueId);
                                liveCurveProps.Set(CurveProp.ReferenceFxCurveUniqueId, fxCurveUniqueId);
                                liveCurveProps.Set(CurveProp.ReferenceCurrency2CurveId, refCurveUniqueId);
                                var spreadCurveFpMLTriplet = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(ps, psv, liveCurveProps);
                                //create and set the pricingstructure
                                ips = CurveLoader.LoadInterestRateCurve(Context.Logger, Context.Cache, nameSpace, baseCurveFpMLTriplet,
                                        fxCurveFpMLTriplet, refCurveFpMLTriplet, spreadCurveFpMLTriplet);
                                //Creator.Create(baseCurveFpMLTriplet, fxCurveFpMLTriplet, refCurveFpMLTriplet, spreadCurveFpMLTriplet);
                            }
                            break;//TODO Add Volatility types as well
                        default:
                            {
                                ips = CurveLoader.LoadCurve(Context.Logger, Context.Cache,
                                        nameSpace, new Pair<PricingStructure, PricingStructureValuation>(ps, psv),
                                        liveCurveProps); 
                                //Creator.Create(new Pair<PricingStructure, PricingStructureValuation>(ps, psv), liveCurveProps);
                            }
                            break;
                    }
                    // retrieve curve
                    liveCurve = PricingStructureHelper.CreateMarketFromFpML(
                        ips.GetPricingStructureId().UniqueIdentifier,
                        ips.GetFpMLData());
                    // curve done
                    response.IncrementItemsPassed();
                }
                catch (Exception innerExcp)
                {
                    response.IncrementItemsFailed();
                    Context.Logger.Log(innerExcp);
                    liveCurveProps.Set(WFPropName.ExcpName, WFHelper.GetExcpName(innerExcp));
                    liveCurveProps.Set(WFPropName.ExcpText, WFHelper.GetExcpText(innerExcp));
                }

                // ================================================================================
                // calculate curve lifetimes
                //   SOD = 8am, EOD = 4:30pm
                // live curves
                // - publish anytime
                // - expires SOD next day
                // EOD (today) curves
                // - publish for 15 minutes prior to EOD today
                // - expires in 7 days
                // EOD (dated) - 7 days
                // - publish for 15 minutes prior to EOD today
                // - expires in 7 days
                DateTime dtNow = DateTime.Now;
                DateTime dtToday = dtNow.Date;
                DateTime dtEODPublishBegin = dtToday.AddHours(16.25); // 4:15pm today
                DateTime dtEODPublishUntil = dtToday.AddHours(16.5); // 4:30pm today
                DateTime dtSODTomorrow = dtToday.AddHours(24 + 8); // 8am tomorrow
                //DateTime dtEODTomorrow = dtToday.AddHours(24 + 16); // 4pm tomorrow
                // publish live curve
                Context.Cache.SaveObject(liveCurve, nameSpace + "." + liveCurveItemName, liveCurveProps, true, dtSODTomorrow);
                // republish as latest EOD curve
                if (request.ForceGenerateEODCurves || ((dtNow >= dtEODPublishBegin) && (dtNow <= dtEODPublishUntil)))
                {
                    NamedValueSet itemProps = PricingStructureIdentifier.CreateMarketCurveIdentifier(liveCurveProps, CurveConst.QR_EOD, null, null, null, null).Properties;
                    var itemName = itemProps.GetValue<string>(CurveProp.UniqueIdentifier, true);
                    // persistent
                    Context.Cache.SaveObject(liveCurve, nameSpace + "." + itemName, itemProps, false, TimeSpan.FromDays(7));
                }
                // republish as dated EOD curve
                if (request.ForceGenerateEODCurves || ((dtNow >= dtEODPublishBegin) && (dtNow <= dtEODPublishUntil)))
                {
                    NamedValueSet itemProps = PricingStructureIdentifier.CreateMarketCurveIdentifier(liveCurveProps, CurveConst.QR_EOD, dtToday, null, null, null).Properties;
                    var itemName = itemProps.GetValue<string>(CurveProp.UniqueIdentifier, true);
                    // persistent
                    Context.Cache.SaveObject(liveCurve, nameSpace + "." + itemName, itemProps, false, TimeSpan.FromDays(7));
                }
            } // foreach curve
            // success
            response.Status = RequestStatusEnum.Completed;
        }

        public Type HandledRequestType => typeof(StressedCurveGenRequest);

        #endregion

        protected override HandlerResponse OnExecute(RequestBase baseRequest)
        {
            if (baseRequest == null)
                throw new ArgumentNullException(nameof(baseRequest));

            var request = baseRequest as OrdinaryCurveGenRequest;
            if (request == null)
                throw new ArgumentNullException(nameof(baseRequest));
            // publish 'initial' status
            var response = new HandlerResponse
                               {
                RequestId = request.RequestId,
                NameSpace = request.NameSpace,
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
            if (response.Status == RequestStatusEnum.Undefined)
                throw new ArgumentNullException(nameof(baseRequest));
            Context.Cache.SaveObject(response);
            return response;
        }
    }
}
