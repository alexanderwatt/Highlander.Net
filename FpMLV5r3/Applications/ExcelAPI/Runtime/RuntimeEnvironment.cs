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
using System.Reflection;
using System.Text;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.Core.V34;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using HLV5r3.Helpers;
using ApplicationHelper = HLV5r3.Helpers.ApplicationHelper;

#endregion

namespace HLV5r3.Runtime
{
    ///<summary>
    /// Contains functions called by ExcelApi on StartUp and CleanUp
    ///</summary>
    public class RuntimeEnvironment
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public Reference<ILogger> LogRef; // { get; } //set; }
        private CoreServer _server;
        private ICoreClient _client;
        /// <summary>
        /// 
        /// </summary>
        public readonly string NameSpace ;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpace"></param>
        public RuntimeEnvironment(string nameSpace)
        {
            NameSpace = nameSpace;
            string fullAppName = $"HLExcelAPI-{ApplicationHelper.Diagnostics("FileVersion")}";
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
        /// <summary>
        /// 
        /// </summary>
        public ICoreClient Proxy => _client;

        /// <summary>
        /// 
        /// </summary>
        public ICoreCache Cache { get; }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            DisposeHelper.SafeDispose(ref _client);
            DisposeHelper.SafeDispose(ref _server);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueIds"></param>
        /// <exception cref="Exception"></exception>
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
