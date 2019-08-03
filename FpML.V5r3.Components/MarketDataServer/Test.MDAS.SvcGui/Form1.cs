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
using System.Linq;
using System.Windows.Forms;
using Core.Common;
using Core.V34;
using Metadata.Common;
using Orion.Build;
using Orion.MDAS.Client;
using Orion.MDAS.Server;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;
using Orion.Util.WinTools;

#endregion

namespace QRMarketDataSvcGUI
{
    public partial class Form1 : Form
    {
        private Reference<ILogger> _loggerRef;
        private IServerBase2 _server;
        private Reference<ICoreClient> _clientRef;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1Load(object sender, EventArgs e)
        {
            // create logger
            _loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));

            // init controls
            // - form title
            WinFormHelper.SetAppFormTitle(this, BuildConst.BuildEnv);
            // - provider list
            foreach (MDSProviderId provider in Enum.GetValues(typeof(MDSProviderId)))
            {
                bool enabled = false;
                switch (provider)
                {
                    case MDSProviderId.GlobalIB: enabled = true; break;
                    case MDSProviderId.Bloomberg: enabled = true; break;
                    case MDSProviderId.Simulator: enabled = true; break;
                }
                clbEnabledProviders.Items.Add(provider.ToString(), enabled);
            }
            // - server port
            int defaultPort = EnvHelper.SvcPort(EnvHelper.ParseEnvName(BuildConst.BuildEnv), SvcId.MarketData);
            chkChangePort.Text = String.Format("Change server port from default ({0}) to:", defaultPort);
        }

        private void Form1FormClosed(object sender, FormClosedEventArgs e)
        {
            CleanUp();
            _loggerRef.Dispose();
        }

        private void StartUp()
        {
            // stop server if already running
            CleanUp();
            // start the service
            var enabledProviders = (from int index in clbEnabledProviders.CheckedIndices select ((MDSProviderId) index).ToString()).ToList();
            var settings = new NamedValueSet();
            if(chkChangePort.Checked)
                settings.Set(MdsPropName.Port, Int32.Parse(txtChangePort.Text));
            settings.Set(MdsPropName.EnabledProviders, enabledProviders.ToArray());
            _clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(_loggerRef).SetEnv(BuildConst.BuildEnv).Create());
            _server = new MarketDataServer {LoggerRef = _loggerRef, Client = _clientRef, OtherSettings = settings};
            _server.Start();
        }

        private void CleanUp()
        {
            DisposeHelper.SafeDispose(ref _server);
            DisposeHelper.SafeDispose(ref _clientRef);
        }

        private void BtnStartClick(object sender, EventArgs e)
        {
            StartUp();
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            CleanUp();
        }
    }
}