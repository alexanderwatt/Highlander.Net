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
    ///This is a test class for PairTest and is intended
    ///to contain all PairTest Unit Tests
    ///</summary>
    [TestClass]
    public class PairTest
    {
        /// <summary>
        ///A test for Pair Constructor
        ///</summary>
        private static void PairConstructorTestHelper<T1, T2>(T1 default1, T2 default2)
        {
            T1 first = default1;
            T2 second = default2;
            Pair<T1, T2> target = new Pair<T1, T2>(first, second);
            Assert.IsNotNull(target);
            Assert.AreEqual(default1, target.First);
            Assert.AreEqual(default2, target.Second);
        }

        [TestMethod]
        public void PairConstructorTest()
        {
            PairConstructorTestHelper("value1", 1);
            PairConstructorTestHelper('d', 2.1);
            PairConstructorTestHelper(2.2m, DateTime.Today);
        }
    }
}