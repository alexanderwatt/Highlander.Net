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
    public class ZeroCurveFixture
    {
        [TestMethod]
        public void FlatRates()
        {
            ZeroCurve myZero = new ZeroCurve {RatePoints = 2};
            myZero.MakeArrays();
            myZero.SetR(0, 0.05, 1.0);
            myZero.SetR(1, 0.05, 2.0);
            //test interpolation prior to point 0
            double tTime = 0.5;
            double val = myZero.LinInterp(tTime);
            Assert.IsTrue(val > 0.0499999999);
            Assert.IsTrue(val < 0.050000001);
            //test interpolation prior to point 1
            tTime = 1.5;
            val = myZero.LinInterp(tTime);
            Assert.IsTrue(val > 0.0499999999);
            Assert.IsTrue(val < 0.050000001);
            //test interpolation post  point 1
            tTime = 2.5;
            val = myZero.LinInterp(tTime);
            Assert.IsTrue(val > 0.0499999999);
            Assert.IsTrue(val < 0.050000001);
            //test forward rates
            //test interpolation prior to point 0
            double tl = 0.25;
            tTime = 0.5;
            val = myZero.ForwardRate(tl,tTime);
            Assert.IsTrue(val > 0.0499999999);
            Assert.IsTrue(val < 0.050000001);
            //test interpolation prior to point 1
            tTime = 1.5;
            val = myZero.ForwardRate(tl, tTime);
            Assert.IsTrue(val > 0.0499999999);
            Assert.IsTrue(val < 0.050000001);
            //test interpolation post  point 1
            tTime = 2.5;
            val = myZero.ForwardRate(tl, tTime);
            Assert.IsTrue(val > 0.0499999999);
            Assert.IsTrue(val < 0.050000001);
        }

        [TestMethod]
        public void NonFlatRates()
        {
            ZeroCurve myZero = new ZeroCurve {RatePoints = 2};
            myZero.MakeArrays();
            myZero.SetR(0, 0.05, 1.0);
            myZero.SetR(1, 0.075, 2.0);
            //test interpolation prior to point 0
            double tTime = 0.5;
            double val = myZero.LinInterp(tTime);
            Assert.IsTrue(val > 0.0499999999);
            Assert.IsTrue(val < 0.050000001);
            //test interpolation prior to point 1
            tTime = 1.5;
            val = myZero.LinInterp(tTime);
            Assert.IsTrue(val > 0.062499999999);
            Assert.IsTrue(val < 0.06250000001);
            //test interpolation post  point 1
            tTime = 2.5;
            val = myZero.LinInterp(tTime);
            Assert.IsTrue(val > 0.07499999999);
            Assert.IsTrue(val < 0.0750000001);
            //test forward rates
            //test interpolation priot to point 0
            double tl = 0.25;
            tTime = 0.5;
            val = myZero.ForwardRate(tl, tTime);
            Assert.IsTrue(val > 0.0499999999);
            Assert.IsTrue(val < 0.050000001);
            //test interpolation prior to point 1
            tTime = 1.5;
            val = myZero.ForwardRate(tl, tTime);
            Assert.IsTrue(val > 0.06499999);   /// 0.065
            Assert.IsTrue(val < 0.065000001);
            //test interpolation post  point 1
            tTime = 2.5;
            val = myZero.ForwardRate(tl, tTime); ///0.077778
            Assert.IsTrue(val > 0.0777777);
            Assert.IsTrue(val < 0.07777778);
        }
    }
}
