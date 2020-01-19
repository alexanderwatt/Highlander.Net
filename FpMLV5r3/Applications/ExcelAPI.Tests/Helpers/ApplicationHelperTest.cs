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

using System.Diagnostics;
using HLV5r3.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Excel.Tests.V5r3.Helpers
{
    [TestClass]
    public class ApplicationHelperTest
    {
        [TestMethod]
        public void DiagnosticsTest()
        {
            object[,] actual = ApplicationHelper.Diagnostics();
            const int expectedItems = 8;
            Assert.AreEqual(expectedItems - 1, actual.GetUpperBound(0));
            Assert.AreEqual(1, actual.GetUpperBound(1));
            for (int i = 0; i < expectedItems - 1; i++)
            {
                Debug.Print("{0}: {1}", actual[i, 0], actual[i, 1]);
                Assert.IsFalse(string.IsNullOrEmpty(actual[i, 0].ToString()));
                Assert.IsFalse(string.IsNullOrEmpty(actual[i, 1].ToString()));
            }
            //When there is a Public Key Token
            Assert.IsFalse(string.IsNullOrEmpty(actual[expectedItems - 1, 1].ToString()));
            //When Public Key Token is empty, this is true.
            //Assert.IsTrue(string.IsNullOrEmpty(actual[expectedItems - 1, 1].ToString()));
        }
    }
}
