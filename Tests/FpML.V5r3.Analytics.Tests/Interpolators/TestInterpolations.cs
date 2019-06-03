
using System;
using System.Diagnostics;
using Orion.Analytics.Interpolations;
using Orion.ModelFramework;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.LinearAlgebra;
using Orion.Analytics.Maths.Collections;

namespace Orion.Analytics.Tests.Interpolators
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class TestInterpolations
    {
        private static double[] _rates = { 0.07d, 0.071d, 0.072d, 0.071d, 0.072d, 0.073d, 0.075d, 0.074d, 0.076d, 0.075d };
        private static readonly double[] Strikes = { 0.7d, 0.8d, 0.9d, 1.0d, 1.1d, 1.2d, 1.3d, 1.4d, 1.5d, 1.6d };
        private static double[] _times = { 0.00d, 0.01d, 0.02d, 0.5d, 0.72d, 1.0d, 2.0d, 3.0d, 5.0d, 10.0d };
        private static readonly double[] Vols = { 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d, 0.5d };
        private readonly string[] _linearInterpolations = {"LinearInterpolation",
                                                          "PiecewiseConstantInterpolation",
                                                          "CubicSplineInterpolation",
                                                          "CubicHermiteSplineInterpolation",
                                                          "PiecewiseConstantRateInterpolation",
                                                          "FlatInterpolation"};

        private readonly string[] _specialInterpolations = { "WingModelInterpolation" };

        private readonly string[] _logLinearInterpolations = {
                                                                "LogLinearInterpolation",
                                                                "LinearRateInterpolation",
                                                                "PiecewiseConstantRateInterpolation",
                                                                "LogRateCubicSplineInterpolation",
                                                                "PiecewiseConstantZeroRateInterpolation"
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
                result[i] = _rates[i] * _times[i];
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
            _times = new[] { 0.00d, 0.01d, 0.02d, 0.5d, 0.72d, 1.0d, 2.0d, 3.0d, 5.0d, 10.0d };
        }

        /// <summary>
        /// Testing the interepolated curve.
        /// </summary>
        [TestMethod]
        public void TestAllInterpolatedCurves()
        {
            foreach (string interp in _linearInterpolations)
            {
                TearDown();
                IInterpolation interpolation = InterpolationFactory.Create(interp);
                DiscreteCurve curve = new DiscreteCurve(_times, _rates);
                IInterpolatedSpace interpCurve = new InterpolatedCurve(curve, interpolation, true);
                Debug.WriteLine($"interpolationType : {interp}");
                foreach (double point in _testPointArray)
                {
                    IPoint p = new Point1D(point);
                    Console.WriteLine(interpCurve.Value(p));
                }
            }
            foreach (string interp in _specialInterpolations)
            {
                TearDown();
                var interpolation = (WingModelInterpolation)InterpolationFactory.Create(interp);
                interpolation.Forward = 1.0;
                interpolation.Spot = 1.0;
                var curve = new DiscreteCurve(Strikes, Vols);
                IInterpolatedSpace interpCurve = new InterpolatedCurve(curve, interpolation, true);
                Debug.WriteLine($"interpolationType : {interp}");
                foreach (double point in _testPointArray)
                {
                    IPoint p = new Point1D(point);
                    Console.WriteLine(interpCurve.Value(p));
                }
            }
            foreach (string interp in _logLinearInterpolations)
            {
                TearDown();
                double[] exp = SetUp();
                IInterpolation interpolation = InterpolationFactory.Create(interp);
                DiscreteCurve curve = new DiscreteCurve(_times, exp);
                IInterpolatedSpace interpCurve = new InterpolatedCurve(curve, interpolation, true);
                Debug.WriteLine($"interpolationType : {interp}");
                foreach (double point in _testPointArray)
                {
                    IPoint p = new Point1D(point);
                    Console.WriteLine(interpCurve.Value(p));
                }
            }
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
                IInterpolation interpolation = InterpolationFactory.Create(interp);
                interpolation.Initialize(_times, _rates);
                Debug.WriteLine($"interpolationType : {interp}");
                for (int i = 1; i < 100; ++i)
                {
                    double time = (i + r.Next(-10000, 10000) / 10000) / 10.0;
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
                exp.CopyTo(temp, 0);
                IInterpolation interpolation = InterpolationFactory.Create(interp);
                interpolation.Initialize(_times, temp);
                Debug.WriteLine($"interpolationType : {interp}");
                for (int i = 1; i < 100; ++i)
                {
                    double time = (i + r.Next(-10000, 10000) / 10000) / 10.0;
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
            IInterpolation interpolation = new LinearInterpolation();
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
        public void TestFlatInterpolation()
        {
            Random r = new Random(Environment.TickCount);
            TearDown();
            IInterpolation interpolation = new FlatInterpolation();
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
            IInterpolation interpolation = new CubicSplineInterpolation();
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
            IInterpolation interpolation = new CubicSplineInterpolation();
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
            double[] exp = SetUp();
            Random r = new Random(Environment.TickCount);
            IInterpolation interpolation = new LogLinearInterpolation();
            //            IList list = new ArrayList(exp.ToGeneralVector().StorageArray);
            interpolation.Initialize(_times, exp);

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
        public void TestLinearRateInterpolation()
        {
            TearDown();
            double[] exp = SetUp();
            Random r = new Random(Environment.TickCount);

            IInterpolation interpolation = new LinearRateInterpolation();
            //            IList list = new ArrayList(exp.ToGeneralVector().StorageArray);
            double[] temp = new double[exp.Length];
            exp.CopyTo(temp, 0);
            interpolation.Initialize(_times, temp);

            for (int i = 0; i < 10; ++i)
            {
                double time = (i + r.Next(-10000, 10000) / 10000);
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
            int index = 0;
            foreach (double time in _times)
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
            IInterpolation interpolation = new PiecewiseConstantInterpolation();
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
        public void TestPiecewiseConstantRateInterpolation()
        {
            Random r = new Random(Environment.TickCount);
            TearDown();
            IInterpolation interpolation = new PiecewiseConstantZeroRateInterpolation();
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
                if (time != 0.0)
                {
                    double interpRate = interpolation.ValueAt(time, true);
                    double interpValue = _rates[index];
                    Assert.AreEqual(interpRate, interpValue, 10 - 8);
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

            IInterpolation interpolation = new PiecewiseConstantZeroRateInterpolation();
            interpolation.Initialize(_times, exp);

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
        public void TestLogRateCubicSpline()
        {
            TearDown();
            double[] exp = SetUp();
            Random r = new Random(Environment.TickCount);
            IInterpolation interpolation = new LogRateCubicSplineInterpolation();
            interpolation.Initialize(_times, exp);

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
        public void TestLogRateClampedCubicSpline()
        {
            TearDown();
            double[] exp = SetUp();
            Random r = new Random(Environment.TickCount);

            var interpolation = new LogRateCubicSplineInterpolation(_times, exp);
            for (int i = 0; i < 10; ++i)
            {
                double time = (i + r.Next(-10000, 10000) / 10000);
                double interpRate = interpolation.ValueAt(time, true);
                Debug.WriteLine($"interpolatedRate : {interpRate} Time: {time}");
            }
        }
    }
}