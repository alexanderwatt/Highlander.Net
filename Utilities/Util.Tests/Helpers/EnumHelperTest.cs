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
using Highlander.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests.Helpers
{
    /// <summary>
    ///This is a test class for EnumHelperTest and is intended
    ///to contain all EnumHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class EnumHelperTest
    {
        public enum TestEnum
        {
            Value1,
            Value2,
            Value3,
            [System.Xml.Serialization.XmlEnumAttribute("A*Test-Value")]
            ATestValue
        }

        private enum PeriodEnum
        {
            D,
            W,
            M,
            Y,
        }

        [TestMethod]
        public void TryParseTestWithCase()
        {
            bool passed = EnumHelper.TryParse("Value2", false, out TestEnum output);
            Assert.IsTrue(passed);
            Assert.AreEqual(TestEnum.Value2, output);

            passed = EnumHelper.TryParse("value2", false, out output);
            Assert.IsFalse(passed);
            Assert.AreNotEqual(TestEnum.Value2, output);
        }

        [TestMethod]
        public void TryParseTestWithoutCase()
        {
            bool passed = EnumHelper.TryParse("Value2", true, out TestEnum output);
            Assert.IsTrue(passed);
            Assert.AreEqual(TestEnum.Value2, output);

            passed = EnumHelper.TryParse("value2", true, out output);
            Assert.IsTrue(passed);
            Assert.AreEqual(TestEnum.Value2, output);

            passed = EnumHelper.TryParse("TeST", true, out output);
            Assert.IsFalse(passed);
        }

        /// <summary>
        ///A test for ToEnumId
        ///</summary>
        [TestMethod]
        public void ToEnumIdTest()
        {
            string actual = EnumHelper.ToEnumId("A&M");
            Assert.AreEqual("A_M", actual);

            actual = EnumHelper.ToEnumId("1A:M");
            Assert.AreEqual("_1A_M", actual);
        }

        /// <summary>
        ///A test for ToEnumId
        ///</summary>
        [TestMethod]
        public void ToEnumIdNullTest()
        {
            try
            {
                string actual = EnumHelper.ToEnumId("");
                Assert.Fail();
            }
            catch (ArgumentNullException)
            {
                // pass
            }

            try
            {
                string actual = EnumHelper.ToEnumId(null);
                Assert.Fail();
            }
            catch (ArgumentNullException)
            {
                // pass
            }
        }

        [TestMethod]
        public void ParseTest3WithCase()
        {
            TestEnum actual = EnumHelper.Parse<TestEnum>("Value1", false);
            Assert.AreEqual(TestEnum.Value1, actual);

            try
            {
                actual = EnumHelper.Parse<TestEnum>("value1", false);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                //pass
            }
        }

        [TestMethod]
        public void ParseTest2WithCase()
        {
            TestEnum actual = EnumHelper.Parse("Value1", false, TestEnum.Value3);
            Assert.AreEqual(TestEnum.Value1, actual);

            actual = EnumHelper.Parse("value1", false, TestEnum.Value3);
            Assert.AreEqual(TestEnum.Value3, actual);
        }

        [TestMethod]
        public void ParseTest1()
        {
            TestEnum actual = EnumHelper.Parse<TestEnum>("Value1");
            Assert.AreEqual(TestEnum.Value1, actual);

            try
            {
                actual = EnumHelper.Parse<TestEnum>("value1");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                //pass
            }
        }

        [TestMethod]
        public void ParseTest0()
        {
            PeriodEnum actual = EnumHelper.Parse<PeriodEnum>('D');
            Assert.AreEqual(PeriodEnum.D, actual);

            try
            {
                actual = EnumHelper.Parse<PeriodEnum>('d');
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                //pass
            }
        }

        [TestMethod]
        public void ToStringTest()
        {
            Assert.AreEqual("Value1", EnumHelper.ToString(TestEnum.Value1));
            Assert.AreEqual("A*Test-Value", EnumHelper.ToString(TestEnum.ATestValue));
        }

        [TestMethod]
        public void ToStringsTest()
        {
            var items = EnumHelper.ToStrings(typeof (TestEnum));
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual("A*Test-Value", items[3]);
        }

        [TestMethod]
        public void EnumToListTest()
        {
            Assert.AreEqual(4, EnumHelper.EnumToList<TestEnum>().Count);
        }
    }
}