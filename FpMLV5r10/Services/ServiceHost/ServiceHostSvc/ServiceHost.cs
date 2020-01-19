using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using National.QRSC.Server.ServiceHost;
using National.QRSC.Utility.Logging;
using National.QRSC.Utility.Servers;
using National.QRSC.Utility.Helpers;

namespace ServiceHostSvc
{
    public partial class QRSCHostSvc : ServiceBase
    {
        private IBasicServer _Server;
        private ILogger _Logger;

        public QRSCHostSvc()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // - change the current directory to the service installation directory
            //   (required in order to access resource files)
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // start the service
            _Logger = new FileLogger(@"C:\_qrsc\ServiceLogs\ServiceHostSvc.{dddd}.log");
            _Server = new ServiceHost(_Logger);
            _Server.Start();
        }

        protected override void OnStop()
        {
            // stop the service
            _Server.Stop();
            DisposeHelper.SafeDispose(ref _Logger);
        }
    }
}
