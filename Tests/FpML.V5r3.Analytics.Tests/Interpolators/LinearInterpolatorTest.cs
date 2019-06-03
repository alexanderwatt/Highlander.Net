
using Orion.Analytics.Interpolators;
using Orion.Analytics.Interpolations.Points;
using Orion.ModelFramework;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.Interpolators
{
    /// <summary>
    ///This is a test class for LinearInterpolatorTest and is intended
    ///to contain all LinearInterpolatorTest Unit Tests
    ///</summary>
    [TestClass]
    public class LinearInterpolatorTest
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
            LinearInterpolator target = new LinearInterpolator();

            // Create the point to interpolate for
            IPoint point = new Point1D(4, 0);

            // Create the bounding points
            List<IPoint> bounds = new List<IPoint> {new Point1D(2, 5), new Point1D(6, 13)};

            // Set the expected value
            double expected = 9;

            // Interpolate and test result
            double actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);

            // Create the point to interpolate for
            point = new Point1D(11, 0);

            // Create the bounding points
            bounds = new List<IPoint> {new Point1D(10, 21), new Point1D(12, 25)};

            // Set the expected value
            expected = 23;

            // Interpolate and test result
            actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);

            // Create the point to interpolate for
            point = new Point1D(11, 0);

            // Create the bounding points
            bounds = new List<IPoint> {new Point1D(6, 13), new Point1D(13, 27)};

            // Set the expected value
            expected = 23;

            // Interpolate and test result
            actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);

            // Test boundary condition x&lt;x1&lt;x2
            // Create the point to interpolate for
            point = new Point1D(1, 0);

            // Create the bounding points
            bounds = new List<IPoint> {new Point1D(2, 5), new Point1D(4, 9)};

            // Set the expected value
            expected = 5.0d;

            // Interpolate and test result
            actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);

            // Test boundary condition x1&lt;x2&lt;x
            // Create the point to interpolate for
            point = new Point1D(12);

            // Create the bounding points
            bounds = new List<IPoint> {new Point1D(8, 17), new Point1D(10, 21)};

            // Set the expected value
            expected = 21.0d;

            // Interpolate and test result
            actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for LinearInterpolator Constructor
        ///</summary>
        [TestMethod]
        public void LinearInterpolatorConstructorTest()
        {
            LinearInterpolator target = new LinearInterpolator();
            Assert.IsNotNull(target);
        }
    }
}