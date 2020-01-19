using System;
using System.ServiceProcess;
using Core.Common;
using Core.V34;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Workflow.Server;

namespace WorkflowEngineSvc
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
            var logger = new FileLogger(@"C:\_qrsc\ServiceLogs\WorkflowEngineSvc.{dddd}.log");
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
