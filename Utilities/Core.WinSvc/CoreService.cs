using System;
using System.Diagnostics;
using System.ServiceProcess;
using Core.Common;
using Core.Server;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;
using Orion.Util.Servers;

namespace Core.WinSvc
{
    public partial class CoreWinSvc : ServiceBase
    {
        public CoreWinSvc()
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
                string moduleName = pm.ModuleName.Split('.')[0];
                _loggerRef = Reference<ILogger>.Create(new MultiLogger(
                    svcLogger,
                    new FileLogger(@"C:\_qds\logs\" + moduleName + ".{dddd}.log")));
                const EnvId env = EnvId.Dev_Development; // hack
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
