using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.PricingEngines;
using Orion.Analytics.Stochastics.SABR;
using Orion.Analytics.Utilities;
using Orion.CurveEngine.Factory;
using National.QRSC.FpML.V47;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.PricingStructures.Helpers;
using Orion.CurveEngine.Helpers;
using Orion.Utility.NamedValues;
using Orion.Configuration;
using Orion.Utility.Logging;
using Orion.Constants;
//using Orion.CurveEngine.Tests.Helpers;
using System.Diagnostics;

namespace Orion.Equities.VolCalc.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SABREquity
    {

        #region Private Fields
   
        private decimal _assetPrice; // ATM level of the relevant asset
        private decimal _beta; // beta used in the calibration of the SABR model
        private SABRCalibrationEngine _calibrationEngine;     
        private SABRCalibrationSettings _calibrationSettings;
        private string _currency;
        private string _engineHandle; // handle for the engine object   
        private decimal _exerciseTime; // time to option exercise 
        private InstrumentType.Instrument _instrument;
        private decimal _nu; // storage for SABR parameter nu
        private decimal _rho; // storage for SABR parameter rho
        private string _settingsHandle; // handle for settings object
        private List<decimal> _strikes; // strikes arranged in ascending order   
        private List<decimal> _volatilities; // implied volatility at each strike

        private decimal _actual; // actual test result
        private decimal _expected; // expected test result
        private const decimal _tolerance = 1.0E-05m; // test accuracy

        #endregion Private Fields

        public SABREquity()
        {
            Initialisation();
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ServerStoreHelper.Initialize();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            ServerStoreHelper.TidyUp();
        }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Setup
   
        public void Initialisation()
        {
            // Initialise the SABR calibration settings object.
            _settingsHandle = "SABR Calibration Engine Unit Test.";
            _currency = "AUD";
            _instrument = InstrumentType.Instrument.CallPut;
            _beta = 0.85m;
            //_calibrationError = 0.0m;
            _calibrationSettings = new SABRCalibrationSettings(_settingsHandle,
                                                               _instrument,
                                                               _currency,
                                                               _beta);

            // Initialise  the SABR calibration engine object.
            _assetPrice = 1350.00m;
            _engineHandle = _settingsHandle;
            _exerciseTime = 5.0m;

            decimal[] tempStrikes ={1242.00m,
                                    1269.00m,
                                    1283.00m,
                                    1296.00m,
                                    1323.00m,
                                    1350.00m,
                                    1377.00m,
                                    1404.00m,
                                    1418.00m,
                                    1431.00m,
                                    1458.00m,
                                    1485.00m,
                                    1519.00m};

            _strikes = new List<decimal>();
            foreach (decimal strike in tempStrikes)
            {
                _strikes.Add(strike);
            }

            decimal[] tempVolatilities =
                {0.3108m,
                 0.3012m,
                 0.2966m,
                 0.2925m,
                 0.2847m,
                 0.2784m,
                 0.2738m,
                 0.2709m,
                 0.2700m,
                 0.2696m,
                 0.2697m,
                 0.2710m,
                 0.2744m
                };

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

            // Initialise the collection of SABR calibration engine objects.
            //_engineHandles = null;
        }
        #endregion


      

        /// <summary>
        /// Tests the class constructor.
        /// </summary>
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
            DateTime start = DateTime.Now;
            _calibrationEngine.CalibrateSABRModel();             
            DateTime end = DateTime.Now;
            double x = end.Subtract(start).Seconds;
            Debug.Print("%d seconds",x);

            // Test: calibration status.
              Assert.IsTrue(_calibrationEngine.IsSABRModelCalibrated);

            // Test: Calculation of the ATM slope.
            _expected = -0.2724637m;
            _actual = _calibrationEngine.ATMSlope;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(_tolerance));

            // Test: Initial guess for the transformed SABR parameter theta.  
            _expected = 1.85616576m;
            _actual = _calibrationEngine.ThetaGuess;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(_tolerance));

            // Test: Initial guess for the transformed SABR parameter mu.  
            _expected = 0.99883966m;
            _actual = _calibrationEngine.MuGuess;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(_tolerance));

            // Test: SABR parameter alpha.
            _expected = 0.5477212m;
            _actual = _calibrationEngine.GetSABRParameters.Alpha;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(_tolerance));

            // Test: SABR parameter beta.
            _expected = 0.85m;
            _actual = _calibrationEngine.GetSABRParameters.Beta;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual));

            // Test: SABR parameter nu.
            _expected = 1.23498996m;
            _actual = _calibrationEngine.GetSABRParameters.Nu;
            _nu = _actual;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(_tolerance));
          

            // Test: SABR parameter rho.
            _expected = -0.2724164m;
            _actual = _calibrationEngine.GetSABRParameters.Rho;
            _rho = _actual;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(_tolerance));          

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
            _assetPrice = 1352;
            _exerciseTime = 3.0m;

            // Initialise information required specifically for an ATM
            // calibration..
            decimal atmVolatility = 0.2884m;
            decimal nu = 1.23498996m;
            decimal rho = -0.2724164m;
            
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
            _expected = 0.65869806m;
            _actual = _calibrationEngine.GetSABRParameters.Alpha;
            Assert.AreEqual(decimal.ToDouble(_expected),
                            decimal.ToDouble(_actual),
                            decimal.ToDouble(_tolerance));

            // Test: SABR parameter beta.
            _expected = 0.85m;
            _actual = _calibrationEngine.GetSABRParameters.Beta;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter nu.
            _expected = 1.23498996m;
            _actual = _calibrationEngine.GetSABRParameters.Nu;
            Assert.AreEqual(_expected, _actual);

            // Test: SABR parameter rho.
            _expected = -0.2724164m;
            _actual = _calibrationEngine.GetSABRParameters.Rho;
            Assert.AreEqual(_expected, _actual);
        }

        #endregion Tests: ATM Calibration of the SABR Model

         [TestMethod]
         public void TestPricingStructure()
         {
            var equityProperties = new NamedValueSet();
            equityProperties.Set(CurveProp.PricingStructureType, "EquityWingVolatilityMatrix");
            equityProperties.Set("BuildDateTime", DateTime.Today);
            equityProperties.Set(CurveProp.BaseDate, DateTime.Today);
            equityProperties.Set(CurveProp.Market, "LIVE");
            equityProperties.Set("Identifier", "EquityWingVolatilityMatrix.AUD-BHP.07/01/2010");
            equityProperties.Set("Currency", "AUD");
            equityProperties.Set("CommodityAsset", "Stock");
            equityProperties.Set(CurveProp.CurveName, "AUD-BHP-EquityTradingDesk");
            equityProperties.Set("Algorithm", "SABR");

            string[] strikes = new string[10] { "0.5", "0.6", "0.75", "0.9", "1", "1.1", "1.25", "1.4", "1.5", "1.6" };

            string[] times = new string[2]{"3d", "5d" };
            //string[] times = new string[10] { "1d", "2d", "3d", "5d", "1m", "3m", "6m", "1y", "2y", "5y" };
            double[,] vols = new double[2,10]{{ 0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} ,  
                                                {0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} };
                                                /* {  0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} ,
                                                 { 0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} ,
                                                 {  0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} ,
                                                 {  0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} ,
                                                 {  0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} ,
                                                 {  0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} ,
                                                 {  0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} ,
                                                 {  0.5d, 0.49d, 0.49d, 0.48d, 0.47d, 0.455d, 0.45d, 0.45d, 0.45d, 0.46d} 
                                                    }; */
            double[] forwards = new double[3] {6.90d, 7d, 7.5d };

            string id = ObjectCacheHelper.CreateVolatilitySurfaceWithProperties(equityProperties, times, strikes, vols, forwards);   
            
            //Get the curvbe.
            var pricingStructure = (IStrikeVolatilitySurface)ObjectCacheHelper.GetPricingStructureFromSerialisable(id);

            var rows = times.Length;
            var width = strikes.Length;
            //popultate the result matrix.
            var result = new double[rows, width];
            for (var i = 0; i < rows; i++)
            {
                for(var j = 0; j < width; j++)
                {                   
                    result[i, j] = pricingStructure.GetValueByExpiryTermAndStrike(times[i], forwards[i+1]*Convert.ToDouble(strikes[j]));                                  
                }                
            }
         
    }


    }


   


}
