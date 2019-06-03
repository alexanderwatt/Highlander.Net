using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.NamedValues;

namespace Util.Tests.NamedValues
{
    /// <summary>
    /// Summary description for NamedValueSetTests
    /// </summary>
    [TestClass]
    public class NamedValueSetTests
    {
        public NamedValueSetTests()
        {
            //
            // Add constructor logic here
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

        [TestMethod]
        public void TestLegalCharsInNames()
        {
            NamedValueSet nvs = new NamedValueSet();
            // typical name
            nvs.Set("AZaz09_", "junk");
            // names can have whitespace which is ignored
            Assert.AreEqual<string>("junk", nvs.GetValue<string>("\tAZaz 09_\r\n", "junk"));
            // fails because '$' is illegal
            UnitTestHelper.AssertThrows<ArgumentException>(() => { new NamedValue("AZaz$09_", "junk"); });
        }

        [TestMethod]
        public void TestTokenReplacement()
        {
            string input = "The {Speed} {COLOUR} {animal} {verb} the lazy dog.";
            NamedValueSet tokens = new NamedValueSet();
            tokens.Set("Speed", "quick");
            tokens.Set("Colour", "brown");
            tokens.Set("Animal", "fox");
            tokens.Set("Verb", "jumps {adverb} over"); // <-- look - recursion!
            tokens.Set("Adverb", "recursively"); // terminate recursion to prevent stack overflow

            string output = tokens.ReplaceTokens(input);
            Assert.AreEqual<string>("The quick brown fox jumps recursively over the lazy dog.", output);
        }

        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual<string>("Scalar1/String=Text", (new NamedValue("Scalar1", "Text")).ToString());
            Assert.AreEqual<string>("Scalar2/Guid=0710d9da-6c86-4f31-83af-516762d7ad3e", (new NamedValue("Scalar2", new Guid("{0710D9DA-6C86-4f31-83AF-516762D7AD3E}"))).ToString());
            Assert.AreEqual<string>("Scalar3/DayOfWeek=Thursday", (new NamedValue("Scalar3", DayOfWeek.Thursday)).ToString());
            Assert.AreEqual<string>("Vector1/String[]=[Hello,|world!]", (new NamedValue("Vector1", new string[] { "Hello,", "world!" })).ToString());
            Assert.AreEqual<string>("Vector2/Char[]=[H|e|l|l|o|!]", (new NamedValue("Vector2", "Hello!".ToCharArray())).ToString());
            Assert.AreEqual<string>("Vector3/Byte[]=[0|1|127|128|255]", new NamedValue("Vector3", new byte[] { 0, 1, 127, 128, 255 }).ToString());
        }

        [TestMethod]
        public void TestNullableGet()
        {
            NamedValueSet properties = new NamedValueSet();
            decimal? notional = null;

            // using GetValue<T>
            notional = properties.GetValue<decimal>("Notional", false);
            Assert.IsTrue(notional.HasValue);
            Assert.AreEqual<decimal>(0.0M, notional.Value);

            // using GetNullable<T>
            // - value is missing
            notional = properties.GetNullable<decimal>("Notional");
            Assert.IsFalse(notional.HasValue);
            Assert.AreEqual<decimal?>(null, notional);

            // - value not missing
            properties.Set("Notional", 1.0M);
            notional = properties.GetNullable<decimal>("Notional");
            Assert.IsTrue(notional.HasValue);
            Assert.AreEqual<decimal?>(1.0M, notional);
        }
    }
}