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

#region Using directives

using System;
using MathNet.Numerics.Differentiation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Numerics
{
    /// <summary>
    /// Testing the class <see cref="CentredFiniteDifferenceDerivative"/>.
    /// </summary>
    [TestClass]
    public class NumericalDerivativesTests
    {

        [TestMethod]
        public void SinFirstDerivativeAtZeroTest()
        {
            Func<double, double> f = Math.Sin;
            var df = new NumericalDerivative();//CentredFiniteDifferenceDerivative();
            Assert.AreEqual(1, df.EvaluateDerivative(f, 0, 1), 1e-10);
        }

        [TestMethod]
        public void CubicPolynomialThirdDerivativeAtAnyValTest()
        {
            double Func(double x) => 3 * Math.Pow(x, 3) + 2 * x - 6;
            var df = new NumericalDerivative(5, 2);//CentredFiniteDifferenceDerivative(5, 2)
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 0;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 1;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 2;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 3;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
            df.Center = 4;
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(Func, 10, 3));
        }

        [TestMethod]
        public void CubicPolynomialFunctionValueTest()
        {
            double Func(double x) => 3 * Math.Pow(x, 3) + 2 * x - 6;
            var current = Func(2);
            var df = new NumericalDerivative(3, 0);//CentredFiniteDifferenceDerivative(3, 0);
            Assert.AreEqual(38, df.EvaluateDerivative(Func, 2, 1, current), 1e-8);
        }
    }
}