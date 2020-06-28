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

using System;
using System.Diagnostics;
using Highlander.Reporting.Analytics.V5r3.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Options
{
    [TestClass]
    public class BlackScholesMertonModelTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            const bool CallFlag = true;
            const double FwdPrice = 0.04884062563019196;
            const double Strike = 0.0496484493972347;
            const double Vol = 0.194919996223031;
            const double T = 0.00821917808219178;
            
            // First do a run to load stuff, but with different values so that cache can't be used
            BlackScholesMertonModel blackScholesMertonModelPreload
                = new BlackScholesMertonModel(CallFlag, FwdPrice + 0.01, Strike, Vol, T);
            
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            BlackScholesMertonModel blackScholesMertonModel 
                = new BlackScholesMertonModel(CallFlag, FwdPrice, Strike, Vol, T);

            stopwatch.Stop();

            Debug.Print("Time (ms):" + stopwatch.Elapsed.TotalMilliseconds);

            Assert.AreEqual(8.29482796203315E-05, blackScholesMertonModel.Value, 1e-9);
            Assert.AreEqual(0.178921151487557, blackScholesMertonModel.SpotDelta, 1e-9);
            Assert.AreEqual(302.880152685535, blackScholesMertonModel.Gamma, 1e-9);
            Assert.AreEqual(0.00115749210381365, blackScholesMertonModel.Vega, 1e-9);
            Assert.AreEqual(0.013725116687299, blackScholesMertonModel.Theta, 1e-9);
            Assert.AreEqual(-6.81766681810943E-07, blackScholesMertonModel.Rho, 1e-9);
        }

        private const double _fwdPrice = 75;
        private const double _strike = 70;
        private const double _volatility = .35;
        private const double _time = .5;
        private const double _rate = .1;
        private const double _caRRY = .05;

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void BlackScholesCallTest()
        {
            RunBSOptTest(true, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void BlackScholesPutTest()
        {
            RunBSOptTest(false, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void BlackScholesMertonGeneralisedTest()
        {
            RunBSMGeneralisedOptTest();
        }

        public static void RunBSOptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            var result = BlackScholesMertonModel.Greeks(CP, fwdPrice, strike, vol, t);
            Debug.WriteLine(
                $"Premium : {result[0, 0]} Delta : {result[0, 1]} Gamma : {result[0, 2]} Vega : {result[0, 3]} Theta : {result[0, 4]} Rho : {result[0, 5]}");
            Debug.WriteLine(
                $"Premium : {result[1, 0]} Delta : {result[1, 1]} Gamma : {result[1, 2]} Vega : {result[1, 3]} Theta : {result[1, 4]} Rho : {result[1, 5]}");
        }

        public static void RunBSMGeneralisedOptTest()
        {
            var result1 = (object[,])BlackScholesMertonModel.BSMGeneralisedWithGreeks(false, _fwdPrice, _strike, _rate, _caRRY, _volatility, _time);
            Debug.WriteLine(
                $"Premium : {result1[1, 0]} Delta : {result1[1, 1]} Gamma : {result1[1, 2]} Vega : {result1[1, 3]} Theta : {result1[1, 4]} Rho : {result1[1, 5]}");
            Assert.AreEqual((double)result1[1, 0], 4.087d, 0.0001d);
            var result2 = (object[,])BlackScholesMertonModel.BSMGeneralisedWithGreeks(true, -_fwdPrice, -_strike, _rate, _caRRY, -_volatility, _time);
            Debug.WriteLine(
                $"Premium : {result2[1, 0]} Delta : {result2[1, 1]} Gamma : {result2[1, 2]} Vega : {result2[1, 3]} Theta : {result2[1, 4]} Rho : {result2[1, 5]}");
            Assert.AreEqual((double)result2[1, 0], 4.087d, 0.0001d);
        }
    }
}
