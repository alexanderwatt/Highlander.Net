using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using National.QRSC.Utility.Helpers;
using National.QRSC.Utility.Logging;
using National.QRSC.Utility.Servers;
using National.QRSC.Server.ServiceHost;
using National.QRSC.Configuration;
using National.QRSC.Runtime.Common;
using National.QRSC.Runtime.V33;

namespace ServiceHostGui
{
    public partial class Form1 : Form
    {
        ILogger _Logger;
        IBasicServer _Server;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _Logger = new MultiLogger(
                new TextBoxLogger(txtLog),
                new FileLogger(@"C:\_qrsc\ServiceLogs\ServiceHostGui.{dddd}.log"));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // (re)start the service
            DisposeHelper.SafeDispose(ref _Server);
            _Server = new ServiceHost(_Logger);
            _Server.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // stop the service
            DisposeHelper.SafeDispose(ref _Server);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (ICoreClient client = new CoreClientFactory(_Logger).Create())
            {
                ServiceHostRulesLoader.Load(_Logger, client);
                FileImportRuleLoader.Load(_Logger, client);
                MarketDataConfigHelper.LoadProviderRules(_Logger, client);
                AlertRulesLoader.Load(_Logger, client);
                TradeImportRuleLoader.Load(_Logger, client);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeHelper.SafeDispose(ref _Server);
        }

    }
}
