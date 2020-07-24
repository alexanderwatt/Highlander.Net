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
using Highlander.Utilities.NamedValues;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests.NamedValues
{
    /// <summary>
    /// Summary description for NamedValueSetTests
    /// </summary>
    [TestClass]
    public class NamedValueSetTests
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
        public void TestLegalCharsInNames()
        {
            NamedValueSet nvs = new NamedValueSet();
            // typical name
            nvs.Set("AZaz09_", "junk");
            // names can have whitespace which is ignored
            Assert.AreEqual("junk", nvs.GetValue("\tAZaz 09_\r\n", "junk"));
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
            Assert.AreEqual("The quick brown fox jumps recursively over the lazy dog.", output);
        }

        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("Scalar1/String=Text", (new NamedValue("Scalar1", "Text")).ToString());
            Assert.AreEqual("Scalar2/Guid=0710d9da-6c86-4f31-83af-516762d7ad3e", (new NamedValue("Scalar2", new Guid("{0710D9DA-6C86-4f31-83AF-516762D7AD3E}"))).ToString());
            Assert.AreEqual("Scalar3/DayOfWeek=Thursday", (new NamedValue("Scalar3", DayOfWeek.Thursday)).ToString());
            Assert.AreEqual("Vector1/String[]=[Hello,|world!]", (new NamedValue("Vector1", new[] { "Hello,", "world!" })).ToString());
            Assert.AreEqual("Vector2/Char[]=[H|e|l|l|o|!]", (new NamedValue("Vector2", "Hello!".ToCharArray())).ToString());
            Assert.AreEqual("Vector3/Byte[]=[0|1|127|128|255]", new NamedValue("Vector3", new byte[] { 0, 1, 127, 128, 255 }).ToString());
        }

        [TestMethod]
        public void TestNullableGet()
        {
            NamedValueSet properties = new NamedValueSet();
            decimal? notional = null;

            // using GetValue<T>
            notional = properties.GetValue<decimal>("Notional", false);
            Assert.IsTrue(notional.HasValue);
            Assert.AreEqual(0.0M, notional.Value);

            // using GetNullable<T>
            // - value is missing
            notional = properties.GetNullable<decimal>("Notional");
            Assert.IsFalse(notional.HasValue);
            Assert.AreEqual(null, notional);

            // - value not missing
            properties.Set("Notional", 1.0M);
            notional = properties.GetNullable<decimal>("Notional");
            Assert.IsTrue(notional.HasValue);
            Assert.AreEqual(1.0M, notional);
        }
    }
}