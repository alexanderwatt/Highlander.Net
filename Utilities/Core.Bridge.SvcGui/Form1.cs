using System;
using System.Windows.Forms;
using Core.Common;
using Core.V34;
using Orion.Build;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

namespace Core.Bridge.SvcGui
{
    public partial class Form1 : Form
    {
        private Reference<ILogger> _loggerRef;
        private Reference<ICoreClient> _clientRef;
        private ServerBridge _server;

        private void StartUp()
        {
            _clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(_loggerRef).SetEnv(BuildConst.BuildEnv).Create());
            // start the service
            _server = new ServerBridge {LoggerRef = _loggerRef, Client = _clientRef};
            _server.Start();
        }

        private void CleanUp()
        {
            // stop the service
            DisposeHelper.SafeDispose(ref _server);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1Load(object sender, EventArgs e)
        {
            _loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanUp();
        }

        private void BtnStartClick(object sender, EventArgs e)
        {
            CleanUp();
            StartUp();
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            CleanUp();
        }
    }
}
