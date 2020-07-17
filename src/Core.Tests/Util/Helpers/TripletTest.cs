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
    ///This is a test class for TripletTest and is intended
    ///to contain all TripletTest Unit Tests
    ///</summary>
    [TestClass]
    public class TripletTest
    {
        /// <summary>
        ///A test for Triplet`3 Constructor
        ///</summary>
        private static void TripletConstructorTestHelper<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            T1 first = value1;
            T2 second = value2;
            T3 third = value3;
            Triplet<T1, T2, T3> target = new Triplet<T1, T2, T3>(first, second, third);
            Assert.IsNotNull(target);
            Assert.AreEqual(value1, target.First);
            Assert.AreEqual(value2, target.Second);
            Assert.AreEqual(value3, target.Third);
        }

        [TestMethod]
        public void TripletConstructorTest()
        {
            TripletConstructorTestHelper("a", 3, 2.3);
            TripletConstructorTestHelper(DateTime.Today, 3m, 'c');
        }
    }
}