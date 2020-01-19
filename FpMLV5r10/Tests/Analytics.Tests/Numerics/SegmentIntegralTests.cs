#region Using directives

using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Distributions;
using Orion.Analytics.Integration;
using Orion.Analytics.Solvers;

#endregion

namespace Orion.Analytics.Tests.Numerics
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
            testCases = new ArrayList();
            testCases.Add(new object[]	{	
                                      	    "f(x) = 1", 
                                      	    new UnaryFunction(one),
                                      	    0.0, 1.0, 1.0 
                                      	});
            testCases.Add(new object[] {	
                                           "f(x) = x", 
                                           new UnaryFunction(identity),
                                           0.0, 1.0, 0.5 
                                       });
            testCases.Add(new object[] { 
                                           "f(x) = x^2", 
                                           new UnaryFunction(squared),
                                           0.0, 1.0, 1.0/3.0 
                                       });
            testCases.Add(new object[] {
                                           "f(x) = sin(x)", 
                                           new UnaryFunction(Math.Sin),
                                           0.0, Math.PI, 2.0 
                                       });
            testCases.Add(new object[] {
                                           "f(x) = cos(x)", 
                                           new UnaryFunction(Math.Cos),
                                           0.0, Math.PI, 0.0 
                                       });
            testCases.Add(new object[] {
                                           "f(x) = Gauss(x)", 
                                           new UnaryFunction(NormalDistribution.Function),
                                           -10.0, 10.0, 1.0 
                                       });
        }

        ArrayList testCases;
        const double tolerance = 1e-4;
        readonly SegmentIntegral integrate = new SegmentIntegral(10000);

        private static double one(double x) { return 1.0; }
        private static double identity(double x) { return x; }
        private static double squared(double x) { return x * x; }

        [TestMethod]
        public void TestIntegration()
        {
            TestSetUp();
            foreach (object[] fn in testCases)
            {
                double numerical = integrate.Value(
                    (UnaryFunction)fn[1],
                    (double)fn[2], (double)fn[3]);
                Assert.AreEqual(numerical, (double)fn[4], tolerance, (string)fn[0]);
            }
        }
    }
}