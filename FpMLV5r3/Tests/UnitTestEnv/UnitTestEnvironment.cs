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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Highlander.Configuration.Data.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.Core.V34;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Exception = System.Exception;

namespace Highlander.UnitTestEnv.V5r3
{
    public class UnitTestEnvironment
    {
        private CoreServer _server;
        private ICoreClient _client;
        private readonly ICoreCache _cache;
        private Reference<ILogger> _logRef = Reference<ILogger>.Create(new TraceLogger(true));

        public ILogger Logger => _logRef.Target;

        public string NameSpace;

        public UnitTestEnvironment()
            : this(EnvironmentProp.DefaultNameSpace)
        {}

        public UnitTestEnvironment(string nameSpace)
        {
            NameSpace = nameSpace;
            string env = EnvHelper.EnvName(EnvId.Utt_UnitTest);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var random = new Random(Environment.TickCount);
            int port = random.Next(8000, 8099);
            _server = new CoreServer(_logRef, env, NodeType.Router, port, WcfConst.NetTcp);
            _server.Start();
            _client = new CoreClientFactory(_logRef)
                .SetEnv(env)
                .SetServers("localhost:" + port.ToString(CultureInfo.InvariantCulture))
                .SetProtocols(WcfConst.NetTcp)
                .Create();
            _cache = _client.CreateCache();
            LoadConfigDataHelper.LoadConfigurationData(_logRef.Target, _cache, nameSpace);
            stopwatch.Stop();
            Debug.Print("Initialized test environment, in {0} seconds", stopwatch.Elapsed.TotalSeconds);
        }

        public ICoreClient Proxy => _client;

        public ICoreCache Cache => _cache;

        public void Dispose()
        {
            DisposeHelper.SafeDispose(ref _client);
            DisposeHelper.SafeDispose(ref _server);
            DisposeHelper.SafeDispose(ref _logRef);
        }

        public void TidyUpMarkets(IEnumerable<string> uniqueIds)
        {
            try
            {
                foreach (string uniqueId in uniqueIds)
                {
                    IExpression expression = Expr.IsEQU("UniqueIdentifier", uniqueId);
                    Cache.DeleteObjects<Market>(expression);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to remove from object store", ex);
            }
        }
    }
}
