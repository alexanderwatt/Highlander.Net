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
using System.IO;
using Highlander.Core.Common.DataPooling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V34.Tests
{
    /// <summary>
    /// Summary description for DataItemTests
    /// </summary>
    [TestClass]
    public class DataItemTests
    {
        public DataItemTests()
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
        public void TestIntegerStringFormats()
        {
            int intValue = 1001;
            Assert.AreEqual<string>("1,001.00", intValue.ToString("N"));
            Assert.AreEqual<string>("1,001.00", intValue.ToString("N2"));
            Assert.AreEqual<string>("1,001", intValue.ToString("N0"));
        }

        [TestMethod]
        public void DataItemsCanContainValues()
        {
            //given a value
            var value = 10;

            //we can wrap it in an DataItem type like this
            DataItem<int> intItem = new DataItem<int>(value);

            Assert.IsTrue(intItem.HasValue);
            Assert.IsFalse(intItem.IsEmpty);
            Assert.AreEqual(10, intItem.Value);
        }

        [TestMethod]
        public void DataItemsDontHaveToContainvalues()
        {
            //given an DataItem type created with "Empty"
            var anyItem = new DataItem<Stream>();

            //we can see that it contains no value - it is set to "Empty"
            Assert.IsTrue(anyItem.IsEmpty);
            Assert.IsFalse(anyItem.HasValue);
            Stream tmp = null;
            UnitTestHelper.AssertThrows<InvalidOperationException>(
                                        () => tmp = anyItem.Value);

        }

        [TestMethod]
        public void UsingDataItemsPractically()
        {
            //when reading data from files
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write('a');
                writer.Flush();

                stream.Position = 0;
                var reader = new StreamReader(stream);

                //the read function doesn't always return a result (it returns -1 if a valid character can't be read)
                Func<int> read = reader.Read;

                //we can use an DataItem type to create a better design for this function
                Func<DataItem<char>> charItem =
                    () =>
                        {
                            var result = read();

                            if (result == -1)
                            {
                                return new DataItem<char>();
                            }
                            else
                            {
                                return new DataItem<char>((char)result);
                            }
                        };

                //the first read returns a result with the value 'a'
                Assert.AreEqual('a', charItem().Value);

                //but the second read returns Empty since it is at the end of the stream
                Assert.IsTrue(charItem().IsEmpty);
            }
        }

        [TestMethod]
        public void MultipleOpertionsWithDataItems()
        {
            //say we have a few functions that may or may not return a value.
            Func<int, DataItem<int>> divideIfEven = value => ((value % 2) == 0) ? new DataItem<int>(value / 2) : new DataItem<int>();
            Func<int, DataItem<int>> subtractIfDivisibleByThree = value => ((value % 3) == 0) ? new DataItem<int>(value - 3) : new DataItem<int>();
            Func<int, DataItem<int>> multiplyIfOdd = value => ((value % 2) != 0) ? new DataItem<int>(value * 2) : new DataItem<int>();

            //we can chain these operations together like this:
            DataItem<int> result =
                divideIfEven(36)
                    .Select(subtractIfDivisibleByThree)
                    .Select(multiplyIfOdd);


            //the result of one carries on to the next to yield the expected result
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(30, result.Value);
        }

        [TestMethod]
        public void MultipeOperationsFailing()
        {
            //say we have a few functions that may or may not return a value.
            Func<int, DataItem<int>> divideIfEven = value => ((value % 2) == 0) ? new DataItem<int>(value / 2) : new DataItem<int>();
            Func<int, DataItem<int>> multiplyIfOdd = value => ((value % 2) != 0) ? new DataItem<int>(value * 2) : new DataItem<int>();
            Func<int, DataItem<int>> subtractIfDivisibleByThree = value => ((value % 3) == 0) ? new DataItem<int>(value - 3) : new DataItem<int>();

            //if one operation in the chain fails...
            DataItem<int> result =
                divideIfEven(36)
                    .Select(multiplyIfOdd) //this will fail
                    .Select(subtractIfDivisibleByThree);


            //the result will be Empty
            Assert.IsTrue(result.IsEmpty);
        }

        #region Unit Tests

        [TestMethod]
        public void InequalityValueEmpty()
        {
            var value1 = new DataItem<int>(1);
            var empty1 = new DataItem<int>();

            Assert.AreNotEqual(value1, empty1);
        }

        [TestMethod]
        public void InequalityValueValue()
        {
            var value1 = new DataItem<int>(1);
            var value2 = new DataItem<int>(2);

            Assert.AreNotEqual(value1, value2);
        }

        [TestMethod]
        public void EqualityValueValue()
        {
            var value1 = new DataItem<int>(1);
            var value2 = new DataItem<int>(1);

            Assert.AreEqual(value1, value2);
        }

        [TestMethod]
        public void EqualityEmptyEmpty()
        {
            var empty1 = new DataItem<int>();
            var empty2 = new DataItem<int>();

            Assert.AreEqual(empty1, empty2);
        }

        [TestMethod]
        public void ChainingOptionsThisException()
        {
            DataItem<int> tmp = null;
            UnitTestHelper.AssertThrows<ArgumentNullException>(() => tmp.Select(value => new DataItem<int>(value++)));
        }

        [TestMethod]
        public void ChainingOptionsArgumentException()
        {
            DataItem<int> tmp = new DataItem<int>(1);
            UnitTestHelper.AssertThrows<ArgumentNullException>(() => tmp.Select<int, int>(null));
        }

        [TestMethod]
        public void DataItemsCanBeUsedAsDictionaryKeys()
        {
            var empty = new DataItem<int>();
            var value1a = new DataItem<int>(1);
            var value1b = new DataItem<int>(1);
            var dictionary = new Dictionary<DataItem<int>, string>();
            dictionary[empty] = "The empty entry";
            dictionary[value1a] = "The initial 1st value";
            dictionary[value1b] = "The updated 1st value";
            Assert.AreEqual<string>(dictionary[empty], "The empty entry");
            Assert.AreEqual<string>(dictionary[value1a], "The updated 1st value");
            Assert.AreEqual<string>(dictionary[value1b], "The updated 1st value");
        }

        [TestMethod]
        public void DataItemDefaultIsEmptySetting()
        {
            // special types
            Assert.IsTrue(DataItem<Guid>.DefaultIsEmpty);
            // value types
            Assert.IsFalse(DataItem<Int32>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<Int64>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<Double>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<Single>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<Decimal>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<DateTime>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<DateTimeOffset>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<TimeSpan>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<DayOfWeek>.DefaultIsEmpty);
            // other types
            Assert.IsTrue(DataItem<String>.DefaultIsEmpty);
        }

        [TestMethod]
        public void DataItemBuiltInDataPooling()
        {
            Guid guid1 = Guid.NewGuid();
            var value1a = new DataItem<Guid>(guid1);
            var value1b = new DataItem<Guid>(guid1);
            Assert.AreEqual<DataItem<Guid>>(value1a, value1b);
            Assert.IsFalse(Object.ReferenceEquals(value1a, value1b));
            Assert.IsTrue(Object.ReferenceEquals(value1a.PoolValue, value1b.PoolValue));
            DataItem<Guid>.ClearPool();
            var value1c = new DataItem<Guid>(guid1);
            var value1d = new DataItem<Guid>(guid1);
            // pool cleared - new values c,d are not same pool values as a,b
            Assert.IsTrue(Object.ReferenceEquals(value1a.PoolValue, value1b.PoolValue));
            Assert.IsTrue(Object.ReferenceEquals(value1c.PoolValue, value1d.PoolValue));
            Assert.IsFalse(Object.ReferenceEquals(value1c.PoolValue, value1b.PoolValue));
            // repool the old items - now a,b,c,d should all be same pool value
            value1a.Repool();
            value1b.Repool();
            Assert.IsTrue(Object.ReferenceEquals(value1a.PoolValue, value1b.PoolValue));
            Assert.IsTrue(Object.ReferenceEquals(value1c.PoolValue, value1d.PoolValue));
            Assert.IsTrue(Object.ReferenceEquals(value1c.PoolValue, value1b.PoolValue));
        }

        #endregion
    }
}