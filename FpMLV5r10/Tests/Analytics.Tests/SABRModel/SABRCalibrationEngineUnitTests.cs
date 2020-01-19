#region Using Directives

using Orion.Analytics.PricingEngines;
using Orion.Analytics.Stochastics.SABR;
using Orion.Analytics.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Orion.Analytics.Tests.SABRModel
{
    [TestClass]
    public class SABRCalibrationEngineUnitTests
    {
        #region Private Fields

        private decimal _alpha; // storage for SABR parameter alpha
        private decimal _assetPrice; // ATM level of the relevant asset
        private decimal _atmVolatility; // ATM volatility
        private decimal _beta; // beta used in the calibration of the SABR model
        private SABRCalibrationEngine _calibrationEngine;
        private decimal _calibrationError; // error from full SABR calibration
        private SABRCalibrationSettings _calibrationSettings;
        private string _currency;
        private string _engineHandle; // handle for the engine object
        private SortedDictionary<SABRKey, SABRCalibrationEngine> _engineHandles; 
        private decimal _exerciseTime; // time to option exercise
        private string _expiry; // swaption expiry
        private InstrumentType.Instrument _instrument;
        private decimal _nu; // storage for SABR parameter nu
        private decimal _rho; // storage for SABR parameter rho
        private string _settingsHandle; // handle for settings object
        private List<decimal> _strikes; // strikes arranged in ascending order
        private string _tenor; // swap tenor
        private Dictionary<SABRKey, SABRCalibrationEngine> _unsorted;
        private List<decimal> _volatilities; // implied volatility at each strike

        private decimal _actual; // actual test result
        private decimal _expected; // expected test result
        private const decimal Tolerance = 1.0E-05m; // test accuracy

        #endregion Private Fields

        #region Private Helper Methods

        /// <summary>
        /// Helper function used to transfer values from
        /// an array to a particular private field.
        /// </summary>
        /// <param name="privateField">Private field that will store
        /// the array values, for example _strikes.</param>
        /// <param name="array">Array of values to transfer to the
        /// private field.</param>
        private static void SetList(ref List<decimal> privateField,
                                    ref decimal[] array)
        {
            // Initialise the private field.
            privateField?.Clear();
            privateField = new List<decimal>();
            // Transfer the values from the array to the container.
            foreach (decimal value in array)
            {
                privateField.Add(value);
            }
        }

        #endregion Private Helper Methods

        #region SetUp

        /// <summary>
        /// Set up method.
        /// </summary>
        [TestInitialize]
        public void Initialisation()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Engine Unit Test.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 0.85m;
            _calibrationError = 0.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 3.98m/100.0m;
            _engineHandle = _settingsHandle;
            _exerciseTime = 5.0m;

            decimal[] tempStrikes =
                {1.98m/100.0m, 2.23m/100.0m, 2.48m/100.0m, 2.73m/100.0m, 2.98m/100.0m,
                 3.23m/100.0m, 3.48m/100.0m, 3.73m/100.0m, 3.98m/100.0m, 4.23m/100.0m,
                 4.48m/100.0m, 4.73m/100.0m, 4.98m/100.0m, 5.23m/100.0m, 5.48m/100.0m,
                 5.73m/100.0m, 5.98m/100.0m};
            _strikes = new List<decimal>();
            foreach(decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {28.55583m/100.0m, 27.06073m/100.0m, 25.77301m/100.0m,
                 24.67008m/100.0m, 23.73767m/100.0m, 22.96630m/100.0m,
                 22.34849m/100.0m, 21.87638m/100.0m, 21.54000m/100.0m,
                 21.32656m/100.0m, 21.22078m/100.0m, 21.20602m/100.0m,
                 21.26568m/100.0m, 21.38436m/100.0m, 21.54860m/100.0m,
                 21.74715m/100.0m, 21.97088m/100.0m};

            _volatilities = new List<decimal>();
            foreach(decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           _strikes,
                                                           _volatilities,
                                                           _assetPrice,
                                                           _exerciseTime);

            // Initialise the collection of SABR calibration engine objects.
            _engineHandles = null;
        }

        #endregion SetUp

        #region Tests: Constructor

        /// <summary>
        /// Tests the class constructor.
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            Assert.IsNotNull(_calibrationEngine);
        }

        #endregion Tests: Constructor

        #region Tests: Full Calibration of the SABR Model

        /// <summary>
        /// Tests the method CalibrateSABRModel.
        /// </summary>
        [TestMethod]
        public void TestCalibrateSABRModel()
        {
            _calibrationEngine.CalibrateSABRModel();

            // Test: calibration status.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Test: Calculation of the ATM slope.
            _expected = -0.043708m;
            _actual = _calibrationEngine.ATMSlope;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: Initial guess for the transformed SABR parameter theta.  
            _expected = 2.094395m;
            _actual = _calibrationEngine.ThetaGuess;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: Initial guess for the transformed SABR parameter mu.  
            _expected = 0.331982m;
            _actual = _calibrationEngine.MuGuess;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: SABR parameter alpha.
            _expected = 12.60192m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Alpha;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: SABR parameter beta.
            _expected = 0.85m;
            _actual = _calibrationEngine.GetSABRParameters.Beta;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual));

            // Test: SABR parameter nu.
            _expected = 40.00m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Nu;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: SABR parameter rho.
            _expected = -13.00m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Rho;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));
        }

        #endregion Tests: Full Calibration of the SABR Model

        #region Tests: ATM Calibration of the SABR Model

        /// <summary>
        /// Tests the method CalibrateATMSABRModel.
        /// </summary>
        [TestMethod]
        public void TestCalibrateATMSABRModel()
        {
            // Override some variable set in the SetUp method.
            _assetPrice = 10.98m/100.0m;
            _exerciseTime = 3.0m;

            // Initialise information required specifically for an ATM
            // calibration..
            decimal atmVolatility = 11.54m/100.0m;
            decimal nu = 10.45m/100.0m;
            decimal rho = -47.0m/100.0m;

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           nu,
                                                           rho,
                                                           atmVolatility,
                                                           _assetPrice,
                                                           _exerciseTime);
            _calibrationEngine.CalibrateATMSABRModel();

            // Test: calibration status.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Test: SABR parameter alpha.
            _expected = 8.29966m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Alpha;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: SABR parameter beta.
            _expected = 0.85m;
            _actual = _calibrationEngine.GetSABRParameters.Beta;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter nu.
            _expected = 10.45m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Nu;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter rho.
            _expected = -47.00m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Rho;
            Assert.AreEqual(_expected, _actual);
        }

        #endregion Tests: ATM Calibration of the SABR Model

        #region Test: Market Case #1

        /// <summary>
        /// Test calibration for Market Test Case #1.
        /// </summary>
        [TestMethod]
        public void MarketTestCase1()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Market Test Case 1.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise the SABR calibration engine object.
            _assetPrice = 6.6961m/100.0m;
            _engineHandle = _settingsHandle;
            _exerciseTime = 3.0m;

            decimal[] tempStrikes =
                {5.6961m/100.0m, 5.9461m/100.0m, 6.1961m/100.0m, 6.4461m/100.0m,
                 6.6961m/100.0m, 6.9461m/100.0m, 7.1961m/100.0m, 7.4461m/100.0m,
                 7.6961m/100.0m};
            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {10.72m/100.0m, 10.47m/100.0m, 10.26m/100.0m, 10.09m/100.0m,
                 10.00m/100.0m, 9.93m/100.0m, 9.83m/100.0m, 9.96m/100.0m, 10.08m/100.0m};

            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }
            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           _strikes,
                                                           _volatilities,
                                                           _assetPrice,
                                                           _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            // Calibrate SABR model.
            _calibrationEngine.CalibrateSABRModel();
            // Retrieve calibration results.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            _alpha = _calibrationEngine.GetSABRParameters.Alpha;
            _beta = _calibrationEngine.GetSABRParameters.Beta;
            _nu = _calibrationEngine.GetSABRParameters.Nu;
            _rho = _calibrationEngine.GetSABRParameters.Rho;
            _calibrationError = _calibrationEngine.CalibrationError;

            // Test SABR parameter alpha.
            _expected = 0.0979081389558388m; //9.79303m /100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_alpha),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter beta.
            _expected = 1.0m;
            Assert.AreEqual(_expected, _beta);

            // Test SABR parameter nu.
            _expected = 0.3180161722999501681537136293M;//31.56066m /100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_nu),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter rho.
            _expected = -0.137205626677806M;//- 13.3719m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_rho),
                            decimal.ToDouble(Tolerance));

            // Test calibration error.
            _expected = 1.37051E-06m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_calibrationError),
                            decimal.ToDouble(Tolerance));
        }

        #endregion Test: Market Case #1

        #region Test: Market Case #2

        /// <summary>
        /// Test calibration for Market Test Case #2.
        /// </summary>
        [TestMethod]
        public void MarketTestCase2()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Market Test Case 2.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 6.3395m/100.0m;
            _engineHandle = _settingsHandle;
            _exerciseTime = 5.0m;

            decimal[] tempStrikes =
                {5.3395m/100.0m, 5.5895m/100.0m, 5.8395m/100.0m, 6.0895m/100.0m,
                 6.3395m/100.0m, 6.5895m/100.0m, 6.8395m/100.0m, 7.0895m/100.0m,
                 7.3395m/100.0m};

            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {12.10m/100.0m, 11.25m/100.0m, 10.69m/100.0m, 10.37m/100.0m,
                 10.15m/100.0m, 9.91m/100.0m, 9.84m/100.0m, 9.98m/100.0m, 10.23m/100.0m};

            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           _strikes,
                                                           _volatilities,
                                                           _assetPrice,
                                                           _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);

            // Calibrate SABR model.
            _calibrationEngine.CalibrateSABRModel();

            // Retrieve calibration results.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            _alpha = _calibrationEngine.GetSABRParameters.Alpha;
            _beta = _calibrationEngine.GetSABRParameters.Beta;
            _nu = _calibrationEngine.GetSABRParameters.Nu;
            _rho = _calibrationEngine.GetSABRParameters.Rho;

            // Test SABR parameter alpha.
            _expected = 9.46252m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_alpha),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter beta.
            _expected = 1.0m;
            Assert.AreEqual(_expected, _beta);

            // Test SABR parameter nu.
            _expected = 47.23717m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_nu),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter rho.
            _expected = -23.0769m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_rho),
                            decimal.ToDouble(Tolerance));
        }

        #endregion Test: Market Case #2

        #region Test: Extreme Case #1

        /// <summary>
        /// Test calibration for Market Test Case #3.
        /// </summary>
        [TestMethod]
        public void MarketTestCase3()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Market Test Case 3.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 6.3395m/100.0m;
 
            _engineHandle = _settingsHandle;
            _exerciseTime = 10.0m;

            decimal[] tempStrikes =
                {4.4000m/100.0m, 5.0000m/100.0m, 5.6000m/100.0m, 6.0000m/100.0m,
                 6.3395m/100.0m, 7.0000m/100.0m, 7.6000m/100.0m, 8.3000m/100.0m,
                 9.3000m/100.0m};
            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {24.67m/100.0m, 24.67m/100.0m, 24.67m/100.0m, 24.67m/100.0m,
                 24.67m/100.0m, 24.67m/100.0m, 24.67m/100.0m, 24.67m/100.0m,
                 24.67m/100.0m};

            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           _strikes,
                                                           _volatilities,
                                                           _assetPrice,
                                                           _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);

            // Calibrate SABR model.
            _calibrationEngine.CalibrateSABRModel();

            // Retrieve calibration results.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            _alpha = _calibrationEngine.GetSABRParameters.Alpha;
            _beta = _calibrationEngine.GetSABRParameters.Beta;
            _nu = _calibrationEngine.GetSABRParameters.Nu;
            _rho = _calibrationEngine.GetSABRParameters.Rho;

            // Test SABR parameter alpha.
            _expected = 24.6700m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_alpha),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter beta.
            _expected = 1.0m;
            Assert.AreEqual(_expected, _beta);

            // Test SABR parameter nu.
            _expected = 0.00002m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_nu),
                            decimal.ToDouble(Tolerance));
            // Test SABR parameter rho.
            _expected = -0.000000703460156731058M;//7.6076m /100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_rho),
                            decimal.ToDouble(Tolerance));
        }

        #endregion Test: Extreme Case #1

        #region Test: Markit Case 1Y1Y

        /// <summary>
        /// Test calibration for Markit Case 1Y1Y.
        /// </summary>
        [TestMethod]
        public void TestMarketCase1Y1Y()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Markit Case 1Y1Y.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 7.34m/100.0m;
            _engineHandle = _settingsHandle;
            _exerciseTime = 1.0m;

            decimal[] tempStrikes =
                {5.80m/100.0m, 6.30m/100.0m, 6.70m/100.0m, 7.10m/100.0m, 7.40m/100.0m,
                 7.70m/100.0m, 8.10m/100.0m, 8.60m/100.0m, 9.20m/100.0m};
            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {10.96m/100.0m, 10.42m/100.0m, 10.03m/100.0m, 9.70m/100.0m, 9.56m/100.0m,
                 9.46m/100.0m, 9.33m/100.0m, 9.43m/100.0m, 9.62m/100.0m};

            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           _strikes,
                                                           _volatilities,
                                                           _assetPrice,
                                                           _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);

            // Calibrate SABR model.
            _calibrationEngine.CalibrateSABRModel();

            // Retrieve calibration results.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            _alpha = _calibrationEngine.GetSABRParameters.Alpha;
            _beta = _calibrationEngine.GetSABRParameters.Beta;
            _nu = _calibrationEngine.GetSABRParameters.Nu;
            _rho = _calibrationEngine.GetSABRParameters.Rho;

            // Test SABR parameter alpha.
            _expected = 9.50704m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_alpha),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter beta.
            _expected = 1.0m;
            Assert.AreEqual(_expected, _beta);

            // Test SABR parameter nu.
            _expected = 0.305819946712337m;//30.58399m /100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_nu),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter rho.
            _expected = -22.4762m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_rho),
                            decimal.ToDouble(Tolerance));
        }

        #endregion Test: Markit Case 1Y1Y

        #region Test: Markit Case 5Y5Y

        /// <summary>
        /// Test calibration for Markit Case 5Y5Y.
        /// </summary>
        [TestMethod]
        public void TestMarketCase5Y5Y()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Markit Case 5Y5Y.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 6.49m/100.0m;
            _engineHandle = _settingsHandle;
            _exerciseTime = 5.0m;

            decimal[] tempStrikes =
                {4.30m/100.0m, 5.00m/100.0m, 5.60m/100.0m, 6.00m/100.0m, 6.50m/100.0m,
                 7.00m/100.0m, 7.70m/100.0m, 8.40m/100.0m, 9.50m/100};
            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {11.92m/100.0m, 10.91m/100.0m, 10.25m/100.0m, 9.94m/100.0m, 9.63m/100.0m,
                 9.45m/100.0m, 9.36m/100.0m, 9.45m/100.0m, 9.75m/100.0m};

            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           _strikes,
                                                           _volatilities,
                                                           _assetPrice,
                                                           _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);

            // Calibrate SABR model.
            _calibrationEngine.CalibrateSABRModel();

            // Retrieve calibration results.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            _alpha = _calibrationEngine.GetSABRParameters.Alpha;
            _beta = _calibrationEngine.GetSABRParameters.Beta;
            _nu = _calibrationEngine.GetSABRParameters.Nu;
            _rho = _calibrationEngine.GetSABRParameters.Rho;

            // Test SABR parameter alpha.
            _expected = 9.50814m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_alpha),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter beta.
            _expected = 1.0m;
            Assert.AreEqual(_expected, _beta);

            // Test SABR parameter nu.
            _expected = 23.50254m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_nu),
                            decimal.ToDouble(Tolerance));

            // Test SABR parameter rho.
            _expected = -27.3013m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_rho),
                            decimal.ToDouble(Tolerance));
        }

        #endregion Test: Markit Case 5Y5Y

        #region Test: Markit Case 6M10Y

        /// <summary>
        /// Test calibration for Markit Case 6M10Y.
        /// </summary>
        [TestMethod]
        public void TestMarketCase6M10Y()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Markit Case 6M10Y.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 0.7m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 6.8805m/100.0m;
            _engineHandle = _settingsHandle;
            _exerciseTime = 0.5m;

            decimal[] tempStrikes =
                {6.00m/100.0m, 6.30m/100.0m, 6.50m/100.0m, 6.70m/100.0m, 6.90m/100.0m,
                 7.10m/100.0m, 7.30m/100.0m, 7.60m/100.0m, 8.00m/100.0m};
            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {10.99m/100.0m, 10.69m/100.0m, 10.53m/100.0m, 10.40m/100.0m,
                 10.31m/100.0m, 10.20m/100.0m, 10.21m/100.0m, 10.21m/100.0m,
                 10.31m/100.0m};

            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           _strikes,
                                                           _volatilities,
                                                           _assetPrice,
                                                           _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);

            // Calibrate SABR model.
            _calibrationEngine.CalibrateSABRModel();

            // Retrieve calibration status.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);            
        }

        #endregion Test: Markit Case 6M10Y

        #region Test: Markit Case 1Y1Y Prediction

        /// <summary>
        /// Tests the ATM calibration for Markit Case 1Y1Y.
        /// </summary>
        [TestMethod]
        public void TestMarketCase1Y1YPrediction()
        {
            // Override some variables set in the SetUp method.
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Engine Unit Test.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            _assetPrice = 7.4377m/100.0m;
            _exerciseTime = 1.0m;

            // Initialise information required specifically for an ATM
            // calibration..
            decimal atmVolatility = 10.66m/100.0m;
            decimal nu = 30.58399m/100.0m;
            decimal rho = -22.4762m/100.0m;

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           nu,
                                                           rho,
                                                           atmVolatility,
                                                           _assetPrice,
                                                           _exerciseTime);
            _calibrationEngine.CalibrateATMSABRModel();

            // Test: calibration status.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Test: SABR parameter alpha.
            _expected = 10.60293m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Alpha;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: SABR parameter beta.
            _expected = 1.0m;
            _actual = _calibrationEngine.GetSABRParameters.Beta;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter nu.
            _expected = 30.58399m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Nu;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter rho.
            _expected = -22.4762m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Rho;
            Assert.AreEqual(_expected, _actual);
        }

        #endregion Test: Markit Case 1Y1Y Prediction

        #region Test: Markit Case 5Y5Y Prediction

        /// <summary>
        /// Tests the ATM calibration for Markit Case 5Y5Y.
        /// </summary>
        [TestMethod]
        public void TestMarketCase5Y5YPrediction()
        {
            // Override some variables set in the SetUp method.
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Engine Unit Test.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            _assetPrice = 6.3592m/100.0m;
            _exerciseTime = 5.0m;

            // Initialise information required specifically for an ATM
            // calibration..
            decimal atmVolatility = 10.34m/100.0m;
            decimal nu = 23.50254m/100.0m;
            decimal rho = -27.3013m/100.0m;

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           nu,
                                                           rho,
                                                           atmVolatility,
                                                           _assetPrice,
                                                           _exerciseTime);
            _calibrationEngine.CalibrateATMSABRModel();

            // Test: calibration status.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Test: SABR parameter alpha.
            _expected = 10.21488m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Alpha;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: SABR parameter beta.
            _expected = 1.0m;
            _actual = _calibrationEngine.GetSABRParameters.Beta;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter nu.
            _expected = 23.50254m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Nu;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter rho.
            _expected = -27.3013m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Rho;
            Assert.AreEqual(_expected, _actual);
        }

        #endregion Test: Markit Case 5Y5Y Prediction

        #region Test: Markit Case 6M10Y Prediction

        /// <summary>
        /// Tests the ATM calibration for Markit Case 6M10Y.
        /// </summary>
        [TestMethod]
        public void TestMarketCase6M10YPrediction()
        {
            // Override some variables set in the SetUp method.
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Engine Unit Test.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 0.95m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);
            _assetPrice = 6.8791m/100.0m;
            _exerciseTime = 0.5m;

            // Initialise information required specifically for an ATM
            // calibration..
            decimal atmVolatility = 12.10m/100.0m;
            decimal nu = 30.0m/100.0m;
            decimal rho = -15.0m/100.0m;

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           nu,
                                                           rho,
                                                           atmVolatility,
                                                           _assetPrice,
                                                           _exerciseTime);
            _calibrationEngine.CalibrateATMSABRModel();

            // Test: calibration status.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Test: SABR parameter alpha.
            _expected = 10.55285m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Alpha;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));
            // Test: SABR parameter beta.
            _expected = 0.95m;
            _actual = _calibrationEngine.GetSABRParameters.Beta;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter nu.
            _expected = 30.0m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Nu;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter rho.
            _expected = -15.0m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Rho;
            Assert.AreEqual(_expected, _actual);
        }

        #endregion Test: Markit Case 6M10Y Prediction

        #region Test: Expiry Interpolation

        /// <summary>
        /// Tests the method CalibrateInterpSABRModel.
        /// </summary>
        [TestMethod]
        public void TestCalibrateInterpSABRModel()
        {
            #region SABR Settings Object

            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Interpolated 2Y5Y Settings";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings =
                new SABRCalibrationSettings(_settingsHandle,
                                            _instrument,
                                            _currency,
                                            _beta);

            Assert.IsNotNull(_calibrationSettings);

            #endregion SABR Settings Object

            #region Container for Calibration Engine Objects

            // Initialise the container that will store the calibration engine
            // objects.
            _unsorted = new Dictionary<SABRKey, SABRCalibrationEngine>
                (new SABRKey());
            _engineHandles =
                new SortedDictionary<SABRKey, SABRCalibrationEngine>
                    (_unsorted, new SABRKey());  

            Assert.IsNotNull(_engineHandles);

            #endregion Container for Calibration Engine Objects

            #region 6M1Y Calibration Engine

            // ***6M1Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 6M1Y Expiry";
            _exerciseTime = 0.5m;
            decimal[] strikes6M1Y =
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes6M1Y);
            decimal[] volatilities6M1Y =
                {10.50m/100.0m, 10.30m/100.0m, 10.04m/100.0m, 9.87m/100.0m, 9.77m/100.0m,
                 9.65m/100.0m, 9.61m/100.0m, 9.61m/100.0m, 9.69m/100.0m};
            SetList(ref _volatilities, ref volatilities6M1Y);

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           _strikes,
                                                           _volatilities,
                                                           _assetPrice,
                                                           _exerciseTime);
            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.7439m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.292347482884482m), //29.2376m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.192814603335497m), //-19.2777m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "6M";
            _tenor = "1Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 6M1Y Calibration Engine

            #region 6M2Y Calibration Engine

            // ***6M2Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 6M2Y Expiry";
            _exerciseTime = 0.5m;
            decimal[] strikes6M2Y =
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes6M2Y);
            decimal[] volatilities6M2Y =
                {10.68m/100.0m, 10.40m/100.0m, 10.16m/100.0m, 10.03m/100.0m, 9.94m/100.0m,
                 9.83m/100.0m, 9.79m/100.0m, 9.78m/100.0m, 9.83m/100.0m};
            SetList(ref _volatilities, ref volatilities6M2Y);
            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.9157m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(28.1943m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.1934892520133m),//-19.3478m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "6M";
            _tenor = "2Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 6M2Y Calibration Engine

            #region 6M3Y Calibration Engine

            // ***6M3Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 6M3Y Expiry";
            _exerciseTime = 0.5m;
            decimal[] strikes6M3Y=
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes6M3Y);
            decimal[] volatilities6M3Y =
                {10.86m/100.0m, 10.60m/100.0m, 10.40m/100.0m, 10.25m/100.0m,
                 10.14m/100.0m, 10.05m/100.0m, 10.01m/100.0m, 10.01m/100.0m,
                 10.06m/100.0m};
            SetList(ref _volatilities, ref volatilities6M3Y);
            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(10.1122m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.292800534713424m), //29.2816m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.177800192408585m), //-17.7787m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "6M";
            _tenor = "3Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 6M3Y Calibration Engine

            #region 6M5Y Calibration Engine

            // ***6M5Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 6M5Y Expiry";
            _exerciseTime = 0.5m;
            decimal[] strikes6M5Y =
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes6M5Y);
            decimal[] volatilities6M5Y =
                {10.82m/100.0m, 10.60m/100.0m, 10.39m/100.0m, 10.26m/100.0m,
                 10.16m/100.0m, 10.02m/100.0m, 9.99m/100.0m, 10.00m/100.0m,
                 10.07m/100.0m};
            SetList(ref _volatilities, ref volatilities6M5Y);
            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.101410554909594m), //10.1375m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.253912966113547m), //26.9834m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.202748825819209m), //-19.0245m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "6M";
            _tenor = "5Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 6M5Y Calibration Engine

            #region 6M7Y Calibration Engine

            // ***6M7Y SABR calibration engine object***.
            _assetPrice = 6.7000m/100.0m;
            _engineHandle = "SABR Full Calibration 6M7Y Expiry";
            _exerciseTime = 0.5m;
            decimal[] strikes6M7Y =
                {5.70m/100.0m, 5.95m/100.0m, 6.20m/100.0m, 6.45m/100.0m, 6.70m/100.0m,
                 6.95m/100.0m, 7.20m/100.0m, 7.45m/100.0m, 7.70m/100.0m};
            SetList(ref _strikes, ref strikes6M7Y);
            decimal[] volatilities6M7Y =
                {10.94m/100.0m, 10.81m/100.0m, 10.56m/100.0m, 10.41m/100.0m,
                 10.31m/100.0m, 10.15m/100.0m, 10.12m/100.0m, 10.13m/100.0m,
                 10.20m/100.0m};
            SetList(ref _volatilities, ref volatilities6M7Y);
            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.102926592217094m), //10.2896m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.247562036826741m), //26.1584m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.216446106499739m), //-20.4091m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "6M";
            _tenor = "7Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 6M7Y Calibration Engine

            #region 6M10Y Calibration Engine

            // ***6M10Y SABR calibration engine object***.
            _assetPrice = 6.6000m/100.0m;
            _engineHandle = "SABR Full Calibration 6M10Y Expiry";
            _exerciseTime = 0.5m;

            decimal[] strikes6M10Y =
                {5.60m/100.0m, 5.85m/100.0m, 6.10m/100.0m, 6.35m/100.0m, 6.60m/100.0m,
                 6.85m/100.0m, 7.10m/100.0m, 7.35m/100.0m, 7.60m/100.0m};
            SetList(ref _strikes, ref strikes6M10Y);
            decimal[] volatilities6M10Y =
                {11.15m/100.0m, 10.92m/100.0m, 10.74m/100.0m, 10.62m/100.0m, 
                 10.52m/100.0m, 10.31m/100.0m, 10.27m/100.0m, 10.29m/100.0m,
                 10.38m/100.0m};
            SetList(ref _volatilities, ref volatilities6M10Y);
            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.105107977253192m), //10.5062m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.206314165296761m), //22.9787m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.263277475328497m), //-23.2923m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "6M";
            _tenor = "10Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 6M10Y Calibration Engine

            #region 1Y1Y Calibration Engine

            // ***1Y1Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 1Y1Y Expiry";
            _exerciseTime = 1.0m;
            decimal[] strikes1Y1Y =
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes1Y1Y);
            decimal[] volatilities1Y1Y =
                {10.75m/100.0m, 10.51m/100.0m, 10.27m/100.0m, 10.07m/100.0m, 9.95m/100.0m,
                 9.84m/100.0m, 9.77m/100.0m, 9.78m/100.0m, 9.84m/100.0m};
            SetList(ref _volatilities, ref volatilities1Y1Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.8934m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(30.5073m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.204556453614928m), //-20.4544m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "1Y";
            _tenor = "1Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 1Y1Y Calibration Engine

            #region 1Y2Y Calibration Engine

            // ***1Y2Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 1Y2Y Expiry";
            _exerciseTime = 1.0m;
            decimal[] strikes1Y2Y =
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes1Y2Y);
            decimal[] volatilities1Y2Y =
                {10.73m/100.0m, 10.46m/100.0m, 10.24m/100.0m, 10.10m/100.0m,
                 10.00m/100.0m, 9.91m/100.0m, 9.86m/100.0m, 9.86m/100.0m,
                 9.91m/100.0m};
            SetList(ref _volatilities, ref volatilities1Y2Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.9479m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(28.7692m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble (-18.3291m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));


            _expiry = "1Y";
            _tenor = "2Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 1Y2Y Calibration Engine

            #region 1Y3Y Calibration Engine

            // ***1Y3Y SABR calibration engine object***.
            _assetPrice = 6.9000m/100.0m;
            _engineHandle = "SABR Full Calibration 1Y3Y Expiry";
            _exerciseTime = 1.0m;
            decimal[] strikes1Y3Y =
                {5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m, 6.65m/100.0m, 6.90m/100.0m,
                 7.15m/100.0m, 7.40m/100.0m, 7.65m/100.0m, 7.90m/100.0m};
            SetList(ref _strikes, ref strikes1Y3Y);
            decimal[] volatilities1Y3Y =
                {10.63m/100.0m, 10.40m/100.0m, 10.20m/100.0m, 10.06m/100.0m, 9.98m/100.0m,
                 9.93m/100.0m, 9.92m/100.0m, 9.92m/100.0m, 9.95m/100.0m};
            SetList(ref _volatilities, ref volatilities1Y3Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.0991672671651654m), //9.9202m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.303922160289479m), //29.5793m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.143308991384418m), //-14.2396m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));


            _expiry = "1Y";
            _tenor = "3Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 1Y3Y Calibration Engine

            #region 1Y4Y Calibration Engine

            // ***1Y4Y SABR calibration engine object***.
            _assetPrice = 6.9000m/100.0m;
            _engineHandle = "SABR Full Calibration 1Y4Y Expiry";
            _exerciseTime = 1.0m;
            decimal[] strikes1Y4Y =
                {5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m, 6.65m/100.0m, 6.90m/100.0m,
                 7.15m/100.0m, 7.40m/100.0m, 7.65m/100.0m, 7.90m/100.0m};
            SetList(ref _strikes, ref strikes1Y4Y);
            decimal[] volatilities1Y4Y =
                {10.62m/100.0m, 10.40m/100.0m, 10.21m/100.0m, 10.06m/100.0m, 9.96m/100.0m,
                 9.90m/100.0m, 9.85m/100.0m, 9.84m/100.0m, 9.88m/100.0m};
            SetList(ref _volatilities, ref volatilities1Y4Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.0991200081487066m), //9.9089m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.278201408518728m),//28.2686m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.185063734162736m),//-17.2644m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "1Y";
            _tenor = "4Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 1Y4Y Calibration Engine

            #region 1Y5Y Calibration Engine

            // ***1Y5Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 1Y5Y Expiry";
            _exerciseTime = 1.0m;
            decimal[] strikes1Y5Y =
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes1Y5Y);
            decimal[] volatilities1Y5Y =
                {10.75m/100.0m, 10.47m/100.0m, 10.28m/100.0m, 10.13m/100.0m,
                 10.04m/100.0m, 9.96m/100.0m, 9.92m/100.0m, 9.92m/100.0m,
                 9.96m/100.0m};
            SetList(ref _volatilities, ref volatilities1Y5Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated); 
            Assert.AreEqual(decimal.ToDouble(0.0999539679554883m),//9.9872m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.270881536782241m),//28.6334m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.193417522642517m),//-17.3229m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "1Y";
            _tenor = "5Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 1Y5Y Calibration Engine

            #region 1Y7Y Calibration Engine

            // ***1Y7Y SABR calibration engine object***.
            _assetPrice = 6.7000m/100.0m;
            _engineHandle = "SABR Full Calibration 1Y7Y Expiry";
            _exerciseTime = 1.0m;
            decimal[] strikes1Y7Y =
                {5.70m/100.0m, 5.95m/100.0m, 6.20m/100.0m, 6.45m/100.0m, 6.70m/100.0m,
                 6.95m/100.0m, 7.20m/100.0m, 7.45m/100.0m, 7.70m/100.0m};
            SetList(ref _strikes, ref strikes1Y7Y);
            decimal[] volatilities1Y7Y =
                {10.79m/100.0m, 10.56m/100.0m, 10.39m/100.0m, 10.24m/100.0m,
                 10.14m/100.0m, 10.05m/100.0m, 10.02m/100.0m, 10.00m/100.0m,
                 10.03m/100.0m};
            SetList(ref _volatilities, ref volatilities1Y7Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated); 
            Assert.AreEqual(decimal.ToDouble(0.101040189093224m),//10.0970m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.247027423988029m),//26.3490m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.19687581564957m),//-18.4560m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "1Y";
            _tenor = "7Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 1Y7Y Calibration Engine

            #region 1Y10Y Calibration Engine

            // ***1Y10Y SABR calibration engine object***.
            _assetPrice = 6.6000m/100.0m;
            _engineHandle = "SABR Full Calibration 1Y10Y Expiry";
            _exerciseTime = 1.0m;
            decimal[] strikes1Y10Y =
                {5.60m/100.0m, 5.85m/100.0m, 6.10m/100.0m, 6.35m/100.0m, 6.60m/100.0m,
                 6.85m/100.0m, 7.10m/100.0m, 7.35m/100.0m, 7.60m/100.0m};
            SetList(ref _strikes, ref strikes1Y10Y);
            decimal[] volatilities1Y10Y =
                {10.98m/100.0m, 10.72m/100.0m, 10.51m/100.0m, 10.36m/100.0m,
                 10.24m/100.0m, 10.07m/100.0m, 10.04m/100.0m, 10.05m/100.0m,
                 10.10m/100.0m};
            SetList(ref _volatilities, ref volatilities1Y10Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated); 
            Assert.AreEqual(decimal.ToDouble(0.10200540534001m),//10.1978m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.264640430519687m),//26.9108m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.225579868477239m),//-21.5256m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "1Y";
            _tenor = "10Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 1Y10Y Calibration Engine

            #region 2Y3Y Calibration Engine

            // ***2Y3Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 2Y3Y Expiry";
            _exerciseTime = 2.0m;
            decimal[] strikes2Y3Y =
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes2Y3Y);
            decimal[] volatilities2Y3Y =
                {10.56m/100.0m, 10.40m/100.0m, 10.21m/100.0m, 10.08m/100.0m,
                 10.00m/100.0m, 9.93m/100.0m, 9.88m/100.0m, 9.88m/100.0m,
                 9.95m/100.0m};
            SetList(ref _volatilities, ref volatilities2Y3Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);//
            Assert.AreEqual(decimal.ToDouble(0.0993275243454278m),//9.9157m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.237487249127005m),//25.6768m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.182950177473184m),//-16.1639m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "2Y";
            _tenor = "3Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 2Y3Y Calibration Engine

            #region 3Y1Y Calibration Engine

            // ***3Y1Y SABR calibration engine object***.
            _assetPrice = 6.9000m/100.0m;
            _engineHandle = "SABR Full Calibration 3Y1Y Expiry";
            _exerciseTime = 3.0m;
            decimal[] strikes3Y1Y =
                {5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m, 6.65m/100.0m, 6.90m/100.0m,
                 7.15m/100.0m, 7.40m/100.0m, 7.65m/100.0m, 7.90m/100.0m};
            SetList(ref _strikes, ref strikes3Y1Y);
            decimal[] volatilities3Y1Y =
                {10.65m/100.0m, 10.40m/100.0m, 10.20m/100.0m, 10.07m/100.0m,
                 9.97m/100.0m, 9.92m/100.0m, 9.80m/100.0m, 9.96m/100.0m, 9.73m/100.0m};
            SetList(ref _volatilities, ref volatilities3Y1Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated); 
            Assert.AreEqual(decimal.ToDouble(0.0986809044979173m),//9.8503m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.247160865171269m),//26.1358m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.213331791935168m),//- 20.1373m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "3Y";
            _tenor = "1Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 3Y1Y Calibration Engine

            #region 3Y2Y Calibration Engine

            // ***3Y2Y SABR calibration engine object***.
            _assetPrice = 6.8000m/100.0m;
            _engineHandle = "SABR Full Calibration 3Y2Y Expiry";
            _exerciseTime = 3.0m;
            decimal[] strikes3Y2Y =
                {5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m, 6.55m/100.0m, 6.80m/100.0m,
                 7.05m/100.0m, 7.30m/100.0m, 7.55m/100.0m, 7.80m/100.0m};
            SetList(ref _strikes, ref strikes3Y2Y);
            decimal[] volatilities3Y2Y =
                {10.72m/100.0m, 10.47m/100.0m, 10.26m/100.0m, 10.09m/100.0m,
                 10.00m/100.0m, 9.93m/100.0m, 9.83m/100.0m, 9.96m/100.0m, 10.08m/100.0m};
            SetList(ref _volatilities, ref volatilities3Y2Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated); 
            Assert.AreEqual(decimal.ToDouble(0.0980140528188413m),//9.7863m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.312767514384571m),//32.0513m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.14763715723534m),//-13.3892m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "3Y";
            _tenor = "2Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 3Y2Y Calibration Engine

            #region 3Y3Y Calibration Engine

            // ***3Y3Y SABR calibration engine object***.
            _assetPrice = 6.7000m/100.0m;
            _engineHandle = "SABR Full Calibration 3Y3Y Expiry";
            _exerciseTime = 3.0m;
            decimal[] strikes3Y3Y =
                {5.70m/100.0m, 5.95m/100.0m, 6.20m/100.0m, 6.45m/100.0m, 6.70m/100.0m,
                 6.95m/100.0m, 7.20m/100.0m, 7.45m/100.0m, 7.70m/100.0m};
            SetList(ref _strikes, ref strikes3Y3Y);
            decimal[] volatilities3Y3Y =
                {10.73m/100.0m, 10.48m/100.0m, 10.28m/100.0m, 10.13m/100.0m,
                 10.02m/100.0m, 9.95m/100.0m, 9.85m/100.0m, 9.92m/100.0m,
                 9.79m/100.0m};
            SetList(ref _volatilities, ref volatilities3Y3Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.0989369728394519m),//9.9100m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.269146459470764m),//25.6385m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.2084686487965m),//-21.8147m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "3Y";
            _tenor = "3Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 3Y3Y Calibration Engine

            #region 3Y5Y Calibration Engine

            // ***3Y5Y SABR calibration engine object***.
            _assetPrice = 6.6000m/100.0m;
            _engineHandle = "SABR Full Calibration 3Y5Y Expiry";
            _exerciseTime = 3.0m;
            decimal[] strikes3Y5Y =
                {5.60m/100.0m, 5.85m/100.0m, 6.10m/100.0m, 6.35m/100.0m, 6.60m/100.0m,
                 6.85m/100.0m, 7.10m/100.0m, 7.35m/100.0m, 7.60m/100.0m};
            SetList(ref _strikes, ref strikes3Y5Y);
            decimal[] volatilities3Y5Y =
                {10.71m/100.0m, 10.42m/100.0m, 10.21m/100.0m, 10.06m/100.0m, 9.96m/100.0m,
                 9.87m/100.0m, 9.77m/100.0m, 9.87m/100.0m, 9.69m/100.0m};
            SetList(ref _volatilities, ref volatilities3Y5Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.0987685841296389m),//9.8591m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.236561689524296m),//25.1257m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.245704161731177m),//-23.0718m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "3Y";
            _tenor = "5Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 3Y5Y Calibration Engine

            #region 3Y7Y Calibration Engine

            // ***3Y7Y SABR calibration engine object***.
            _assetPrice = 6.5000m/100.0m;
            _engineHandle = "SABR Full Calibration 3Y7Y Expiry";
            _exerciseTime = 3.0m;
            decimal[] strikes3Y7Y =
                {5.50m/100.0m, 5.75m/100.0m, 6.00m/100.0m, 6.25m/100.0m, 6.50m/100.0m,
                 6.75m/100.0m, 7.00m/100.0m, 7.25m/100.0m, 7.50m/100.0m};
            SetList(ref _strikes, ref strikes3Y7Y);
            decimal[] volatilities3Y7Y =
                {10.74m/100.0m, 10.48m/100.0m, 10.28m/100.0m, 10.10m/100.0m, 9.99m/100.0m,
                 9.90m/100.0m, 9.77m/100.0m, 9.85m/100.0m, 9.68m/100.0m};
            SetList(ref _volatilities, ref volatilities3Y7Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.099252040016432m),//9.9090m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.227342589183341m),//23.8677m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.284833735753549m),//-26.0412m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "3Y";
            _tenor = "7Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 3Y7Y Calibration Engine

            #region 3Y10Y Calibration Engine

            // ***3Y10Y SABR calibration engine object***.
            _assetPrice = 6.5000m/100.0m;
            _engineHandle = "SABR Full Calibration 3Y10Y Expiry";
            _exerciseTime = 3.0m;
            decimal[] strikes3Y10Y =
                {5.50m/100.0m, 5.75m/100.0m, 6.00m/100.0m, 6.25m/100.0m, 6.50m/100.0m,
                 6.75m/100.0m, 7.00m/100.0m, 7.25m/100.0m, 7.50m/100.0m};
            SetList(ref _strikes, ref strikes3Y10Y);
            decimal[] volatilities3Y10Y =
                {10.63m/100.0m, 10.35m/100.0m, 10.13m/100.0m, 9.95m/100.0m, 9.83m/100.0m,
                 9.75m/100.0m, 9.61m/100.0m, 9.72m/100.0m, 9.52m/100.0m};
            SetList(ref _volatilities, ref volatilities3Y10Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.7338m/100.0m),//9.7338m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(25.3610m/100.0m),//25.3610m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-25.2023m/100.0m),//-25.2023m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "3Y";
            _tenor = "10Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 3Y10Y Calibration Engine

            #region 5Y1Y Calibration Engine

            _assetPrice = 6.4000m/100.0m;
            _engineHandle = "SABR Full Calibration 5Y1Y Expiry";
            _exerciseTime = 5.0m;
            decimal[] strikes5Y1Y =
                {5.40m/100.0m, 5.65m/100.0m, 5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m,
                 6.65m/100.0m, 6.90m/100.0m, 7.15m/100.0m, 7.40m/100.0m};
            SetList(ref _strikes, ref strikes5Y1Y);
            decimal[] volatilities5Y1Y =
                {10.95m/100.0m, 10.67m/100.0m, 10.48m/100.0m, 10.29m/100.0m, 
                 10.15m/100.0m, 10.05m/100.0m, 10.01m/100.0m, 10.07m/100.0m,
                 10.19m/100.0m};
            SetList(ref _volatilities, ref volatilities5Y1Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.8262m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));

            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.31053036233426m),//31.0546m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.15275758438855m),//-15.2746m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "5Y";
            _tenor = "1Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 5Y1Y Calibration Engine

            #region 5Y2Y Calibration Engine

            _assetPrice = 6.4000m/100.0m;
            _engineHandle = "SABR Full Calibration 5Y2Y Expiry";
            _exerciseTime = 5.0m;
            decimal[] strikes5Y2Y =
                {5.40m/100.0m, 5.65m/100.0m, 5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m,
                 6.65m/100.0m, 6.90m/100.0m, 7.15m/100.0m, 7.40m/100.0m};
            SetList(ref _strikes, ref strikes5Y2Y);
            decimal[] volatilities5Y2Y =
                {10.82m/100.0m, 10.56m/100.0m, 10.34m/100.0m, 10.17m/100.0m,
                 10.04m/100.0m, 9.95m/100.0m, 9.92m/100.0m, 9.95m/100.0m,
                 10.10m/100.0m};
            SetList(ref _volatilities, ref volatilities5Y2Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.7220m/100.0m),//9.7220m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.307879838376356m),//30.7869m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.147070509969349m),//-14.7089m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "5Y";
            _tenor = "2Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 5Y2Y Calibration Engine

            #region 5Y3Y Calibration Engine

            _assetPrice = 6.4000m/100.0m;
            _engineHandle = "SABR Full Calibration 5Y3Y Expiry";
            _exerciseTime = 5.0m;
            decimal[] strikes5Y3Y =
                {5.40m/100.0m, 5.65m/100.0m, 5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m,
                 6.65m/100.0m, 6.90m/100.0m, 7.15m/100.0m, 7.40m/100.0m};
            SetList(ref _strikes, ref strikes5Y3Y);
            decimal[] volatilities5Y3Y =
                {10.92m/100.0m, 10.61m/100.0m, 10.37m/100.0m, 10.20m/100.0m,
                 10.10m/100.0m, 9.99m/100.0m, 9.95m/100.0m, 9.96m/100.0m, 10.10m/100.0m};
            SetList(ref _volatilities, ref volatilities5Y3Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.8109m/100.0m),//9.8109m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(29.8614m/100.0m),//29.8614m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.16742089268148m),//-16.7398m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "5Y";
            _tenor = "3Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 5Y3Y Calibration Engine

            #region 5Y4Y Calibration Engine

            _assetPrice = 6.4000m/100.0m;
            _engineHandle = "SABR Full Calibration 5Y4Y Expiry";
            _exerciseTime = 5.0m;
            decimal[] strikes5Y4Y =
                {5.40m/100.0m, 5.65m/100.0m, 5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m,
                 6.65m/100.0m, 6.90m/100.0m, 7.15m/100.0m, 7.40m/100.0m};
            SetList(ref _strikes, ref strikes5Y4Y);
            decimal[] volatilities5Y4Y =
                {10.78m/100.0m, 10.53m/100.0m, 10.29m/100.0m, 10.12m/100.0m,
                 10.00m/100.0m, 9.89m/100.0m, 9.84m/100.0m, 9.87m/100.0m, 9.99m/100.0m};
            SetList(ref _volatilities, ref volatilities5Y4Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated); 
            Assert.AreEqual(decimal.ToDouble(0.0974042329222542m),//9.7263m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.288920722073854m),//29.3142m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.18202631827791m),//-17.0922m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "5Y";
            _tenor = "4Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 5Y4Y Calibration Engine

            #region 5Y5Y Calibration Engine

            _assetPrice = 6.4000m/100.0m;
            _engineHandle = "SABR Full Calibration 5Y5Y Expiry";
            _exerciseTime = 5.0m;
            decimal[] strikes5Y5Y =
                {5.40m/100.0m, 5.65m/100.0m, 5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m,
                 6.65m/100.0m, 6.90m/100.0m, 7.15m/100.0m, 7.40m/100.0m};
            SetList(ref _strikes, ref strikes5Y5Y);
            decimal[] volatilities5Y5Y =
                {10.78m/100.0m, 10.51m/100.0m, 10.30m/100.0m, 10.12m/100.0m, 9.99m/100.0m,
                 9.89m/100.0m, 9.84m/100.0m, 9.86m/100.0m, 9.96m/100.0m};
            SetList(ref _volatilities, ref volatilities5Y5Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(0.097384126722906m),//9.7204m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.286565609967895m),//29.2209m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.18818293680353m),//-17.4833m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "5Y";
            _tenor = "5Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 5Y5Y Calibration Engine

            #region 5Y7Y Calibration Engine

            _assetPrice = 6.4000m/100.0m;
            _engineHandle = "SABR Full Calibration 5Y7Y Expiry";
            _exerciseTime = 5.0m;
            decimal[] strikes5Y7Y =
                {5.40m/100.0m, 5.65m/100.0m, 5.90m/100.0m, 6.15m/100.0m, 6.40m/100.0m,
                 6.65m/100.0m, 6.90m/100.0m, 7.15m/100.0m, 7.40m/100.0m};
            SetList(ref _strikes, ref strikes5Y7Y);
            decimal[] volatilities5Y7Y =
                {10.72m/100.0m, 10.46m/100.0m, 10.26m/100.0m, 10.08m/100.0m, 9.97m/100.0m,
                 9.86m/100.0m, 9.80m/100.0m, 9.81m/100.0m, 9.93m/100.0m};
            SetList(ref _volatilities, ref volatilities5Y7Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.7331m/100.0m),//9.7331m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(27.7317m/100.0m),//27.7317m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.181423083333099m),//-18.1434m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "5Y";
            _tenor = "7Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 5Y7Y Calibration Engine

            #region 5Y10Y Calibration Engine

            _assetPrice = 6.3000m/100.0m;
            _engineHandle = "SABR Full Calibration 5Y10Y Expiry";
            _exerciseTime = 5.0m;
            decimal[] strikes5Y10Y =
                {5.30m/100.0m, 5.55m/100.0m, 5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m,
                 6.55m/100.0m, 6.80m/100.0m, 7.05m/100.0m, 7.30m/100.0m};
            SetList(ref _strikes, ref strikes5Y10Y);
            decimal[] volatilities5Y10Y =
                {10.76m/100.0m, 10.52m/100.0m, 10.25m/100.0m, 10.07m/100.0m, 9.94m/100.0m,
                 9.82m/100.0m, 9.76m/100.0m, 9.76m/100.0m, 9.86m/100.0m};
            SetList(ref _volatilities, ref volatilities5Y10Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.7001m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta); 
            Assert.AreEqual(decimal.ToDouble(0.284116821500179m),//28.4095m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-19.9749m/100.0m),//-19.9749m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "5Y";
            _tenor = "10Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 5Y10Y Calibration Engine

            #region 7Y3Y Calibration Engine

            _assetPrice = 6.3000m/100.0m;
            _engineHandle = "SABR Full Calibration 7Y3Y Expiry";
            _exerciseTime = 7.0m;
            decimal[] strikes7Y3Y =
                {5.30m/100.0m, 5.55m/100.0m, 5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m,
                 6.55m/100.0m, 6.80m/100.0m, 7.05m/100.0m, 7.30m/100.0m};
            SetList(ref _strikes, ref strikes7Y3Y);
            decimal[] volatilities7Y3Y =
                {10.80m/100.0m, 10.64m/100.0m, 10.35m/100.0m, 10.20m/100.0m,
                 10.06m/100.0m, 9.94m/100.0m, 9.90m/100.0m, 9.93m/100.0m,
                 10.06m/100.0m};
            SetList(ref _volatilities, ref volatilities7Y3Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.6967m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(28.6243m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance)); 
            Assert.AreEqual(decimal.ToDouble(-0.170021247690905m),//-17.0033m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "7Y";
            _tenor = "3Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 7Y3Y Calibration Engine

            #region 10Y5Y Calibration Engine

            _assetPrice = 6.3000m/100.0m;
            _engineHandle = "SABR Full Calibration 10Y5Y Expiry";
            _exerciseTime = 10.0m;
            decimal[] strikes10Y5Y =
                {5.30m/100.0m, 5.55m/100.0m, 5.80m/100.0m, 6.05m/100.0m, 6.30m/100.0m,
                 6.55m/100.0m, 6.80m/100.0m, 7.05m/100.0m, 7.30m/100.0m};
            SetList(ref _strikes, ref strikes10Y5Y);
            decimal[] volatilities10Y5Y =
                {10.42m/100.0m, 10.21m/100.0m, 9.96m/100.0m, 9.77m/100.0m, 9.63m/100.0m,
                 9.55m/100.0m, 9.44m/100.0m, 9.58m/100.0m, 9.36m/100.0m};
            SetList(ref _volatilities, ref volatilities10Y5Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated); 
            Assert.AreEqual(decimal.ToDouble(0.093585750516126m),//9.2891m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(0.238650507767063m),//25.5990m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-0.250551527673566m),//-22.9049m/100.0m
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "10Y";
            _tenor = "5Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 10Y5Y Calibration Engine

            #region 10Y10Y Calibration Engine

            _assetPrice = 6.1000m/100.0m;
            _engineHandle = "SABR Full Calibration 10Y10Y Expiry";
            _exerciseTime = 10.0m;
            decimal[] strikes10Y10Y =
                {5.10m/100.0m, 5.35m/100.0m, 5.60m/100.0m, 5.85m/100.0m, 6.10m/100.0m,
                 6.35m/100.0m, 6.60m/100.0m, 6.85m/100.0m, 7.10m/100.0m};
            SetList(ref _strikes, ref strikes10Y10Y);
            decimal[] volatilities10Y10Y =
                {10.58m/100.0m, 10.43m/100.0m, 10.13m/100.0m, 9.97m/100.0m, 9.83m/100.0m,
                 9.72m/100.0m, 9.70m/100.0m, 9.74m/100.0m, 9.88m/100.0m};
            SetList(ref _volatilities, ref volatilities10Y10Y);

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
            Assert.AreEqual(decimal.ToDouble(9.3206m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Alpha),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(1.0m, _calibrationEngine.GetSABRParameters.Beta);
            Assert.AreEqual(decimal.ToDouble(28.4132m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Nu),
                            decimal.ToDouble(Tolerance));
            Assert.AreEqual(decimal.ToDouble(-15.4313m/100.0m),
                            decimal.ToDouble(_calibrationEngine.GetSABRParameters.Rho),
                            decimal.ToDouble(Tolerance));

            _expiry = "10Y";
            _tenor = "10Y";
            _engineHandles.Add(new SABRKey(_expiry, _tenor), _calibrationEngine);

            #endregion 10Y10Y Calibration Engine

            #region Interpolated Volatility Surafce

            // Instantiate the calibration engine object for the calculation
            // of the interpolated volatility surface.
            _engineHandle = "SABR Interpolated Calibration 2Y5Y";
            _atmVolatility = 10.01m/100.0m;
            _assetPrice = 6.70m;
            _exerciseTime = 2.0m;
            decimal tenor = 5.0m;

            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _engineHandles,
                                          _atmVolatility,
                                          _assetPrice,
                                          _exerciseTime,
                                          tenor);

            Assert.IsNotNull(_calibrationEngine);

            // Check the SABR Nu surface.
            //decimal[,] sabrNuSurface =
            //    {{29.2376m/100.0m, 28.1943m/100.0m, 29.2816m/100.0m, 28.2686m/100.0m, 26.9834m/100.0m, 26.1584m/100.0m, 22.9787m/100.0m},
            //     {30.5073m/100.0m, 28.7692m/100.0m, 29.5793m/100.0m, 28.2686m/100.0m, 28.6334m/100.0m, 26.3490m/100.0m, 26.9108m/100.0m},
            //     {28.3216m/100.0m, 30.4103m/100.0m, 25.6768m/100.0m, 28.5300m/100.0m, 26.8795m/100.0m, 25.1083m/100.0m, 26.1359m/100.0m},
            //     {26.1358m/100.0m, 32.0513m/100.0m, 25.6385m/100.0m, 28.7914m/100.0m, 25.1257m/100.0m, 23.8677m/100.0m, 25.3610m/100.0m},
            //     {31.0546m/100.0m, 30.7869m/100.0m, 29.8614m/100.0m, 29.3142m/100.0m, 29.2209m/100.0m, 27.7317m/100.0m, 28.4095m/100.0m},
            //     {31.0546m/100.0m, 30.7869m/100.0m, 28.6243m/100.0m, 29.3142m/100.0m, 27.7721m/100.0m, 27.7317m/100.0m, 28.4110m/100.0m},
            //     {31.0546m/100.0m, 30.7869m/100.0m, 28.6243m/100.0m, 29.3142m/100.0m, 25.5990m/100.0m, 27.7317m/100.0m, 28.4132m/100.0m}};

            decimal[,] sabrNuSurface =
                {{0.292347482m, 0.28194462229m, 0.29280053471342m, 0.276861494324337m, 0.2539129661135m, 0.24756203682674m, 0.206314165296761m},//done
                {0.30507367894739m, 0.28769596771m, 0.303922160289m, 0.2782014085187m, 0.2708815367822m, 0.247027423988m, 0.26464043052m},//done
                {0.27611727m, 0.300231741m, 0.237487249127m, 0.280881236907509m, 0.253721613153268m, 0.237185006585685m, 0.259127014640826m},//done
                {0.247160865171m, 0.31276751438457m, 0.269146459470764m, 0.283561065m, 0.23656168952m, 0.227342589183m, 0.253613598762m},//done
                {0.31053036233426m, 0.307879838376356m, 0.29862200864064m, 0.288920722073854m, 0.286565609967895m, 0.2773109031146m, 0.2841168215002m},//done
                {0.3738998595m, 0.30299216m, 0.28624270573m, 0.29428037885m, 0.2673995691m, 0.3272792171m, 0.2841236861m},//done
                {0.46895411m, 0.29566065m, 0.267673751m, 0.30231986402m, 0.2386505077671m, 0.4022316879m, 0.28413398306m}};//done

            var constructedSABRNuSurface = _calibrationEngine.SABRNuSurface;
            int numRows = sabrNuSurface.GetLength(0);
            int numColumns = sabrNuSurface.GetLength(1);

            Assert.AreEqual(numRows, constructedSABRNuSurface.GetLength(0));
            Assert.AreEqual(numColumns, constructedSABRNuSurface.GetLength(1));
            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                {
                    //Debug.Print(constructedSABRNuSurface[i, j].ToString(CultureInfo.InvariantCulture));
                    Assert.AreEqual
                        (decimal.ToDouble(sabrNuSurface[i, j]),
                         decimal.ToDouble(constructedSABRNuSurface[i, j]),
                         decimal.ToDouble(Tolerance));
                }
            }

            //// Check the SABR Rho Surface.
            //decimal[,] sabrRhoSurface =
            //    {{-19.2777m/100.0m, -19.3478m/100.0m, -17.7787m/100.0m, -17.2644m/100.0m, -19.0245m/100.0m, -20.4091m/100.0m, -23.2923m/100.0m},
            //     {-20.4544m/100.0m, -18.3291m/100.0m, -14.2396m/100.0m, -17.2644m/100.0m, -17.3229m/100.0m, -18.4560m/100.0m, -21.5256m/100.0m},
            //     {-20.2959m/100.0m, -15.8591m/100.0m, -16.1639m/100.0m, -17.2214m/100.0m, -20.1974m/100.0m, -22.2486m/100.0m, -23.3639m/100.0m},
            //     {-20.1373m/100.0m, -13.3892m/100.0m, -21.8147m/100.0m, -17.1783m/100.0m, -23.0718m/100.0m, -26.0412m/100.0m, -25.2023m/100.0m},
            //     {-15.2746m/100.0m, -14.7089m/100.0m, -16.7398m/100.0m, -17.0922m/100.0m, -17.4833m/100.0m, -18.1434m/100.0m, -19.9749m/100.0m},
            //     {-15.2746m/100.0m, -14.7089m/100.0m, -17.0033m/100.0m, -17.0922m/100.0m, -19.6519m/100.0m, -18.1434m/100.0m, -18.1574m/100.0m},
            //     {-15.2746m/100.0m, -14.7089m/100.0m, -17.0033m/100.0m, -17.0922m/100.0m, -22.9049m/100.0m, -18.1434m/100.0m, -15.4313m/100.0m}
            //    };

            decimal[,] sabrRhoSurface =
            {{-0.192814603335497m, -0.1934892520133m, -0.177800192408585m, -0.185443411148339m, -0.202748825819209m, -0.216446106499739m, -0.263277475328497m},
                {-0.204556453614928m, -0.183296641405637m, -0.143308991384418m, -0.185063734162736m, -0.193417522642517m, -0.19687581564957m, -0.225579868477239m},
                {-0.208944122775048m, -0.165466899320488m, -0.182950177473184m, -0.18430438019153m, -0.219560842186847m, -0.24085477570156m, -0.238799565329645m},
                {-0.213331791935168m, -0.14763715723534m, -0.2084686487965m, -0.183545026220323m, -0.245704161731177m, -0.284833735753549m, -0.252019262182051m},
                {-0.15275758438855m, -0.147070509969349m, -0.16742089268148m, -0.18202631827791m, -0.18818293680353m, -0.181423083333099m, -0.199756813306951m},
                {-0.092183376841932m, -0.146503862703358m, -0.170021247690905m, -0.180507610335497m, -0.213130373151544m, -0.078012430912649m, -0.181575530410115m},
                {-0.00132206552200498m, -0.145653891804372m, -0.173921780205042m, -0.178229548421878m, -0.250551527673566m, 0.077103547718026m, -0.15430360606486m}
            };

            decimal[,] constructedSABRRhoSurface = _calibrationEngine.SABRRhoSurface;
            Assert.AreEqual(numRows, constructedSABRRhoSurface.GetLength(0));
            Assert.AreEqual(numColumns, constructedSABRRhoSurface.GetLength(0));

            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                {
                    if (decimal.Compare(sabrRhoSurface[i, j], decimal.MinValue) != 0)
                    {
                        //Debug.Print(constructedSABRRhoSurface[i, j].ToString(CultureInfo.InvariantCulture));
                        Assert.AreEqual(decimal.ToDouble(sabrRhoSurface[i, j]),
                                        decimal.ToDouble(constructedSABRRhoSurface[i, j]),
                                        decimal.ToDouble(Tolerance));
                    }
                }
            }

            // Calibrate the engine and test SABR parameters.
            _calibrationEngine.CalibrateInterpSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Test the SABR parameters.
            _actual = _calibrationEngine.GetSABRParameters.Alpha; 
            _expected = 0.0993859101570217m;//9.9245m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            _actual = _calibrationEngine.GetSABRParameters.Beta;
            _expected = _beta;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            _actual = _calibrationEngine.GetSABRParameters.Nu; 
            _expected = 0.253721613153268m;//26.8795m/100.0m;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            _actual = _calibrationEngine.GetSABRParameters.Rho; 
            _expected = -0.219560842186847m;//-20.1974m/100.0m
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));


            #endregion Interpolated Volatility Surafce
        }

        #endregion Test: Expiry Interpolation

        #region Test: Enhanced Full Calibration of the SABR Model

        /// <summary>
        /// Tests the enhanced full calibration of the SABR model.
        /// </summary>
        [TestMethod]
        public void TestEnhancedFullCalibration()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Interpolated 6M7Y Settings";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings =
                new SABRCalibrationSettings(_settingsHandle,
                                            _instrument,
                                            _currency,
                                            _beta);

            Assert.IsNotNull(_calibrationSettings);

            // Setup a calibration, which is known to have failed.
            _assetPrice = 6.7000m/100.0m;
            _engineHandle = "SABR Full Calibration 6M7Y Expiry";
            _exerciseTime = 0.5m;
            decimal[] strikes6M7Y =
                {5.70m/100.0m, 5.95m/100.0m, 6.20m/100.0m, 6.45m/100.0m, 6.70m/100.0m,
                 6.95m/100.0m, 7.20m/100.0m, 7.45m/100.0m, 7.70m/100.0m};
            SetList(ref _strikes, ref strikes6M7Y);
            decimal[] volatilities6M7Y =
                {10.94m/100.0m, 10.81m/100.0m, 10.56m/100.0m, 10.41m/100.0m,
                 10.31m/100.0m, 10.15m/100.0m, 10.12m/100.0m, 10.13m/100.0m,
                 10.20m/100.0m};
            SetList(ref _volatilities, ref volatilities6M7Y);
            _calibrationEngine =
                new SABRCalibrationEngine(_engineHandle,
                                          _calibrationSettings,
                                          _strikes,
                                          _volatilities,
                                          _assetPrice,
                                          _exerciseTime);

            Assert.IsNotNull(_calibrationEngine);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);
        }

        #endregion Test: Enhanced Full Calibration of the SABR Model        

        #region Test: Cap/Floor Calibration Problem

        /// <summary>
        /// Test case for the SABR Cap/Floor calibration problem.
        /// </summary>
        [TestMethod]
        public void TestSABRCapFloorCalibrationProblem()
        {
            // Instantiate the settings object.
            _settingsHandle = "SABR CapFloor Settings";
            _instrument = InstrumentType.Instrument.CapFloor;
            _currency = "AUD";
            _beta = 0.50m;

            _calibrationSettings = new SABRCalibrationSettings
                (_settingsHandle, _instrument, _currency, _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 5.3869m/100.0m;
            _engineHandle = "SABR CapFloor Calibration";
            _exerciseTime = 17.2740m;

            decimal[] tempStrikes =
                {1.00m/100.0m, 1.50m/100.0m, 2.00m/100.0m, 2.50m/100.0m,
                 3.00m/100.0m, 3.50m/100.0m, 4.00m/100.0m, 5.00m/100.0m,
                 5.3869m/100.0m, 6.00m/100.0m, 7.00m/100.0m, 8.00m/100.0m,
                 9.00m/100.0m};
            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {35.51m/100.0m, 27.88m/100.0m, 24.22m/100.0m, 21.94m/100.0m,
                 19.83m/100.0m,	18.11m/100.0m, 16.58m/100.0m, 14.25m/100.0m,
                 13.87m/100.0m, 12.63m/100.0m, 12.13m/100.0m, 12.10m/100.0m,
                 11.79m/100.0m};
            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            Assert.AreEqual(_strikes.Count, _volatilities.Count);

            _calibrationEngine = new SABRCalibrationEngine
                (_engineHandle,
                 _calibrationSettings,
                 _strikes,
                 _volatilities,
                 _assetPrice,
                 _exerciseTime);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Check SABR parameters.
            SABRParameters sabrParameters =
                _calibrationEngine.GetSABRParameters;

            decimal alpha = sabrParameters.Alpha;
            decimal beta = sabrParameters.Beta;
            decimal nu = sabrParameters.Nu;
            decimal rho = sabrParameters.Rho;

            Debug.Print(alpha.ToString(CultureInfo.InvariantCulture));
            Debug.Print(beta.ToString(CultureInfo.InvariantCulture));
            Debug.Print(nu.ToString(CultureInfo.InvariantCulture));
            Debug.Print(rho.ToString(CultureInfo.InvariantCulture));
            // Compute implied volatility.
            SABRImpliedVolatility impliedVolObj =
                new SABRImpliedVolatility(sabrParameters, false);

            Assert.IsNotNull(impliedVolObj);

            string errorMessage ="";
            decimal result = -1.0m;
            foreach (decimal strike in _strikes)
            {
                bool success = impliedVolObj.SABRInterpolatedVolatility
                    (_assetPrice,
                     _exerciseTime,
                     strike,
                     ref errorMessage,
                     ref result, false);
                Assert.AreEqual(success, true);
            }
        }

        #endregion

        #region Test: USD SuperNormal Data

        /// <summary>
        /// Test case for the USD Super Normal data calibration problem.
        /// </summary>
        [TestMethod]
        public void TestUSDSuperNormalData()
        {
            // Instantiate the settings object.
            _settingsHandle = "SABR Super Normal Data Settings";
            _instrument = InstrumentType.Instrument.Swaption;
            _currency = "AUD";
            _beta = 1.0m;

            _calibrationSettings = new SABRCalibrationSettings
                (_settingsHandle, _instrument, _currency, _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 4.8337m/100.0m;
            _engineHandle = "SABR Super Normal Data Calibration";
            _exerciseTime = 2.0m;

            decimal[] tempStrikes =
                {2.3337m/100.0m, 2.5837m/100.0m, 2.8337m/100.0m, 3.0837m/100.0m,
                 3.3337m/100.0m, 3.5837m/100.0m, 3.8337m/100.0m, 4.0837m/100.0m,
                 4.3337m/100.0m, 4.5837m/100.0m, 4.8337m/100.0m, 5.0837m/100.0m,
                 5.3337m/100.0m, 5.5837m/100.0m, 5.8337m/100.0m, 6.0837m/100.0m,
                 6.3337m/100.0m, 6.5837m/100.0m, 6.8337m/100.0m, 7.0837m/100.0m,
                 7.3337m/100.0m};
            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {123.58m/100.0m, 121.89m/100.0m, 119.50m/100.0m, 118.21m/100.0m,
                 116.29m/100.0m, 114.36m/100.0m, 112.00m/100.0m, 110.00m/100.0m,
                 107.89m/100.0m, 106.83m/100.0m, 105.94m/100.0m, 105.33m/100.0m,
                 104.50m/100.0m, 103.60m/100.0m, 103.30m/100.0m, 103.02m/100.0m,
                 103.33m/100.0m, 103.92m/100.0m, 105.00m/100.0m, 106.00m/100.0m,
                 107.12m/100.0m};
            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            Assert.AreEqual(_strikes.Count, _volatilities.Count);

            _calibrationEngine = new SABRCalibrationEngine
                (_engineHandle,
                 _calibrationSettings,
                 _strikes,
                 _volatilities,
                 _assetPrice,
                 _exerciseTime);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Check SABR parameters.
            SABRParameters sabrParameters =
                _calibrationEngine.GetSABRParameters;

            decimal alpha = sabrParameters.Alpha;
            decimal beta = sabrParameters.Beta;
            decimal nu = sabrParameters.Nu;
            decimal rho = sabrParameters.Rho;
            Debug.Print(alpha.ToString(CultureInfo.InvariantCulture));
            Debug.Print(beta.ToString(CultureInfo.InvariantCulture));
            Debug.Print(nu.ToString(CultureInfo.InvariantCulture));
            Debug.Print(rho.ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        #region Test: ATM Calibration Error

        /// <summary>
        /// Tests for an erroneous ATM calibration.
        /// </summary>
        [TestMethod]
        public void TestATMCalibrationError()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR ATM 6M7Y Settings.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.Swaption;
            _beta = 1.0m;
            _calibrationSettings = new SABRCalibrationSettings
                (_settingsHandle, _instrument, _currency, _beta);

            // Override some variable set in the SetUp method.
            _assetPrice = 6.7000m/100.0m;
            _exerciseTime = 0.5m;

            // Initialise information required specifically for an ATM
            // calibration..
            decimal atmVolatility = 10.31m/100.0m;
            decimal nu = 25.00m/100.0m;
            decimal rho = -20.0m/100.0m;

            _calibrationEngine = new SABRCalibrationEngine(_engineHandle,
                                                           _calibrationSettings,
                                                           nu,
                                                           rho,
                                                           atmVolatility,
                                                           _assetPrice,
                                                           _exerciseTime);
            _calibrationEngine.CalibrateATMSABRModel();

            // Test: calibration status.
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Test: SABR parameter alpha.
            _expected = 10.2914m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Alpha;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(Tolerance));

            // Test: SABR parameter beta.
            _expected = 1.00m;
            _actual = _calibrationEngine.GetSABRParameters.Beta;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter nu.
            _expected = 25.00m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Nu;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter rho.
            _expected = -20.00m/100.0m;
            _actual = _calibrationEngine.GetSABRParameters.Rho;
            Assert.AreEqual(_expected, _actual);

            // Calculate an implied volatility.
            SABRParameters sabrParameters =
                _calibrationEngine.GetSABRParameters;
            SABRImpliedVolatility impliedVolObj = 
                new SABRImpliedVolatility(sabrParameters, true);

            decimal strike = 5.700m/100.0m;
            string errorMessage = "";
            decimal result = -1.0m;
            impliedVolObj.SABRInterpolatedVolatility
                (_assetPrice,
                 _exerciseTime,
                 strike,
                 ref errorMessage,
                 ref result,
                 true);

        }


        #endregion

        #region Anomolous Calibration

        /// <summary>
        /// Test case for the USD Super Normal data calibration problem.
        /// </summary>
        [TestMethod]
        public void TestAnomolousData()
        {
            // Instantiate the settings object.
            _settingsHandle = "SABR Anomolous Data Settings";
            _instrument = InstrumentType.Instrument.Swaption;
            _currency = "AUD";
            _beta = 0.4m;

            _calibrationSettings = new SABRCalibrationSettings
                (_settingsHandle, _instrument, _currency, _beta);
            Assert.IsNotNull(_calibrationSettings);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 3.5255m / 100.0m;
            _engineHandle = "SABR Anomolous Data Calibration";
            _exerciseTime = 1.2603m;

            decimal[] tempStrikes =
                											

                {1.00m/100.0m, 1.50m/100.0m, 2.00m/100.0m, 2.50m/100.0m,
                 3.00m/100.0m, 3.50m/100.0m, 3.5255m/100.0m, 4.00m/100.0m,
                 5.0000m/100.0m, 6.00m/100.0m, 7.00m/100.0m, 8.00m/100.0m,
                 9.00m/100.0m};

            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {52.06m/100.0m, 48.35m/100.0m, 42.86m/100.0m, 38.86m/100.0m,
                 35.68m/100.0m, 33.34m/100.0m, 34.09m/100.0m, 31.62m/100.0m,
                 29.15m/100.0m, 26.49m/100.0m, 24.05m/100.0m, 22.17m/100.0m,
                 20.64m/100.0m};
            _volatilities = new List<decimal>();
            foreach (decimal volatility in tempVolatilities)
            {
                _volatilities.Add(volatility);
            }

            Assert.AreEqual(_strikes.Count, _volatilities.Count);

            _calibrationEngine = new SABRCalibrationEngine
                (_engineHandle,
                 _calibrationSettings,
                 _strikes,
                 _volatilities,
                 _assetPrice,
                 _exerciseTime);
            _calibrationEngine.CalibrateSABRModel();
            Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Check SABR parameters.
            SABRParameters sabrParameters =
                _calibrationEngine.GetSABRParameters;

            decimal alpha = sabrParameters.Alpha;
            decimal beta = sabrParameters.Beta;
            decimal nu = sabrParameters.Nu;
            decimal rho = sabrParameters.Rho;
            Debug.Print(alpha.ToString(CultureInfo.InvariantCulture));
            Debug.Print(beta.ToString(CultureInfo.InvariantCulture));
            Debug.Print(nu.ToString(CultureInfo.InvariantCulture));
            Debug.Print(rho.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}