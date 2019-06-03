using System;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Stochastics.Volatilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.Volatilities
{
    [TestClass]
    public class SwaptionDataMatrixTest
    {
        private decimal[] _volStrikes;
        private string[] _volTenors;
        private decimal[][] _volData;
        private string _volExpiry;
        private string _id;
        private decimal[] _strikes;

        #region Additional test attributes

        //You can use the following additional attributes as you write your tests:
        //Use ClassInitialize to run code before running the first test in the class
        [TestInitialize]
        public void Initialise()
        {
            _volTenors = new[] { "1yr", "2yr", "3yr", "4yr", "5yr", "7yr", "10yr" };
            _volStrikes = new[] { -0.01m, -0.0075m, -0.0050m, -0.0025m, 0.000m, 0.0025m, 0.0050m, 0.0075m, 0.01m };
            _volData = new[]
                {
                                           new[] { 10.65m/100.0m, 10.40m/100.0m, 10.20m/100.0m, 10.07m/100.0m, 9.97m/100.0m, 9.92m/100.0m, 9.80m/100.0m, 9.96m/100.0m, 9.73m/100.0m },
                                           new[] { 10.72m/100.0m, 10.47m/100.0m, 10.26m/100.0m, 10.09m/100.0m, 10.00m/100.0m, 9.93m/100.0m, 9.83m/100.0m, 9.96m/100.0m, 10.08m/100.0m },
                                           new[] { 10.73m/100.0m, 10.48m/100.0m, 10.28m/100.0m, 10.13m/100.0m, 10.02m/100.0m, 9.95m/100.0m, 9.85m/100.0m, 9.92m/100.0m, 9.79m/100.0m },
                                           new[] { 0.00m/100.0m, 0.00m/100.0m, 0.00m/100.0m, 0.00m/100.0m, 0.00m/100.0m, 0.00m/100.0m, 0.00m/100.0m, 0.00m/100.0m, 0.00m/100.0m },
                                           new[] { 10.71m/100.0m, 10.42m/100.0m, 10.21m/100.0m, 10.06m/100.0m, 9.96m/100.0m, 9.87m/100.0m, 9.77m/100.0m, 9.87m/100.0m, 9.69m/100.0m },
                                           new[] { 10.74m/100.0m, 10.48m/100.0m, 10.28m/100.0m, 10.10m/100.0m, 9.99m/100.0m, 9.90m/100.0m, 9.77m/100.0m, 9.85m/100.0m, 9.68m/100.0m },
                                           new[] { 10.63m/100.0m, 10.35m/100.0m, 10.13m/100.0m, 9.95m/100.0m, 9.83m/100.0m, 9.75m/100.0m, 9.61m/100.0m, 9.72m/100.0m, 9.52m/100.0m }
                                       };

            _volExpiry = "6M";
            _id = "SwaptionMatrix.Test";

            // Basis point conversion to percentage
            _strikes = new[] { -100.0m / 10000.0m, -75.0m / 10000.0m, -50.0m / 10000.0m, -25.0m / 10000.0m, 0.0m, 25.0m / 10000.0m, 50.0m / 10000.0m, 75.0m / 10000.0m, 100.0m / 10000.0m };

        }

        #endregion

        #region Tests

        /// <summary>
        /// Test the GetVolatility(tenor, strike) method
        /// </summary>
        [TestMethod]
        public void GetVolatilityTest()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);

            decimal actual;
            const decimal expected = 10.26m / 100.0m;
            string tenor = "2yr";
            decimal strike = -0.50m;

            try
            {
                actual = target.GetVolatility(tenor, strike);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }

            tenor = "22yr";
            strike = -7.50m;

            try
            {
                actual = target.GetVolatility(tenor, strike);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
        }

        /// <summary>
        /// Test the GetVolatility(tenor, strike) method
        /// </summary>
        [TestMethod]
        public void GetVolatilityTest1()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);
            decimal actual;
            const decimal expected = 9.83m / 100.0m;
            Period tenor = PeriodHelper.Parse("2yr");
            decimal strike = 50.0m / 10000.0m;
            try
            {
                actual = target.GetVolatility(tenor, strike);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }

            tenor = PeriodHelper.Parse("32yr");
            strike = 50.0m / 10000.0m;

            try
            {
                actual = target.GetVolatility(tenor, strike);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
        }

        /// <summary>
        /// Test the GetVolatility(tenor, strike) method
        /// Test both versions return the same result given the same inputs
        /// </summary>
        [TestMethod]
        public void GetVolatilityTest2()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);
            decimal expected = 9.83m / 100.0m;
            decimal actual = 0;
            Period tenor = PeriodHelper.Parse("2yr");
            const decimal strike = 50.0m / 10000.0m;
            try
            {
                actual = target.GetVolatility(tenor, strike);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
            expected = actual;
            const string tenor1 = "2yr";
            try
            {
                actual = target.GetVolatility(tenor1, strike);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
        }

        /// <summary>
        /// Test the variant Volatilty retrieval. This version takes a tenor and returns all volatilities
        /// for all strikes that match this tenor
        /// </summary>
        [TestMethod]
        public void GetVolatilityTest3()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);
            const decimal expected = 10.26m / 100.0m;
            decimal[] actual;
            string tenor = "2yr";
            try
            {
                actual = target.GetVolatility(tenor);
                CollectionAssert.Contains(actual, expected);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
            tenor = "21yr";
            try
            {
                actual = target.GetVolatility(tenor);
                CollectionAssert.Contains(actual, expected);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
        }

        /// <summary>
        /// Test the variant Volatilty retrieval. This version takes a tenor and returns all volatilities
        /// for all strikes that match this tenor
        /// </summary>
        [TestMethod]
        public void GetVolatilityTest4()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);
            const decimal expected = 10.26m / 100.0m;
            decimal[] actual;
            Period tenor = PeriodHelper.Parse("2yr");
            try
            {
                actual = target.GetVolatility(tenor);
                CollectionAssert.Contains(actual, expected);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
            tenor = PeriodHelper.Parse("21yr");
            try
            {
                actual = target.GetVolatility(tenor);
                CollectionAssert.Contains(actual, expected);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
        }

        /// <summary>
        /// Test the variant Volatilty retrieval. This version takes a tenor and returns all volatilities
        /// for all strikes that match this tenor
        /// </summary>
        [TestMethod]
        public void GetVolatilityTest5()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);
            const decimal expected = 10.26m / 100.0m;
            var actual = new decimal[0];
            Period tenor = PeriodHelper.Parse("2yr");
            try
            {
                actual = target.GetVolatility(tenor);
                CollectionAssert.Contains(actual, expected);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
            var expected1 = new decimal[actual.Length];
            Array.Copy(actual, expected1, actual.Length);
            const string tenor1 = "2yr";
            try
            {
                int i = 0;
                actual = target.GetVolatility(tenor1);
                foreach (decimal d in actual)
                    Assert.AreEqual(expected1[i++], d);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No volatilities are available.", ex.Message);
            }
        }

        /// <summary>
        /// Test the expiry stored with the volatility matrix
        /// </summary>
        [TestMethod]
        public void GetExpiryTest()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);
            string expected = _volExpiry;
            try
            {
                string actual = target.GetExpiry();
                Assert.AreEqual(PeriodHelper.Parse(expected).ToString(), actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No expiry available.", ex.Message);
            }
        }

        /// <summary>
        /// Test the return of the tenors from a matrix
        /// A tenor will represent a logical row of the matrix
        /// </summary>
        [TestMethod]
        public void GetTenorsTest()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);
            string[] expected = _volTenors;

            try
            {
                string[] actual = target.GetTenors();
                for (int i = 0; i < expected.Length; i++)
                    Assert.AreEqual(PeriodHelper.Parse(expected[i]).ToString(), actual[i]);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("The matrix is malformed. No swap tenors available.", ex.Message);
            }
        }

        /// <summary>
        /// Test the strikes. Strikes are converted to decimal internally by the matrix.
        /// </summary>
        [TestMethod]
        public void GetStrikesTest()
        {
            var target = new SwaptionDataMatrix(_volTenors, _volStrikes, _volExpiry, _volData, _id);
            decimal[] expected = _strikes;
            decimal[] actual = target.GetStrikes();
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        #endregion
    }
}