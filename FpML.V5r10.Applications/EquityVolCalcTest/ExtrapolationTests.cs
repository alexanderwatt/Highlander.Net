using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.EquitiesVolCalc.TestData;
using Orion.Equity.VolatilityCalculator;
using Orion.Equity.VolatilityCalculator.Helpers;
using ForwardExpiry = Orion.Equity.VolatilityCalculator.ForwardExpiry;
using OptionPosition = Orion.Equity.VolatilityCalculator.OptionPosition;
using Stock = Orion.Equity.VolatilityCalculator.Stock;
using Strike = Orion.Equity.VolatilityCalculator.Strike;
using VolatilityPoint = Orion.Equity.VolatilityCalculator.VolatilityPoint;
using VolatilitySurface = Orion.Equity.VolatilityCalculator.VolatilitySurface;

namespace Orion.EquitiesVolCalc.Tests
{
    /// <summary>
    /// Summary description for ExtrapolationTests
    /// </summary>
    [TestClass]
    public class ExtrapolationTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestGetVol()
        {
            IStock stockASXParent = LoadStock("AGK");           
            ExtrapolationHelper extrapHelper = new ExtrapolationHelper();
            double vol0 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009,9,16), 0.05);
            double vol1 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009,9,16), 0.60);
            double vol2 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009, 9, 16), 0.80);
            double vol3 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009, 9, 16), 1.00);
            double vol4 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009, 9, 16), 2.00);
            double vol5 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009, 9, 16), 3.00);
            //Extrap below
            Assert.AreEqual(0.6413, vol0, 0.001);
            Assert.AreEqual(0.5950, vol1,0.001);
            Assert.AreEqual(0.3545, vol2,0.001);
            Assert.AreEqual(0.2962, vol3,0.001);
            Assert.AreEqual(0.3765, vol4,0.001);
            //Extrap above
            Assert.AreEqual(0.3765, vol5, 0.001);
        }

        /// <summary>
        /// Test various strikes at a given non-nodal expiry to check interpolation
        /// in both dimensions is functioning correctly
        /// </summary>
        [TestMethod]
        public void TestGetVolWithTimeInterp()
        {         
            IStock stockASXParent = LoadStock("AGK");
            ExtrapolationHelper extrapHelper = new ExtrapolationHelper();
            double vol0 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009, 9, 30), 0.05);
            double vol1 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009, 9, 30), 1.00);
            double vol2 = extrapHelper.GetVolAt(stockASXParent, new DateTime(2009, 9, 30), 2.00);
            //Extrap below
            Assert.AreEqual(0.592322, vol0, 0.001);
            Assert.AreEqual(0.290393, vol1, 0.001);
            Assert.AreEqual(0.360499, vol2, 0.001);
        }

        /// <summary>
        /// At a given nodal expiry check volatility surface fitting and scaling is 
        /// functioning as expected.
        /// </summary>
        [TestMethod]
        public void TestExtrapolate1()
        {            
            IStock stockASXParent = LoadStock("AGK");
            IStock stockASXChild = LoadStock("ANZ");
            IStock stockSDParent = LoadStock("AGK");
            IVolatilitySurface child = CreateTestVolSurface();
            ExtrapolationHelper extrapHelper = new ExtrapolationHelper();            
            if (stockASXParent.VolatilitySurface.Expiries[0].Strikes[0].InterpModel.GetType() == typeof(WingInterp))
            {
                extrapHelper.DoExtrap(stockASXParent,
                                      stockASXChild,
                                      stockSDParent,
                                      child);
                Assert.AreEqual(Convert.ToDouble(child.NodalExpiries[0].Strikes[0].Volatility.Value), 0.295126142, 0.0001);
            }        
        }

        /// <summary>
        /// Load in three surfaces.
        /// Check extrapolation calc at a time point inbetween two maturities, at moneyness = 0.3,1,1.2
        /// Checks interpolation between maturities, Wing calibration in strike, (in and outside of fit domain) 
        /// and resulting scaled curve is performing as expected
        /// 
        /// </summary>
        [TestMethod]
        public void TestExtrapolate2()
        {        
            IStock stockASXParent = LoadStock("AGK");
            IStock stockASXChild = LoadStock("ANZ");
            IStock stockSDParent = LoadStock("BHP");
            IVolatilitySurface child = CreateNullVolSurface();
            ExtrapolationHelper extrapHelper = new ExtrapolationHelper();
            double factor = 0.314843 / 0.290393;
            double parentVol1 = 0.394634;         
            double scal = extrapHelper.CalcExtrapFactor(stockASXParent, stockASXChild, new DateTime(2009, 9, 30));
            Assert.AreEqual(scal, factor, 0.001);
            double parentVol2 = 0.38754555;
            double parentVol3 = 0.71063645;
            if (stockASXParent.VolatilitySurface.Expiries[0].Strikes[0].InterpModel.GetType() == typeof(WingInterp))
            {
                extrapHelper.DoExtrap(stockASXParent,
                                      stockASXChild,
                                      stockSDParent,
                                      child);
                //moneyness = 0.3;
                Assert.AreEqual(Convert.ToDouble(child.NodalExpiries[2].Strikes[0].Volatility.Value), factor * parentVol3, 0.001);
                //moneyness = 1.0;
                Assert.AreEqual(Convert.ToDouble(child.NodalExpiries[2].Strikes[1].Volatility.Value), factor * parentVol1, 0.001);
                //moneyness = 1.2;
                Assert.AreEqual(Convert.ToDouble(child.NodalExpiries[2].Strikes[2].Volatility.Value), factor * parentVol2, 0.001);
            }
        }

        /// <summary>
        /// This tests the extrapolation of the temporal interpolation to a point 2009/9/10 outside of the
        /// SD nodal range. Also tests the application of extrapolation to a single point child surface.
        /// </summary>
        [TestMethod]
        public void TestExtrapolate3()
        {          
            IStock stockASXParent = LoadStock("AGK");
            IStock stockASXChild = LoadStock("ANZ");
            IStock stockSDParent = LoadStock("BHP");
            IVolatilitySurface child = CreateOnePointChild();
            ExtrapolationHelper extrapHelper = new ExtrapolationHelper();
            double scal = extrapHelper.CalcExtrapFactor(stockASXParent, stockASXChild, new DateTime(2009, 9, 10));
            Assert.AreEqual(scal, 1.05239899198348, 0.001);//OLD 0.314301/0.298668 //1.06102304848855
            extrapHelper.DoExtrap(stockASXParent, stockASXChild, stockSDParent, child);
            Assert.AreEqual(Convert.ToDouble(child.Expiries[0].Strikes[0].Volatility.Value), 0.997545918986369, 0.001);//0.930043543995147
        }

        /// <summary>
        /// Tests the calculation of the scaling factor, when there are no ETOs in a stock.
        /// Tests application to mapped interpolated points in (strike, time) space on the parent.        
        /// </summary>
        [TestMethod]
        public void TestHistoricalExtrapolate()
        {
            IStock stockASXParent = LoadStock("AGK");
            IVolatilitySurface stockASXChild = CreateNullVolSurface();
            IVolatilitySurface targetChild = CreateNullVolSurface();
            IStock stockSDParent = LoadStock("AGK");
            stockASXParent.Valuations.Add(new Valuation(new DateTime(2008,8,28),1208));
            stockASXParent.Valuations.Add(new Valuation(new DateTime(2008,8,29),1221));
            stockASXParent.Valuations.Add(new Valuation(new DateTime(2008,8,30),1218));
            stockASXParent.Valuations.Add(new Valuation(new DateTime(2008,8,31),1207));
            stockASXParent.Valuations.Add(new Valuation(new DateTime(2008,9,1),1250));                      
            RateCurve rateCurve = CreateRateCurve();
            List<Dividend> divCurve = CreateDividends();
            IStock nullASXChild = new Stock(new DateTime(2009, 9, 9), 200.0M, "ZZZ", "ZZZ", rateCurve, divCurve);
            nullASXChild.VolatilitySurface = stockASXChild;
            nullASXChild.Valuations.Add(new Valuation(new DateTime(2008, 12, 12), 208));
            nullASXChild.Valuations.Add(new Valuation(new DateTime(2008, 12, 13), 221));
            nullASXChild.Valuations.Add(new Valuation(new DateTime(2008, 12, 14), 218));
            nullASXChild.Valuations.Add(new Valuation(new DateTime(2008, 12, 15), 207));
            nullASXChild.Valuations.Add(new Valuation(new DateTime(2008, 12, 16), 201));
            ExtrapolationHelper extrap  = new ExtrapolationHelper();
            decimal histvol1 = extrap.DoHistVolCalc(stockASXParent);
            decimal histvol2 = extrap.DoHistVolCalc(nullASXChild);
            Assert.AreEqual(0.375846, Convert.ToDouble(histvol1), 0.0001);
            Assert.AreEqual(0.770018, Convert.ToDouble(histvol2), 0.0001);
            extrap.PopulateHistoricalVols(stockASXParent, nullASXChild, targetChild);
            double scalFactor = Convert.ToDouble(extrap.GetHistoricalScalFactor(stockASXParent, nullASXChild));
            Assert.AreEqual(scalFactor, 2.0487573, 0.0001);
            //Spreadsheet fit SD parent to (5d, 1.000 * F) point, flatline endpoints.
            decimal SDParentVol0 = 0.296175M;
            // Spreadsheet fit SD parent to (7d,0.867 * F) point
            decimal SDParentVol1 = 0.320240M;
            // Spreadsheet fit SD parent to (21d,1.00 * F) point
            decimal SDParentVol2 = 0.287656M;
            double childExtrapVol0 = scalFactor*Convert.ToDouble(SDParentVol0);
            double childExtrapVol1 = scalFactor*Convert.ToDouble(SDParentVol1);            
            double childExtrapVol2 = scalFactor*Convert.ToDouble(SDParentVol2);
            Assert.AreEqual(Convert.ToDouble(SDParentVol0 * histvol2 / histvol1), childExtrapVol0, 0.001);
            Assert.AreEqual(Convert.ToDouble(SDParentVol1 * histvol2 / histvol1), childExtrapVol1, 0.001);
            Assert.AreEqual(Convert.ToDouble(SDParentVol2 * histvol2 / histvol1), childExtrapVol2, 0.001);
        }

        /// <summary>
        /// Loads the stock.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static IStock LoadStock(String name)
        {
            var stock = TestDataHelper.GetStock(name);
            Assert.AreEqual(name, stock.Name);
            Stock stockObject = TestHelper.CreateStock(stock);
            return stockObject;
        }

        /// <summary>
        /// Creates a test vol surface to test temporal and strike volatility interpolation
        /// </summary>
        /// <returns></returns>
        public IVolatilitySurface CreateTestVolSurface()
        {
            IVolatilitySurface volSurface = new VolatilitySurface("BHP", 4500M, DateTime.Today);
            ForwardExpiry expiry1 = new ForwardExpiry(DateTime.Parse("01-Jan-2010"), 4200);
            ForwardExpiry expiry2 = new ForwardExpiry(DateTime.Parse("01-Jan-2011"), 4400);
            OptionPosition call1 = new OptionPosition("1245", 104, PositionType.Call);
            OptionPosition put1 = new OptionPosition("1246", 1200, PositionType.Put);
            OptionPosition call2 = new OptionPosition("1645", 180, PositionType.Call);
            OptionPosition put2 = new OptionPosition("1646", 1300, PositionType.Put);
            Strike strike1 = new Strike(4200, call1, put1);
            Strike strike2 = new Strike(4000, call2, put2);
            IVolatilityPoint point1 = new VolatilityPoint();
            point1.SetVolatility(0.30M, VolatilityState.Default());
            put1.SetVolatility(point1);
            call1.SetVolatility(point1);
            IVolatilityPoint point2 = new VolatilityPoint();
            point2.SetVolatility(0.40M, VolatilityState.Default());
            call2.SetVolatility(point2);
            put2.SetVolatility(point2);
            expiry1.AddStrike(strike1, true);
            expiry2.AddStrike(strike2, true);
            volSurface.AddExpiry(expiry1);
            volSurface.AddExpiry(expiry2);
            return volSurface;
        }

        /// <summary>
        /// Creates a null volatility surface to be used in extrapolation tests that utilise
        /// the historical volatility ratio
        /// </summary>
        /// <returns></returns>
        public IVolatilitySurface CreateNullVolSurface()
        {
            IVolatilitySurface volSurface = new VolatilitySurface("BHP", 4500M, DateTime.Today);
            ForwardExpiry expiry0 = new ForwardExpiry(DateTime.Parse("14-9-2009"), 4700);
            ForwardExpiry expiry1 = new ForwardExpiry(DateTime.Parse("16-9-2009"), 4700);
            ForwardExpiry expiry2 = new ForwardExpiry(DateTime.Parse("30-9-2009"), 4750);
            OptionPosition call0 = new OptionPosition("1145", 104, PositionType.Call);
            OptionPosition put0 = new OptionPosition("1146", 1200, PositionType.Put);
            OptionPosition call1 = new OptionPosition("1245", 104, PositionType.Call);
            OptionPosition put1 = new OptionPosition("1246", 1200, PositionType.Put);
            OptionPosition call2 = new OptionPosition("1645", 180, PositionType.Call);
            OptionPosition put2 = new OptionPosition("1646", 1300, PositionType.Put);
            Strike strike0 = new Strike(1.00,4599, call0, put0);            
            Strike strike1 = new Strike(0.867,4700, call1, put1);
            Strike strike2 = new Strike(1.00,4750, call2, put2);
            Strike strike3 = new Strike(1.2, 4750, call2, put2);
            Strike strike4 = new Strike(0.30, 4750, call2, put2);
            IVolatilityPoint point0 = new VolatilityPoint();
            point0.SetVolatility(0.00M, VolatilityState.Default());
            put0.SetVolatility(point0);
            call0.SetVolatility(point0);
            strike0.SetVolatility(point0);  
            IVolatilityPoint point1 = new VolatilityPoint();
            point1.SetVolatility(0.00M, VolatilityState.Default());
            put1.SetVolatility(point1);
            call1.SetVolatility(point1);
            strike1.SetVolatility(point1);            
            IVolatilityPoint point2 = new VolatilityPoint();
            point2.SetVolatility(0.00M, VolatilityState.Default());
            strike2.SetVolatility(point2);
            call2.SetVolatility(point2);
            put2.SetVolatility(point2);
            IVolatilityPoint point3 = new VolatilityPoint();
            point3.SetVolatility(0.00M, VolatilityState.Default());
            strike3.SetVolatility(point3);
            IVolatilityPoint point4 = new VolatilityPoint();
            point4.SetVolatility(0.00M, VolatilityState.Default());
            strike4.SetVolatility(point4);                     
            expiry0.AddStrike(strike0, true);
            expiry1.AddStrike(strike1, true);
            expiry2.AddStrike(strike2, true);
            expiry2.AddStrike(strike3, true);
            expiry2.AddStrike(strike4, true);
            volSurface.AddExpiry(expiry0);
            volSurface.AddExpiry(expiry1);
            volSurface.AddExpiry(expiry2);            
            return volSurface;
        }

        /// <summary>
        /// Creates the one point child.
        /// </summary>
        /// <returns></returns>
        public IVolatilitySurface CreateOnePointChild()
        {
            IVolatilitySurface volSurface = new VolatilitySurface("BHP", 4500M, DateTime.Today);
            ForwardExpiry expiry0 = new ForwardExpiry(DateTime.Parse("10-9-2009"), 4700);
            OptionPosition call0 = new OptionPosition("1145", 104, PositionType.Call);
            OptionPosition put0 = new OptionPosition("1146", 1200, PositionType.Put);
            Strike strike0 = new Strike(0.20, 4599, call0, put0);
            expiry0.AddStrike(strike0,true);
            volSurface.AddExpiry(expiry0);
            return volSurface;
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
            DateTime[] dates = new[] { new DateTime(2009, 9, 10), new DateTime(2009, 10, 10), new DateTime(2009, 12, 10), new DateTime(2010, 3, 10), new DateTime(2010, 9, 10), new DateTime(2011, 3, 10) };
            double[] rates = new[] { 0.03, 0.035, 0.04, 0.04, 0.045, 0.05 };

            RateCurve rc =  new RateCurve("AUD", "Semi-Annual", new DateTime(2009, 9, 9), dates, rates);
            return rc;

        }

        [TestMethod]
        public void TestRateCurve()
        {
            DateTime[] dates = { new DateTime(2009, 9, 10), new DateTime(2009, 10, 10), new DateTime(2009, 12, 10), new DateTime(2010, 3, 10), new DateTime(2010, 9, 10), new DateTime(2011, 3, 10) };
            double[] rates = { 0.03, 0.035, 0.04, 0.04, 0.045, 0.05 };
            RateCurve rc0 = new RateCurve("AUD", "Semi-Annual", new DateTime(2009, 9, 9), dates, rates);
            double rf0 = Convert.ToDouble(rc0.ForwardRate(0,365));
            double df0 = Convert.ToDouble(rc0.GetDf(101));                  
            Assert.AreEqual(0.98910057, df0,0.0001);
            Assert.AreEqual(Math.Pow(1 + 0.044973 / 2, 2)-1, rf0,0.0001);
            RateCurve rc1 = new RateCurve("AUD", "Continuous", new DateTime(2009, 9, 9), dates, rates);
            double rf1 = Convert.ToDouble(rc1.ForwardRate(0, 365));
            double df1 = Convert.ToDouble(rc1.GetDf(101));            
            Assert.AreEqual(0.98899254, df1, 0.0001);
            Assert.AreEqual(Math.Exp(0.044973) - 1, rf1, 0.0001);                     
        }

        public decimal GetTailedFutures(int numBasket, int daysToExpiry)
        {
            DateTime[] dates = { new DateTime(2009, 9, 10), new DateTime(2009, 10, 10), new DateTime(2009, 12, 10), new DateTime(2010, 3, 10), new DateTime(2010, 9, 10), new DateTime(2011, 3, 10) };
            double[] rates = { 0.03, 0.035, 0.04, 0.04, 0.045, 0.05 };
            RateCurve rc = new RateCurve("AUD", "Semi-Annual", new DateTime(2009, 9, 9), dates, rates);
            decimal df = rc.GetDf(daysToExpiry);
            return df * Convert.ToDecimal(numBasket);
        }
    }   
}
