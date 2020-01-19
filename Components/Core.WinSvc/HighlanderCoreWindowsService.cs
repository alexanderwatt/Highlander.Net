/*
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

using System;
using System.Diagnostics;
using System.ServiceProcess;
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Servers;

namespace Highlander.Core.Service
{
    public partial class HighlanderCoreWindowsService : ServiceBase
    {
        public HighlanderCoreWindowsService()
        {
            InitializeComponent();
        }

        private Reference<ILogger> _loggerRef;
        private IBasicServer _server;

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
                _loggerRef = Reference<ILogger>.Create(new MultiLogger(
                    svcLogger,
                    new FileLogger(@"C:\_highlander\logs\" + moduleName + ".{dddd}.log")));
                const EnvId env = EnvId.Dev_Development; // hack EnvId.Dev_Development
                var settings = new NamedValueSet(EnvHelper.GetAppSettings(env, EnvHelper.SvcPrefix(SvcId.CoreServer), true));
                DisposeHelper.SafeDispose(ref _server);
                _server = new CoreServer(_loggerRef, settings);
                _server.Start();
            }
            catch (Exception ex)
            {
                svcLogger.Log(ex);
            }
        }

        protected override void OnStop()
        {
            // stop the service
            DisposeHelper.SafeDispose(ref _server);
            DisposeHelper.SafeDispose(ref _loggerRef);
        }
    }
}
