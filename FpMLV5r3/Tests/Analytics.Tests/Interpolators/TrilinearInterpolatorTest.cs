﻿/*
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
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Interpolators;
using Highlander.Reporting.ModelFramework.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Interpolators
{
    /// <summary>
    ///This is a test class for TrilinearInterpolatorTest and is intended
    ///to contain all TrilinearInterpolatorTest Unit Tests
    ///</summary>
    [TestClass]
    public class TrilinearInterpolatorTest
    {
        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Value
        ///</summary>
        [TestMethod]
        public void ValueTest()
        {
            TrilinearInterpolator target = new TrilinearInterpolator();

            // Create the point to interpolate for
            IPoint point = new Point3D(3, 2, 3, 0);

            // Set up the bounding cube
            List<IPoint> bounds = new List<IPoint>();
            bounds.Add(new Point3D(2, 1, 1, -4));
            bounds.Add(new Point3D(4, 1, 1, -2));
            bounds.Add(new Point3D(2, 3, 1, -12));
            bounds.Add(new Point3D(4, 3, 1, -10));

            bounds.Add(new Point3D(2, 1, 4, 5));
            bounds.Add(new Point3D(4, 1, 4, 7));
            bounds.Add(new Point3D(2, 3, 4, -3));
            bounds.Add(new Point3D(4, 3, 4, -1));

            // Set the expected value
            double expected = -1;

            // Perform the interpolation and test the results
            double actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);

            // Test boundary condition
            // Create the point to interpolate for
            point = new Point3D(3, 5, 5, 0);

            // Set up the bounding cube
            bounds = new List<IPoint>();
            bounds.Add(new Point3D(2, 3, 2, -9));
            bounds.Add(new Point3D(3, 3, 2, -8));
            bounds.Add(new Point3D(2, 5, 2, -17));
            bounds.Add(new Point3D(3, 5, 2, -16));

            bounds.Add(new Point3D(2, 3, 4, -3));
            bounds.Add(new Point3D(3, 3, 4, -2));
            bounds.Add(new Point3D(2, 5, 4, -11));
            bounds.Add(new Point3D(3, 5, 4, -10));

            // Set the expected value
            expected = -10.0d;

            // Perform the interpolation and test the results
            actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TrilinearInterpolator Constructor
        ///</summary>
        [TestMethod]
        public void TrilinearInterpolatorConstructorTest()
        {
            TrilinearInterpolator target = new TrilinearInterpolator();
            Assert.IsNotNull(target);
        }
    }
}