using System;
using System.Collections.Generic;
using FpML.V5r10.EquityVolatilityCalculator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FpML.V5r10.EquitiesVolCalcTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class CalibrateToFoWing
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
            Stock stock = new Stock(new DateTime(2009, 9, 9), 3769, "BHP", "BHP", rateCurve, divCurve);         
            //create SD grid vols
            stock.VolatilitySurface = CreateSurface(stock);     
          //  stock.VolatilitySurface.AddExpiry(expiryFo1);
          //  stock.VolatilitySurface.AddExpiry(expiryFo2);
            stock.VolatilitySurface.Calibrate();
            List<double> strikes = new List<double> { (double)stock.VolatilitySurface.Expiries[0].FwdPrice };
            List<DateTime> dates = new List<DateTime> { new DateTime(2014,09,25) };
            var result = stock.VolatilitySurface.ValueAt(stock, dates, strikes, false);
            decimal volatility = result[0].Strikes[0].Volatility.Value;
            double cc = result[0].Strikes[0].InterpModel.WingParams.CallCurve;
            Assert.AreNotEqual(volatility, 0);
            Assert.AreEqual(0.013778, cc, 0.0001);
        }     

        private static List<Dividend> CreateDividends()
        {
            return new List<Dividend>();
        }

        private static RateCurve CreateRateCurve()
        {
            DateTime[] dates = { new DateTime(2009, 9, 10), new DateTime(2009, 10, 10), new DateTime(2009, 12, 10), new DateTime(2010, 3, 10), new DateTime(2010, 9, 10), new DateTime(2015, 3, 10) };
            double[] rates = { 0.033309, 0.0333309, 0.033309, 0.033309, 0.033309, 0.033309 };
            return new RateCurve("AUD", "Continuous", new DateTime(2009, 9, 9), dates, rates);
        }

        private static VolatilitySurface CreateSurface(Stock stock)
        {
            VolatilitySurface surface = new VolatilitySurface(stock.AssetId, stock.Spot, stock.Date);
            List<DateTime> dates = new List<DateTime>
                                   {
                                       new DateTime(2014, 9, 9),
                                       new DateTime(2016, 9, 9)                                    
                                   };
            IList<double> strikes = new List<double> {  2261,
                                                        2638,
                                                        2827,
                                                        3015,
                                                        3204,
                                                        3298,
                                                        3392,
                                                        3467,
                                                        3543,
                                                        3581,
                                                        3618,
                                                        3694,
                                                        3769,
                                                        3844,
                                                        3920,
                                                        3957,
                                                        3995,
                                                        4071,
                                                        4146,
                                                        4240,
                                                        4334,
                                                        4523,
                                                        4711,
                                                        4900,
                                                        5277,
                                                        5465,
                                                        5654,
                                                        5842,
                                                        6030,
                                                        6219,
                                                        6407,
                                                        6596,
                                                        6784,
                                                        6973,
                                                        7161,
                                                        7350,
                                                        7538
                                                         };

            IList<double> vols1 = new List<double> {   0.4162,
                                                        0.4094,
                                                        0.4062,
                                                        0.4031,
                                                        0.4,
                                                        0.3985,
                                                        0.3972,
                                                        0.3961,
                                                        0.3949,
                                                        0.3944,
                                                        0.3938,
                                                        0.3925,
                                                        0.3912,
                                                        0.3899,
                                                        0.3886,
                                                        0.388,
                                                        0.3874,
                                                        0.3862,
                                                        0.385,
                                                        0.3835,
                                                        0.382,
                                                        0.3792,
                                                        0.3764,
                                                        0.3738,
                                                        0.3688,
                                                        0.3665,
                                                        0.3643,
                                                        0.3622,
                                                        0.3602,
                                                        0.3584,
                                                        0.3566,
                                                        0.3549,
                                                        0.3533,
                                                        0.3518,
                                                        0.3503,
                                                        0.3489,
                                                        0.3476
                                                         };

            IList<double> vols2 = new List<double> {   0.4085,
                                                        0.4021,
                                                        0.399,
                                                        0.3961,
                                                        0.3932,
                                                        0.3918,
                                                        0.3906,
                                                        0.3897,
                                                        0.3888,
                                                        0.3883,
                                                        0.3878,
                                                        0.3866,
                                                        0.3853,
                                                        0.384,
                                                        0.3828,
                                                        0.3822,
                                                        0.3816,
                                                        0.3803,
                                                        0.3792,
                                                        0.3777,
                                                        0.3763,
                                                        0.3734,
                                                        0.3707,
                                                        0.3681,
                                                        0.3631,
                                                        0.3608,
                                                        0.3585,
                                                        0.3564,
                                                        0.3544,
                                                        0.3525,
                                                        0.3506,
                                                        0.3489,
                                                        0.3472,
                                                        0.3456,
                                                        0.344,
                                                        0.3425,
                                                        0.3411
                                                         };          

            
           var expiry1 = new ForwardExpiry
           {
                FwdPrice = (decimal)stock.GetForward(stock.Date, dates[0]),
                ExpiryDate = dates[0]
           };
           var expiry2 = new ForwardExpiry
           {
                FwdPrice = (decimal)stock.GetForward(stock.Date, dates[1]),
                ExpiryDate = dates[1]
           };         
            for (int idx=0; idx<strikes.Count;idx++)
            {
                Strike str = new Strike(strikes[idx], null, null, Units.Cents);
                IVolatilityPoint vp = new VolatilityPoint();
                vp.SetVolatility(Convert.ToDecimal(vols1[idx]), VolatilityState.Default());
                str.SetVolatility(vp);
                expiry1.AddStrike(str,true);
            }
            surface.AddExpiry(expiry1);
            for (int idx = 0; idx < strikes.Count; idx++)
            {
                Strike str = new Strike(strikes[idx], null, null, Units.Cents);
                IVolatilityPoint vp = new VolatilityPoint();
                vp.SetVolatility(Convert.ToDecimal(vols2[idx]), VolatilityState.Default());
                str.SetVolatility(vp);
                expiry2.AddStrike(str, true);
            }
            surface.AddExpiry(expiry2);
           return surface;
        }      
    }
}
