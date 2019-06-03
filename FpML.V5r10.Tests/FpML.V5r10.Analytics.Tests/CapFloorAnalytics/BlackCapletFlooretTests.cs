
#region Using Directives

using Orion.Analytics.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

#endregion

namespace Orion.Analytics.Tests.CapFloorAnalytics
{
    /// <summary>
    /// Unit tests for the class BlackCapletFlooret.
    /// </summary>
    [TestClass]
    public class BlackCapletFlooretTests
    {
        #region Private Fields 

        double _discountFactor; // discount factor
        double _forwardRate; // forward rate
        double _notional; // Caplet/Flooret face value
        double _optionExpiry; // option expiry expressed in years
        BlackCapletFlooret.OptionType _optionType; // option type identifier
        double _sigma; // volatility
        double _strike; // option strike expressed as a decimal
        double _tau; // year fraction for the Caplet/Flooret
        bool _validateArguments;

        BlackCapletFlooret _capletFlooretObj;

        double _actual;
        double _expected;
        double _tolerance = 1.0E-2d;

        #endregion

        #region SetUp

        [TestInitialize]
        public void Initialisation()
        {
            _discountFactor = 0.94617d;
            _forwardRate = 0.0743100d;
            _notional = 1.0E+08d;
            _optionExpiry = 0.50959d;
            _optionType = BlackCapletFlooret.OptionType.Caplet;
            _sigma = 9.43000d/100d;
            _strike = 0.075d;
            _tau = 0.25205d;
            _validateArguments = true;
            _capletFlooretObj = new BlackCapletFlooret
                (_notional,
                 _optionExpiry,
                 _optionType,
                 _strike,
                 _tau,
                 _validateArguments);
        }

        #endregion

        #region Test: Constructor

        /// <summary>
        /// Tests that the constructor detects invalid arguments.
        /// </summary>
        [TestMethod]
        public void TestBlackCapletFlooretDataValidation()
        {
            #region Test that an Incorrect Notional is Detected

            try
            {
                _notional = 0d;
                _capletFlooretObj =
                    new BlackCapletFlooret
                        (_notional,
                         _optionExpiry,
                         _optionType,
                         _strike,
                         _tau,
                         _validateArguments);
            }
            catch (ArgumentException e)
            {
                const string ErrorMessage =
                    "Notional for a Caplet/Flooret must be positive";

                Assert.AreEqual(ErrorMessage, e.Message);            	
            }            

            #endregion

            #region Test that Zero Time to Expiry is Acceptable

            Initialisation(); 
            _optionExpiry = 0d;
            _capletFlooretObj =
                new BlackCapletFlooret
                    (_notional,
                     _optionExpiry,
                     _optionType,
                     _strike,
                     _tau,
                     _validateArguments);

            Assert.IsNotNull(_capletFlooretObj);

            #endregion 
          
            #region Test that an Incorrect Expiry is Detected

            try
            {
                Initialisation();
                _optionExpiry = -0.5d;
                _capletFlooretObj =
                    new BlackCapletFlooret
                        (_notional,
                         _optionExpiry,
                         _optionType,
                         _strike,
                         _tau,
                         _validateArguments);
            }
            catch (ArgumentException e)
            {
                const string ErrorMessage =
                    "Expiry for a Caplet/Flooret cannot be negative";

                Assert.AreEqual(ErrorMessage, e.Message);
            }

            #endregion 

            #region Test that an Incorrect Strike is Detected

            try
            {
                Initialisation();
                _strike = 0d;
                _capletFlooretObj =
                    new BlackCapletFlooret
                        (_notional,
                         _optionExpiry,
                         _optionType,
                         _strike,
                         _tau,
                         _validateArguments);
            }
            catch (ArgumentException e)
            {
                const string ErrorMessage =
                    "Strike for a Caplet/Flooret must be positive";

                Assert.AreEqual(ErrorMessage, e.Message);
            }

            #endregion

            #region Test that an Incorrect Year Fraction is Detected

            try
            {
                Initialisation();
                _tau = 0d;
                _capletFlooretObj =
                    new BlackCapletFlooret
                        (_notional,
                         _optionExpiry,
                         _optionType,
                         _strike,
                         _tau,
                         _validateArguments);
            }
            catch (ArgumentException e)
            {
                const string ErrorMessage =
                    "Year fraction for a Caplet/Flooret must be positive";

                Assert.AreEqual(ErrorMessage, e.Message);
            }

            #endregion
        }

        /// <summary>
        /// Test the BlackCapletFlooret class constructor.
        /// </summary>
        [TestMethod]
        public void TestBlackCapletFlooret()
        {
            Assert.IsNotNull(_capletFlooretObj);
        }

        #endregion

        #region Test: ComputePrice Method

        /// <summary>
        /// Tests that the method ComputePrice detects invalid arguments.
        /// </summary>
        [TestMethod]
        public void TestComputePriceDataValidation()
        {
            #region Test that an Incorrect Discount Factor is Detected

            try
            {
                Initialisation();
                _discountFactor = 0d;
                _capletFlooretObj.ComputePrice(_discountFactor,
                                               _forwardRate,
                                               _sigma,
                                               _validateArguments);
            }
            catch (ArgumentException e)
            {
                const string ErrorMessage =
                    "Discount factor used to price a Caplet/Flooret must be positive";

                Assert.AreEqual(ErrorMessage, e.Message);
            }

            #endregion

            #region Test that an Incorrect Forward Rate is Detected

            try
            {
                Initialisation();
                _forwardRate = 0d;
                _capletFlooretObj.ComputePrice(_discountFactor,
                                               _forwardRate,
                                               _sigma,
                                               _validateArguments);
            }
            catch (ArgumentException e)
            {
                const string ErrorMessage =
                    "Forward rate used to price a Caplet/Flooret must be positive";

                Assert.AreEqual(ErrorMessage, e.Message);
            }

            #endregion

            #region Test that an Incorrect Volatility is Detected

            try
            {
                Initialisation();
                _sigma = 0d;
                _capletFlooretObj.ComputePrice(_discountFactor,
                                               _forwardRate,
                                               _sigma,
                                               _validateArguments);
            }
            catch (ArgumentException e)
            {
                const string ErrorMessage =
                    "Volatility used to price a Caplet/Flooret must be positive";

                Assert.AreEqual(ErrorMessage, e.Message);
            }

            #endregion

        }

        /// <summary>
        /// Tests the method ComputePrice.
        /// </summary>
        [TestMethod]
        public void TestComputePrice()
        {
            #region Test #1: Computation of Price Near Expiry

            // Instantiate a Caplet that has an expiry less than one day.
            _optionType = BlackCapletFlooret.OptionType.Caplet;
            _notional = 1.0E+08d;
            _optionExpiry = 1.36986E-03d;
            _strike = 7.50d/100d;
            _tau = 0.25205d;
            _capletFlooretObj = new BlackCapletFlooret
                (_notional,
                 _optionExpiry,
                 _optionType,
                 _strike,
                 _tau,
                 _validateArguments);

            Assert.IsNotNull(_capletFlooretObj);

            // Price the Caplet.
            _discountFactor = 0.99617d;
            _forwardRate = 8.53100d/100d;
            _sigma = 9.43000d/100d;
            _actual = _capletFlooretObj.ComputePrice
                (_discountFactor,
                 _forwardRate,
                 _sigma,
                 _validateArguments);
            _expected = 258868.27d;
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            #endregion 

            #region Test #2: Computation of a Caplet Price

            _optionType = BlackCapletFlooret.OptionType.Caplet;
            _notional = 1.0E+08d;
            _optionExpiry = 0.50959d;
            _strike = 7.50d/100d;
            _tau = 0.25205d;
            _capletFlooretObj = new BlackCapletFlooret
                (_notional,
                 _optionExpiry,
                 _optionType,
                 _strike,
                 _tau,
                 _validateArguments);

            Assert.IsNotNull(_capletFlooretObj);

            // Price the Caplet.
            _discountFactor = 0.94617d;
            _forwardRate = 7.43100d/100d;
            _sigma = 9.43000d/100d;
            _actual = _capletFlooretObj.ComputePrice
                (_discountFactor,
                 _forwardRate,
                 _sigma,
                 _validateArguments);
            _expected = 40027.17d;
            _tolerance = 2.0d;
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            #endregion

            #region Test #3: Computation of a Flooret Price

            _optionType = BlackCapletFlooret.OptionType.Flooret;
            _notional = 1.0E+08d;
            _optionExpiry = 0.50959d;
            _strike = 7.50d/100d;
            _tau = 0.25205d;
            _capletFlooretObj = new BlackCapletFlooret
                (_notional,
                 _optionExpiry,
                 _optionType,
                 _strike,
                 _tau,
                 _validateArguments);

            Assert.IsNotNull(_capletFlooretObj);

            // Price the Flooret.
            _discountFactor = 0.94617d;
            _forwardRate = 7.43100d/100d;
            _sigma = 9.43000d/100d;
            _actual = _capletFlooretObj.ComputePrice
                (_discountFactor,
                 _forwardRate,
                 _sigma,
                 _validateArguments);
            _expected = 56482.43d;
            _tolerance = 2.0d;
            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            #endregion
        }

        #endregion

        #region Test: ComputeVega Method

        /// <summary>
        /// Tests the method ComputeVega.
        /// </summary>
        [TestMethod]
        public void TestComputeVega()
        {
            _actual = _capletFlooretObj.ComputeVega
                (_discountFactor, _forwardRate, _sigma, true);
            _expected = 5019.85d;
            _tolerance = 1.0E-2d;

            Assert.AreEqual(_expected,
                            _actual,
                            _tolerance);

            

        }

        #endregion

    }
}