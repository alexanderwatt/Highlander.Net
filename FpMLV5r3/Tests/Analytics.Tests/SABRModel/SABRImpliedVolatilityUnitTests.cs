
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

#region Using Directives

using Highlander.Reporting.Analytics.V5r3.Stochastics.SABR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.SABRModel
{
    /// <summary>
    /// Unit Tests for the class SABRImpliedVolatility.
    /// </summary>
    [TestClass]
    public class SABRImpliedVolatilityUnitTests
    {
        #region Private Fields

        private double _alpha; // parameter alpha in the SABR model
        private double _assetPrice; // price of the relevant asset
        private double _beta; // parameter beta in the SABR model
        private double _exerciseTime; // time to option exercise
        private double _expected; // expected result
        private double _nu; // parameter nu in the SABR model
        private double _result; // container to store the implied volatility 
        private double _rho; // parameter rho in the SABR model
        private double _strike; // option strike price
        private bool _checkImpliedVolParameters; // check implied vol parameters
        private bool _checkSABRParameters; // check SABR model parameters
        private bool _isSuccessful; // is SABR implied volatility calculated
        private string _errorMessage; // container for possible error message
        private SABRParameters _sabrParameters; // store for all SABR parameters

        #endregion Private Fields

        #region SetUp

        [TestInitialize]
        public  void Initialisation()
        {
            _alpha = 15.53173d/100.0d;
            _assetPrice = 3.98d/100.0d;
            _beta = 1.0d;
            _exerciseTime = 5.0d;
            _expected = 0.0d;
            _nu = 40.0d/100.0d;
            _result = 0.0d;
            _rho = -33.0d/100.0d;
            _strike = _assetPrice;
            _checkImpliedVolParameters = true;
            _checkSABRParameters = true;
            _isSuccessful = true;
            _errorMessage = "";
            _sabrParameters = new SABRParameters(_alpha, _beta, _nu, _rho);
        }

        #endregion SetUp

        #region Test Constructor

        /// <summary>
        /// Tests the method SABRImpliedVolatility (constructor).
        /// </summary>
        [TestMethod]
        public void TestSABRImpliedVolatility()
        {
            SABRImpliedVolatility impliedVolObj =
                new SABRImpliedVolatility(_sabrParameters,
                                          _checkSABRParameters);
            Assert.IsNotNull(impliedVolObj);
        }

        #endregion Test Constructor

        #region Test the Method SABRInterpolatedVolatility

        /// <summary>
        /// Tests the method SABRInterpolatedVolatility.
        /// </summary>
        [TestMethod]
        public void TestSABRInterpolatedVolatility()
        {
            #region Tests: SABR Implied Volatility for beta = 0.0

            // Test computation of the SABR implied volatility for
            // the case beta = 0.0.
            _alpha = 0.60897d/100d;
            _beta = 0.0d;
            _nu = 32.0d/100d;
            _rho = 17.0d/100d;
            _checkSABRParameters = false;
            SABRParameters sabrParameters1 = 
                new SABRParameters(_alpha, _beta, _nu, _rho);

            SABRImpliedVolatility impliedVolObj1 =
                new SABRImpliedVolatility(sabrParameters1,
                                          _checkSABRParameters);

            // Strike = ATM - 200bp.
            _strike = _assetPrice - 200.0d/10000d; 
            _expected = 23.96039d/100d;
            const double tolerance1 = 1.0E-5d;

            _isSuccessful =
                impliedVolObj1.SABRInterpolatedVolatility(
                    _assetPrice,
                    _exerciseTime,
                    _strike,
                    ref _errorMessage,
                    ref _result,
                    _checkImpliedVolParameters);

            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance1);           

            // Strike = ATM.
            _strike = _assetPrice;
            _expected = 15.99991d/100d;

            _isSuccessful =
                impliedVolObj1.SABRInterpolatedVolatility(
                    _assetPrice,
                    _exerciseTime,
                    _strike,
                    ref _errorMessage,
                    ref _result,
                    _checkImpliedVolParameters);

            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance1);

            // Strike = ATM + 200bp.
            _strike = _assetPrice + 200.0d/10000d;
            _expected = 15.62789d/100d;

            _isSuccessful =
                impliedVolObj1.SABRInterpolatedVolatility(
                    _assetPrice,
                    _exerciseTime,
                    _strike,
                    ref _errorMessage,
                    ref _result,
                    _checkImpliedVolParameters);

            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance1);

            #endregion Tests: SABR Implied Volatility for beta = 0.0

            #region Tests: SABR Implied Volatility for beta = 0.5

            // Test computation of the SABR implied volatility for
            // the case beta = 0.5.
            _alpha = 3.05473d/100d;
            _beta = 0.5d;
            _nu = 34.0d/100d;
            _rho = - 11.0d/100d;
            _checkSABRParameters = false;
            SABRParameters sabrParameters2 =
                new SABRParameters(_alpha, _beta, _nu, _rho);

            SABRImpliedVolatility impliedVolObj2 =
                new SABRImpliedVolatility(sabrParameters2,
                                          _checkSABRParameters);

            // Strike = ATM - 200bp.
            _strike = _assetPrice - 200.0d/10000d;
            _expected = 23.73848d/100d;
            const double tolerance2 = 1.0E-5d;

            _isSuccessful =
                impliedVolObj2.SABRInterpolatedVolatility(_assetPrice,
                                                          _exerciseTime,
                                                          _strike,
                                                          ref _errorMessage,
                                                          ref _result,
                                                          _checkSABRParameters);
            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance2);

            // Strike = ATM
            _strike = _assetPrice;
            _expected = 16.00001d/100d;

            _isSuccessful =
                impliedVolObj2.SABRInterpolatedVolatility(_assetPrice,
                                                          _exerciseTime,
                                                          _strike,
                                                          ref _errorMessage,
                                                          ref _result,
                                                          _checkSABRParameters);
            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance2);

            // Strike = ATM + 200bp
            _strike = _assetPrice + 200.0d/10000d;
            _expected = 15.75552d/100d;

            _isSuccessful =
                impliedVolObj2.SABRInterpolatedVolatility(_assetPrice,
                                                          _exerciseTime,
                                                          _strike,
                                                          ref _errorMessage,
                                                          ref _result,
                                                          _checkSABRParameters);
            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance2);


            #endregion Tests: SABR Implied Volatility for beta = 0.5

            #region Tests: SABR Implied Volatility for beta = 1.0

            // Test computation of the SABR implied volatility for
            // the case beta = 1.0.
            _alpha = 15.53173d/100d;
            _beta = 1.0d;
            _nu = 40.0d/100d;
            _rho = -33.0d/100d;
            _checkSABRParameters = false;
            SABRParameters sabrParameters3 =
                new SABRParameters(_alpha, _beta, _nu, _rho);

            SABRImpliedVolatility impliedVolObj3 =
                new SABRImpliedVolatility(sabrParameters3,
                                          _checkSABRParameters);

            // Strike = ATM - 200bp.
            _strike = _assetPrice - 200.0d/10000d;
            _expected = 23.79395d/100d;
            const double tolerance3 = 1.0E-5d;

            _isSuccessful =
                impliedVolObj3.SABRInterpolatedVolatility
                    (_assetPrice,
                     _exerciseTime,
                     _strike,
                     ref _errorMessage,
                     ref _result,
                     _checkImpliedVolParameters);

            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance3);

            // Strike = ATM
            _strike = _assetPrice;
            _expected = 16.00000d/100d;

            _isSuccessful =
                impliedVolObj3.SABRInterpolatedVolatility
                    (_assetPrice,
                     _exerciseTime,
                     _strike,
                     ref _errorMessage,
                     ref _result,
                     _checkImpliedVolParameters);

            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance3);

            // Strike = ATM + 200bp
            _strike = _assetPrice + 200.0d/10000d;
            _expected = 16.05579d/100d;

            _isSuccessful =
                impliedVolObj3.SABRInterpolatedVolatility
                    (_assetPrice,
                     _exerciseTime,
                     _strike,
                     ref _errorMessage,
                     ref _result,
                     _checkImpliedVolParameters);

            Assert.IsTrue(_isSuccessful);
            Assert.AreEqual(_expected,
                            _result,
                            tolerance3);


            #endregion Tests: SABR Implied Volatility for beta = 1.0
        }

        /// <summary>
        /// These settings were causing an error in ComputeX, now fixed
        /// </summary>
        [TestMethod]
        public void TestSabrInterpolatedVolatilityComputeXDecimal()
        {
            const decimal Alpha = 0.219540520585315M;
            const decimal Beta = 1m;
            const decimal Nu = 0.0000000000021506467396720648M;
            const decimal Rho = 0.999999999999999M;
            SABRParameters sabrParameters = new SABRParameters(Alpha, Beta, Nu, Rho);

            const decimal AssetPrice = 0.07226393165M;
            const decimal ExerciseTime = 1 / 12M;
            const decimal Strike = 0.0722905443874852823M;
            const bool CheckImpliedVolParameters = false;
            decimal answer = 0;
            string error = "";

            SABRImpliedVolatility impliedVolObject = new SABRImpliedVolatility(sabrParameters, CheckImpliedVolParameters);

            bool result = impliedVolObject.SABRInterpolatedVolatility(AssetPrice, ExerciseTime, Strike,
                                                ref error, ref answer, CheckImpliedVolParameters);

            Assert.IsTrue(result);
            Assert.AreEqual("", error);
            Assert.AreEqual(6.72317387E-15, (double)answer, 1E-23);
        }

        /// <summary>
        /// These settings were causing an error in ComputeX, now fixed
        /// </summary>
        [TestMethod]
        public void TestSabrInterpolatedVolatilityComputeXDouble()
        {
            const double Alpha = 0.219540520585315;
            const double Beta = 1;
            const double Nu = 0.0000000000021506467396720648;
            const double Rho = 0.999999999999999;
            SABRParameters sabrParameters = new SABRParameters(Alpha, Beta, Nu, Rho);

            const double AssetPrice = 0.07226393165;
            const double ExerciseTime = 1d / 12;
            const double Strike = 0.0722905443874852823;
            const bool CheckImpliedVolParameters = false;
            double answer = 0;
            string error = "";

            SABRImpliedVolatility impliedVolObject = new SABRImpliedVolatility(sabrParameters, CheckImpliedVolParameters);

            bool result = impliedVolObject.SABRInterpolatedVolatility(AssetPrice, ExerciseTime, Strike,
                                                ref error, ref answer, CheckImpliedVolParameters);

            Assert.IsTrue(result);
            Assert.AreEqual("", error);
            Assert.AreEqual(6.72317387E-15, answer, 1E-23);
        }

        #endregion Test the Method SABRInterpolatedVolatility
  
    }
}