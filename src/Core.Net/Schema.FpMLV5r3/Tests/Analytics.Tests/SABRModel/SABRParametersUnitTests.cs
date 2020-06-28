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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.SABRModel
{
    /// <summary>
    /// Unit Tests for the class SABRParameters.
    /// </summary>
    [TestClass]
    public class SABRParametersUnitTests
    {
        #region Private Fields
        
        private decimal _alpha; // parameter alpha in the SABR model
        private decimal _beta; // parameter beta in the SABR model
        private decimal _nu; // parameter nu in the SABR model
        private decimal _rho; // parameter rho in the SABR model
        private bool _isValidSABRParameter; // is valid SABR parameter
        private string _errorMessage; // container for possible error message

        #endregion Private Fields

        #region SetUp

        /// <summary>
        /// Initialise an instance of the class with all valid parameters.
        /// </summary>
        [TestInitialize]
        public void Initialisation()
        {
            _alpha = 15.53173m/100.0m;
            _beta = 1.0m;
            _nu = 40.0m/100.0m;
            _rho = -33.0m/100.0m;
            _isValidSABRParameter = true;
            _errorMessage = "";
        }

        #endregion SetUp

        #region Test Constructor

        /// <summary>
        /// Tests the method SABRImpliedVolatility (constructor).
        /// </summary>
        [TestMethod]
        public void TestSABRParameters()
        {
            // Test: object instantiation.
            SABRParameters sabrParameters =
                new SABRParameters(_alpha, _beta, _nu, _rho);

            Assert.IsNotNull(sabrParameters);

            // Test: data mapped to the correct private fields.
            Assert.AreEqual(_alpha, sabrParameters.Alpha);
            Assert.AreEqual(_beta, sabrParameters.Beta);
            Assert.AreEqual(_nu, sabrParameters.Nu);
            Assert.AreEqual(_rho, sabrParameters.Rho);
        }

        #endregion Test Constructor

        #region Tests: Check SABR Parameter Alpha

        /// <summary>
        /// Tests the static method CheckSABRParamterAlpha.
        /// </summary>
        [TestMethod]
        public void TestCheckSABRParamterAlpha()
        {
            _alpha = 0.0m;
            _isValidSABRParameter =
                SABRParameters.CheckSABRParameterAlpha(_alpha, ref _errorMessage);
            const string expected = "SABR parameter alpha must be positive.";

            Assert.IsFalse(_isValidSABRParameter);
            Assert.AreEqual(expected, _errorMessage);
        }

        #endregion Tests: Check SABR Parameter Alpha

        #region Tests: Check SABR Parameter Beta

        /// <summary>
        /// Tests the static method CheckSABRParamterBeta.
        /// </summary>
        [TestMethod]
        public void TestCheckSABRParameterBeta()
        {
            // Check the case beta < 0.0.
            _beta = -0.1m;
            _isValidSABRParameter =
                SABRParameters.CheckSABRParameterBeta(_beta, ref _errorMessage);
            const string expected =
                "SABR parameter beta not in the range [0,1].";

            Assert.IsFalse(_isValidSABRParameter);
            Assert.AreEqual(expected, _errorMessage);

            // Check the case beta > 1.0.
            _beta = 1.03m;
            _isValidSABRParameter =
                SABRParameters.CheckSABRParameterBeta(_beta, ref _errorMessage);

            Assert.IsFalse(_isValidSABRParameter);
            Assert.AreEqual(expected, _errorMessage);
        }

        #endregion Tests: Check SABR Parameter Beta

        #region Tests: Check SABR Parameter Nu

        /// <summary>
        /// Tests the static method CheckSABRParamterNu.
        /// </summary>
        [TestMethod]
        public void TestCheckSABRParameterNu()
        {
            _nu = -0.4m;
            _isValidSABRParameter =
                SABRParameters.CheckSABRParameterNu(_nu, ref _errorMessage);
            const string expected = "SABR parameter nu cannot be negative.";

            Assert.IsFalse(_isValidSABRParameter);
            Assert.AreEqual(expected, _errorMessage);
        }

        #endregion Tests: Check SABR Parameter Nu

        #region Tests: Check SABR Parameter Rho

        /// <summary>
        /// Tests the Tests the static method CheckSABRParamterRho.
        /// </summary>
        [TestMethod]
        public void TestCheckSABRParameterRho()
        {
            _rho = 1.001m;
            _isValidSABRParameter =
                SABRParameters.CheckSABRParameterRho(_rho, ref _errorMessage);
            const string expected = "SABR parameter rho not in the range (-1,1)";

            Assert.IsFalse(_isValidSABRParameter);
            Assert.AreEqual(expected, _errorMessage);
        }

        #endregion Tests: Check SABR Parameter Rho
    }
}