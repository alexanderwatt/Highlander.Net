using System;
using System.Collections.Generic;
using Core.Common;
using Core.Server;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.Util.Serialisation;

namespace Core.V34.Tests
{
    /// <summary>
    /// Summary description for SerialisationTests
    /// </summary>
    [TestClass]
    public class SerialisationTests
    {
        public SerialisationTests()
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
        public void TestSerialisingDerivedTypes()
        {
            // tests control of the serialisation type
            // - type b (derived from a) is saved as b, loaded as b;
            // - type b (derived from a) is saved as a, loaded as a (but is type b).
            // (in this example a = PricingStructure, b = YieldCurve)
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        {
                            YieldCurve dataA = new YieldCurve
                            {
                                currency = new Currency { Value = "USD" },
                                algorithm = "FastCubicSpline"
                            };
                            // - save as derived type
                            client.SaveObject(dataA, "TestA", null, TimeSpan.MaxValue);
                            ICoreItem test1 = client.LoadItem<YieldCurve>("TestA");
                            Assert.IsNotNull(test1);
                            Assert.IsNotNull(test1.Text);
                            Assert.AreEqual(
                                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<yieldCurve xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.fpml.org/FpML-5/reporting\">\r\n  <currency>USD</currency>\r\n  <algorithm>FastCubicSpline</algorithm>\r\n</yieldCurve>",
                                test1.Text);
                            Assert.AreEqual(typeof(YieldCurve).FullName, test1.DataTypeName);
                            Assert.AreEqual(typeof(YieldCurve), test1.DataType);
                            Assert.IsNotNull(test1.Data);
                            Assert.AreEqual(typeof(YieldCurve), test1.Data.GetType());

                            // - save as base type
                            client.SaveObject<PricingStructure>(dataA, "TestA", null, TimeSpan.MaxValue);
                            ICoreItem test2 = client.LoadItem<PricingStructure>("TestA");
                            Assert.IsNotNull(test2);
                            Assert.IsNotNull(test2.Text);
                            Assert.AreEqual(
                                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<PricingStructure xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:q1=\"http://www.fpml.org/FpML-5/reporting\" xsi:type=\"q1:YieldCurve\">\r\n  <q1:currency>USD</q1:currency>\r\n  <q1:algorithm>FastCubicSpline</q1:algorithm>\r\n</PricingStructure>",
                                test2.Text);
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
                                    currency1 = new Currency() { Value = "USD" },
                                    currency2 = new Currency() { Value = "JPY" },
                                    quoteBasis = QuoteBasisEnum.Currency2PerCurrency1
                                }
                            };
                            // - save as derived type
                            client.SaveObject(dataB, "TestB", null, TimeSpan.MaxValue);
                            ICoreItem test1 = client.LoadItem<FxCurve>("TestB");
                            Assert.IsNotNull(test1);
                            Assert.IsNotNull(test1.Text);
                            Assert.AreEqual(
                                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<fxCurve xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.fpml.org/FpML-5/reporting\">\r\n  <quotedCurrencyPair>\r\n    <currency1>USD</currency1>\r\n    <currency2>JPY</currency2>\r\n  </quotedCurrencyPair>\r\n</fxCurve>",
                                test1.Text);
                            Assert.AreEqual(typeof(FxCurve).FullName, test1.DataTypeName);
                            Assert.AreEqual(typeof(FxCurve), test1.DataType);
                            Assert.IsNotNull(test1.Data);
                            Assert.AreEqual(typeof(FxCurve), test1.Data.GetType());
                            // - save as base type
                            client.SaveObject<PricingStructure>(dataB, "TestB", null, TimeSpan.MaxValue);
                            ICoreItem test2 = client.LoadItem<PricingStructure>("TestB");
                            Assert.IsNotNull(test2);
                            Assert.IsNotNull(test2.Text);
                            Assert.AreEqual(
                                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<PricingStructure xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:q1=\"http://www.fpml.org/FpML-5/reporting\" xsi:type=\"q1:FxCurve\">\r\n  <q1:quotedCurrencyPair>\r\n    <q1:currency1>USD</q1:currency1>\r\n    <q1:currency2>JPY</q1:currency2>\r\n  </q1:quotedCurrencyPair>\r\n</PricingStructure>",
                                test2.Text);
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
                    // shutdown
                    server.Stop();
                }
            }
        }

        [TestMethod]
        public void TestSerialisingInterfaceTypes()
        {
            // tests control of the serialisation type
            // - interface types
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
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
                                client.SaveObject(codeValueInterface, "Test3", null, TimeSpan.MaxValue);
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
                    // shutdown
                    server.Stop();
                }
            }
        }

        [TestMethod]
        public void TestMutationDetection()
        {
            // tests the object mutation detector
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (CoreServer server = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    // start server
                    server.Start();
                    using (ICoreClient client = new CoreClientFactory(loggerRef).SetEnv("UTT").Create())
                    {
                        // create reference object
                        TestData data0 = new TestData("Zero", 0);
                        client.SaveObject(data0, "Item0", null);
                        ICoreItem item = client.LoadItem<TestData>("Item0");
                        string text = item.Text;
                        TestData data = item.GetData<TestData>(false);
                        // - assert not mutated
                        Assert.AreEqual(text, XmlSerializerHelper.SerializeToString(item.GetData<TestData>(false)));
                        // - mutate the object
                        data.field1 = "One";
                        data.field2 = 1;
                        // - assert mutated
                        Assert.AreNotEqual(text, XmlSerializerHelper.SerializeToString(item.GetData<TestData>(false)));
                    }
                    // shutdown
                    server.Stop();
                }
            }
        }
    }
}
