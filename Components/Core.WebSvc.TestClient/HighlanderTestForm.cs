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
using System.ServiceModel;
using Highlander.Core.WebSvc.TestClient.HighlanderServiceReference;
using Highlander.Utilities.Logging;

namespace Highlander.Core.WebSvc.TestClient
{
    public partial class HighlanderTestForm : Form
    {
        private ILogger _logger;

        public HighlanderTestForm()
        {
            InitializeComponent();
        }

        private void BtnConnectClick(object sender, EventArgs e)
        {
            try
            {
                var binding = new BasicHttpBinding
                                  {
                                      BypassProxyOnLocal = false,
                                      UseDefaultWebProxy = true,
                                      Security = {Mode = BasicHttpSecurityMode.TransportCredentialOnly}
                                  };
                //binding.Name = "FXServiceSoap";
                //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                //binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                //binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                //binding.MaxBufferSize = 65536;
                //binding.MaxBufferPoolSize = 4 * 1024 * 1024; // default = 524288;
                //binding.MaxReceivedMessageSize = 4 * 1024 * 1024; // default = 65536;
                //binding.ReaderQuotas.MaxDepth = 32;
                //binding.ReaderQuotas.MaxStringContentLength = 8192;
                //binding.ReaderQuotas.MaxArrayLength = 16384;
                //binding.ReaderQuotas.MaxBytesPerRead = 4096;
                //binding.ReaderQuotas.MaxNameTableCharCount = 16384;
                const string url = "http://localhost:9222/Highlander_Prxy_IWebProxyV101";
                using (var client = new WebProxyV101Client(binding, new EndpointAddress(url)))
                {
                    V101ResultSet result = client.V101LoadObjectByName("test");
                    if (result.Error != null)
                    {
                        _logger.LogError(" Failed: {0}: {1}", result.Error.FullName, result.Error.Message);
                    }
                    else
                    {
                        _logger.LogDebug("Result: {0} items", result.Items.Length);
                        foreach (var item in result.Items)
                        {
                            _logger.LogDebug("  {0} ({1}) {2}", item.ItemName, item.DataTypeName, item.ItemId);
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                _logger.Log(excp);
            }
        }

        private void Form1Load(object sender, EventArgs e)
        {
            _logger = new TextBoxLogger(txtLog);
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            //xxx;
        }
    }
}
