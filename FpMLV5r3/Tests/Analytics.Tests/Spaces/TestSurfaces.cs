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

#region Using Directives

using System;
using System.Diagnostics;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.Analytics.V5r3.LinearAlgebra;
using Highlander.Reporting.ModelFramework.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Spaces
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class TestSurfaces 
    {
        private readonly double[] _pointCoords =  {0.0, 0.22, 0.50, 0.75, 1.00 };

        private readonly double[] _strikes = { 1.0, 2.0, 3.0, 4.0, 5.0 };

        private readonly double[] _rows = { 0.0, 0.22, 1.00};

        private readonly double[] _fwds = { 3.0, 3.0, 3.0, 3.0 };

        private static readonly Matrix VMatrix = new Matrix(new[,] 
                                                                       {	// Initiliazation from double[,]
                                                                           { 0.025, 0.035, 0.045, 0.055, 0.065 },
                                                                           { 0.035, 0.045, 0.055, 0.065, 0.075 },
                                                                           { 0.055, 0.065, 0.075, 0.085, 0.095 }
                                                                       });

        private static readonly Matrix VVols = new Matrix(new[,] 
                                                                       {	// Initiliazation from double[,]
                                                                           { 0.25, 0.25, 0.25, 0.25, 0.25 },
                                                                           { 0.25, 0.25, 0.25, 0.25, 0.25 },
                                                                           { 0.25, 0.25, 0.25, 0.25, 0.25 }
                                                                       });

        private readonly double[] _pointValues = 
            {0.065, .066, .067, .065, .063};

        private readonly double[] _pointValues2 = { 1.0, .99, .97, .95, .91 };

        private readonly double[] _testPointArray = {0.1, 0.26, 0.34, 0.4, 0.73, 0.87, 0.93};

        /// <summary>
        /// Testing the discrete curve.
        /// </summary>
        [TestMethod]
        public void TestDiscreteSurfaceValues()
        {
            var curve = new DiscreteSurface(_rows, _pointCoords, VMatrix);
            for (int i = 0; i < curve.GetPointList().Count; i++)
            {
                var point = curve.GetPointList()[i];
                var closest = curve.GetClosestValues(point);
                foreach (var close in closest)
                {
                    Console.WriteLine(" X: {0} Y : {1} Value1 : {2} Value2 : {3}", close.Coords[0], 
                        close.Coords[1], close.Coords[2], close.FunctionValue);
                }
            }
        }

        /// <summary>
        /// Testing the discrete curve.
        /// </summary>
        [TestMethod]
        public void TestNodeSurfaceValues()
        {
            var curve = new InterpolatedSurface(new DiscreteSurface(_rows, _pointCoords, VMatrix),new LinearInterpolation(),true);
            for (var cols = 0;  cols<_pointCoords.Length; cols++)
            {
                for (var row = 0; row < _rows.Length; row++ )
                {
                    IPoint pt = new Point2D(_rows[row], _pointCoords[cols]);
                    double actual = curve.Value(pt);
                    Debug.Print("Actual Value : {0} Row : {1} Column : {2}", actual, row, cols);
                    Assert.AreEqual(VMatrix[row, cols], actual, 1e-3);
                }
            }
        }

        /// <summary>
        /// Testing the discrete curve.
        /// </summary>
        [TestMethod]
        public void TestDiscreteSurfaceClosestValues()
        {
            var curve = new DiscreteSurface(_rows, _pointCoords, VMatrix);
            var point = new Point2D(0.3, 0.5);
            var closest = curve.GetClosestValues(point);
            Assert.AreEqual(closest[0].Coords[0], 0.22);
            Assert.AreEqual(closest[0].Coords[1], 0.5d);
            Assert.AreEqual(closest[0].FunctionValue, 0.055d);
            Assert.AreEqual(closest[1].Coords[0], 0.22d);
            Assert.AreEqual(closest[1].Coords[1], 0.5d);
            Assert.AreEqual(closest[1].FunctionValue, 0.055d);
            Assert.AreEqual(closest[2].Coords[0], 1.0d);
            Assert.AreEqual(closest[2].Coords[1], 0.5d);
            Assert.AreEqual(closest[2].FunctionValue, 0.075d);
            Assert.AreEqual(closest[3].Coords[0], 1.0d);
            Assert.AreEqual(closest[3].Coords[1], 0.5d);
            Assert.AreEqual(closest[3].FunctionValue, 0.075d);
        }

        /// <summary>
        /// Testing the discrete surface.
        /// </summary>
        [TestMethod]
        public void TestExtendedInterpolatedSurface()
        {
            var interpCurve = new ExtendedInterpolatedSurface(_rows, _pointCoords, _fwds, VMatrix, new LinearInterpolation(), new LinearInterpolation());
            for (int i = 0; i < interpCurve.GetDiscreteSpace().GetPointList().Count; i++)
            {
                IPoint point = interpCurve.GetDiscreteSpace().GetPointList()[i];
                var val = interpCurve.Value(point);
                Console.WriteLine(point.FunctionValue);
            }
            IPoint point2D = new Point2D(0.1, 0.2);
            Console.WriteLine(interpCurve.Value(point2D));
            point2D = new Point2D(0.2, 0.4);
            Console.WriteLine(interpCurve.Value(point2D));
            point2D = new Point2D(0.35, 0.6);
            Console.WriteLine(interpCurve.Value(point2D));
        }

        /// <summary>
        /// Testing the discrete surface.
        /// </summary>
        [TestMethod]
        public void TestExtendedInterpolatedSurface2()
        {
            var interpCurve = new ExtendedInterpolatedSurface(_rows, _pointCoords, VMatrix, new LinearInterpolation(), new LinearInterpolation());
            for (int i = 0; i < interpCurve.GetDiscreteSpace().GetPointList().Count; i++)
            {
                IPoint point = interpCurve.GetDiscreteSpace().GetPointList()[i];
                var val = interpCurve.Value(point);
                Console.WriteLine(point.FunctionValue);
            }
            IPoint point2d = new Point2D(0.1, 0.2);
            Console.WriteLine(interpCurve.Value(point2d));
            point2d = new Point2D(0.2, 0.4);
            Console.WriteLine(interpCurve.Value(point2d));
            point2d = new Point2D(0.35, 0.6);
            Console.WriteLine(interpCurve.Value(point2d));
        }

        /// <summary>
        /// Testing the discrete surface.
        /// </summary>
        [TestMethod]
        public void TestExtendedWingInterpolatedSurface()
        {
            var interpCurve = new ExtendedInterpolatedSurface(_rows, _strikes, null, VVols, new LinearInterpolation(), new WingModelInterpolation())
                                  {
                                      Forward = 3.50,
                                      Spot = 3.50
                                  };
            IPoint point2D = new Point2D(0.1, 0.2);
            Console.WriteLine(interpCurve.Value(point2D));
            point2D = new Point2D(0.2, 0.4);
            Console.WriteLine(interpCurve.Value(point2D));
            point2D = new Point2D(0.35, 0.6);
            Console.WriteLine(interpCurve.Value(point2D));
        }

        /// <summary>
        /// Testing the discrete surface.
        /// </summary>
        [TestMethod]
        public void TestExtendedWingInterpolatedSurface2()
        {
            var interpCurve = new ExtendedInterpolatedSurface(_rows, _strikes, VVols, new LinearInterpolation(), new WingModelInterpolation())
            {
                Forward = 3.50,
                Spot = 3.50
            };
            IPoint point2D = new Point2D(0.1, 0.2);
            Console.WriteLine(interpCurve.Value(point2D));
            point2D = new Point2D(0.2, 0.4);
            Console.WriteLine(interpCurve.Value(point2D));
            point2D = new Point2D(0.35, 0.6);
            Console.WriteLine(interpCurve.Value(point2D));
        }

        /// <summary>
        /// Testing the discrete surface.
        /// </summary>
        [TestMethod]
        public void
            TestInterpolatedSurface()
        {
            var curve = new DiscreteSurface(_rows, _pointCoords, VMatrix);
            var interpCurve = new InterpolatedSurface(curve, new LinearInterpolation(), true);
            for (int i = 0; i < curve.GetPointList().Count; i++)
            {
                IPoint point = curve.GetPointList()[i];
                var val = interpCurve.Value(point);
                Console.WriteLine(point.FunctionValue);
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
            TestDiscreteSurface2()
        {
            var curve = new DiscreteSurface(_rows, _pointCoords, VMatrix);
            var result = curve.GetMatrixOfValues();
            var rowlength = result.RowCount;
            var columns = result.ColumnCount;
            var length = curve.GetPointList().Count - 1;
            for (int i = 0; i < length; i++)
            {
                var point = curve.GetPointList()[i];
                Debug.Print("FunctionValue : {0} X Coord : {1} Y Coord : {2}",
                    point.FunctionValue, point.Coords[0], point.Coords[1]);
                //                    Assert.AreEqual(result[i, j], point.FunctionValue);
            }
            for (int j = 0; j < columns; j++)
            {
                for (int i = 0; i < rowlength; i++)
                {
                    Assert.AreEqual(result[i, j], VMatrix[i, j]);
                    Debug.Print("Matrix : {0} i : {1} j : {2}",
                                result[i, j], i, j);
                }
                //                    Assert.AreEqual(result[i, j], point.FunctionValue);
            }
        }
    }
}