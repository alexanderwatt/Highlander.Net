#region Using directives

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Distributions;
using Orion.Analytics.Maths.Collections;

#endregion

namespace Orion.Analytics.Tests.Numerics
{
    /// <summary>
    /// Testing the class <see cref="CumulativeNormalDistribution"/>.
    /// </summary>
    [TestClass]
    public class NormalDistributionTestingSuite
    {
        // declare private members here used for test setup

        /// <summary>
        /// Initialize this test case.
        /// </summary>

        private const double mean = 0.0;
        private const double sigma = 1.0;
        private const double xMin = mean - 4 * sigma;
        private const double xMax = mean + 4 * sigma;
        private const int N = 10001;
        private const double h = (xMax - xMin) / (N - 1);

        private NormalDistribution normal =
            new NormalDistribution(mean, sigma);
        private CumulativeNormalDistribution cum =
            new CumulativeNormalDistribution(mean, sigma);
        private InvCumulativeNormalDistribution invCum =
            new InvCumulativeNormalDistribution(mean, sigma);

        private double gaussian(double x)
        {
            double normFact = sigma * Math.Sqrt(2 * Math.PI);
            double dx = x - mean;
            return Math.Exp(-dx * dx / (2.0 * sigma * sigma)) / normFact;
        }

        private double gaussianDerivative(double x)
        {
            double normFact = sigma * sigma * sigma * Math.Sqrt(2 * Math.PI);
            double dx = x - mean;
            return -dx * Math.Exp(-dx * dx / (2.0 * sigma * sigma)) / normFact;
        }

        private double norm(DoubleVector f, double h)
        {
            double sum = 0.0;
            foreach (double d in f)
                sum += d * d;
            // numeric integral of f^2
            double res = h * (sum - 0.5 * f[0] * f[0] -
                              0.5 * f[f.Count - 1] * f[f.Count - 1]);
            return Math.Sqrt(res);
        }

        /// <summary>
        /// Testing <see cref="CumulativeNormalDistribution.ValueOf"/> on few values.
        /// </summary>
        [TestMethod]
        public void SmallSampleCheck()
        {
            CumulativeNormalDistribution cnd = new CumulativeNormalDistribution();

            Assert.AreEqual(0.5, cnd.ValueOf(0.0), 1e-6);
            Assert.AreEqual(1.0, cnd.ValueOf(double.PositiveInfinity));
            Assert.AreEqual(0.0, cnd.ValueOf(double.NegativeInfinity));

            // values obtained with Maple 9.01
            Assert.AreEqual(.6914624613, cnd.ValueOf(0.5), 1e-6);
            Assert.AreEqual(.3085375387, cnd.ValueOf(-0.5), 1e-6);
            Assert.AreEqual(.9331927987, cnd.ValueOf(1.5), 1e-6);
            Assert.AreEqual(.06680720127, cnd.ValueOf(-1.5), 1e-6);

            cnd = new CumulativeNormalDistribution(1.0, 2.0);
            Assert.AreEqual(0.0, cnd.ValueOf(1.0), 0.5);
            Assert.AreEqual(1.0, cnd.ValueOf(double.PositiveInfinity));
            Assert.AreEqual(0.0, cnd.ValueOf(double.NegativeInfinity));

            // values obtained with Maple 9.01
            Assert.AreEqual(.4012936743, cnd.ValueOf(0.5), 1e-6);
            Assert.AreEqual(.2266273524, cnd.ValueOf(-0.5), 1e-6);
            Assert.AreEqual(.5987063257, cnd.ValueOf(1.5), 1e-6);
            Assert.AreEqual(.1056497737, cnd.ValueOf(-1.5), 1e-6);
        }

        ///// <summary>
        ///// Testing <see cref="Testing the cummulativenormal"/> on few values.
        ///// </summary>
        ///// 
        //[TestMethod]
        //public void TestGaussian()
        //{
        //    DoubleVector x = new DoubleVector(N, xMin, h);
        //    DoubleVector y = x.Apply(new UnaryFunction(this.gaussian));
        //    DoubleVector yDerivative = x.Apply(
        //        new UnaryFunction(this.gaussianDerivative));
        //    DoubleVector yIntegrated = x.Apply(cum);


        //    NormalDistribution normal =
        //        new NormalDistribution(0.0, 1.0);
        //    DoubleVector yTemp = x.Apply(normal);
        //    Blas.Default.Add(yTemp, -1.0, y);
        //    Assert.AreEqual(norm(yTemp, h), 0.0, 1.0e-10);
        //}

        //[TestMethod]
        //public void TestGaussianDelegate()
        //    /// <summary>
        //    /// Testing <see cref="Testing the cummulativenormal"/> of the normal.
        //    /// </summary>
        //{
        //    normal = new NormalDistribution();
        //    {
        //        DoubleVector x = new DoubleVector(N, xMin, h);
        //        foreach (double xi in x)
        //            Assert.AreEqual(NormalDistribution.Function(xi), normal.Value(xi), 1.0e-16);
        //    }
        //}

        //[TestMethod]
        //public void TestGaussianDerivative()
        //    /// <summary>
        //    /// Testing <see cref="Testing the cummulativenormal"/> of the gaussian derivative.
        //    /// </summary>
        //{
        //    DoubleVector x = new DoubleVector(N, xMin, h);
        //    DoubleVector y = x.Apply(new UnaryFunction(this.gaussian));
        //    DoubleVector yDerivative = x.Apply(
        //        new UnaryFunction(this.gaussianDerivative));
        //    DoubleVector yIntegrated = x.Apply(cum);
        //    DoubleVector yTemp2 = x.Apply(
        //        new UnaryFunction(normal.Derivative));
        //    Blas.Default.Add(yTemp2, -1.0, yDerivative);
        //    Assert.AreEqual(norm(yTemp2, h), 0.0, 1.0e-16);
        //}

        //[TestMethod]
        //public void TestCummulative()
        //    /// <summary>
        //    /// Testing <see cref="Testing the cummulativenormal"/> of the cummulative derivative.
        //    /// </summary>
        //{
        //    DoubleVector x = new DoubleVector(N, xMin, h);
        //    DoubleVector y = x.Apply(new UnaryFunction(this.gaussian));
        //    DoubleVector yDerivative = x.Apply(
        //        new UnaryFunction(this.gaussianDerivative));
        //    DoubleVector yIntegrated = x.Apply(cum);
        //    DoubleVector yTemp3 = x.Apply(
        //        new UnaryFunction(cum.Derivative));
        //    Blas.Default.Add(yTemp3, -1.0, y);
        //    Assert.AreEqual(norm(yTemp3, h), 0.0, 1.0e-16);
        //}

        //[TestMethod]
        //public void TestCummulativeDlegate()
        //    /// <summary>
        //    /// Testing <see cref="Testing the cummulativenormal"/> of the cummulative delegate.
        //    /// </summary>
        //{
        //    DoubleVector x = new DoubleVector(N, xMin, h);
        //    foreach (double xi in x)
        //        Assert.AreEqual(CumulativeNormalDistribution.Function(xi), cum.Value(xi), 1.0e-16);
        //}

        //[TestMethod]
        //public void TestInverseCumulative()
        //    /// <summary>
        //    /// Testing <see cref="Testing the inverse cummulativenormal"/> of the inverse cummulative delegate.
        //    /// </summary>
        //{
        //    DoubleVector x = new DoubleVector(N, xMin, h);
        //    DoubleVector yIntegrated = x.Apply(cum);
        //    DoubleVector yTemp = yIntegrated.Apply(invCum);
        //    Blas.Default.Add(yTemp, -1.0, x);
        //    Assert.AreEqual(norm(yTemp, h), 0.0, 1.0e-4);
        //}

        //[TestMethod]
        //public void TestInverseCumulative2()
        //    /// <summary>
        //    /// Testing <see cref="Testing the inverse cummulativenormal"/> of the inverse cummulative2 delegate.
        //    /// </summary>
        //{
        //    DoubleVector x = new DoubleVector(N, xMin, h);
        //    DoubleVector yIntegrated = x.Apply(cum);
        //    DoubleVector yTemp = yIntegrated.Apply(invCum2);
        //    Blas.Default.Add(yTemp, -1.0, x);
        //    Assert.AreEqual(norm(yTemp, h), 0.0, 1.0e-3);
        //}
    }
}