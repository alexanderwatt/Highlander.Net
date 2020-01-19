
using Orion.Analytics.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.SABRModel
{
    /// <summary>
    /// Unit Tests for the class BlackVanillaSwaption.
    /// </summary>
    [TestClass]
    public class BlackVanillaSwaptionTests
    {
        #region Private Fields

        private double _annuityFactor; // annuity factor for the swap
        private double _optionExpiry; // option expiry expressed in years
        private double _sigma; // Black yield volatility expressed as a decimal
        private double _strike; // swaption strike expressed as a decimal
        private double _swapRate; // swap rate expressed as a decimal
        private BlackVanillaSwaption.SwaptionType _swaptionType;

        private double _actual; // actual result
        private double _expected; // expected result
        private double _tolerance; // accuracy for results comparison

        #endregion Private Fields

        #region SetUp

        [TestInitialize]
        public void Initialisation()
        {
            _annuityFactor = 1.51879;
            _optionExpiry = 3.0;
            _sigma = 9.36/100.0;
            _strike = 9.0/100.0;
            _swapRate = 6.69612/100.0;
            _tolerance = 1.0E-5;
        }

        #endregion SetUp

        #region Tests: PriceBlackVanillaSwaption method

        /// <summary>
        /// Tests the method PriceBlackVanillaSwaption.
        /// </summary>
        [TestMethod]
        public void TestPriceBlackVanillaSwaption()
        {
            // Test case: price of a Payer swaption.
            _swaptionType = BlackVanillaSwaption.SwaptionType.Payer;
            BlackVanillaSwaption paySwaptionObj = 
                new BlackVanillaSwaption(_swaptionType, _strike, _optionExpiry);
            _actual = paySwaptionObj.PriceBlackVanillaSwaption(_swapRate,
                                                               _annuityFactor,
                                                               _sigma);
            _expected = 2.56215E-04;
            Assert.AreEqual(_expected, _actual, _tolerance);

            // Test case: price of a Receiver swaption.
            _swaptionType = BlackVanillaSwaption.SwaptionType.Receiver;
            BlackVanillaSwaption recSwaptionObj =
                new BlackVanillaSwaption(_swaptionType, _strike, _optionExpiry);
            _actual = recSwaptionObj.PriceBlackVanillaSwaption(_swapRate,
                                                               _annuityFactor,
                                                               _sigma);
            _expected = 3.52473E-02;
            Assert.AreEqual(_expected, _actual, _tolerance);
        }

        #endregion Tests: PriceBlackVanillaSwaption method
    }
}