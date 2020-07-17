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

namespace Highlander.Core.V1.Tests
{
    /// <summary>
    /// Summary description for DataItemTests
    /// </summary>
    [TestClass]
    public class DataItemTests
    {
        [TestMethod]
        public void TestIntegerStringFormats()
        {
            int intValue = 1001;
            Assert.AreEqual("1,001.000", intValue.ToString("N"));
            Assert.AreEqual("1,001.00", intValue.ToString("N2"));
            Assert.AreEqual("1,001", intValue.ToString("N0"));
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
        public void DataItemsDoNotHaveToContainValues()
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
            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write('a');
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);
            //the read function doesn't always return a result (it returns -1 if a valid character can't be read)
            Func<int> read = reader.Read;
            //we can use an DataItem type to create a better design for this function
            DataItem<char> CharItem()
            {
                var result = read();
                return result == -1 ? new DataItem<char>() : new DataItem<char>((char) result);
            }
            //the first read returns a result with the value 'a'
            Assert.AreEqual('a', CharItem().Value);
            //but the second read returns Empty since it is at the end of the stream
            Assert.IsTrue(CharItem().IsEmpty);
        }

        [TestMethod]
        public void MultipleOperationsWithDataItems()
        {
            //say we have a few functions that may or may not return a value.
            DataItem<int> DivideIfEven(int value) => ((value % 2) == 0) ? new DataItem<int>(value / 2) : new DataItem<int>();
            DataItem<int> SubtractIfDivisibleByThree(int value) => ((value % 3) == 0) ? new DataItem<int>(value - 3) : new DataItem<int>();
            DataItem<int> MultiplyIfOdd(int value) => ((value % 2) != 0) ? new DataItem<int>(value * 2) : new DataItem<int>();
            //we can chain these operations together like this:
            DataItem<int> result =
                DivideIfEven(36)
                    .Select(SubtractIfDivisibleByThree)
                    .Select(MultiplyIfOdd);
            //the result of one carries on to the next to yield the expected result
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(30, result.Value);
        }

        [TestMethod]
        public void MultipleOperationsFailing()
        {
            //say we have a few functions that may or may not return a value.
            DataItem<int> DivideIfEven(int value) => ((value % 2) == 0) ? new DataItem<int>(value / 2) : new DataItem<int>();
            DataItem<int> MultiplyIfOdd(int value) => ((value % 2) != 0) ? new DataItem<int>(value * 2) : new DataItem<int>();
            DataItem<int> SubtractIfDivisibleByThree(int value) => ((value % 3) == 0) ? new DataItem<int>(value - 3) : new DataItem<int>();
            //if one operation in the chain fails...
            DataItem<int> result =
                DivideIfEven(36)
                    .Select(MultiplyIfOdd) //this will fail
                    .Select(SubtractIfDivisibleByThree);
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
            var value1A = new DataItem<int>(1);
            var value1B = new DataItem<int>(1);
            var dictionary = new Dictionary<DataItem<int>, string>
            {
                [empty] = "The empty entry",
                [value1A] = "The initial 1st value",
                [value1B] = "The updated 1st value"
            };
            Assert.AreEqual(dictionary[empty], "The empty entry");
            Assert.AreEqual(dictionary[value1A], "The updated 1st value");
            Assert.AreEqual(dictionary[value1B], "The updated 1st value");
        }

        [TestMethod]
        public void DataItemDefaultIsEmptySetting()
        {
            // special types
            Assert.IsTrue(DataItem<Guid>.DefaultIsEmpty);
            // value types
            Assert.IsFalse(DataItem<int>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<long>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<double>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<float>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<decimal>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<DateTime>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<DateTimeOffset>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<TimeSpan>.DefaultIsEmpty);
            Assert.IsFalse(DataItem<DayOfWeek>.DefaultIsEmpty);
            // other types
            Assert.IsTrue(DataItem<string>.DefaultIsEmpty);
        }

        [TestMethod]
        public void DataItemBuiltInDataPooling()
        {
            Guid guid1 = Guid.NewGuid();
            var value1A = new DataItem<Guid>(guid1);
            var value1B = new DataItem<Guid>(guid1);
            Assert.AreEqual(value1A, value1B);
            Assert.IsFalse(ReferenceEquals(value1A, value1B));
            Assert.IsTrue(ReferenceEquals(value1A.PoolValue, value1B.PoolValue));
            DataItem<Guid>.ClearPool();
            var value1C = new DataItem<Guid>(guid1);
            var value1d = new DataItem<Guid>(guid1);
            // pool cleared - new values c,d are not same pool values as a,b
            Assert.IsTrue(ReferenceEquals(value1A.PoolValue, value1B.PoolValue));
            Assert.IsTrue(ReferenceEquals(value1C.PoolValue, value1d.PoolValue));
            Assert.IsFalse(ReferenceEquals(value1C.PoolValue, value1B.PoolValue));
            // repool the old items - now a,b,c,d should all be same pool value
            value1A.Repool();
            value1B.Repool();
            Assert.IsTrue(ReferenceEquals(value1A.PoolValue, value1B.PoolValue));
            Assert.IsTrue(ReferenceEquals(value1C.PoolValue, value1d.PoolValue));
            Assert.IsTrue(ReferenceEquals(value1C.PoolValue, value1B.PoolValue));
        }

        #endregion
    }
}