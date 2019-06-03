using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Counterparty;
using Orion.Analytics.Equities;
using Orion.Analytics.Helpers;
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Tests.Equities
{
    [TestClass]
    public class EquityPCETests
    {
        [TestMethod]
        public void TestConvergenceAllParams()
        {
            TestDistroConvergence(0.5, 0.60, Math.Log(2000), 20000);  // Low kappa(speed), Low theta(reversion level)
            TestDistroConvergence(100, 0.60, Math.Log(4200) , 20000); // High kappa(speed)
            TestDistroConvergence(10, 0.60, Math.Log(6000), 20000); // Med kappa(speed) , Hgih theta(reversion level)            
            TestDistroConvergence(10, 0.05, Math.Log(4000) , 20000); // Low vol
            TestDistroConvergence(10, 1.20, Math.Log(4000) , 20000); // High vol
            TestDistroConvergence(0.9, 0.20,Math.Log(4200), 20000);
        }

        /// <summary>
        /// Before calculating any PCE numbers on the simulation set, first check the simulated asset values
        /// at the time slices and compare to the expected values of the theoretical distribution.
        /// Check we're within 3 standard errors, reason for 3 being that there is a discretisation error introduced
        /// by the time scheme approximation as well as sample error introduced by the simulation sample mean.
        /// </summary>
        /// <param name="kappa"></param>
        /// <param name="vol"></param>
        /// <param name="theta"></param>
        /// <param name="simuls"></param>
        public void TestDistroConvergence(double kappa, double vol, double theta, int simuls)
        {
            DateTime today = new DateTime(2010, 10, 22);
            DateTime expiry = new DateTime(2011, 10, 22);
            double maturity = Convert.ToDouble(expiry.Subtract(today).Days) / 365.0;
            double spot = 4200.00;
            double callstrike = 1.3 * spot;
            double putstrike = 1.0 * spot;
            double confidence = 0.95;
            int[] zerodays = { 0, 365 };
            double[] zerorates = { 0.065, 0.065 };
            int[] divdays = { 1, 51, 247 };
            double[] divamts = { 22.5, 50, 50 };
            double fwd = EquityAnalytics.GetForwardCCLin365(spot, maturity, divdays, divamts, zerodays, zerorates);          
            double histvol = vol;
            double[] times = { 128.0/365.0, 250.0/365.0,365.0/365.0 };
            OrcWingParameters owp = new OrcWingParameters() { AtmForward = fwd, CallCurve = 0.1250, CurrentVol = 0.26, DnCutoff = -0.25, Dsr = 0.9, PutCurve = 0.10, RefFwd = fwd, RefVol = 0.26, Scr = 0.0, SlopeRef = -0.1750, Ssr = 100, TimeToMaturity = maturity, UpCutoff = 0.20, Usr = 0.50, Vcr = 0.0 };
            List<OrcWingParameters> owpList = new List<OrcWingParameters> {owp};
            double[,] results = EquityPCEAnalytics.GetCollarPCE("Asset",
                                                                zerodays,
                                                                zerorates,
                                                                divdays,
                                                                divamts,
                                                                owpList,
                                                               spot,
                                                                 callstrike,
                                                                 putstrike,
                                                                 maturity,
                                                                 kappa,
                                                                 theta,
                                                                 histvol,
                                                                  times, confidence, 1.0 / 365.0,  simuls, 3151);
            // Check convergence to mean ln(S_t) ~ N(  theta + exp(-kappa*t)*( ln(S_0) - theta , sigma^2/(2*kappa)*(1-exp(-2*kappa*t) )
            // => E(S_t) = exp(theta + 0.5*sigma^2)
            // 
            double LNmean1 = EquityPCEAnalytics.OUMean(kappa, theta, spot, times[0]);
            double LNmean2 = EquityPCEAnalytics.OUMean(kappa, theta, spot, times[1]);
            double LNmean3 = EquityPCEAnalytics.OUMean(kappa, theta, spot, times[2]);
            double Var_1 = EquityPCEAnalytics.OUVar(histvol, kappa, times[0]);         
            double Var_2 = EquityPCEAnalytics.OUVar(histvol, kappa, times[1]);
            double Var_3 = EquityPCEAnalytics.OUVar(histvol, kappa, times[2]);       
            //lognormal moments
            double lhs1 = Math.Exp(LNmean1 + Var_1/2.0);
            double lhs2 = Math.Exp(LNmean2 + Var_2/2.0);
            double lhs3 = Math.Exp(LNmean3 + Var_3/2.0);
            //double deltat = 1.0/365.0;        
            double logvar1 = EquityPCEAnalytics.LNOUVar(spot, histvol, kappa, theta, times[0]);
            double logvar2 = EquityPCEAnalytics.LNOUVar(spot, histvol, kappa, theta, times[1]);
            double logvar3 = EquityPCEAnalytics.LNOUVar(spot, histvol, kappa, theta, times[2]);
            double stderr1 = Math.Sqrt(logvar1 / simuls);
            double stderr2 = Math.Sqrt(logvar2 / simuls);
            double stderr3 = Math.Sqrt(logvar3 / simuls);
            //double accuracy = histvol*histvol*(1/(2*kappa) - deltat /(1-Math.Exp(-2*kappa*deltat)))*(1-Math.Exp(-2*kappa*times[0]));          
            Assert.AreEqual(lhs1, results[0, 0], 3.0 * stderr1);
            Assert.AreEqual(lhs2, results[1, 0], 3.0 * stderr2);
            Assert.AreEqual(lhs3, results[2, 0], 3.0 * stderr3);                
        }
        /// <summary>
        /// Test a profile calculation against the PCE at upper confidence levels 
        /// of the stock price. This is a rough comparison and we just want to 
        /// see that the simulation is returning a PCE +/-10% of the proxy 
        /// calculation. See Equity PCE Incubator 4.0.doc for more information.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void TestPCEProfile()
        {
            DateTime today = new DateTime(2010, 10, 22);
            DateTime expiry = new DateTime(2011, 10, 22);
            double maturity = Convert.ToDouble(expiry.Subtract(today).Days) / 365.0;
            double spot = 4200.00;
            int simuls = 10000;
            double callstrike = 1.3 * spot;
            double putstrike = 1.0 * spot;
            int[] zerodays = { 0, 365 };
            double r0 = 0.065;
            double confidence = 0.95;
            double[] zerorates = { r0, r0 };
            int[] divdays = { 1, 51, 247 };
            double[] divamts = { 22.5, 50, 50 };
            double fwd = EquityAnalytics.GetForwardCCLin365(spot, maturity, divdays, divamts, zerodays, zerorates);
            double histvol = 0.60;
            double theta = Math.Log(spot);
            double kappa = 0.9;
            double[] times = { 7.0/365.0, 128.0 / 365.0, 250.0 / 365.0, 365.0 / 365.0, 730.0/365.0 };
            OrcWingParameters owp = new OrcWingParameters { AtmForward = fwd, CallCurve = 0.1250, CurrentVol = 0.26, DnCutoff = -0.25, Dsr = 0.9, PutCurve = 0.10, RefFwd = fwd, RefVol = 0.26, Scr = 0.0, SlopeRef = -0.1750, Ssr = 100, TimeToMaturity = maturity, UpCutoff = 0.20, Usr = 0.50, Vcr = 0.0 };
            List<OrcWingParameters> owpList = new List<OrcWingParameters> {owp};
            double[,] results = EquityPCEAnalytics.GetCollarPCE("CollarPCE",
                                                                zerodays,
                                                                zerorates,
                                                                divdays,
                                                                divamts,
                                                                owpList,
                                                                spot,
                                                                 callstrike,
                                                                 putstrike,
                                                                 maturity,
                                                                 kappa,
                                                                 theta,
                                                                 histvol,
                                                                 times,
                                                                 confidence,
                                                                 0.25/365.0,
                                                                 simuls,
                                                                 3151);
            double lhs1 = PCEProxyCalc(spot, kappa, theta, histvol, times[0], maturity, callstrike, putstrike, zerodays, zerorates, divdays, divamts, owpList);
            double lhs2 = PCEProxyCalc(spot, kappa, theta, histvol, times[1], maturity, callstrike, putstrike, zerodays, zerorates, divdays, divamts, owpList);
            double lhs3 = PCEProxyCalc(spot, kappa, theta, histvol, times[2], maturity, callstrike, putstrike, zerodays, zerorates, divdays, divamts, owpList);
            Assert.AreEqual(results[0, 1], lhs1, Math.Abs(0.10 * results[0, 1]));
            Assert.AreEqual(results[1, 1], lhs2, Math.Abs(0.10 * results[1, 1]));
            Assert.AreEqual(results[2, 1], lhs3, Math.Abs(0.10 * results[2, 1]));
        }

        /// <summary>
        /// We can roughly proxy the alpha-th percentile of the simulation 
        /// by calculating the upper alpha percentile confidence point for a given time slice
        /// of the ln-OU process and pricing the collar with this asset value for a given profile point
        /// This should roughly compare to the calculated (1-alpha)-worst PCE point.
        /// </summary>
        /// <param name="spot"></param>
        /// <param name="kappa"></param>
        /// <param name="theta"></param>
        /// <param name="histvol"></param>
        /// <param name="time0"></param>
        /// <param name="maturity"></param>
        /// <param name="callstrike"></param>
        /// <param name="putstrike"></param>
        /// <param name="zerodays"></param>
        /// <param name="zerorates"></param>
        /// <param name="divdays"></param>
        /// <param name="divamts"></param>
        /// <param name="volSurface"></param>
        /// <returns></returns>
        public static double PCEProxyCalc(double spot,
                                          double kappa,
                                          double theta,
                                          double histvol,
                                          double time0,
                                          double maturity,
                                          double callstrike,
                                          double putstrike,
                                          int[] zerodays,
                                          double[] zerorates,
                                          int[] divdays,
                                          double[] divamts,                                                                      
                                          List<OrcWingParameters> volSurface )
        {
            double tau = maturity - time0;
            double upperBound = EquityPCEAnalytics.LNOUUpperBound(spot, 0.95, kappa, theta, histvol, time0);
            double fStar = EquityAnalytics.GetForwardCCLin365(upperBound, time0, maturity, divdays, divamts, zerodays, zerorates);
            double callvol = EquityAnalytics.GetWingValue(volSurface, new LinearInterpolation(), tau, callstrike);
            double putvol = EquityAnalytics.GetWingValue(volSurface, new LinearInterpolation(), tau, putstrike);
            double r0 = EquityAnalytics.GetRateCCLin365(time0, maturity, zerodays, zerorates);
            BlackScholes bs = new BlackScholes();
            double lhs = Math.Max(bs.BSprice(fStar, tau, callstrike, r0, callvol, true) - bs.BSprice(fStar, tau, putstrike, r0, putvol, false),0);
            return lhs;
        }       
    }
}
