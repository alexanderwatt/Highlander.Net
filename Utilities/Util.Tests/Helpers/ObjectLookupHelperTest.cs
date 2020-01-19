using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Highlander.Utilities.Helpers;

namespace Highlander.Utilities.Tests.Helpers
{
    /// <summary>
    ///This is a test class for ObjectLookupHelperTest and is intended
    ///to contain all ObjectLookupHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class ObjectLookupHelperTest
    {
        /// <summary>
        /// fpml style object with lower case id
        /// </summary>
        private class TestSubClass
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        private class TestClass
        {
            public int PropertyInt { get; set; }
            public string PropertyString { get; set; }
            public TestSubClass Sub { get; set; }
            public TestSubClass[] Subs { get; set; }
        }

        /// <summary>
        ///A test for SetPropertyValue
        ///</summary>
        [TestMethod]
        public void SetPropertyValueTest()
        {
            TestClass theObject = new TestClass { PropertyInt = 7, PropertyString = "Test"};
            const int expected = 6;
            ObjectLookupHelper.SetPropertyValue(theObject, "PropertyInt", expected);
            Assert.AreEqual(theObject.PropertyInt, expected);
        }

        /// <summary>
        ///A test for ObjectPropertyExists
        ///</summary>
        [TestMethod]
        public void ObjectPropertyExistsTest()
        {
            TestClass theObject = new TestClass { PropertyInt = 7, PropertyString = "Test" };
            bool actual = ObjectLookupHelper.ObjectPropertyExists(theObject, "PropertyString");
            Assert.IsTrue(actual);

            actual = ObjectLookupHelper.ObjectPropertyExists(theObject, "PropertyString1");
            Assert.IsFalse(actual);
        }

        /// <summary>
        ///A test for GetPropertyValue
        ///</summary>
        [TestMethod]
        public void GetPropertyValueTest()
        {
            TestClass theObject = new TestClass { PropertyInt = 7, PropertyString = "Test" };
            var actual = ObjectLookupHelper.GetPropertyValue(theObject, "PropertyString");
            Assert.AreEqual(theObject.PropertyString, actual);
        }

        /// <summary>
        ///A test for GetObjectProperty
        ///</summary>
        [TestMethod]
        public void GetObjectPropertyTest()
        {
            TestClass theObject = new TestClass { PropertyInt = 7, PropertyString = "Test" };
            PropertyInfo expected = theObject.GetType().GetProperty("PropertyString");
            PropertyInfo actual = ObjectLookupHelper.GetObjectProperty(theObject, "PropertyString");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetObjectProperties
        ///</summary>
        [TestMethod]
        public void GetObjectPropertiesTest()
        {
            TestClass theObject = new TestClass { PropertyInt = 7, PropertyString = "Test" };

            PropertyInfo[] expected
                = new PropertyInfo[]
                      {
                          theObject.GetType().GetProperty("PropertyInt"),
                          theObject.GetType().GetProperty("PropertyString"),
                          theObject.GetType().GetProperty("Sub"),
                          theObject.GetType().GetProperty("Subs"),
                      };

            PropertyInfo[] actual = ObjectLookupHelper.GetObjectProperties(theObject);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetByIdTest()
        {
            // When Id is set the object is returned
            TestSubClass testSubClass = new TestSubClass {id = "theId", name = "theName"};
            TestClass theObject = new TestClass {PropertyInt = 7, PropertyString = "Test", Sub = testSubClass};
            TestSubClass actual = ObjectLookupHelper.GetById<TestSubClass>(theObject, testSubClass.id);
            Assert.AreEqual(theObject.Sub, actual);

            // Id is null, returns null
            testSubClass = new TestSubClass { id = null, name = "theName" };
            theObject = new TestClass { PropertyInt = 7, PropertyString = "Test", Sub = testSubClass };
            actual = ObjectLookupHelper.GetById<TestSubClass>(theObject, testSubClass.id);
            Assert.AreEqual(null, actual);

            // Id is set in Subs, that Sub is returned
            testSubClass = new TestSubClass { id = "theId", name = "theName" };
            theObject = new TestClass { PropertyInt = 7, PropertyString = "Test", Sub = null, Subs = new TestSubClass[]{testSubClass} };
            actual = ObjectLookupHelper.GetById<TestSubClass>(theObject, testSubClass.id);
            Assert.AreEqual(theObject.Subs[0], actual);

            // This time the Id is not found
            testSubClass = new TestSubClass { id = "theId", name = "theName" };
            theObject = new TestClass { PropertyInt = 7, PropertyString = "Test", Sub = null, Subs = new TestSubClass[] { testSubClass } };
            actual = ObjectLookupHelper.GetById<TestSubClass>(theObject, "invalid Id");
            Assert.AreEqual(null, actual);
        }
    }
}