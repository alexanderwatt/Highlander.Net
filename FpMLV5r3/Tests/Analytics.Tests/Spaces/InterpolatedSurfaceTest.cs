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

using System.Collections.Generic;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.ModelFramework.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Spaces
{
    [TestClass]
    public class InterpolatedSurfaceTest
    {
        [TestMethod]
        public void ValueTestSixteenPoints()
        {
            List<IPoint> points
                = new List<IPoint>
                      {
                          new Point2D(3, 1, 5.2),
                          new Point2D(3, 3, 7.9),
                          new Point2D(3, 8, 8.7),
                          new Point2D(3, 9, 8.9),
                          new Point2D(7, 1, 8.1),
                          new Point2D(7, 3, 12.4),
                          new Point2D(7, 8, 12.5),
                          new Point2D(7, 9, 14.7),
                          new Point2D(8, 1, 8.2),
                          new Point2D(8, 3, 12.2),
                          new Point2D(8, 8, 12.7),
                          new Point2D(8, 9, 13.6),
                          new Point2D(9, 1, 8.3),
                          new Point2D(9, 3, 12.2),
                          new Point2D(9, 8, 12.6),
                          new Point2D(9, 9, 12.9),
                      };
            DiscreteSpace surface = new DiscreteSurface(points);
            InterpolatedSurface interp = new InterpolatedSurface(surface, new LinearInterpolation(), true);
            // Actual points
            Assert.AreEqual(5.2, interp.Value(new Point2D(3, 1, 0)));
            Assert.AreEqual(7.9, interp.Value(new Point2D(3, 3, 0)));
            Assert.AreEqual(8.7, interp.Value(new Point2D(3, 8, 0)));
            Assert.AreEqual(8.9, interp.Value(new Point2D(3, 9, 0)));
            Assert.AreEqual(8.1, interp.Value(new Point2D(7, 1, 0)));
            Assert.AreEqual(12.4, interp.Value(new Point2D(7, 3, 0)));
            Assert.AreEqual(12.5, interp.Value(new Point2D(7, 8, 0)));
            Assert.AreEqual(14.7, interp.Value(new Point2D(7, 9, 0)));
            Assert.AreEqual(8.2, interp.Value(new Point2D(8, 1, 0)));
            Assert.AreEqual(12.2, interp.Value(new Point2D(8, 3, 0)));
            Assert.AreEqual(12.7, interp.Value(new Point2D(8, 8, 0)));
            Assert.AreEqual(13.6, interp.Value(new Point2D(8, 9, 0)));
            Assert.AreEqual(8.3, interp.Value(new Point2D(9, 1, 0)));
            Assert.AreEqual(12.2, interp.Value(new Point2D(9, 3, 0)));
            Assert.AreEqual(12.6, interp.Value(new Point2D(9, 8, 0)));
            Assert.AreEqual(12.9, interp.Value(new Point2D(9, 9, 0)));
            // Interpolate
            Assert.AreEqual(12.42, interp.Value(new Point2D(7.5, 5, 0)));
            Assert.AreEqual(7.4750000000000005, interp.Value(new Point2D(4, 2, 0)));
            Assert.AreEqual(12.95, interp.Value(new Point2D(8.5, 8.5, 0)));
            // Past the boundary
            Assert.AreEqual(5.2, interp.Value(new Point2D(2, 0, 0)));
            Assert.AreEqual(12.9, interp.Value(new Point2D(10, 10, 0)));
            Assert.AreEqual(14.7, interp.Value(new Point2D(7, 10, 0)));
        }
    }
}