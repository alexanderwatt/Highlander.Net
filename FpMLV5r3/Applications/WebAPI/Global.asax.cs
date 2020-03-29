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
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

namespace Highlander.WebAPI.V5r3
{
    /// <summary>
    /// 
    /// </summary>
    public class HighlanderWebApplication : HttpApplication
    {
        /// <summary>
        /// The logger
        /// </summary>
        public static readonly Reference<ILogger> LoggerRef = Reference<ILogger>.Create(new TraceLogger(true));

        /// <summary>
        /// Application start up.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
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
            }
            catch (Exception ex)
            {
                LoggerRef.Target.LogError(ex);
                throw;
            }
        }
    }
}
