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
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.MDS.Client.V5r3;
using Highlander.MDS.Server.V5r3;
using Highlander.Metadata.Common;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.WinTools;

#endregion

namespace Highlander.MDS.ServiceGui.V5r3
{
    public partial class MDSServiceForm : Form
    {
        private Reference<ILogger> _loggerRef;
        private IServerBase2 _server;
        private Reference<ICoreClient> _clientRef;


        public MDSServiceForm()
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
            chkChangePort.Text = $"Change server port from default ({defaultPort}) to:";
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
                settings.Set(MdsPropName.Port, int.Parse(txtChangePort.Text));
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