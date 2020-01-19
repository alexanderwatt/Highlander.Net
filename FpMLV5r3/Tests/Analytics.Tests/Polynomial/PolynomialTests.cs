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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Polynomial
{
    /// <summary>
    /// This class contains all the unit tests
    /// for polynomial
    /// </summary>
    [TestClass]
    public class PolynomialTests
    {

        #region Test Constructor

        /// <summary>
        /// Test the degree of polynomial
        /// </summary>
        [TestMethod]
        public void ConstructorTest1()
        {
            try
            {
                var poly=
                    new Reporting.Analytics.V5r3.Solvers.Polynomial(-1);
            }
            catch (Exception e)
            {
                const string errorMessage =
                    "A degree of polynomial can't be a negative number.";
                Assert.AreEqual(e.Message, errorMessage);
            }

        }

        /// <summary>
        /// Test the degree of polynomial
        /// </summary>
        [TestMethod]
        public void ConstructorTest2()
        {
            var poly =
                      new Reporting.Analytics.V5r3.Solvers.Polynomial(1);
            decimal firstCoeff = poly.GetCoeff(0);
            decimal secondCoeff = poly.GetCoeff(1);
            decimal expectedCoeff = 0.0m;
            Assert.AreEqual(expectedCoeff, firstCoeff);
            Assert.AreEqual(expectedCoeff, secondCoeff);
        }

        /// <summary>
        /// Test the coefficients of polynomial
        /// </summary>
        [TestMethod]
        public void ConstructorTest3()
        {
            var poly =
                      new Reporting.Analytics.V5r3.Solvers.Polynomial(1,1);
            decimal firstCoeff = poly.GetCoeff(0);
            decimal secondCoeff = poly.GetCoeff(1);
            decimal expectedCoeff = 1.0m;
            Assert.AreEqual(expectedCoeff, firstCoeff);
            Assert.AreEqual(expectedCoeff, secondCoeff);
        }

        /// <summary>
        /// Test the coefficients of polynomial
        /// </summary>
        [TestMethod]
        public void ConstructorTest4()
        {
           
            decimal[] coeffs = { 0.0m, 1.0m };
            var poly =
                       new Reporting.Analytics.V5r3.Solvers.Polynomial(coeffs, 1);
            decimal firstCoeff = poly.GetCoeff(0);
            decimal secondCoeff = poly.GetCoeff(1);
            decimal expectedCoeff = 0.0m;
            Assert.AreEqual(expectedCoeff, firstCoeff);
            expectedCoeff = 1.0m;
            Assert.AreEqual(expectedCoeff, secondCoeff);
            var newPoly =
                        new Reporting.Analytics.V5r3.Solvers.Polynomial(poly);
            firstCoeff = newPoly.GetCoeff(0);
            secondCoeff = newPoly.GetCoeff(1);
            expectedCoeff = 0.0m;
            Assert.AreEqual(expectedCoeff, firstCoeff);
            expectedCoeff = 1.0m;
            Assert.AreEqual(expectedCoeff, secondCoeff);
        }

        #endregion

        #region Test Business Logic

            /// <summary>
        /// Test the value function
        /// </summary>
        [TestMethod]
        public void ValueTest1()
        {
            var poly =
                     new Reporting.Analytics.V5r3.Solvers.Polynomial(2, 2);
            decimal calculatedResult = poly.Value(5);
            decimal expectedResult = 62.0m;
            Assert.AreEqual(expectedResult, calculatedResult);
        }

        [TestMethod]
        public void ValueTest2()
        {
            var poly =
                    new Reporting.Analytics.V5r3.Solvers.Polynomial(1, 0);
            decimal calculatedResult = poly.Value(5);
            decimal expectedResult = 1.0m;
            Assert.AreEqual(expectedResult, calculatedResult);
        }

        #endregion

        #region Test binary functions
        [TestMethod]
        public void TestBinary1()
        {
            var poly =
                    new Reporting.Analytics.V5r3.Solvers.Polynomial(1, 0);
            var secondPoly =
                    new Reporting.Analytics.V5r3.Solvers.Polynomial(1, 1);
            var result = poly * secondPoly;
            decimal expectedResult = 3.0m;
            decimal calculatedResult = result.Value(2.0m);
            Assert.AreEqual(expectedResult, calculatedResult);
        }


        [TestMethod]
        public void TestBinary2()
        {
            decimal[] coeffs = new decimal[2];
            coeffs[0] = 1.0m;
            coeffs[1] = 0.25m;
            var poly =
                 new Reporting.Analytics.V5r3.Solvers.Polynomial(coeffs, 1);
            var secondPoly =
                 new Reporting.Analytics.V5r3.Solvers.Polynomial(coeffs, 1);
            var result = poly * secondPoly;
            decimal expectedResult = 1.5625m;
            decimal calculatedResult = result.Value(1.0m);
            Assert.AreEqual(expectedResult, calculatedResult);
        }

        #endregion

    }
}
