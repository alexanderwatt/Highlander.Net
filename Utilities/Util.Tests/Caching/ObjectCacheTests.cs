using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Caching;

namespace Util.Tests.Caching
{
    /// <summary>
    /// Summary description for CacheTests
    /// </summary>
    [TestClass]
    public class ObjectCacheTests
    {
        public ObjectCacheTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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

        private class TestCacheCS : CacheBase<string, string, object>
        {
            public int LoadCount { get; private set; }
            public int SaveCount { get; private set; }
            protected override string OnLoad(string key, object userParam)
            {
                LoadCount += 1;
                return String.Format("Key={0}", key);
            }
            protected override void OnSave(string oldItem, string newItem, object userParam)
            {
                SaveCount += 1;
            }
        }

        private class TestCacheCI : CacheBase<string, string, object>
        {
            public int OnLoadCount { get; private set; }
            public int OnSaveCount { get; private set; }
            public int ClearedCount { get; private set; }
            public int CreatedCount { get; private set; }
            public int UpdatedCount { get; private set; }
            public int ExpiredCount { get; private set; }
            public int RemovedCount { get; private set; }
            protected override string OnGetKey(string userKey) { return userKey.ToLower(); }
            protected override string OnLoad(string key, object userParam)
            {
                OnLoadCount += 1;
                return String.Format("Key={0}", key);
            }
            protected override void OnSave(string oldItem, string newItem, object userParam)
            {
                OnSaveCount += 1;
            }
            protected override void OnUpdate(CacheChange change, string userKey, string oldValue, string newValue, object userParam)
            {
                switch (change)
                {
                    case CacheChange.CacheCleared:
                        ClearedCount += 1;
                        break;
                    case CacheChange.ItemCreated:
                        CreatedCount += 1;
                        break;
                    case CacheChange.ItemUpdated:
                        UpdatedCount += 1;
                        break;
                    case CacheChange.ItemRemoved:
                        RemovedCount += 1;
                        break;
                    case CacheChange.ItemExpired:
                        ExpiredCount += 1;
                        break;
                    default:
                        throw new NotSupportedException("CacheChange=" + change);
                }
            }
        }

        [TestMethod]
        public void TestObjectCaching()
        {
            // test case sensitive keys
            {
                TestCacheCS cache = new TestCacheCS(); ;
                cache.Put("Key1", "Value1");
                Assert.AreEqual<int>(1, cache.GetValues().Count);

                cache.Put("key1", "Value2");
                Assert.AreEqual<int>(2, cache.GetValues().Count);
                Assert.AreEqual<string>("Value1", cache.Get("Key1"));
                Assert.AreEqual<string>("Value2", cache.Get("key1"));
            }
            // test case in-sensitive keys
            {
                TestCacheCI cache = new TestCacheCI();
                cache.Put("Key1", "Value1");
                Assert.AreEqual<int>(1, cache.GetValues().Count);

                cache.Put("key1", "Value2");
                Assert.AreEqual<int>(1, cache.GetValues().Count);
                Assert.AreEqual<string>("Value2", cache.Get("Key1"));
            }

            // test cache load/save overrides
            {
                TestCacheCI cache = new TestCacheCI();
                Assert.AreEqual<int>(0, cache.GetValues().Count);
                Assert.AreEqual<int>(0, cache.OnLoadCount);
                Assert.AreEqual<int>(0, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(0, cache.CreatedCount);
                Assert.AreEqual<int>(0, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                cache.Get("Key1");
                Assert.AreEqual<int>(1, cache.GetValues().Count);
                Assert.AreEqual<int>(1, cache.OnLoadCount);
                Assert.AreEqual<int>(0, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(1, cache.CreatedCount);
                Assert.AreEqual<int>(0, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                cache.Get("Key1");
                Assert.AreEqual<int>(1, cache.GetValues().Count);
                Assert.AreEqual<int>(1, cache.OnLoadCount);
                Assert.AreEqual<int>(0, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(1, cache.CreatedCount);
                Assert.AreEqual<int>(0, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                // force reload
                cache.Get("Key1", LoadSaveType.Force);
                Assert.AreEqual<int>(1, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(0, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(1, cache.CreatedCount);
                Assert.AreEqual<int>(1, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                cache.Put("Key1", "Value1");
                Assert.AreEqual<int>(1, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(1, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(1, cache.CreatedCount);
                Assert.AreEqual<int>(2, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                cache.Put("Key2", "Value2");
                Assert.AreEqual<int>(2, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(2, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(2, cache.CreatedCount);
                Assert.AreEqual<int>(2, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                // skip save - value has not changed
                cache.Put("Key2", "Value2");
                Assert.AreEqual<int>(2, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(2, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(2, cache.CreatedCount);
                Assert.AreEqual<int>(2, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                // forced save - value has not changed
                cache.Put("Key2", "Value2", LoadSaveType.Force);
                Assert.AreEqual<int>(2, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(3, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(2, cache.CreatedCount);
                Assert.AreEqual<int>(2, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                // expiry
                cache.Put("Key3", "Value3", LoadSaveType.Default, null, TimeSpan.FromSeconds(1));
                Assert.AreEqual<int>(3, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(4, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(3, cache.CreatedCount);
                Assert.AreEqual<int>(2, cache.UpdatedCount);
                Assert.AreEqual<int>(0, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                Thread.Sleep(TimeSpan.FromSeconds(1.5));
                cache.Get("Key3", LoadSaveType.Avoid);
                Assert.AreEqual<int>(2, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(4, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(3, cache.CreatedCount);
                Assert.AreEqual<int>(2, cache.UpdatedCount);
                Assert.AreEqual<int>(1, cache.ExpiredCount);
                Assert.AreEqual<int>(0, cache.RemovedCount);

                // removals
                cache.Remove("Key2");
                Assert.AreEqual<int>(1, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(4, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(3, cache.CreatedCount);
                Assert.AreEqual<int>(2, cache.UpdatedCount);
                Assert.AreEqual<int>(1, cache.ExpiredCount);
                Assert.AreEqual<int>(1, cache.RemovedCount);

                cache.Remove("Key1");
                Assert.AreEqual<int>(0, cache.GetValues().Count);
                Assert.AreEqual<int>(2, cache.OnLoadCount);
                Assert.AreEqual<int>(4, cache.OnSaveCount);
                Assert.AreEqual<int>(0, cache.ClearedCount);
                Assert.AreEqual<int>(3, cache.CreatedCount);
                Assert.AreEqual<int>(2, cache.UpdatedCount);
                Assert.AreEqual<int>(1, cache.ExpiredCount);
                Assert.AreEqual<int>(2, cache.RemovedCount);
            }
        }
    }
}
