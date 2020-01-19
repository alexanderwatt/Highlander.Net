/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Numerics
{
    [TestClass]
    public class IntDoubleParseTests
    {                                                                                                                                   
        [TestMethod]                                                                  
        public void TestIntParse()                                         
        {
            string sInt = "2323";
            int i = int.Parse(sInt);
            Assert.AreEqual(i, 2323);
        }

        [TestMethod]                                                                  
        public void TestDoubleParse1()                                         
        {
            string sDouble = "2323.3434";
            double d = double.Parse(sDouble);
            Assert.AreEqual(d, 2323.3434);
        }
    }
}