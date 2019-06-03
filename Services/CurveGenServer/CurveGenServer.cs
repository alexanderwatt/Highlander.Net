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
using Core.Common;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.Contracts;
using Orion.Util.Expressions;
using Orion.Util.Servers;
using Orion.Workflow;
using Orion.Workflow.CurveGeneration;
using Exception = System.Exception;

#endregion

namespace Server.CurveGenerator
{
    public class CurveGenServer : ServerBase2
    {
        private ISubscription _curveSubs;
        private IWorkContext _workContext;
        private WFGenerateOrdinaryCurve _workflow;

        protected override void OnServerStarted()
        {
            // create workflow
            //_cache = this.Client.CreateCache();
            _workContext = new WorkContext(Logger, IntClient.Target, HostInstance, ServerInstance);
            _workflow = new WFGenerateOrdinaryCurve();
        }

        protected override void OnFirstCallback()
        {
            base.OnFirstCallback();
            // initialise workflow
            _workflow.Initialise(_workContext);
            // subscribe to curve definitions
            _curveSubs = IntClient.Target.Subscribe<QuotedAssetSet>(
                Expr.BoolAND(
                    Expr.IsEQU(CurveProp.Function,FunctionProp.QuotedAssetSet.ToString()),
                    Expr.IsEQU(CurveProp.Market, CurveConst.QR_LIVE),
                    Expr.IsNull(CurveProp.MarketDate)),
                    (subscription, item) =>
                    {
                        MainThreadQueue?.Dispatch(item, BaseCurveCallback);
                    }, null);
        }

        protected override void OnCloseCallback()
        {
            _workflow.Cancel("Server shutdown");
        }

        protected override void OnServerStopping()
        {
            _curveSubs.Cancel();
            _workflow.Dispose();
        }

        private void BaseCurveCallback(ICoreItem baseCurveItem)
        {
            try
            {
                // check for server shutdown
                if (GetState() != BasicServerState.Running)
                    return;
                // refresh all curves
                // - update market data
                // - regenerate curves
                DateTime dtNow = DateTime.Now;
                // Curve identifiers
                var nameSpace = baseCurveItem.AppProps.GetValue<string>(EnvironmentProp.NameSpace, true);
                var marketName = baseCurveItem.AppProps.GetValue<string>(CurveProp.Market, true);
                var curveType = baseCurveItem.AppProps.GetValue<string>(CurveProp.PricingStructureType, true);
                var curveName = baseCurveItem.AppProps.GetValue<string>(CurveProp.CurveName, true);
                //    });
                //}
                var curveGenRequest = new OrdinaryCurveGenRequest
                    {
                    RequestId = Guid.NewGuid().ToString(),
                    RequesterId = new UserIdentity
                    {
                        Name = IntClient.Target.ClientInfo.Name,
                        DisplayName = "Curve Generation Server"
                    },
                    BaseDate = dtNow.Date,
                    NameSpace = nameSpace,
                    SaveMarketData = false,
                    UseSavedMarketData = true,
                    ForceGenerateEODCurves = false,
                    CurveSelector = new[] { new CurveSelection
                                                {
                        NameSpace = nameSpace,
                        MarketName = marketName,
                        CurveType = curveType,
                        CurveName = curveName
                    }}
                };
                IWorkContext context = new WorkContext(Logger, IntClient.Target, "DEV");
                using (var workflow = new WFGenerateOrdinaryCurve())
                {
                    workflow.Initialise(context);
                    WorkflowOutput<HandlerResponse> output = workflow.Execute(curveGenRequest);
                    WorkflowHelper.ThrowErrors(output.Errors);
                }
            }
            catch (Exception excp)
            {
                Logger.LogDebug("Base curve generation failed with exception: {0}", excp.GetType().Name);
                Logger.Log(excp);
            }
        }

    }
}
