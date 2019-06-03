
using Orion.Analytics.Interpolators;
using Orion.Analytics.Interpolations.Points;
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.Interpolators
{
    /// <summary>
    ///This is a test class for BilinearInterpolatorTest and is intended
    ///to contain all BilinearInterpolatorTest Unit Tests
    ///</summary>
    [TestClass]
    public class BilinearInterpolatorTest
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
            BilinearInterpolator target = new BilinearInterpolator();
            IPoint point = new Point2D(4, 2, 0);
            List<IPoint> bounds = new List<IPoint>();
            bounds.Add(new Point2D(2,1,9));
            bounds.Add(new Point2D(6,1,13));
            bounds.Add(new Point2D(2,3,7));
            bounds.Add(new Point2D(6,3,11));


            double expected = 10;
            double actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);

            bounds = new List<IPoint>();
            bounds.Add(new Point2D(2, 1, 3));
            bounds.Add(new Point2D(6, 1, 7));
            bounds.Add(new Point2D(2, 3, 5));
            bounds.Add(new Point2D(6, 3, 9));


            expected = 6;
            actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Value
        ///</summary>
        [TestMethod]
        public void ValueTest1()
        {
            BilinearInterpolator target = new BilinearInterpolator();

            // Create the point to interpolate at
            IPoint point = new Point2D(4, 2, 0);

            // Create the bounding points
            List<IPoint> bounds = new List<IPoint>();
            bounds.Add(new Point2D(3, 1, 3));
            bounds.Add(new Point2D(5, 1, 9));
            bounds.Add(new Point2D(3, 3, 7));
            bounds.Add(new Point2D(5, 3, 13));

            // Set the expected value
            double expected = 8;

            // Interpolate at the point and test result
            double actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);

            // Create the point to interpolate at
            point = new Point2D(4, 3, 0);

            // Create the bounding points
            bounds = new List<IPoint>();
            bounds.Add(new Point2D(2, 2, 2));
            bounds.Add(new Point2D(5, 2, 11));
            bounds.Add(new Point2D(2, 4, 6));
            bounds.Add(new Point2D(5, 4, 15));

            // Set the expected value
            expected = 10;

            // Interpolate at the point and test result
            actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);

            // Test the boundary condition xy &gt; x1y1
            // Create the point to interpolate at
            point = new Point2D(5, 5, 0);

            // Create the bounding points
            bounds = new List<IPoint>();
            bounds.Add(new Point2D(4, 3, 10));
            bounds.Add(new Point2D(5, 3, 13));
            bounds.Add(new Point2D(4, 4, 12));
            bounds.Add(new Point2D(5, 4, 15));

            // Set the expected value
            expected = 15.0d;

            // Interpolate at the point and test result
            actual = target.Value(point, bounds);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for BilinearInterpolator Constructor
        ///</summary>
        [TestMethod]
        public void BilinearInterpolatorConstructorTest()
        {
            BilinearInterpolator target = new BilinearInterpolator();
            Assert.IsNotNull(target);
        }
    }
}