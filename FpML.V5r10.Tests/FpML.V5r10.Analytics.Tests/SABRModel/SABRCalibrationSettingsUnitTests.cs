using Orion.Analytics.Stochastics.SABR;
using Orion.Analytics.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.SABRModel
{
    /// <summary>
    /// Unit Tests for the class SABRCalibrationSettings. 
    /// </summary>
    [TestClass]
    public class SABRCalibrationSettingsUnitTests
    {
        #region Private Fields 

        private string _handle; // name of the SABR calibration settings object
        private decimal _beta; // SABR parameter beta
        private InstrumentType.Instrument _instrument; // instrument type
        private string _currency; // currency type

        #endregion Private Fields

        #region SetUp

        /// <summary>
        /// Set up method.
        /// </summary>
        [TestInitialize]
        public void Initialisation()
        {
            _handle = "SABR Calibration Settings Unit Test";
            _instrument = InstrumentType.Instrument.Swaption;
            _currency = "AUD";
            _beta = 0.85m;
        }

        #endregion SetUp 

        #region Tests: Constructor

        /// <summary>
        /// Tests the class constructor.
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            var settingsObj = new
                SABRCalibrationSettings(_handle, _instrument,_currency, _beta);

            Assert.IsNotNull(settingsObj);
            Assert.AreEqual(_beta, settingsObj.Beta);
            Assert.AreEqual(_handle, settingsObj.Handle);
            Assert.AreEqual(_instrument, settingsObj.Instrument);
            Assert.AreEqual(_currency, settingsObj.Currency);
        }

        #endregion Tests: Constructor

    }
}