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
using System.Threading;
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.GrpcService.Data;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V1.Tests
{
    /// <summary>
    /// Summary description for CacheTests
    /// </summary>
    [TestClass]
    public class CoreCacheTests
    {
        private static HighlanderContext _dbContext;

        public CoreCacheTests()
        {
            _dbContext = new HighlanderContext(null);
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestSaveLoadPrivateCacheItems()
        {
            using Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            using CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router, _dbContext);
            server.Start();
            using ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create();
            using ICoreCache cacheA = client.CreateCache();
            Guid id = cacheA.SavePrivateObject("data", "name1", null);
            //ICoreItem item = cacheA.LoadPrivateItem<string>("name1");
            ICoreItem item = cacheA.LoadItem<string>("name1");
            string data = (string)item.Data;
            Assert.AreEqual("data", data);
        }

        [TestMethod]
        public void TestUttCoreCaching()
        {
            using Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            using CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router, _dbContext);
            server.Start();
            using ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create();
            using ICoreCache cacheA = client.CreateCache();
            using ICoreCache cacheB = client.CreateCache();
            using (cacheA.SubscribeNoWait<TestData>(Expr.ALL, null, null))
            {
                using (cacheB.SubscribeNoWait<TestData>(Expr.ALL, null, null))
                {
                    // publish an item and check all caches are consistent
                    Guid id0 = cacheA.SaveObject(new TestData("Zero", 0), "Item0", null, TimeSpan.MaxValue);
                    // cacheA is publisher - should be immediately consistent
                    Assert.AreEqual(1, cacheA.ItemCount);
                    ICoreItem item0A = cacheA.Items[0];
                    Assert.IsNotNull(item0A);
                    TestData data0A = (TestData)item0A.Data;
                    Assert.IsNotNull(data0A);
                    Assert.AreEqual("Zero", (data0A.field1));
                    // cacheB is not publisher - allow for propagation delay
                    Thread.Sleep(500);
                    // both caches should now be consistent
                    Assert.AreEqual(1, cacheA.ItemCount);
                    Assert.AreEqual(1, cacheB.ItemCount);
                    ICoreItem item0B = cacheA.Items[0];
                    Assert.IsNotNull(item0B);
                    TestData data0B = (TestData)item0B.Data;
                    Assert.IsNotNull(data0B);
                    Assert.AreEqual("Zero", (data0B.field1));
                    Assert.AreEqual(item0A.Id, item0B.Id);
                }
                // cacheB unsubscribed - should not receive updates from cacheA
                Guid id1 = cacheA.SaveObject(new TestData("One", 1), "Item1", null);
                ICoreItem data1A = cacheA.LoadItem<TestData>("Item1");
                Guid id2 = cacheB.SaveObject(new TestData("Two", 2), "Item2", null);
                ICoreItem data2B = cacheB.LoadItem<TestData>("Item2");
                // object is immediately available in local cache but not others
                Assert.IsNotNull(data1A);
                Assert.AreEqual(id1, data1A.Id);
                Assert.IsNotNull(data2B);
                Assert.AreEqual(id2, data2B.Id);
                // wait a bit for propagation
                Thread.Sleep(500);
                // both caches should now be 'up-to-date'
                // - A should receive from B, but not vice versa
                Assert.AreEqual(3, cacheA.ItemCount);
                ICoreItem data2A = cacheA.LoadItem<TestData>("Item2");
                Assert.IsNotNull(data2A);
                Assert.AreEqual(id2, data2A.Id);
                // - B should be inconsistent with A
                Assert.AreEqual(2, cacheB.ItemCount);
                // - but becomes consistent as soon as we load the missing item
                ICoreItem data1B = cacheB.LoadItem<TestData>("Item1");
                Assert.AreEqual(3, cacheB.ItemCount);
                Assert.IsNotNull(data1B);
                Assert.AreEqual(id1, data1B.Id);
            }
        }
    }
}
