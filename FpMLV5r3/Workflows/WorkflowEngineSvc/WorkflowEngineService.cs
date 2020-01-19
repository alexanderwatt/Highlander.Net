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
using System.ServiceProcess;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Highlander.Workflow.Server.V5r3;

#endregion

namespace Highlander.Workflow.Service.V5r3
{
    public partial class WorkflowEngineService : ServiceBase
    {
        public WorkflowEngineService()
        {
            InitializeComponent();
        }

        private Reference<ILogger> _logger;
        private IServerBase2 _server;
        private Reference<ICoreClient> _clientRef;

        protected override void OnStart(string[] args)
        {
            // start the service
            // - change the current directory to the service installation directory
            //   (required in order to access resource files)
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //_Logger = new ServiceLogger(base.EventLog);
            var logger = new FileLogger(@"C:\_highlander\ServiceLogs\WorkflowEngineSvc.{dddd}.log");
            _logger = Reference<ILogger>.Create(logger);
            _clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(_logger).Create());
            _server = new WorkflowServer {LoggerRef = _logger, Client = _clientRef};
            _server.Start();
        }

        protected override void OnStop()
        {
            DisposeHelper.SafeDispose(ref _server);
            DisposeHelper.SafeDispose(ref _clientRef);
            DisposeHelper.SafeDispose(ref _logger);
        }
    }
}
