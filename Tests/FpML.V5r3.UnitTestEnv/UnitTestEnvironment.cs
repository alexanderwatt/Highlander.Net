using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Core.Common;
using Core.Server;
using Core.V34;
using FpML.V5r3.Reporting;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.V5r3.Configuration;
using Exception = System.Exception;

namespace Orion.UnitTestEnv
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
            : this(Constants.EnvironmentProp.DefaultNameSpace)
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
