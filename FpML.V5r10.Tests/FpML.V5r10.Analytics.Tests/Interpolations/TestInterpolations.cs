using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Interpolations;
using Orion.Analytics.LinearAlgebra;
using Orion.Analytics.Maths.Collections;

namespace Orion.Analytics.Tests.Interpolations
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class TestInterpolations
    {
        private static double[] _rates = { 0.07d, 0.071d, 0.072d, 0.071d, 0.072d, 0.073d, 0.075d, 0.074d, 0.076d, 0.075d };
        private static double[] _times = { 0.00d, 0.01d, 0.02d, 0.5d, 0.72d, 1.0d, 2.0d, 3.0d, 5.0d, 10.0d };
        private readonly string[] _linearInterpolations = {"LinearInterpolation",
                                                          "PiecewiseConstantInterpolation",
                                                          "CubicSplineInterpolation",
                                                          "CubicHermiteSplineInterpolation",
                                                          "FlatInterpolation"};

        private readonly string[] _logLinearInterpolations = {
                                                                "LogLinearInterpolation",
                                                                "LinearRateInterpolation",
                                                                "PiecewiseConstantRateInterpolation",
                                                                "PiecewiseConstantZeroRateInterpolation",
                                                                "LogRateCubicSplineInterpolation"
                                                            };

        private static readonly Matrix VMatrix = new Matrix(new[,] 
                                                                       {	// Initiliazation from double[,]
                                                                           { 0.065, 0.065, 0.065, 0.065, 0.065 },
                                                                           { 0.065, 0.065, 0.065, 0.065, 0.065 },
                                                                           { 0.065, 0.065, 0.065, 0.065, 0.065 }
                                                                       });

        private readonly double[] _testPointArray = { 0.1, 0.26, 0.34, 0.4, 0.73, 0.87, 0.93 };

//        private double[] exp;


        /// <summary>
        /// Initialize this test case.
        /// </summary>
        protected static double[] SetUp()
        {
            double[] result = new double[_rates.Length];
            for (int i = 0; i < _rates.Length; i++)
            {
                result[i] = _rates[i]*_times[i];
            }
            var vectorRatesTimes = new DoubleVector(result);
            vectorRatesTimes.Multiply(-1.0d);
            return vectorRatesTimes.Apply(Math.Exp).Vector;
        }

        /// <summary>
        /// Initialize this test case.
        /// </summary>
        protected void TearDown()
        {
            _rates = new[] { 0.07d, 0.071d, 0.072d, 0.071d, 0.072d, 0.073d, 0.075d, 0.074d, 0.076d, 0.075d };
            _times = new[] {0.00d, 0.01d, 0.02d, 0.5d, 0.72d, 1.0d, 2.0d, 3.0d, 5.0d, 10.0d};
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestAllInterpolations()
        {
            Random r = new Random(Environment.TickCount);
            foreach (string interp in _linearInterpolations)
            {
                TearDown();
                var interpolation = InterpolationFactory.Create(interp);
                interpolation.Initialize(_times, _rates);
                Debug.WriteLine($"interpolationType : {interp}");
                for (int i = 1; i < 100; ++i)
                {
                    double time = (i + r.Next(-10000, 10000)/10000)/10.0;
                    double interpRate = interpolation.ValueAt(time, true);
                    Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
                }
                int index = 0;
                foreach (double time in _times)
                {
                    if (time != 0.0)
                    {
                        double interpRate = interpolation.ValueAt(time, true);
                        double interpValue = _rates[index];
                        Assert.AreEqual(interpRate, interpValue, 10 - 8);
                    }
                    index++;
                }
            }
            foreach (string interp in _logLinearInterpolations)
            {
                TearDown();
                double[] exp = SetUp();
                double[] temp = new double[exp.Length];
                exp.CopyTo(temp,0);
                var interpolation = InterpolationFactory.Create(interp);
                interpolation.Initialize(_times, temp);
                Debug.WriteLine($"interpolationType : {interp}");
                for (int i = 1; i < 100; ++i)
                {
                    double time = (i + r.Next(-10000, 10000) / 10000)/10.0;
                    double interpRate = interpolation.ValueAt(time, true);
                    Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
                }
                int index = 0;
                foreach (double time in _times)
                {
                    if (time != 0.0)
                    {
                        double interpRate = interpolation.ValueAt(time, true);
                        double interpValue = exp[index];
                        Assert.AreEqual(interpRate, interpValue, 10 - 8);
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestLinearInterpolation()
        {
            Random r = new Random(Environment.TickCount);
            TearDown();
            var interpolation = new LinearInterpolation();
            interpolation.Initialize(_times, _rates);
            for (int i = 0; i < 10; ++i)
            {
                double time = (i + r.Next(-10000, 10000)/10000);
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestFlatInterpolation()
        {
            Random r = new Random(Environment.TickCount);
            TearDown();
            var interpolation = new FlatInterpolation();
            interpolation.Initialize(_times, _rates);
            for (int i = 0; i < 10; ++i)
            {
                double time = (i + r.Next(-10000, 10000) / 10000);
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestCubicSplineInterpolation()
        {
            Random r = new Random(Environment.TickCount);
            TearDown();
            var interpolation = new CubicSplineInterpolation();
            interpolation.Initialize(_times, _rates);
            for (int i = 0; i < 10; ++i)
            {
                double time = (i + r.Next(-10000, 10000) / 10000);
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestClampedCubicSplineInterpolation()
        {
            Random r = new Random(Environment.TickCount);
            TearDown();
            var interpolation = new CubicSplineInterpolation();
            interpolation.Initialize(_times, _rates);
            for (int i = 0; i < 10; ++i)
            {
                double time = (i + r.Next(-10000, 10000) / 10000);
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestLogLinearInterpolation()
        {
            double[] t = { 0.0, 1.0, 2.0, 3.0, 5.0 };
            double[] y = { 1.0, .98, .95, .93, .88 };
            double[] exp = SetUp();
            Random r = new Random(Environment.TickCount);
            var interpolation = new LogLinearInterpolation();
            interpolation.Initialize(_times, exp);
            for (int i = 0; i < 10; ++i)
            {
                double time = i + r.Next(-10000, 10000) / 10000;
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
            interpolation.Initialize(t, y);
            double[] a = { 2.4, 10.9 };
            double[] b = { 0.941948898488354, 0.724019983080858 };
            double c = 1e-15;
            for (var i = 0; i < a.Length; i++)
            {
                var val = interpolation.ValueAt(a[i], true);
                Assert.AreEqual(b[i], val, c);
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestLinearRateInterpolation()
        {
            TearDown();
            double[] exp = SetUp();
            Random r = new Random(Environment.TickCount);
            var interpolation = new LinearRateInterpolation();
            double[] temp = new double[exp.Length];
            exp.CopyTo(temp,0);
            interpolation.Initialize(_times, temp);
            for (int i = 0; i < 10; ++i)
            {
                double time = i + r.Next(-10000, 10000) / 10000;
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
            int index = 0;
            foreach(double time in _times)
            {
                double interpRate = interpolation.ValueAt(time, true);
                double interpValue = exp[index];
                Assert.AreEqual(interpRate, interpValue);
                index++;
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestPiecewiseConstantInterpolation()
        {
            Random r = new Random(Environment.TickCount);
            TearDown();
            var interpolation = new PiecewiseConstantInterpolation();
            interpolation.Initialize(_times, _rates);
            for (int i = 0; i < 10; ++i)
            {
                double time = i + r.Next(-10000, 10000) / 10000;
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestPiecewiseConstantRateInterpolation()
        {
            Random r = new Random(Environment.TickCount);
            TearDown();
            var interpolation = new PiecewiseConstantRateInterpolation();
            interpolation.Initialize(_times, _rates);
            for (int i = 0; i < 10; ++i)
            {
                double time = i;
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
            int index = 0;
            foreach (double time in _times)
            {
                if(time!=0.0)
                {
                    double interpRate = interpolation.ValueAt(time, true);
                    double interpValue = _rates[index];
                    Assert.AreEqual(interpRate, interpValue, 10-8);
                }
                index++;
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestPiecewiseZeroRateConstantInterpolation()
        {
            TearDown();
            double[] exp = SetUp();
            Random r = new Random(Environment.TickCount);
            var interpolation = new PiecewiseConstantZeroRateInterpolation();
            interpolation.Initialize(_times, exp);
            for (int i = 0; i < 10; ++i)
            {
                double time = i + r.Next(-10000, 10000) / 10000;
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
        }

        /// <summary>
        /// Testing that correct exception is thrown from static methods.
        /// </summary>
        [TestMethod]
        public void TestLogRateCubicSpline()
        {
            TearDown();
            double[] exp = SetUp();
            Random r = new Random(Environment.TickCount);
            var interpolation = new LogRateCubicSplineInterpolation();
            interpolation.Initialize(_times, exp);
            for (int i = 0; i < 10; ++i)
            {
                double time = (i + r.Next(-10000, 10000) / 10000);
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
        }
    }
}