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

using System;
using System.Diagnostics;
using System.ServiceProcess;
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.MDS.Server.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

namespace Highlander.MDS.Service.V5r3
{
    public partial class MarketDataService : ServiceBase
    {
        public MarketDataService()
        {
            InitializeComponent();
        }

        private IServerBase2 _server;
        private Reference<ICoreClient> _clientRef;

        protected override void OnStart(string[] args)
        {
            // start the service
            ILogger svcLogger = new ServiceLogger(base.EventLog);
            try
            {
                // - set current directory to the install location
                // - get executable name for file logger
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                ProcessModule pm = Process.GetCurrentProcess().MainModule;
                string moduleName = pm?.ModuleName.Split('.')[0];
                Reference<ILogger> loggerRef = Reference<ILogger>.Create(new MultiLogger(
                    svcLogger,
                    new FileLogger(@"C:\_highlander\logs\" + moduleName + ".{dddd}.log")));
                _clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(loggerRef).SetEnv(BuildConst.BuildEnv).Create());
                _server = new MarketDataServer {LoggerRef = loggerRef, Client = _clientRef};
                _server.Start();
            }
            catch (Exception ex)
            {
                svcLogger.Log(ex);
            }
        }

        protected override void OnStop()
        {
            DisposeHelper.SafeDispose(ref _server);
            DisposeHelper.SafeDispose(ref _clientRef);
        }
    }
}
