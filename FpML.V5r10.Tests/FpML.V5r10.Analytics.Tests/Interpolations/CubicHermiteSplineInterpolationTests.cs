using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Tests.Interpolations
{
    /// <summary>
    /// Unit Tests for the class CubicHermiteSplineInterpolation
    /// </summary>
    [TestClass]
    public class CubicHermiteSplineInterpolationTests
    {
        #region Private Fields

        CubicHermiteSplineInterpolation _interpObj; // interpolation object
        double[] _xArray; // array of x values
        double[] _yArray; // array of y values 
        decimal[] _x2Array; // array of x values
        decimal[] _y2Array; // array of y values 
        double _target;
        double _actual;
        double _expected;
        double _tolerance;

        #endregion Private Fields

        #region Setup

        [TestInitialize]
        public void Initialisation()
        {
            _xArray = new[] {274d, 365d, 732d, 1097d, 1462d, 1828d,
                                    2556d, 3654d, 5480d};
            _yArray = new[] {9.28622644581371d/100d,
                                    9.45686973002003d/100d, 10.3375d/100d, 10.7525d/100d,
                                    10.8400d/100d, 10.9325d/100d, 10.9450d/100d,
                                    10.9900d/100d, 11.0000d/100d};

            _x2Array = new decimal[] {274, 365, 732, 1097, 1462, 1828,
                                      2556, 3654, 5480};
            _y2Array = new[] {9.28622644581371m/100m,
                                      9.45686973002003m/100m, 10.3375m/100m, 10.7525m/100m,
                                      10.8400m/100m, 10.9325m/100m, 10.9450m/100m,
                                      10.9900m/100m, 11.0000m/100m};

            // Instantiate an instance of the class.
            _interpObj = new CubicHermiteSplineInterpolation();
            _interpObj.Initialize(_x2Array, _y2Array);
            Assert.IsNotNull(_interpObj);
            _target = 400d;
            _tolerance = 1.0E-5d;
        }

        #endregion Setup

        #region Test: Constructor

        /// <summary>
        /// Tests the class constructor.
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            // Check that a mismatch in the sizes of the array data is detected.
            _xArray = new double[2];
            _xArray[0] = 274d;
            _xArray[1] = 365d;

            _yArray = new double[3];
            _yArray[0] = 9.29d/100.0d;
            _yArray[1] = 9.46d/100.0d;
            _yArray[2] = 10.34d/100.0d;

            try
            {
                _interpObj =
                    new CubicHermiteSplineInterpolation();
                _interpObj.Initialize(_xArray, _yArray);
            }
            catch (System.Exception e)
            {
                const string errorMessage =
                    "Cubic Hermite Spline interpolation needs equal length arrays";
                Assert.AreEqual(errorMessage, e.Message);            	
            }

            // Check that the minimum array sizes is detected.
            _xArray = new double[1];
            _yArray = new double[1];

            try
            {
                _interpObj =
                    new CubicHermiteSplineInterpolation();
                _interpObj.Initialize(_xArray, _yArray);
            }
            catch (System.Exception e)
            {
                const string errorMessage =
                    "Cubic Hermite Spline interpolation needs at least two (x,y)";
                Assert.AreEqual(errorMessage, e.Message);                 
            }
            // Check that unsorted x array is detected.
            _xArray = new double[2];
            _yArray = new double[2];

            try
            {
                _interpObj =
                    new CubicHermiteSplineInterpolation();
                _interpObj.Initialize(_xArray, _yArray);
            }
            catch (System.Exception e)
            {
                const string errorMessage =
                    "Cubic Hermite Spline interpolation needs sorted x values";
                Assert.AreEqual(errorMessage, e.Message);
            }
        }

        #endregion Test: Constructor

        #region Test: DataTable Method

        /// <summary>
        /// Tests that the data table is constructed correctly.
        /// </summary>
        [TestMethod]
        public void TestDataTable()
        {
            // Check that the x values are constructed correctly.
            int i = 0; // array index

            foreach (decimal x in _interpObj.DataTable.Keys)
            {
                Assert.AreEqual(_x2Array[i], x);
                ++i; // move to the next element in the array of x values
            }

            // Check that the y values are constructed correctly.
            i = 0; // reset array index
            foreach (decimal y in _interpObj.DataTable.Values)
            {
                Assert.AreEqual(_y2Array[i], y);
                ++i; // move to the next element in the array of y values
            }
        }

        #endregion Test: DataTable Method

        #region Test: FindBoundingInterval Method

        /// <summary>
        /// Tests the method FindBoundingInterval.
        /// </summary>
        [TestMethod]
        public void TestFindBoundingInterval()
        {
            #region Test: Extrapolation at Left End Detected

            _target = 273d;

            try
            {
                _interpObj.ValueAt(_target, true);
            }
            catch (System.Exception e)
            {
                const string ErrorMessage =
                    "Cubic Hermite Spline does not support extrapolation";

                Assert.AreEqual(ErrorMessage, e.Message);            	
            }

            #endregion Test: Extrapolation at Left End Detected

            #region Test: Extrapolation at Right End Detected 

            _target = 5480.0001d;

            try
            {
                _interpObj.ValueAt(_target, true);
            }
            catch (System.Exception e)
            {
                const string ErrorMessage =
                    "Cubic Hermite Spline does not support extrapolation";

                Assert.AreEqual(ErrorMessage, e.Message);
            }

            #endregion Test: Extrapolation at Right End Detected

            #region Test: Target Point at the Extreme Left

            _target = 274d;
            _interpObj.ValueAt(_target, true);
            Assert.AreEqual(0m, _interpObj.LeftIndex);
            Assert.AreEqual(1m, _interpObj.RightIndex);

            #endregion  Test: Target Point at the Extreme Left

            #region Test: Target Point at the Extreme Right

            _target = 5480d;
            _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_xArray.Length - 2, _interpObj.LeftIndex);
            Assert.AreEqual(_xArray.Length - 1, _interpObj.RightIndex);

            #endregion Test: Target Point at the Extreme Right

            #region Test: Target Point at the Second Knot Point

            _target = 365d;
            _interpObj.ValueAt(_target, true);
            Assert.AreEqual(1, _interpObj.LeftIndex);
            Assert.AreEqual(2, _interpObj.RightIndex);

            #endregion Test: Target Point at the Second Knot Point

            #region Test: Target Point at the Second Last Knot Point

            _target = 3654d;
            _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_xArray.Length - 2, _interpObj.LeftIndex);
            Assert.AreEqual(_xArray.Length - 1, _interpObj.RightIndex);

            #endregion Test: Target Point at the Second Last Knot Point

            #region Test: Target Point Bounded by Second and Third Points

            _target = 500d;
            _interpObj.ValueAt(_target, true);
            Assert.AreEqual(1, _interpObj.LeftIndex);
            Assert.AreEqual(2, _interpObj.RightIndex);

            #endregion Test: Target Point Bounded by Second and Third Points

            #region Test:Target Point Bounded by Second and Third Last Points

            _target = 3000d;
            _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_xArray.Length - 3, _interpObj.LeftIndex);
            Assert.AreEqual(_xArray.Length - 2, _interpObj.RightIndex);

            #endregion Test:Target Point Bounded by Second and Third Last Points
        }

        #endregion Test: FindBoundingInterval Method

        #region Test: NormalisedTarget Method

        /// <summary>
        /// Tests the method NormalisedTarget.
        /// </summary>
        [TestMethod]
        public void TestNormalisedTarget()
        {
            _expected = .095352505351591248d;
            _actual = _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);
        }

        #endregion Test: NormalisedTarget Method

        #region Test: Interpolate Method

        /// <summary>
        /// Tests the method Interpolate.
        /// </summary>
        [TestMethod]
        public void TestInterpolate()
        {
            #region Test: Interpolation at Grid Points

            _target = 274d; // extreme left knot point
            _expected = 9.28622644581371d/100d;
            _actual = _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            _target = 1462d; // interior knot point
            _expected = 10.84d/100d;
            _actual = _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            _target = 5480d; // extreme right knot point
            _expected = 11d/100d;
            _actual = _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            #endregion Test: Interpolation at Grid Points

            #region Test: Interpolation at Non Grid Points

            _tolerance = 1.0E-10d;
            _target = 456d;
            _expected = 9.67244687477861d/100d;
            _actual = _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            _target = 820d;
            _expected = 10.4767728091016d/100d;
            _actual = _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            _target = 2009d;
            _expected = 10.9472407712689d/100d;
            _actual = _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            _target = 3289d;
            _expected = 10.9769644449966d/100d;
            _actual = _interpObj.ValueAt(_target, true);
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            #endregion Test: Interpolation at Non Grid Points
        }

        #endregion Test: Interpolate Method
    }
}