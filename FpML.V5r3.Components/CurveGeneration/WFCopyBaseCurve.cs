using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Orion.Configuration;
using Orion.Constants;
using Orion.Contracts;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures;
using nab.QDS.FpML.V47;
using Orion.Identifiers;
//using National.QRSC.MarketData;
using Orion.ModelFramework;
using Orion.ModelFramework.Identifiers;
using nab.QDS.Core.Common;
using nab.QDS.Util.Helpers;
using nab.QDS.Util.Logging;
using nab.QDS.Util.NamedValues;
using nab.QDS.Util.Serialisation;
using nab.QDS.Util.Expressions;

namespace Orion.Workflow.CurveGeneration
{
    [Export(typeof(IRequestHandler<RequestBase, HandlerResponse>))]
    public class WFCopyBaseCurve : WFGenerateCurveBase, IRequestHandler<RequestBase, HandlerResponse>
    {
        #region IRequestHandler<RequestBase,HandlerResponse> Members

        public void InitialiseRequest(ILogger logger, ICoreCache cache)
        {
            this.Initialise(new WorkContext(logger, cache, null));
        }

        public void ProcessRequest(RequestBase baseRequest, HandlerResponse response)
        {
            if (baseRequest == null)
                throw new ArgumentNullException("request");
            OrdinaryCurveGenRequest request = baseRequest as OrdinaryCurveGenRequest;
            if (request == null)
                throw new InvalidCastException(String.Format("{0} is not a {1}", typeof(RequestBase).Name, typeof(OrdinaryCurveGenRequest).Name));

            CurveSelection[] curveSelectors = request.CurveSelector ?? new List<CurveSelection>().ToArray();
            response.ItemCount = curveSelectors.Length;

            DateTime lastStatusPublishedAt = DateTime.Now;

            // check for workflow cancellation
            if (this.Cancelled)
                throw new OperationCanceledException(this.CancelReason);

            // iterate selected curves
            foreach (CurveSelection curveSelector in curveSelectors)
            {
                    // publish 'intermediate' in-progress result (throttled)
                    if ((DateTime.Now - lastStatusPublishedAt) > TimeSpan.FromSeconds(5))
                    {
                        lastStatusPublishedAt = DateTime.Now;
                        response.Status = RequestStatusEnum.InProgress;
                        _Context.Cache.SaveObject<HandlerResponse>(response);
                    }

                    string inputMarketName = curveSelector.MarketName;
                    string inputCurveName = curveSelector.CurveName;
                    string inputCurveType = curveSelector.CurveType;

                    // given a curve definition, this workflow generates:
                    // - a live base curve using current market data

                    // load curve definition
                    _Context.Logger.LogDebug("Building ordinary curve: {0}.{1}.{2}", inputMarketName, inputCurveType, inputCurveName);
                    string curveUniqueId = string.Format("Orion.Configuration.PricingStructures.{0}.{1}.{2}", inputMarketName, inputCurveType, inputCurveName);
                    ICoreItem marketItem = LoadAndCheckMarketItem(_Context.Cache, curveUniqueId);
                    var marketDate = curveSelector.MarketDate;
                    Market market = marketItem.GetData<Market>(true);
                    //AssertSomeQuotesMissing(((YieldCurveValuation)(cachedMarket.Items1[0])).inputs);
                    //Market clonedMarket = BinarySerializerHelper.Clone<Market>(cachedMarket);
                    PricingStructure ps = market.Items[0];
                    PricingStructureValuation psv = market.Items1[0];
                    // supply base data and  build datetime
                    psv.baseDate = new IdentifiedDate { Value = request.BaseDate };
                    QuotedAssetSet curveDefinition;
                    if (psv is YieldCurveValuation)
                    {
                        curveDefinition = ((YieldCurveValuation)psv).inputs;
                    }
                    else if (psv is FxCurveValuation)
                    {
                        curveDefinition = ((FxCurveValuation)psv).spotRate;
                    }
                    else
                        throw new NotSupportedException("Unsupported PricingStructureValuation type: " + psv.GetType().Name);

                    // default outputs
                    var curveDefProps = new NamedValueSet(marketItem.AppProps);
                    var curveType = PropertyHelper.ExtractPricingStructureType(curveDefProps);//.GetValue<string>(CurveProp.PricingStructureType, true));
                    var curveName = curveDefProps.GetValue<string>(CurveProp.CurveName, true);
                    string marketDataItemName = String.Format("Highlander.MarketData.{0}.{1}.{2}", inputMarketName, curveType, curveName);

                    curveDefProps.Set("BootStrap", true);
                    curveDefProps.Set(CurveProp.BaseDate, request.BaseDate);
                    IPricingStructureIdentifier liveCurveId = PricingStructureIdentifier.CreateMarketCurveIdentifier(curveDefProps, inputMarketName, null, null, null, null);
                    NamedValueSet liveCurveProps = liveCurveId.Properties;
                    var liveCurveItemName = liveCurveProps.GetValue<string>(CurveProp.UniqueIdentifier, true);
                    var liveCurve = new Market(); // empty

                    // given a curve definition, this workflow generates:
                    // - a live base curve using current market data
                    var curveGenProps = new NamedValueSet();
                    curveGenProps.Set(CurveProp.BaseDate, request.BaseDate);
                    IPricingStructureIdentifier curveUniqueId =
                        PricingStructureIdentifier.CreateMarketCurveIdentifier(curveGenProps, inputMarketName, marketDate, inputCurveType, inputCurveName, null);
                    var baseCurveUniqueId = curveUniqueId.Properties.GetValue<string>(CurveProp.UniqueIdentifier, true);
                    ICoreItem marketItem = LoadAndCheckMarketItem(_Context.Cache, baseCurveUniqueId);
                    // load curve definition
                    _Context.Logger.LogDebug("Copy curve: {0} to {1}", baseCurveUniqueId, inputMarketName);

                    // default outputs
                    curveDefProps = new NamedValueSet(marketItem.AppProps);
                    curveDefProps.Set(CurveProp.MarketAndDate, inputMarketName);
                    curveDefProps.Set(CurveProp.MarketDate, null);   
                    var curveType = PropertyHelper.ExtractPricingStructureType(curveDefProps);
                    IPricingStructureIdentifier newCurveUniqueId =
                        PricingStructureIdentifier.CreateMarketCurveIdentifier(curveGenProps, inputMarketName, null, inputCurveType, inputCurveName, null);
                    var curveId = newCurveUniqueId.Properties.GetValue<string>(CurveProp.UniqueIdentifier, true);
                    //var curveName = curveDefProps.GetValue<string>(CurveProp.CurveName, true);
                    //string marketItemName = String.Format("Highlander.Market.{0}.{1}.{2}", inputMarketName, curveType, curveName);
                    curveDefProps.Set(CurveProp.UniqueIdentifier, curveId);
                    _Context.Cache.SaveObject<Market>((Market)marketItem.Data, curveId, curveDefProps, true, TimeSpan.FromDays(50));

                    // curve done
                    requestStatus.IncrementItemsPassed();
                    // publish 'completed' in-progress result
                    requestStatus.Publish(_Context.Logger, _Context.Cache);

            }

            // success
            requestStatus.Status = RequestStatusEnum.Completed;
            // publish 'completed' in-progress result
            requestStatus.Publish(_Context.Logger, _Context.Cache);
        
            return requestStatus;
        }

        public Type HandledRequestType
        {
            get { return typeof(StressedCurveGenRequest); }
        }
    }
}
