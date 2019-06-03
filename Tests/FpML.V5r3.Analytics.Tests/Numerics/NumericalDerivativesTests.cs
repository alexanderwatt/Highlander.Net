#region Using directives

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.Differentiation;

#endregion

namespace Orion.Analytics.Tests.Numerics
{
    /// <summary>
    /// Testing the class <see cref="CentredFiniteDifferenceDerivative"/>.
    /// </summary>
    [TestClass]
    public class NumericalDerivativesTests
    {

        [TestMethod]
        public void SinFirstDerivativeAtZeroTest()
        {
            Func<double, double> f = Math.Sin;
            var df = new NumericalDerivative();//CentredFiniteDifferenceDerivative();
            Assert.AreEqual(1, df.EvaluateDerivative(f, 0, 1), 1e-10);
        }

        [TestMethod]
        public void CubicPolynomialThirdDerivativeAtAnyValTest()
        {
            double Func(double x) => 3 * Math.Pow(x, 3) + 2 * x - 6;
            var df = new NumericalDerivative(5, 2);//CentredFiniteDifferenceDerivative(5, 2)
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 0;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 1;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 2;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 3;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 4;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
        }

        [TestMethod]
        public void CubicPolynomialFunctionValueTest()
        {
            double Func(double x) => 3 * Math.Pow(x, 3) + 2 * x - 6;
            var current = Func(2);
            var df = new NumericalDerivative(3, 0);//CentredFiniteDifferenceDerivative(3, 0);
            Assert.AreEqual(38, df.EvaluateDerivative(Func, 2, 1, current), 1e-8);
        }
    }
}