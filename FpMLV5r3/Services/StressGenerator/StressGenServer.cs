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
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Servers;
using Highlander.Utilities.Threading;
using Highlander.Workflow;
using Highlander.Workflow.CurveGeneration.V5r3;
using Exception = System.Exception;

#endregion

namespace Highlander.StressGenerator.Server.V5r3
{
    internal class CurvesAndStresses
    {
        public readonly string NameSpace;  // e,d Highlander.V5r3
        public readonly string MarketName; // eg. QR_EOD
        public readonly string CurveType; // eg. RateCurve
        public readonly string CurveName; // eg. AUD-LIBOR-BBA-6M
        public readonly Dictionary<string, ICoreItem> BaseCurves = new Dictionary<string, ICoreItem>();
        public readonly Dictionary<string, ICoreItem> StressDefs = new Dictionary<string, ICoreItem>();
        public CurvesAndStresses(string nameSpace, string marketName, string curveType, string curveName)
        {
            NameSpace = nameSpace;
            MarketName = marketName;
            CurveType = curveType;
            CurveName = curveName;
        }
    }

    public class StressGenServer : ServerBase2
    {
        private readonly Guarded<Dictionary<string, CurvesAndStresses>> _store
            = new Guarded<Dictionary<string, CurvesAndStresses>>(new Dictionary<string, CurvesAndStresses>());

        private ISubscription _baseCurveSubs;

        private IWorkContext _workContext;
        private WFGenerateStressedCurve _workflow;

        protected override void OnServerStarted()
        {
            // create workflow
            _workContext = new WorkContext(Logger, IntClient.Target, HostInstance, ServerInstance);
            _workflow = new WFGenerateStressedCurve();
        }

        protected override void OnFirstCallback()
        {
            // initialise workflow
            _workflow.Initialise(_workContext);
            // subscribe to all base curves
            _baseCurveSubs = IntClient.Target.Subscribe<Market>(
                Expr.BoolAND(
                    //Expr.BoolOR(
                        Expr.IsEQU(CurveProp.Market, CurveConst.QR_LIVE),
                        //Expr.IsEQU(CurveProp.Market, CurveConst.QR_EOD),
                        //Expr.IsEQU(CurveProp.Market, CurveConst.NAB_EOD)),
                    Expr.IsEQU(EnvironmentProp.Function, FunctionProp.Market.ToString()),
                    Expr.IsNull(CurveProp.MarketDate),
                    Expr.IsNull(CurveProp.StressName)
                    //Expr.StartsWith(Expr.SysPropItemName, "Highlander.Market.")
                    ),
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
            _baseCurveSubs.Cancel();
            _workflow.Dispose();
        }

        private void BaseCurveCallback(ICoreItem baseCurveItem)
        {
            try
            {
                // check for server shutdown
                if (GetState() != BasicServerState.Running)
                    return;

                // base curve changed
                DateTime dtNow = DateTime.Now;
                //Market baseCurve = (Market)baseCurveItem.Data;
                var curveType = baseCurveItem.AppProps.GetValue<string>(CurveProp.PricingStructureType, true);
                var curveName = baseCurveItem.AppProps.GetValue<string>(CurveProp.CurveName, true);
                var marketName = baseCurveItem.AppProps.GetValue<string>(CurveProp.Market, true);
                //var marketDate = baseCurveItem.AppProps.GetValue<DateTime?>(CurveProp.MarketDate, null);
                var nameSpace = baseCurveItem.AppProps.GetValue<string>(EnvironmentProp.NameSpace, true);
                DateTime curveBaseDate = baseCurveItem.AppProps.GetValue(CurveProp.BaseDate, dtNow.Date);
                var curveGenRequest = new StressedCurveGenRequest
                                          {
                    RequestId = Guid.NewGuid().ToString(),
                    RequesterId = new UserIdentity
                                      {
                        Name = IntClient.Target.ClientInfo.Name,
                        DisplayName = "Stress Curve Generation Server"
                    },
                    BaseDate = curveBaseDate,
                    CurveSelector = new[] { new CurveSelection
                                                {
                          NameSpace = nameSpace,
                          MarketName = marketName,
                          MarketDate = null,//marketDate,
                          CurveType = curveType,
                          CurveName = curveName
                     }}
                };
                IWorkContext context = new WorkContext(Logger, IntClient.Target, "DEV");
                using (var workflow = new WFGenerateStressedCurve())
                {
                    workflow.Initialise(context);
                    WorkflowOutput<HandlerResponse> output = workflow.Execute(curveGenRequest);
                    WorkflowHelper.ThrowErrors(output.Errors);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }
    }
}
