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

using System.Collections;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Points
{
    /// <summary>
    ///This is a test class for CoordinateTest and is intended
    ///to contain all CoordinateTest Unit Tests
    ///</summary>
    [TestClass]
    public class CoordinateTest
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
        ///A test for PricingDataCoordinate
        ///</summary>
        [TestMethod]
        public void PricingDataCoordinateTest()
        {
            var expiry = "5Y";
            var strike = "25";
            var target = new Coordinate(expiry, strike);
            var actual = target.PricingDataCoordinate;
            Assert.AreEqual(((Period)actual.expiration[0].Items[0]).ToString(), expiry);
            Assert.AreEqual(actual.strike[0], 25m);
            var term = "3Y";
            expiry = "2Y";
            strike = "-50";
            target = new Coordinate(expiry, term, strike);
            actual = target.PricingDataCoordinate;
            Assert.AreEqual(((Period)actual.expiration[0].Items[0]).ToString(), expiry);
            Assert.AreEqual(((Period)actual.term[0].Items[0]).ToString(), term);
            Assert.AreEqual(actual.strike[0], -50m);
        }

        ///// <summary>
        /////A test for FunctionValue
        /////</summary>
        //[TestMethod()]
        //public void FunctionValueTest()
        //{
        //    string expiry = string.Empty; // TODO: Initialize to an appropriate value
        //    string strike = string.Empty; // TODO: Initialize to an appropriate value
        //    Coordinate target = new Coordinate(expiry, strike); // TODO: Initialize to an appropriate value
        //    double expected = 0F; // TODO: Initialize to an appropriate value
        //    double actual;
        //    target.FunctionValue = expected;
        //    actual = target.FunctionValue;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        /// <summary>
        ///A test for Coords
        ///</summary>
        [TestMethod]
        public void CoordsTest()
        {
            var term = "3Y";
            var expiry = "5Y";
            var strike = "-50";
            var target = new Coordinate(expiry, term, strike);
            IList actual = target.Coords;
            Assert.AreEqual(((Period)actual[0]).ToString(), expiry);
            Assert.AreEqual(((Period)actual[1]).ToString(), term);
            Assert.AreEqual(actual[2], -50m);
        }

        /// <summary>
        ///A test for Coordinate Constructor
        ///</summary>
        [TestMethod]
        public void CoordinateConstructorTest2()
        {
            string expiry = "6M";
            string term = "5Y";
            string strike = "75";
            string generic = "Generic";
            Coordinate target = new Coordinate(expiry, term, strike, generic);
            Assert.AreEqual(((Period)target.Coords[0]).ToString(), expiry);
            Assert.AreEqual(((Period)target.Coords[1]).ToString(), term);
            Assert.AreEqual(target.Coords[2], 75m);
        }

        /// <summary>
        ///A test for Coordinate Constructor
        ///</summary>
        [TestMethod]
        public void CoordinateConstructorTest1()
        {
            string term = "3Y";
            string expiry = "5Y";
            string strike = "-50";
            Coordinate target = new Coordinate(expiry, term, strike);
            Assert.AreEqual(((Period)target.Coords[0]).ToString(), expiry);
            Assert.AreEqual(((Period)target.Coords[1]).ToString(), term);
            Assert.AreEqual(target.Coords[2], -50m);
        }

        /// <summary>
        ///A test for Coordinate Constructor
        ///</summary>
        [TestMethod]
        public void CoordinateConstructorTest()
        {
            string expiry = "6M";
            string strike = "-25";
            Coordinate target = new Coordinate(expiry, strike);
            Assert.AreEqual(((Period)target.Coords[0]).ToString(), expiry);
            Assert.AreEqual(target.Coords[1], -25m);
        }

        /// <summary>
        ///A test for Coordinate Constructor
        ///</summary>
        [TestMethod]
        public void CoordinatePointTest()
        {
            string expiry = "6M";
            string strike = "-25";
            var target = new Coordinate(expiry, strike);
            var result = target.ContainedPoint;
            Assert.AreEqual(result.Coords[0], 0.5);
            Assert.AreEqual(target.Coords[1], -25m);
        }
    }
}