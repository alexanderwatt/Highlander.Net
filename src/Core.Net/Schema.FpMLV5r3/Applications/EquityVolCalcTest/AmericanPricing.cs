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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Highlander.Equities;
using Highlander.EquitiesCalculator.TestData.V5r3;
using Highlander.Reporting.Analytics.V5r3.Equities;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Orion.Equities.VolCalc.TestData;
using ForwardExpiry = Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities.ForwardExpiry;
using OptionPosition = Highlander.Equities.OptionPosition;
using Stock = Highlander.Reporting.Analytics.V5r3.Equities.Stock;
using Strike = Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities.EquityStrike;
using VolatilityPoint = Highlander.Equities.VolatilityPoint;
using VolatilitySurface = Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities.VolatilitySurface;

namespace Highlander.EquitiesCalculator.Tests.V5r3
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class AmericanPricing
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestLongDatedPrice()
        {
            DateTime date0 = DateTime.Today;
            DateTime exp = date0.AddDays(5*365);
            DateTime ex = date0.AddDays(20);
            double spot = 100;
            double strike = 100;
            double vol = 0.90;
            string paystyle = "C";
            string exercise = "A";          
            DateTime[] dates = {date0, exp};
            double [] rates = {0.05, 0.05};
            RateCurve rc = new RateCurve("AUD","Continuous",date0,dates,rates);
            List<Dividend> divs = new List<Dividend> {new Dividend(ex, 20)};
            AmOptionAnalytics utils = new AmOptionAnalytics(date0, exp, spot, strike, vol, exercise, paystyle, rc, divs, 120);
            double pr = Math.Round(utils.Price(),7);
            Debug.Print("Price is {0}" , pr);
            Assert.AreEqual(55.5434072,pr);
        }

        [TestMethod]
        public void TestITMPrice()
        {
            DateTime date0 = DateTime.Today;
            DateTime exp = date0.AddDays(90);
            DateTime ex = date0.AddDays(20);
            double spot = 100;
            double strike = 50;
            double vol = 0.50;
            string paystyle = "C";
            string exercise = "A";
            DateTime[] dates = { date0, exp };
            double[] rates = { 0.05, 0.05 };
            RateCurve rc = new RateCurve("AUD", "Continuous", date0, dates, rates);
            List<Dividend> divs = new List<Dividend> {new Dividend(ex, 20)};
            AmOptionAnalytics utils = new AmOptionAnalytics(date0, exp, spot, strike, vol, exercise, paystyle, rc, divs, 120);
            double pr = Math.Round(utils.Price(), 7);
            Debug.Print("Price is {0}", pr);
            Assert.AreEqual(50.1333834, pr);
        }

        [TestMethod]
        public void TestOTMPrice()
        {
            DateTime date0 = DateTime.Today;
            DateTime exp = date0.AddDays(90);
            DateTime ex = date0.AddDays(20);
            double spot = 100;
            double strike = 150;
            double vol = 0.50;
            string paystyle = "C";
            string exercise = "A";
            DateTime[] dates = { date0, exp };
            double[] rates = { 0.05, 0.05 };
            RateCurve rc = new RateCurve("AUD", "Continuous", date0, dates, rates);
            List<Dividend> divs = new List<Dividend> {new Dividend(ex, 20)};
            AmOptionAnalytics utils = new AmOptionAnalytics(date0, exp, spot, strike, vol, exercise, paystyle, rc, divs, 120);
            double pr = Math.Round(utils.Price(), 7);
            Debug.Print("Price is {0}", pr);
            Assert.AreEqual(0.0556798, pr);
        }

        [TestMethod]
        public void TestEuroPrice()
        {
            DateTime date0 = DateTime.Today;
            DateTime exp = date0.AddDays(90);
            DateTime ex = date0.AddDays(20);
            double spot = 100;
            double strike = 100;
            double vol = 0.50;
            string paystyle = "C";
            string exercise = "E";
            DateTime[] dates = { date0, exp };
            double[] rates = { 0.05, 0.05 };
            RateCurve rc = new RateCurve("AUD", "Continuous", date0, dates, rates);
            List<Dividend> divs = new List<Dividend> {new Dividend(ex, 20)};
            AmOptionAnalytics utils = new AmOptionAnalytics(date0, exp, spot, strike, vol, exercise, paystyle, rc, divs, 120);
            double pr = Math.Round(utils.Price(), 7);
            Debug.Print("Price is {0}", pr);
            Assert.AreEqual(2.4341099, pr);
        }

        [TestMethod]
        public void TestVega()
        {
            DateTime date0 = DateTime.Today;
            DateTime exp = date0.AddDays(90);
            DateTime ex = date0.AddDays(20);
            double spot = 100;
            double strike = 100;
            double vol = 0.50;
            string paystyle = "C";
            string exercise = "E";
            DateTime[] dates = { date0, exp };
            double[] rates = { 0.05, 0.05 };
            RateCurve rc = new RateCurve("AUD", "Continuous", date0, dates, rates);
            List<Dividend> divs = new List<Dividend> {new Dividend(ex, 20)};
            AmOptionAnalytics utils = new AmOptionAnalytics(date0, exp, spot, strike, vol, exercise, paystyle, rc, divs, 120);
            double pr = Math.Round(utils.Price(), 7);
            utils.MakeVega();
            double vega = Math.Round(utils.Vega,7);           
            Assert.AreEqual(0.1223754, vega);
        }

        [TestMethod]
        public void TestAmVega()
        {
            var stock = TestDataHelper.GetStock("ANZ");
            Assert.AreEqual("ANZ", stock.Name);
            var stockObject = TestHelper.CreateStock(stock);
            var fwd = new ForwardExpiry(DateTime.Parse("23-12-2010"), 2220);
            var str = new Strike(2100, null, null, Units.Cents);
            var vp = new VolatilityPoint {Value = 0.30M};
            str.SetVolatility(vp);
            fwd.AddStrike(str, true);
            stockObject.VolatilitySurface.AddExpiry(fwd);
            foreach (ForwardExpiry fwdExp in stockObject.VolatilitySurface.Expiry)
            {
                foreach (Strike strike in fwdExp.Strikes)
                {
                    if ((strike.StrikePrice == 2100.0) && (fwdExp.ExpiryDate == DateTime.Parse("23-12-2010")))
                    {
                        AmOptionAnalytics call = new AmOptionAnalytics(stockObject.Date, fwdExp.ExpiryDate, Convert.ToDouble(stockObject.Spot), strike.StrikePrice, Convert.ToDouble(strike.Volatility.Value), "A", "C", stockObject.RateCurve, stockObject.Dividends, 20);
                        double callprice = call.Price();
                        Assert.AreEqual(callprice, 338.8661, 0.5);
                        call.MakeVega();
                        Assert.AreEqual(call.Vega, 9.093, 0.02);
                        AmOptionAnalytics put = new AmOptionAnalytics(stockObject.Date, fwdExp.ExpiryDate, Convert.ToDouble(stockObject.Spot), strike.StrikePrice, Convert.ToDouble(strike.Volatility.Value), "A", "P", stockObject.RateCurve, stockObject.Dividends, 20);
                        double putprice = put.Price();
                        Assert.AreEqual(putprice, 239.6014, 0.5);
                        put.MakeVega();
                        Assert.AreEqual(put.Vega, 9.093, 0.02);                  
                    }
                }
            }
        }
      
        [TestMethod]
        public void TestImpliedVolCall()
        {
            DateTime date0 = DateTime.Today;
            DateTime exp = date0.AddDays(90);
            DateTime ex = date0.AddDays(20);
            double spot = 100;
            double strike = 100;
            double vol = 0.50;
            string paystyle = "C";
            DateTime[] dates = { date0, exp };
            double[] rates = { 0.05, 0.05 };
            RateCurve rc = new RateCurve("AUD", "Continuous", date0, dates, rates);
            List<Dividend> divs = new List<Dividend> {new Dividend(ex, 20)};
            AmOptionAnalytics amInstr = new AmOptionAnalytics(date0, exp, spot, strike, vol, "A", paystyle, rc, divs, 120);
            AmOptionAnalytics euInstr = new AmOptionAnalytics(date0, exp, spot, strike, 2*vol, "E", paystyle, rc, divs, 120);
            double am = amInstr.Price();
            double eu = euInstr.Price();
            AmOptionAnalytics amInstr0 = new AmOptionAnalytics(date0, exp, spot, strike, 0.20, "A", paystyle, rc, divs, 120);
            AmOptionAnalytics euInstr0 = new AmOptionAnalytics(date0, exp, spot, strike, 0.80, "E", paystyle, rc, divs, 120);
            double avol0 = amInstr0.OptSolveVol(am,100);
            double evol0 = euInstr0.OptSolveVol(eu, 100);
            var stock = new Stock(date0,100,"dummy","BHP",rc,divs);           
            DateTime start = DateTime.Now;
            double vol1 = OptionHelper.GetImpliedVol(stock, exp, 0.3, strike, true, "A", am, 120);
            DateTime end = DateTime.Now;
            double x1 = end.Subtract(start).Milliseconds;
            start = DateTime.Now;
            double vol11 = OptionHelper.GetImpliedVol(stock, exp, 0.2, strike, true, "American", am, 120);
            end = DateTime.Now;
            double x2 = end.Subtract(start).Milliseconds;
            start = DateTime.Now;
            double vol0 = OptionHelper.GetImpliedVol(stock, exp, 0.8, strike, true, "E", eu, 120);
            end = DateTime.Now;            
            double x3 = end.Subtract(start).Milliseconds;
            start = DateTime.Now;
            double vol00 = OptionHelper.GetImpliedVol(stock, exp, 0.8, strike, true, "European", eu, 120);
            end = DateTime.Now;
            double x4 = end.Subtract(start).Milliseconds;

            Assert.AreEqual(avol0, 0.50, 0.001); 
            Assert.AreEqual(evol0, 1.00, 0.001);

            Assert.AreEqual(vol1, avol0, 0.001);
            Assert.AreEqual(vol11, avol0, 0.001);

            Assert.AreEqual(vol0, evol0, 0.001);
            Assert.AreEqual(vol00, evol0, 0.001);

            Debug.Print("Time 1 : " + x1);
            Debug.Print("Time 2 : " + x2);
            Debug.Print("Time 3 : " + x3);
            Debug.Print("Time 4 : " + x4);


            


        }

        [TestMethod]
        public void TestImpliedVolPut()
        {
            DateTime date0 = DateTime.Today;
            DateTime exp = date0.AddDays(90);
            DateTime ex = date0.AddDays(20);
            double spot = 100;
            double strike = 100;
            double vol = 0.50;
            string paystyle = "P";
            DateTime[] dates = { date0, exp };
            double[] rates = { 0.05, 0.05 };
            RateCurve rc = new RateCurve("AUD", "Continuous", date0, dates, rates);
            List<Dividend> divs = new List<Dividend> {new Dividend(ex, 20)};
            AmOptionAnalytics amInstr = new AmOptionAnalytics(date0, exp, spot, strike, vol, "A", paystyle, rc, divs, 120);
            AmOptionAnalytics euInstr = new AmOptionAnalytics(date0, exp, spot, strike, 2 * vol, "E", paystyle, rc, divs, 120);
            double am = amInstr.Price();
            double eu = euInstr.Price();
            AmOptionAnalytics amInstr0 = new AmOptionAnalytics(date0, exp, spot, strike, 0.20, "A", paystyle, rc, divs, 120);
            AmOptionAnalytics euInstr0 = new AmOptionAnalytics(date0, exp, spot, strike, 0.80, "E", paystyle, rc, divs, 120);
            double avol0 =  amInstr0.OptSolveVol(am, 100);
            double evol0 =  euInstr0.OptSolveVol(eu, 100);
            Stock stock = new Stock(date0, 100, "dummy", "BHP", rc, divs);
            double vol1 = OptionHelper.GetImpliedVol(stock, exp,  strike, false, "A", am, 120);
            double vol11 = OptionHelper.GetImpliedVol(stock, exp, strike, false, "American", am, 120);
            double vol0 = OptionHelper.GetImpliedVol(stock, exp, strike, false, "E", eu, 120);
            double vol00 = OptionHelper.GetImpliedVol(stock, exp, strike, false, "European", eu, 120);
            Assert.AreEqual(avol0, 0.50, 0.001);
            Assert.AreEqual(evol0, 1.00, 0.001);
            Assert.AreEqual(vol1, avol0, 0.001);
            Assert.AreEqual(vol11, avol0, 0.001);
            Assert.AreEqual(vol0, evol0, 0.001);
            Assert.AreEqual(vol00, evol0, 0.001);
        }


        [TestMethod]
        public void TestImpliedVol1()
        {
            var stock = TestDataHelper.GetStock("ANZ");
            Assert.AreEqual("ANZ", stock.Name);
            var stockObject = TestHelper.CreateStock(stock);
            ForwardExpiry fwd = new ForwardExpiry(DateTime.Parse("23-12-2010"), 2220);
            Strike str = new Strike(2100, null, null, Units.Cents);
            fwd.AddStrike(str, true);
            stockObject.VolatilitySurface.AddExpiry(fwd);            
            foreach (ForwardExpiry fwdExp in stockObject.VolatilitySurface.Expiry)
            {
                foreach (Strike strike in fwdExp.Strikes)
                {
                    if (strike.StrikePrice == 2100.0 && (fwdExp.ExpiryDate == DateTime.Parse("23-12-2010")))
                    {
                        var vol0 = OptionHelper.GetImpliedVol(stockObject, fwdExp.ExpiryDate, strike.StrikePrice, true, "A", 352.5, 120);
                        AmOptionAnalytics call = new AmOptionAnalytics(stockObject.Date, fwdExp.ExpiryDate, Convert.ToDouble(stockObject.Spot), strike.StrikePrice, vol0, "A", "C", stockObject.RateCurve, stockObject.Dividends, 120);
                        Assert.AreEqual(call.Price(), 352.5, 0.01);
                        var vol1 = OptionHelper.GetImpliedVol(stockObject, fwdExp.ExpiryDate, strike.StrikePrice, false, "A", 200.5, 120);
                        AmOptionAnalytics put = new AmOptionAnalytics(stockObject.Date, fwdExp.ExpiryDate, Convert.ToDouble(stockObject.Spot), strike.StrikePrice, vol1, "A", "P", stockObject.RateCurve, stockObject.Dividends, 120);
                        Assert.AreEqual(put.Price(), 200.5, 0.01);
                    }
                }
            }         
        }

        [TestMethod]
        public void TestImpliedVol2()
        {                      
            var stock = TestDataHelper.GetStock("ANZ");
            Assert.AreEqual("ANZ", stock.Name);
            Stock stockObject = TestHelper.CreateStock(stock);
            ForwardExpiry fwd = new ForwardExpiry(DateTime.Parse("29-10-2009"), 3676);
            Strike str = new Strike(3650, null, null, Units.Cents);
            fwd.AddStrike(str, true);
            stockObject.VolatilitySurface.AddExpiry(fwd);
            foreach (ForwardExpiry fwdExp in stockObject.VolatilitySurface.Expiry)
            {
                foreach (Strike strike in fwdExp.Strikes)
                {
                    if (strike.StrikePrice == 3650 && (fwdExp.ExpiryDate == DateTime.Parse("29-10-2009")))
                    {
                        stockObject.Spot = 3676;
                        var vol0 = OptionHelper.GetImpliedVol(stockObject, fwdExp.ExpiryDate, strike.StrikePrice, true, "A", 224.5, 120);
                        AmOptionAnalytics call = new AmOptionAnalytics(stockObject.Date, fwdExp.ExpiryDate, Convert.ToDouble(stockObject.Spot), strike.StrikePrice, vol0, "A", "C", stockObject.RateCurve, stockObject.Dividends, 120);
                        Assert.AreEqual(call.Price(), 224.5, 0.01);
                    }
                }
            }
        }

        [TestMethod]
        public void TestImpliedVol3()
        {                     
            var stock = TestDataHelper.GetStock("RIO");
            Assert.AreEqual("RIO", stock.Name);
            var stockObject = TestHelper.CreateStock(stock);
            ForwardExpiry fwd = new ForwardExpiry(DateTime.Parse("24-09-2009"), 5818);
            Strike str = new Strike(5800, null, null, Units.Cents);
            fwd.AddStrike(str, true);
            stockObject.VolatilitySurface.AddExpiry(fwd);
            foreach (ForwardExpiry fwdExp in stockObject.VolatilitySurface.Expiry)
            {
                foreach (Strike strike in fwdExp.Strikes)
                {
                    if (strike.StrikePrice == 5800 && (fwdExp.ExpiryDate == DateTime.Parse("24-09-2009")))
                    {
                        var vol0 = OptionHelper.GetImpliedVol(stockObject, fwdExp.ExpiryDate, strike.StrikePrice, false, "A", 191.0, 120);
                        AmOptionAnalytics call = new AmOptionAnalytics(stockObject.Date, fwdExp.ExpiryDate, Convert.ToDouble(stockObject.Spot), strike.StrikePrice, vol0, "A", "P", stockObject.RateCurve, stockObject.Dividends, 120);
                        Assert.AreEqual(call.Price(), 191, 0.01);
                    }
                }
            }
        }

        [TestMethod]
        public void TestLargeImpliedVol()
        {                      
            var stock = TestDataHelper.GetStock("RIO");
            Assert.AreEqual("RIO", stock.Name);
            var stockObject = TestHelper.CreateStock(stock);

            ForwardExpiry fwd = new ForwardExpiry(DateTime.Parse("24-09-2009"), 5818);
            Strike str = new Strike(5800, null, null, Units.Cents);
            fwd.AddStrike(str, true);
            stockObject.VolatilitySurface.AddExpiry(fwd);
            foreach (ForwardExpiry fwdExp in stockObject.VolatilitySurface.Expiry)
            {
                foreach (Strike strike in fwdExp.Strikes)
                {
                    if (strike.StrikePrice == 5800 && (fwdExp.ExpiryDate == DateTime.Parse("24-09-2009")))
                    {
                        var vol0 = OptionHelper.GetImpliedVol(stockObject, fwdExp.ExpiryDate, strike.StrikePrice, false, "A", 450.0, 120);
                        AmOptionAnalytics call = new AmOptionAnalytics(stockObject.Date, fwdExp.ExpiryDate, Convert.ToDouble(stockObject.Spot), strike.StrikePrice, vol0, "A", "P", stockObject.RateCurve, stockObject.Dividends, 120);
                        Assert.AreEqual(call.Price(), 450, 0.01);
                    }
                }
            }
        }

        [TestMethod]
        public void TestSmallImpliedVol()
        {           
            var stock = TestDataHelper.GetStock("RIO");
            Assert.AreEqual("RIO", stock.Name);
            var stockObject = TestHelper.CreateStock(stock);
            ForwardExpiry fwd = new ForwardExpiry(DateTime.Parse("24-09-2009"), 5818);
            Strike str = new Strike(5800, null, null, Units.Cents);
            fwd.AddStrike(str, true);
            stockObject.VolatilitySurface.AddExpiry(fwd);
            foreach (ForwardExpiry fwdExp in stockObject.VolatilitySurface.Expiry)
            {
                foreach (Strike strike in fwdExp.Strikes)
                {
                    if ((strike.StrikePrice == 5800) && (fwdExp.ExpiryDate == DateTime.Parse("24-09-2009")))
                    {
                        var vol0 = OptionHelper.GetImpliedVol(stockObject, fwdExp.ExpiryDate, strike.StrikePrice, false, "A", 40.0, 120);
                        AmOptionAnalytics call = new AmOptionAnalytics(stockObject.Date, fwdExp.ExpiryDate, Convert.ToDouble(stockObject.Spot), strike.StrikePrice, vol0, "A", "P", stockObject.RateCurve, stockObject.Dividends, 120);
                        Assert.AreEqual(call.Price(), 40.0, 0.01);
                    }
                }
            }
        }

        public void TestImpliedVolLargeDiv()
        {          
            var mig = TestDataHelper.GetStock("MIG_20091211.xml");
            var migStock = TestHelper.CreateStock(mig);
            double vol = OptionHelper.GetImpliedVol(migStock, new DateTime(2010, 01, 28), 0.30, 110, true, "A", 21, 120);                       
        }

        /// <summary>
        /// This unit test was created in response to an error we were getting with maturities over 2 years
        /// There was a problem with the initial guess for longer maturity options (from Fusai Roncorini text)
        /// </summary>
        [TestMethod]
        public void TestImpliedVol4()
        {

            DateTime today = new DateTime(2010,07,14);
            RateCurve rc = new RateCurve("AUD", "Continuous", DateTime.Parse("14-Jul-2010"), new[] { DateTime.Parse("14-Jul-2010"), DateTime.Parse("28-Jun-2012") }, new[] { 0.045507, 0.050645 });
            Dividend d1 = new Dividend(DateTime.Parse("6-9-2010"), 49.90M);
            Dividend d2 = new Dividend(DateTime.Parse("28-2-2011"), 54.43M);
            Dividend d3 = new Dividend(DateTime.Parse("5-9-2011"), 58.97M);
            Dividend d4 = new Dividend(DateTime.Parse("27-2-2012"), 61.24M);
            Dividend d5 = new Dividend(DateTime.Parse("3-9-2012"), 65.78M);
            Dividend d6 = new Dividend(DateTime.Parse("25-2-2013"), 68.05M);
            Dividend d7 = new Dividend(DateTime.Parse("2-9-2013"), 72.58M);
            Dividend d8 = new Dividend(DateTime.Parse("2-9-2013"), 72.58M);
            Dividend d9 = new Dividend(DateTime.Parse("24-2-2014"), 73.72M);
            Dividend d10 = new Dividend(DateTime.Parse("1-9-2014"), 75.99M);
            List<Dividend> divCurve = new List<Dividend>() { d1, d2, d3, d4, d5, d6, d7, d8, d9, d10 };
            var stockObject0 = new Stock(today,3840,"BHP12JUN4100P.WY","BHP_Vanilla_ETO_Jun12_41.00_Put",rc,divCurve);
            double vol = OptionHelper.GetImpliedVol(stockObject0, DateTime.Parse("28-Jun-2012"), 4100, false, "American", 719, 120);
        }

        /// <summary>
        ///  Added 3/9/2010 to test nETS output. nETS is replacing the Orc dependency
        ///  for Fair Value calculation of Index Options
        /// </summary>
        [TestMethod]
        public void TestPricevsORCExample()
        {
            DateTime[] rtDates = { DateTime.Parse("17-Aug-2010"), 
                                                   DateTime.Parse("17-Sep-2010"),
                                                   DateTime.Parse("18-Oct-2010"),
                                                   DateTime.Parse("17-Nov-2010"),
                                                   DateTime.Parse("10-Dec-2010") };
            
            double[] rates = { 0.045507232, 
                                            0.046609656,
                                            0.047336042,
                                            0.047655159,
                                            0.047737236
                                          };
            RateCurve rc = new RateCurve("AUD", "Semi-Annual", DateTime.Parse("16-Aug-2010"),rtDates , rates);
            //Dividend d1 = new Dividend(DateTime.Parse("16-8-2010"),  11.745786M);
            Dividend d2 = new Dividend(DateTime.Parse("17-8-2010"),  0.893295M);
            Dividend d3 = new Dividend(DateTime.Parse("23-8-2010"),  7.856689M);
            Dividend d4 = new Dividend(DateTime.Parse("24-8-2010"), 2.898251M);
            Dividend d5 = new Dividend(DateTime.Parse("25-8-2010"), 3.344721M);
            Dividend d6 = new Dividend(DateTime.Parse("26-8-2010"), 0.485070M);
            Dividend d7 = new Dividend(DateTime.Parse("27-8-2010"), 0.835305M);
            Dividend d8 = new Dividend(DateTime.Parse("30-8-2010"), 3.952976M);
            Dividend d9 = new Dividend(DateTime.Parse("31-8-2010"), 0.884255M);
            Dividend d10 = new Dividend(DateTime.Parse("1-9-2010"), 2.013798M);
            Dividend d11 = new Dividend(DateTime.Parse("2-9-2010"), 1.241407M);
            Dividend d12 = new Dividend(DateTime.Parse("3-9-2010"), 0.613699M);
            Dividend d13 = new Dividend(DateTime.Parse("6-9-2010"), 11.712946M);
            Dividend d14 = new Dividend(DateTime.Parse("7-9-2010"), 3.775104M);
            Dividend d15 = new Dividend(DateTime.Parse("8-9-2010"), 0.606597M);
            Dividend d16 = new Dividend(DateTime.Parse("9-9-2010"), 0.268093M);
            Dividend d17 = new Dividend(DateTime.Parse("10-9-2010"), 0.144851M);
            Dividend d18 = new Dividend(DateTime.Parse("13-9-2010"), 1.600975M);
            Dividend d19 = new Dividend(DateTime.Parse("14-9-2010"), 1.499946M);
            Dividend d20 = new Dividend(DateTime.Parse("15-9-2010"), 0.238824M);
            Dividend d21 = new Dividend(DateTime.Parse("16-9-2010"), 0.117931M);
            List<Dividend> divCurve = new List<Dividend>() { d2, d3, d4, d5, d6, d7, d8, d9, d10, 
                                                             d11,d12,d13,d14,d15,d16,d17, d18, d19, d20, d21 };       
            DateTime date0 = new DateTime(2010, 08, 16);
            DateTime exp = new DateTime(2010, 9, 16);
            double spot = 4447.62;
            //double future = 4420;
            double strike = 4200;
            string stockId = "XJO_Spot";
            Stock stock = new Stock(date0, Convert.ToDecimal(spot), stockId, "XJO", rc, divCurve);
            double fwd = stock.GetForward(date0, exp);     
            var wingParams1 = new OrcWingParameters
            {
                CurrentVol = 0.234828,
                RefVol = 0.234828,
                SlopeRef = -0.109109,
                PutCurve = 1.235556,
                CallCurve = 0.60895,
                DnCutoff = -0.493791,
                UpCutoff = 0.506209,
                RefFwd = 4420.092383,
                AtmForward = 4420.092383,
                Vcr = 0,
                Scr = 0,
                Ssr = 100,
                Dsr = 0.2,
                Usr = 0.2,
            };
            VolatilitySurface vs = new VolatilitySurface(stockId, Convert.ToDecimal(spot), date0);
            ForwardExpiry expiry = new ForwardExpiry(exp, Convert.ToDecimal(fwd));
            OptionPosition op = new OptionPosition( "XJO_Vanilla_ETO_Sep10_4200.000_Put", 30.237108, PositionType.Put);
            Strike strike0 = new Strike(strike,null,op);
            expiry.AddStrike(strike0, true);        
            List<double> strikes1 = new List<double> { 4200 };           
            ForwardExpiry forwardexpiry = vs.ValueAt(stock, exp,strikes1,wingParams1, true, false);
            double vol = Convert.ToDouble(forwardexpiry.Strikes[0].Volatility.Value);
            var utils = new AmOptionAnalytics(date0, exp, spot, strike, vol, "European", "Put", rc, divCurve, 120);            
            double pr = Math.Round(utils.Price(), 7);
        }
    }
}
