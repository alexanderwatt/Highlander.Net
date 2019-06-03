using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Serialisation;

namespace Util.Tests.Serialisation
{
    [TestClass]
    public class JsonSerializerHelperTest
    {
        public class TestClass
        {
            public string Name { get; set; }
            public int Id { get; set; }
        }

        [TestMethod]
        public void SerializeAndDeserializeFileTest()
        {
            TestClass testClass = new TestClass{Id=1, Name="This Is A Test"};
            var json = JsonSerializerHelper.SerializeToString(testClass);
            TestClass testClassOut = JsonSerializerHelper.DeserializeFromString<TestClass>(json);
            string result = XmlSerializerHelper.SerializeToString(testClassOut);
            //Debug.Print(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreEqual(testClass.Id, testClassOut.Id);
            Assert.AreEqual(testClass.Name, testClassOut.Name);
        }
    }
}
