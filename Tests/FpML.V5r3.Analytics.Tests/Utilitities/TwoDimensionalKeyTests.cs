
using System.Collections;
using Orion.Analytics.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.Utilitities
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