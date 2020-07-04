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
using Highlander.Codes.V5r3;
using Highlander.Core.Common;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V1.Tests
{
    [TestClass]
    public class PrivateCoreTests
    {
        readonly ICoreClient _client;

        public PrivateCoreTests()
        {
            _client = new PrivateCore(new TraceLogger(false));
            Dictionary<string, object> properties = new Dictionary<string, object>();
            NamedValueSet namedValueSet = new NamedValueSet(properties);
            Market market0 = new Market { id = "Market0" };
            _client.SaveObject(market0, market0.id, namedValueSet, TimeSpan.MaxValue);
            Market market1 = new Market { id = "Market1" };
            _client.SaveObject(market1, market1.id, namedValueSet, TimeSpan.MaxValue);
            Market market2 = new Market { id = "Market2" };
            _client.SaveObject(market2, market2.id, namedValueSet, TimeSpan.MaxValue);
            Market market3 = new Market { id = "Market3" };
            _client.SaveObject(market3, market3.id, namedValueSet, TimeSpan.MaxValue);
        }

        [TestMethod]
        public void CountObjectsTest1()
        {
            int count = _client.CountObjects<Market>(null);
            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void CountObjectsTest2()
        {
            int count = _client.CountObjects(typeof(Market), null);
            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void LoadObjectsPagingTest()
        {
            const int startRow = 1;
            const int rowCount = 2;
            var objects = _client.LoadObjects<Market>(null, null, startRow, rowCount);
            Assert.AreEqual(2, objects.Count);
            Assert.AreEqual("Market1", objects[0].id);
            Assert.AreEqual("Market2", objects[1].id);
        }

        [TestMethod]
        public void LoadItemsTest()
        {
            const int startRow = 1;
            const int rowCount = 2;
            List<ICoreItem> items = _client.LoadItems(typeof(Market), null, null, startRow, rowCount);
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual("Market1", items[0].Name);
            Assert.AreEqual("Market2", items[1].Name);
        }

        [TestMethod]
        public void TestPrivateCore()
        {
            using Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            using ICoreClient client = new PrivateCore(loggerRef.Target);
            // save/load single object
            const string name = "Id1";
            TestData data0 = new TestData("MyData", 123);
            client.SaveObject(data0, name, null, TimeSpan.MaxValue);
            ICoreItem item1A = client.LoadItem<TestData>(name);
            TestData data1A = (TestData)item1A.Data;
            Assert.AreEqual("MyData", data1A.field1);
            Assert.AreEqual(123, data1A.field2);
            ICoreItem item1B = client.LoadItem<TestData>(name);
            TestData data1B = (TestData)item1B.Data;
            Assert.AreEqual("MyData", data1B.field1);
            Assert.AreEqual(123, data1B.field2);
            // load multiple
            List<ICoreItem> list = client.LoadItems(Expr.ALL);
            Assert.AreEqual(1, list.Count);
            ICoreItem item1C = list[0];
            TestData data1C = (TestData)item1C.Data;
            Assert.AreEqual("MyData", data1C.field1);
            Assert.AreEqual(123, data1C.field2);
            // update the object
            Guid obj1BId = client.SaveObject(new TestData(data0.field1, data0.field2 + 1), item1C.Name, item1C.AppProps, TimeSpan.MaxValue);
            ICoreItem item2 = client.LoadItem<TestData>(name);
            Assert.IsNotNull(item2);
            Assert.IsNotNull(item2.Data);
            Assert.AreEqual(typeof(TestData), item2.Data.GetType());
            Assert.AreEqual(obj1BId, item2.Id);
            TestData data3 = (TestData)item2.Data;
            Assert.AreEqual(data0.field1, data3.field1);
            Assert.AreEqual(data0.field2 + 1, data3.field2);
            // delete the object
            client.DeleteItem(item1B);
            ICoreItem item3C = client.LoadItem<TestData>(name);
            Assert.IsNull(item3C);
        }

        [TestMethod]
        public void TestUsingDerivedTypes()
        {
            // tests control of the serialisation type
            // - type b (derived from a) is saved as b, loaded as b;
            // - type b (derived from a) is saved as a, loaded as a (but is type b).
            // (in this example a = PricingStructure, b = YieldCurve)
            using Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            using ICoreClient client = new PrivateCore(loggerRef.Target);
            {
                YieldCurve dataA = new YieldCurve()
                {
                    currency = new Currency() { Value = "USD" },
                    algorithm = "FastCubicSpline"
                };
                // - save as derived type
                client.SaveObject(dataA, "TestA", null, TimeSpan.MaxValue);
                ICoreItem test1 = client.LoadItem<YieldCurve>("TestA");
                Assert.IsNotNull(test1);
                Assert.AreEqual(typeof(YieldCurve).FullName, test1.DataTypeName);
                Assert.AreEqual(typeof(YieldCurve), test1.DataType);
                Assert.IsNotNull(test1.Data);
                Assert.AreEqual(typeof(YieldCurve), test1.Data.GetType());
                // - save as base type
                client.SaveObject<PricingStructure>(dataA, "TestA", null, TimeSpan.MaxValue);
                ICoreItem test2 = client.LoadItem<PricingStructure>("TestA");
                Assert.IsNotNull(test2);
                Assert.AreEqual(typeof(PricingStructure).FullName, test2.DataTypeName);
                Assert.AreEqual(typeof(PricingStructure), test2.DataType);
                Assert.IsNotNull(test2.Data);
                Assert.AreEqual(typeof(YieldCurve), test2.Data.GetType());
            }
            {
                FxCurve dataB = new FxCurve()
                {
                    quotedCurrencyPair = new QuotedCurrencyPair()
                    {
                        currency1 = new Currency { Value = "USD" },
                        currency2 = new Currency { Value = "JPY" },
                        quoteBasis = QuoteBasisEnum.Currency2PerCurrency1
                    }
                };
                // - save as derived type
                client.SaveObject(dataB, "TestB", null, TimeSpan.MaxValue);
                ICoreItem test1 = client.LoadItem<FxCurve>("TestB");
                Assert.IsNotNull(test1);
                Assert.AreEqual(typeof(FxCurve).FullName, test1.DataTypeName);
                Assert.AreEqual(typeof(FxCurve), test1.DataType);
                Assert.IsNotNull(test1.Data);
                Assert.AreEqual(typeof(FxCurve), test1.Data.GetType());

                // - save as base type
                client.SaveObject<PricingStructure>(dataB, "TestB", null, TimeSpan.MaxValue);
                ICoreItem test2 = client.LoadItem<PricingStructure>("TestB");
                Assert.IsNotNull(test2);
                Assert.AreEqual(typeof(PricingStructure).FullName, test2.DataTypeName);
                Assert.AreEqual(typeof(PricingStructure), test2.DataType);
                Assert.IsNotNull(test2.Data);
                Assert.AreEqual(typeof(FxCurve), test2.Data.GetType());
            }
            {
                // load a collection of the base type and verify specific types
                List<ICoreItem> items = client.LoadItems<PricingStructure>(Expr.ALL);
                Assert.AreEqual(2, items.Count);
                Dictionary<string, PricingStructure> index = new Dictionary<string, PricingStructure>();
                foreach (ICoreItem item in items)
                    index[item.Name] = (PricingStructure)item.Data;
                Assert.AreEqual(typeof(YieldCurve), index["TestA"].GetType());
                Assert.AreEqual(typeof(FxCurve), index["TestB"].GetType());
            }
        }

        [TestMethod]
        public void TestUsingInterfaceTypes()
        {
            // tests control of the serialisation type
            // - interface types
            using Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true));
            using ICoreClient client = new PrivateCore(loggerRef.Target);
            AssetMeasureValue codeValueInstance = new AssetMeasureValue()
            {
                Code = "Test",
                Description = "This is a test",
                Source = "UnitTest"
            };
            IFpMLCodeValue codeValueInterface = codeValueInstance;
            // save reference item
            client.SaveObject(codeValueInstance, "Test0", null, TimeSpan.MaxValue);
            ICoreItem itemA = client.LoadItem("Test0");
            Assert.AreEqual(typeof(AssetMeasureValue).FullName, itemA.DataTypeName);
            // test interface and instance both serialise identically
            {
                client.SaveUntypedObject(codeValueInterface, "Test1", null);
                ICoreItem itemB = client.LoadItem("Test1");
                Assert.AreEqual(typeof(AssetMeasureValue).FullName, itemB.DataTypeName);
                Assert.AreEqual(itemA.Text, itemB.Text);
            }
            {
                client.SaveObject((AssetMeasureValue)codeValueInterface, "Test2", null, TimeSpan.MaxValue);
                ICoreItem itemB = client.LoadItem("Test2");
                Assert.AreEqual(typeof(AssetMeasureValue).FullName, itemB.DataTypeName);
                Assert.AreEqual(itemA.Text, itemB.Text);
            }
            {
                // this should fail because interfaces cant be serialised
                UnitTestHelper.AssertThrows<ArgumentException>("Cannot be an interface type!\r\nParameter name: dataType", () =>
                {
                    client.SaveObject<IFpMLCodeValue>(codeValueInterface, "Test3", null, TimeSpan.MaxValue);
                });
            }
            {
                // note: this silently binds to SaveObject<IFpMLCodeValue>(...) which should fail
                UnitTestHelper.AssertThrows<ArgumentException>("Cannot be an interface type!\r\nParameter name: dataType", () =>
                {
                    client.SaveObject(codeValueInterface, "Test4", null, TimeSpan.MaxValue);
                });
            }
        }
    }
}
