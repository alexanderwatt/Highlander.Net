/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using Highlander.Reporting.Analytics.V5r3.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Excel.Tests.V5r3.ExcelApi
{
    /// <summary>
    /// Tests for "BGM CashForward Example.xls"
    /// </summary>
    [TestClass]
    public class BgmCashForward
    {
        [TestMethod]
        public void CalculateCashForward1DTest()
        {
            double[] futuresPrice = { 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05 };
            double[,] volatility = {{ 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2 }};
            double[,] correlation = {{1}};
            double[] shift = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] coverage = { 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25 };
            double[] timeNodes = { 0, 0.25, 0.5, 0.75, 1, 1.25, 1.5, 1.75, 2, 2.25, 2.5 };
            double[,] result
                = CashForward.CalculateCashForward(futuresPrice, volatility,
                                                        correlation, shift, coverage, timeNodes);
            Assert.AreEqual(futuresPrice.Length, result.GetLength(0));
            Assert.AreEqual(5, result.GetLength(1));
            Assert.AreEqual(0.04999379, result[0, 0], 0.00000001);
            Assert.AreEqual(3.84e-12, result[0, 1], 0.01e-12);
            Assert.AreEqual(1, result[0, 2]);
        }

        [TestMethod]
        public void CalculateCashForward2DTest()
        {
            double[] futuresPrice = { 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05 };
            double[,] volatility
                = {
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2},
                      {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2}
                };
            double[,] correlation 
                = {
                    {1,0.951229424500714,0.90483741803596,0.860707976425058,0.818730753077982,0.778800783071405,0.740818220681718,0.704688089718713,0.670320046035639,0.637628151621773},
                    {0.951229424500714,1,0.951229424500714,0.90483741803596,0.860707976425058,0.818730753077982,0.778800783071405,0.740818220681718,0.704688089718713,0.670320046035639},
                    {0.90483741803596,0.951229424500714,1,0.951229424500714,0.90483741803596,0.860707976425058,0.818730753077982,0.778800783071405,0.740818220681718,0.704688089718713},
                    {0.860707976425058,0.90483741803596,0.951229424500714,1,0.951229424500714,0.90483741803596,0.860707976425058,0.818730753077982,0.778800783071405,0.740818220681718},
                    {0.818730753077982,0.860707976425058,0.90483741803596,0.951229424500714,1,0.951229424500714,0.90483741803596,0.860707976425058,0.818730753077982,0.778800783071405},
                    {0.778800783071405,0.818730753077982,0.860707976425058,0.90483741803596,0.951229424500714,1,0.951229424500714,0.90483741803596,0.860707976425058,0.818730753077982},
                    {0.740818220681718,0.778800783071405,0.818730753077982,0.860707976425058,0.90483741803596,0.951229424500714,1,0.951229424500714,0.90483741803596,0.860707976425058},
                    {0.704688089718713,0.740818220681718,0.778800783071405,0.818730753077982,0.860707976425058,0.90483741803596,0.951229424500714,1,0.951229424500714,0.90483741803596},
                    {0.670320046035639,0.704688089718713,0.740818220681718,0.778800783071405,0.818730753077982,0.860707976425058,0.90483741803596,0.951229424500714,1,0.951229424500714},
                    {0.637628151621773,0.670320046035639,0.704688089718713,0.740818220681718,0.778800783071405,0.818730753077982,0.860707976425058,0.90483741803596,0.951229424500714,1}
                };
            double[] shift = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] coverage = { 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25 };
            double[] timeNodes = { 0, 0.25, 0.5, 0.75, 1, 1.25, 1.5, 1.75, 2, 2.25, 2.5 };
            double[,] result
                = CashForward.CalculateCashForward(futuresPrice, volatility,
                                                        correlation, shift, coverage, timeNodes);
            Assert.AreEqual(futuresPrice.Length, result.GetLength(0));
            Assert.AreEqual(5, result.GetLength(1));
            Assert.AreEqual(0.04999379, result[0, 0], 0.00000001);
            Assert.AreEqual(3.84e-12, result[0, 1], 0.01e-12);
            Assert.AreEqual(1, result[0, 2]);
        }
    }
}