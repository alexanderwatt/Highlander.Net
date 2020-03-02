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

using System;
using System.Reflection;
using System.Web;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

namespace Highlander.PublisherWebService.V5r3
{
    /// <summary>
    /// Global class
    /// </summary>
    public class Global : HttpApplication
    {
        /// <summary>
        /// The logger
        /// </summary>
        public static Reference<ILogger> LoggerRef = Reference<ILogger>.Create(new TraceLogger(true));
        //private CoreServer _server;
        private ICoreClient _client;
        private ICoreCache _cache;

        #region Application Methods

        /// <summary>
        /// Application start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Start(object sender, EventArgs e)
        {
            const string fullAppName = "Highlander.PublisherWebService.V5r3";
            LoggerRef.Target.LogInfo("Starting up...");
            try
            {
                //var stopwatch = new Stopwatch();
                //stopwatch.Start();
                var factory = new CoreClientFactory(LoggerRef)
                    .SetEnv("DEV")
                    .SetApplication(Assembly.GetExecutingAssembly())
                    .SetProtocols(WcfConst.AllProtocolsStr);
                _client = factory.SetServers("localhost").Create();
                _cache = _client.CreateCache();
                //stopwatch.Stop();
                //Debug.Print("Initialized test environment, in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                LoggerRef.Target.LogInfo("Loaded..." + fullAppName);
            }
            catch (Exception excp)
            {
                LoggerRef.Target.Log(excp);
            }
        }

        /// <summary>
        /// Application end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_End(object sender, EventArgs e)
        {
            try
            {
                LoggerRef.Target.LogInfo("Stopped.");
                DisposeHelper.SafeDispose(ref _client);
                DisposeHelper.SafeDispose(ref _cache);
            }
            catch (Exception ex)
            {
                LoggerRef.Target.LogError(ex);
                throw;
            }
        }

        #endregion
    }
}