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

using System.Reflection;
using Highlander.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests.Helpers
{
    /// <summary>
    ///This is a test class for ReflectionHelperTest and is intended
    ///to contain all ReflectionHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class ReflectionHelperTest
    {
        /// <summary>
        ///A test for GetAssemblyCodeBaseLocation
        ///</summary>
        [TestMethod]
        public void GetAssemblyCodeBaseLocationTest()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string actual = ReflectionHelper.GetAssemblyCodeBaseLocation(assembly);

            Assert.IsTrue(!string.IsNullOrEmpty(actual));
        }

        /// <summary>
        ///A test for ChangeType
        ///</summary>
        [TestMethod]
        public void ChangeTypeTest()
        {
            object actual = ReflectionHelper.ChangeType(3, typeof(double));
            Assert.AreEqual(3d, actual);

            actual = ReflectionHelper.ChangeType('f', typeof(string));
            Assert.AreEqual("f", actual);

            const EnumHelperTest.TestEnum value = EnumHelperTest.TestEnum.Value2;
            actual = ReflectionHelper.ChangeType(value, typeof(decimal));
            Assert.AreEqual((decimal)value, actual);

            const decimal value2 = (decimal)EnumHelperTest.TestEnum.Value2;
            actual = ReflectionHelper.ChangeType(value2, typeof(EnumHelperTest.TestEnum));
            Assert.AreEqual((EnumHelperTest.TestEnum)value2, actual);
        }
    }
}