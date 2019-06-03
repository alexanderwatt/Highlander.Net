
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolators;
using Orion.TestHelpers;

namespace Orion.Analytics.Tests.Interpolators
{
    /// <summary>
    ///This is a test class for LinearInterpolatorTest and is intended
    ///to contain all LinearInterpolatorTest Unit Tests
    ///</summary>
    [TestClass]
    public class PiecewiseLinearInterpolatorTest
    {


        /// <summary>
        ///A test for Value
        ///</summary>
        [TestMethod]
        public void ValueTest()
        {
            double[] xValues = { 0.00273972602739726,
                                 0.0191780821917808,
                                 0.0383561643835616,
                                 0.0821917808219178,
                                 0.16986301369863,
                                 0.252054794520547,
                                 0.265753424657534,
                                 0.506849315068493,
                                 0.753424657534246,
                                 1.01369863013698,
                                 1.26301369863013, 
                                 1.50410958904109,
                                 1.75068493150684,
                                 2.01369863
                               };

            double[] yValues = { 0.000198610398037118,
                                 0.00144115280642265,
                                 0.00292486360387681,
                                 0.00631290829444216,
                                 0.0129485443009048,
                                 0.0191542839259883,
                                 0.0203498757471952,
                                 0.0391594113610678,
                                 0.0585407323615074,
                                 0.0789041494478555,
                                 0.0982464310004563,
                                 0.116728055717234,
                                 0.135405191240189,
                                 0.154521506325593
                               };


            var plc = new LinearInterpolation(xValues, yValues);
            // Test the actual nodes
            double actual = plc.ValueAt(0.00273972602739726);
            var expected = 0.000198610398037118;
            Assert.AreEqual(expected, actual);

            actual = plc.ValueAt(2.01369863);
            expected = 0.154521506325593;
            Assert.AreEqual(expected, actual);

            actual = plc.ValueAt(1.75068493150684);
            expected = 0.135405191240189;
            Assert.AreEqual(expected, actual);
            
            // Test mid point
            actual = plc.ValueAt(1.882191780822);
            expected = 0.14496334878289;
            AssertExtension.LessOrEqual(Math.Abs(actual - expected), 10E-12);
      

        }

    }
}