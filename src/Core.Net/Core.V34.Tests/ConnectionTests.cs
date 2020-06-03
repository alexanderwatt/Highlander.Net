/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using System.ServiceModel;
using System.Threading;
using Highlander.Core.Bridge;
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V34.Tests
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void TestManualRecovery()
        {
            // demonstrates how an application should use the client in its simplest (transient) mode:
            // - exceptions should be handled by the application;
            // - client must be disposed and recreated after an exception has occurred;
            // - subscriptions are not supported.
            //TimeSpan outageDuration = TimeSpan.FromMinutes(1);
            //TimeSpan requestTimeout = TimeSpan.FromSeconds(30);
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                loggerRef.Target.LogDebug("----------> test commenced");
                // create unit test storage
                IStoreEngine unitTestStore = new UnitTestStoreEngine(loggerRef.Target);
                CoreServer server = null;
                try
                {
                    var serverSettings = new NamedValueSet();
                    serverSettings.Set(CfgPropName.NodeType, (int)NodeType.Server);
                    serverSettings.Set(CfgPropName.EnvName, "UTT");
                    server = new CoreServer(loggerRef, serverSettings) {StoreEngine = unitTestStore};
                    loggerRef.Target.LogDebug("----------> starting server");
                    server.Start();
                    // create 1st client and save object - should succeed
                    Guid id1;
                    using (ICoreClient client1 = new CoreClientFactory(loggerRef)
                        .SetEnv("UTT")
                        .Create())
                    {
                        id1 = client1.SaveObject(new TestData("Test1", 1), "Test1", null, TimeSpan.MaxValue);
                        // stop server and begin save - should fail with timeout
                        loggerRef.Target.LogDebug("----------> stopping server");
                        server.Stop();
                        DisposeHelper.SafeDispose(ref server);
                        client1.RequestTimeout = TimeSpan.FromSeconds(10);
                        UnitTestHelper.AssertThrows<TimeoutException>(() =>
                        {
                            client1.SaveObject(new TestData("Test2a", 2), "Test2", null, TimeSpan.MaxValue);
                        });
                        // further use of client throws more timeout errors
                        UnitTestHelper.AssertThrows<TimeoutException>(() => { client1.LoadObject<TestData>("Test2a"); });
                    }
                    // create 2nd client - should fail to connect
                    UnitTestHelper.AssertThrows<EndpointNotFoundException>("No server in list 'localhost:9113' found!", () =>
                    {
                        using (ICoreClient client2 = new CoreClientFactory(loggerRef)
                            .SetEnv("UTT")
                            .Create())
                        {
                            client2.SaveObject(new TestData("Test2b", 2), "Test2", null, TimeSpan.MaxValue);
                        }
                    });
                    // restart server
                    server = new CoreServer(loggerRef, serverSettings) {StoreEngine = unitTestStore};
                    loggerRef.Target.LogDebug("----------> restarting server");
                    server.Start();
                    // load 1st object - should succeed
                    ICoreItem item1;
                    ICoreItem item2;
                    using (ICoreClient client3 = new CoreClientFactory(loggerRef)
                        .SetEnv("UTT")
                        .Create())
                    {
                        item1 = client3.LoadItem<TestData>("Test1");
                        item2 = client3.LoadItem<TestData>("Test2");
                    }
                    Assert.IsNotNull(item1);
                    Assert.AreEqual(id1, item1.Id);
                    Assert.IsNull(item2);
                    // done
                    loggerRef.Target.LogDebug("----------> test completed");
                    server.Stop();
                    DisposeHelper.SafeDispose(ref server);
                }
                finally
                {
                    DisposeHelper.SafeDispose(ref server);
                }
            }
        }

        [TestMethod]
        public void TestServerBridge()
        {
            // required environment:
            // - local env (eg. DEV) server
            // - lower env (eg. UTT) server
            // creates the bridge, and check all messages published 
            //   in local env are re-published correctly in lower env
            EnvId localEnvId = EnvId.Dev_Development;
            string localEnvName = EnvHelper.EnvName(localEnvId);
            EnvId lowerEnvId = (localEnvId - 1);
            string lowerEnvName = EnvHelper.EnvName(lowerEnvId);
            int localPort = 9214;
            int lowerPort = 9114;
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                CoreClientFactory clientfactory = new CoreClientFactory(loggerRef);
                using (CoreServer localServer =
                    new CoreServer(loggerRef, localEnvName, NodeType.Router, localPort))
                using (CoreServer lowerServer =
                    new CoreServer(loggerRef, lowerEnvName, NodeType.Router, lowerPort))
                {
                    // start servers
                    localServer.Start();
                    lowerServer.Start();
                    // create clients
                    using (Reference<ICoreClient> localClientRef = Reference<ICoreClient>.Create(clientfactory.SetEnv(localEnvName).SetServers("localhost:" + localPort).Create()))
                    using (ICoreClient lowerClient = clientfactory.SetEnv(lowerEnvName).SetServers("localhost:" + lowerPort).Create())
                    {
                        using (ServerBridge bridge = new ServerBridge())
                        {
                            bridge.LoggerRef = loggerRef;
                            bridge.Client = localClientRef;
                            bridge.TargetClient = lowerClient;
                            // test begins here 
                            // - start the bridge
                            bridge.Start();
                            const int sendCount = 500;
                            const int maxWaitSeconds = 5;
                            long excpCount = 0;
                            long recdCount = 0;
                            // subscribe to objects on downstream server
                            ISubscription subs = lowerClient.Subscribe<TestData>(Expr.ALL,
                                delegate(ISubscription subscription, ICoreItem item)
                                {
                                    // receiver
                                    long count = Interlocked.Increment(ref recdCount);
                                    try
                                    {
                                        TestData data = (TestData)item.Data;
                                        //loggerRef.Target.LogDebug("Recd[{0}]", data.field2);
                                        Assert.AreEqual<long>(count, data.field2);
                                    }
                                    catch (Exception)
                                    {
                                        Interlocked.Increment(ref excpCount);
                                    }
                                }, null);

                            long sentCount = 0;
                            // publish n Server events
                            for (int i = 1; i <= sendCount; i++)
                            {
                                Interlocked.Increment(ref sentCount);
                                localClientRef.Target.SaveObject(new TestData("Test", i), "Test", null, TimeSpan.MaxValue);
                            }
                            // wait for a short period
                            DateTimeOffset waitStart = DateTimeOffset.Now;
                            DateTimeOffset waitExpiry = waitStart.AddSeconds(maxWaitSeconds);
                            while ((Interlocked.Add(ref recdCount, 0) < Interlocked.Add(ref sentCount, 0))
                                && (DateTimeOffset.Now < waitExpiry)
                                && (Interlocked.Add(ref excpCount, 0) == 0))
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(1.0));
                                loggerRef.Target.LogDebug("Recd/Sent: {0}/{1} items...", Interlocked.Add(ref recdCount, 0), Interlocked.Add(ref sentCount, 0));
                            }
                            loggerRef.Target.LogDebug("Duration: {0}", (DateTimeOffset.Now - waitStart));
                            Assert.AreEqual(0, excpCount);
                            Assert.AreEqual(sendCount, recdCount);
                            // done
                            Assert.IsTrue(true);
                        }
                    }
                }
            }
        }
    }
}
