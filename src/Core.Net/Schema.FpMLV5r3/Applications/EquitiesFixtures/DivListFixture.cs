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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Equities.Tests
{
    [TestClass]
    public class DivListFixture
    {
        [TestMethod]
        public void DivTest()
        {
            DivList myDiv = new DivList {DivPoints = 3};
            myDiv.MakeArrays();
            myDiv.SetD(0, 1.0, 1.0);
            myDiv.SetD(1, 1.0, 2.0);
            myDiv.SetD(2, 1.0, 3.0);
            //test 1.5 year sum
            double sum = 0.0; 
            double testT = 1.5;
            for (int idx = 0; idx < myDiv.DivPoints; idx++)
            {
                if (myDiv.GetT(idx) < testT)
                    sum += myDiv.GetD(idx);
            }
            Assert.IsTrue(sum > 0.9999999);
            Assert.IsTrue(sum < 1.0000001);
            //test 2.5 year sum
            sum = 0.0;
            testT = 2.5;
            for (int idx = 0; idx < myDiv.DivPoints; idx++)
            {
                if (myDiv.GetT(idx) < testT)
                    sum += myDiv.GetD(idx);
            }
            Assert.IsTrue(sum > 1.9999999);
            Assert.IsTrue(sum < 2.0000001);
            //test 3.5 year sum
            sum = 0.0;
            testT = 3.5;
            for (int idx = 0; idx < myDiv.DivPoints; idx++)
            {
                if (myDiv.GetT(idx) < testT)
                    sum += myDiv.GetD(idx);
            }
            Assert.IsTrue(sum > 2.9999999);
            Assert.IsTrue(sum < 3.0000001);
        }
    }
}
