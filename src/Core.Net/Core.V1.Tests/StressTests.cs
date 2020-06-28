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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Compression;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V1.Tests
{
    /// <summary>
    /// Summary description for StressTests
    /// </summary>
    [TestClass]
    public class StressTests
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

        [TestMethod]
        public void TestPagingTechniques()
        {
            const bool debugRequests = true;
            // loads/saves a very large number of objects
            const int maxLoopCount = 5;
            const int itemsPerLoop = 400;
            const int itemsPerPage = 154;
            const int totalItems = maxLoopCount * itemsPerLoop;
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                NamedValueSet serverSettings = new NamedValueSet();
                serverSettings.Set(CfgPropName.NodeType, (int)NodeType.Router);
                serverSettings.Set(CfgPropName.EnvName, "UTT");
                using (CoreServer server = new CoreServer(loggerRef, serverSettings))
                {
                    server.Start();
                    // save
                    using (ICoreClient client1 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        for (int loop = 0; loop < maxLoopCount; loop++)
                        {
                            ICoreItem[] items = new ICoreItem[itemsPerLoop];
                            for (int i = 0; i < itemsPerLoop; i++)
                            {
                                client1.DebugRequests = i == 0;
                                int n = loop * itemsPerLoop + i;
                                TestData data = new TestData(n.ToString(), n);
                                NamedValueSet props = new NamedValueSet(new NamedValue("n", n));
                                items[i] = client1.MakeObject(data, "Test." + n, props);
                            }
                            client1.SaveItems(items);
                        }
                    }
                    // load (using original paging technique)
                    loggerRef.Target.LogDebug("---------- multi page load");
                    // - slow because it scans and sort the entire data set for each page
                    using (ICoreClient client3 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // count the items
                        int itemCount1 = client3.CountObjects<TestData>(null);
                        // now get the items in pages
                        IExpression orderExpr = Expr.Prop("n");
                        client3.DebugRequests = debugRequests;
                        int itemCount2 = 0;
                        int startRow = 0;
                        List<ICoreItem> page;
                        do
                        {
                            page = client3.LoadItems<TestData>(null, orderExpr, startRow, itemsPerPage);
                            itemCount2 += page.Count;
                            startRow += page.Count;
                        }
                        while (page.Count == itemsPerPage);
                        Assert.AreEqual(totalItems, itemCount1);
                        Assert.AreEqual(totalItems, itemCount2);
                    }
                    // paged load (using pre-load of item header info)
                    loggerRef.Target.LogDebug("---------- header pre-load");
                    // - faster because it only scans sorts the entire data set once and sorting
                    //   can use local compiled methods instead of server-side expression evaluation.
                    using (ICoreClient client4 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // get all the items (without data)
                        List<ICoreItemInfo> itemInfoList = client4.LoadItemInfos<TestData>(null);
                        int itemCount1 = itemInfoList.Count;
                        // optionally sort the list here
                        //IExpression orderExpr = Expr.Prop("n");
                        // now get the items in pages
                        client4.DebugRequests = debugRequests;
                        int itemCount2 = 0;
                        int maxPages = ((itemCount1 / itemsPerPage) + 1);
                        for (int pageNum = 0; pageNum < maxPages; pageNum++)
                        {
                            // get the item names/ids for the current page
                            int startRow = itemsPerPage * pageNum;
                            List<string> itemNames = new List<string>();
                            for (int i = 0; i < itemsPerPage; i++)
                            {
                                int n = startRow + i;
                                if (n < itemCount1)
                                    itemNames.Add(itemInfoList[n].Name);
                            }
                            // now get the objects (with data)
                            var page = client4.LoadItems<TestData>(itemNames);
                            itemCount2 += page.Count;
                        }
                        Assert.AreEqual(totalItems, itemCount1);
                        Assert.AreEqual(totalItems, itemCount2);
                    }
                }
            }
        }

        [TestMethod]
        public void TestFailingExpressions()
        {
            // tests expressions that fail to evaluate on the server
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                NamedValueSet serverSettings = new NamedValueSet();
                serverSettings.Set(CfgPropName.NodeType, (int)NodeType.Router);
                serverSettings.Set(CfgPropName.EnvName, "UTT");
                using (CoreServer server = new CoreServer(loggerRef, serverSettings))
                {
                    server.Start();
                    // save
                    using (ICoreClient client1 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        client1.SaveObject(new TestData(), "AllValuesNotNull", new NamedValueSet("StrValue/String=A|NumValue/Int32=0"), TimeSpan.MaxValue);
                        client1.SaveObject(new TestData(), "NumValueIsNull", new NamedValueSet("StrValue/String=A"), TimeSpan.MaxValue);
                        client1.SaveObject(new TestData(), "StrValueIsNull", new NamedValueSet("NumValue/Int32=0"), TimeSpan.MaxValue);
                        client1.SaveObject(new TestData(), "AllValuesAreNull", null, TimeSpan.MaxValue);
                    }

                    // load using expression
                    using (ICoreClient client2 = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        client2.DebugRequests = true;
                        {
                            var items = client2.LoadObjects<TestData>(Expr.BoolAND(Expr.IsEQU("StrValue", "A"), Expr.IsEQU("NumValue", 0)));
                            Assert.AreEqual(1, items.Count);
                        }
                        {
                            var items = client2.LoadObjects<TestData>(Expr.BoolOR(Expr.IsEQU("StrValue", "A"), Expr.IsEQU("NumValue", 0)));
                            Assert.AreEqual(3, items.Count);
                        }
                        {
                            // unknown/missing string property
                            var items = client2.LoadObjects<TestData>(Expr.BoolOR(Expr.IsEQU("StrValue", "A"), Expr.IsEQU("XValue", "X")));
                            Assert.AreEqual(2, items.Count);
                        }
                        {
                            // unknown/missing non-string property
                            var items = client2.LoadObjects<TestData>(Expr.BoolOR(Expr.IsEQU("StrValue", "A"), Expr.IsEQU("YValue", 1)));
                            Assert.AreEqual(2, items.Count);
                        }
                        {
                            // property missing
                            var items = client2.LoadObjects<TestData>(Expr.IsNull("StrValue"));
                            Assert.AreEqual(2, items.Count);
                        }
                        {
                            // property has value
                            var items = client2.LoadObjects<TestData>(Expr.IsNotNull("StrValue"));
                            Assert.AreEqual(2, items.Count);
                        }
                        {
                            var items = client2.LoadObjects<TestData>(Expr.StartsWith("StrValue", "A"));
                            Assert.AreEqual(2, items.Count);
                        }
                        {
                            var items = client2.LoadObjects<TestData>(Expr.BoolOR(Expr.StartsWith("StrValue", "A"), Expr.IsEQU("NumValue", 0)));
                            Assert.AreEqual(3, items.Count);
                        }
                    }

                }
            }
        }

        [TestMethod]
        [Ignore]
        public void TestStressServerLoadAndRetrieve()
        {
            // return an increasing number of trades until failure
            Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
            NamedValueSet tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                NamedValueSet serverSettings = new NamedValueSet();
                serverSettings.Set(CfgPropName.NodeType, (int)NodeType.Router);
                serverSettings.Set(CfgPropName.EnvName, "UTT");
                using (CoreServer server = new CoreServer(loggerRef, serverSettings))
                {
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").SetServers("localhost:8113").Create())
                    //using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv(EnvId.DEV_Development).Create())
                    {
                        // delete the test trades
                        IExpression deleteExpr = Expr.StartsWith(Expr.SysPropItemName, "Test.");
                        client.DeleteObjects<Trade>(deleteExpr);
                        const int maxLoops = 1;
                        const int loopRepeat = 1;
                        const int incrementPerLoop = 1024;
                        int itemsPerLoop = 8192 - 1024;
                        int itemsSaved = 0;
                        for (int loop = 0; loop < maxLoops; loop++)
                        {
                            for (int repeat = 0; repeat < loopRepeat; repeat++)
                            {
                                // create trades
                                loggerRef.Target.LogDebug("[{0}-{1}] Making {2} trades...", loop, repeat, itemsPerLoop);
                                List<IAsyncResult> completions = new List<IAsyncResult>();
                                ICoreItem[] items = new ICoreItem[itemsPerLoop];
                                for (int i = 0; i < itemsPerLoop; i++)
                                {
                                    string tradeName = $"Test.{loop}.{i}";
                                    ICoreItem item = client.MakeObject(trade, tradeName, tradeProps);
                                    item.Freeze(); // serialises
                                    items[i] = item;
                                }
                                loggerRef.Target.LogDebug("[{0}-{1}] Commencing save of {2} trades...", loop, repeat, itemsPerLoop);
                                for (int i = 0; i < itemsPerLoop; i++)
                                {
                                    completions.Add(client.SaveItemBegin(items[i]));
                                    itemsSaved++;
                                }
                                loggerRef.Target.LogDebug("[{0}-{1}] Completing save of {2} trades...", loop, repeat, itemsPerLoop);
                                foreach (IAsyncResult ar in completions)
                                {
                                    client.SaveEnd(ar);
                                }
                                // load all that have been saved
                                string loopName = $"Test.{loop}.";
                                loggerRef.Target.LogDebug("[{0}-{1}] Loading {2} trades...", loop, repeat, itemsPerLoop);
                                List<ICoreItem> tradeItems = client.LoadItems<Trade>(Expr.StartsWith(Expr.SysPropItemName, loopName));
                                Assert.AreEqual(itemsPerLoop, tradeItems.Count);
                                loggerRef.Target.LogDebug("[{0}-{1}] Unpacking {2} trades...", loop, repeat, itemsPerLoop);
                                List<Trade> trades = new List<Trade>();
                                foreach (ICoreItem item in tradeItems)
                                {
                                    trades.Add((Trade)item.Data);
                                }
                                loggerRef.Target.LogDebug("[{0}-{1}] Retrieved {2} trades.", loop, repeat, itemsPerLoop);
                                // delete the test trades
                                client.DeleteObjects<Trade>(deleteExpr);
                                GC.Collect();
                            }
                            // next loop
                            itemsPerLoop += incrementPerLoop;
                        } // for loop
                    }
                }
            }
        }

        [TestMethod]
        [Ignore]
        public void TestStressServerLoadAndSubscribe()
        {
            // return an increasing number of trades until failure
            // on a 4GB workstation, the limit is about 13,000 trades.
            Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
            NamedValueSet tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                NamedValueSet serverSettings = new NamedValueSet();
                serverSettings.Set(CfgPropName.NodeType, (int)NodeType.Router);
                serverSettings.Set(CfgPropName.EnvName, "UTT");
                using (CoreServer server = new CoreServer(loggerRef, serverSettings))
                {
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").SetServers("localhost:8113").Create())
                    //using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv(EnvId.DEV_Development).Create())
                    {
                        // delete the test trades
                        IExpression deleteExpr = Expr.StartsWith(Expr.SysPropItemName, "Test.");
                        client.DeleteObjects<Trade>(deleteExpr);
                        const int maxLoops = 6;
                        const int loopRepeat = 1;
                        const int incrementPerLoop = 1024;
                        int itemsPerLoop = 8192;
                        int itemsSaved = 0;
                        for (int loop = 0; loop < maxLoops; loop++)
                        {
                            string loopName = $"Test.{loop}.";
                            for (int repeat = 0; repeat < loopRepeat; repeat++)
                            {
                                // create trades
                                loggerRef.Target.LogDebug("[{0}-{1}] Creating {2} trades...", loop, repeat, itemsPerLoop);
                                ICoreItem[] items = new ICoreItem[itemsPerLoop];
                                for (int i = 0; i < itemsPerLoop; i++)
                                {
                                    string tradeName = $"Test.{loop}.{i}";
                                    items[i] = client.MakeObject(trade, tradeName, tradeProps);
                                    itemsSaved++;
                                }
                                client.SaveItems(items);
                                // start subscription
                                loggerRef.Target.LogDebug("[{0}-{1}] Receiving {2} trades...", loop, repeat, itemsPerLoop);
                                long itemsReceived = 0;
                                DateTime testStartedTime = DateTime.Now;
                                DateTime lastReceiveTime = testStartedTime;
                                ISubscription subscription = client.CreateSubscription<Trade>(Expr.StartsWith(Expr.SysPropItemName, loopName));
                                subscription.UserCallback = delegate
                                {
                                    Interlocked.Increment(ref itemsReceived);
                                    lastReceiveTime = DateTime.Now;
                                };
                                //subscription.ExcludeDataBody = true;
                                subscription.Start();
                                // wait a bit to ensure subscription is up to date
                                while ((Interlocked.Add(ref itemsReceived, 0) < itemsPerLoop) && ((DateTime.Now - testStartedTime) < TimeSpan.FromSeconds(10)))
                                {
                                    Thread.Sleep(100);
                                }
                                Thread.Sleep(100);
                                subscription.Cancel();
                                // load all that have been saved
                                loggerRef.Target.LogDebug("[{0}-{1}] Received {2} trades in {3} seconds.", loop, repeat, itemsReceived, (lastReceiveTime - testStartedTime).TotalSeconds);
                                Assert.AreEqual(itemsPerLoop, Interlocked.Add(ref itemsReceived, 0));
                                // delete the test trades
                                client.DeleteObjects<Trade>(deleteExpr);
                                GC.Collect();
                            }
                            // next loop
                            itemsPerLoop += incrementPerLoop;
                        } // for loop
                    }
                }
            }
        }

        [TestMethod]
        [Ignore] // this is an integration test
        public void StressUttServerLoadAndSubscribe()
        {
            // return an increasing number of trades until failure
            // on a 4GB workstation, the limit is about 13,000 trades.
            Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
            NamedValueSet tradeProps = new NamedValueSet(
                ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                //NamedValueSet serverSettings = new NamedValueSet();
                //serverSettings.Set(CfgPropName.NodeType, (int)NodeType.Router);
                //serverSettings.Set(CfgPropName.EnvName, "UTT");
                //using (CoreServer server = new CoreServer(logger, serverSettings))
                {
                    //server.Start();
                    //using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").SetHosts("localhost:8113").Create())
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // delete the test trades
                        IExpression deleteExpr = Expr.ALL;
                        client.DeleteObjects<Trade>(deleteExpr);
                        const int maxLoops = 3;
                        const int loopRepeat = 3;
                        const int incrementPerLoop = 1024;
                        int itemsPerLoop = 1024;
                        int itemsSaved = 0;
                        for (int loop = 0; loop < maxLoops; loop++)
                        {
                            string loopName = $"Test.{loop}.";
                            for (int repeat = 0; repeat < loopRepeat; repeat++)
                            {
                                // create trades
                                loggerRef.Target.LogDebug("[{0}-{1}] Creating {2} trades...", loop, repeat, itemsPerLoop);
                                ICoreItem[] items = new ICoreItem[itemsPerLoop];
                                for (int i = 0; i < itemsPerLoop; i++)
                                {
                                    string tradeName = $"Test.{loop}.{i}";
                                    items[i] = client.MakeObject<Trade>(trade, tradeName, tradeProps);
                                    itemsSaved++;
                                }
                                client.SaveItems(items);
                                // start subscription
                                loggerRef.Target.LogDebug("[{0}-{1}] Receiving {2} trades...", loop, repeat, itemsPerLoop);
                                long itemsReceived = 0;
                                DateTime testStartedTime = DateTime.Now;
                                DateTime lastReceiveTime = testStartedTime;
                                ISubscription subscription = client.CreateSubscription<Trade>(Expr.StartsWith(Expr.SysPropItemName, loopName));
                                subscription.UserCallback = delegate
                                {
                                    Interlocked.Increment(ref itemsReceived);
                                    lastReceiveTime = DateTime.Now;
                                };
                                //subscription.ExcludeDataBody = true;
                                subscription.Start();
                                // wait a bit to ensure subscription is up to date
                                while ((Interlocked.Add(ref itemsReceived, 0) < itemsPerLoop) && ((DateTime.Now - testStartedTime) < TimeSpan.FromSeconds(10)))
                                {
                                    Thread.Sleep(100);
                                }
                                Thread.Sleep(100);
                                subscription.Cancel();
                                // load all that have been saved
                                loggerRef.Target.LogDebug("[{0}-{1}] Received {2} trades in {3} seconds.", loop, repeat, itemsReceived, (lastReceiveTime - testStartedTime).TotalSeconds);
                                Assert.AreEqual(itemsPerLoop, Interlocked.Add(ref itemsReceived, 0));
                                // delete the test trades
                                //client.DeleteObjects<Trade>(deleteExpr);
                                //GC.Collect();
                            }

                            // next loop
                            itemsPerLoop += incrementPerLoop;
                        } // for loop
                    }
                }
            }
        }

        [TestMethod]
        public void TestRequestThrottling()
        {
            const int numberOfAsyncRequests = 1000;
            const int serverPort = 9001;
            string hostAddress = $"localhost:{serverPort}";
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                NamedValueSet serverSettings = new NamedValueSet();
                serverSettings.Set(CfgPropName.EnvName, "UTT");
                serverSettings.Set(CfgPropName.Port, serverPort);
                serverSettings.Set(CfgPropName.Endpoints, WcfConst.NetTcp);
                using (CoreServer server = new CoreServer(loggerRef, serverSettings))
                {
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").SetServers(hostAddress).Create())
                    {
                        List<IAsyncResult> completions = new List<IAsyncResult>();
                        try
                        {
                            for (int i = 0; i < numberOfAsyncRequests; i++)
                            {
                                completions.Add(client.SaveObjectBegin(new TestData("Test", i), "Test", null, false, TimeSpan.MaxValue));
                                //loggerRef.Target.LogDebug("[{0}] save begun...", i.ToString("0000"));
                            }
                        }
                        finally
                        {
                            for (int i = 0; i < numberOfAsyncRequests; i++)
                            {
                                client.SaveEnd(completions[i]);
                                //loggerRef.Target.LogDebug("[{0}] save done.", i.ToString("0000"));
                            }
                        }
                    }
                }
            }
        }

        private static Guid CalculateBufferHash(byte[] buffer)
        {
            if (buffer == null)
                return Guid.Empty;
            HashAlgorithm hash = new MD5CryptoServiceProvider();
            byte[] hashBytes = hash.ComputeHash(buffer);
            return new Guid(hashBytes);
        }

        [TestMethod]
        public void TestBufferHashing()
        {
            // tests that identical objects are interned in the server
            // - save the same large (1M) object 1000 times (1G RAM virtual, 1M actual).
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                // create a large object
                Random random = new Random(Environment.TickCount);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 10000; i++)
                {
                    byte b = (byte)random.Next(0, 255);
                    char ch = Convert.ToChar(b);
                    sb.Append(ch);
                }
                string largeString = sb.ToString();
                TestData data = new TestData(largeString, 1);
                string text0 = XmlSerializerHelper.SerializeToString(data);
                byte[] buff0 = CompressionHelper.CompressToBuffer(text0);
                Guid hash0 = CalculateBufferHash(buff0);
                for (int n = 1; n < 9; n++)
                {
                    string text = XmlSerializerHelper.SerializeToString(data);
                    Assert.AreEqual(text0.GetHashCode(), text.GetHashCode());
                    Assert.AreEqual(text0, text);
                    byte[] buff = CompressionHelper.CompressToBuffer(text);
                    Assert.AreEqual(buff0.Length, buff.Length);
                    for (int b = 0; b < buff0.Length; b++)
                    {
                        if (buff0[b] != buff[b])
                        {
                            Assert.AreEqual(buff0[b], buff[b]);
                        }
                    }
                    Guid hash = CalculateBufferHash(buff);
                    Assert.AreEqual(hash0, hash);
                }
            }
        }
    }
}
