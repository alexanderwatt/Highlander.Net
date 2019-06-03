using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Core.Common;
using Core.Server;
using Core.V34;
using FpML.V5r10.ConfigData;
using FpML.V5r10.Reporting;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Exception = System.Exception;

namespace Orion.UnitTestEnv
{
    public class CalendarUnitTestEnvironment
    {
        private CoreServer _server;
        private ICoreClient _client;
        private Reference<ILogger> _logRef = Reference<ILogger>.Create(new TraceLogger(true));
        public ILogger Logger => _logRef.Target;
        public string NameSpace;

        public CalendarUnitTestEnvironment()
            : this(Constants.EnvironmentProp.LatestNameSpace)
        { }

        public CalendarUnitTestEnvironment(string nameSpace)
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
            Cache = _client.CreateCache();
            LoadConfigDataHelper.LoadCalendarConfigurationData(_logRef.Target, Cache, nameSpace);
            stopwatch.Stop();
            Debug.Print("Initialized test environment, in {0} seconds", stopwatch.Elapsed.TotalSeconds);
        }

        public ICoreClient Proxy => _client;
        public ICoreCache Cache { get; }

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

    public class CurveUnitTestEnvironment
    {
        private CoreServer _server;
        private ICoreClient _client;
        private Reference<ILogger> _logRef = Reference<ILogger>.Create(new TraceLogger(true));
        public ILogger Logger => _logRef.Target;
        public string NameSpace;

        public CurveUnitTestEnvironment()
            : this(Constants.EnvironmentProp.LatestNameSpace)
        { }

        public CurveUnitTestEnvironment(string nameSpace)
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
            Cache = _client.CreateCache();
            LoadConfigDataHelper.LoadCurveConfigurationData(_logRef.Target, Cache, nameSpace);
            stopwatch.Stop();
            Debug.Print("Initialized test environment, in {0} seconds", stopwatch.Elapsed.TotalSeconds);
        }

        public ICoreClient Proxy => _client;
        public ICoreCache Cache { get; }

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

    public class UnitTestEnvironment
    {
        private CoreServer _server;
        private ICoreClient _client;
        private Reference<ILogger> _logRef = Reference<ILogger>.Create(new TraceLogger(true));
        public ILogger Logger => _logRef.Target;
        public string NameSpace;

        public UnitTestEnvironment()
            : this(Constants.EnvironmentProp.LatestNameSpace)
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
            Cache = _client.CreateCache();
            LoadConfigDataHelper.LoadConfigurationData(_logRef.Target, Cache, nameSpace);
            stopwatch.Stop();
            Debug.Print("Initialized test environment, in {0} seconds", stopwatch.Elapsed.TotalSeconds);
        }

        public ICoreClient Proxy => _client;
        public ICoreCache Cache { get; }

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
