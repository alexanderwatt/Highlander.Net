using System;
using System.Threading;
using Core.Common;
using Core.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

namespace Core.V34.Tests
{
    /// <summary>
    /// Summary description for CacheTests
    /// </summary>
    [TestClass]
    public class CoreCacheTests
    {
        public CoreCacheTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestSaveLoadPrivateCacheItems()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
            {
                server.Start();
                using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                using (ICoreCache cacheA = client.CreateCache())
                {
                    Guid id = cacheA.SavePrivateObject("data", "name1", null);
                    //ICoreItem item = cacheA.LoadPrivateItem<string>("name1");
                    ICoreItem item = cacheA.LoadItem<string>("name1");
                    string data = (string)item.Data;
                    Assert.AreEqual<string>("data", data);
                }
            }
        }


        [TestMethod]
        public void TestCoreCaching()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
            {
                server.Start();
                using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                using (ICoreCache cacheA = client.CreateCache())
                using (ICoreCache cacheB = client.CreateCache())
                {
                    using (cacheA.SubscribeNoWait<TestData>(Expr.ALL, null, null))
                    {
                        using (cacheB.SubscribeNoWait<TestData>(Expr.ALL, null, null))
                        {
                            // publish an item and check all caches are consistent
                            Guid id0 = cacheA.SaveObject<TestData>(new TestData("Zero", 0), "Item0", null, TimeSpan.MaxValue);

                            // cacheA is publisher - should be immediately consistent
                            Assert.AreEqual<int>(1, cacheA.ItemCount);
                            ICoreItem item0a = cacheA.Items[0];
                            Assert.IsNotNull(item0a);
                            TestData data0a = (TestData)item0a.Data;
                            Assert.IsNotNull(data0a);
                            Assert.AreEqual<string>("Zero", (data0a.field1));

                            // cacheB is not publisher - allow for propagation delay
                            Thread.Sleep(500);
                            // both caches should now be consistent
                            Assert.AreEqual<int>(1, cacheA.ItemCount);
                            Assert.AreEqual<int>(1, cacheB.ItemCount);
                            ICoreItem item0b = cacheA.Items[0];
                            Assert.IsNotNull(item0b);
                            TestData data0b = (TestData)item0b.Data;
                            Assert.IsNotNull(data0b);
                            Assert.AreEqual<string>("Zero", (data0b.field1));
                            Assert.AreEqual<Guid>(item0a.Id, item0b.Id);
                        }

                        // cacheB unsubscribed - should not receive updates from cacheA
                        Guid id1 = cacheA.SaveObject<TestData>(new TestData("One", 1), "Item1", null);
                        ICoreItem data1a = cacheA.LoadItem<TestData>("Item1");
                        Guid id2 = cacheB.SaveObject<TestData>(new TestData("Two", 2), "Item2", null);
                        ICoreItem data2b = cacheB.LoadItem<TestData>("Item2");
                        // object is immediately available in local cache but not others
                        Assert.IsNotNull(data1a);
                        Assert.AreEqual<Guid>(id1, data1a.Id);
                        Assert.IsNotNull(data2b);
                        Assert.AreEqual<Guid>(id2, data2b.Id);
                        // wait a bit for propagation
                        Thread.Sleep(500);
                        // both caches should now be 'up-to-date'
                        // - A should receive from B, but not vice versa
                        Assert.AreEqual<int>(3, cacheA.ItemCount);
                        ICoreItem data2a = cacheA.LoadItem<TestData>("Item2");
                        Assert.IsNotNull(data2a);
                        Assert.AreEqual<Guid>(id2, data2a.Id);
                        // - B should be inconsistent with A
                        Assert.AreEqual<int>(2, cacheB.ItemCount);
                        // - but becomes consistent as soon as we load the missing item
                        ICoreItem data1b = cacheB.LoadItem<TestData>("Item1");
                        Assert.AreEqual<int>(3, cacheB.ItemCount);
                        Assert.IsNotNull(data1b);
                        Assert.AreEqual<Guid>(id1, data1b.Id);
                    }
                }
            }
        }
    }
}
