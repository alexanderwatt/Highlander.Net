using System;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;

namespace Core.V34.Tests
{
    [TestClass]
    public class PrivateCoreTests
    {
        readonly ICoreClient client;

        [TestMethod]
        public void TestPrivateCore()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (ICoreClient client = new PrivateCore(loggerRef.Target))
                {
                    // save/load single object
                    const string name = "Id1";
                    TestData data0 = new TestData("MyData", 123);
                    client.SaveObject(data0, name, null, TimeSpan.MaxValue);
                    ICoreItem item1a = client.LoadItem<TestData>(name);
                    TestData data1a = (TestData)item1a.Data;
                    Assert.AreEqual("MyData", data1a.field1);
                    Assert.AreEqual(123, data1a.field2);
                    ICoreItem item1b = client.LoadItem<TestData>(name);
                    TestData data1b = (TestData)item1b.Data;
                    Assert.AreEqual("MyData", data1b.field1);
                    Assert.AreEqual(123, data1b.field2);
                    // load multiple
                    List<ICoreItem> list = client.LoadItems(Expr.ALL);
                    Assert.AreEqual(1, list.Count);
                    ICoreItem item1c = list[0];
                    TestData data1c = (TestData)item1c.Data;
                    Assert.AreEqual("MyData", data1c.field1);
                    Assert.AreEqual(123, data1c.field2);
                    // update the object
                    Guid obj1bId = client.SaveObject<TestData>(new TestData(data0.field1, data0.field2 + 1), item1c.Name, item1c.AppProps, TimeSpan.MaxValue);
                    ICoreItem item2 = client.LoadItem<TestData>(name);
                    Assert.IsNotNull(item2);
                    Assert.IsNotNull(item2.Data);
                    Assert.AreEqual(typeof(TestData), item2.Data.GetType());
                    Assert.AreEqual(obj1bId, item2.Id);
                    TestData data3 = (TestData)item2.Data;
                    Assert.AreEqual(data0.field1, data3.field1);
                    Assert.AreEqual(data0.field2 + 1, data3.field2);
                    // delete the object
                    client.DeleteItem(item1b);
                    ICoreItem item3c = client.LoadItem<TestData>(name);
                    Assert.IsNull(item3c);
                }
            }
        }
    }
}
