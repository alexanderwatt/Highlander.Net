/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Windows.Forms;
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

namespace Highlander.Core.Bridge.SvcGui
{
    public partial class BridgeForm : Form
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

        public BridgeForm()
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
