using System;
using System.Diagnostics;
using System.ServiceProcess;
using Core.Common;
using Core.V34;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Server.CurveGenerator;

namespace CurveGenWinSvc
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
                    new FileLogger(@"C:\_qrsc\ServiceLogs\" + moduleName + ".{dddd}.log"));
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
