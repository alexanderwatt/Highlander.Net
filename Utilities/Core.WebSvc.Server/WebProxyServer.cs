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

#region Usings

using System;
using System.Collections.Generic;
using Core.Common;
using Orion.Util.Helpers;

#endregion

namespace Core.WebSvc.Server
{
    public static class WebProxyPropName
    {
        //public const string EnvId = "EnvId";
        //public const string Port = "Port";
    }

    public class WebProxyServer : ServerBase2, IWebProxyV101
    {
        private CustomServiceHost<IWebProxyV101, WebProxyRecverV101> _webProxyServerHostV101;
        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                CheckNotStarted();
                _port = value;
            }
        }

        //public WebProxyServer(ILogger logger, EnvId env, int webPort, string serverAddress)
        //    : base(logger, env, serverAddress, null, null)
        //{
        //    // web server port
        //    _Port = webPort;
        //    if (_Port <= 0)
        //        _Port = EnvHelper.SvcPort(env, SvcId.CoreWProxy);
        //}

        protected override void OnServerStarted()
        {
            // set default port if not provided
            if (_port <= 0)
                _port = EnvHelper.SvcPort(IntClient.Target.ClientInfo.ConfigEnv, SvcId.CoreWProxy);
            _webProxyServerHostV101 = new CustomServiceHost<IWebProxyV101, WebProxyRecverV101>(
                Logger, new WebProxyRecverV101(this), ServiceHelper.FormatEndpoint(WcfConst.Http, _port),
                EnvHelper.SvcPrefix(SvcId.CoreWProxy), typeof(IWebProxyV101).Name, true);
        }

        protected override void OnServerStopping()
        {
            DisposeHelper.SafeDispose(ref _webProxyServerHostV101);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public V101ResultSet V101LoadObjectByName(string itemName)
        {
            var results = new List<V101CoreItem>();
            V101ErrorDetail error = null;
            try
            {
                ICoreItem item = IntClient.Target.LoadItem(itemName);
                if (item != null)
                {
                    results.Add(new V101CoreItem
                                    {
                        ItemId = item.Id,
                        ItemName = item.Name,
                        DataTypeName = item.DataTypeName,
                        ItemBody = item.Text
                    });
                }
            }
            catch (Exception excp)
            {
                error = new V101ErrorDetail(excp);
            }
            return new V101ResultSet
                       {
                 Items = results.ToArray(),
                 Error = error
            };
        }
    }
}
