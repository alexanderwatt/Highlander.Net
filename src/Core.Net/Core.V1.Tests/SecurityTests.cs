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
using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.GrpcService.Data;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V1.Tests
{
    /// <summary>
    /// Summary description for SecurityTests
    /// </summary>
    [TestClass]
    public class SecurityTests
    {
        private static HighlanderContext _dbContext;

        public SecurityTests()
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
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _dbContext = new HighlanderContext(null);
        }
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestBasicEncryption()
        {
            // tests key creation and exchange
            // - between 2 clients
            using Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            using CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router, _dbContext);
            server.Start();

            using (ICoreClient sender = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
            using (ICoreClient recver = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
            {
                // check un-encrypted traffic
                Guid evtId1 = sender.SaveObject<TestData>(new TestData("1", 1), "test", null, TimeSpan.MaxValue);

                List<ICoreItem> recdList1 = recver.LoadItems<TestData>(Expr.ALL);
                Assert.AreEqual<int>(1, recdList1.Count);
                ICoreItem recdItem1 = recdList1[0];
                Assert.AreEqual<Guid>(evtId1, recdItem1.Id);
                Assert.IsNull(recdItem1.TranspKeyId);
                Assert.IsNull(recdItem1.SenderKeyId);
                Assert.IsNull(recdItem1.RecverKeyId);

                // generate keys
                string senderKeyId = sender.CryptoManager.GenerateNewKeys();
                string recverKeyId = recver.CryptoManager.GenerateNewKeys();

                // send encrypted message and check receiver fails to decrypt
                ICoreItem sentItem2 = sender.MakeObject<TestData>(new TestData("2", 2), "test", null);
                sentItem2.TranspKeyId = senderKeyId;
                sentItem2.SenderKeyId = senderKeyId;
                Guid evtId2 = sender.SaveItem(sentItem2);
                List<ICoreItem> recdList2 = recver.LoadItems<TestData>(Expr.ALL);
                Assert.AreEqual<int>(1, recdList2.Count);
                ICoreItem recdItem2 = recdList2[0];
                Assert.AreEqual<Guid>(evtId2, recdItem2.Id);
                Assert.AreEqual<Guid>(evtId2, recdItem2.Id);
                Assert.AreEqual<string>(senderKeyId, recdItem2.TranspKeyId);
                Assert.AreEqual<string>(senderKeyId, recdItem2.SenderKeyId);
                Assert.IsNull(recver.CryptoManager.GetTransportKey(senderKeyId));
                // set public key at receiver and check authentication
                recver.CryptoManager.SetPublicKey(senderKeyId, sender.CryptoManager.GetPublicKey(senderKeyId));
                Assert.IsTrue(recdItem2.IsSigned);
                Assert.IsFalse(recdItem2.IsSecret);
                // set transport key at receiver and recheck decryption
                recver.CryptoManager.SetTransportKey(senderKeyId, sender.CryptoManager.GetTransportKey(senderKeyId));
                object data2 = recdItem2.Data;
                Assert.IsNotNull(data2);
                Assert.AreEqual<Type>(typeof(TestData), data2.GetType());
            }

            server.Stop();
        }

        [TestMethod]
        public void TestSecretKeyExchange()
        {
            // tests key creation and exchange
            // - between 2 clients
            using Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            using CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router, _dbContext);
            server.Start();

            using (ICoreClient sender = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
            using (ICoreClient recver = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
            {
                // generate keys
                string senderKeyId = sender.CryptoManager.GenerateNewKeys();
                string recverKeyId = recver.CryptoManager.GenerateNewKeys();

                // hardcode the public key exchange
                sender.CryptoManager.SetPublicKey(recverKeyId, recver.CryptoManager.GetPublicKey(recverKeyId));
                recver.CryptoManager.SetPublicKey(senderKeyId, sender.CryptoManager.GetPublicKey(senderKeyId));

                sender.DefaultLifetime = TimeSpan.FromMinutes(5);

                // send secret message containing transport key
                ICoreItem item1 = sender.MakeObject<string>("", "key", null);
                TestData sendXKey = new TestData();
                sendXKey.field1 = senderKeyId;
                sendXKey.array1 = new string[1] { sender.CryptoManager.GetTransportKey(senderKeyId) };
                item1.SetData(sendXKey);
                //evt1.ItemName = "key";
                item1.SenderKeyId = senderKeyId;
                item1.RecverKeyId = recverKeyId;
                Guid evtIdXKey = sender.SaveItem(item1);
                // send encrypted message
                ICoreItem item2 = sender.MakeObject<string>("", "data", null);
                string text2 = XmlSerializerHelper.SerializeToString(new TestData("data", 1));
                item2.SetText(text2, typeof(TestData));
                item2.TranspKeyId = senderKeyId;
                item2.SenderKeyId = senderKeyId;
                Guid evtIdData = sender.SaveItem(item2);

                // check sender
                // note: 
                // - although sender published the key, sender cannot see it
                // - however, sender can see the data
                ICoreItem sentItemXKey = sender.LoadItem<TestData>("key");
                Assert.IsNotNull(sentItemXKey);
                Assert.AreEqual<Guid>(evtIdXKey, sentItemXKey.Id);
                Assert.AreEqual<string>("key", sentItemXKey.Name);
                Assert.IsTrue(sentItemXKey.IsSigned);
                Assert.IsTrue(sentItemXKey.IsSecret);
                //object sentTempXKey = sentItemXKey.Data; // this will fail

                ICoreItem sentItemData = sender.LoadItem<TestData>("data");
                Assert.IsNotNull(sentItemData);
                Assert.AreEqual<Guid>(evtIdData, sentItemData.Id);
                Assert.AreEqual<string>("data", sentItemData.Name);
                Assert.IsTrue(sentItemData.IsSigned);
                Assert.IsFalse(sentItemData.IsSecret);
                object sentTempData = sentItemData.Data;
                Assert.IsNotNull(sentTempData);
                Assert.AreEqual<Type>(typeof(TestData), sentTempData.GetType());
                TestData sentTestData = (TestData)sentTempData;
                Assert.AreEqual<string>("data", sentTestData.field1);
                Assert.AreEqual<int>(1, sentTestData.field2);

                // check that secret transport key is received by recver
                ICoreItem recdItemXKey = recver.LoadItem<TestData>("key");
                Assert.IsNotNull(recdItemXKey);
                Assert.AreEqual<Guid>(evtIdXKey, recdItemXKey.Id);
                Assert.AreEqual<string>("key", recdItemXKey.Name);
                Assert.IsTrue(recdItemXKey.IsSigned);
                Assert.IsTrue(recdItemXKey.IsSecret);
                object recdTempXKey = recdItemXKey.Data;
                Assert.IsNotNull(recdTempXKey);
                Assert.AreEqual<Type>(typeof(TestData), recdTempXKey.GetType());
                TestData recdTestXKey = (TestData)recdTempXKey;
                Assert.AreEqual<string>(recdTestXKey.field1, senderKeyId);
                Assert.IsNotNull(recdTestXKey.array1);
                Assert.AreEqual<int>(1, recdTestXKey.array1.Length);
                Assert.AreEqual<string>(recdTestXKey.array1[0], sender.CryptoManager.GetTransportKey(senderKeyId));

                // set the transport key and receive the 2nd data message
                recver.CryptoManager.SetTransportKey(recdTestXKey.field1, recdTestXKey.array1[0]);
                ICoreItem recdItemData = recver.LoadItem<TestData>("data");
                Assert.IsNotNull(recdItemData);
                Assert.AreEqual<Guid>(evtIdData, recdItemData.Id);
                Assert.AreEqual<string>("data", recdItemData.Name);
                Assert.IsTrue(recdItemData.IsSigned);
                Assert.IsFalse(recdItemData.IsSecret);
                object recdTempData = recdItemData.Data;
                Assert.IsNotNull(recdTempData);
                Assert.AreEqual<Type>(typeof(TestData), recdTempData.GetType());
                TestData recdTestData = (TestData)recdTempData;
                Assert.AreEqual<string>("data", recdTestData.field1);
                Assert.AreEqual<int>(1, recdTestData.field2);
            }

            server.Stop();
        }
    }
}
