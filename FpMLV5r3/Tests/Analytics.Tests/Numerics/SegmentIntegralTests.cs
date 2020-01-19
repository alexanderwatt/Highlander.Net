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
using System.Collections;
using Highlander.Reporting.Analytics.V5r3.Distributions;
using Highlander.Reporting.Analytics.V5r3.Integration;
using Highlander.Reporting.Analytics.V5r3.Solvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Numerics
{
    /// <summary>
    /// Testing the class <see cref="SegmentIntegral"/>.
    /// </summary>
    [TestClass]
    public class SegmentIntegralTests
    {
        /// <summary>
        /// Testing <see cref="SegmentIntegral"/> on few values.
        /// </summary>

        [TestMethod]
        public void TestSetUp()
        {
            _testCases = new ArrayList();
            _testCases.Add(new object[]	{	
                                      	    "f(x) = 1", 
                                      	    new UnaryFunction(One),
                                      	    0.0, 1.0, 1.0 
                                      	});
            _testCases.Add(new object[] {	
                                           "f(x) = x", 
                                           new UnaryFunction(Identity),
                                           0.0, 1.0, 0.5 
                                       });
            _testCases.Add(new object[] { 
                                           "f(x) = x^2", 
                                           new UnaryFunction(Squared),
                                           0.0, 1.0, 1.0/3.0 
                                       });
            _testCases.Add(new object[] {
                                           "f(x) = sin(x)", 
                                           new UnaryFunction(Math.Sin),
                                           0.0, Math.PI, 2.0 
                                       });
            _testCases.Add(new object[] {
                                           "f(x) = cos(x)", 
                                           new UnaryFunction(Math.Cos),
                                           0.0, Math.PI, 0.0 
                                       });
            _testCases.Add(new object[] {
                                           "f(x) = Gauss(x)", 
                                           new UnaryFunction(NormalDistribution.Function),
                                           -10.0, 10.0, 1.0 
                                       });
        }

        ArrayList _testCases;
        const double Tolerance = 1e-4;
        readonly SegmentIntegral _integrate = new SegmentIntegral(10000);

        private static double One(double x) { return 1.0; }
        private static double Identity(double x) { return x; }
        private static double Squared(double x) { return x * x; }

        [TestMethod]
        public void TestIntegration()
        {
            TestSetUp();
            foreach (object[] fn in _testCases)
            {
                double numerical = _integrate.Value(
                    (UnaryFunction)fn[1],
                    (double)fn[2], (double)fn[3]);
                Assert.AreEqual(numerical, (double)fn[4], Tolerance, (string)fn[0]);
            }
        }
    }
}