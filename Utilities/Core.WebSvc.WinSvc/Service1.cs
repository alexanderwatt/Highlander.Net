using System;
using System.Diagnostics;
using System.ServiceProcess;
using Core.Common;
using Core.V34;
using Core.WebSvc.Server;
using Orion.Build;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

namespace Core.WebSvc.WinSvc
{
    public partial class WebProxySvc : ServiceBase
    {
        public WebProxySvc()
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
                string moduleName = pm.ModuleName.Split('.')[0];
                Reference<ILogger> loggerRef = Reference<ILogger>.Create(new MultiLogger(
                    svcLogger,
                    new FileLogger(@"C:\_qds\logs\" + moduleName + ".{dddd}.log")));

                _clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(loggerRef).SetEnv(BuildConst.BuildEnv).Create());
                _server = new WebProxyServer {LoggerRef = loggerRef, Client = _clientRef};
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
            DisposeHelper.SafeDispose(ref _clientRef);
        }
    }
}
