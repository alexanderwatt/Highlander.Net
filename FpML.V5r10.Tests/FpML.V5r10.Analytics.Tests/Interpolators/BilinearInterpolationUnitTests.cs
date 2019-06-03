
using System;
using Orion.Analytics.Interpolators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Tests.Interpolators
{
    /// <summary>
    /// Unit Tests for the class BilinearInterpolation.
    /// </summary>
    [TestClass]
    public class BilinearInterpolationUnitTests
    {
        #region Private Constants

        private const int NumColumns = 3; //number of column labels
        private const int NumRows = 2; // number of row labels

        #endregion Private Constants

        #region Private Fields

        private double[] _columnLabels; // storage for the column labels
        private double[,] _dataTable; // storage for the table of known values
        private double[] _rowLabels; // storage for the row labels

        private double _columnTarget; // column target for interpolation
        private double _rowTarget; // row target for interpolation

        private double _actualValue; // actual value from test
        private double _expectedValue; // expected value from test

        #endregion Private Fields

        #region SetUp Method

        [TestInitialize]
        public void Initialisation()
        {
            // Fill the array that stores the column labels.
            _columnLabels = new double[NumColumns];
            _columnLabels[0] = 85/100.0;
            _columnLabels[1] = 90/100.0;
            _columnLabels[2] = 100/100.0;

            // Fill the array that stores the row labels.
            _rowLabels = new double[NumRows];
            _rowLabels[0] = 38969;
            _rowLabels[1] = 39524;

            // Fill the data table.
            _dataTable = new double[NumRows, NumColumns];
            _dataTable[0, 0] = 41.50/100.0;
            _dataTable[0, 1] = 30/100.0;
            _dataTable[0, 2] = 42.50/100.0;

            _dataTable[1, 0] = 20.25/100.0;
            _dataTable[1, 1] = 21.35/100.0;
            _dataTable[1, 2] = 22.80/100.0;

        }

        #endregion SetUp Method

        #region Test: CheckDataQuality

        /// <summary>
        /// Tests the (private) method CheckDataQuality.
        /// </summary>
        [TestMethod]
        public void TestCheckDataQuality()
        {
            // Test: Empty array of column labels detected.
            {
                double[] emptyColumnLabels = new double[0];

                try
                {
#pragma warning disable 168
                    BilinearInterpolation interpObj =
#pragma warning restore 168
                        new BilinearInterpolation(ref emptyColumnLabels,
                                                  ref _rowLabels,
                                                  ref _dataTable);
                }
                catch(Exception exception)
                {
                    string ErrorMessage =
                        "Bilinear interpolation found an empty array.";

                    Assert.AreSame(ErrorMessage, exception.Message);
                }
            }

            // Test: Empty array of row labels detected.
            {
                double[] emptyRowLabels = new double[0];

                try
                {
#pragma warning disable 168
                    BilinearInterpolation interpObj =
#pragma warning restore 168
                        new BilinearInterpolation(ref _columnLabels,
                                                  ref emptyRowLabels,
                                                  ref _dataTable);
                }
                catch (Exception exception)
                {
                    string ErrorMessage =
                        "Bilinear interpolation found an empty array.";

                    Assert.AreSame(ErrorMessage, exception.Message);
                }
            }

            // Test: Empty data table detected.
            {
                double[,] emptyDataTable = new double[0,0];

                try
                {
#pragma warning disable 168
                    BilinearInterpolation interpObj =
#pragma warning restore 168
                        new BilinearInterpolation(ref _columnLabels,
                                                  ref _rowLabels,
                                                  ref emptyDataTable);
                }
                catch (Exception exception)
                {
                    string ErrorMessage =
                        "Bilinear interpolation found an empty array.";

                    Assert.AreSame(ErrorMessage, exception.Message);
                }
            }

            // Test: Unsorted column labels are detected.
            {
                double[] unsortedColumnLabels = {85/100.0, 90/100.0, 90/100.0};

                try
                {
#pragma warning disable 168
                    BilinearInterpolation interpObj =
#pragma warning restore 168
                        new BilinearInterpolation(ref unsortedColumnLabels,
                                                  ref _rowLabels,
                                                  ref _dataTable);
                }
                catch (Exception exception)
                {
                    string ErrorMessage =
                        "Bilinear interpolation requires column/row labels in ascending order";

                    Assert.AreSame(ErrorMessage, exception.Message);
                }
            }

            // Test: Unsorted row labels are detected.
            {
                double[] unsortedRowLabels = {38969, 38968};

                try
                {
#pragma warning disable 168
                    BilinearInterpolation interpObj =
#pragma warning restore 168
                        new BilinearInterpolation(ref _columnLabels,
                                                  ref unsortedRowLabels,
                                                  ref _dataTable);
                }
                catch (Exception exception)
                {
                    string ErrorMessage =
                        "Bilinear interpolation requires column/row labels in ascending order";

                    Assert.AreSame(ErrorMessage, exception.Message);
                }
            }

            // Test: Incorrect column sizes are detected.
            {
                double[] incorrectColumnLabels =
                    {85/100.0, 90/100.0, 100/100.0, 142/100.0};
 
                try
                {
#pragma warning disable 168
                    BilinearInterpolation interpObj =
#pragma warning restore 168
                        new BilinearInterpolation(ref incorrectColumnLabels,
                                                  ref _rowLabels,
                                                  ref _dataTable);
                }
                catch (Exception exception)
                {
                    string ErrorMessage =
                        "Bilinear interpolation found incorrect array dimensions.";

                    Assert.AreSame(ErrorMessage, exception.Message);
                }
            }

            // Test: Incorrect row sizes are detected.
            {
                double[] incorrectRowLabels = {39524};

                try
                {
#pragma warning disable 168
                    BilinearInterpolation interpObj =
#pragma warning restore 168
                        new BilinearInterpolation(ref _columnLabels,
                                                  ref incorrectRowLabels,
                                                  ref _dataTable);
                }
                catch (Exception exception)
                {
                    string ErrorMessage =
                        "Bilinear interpolation found incorrect array dimensions.";

                    Assert.AreSame(ErrorMessage, exception.Message);
                }
            }
        }


        #endregion Test: CheckDataQuality

        #region Test: Constructor

        /// <summary>
        /// Test the class constructor.
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            BilinearInterpolation interpObj =
                new BilinearInterpolation(ref _columnLabels,
                                          ref _rowLabels,
                                          ref _dataTable);

            Assert.IsNotNull(interpObj);
        }

        #endregion Test: Constructor

        #region Test: Interpolate method

        /// <summary>
        /// Tests the method Interpolate.
        /// </summary>
        [TestMethod]
        public void TestInterpolate()
        {
            BilinearInterpolation interpObj =
                new BilinearInterpolation(ref _columnLabels,
                                          ref _rowLabels,
                                          ref _dataTable);
            Assert.IsNotNull(interpObj);

            // Test that the node points are recovered by the interpolation.
            _rowTarget = 38969;
            _columnTarget = 85/100.0;
            _expectedValue = 41.5/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 38969;
            _columnTarget = 90/100.0;
            _expectedValue = 30/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 38969;
            _columnTarget = 100/100.0;
            _expectedValue = 42.5/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 39524;
            _columnTarget = 85/100.0;
            _expectedValue = 20.25/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 39524;
            _columnTarget = 90/100.0;
            _expectedValue = 21.35/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 39524;
            _columnTarget = 100/100.0;
            _expectedValue = 22.8/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            // Test interpolation at points that do not coincide with
            // the node points.
            _rowTarget = 38969;
            _columnTarget = 70/100.0;
            _expectedValue = 41.5/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 38969;
            _columnTarget = 140/100.0;
            _expectedValue = 42.5/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 38735;
            _columnTarget = 90/100.0;
            _expectedValue = 30/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 39889;
            _columnTarget = 90/100.0;
            _expectedValue = 21.35/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 38969;
            _columnTarget = 87.5/100.0;
            _expectedValue = 35.75/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            Assert.AreEqual(_expectedValue, _actualValue);

            _rowTarget = 39334;
            _columnTarget = 85.1/100.0;
            _expectedValue = 27.46/100.0;
            _actualValue = interpObj.Interpolate(_columnTarget, _rowTarget);
            const double tolerance = 1.0E-5;
            Assert.AreEqual(_expectedValue, _actualValue, tolerance);
        }

        #endregion Test: Interpolate method

    }
}