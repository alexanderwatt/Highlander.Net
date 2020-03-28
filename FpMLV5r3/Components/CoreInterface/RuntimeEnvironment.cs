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

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Highlander.Build;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

#endregion

namespace Highlander.Core.Interface.V5r3
{
    ///<summary>
    /// Contains functions called by the Api on StartUp and CleanUp
    ///</summary>
    public class RuntimeEnvironment
    {
        #region Properties

        public Reference<ILogger> LogRef;
        //private CoreServer _server;
        private ICoreClient _client;
        public readonly string NameSpace;
        public readonly string ApplicationName;

        public RuntimeEnvironment(string nameSpace)
        {
            NameSpace = nameSpace;
            ApplicationName = $"Highlander.Core.API-{ApplicationHelper.Diagnostics("FileVersion")}";
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                Log(LogRef.Target, "Starting...", ApplicationName, "StartUp");
                LogRef = Reference<ILogger>.Create(new TraceLogger(true));
                CoreClientFactory factory = new CoreClientFactory(LogRef)
                    .SetEnv(BuildConst.BuildEnv)
                    .SetApplication(Assembly.GetExecutingAssembly())
                    .SetProtocols(WcfConst.AllProtocolsStr);
                _client = factory.SetServers("localhost").Create();
                Cache = _client.CreateCache();
                var time = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();
                Log(LogRef.Target, "The application :" + ApplicationName + "took " + time + " to start.", ApplicationName, "RuntimeEnvironment");
            }
            catch (System.Exception excp)
            {
                LogRef.Target.Log(excp);
            }
        }
        public ICoreClient Proxy => _client;

        public ICoreCache Cache { get; }

        public void Dispose()
        {
            Log(LogRef.Target, "Closing...", ApplicationName, "Closed");
            DisposeHelper.SafeDispose(ref _client);
//            DisposeHelper.SafeDispose(ref _server);
        }

        public void TidyUpMarkets(IEnumerable<string> uniqueIds)
        {
            try
            {
                foreach (string uniqueId in uniqueIds)
                {
                    IExpression expression = Expr.BoolAND(Expr.IsEQU("UniqueIdentifier", uniqueId),Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace));
                    Cache.DeleteObjects<Market>(expression);
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("Failed to remove from object store", ex);
            }
        }

        #endregion

        private static void Log(ILogger logger, string message, string appName, string methodName)
        {
            var fullMessage = new StringBuilder(appName);
            fullMessage.Append(".Environment.");
            fullMessage.Append(methodName);
            fullMessage.Append(" ");
            fullMessage.Append(message);
            logger.LogDebug(fullMessage.ToString());
        }
    }
}
