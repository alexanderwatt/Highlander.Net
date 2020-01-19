using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Distributions;
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Tests.Interpolations
{
    /// <summary>
    ///This is a test class for LinearInterpolatorTest and is intended
    ///to contain all LinearInterpolatorTest Unit Tests
    ///</summary>
    [TestClass]
    public class PiecewiseLinearInterpolatorTest
    {
        public static void LessOrEqual(double item1, double item2)
        {
            Assert.IsTrue(item1 <= item2);
        }

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


            var plc = LinearInterpolation.Interpolate(xValues, yValues);

            // Test the actual nodes
            var actual = plc.ValueAt(0.00273972602739726);
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
            LessOrEqual(Math.Abs(actual - expected), 10E-12);     
        }
    }

    [TestClass, Category("Interpolation")]
    public class StepInterpolationTest
    {
        readonly double[] _t = { -2.0, -1.0, 0.0, 1.0, 2.0, 3.0 };
        readonly double[] _y = { 1.0, 2.0, -1.0, 0.0, 1.0, 0.0 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [TestMethod]
        public void FitsAtSamplePoints()
        {
            var ip = new PiecewiseConstantInterpolation(_t, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], ip.ValueAt(_t[i]), "A Exact Point " + i);
            }
        }
    }

    [TestClass, Category("Interpolation")]
    public class LinearRateTest
    {
        readonly double[] _t = { 0.0, 1.0, 2.0, 3.0, 5.0 };
        readonly double[] _y = { 1.0, .98, .95, .93, .88 };

        [TestMethod]
        public void ValueTest1()
        {
            double[] a = { 2.4, 10.9};
            double[] b = { .942, 0.7325 };
            double c = 1e-15;
            for(var i= 0; i < a.Length; i++)
            {
                FitsAtArbitraryPoints(a[i], b[i], c);
            }
        }

        /// <summary>
        /// Verifies that at points other than the provided sample points, the interpolation matches the one computed by Maple as a reference.
        /// </summary>
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        /// <remarks>
        /// Maple:
        /// f := x -> piecewise(x&lt;-1,3+x,x&lt;0,-1-3*x,x&lt;1,-1+x,-1+x);
        /// f(x)
        /// </remarks>
        public void FitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            var ip = new LinearInterpolation(_t, _y);
            Assert.AreEqual(x, ip.ValueAt(t), maxAbsoluteError, "Interpolation at {0}", t);
        }
    }

    [TestClass, Category("Interpolation")]
    public class LinearTest
    {
        readonly double[] _t = { -2.0, -1.0, 0.0, 1.0, 2.0 };
        readonly double[] _y = { 1.0, 2.0, -1.0, 0.0, 1.0 };

        [TestMethod]
        public void ValueTest1()
        {
            double[] a = { -2.4, -0.9, -0.5, -0.1, 0.1, 0.4, 1.2, 10.0, -10.0};
            double[] b = { .6, 1.7, .5, -.7, -.9, -.6, .2, 9.0, -7.0 };
            double c =  1e-15;
            for(var i=0; i < a.Length; i++)
            {
                FitsAtArbitraryPoints(a[i], b[i], c);
            }
        }

        /// <summary>
        /// Verifies that at points other than the provided sample points, the interpolation matches the one computed by Maple as a reference.
        /// </summary>
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        /// <remarks>
        /// Maple:
        /// f := x -> piecewise(x&lt;-1,3+x,x&lt;0,-1-3*x,x&lt;1,-1+x,-1+x);
        /// f(x)
        /// </remarks>
        public void FitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            var ip = new LinearInterpolation(_t, _y);
            Assert.AreEqual(x, ip.ValueAt(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        [TestMethod]
        public void ValueTest2()
        {
            int[] a = { 2, 4, 12};
            foreach (int t in a)
            {
                SupportsLinearCase(t);
            }
        }

        /// <summary>
        /// Verifies that the interpolation supports the linear case appropriately
        /// </summary>
        /// <param name="samples">Samples array.</param>
        public void SupportsLinearCase(int samples)
        {
            double[] x, y, xtest, ytest;
            LinearInterpolationCase.Build(out x, out y, out xtest, out ytest, samples);
            var ip = new LinearInterpolation(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], ip.ValueAt(xtest[i]), 1e-15, "Linear with {0} samples, sample {1}", samples, i);
            }
        }

        [TestMethod]
        public void FewSamples()
        {
            Assert.AreEqual(new LinearInterpolation(new[] { 1.0, 2.0 }, new[] { 2.0, 2.0 }).ValueAt(1.0), 2.0);
        }
    }

    /// <summary>
    /// LinearInterpolation test case.
    /// </summary>
    internal static class LinearInterpolationCase
    {
        /// <summary>
        /// Build linear samples.
        /// </summary>
        /// <param name="x">X sample values.</param>
        /// <param name="y">Y samples values.</param>
        /// <param name="xtest">X test values.</param>
        /// <param name="ytest">Y test values.</param>
        /// <param name="samples">Sample values.</param>
        /// <param name="sampleOffset">Sample offset.</param>
        /// <param name="slope">Slope number.</param>
        /// <param name="intercept">Intercept criteria.</param>
        public static void Build(out double[] x, out double[] y, out double[] xtest, out double[] ytest, int samples = 3, double sampleOffset = -0.5, double slope = 2.0, double intercept = -1.0)
        {
            // Fixed-seed "random" distribution to ensure we always test with the same data
            var uniform = new ContinuousUniformDistribution(0.0, 1.0, new Random(42));
            // build linear samples
            x = new double[samples];
            y = new double[samples];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = i + sampleOffset;
                y[i] = (x[i] * slope) + intercept;
            }
            // build linear test vectors randomly between the sample points
            xtest = new double[samples + 1];
            ytest = new double[samples + 1];
            if (samples == 1)
            {
                // y = const
                xtest[0] = sampleOffset - uniform.Sample();
                xtest[1] = sampleOffset + uniform.Sample();
                ytest[0] = ytest[1] = (sampleOffset * slope) + intercept;
            }
            else
            {
                for (int i = 0; i < xtest.Length; i++)
                {
                    xtest[i] = (i - 1) + sampleOffset + uniform.Sample();
                    ytest[i] = (xtest[i] * slope) + intercept;
                }
            }
        }
    }
}