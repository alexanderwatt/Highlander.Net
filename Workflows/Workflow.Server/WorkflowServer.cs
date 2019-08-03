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
using System.Linq;
using Core.Common;
using Orion.Build;
using Orion.Constants;
using Orion.PortfolioValuation;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using Orion.Workflow;
using Orion.Workflow.CurveGeneration;

#endregion

namespace Workflow.Server
{
    internal static class GridWorksteps
    {
        // ================================================================================
        // define supported workflows/steps here
        public static List<IWorkstep> Create()
        {
            var result = new List<IWorkstep> { new WFGenerateOrdinaryCurve() };//, new WFGenerateStressedCurve(), new WFCalculatePortfolioStressValuation(), new WFCalculatePortfolioHistoricalValuation() 
            return result;
        }
        // ================================================================================
    }

    public class WorkflowClient : IDisposable
    {
        public readonly IWorkstep[] Clients;

        public T GetWorkstep<T>() where T : class
        {
            return Clients.Where(workstep => workstep.GetType() == typeof (T)).Cast<T>().FirstOrDefault();
        }

        public WorkflowClient(IWorkContext context, NamedValueSet settings)
        {
            // default configuration
            const SvcId svc = SvcId.GridSwitch;
            EnvId env = EnvHelper.ParseEnvName(BuildConst.BuildEnv);
            Guid nodeId = Guid.NewGuid();
            // custom configuration
            if (settings != null)
            {
                // environment
                env = (EnvId)settings.GetValue(WFPropName.EnvId, (int)env);
            }
            env = EnvHelper.CheckEnv(env);
            // derived configuration
            string hosts = null;
            int port = EnvHelper.SvcPort(env, svc);
            if (settings != null)
            {
                port = settings.GetValue(WFPropName.Port, port);
                hosts = settings.GetValue(WFPropName.Hosts, hosts);
                nodeId = settings.GetValue(WFPropName.NodeId, nodeId);
            }
            string svcName = EnvHelper.SvcPrefix(svc);
            string[] serviceAddrs = EnvHelper.GetServiceAddrs(env, svc, false);
            if (hosts != null)
                serviceAddrs = hosts.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            ServiceAddress resolvedServer = V111Helpers.ResolveServer(context.Logger, svcName, new ServiceAddresses(WcfConst.AllProtocols, serviceAddrs, port),
                new[] { typeof(IDiscoverV111).FullName });
            // initialise worksteps
            List<IWorkstep> worksteps = GridWorksteps.Create();
            foreach (IWorkstep workstep in worksteps)
            {
                workstep.Initialise(context);
                workstep.EnableGrid(GridLevel.Client, nodeId, resolvedServer.Port, resolvedServer.Host);
            }
            Clients = worksteps.ToArray();
        }

        public void Dispose()
        {
            for (int i = 0; i < Clients.Length; i++)
            {
                DisposeHelper.SafeDispose(ref Clients[i]);
            }
        }
    }

    public class WorkflowServer : ServerBase2, IDiscoverV111
    {
        public IWorkstep[] Routers;
        public IWorkstep[] Workers;
        private int _serverPort;
        public int ServerPort => _serverPort;
        private Guid _nodeId;

        private CustomServiceHost<IDiscoverV111, DiscoverRecverV111> _discoServerHost;

        private void StartUp()
        {
            // default configuration
            EnvId env = IntClient.Target.ClientInfo.ConfigEnv;
            // derived configuration
            _serverPort = OtherSettings.GetValue(WFPropName.Port, EnvHelper.SvcPort(env, SvcId.GridSwitch));
            _nodeId = OtherSettings.GetValue(WFPropName.NodeId, Guid.NewGuid());
            // create router/worker worksteps
            Routers = GridWorksteps.Create().ToArray();
            Workers = GridWorksteps.Create().ToArray();
            // start discovery endpoint
            string endpoint = ServiceHelper.FormatEndpoint(WcfConst.NetTcp, _serverPort);
            string svcName = EnvHelper.SvcPrefix(SvcId.GridSwitch);
            _discoServerHost = new CustomServiceHost<IDiscoverV111, DiscoverRecverV111>(
                Logger, new DiscoverRecverV111(this), endpoint,
                svcName, typeof(IDiscoverV111).Name, true);
            IWorkContext context = new WorkContext(Logger, IntClient.Target, HostInstance, ServerInstance);
            // start gridswitch endpoints - routers before workers
            foreach (IWorkstep router in Routers)
            {
                router.Initialise(context);
                router.EnableGrid(GridLevel.Router, _nodeId, _serverPort, null);
            }
            foreach (IWorkstep worker in Workers)
            {
                worker.Initialise(context);
                worker.EnableGrid(GridLevel.Worker, _nodeId, _serverPort, null);
            }
        }

        private void CleanUp()
        {
            // dispose workers before routers
            for (int i = 0; i < Workers.Length; i++)
            {
                DisposeHelper.SafeDispose(ref Workers[i]);
            }
            for (int i = 0; i < Routers.Length; i++)
            {
                DisposeHelper.SafeDispose(ref Routers[i]);
            }
            DisposeHelper.SafeDispose(ref _discoServerHost);
        }

        protected override void OnServerStarted()
        {
            StartUp();
        }

        protected override void OnServerStopping()
        {
            CleanUp();
        }

        #region IDiscoverV111 Members

        public V111DiscoverReply DiscoverServiceV111()
        {
            return new V111DiscoverReply
                       {
                SupportedContracts = new[]
                {
                    typeof(IDiscoverV111).FullName
                    //,typeof(ISessCtrlV111).FullName
                }
            };
        }

        #endregion

    }
}
