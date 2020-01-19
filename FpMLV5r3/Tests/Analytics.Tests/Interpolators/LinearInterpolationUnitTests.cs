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
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.TestHelpers.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Interpolators
{
    /// <summary>
    /// Unit Tests for the class LinearInterpolation.
    /// </summary>
    [TestClass]
    public class LinearInterpolationUnitTests
    {
        /// <summary>
        /// Tests the method CheckDataQuality.
        /// </summary>
        [TestMethod]
        public void TestCheckDataQuality()
        {
            // Set-up two arrays that have a mismatch in their sizes, so that
            // we can check that the constructor detects incorrect arguments.
            double[] xArray = {0, 2.35,-3.79,1.2};
            double[] yArray = {1,0.45,2.4};

            try
            {
#pragma warning disable 168
                LinearInterpolation interpObj =
#pragma warning restore 168
                    new LinearInterpolation(xArray, yArray);   
            }
            catch(Exception exception)
            {
                string ErrorMessage = "ArgumentVectorsSameLength";
                Assert.AreSame(ErrorMessage, exception.Message);
            }           
        }

        /// <summary>
        /// Tests the method Interpolate.
        /// </summary>
        [TestMethod]
        public void TestInterpolate()
        {
            //// Degenerate case of a single point.
            double[] xArray1 = {0.1};
            double[] yArray1 = {-3.45};
            LinearInterpolation interpObj1 = 
                new LinearInterpolation(xArray1, yArray1);
            Assert.AreNotEqual(interpObj1,null);
            double target1 = 10;
            var val = interpObj1.ValueAt(target1, true);
            Assert.AreEqual( val,yArray1[0]);
            // Generic case.
            double[] xArray2 = { 0.0028, 0.0250, 0.0444 }; 
            double[] yArray2 = { 3.3285, 3.3174, 3.3222 };//3.3285
            double[] expectedArray = {3.829899999999995, 3.3174, 3.8060597938144243 };
            LinearInterpolation interpObj2 = 
                new LinearInterpolation(xArray2, yArray2);
            Assert.AreNotEqual(interpObj2, null);
            double target2 = -1; // left end extrapolation
            val = interpObj2.ValueAt(target2, true);
            Assert.AreEqual( val, expectedArray[0]);
            double target3 = 2; // right end extrapolation
            val = interpObj2.ValueAt(target3, true);
            Assert.AreEqual( val, expectedArray[2]);
            double target4 = 0.0250; // nodal point
            val = interpObj2.ValueAt(target4, true);
            Assert.AreEqual(val, expectedArray[1]);
            double target5 = 0.0347;
            val = interpObj2.ValueAt(target5, true);
            double expected = 3.3198;
            double tolerance = 1.0E-10;
            AssertExtension.Less(Math.Abs(val - expected), tolerance);
        }
    }
}