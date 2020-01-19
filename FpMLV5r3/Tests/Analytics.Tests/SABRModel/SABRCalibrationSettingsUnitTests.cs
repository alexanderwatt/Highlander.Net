/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using Highlander.Reporting.Analytics.V5r3.Stochastics.SABR;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.SABRModel
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