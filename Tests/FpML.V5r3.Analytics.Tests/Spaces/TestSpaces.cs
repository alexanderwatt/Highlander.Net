#region Using Directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Orion.Analytics.Interpolations;
using Orion.ModelFramework;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.LinearAlgebra;

#endregion

namespace Orion.Analytics.Tests.Spaces
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class TestSpaces 
    {
        private readonly double[] _pointCoords = 
            {0.0, 0.22, 0.50, 0.75, 1.00 };

        private readonly double[] _rows = 
            { 0.0, 0.22, 1.00};

        private static readonly Matrix VMatrix = new Matrix(new[,] 
                                                                       {	// Initiliazation from double[,]
                                                                           { 0.065, 0.065, 0.065, 0.065, 0.065 },
                                                                           { 0.065, 0.065, 0.065, 0.065, 0.065 },
                                                                           { 0.065, 0.065, 0.065, 0.065, 0.065 }
                                                                       });

        private readonly double[] _pointValues = 
            {0.065, .066, .067, .065, .063};

        private readonly double[] _pointValues2 = { 1.0, .99, .97, .95, .91 };

        private readonly double[] _testPointArray = {0.1, 0.26, 0.34, 0.4, 0.73, 0.87, 0.93};

        /// <summary>
        /// Testing the discrete curve.
        /// </summary>
        [TestMethod]
        public void TestDiscreteCurve1()
        {
            IDiscreteSpace curve = new DiscreteCurve(_pointCoords, _pointValues);
            for (int i = 0; i < curve.GetPointList().Count; i++)
            {
                Debug.Print("X Coord {0} Y Coord {1}", curve.GetPointList()[i].Coords[0], curve.GetPointList()[i].Coords[1]);
                Assert.AreEqual(curve.GetPointList()[i].FunctionValue, _pointValues[i]);
                Assert.AreEqual(curve.GetPointList()[i].Coords[1], _pointValues[i]);
                Assert.AreEqual(curve.GetPointList()[i].Coords[0], _pointCoords[i]);
            }
        }

        /// <summary>
        /// Testing the discrete curve.
        /// </summary>
        [TestMethod]
        public void TestDiscreteCurveFromPointList()
        {
            var index = 0;
            var pointList = new List<IPoint>();
            foreach (var pt in _pointCoords)
            {
                var point = new Point1D(pt, _pointValues[index]);
                pointList.Add(point);
                index++;
            }
            IDiscreteSpace curve = new DiscreteCurve(pointList);
            for (int i = 0; i < curve.GetPointList().Count; i++)
            {
                Debug.Print("X Coord {0} Y Coord {1}", curve.GetPointList()[i].Coords[0], curve.GetPointList()[i].Coords[1]);
                Assert.AreEqual(curve.GetPointList()[i].FunctionValue, _pointValues[i]);
                Assert.AreEqual(curve.GetPointList()[i].Coords[1], _pointValues[i]);
                Assert.AreEqual(curve.GetPointList()[i].Coords[0], _pointCoords[i]);
            }
        }

        /// <summary>
        /// Testing the discrete curve.
        /// </summary>
        [TestMethod]
        public void TestDiscreteCurve1ClosestValues()
        {
            var curve = new DiscreteCurve(_pointCoords, _pointValues);
            var point = new Point1D(0.3);
            var closest = curve.GetClosestValues(point);
            Assert.AreEqual(closest[0].Coords[0], 0.22);
            Assert.AreEqual(closest[0].FunctionValue, 0.066000000000000003d);
            Assert.AreEqual(closest[1].Coords[0], 0.5);
            Assert.AreEqual(closest[1].FunctionValue, 0.067000000000000004d);
        }

        /// <summary>
        /// Testing the discrete curve.
        /// </summary>
        [TestMethod]
        public void TestDiscreteCurve2()
        {
            double[] p = { .1, .23, .4, .55, .73, .88 };

            double[] results = { .99, .98, .97, .94, .93, .88 };
            var points = new List<IPoint>();
            for (int i = 0; i < p.Length; i++)
            {
                var pi = new Point1D(p[i], results[i]);
                points.Add(pi);
            }

            IDiscreteSpace curve = new DiscreteCurve(points);
            for ( int i = 0; i < curve.GetPointList().Count; i++)
            {
                Assert.AreEqual(curve.GetPointList()[i].FunctionValue, results[i]);
                Assert.AreEqual(curve.GetPointList()[i].Coords[1], results[i]);
                Assert.AreEqual(curve.GetPointList()[i].Coords[0], p[i]);
            }
        }

        /// <summary>
        /// Testing the discrete curve.
        /// </summary>
        [TestMethod]
        public void TestDiscreteCurve3()
        {
            double[] p = { .1, .23, .4, .55, .73, .88 };

            double[] results = { .99, .98, .97, .94, .93, .88 };
            var points = new List<IPoint>();
            for (int i = 0; i < p.Length; i++)
            {
                var pi = new Point1D(p[i], results[i]);
                points.Add(pi);
            }

            IDiscreteSpace curve = new DiscreteCurve(points);
            for (int i = 0; i < curve.GetPointList().Count; i++)
            {
                Assert.AreEqual(curve.GetCoordinateArray(1)[i], curve.GetPointList()[i].Coords[0]);
            }
        }

        /// <summary>
        /// Testing the interepolated curve.
        /// </summary>
        [TestMethod]
        public void TestInterpolatedCurve()
        {
            IInterpolation interp = new LinearInterpolation();
            DiscreteCurve curve = new DiscreteCurve(_pointCoords, _pointValues);
            IInterpolatedSpace interpCurve = new InterpolatedCurve(curve, interp, true);
            foreach (double point in _testPointArray)
            {
                IPoint p = new Point1D(point);
                Console.WriteLine(interpCurve.Value(p));
            }
        }

        /// <summary>
        /// Testing the interepolated curve.
        /// </summary>
        [TestMethod]
        public void TestInterpolatedCurve2()
        {
            IInterpolation interp = new LogLinearInterpolation();
            DiscreteCurve curve = new DiscreteCurve(_pointCoords, _pointValues2);
            IInterpolatedSpace interpCurve = new InterpolatedCurve(curve, interp, true);
            foreach (double point in _testPointArray)
            {
                IPoint p = new Point1D(point);
                Console.WriteLine(interpCurve.Value(p));
            }
        }

        /// <summary>
        /// Testing the interepolated curve.
        /// </summary>
        [TestMethod]
        public void TestPiecewiseInterpolatedCurve()
        {
            IInterpolation interp = new PiecewiseConstantZeroRateInterpolation();//PWL requires the first point to be after the first 2 in the curve..
            DiscreteCurve curve = new DiscreteCurve(_pointCoords, _pointValues2);
            IInterpolatedSpace interpCurve = new InterpolatedCurve(curve, interp, true);
            foreach (double point in _testPointArray)
            {
                IPoint p = new Point1D(point);
                double val = interpCurve.Value(p);
                double rate = -Math.Log(val)/(double)p.Coords[0];
                Console.WriteLine(val);
                Console.WriteLine(rate);
            }
        }

        /// <summary>
        /// Testing the interepolated curve.
        /// </summary>
        [TestMethod]
        public void TestPiecewiseRateInterpolatedCurve()
        {
            var interp = new PiecewiseConstantZeroRateInterpolation();//PWL requires the first point to be after the first 2 in the curve..
            DiscreteCurve curve = new DiscreteCurve(_pointCoords, _pointValues);
            IInterpolatedSpace interpCurve = new InterpolatedCurve(curve, interp, true);
            foreach (double point in _testPointArray)
            {
                IPoint p = new Point1D(point);
                double val = interpCurve.Value(p);
                double rate = val;
                Console.WriteLine(val);
                Console.WriteLine(rate);
            }
        }

        /// <summary>
        /// Testing the interepolated curve.
        /// </summary>
        [TestMethod]
        public void TestInterpolatedCubicCurve()
        {
            IInterpolation interp = new CubicSplineInterpolation();
            DiscreteCurve curve = new DiscreteCurve(_pointCoords, _pointValues);
            IInterpolatedSpace interpCurve = new InterpolatedCurve(curve, interp, true);
            foreach (double point in _testPointArray)
            {
                IPoint p = new Point1D(point);
                Console.WriteLine(interpCurve.Value(p));
            }
        }

        /// <summary>
        /// Testing the discrete surface.
        /// </summary>
        [TestMethod]
        public void 
            TestDiscreteSurface()
        {
            IDiscreteSpace curve = new DiscreteSurface(_rows, _pointCoords, VMatrix);
            for (int i = 0; i < curve.GetPointList().Count; i++)
            {
                IPoint point = curve.GetPointList()[i];
                Console.WriteLine(point.FunctionValue);
            }
        }

        /// <summary>
        /// Testing the discrete surface.
        /// </summary>
        [TestMethod]
        public void
            TestDiscreteSurfaceFromPointList()
        {
            var pointList = new List<IPoint>();
            for(var j = 0; j < _pointCoords.Length;j++)
            {
                for(var i = 0; i < _rows.Length;i++)
                {
                    var point = new Point2D(_rows[i], _pointCoords[j], VMatrix[i,j]);
                    pointList.Add(point);
                }
            }
            IDiscreteSpace curve = new DiscreteSurface(pointList);
            for (int i = 0; i < curve.GetPointList().Count; i++)
            {
                IPoint point = curve.GetPointList()[i];
                Console.WriteLine(point.FunctionValue);
            }
        }
    }
}