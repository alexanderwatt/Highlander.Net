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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.GrpcService.Data;
using Highlander.Utilities.Caching;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V1.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class BasicTests
    {
        public BasicTests()
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
        public void TestBasicStartStop()
        {
            HighlanderContext dbContext = null;
            // start the server, connect a client1, and shutdown
            using Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            using CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router, dbContext);
            // start server
            server.Start();
            // connect client
            using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
            {
                // load something
                NamedValueSet settings = client.LoadAppSettings();
                loggerRef.Target.LogDebug("Loaded settings:");
                settings.LogValues((text) => loggerRef.Target.LogDebug("  " + text));
            }
            // explicit shutdown
            // - not necessary in a "using" block but run it anyway
            server.Stop();
        }

        [TestMethod]
        public void TestRepeatableStartStop()
        {
            const int maxLoops = 2;
            using (Reference<ILogger> outerLoggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                for (int loop = 0; loop < maxLoops; loop++)
                {
                    using (Reference<ILogger> innerLoggerRef = Reference<ILogger>.Create(new FilterLogger(outerLoggerRef,
                        $"[{loop}] ")))
                    {
                        using (CoreServer server = new CoreServer(innerLoggerRef, "UTT", NodeType.Router))
                        {
                            // start server
                            server.Start();
                            // connect client
                            using (ICoreClient client = new CoreClientFactory(innerLoggerRef).SetEnv("UTT").Create())
                            {
                                // load something
                                //NamedValueSet settings = client.LoadAppSettings();
                                //innerLogger.LogDebug("Loaded settings:");
                                //settings.LogValues((text) => innerLogger.LogDebug("  " + text));
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestAppConfiguration()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    server.Start();
                    using (ICoreClient manager = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    using (ICoreClient client1 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // check initial config is empty
                        NamedValueSet nvs0 = client1.LoadAppSettings("UnitTest");
                        Assert.AreEqual(0, nvs0.Count);
                        // set a global (all- envs, all-users, all-hosts) property
                        {
                            int value = 1;
                            NamedValueSet input = new NamedValueSet();
                            input.Set("prop1", value);
                            manager.SaveAppSettings(input, "UnitTest", null, null, true);
                            NamedValueSet output = client1.LoadAppSettings("UnitTest");
                            Assert.AreEqual(1, output.Count);
                            Assert.AreEqual(value, output.GetValue("prop1", 0));
                        }
                        // set a user-specific property (overrides global)
                        {
                            int value = 2;
                            NamedValueSet input = new NamedValueSet();
                            input.Set("prop1", value);
                            manager.SaveAppSettings(input, "UnitTest", client1.ClientInfo.UserName, null, false);
                            NamedValueSet output = client1.LoadAppSettings("UnitTest");
                            Assert.AreEqual(1, output.Count);
                            Assert.AreEqual(value, output.GetValue("prop1", 0));
                        }
                        // set a user/host-specific property (overrides user-specific)
                        {
                            int value = 3;
                            NamedValueSet input = new NamedValueSet();
                            input.Set("prop1", value);
                            manager.SaveAppSettings(input, "UnitTest", client1.ClientInfo.UserName, client1.ClientInfo.HostName, false);
                            NamedValueSet output = client1.LoadAppSettings("UnitTest");
                            Assert.AreEqual(1, output.Count);
                            Assert.AreEqual(value, output.GetValue("prop1", 0));
                        }
                    }
                    server.Stop();
                }
            }
        }

        [TestMethod]
        public void TestObjectsWithSimpleTypes()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                // tests that object with simple types (including null) can be saved and loaded
                NamedValueSet props = new NamedValueSet();
                props.Set("Category", 10);
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // initialisation - delete all old data
                        client.DeleteObjects<TestData>(Expr.ALL);
                        // create the "null" (properties-only) object
                        Guid obj0Id = client.SaveObject<TestData>(null, "Test", props, TimeSpan.MaxValue);
                        // retrieve all (untyped)
                        List<ICoreItem> objset0 = client.LoadItems(Expr.ALL);
                        Assert.IsNotNull(objset0);
                        Assert.AreEqual(1, objset0.Count);
                        ICoreItem item1 = objset0[0];
                        Assert.IsNotNull(item1);
                        Assert.AreEqual(obj0Id, item1.Id);
                        Assert.IsTrue(item1.Frozen);
                        Assert.IsNull(item1.Text);
                        NamedValueSet props1 = item1.AppProps;
                        Assert.IsNotNull(props1);
                        Assert.AreEqual(1, props1.Count);
                        Assert.AreEqual(10, props1.GetValue("Category", 0));
                        Assert.IsNull(item1.Data);
                        // retrieve using query
                        IList<ICoreItem> objset2 = client.LoadItems(Expr.BoolAND(props));
                        Assert.IsNotNull(objset2);
                        Assert.AreEqual(1, objset2.Count);
                        ICoreItem item2 = objset2[0];
                        Assert.IsNotNull(item2);
                        Assert.IsNull(item2.Data);
                        Assert.AreEqual(obj0Id, item2.Id);
                        // retrieve using name
                        ICoreItem item3 = client.LoadItem("Test");
                        Assert.IsNotNull(item3);
                        Assert.IsNull(item3.Data);
                        Assert.AreEqual(obj0Id, item3.Id);
                        // retrieve using id
                        ICoreItem item4 = client.LoadItem(obj0Id);
                        Assert.IsNotNull(item4);
                        Assert.IsNull(item4.Data);
                        Assert.AreEqual(obj0Id, item4.Id);
                        // simple types eg. string, int, guid, decimal
                        // - standard .net types will auto-deserialise
                        client.SaveObject("5", "String5", null, TimeSpan.MaxValue);
                        ICoreItem item5A = client.LoadItem("String5");
                        Assert.IsNotNull(item5A.Data);
                        Assert.AreEqual(typeof(string), item5A.Data.GetType());
                        Assert.AreEqual("5", (string)item5A.Data);
                        ICoreItem item5B = client.LoadItem<string>("String5");
                        Assert.IsNotNull(item5B.Data);
                        Assert.AreEqual(typeof(string), item5B.Data.GetType());
                        client.SaveTypedObject(typeof(Int64), (Int64)6, "Int64_6", null);
                        ICoreItem item6A = client.LoadItem("Int64_6");
                        Assert.IsNotNull(item6A.Data);
                        Assert.AreEqual(typeof(Int64), item6A.Data.GetType());
                        Assert.AreEqual((Int64)6, (Int64)item6A.Data);
                        ICoreItem item6B = client.LoadItem(typeof(Int64), "Int64_6");
                        Assert.IsNotNull(item6B.Data);
                        Assert.AreEqual(typeof(Int64), item6B.Data.GetType());
                        client.SaveTypedObject(typeof(Decimal), 7M, "Decimal_7", null);
                        ICoreItem item7A = client.LoadItem("Decimal_7");
                        Assert.IsNotNull(item7A.Data);
                        Assert.AreEqual(typeof(Decimal), item7A.Data.GetType());
                        Assert.AreEqual((Decimal)7, (Decimal)item7A.Data);
                        ICoreItem item7B = client.LoadItem(typeof(Decimal), "Decimal_7");
                        Assert.IsNotNull(item7B.Data);
                        Assert.AreEqual(typeof(Decimal), item7B.Data.GetType());
                        Guid guid8 = Guid.NewGuid();
                        client.SaveTypedObject(typeof(Guid), guid8, "Guid_8", null);
                        ICoreItem item8A = client.LoadItem("Guid_8");
                        Assert.IsNotNull(item8A.Data);
                        Assert.AreEqual(typeof(Guid), item8A.Data.GetType());
                        Assert.AreEqual(guid8, (Guid)item8A.Data);
                        ICoreItem item8B = client.LoadItem(typeof(Guid), "Guid_8");
                        Assert.IsNotNull(item8B.Data);
                        Assert.AreEqual(typeof(Guid), item8B.Data.GetType());
                        // - custom types will not auto-deserialise (unless the type is supplied)
                        client.SaveObject(new TestData("9", 9), "TestData9", null, TimeSpan.MaxValue);
                        ICoreItem item9A = client.LoadItem("TestData9");
                        TestData data9A = null;
                        UnitTestHelper.AssertThrows<ApplicationException>("Cannot deserialise!",
                            () => data9A = (TestData)item9A.Data);
                        Assert.IsNull(data9A);
                        ICoreItem item9B = client.LoadItem<TestData>("TestData9");
                        Assert.IsNotNull(item9B.Data);
                        Assert.AreEqual(typeof(TestData), item9B.Data.GetType());
                        TestData data9B = (TestData)item9B.Data;
                        Assert.IsNotNull(data9B);
                    }
                    // save and load a string
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        string data0 = "MyString";
                        client.SaveObject<string>(data0, "Test", null, TimeSpan.MaxValue);

                        // retrieve using name
                        ICoreItem item = client.LoadItem(typeof(string), "Test");
                        Assert.IsNotNull(item);
                        Assert.IsNotNull(item.Data);
                        Assert.AreEqual<Type>(typeof(string), item.Data.GetType());
                        string text1 = item.Text;
                        string data1 = (string)(item.Data);
                        Assert.AreEqual(typeof(string), item.Data.GetType());
                        Assert.AreEqual(data0, data1);
                    }
                    // save and load an int
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        int data0 = 123456;
                        client.SaveTypedObject(typeof(int), data0, "Test", null);
                        // retrieve using name
                        ICoreItem item = client.LoadItem(typeof(int), "Test");
                        Assert.IsNotNull(item);
                        Assert.IsNotNull(item.Data);
                        Assert.AreEqual(typeof(int), item.Data.GetType());
                        string text1 = item.Text;
                        int data1 = (int)(item.Data);
                        Assert.AreEqual(typeof(int), item.Data.GetType());
                        Assert.AreEqual(data0, data1);
                    }
                    // shutdown
                    server.Stop();
                }
            }
        }

        [TestMethod]
        public void TestObjectsWithComplexTypes()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                // 1) tests that a published Object can be retrieved
                TestData data0 = new TestData
                {
                    field1 = "test",
                    field2 = 2
                };
                NamedValueSet props = new NamedValueSet(new NamedValue("Category", 10));
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // initialisation - delete all old data
                        client.DeleteObjects<TestData>(Expr.ALL);
                        // check db is empty
                        List<ICoreItem> objset0 = client.LoadItems(Expr.ALL);
                        Assert.IsNotNull(objset0);
                        Assert.AreEqual<int>(0, objset0.Count);
                        // test builtin deserialisation of item where type is known
                        ICoreItem item1A = client.MakeObject<TestData>(null, "Test", props);
                        item1A.SetData(data0);
                        Assert.IsNotNull(item1A.Data);
                        Assert.AreEqual(typeof(TestData), item1A.Data.GetType());
                        string text0 = XmlSerializerHelper.SerializeToString(data0);
                        item1A.SetText(text0, typeof(TestData));
                        Assert.IsNotNull(item1A.Data);
                        Assert.AreEqual(typeof(TestData), item1A.Data.GetType());
                        // test builtin deserialisation fails until type is supplied
                        item1A.SetText(text0, typeof(TestData).FullName);
                        object data1a = null;
                        UnitTestHelper.AssertThrows<ApplicationException>("Cannot deserialise!",
                            () => data1a = item1A.Data);
                        // deserialisation succeeds when type is supplied
                        item1A.SetText(text0, typeof(TestData));
                        TestData data1B = (TestData)item1A.Data;
                        Assert.IsNotNull(data1B);
                        Assert.AreEqual(data0.field1, data1B.field1);
                        Assert.AreEqual(data0.field2, data1B.field2);
                        item1A.Lifetime = TimeSpan.MaxValue;
                        Guid obj1AId = client.SaveItem(item1A);
                        // update the object
                        Guid obj1BId = client.SaveObject(new TestData(data1B.field1, data1B.field2 + 1), item1A.Name, item1A.AppProps, TimeSpan.MaxValue);
                        // retrieve all (untyped)
                        List<ICoreItem> objset1 = client.LoadItems(Expr.ALL);
                        Assert.IsNotNull(objset1);
                        Assert.AreEqual(1, objset1.Count);
                        // retrieve using typed query
                        IList<ICoreItem> objset2 = client.LoadItems<TestData>(Expr.BoolAND(props));
                        Assert.IsNotNull(objset2);
                        Assert.AreEqual(1, objset2.Count);
                        ICoreItem item2 = objset2[0];
                        Assert.IsNotNull(item2);
                        Assert.IsNotNull(item2.Data);
                        Assert.AreEqual(typeof(TestData), item2.Data.GetType());
                        Assert.AreEqual(obj1BId, item2.Id);
                        TestData data2 = (TestData)item2.Data;
                        Assert.AreEqual(data0.field1, data2.field1);
                        Assert.AreEqual(data0.field2 + 1, data2.field2);
                        // retrieve using name
                        ICoreItem item3 = client.LoadItem<TestData>("Test");
                        Assert.IsNotNull(item3);
                        Assert.IsNotNull(item3.Data);
                        Assert.AreEqual(typeof(TestData), item3.Data.GetType());
                        Assert.AreEqual(obj1BId, item3.Id);
                        TestData data3 = (TestData)item3.Data;
                        Assert.AreEqual(data0.field1, data3.field1);
                        Assert.AreEqual(data0.field2 + 1, data3.field2);
                        // retrieve using id
                        ICoreItem item4A = client.LoadItem<TestData>(obj1AId);
                        Assert.IsNotNull(item4A);
                        Assert.IsTrue(item4A.IsCurrent());
                        Assert.IsNotNull(item4A.Data);
                        ICoreItem item4B = client.LoadItem<TestData>(obj1BId);
                        Assert.IsNotNull(item4B);
                        Assert.IsTrue(item4B.IsCurrent());
                        Assert.IsNotNull(item4B.Data);
                        Assert.AreEqual(typeof(TestData), item4B.Data.GetType());
                        Assert.AreEqual(obj1BId, item4B.Id);
                        TestData data4 = (TestData)item4B.Data;
                        Assert.AreEqual(data0.field1, data4.field1);
                        Assert.AreEqual(data0.field2 + 1, data4.field2);
                        // delete the object
                        Guid obj1CId = client.DeleteItem(item4B);
                        // retrieve using id - returns the deleted item
                        ICoreItem item2C = client.LoadItem<TestData>(obj1CId);
                        Assert.IsNotNull(item2C);
                        Assert.IsFalse(item2C.IsCurrent());
                        Assert.IsNull(item2C.Data);
                        // retrieve using name - returns null
                        ICoreItem item3C = client.LoadItem<TestData>("Test");
                        Assert.IsNull(item3C);
                        //Assert.IsNull(item3c.Data);
                        // retrieve using query - returns empty set
                        IList<ICoreItem> objset4C = client.LoadItems<TestData>(Expr.ALL);
                        Assert.IsNotNull(objset4C);
                        Assert.AreEqual<int>(0, objset4C.Count);
                    }
                    // shutdown
                    server.Stop();
                }
            }
        }

        [TestMethod]
        public void TestObjectsWithoutTypes()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                NamedValueSet props = new NamedValueSet();
                props.Set("Category", 10);
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    using (ICoreClient core = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // initialisation - delete all old data
                        core.DeleteObjects<TestData>(Expr.ALL);
                        TestData data0 = new TestData("test", 2);
                        // create the untyped object
                        string text0 = XmlSerializerHelper.SerializeToString(data0);
                        ICoreItem item0 = core.MakeItemFromText(typeof(TestData).FullName, text0, "Test", props);
                        Assert.AreEqual<string>(text0, item0.Text);
                        Assert.AreEqual<string>(typeof(TestData).FullName, item0.DataTypeName);
                        item0.Lifetime = TimeSpan.FromMinutes(5);
                        Guid obj0Id = core.SaveItem(item0);
                        // retrieve all (untyped)
                        List<ICoreItem> objset0 = core.LoadItems(Expr.ALL);
                        Assert.IsNotNull(objset0);
                        Assert.AreEqual(1, objset0.Count);
                        ICoreItem item1 = objset0[0];
                        Assert.IsNotNull(item1);
                        Assert.IsTrue(item1.Frozen);
                        Assert.IsNotNull(item1.Text);
                        object data1A = null;
                        UnitTestHelper.AssertThrows<ApplicationException>("Cannot deserialise!",
                            () => data1A = item1.Data);
                        // succeeds when user supplies type
                        TestData data1B = item1.GetData<TestData>(false);
                        Assert.IsNotNull(data1B);
                        Assert.AreEqual(data0.field1, data1B.field1);
                        Assert.AreEqual(data0.field2, data1B.field2);
                        // retrieve using typed query
                        IList<ICoreItem> objset2 = core.LoadItems<TestData>(Expr.BoolAND(props));
                        Assert.IsNotNull(objset2);
                        Assert.AreEqual(1, objset2.Count);
                        ICoreItem item2 = objset2[0];
                        Assert.IsNotNull(item2);
                        Assert.IsNotNull(item2.Data);
                        Assert.AreEqual(typeof(TestData), item2.Data.GetType());
                        Assert.AreEqual(obj0Id, item2.Id);
                        TestData data2 = (TestData)item2.Data;
                        Assert.AreEqual(data0.field1, data2.field1);
                        Assert.AreEqual(data0.field2, data2.field2);
                        // retrieve using name
                        ICoreItem item3 = core.LoadItem<TestData>("Test");
                        Assert.IsNotNull(item3);
                        Assert.IsNotNull(item3.Data);
                        Assert.AreEqual(typeof(TestData), item3.Data.GetType());
                        Assert.AreEqual(obj0Id, item3.Id);
                        TestData data3 = (TestData)item3.Data;
                        Assert.AreEqual(data0.field1, data3.field1);
                        Assert.AreEqual(data0.field2, data3.field2);
                        // retrieve using id
                        ICoreItem item4 = core.LoadItem<TestData>(obj0Id);
                        Assert.IsNotNull(item4);
                        Assert.IsNotNull(item4.Data);
                        Assert.AreEqual(typeof(TestData), item4.Data.GetType());
                        Assert.AreEqual(obj0Id, item4.Id);
                        TestData data4 = (TestData)item4.Data;
                        Assert.AreEqual(data0.field1, data4.field1);
                        Assert.AreEqual(data0.field2, data4.field2);
                    }
                    // shutdown
                    server.Stop();
                }
            }
        }

        [TestMethod]
        public void TestIsolatedAppScopes()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {

                const string cTestAppScopeA = "HL.AU.Syd.AppA";
                const string cTestAppScopeB = "HL.AU.Syd.AppB";
                string env = "UTT";
                using (CoreServer server = new CoreServer(loggerRef, env, NodeType.Router))
                {
                    server.Start();
                    CoreClientFactory clientFactory = new CoreClientFactory(loggerRef).SetEnv(env);
                    using (ICoreClient recverAppA = clientFactory.Create())
                    using (ICoreClient senderAppA = clientFactory.Create())
                    using (ICoreClient recverAppB = clientFactory.Create())
                    using (ICoreClient senderAppB = clientFactory.Create())
                    {
                        // AppA 'sender' saves
                        TestData dataSentAppA = new TestData
                        {
                            field1 = "AppA",
                            field2 = 1
                        };
                        senderAppA.DefaultAppScopes = new[] { cTestAppScopeA };
                        ICoreItem evtSentAppA = senderAppA.MakeObject(dataSentAppA, "Test", null);
                        Guid evtIdAppA = senderAppA.SaveItem(evtSentAppA);
                        // AppB 'sender' saves
                        TestData dataSentAppB = new TestData {field1 = "AppB", field2 = 2};
                        senderAppB.DefaultAppScopes = new[] { cTestAppScopeB };
                        ICoreItem evtSentAppB = senderAppB.MakeObject<TestData>(dataSentAppB, "Test", null);
                        Guid evtIdAppB = senderAppB.SaveItem(evtSentAppB);
                        // AppA 'recver' loads
                        recverAppA.DefaultAppScopes = new[] { cTestAppScopeA };
                        List<ICoreItem> subsAppA = recverAppA.LoadItems<TestData>(Expr.ALL);
                        Assert.AreEqual(1, subsAppA.Count);
                        ICoreItem itemAppA = subsAppA[0];
                        Assert.IsNotNull(itemAppA);
                        Assert.IsNotNull(itemAppA.Data);
                        Assert.AreNotEqual(evtIdAppB, itemAppA.Id);
                        Assert.AreEqual(evtIdAppA, itemAppA.Id);
                        TestData dataRecdAppA = (TestData)itemAppA.Data;
                        Assert.AreEqual("AppA", dataRecdAppA.field1);
                        Assert.AreEqual(1, dataRecdAppA.field2);
                        Assert.AreEqual(cTestAppScopeA, itemAppA.AppScope);
                        // AppB 'recver' loads
                        recverAppB.DefaultAppScopes = new[] { cTestAppScopeB };
                        List<ICoreItem> subsAppB = recverAppB.LoadItems<TestData>(Expr.ALL);
                        Assert.AreEqual(1, subsAppB.Count);
                        ICoreItem itemAppB = subsAppB[0];
                        Assert.IsNotNull(itemAppB);
                        Assert.IsNotNull(itemAppB.Data);
                        Assert.AreNotEqual(evtIdAppA, itemAppB.Id);
                        Assert.AreEqual(evtIdAppB, itemAppB.Id);
                        TestData dataRecdAppB = (TestData)itemAppB.Data;
                        Assert.AreEqual("AppB", dataRecdAppB.field1);
                        Assert.AreEqual(2, dataRecdAppB.field2);
                        Assert.AreEqual(cTestAppScopeB, itemAppB.AppScope);
                    }
                    server.Stop();
                }
            }
        }

        [TestMethod]
        public void TestStoreAsynchronously()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                // 1) tests that a published Object can be retrieved
                TestData data0 = new TestData {field1 = "test", field2 = 2};
                NamedValueSet props = new NamedValueSet(new NamedValue("Category", 10));
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    using (ICoreClient client1 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    using (ICoreClient client2 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // initialisation - delete all old data
                        client1.DeleteObjects<TestData>(Expr.ALL);
                        // check db is empty
                        List<ICoreItem> objset0 = client1.LoadItems(Expr.ALL);
                        Assert.IsNotNull(objset0);
                        Assert.AreEqual(0, objset0.Count);
                        // create the object
                        Guid obj1AId = client1.SaveObject<TestData>(data0, "Test", props, TimeSpan.MaxValue);
                        // retrieve asynchronously (typed)
                        IAsyncResult ar1 = client2.LoadItemBegin<TestData>(null, obj1AId);
                        // wait for completion
                        ICoreItem item1B = client2.LoadItemEnd(ar1);
                        Assert.IsNotNull(item1B);
                        Assert.AreEqual(obj1AId, item1B.Id);
                        Assert.IsNotNull(item1B.Data);
                        Assert.AreEqual(typeof(TestData), item1B.Data.GetType());
                        TestData data1B = (TestData)item1B.Data;
                        Assert.AreEqual(data0.field1, data1B.field1);
                        Assert.AreEqual(data0.field2, data1B.field2);
                        // update the object
                        client1.SaveObject(new TestData(data0.field1, data0.field2 + 1), item1B.Name, item1B.AppProps, TimeSpan.MaxValue);
                        // retrieve all (untyped)
                        IAsyncResult ar2 = client2.LoadObjectsBegin(null, null, null);
                        // - wait for completion
                        List<ICoreItem> objset1 = client2.LoadItemsEnd(ar2);
                        Assert.IsNotNull(objset1);
                        Assert.AreEqual(1, objset1.Count);
                    }
                    // shutdown
                    server.Stop();
                }
            }
        }

        [TestMethod]
        public void TestConnectionProtocols()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                NamedValueSet serverSettings = new NamedValueSet();
                serverSettings.Set(CfgPropName.NodeType, (int)NodeType.Router);
                serverSettings.Set(CfgPropName.EnvName, "UTT");
                string hostIpv4 = null;
                foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        hostIpv4 = ipAddress.ToString();
                }
                Assert.IsNotNull(hostIpv4);
                using (CoreServer server = new CoreServer(loggerRef, serverSettings))
                {
                    server.Start();
                    List<string> serverAddresses = server.GetServerAddresses(null);
                    Assert.AreEqual(3, serverAddresses.Count);
                    Assert.AreEqual(WcfConst.NetMsmq + "://" + hostIpv4 + "/private/HL_Core_9113_ITransferV341", serverAddresses[0]);
                    Assert.AreEqual(WcfConst.NetTcp + "://" + hostIpv4 + ":9113/HL_Core_ITransferV341", serverAddresses[1]);
                    Assert.AreEqual(WcfConst.NetPipe + "://" + hostIpv4 + "/HL_Core_9113_ITransferV341", serverAddresses[2]);
                    // check that a client can connect using different protocols
                    string[] protocols = { WcfConst.NetTcp, WcfConst.NetMsmq, WcfConst.NetPipe };
                    Guid[] itemIds = new Guid[protocols.Length];
                    int p = 0;
                    foreach (string protocol in protocols)
                    {
                        using (ICoreClient client = new CoreClientFactory(loggerRef)
                            .SetEnv("UTT")
                            .SetServers("localhost:9113")
                            .SetProtocols(protocol)
                            .Create())
                        {
                            string itemName = $"Test.{p}";
                            client.RequestTimeout = TimeSpan.FromSeconds(15);
                            itemIds[p] = client.SaveObject(
                                new TestData(itemName, p),
                                itemName,
                                new NamedValueSet(new NamedValue("Protocol", protocol)),
                                TimeSpan.MaxValue);
                            ICoreItem item = client.LoadItem<TestData>(itemName);
                            Assert.IsNotNull(item);
                            Assert.IsNotNull(item.Data);
                            Assert.AreEqual(typeof(TestData), item.Data.GetType());
                            TestData data = (TestData)item.Data;
                            Assert.AreEqual(itemName, data.field1);
                            Assert.AreEqual(p, data.field2);
                            Assert.AreEqual(itemIds[p], item.Id);
                        }
                        // next protocol
                        p++;
                    }
                }
            }
        }

        [TestMethod]
        public void TestNetConfiguration()
        {
            // test network config redirection to a 2nd server
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                NamedValueSet settings2 = new NamedValueSet();
                settings2.Set(CfgPropName.NodeType, (int)NodeType.Router);
                settings2.Set(CfgPropName.Port, 9002);
                settings2.Set(CfgPropName.EnvName, "UTT");
                using (CoreServer server1 = new CoreServer(loggerRef, "UTT", NodeType.Router))
                using (CoreServer server2 = new CoreServer(loggerRef, settings2))
                {
                    server1.Start();
                    server2.Start();
                    using (ICoreClient client1 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // do nothing
                    }
                    using (ICoreClient client2 = new CoreClientFactory(loggerRef).SetEnv("UTT").SetServers("localhost:9002").Create())
                    {
                        // do nothing
                    }

                    // add redirection data
                    using (ICoreClient manager = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // do nothing
                    }

                    // test redirection
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // do nothing
                    }
                }
            }
        }

        [TestMethod]
        public void TestObjectExpiry()
        {
            // tests that a published object can be retrieved before it expires
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                TimeSpan itemLifetime = TimeSpan.FromSeconds(3);
                TestData data0 = new TestData {field1 = "test", field2 = 2};
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();

                    using (ICoreClient client1 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        NamedValueSet nvs1 = new NamedValueSet();
                        nvs1.Set("NameOfItem", "0123456789abcdef");
                        nvs1.Set("ObjectId", Guid.NewGuid());
                        nvs1.Set("ItemCount", 10);
                        // publish a null (deleted) object
                        Guid objId0 = client1.SaveObject<TestData>(null, "Null", null, TimeSpan.Zero);
                        // retrieve object
                        ICoreItem item0RecdA = client1.LoadItem(objId0);
                        Assert.IsNotNull(item0RecdA);
                        Assert.IsFalse(item0RecdA.IsCurrent());
                        ICoreItem item0RecdB = client1.LoadItem("null");
                        Assert.IsNull(item0RecdB);
                        // publish a data object with a short lifetime
                        Guid objId1 = client1.SaveObject(data0, "Test", nvs1, itemLifetime);
                        // retrieve all objects (untyped)
                        List<ICoreItem> cache1 = client1.LoadItems(Expr.ALL);
                        Assert.IsNotNull(cache1);
                        Assert.AreEqual(1, cache1.Count);
                        // retrieve the object (typed)
                        List<ICoreItem> cache2 = client1.LoadItems<TestData>(Expr.BoolAND(nvs1));
                        Assert.IsNotNull(cache2);
                        Assert.AreEqual(1, cache2.Count);
                        ICoreItem obj2 = cache2[0];
                        Assert.IsNotNull(obj2);
                        Assert.IsNotNull(obj2.Data);
                        Assert.AreEqual(obj2.Data.GetType(), typeof(TestData));
                        Assert.AreEqual(obj2.Id, objId1);
                        TestData data2 = (TestData)obj2.Data;
                        Assert.AreEqual(data2.field1, data0.field1);
                        Assert.AreEqual(data2.field2, data0.field2);
                        Assert.IsTrue(obj2.IsCurrent());
                        // wait until object has expired
                        Thread.Sleep(itemLifetime + TimeSpan.FromSeconds(0.5));
                        // check object has expired
                        Assert.IsFalse(obj2.IsCurrent());
                        // try to retrieve it
                        List<ICoreItem> cache3 = client1.LoadItems<TestData>(Expr.BoolAND(nvs1));
                        Assert.IsNotNull(cache3);
                        Assert.AreEqual(0, cache3.Count);
                        // retrieve expired object - fails
                        ICoreItem item4 = client1.LoadItem<TestData>("Test");
                        Assert.IsNull(item4);
                        // retrieve expired object (untyped) - fails
                        ICoreItem item5 = client1.LoadItem("test");
                        Assert.IsNull(item5);
                        // retrieve expired object with id - passes
                        ICoreItem item6 = client1.LoadItem(objId1);
                        Assert.IsNotNull(item6);
                        Assert.IsFalse(item6.IsCurrent());
                    }
                }
            }
        }

        [TestMethod]
        public void TestDisposeUnstartedSubscription()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef)
                        .SetEnv("UTT")
                        .Create())
                    {
                        using (ISubscription subs = client.CreateSubscription<TestData>(Expr.ALL))
                        {
                            // don't start subscription
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestCoreCacheSubscriptions()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                TestData data0 = new TestData {field1 = "test", field2 = 2};
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    //TimeSpan refreshPeriod = TimeSpan.FromSeconds(3);
                    // tests that a newly published event is notified to an existing subscriber
                    using (ICoreClient client = new CoreClientFactory(loggerRef)
                        .SetEnv("UTT")
                        .Create())
                    {
                        client.DebugRequests = false;
                        NamedValueSet itemProps = new NamedValueSet();
                        itemProps.Set("NameOfItem", "3456789abcdef012");
                        itemProps.Set("EventId", Guid.NewGuid());
                        itemProps.Set("ItemCount", 10);
                        // setup the subscribers
                        int allClearedCount = 0;
                        int allCreatedCount = 0;
                        int allUpdatedCount = 0;
                        int allDeletedCount = 0;
                        using (ICoreCache allItems = client.CreateCache(delegate(CacheChangeData update)
                        {
                            loggerRef.Target.LogDebug("AllItems: {0}", update.Change);
                            switch (update.Change)
                            {
                                case CacheChange.CacheCleared:
                                    allClearedCount++;
                                    break;
                                case CacheChange.ItemCreated:
                                    allCreatedCount++;
                                    break;
                                case CacheChange.ItemExpired:
                                case CacheChange.ItemRemoved:
                                    allDeletedCount++;
                                    break;
                                case CacheChange.ItemUpdated:
                                    allUpdatedCount++;
                                    break;
                            }
                        }, null))
                        {
                            allItems.SubscribeNoWait<TestData>(Expr.BoolAND(itemProps), null, null);
                            Assert.AreEqual(0, allItems.ItemCount);
                            Assert.AreEqual(0, allItems.UpdateCount);
                            // check clear notification
                            allItems.Clear();
                            // wait a bit for propagation
                            Thread.Sleep(500);
                            Assert.AreEqual(0, allItems.ItemCount);
                            Assert.AreEqual(0, allItems.UpdateCount);
                            Assert.AreEqual(1, allClearedCount);
                            // create an object
                            Guid itemId1 = client.SaveObject(data0, "Test", itemProps, TimeSpan.MaxValue);
                            // wait a bit for propagation
                            Thread.Sleep(500);
                            Assert.AreEqual(1, allItems.ItemCount);
                            Assert.AreEqual(1, allItems.CreateCount);
                            Assert.AreEqual(0, allItems.UpdateCount);
                            Assert.AreEqual(0, allItems.DeleteCount);
                            Assert.AreEqual(1, allClearedCount);
                            Assert.AreEqual(1, allCreatedCount);
                            Assert.AreEqual(0, allUpdatedCount);
                            Assert.AreEqual(0, allDeletedCount);
                            ICoreItem item1B = allItems.Items[0];
                            Assert.AreEqual(itemId1, item1B.Id);
                            // start 2nd subscription for new items only
                            int newClearedCount = 0;
                            int newCreatedCount = 0;
                            int newUpdatedCount = 0;
                            int newDeletedCount = 0;
                            using (ICoreCache newItems = client.CreateCache(delegate(CacheChangeData update)
                            {
                                loggerRef.Target.LogDebug("NewItems: {0}", update.Change);
                                switch (update.Change)
                                {
                                    case CacheChange.CacheCleared:
                                        newClearedCount++;
                                        break;
                                    case CacheChange.ItemCreated:
                                        newCreatedCount++;
                                        break;
                                    case CacheChange.ItemExpired:
                                    case CacheChange.ItemRemoved:
                                        newDeletedCount++;
                                        break;
                                    case CacheChange.ItemUpdated:
                                        newUpdatedCount++;
                                        break;
                                }
                            }, null))
                            {
                                newItems.SubscribeNewOnly<TestData>(Expr.BoolAND(itemProps), null, null);
                                Assert.AreEqual(0, newItems.ItemCount);
                                Assert.AreEqual(0, newItems.UpdateCount);
                                // check clear notification
                                newItems.Clear();
                                // wait a bit for propagation
                                Thread.Sleep(1000);
                                Assert.AreEqual(0, newItems.ItemCount);
                                Assert.AreEqual(0, newItems.CreateCount);
                                Assert.AreEqual(0, newItems.UpdateCount);
                                Assert.AreEqual(0, newItems.DeleteCount);
                                Assert.AreEqual(1, newClearedCount);
                                // update the object
                                Guid itemId2 = client.SaveObject(data0, item1B.Name, item1B.AppProps, TimeSpan.MaxValue);
                                // wait a bit for propagation
                                Thread.Sleep(1000);
                                Assert.AreEqual(1, allItems.ItemCount);
                                Assert.AreEqual(1, allItems.CreateCount);
                                Assert.AreEqual(1, allItems.UpdateCount);
                                Assert.AreEqual(0, allItems.DeleteCount);
                                Assert.AreEqual(1, allClearedCount);
                                Assert.AreEqual(1, allCreatedCount);
                                Assert.AreEqual(1, allUpdatedCount);
                                Assert.AreEqual(0, allDeletedCount);
                                ICoreItem item2B = allItems.Items[0];
                                Assert.AreEqual(itemId2, item2B.Id);
                                Assert.AreEqual(1, newItems.ItemCount);
                                Assert.AreEqual(1, newItems.CreateCount);
                                Assert.AreEqual(0, newItems.UpdateCount);
                                Assert.AreEqual(0, newItems.DeleteCount);
                                Assert.AreEqual(1, newClearedCount);
                                Assert.AreEqual(1, newCreatedCount);
                                Assert.AreEqual(0, newUpdatedCount);
                                Assert.AreEqual(0, newDeletedCount);
                                ICoreItem item2C = newItems.Items[0];
                                Assert.AreEqual(itemId2, item2C.Id);
                                // "delete" the object
                                // note this is really an update with null content
                                client.DeleteItem(item2B);
                                // wait a bit for propagation
                                Thread.Sleep(1000);
                                Assert.AreEqual(1, allItems.ItemCount);
                                Assert.AreEqual(1, allItems.CreateCount);
                                Assert.AreEqual(2, allItems.UpdateCount);
                                Assert.AreEqual(0, allItems.DeleteCount);
                                Assert.AreEqual(1, allClearedCount);
                                Assert.AreEqual(1, allCreatedCount);
                                Assert.AreEqual(2, allUpdatedCount);
                                Assert.AreEqual(0, allDeletedCount);
                                Assert.AreEqual(1, newItems.ItemCount);
                                Assert.AreEqual(1, newItems.CreateCount);
                                Assert.AreEqual(1, newItems.UpdateCount);
                                Assert.AreEqual(0, newItems.DeleteCount);
                                Assert.AreEqual(1, newClearedCount);
                                Assert.AreEqual(1, newCreatedCount);
                                Assert.AreEqual(1, newUpdatedCount);
                                Assert.AreEqual(0, newDeletedCount);
                                // null refresh
                                // wait a bit for propagation
                                Thread.Sleep(500);
                                Assert.AreEqual(1, allItems.ItemCount);
                                Assert.AreEqual(1, allItems.CreateCount);
                                Assert.AreEqual(2, allItems.UpdateCount);
                                Assert.AreEqual(0, allItems.DeleteCount);
                                Assert.AreEqual(1, allClearedCount);
                                Assert.AreEqual(1, allCreatedCount);
                                Assert.AreEqual(2, allUpdatedCount);
                                Assert.AreEqual(0, allDeletedCount);
                                Assert.AreEqual(1, newItems.ItemCount);
                                Assert.AreEqual(1, newItems.CreateCount);
                                Assert.AreEqual(1, newItems.UpdateCount);
                                Assert.AreEqual(0, newItems.DeleteCount);
                                Assert.AreEqual(1, newClearedCount);
                                Assert.AreEqual(1, newCreatedCount);
                                Assert.AreEqual(1, newUpdatedCount);
                                Assert.AreEqual(0, newDeletedCount);
                            }
                        }
                    }
                }
            }
        }
    }
}
