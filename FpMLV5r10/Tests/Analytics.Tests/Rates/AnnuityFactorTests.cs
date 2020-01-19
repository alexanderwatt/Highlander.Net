

using Orion.Analytics.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.Rates
{
    /// <summary>
    /// Unit Tests for the class AnnuityFactor.
    /// </summary>
    [TestClass]
    public class AnnuityFactorTests
    {
        #region Private Fields

        private double _actual; // computed annuity factor
        private double _dfSwapStart; // discount factor from today to swap start
        private double _dfSwapEnd; // discount factor from today to swap end
        private double _forwardSwapRate; // forward swap rate 
        private double _expected; // expected result
        private double _tolerance; // accuracy for results comparison

        #endregion Private Fields

        #region SetUp

        [TestInitialize]
        public void Initialisation()
        {
            _tolerance = 1.0E-5;
        }

        #endregion SetUp

        #region Tests: ComputeAnnuityFactor method

        /// <summary>
        /// Tests the static method ComputeAnnuityFactor.
        /// </summary>
        [TestMethod]
        public void TestComputeAnnuityFactor()
        {
            // Test #1: 2M option into a 3YR swap.
            _forwardSwapRate = 6.820177/100.0;
            _dfSwapStart = 0.99437584934;
            _dfSwapEnd = 0.81178430567;
            _expected = 2.67723;
            _actual = SwapAnalytics.ComputeAnnuityFactor(_forwardSwapRate,
                                                         _dfSwapStart,
                                                         _dfSwapEnd);

            Assert.AreEqual(_expected, _actual, _tolerance);

            // Test #2: 6M option into a 4YR swap.
            _forwardSwapRate = 6.824469/100.0;
            _dfSwapStart = 0.96673145605;
            _dfSwapEnd = 0.73858756401;
            _expected = 3.33348;
            _actual = SwapAnalytics.ComputeAnnuityFactor(_forwardSwapRate,
                                                         _dfSwapStart,
                                                         _dfSwapEnd);

            // Test #3: 5YR option into a 10YR swap.
            _forwardSwapRate =  6.352136/100.0;
            _dfSwapStart = 0.73016181577;
            _dfSwapEnd = 0.39069662207;
            _expected = 5.34411;
            _actual = SwapAnalytics.ComputeAnnuityFactor(_forwardSwapRate,
                                                         _dfSwapStart,
                                                         _dfSwapEnd);

            // Test #4: 3YR option into a 7YR swap.
            _forwardSwapRate =  6.507184/100.0;
            _dfSwapStart = 0.81864063634;
            _dfSwapEnd = 0.52290276017;
            _expected = 4.54479;
            _actual = SwapAnalytics.ComputeAnnuityFactor(_forwardSwapRate,
                                                         _dfSwapStart,
                                                         _dfSwapEnd);
        }


        #endregion Tests: ComputeAnnuityFactor method

    }
}