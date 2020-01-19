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

using System.ComponentModel;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Interpolations
{
    /// <summary>
    /// Unit Tests for the class CubicHermiteSplineInterpolation
    /// </summary>
    [TestClass, Category("Interpolation")]
    public class AkimaSplineTest
    {
        readonly double[] _t = {-2.0, -1.0, 0.0, 1.0, 2.0};
        readonly double[] _y = {1.0, 2.0, -1.0, 0.0, 1.0};

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [TestMethod]
        public void FitsAtSamplePoints()
        {
            var it = CubicSplineInterpolation.InterpolateAkima(_t, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.ValueAt(_t[i]), "A Exact Point " + i);
            }
        }

        [TestMethod]
        public void ValueTest1()
        {
            double[] a = {-2.4, -0.9, -0.5, -0.1, 0.1, 0.4, 1.2, 10.0, -10.0};
            double[] b = {-0.52, 1.826, 0.25, -1.006, -0.9, -.6, .2, 9.0, -151};
            double c = 1e-15;
            for (var i = 0; i < a.Length; i++)
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
        public void FitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            var it = CubicSplineInterpolation.InterpolateAkima(_t, _y);
            Assert.AreEqual(x, it.ValueAt(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        [TestMethod]
        public void FewSamples()
        {
            Assert.AreEqual(
                CubicSplineInterpolation.InterpolateAkima(new[] {1.0, 2.0, 3.0, 4.0, 5.0}, new[] {2.0, 2.0, 2.0, 2.0, 2.0})
                    .ValueAt(1.0), 2.0);
        }
    }

    [TestClass, Category("Interpolation")]
    public class CubicSplineTest
    {
        readonly double[] _t = {-2.0, -1.0, 0.0, 1.0, 2.0};
        readonly double[] _y = {1.0, 2.0, -1.0, 0.0, 1.0};

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [TestMethod]
        public void NaturalFitsAtSamplePoints()
        {
            var it = CubicSplineInterpolation.InterpolateNatural(_t, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.ValueAt(_t[i]), "A Exact Point " + i);
            }
        }

        [TestMethod]
        public void ValueTest()
        {
            double[] a = { -2.4, -0.9, -0.5, -0.1, 0.1, 0.4, 1.2, 10.0};
            double[] b = { .144, 1.7906428571428571429, .47321428571428571431, -.80992857142857142857,
                -1.1089285714285714286, -1.0285714285714285714, .30285714285714285716, 189 };
            double c = 1e-15;
            for (var i = 0; i < a.Length; i++)
            {
                NaturalFitsAtArbitraryPoints(a[i], b[i], c);
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
        /// with(CurveFitting);
        /// evalf(subs({x=-2.4},Spline([[-2,1],[-1,2],[0,-1],[1,0],[2,1]], x, degree=3, endpoints='natural')),20);
        /// </remarks>
        public void NaturalFitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            var it = CubicSplineInterpolation.InterpolateNatural(_t, _y);
            Assert.AreEqual(x, it.ValueAt(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [TestMethod]
        public void FixedFirstDerivativeFitsAtSamplePoints()
        {
            var it = CubicSplineInterpolation.InterpolateBoundaries(_t, _y, SplineBoundaryCondition.FirstDerivative, 1.0,
                SplineBoundaryCondition.FirstDerivative, -1.0);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.ValueAt(_t[i]), "A Exact Point " + i);
            }
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [TestMethod]
        public void FixedSecondDerivativeFitsAtSamplePoints()
        {
            var it = CubicSplineInterpolation.InterpolateBoundaries(_t, _y, SplineBoundaryCondition.SecondDerivative,
                -5.0, SplineBoundaryCondition.SecondDerivative, -1.0);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.ValueAt(_t[i]), "A Exact Point " + i);
            }
        }

        [TestMethod]
        public void ValueTest4()
        {
            double[] a = { -2.4, -0.9, -0.5, -0.1, 0.1, 0.4, 1.2};
            double[] b = { -.8999999999999999993, 1.7590357142857142857, .41517857142857142854, -.82010714285714285714,
                -1.1026071428571428572, -1.0211428571428571429, .31771428571428571421};
            double c = 1e-15;
            for (var i = 0; i < a.Length; i++)
            {
                FixedSecondDerivativeFitsAtArbitraryPoints(a[i], b[i], c);
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
        /// with(CurveFitting);
        /// evalf(subs({x=-2.4},Spline([[-2,1],[-1,2],[0,-1],[1,0],[2,1]], x, degree=3, endpoints=Matrix(2,13,{(1,3)=1,(1,13)=-5,(2,10)=1,(2,13)=-1}))),20);
        /// </remarks>
        public void FixedSecondDerivativeFitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            var it = CubicSplineInterpolation.InterpolateBoundaries(_t, _y, SplineBoundaryCondition.SecondDerivative,
                -5.0, SplineBoundaryCondition.SecondDerivative, -1.0);
            Assert.AreEqual(x, it.ValueAt(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        //[TestMethod]
        //public void ValueTest2()
        //{
        //    int[] a = { 2, 4, 12 };
        //    foreach (int t in a)
        //    {
        //        NaturalSupportsLinearCase(t);
        //    }
        //}

        ///// <summary>
        ///// Verifies that the interpolation supports the linear case appropriately
        ///// </summary>
        ///// <param name="samples">Samples array.</param>
        //public void NaturalSupportsLinearCase(int samples)
        //{
        //    double[] x, y, xtest, ytest;
        //    LinearInterpolationCase.Build(out x, out y, out xtest, out ytest, samples);
        //    var it = CubicSpline.InterpolateNatural(x, y);
        //    for (int i = 0; i < xtest.Length; i++)
        //    {
        //        Assert.AreEqual(ytest[i], it.ValueAt(xtest[i]), 1e-15, "Linear with {0} samples, sample {1}",
        //            samples, i);
        //    }
        //}

        [TestMethod]
        public void FewSamples()
        {
            Assert.AreEqual(CubicSplineInterpolation.InterpolateNatural(new[] {1.0, 2.0}, new[] {2.0, 2.0}).ValueAt(1.0),
                2.0);
        }
    }
}