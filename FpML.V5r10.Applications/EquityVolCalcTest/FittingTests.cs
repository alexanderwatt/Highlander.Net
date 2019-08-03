using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.EquityVolatilityCalculator;
using FpML.V5r10.EquityVolatilityCalculator.Helpers;
using Highlander.Numerics.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.EquitiesVolCalc.TestData;
using ForwardExpiry = FpML.V5r10.EquityVolatilityCalculator.ForwardExpiry;
using Stock = FpML.V5r10.EquityVolatilityCalculator.Stock;
using Strike = FpML.V5r10.EquityVolatilityCalculator.Strike;

namespace FpML.V5r10.EquitiesVolCalcTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class FittingTests
    {
        [TestMethod]
        public void WingCalibration()
        {           
            var stock = TestDataHelper.GetStock("AGK");
            Assert.AreEqual("AGK", stock.Name);
            var stockObject = TestHelper.CreateStock(stock);
            stockObject.CalcForwards();
            if (stockObject.VolatilitySurface.Expiries[0].Strikes[0].InterpModel.GetType() == typeof(WingInterp))
            {
                stockObject.VolatilitySurface.Calibrate();
                var exp0 = new List<DateTime>();
                var strike0 = new List<double>();
                exp0.Add(new DateTime(2009, 09, 16));
                strike0.Add(810);
                List<ForwardExpiry> expiry = stockObject.VolatilitySurface.ValueAt(stockObject, exp0, strike0, false);
                OrcWingParameters wing = new OrcWingParameters();
                ForwardExpiry exp = stockObject.VolatilitySurface.Expiries[0];
                Strike str = exp.Strikes[0];                               
                wing.AtmForward = str.InterpModel.WingParams.AtmForward;
                wing.CallCurve = str.InterpModel.WingParams.CallCurve;
                wing.CurrentVol = str.InterpModel.WingParams.CurrentVol;
                double moneyness = Convert.ToDouble(exp.FwdPrice / stockObject.Spot);
                wing.DnCutoff = str.InterpModel.WingParams.DnCutoff;
                wing.Dsr = str.InterpModel.WingParams.Dsr;
                wing.PutCurve = str.InterpModel.WingParams.PutCurve;
                wing.RefFwd = str.InterpModel.WingParams.RefFwd;
                wing.RefVol = str.InterpModel.WingParams.CurrentVol;
                wing.Scr = str.InterpModel.WingParams.Scr;
                wing.SlopeRef = str.InterpModel.WingParams.SlopeRef;
                wing.Ssr = str.InterpModel.WingParams.Ssr;
                wing.UpCutoff = str.InterpModel.WingParams.UpCutoff;
                wing.Usr = str.InterpModel.WingParams.Usr;
                wing.Vcr = str.InterpModel.WingParams.Vcr;
                var forwardexpiry = stockObject.VolatilitySurface.ValueAt(stockObject, new DateTime(2009, 09, 16), strike0, wing, false, false);
                decimal calibrateBack = Math.Round(expiry[0].Strikes[0].Volatility.Value, 4);
                decimal ovridePoint = Math.Round(forwardexpiry.Strikes[0].Volatility.Value, 4);
                // Can we use ValueAt to value back at our calibrated point (x=7/365,y=8.66);
                Assert.AreEqual(0.59500M, calibrateBack);
                // Can we override with fitted model and arrive back at calibrated point (x=7/365,y=8.66);
                Assert.AreEqual(ovridePoint, 0.5950M);
            }
        }

        /// <summary>
        /// Tests the fitter.
        /// </summary>
        [TestMethod]
        public void TestFitter()
        {
            var stock = TestDataHelper.GetStock("AGK");
            Assert.AreEqual("AGK", stock.Name);
            Stock stockObject = TestHelper.CreateStock(stock);
            stockObject.VolatilitySurface.Calibrate();
            decimal spot = stockObject.Spot;
            var fwdexp = ForwardExpiryHelper.FindExpiry(new DateTime(2016, 9, 9), stockObject.VolatilitySurface.NodalExpiries.ToList());
            int i = -1;
            var str = StrikeHelper.FindStrikeWithPrice(1404, new List<Strike>(fwdexp.Strikes), out i);
            Assert.AreNotEqual(i, -1);
            double fwd = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.AtmForward, 7);
            double cc = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.CallCurve, 7);
            double vc = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.CurrentVol, 7);
            double dc = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.DnCutoff, 7);
            double dsr = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.Dsr, 7);
            double pc = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.PutCurve, 7);
            double reffwd = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.RefFwd, 7);
            double refvol = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.RefVol, 7);
            double scr = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.Scr, 7);
            double sr = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.SlopeRef, 7);
            double ssr = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.Ssr, 7);
            double uc = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.UpCutoff, 7);
            double usr = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.Usr, 7);
            double vcr = Math.Round(fwdexp.Strikes[i].InterpModel.WingParams.Vcr, 7);
            Assert.AreNotEqual(spot, 0);
            Assert.AreEqual(fwd, 1682);
            Assert.AreEqual(cc, 0.0705147);
            Assert.AreEqual(vc, 0.2933592);
            Assert.AreEqual(dc, Math.Round(-0.5 - Math.Log(Convert.ToDouble(fwdexp.FwdPrice / spot))), 7);
            Assert.AreEqual(uc, Math.Round(0.5 - Math.Log(Convert.ToDouble(fwdexp.FwdPrice / spot))), 7);
            Assert.AreEqual(dsr, 0.20);
            Assert.AreEqual(pc, 0.0025281);
            Assert.AreEqual(reffwd, 1682);
            Assert.AreEqual(refvol, vc);
            Assert.AreEqual(scr, 0);
            Assert.AreEqual(sr, -0.0565697);
            Assert.AreEqual(ssr, 100);
            Assert.AreEqual(usr, 0.20);
            Assert.AreEqual(vcr, 0.0);
        }

        /// <summary>
        /// Tests the fitter.
        /// </summary>
        private void CalibrateToFoCurve(double fwd1, double cc1, double vc1, double dc1, double dsr1, double pc1, double scr1, double sr1, double ssr1, double uc1, double usr1, double vcr1,
                                         double fwd2, double cc2, double vc2, double dc2, double dsr2, double pc2, double scr2, double sr2, double ssr2, double uc2, double usr2, double vcr2,
                                         DateTime exp1, DateTime exp2)
        {
            var stock = TestDataHelper.GetStock("AGK");
            Assert.AreEqual("AGK", stock.Name);
            var stockObject = TestHelper.CreateStock(stock);
            ForwardExpiry expiry1 = new ForwardExpiry(exp1, Convert.ToDecimal(fwd1));
            ForwardExpiry expiry2 = new ForwardExpiry(exp2, Convert.ToDecimal(fwd2));
            double[] sdStrikeMults = {     0.6,
                                                        0.7,
                                                        0.75,
                                                        0.8,
                                                        0.85,
                                                        0.87,
                                                        0.9,
                                                        0.92,
                                                        0.94,
                                                        0.95,
                                                        0.96,
                                                        0.98,
                                                        1,
                                                        1.02,
                                                        1.04,
                                                        1.05,
                                                        1.06,
                                                        1.08,
                                                        1.1,
                                                        1.12,
                                                        1.15,
                                                        1.2,
                                                        1.25,
                                                        1.3,
                                                        1.4,
                                                        1.45,
                                                        1.5,
                                                        1.55,
                                                        1.6,
                                                        1.65,
                                                        1.7,
                                                        1.75,
                                                        1.8,
                                                        1.85,
                                                        1.9,
                                                        1.95,
                                                        2 };

            OrcWingParameters parms1 = new OrcWingParameters
            {
                AtmForward = fwd1,
                CallCurve = cc1,
                CurrentVol = vc1,
                DnCutoff = dc1,
                Dsr = dsr1,
                PutCurve = pc1,
                RefFwd = fwd1,
                RefVol = vc1,
                Scr = scr1,
                SlopeRef = sr1,
                Ssr = ssr1,
                UpCutoff = uc1,
                Usr = usr1,
                Vcr = vcr1
            };
            OrcWingParameters parms2 = new OrcWingParameters
            {
                AtmForward = fwd2,
                CallCurve = cc2,
                CurrentVol = vc2,
                DnCutoff = dc2,
                Dsr = dsr2,
                PutCurve = pc2,
                RefFwd = fwd2,
                RefVol = vc1,
                Scr = scr2,
                SlopeRef = sr2,
                Ssr = ssr2,
                UpCutoff = uc2,
                Usr = usr2,
                Vcr = vcr2
            };
            //Override
            double spot = Convert.ToDouble(stockObject.Spot);
            List<double> strikes = new List<double>();
            foreach (double mul in sdStrikeMults)
            {
                Strike str0 = new Strike(mul * spot, null, null, Units.Cents);
                strikes.Add(str0.StrikePrice);
            }
            ForwardExpiry FOexpiry1 = stockObject.VolatilitySurface.ValueAt(stockObject, exp1, strikes, parms1, true, false);
            ForwardExpiry FOexpiry2 = stockObject.VolatilitySurface.ValueAt(stockObject, exp2, strikes, parms2, true, false);
            stockObject.VolatilitySurface.AddExpiry(FOexpiry1);
            stockObject.VolatilitySurface.AddExpiry(FOexpiry2);
            stockObject.VolatilitySurface.Calibrate();
            // Then  ValueAt() on an arbitrary grid now you have calibrated to your FO vols 
        }

        //[TestMethod]
        //public void CalibrateAndOverride()
        //{
        //    var fwdExp = CalibrateAndOverride(1351, 1.125, 0.25, -0.4, 0.5, 0.6, 0, -0.375, 100, 0.225, 0.5, 0,
        //                         1354, 1.125, 0.25, -0.4, 0.5, 0.6, 0, -0.375, 100, 0.225, 0.5, 0,
        //                         new DateTime(2009, 09, 16),
        //                         new DateTime(2009, 10, 09),
        //                         new DateTime(2009, 9, 30),
        //                         2000);

        //    double vol = Convert.ToDouble(fwdExp[0].Strikes[0].Volatility.Value);
        //    Assert.AreEqual(vol, 0.5000, 0.01);
        //}
    }
}
