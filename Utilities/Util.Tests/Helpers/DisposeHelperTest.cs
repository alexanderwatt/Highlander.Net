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

using System.IO;
using Highlander.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests.Helpers
{
    /// <summary>
    ///This is a test class for DisposeHelperTest and is intended
    ///to contain all DisposeHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class DisposeHelperTest
    {
        [TestMethod]
        public void SafeDisposeTest()
        {
            StreamReader a = new StreamReader(@"C:\windows\explorer.exe");
            DisposeHelper.SafeDispose(ref a);
            Assert.AreEqual(null, a);
        }
    }
}