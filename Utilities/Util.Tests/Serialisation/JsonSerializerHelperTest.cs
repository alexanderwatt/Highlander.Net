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

using Highlander.Utilities.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests.Serialisation
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
