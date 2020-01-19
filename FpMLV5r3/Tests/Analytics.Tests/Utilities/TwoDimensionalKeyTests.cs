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

using System.Collections;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Utilities
{
    /// <summary>
    /// Unit Tests for the class TwoDimensionalKey.
    /// </summary>
    [TestClass]
    public class TwoDimensionalKeyTests
    {
        #region Private Fields

        private InstrumentType.Instrument _first; // first dimension of the key
        private string _second; // second dimension of the key
        private TwoDimensionalKey _key; 

        #endregion Private Fields

        #region SetUp

        [TestInitialize]
        public void Initialization()
        {
            _first = InstrumentType.Instrument.Swaption;
            _second = "USD";
        }

        #endregion SetUp

        #region Tests: Constructor

        /// <summary>
        /// Tests the class constructor.
        /// </summary>
        [TestMethod]
        public void TestConstructor()
        {
            _key = new
                TwoDimensionalKey(_first, _second);
            Assert.AreEqual(_first, _key.FirstKeyPart);
            Assert.AreEqual(_second, _key.SecondKeyPart);
        }

        #endregion Tests: Constructor

        #region Tests: Equals

        /// <summary>
        /// Tests the method Equals in TwoDimensionalKeyEqualityComparer class.
        /// </summary>
        [TestMethod]
        public void TestEquals()
        {
            // Test case: equality of two keys.
            TwoDimensionalKey key1 = new TwoDimensionalKey(
                InstrumentType.Instrument.Swaption, "AUD");
            TwoDimensionalKey key2 = new TwoDimensionalKey(
                InstrumentType.Instrument.Swaption, "AUD");

            TwoDimensionalKeyEqualityComparer comparerObj =
                new TwoDimensionalKeyEqualityComparer();

            Assert.IsTrue(((IEqualityComparer)comparerObj).Equals(key1, key2));

            // Test case: two keys are different.
            TwoDimensionalKey key3 = new TwoDimensionalKey(
                InstrumentType.Instrument.Swaption, "USD");

            Assert.IsFalse(((IEqualityComparer)comparerObj).Equals(key1, key3));
        }

        #endregion Tests: Equal
    }
}