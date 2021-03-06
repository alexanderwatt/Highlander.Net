﻿/*
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
using System.Diagnostics;
using System.ServiceProcess;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.CurveGenerator.Server.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

#endregion

namespace Highlander.CurveGenerator.Service.V5r3
{
    public partial class CurveGenService : ServiceBase
    {
        public CurveGenService()
        {
            InitializeComponent();
            //if(!EventLog.SourceExists("MySource"))
            //{
            //    EventLog.CreateEventSource("MySource", "MyNewLog");
            //}
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
                string moduleName = pm.ModuleName.Split('.')[0];
                ILogger logger = new MultiLogger(svcLogger,
                    new FileLogger(@"C:\_highlander\ServiceLogs\" + moduleName + ".{dddd}.log"));
                var tempLogger = Reference<ILogger>.Create(logger);
                _clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(tempLogger).Create());
                _server = new CurveGenServer { LoggerRef = tempLogger, Client = _clientRef, HostInstance = null };
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
