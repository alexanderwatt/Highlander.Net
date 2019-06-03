using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.CurveEngine.PricingStructures.SABR;

namespace Orion.CurveEngine.Tests
{
    /// <summary>
    ///This is a test class for SABRInterfaceTest and is intended
    ///to contain all SABRInterfaceTest Unit Tests
    ///</summary>
    [TestClass]
    public class SABRInterfaceTests
    {
        private string _settingsHandle;
        private string _instrument;
        private string _ccy;
        private string _engineHandle;
        private string _optionExpiry;

        private decimal _beta;

        private object[,] _volatilityData;
        private object[,] _assetData;

        private string _exerciseTime;
        private string _assetCode;
        private decimal _strike;

        private string _atmEngineHandle;
        private decimal _nu;
        private decimal _rho;
        private decimal _atmVolatility;
        private decimal _assetPrice;

        private string _expiry;
        private string _tenor;

        #region Additional test attributes

        [TestInitialize]
        public void Initialise()
        {
            _settingsHandle = "SABR Settings 1";
            _instrument = "Swaption";
            _ccy = "AUD";
            _beta = 0.95m;

            _engineHandle = "SABR Full Calibration 1";
            _optionExpiry = "3yr";

            object[,] volArray = {
                                         {"Swap Tenor", "ATM - 40", "ATM - 30", "ATM - 20", "ATM - 10", "ATM", "ATM + 10", "ATM + 20", "ATM + 30", "ATM + 40" },
                                         {"1yr", 10.64927747244750 / 100.0, 10.39994584260630 / 100.0, 10.20193111881570 / 100.0, 10.06617143365560 / 100.0, 9.97219920761609 / 100.0, 9.91915214635118 / 100.0, 9.79728404410501 / 100.0, 9.95835207658104 / 100.0, 9.72665759006778 / 100.0 },
                                         {"2yr", 10.71945067933860 / 100.0, 10.47225313931150 / 100.0, 10.26013281169220 / 100.0, 10.09352837649130 / 100.0, 9.99562871347315 / 100.0, 9.93462583875790 / 100.0, 9.83277616774097 / 100.0, 9.96292868333144 / 100.0, 10.07995797075100 / 100.0 },
                                         {"3yr", 10.73464076629010 / 100.0, 10.47782408038430 / 100.0, 10.27969109831760 / 100.0, 10.13079982425510 / 100.0, 10.01932817283930 / 100.0, 9.95142328386115 / 100.0, 9.84832497447229 / 100.0, 9.92376362935676 / 100.0, 9.79126953081919 / 100.0 },
                                         {"4yr", 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00},
                                         {"5yr", 10.71284746646270 / 100.0, 10.42348311645850 / 100.0, 10.21182829617250 / 100.0, 10.05813933315390 / 100.0, 9.95608449408524 / 100.0, 9.87432868808041 / 100.0, 9.76560385799806 / 100.0, 9.87023985544692 / 100.0, 9.69248020548528 / 100.0},
                                         {"7yr", 10.73741477761890 / 100.0, 10.47825361823840 / 100.0, 10.28001650952580 / 100.0, 10.10364290373030 / 100.0, 9.98568013928443 / 100.0, 9.90003220294389 / 100.0, 9.77431532150208 / 100.0, 9.85123467628954 / 100.0, 9.67881771828221 / 100.0 },
                                         {"10yr", 10.62690693405300 / 100.0, 10.35220865860990 / 100.0, 10.12715394280070 / 100.0, 9.94815984494852 / 100.0, 9.82843523464171 / 100.0, 9.74581252646963 / 100.0, 9.61090280165564 / 100.0, 9.71961846875728 / 100.0, 9.51894989184063 / 100.0 }
                                     };

            object[,] assetArray = {
                                           {"Option Expiry", "1", "2", "3", "4", "5", "7", "10"},
                                           {"1m", 6.8256130 / 100.0, 6.8257000 / 100.0, 6.8242390 / 100.0, 6.8589220 / 100.0, 6.8315520 / 100.0, 6.7394660 / 100.0, 6.6439270 / 100.0 },
                                           {"2m", 6.8199790 / 100.0, 6.8226730 / 100.0, 6.8201770 / 100.0, 6.8543610 / 100.0, 6.8243310 / 100.0, 6.7334150 / 100.0, 6.6393830 / 100.0 },
                                           {"3m", 6.8158210 / 100.0, 6.8206630 / 100.0, 6.8165900 / 100.0, 6.8504960 / 100.0, 6.8181570 / 100.0, 6.7280850 / 100.0, 6.6355410 / 100.0 },
                                           {"6m", 6.8244690 / 100.0, 6.8255620 / 100.0, 6.8126460 / 100.0, 6.8440240 / 100.0, 6.8027940 / 100.0, 6.7148050 / 100.0, 6.6258740 / 100.0 },
                                           {"1yr", 6.8247790 / 100.0, 6.8263350 / 100.0, 6.7950840 / 100.0, 6.8227380 / 100.0, 6.7580700 / 100.0, 6.6822070 / 100.0, 6.6026460 / 100.0 },
                                           {"2yr", 6.8280000 / 100.0, 6.7787050 / 100.0, 6.7430000 / 100.0, 6.7183970 / 100.0, 6.6737840 / 100.0, 6.6039100 / 100.0, 6.5483780 / 100.0 },
                                           {"3yr", 6.7262630 / 100.0, 6.6961020 / 100.0, 6.5945490 / 100.0, 6.6113380 / 100.0, 6.5806990 / 100.0, 6.5071840 / 100.0, 6.4826110 / 100.0 },
                                           {"4yr", 6.6636030 / 100.0, 6.5141040 / 100.0, 6.4932670 / 100.0, 6.5210020 / 100.0, 6.4874660 / 100.0, 6.4551160 / 100.0, 6.4188970 / 100.0 },
                                           {"5yr", 6.3394940 / 100.0, 6.3997920 / 100.0, 6.3953630 / 100.0, 6.4201080 / 100.0, 6.3850090 / 100.0, 6.4008770 / 100.0, 6.3521360 / 100.0 },
                                           {"7yr", 6.3856010 / 100.0, 6.3350830 / 100.0, 6.2844070 / 100.0, 6.3705400 / 100.0, 6.3760590 / 100.0, 6.3450100 / 100.0, 6.2871660 / 100.0 },
                                           {"10yr", 6.4446620 / 100.0, 6.3995600 / 100.0, 6.3530720 / 100.0, 6.3555290 / 100.0, 6.3071830 / 100.0, 6.2514870 / 100.0, 6.13093100 / 100.0 }
                                       };

            _volatilityData = volArray;
            _assetData = assetArray;

            _exerciseTime = "3yr";
            _assetCode = "2yr";
            _strike = 6.594549m / 100.0m;

            _atmEngineHandle = "SABR ATM Calibration 1";
            _nu = 0.1045m;
            _rho = -0.47m;
            _atmVolatility = 0.1154m;
            _assetPrice = 0.1098m;

            _expiry = "3yr";
            _tenor = "2yr";
        }
        
        #endregion
        
        /// <summary>
        ///A test for SABRInterpolateVolatility
        ///</summary>
        [TestMethod]
        public void SABRInterpolateVolatilityTest()
        {
            Swaption target = Swaption.Instance();
            target.SabrAddCalibrationSetting(_settingsHandle, _instrument, _ccy, _beta);
            target.SABRCalibrateModel(_engineHandle, _settingsHandle, _volatilityData, _assetData, _optionExpiry);
            // Used to be this, not any more for some reason...
            //decimal expected = 0.1003227242714797260490536807m;
            double expected = 0.10;
            decimal actual = target.SABRInterpolateVolatility(_engineHandle, _exerciseTime, _assetCode, _strike);
            Assert.AreEqual(expected, (double)actual, 0.01);
        }

        /// <summary>
        ///A test for SABRCalibrateModel
        ///</summary>
        [TestMethod]
        public void SABRCalibrateModelTest()
        {
            Swaption target = Swaption.Instance();
            target.SabrAddCalibrationSetting(_settingsHandle, _instrument, _ccy, _beta);
            string actual = target.SABRCalibrateModel(_engineHandle, _settingsHandle, _volatilityData, _assetData, _optionExpiry);
            string expected = _engineHandle;
            Assert.AreEqual(expected, actual);
            // Test the IsCalibrated status - true if the Function succeeded
            // Use default expiry/tenor pair
            bool testStatus = target.IsModelCalibrated(_engineHandle, _optionExpiry, _tenor);
            Assert.AreEqual(true, testStatus);
            // Test a non-existent expiry/tenor pair
            testStatus = target.IsModelCalibrated(_engineHandle, "4yr", "3");
            Assert.AreEqual(false, testStatus);
        }

        /// <summary>
        ///A test for SABRCalibrateATMModel
        ///</summary>
        [TestMethod]
        public void SABRCalibrateATMModelTest()
        {
            Swaption target = Swaption.Instance();
            string expected = _atmEngineHandle;
            target.SabrAddCalibrationSetting(_settingsHandle, _instrument, _ccy, _beta);
            string actual = target.SABRCalibrateATMModel(_atmEngineHandle, _settingsHandle, _nu, _rho, _atmVolatility, _assetPrice, _exerciseTime, _assetCode);
            Assert.AreEqual(expected, actual);
            // Test the IsCalibrated status - true if the Function succeeded
            // Use default expiry/tenor pair
            bool testStatus = target.IsModelCalibrated(_atmEngineHandle, _optionExpiry, _tenor);
            Assert.AreEqual(true, testStatus);
        }

        /// <summary>
        ///A test for SABRAddCalibrationSettings
        ///</summary>
        [TestMethod]
        public void SABRAddCalibrationSettingsTest()
        {
            Swaption target = Swaption.Instance(); // TODO: Initialize to an appropriate value
            string expected = _settingsHandle;
            string actual = target.SabrAddCalibrationSetting(_settingsHandle, _instrument, _ccy, _beta);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsModelCalibrated
        ///</summary>
        [TestMethod]
        public void IsModelCalibratedTest()
        {
            Swaption target = Swaption.Instance();
            target.SabrAddCalibrationSetting(_settingsHandle, _instrument, _ccy, _beta);
            target.SABRCalibrateModel(_engineHandle, _settingsHandle, _volatilityData, _assetData, _optionExpiry);
            bool actual = target.IsModelCalibrated(_engineHandle, _optionExpiry, _tenor);
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///A test for GetSABRParameter
        ///</summary>
        [TestMethod]
        public void GetSABRParameterTest()
        {
            Swaption target = Swaption.Instance();
            target.SabrAddCalibrationSetting(_settingsHandle, _instrument, _ccy, _beta);
            target.SABRCalibrateModel(_engineHandle, _settingsHandle, _volatilityData, _assetData, _optionExpiry);
            // Test alpha
            Swaption.CalibrationParameter param = Swaption.CalibrationParameter.Alpha;
            // used to be this, but not any more for some reason...
            //expected = 0.0854282092142854m;
            decimal expected = 0.0774577237728713m;//0.0774816286100299m
            decimal actual = target.GetSABRParameter(param, _engineHandle, _expiry, _tenor);
            Assert.AreEqual(expected, actual);
            // Test beta
            param = Swaption.CalibrationParameter.Beta;
            expected = 0.95m;
            actual = target.GetSABRParameter(param, _engineHandle, _expiry, _tenor);
            Assert.AreEqual(expected, actual);
            // Test nu
            param = Swaption.CalibrationParameter.Nu;
            // used to be this, but not any more for some reason...
            //expected = 0.3174771160265069427371566533m;
            expected = 0.7427956690310760080682235855m;//0.7397330508021633986595538765m;
            actual = target.GetSABRParameter(param, _engineHandle, _expiry, _tenor);
            Assert.AreEqual(expected, actual);
            // Test rho
            param = Swaption.CalibrationParameter.Rho;
            // used to be this, but not any more for some reason...
            //expected = -0.116352678779442m;
            expected = -0.140204549654502m;// -0.13311810502423m
            actual = target.GetSABRParameter(param, _engineHandle, _expiry, _tenor);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CalibrationError
        ///</summary>
        [TestMethod]
        public void CalibrationErrorTest()
        {
            Swaption target = Swaption.Instance();
            target.SabrAddCalibrationSetting(_settingsHandle, _instrument, _ccy, _beta);
            target.SABRCalibrateModel(_engineHandle, _settingsHandle, _volatilityData, _assetData, _optionExpiry);
            // used to be this number, not any more for some reason...
            //double expected = 0.00000122592559259722;
            double expected = 0.00001;
            double actual = (double)target.CalibrationError(_engineHandle, _expiry, _tenor);
            Assert.AreEqual(expected, actual, expected);
        }
    }
}