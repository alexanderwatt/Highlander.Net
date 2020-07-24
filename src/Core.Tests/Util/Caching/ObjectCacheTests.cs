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
using Highlander.Utilities.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests.Caching
{
    /// <summary>
    /// Summary description for CacheTests
    /// </summary>
    [TestClass]
    public class ObjectCacheTests
    {
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

        private class TestCacheCS : CacheBase<string, string, object>
        {
            private int LoadCount { get; set; }
            private int SaveCount { get; set; }
            protected override string OnLoad(string key, object userParam)
            {
                LoadCount += 1;
                return $"Key={key}";
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
                return $"Key={key}";
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
                Assert.AreEqual(1, cache.GetValues().Count);

                cache.Put("key1", "Value2");
                Assert.AreEqual(2, cache.GetValues().Count);
                Assert.AreEqual("Value1", cache.Get("Key1"));
                Assert.AreEqual("Value2", cache.Get("key1"));
            }
            // test case in-sensitive keys
            {
                TestCacheCI cache = new TestCacheCI();
                cache.Put("Key1", "Value1");
                Assert.AreEqual(1, cache.GetValues().Count);

                cache.Put("key1", "Value2");
                Assert.AreEqual(1, cache.GetValues().Count);
                Assert.AreEqual("Value2", cache.Get("Key1"));
            }

            // test cache load/save overrides
            {
                TestCacheCI cache = new TestCacheCI();
                Assert.AreEqual(0, cache.GetValues().Count);
                Assert.AreEqual(0, cache.OnLoadCount);
                Assert.AreEqual(0, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(0, cache.CreatedCount);
                Assert.AreEqual(0, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                cache.Get("Key1");
                Assert.AreEqual(1, cache.GetValues().Count);
                Assert.AreEqual(1, cache.OnLoadCount);
                Assert.AreEqual(0, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(1, cache.CreatedCount);
                Assert.AreEqual(0, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                cache.Get("Key1");
                Assert.AreEqual(1, cache.GetValues().Count);
                Assert.AreEqual(1, cache.OnLoadCount);
                Assert.AreEqual(0, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(1, cache.CreatedCount);
                Assert.AreEqual(0, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                // force reload
                cache.Get("Key1", LoadSaveType.Force);
                Assert.AreEqual(1, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(0, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(1, cache.CreatedCount);
                Assert.AreEqual(1, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                cache.Put("Key1", "Value1");
                Assert.AreEqual(1, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(1, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(1, cache.CreatedCount);
                Assert.AreEqual(2, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                cache.Put("Key2", "Value2");
                Assert.AreEqual(2, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(2, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(2, cache.CreatedCount);
                Assert.AreEqual(2, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                // skip save - value has not changed
                cache.Put("Key2", "Value2");
                Assert.AreEqual(2, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(2, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(2, cache.CreatedCount);
                Assert.AreEqual(2, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                // forced save - value has not changed
                cache.Put("Key2", "Value2", LoadSaveType.Force);
                Assert.AreEqual(2, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(3, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(2, cache.CreatedCount);
                Assert.AreEqual(2, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                // expiry
                cache.Put("Key3", "Value3", LoadSaveType.Default, null, TimeSpan.FromSeconds(1));
                Assert.AreEqual(3, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(4, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(3, cache.CreatedCount);
                Assert.AreEqual(2, cache.UpdatedCount);
                Assert.AreEqual(0, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                Thread.Sleep(TimeSpan.FromSeconds(1.5));
                cache.Get("Key3", LoadSaveType.Avoid);
                Assert.AreEqual(2, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(4, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(3, cache.CreatedCount);
                Assert.AreEqual(2, cache.UpdatedCount);
                Assert.AreEqual(1, cache.ExpiredCount);
                Assert.AreEqual(0, cache.RemovedCount);

                // removals
                cache.Remove("Key2");
                Assert.AreEqual(1, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(4, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(3, cache.CreatedCount);
                Assert.AreEqual(2, cache.UpdatedCount);
                Assert.AreEqual(1, cache.ExpiredCount);
                Assert.AreEqual(1, cache.RemovedCount);

                cache.Remove("Key1");
                Assert.AreEqual(0, cache.GetValues().Count);
                Assert.AreEqual(2, cache.OnLoadCount);
                Assert.AreEqual(4, cache.OnSaveCount);
                Assert.AreEqual(0, cache.ClearedCount);
                Assert.AreEqual(3, cache.CreatedCount);
                Assert.AreEqual(2, cache.UpdatedCount);
                Assert.AreEqual(1, cache.ExpiredCount);
                Assert.AreEqual(2, cache.RemovedCount);
            }
        }
    }
}
