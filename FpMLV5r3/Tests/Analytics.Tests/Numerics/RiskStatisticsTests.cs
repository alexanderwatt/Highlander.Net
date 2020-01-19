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

using System;
using Highlander.Reporting.Analytics.V5r3.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Numerics
{
    /// <summary>
    /// Testing the class <see cref="RiskStatistics"/>.
    /// </summary>
    [TestClass]
    public class RiskStatisticsTests
    {
        // declare private members here used for test setup

        /// <summary>
        /// Initialize this test case.
        /// </summary>
        protected void SetUp()
        {
            _stat.Clear();
            _stat.AddRange(_data);
        }

        private RiskStatistics _stat = new RiskStatistics();

        private static double tol = 1e-9;

        private readonly double[] _data = 
            {
                0.0434315512407002, -0.2474082650833880,  0.4147556160444750,
                -0.4737678272134720,  0.3374959364872000, -0.2410807592120820,
                -0.4428092218556510,  0.3819591556474660,  0.4321946137654170,
                -0.2053712098384270,  0.2606751768079520,  0.3022384308926920,
                0.4949567971300990,  0.1820316147021910, -0.0561850097845635,
                0.4183459058015400, -0.2135321230760190,  0.1123462704131670,
                -0.4359512722844530, -0.2661181056125400, -0.3345882207505550,
                0.2204110087988120, -0.1321555868691760,  0.3113083578186830,
                -0.4292908627159880,  0.1457642662781690, -0.2378138820993390,
                -0.3588090976035450,  0.3471794355893340, -0.1394360771545360
            };

        /// <summary>
        /// Free all you resources.
        /// </summary>
        protected void TearDown()
        {
            _stat = null;
        }

        [TestMethod]
        public void TestCount()
        {
            SetUp();
            Assert.AreEqual(_stat.Count, _data.Length);
        }

        [TestMethod]
        public void TestWeightSum()
        {
            SetUp();
            Assert.AreEqual(_stat.WeightSum, (double)_data.Length, tol);

        }

        //////  The following Tests use Excel reference values  //////

        [TestMethod]
        public void TestMin()
        {
            SetUp();
            Assert.AreEqual(_stat.Min, -0.473767827213472);
        }

        [TestMethod]
        public void TestMax()
        {
            SetUp();
            Assert.AreEqual(_stat.Max, 0.494956797130099);
        }

        [TestMethod]
        public void TestMean()
        {
            SetUp();
            Assert.AreEqual(_stat.Mean, 0.0063592205421387, tol);
        }

        [TestMethod]
        public void TestVariance()
        {
            SetUp();
            Assert.AreEqual(_stat.Variance, 0.0980575618, tol);
        }

        [TestMethod]
        public void TestStandardDeviation()
        {
            SetUp();
            Assert.AreEqual(_stat.StandardDeviation, 0.31314144057, tol);
        }

        //[TestMethod]
        //"This test been excluded from test fixture by Igor Sukhov on 17.10.2007"
        public void TestSkewness()
        {
            SetUp();
            Assert.AreEqual(_stat.Skewness, -0.000650903371128743, tol);
        }

        [TestMethod]
        public void TestKurtosis()
        {
            SetUp();
            Assert.AreEqual(_stat.Kurtosis, -1.406978651266, tol);
        }

        // TODO: implement tests for the remaining mthods and properties


        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestExceptionFromStaticMethod()
        {
            try
            {
                double result = RiskStatistics.ValueAtRisk(
                    0.75, 0.0, 0.1);
            }
            catch (Exception e)
            {
                if (e is ArgumentException)
                    return;
                else
                    throw e;
            }
            Assert.Fail("No exception thrown");
        }
    }
}