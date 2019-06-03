using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Metadata.Common;

namespace FpML.V5r3.Tests
{
    /// <summary>
    /// Summary description for DateTimeZoneParserTests
    /// </summary>
    [TestClass]
    public class DateTimeZoneParserTests
    {
        public DateTimeZoneParserTests()
        {
            //
            // TODO: Add constructor logic here
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
        public void TestRDP_EmptyStream()
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("")))
            {
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
            }
        }

        [TestMethod]
        public void TestRDP_RequiredChar()
        {
            // fails due to end of stream
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("")))
            {
                char? result;
                Assert.IsFalse(DateTimeZoneParser.ParseChar(ms, null, true, out result));
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                Assert.IsFalse(result.HasValue);
            }
            // specific value
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("+")))
            {
                char? result;
                Assert.IsTrue(DateTimeZoneParser.ParseChar(ms, '+', true, out result));
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual<char>('+', result.Value);
            }
            // any value
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("*")))
            {
                char? result;
                Assert.IsTrue(DateTimeZoneParser.ParseChar(ms, null, true, out result));
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual<char>('*', result.Value);
            }
            // fails due to incorrect value, but retry succeeds
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("+")))
            {
                char? result;
                Assert.IsFalse(DateTimeZoneParser.ParseChar(ms, '-', true, out result));
                Assert.IsFalse(result.HasValue);
                // retry
                Assert.IsTrue(DateTimeZoneParser.ParseChar(ms, '+', true, out result));
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual<char>('+', result.Value);
            }
        }

        [TestMethod]
        public void TestRDP_OptionalChar()
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("")))
            {
                char? result;
                Assert.IsTrue(DateTimeZoneParser.ParseChar(ms, '+', false, out result));
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                Assert.IsFalse(result.HasValue);
            }
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("+")))
            {
                char? result;
                Assert.IsTrue(DateTimeZoneParser.ParseChar(ms, '+', false, out result));
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual<char>('+', result.Value);
            }
        }

        [TestMethod]
        public void TestRDP_RequiredDecDigits()
        {
            // fails due to end of stream
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("")))
            {
                int? result;
                // required
                Assert.IsFalse(DateTimeZoneParser.ParseDecDigits(ms, true, out result));
                // optional
                Assert.IsTrue(DateTimeZoneParser.ParseDecDigits(ms, false, out result));
                Assert.IsFalse(result.HasValue);
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                Assert.IsFalse(result.HasValue);
            }
            // any value
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("999 111")))
            {
                int? result;
                char? space;
                Assert.AreEqual<long>(0, ms.Position);
                Assert.IsTrue(DateTimeZoneParser.ParseDecDigits(ms, true, out result));
                Assert.AreEqual<long>(3, ms.Position);
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual<int>(999, result.Value);
                Assert.IsTrue(DateTimeZoneParser.ParseChar(ms, ' ', true, out space));
                Assert.AreEqual<long>(4, ms.Position);
                Assert.IsTrue(DateTimeZoneParser.ParseDecDigits(ms, true, out result));
                Assert.AreEqual<long>(7, ms.Position);
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual<int>(111, result.Value);
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
            }
        }

        [TestMethod]
        public void TestRDP_RequiredHexDigits()
        {
            // fails due to end of stream
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("")))
            {
                int? result;
                // required
                Assert.IsFalse(DateTimeZoneParser.ParseHexDigits(ms, null, true, out result));
                // optional
                Assert.IsTrue(DateTimeZoneParser.ParseHexDigits(ms, null, false, out result));
                Assert.IsFalse(result.HasValue);
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                Assert.IsFalse(result.HasValue);
            }
            // any value
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("1ff FFFF")))
            {
                int? result;
                char? space;
                Assert.AreEqual<long>(0, ms.Position);
                Assert.IsTrue(DateTimeZoneParser.ParseHexDigits(ms, null, true, out result));
                Assert.AreEqual<long>(3, ms.Position);
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual<int>(511, result.Value);
                Assert.IsTrue(DateTimeZoneParser.ParseChar(ms, ' ', true, out space));
                Assert.AreEqual<long>(4, ms.Position);
                Assert.IsTrue(DateTimeZoneParser.ParseHexDigits(ms, null, true, out result));
                Assert.AreEqual<long>(8, ms.Position);
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual<int>(65535, result.Value);
                Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
            }
        }

        public class TestPair<T> where T : struct
        {
            public readonly string Input;
            public readonly T? Result;
            public TestPair(string input, T? result)
            {
                Input = input;
                Result = result;
            }
        }

        [TestMethod]
        public void TestRDP_Integers()
        {
            List<TestPair<int>> testPairs = new List<TestPair<int>>();
            testPairs.Add(new TestPair<int>("", null));
            testPairs.Add(new TestPair<int>("123", 123));
            testPairs.Add(new TestPair<int>("-123", -123));
            testPairs.Add(new TestPair<int>("+123", 123));
            testPairs.Add(new TestPair<int>("0x7b", 123));

            foreach (var testPair in testPairs)
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(testPair.Input)))
                {
                    int? result;
                    Assert.IsTrue(DateTimeZoneParser.ParseInt32(ms, false, out result), String.Format("Input='{0}'", testPair.Input));
                    Assert.AreEqual<int?>(testPair.Result, result, String.Format("Input='{0}'", testPair.Input));
                    Assert.IsTrue(DateTimeZoneParser.ParseEndOfStream(ms));
                }
            }
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestDTZParserFailsWhenInputIsNull()
        {
            var dtz = new DateTimeZoneParser(null);
        }

        [ExpectedException(typeof(FormatException))]
        [TestMethod]
        public void TestDTZParserFailsWithWrongDateSeparatorsA()
        {
            var dtz = new DateTimeZoneParser("2012/10/31");
        }

        [ExpectedException(typeof(FormatException))]
        [TestMethod]
        public void TestDTZParserFailsWithWrongDateSeparatorsB()
        {
            var dtz = new DateTimeZoneParser("2012.10.31");
        }

        [ExpectedException(typeof(FormatException))]
        [TestMethod]
        public void TestDTZParserFailsWithWrongDateSeparatorsC()
        {
            var dtz = new DateTimeZoneParser("2012:10:31");
        }

        [ExpectedException(typeof(FormatException))]
        [TestMethod]
        public void TestDTZParserFailsWithWrongTimeSeparatorsA()
        {
            var dtz = new DateTimeZoneParser("23.59.59");
        }

        [ExpectedException(typeof(FormatException))]
        [TestMethod]
        public void TestDTZParserFailsWithWrongTimeSeparatorsB()
        {
            var dtz = new DateTimeZoneParser("23/59/59");
        }

        [TestMethod]
        public void TestDTZParserPassesForDateOnly()
        {
            var dtz = new DateTimeZoneParser("2012-10-31");
            Assert.IsTrue(dtz.DatePart.HasValue);
            Assert.IsFalse(dtz.TimePart.HasValue);
            Assert.IsFalse(dtz.ZonePart.HasValue);
        }

        [TestMethod]
        public void TestDTZParserPassesForTimeOnly()
        {
            var dtz = new DateTimeZoneParser("23:59:59");
            Assert.IsFalse(dtz.DatePart.HasValue);
            Assert.IsTrue(dtz.TimePart.HasValue);
            Assert.IsFalse(dtz.ZonePart.HasValue);
        }

        [TestMethod]
        public void TestDTZParserPassesForDateAndTime()
        {
            var dtz = new DateTimeZoneParser("2012-10-31T23:59:59");
            Assert.IsTrue(dtz.DatePart.HasValue);
            Assert.IsTrue(dtz.TimePart.HasValue);
            Assert.IsFalse(dtz.ZonePart.HasValue);
        }

        [ExpectedException(typeof(FormatException))]
        [TestMethod]
        public void TestDTZParserFailsWithZoneOnly()
        {
            var dtz = new DateTimeZoneParser("+10:00");
        }

        [TestMethod]
        public void TestDTZParserPassesForDateWithZoneOffset()
        {
            var dtz = new DateTimeZoneParser("2012-10-31-05:00");
            Assert.IsTrue(dtz.DatePart.HasValue);
            Assert.IsFalse(dtz.TimePart.HasValue);
            Assert.IsTrue(dtz.ZonePart.HasValue);
        }

        [TestMethod]
        public void TestDTZParserPassesForDateWithZoneUtc()
        {
            var dtz = new DateTimeZoneParser("2012-10-31Z");
            Assert.IsTrue(dtz.DatePart.HasValue);
            Assert.IsFalse(dtz.TimePart.HasValue);
            Assert.IsTrue(dtz.ZonePart.HasValue);
        }

        [TestMethod]
        public void TestDTZParserPassesForDateAndTimeAndZone()
        {
            var dtz = new DateTimeZoneParser("2012-10-31T23:59:59+10:00");
            Assert.IsTrue(dtz.DatePart.HasValue);
            Assert.IsTrue(dtz.TimePart.HasValue);
            Assert.IsTrue(dtz.ZonePart.HasValue);
        }

        [TestMethod]
        public void TestDTZParserPassesForTimeWithFractionalSeconds()
        {
            var dtz = new DateTimeZoneParser("20:08:10.5000000");
            Assert.IsFalse(dtz.DatePart.HasValue);
            Assert.IsTrue(dtz.TimePart.HasValue);
            Assert.IsFalse(dtz.ZonePart.HasValue);
        }

        [TestMethod]
        public void TestDTZParserPassesForTimeWithFractionalSecondsPlusZone()
        {
            var dtz = new DateTimeZoneParser("20:08:10.5000000-05:00");
            Assert.IsFalse(dtz.DatePart.HasValue);
            Assert.IsTrue(dtz.TimePart.HasValue);
            Assert.IsTrue(dtz.ZonePart.HasValue);
        }

        [TestMethod]
        public void TestDTZParserDefaultOutputValuesAreSameAsInput()
        {
            // default is to preserve source formatting
            // - date part
            Assert.AreEqual<string>("2012-10-31", new DateTimeZoneParser("2012-10-31").ToString());
            Assert.AreEqual<string>("2012-10-31Z", new DateTimeZoneParser("2012-10-31Z").ToString());
            Assert.AreEqual<string>("2012-10-31-05:00", new DateTimeZoneParser("2012-10-31-05:00").ToString());
            Assert.AreEqual<string>("2012-10-31+10:00", new DateTimeZoneParser("2012-10-31+10:00").ToString());
            // - time part
            Assert.AreEqual<string>("20:08:10", new DateTimeZoneParser("20:08:10").ToString());
            Assert.AreEqual<string>("20:08:10Z", new DateTimeZoneParser("20:08:10Z").ToString());
            Assert.AreEqual<string>("20:08:10-05:00", new DateTimeZoneParser("20:08:10-05:00").ToString());
            Assert.AreEqual<string>("20:08:10+10:00", new DateTimeZoneParser("20:08:10+10:00").ToString());
            Assert.AreEqual<string>("20:08:10.5000000", new DateTimeZoneParser("20:08:10.5000000").ToString());
            Assert.AreEqual<string>("20:08:10.5000000Z", new DateTimeZoneParser("20:08:10.5000000Z").ToString());
            Assert.AreEqual<string>("20:08:10.5000000-05:00", new DateTimeZoneParser("20:08:10.5000000-05:00").ToString());
            Assert.AreEqual<string>("20:08:10.5000000+10:00", new DateTimeZoneParser("20:08:10.5000000+10:00").ToString());
            // - date and time parts
            Assert.AreEqual<string>("2012-10-31T20:08:10", new DateTimeZoneParser("2012-10-31T20:08:10").ToString());
            Assert.AreEqual<string>("2012-10-31T20:08:10Z", new DateTimeZoneParser("2012-10-31T20:08:10Z").ToString());
            Assert.AreEqual<string>("2012-10-31T20:08:10-05:00", new DateTimeZoneParser("2012-10-31T20:08:10-05:00").ToString());
            Assert.AreEqual<string>("2012-10-31T20:08:10+10:00", new DateTimeZoneParser("2012-10-31T20:08:10+10:00").ToString());
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000", new DateTimeZoneParser("2012-10-31T20:08:10.5000000").ToString());
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000Z", new DateTimeZoneParser("2012-10-31T20:08:10.5000000Z").ToString());
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000-05:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000-05:00").ToString());
        }

        [TestMethod]
        public void TestDTZParserConvertsToUniversalTime()
        {
            // convert to UTC
            OutputDateTimeKind outputFormat = OutputDateTimeKind.ConvertToUniversal;
            // - date part
            Assert.AreEqual<string>("2012-10-31Z", new DateTimeZoneParser("2012-10-31").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31Z", new DateTimeZoneParser("2012-10-31Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31Z", new DateTimeZoneParser("2012-10-31-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-30Z", new DateTimeZoneParser("2012-10-31+10:00").ToString(outputFormat, null));
            // - time part
            Assert.AreEqual<string>("20:08:10Z", new DateTimeZoneParser("20:08:10").ToString(outputFormat, null));
            Assert.AreEqual<string>("20:08:10Z", new DateTimeZoneParser("20:08:10Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("01:08:10Z", new DateTimeZoneParser("20:08:10-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("10:08:10Z", new DateTimeZoneParser("20:08:10+10:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("20:08:10.5000000Z", new DateTimeZoneParser("20:08:10.5000000").ToString(outputFormat, null));
            Assert.AreEqual<string>("20:08:10.5000000Z", new DateTimeZoneParser("20:08:10.5000000Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("01:08:10.5000000Z", new DateTimeZoneParser("20:08:10.5000000-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("10:08:10.5000000Z", new DateTimeZoneParser("20:08:10.5000000+10:00").ToString(outputFormat, null));
            // - date and time parts
            Assert.AreEqual<string>("2012-10-31T20:08:10Z", new DateTimeZoneParser("2012-10-31T20:08:10").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T20:08:10Z", new DateTimeZoneParser("2012-10-31T20:08:10Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-11-01T01:08:10Z", new DateTimeZoneParser("2012-10-31T20:08:10-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T10:08:10Z", new DateTimeZoneParser("2012-10-31T20:08:10+10:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000Z", new DateTimeZoneParser("2012-10-31T20:08:10.5000000").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000Z", new DateTimeZoneParser("2012-10-31T20:08:10.5000000Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-11-01T01:08:10.5000000Z", new DateTimeZoneParser("2012-10-31T20:08:10.5000000-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T10:08:10.5000000Z", new DateTimeZoneParser("2012-10-31T20:08:10.5000000+10:00").ToString(outputFormat, null));
        }

        [TestMethod]
        public void TestDTZParserConvertsToCustomTimeNYC()
        {
            // convert to user-defined time zone (NYC)
            OutputDateTimeKind outputFormat = OutputDateTimeKind.ConvertToCustom;
            TimeSpan customOffset = TimeSpan.FromHours(-5);
            // - date part
            Assert.AreEqual<string>("2012-10-30-05:00", new DateTimeZoneParser("2012-10-31").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-30-05:00", new DateTimeZoneParser("2012-10-31Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31-05:00", new DateTimeZoneParser("2012-10-31-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-30-05:00", new DateTimeZoneParser("2012-10-31+10:00").ToString(outputFormat, customOffset));
            // - time part
            Assert.AreEqual<string>("15:08:10-05:00", new DateTimeZoneParser("20:08:10").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("15:08:10-05:00", new DateTimeZoneParser("20:08:10Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("20:08:10-05:00", new DateTimeZoneParser("20:08:10-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("05:08:10-05:00", new DateTimeZoneParser("20:08:10+10:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("15:08:10.5000000-05:00", new DateTimeZoneParser("20:08:10.5000000").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("15:08:10.5000000-05:00", new DateTimeZoneParser("20:08:10.5000000Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("20:08:10.5000000-05:00", new DateTimeZoneParser("20:08:10.5000000-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("05:08:10.5000000-05:00", new DateTimeZoneParser("20:08:10.5000000+10:00").ToString(outputFormat, customOffset));
            // - date and time parts
            Assert.AreEqual<string>("2012-10-31T15:08:10-05:00", new DateTimeZoneParser("2012-10-31T20:08:10").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T15:08:10-05:00", new DateTimeZoneParser("2012-10-31T20:08:10Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T20:08:10-05:00", new DateTimeZoneParser("2012-10-31T20:08:10-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T05:08:10-05:00", new DateTimeZoneParser("2012-10-31T20:08:10+10:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T15:08:10.5000000-05:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T15:08:10.5000000-05:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000-05:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T05:08:10.5000000-05:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000+10:00").ToString(outputFormat, customOffset));
        }

        [TestMethod]
        public void TestDTZParserConvertsToCustomTimeSYD()
        {
            // convert to user-defined time zone (SYD)
            OutputDateTimeKind outputFormat = OutputDateTimeKind.ConvertToCustom;
            TimeSpan customOffset = TimeSpan.FromHours(10);
            // - date part
            Assert.AreEqual<string>("2012-10-31+10:00", new DateTimeZoneParser("2012-10-31").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31+10:00", new DateTimeZoneParser("2012-10-31Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31+10:00", new DateTimeZoneParser("2012-10-31-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31+10:00", new DateTimeZoneParser("2012-10-31+10:00").ToString(outputFormat, customOffset));
            // - time part
            Assert.AreEqual<string>("06:08:10+10:00", new DateTimeZoneParser("20:08:10").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("06:08:10+10:00", new DateTimeZoneParser("20:08:10Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("11:08:10+10:00", new DateTimeZoneParser("20:08:10-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("20:08:10+10:00", new DateTimeZoneParser("20:08:10+10:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("06:08:10.5000000+10:00", new DateTimeZoneParser("20:08:10.5000000").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("06:08:10.5000000+10:00", new DateTimeZoneParser("20:08:10.5000000Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("11:08:10.5000000+10:00", new DateTimeZoneParser("20:08:10.5000000-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("20:08:10.5000000+10:00", new DateTimeZoneParser("20:08:10.5000000+10:00").ToString(outputFormat, customOffset));
            // - date and time parts
            Assert.AreEqual<string>("2012-11-01T06:08:10+10:00", new DateTimeZoneParser("2012-10-31T20:08:10").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-11-01T06:08:10+10:00", new DateTimeZoneParser("2012-10-31T20:08:10Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-11-01T11:08:10+10:00", new DateTimeZoneParser("2012-10-31T20:08:10-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T20:08:10+10:00", new DateTimeZoneParser("2012-10-31T20:08:10+10:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-11-01T06:08:10.5000000+10:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-11-01T06:08:10.5000000+10:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-11-01T11:08:10.5000000+10:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000+10:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000+10:00").ToString(outputFormat, customOffset));
        }

        [TestMethod]
        public void TestDTZParserConvertsToUniversalTimeOnlyIfSpecified()
        {
            // convert specified time zones to UTC
            OutputDateTimeKind outputFormat = OutputDateTimeKind.UnspecifiedOrUniversal;
            // - date part
            Assert.AreEqual<string>("2012-10-31", new DateTimeZoneParser("2012-10-31").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31Z", new DateTimeZoneParser("2012-10-31Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31Z", new DateTimeZoneParser("2012-10-31-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-30Z", new DateTimeZoneParser("2012-10-31+10:00").ToString(outputFormat, null));
            // - time part
            Assert.AreEqual<string>("20:08:10", new DateTimeZoneParser("20:08:10").ToString(outputFormat, null));
            Assert.AreEqual<string>("20:08:10Z", new DateTimeZoneParser("20:08:10Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("01:08:10Z", new DateTimeZoneParser("20:08:10-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("10:08:10Z", new DateTimeZoneParser("20:08:10+10:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("20:08:10.5000000", new DateTimeZoneParser("20:08:10.5000000").ToString(outputFormat, null));
            Assert.AreEqual<string>("20:08:10.5000000Z", new DateTimeZoneParser("20:08:10.5000000Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("01:08:10.5000000Z", new DateTimeZoneParser("20:08:10.5000000-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("10:08:10.5000000Z", new DateTimeZoneParser("20:08:10.5000000+10:00").ToString(outputFormat, null));
            // - date and time parts
            Assert.AreEqual<string>("2012-10-31T20:08:10", new DateTimeZoneParser("2012-10-31T20:08:10").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T20:08:10Z", new DateTimeZoneParser("2012-10-31T20:08:10Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-11-01T01:08:10Z", new DateTimeZoneParser("2012-10-31T20:08:10-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T10:08:10Z", new DateTimeZoneParser("2012-10-31T20:08:10+10:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000", new DateTimeZoneParser("2012-10-31T20:08:10.5000000").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000Z", new DateTimeZoneParser("2012-10-31T20:08:10.5000000Z").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-11-01T01:08:10.5000000Z", new DateTimeZoneParser("2012-10-31T20:08:10.5000000-05:00").ToString(outputFormat, null));
            Assert.AreEqual<string>("2012-10-31T10:08:10.5000000Z", new DateTimeZoneParser("2012-10-31T20:08:10.5000000+10:00").ToString(outputFormat, null));
        }

        [TestMethod]
        public void TestDTZParserConvertsToCustomTimeOnlyIfSpecified()
        {
            // convert input time zone to user-defined time zone (NYC) if specified
            OutputDateTimeKind outputFormat = OutputDateTimeKind.UnspecifiedOrCustom;
            TimeSpan customOffset = TimeSpan.FromHours(-5);
            // - date part
            Assert.AreEqual<string>("2012-10-31", new DateTimeZoneParser("2012-10-31").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-30-05:00", new DateTimeZoneParser("2012-10-31Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31-05:00", new DateTimeZoneParser("2012-10-31-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-30-05:00", new DateTimeZoneParser("2012-10-31+10:00").ToString(outputFormat, customOffset));
            // - time part
            Assert.AreEqual<string>("20:08:10", new DateTimeZoneParser("20:08:10").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("15:08:10-05:00", new DateTimeZoneParser("20:08:10Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("20:08:10-05:00", new DateTimeZoneParser("20:08:10-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("05:08:10-05:00", new DateTimeZoneParser("20:08:10+10:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("20:08:10.5000000", new DateTimeZoneParser("20:08:10.5000000").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("15:08:10.5000000-05:00", new DateTimeZoneParser("20:08:10.5000000Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("20:08:10.5000000-05:00", new DateTimeZoneParser("20:08:10.5000000-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("05:08:10.5000000-05:00", new DateTimeZoneParser("20:08:10.5000000+10:00").ToString(outputFormat, customOffset));
            // - date and time parts
            Assert.AreEqual<string>("2012-10-31T20:08:10", new DateTimeZoneParser("2012-10-31T20:08:10").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T15:08:10-05:00", new DateTimeZoneParser("2012-10-31T20:08:10Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T20:08:10-05:00", new DateTimeZoneParser("2012-10-31T20:08:10-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T05:08:10-05:00", new DateTimeZoneParser("2012-10-31T20:08:10+10:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000", new DateTimeZoneParser("2012-10-31T20:08:10.5000000").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T15:08:10.5000000-05:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000Z").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T20:08:10.5000000-05:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000-05:00").ToString(outputFormat, customOffset));
            Assert.AreEqual<string>("2012-10-31T05:08:10.5000000-05:00", new DateTimeZoneParser("2012-10-31T20:08:10.5000000+10:00").ToString(outputFormat, customOffset));
        }

    }
}
