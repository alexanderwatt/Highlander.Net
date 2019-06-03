
using Orion.Analytics.Stochastics.Volatilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.SABRModel
{
    /// <summary>
    /// Unit Tests for the class ImpliedVolatilityParameters.
    /// </summary>
    [TestClass]
    public class ImpliedVolatilityParametersUnitTests
    {
        #region Private Fields

        private double _assetPrice; // price of the relevant asset
        private double _exerciseTime; // time to option exercise
        private double _strike; // option strike price
        private string _errorMessage; // container for possible error message
        private bool _isValidImpliedVolatilityParameter;

        #endregion Private Fields

        #region SetUp

        [TestInitialize]
        public void Initialisation()
        {
            _assetPrice = 3.98d/100.0d;
            _exerciseTime = 5.0d;
            _strike = _assetPrice;
            _isValidImpliedVolatilityParameter = true;
            _errorMessage = "";
        }

        #endregion SetUp

        #region Tests: Invalid Asset Price Detected

        /// <summary>
        /// Tests the static method CheckAssetPrice.
        /// </summary>
        [TestMethod]
        public void TestCheckAssetPrice()
        {
            _assetPrice = -3.98d/100.0d;
            _isValidImpliedVolatilityParameter =
                ImpliedVolatilityParameters.CheckAssetPrice(_assetPrice,
                                                            ref _errorMessage);
            const string expected = "Asset price must be positive.";

            Assert.IsFalse(_isValidImpliedVolatilityParameter);
            Assert.AreEqual(expected, _errorMessage);
        }

        #endregion Tests: Invalid Asset Price Detected

        #region Tests: Invalid Exercise Time Detected

        /// <summary>
        /// Tests the static method CheckExerciseTime.
        /// </summary>
        public void TestCheckExerciseTime()
        {
            _exerciseTime = -5.0d;
            _isValidImpliedVolatilityParameter =
                ImpliedVolatilityParameters.CheckExerciseTime(_exerciseTime,
                                                              ref _errorMessage);
            const string expected = "Exercise time cannot be negative.";

            Assert.IsFalse(_isValidImpliedVolatilityParameter);
            Assert.AreEqual(expected, _errorMessage);
        }

        #endregion Tests: Invalid Exercise Time Detected

        #region Tests: Invalid Strike Detected

        /// <summary>
        /// Tests the static method CheckStrike.
        /// </summary>
        [TestMethod]
        public void TestCheckStrike()
        {
            _strike = 0.0d;
            _isValidImpliedVolatilityParameter =
                ImpliedVolatilityParameters.CheckStrike(_strike,
                                                        ref _errorMessage);
            const string expected = "Strike must be positive.";

            Assert.IsFalse(_isValidImpliedVolatilityParameter);
            Assert.AreEqual(expected, _errorMessage);
        }

        #endregion Tests: Invalid Strike Detected
    }
}