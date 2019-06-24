#region Using

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.UnitTestEnv;
using Orion.CurveEngine.PricingStructures.SABR;
using Math = System.Math;
using Exception = System.Exception;

#endregion

namespace Orion.CurveEngine.Tests 
{
    [TestClass]
    public class SABRTests
    {
        #region Constants

        const string CalculationDateString = "2007-11-29";
        const string SettingsHandle = "CapFloorSettings1";
        const string EngineHandle = "CapFloorEngine1";
        const string ATMEngineHandle = "ATM CapFloor Engine";
        const string AtmEngineHandle = "ATM CapFloor Engine";
        const string SurfaceSmileSettings = "USD Caplet Smile Calibration Settings";
        const string SurfaceATMEngine = "USD ATM Bootstrap Engine";
        const string SurfaceFixedEngine = "USD Fixed Strike Bootstrap Engine";
        const string SurfaceSmileEngine = "USD Caplet Smile Calibration Engine";

        #endregion

        #region Additional test attributes

            private string settingsHandle = "SABR Settings 1";
            private string instrument = "Swaption";
            private string ccy = "AUD";
            private decimal beta = 0.95m;

            private string engineHandle = "SABR Full Calibration 1";
            private string optionExpiry = "3y";
            private readonly decimal[] _volStrikes = {
                                    -0.01m, -0.0075m, -0.0050m, -0.0025m, 0m, 0.0025m, 0.0050m, 0.0075m, 0.01m
                                };
            private readonly String[] _expiries = {"1y","2y", "3y", "4y", "5y", "7y", "10y"};

            private readonly Decimal[,] _volatilityData = {
                                          {10.64927747244750m / 100.0m, 10.39994584260630m / 100.0m, 10.20193111881570m / 100.0m, 10.06617143365560m / 100.0m, 9.97219920761609m / 100.0m, 9.91915214635118m / 100.0m, 9.79728404410501m / 100.0m, 9.95835207658104m / 100.0m, 9.72665759006778m / 100.0m },
                                          {10.71945067933860m / 100.0m, 10.47225313931150m / 100.0m, 10.26013281169220m / 100.0m, 10.09352837649130m / 100.0m, 9.99562871347315m / 100.0m, 9.93462583875790m / 100.0m, 9.83277616774097m / 100.0m, 9.96292868333144m / 100.0m, 10.07995797075100m / 100.0m },
                                          {10.73464076629010m / 100.0m, 10.47782408038430m / 100.0m, 10.27969109831760m / 100.0m, 10.13079982425510m / 100.0m, 10.01932817283930m / 100.0m, 9.95142328386115m / 100.0m, 9.84832497447229m / 100.0m, 9.92376362935676m / 100.0m, 9.79126953081919m / 100.0m },
                                          {0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m},
                                          {10.71284746646270m / 100.0m, 10.42348311645850m / 100.0m, 10.21182829617250m / 100.0m, 10.05813933315390m / 100.0m, 9.95608449408524m / 100.0m, 9.87432868808041m / 100.0m, 9.76560385799806m / 100.0m, 9.87023985544692m / 100.0m, 9.69248020548528m / 100.0m},
                                          {10.73741477761890m / 100.0m, 10.47825361823840m / 100.0m, 10.28001650952580m / 100.0m, 10.10364290373030m / 100.0m, 9.98568013928443m / 100.0m, 9.90003220294389m / 100.0m, 9.77431532150208m / 100.0m, 9.85123467628954m / 100.0m, 9.67881771828221m / 100.0m },
                                          {10.62690693405300m/ 100.0m, 10.35220865860990m / 100.0m, 10.12715394280070m / 100.0m, 9.94815984494852m / 100.0m, 9.82843523464171m / 100.0m, 9.74581252646963m / 100.0m, 9.61090280165564m / 100.0m, 9.71961846875728m / 100.0m, 9.51894989184063m / 100.0m }
                                      };

            private readonly String[] _assetExpiry = { "1m", "2m", "3m", "6m", "1y", "2y", "3y", "4y", "5y", "7y", "10y"};
            private readonly String[] _assetTenor = {"1y", "2y", "3y", "4y", "5y", "7y", "10y"};
            private readonly Decimal[,] _assetData = {
                                            {6.8256130m / 100.0m, 6.8257000m / 100.0m, 6.8242390m / 100.0m, 6.8589220m / 100.0m, 6.8315520m / 100.0m, 6.7394660m / 100.0m, 6.6439270m / 100.0m },
                                            {6.8199790m / 100.0m, 6.8226730m / 100.0m, 6.8201770m / 100.0m, 6.8543610m / 100.0m, 6.8243310m / 100.0m, 6.7334150m / 100.0m, 6.6393830m / 100.0m },
                                            {6.8158210m / 100.0m, 6.8206630m / 100.0m, 6.8165900m / 100.0m, 6.8504960m / 100.0m, 6.8181570m / 100.0m, 6.7280850m / 100.0m, 6.6355410m / 100.0m },
                                            {6.8244690m / 100.0m, 6.8255620m / 100.0m, 6.8126460m / 100.0m, 6.8440240m / 100.0m, 6.8027940m / 100.0m, 6.7148050m / 100.0m, 6.6258740m / 100.0m },
                                            {6.8247790m / 100.0m, 6.8263350m / 100.0m, 6.7950840m / 100.0m, 6.8227380m / 100.0m, 6.7580700m / 100.0m, 6.6822070m / 100.0m, 6.6026460m / 100.0m },
                                            {6.8280000m / 100.0m, 6.7787050m / 100.0m, 6.7430000m / 100.0m, 6.7183970m / 100.0m, 6.6737840m / 100.0m, 6.6039100m / 100.0m, 6.5483780m / 100.0m},
                                            {6.7262630m / 100.0m, 6.6961020m / 100.0m, 6.5945490m / 100.0m, 6.6113380m / 100.0m, 6.5806990m / 100.0m, 6.5071840m / 100.0m, 6.4826110m / 100.0m },
                                            {6.6636030m / 100.0m, 6.5141040m / 100.0m, 6.4932670m / 100.0m, 6.5210020m / 100.0m, 6.4874660m / 100.0m, 6.4551160m / 100.0m, 6.4188970m / 100.0m },
                                            {6.3394940m / 100.0m, 6.3997920m / 100.0m, 6.3953630m / 100.0m, 6.4201080m / 100.0m, 6.3850090m / 100.0m, 6.4008770m / 100.0m, 6.3521360m / 100.0m },
                                            {6.3856010m / 100.0m, 6.3350830m / 100.0m, 6.2844070m / 100.0m, 6.3705400m / 100.0m, 6.3760590m / 100.0m, 6.3450100m / 100.0m, 6.2871660m / 100.0m },
                                            {6.4446620m / 100.0m, 6.3995600m / 100.0m, 6.3530720m / 100.0m, 6.3555290m / 100.0m, 6.3071830m / 100.0m, 6.2514870m / 100.0m, 6.13093100m / 100.0m }
                                        };

            //private Decimal[,] volatilityData = volArray;
            //private Decimal[,] assetData = assetArray;

            private string exerciseTime = "3y";
            private string assetCode = "2y";
            private decimal strike = 6.594549m / 100.0m;

            private string atmEngineHandle = "SABR ATM Calibration 1";
            private decimal nu = 0.1045m;
            private decimal rho = -0.47m;
            private decimal atmVolatility = 0.1154m;
            private decimal assetPrice = 0.1098m;

            private string expiry = "3y";
            private string tenor = "2y";

        #endregion

        #region Properties

        //private static ILogger Logger_obs { get; set; }
        private static UnitTestEnvironment UTE { get; set; }
        //private static CurveEngine CurveEngine { get; set; }
        //private static CalendarEngine.CalendarEngine CalendarEngine { get; set; }
        //private static TimeSpan Retention { get; set; }
        //private static IBusinessCalendar FixingCalendar { get; set; }
        //private static IBusinessCalendar PaymentCalendar { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
        }

        [ClassCleanup]
        public static void Teardown()
        {
            UTE.Dispose();
        }

        #endregion

        #region Swaption SABR Tests

        /// <summary>
        ///A test for SABRInterpolateVolatility
        ///</summary>
        [TestMethod]
        public void SABRInterpolateVolatilityTest()
        {
            var target = SABRSwaptionInterface.Instance();
            target.SABRAddCalibrationSettings(settingsHandle, instrument, ccy, beta);
            target.SABRCalibrateModel(engineHandle, settingsHandle, _expiries, _volStrikes, _volatilityData,
                                      _assetData, _assetTenor, _assetExpiry, optionExpiry);
            double expected = 0.100330471262802;//0.1003227242714797           
            decimal actual = target.SABRInterpolateVolatility(engineHandle, exerciseTime, assetCode, strike);
            Assert.AreEqual(expected, (double)actual, 1e-10);
        }

        /// <summary>
        ///A test for SABRCalibrateModel
        ///</summary>
        [TestMethod]
        public void SABRCalibrateModelTest()
        {
            var target = SABRSwaptionInterface.Instance();
            target.SABRAddCalibrationSettings(settingsHandle, instrument, ccy, beta);
            string actual = target.SABRCalibrateModel(engineHandle, settingsHandle, _expiries, _volStrikes, _volatilityData,
                                                      _assetData, _assetTenor, _assetExpiry, optionExpiry);
            string expected = engineHandle;
            Assert.AreEqual(expected, actual);
            // Test the IsCalibrated status - true if the Function succeeded
            // Use default expiry/tenor pair
            //bool calibrationStatus = true;
            bool testStatus = SABRSwaptionInterface.IsModelCalibrated(engineHandle, optionExpiry, tenor);
            Assert.AreEqual(true, testStatus);
            // Test a non-existent expiry/tenor pair
            testStatus = SABRSwaptionInterface.IsModelCalibrated(engineHandle, "4yr", "3y");
            Assert.AreEqual(false, testStatus);
        }

        /// <summary>
        ///A test for SABRCalibrateATMModel
        ///</summary>
        [TestMethod]
        public void SABRCalibrateATMModelTest1()
        {
            var target = SABRSwaptionInterface.Instance();
            string expected = atmEngineHandle;
            target.SABRAddCalibrationSettings(settingsHandle, instrument, ccy, beta);
            string actual = target.SABRCalibrateATMModel(atmEngineHandle, settingsHandle, nu, rho, atmVolatility, assetPrice, exerciseTime);
            Assert.AreEqual(expected, actual);
            // Test the IsCalibrated status - true if the Function succeeded
            // Use default expiry/tenor pair
            bool testStatus = SABRSwaptionInterface.IsModelCalibrated(atmEngineHandle, optionExpiry, tenor);
            Assert.AreEqual(true, testStatus);
        }

        /// <summary>
        ///A test for SABRCalibrateATMModel
        ///</summary>
        [TestMethod]
        public void SABRCalibrateATMModelTest()
        {
            var target = SABRSwaptionInterface.Instance();
            string expected = atmEngineHandle;
            target.SABRAddCalibrationSettings(settingsHandle, instrument, ccy, beta);
            string actual = target.SABRCalibrateATMModel(atmEngineHandle, settingsHandle, nu, rho, atmVolatility, assetPrice, exerciseTime, assetCode);
            Assert.AreEqual(expected, actual);
            // Test the IsCalibrated status - true if the Function succeeded
            // Use default expiry/tenor pair
            bool testStatus = SABRSwaptionInterface.IsModelCalibrated(atmEngineHandle, optionExpiry, tenor);
            Assert.AreEqual(true, testStatus);
        }

        /// <summary>
        ///A test for SABRAddCalibrationSettings
        ///</summary>
        [TestMethod]
        public void SABRAddCalibrationSettingsTest()
        {
            var target = SABRSwaptionInterface.Instance();
            string expected = settingsHandle;
            string actual = target.SABRAddCalibrationSettings(settingsHandle, instrument, ccy, beta);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsModelCalibrated
        ///</summary>
        [TestMethod]
        public void IsModelCalibratedTest()
        {
            var target = SABRSwaptionInterface.Instance();
            target.SABRAddCalibrationSettings(settingsHandle, instrument, ccy, beta);
            target.SABRCalibrateModel(engineHandle, settingsHandle, _expiries, _volStrikes, _volatilityData,
                                      _assetData, _assetTenor, _assetExpiry, optionExpiry);
            bool actual = SABRSwaptionInterface.IsModelCalibrated(engineHandle, optionExpiry, tenor);
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///A test for GetSABRParameter
        ///</summary>
        [TestMethod]
        public void GetSABRParameterTest()
        {
            var target = SABRSwaptionInterface.Instance();
            target.SABRAddCalibrationSettings(settingsHandle, instrument, ccy, beta);
            target.SABRCalibrateModel(engineHandle, settingsHandle, _expiries, _volStrikes, _volatilityData,
                                      _assetData, _assetTenor, _assetExpiry, optionExpiry);
            // Test alpha
            var param = SABRSwaptionInterface.CalibrationParameter.Alpha;
            var expected = 0.0855935199424062m;         
            var actual = target.GetSABRParameter(param, engineHandle, expiry, tenor);
            Assert.AreEqual(Convert.ToDecimal(expected), Convert.ToDecimal(actual));
            // Test beta
            param = SABRSwaptionInterface.CalibrationParameter.Beta;
            expected = 0.95m;
            actual = target.GetSABRParameter(param, engineHandle, expiry, tenor);
            Assert.AreEqual(expected, actual);
            // Test nu
            param = SABRSwaptionInterface.CalibrationParameter.Nu;
            expected = 0.3057656451194682178593093540m;// 0.3174771160265069427371566533m
            actual = target.GetSABRParameter(param, engineHandle, expiry, tenor);
            Assert.AreEqual(Convert.ToDecimal(expected), Convert.ToDecimal(actual));
            // Test rho
            param = SABRSwaptionInterface.CalibrationParameter.Rho;
            expected = -0.125622311818193m;// -0.116352678779442m
            actual = target.GetSABRParameter(param, engineHandle, expiry, tenor);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CalibrationError
        ///</summary>
        [TestMethod]
        public void CalibrationErrorTest()
        {
            var target = SABRSwaptionInterface.Instance();
            target.SABRAddCalibrationSettings(settingsHandle, instrument, ccy, beta);
            target.SABRCalibrateModel(engineHandle, settingsHandle, _expiries, _volStrikes, _volatilityData,
                                      _assetData, _assetTenor, _assetExpiry, optionExpiry);
            double expected = 1E-05;//0.00000122592559259722;
            decimal actual = SABRSwaptionInterface.CalibrationError(engineHandle, expiry, tenor);
            Assert.AreEqual(expected, (double)actual, 1e-16);
        }

        /// <summary>
        ///A test for SABRInterface Constructor
        ///</summary>
        [TestMethod]
        public void SABRInterfaceConstructorTest()
        {
            var target = SABRSwaptionInterface.Instance();
            Assert.IsNotNull(target);
        }

        #endregion

        #region CapFloor SABR Tests

        #region Test Objects

        private object[,] _rawVolGrid;
        private object[,] _rawATMVolGrid;
        private object[,] _dfGrid;
        private object[,] _dfatmGrid;
        private DateTime _valDate;

        private object[,] _settings;
        private object[,] _atmSettings;

        private object[,] _surfaceATMSettings;
        private object[,] _surfaceFixSettings;
        private object[,] _surfaceATMVols;
        private object[,] _surfaceFixedVols;
        private object[,] _surfaceDF;
        private object[,] _surfaceDF2;

        private string _engineHandle;
        //private String[] _expiryTenors;
        private String[] _volTypes;
        private String[] _dfDateTimeGrid;
        private DateTime[] _dates;
        private DateTime[] _atmdates;
        private Decimal[] _atmdfGrid;

        private DateTime[] _surfaceatmdates;
        private Decimal[] _surfaceAtmdfGrid;
        private String[] _surfaceInstruments;
        private Decimal[] _surfaceStrikes;
        private Decimal[] _aTmVols;

        private Decimal[,] _rawVolGrid2;
        private Decimal[] _rawATMVolGrid2;
        private Decimal[] _dfGrid2;
        private Decimal[,] _surfaceFixedVols2;

        #endregion

        #region Test Data

        /// <summary>
        /// Create the test data
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            #region Matrix Setup
            _engineHandle = EngineHandle;

            _rawVolGrid = new object[,]
                              {
                                  { "Expiry", 7m, 7.125m, 7.25m, 7.375m, 7.5m, 7.625m, 7.75m, 7.875m, 8m, "Type" },
                                  { "0D", 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, "ETO" },
                                  { "8D", 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, "ETO" },
                                  { "99D", 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, "ETO" },
                                  { "190D", 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, "ETO" },
                                  { "281D", 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, "ETO" },
                                  { "2Y", 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, "Cap/Floor" },
                                  { "3Y", 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, "Cap/Floor" },
                                  { "4Y", 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, "Cap/Floor" },
                                  { "5Y", 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, "Cap/Floor" },
                                  { "7Y", 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, "Cap/Floor" },
                                  { "10Y", 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, "Cap/Floor" }
                              };

            _rawATMVolGrid = new object[,] {
                                               { 
                                                   "Expiry", "ATM", "Type" },
                                               { "36D", 13.54m / 100.0m, "ETO" },
                                               { "127D", 12.34m / 100.0m, "ETO" },
                                               { "218D", 13.81m / 100.0m, "ETO" },
                                               { "309D", 12.16m / 100.0m, "ETO" },
                                               { "2Y", 13.083m / 100.0m, "Cap/Floor" }, 
                                               { "3Y", 13.37m / 100.0m, "Cap/Floor" }, 
                                               { "4Y", 13.49m / 100.0m, "Cap/Floor" }, 
                                               { "5Y", 13.395m / 100.0m, "Cap/Floor" }, 
                                               { "7Y", 13.158m / 100.0m, "Cap/Floor" }, 
                                               { "10Y", 12.013m / 100.0m, "Cap/Floor" }, 
                                               { "15Y", 10.865m / 100.0m, "Cap/Floor" } 
                                           };

            _dfGrid = new object[,]
                          {
                              { "Date", "Discount Factor" },
                              { "30-Nov-07", 0.99982m },
                              { "29-Feb-08", 0.98209m },
                              { "30-May-08", 0.96448m },
                              { "29-Aug-08", 0.94694m },
                              { "28-Nov-08", 0.92971m },
                              { "27-Feb-09", 0.9128m },
                              { "29-May-09", 0.89617m },
                              { "31-Aug-09", 0.8793m },
                              { "30-Nov-09", 0.86323m },
                              { "26-Feb-10", 0.84812m },
                              { "31-May-10", 0.83231m },
                              { "30-Aug-10", 0.81729m },
                              { "30-Nov-10", 0.8024m },
                              { "28-Feb-11", 0.78839m },
                              { "30-May-11", 0.77454m },
                              { "30-Aug-11", 0.76083m },
                              { "30-Nov-11", 0.74742m },
                              { "29-Feb-12", 0.73481m },
                              { "30-May-12", 0.7225m },
                              { "30-Aug-12", 0.71036m },
                              { "30-Nov-12", 0.69852m },
                              { "28-Feb-13", 0.68736m },
                              { "30-May-13", 0.67636m },
                              { "30-Aug-13", 0.66553m },
                              { "29-Nov-13", 0.65508m },
                              { "28-Feb-14", 0.64489m },
                              { "30-May-14", 0.63495m },
                              { "29-Aug-14", 0.62526m },
                              { "28-Nov-14", 0.61581m },
                              { "27-Feb-15", 0.60617m },
                              { "29-May-15", 0.59673m },
                              { "31-Aug-15", 0.5872m },
                              { "30-Nov-15", 0.57818m },
                              { "29-Feb-16", 0.56936m },
                              { "30-May-16", 0.56073m },
                              { "30-Aug-16", 0.55219m },
                              { "30-Nov-16", 0.54384m },
                              { "28-Feb-17", 0.53584m },
                              { "30-May-17", 0.52793m },
                              { "30-Aug-17", 0.52011m },
                              { "30-Nov-17", 0.51245m }
                          };

            _dfatmGrid = new object[,]
                             {
                                 { "Date", "Discount Factor" }, 
                                 { "9-May-08", 0.99980m },
                                 { "11-Aug-08", 0.98013m },
                                 { "11-Nov-08", 0.96125m },
                                 { "9-Feb-09", 0.94325m },
                                 { "11-May-09", 0.92562m },
                                 { "10-Aug-09", 0.90854m },
                                 { "9-Nov-09", 0.89198m },
                                 { "9-Feb-10", 0.87574m },
                                 { "10-May-10", 0.86029m },
                                 { "9-Aug-10", 0.84500m },
                                 { "9-Nov-10", 0.82997m },
                                 { "9-Feb-11", 0.81536m },
                                 { "9-May-11", 0.80161m },
                                 { "9-Aug-11", 0.78707m },
                                 { "9-Nov-11", 0.77283m },
                                 { "9-Feb-12", 0.75889m },
                                 { "9-May-12", 0.74553m },
                                 { "9-Aug-12", 0.73247m },
                                 { "9-Nov-12", 0.71971m },
                                 { "11-Feb-13", 0.70699m },
                                 { "9-May-13", 0.69548m },
                                 { "9-Aug-13", 0.68325m },
                                 { "11-Nov-13", 0.67102m },
                                 { "10-Feb-14", 0.65944m },
                                 { "9-May-14", 0.64846m },
                                 { "11-Aug-14", 0.63697m },
                                 { "10-Nov-14", 0.62608m },
                                 { "9-Feb-15", 0.61542m },
                                 { "11-May-15", 0.60497m },
                                 { "10-Aug-15", 0.59456m },
                                 { "9-Nov-15", 0.58435m },
                                 { "9-Feb-16", 0.57423m },
                                 { "9-May-16", 0.56452m },
                                 { "9-Aug-16", 0.55479m },
                                 { "9-Nov-16", 0.54524m },
                                 { "9-Feb-17", 0.53590m },
                                 { "9-May-17", 0.52702m },
                                 { "9-Aug-17", 0.51803m },
                                 { "9-Nov-17", 0.50921m },
                                 { "9-Feb-18", 0.50057m },
                                 { "9-May-18", 0.49237m },
                                 { "9-Aug-18", 0.48411m },
                                 { "9-Nov-18", 0.47600m },
                                 { "11-Feb-19", 0.46789m },
                                 { "9-May-19", 0.46052m },
                                 { "9-Aug-19", 0.45288m },
                                 { "11-Nov-19", 0.44523m },
                                 { "10-Feb-20", 0.43797m },
                                 { "11-May-20", 0.43085m },
                                 { "10-Aug-20", 0.42388m },
                                 { "9-Nov-20", 0.41704m },
                                 { "9-Feb-21", 0.41026m },
                                 { "10-May-21", 0.40375m },
                                 { "9-Aug-21", 0.39730m },
                                 { "9-Nov-21", 0.39091m },
                                 { "9-Feb-22", 0.38464m },
                                 { "9-May-22", 0.37870m },
                                 { "9-Aug-22", 0.37267m },
                                 { "9-Nov-22", 0.36676m },
                                 { "9-Feb-23", 0.36097m },
                                 { "9-May-23", 0.35548m }
                             };

            _valDate = DateTime.Parse(CalculationDateString);

            _rawVolGrid2 = new[,]
                    {
                        { 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m},
                        { 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m, 8.82m / 100.0m},
                        { 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m, 9.12m / 100.0m},
                        { 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m, 9.43m / 100.0m},
                        { 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m, 9.71m / 100.0m},
                        { 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m, 10.34m / 100.0m},
                        { 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m, 10.75m / 100.0m},
                        { 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m, 10.84m / 100.0m},
                        {  10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m, 10.93m / 100.0m},
                        { 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m, 10.95m / 100.0m},
                        { 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m, 10.99m / 100.0m}
                    };

            _volTypes = new[] { "AUD-CAPLET-0D-90D", "AUD-CAPLET-8D-90D", "AUD-CAPLET-99D-90D", "AUD-CAPLET-190D-90D", "AUD-CAPLET-281D-90D", "AUD-IRCAP-2Y", "AUD-IRCAP-3Y"};
            _rawATMVolGrid2 = new[] {13.54m / 100.0m, 12.34m / 100.0m, 13.81m / 100.0m,12.16m / 100.0m, 13.083m / 100.0m, 

                13.37m / 100.0m, 13.49m / 100.0m};
            _dfDateTimeGrid = new[]
                          {"30-Nov-07",  "29-Feb-08", "30-May-08", "29-Aug-08", "28-Nov-08","27-Feb-09", "29-May-09", "31-Aug-09", 
                               "30-Nov-09",  "26-Feb-10", "31-May-10", "30-Aug-10","30-Nov-10", "28-Feb-11", "30-May-11", "30-Aug-11", 
                               "30-Nov-11",  "29-Feb-12", "30-May-12","30-Aug-12", "30-Nov-12","28-Feb-13", "30-May-13", "30-Aug-13", 
                                "29-Nov-13", "28-Feb-14", "30-May-14", "29-Aug-14", "28-Nov-14", "27-Feb-15", "29-May-15", "31-Aug-15", 
                                "30-Nov-15", "29-Feb-16", "30-May-16", "30-Aug-16", "30-Nov-16", "28-Feb-17", "30-May-17", "30-Aug-17", "30-Nov-17"
                          };
            var index = 0;
            _dates = new DateTime[_dfDateTimeGrid.Length];
            foreach (var dateString in _dfDateTimeGrid)
            {
                _dates[index] = Convert.ToDateTime(dateString);
                index++;
            }

            _dfGrid2 = new[]
                          {0.99982m, 0.98209m, 0.96448m, 0.94694m , 0.92971m , 0.9128m , 0.89617m, 0.8793m, 0.86323m, 0.84812m , 0.83231m ,
                              0.81729m, 0.8024m, 0.78839m, 0.77454m, 0.76083m, 0.74742m, 0.73481m, 0.7225m, 0.71036m, 0.69852m , 0.68736m,
                              0.67636m, 0.66553m, 0.65508m, 0.64489m, 0.63495m, 0.62526m, 0.61581m, 0.60617m, 0.59673m, 0.5872m, 0.57818m ,
                          0.56936m, 0.56073m , 0.55219m, 0.54384m, 0.53584m, 0.52793m, 0.52011m, 0.51245m };

            _dfatmGrid = new object[,]
            {
                { "9-May-08", 0.99980m },
                { "11-Aug-08", 0.98013m },
                { "11-Nov-08", 0.96125m },
                { "9-Feb-09", 0.94325m },
                { "11-May-09", 0.92562m },
                { "10-Aug-09", 0.90854m },
                { "9-Nov-09", 0.89198m },
                { "9-Feb-10", 0.87574m },
                { "10-May-10", 0.86029m },
                { "9-Aug-10", 0.84500m },
                { "9-Nov-10", 0.82997m },
                { "9-Feb-11", 0.81536m },
                { "9-May-11", 0.80161m },
                { "9-Aug-11", 0.78707m },
                { "9-Nov-11", 0.77283m },
                { "9-Feb-12", 0.75889m },
                { "9-May-12", 0.74553m },
                { "9-Aug-12", 0.73247m },
                { "9-Nov-12", 0.71971m },
                { "11-Feb-13", 0.70699m },
                { "9-May-13", 0.69548m },
                { "9-Aug-13", 0.68325m },
                { "11-Nov-13", 0.67102m },
                { "10-Feb-14", 0.65944m },
                { "9-May-14", 0.64846m },
                { "11-Aug-14", 0.63697m },
                { "10-Nov-14", 0.62608m },
                { "9-Feb-15", 0.61542m },
                { "11-May-15", 0.60497m },
                { "10-Aug-15", 0.59456m },
                { "9-Nov-15", 0.58435m },
                { "9-Feb-16", 0.57423m },
                { "9-May-16", 0.56452m },
                { "9-Aug-16", 0.55479m },
                { "9-Nov-16", 0.54524m },
                { "9-Feb-17", 0.53590m },
                { "9-May-17", 0.52702m },
                { "9-Aug-17", 0.51803m },
                { "9-Nov-17", 0.50921m },
                { "9-Feb-18", 0.50057m },
                { "9-May-18", 0.49237m },
                { "9-Aug-18", 0.48411m },
                { "9-Nov-18", 0.47600m },
                { "11-Feb-19", 0.46789m },
                { "9-May-19", 0.46052m },
                { "9-Aug-19", 0.45288m },
                { "11-Nov-19", 0.44523m },
                { "10-Feb-20", 0.43797m },
                { "11-May-20", 0.43085m },
                { "10-Aug-20", 0.42388m },
                { "9-Nov-20", 0.41704m },
                { "9-Feb-21", 0.41026m },
                { "10-May-21", 0.40375m },
                { "9-Aug-21", 0.39730m },
                { "9-Nov-21", 0.39091m },
                { "9-Feb-22", 0.38464m },
                { "9-May-22", 0.37870m },
                { "9-Aug-22", 0.37267m },
                { "9-Nov-22", 0.36676m },
                { "9-Feb-23", 0.36097m },
                { "9-May-23", 0.35548m }
            };
            _atmdates = new DateTime[_dfatmGrid.GetUpperBound(0)+1];
            _atmdfGrid = new decimal[_dfatmGrid.GetUpperBound(0)+1];
            for (index = 0; index < _dfatmGrid.GetUpperBound(0) + 1; index++)
            {
                _atmdates[index] = Convert.ToDateTime(_dfatmGrid[index,0]);
                _atmdfGrid[index] = (decimal)_dfatmGrid[index, 1];
            }

            #endregion

            #region Initialisation of the Caplet Bootstrap Settings

            _settings = new object[,]
                            {
                                {"Instrument", "AUD-Xibor-3M"},
                                { "PricingStructureType", "CapVolatilityCurve"},
                                { "BaseDate", new DateTime(2007,11,29) },
                                { "ValuationDate", new DateTime(2007,11,29) },
                                { "IndexTenor", "3M" },
                                { "IndexName", "AUD-BBR-BBSW" },
                                { "MarketName", "Test" },
                                { "Currency", "AUD" },
                                { "StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()},
                                { "MeasureType", MeasureTypesEnum.Volatility.ToString()},
                                { "QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString()},
                                { "Algorithm", "Default"},
                                { "EngineHandle", EngineHandle },
                                { "SettingsHandle", SettingsHandle }
                            };

            _atmSettings = new object[,]
                               {
                                   {"Instrument", "AUD-Xibor-3M"},
                                   { "PricingStructureType", "CapVolatilityCurve"},
                                   { "BaseDate", new DateTime(2008,5, 8) },
                                   { "IndexTenor", "3M" },
                                   { "IndexName", "AUD-BBR-BBSW" },
                                   { "MarketName", "Test" },
                                   { "Currency", "AUD" },
                                   { "StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()},
                                   { "MeasureType", MeasureTypesEnum.Volatility.ToString()},
                                   { "QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString()},
                                   { "Algorithm", "Default"},
                                   { "EngineHandle", ATMEngineHandle }
                               };

            _surfaceATMSettings = new object[,]
                                      {
                                          {"Instrument", "USD-Xibor-3M"},
                                          { "PricingStructureType", "CapVolatilityCurve"},
                                          { "BaseDate", new DateTime(2008,5, 14) },
                                          { "IndexTenor", "3M" },
                                          { "IndexName", "USD-LIBOR-BBA" },
                                          { "MarketName", "Test" },
                                          { "Currency", "USD" },
                                          { "StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()},
                                          { "MeasureType", MeasureTypesEnum.Volatility.ToString()},
                                          { "QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString()},
                                          { "Algorithm", "Default"},
                                          { "EngineHandle", SurfaceATMEngine }
                                      };

            _surfaceFixSettings = new object[,]
                                      {
                                          {"Instrument", "USD-Xibor-3M"},
                                          { "PricingStructureType", "CapVolatilityCurve"},
                                          { "BaseDate", new DateTime(2008,5, 14) },
                                          { "IndexTenor", "3M" },
                                          { "IndexName", "USD-LIBOR-BBA" },
                                          { "MarketName", "Test" },
                                          { "Currency", "USD" },
                                          { "StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()},
                                          { "MeasureType", MeasureTypesEnum.Volatility.ToString()},
                                          { "QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString()},
                                          { "Algorithm", "Default"},
                                          { "EngineHandle", SurfaceFixedEngine}
                                      };

            #endregion

            #region Surface Smile Setup

            _surfaceATMVols = new object[,]
                                  {
                                      { "USD-IRCAP-1Y", 33.24m / 100.0m},
                                      { "USD-IRCAP-2Y", 33.59m / 100.0m},
                                      { "USD-IRCAP-3Y", 31.27m / 100.0m},
                                      { "USD-IRCAP-4Y", 29.09m / 100.0m},
                                      { "USD-IRCAP-5Y", 27.38m / 100.0m},
                                      { "USD-IRCAP-6Y", 25.86m / 100.0m},
                                      { "USD-IRCAP-7Y", 24.62m / 100.0m},
                                      { "USD-IRCAP-8Y", 23.60m / 100.0m},
                                      { "USD-IRCAP-9Y", 22.76m / 100.0m},
                                      { "USD-IRCAP-10Y", 21.99m / 100.0m},
                                      { "USD-IRCAP-12Y", 20.69m / 100.0m},
                                      { "USD-IRCAP-15Y", 19.22m / 100.0m},
                                      { "USD-IRCAP-20Y", 17.67m / 100.0m}
                                  };

            // Surface test DFs
            _surfaceDF = new object[,]
                             {
                                 { "Date", "Discount Factor" },
                                 { "16-May-08", 0.9999m },
                                 { "18-Aug-08", 0.9929m },
                                 { "17-Nov-08", 0.9858m },
                                 { "17-Feb-09", 0.9782m },
                                 { "18-May-09", 0.9704m },
                                 { "17-Aug-09", 0.9623m },
                                 { "16-Nov-09", 0.9538m },
                                 { "16-Feb-10", 0.9448m },
                                 { "17-May-10", 0.9358m },
                                 { "16-Aug-10", 0.9265m },
                                 { "16-Nov-10", 0.9168m },
                                 { "16-Feb-11", 0.9070m },
                                 { "16-May-11", 0.8975m },
                                 { "16-Aug-11", 0.8876m },
                                 { "16-Nov-11", 0.8778m },
                                 { "16-Feb-12", 0.8678m },
                                 { "16-May-12", 0.8579m },
                                 { "16-Aug-12", 0.8480m },
                                 { "16-Nov-12", 0.8380m },
                                 { "19-Feb-13", 0.8276m },
                                 { "16-May-13", 0.8181m },
                                 { "16-Aug-13", 0.8081m },
                                 { "18-Nov-13", 0.7979m },
                                 { "18-Feb-14", 0.7879m },
                                 { "16-May-14", 0.7783m },
                                 { "18-Aug-14", 0.7683m },
                                 { "17-Nov-14", 0.7586m },
                                 { "17-Feb-15", 0.7489m },
                                 { "18-May-15", 0.7393m },
                                 { "17-Aug-15", 0.7299m },
                                 { "16-Nov-15", 0.7205m },
                                 { "16-Feb-16", 0.7111m },
                                 { "16-May-16", 0.7019m },
                                 { "16-Aug-16", 0.6928m },
                                 { "16-Nov-16", 0.6837m },
                                 { "16-Feb-17", 0.6747m },
                                 { "16-May-17", 0.6659m },
                                 { "16-Aug-17", 0.6571m },
                                 { "16-Nov-17", 0.6483m },
                                 { "16-Feb-18", 0.6395m },
                                 { "16-May-18", 0.6311m },
                                 { "16-Aug-18", 0.6227m },
                                 { "16-Nov-18", 0.6143m },
                                 { "19-Feb-19", 0.6057m },
                                 { "16-May-19", 0.5979m },
                                 { "16-Aug-19", 0.5897m },
                                 { "18-Nov-19", 0.5814m },
                                 { "18-Feb-20", 0.5733m },
                                 { "18-May-20", 0.5654m },
                                 { "17-Aug-20", 0.5578m },
                                 { "16-Nov-20", 0.5503m },
                                 { "16-Feb-21", 0.5428m },
                                 { "17-May-21", 0.5355m },
                                 { "16-Aug-21", 0.5282m },
                                 { "16-Nov-21", 0.5208m },
                                 { "16-Feb-22", 0.5136m },
                                 { "16-May-22", 0.5066m },
                                 { "16-Aug-22", 0.4995m },
                                 { "16-Nov-22", 0.4924m },
                                 { "16-Feb-23", 0.4854m },
                                 { "16-May-23", 0.4787m },
                                 { "16-Aug-23", 0.4723m },
                                 { "16-Nov-23", 0.4661m },
                                 { "16-Feb-24", 0.4599m },
                                 { "16-May-24", 0.4539m },
                                 { "16-Aug-24", 0.4478m },
                                 { "18-Nov-24", 0.4417m },
                                 { "18-Feb-25", 0.4358m },
                                 { "16-May-25", 0.4302m },
                                 { "18-Aug-25", 0.4243m },
                                 { "17-Nov-25", 0.4186m },
                                 { "17-Feb-26", 0.4129m },
                                 { "18-May-26", 0.4075m },
                                 { "17-Aug-26", 0.4020m },
                                 { "16-Nov-26", 0.3966m },
                                 { "16-Feb-27", 0.3911m },
                                 { "17-May-27", 0.3859m },
                                 { "16-Aug-27", 0.3807m },
                                 { "16-Nov-27", 0.3754m },
                                 { "16-Feb-28", 0.3702m },
                                 { "16-May-28", 0.3652m }
                             };

            // Fixed Strikes
            _surfaceFixedVols = new object[,]
                                    {
                                        { "Expiry", 1.00m / 100.0m, 1.50m / 100.0m, 2.00m / 100.0m, 2.50m / 100.0m, 3.00m / 100.0m, 3.50m / 100.0m, 4.00m / 100.0m, 5.00m / 100.0m, 6.00m / 100.0m, 7.00m / 100.0m, 8.00m / 100.0m, 9.00m / 100.0m, "Type" },
                                        { "1Y", 65.7m / 100.0m, 53m / 100.0m, 44.2m / 100.0m, 37m / 100.0m, 33.3m / 100.0m, 32.3m / 100.0m, 31.3m / 100.0m, 30.4m / 100.0m, 27.3m / 100.0m, 24.6m / 100.0m, 22.8m / 100.0m, 21.3m / 100.0m, "Cap/Floor" },
                                        { "2Y", 56.9m / 100.0m, 48.5m / 100.0m, 42.9m / 100.0m, 38.2m / 100.0m, 34.9m / 100.0m, 33m / 100.0m, 31.4m / 100.0m, 28.7m / 100.0m, 26.2m / 100.0m, 23.8m / 100.0m, 21.9m / 100.0m, 20.4m / 100.0m, "Cap/Floor" },
                                        { "3Y", 53.3m / 100.0m, 45.9m / 100.0m, 41m / 100.0m, 37.1m / 100.0m, 34.1m / 100.0m, 31.9m / 100.0m, 29.8m / 100.0m, 26.5m / 100.0m, 24.6m / 100.0m, 23.4m / 100.0m, 22.6m / 100.0m, 22m / 100.0m, "Cap/Floor" },
                                        { "4Y", 50.6m / 100.0m, 43.6m / 100.0m, 39.2m / 100.0m, 35.7m / 100.0m, 32.9m / 100.0m, 30.6m / 100.0m, 28.5m / 100.0m, 24.9m / 100.0m, 22.8m / 100.0m, 21.8m / 100.0m, 21.1m / 100.0m, 20.5m / 100.0m, "Cap/Floor" },
                                        { "5Y", 48.7m / 100.0m, 42m / 100.0m, 37.8m / 100.0m, 34.5m / 100.0m, 31.8m / 100.0m, 29.6m / 100.0m, 27.4m / 100.0m, 23.8m / 100.0m, 21.7m / 100.0m, 20.6m / 100.0m, 20m / 100.0m, 19.5m / 100.0m, "Cap/Floor" },
                                        { "6Y", 46.8m / 100.0m, 40.4m / 100.0m, 36.4m / 100.0m, 33.4m / 100.0m, 30.8m / 100.0m, 28.6m / 100.0m, 26.5m / 100.0m, 22.9m / 100.0m, 20.6m / 100.0m, 19.6m / 100.0m, 19m / 100.0m, 18.6m / 100.0m, "Cap/Floor" },
                                        { "7Y", 45.4m / 100.0m, 39.2m / 100.0m, 35.3m / 100.0m, 32.4m / 100.0m, 29.9m / 100.0m, 27.8m / 100.0m, 25.7m / 100.0m, 22.1m / 100.0m, 19.8m / 100.0m, 18.7m / 100.0m, 18.2m / 100.0m, 17.8m / 100.0m, "Cap/Floor" },
                                        { "8Y", 44.2m / 100.0m, 38.2m / 100.0m, 34.4m / 100.0m, 31.6m / 100.0m, 29.2m / 100.0m, 27.1m / 100.0m, 25m / 100.0m, 21.5m / 100.0m, 19.2m / 100.0m, 18.1m / 100.0m / 100.0m, 17.6m / 100.0m, 17.2m / 100.0m, "Cap/Floor" },
                                        { "9Y", 43.3m / 100.0m, 37.3m / 100.0m, 33.7m / 100.0m, 30.9m / 100.0m, 28.6m / 100.0m, 26.4m / 100.0m, 24.5m / 100.0m, 20.9m / 100.0m, 18.6m / 100.0m, 17.5m / 100.0m, 17m / 100.0m, 16.7m / 100.0m, "Cap/Floor" },
                                        { "10Y", 42.3m / 100.0m, 36.5m / 100.0m, 32.9m / 100.0m, 30.2m / 100.0m, 27.9m / 100.0m, 25.9m / 100.0m, 23.9m / 100.0m, 20.5m / 100.0m, 18.1m / 100.0m, 17m / 100.0m, 16.5m / 100.0m, 16.2m / 100.0m, "Cap/Floor" },
                                        { "12Y", 40.9m / 100.0m, 35.3m / 100.0m, 31.7m / 100.0m, 29.1m / 100.0m, 26.9m / 100.0m, 24.9m / 100.0m, 22.9m / 100.0m, 19.6m / 100.0m, 17.2m / 100.0m, 16.1m / 100.0m, 15.5m / 100.0m, 15.3m / 100.0m, "Cap/Floor" },
                                        { "15Y", 39.3m / 100.0m, 33.6m / 100.0m, 30.1m / 100.0m, 27.6m / 100.0m, 25.5m / 100.0m, 23.6m / 100.0m, 21.8m / 100.0m, 18.5m / 100.0m, 16.3m / 100.0m, 15.1m / 100.0m, 14.5m / 100.0m, 14.3m / 100.0m, "Cap/Floor" },
                                        { "20Y", 37.2m / 100.0m, 31.6m / 100.0m, 28.2m / 100.0m, 25.9m / 100.0m, 23.9m / 100.0m, 22.1m / 100.0m, 20.4m / 100.0m, 17.3m / 100.0m, 15.2m / 100.0m, 14.1m / 100.0m, 13.6m / 100.0m, 13.3m / 100.0m, "Cap/Floor" }
                                    };

           _aTmVols = new[]
                                  {   33.24m / 100.0m,
                                      33.59m / 100.0m,
                                      31.27m / 100.0m,
                                      29.09m / 100.0m,
                                      27.38m / 100.0m,
                                      25.86m / 100.0m,
                                      24.62m / 100.0m,
                                      23.60m / 100.0m,
                                      22.76m / 100.0m,
                                      21.99m / 100.0m,
                                      20.69m / 100.0m,
                                      19.22m / 100.0m,
                                      17.67m / 100.0m
                                  };

            // Surface test DFs
           _surfaceDF2 = new object[,]
                             {
                                 { "16-May-08", 0.9999m },
                                 { "18-Aug-08", 0.9929m },
                                 { "17-Nov-08", 0.9858m },
                                 { "17-Feb-09", 0.9782m },
                                 { "18-May-09", 0.9704m },
                                 { "17-Aug-09", 0.9623m },
                                 { "16-Nov-09", 0.9538m },
                                 { "16-Feb-10", 0.9448m },
                                 { "17-May-10", 0.9358m },
                                 { "16-Aug-10", 0.9265m },
                                 { "16-Nov-10", 0.9168m },
                                 { "16-Feb-11", 0.9070m },
                                 { "16-May-11", 0.8975m },
                                 { "16-Aug-11", 0.8876m },
                                 { "16-Nov-11", 0.8778m },
                                 { "16-Feb-12", 0.8678m },
                                 { "16-May-12", 0.8579m },
                                 { "16-Aug-12", 0.8480m },
                                 { "16-Nov-12", 0.8380m },
                                 { "19-Feb-13", 0.8276m },
                                 { "16-May-13", 0.8181m },
                                 { "16-Aug-13", 0.8081m },
                                 { "18-Nov-13", 0.7979m },
                                 { "18-Feb-14", 0.7879m },
                                 { "16-May-14", 0.7783m },
                                 { "18-Aug-14", 0.7683m },
                                 { "17-Nov-14", 0.7586m },
                                 { "17-Feb-15", 0.7489m },
                                 { "18-May-15", 0.7393m },
                                 { "17-Aug-15", 0.7299m },
                                 { "16-Nov-15", 0.7205m },
                                 { "16-Feb-16", 0.7111m },
                                 { "16-May-16", 0.7019m },
                                 { "16-Aug-16", 0.6928m },
                                 { "16-Nov-16", 0.6837m },
                                 { "16-Feb-17", 0.6747m },
                                 { "16-May-17", 0.6659m },
                                 { "16-Aug-17", 0.6571m },
                                 { "16-Nov-17", 0.6483m },
                                 { "16-Feb-18", 0.6395m },
                                 { "16-May-18", 0.6311m },
                                 { "16-Aug-18", 0.6227m },
                                 { "16-Nov-18", 0.6143m },
                                 { "19-Feb-19", 0.6057m },
                                 { "16-May-19", 0.5979m },
                                 { "16-Aug-19", 0.5897m },
                                 { "18-Nov-19", 0.5814m },
                                 { "18-Feb-20", 0.5733m },
                                 { "18-May-20", 0.5654m },
                                 { "17-Aug-20", 0.5578m },
                                 { "16-Nov-20", 0.5503m },
                                 { "16-Feb-21", 0.5428m },
                                 { "17-May-21", 0.5355m },
                                 { "16-Aug-21", 0.5282m },
                                 { "16-Nov-21", 0.5208m },
                                 { "16-Feb-22", 0.5136m },
                                 { "16-May-22", 0.5066m },
                                 { "16-Aug-22", 0.4995m },
                                 { "16-Nov-22", 0.4924m },
                                 { "16-Feb-23", 0.4854m },
                                 { "16-May-23", 0.4787m },
                                 { "16-Aug-23", 0.4723m },
                                 { "16-Nov-23", 0.4661m },
                                 { "16-Feb-24", 0.4599m },
                                 { "16-May-24", 0.4539m },
                                 { "16-Aug-24", 0.4478m },
                                 { "18-Nov-24", 0.4417m },
                                 { "18-Feb-25", 0.4358m },
                                 { "16-May-25", 0.4302m },
                                 { "18-Aug-25", 0.4243m },
                                 { "17-Nov-25", 0.4186m },
                                 { "17-Feb-26", 0.4129m },
                                 { "18-May-26", 0.4075m },
                                 { "17-Aug-26", 0.4020m },
                                 { "16-Nov-26", 0.3966m },
                                 { "16-Feb-27", 0.3911m },
                                 { "17-May-27", 0.3859m },
                                 { "16-Aug-27", 0.3807m },
                                 { "16-Nov-27", 0.3754m },
                                 { "16-Feb-28", 0.3702m },
                                 { "16-May-28", 0.3652m }
                             };
           _surfaceatmdates = new DateTime[_surfaceDF2.GetUpperBound(0) + 1];
           _surfaceAtmdfGrid = new decimal[_surfaceDF2.GetUpperBound(0) + 1];
            for (index = 0; index < _surfaceDF2.GetUpperBound(0) + 1; index++)
            {
                _surfaceatmdates[index] = Convert.ToDateTime(_surfaceDF2[index, 0]);
                _surfaceAtmdfGrid[index] = (decimal)_surfaceDF2[index, 1];
            }

            // Fixed Strikes
            _surfaceFixedVols2 = new[,]
                                    {
                                        { 65.7m / 100.0m, 53m / 100.0m, 44.2m / 100.0m, 37m / 100.0m, 33.3m / 100.0m, 32.3m / 100.0m, 31.3m / 100.0m, 30.4m / 100.0m, 27.3m / 100.0m, 24.6m / 100.0m, 22.8m / 100.0m, 21.3m / 100.0m},
                                        { 56.9m / 100.0m, 48.5m / 100.0m, 42.9m / 100.0m, 38.2m / 100.0m, 34.9m / 100.0m, 33m / 100.0m, 31.4m / 100.0m, 28.7m / 100.0m, 26.2m / 100.0m, 23.8m / 100.0m, 21.9m / 100.0m, 20.4m / 100.0m},
                                        { 53.3m / 100.0m, 45.9m / 100.0m, 41m / 100.0m, 37.1m / 100.0m, 34.1m / 100.0m, 31.9m / 100.0m, 29.8m / 100.0m, 26.5m / 100.0m, 24.6m / 100.0m, 23.4m / 100.0m, 22.6m / 100.0m, 22m / 100.0m},
                                        { 50.6m / 100.0m, 43.6m / 100.0m, 39.2m / 100.0m, 35.7m / 100.0m, 32.9m / 100.0m, 30.6m / 100.0m, 28.5m / 100.0m, 24.9m / 100.0m, 22.8m / 100.0m, 21.8m / 100.0m, 21.1m / 100.0m, 20.5m / 100.0m},
                                        { 48.7m / 100.0m, 42m / 100.0m, 37.8m / 100.0m, 34.5m / 100.0m, 31.8m / 100.0m, 29.6m / 100.0m, 27.4m / 100.0m, 23.8m / 100.0m, 21.7m / 100.0m, 20.6m / 100.0m, 20m / 100.0m, 19.5m / 100.0m},
                                        { 46.8m / 100.0m, 40.4m / 100.0m, 36.4m / 100.0m, 33.4m / 100.0m, 30.8m / 100.0m, 28.6m / 100.0m, 26.5m / 100.0m, 22.9m / 100.0m, 20.6m / 100.0m, 19.6m / 100.0m, 19m / 100.0m, 18.6m / 100.0m},
                                        { 45.4m / 100.0m, 39.2m / 100.0m, 35.3m / 100.0m, 32.4m / 100.0m, 29.9m / 100.0m, 27.8m / 100.0m, 25.7m / 100.0m, 22.1m / 100.0m, 19.8m / 100.0m, 18.7m / 100.0m, 18.2m / 100.0m, 17.8m / 100.0m},
                                        { 44.2m / 100.0m, 38.2m / 100.0m, 34.4m / 100.0m, 31.6m / 100.0m, 29.2m / 100.0m, 27.1m / 100.0m, 25m / 100.0m, 21.5m / 100.0m, 19.2m / 100.0m, 18.1m / 100.0m , 17.6m / 100.0m, 17.2m / 100.0m},
                                        { 43.3m / 100.0m, 37.3m / 100.0m, 33.7m / 100.0m, 30.9m / 100.0m, 28.6m / 100.0m, 26.4m / 100.0m, 24.5m / 100.0m, 20.9m / 100.0m, 18.6m / 100.0m, 17.5m / 100.0m, 17m / 100.0m, 16.7m / 100.0m},
                                        { 42.3m / 100.0m, 36.5m / 100.0m, 32.9m / 100.0m, 30.2m / 100.0m, 27.9m / 100.0m, 25.9m / 100.0m, 23.9m / 100.0m, 20.5m / 100.0m, 18.1m / 100.0m, 17m / 100.0m, 16.5m / 100.0m, 16.2m / 100.0m},
                                        { 40.9m / 100.0m, 35.3m / 100.0m, 31.7m / 100.0m, 29.1m / 100.0m, 26.9m / 100.0m, 24.9m / 100.0m, 22.9m / 100.0m, 19.6m / 100.0m, 17.2m / 100.0m, 16.1m / 100.0m, 15.5m / 100.0m, 15.3m / 100.0m},
                                        { 39.3m / 100.0m, 33.6m / 100.0m, 30.1m / 100.0m, 27.6m / 100.0m, 25.5m / 100.0m, 23.6m / 100.0m, 21.8m / 100.0m, 18.5m / 100.0m, 16.3m / 100.0m, 15.1m / 100.0m, 14.5m / 100.0m, 14.3m / 100.0m},
                                        { 37.2m / 100.0m, 31.6m / 100.0m, 28.2m / 100.0m, 25.9m / 100.0m, 23.9m / 100.0m, 22.1m / 100.0m, 20.4m / 100.0m, 17.3m / 100.0m, 15.2m / 100.0m, 14.1m / 100.0m, 13.6m / 100.0m, 13.3m / 100.0m}
                                    };

            _surfaceInstruments = new[] { "USD-CAPLET-1M-3M", "USD-CAPLET-4M-3M", "USD-CAPLET-7M-3M", "USD-IRCAP-1Y",
                "USD-IRCAP-2Y", "USD-IRCAP-3Y", "USD-IRCAP-4Y", "USD-IRCAP-5Y" };
            _surfaceStrikes = new []
                                  {
                                      1.00m/100.0m, 1.50m/100.0m, 2.00m/100.0m, 2.50m/100.0m, 3.00m/100.0m, 3.50m/100.0m,
                                      4.00m/100.0m, 5.00m/100.0m, 6.00m/100.0m, 7.00m/100.0m, 8.00m/100.0m, 9.00m/100.0m
                                  };
            #endregion
        }

        /// <summary>
        /// Test the CreateSettings method
        /// </summary>
        [TestMethod]
        public void Test02SABRCreateCapFloorSettingsTest()
        {
            object[,] settings = {
                {"Instrument", "AUD-Xibor-3M"},
                { "PricingStructureType", "CapVolatilityCurve"},//Default value
                { "BaseDate", new DateTime(2007,11,29) },
                { "ValuationDate", new DateTime(2007,11,29) },//Default value is the base date
                { "IndexTenor", "3M" },
                { "IndexName", "AUD-BBR-BBSW" },
                { "MarketName", "Test" },
                { "Currency", "AUD" },
                { "StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()},//Default value
                { "MeasureType", MeasureTypesEnum.Volatility.ToString()},//Default value
                { "QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString()},//Default value
                { "Algorithm", "Default"},//Default value
                { "EngineHandle", _engineHandle }
            };
            string errorNoConvention = "Unknown roll convention MODBANANAS specified.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            // This one to fail
            try
            {
                var properties = cfInterface.CreateCapFloorProperties(settings);
                Assert.IsNotNull(properties);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(errorNoConvention, ex.Message);
            }
        }

        #endregion

        #region New Tests

        /// <summary>
        /// Test the CreateSettings method
        /// </summary>
        [TestMethod]
        public void Test04SABRCreateCapFloorSettingsTest2()
        {
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_settings);
            var s = properties.GetString("SettingsHandle", null);
            Assert.AreEqual(SettingsHandle, s);
        }

        /// <summary>
        /// Create an engine with no settings
        /// </summary>
        [TestMethod]
        public void Test1SABRCreateCapFloorEngineTest()
        {
            string expected = "Invalid Handle - settings object not found.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_settings);
            try
            {
                string e = cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, _volTypes, _rawATMVolGrid2, _dates, _dfGrid2);
                Assert.AreEqual(_engineHandle, e);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Test creating CapFloor engines with fixed strike
        /// </summary>
        [TestMethod]
        public void Test11SABRCreateCapFloorEngineTest1()
        {
            //_engineHandle,
            //Tests the simple case of an ATM array and the corresponding volatility array. 
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_settings);
            Assert.IsNotNull(properties);
            string e = cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, _volTypes, _rawATMVolGrid2, _dates, _dfGrid2);
            Assert.AreEqual(_engineHandle, e);
        }

        /// <summary>
        /// Test creating CapFloor engines with fixed strike and add it twice
        /// </summary>
        [TestMethod]
        public void Test12SABRCreateCapFloorEngineTest2()
        {
            //"MultiLoad Engine", 
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_settings);
            properties.Set("EngineHandle", "MultiLoad Engine");
            properties.Set("ValuationDate", DateTime.Parse(CalculationDateString));
            Assert.IsNotNull(properties);
            var e = cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, _volTypes, _rawVolGrid2, _dates, _dfGrid2);
            Assert.AreEqual("MultiLoad Engine", e);
        }

        /// <summary>
        /// Test creating CapFloor engines with ATM vols only (no strike vals)
        /// </summary>
        [TestMethod]
        public void Test13SABRCreateATMCapFloorEngineTest()
        {
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_atmSettings);
            properties.Set("EngineHandle", AtmEngineHandle);
            properties.Set("ValuationDate", DateTime.Parse(CalculationDateString));
            Assert.IsNotNull(properties);
            string e = cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties,
                 _volTypes, _rawATMVolGrid2, _atmdates, _atmdfGrid);
            Assert.AreEqual(AtmEngineHandle, e);
        }

        /// <summary>
        /// Test creating CapFloor engines with ATM vols only - Fail with a value &lt;0
        /// </summary>
        [TestMethod]
        public void BootstrapCreateCapFloorAtmEngineIllegalVolatilityTest()
        {
            Decimal[] rawVolGrid = { -1.3m / 100.0m, 12.34m / 100.0m, 13.81m / 100.0m, 12.16m / 100.0m, 13.083m / 100.0m };
            String[] instrumentTypes = { "AUD-CAPLET-36D-90D", "AUD-CAPLET-127D-90D", "AUD-CAPLET-218D-90D", "AUD-CAPLET-309D-90D", "AUD-IRCAP-2Y" };
            const string expected = "An illegal volatility value";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_atmSettings);
            Assert.IsNotNull(properties);
            try
            {
                cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, instrumentTypes,
                    rawVolGrid, _atmdates, _atmdfGrid);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith(expected));
            }
        }

        /// <summary>
        /// Test creating CapFloor engines with ATM vols only
        /// </summary>
        [TestMethod]
        public void Test17SABRCreateATMCapFloorEngineTest4()
        {
            String[] instruments = {"AUD-CAPLET-36D-90D", "AUD-CAPLET-127D-90D", "AUD-CAPLET-218D-90D", "AUD-CAPLET-309D-90D", "AUD-IRCAP-2Y" ,
                "AUD-IRCAP-3Y", "AUD-IRCAP-4Y", "AUD-IRCAP-5Y", "AUD-IRCAP-7Y", "AUD-IRCAP-10Y", "AUD-IRCAP-15Y"};
            var rawVolGrid = new[] {11.55m / 100.0m, 12.34m / 100.0m, 13.81m / 100.0m, 12.16m / 100.0m, 13.083m / 100.0m, 13.37m / 100.0m, 13.49m / 100.0m, 13.395m / 100.0m,
                13.158m / 100.0m, 12.013m / 100.0m, 10.865m / 100.0m};         
            const string expected = "Invalid Handle - settings object not found.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_atmSettings);
            properties.Set("EngineHandle", AtmEngineHandle);
            properties.Set("ValuationDate", DateTime.Parse(CalculationDateString));
            Assert.IsNotNull(properties);
            try
            {
                var e = cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, instruments,
                    rawVolGrid, _atmdates, _atmdfGrid);
                Assert.AreEqual(AtmEngineHandle, e);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Test ComputeCapletVol when the ATM curve only exists.
        /// </summary>
        [TestMethod]
        public void BootstrapComputeATMCapletVolatilityTest()
        {
            String[] instruments = { "AUD-CAPLET-0D-90D", "AUD-CAPLET-36D-90D", "AUD-CAPLET-127D-90D", "AUD-CAPLET-218D-90D", "AUD-CAPLET-309D-90D", "AUD-IRCAP-2Y" ,
                "AUD-IRCAP-3Y", "AUD-IRCAP-4Y" };
            var rawVolGrid = new[] { 11.55m / 100.0m, 11.55m / 100.0m, 12.34m / 100.0m, 13.81m / 100.0m, 12.16m / 100.0m, 13.083m / 100.0m, 13.37m / 100.0m, 13.49m / 100.0m };
            DateTime target = new DateTime(2011, 8, 9);
            const decimal expected = 0.136825790124822M;// 0.112374725274725M
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_atmSettings);
            properties.Set("EngineHandle", AtmEngineHandle);
            Assert.IsNotNull(properties);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var e = cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, instruments,
                rawVolGrid, _atmdates, _atmdfGrid);
            var vol = cfInterface.BootstrapComputeCapletVolatility(e, 0.0m, baseDate, target);
            Assert.AreEqual(expected, vol);
        }

        /// <summary>
        /// Test ComputeCapletVol fail with incorrect strike specified
        /// </summary>
        [TestMethod]
        public void BootstrapComputeCapletVolatilityInvalidStrikeTest()
        {
            String[] instruments = { "AUD-CAPLET-36D-90D", "AUD-CAPLET-127D-90D", "AUD-CAPLET-218D-90D", "AUD-CAPLET-309D-90D", "AUD-IRCAP-2Y" ,
                "AUD-IRCAP-3Y", "AUD-IRCAP-4Y" };
            var rawVolGrid = new[] { 11.55m / 100.0m, 12.34m / 100.0m, 13.81m / 100.0m, 12.16m / 100.0m, 13.083m / 100.0m, 13.37m / 100.0m, 13.49m / 100.0m };
            DateTime target = new DateTime(2011, 8, 9);
            const string expectedFail = "The strike value: 99.99 is not valid for this CapletBootstrapper.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_settings);
            properties.Set("Strike", 99.99m);//Don't understand this for an ATM;
            Assert.IsNotNull(properties);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            string tempEngineHandle = cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, instruments,
                    rawVolGrid, _atmdates, _atmdfGrid);
            Assert.AreEqual(_engineHandle, tempEngineHandle);
            try
            {
                cfInterface.BootstrapComputeCapletVolatility(tempEngineHandle, 99.99m, baseDate, target);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expectedFail, ex.Message);
            }
        }

        /// <summary>
        /// Test ComputeCapletVol fail with incorrect engine specified
        /// </summary>
        [TestMethod]
        public void BootstrapComputeCapletVolatilityMissingEngineTest()
        {
            DateTime target = new DateTime(2011, 8, 9);
            const string expectedFail = "The engine: Not an Engine is not present. The volatility cannot be computed.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_settings);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            try
            {
                cfInterface.BootstrapComputeCapletVolatility("Not an Engine", 0, baseDate, target);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(expectedFail, ex.Message);
            }
        }

        /// <summary>
        /// Test ComputeCapletVol using an ATM engine
        /// </summary>
        [TestMethod]
        public void Test24BootstrapComputeCapletVolatilityTest3()
        {
            String[] instruments = {"AUD-CAPLET-36D-90D", "AUD-CAPLET-127D-90D", "AUD-CAPLET-218D-90D", "AUD-CAPLET-309D-90D", "AUD-IRCAP-2Y" ,
                "AUD-IRCAP-3Y", "AUD-IRCAP-4Y", "AUD-IRCAP-5Y", "AUD-IRCAP-7Y", "AUD-IRCAP-10Y", "AUD-IRCAP-15Y"};
            var rawVolGrid = new[] {11.55m/100m, 12.34m/100m, 13.81m/100m, 12.16m/100m, 13.083m/100m,
                13.37m/100m, 13.49m/100m, 13.395m/100m,  13.158m/100m, 12.013m/100m, 10.865m/100m};
            DateTime valDate = DateTime.Parse("8-May-2008");
            DateTime target = DateTime.Parse("9-Aug-2011");
            const decimal expected = 0.136806353314042M;
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(_atmSettings);
            Assert.IsNotNull(properties);
            properties.Set("EngineHandle", AtmEngineHandle);
            properties.Set("ValuationDate", valDate); 
            cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, instruments,
                rawVolGrid, _atmdates, _atmdfGrid);
            object o = cfInterface.BootstrapComputeCapletVolatility(AtmEngineHandle, 0, valDate, target);
            Assert.AreEqual(Math.Round(expected, 3), Math.Round(Convert.ToDecimal(o), 3));
        }

        /// <summary>
        /// Test listing engines - check no engines
        /// </summary>
        [TestMethod]
        public void Test01ListCapletBootstrapEnginesTest()
        {
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.NewInstance();
            const string expected = "No SABR Volatility Surfaces located.";
            string actual = cfInterface.ListCapletBootstrapEngines()[0];
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test listing engines
        /// </summary>
        [TestMethod]
        public void ListCapletBootstrapEnginesTest()
        {
            String[] instruments = { "AUD-CAPLET-36D-90D", "AUD-CAPLET-127D-90D", "AUD-CAPLET-218D-90D", "AUD-CAPLET-309D-90D", "AUD-IRCAP-2Y" };
            var rawVolGrid = new[,] { { .1155m }, { .1234m }, { .1381m }, { .1216m }, { .13083m } };
            var cfInterface = SABRCapFloorInterface.NewInstance();
            var properties = cfInterface.CreateCapFloorProperties(_settings);
            Assert.IsNotNull(properties);
            string engine = cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, instruments,
                    rawVolGrid, _atmdates, _atmdfGrid);
            Assert.AreEqual(_engineHandle, engine);
            string[] actual = cfInterface.ListCapletBootstrapEngines();
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual(_engineHandle, actual[0]);
        }

        /// <summary>
        /// Test listing engines
        /// </summary>
        [TestMethod]
        public void ListCapletBootstrapEnginesTest2()
        {
            String[] instruments = { "AUD-CAPLET-1D-90D", "AUD-CAPLET-3M-3M", "AUD-CAPLET-6M-3M", "AUD-IRCAP-1Y", "AUD-IRCAP-2Y" };
            var rawVolGrid = new[,] { { .1155m }, { .1234m }, { .1381m }, { .1216m }, { .13083m } };
            var cfInterface = SABRCapFloorInterface.NewInstance();
            var properties = cfInterface.CreateCapFloorProperties(_settings);
            Assert.IsNotNull(properties);
            string engine = cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, properties, instruments,
                rawVolGrid, _atmdates, _atmdfGrid);
            Assert.AreEqual(_engineHandle, engine);
            string[] actual = cfInterface.ListCapletBootstrapEngines();
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual(_engineHandle, actual[0]);
        }

        /// <summary>
        /// Create a surface settings object
        /// </summary>
        [TestMethod]
        public void Test26SABRCapFloorCalibrationSettingsTest()
        {
            const decimal smileBeta = 0.5m;
            const string interpolation = "CubicHermiteSpline";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            string s = cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
            Assert.AreEqual(SurfaceSmileSettings, s);
        }

        /// <summary>
        /// Create a surface settings object - Fail with an unknown interpolation method
        /// </summary>
        [TestMethod]
        public void Test27SABRCapFloorCalibrationSettingsTest()
        {
            //string smileSettingsHandle = SurfaceSmileSettings;
            decimal smileBeta = 0.5m;
            string interpolation = "DodgyInterp";
            string expected = $"Unknown interpolation method {interpolation} specified.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            try
            {
                string s = cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
                Assert.AreEqual(SurfaceSmileSettings, s);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Create a CapFloor surface from an ATM bootstrap engine, a set fixed bootstrap engines and settings
        /// Fail with an unknown ATM bootstrap engine
        /// </summary>
        [TestMethod]
        public void Test28SABRCapFloorCalibrationEngineTest()
        {
            const decimal smileBeta = 0.5m;
            const string interpolation = "CubicHermiteSpline";
            //DateTime valDate = DateTime.Parse("14-May-2008");
            const string calibrationEngine = "TestSurfaceEngine";
            const string expected = "ATM CapFloor Bootstrap engine not found.";
            try
            {
                SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
                var surfaceFixedSettings = cfInterface.CreateCapFloorProperties(_surfaceFixSettings);
                var surfaceATMProperties = cfInterface.CreateCapFloorProperties(_surfaceATMSettings);
                cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceFixedSettings, _surfaceInstruments,
                    _surfaceStrikes, _surfaceFixedVols2, _surfaceatmdates, _surfaceAtmdfGrid);//SurfaceFixedEngine, 
                cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceATMProperties, _surfaceInstruments,
                    _aTmVols, _surfaceatmdates, _surfaceAtmdfGrid);
                cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
                object actual = cfInterface.SABRCapFloorCalibrationEngine(calibrationEngine, SurfaceSmileSettings, SurfaceFixedEngine, "Broken Biscuit");
                Assert.AreEqual(calibrationEngine, Convert.ToString(actual));
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Create a CapFloor surface from an ATM bootstrap engine, a set fixed bootstrap engines and settings
        /// Fail with an unknown Fixed Strike bootstrap engine
        /// </summary>
        [TestMethod]
        public void SABRCapFloorCalibrationEngineUnknownEngineTest()
        {
            decimal smileBeta = 0.5m;
            string interpolation = "CubicHermiteSpline";
            string calibrationEngine = "TestSurfaceEngine";
            string expected = "Fixed Strike Bootstrap engines not found.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var surfaceFixedSettings = cfInterface.CreateCapFloorProperties(_surfaceFixSettings);
            var surfaceATMProperties = cfInterface.CreateCapFloorProperties(_surfaceATMSettings);
            cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceFixedSettings, _surfaceInstruments,
                _surfaceStrikes, _surfaceFixedVols2, _surfaceatmdates, _surfaceAtmdfGrid);
            cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceATMProperties, _surfaceInstruments,
                _aTmVols, _surfaceatmdates, _surfaceAtmdfGrid);
            cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
            try
            {
                cfInterface.SABRCapFloorCalibrationEngine(calibrationEngine, SurfaceSmileSettings, "Broken Biscuit", SurfaceATMEngine);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Create a CapFloor surface from an ATM bootstrap engine, a set fixed bootstrap engines and settings
        /// Fail with an unknown Surface Smile Settings handle
        /// </summary>
        [TestMethod]
        public void SabrCapFloorCalibrationEngineUnknownSettingsTest()
        {
            const decimal smileBeta = 0.5m;
            const string interpolation = "CubicHermiteSpline";
            const string expected = "Caplet Smile Settings not found.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var surfaceFixedSettings = cfInterface.CreateCapFloorProperties(_surfaceFixSettings);
            var surfaceATMProperties = cfInterface.CreateCapFloorProperties(_surfaceATMSettings);
            string engine = cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceFixedSettings, _surfaceInstruments,
                _surfaceStrikes, _surfaceFixedVols2, _surfaceatmdates, _surfaceAtmdfGrid);
            Assert.AreEqual(SurfaceFixedEngine, engine);
            string atmEngine = cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceATMProperties, _surfaceInstruments,
                _aTmVols, _surfaceatmdates, _surfaceAtmdfGrid);
            Assert.AreEqual(SurfaceATMEngine, atmEngine);
            string calibrationEngine = cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
            Assert.AreEqual(SurfaceSmileSettings, calibrationEngine);
            try
            {
                cfInterface.SABRCapFloorCalibrationEngine(calibrationEngine, "Broken Biscuit", engine, atmEngine);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Create a CapFloor surface from an ATM bootstrap engine, a set fixed bootstrap engines and settings
        /// </summary>
        [TestMethod]
        public void Test31SABRCapFloorCalibrationEngineTest()
        {
            decimal smileBeta = 0.5m;
            string interpolation = "CubicHermiteSpline";
            //DateTime valDate = DateTime.Parse("14-May-2008");
            string calibrationEngine = "TestSurfaceEngine";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var surfaceFixedSettings = cfInterface.CreateCapFloorProperties(_surfaceFixSettings);
            var surfaceATMProperties = cfInterface.CreateCapFloorProperties(_surfaceATMSettings);
            cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceFixedSettings, _surfaceInstruments,
                _surfaceStrikes, _surfaceFixedVols2, _surfaceatmdates, _surfaceAtmdfGrid);
            cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceATMProperties, _surfaceInstruments,
                _aTmVols, _surfaceatmdates, _surfaceAtmdfGrid);
            cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
            object actual = cfInterface.SABRCapFloorCalibrationEngine(calibrationEngine, SurfaceSmileSettings, SurfaceFixedEngine, SurfaceATMEngine);
            Assert.AreEqual(calibrationEngine, Convert.ToString(actual));
        }

        /// <summary>
        /// Create a CapFloor surface from an ATM bootstrap engine, a set fixed bootstrap engines and settings
        /// Add the caplet smile engine twice to check that multiple updates are possible
        /// </summary>
        [TestMethod]
        public void SabrCapFloorCalibrationEngineAddTwiceTest()
        {
            const decimal smileBeta = 0.5m;
            const string interpolation = "CubicHermiteSpline";
            const string calibrationEngine = "TestSurfaceEngine";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var surfaceFixedSettings = cfInterface.CreateCapFloorProperties(_surfaceFixSettings);
            var surfaceATMProperties = cfInterface.CreateCapFloorProperties(_surfaceATMSettings);
            string fixedEngine = cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceFixedSettings, _surfaceInstruments,
                _surfaceStrikes, _surfaceFixedVols2, _surfaceatmdates, _surfaceAtmdfGrid);
            string surfaceAtmEngine = cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceATMProperties, _surfaceInstruments,
                _aTmVols, _surfaceatmdates, _surfaceAtmdfGrid);
            Assert.AreEqual(SurfaceATMEngine, surfaceAtmEngine);
            string calibrationSettings = cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
            Assert.AreEqual(SurfaceSmileSettings, calibrationSettings);
            string actual = cfInterface.SABRCapFloorCalibrationEngine(calibrationEngine, SurfaceSmileSettings, fixedEngine, surfaceAtmEngine);
            Assert.AreEqual(calibrationEngine, actual);
            // Add it again
            actual = cfInterface.SABRCapFloorCalibrationEngine(calibrationEngine, SurfaceSmileSettings, fixedEngine, surfaceAtmEngine);
            Assert.AreEqual(calibrationEngine, actual);
        }

        /// <summary>
        /// Compute a volatility given a Caplet smile engine
        /// Fail with a non-existent engine
        /// </summary>
        [TestMethod]
        public void Test33SABRCapFloorComputeVolatilityTest()
        {
            decimal smileBeta = 0.5m;
            string interpolation = "CubicHermiteSpline";
            string calibrationEngine = "TestSurfaceEngine";
            //DateTime valDate = DateTime.Parse("14-May-2008");
            DateTime target = DateTime.Parse("9-Aug-2011");
            decimal[] strikeArray = { 0 };
            string expected = "SABR Caplet Smile Calibration engine not found.";
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var surfaceFixedSettings = cfInterface.CreateCapFloorProperties(_surfaceFixSettings);
            var surfaceATMProperties = cfInterface.CreateCapFloorProperties(_surfaceATMSettings);
            cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceFixedSettings, _surfaceInstruments,
                _surfaceStrikes, _surfaceFixedVols2, _surfaceatmdates, _surfaceAtmdfGrid);
            cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceATMProperties, _surfaceInstruments,
                _aTmVols, _surfaceatmdates, _surfaceAtmdfGrid);
            cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
            cfInterface.SABRCapFloorCalibrationEngine(calibrationEngine, SurfaceSmileSettings, SurfaceFixedEngine, SurfaceATMEngine);
            try
            {
                object actual = cfInterface.SABRCapFloorComputeVolatility(UTE.Logger, UTE.Cache, UTE.NameSpace, "Broken Biscuit", target, strikeArray);
                Assert.AreEqual(expected, actual);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Compute a volatility given a Caplet smile engine
        /// </summary>
        [TestMethod]
        public void Test34SABRCapFloorComputeVolatilityTest()
        {
            decimal smileBeta = 0.5m;
            string interpolation = "CubicHermiteSpline";
            string calibrationEngine = "TestSurfaceEngine";
            DateTime target = DateTime.Parse("16-Jun-2008");
            var strikeArray = new[] { 1.00m / 100.0m, 1.50m / 100.0m, 2.00m / 100.0m, 2.50m / 100.0m, 3.00m / 100.0m, 3.50m / 100.0m, 4.00m / 100.0m, 5.00m / 100.0m, 6.00m / 100.0m, 7.00m / 100.0m, 8.00m / 100.0m, 9.00m / 100.0m };
            decimal[] expected
                = 
                {
                    0.6153203956m, //62.03m / 100.0m
                    0.4988121502m, //50.38m / 100.0m
                    0.4161117610m, //42.10m / 100.0m
                    0.3533430143m, //35.79m / 100.0m
                    0.3056105837m, //30.97m / 100.0m
                    0.2718536124m, //27.50m / 100.0m
                    0.2516355430m, //25.35m / 100.0m
                    0.2401779861m, //24.02m / 100.0m
                    0.2451611840m, //24.44m / 100.0m
                    0.2541022321m, //25.31m / 100.0m
                    0.2634788102m, //26.24m / 100.0m
                    0.2723494869m  //27.12m / 100.0m
                };

            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var surfaceFixedSettings = cfInterface.CreateCapFloorProperties(_surfaceFixSettings);
            var surfaceATMProperties = cfInterface.CreateCapFloorProperties(_surfaceATMSettings);
            cfInterface.CreateCapFloorCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceFixedSettings, _surfaceInstruments,
                _surfaceStrikes, _surfaceFixedVols2, _surfaceatmdates, _surfaceAtmdfGrid);
            cfInterface.CreateCapFloorATMCurve(UTE.Logger, UTE.Cache, UTE.NameSpace, surfaceATMProperties,
                _surfaceInstruments, _aTmVols, _surfaceatmdates, _surfaceAtmdfGrid);
            cfInterface.SABRCapFloorCalibrationSettings(SurfaceSmileSettings, smileBeta, interpolation);
            cfInterface.SABRCapFloorCalibrationEngine(calibrationEngine, SurfaceSmileSettings, SurfaceFixedEngine, SurfaceATMEngine);
            var retVals = cfInterface.SABRCapFloorComputeVolatility(UTE.Logger, UTE.Cache, UTE.NameSpace, calibrationEngine, target, strikeArray);
            var actual = new List<decimal>();
            //var col = 0;
            foreach (var row in retVals)
            {
                actual.Add(row);
            }
            for (int i = 0; i < strikeArray.Length; i++)
            {
                //Debug.Print(actual[i].ToString(CultureInfo.InvariantCulture));
                Assert.AreEqual(Math.Round(expected[i], 4), Math.Round(actual[i], 4));
            }
        }

        #endregion

        #endregion
    }
}
