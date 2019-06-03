using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Helpers;
using Orion.Equity.VolatilityCalculator;

namespace Orion.EquitiesVolCalc.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class MiscTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO: Add test logic	here
            //
        }
            
        [TestMethod]
        public void CanGetVolatilityFromWingParameterSurface()
        {

            RateCurve rateCurve = CreateRateCurve();
            List<Dividend> divCurve = CreateDividends();
            Stock stock = new Stock(new DateTime(2009, 9, 9), 4665, "CBA", "CBA", rateCurve, divCurve);
            //create SD grid vols
            stock.VolatilitySurface = CreateSurface(stock);
            if (stock.VolatilitySurface.Expiries[0].Strikes[0].InterpModel.GetType() == typeof(WingInterp))
            {
                //FO Wing parameters
                var expiryDate1 = new DateTime(2009, 10, 05);
                var atmForward1 = stock.GetForward(new DateTime(2009, 9, 9), expiryDate1);
                var wingParams1 = new OrcWingParameters
                {
                    CurrentVol = 0.25,
                    RefVol = 0.25,
                    SlopeRef = -0.375,
                    PutCurve = 0.6,
                    CallCurve = 1.125,
                    DnCutoff = -0.4,
                    UpCutoff = 0.225,
                    RefFwd = atmForward1,
                    AtmForward = atmForward1,
                    Vcr = 0,
                    Scr = 0,
                    Ssr = 100,
                    Dsr = 0.5,
                    Usr = 0.5,
                };
                var expiryDate2 = new DateTime(2009, 11, 26);
                var atmForward2 = stock.GetForward(new DateTime(2009, 9, 9), expiryDate2);
                var wingParams2 = new OrcWingParameters
                {
                    CurrentVol = 0.275,
                    RefVol = 0.275,
                    SlopeRef = -0.275,
                    PutCurve = 0.325,
                    CallCurve = 0.9,
                    DnCutoff = -0.4,
                    UpCutoff = 0.225,
                    RefFwd = atmForward2,
                    AtmForward = atmForward2,
                    Vcr = 0,
                    Scr = 0,
                    Ssr = 100,
                    Dsr = 0.5,
                    Usr = 0.5,
                };
                List<double> strikes1 = new List<double> { Math.Round(atmForward1 * 0.5, 2), Math.Round(atmForward1 * 0.9, 2), Math.Round(atmForward1, 2), Math.Round(atmForward1 * 1.1, 2), Math.Round(atmForward1 * 1.5, 2) };
                var expiryFo1 = stock.VolatilitySurface.ValueAt(stock, expiryDate1, strikes1, wingParams1, true, true);
                List<double> strikes2 = new List<double> { Math.Round(atmForward2 * 0.5, 2), Math.Round(atmForward2 * 0.9, 2), Math.Round(atmForward2, 2), Math.Round(atmForward2 * 1.1, 2), Math.Round(atmForward2 * 1.5, 2) };
                var expiryFo2 = stock.VolatilitySurface.ValueAt(stock, expiryDate2, strikes2, wingParams2, true, true);
                //  stock.VolatilitySurface.AddExpiry(expiryFo1);
                //  stock.VolatilitySurface.AddExpiry(expiryFo2);
                stock.VolatilitySurface.Calibrate();
                List<double> strikes = new List<double> { (double)stock.VolatilitySurface.Expiries[0].FwdPrice };
                List<DateTime> dates = new List<DateTime> { new DateTime(2013, 10, 7) };
                var result = stock.VolatilitySurface.ValueAt(stock, dates, strikes, false);
                decimal volatility = result[0].Strikes[0].Volatility.Value;
                Assert.AreNotEqual(volatility, 0);
            }
        }

        private static List<Dividend> CreateDividends()
        {
            return new List<Dividend>
                   {
                       new Dividend
                           {
                               Amount = 115,
                               ExDate = new DateTime(2009, 8, 17),
                               PriceUnits = Units.Cents
                           },
                       new Dividend
                           {
                               Amount = 100,
                               ExDate = new DateTime(2010, 2, 16),
                               PriceUnits = Units.Cents
                           },
                       new Dividend
                           {
                               Amount = 100,
                               ExDate = new DateTime(2010, 8, 17),
                               PriceUnits = Units.Cents
                           },
                   };
        }

        private static RateCurve CreateRateCurve()
        {
            DateTime[] dates = { new DateTime(2009, 9, 10), new DateTime(2009, 10, 10), new DateTime(2009, 12, 10), new DateTime(2010, 3, 10), new DateTime(2010, 9, 10), new DateTime(2011, 3, 10) };
            double[] rates = { 0.03, 0.035, 0.04, 0.04, 0.045, 0.05 };
            return new RateCurve("AUD", "Semi-Annual", new DateTime(2009, 9, 9), dates, rates);
        }

        private static VolatilitySurface CreateSurface(Stock stock)
        {
            VolatilitySurface surface = new VolatilitySurface(stock.AssetId, stock.Spot, stock.Date);
            List<DateTime> dates = new List<DateTime>
                                   {
                                       new DateTime(2009, 9, 16),
                                       new DateTime(2009, 10, 05),
                                       new DateTime(2009, 10, 10),
                                       new DateTime(2009, 11, 9),
                                       new DateTime(2009, 11, 26),
                                       new DateTime(2009, 12, 9),
                                       new DateTime(2010, 3, 10),
                                       new DateTime(2010, 6, 9),
                                       new DateTime(2010, 9, 8),
                                       new DateTime(2011, 3, 12),
                                       new DateTime(2011, 9, 7),
                                       new DateTime(2012, 9, 9),
                                       new DateTime(2013, 9, 9),
                                      new DateTime(2014, 9, 9),                                 
                                   };
            IList<double> moneynesses = new List<double> { 0.5, 0.9, 1, 1.1, 1.5 };
            foreach (DateTime date in dates)
            {
                var expiry = new ForwardExpiry
                {
                    FwdPrice = (decimal)stock.GetForward(stock.Date, date),
                    ExpiryDate = date
                };
                foreach (double moneyness in moneynesses)
                {
                    Strike str = new Strike(Math.Round(Convert.ToDouble(expiry.FwdPrice) * moneyness,2) , null, null, Units.Cents);
                    IVolatilityPoint vp = new VolatilityPoint();
                    vp.SetVolatility(Convert.ToDecimal(Math.Exp(moneyness)/3*0.32), VolatilityState.Default());
                    str.SetVolatility(vp);
                    expiry.AddStrike(str,true);
                }
                surface.AddExpiry(expiry);
            }
            return surface;
        }      
    }
}
