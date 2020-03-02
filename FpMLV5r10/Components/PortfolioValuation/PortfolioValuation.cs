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

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Common;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Contracts;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Metadata.Common;
using Orion.CurveEngine.Factory;
using Orion.Constants;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.ValuationEngine;
using Orion.Workflow;
using Exception = System.Exception;

#endregion

namespace Orion.PortfolioValuation
{
    [Export(typeof(IRequestHandler<RequestBase, HandlerResponse>))]
    public class WFCalculatePortfolioStressValuation : WFCalculateValuationBase, IRequestHandler<RequestBase, HandlerResponse>
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
            if (!(baseRequest is PortfolioValuationRequest request))
                throw new InvalidCastException(
                    $"{typeof(RequestBase).Name} is not a {typeof(PortfolioValuationRequest).Name}");
            DateTime lastStatusPublishedAt = DateTime.Now;
            // common properties
            var nameSpace = request.NameSpace;
            string reportingCurrency = request.ReportingCurrency;
            string baseParty = request.BaseParty;
            string market = request.MarketName;
            // resolve portfolio valuation request
            var identifier = (new PortfolioSpecification(request.PortfolioId, request.NameSpace)).NetworkKey;
            var portfolio =
                Context.Cache.LoadObject<PortfolioSpecification>(identifier);
            if (portfolio == null)
                throw new ArgumentException($"Unknown portfolio id: '{request.PortfolioId}'");
            // build trade query from portfolio definition
            var tradeItemInfos = new Dictionary<string, ICoreItemInfo>();
            if (portfolio.PortfolioSubqueries != null)
            {
                foreach (var subQuery in portfolio.PortfolioSubqueries.OrderBy(x => x.SequenceOrder))
                {
                    var clauses = new List<IExpression> {Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace)};
                    if (subQuery.CounterpartyId != null)
                        clauses.Add(Expr.IsEQU(TradeProp.CounterPartyId, subQuery.CounterpartyId));
                    if (subQuery.TradingBookId != null)
                        clauses.Add(Expr.IsEQU(TradeProp.TradingBookId, subQuery.TradingBookId));
                    // load trades defined by the query
                    if (clauses.Count <= 0) continue;
                    List<ICoreItemInfo> subQueryItems = Context.Cache.LoadItemInfos<Trade>(Expr.BoolAND(clauses.ToArray()));
                    //TODO again have to handle confirmation
                    foreach (var tradeItemInfo in subQueryItems)
                    {
                        if (subQuery.ExcludeItems)
                            tradeItemInfos.Remove(tradeItemInfo.Name);
                        else
                            tradeItemInfos[tradeItemInfo.Name] = tradeItemInfo;
                    }
                }
            }
            // process included/excluded trades ids
            if (portfolio.ExcludeOverridesInclude)
            {
                // add included names
                if (portfolio.IncludedTradeItemNames != null)
                {
                    foreach (var name in portfolio.IncludedTradeItemNames)
                    {
                        var tradeItemInfo = name.Contains(FpML5R10NameSpaces.Confirmation) ? Context.Cache.LoadItemInfo<FpML.V5r10.Confirmation.Trade>(name) 
                            : Context.Cache.LoadItemInfo<Trade>(name);
                        if (tradeItemInfo != null)
                            tradeItemInfos[name] = tradeItemInfo;
                    }
                }
            }
            // remove excluded names
            if (portfolio.ExcludedTradeItemNames != null)
            {
                foreach (var name in portfolio.ExcludedTradeItemNames)
                {
                    tradeItemInfos.Remove(name);
                }
            }
            if (!portfolio.ExcludeOverridesInclude)
            {
                // add included names
                if (portfolio.IncludedTradeItemNames != null)
                {
                    foreach (var name in portfolio.IncludedTradeItemNames)
                    {
                        var tradeItemInfo = name.Contains(FpML5R10NameSpaces.Confirmation) ? Context.Cache.LoadItemInfo<FpML.V5r10.Confirmation.Trade>(name) 
                            : Context.Cache.LoadItemInfo<Trade>(name);
                        if (tradeItemInfo != null)
                            tradeItemInfos[name] = tradeItemInfo;
                    }
                }
            }
            // define scenario loops
            // - always include un-stressed scenario (null)
            var irScenarios = new List<string> { null };
            if (request.IRScenarioNames != null)
                irScenarios.AddRange(request.IRScenarioNames);
            string[] irScenarioNames = irScenarios.Distinct().ToArray();
            var fxScenarios = new List<string> { null };
            if (request.FXScenarioNames != null)
                fxScenarios.AddRange(request.FXScenarioNames);
            string[] fxScenarioNames = fxScenarios.Distinct().ToArray();
            // update progress status
            response.ItemCount = irScenarios.Count * fxScenarios.Count * tradeItemInfos.Count;
            response.Status = RequestStatusEnum.InProgress;
            Context.Cache.SaveObject(response);
            // preload *all* curves into the cache
            // note: this is required to optimise all subsequent curve queries
            if (market.Contains(CurveConst.NAB_EOD) || market.Contains(CurveConst.TEST_EOD))
            {
                Context.Cache.LoadItems<Market>(Expr.ALL);//TODO make specific to the namespace
            }
            // load and sort scenario definition rules
            var scenarioRules = Context.Cache.LoadObjects<ScenarioRule>(Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace));
            var sortedScenarioRules = new List<CachedScenarioRule>();
            {
                sortedScenarioRules.AddRange(from scenarioRule in scenarioRules
                                             where !scenarioRule.Disabled
                                             select new CachedScenarioRule(scenarioRule.ScenarioId, scenarioRule.RuleId, scenarioRule.Priority, (scenarioRule.FilterExpr != null) 
                                                 ? Expr.Create(scenarioRule.FilterExpr) : Expr.ALL, scenarioRule.StressId));
            }
            sortedScenarioRules.Sort();
            // build distinct lists of curve names and currencies required by the portfolio
            var curvenamesList = new List<string>();
            var currenciesList = new List<string>();
            foreach (var item in tradeItemInfos.Values)
            {
                curvenamesList.AddRange(item.AppProps.GetArray<string>(TradeProp.RequiredPricingStructures));
                currenciesList.AddRange(item.AppProps.GetArray<string>(TradeProp.RequiredCurrencies));
            }
            curvenamesList = new List<string>(curvenamesList.Distinct().Where(x => !String.IsNullOrEmpty(x)));
            currenciesList = new List<string>(currenciesList.Distinct().Where(x => !String.IsNullOrEmpty(x)));
            var metrics = GetSwapMetrics();
            // run the scenario rules ONCE for each IR and FX scenario to determine which
            // stressed curves to use when pricing.
            var resolvedCurveProps = new Dictionary<string, NamedValueSet>();
            // IR loop
            var irScenarioCurveMap = new List<CurveStressPair>[irScenarioNames.Length];
            for (int i = 0; i < irScenarioNames.Length; i++)
            {
                string irScenario = irScenarioNames[i];
                irScenarioCurveMap[i] = new List<CurveStressPair>();
                foreach (string curveName in curvenamesList)
                {
                    string curveSignature = CurveLoader.IrCurveSignature(market, curveName, null);
                    if (!resolvedCurveProps.TryGetValue(curveSignature, out var curveProperties))
                    {
                        // not cached - resolve and cache
                        curveProperties = PricingStructureFactory.GetInterestRateCurveProperties(Context.Logger, Context.Cache, request.NameSpace, market, curveName, null);//TODO not this namespace. Use the curves.
                        resolvedCurveProps[curveSignature] = curveProperties;
                    }
                    var stressName = CachedScenarioRule.RunScenarioRules(sortedScenarioRules, irScenario, curveProperties);
                    irScenarioCurveMap[i].Add(new CurveStressPair(curveName, stressName));
                }
            }
            // FX loop
            var fxScenarioCurveMap = new List<CurveStressPair>[fxScenarioNames.Length];
            for (var j = 0; j < fxScenarioNames.Length; j++)
            {
                string fxScenario = fxScenarioNames[j];
                fxScenarioCurveMap[j] = new List<CurveStressPair>();
                foreach (string currency in currenciesList)
                {
                    string curveSignature = CurveLoader.FxCurveSignature(market, currency, reportingCurrency, null);
                    if (!resolvedCurveProps.TryGetValue(curveSignature, out var curveProperties))
                    {
                        // not cached - resolve and cache
                        if (currency != reportingCurrency)
                        {
                            curveProperties = PricingStructureFactory.GetFxCurveProperties(Context.Logger, Context.Cache, request.NameSpace, market, currency, reportingCurrency);
                            resolvedCurveProps[curveSignature] = curveProperties;
                        }
                    }
                    string stressName = CachedScenarioRule.RunScenarioRules(sortedScenarioRules, fxScenario, curveProperties);
                    fxScenarioCurveMap[j].Add(new CurveStressPair(currency, stressName));
                }
            }
            // iterate the scenario loops
            var resolvedCurveCache = new Dictionary<string, ICurve>();
            var reportNameCache = new Dictionary<string, string>();
            for (int i = 0; i < irScenarioNames.Length; i++)
            {
                string irScenario = irScenarioNames[i];
                for (int j = 0; j < fxScenarioNames.Length; j++)
                {
                    string fxScenario = fxScenarioNames[j];
                    // check for workflow cancellation (user abort, server shutdown etc.)
                    if (Cancelled)
                        throw new OperationCanceledException(CancelReason);
                    // initialise the pricer with the IR/FX scenario curve maps
                    var portfolioPricer = new PortfolioPricer(irScenarioCurveMap[i], fxScenarioCurveMap[j]);
                    // now price the portfolio
                    portfolioPricer.PriceAndPublish(
                        Context.Logger, Context.Cache,
                        resolvedCurveCache,
                        reportNameCache, response,
                        tradeItemInfos.Keys, request,
                        irScenario, fxScenario, reportingCurrency,
                        baseParty, metrics, false);
                    var dtNow = DateTime.Now;
                    if ((dtNow - lastStatusPublishedAt) <= TimeSpan.FromSeconds(5)) continue;
                    lastStatusPublishedAt = dtNow;
                    response.Status = RequestStatusEnum.InProgress;
                    Context.Cache.SaveObject(response);
                } // foreach ir scenario
            } // foreach fx scenario
            // success
            response.Status = RequestStatusEnum.Completed;
        }

        public Type HandledRequestType => typeof(TradeValuationRequest);

        #endregion

        protected override HandlerResponse OnExecute(RequestBase baseRequest)
        {
            if (baseRequest == null)
                throw new ArgumentNullException(nameof(baseRequest));
            if (!(baseRequest is PortfolioValuationRequest request))
                throw new ArgumentNullException(nameof(baseRequest));
            // publish 'initial' status
            var response = new HandlerResponse
                               {
                                   NameSpace = request.NameSpace,
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
            if (response.Status == RequestStatusEnum.Undefined)
                throw new ArgumentNullException(nameof(baseRequest));
            Context.Cache.SaveObject(response);
            return response;
        }
    }
}