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
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.ValuationEngine.V5r3;
using Exception = System.Exception;

#endregion

namespace Highlander.Workflow.PortfolioValuation.V5r3
{
    [Export(typeof(IRequestHandler<RequestBase, HandlerResponse>))]
    public class WFCalculatePortfolioHistoricalValuation : WFCalculateValuationBase, IRequestHandler<RequestBase, HandlerResponse>
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
            var request = baseRequest as PortfolioValuationRequest;
            if (request == null)
                throw new InvalidCastException(
                    $"{typeof(RequestBase).Name} is not a {typeof(PortfolioValuationRequest).Name}");
            DateTime lastStatusPublishedAt = DateTime.Now;
            // common properties
            var nameSpace = request.NameSpace;
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
                        var tradeItemInfo = name.Contains(FpML5R3NameSpaces.Confirmation) ? Context.Cache.LoadItemInfo<Confirmation.V5r3.Trade>(name) 
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
                        var tradeItemInfo = name.Contains(FpML5R3NameSpaces.Confirmation) ? Context.Cache.LoadItemInfo<Confirmation.V5r3.Trade>(name) 
                            : Context.Cache.LoadItemInfo<Trade>(name);
                        if (tradeItemInfo != null)
                            tradeItemInfos[name] = tradeItemInfo;
                    }
                }
            }
            // define scenario loops
            // - always include un-stressed scenario (null)
            var marketScenarios = new List<DateTime>();
            if (request.DateScenarios != null)
                marketScenarios.AddRange(request.DateScenarios);
            // update progress status
            response.ItemCount = marketScenarios.Count * tradeItemInfos.Count;
            response.Status = RequestStatusEnum.InProgress;
            Context.Cache.SaveObject(response);
            // pre load *all* curves into the cache
            // note: this is required to optimise all subsequent curve queries
            var markets = new List<IExpression> { Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace) };
            Context.Cache.LoadItems<Market>(Expr.BoolAND(markets.ToArray()));
            // build distinct lists of curve names and currencies required by the portfolio
            var curveNamesList = new List<string>();
            var currenciesList = new List<string>();
            foreach (var item in tradeItemInfos.Values)
            {
                curveNamesList.AddRange(item.AppProps.GetArray<string>(TradeProp.RequiredPricingStructures));
                currenciesList.AddRange(item.AppProps.GetArray<string>(TradeProp.RequiredCurrencies));
            }
            curveNamesList = new List<string>(curveNamesList.Distinct().Where(x => !String.IsNullOrEmpty(x)));
            currenciesList = new List<string>(currenciesList.Distinct().Where(x => !String.IsNullOrEmpty(x)));
            var metrics = GetSwapMetrics();
            // check for workflow cancellation (user abort, server shutdown etc.)
            if (Cancelled)
                throw new OperationCanceledException(CancelReason);
            // initialise the pricer with the IR/FX scenario curve maps
            var portfolioPricer = new PortfolioPricer();
            var keyList = tradeItemInfos.Keys.ToList();
            // now price the portfolio
            portfolioPricer.PriceAndPublish(
                Context.Logger, Context.Cache,
                response, curveNamesList,
                currenciesList,
                keyList, request,
                metrics, false);
            var dtNow = DateTime.Now;
            if ((dtNow - lastStatusPublishedAt) > TimeSpan.FromSeconds(5))
            {
                //lastStatusPublishedAt = dtNow;
                response.Status = RequestStatusEnum.InProgress;
                Context.Cache.SaveObject(response);
            }
            // success
            response.Status = RequestStatusEnum.Completed;
        }

        public Type HandledRequestType => typeof(TradeValuationRequest);

        #endregion

        protected override HandlerResponse OnExecute(RequestBase baseRequest)
        {
            if (baseRequest == null)
                throw new ArgumentNullException(nameof(baseRequest));
            var request = baseRequest as PortfolioValuationRequest;
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
