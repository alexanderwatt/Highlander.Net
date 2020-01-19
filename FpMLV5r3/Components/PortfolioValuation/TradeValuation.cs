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
using Highlander.Metadata.Common;
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.ValuationEngine.V5r3;
using Exception = System.Exception;

#endregion

namespace Highlander.Workflow.PortfolioValuation.V5r3
{
    [Export(typeof(IRequestHandler<RequestBase, HandlerResponse>))]
    public class WFCalculateTradeValuation : WFCalculateValuationBase, IRequestHandler<RequestBase, HandlerResponse>
    {
        #region IRequestHandler<RequestBase,HandlerResponse> Members

        public void InitialiseRequest(ILogger logger, ICoreCache cache)
        {
            Initialise(new WorkContext(logger, cache, null));
        }

        public void ProcessRequest(RequestBase request, HandlerResponse response)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            var tradeValRequest = request as TradeValuationRequest;
            if (tradeValRequest == null)
                throw new InvalidCastException(
                    $"{typeof(RequestBase).Name} is not a {typeof(TradeValuationRequest).Name}");

            DateTime lastStatusPublishedAt = DateTime.Now;
            var nameSpace = tradeValRequest.NameSpace;
            // common properties
            string reportingCurrency = tradeValRequest.ReportingCurrency;
            string market = tradeValRequest.MarketName;

            // build a single trade portfolio
            var tradeItemInfos = new Dictionary<string, ICoreItemInfo>();
            ICoreItemInfo tradeItemInfo = Context.Cache.LoadItemInfo<Trade>(tradeValRequest.TradeItemName);
            if (tradeItemInfo != null)
                tradeItemInfos[tradeValRequest.TradeItemName] = tradeItemInfo;

            // define scenario loops
            // - always include un-stressed scenario (null)
            var irScenarios = new List<string> { null };
            if (tradeValRequest.IRScenarioNames != null)
                irScenarios.AddRange(tradeValRequest.IRScenarioNames);
            string[] irScenarioNames = irScenarios.Distinct().ToArray();

            var fxScenarios = new List<string> { null };
            if (tradeValRequest.FXScenarioNames != null)
                fxScenarios.AddRange(tradeValRequest.FXScenarioNames);
            string[] fxScenarioNames = fxScenarios.Distinct().ToArray();

            // update progress status
            response.ItemCount = irScenarios.Count * fxScenarios.Count * tradeItemInfos.Count;
            response.Status = RequestStatusEnum.InProgress;
            Context.Cache.SaveObject(response);

            // preload *all* curves into the cache
            // note: this is required to optimise all subsequent curve queries
            Context.Cache.LoadItems<Market>(Expr.ALL);

            // load and sort scenario definition rules
            var clauses = new List<IExpression> { Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace)};
            var scenarioRules = Context.Cache.LoadObjects<ScenarioRule>(Expr.BoolAND(clauses.ToArray()));
            var sortedScenarioRules = new List<CachedScenarioRule>();
            {
                sortedScenarioRules.AddRange(from scenarioRule in scenarioRules
                                             where !scenarioRule.Disabled
                                             select new CachedScenarioRule(scenarioRule.ScenarioId, scenarioRule.RuleId, scenarioRule.Priority, (scenarioRule.FilterExpr != null) ? Expr.Create(scenarioRule.FilterExpr) : Expr.ALL, scenarioRule.StressId));
            }
            sortedScenarioRules.Sort();

            // build distinct lists of curve names and currencies required by the Trade
            var curvenamesList = new List<string>();
            var currenciesList = new List<string>();
            foreach (var item in tradeItemInfos.Values)
            {
                curvenamesList.AddRange(item.AppProps.GetArray<string>(TradeProp.RequiredPricingStructures));
                currenciesList.AddRange(item.AppProps.GetArray<string>(TradeProp.RequiredCurrencies));
            }
            curvenamesList = new List<string>(curvenamesList.Distinct().Where(x => !String.IsNullOrEmpty(x)));
            currenciesList = new List<string>(currenciesList.Distinct().Where(x => !String.IsNullOrEmpty(x)));

            IEnumerable<string> metrics = GetSwapMetrics();

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
                        curveProperties = PricingStructureFactory.GetInterestRateCurveProperties(Context.Logger, Context.Cache, request.NameSpace, market, curveName, null);
                        resolvedCurveProps[curveSignature] = curveProperties;
                    }
                    string stressName = CachedScenarioRule.RunScenarioRules(sortedScenarioRules, irScenario, curveProperties);
                    irScenarioCurveMap[i].Add(new CurveStressPair(curveName, stressName));
                }
            }
            // FX loop
            var fxScenarioCurveMap = new List<CurveStressPair>[fxScenarioNames.Length];
            for (int j = 0; j < fxScenarioNames.Length; j++)
            {
                string fxScenario = fxScenarioNames[j];
                fxScenarioCurveMap[j] = new List<CurveStressPair>();
                foreach (string currency in currenciesList)
                {
                    string curveSignature = CurveLoader.FxCurveSignature(market, currency, reportingCurrency, null);
                    if (!resolvedCurveProps.TryGetValue(curveSignature, out var curveProperties))
                    {
                        // not cached - resolve and cache
                        curveProperties = PricingStructureFactory.GetFxCurveProperties(Context.Logger, Context.Cache, request.NameSpace, market, currency, reportingCurrency);
                        resolvedCurveProps[curveSignature] = curveProperties;
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
                    var pricer = new PortfolioPricer(irScenarioCurveMap[i], fxScenarioCurveMap[j]);

                    // now price the Trade
                    if (metrics != null)
                    {
                        var enumerable = metrics.ToArray();
                        pricer.PriceAndPublish(
                            Context.Logger, Context.Cache,
                            resolvedCurveCache,
                            reportNameCache, response,
                            tradeItemInfos.Keys, tradeValRequest,
                            irScenario, fxScenario, reportingCurrency,
                            tradeValRequest.BaseParty, enumerable, false);
                    }

                    // export to valuation database
                    //foreach (var valuationItem in valuationItems)
                    //{
                    //    ExportValuation(valuationItem);
                    //}

                    DateTime dtNow = DateTime.Now;
                    if ((dtNow - lastStatusPublishedAt) > TimeSpan.FromSeconds(5))
                    {
                        lastStatusPublishedAt = dtNow;
                        response.Status = RequestStatusEnum.InProgress;
                        Context.Cache.SaveObject(response);
                    }
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
            var request = baseRequest as TradeValuationRequest;
            if (request == null)
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
