using System;
using Orion.Analytics.Interpolators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Utilities;

namespace Orion.Analytics.Tests.SABRModel
{
    [TestClass]
    public class SABRInterpolationTest
    {
        private double[] _linXArray;
        private double[] _linYArray;

        private decimal[] _linInterpTarget;
        private decimal[] _linExpected;

        private double[] _chsXArray;
        private double[] _chsYArray;

        private decimal[] _chsTarget;
        private decimal[] _chsExpected;

        private double[] _bilinColumns;
        private double[] _bilinRows;
        private double[,] _bilinData;

        private double[] _bilinColumnTarget;
        private double[] _bilinRowTarget;
        private double[] _bilinExpected;

        [TestInitialize]
        public void Initialize()
        {
            #region Linear Interpolation Setup

            _linXArray = new []
                             {
                                     1.0 / 360, 9.0 / 360, 16.0 / 360.0, 23.0 / 360.0, 34.0 / 360.0, 64.0 / 360.0,
                                     94.0 / 360.0, 125.0 / 360.0, 155.0 / 360.0, 188.0 / 360.0, 276.0 / 360.0, 367.0 / 360.0,
                                     462.0 / 360.0, 552.0 / 360.0, 643.0 / 360.0, 734.0 / 360.0, 1098.0 / 360.0,
                                     1463.0 / 360.0, 1828.0 / 360.0, 2561.0 / 360.0, 3654.0 / 360.0
                             };

            _linYArray
                = new []
                      {
                          3.3285, 3.3174, 3.3222, 3.3271, 3.3346, 3.4520, 3.5272, 3.5971, 3.6607, 3.7300,
                          3.8371, 3.8802, 3.9112, 3.9405, 3.9809, 4.0213, 4.0805, 4.1257, 4.1820, 4.2684, 4.4365
                      };

            _linInterpTarget = new[]
                                   {
                                       1.0m / 360.0m, 643.0m / 360.0m, 3654.0m / 360.0m, 360.0m / 360.0m, 3960.0m / 360.0m,
                                       87.0m / 360.0m, 451.0m / 360.0m, 1183.0m / 360.0m, 7200.0m / 360.0m
                                   };

            _linExpected = new[] { 3.3285m, 3.9809m, 4.4365m, 3.8769m, 4.4836m, 3.5097m, 3.9076m, 4.0910m, 4.9819m };

            #endregion

            #region Cubic Hermite Spline Interpolation Setup

            _chsXArray = new double[] { 274, 365, 732, 1097, 1462, 1828, 2556, 3654 };

            _chsYArray = new[] { 9.29, 9.46, 10.34, 10.75, 10.84, 10.93, 10.95, 10.99 };

            _chsTarget = new decimal[]
                             {
                                 365, 456, 547, 641, 732, 820, 914, 1005, 1097, 1187, 1278, 1370, 1462, 1553,
                                 1644, 1736, 1828, 1918, 2009, 2101, 2192, 2283, 2374, 2465, 2556, 2647, 2738,
                                 2832, 2923, 3014, 3105, 3197, 3289, 3379, 3470, 3562, 3654
                             };

            _chsExpected = new[]
                               {
                                   9.46m, 9.68m, 9.91m, 10.15m, 10.34m, 10.48m, 10.59m, 10.68m, 10.75m, 10.79m,
                                   10.81m, 10.82m, 10.84m, 10.86m, 10.89m, 10.91m, 10.93m, 10.94m, 10.95m, 10.95m,
                                   10.95m, 10.95m, 10.95m, 10.95m, 10.95m, 10.95m, 10.96m, 10.96m, 10.96m, 10.97m,
                                   10.97m, 10.97m, 10.98m, 10.98m, 10.98m, 10.99m, 10.99m
                               };

            #endregion

            #region Bilinear Interpolation Setup

            _bilinColumns = new [] { 0.85, 0.90, 1 };
            _bilinRows = new[] { 0.5, 1.2 };
            _bilinData = new[,] { { 0.4150, 0.3, 0.425 }, { 0.2025, 0.2135, 0.228 } };
            _bilinColumnTarget = new[] { 0.85, 0.90, 1, 0.85, 0.90, 1, 0.7, 1.4, 0.9, 0.9, 0.875, 0.851 };
            _bilinRowTarget = new[] { 0.67, 0.27, 0.38, 0.33, 0.01, 0.76, 0.78, 0.03, 0.35, 0.25, 0.56, 0.06 };
            _bilinExpected = new[] { 0.3634, 0.3000, 0.4250, 0.4150, 0.3000, 0.3518, 0.3300, 0.4250, 0.3000, 0.3000, 0.3447, 0.4127 };

            #endregion
        }

        /// <summary>
        /// Test Linear Innterpolation
        /// Check when xArrays are null/zero length
        /// </summary>
        [TestMethod]
        public void Test01LinearInterpolateTest()
        {
            double expected = 0;
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            double actual = siif.LinearInterpolate(null, _linYArray, 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test Linear Innterpolation
        /// Check when yArrays are null/zero length
        /// </summary>
        [TestMethod]
        public void Test02LinearInterpolateTest()
        {
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            double actual = siif.LinearInterpolate(_linXArray, null, 0);
            Assert.AreEqual(0d, actual);
        }

        /// <summary>
        /// Test Linear Innterpolation
        /// Test a range of values
        /// </summary>
        [TestMethod]
        public void Test03LinearInterpolateTest()
        {
            double[] target = ArrayUtilities.DecimalArrayToDouble(_linInterpTarget);
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            for (int i = 0; i < target.Length; i++)
            {
                double actual = siif.LinearInterpolate(_linXArray, _linYArray, target[i]);
                Assert.AreEqual(_linExpected[i], (decimal)Math.Round(actual, 4));
            }
        }

        /// <summary>
        /// Test Cubic Hermite Spline Innterpolation
        /// Check when xArrays are null/zero length
        /// </summary>
        [TestMethod]
        public void Test04CubicHermiteSplineInterpolateTest()
        {
            double expected = 0;
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            double actual = siif.CubicHermiteSplineInterpolate(null, _chsYArray, 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test Cubic Hermite Spline Innterpolation
        /// Check when yArrays are null/zero length
        /// </summary>
        [TestMethod]
        public void Test05CubicHermiteSplineInterpolateTest()
        {
            double expected = 0;
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            double actual = siif.CubicHermiteSplineInterpolate(_chsXArray, null, 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test Cubic Hermite Spline Innterpolation
        /// Check when the target is too large
        /// </summary>
        [TestMethod]
        public void Test06CubicHermiteSplineInterpolateTest()
        {
            decimal expected = 0;
            double target = 10000;
            string expectedFail = "Cubic Hermite Spline does not support extrapolation";
            try
            {
                SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
                double actual = siif.CubicHermiteSplineInterpolate(_chsXArray, _chsYArray, target);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expectedFail, ex.Message);
            }
        }

        /// <summary>
        /// Test Cubic Hermite Spline Innterpolation
        /// Test a range of values
        /// </summary>
        [TestMethod]
        public void Test07CubicHermiteSplineInterpolateTest()
        {
            double[] target = ArrayUtilities.DecimalArrayToDouble(_chsTarget);
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            for (int i = 0; i < target.Length; i++)
            {
                double actual = siif.CubicHermiteSplineInterpolate(_chsXArray, _chsYArray, target[i]);
                Assert.AreEqual(_chsExpected[i], (decimal)Math.Round(actual, 2));
            }
        }

        /// <summary>
        /// Test Bilinear Innterpolation
        /// Check when column arrays are null/zero length
        /// </summary>
        [TestMethod]
        public void Test08BilinearInterpolateTest()
        {
            double[] columnArray = null;
            double columnTarget = 0;
            double rowTarget = 0;
            double expected = 0;
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            double actual = siif.BilinearInterpolate(columnArray, _bilinRows,
                _bilinData, columnTarget, rowTarget);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test Cubic Hermite Spline Innterpolation
        /// Check when yArrays are null/zero length
        /// </summary>
        [TestMethod]
        public void Test09BilinearInterpolateTest()
        {
            double[] rowArray = null;
            double columnTarget = 0;
            double rowTarget = 0;
            double expected = 0;
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            double actual = siif.BilinearInterpolate(_bilinColumns,
                    rowArray, _bilinData, columnTarget, rowTarget);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test Cubic Hermite Spline Innterpolation
        /// Check when the data is null/zero length
        /// </summary>
        [TestMethod]
        public void Test10BilinearInterpolateTest()
        {
            double[,] dataArray = null;
            double columnTarget = 0;
            double rowTarget = 0;
            double expected = 0;
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            double actual = siif.BilinearInterpolate(_bilinColumns,
                _bilinRows, dataArray, columnTarget, rowTarget);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test Cubic Hermite Spline Innterpolation
        /// Test a range of values
        /// </summary>
        [TestMethod]
        public void Test11BilinearInterpolateTest()
        {
            double[] columnTarget = _bilinColumnTarget;
            double[] rowTarget = _bilinRowTarget;
            double[] expected = _bilinExpected;
            SABRInterpolationInterface siif = SABRInterpolationInterface.Instance();
            for (int i = 0; i < rowTarget.Length; i++)
            {
                double actual = siif.BilinearInterpolate(_bilinColumns,
                    _bilinRows, _bilinData, 
                    columnTarget[i], rowTarget[i]);
                Assert.AreEqual(expected[i], Math.Round(actual, 4));
            }
        }
    }
}