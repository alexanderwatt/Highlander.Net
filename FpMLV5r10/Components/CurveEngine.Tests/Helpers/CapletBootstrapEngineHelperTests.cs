
#region Using Directives

using National.QRSC.Analytics.Options;
using National.QRSC.Analytics.PricingEngines;
using National.QRSC.Engine.Tests.Helpers;
using National.QRSC.FpML.V47;
using National.QRSC.Engine.Assets.Rates.CapFloorLet;
using National.QRSC.Engine.Helpers;
using National.QRSC.ObjectCache;
using National.QRSC.Utility.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

#endregion

namespace National.QRSC.Engine.Tests.Caplets
{
    /// <summary>
    /// Unit tests for the class CapletBootstrapEngineHelper.
    /// </summary>
    [TestClass]
    public class CapletBootstrapEngineHelperTests
    {
        #region Private Fields

        private DateTime _calculationDate; // reference date for all cashflows
        private CapFrequency _capFrequency; // number of Caplets per year
        private int _capStartLag; // offset from calculation date to Cap start
        private string _currency; // three letter currency code
        private DateTime _endDate;
        private string _handle;
        private double _numYears; // number of years to maturity 
        private List<double> _offsets;
        private ParVolatilityInterpolationType _parVolatilityInterpolation;
        private BusinessDayConventionEnum _rollConvention; // MODFOLLOWING
        private DateTime _startDate; // start date for the swap
        private List<DateTime> _rollSchedule; // container for the output
        private CapletBootstrapSettings _settingsObj;

        private double _actual;
        private double _expected;
        private double _tolerance;

        private DateTime[] _dates =
            {DateTime.Parse("2007-11-30"), DateTime.Parse("2008-02-29"),
             DateTime.Parse("2008-05-30"), DateTime.Parse("2008-08-29"),
             DateTime.Parse("2008-11-28"), DateTime.Parse("2009-02-27"),
             DateTime.Parse("2009-05-29"), DateTime.Parse("2009-08-31"),
             DateTime.Parse("2009-11-30"), DateTime.Parse("2010-02-26"),
             DateTime.Parse("2010-05-31"), DateTime.Parse("2010-08-30"),
             DateTime.Parse("2010-11-30"), DateTime.Parse("2011-02-28"),
             DateTime.Parse("2011-05-30"), DateTime.Parse("2011-08-30"),
             DateTime.Parse("2011-11-30"), DateTime.Parse("2012-02-29"),
             DateTime.Parse("2012-05-30"), DateTime.Parse("2012-08-30"),
             DateTime.Parse("2012-11-30"), DateTime.Parse("2013-02-28"),
             DateTime.Parse("2013-05-30"), DateTime.Parse("2013-08-30"),
             DateTime.Parse("2013-11-29"), DateTime.Parse("2014-02-28"),
             DateTime.Parse("2014-05-30"), DateTime.Parse("2014-08-29"),
             DateTime.Parse("2014-11-28"), DateTime.Parse("2015-02-27"),
             DateTime.Parse("2015-05-29"), DateTime.Parse("2015-08-31"),
             DateTime.Parse("2015-11-30"), DateTime.Parse("2016-02-29"),
             DateTime.Parse("2016-05-30"), DateTime.Parse("2016-08-30"),
             DateTime.Parse("2016-11-30"), DateTime.Parse("2017-02-28"),
             DateTime.Parse("2017-05-30"), DateTime.Parse("2017-08-30"),
             DateTime.Parse("2017-11-30")};

        // Note: Length of the array that stores the discount factors
        // should equal the length of the dates array.
        private double[] _discountFactors =
            {0.999820d, 0.982090d, 0.964480d, 0.946940d, 0.929710d,
             0.912800d, 0.896170d, 0.879300d, 0.863230d, 0.848120d,
             0.832310d, 0.817290d, 0.802400d, 0.788390d, 0.774540d,
             0.760830d, 0.747420d, 0.734810d, 0.722500d, 0.710360d,
             0.698520d, 0.687360d, 0.676360d, 0.665530d, 0.655080d,
             0.644890d, 0.634950d, 0.625260d, 0.615810d, 0.606170d,
             0.596730d, 0.587200d, 0.578180d, 0.569360d, 0.560730d,
             0.552190d, 0.543840d, 0.535840d, 0.527930d, 0.520110d,
             0.512450d};

        // Note: Length of the array that stores the forward rates
        // should be one less than the dates array.
        private double[] _forwardRates =
            {7.24000d/100d, 7.32400d/100d, 7.43000d/100d, 7.43300d/100d,
             7.43200d/100d, 7.44000d/100d, 7.45000d/100d, 7.46900d/100d,
             7.39100d/100d, 7.37400d/100d, 7.36900d/100d, 7.36400d/100d,
             7.20500d/100d, 7.17600d/100d, 7.14700d/100d, 7.11800d/100d,
             6.88500d/100d, 6.83200d/100d, 6.77900d/100d, 6.72600d/100d,
             6.58300d/100d, 6.52200d/100d, 6.46100d/100d, 6.39900d/100d,
             6.33800d/100d, 6.27700d/100d, 6.21500d/100d, 6.15400d/100d,
             6.38300d/100d, 6.34200d/100d, 6.30000d/100d, 6.25800d/100d,
             6.21700d/100d, 6.17500d/100d, 6.13400d/100d, 6.09200d/100d,
             6.05100d/100d, 6.01000d/100d, 5.96800d/100d, 5.92700d/100d};    

        #endregion

        #region SetUp

        public CapletBootstrapEngineHelperTests()
        {
            const string CalculationDateString = @"2007-11-29";
            const string StartDateString = @"2007-11-30";
            const bool ValidateArguments = true;

            _calculationDate = DateTime.Parse(CalculationDateString);
            _capFrequency = CapFrequency.Quarterly;
            _capStartLag = 1;
            _currency = "AUD";
            _handle = "Caplet Bootstrap Engine Helper tests";
            _numYears = 10.0d;
            _parVolatilityInterpolation =
                ParVolatilityInterpolationType.CubicHermiteSpline;
            _rollConvention = BusinessDayConventionEnum.MODFOLLOWING;
            _startDate = DateTime.Parse(StartDateString);
            _tolerance = 1.0E-5d;
            _settingsObj = new CapletBootstrapSettings
                (_calculationDate,
                 _capFrequency,
                 _capStartLag,
                 _currency,
                 _handle,
                 _parVolatilityInterpolation,
                 _rollConvention,
                 ValidateArguments);

            // Initialise the offsets.
            _offsets = new List<double>();

            foreach (DateTime date in _dates)
            {
                TimeSpan dateDiff = date - _calculationDate;
                _offsets.Add(dateDiff.Days);
            }
        }

        #endregion

        #region Test: ComputeDiscountFactor Method

        /// <summary>
        /// Tests the method ComputeDiscountFactor.
        /// </summary>
        [TestMethod]
        public void TestComputeDiscountFactorMethod()
        {
            // Initialise the array of offsets.
            List<double> offsets = new List<double>();

            foreach (DateTime date in _dates)
            {
                TimeSpan dateDiff = date - _calculationDate;
                offsets.Add(dateDiff.Days);
            }

            #region Test: End Date Coincides with the Calculation Date

            _endDate = _calculationDate;
            _actual = (double)CapletBootstrapEngineHelper.ComputeDiscountFactor
                                  (_settingsObj, offsets.ToArray(), _discountFactors, _endDate);
            _expected = 1.0d;

            Assert.AreEqual(_expected, _actual);

            #endregion

            #region Test: End Date Coincides with a Nodal Date

            int idx = 0;
            foreach (DateTime date in _dates)
            {
                _endDate = date;
                _actual = (double)CapletBootstrapEngineHelper.ComputeDiscountFactor
                                      (_settingsObj, offsets.ToArray(), _discountFactors, _endDate);
                _expected = _discountFactors[idx];

                Assert.AreEqual(_expected, _actual);
                ++idx;
            }

            #endregion

            #region Test: End Date not at a Nodal Date

            string dateString = "2010-07-21";
            _endDate = DateTime.Parse(dateString);
            _actual = (double)CapletBootstrapEngineHelper.ComputeDiscountFactor
                                  (_settingsObj, offsets.ToArray(), _discountFactors, _endDate);
            _expected = 0.82389d;
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            #endregion
        }

        #endregion

        #region Test: ComputeDiscountFactor Method (OverLoaded Function)

        /// <summary>
        /// Tests the method ComputeDiscountFactor.
        /// </summary>
        [TestMethod]
        public void TestComputeDiscountFactorMethod1()
        {
            // Initialise the array of offsets.
            List<double> offsets = new List<double>();

            foreach (DateTime date in _dates)
            {
                TimeSpan dateDiff = date - _calculationDate;
                offsets.Add(dateDiff.Days);
            }

            #region Test: End Date Coincides with the Calculation Date
            _endDate = _calculationDate;
            _actual = (double)CapletBootstrapEngineHelper.ComputeDiscountFactor
                                  (_calculationDate, offsets.ToArray(), _discountFactors, _endDate);
            _expected = 1.0d;

            Assert.AreEqual(_expected, _actual);

            #endregion

            #region Test: End Date Coincides with a Nodal Date

            int idx = 0;
            foreach (DateTime date in _dates)
            {
                _endDate = date;
                _actual = (double)CapletBootstrapEngineHelper.ComputeDiscountFactor
                                      (_calculationDate, offsets.ToArray(), _discountFactors, _endDate);
                _expected = _discountFactors[idx];

                Assert.AreEqual(_expected, _actual);
                ++idx;
            }

            #endregion

            #region Test: End Date not at a Nodal Date

            string dateString = "2010-07-21";
            _endDate = DateTime.Parse(dateString);
            _actual = (double)CapletBootstrapEngineHelper.ComputeDiscountFactor
                                  (_calculationDate, offsets.ToArray(), _discountFactors, _endDate);
            _expected = 0.82389d;
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            #endregion

        }

        #endregion

        #region Test: ComputeForwardRate Method

        /// <summary>
        /// Tests the method ComputeForwardRate.
        /// </summary>
        [TestMethod]
        public void TestComputeForwardRate()
        {
            // Set the number of forward rates to be computed in the test.
            int numForwardRates = _forwardRates.Length;

            // Set test accuracy level.
            _tolerance = 1.0E-4d;

            // Compute each forward rate and test against expected rates.
            for (int i = 1; i < numForwardRates; ++i)
            {
                _actual = (double)CapletBootstrapEngineHelper.ComputeForwardRate
                                      (_settingsObj,
                                       _offsets.ToArray(),
                                       _discountFactors,
                                       _dates[i - 1],
                                       _dates[i]);

                _expected = _forwardRates[i - 1];

                Assert.AreEqual(_expected,
                                _actual,
                                _tolerance);
            }
        }

        #endregion

        #region Test: ComputeForwardRate Method (OverLoaded Function)

        /// <summary>
        /// Tests the method ComputeForwardRate.
        /// </summary>
        [TestMethod]
        public void TestComputeForwardRate1()
        {
            // Set the number of forward rates to be computed in the test.
            int numForwardRates = _forwardRates.Length;

            // Set test accuracy level.
            _tolerance = 1.0E-4d;

            // Compute each forward rate and test against expected rates.
            for (int i = 1; i < numForwardRates; ++i)
            {
                _actual = (double)CapletBootstrapEngineHelper.ComputeForwardRate
                                      ("ACT/365.FIXED",
                                       _offsets.ToArray(),
                                       _discountFactors,
                                       _calculationDate,
                                       _dates[i - 1],
                                       _dates[i]);

                _expected = _forwardRates[i - 1];

                Assert.AreEqual(_expected,
                                _actual,
                                _tolerance);
            }
        }

        #endregion

        #region Test: GenerateRollSchedule Method

        /// <summary>
        /// Tests the method GenerateRollSchedule.
        /// </summary>
        [TestMethod]
        public void TestGenerateRollSchedule()
        {    
            // Generate the roll schedule.
            CapletBootstrapEngineHelper.GenerateRollSchedule
                (_settingsObj, _numYears, out _rollSchedule);

            // Test date output.
            Assert.AreEqual(_dates.Length, _rollSchedule.Count);

            int idx = 0;
            foreach (DateTime date in _rollSchedule)
            {
                Assert.AreEqual(_dates[idx], date);
                ++idx;
            }
        }

        #endregion

        #region Test: GenerateUSDCapRollSchedule

        /// <summary>
        /// Tests the method GenerateRollSchedule for the case of a USD Cap.
        /// </summary>
        [TestMethod]
        public void TestGenerateUSDCapRollSchedule()
        {
            // Construct the settings object: only change the parameters that
            // are particular to this test case.
            const string CalculationDateString = @"2008-05-14";
            const bool ValidateArguments = true;

            _calculationDate = DateTime.Parse(CalculationDateString);
            _capStartLag = 2;
            _currency = "USD";
            _numYears = 20.0d;

            _settingsObj = new CapletBootstrapSettings
                (_calculationDate,
                 _capFrequency,
                 _capStartLag,
                 _currency,
                 _handle,
                 _parVolatilityInterpolation,
                 _rollConvention,
                 ValidateArguments);
            Assert.IsNotNull(_settingsObj);

            // Generate the roll schedule.
            CapletBootstrapEngineHelper.GenerateRollSchedule
                (_settingsObj, _numYears, out _rollSchedule);
       
            // Check the first and last dates in the roll schedule.
            const string FirstDateInSchedule = "2008-08-18";
            const string LastDateInSchedule = "2028-05-16";

            Assert.AreEqual
                (DateTime.Parse(FirstDateInSchedule), _rollSchedule[0]);
            Assert.AreEqual(DateTime.Parse(LastDateInSchedule),
                            _rollSchedule[_rollSchedule.Count - 1]);
        }

        #endregion
    }
}