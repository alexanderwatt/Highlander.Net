
using Orion.Analytics.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Orion.Analytics.DayCounters;
using Orion.ModelFramework;

namespace Orion.Analytics.Tests.BGSAnalytics
{
    /// <summary>
    /// Unit Tests for the class BGSConvexityCorrection.
    /// The names of the variables that appear in the code follow the notation
    /// used in the document "Pricing Analytics for a Balance Guaranteed Swap",
    /// prepared by George Scoufis, Reference Number: GS-02042007/1.
    /// </summary>
    [TestClass]
    public class BGSConvexityCorrectionUnitTests
    {
        private double _actual;
        private double _alpha;
        private DateTime _amortisationDate;
        private double _cleanUp;
        private double _currentBondFactor;
        private double _expected;
        private DateTime _lastAmortisationDate;
        private double _sigma;
 
        [TestInitialize]
        public void Initialisation()
        {
            _actual = 0.0;
            _alpha = 0.0;
            _cleanUp = 0.0;
            _currentBondFactor = 0.0;
            _expected = 0.0;
            _sigma = 0.0;
            _amortisationDate = DateTime.Today;
            _lastAmortisationDate = DateTime.Parse("2007-01-16");
        }

        /// <summary>
        /// Tests the method ComputeBGSConvexityCorrection.
        /// </summary>
        [TestMethod]
        public void TestComputeBGSConvexityCorrection()
        {
            // Instantiate the object that will provide access to the BGS 
            // convexity correction functionality.
            var convexityObj = new BGSConvexityCorrection();
            Assert.AreNotEqual(convexityObj, null);
            IDayCounter dayCountObj = Actual365.Instance;
            // Test the case: B0 < L.
            _amortisationDate = DateTime.Parse("2011-01-18");
            _currentBondFactor = 19/100;
            _alpha = -0.4532;
            _sigma = 2.17/100;
            _cleanUp = 20/100;
            _expected = 0.0;
            double tenor = dayCountObj.YearFraction(_lastAmortisationDate, _amortisationDate);
            _actual = convexityObj.ComputeBGSConvexityCorrection(tenor,
                                                                 _currentBondFactor,
                                                                 _alpha,
                                                                 _sigma,
                                                                 _cleanUp);
            Assert.AreEqual(_expected, _actual);

            // Test the limiting case: L = 0.
            _amortisationDate = DateTime.Parse("2007-04-16");
            _currentBondFactor = 68.29/100;
            _alpha = -0.3051;
            _sigma = 3.12/100;
            _cleanUp = 0.0;
            _expected = 0.6334102389;
            tenor = dayCountObj.YearFraction(_lastAmortisationDate, _amortisationDate);
            _actual = convexityObj.ComputeBGSConvexityCorrection(tenor,
                                                                 _currentBondFactor,
                                                                 _alpha,
                                                                 _sigma,
                                                                 _cleanUp);
            const double tolerance2 = 1.0E-6;
            Assert.AreEqual(_expected, _actual, tolerance2);

            // Test the linmiting case: sigma = 0.
            _amortisationDate = DateTime.Parse("2007-07-16");
            _currentBondFactor = 61.82/100;
            _alpha = -0.3051;
            _sigma = 0.0;
            _cleanUp = 10.0/100;
            _expected = 0.5314066262;
            tenor = dayCountObj.YearFraction(_lastAmortisationDate, _amortisationDate);
            _actual = convexityObj.ComputeBGSConvexityCorrection(tenor,
                                                                 _currentBondFactor,
                                                                 _alpha,
                                                                 _sigma,
                                                                 _cleanUp);
            const double tolerance3 = 1.0E-5;
            Assert.AreEqual(_expected, _actual, tolerance3);

            // Test the generic case.
            
            // Full convexity correction.
            _amortisationDate = DateTime.Parse("2008-10-15");
            _currentBondFactor = 91.73/100;
            _alpha = -0.3000;
            _sigma = 45.0/100;
            _cleanUp = 10.0/100;
            _expected = 0.5421666386;
            tenor = dayCountObj.YearFraction(_lastAmortisationDate, _amortisationDate);
            _actual = convexityObj.ComputeBGSConvexityCorrection(tenor,
                                                                 _currentBondFactor,
                                                                 _alpha,
                                                                 _sigma,
                                                                 _cleanUp);
            const double tolerance4 = 1.0E-7;
            Assert.AreEqual(_expected, _actual, tolerance4);

            // Truncated convexity correction.
            _amortisationDate = DateTime.Parse("2010-01-15");
            _currentBondFactor = 28.34/100;
            _alpha = -0.2567;
            _sigma = 2.71/100;
            _cleanUp = 10.0/100;
            _expected = 0.1312047820;
            tenor = dayCountObj.YearFraction(_lastAmortisationDate, _amortisationDate);
            _actual = convexityObj.ComputeBGSConvexityCorrection(tenor,
                                                                 _currentBondFactor,
                                                                 _alpha,
                                                                 _sigma,
                                                                 _cleanUp);
            const double tolerance5 = 1.0E-7;
            Assert.AreEqual(_expected, _actual, tolerance5);
        }
    }
}