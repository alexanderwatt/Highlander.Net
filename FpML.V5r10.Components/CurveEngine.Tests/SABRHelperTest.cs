using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.CurveEngine.Helpers;

namespace Orion.CurveEngine.Tests
{
    [TestClass]
    public class SABRHelperTests
    {
        private static string Handle(string expiry, string tenor)
        {
            return expiry + tenor + " ATM VOLGRID Calibration";
        }

        [TestMethod]
        public void SabrCalibrationSettingsTest()
        {
            const string expiry = "3m";
            const string tenor = "0.25y";
            const string swaption = "VOLGRID " + expiry + tenor;
            string actual = SabrCalibrationSettings(swaption);
            Assert.AreEqual(swaption, actual);
        }

        private static string SabrCalibrationSettings(string swaption)
        {
            const string instrumentType = "Swaption";
            const string currency = "AUD";
            const double beta = 1;
            return SABRHelper.AddSabrCalibrationSettings(swaption, instrumentType, currency, beta);
        }

        [TestMethod]
        public void SabrAtmCalibrationWithTenorTest()
        {
            const string expiry = "3m";
            const string tenor = "0.25y";
            string actual = SabrAtmCalibrationWithTenor();
            string expected = Handle(expiry, tenor);
            Assert.AreEqual(expected, actual);
        }

        private static string SabrAtmCalibrationWithTenor()
        {
            const string expiry = "3m";
            const string tenor = "0.25y";
            const string swaption = "VOLGRID " + expiry + tenor;
            const double atmVolatility = 20;
            const double assetPrice = 3.44;
            const double nu = 0.7561;
            const double rho = -0.3702;
            string handle = Handle(expiry, tenor);
            SabrCalibrationSettings(swaption);
            return SABRHelper.CalibrateSabrAtmModelWithTenor(handle, swaption, nu, rho, atmVolatility,
                                                                         assetPrice, expiry, tenor);
        }

        [TestMethod]
        public void SabrInterpolateVolatilityTest()
        {
            const string expiry = "3m";
            const string tenor = "0.25y";
            string handle = SabrAtmCalibrationWithTenor();
            const double strike = 2;
            double actual = (double)SABRHelper.SabrImpliedVolatility(handle, expiry, 
                tenor, strike);
            const double expected = 0.31687;
            Assert.AreEqual(expected, actual, 0.00001);
        }

        [TestMethod]
        public void SabrInterpolateVolatilitiesTest()
        {
            const string expiry = "3m";
            const string tenor = "0.25y";
            string handle = SabrAtmCalibrationWithTenor();
            double[] strikes = { 0.02, 0.03 };
            decimal[] actuals = SABRHelper.SabrInterpolateVolatilities(handle, expiry, tenor, strikes);
            const double expected = 0.31687;
            Assert.AreEqual(expected, (double)actuals[0], 0.00001);
        }

        [TestMethod]
        public void SabrInterpolateVolatilityTest2()
        {
            const string expiry = "6m";
            const double strike = 0.03;
            const double assetCode = 5.0587;
            const double nu = 0.9211;
            const double rho = -0.2823;
            const double atmVol = 23.18;
            const string tenor = "1y";
            const string handle = "6m1y ATM VOLGRID Calibration";
            string swaption = SABRHelper.AddSabrCalibrationSettings("VOLGRID 6m1y", "Swaption", "AUD", 1);
            string handle2 =
                SABRHelper.CalibrateSabrAtmModelWithTenor(handle, swaption, nu, rho, atmVol,
                                                                         assetCode, expiry, tenor);
            decimal actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, strike);
            const double expected = 1.3829;
            Assert.AreEqual(expected, (double)actual, 0.0001);
        }

        [TestMethod]
        public void SabrInterpolateVolatilityStandardInputsTest()
        {
            const string calibrationHandle = "SABR Full Settings 20y1y";
            const string calibrationInstrument = "Swaption";
            const string calibrationCurrency = "AUD";
            const double beta = 1;
            string result = SABRHelper.AddSabrCalibrationSettings(calibrationHandle, calibrationInstrument,
                                                                                calibrationCurrency, beta);
            Assert.AreEqual(calibrationHandle, result);
            object[,] vols
                =
                {
                    {"Swap Tenor", "ATM - 193.810720549067", "ATM - 93.8107205490671", "ATM - 43.8107205490671", "ATM - 18.8107205490671", "ATM", "ATM + 6.18927945093288", "ATM + 31.1892794509329", "ATM + 56.1892794509329", "ATM + 106.189279450933", "ATM + 206.189279450933"},
                    {"1y", 18.55, 16.32, 15.60, 15.35, 15.20, 15.16, 15.01, 14.91, 14.80, 14.83}
                };
            object[,] assets
                = {
                      {"Option Expiry", 1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d, 12d, 15d, 20d, 30d},
                      {"0m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"1m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"2m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"3m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"6m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"1y", 7.01, 7.01, 7.08, 7.11, 7.11, 7.09, 7.09, 7.08, 7.08, 7.06, 7.06, 6.97, 6.97, 6.97},
                      {"2y", 7.00, 7.10, 7.09, 7.12, 7.10, 7.10, 7.09, 7.08, 7.07, 7.05, 7.05, 6.92, 6.92, 6.92},
                      {"3y", 7.21, 7.13, 7.10, 7.12, 7.11, 7.10, 7.09, 7.07, 7.05, 7.02, 7.02, 6.87, 6.87, 6.87},
                      {"4y", 7.02, 7.03, 7.01, 7.06, 7.06, 7.05, 7.03, 7.01, 6.98, 6.96, 6.96, 6.79, 6.79, 6.79},
                      {"5y", 7.01, 7.00, 7.01, 7.05, 7.04, 7.02, 7.00, 6.97, 6.94, 6.92, 6.92, 6.71, 6.71, 6.71},
                      {"6y", 7.01, 7.00, 7.01, 7.05, 7.04, 7.02, 7.00, 6.97, 6.94, 6.92, 6.92, 6.71, 6.71, 6.71},
                      {"7y", 7.02, 7.01, 7.00, 7.01, 6.97, 6.94, 6.91, 6.87, 6.82, 6.78, 6.92, 6.50, 6.71, 6.71},
                      {"8y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.92, 6.71, 6.71, 6.71},
                      {"9y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.92, 6.71, 6.71, 6.71},
                      {"10y", 6.83, 6.79, 6.76, 6.77, 6.74, 6.68, 6.62, 6.56, 6.50, 6.45, 6.92, 6.08, 6.71, 6.71},
                      {"12y", 6.69, 6.65, 6.61, 6.58, 6.51, 6.45, 6.38, 6.31, 6.22, 6.14, 6.92, 5.78, 6.71, 6.71},
                      {"15y", 6.26, 6.23, 6.16, 6.12, 6.04, 5.94, 5.84, 5.75, 5.67, 5.59, 6.92, 5.25, 6.71, 6.71},
                      {"20y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71},
                      {"30y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71},
                  };
            const string expiry = "20y";
            const string tenor = "1y";
            const string engineHandle = "Full Calibration 20y1y";
            const double strike = 7;
            string handle2 = SABRHelper.CalibrateSabrModel(engineHandle, calibrationHandle, vols, assets, expiry);
            decimal actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, strike);
            const double expected = 0.149766076068191;//0.14976614         
            Assert.AreEqual(expected, (double)actual, 0.00000001);
        }

        [TestMethod]
        public void SabrInterpolateVolatilityLegacyInputsTest()
        {
            const string calibrationHandle = "SABR Full Settings 20y1y";
            const string calibrationInstrument = "Swaption";
            const string calibrationCurrency = "AUD";
            const double beta = 1;
            string result = SABRHelper.AddSabrCalibrationSettings(calibrationHandle, calibrationInstrument,
                                                                                calibrationCurrency, beta);
            Assert.AreEqual(calibrationHandle, result);
            object[,] vols
                =
                {
                    {"Swap Tenor", "ATM - 193.810720549067", "ATM - 93.8107205490671", "ATM - 43.8107205490671", "ATM - 18.8107205490671", "ATM", "ATM + 6.18927945093288", "ATM + 31.1892794509329", "ATM + 56.1892794509329", "ATM + 106.189279450933", "ATM + 206.189279450933"},
                    {"1y", 18.55, 16.32, 15.60, 15.35, 15.20, 15.16, 15.01, 14.91, 14.80, 14.83}
                };
            object[,] assets
                = {
                      {null, null, null, null, null, null, null, null, null, null, null, null, null, null, null},
                      {"Option Expiry", 1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d, 12d, 15d, 20d, 30d},
                      {"0m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"1m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"2m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"3m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"6m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10, 7.10, 7.00, 7.00, 7.00},
                      {"1y", 7.01, 7.01, 7.08, 7.11, 7.11, 7.09, 7.09, 7.08, 7.08, 7.06, 7.06, 6.97, 6.97, 6.97},
                      {"2y", 7.00, 7.10, 7.09, 7.12, 7.10, 7.10, 7.09, 7.08, 7.07, 7.05, 7.05, 6.92, 6.92, 6.92},
                      {"3y", 7.21, 7.13, 7.10, 7.12, 7.11, 7.10, 7.09, 7.07, 7.05, 7.02, 7.02, 6.87, 6.87, 6.87},
                      {"4y", 7.02, 7.03, 7.01, 7.06, 7.06, 7.05, 7.03, 7.01, 6.98, 6.96, 6.96, 6.79, 6.79, 6.79},
                      {"5y", 7.01, 7.00, 7.01, 7.05, 7.04, 7.02, 7.00, 6.97, 6.94, 6.92, 6.92, 6.71, 6.71, 6.71},
                      {"6y", 7.01, 7.00, 7.01, 7.05, 7.04, 7.02, 7.00, 6.97, 6.94, 6.92, 6.92, 6.71, 6.71, 6.71},
                      {"7y", 7.02, 7.01, 7.00, 7.01, 6.97, 6.94, 6.91, 6.87, 6.82, 6.78, 6.92, 6.50, 6.71, 6.71},
                      {"8y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.92, 6.71, 6.71, 6.71},
                      {"9y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.92, 6.71, 6.71, 6.71},
                      {"10y", 6.83, 6.79, 6.76, 6.77, 6.74, 6.68, 6.62, 6.56, 6.50, 6.45, 6.92, 6.08, 6.71, 6.71},
                      {"12y", 6.69, 6.65, 6.61, 6.58, 6.51, 6.45, 6.38, 6.31, 6.22, 6.14, 6.92, 5.78, 6.71, 6.71},
                      {"15y", 6.26, 6.23, 6.16, 6.12, 6.04, 5.94, 5.84, 5.75, 5.67, 5.59, 6.92, 5.25, 6.71, 6.71},
                      {"20y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71},
                      {"30y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71},
                  };
            const string expiry = "20y";
            const string tenor = "1y";
            const string engineHandle = "Full Calibration 20y1y";
            const double strike = 7;
            string handle2 = SABRHelper.CalibrateSabrModel(engineHandle, calibrationHandle, vols, assets, expiry);
            decimal actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, strike);
            const double expected = 0.149766076068191;// 0.14976614
            Assert.AreEqual(expected, (double)actual, 0.00000001);
        }

        [TestMethod]
        public void SabrInterpolateVolatilityStandardInputsTest2()
        {
            const string calibrationHandle = "SABR Full Settings 6m1y";
            const string calibrationInstrument = "Swaption";
            const string calibrationCurrency = "AUD";
            const double beta = 1;
            object[,] vols
                =
                {
                    {"Swap Tenor","ATM - 199.054193939524","ATM - 99.0541939395239","ATM - 49.0541939395239","ATM - 24.0541939395239","ATM","ATM + 0.945806060476073","ATM + 25.9458060604761","ATM + 50.9458060604761","ATM + 100.945806060476","ATM + 200.945806060476"},
                    {"1y", 39.10, 33.00, 31.10, 30.43, 29.93, 29.91, 29.50, 29.21, 28.96, 29.28}
                };
            object[,] assets
                = {
                      {"Option Expiry", 1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d},
                      {"0m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10},
                      {"1m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10},
                      {"2m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10},
                      {"3m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10},
                      {"6m", 7.23, 7.10, 7.12, 7.16, 7.15, 7.13, 7.12, 7.11, 7.11, 7.10},
                      {"1y", 7.01, 7.01, 7.08, 7.11, 7.11, 7.09, 7.09, 7.08, 7.08, 7.06},
                      {"2y", 7.00, 7.10, 7.09, 7.12, 7.10, 7.10, 7.09, 7.08, 7.07, 7.05},
                      {"3y", 7.21, 7.13, 7.10, 7.12, 7.11, 7.10, 7.09, 7.07, 7.05, 7.02},
                      {"4y", 7.02, 7.03, 7.01, 7.06, 7.06, 7.05, 7.03, 7.01, 6.98, 6.96},
                      {"5y", 7.01, 7.00, 7.01, 7.05, 7.04, 7.02, 7.00, 6.97, 6.94, 6.92},
                      {"6y", 7.01, 7.00, 7.01, 7.05, 7.04, 7.02, 7.00, 6.97, 6.94, 6.92},
                      {"7y", 7.02, 7.01, 7.00, 7.01, 6.97, 6.94, 6.91, 6.87, 6.82, 6.78},
                      {"8y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71},
                      {"9y", 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71, 6.71},
                      {"10y", 6.83, 6.79, 6.76, 6.77, 6.74, 6.68, 6.62, 6.56, 6.50, 6.45},
                  };
            const string expiry = "6m";
            const string tenor = "1y";
            const string engineHandle = "Full Calibration 6m1y";
            const double strike = 2.5;
            string result = SABRHelper.AddSabrCalibrationSettings(calibrationHandle, calibrationInstrument, calibrationCurrency, beta);
            Assert.AreEqual(calibrationHandle, result);
            string handle2 = SABRHelper.CalibrateSabrModel(engineHandle, calibrationHandle, vols, assets, expiry);
            Assert.AreEqual(engineHandle, handle2);
            decimal actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, strike);
            Assert.AreEqual(0.6210, (double)actual, 0.0001);
        }

        [TestMethod]
        public void SabrInterpolateVolatilityMultipleTenorsTest()
        {
            const string calibrationHandle = "SABR Full 6m Settings";
            const string calibrationInstrument = "Swaption";
            const string calibrationCurrency = "AUD";
            const double beta = 1;
            string result = SABRHelper.AddSabrCalibrationSettings(calibrationHandle, calibrationInstrument,
                                                                                calibrationCurrency, beta);
            Assert.AreEqual(calibrationHandle, result);
            object[,] assets
                =
                {
                    {"Option Expiry", "Swap Tenor", 1d, 2d, 3d, 4d, 5d, 7d, 10d},
                    {"1m", null, 6.8000, 6.8000, 6.8000, 6.8000, 6.8000, 6.7000, 6.6000},
                    {"2m", null, 6.8000, 6.8000, 6.8000, 6.8000, 6.8000, 6.7000, 6.6000},
                    {"3m", "Years to E", 6.8000, 6.8000, 6.8000, 6.8000, 6.8000, 6.7000, 6.6000},
                    {"6m", 0.5, 6.8000, 6.8000, 6.8000, 6.8000, 6.8000, 6.7000, 6.6000},
                    {"1yr", 1, 6.8000, 6.8000, 6.9000, 6.9000, 6.8000, 6.7000, 6.6000},
                    {"2yr", 2, 6.8500, 6.8000, 6.8000, 6.7750, 6.7000, 6.6000, 6.5500},
                    {"3yr", 3, 6.9000, 6.8000, 6.7000, 6.6500, 6.6000, 6.5000, 6.5000},
                    {"4yr", 4, 6.7000, 6.6000, 6.5500, 6.5250, 6.5000, 6.4500, 6.4000},
                    {"5yr", 5, 6.4000, 6.4000, 6.4000, 6.4000, 6.4000, 6.4000, 6.3000},
                    {"7yr", 7, 6.4000, 6.4000, 6.3000, 6.4000, 6.3500, 6.4000, 6.2000},
                    {"10yr", 10, 6.4000, 6.4000, 6.3000, 6.4000, 6.3000, 6.4000, 6.1000},
                };
            object[,] vols
                = {
                      {"Swap Tenor","ATM - 100","ATM - 75","ATM - 50","ATM - 25","ATM","ATM + 25","ATM + 50","ATM + 75","ATM + 100"},
                      {"1yr",10.50,10.30,10.04,9.87,9.77,9.65,9.61,9.61,9.69},
                      {"2yr",10.68,10.40,10.16,10.03,9.94,9.83,9.79,9.78,9.83},
                      {"3yr",10.86,10.60,10.40,10.25,10.14,10.05,10.01,10.01,10.06},
                      {"4yr",0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00,0.00},
                      {"5yr",10.82,10.60,10.39,10.26,10.16,10.02,9.99,10.00,10.07},
                      {"7yr",10.94,10.81,10.56,10.41,10.31,10.15,10.12,10.13,10.20},
                      {"10yr",11.15,10.92,10.74,10.62,10.52,10.31,10.27,10.29,10.38},
                  };
            const string expiry = "6M";
            const string tenor = "1y";
            const string engineHandle = "Full Calibration 20y1y";
            string handle2 = SABRHelper.CalibrateSabrModel(engineHandle, calibrationHandle, vols, assets, expiry);
            decimal actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 5.8);
            Assert.AreEqual(.1052, (double)actual, 0.0001);
            actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 6.05);
            Assert.AreEqual(.1027, (double)actual, 0.0001);
            actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 6.3);
            Assert.AreEqual(.1006, (double)actual, 0.0001);
            actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 6.55);
            Assert.AreEqual(.0989, (double)actual, 0.0001);
            actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 6.8);
            Assert.AreEqual(.0977, (double)actual, 0.0001);
            actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 7.05);
            Assert.AreEqual(.0969, (double)actual, 0.0001);
            actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 7.30);
            Assert.AreEqual(.0964, (double)actual, 0.0001);
            actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 7.55);
            Assert.AreEqual(.0963, (double)actual, 0.0001);
            actual = SABRHelper.SabrImpliedVolatility(handle2, expiry, tenor, 7.80);
            Assert.AreEqual(.0966, (double)actual, 0.0001);
        }
    }
}
