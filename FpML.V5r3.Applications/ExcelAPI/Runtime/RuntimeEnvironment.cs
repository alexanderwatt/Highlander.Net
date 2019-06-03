/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

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

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Core.Common;
using Core.Server;
using Core.V34;
using FpML.V5r3.Reporting;
using HLV5r3.Helpers;
using Orion.Constants;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

#endregion

namespace HLV5r3.Runtime
{
    ///<summary>
    /// Contains functions called by ExcelApi on StartUp and CleanUp
    ///</summary>
    public class RuntimeEnvironment
    {
        #region Properties

        public Reference<ILogger> LogRef; // { get; } //set; }
        private CoreServer _server;
        private ICoreClient _client;
        public readonly string NameSpace ;

        public RuntimeEnvironment(string nameSpace)
        {
            NameSpace = nameSpace;
            string fullAppName = $"ExcelAPI-{ApplicationHelper.Diagnostics("FileVersion")}";
            try
            {
                //var stopwatch = new Stopwatch();
                LogRef = Reference<ILogger>.Create(new TraceLogger(true));
                //stopwatch.Start();
                CoreClientFactory factory = new CoreClientFactory(LogRef)
                    .SetEnv("DEV")
                    .SetApplication(Assembly.GetExecutingAssembly())
                    .SetProtocols(WcfConst.AllProtocolsStr);
                _client = factory.SetServers("localhost").Create();
                Cache = _client.CreateCache();
                Log(LogRef.Target, "Starting...", fullAppName, "StartUp");
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
            DisposeHelper.SafeDispose(ref _client);
            DisposeHelper.SafeDispose(ref _server);
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
