using System;
using System.Diagnostics;
using System.ServiceProcess;
using National.QRSC.Grid.Manager;
using nab.QDS.Core.Common;
using nab.QDS.Core.V34;
using nab.QDS.Util.Helpers;
using nab.QDS.Util.Logging;
using nab.QDS.Util.RefCounting;

namespace QRSCGridManagerService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        private IServerBase2 _Server;
        private Reference<ICoreClient> _ClientRef;

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

                _ClientRef = Reference<ICoreClient>.Create(new CoreClientFactory(logger).Create());
                _Server = new GridManagerServer();
                _Server.Logger = logger;
                _Server.Client = _ClientRef;
                _Server.HostInstance = null;
                _Server.Start();
            }
            catch (Exception ex)
            {
                svcLogger.Log(ex);
            }
        }

        protected override void OnStop()
        {
            DisposeHelper.SafeDispose(ref _Server);
            DisposeHelper.SafeDispose(ref _ClientRef);
        }
    }
}
