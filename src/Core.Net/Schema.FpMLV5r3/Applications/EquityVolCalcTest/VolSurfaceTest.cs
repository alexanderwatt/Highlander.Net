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
using System.Linq;
using Highlander.Equities;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.EquitiesCalculator.Tests.V5r3
{
    [TestClass]
    public class VolSurfaceTest
    {
        [TestMethod]
        public void TestVolSurface()
        {
            IVolatilitySurface volSurface = new VolatilitySurface("BHP", 4500M, DateTime.Today);
            ForwardExpiry expiry1 = new ForwardExpiry(DateTime.Parse("01-Jan-2010"), 4200);
            ForwardExpiry expiry2 = new ForwardExpiry(DateTime.Parse("01-Jan-2011"), 4400);
            OptionPosition call1 = new OptionPosition("1245", 104, PositionType.Call);
            OptionPosition put1 = new OptionPosition("1246", 1200, PositionType.Put);
            OptionPosition call2 = new OptionPosition("1645", 180, PositionType.Call);
            OptionPosition put2 = new OptionPosition("1646", 1300, PositionType.Put);
            var strike1 = new EquityStrike(4200, call1, put1);
            var strike2 = new EquityStrike(4000, call2, put2);
            IVolatilityPoint point1 = new VolatilityPoint();
            point1.SetVolatility(0.30M, VolatilityState.Default());
            put1.SetVolatility(point1);
            call1.SetVolatility(point1);
            IVolatilityPoint point2 = new VolatilityPoint();
            point2.SetVolatility(0.40M, VolatilityState.Default());
            call2.SetVolatility(point2);
            put2.SetVolatility(point2);
            expiry1.AddStrike(strike1, true);
            expiry2.AddStrike(strike2, false);
            volSurface.AddExpiry(expiry1);
            volSurface.AddExpiry(expiry2);
            List<ForwardExpiry> forwardExpiries =  volSurface.NodalExpiry.ToList();
            int n1 = forwardExpiries[0].Strikes.Count(item => item.NodalPoint);
           // int n2 = forwardExpiries[1].Strikes.Count(delegate(Strike item) { return item.NodalPoint == true; });
            int n2 = 0;
            Assert.AreEqual(1, n1 + n2);
        }
    }
}
