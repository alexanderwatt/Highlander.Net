using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Equities;
using Orion.Analytics.Helpers;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Lattices;
using Orion.Analytics.Options;

namespace Orion.Analytics.Tests.Equities
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class EquitiesTests
    {
        double tau;
        double spot;
        double strike ;
        double vol;
        int steps;
        string pay;
        string style ;
        string smoothing ;  
        private int[] rtdays;
        private double[] rtamts;
        private int[] divdays ;
        private double[] dd ;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private void SetUpExampleOption1()
        {
            tau = 1.0;
            spot = 100;
            strike = 100;
            vol = 0.50;
            steps = 120;          
            style = "American";
            smoothing = "N";  
            rtdays = new int[] { 73, 365 };
            rtamts = new double[] { 0.05, 0.05 };
            divdays = new int[] { 0, 182, 365, 438 };
            dd = new double[] { 5, 5, 5, 5 };
        }

        private void SetUpExampleCliquet()
        {         
            tau = 5.498630137;
            spot = 4861;
            strike = 1;
            rtdays = new[] { 1, 2925 };
            rtamts = new[] { 0.065, 0.065 };
            // resetAmts[4] = 6000;
            // resetAmts[5] = 4000;
            // resetAmts[9] = 3500;
            // resetAmts[11] = 4000;
            // resetAmts[12] = 6000;
            divdays = new int[] { 31, 61, 92, 123, 151, 182, 212, 243, 273, 304, 335, 365, 396,
                                          1643, 1673, 1704, 1734, 1765, 1796, 1826, 1857, 1887, 1918,
                                          1949, 1977, 2008};
            dd = new double[] { 12.438273064236,
                                12.0712191050975,
                                12.5112416423502,
                                12.5484879253083,
                                11.3631887529053,
                                12.6196826288268,
                                12.2472752658376,
                                12.6937154381183,
                                12.3191233638255,
                                12.7681825576065,
                                12.8061937601711,
                                12.4282824458834,
                                12.8813207287543,
                                614.170352033592,
                                14.0890707877012,
                                14.6026484654462,
                                14.1717236975551,
                                14.6883142560039,
                                14.732041739222,
                                14.2972985704028,
                                14.8184666096908,
                                14.381173039323,
                                14.9053984878347,
                                14.9497722363035,
                                13.5376536794877,
                                15.0345907904096
                                    };
            vol = 0.60;
        }

        /// <summary>
        /// Tests the binomial relative call.
        /// </summary>
        [TestMethod]
        public void TestBinomialRelativeCall()
        {
            SetUpExampleOption1();
            BinomialTreePricer lhs = new BinomialTreePricer(spot,strike,false,tau,vol,steps,true,style,smoothing,rtdays,rtamts,divdays,dd,typeof(EquityPropDivTree));
            double price = lhs.GetPrice();
            double delta = lhs.GetDelta();
            double gamma = lhs.GetGamma();
            double vega = lhs.GetVega();
            double theta = lhs.GetTheta();
            Assert.AreEqual(19.19592, price, 0.0001);
            Assert.AreEqual(0.59397, delta, 0.0001);
            Assert.AreEqual(0.00832, gamma, 0.0001);
            Assert.AreEqual(0.36613, vega, 0.0001);
            Assert.AreEqual(-0.02997, theta, 0.0001);
        }

        /// <summary>
        /// Tests the binomial relative put.
        /// </summary>
        [TestMethod]
        public void TestBinomialRelativePut()
        {
            SetUpExampleOption1();
            BinomialTreePricer lhs = new BinomialTreePricer(spot, strike, true, tau, vol, steps, true, style, smoothing, rtdays, rtamts, divdays, dd, typeof(EquityPropDivTree));
            double price = lhs.GetPrice();
            double delta = lhs.GetDelta();
            double gamma = lhs.GetGamma();
            double vega = lhs.GetVega();
            double theta = lhs.GetTheta();
            Assert.AreEqual(20.79427, price, 0.0001);
            Assert.AreEqual(-0.4023769, delta, 0.0001);
            Assert.AreEqual(0.00721022, gamma, 0.0001);
            Assert.AreEqual(0.35311, vega, 0.0001);
            Assert.AreEqual(-0.0155038, theta, 0.0001);
        }

        /// <summary>
        /// Bins the relative long dated.
        /// </summary>
        [TestMethod]
        public void BinRelativeLongDated()
        {
            double _spot = 26.4;
            double _strike = 40.0;
            double[] _rt = {  0.0531835066,
                                0.0533209825,
                                0.0505847905,
                                0.0476071704,
                                0.0462601791,
                                0.0413960339,
                                0.0395067537,
                                0.0391989121,
                                0.0397679692,
                                0.0409565679,
                                0.0424276371,
                                0.0436165574,
                                0.0447919141,
                                0.0459378964,
                                0.047129656,
                                0.0483301138,
                                0.0490433005,
                                0.049747259,
                                0.0504720079,
                                0.051218 };
            int[] _rtdays = { 3, 33, 66, 95, 119, 210, 
                            301, 392, 483,
                            574, 665, 733, 825, 914,
                            1006, 1098, 1190,
                            1280, 1372, 1466 };
            int[] _dvdays = { 101, 297, 465, 661, 829, 1025, 1193, 1389 };
            double[] _dd = { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 };
            double _vol = 0.50;
            DateTime _today = new DateTime(2008, 11, 14);
            DateTime _expiry = new DateTime(2012, 05, 17);       
            int _steps = 120;
            string _style = "American";
            double _tau = _expiry.Subtract(_today).Days / 365.0;
            string _smoothing = "N";
            BinomialTreePricer lhs = new BinomialTreePricer(_spot, _strike, false, _tau, _vol, _steps, true, _style, _smoothing, _rtdays, _rt, _dvdays, _dd, typeof(EquityDiscreteDivTree));
            double price = lhs.GetPrice();
            double delta = lhs.GetDelta();
            double gamma = lhs.GetGamma();
            double vega = lhs.GetVega();
            double theta = lhs.GetTheta();
            //Price            
            Assert.AreEqual(7.20339, price, 0.001);
            //Delta
            Assert.AreEqual(0.5685058, delta, 0.001);
            //Gamma
            Assert.AreEqual(0.0162134, gamma, 0.001);
            //Vega
            Assert.AreEqual(0.191464, vega, 0.001);
            //Theta
            Assert.AreEqual(-0.0048253, theta, 0.001);
        }

        /// <summary>
        /// Bins the rel div at expiry.
        /// </summary>
        [TestMethod]
        public void BinRelDivAtExpiry()
        {
            double _spot = 385.5;
            double _strike = 400;
            double[] _rt = {  0.0425,
                                0.0452,
                                0.0457,
                                0.0462 };
            int[] _rtdays = { 1, 32, 63, 93 };
            int[] _dvdays = { 0, 48 };
            double[] _divs = { 20, 10 };
            double _vol = 0.2046;
            DateTime _today = new DateTime(2010, 05, 07);
            DateTime _expiry = new DateTime(2010, 06, 24);
            double _tau = _expiry.Subtract(_today).Days / 365.0;
            int _steps = 120;
            string _style = "American";         
            string _smoothing = "N";
            BinomialTreePricer lhs = new BinomialTreePricer(_spot, _strike, true, _tau, _vol, _steps, true, _style, _smoothing, _rtdays, _rt, _dvdays, _divs, typeof(EquityPropDivTree));
            double price = lhs.GetPrice();
            double delta = lhs.GetDelta();
            double gamma = lhs.GetGamma();
            double vega = lhs.GetVega();
            double theta = lhs.GetTheta();
            //Price
            Assert.AreEqual(25.7028, price, 0.001);
            //Delta
            Assert.AreEqual(-0.74884, delta, 0.001);
            //Gamma
            Assert.AreEqual(0.01041, gamma, 0.001);
            //Vega
            Assert.AreEqual(0.42628, vega, 0.001);
            //Theta
            Assert.AreEqual(-0.0512, theta, 0.001);
        }

        /// <summary>
        /// Discretes the div at expiry.
        /// </summary>
        [TestMethod]
        public void DiscreteDivAtExpiry()
        {
            double _spot = 385.5;
            double _strike = 400;
            double[] _rt = {  0.0425,
                                0.0452,
                                0.0457,
                                0.0462 };
            int[] _rtdays = { 1, 32, 63, 93 };
            int[] _dvdays = { 0, 48 };
            double[] _divs = { 20, 10 };
            double _vol = 0.2046;
            DateTime _today = new DateTime(2010, 05, 07);
            DateTime _expiry = new DateTime(2010, 06, 24);
            double _tau = _expiry.Subtract(_today).Days / 365.0;
            int _steps = 120;
            string _style = "American";
            string _smoothing = "N";
            BinomialTreePricer lhs = new BinomialTreePricer(_spot, _strike, true, _tau, _vol, _steps, true, _style, _smoothing, _rtdays, _rt, _dvdays, _divs, typeof(EquityPropDivTree));
            double price = lhs.GetPrice();
            double delta = lhs.GetDelta();
            double gamma = lhs.GetGamma();
            double vega = lhs.GetVega();
            double theta = lhs.GetTheta();
            //Price
            Assert.AreEqual(25.7028, price, 0.001);
            //Delta
            Assert.AreEqual(-0.74884, delta, 0.001);
            //Gamma
            Assert.AreEqual(0.01041, gamma, 0.001);
            //Vega
            Assert.AreEqual(0.42628, vega, 0.001);
            //Theta
            Assert.AreEqual(-0.0512, theta, 0.001);
        }

        /// <summary>
        /// Tests the asian hybrid cliquet5 Y.
        /// </summary>
        [TestMethod]
        public void TestAsianHybridCliquet5Y()
        {
            // DateTime valueDate = new DateTime(2009, 10, 20);
            //DateTime expiryDate = new DateTime(2015, 04, 19);         
            SetUpExampleCliquet();
            int nobsin = 13;
            int nobsout = 13;
            int numResets = nobsin + nobsout;
            int[] resetDays = new int[] { 0, 31, 61, 92, 123, 151, 182, 212, 243, 273, 304, 335, 365,
                                          1643, 1673, 1704, 1734, 1765, 1796, 1826, 1857, 1887, 1918,
                                          1949, 1977, 2008};
            double[] resetAmts = new double[numResets];
            double[] vols = new double[numResets];
            for (int idx = 0; idx < numResets; idx++)
            {
                resetAmts[idx] = 4861;
                vols[idx] = 0.35;
            }
            // resetAmts[4] = 6000;
            // resetAmts[5] = 4000;
            // resetAmts[9] = 3500;
            // resetAmts[11] = 4000;
            // resetAmts[12] = 6000;
            double floor = 0.0;
            double cap = 0.0;
            int numsimulations = 100000;
            CliquetPricer ah = new CliquetPricer("AsianHybrid", spot, strike, tau, divdays, dd, rtdays, rtamts, resetDays, resetAmts, vols, floor, cap, numsimulations, nobsin, nobsout,3151,true);
            double[,] vpr = ah.GetPriceAndGreeks();
            double stderr1 = vpr[0,1];
            double price = vpr[0, 0];
            Assert.AreEqual(0.2632689, price, 2 * stderr1);
        }

        /// <summary>
        /// Tests the taurus cliquet 1Y.
        /// </summary>
        [TestMethod]
        public void TestTaurusCliquet1Y()
        {
            DateTime valueDate = new DateTime(2009, 08, 28);
            DateTime expiryDate = new DateTime(2009, 12, 15);
            double t = System.Convert.ToDouble(expiryDate.Subtract(valueDate).Days) / 365.0;
            double spot = 4200;
            double strike = 1;
            int[] ratedays = new int[] { 1, 2925 };
            double[] rateamts = new double[] { 0.034932296, 0.0349322696 };
            int[] resetDays = new int[] { -347,-317,-286,-256,-225,-194,-166,-135,-105,-74,-44,-13,18,48,79,109};
            double[] vols = new double[] { 0.60, 0.60, 0.60, 0.60, 0.60, 0.60, 0.60, 0.60, 0.60, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6 };
            double[] resetAmts = new double[] { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 0, 0, 0, 0 };
            int[] divdays = new int[] { 0 };
            double[] divamts = new double[] { 0.0 };
            int numsimulations = 100000;
            int nobsin = 8;
            int nobsout = 8;
            double floor = 0;
            double cap = 0.067;
            CliquetPricer ah = new CliquetPricer("TaurusCliquet", spot, strike, t, divdays, divamts, ratedays, rateamts, resetDays, resetAmts, vols, floor, cap, numsimulations, nobsin, nobsout, 3151,true);
            double[,] vpr = ah.GetPriceAndGreeks();
            double price = vpr[0, 0];
            double stderr1 = vpr[0, 1];
            Assert.AreEqual(0.05207, price, 2 * stderr1);        
        }


        /// <summary>
        /// Tests the monte carlo with a vanilla payoff;
        /// </summary>
        [TestMethod]
        public void TestMonteVanilla()
        {
            DateTime valueDate = new DateTime(2009, 08, 28);
            DateTime expiryDate = new DateTime(2009, 12, 15);
            double t = System.Convert.ToDouble(expiryDate.Subtract(valueDate).Days) / 365.0;
            double spot = 4200;
            double strike = 4200;
            int[] ratedays = new int[] { 1, 2925 };
            double[] rateamts = new double[] { 0.034932296, 0.0349322696 };
            int[] resetDays = new int[] { -347, -317, -286, -256, -225, -194, -166, -135, -105, -74, -44, -13, 18, 48, 79, 109 };
            double[] vols = new double[] { 0.60, 0.60, 0.60, 0.60, 0.60, 0.60, 0.60, 0.60, 0.60, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6 };
            double[] resetAmts = new double[] { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 0, 0, 0, 0 };
            int[] divdays = new int[] { 0 };
            double[] divamts = new double[] { 0.0 };
            int numsimulations = 100000;
            int nobsin = 8;
            int nobsout = 8;
            double floor = 0;
            double cap = 0.067;
            CliquetPricer ah = new CliquetPricer("Vanilla", spot, strike, t, divdays, divamts, ratedays, rateamts, resetDays, resetAmts, vols, floor, cap, numsimulations, nobsin, nobsout, 3151,true);
            double[,] vpr = ah.GetPriceAndGreeks();
            double stderr1 = vpr[0, 1];
            double stderr2 = vpr[1, 1];
            double stderr3 = vpr[2, 1];
            double stderr4 = vpr[3, 1];
            double stderr5 = vpr[4, 1];
            double delta = vpr[1, 0];
            double gamma = vpr[2, 0];
            double vega = vpr[3, 0];
            double theta = vpr[4, 0];
            double rho = vpr[5, 0];
            Assert.AreEqual(566.1626786, vpr[0,0], 2 * stderr1);
            Assert.AreEqual(0.57760, delta, 2 * stderr2);
            Assert.AreEqual(0.00028, gamma, 2 * stderr3);
            Assert.AreEqual(0.01*898.266, vega, 2 * stderr4);
            Assert.AreEqual(-2.65028, theta, 2 * stderr5);
        }

        /// <summary>
        /// Tests the skew digital euro call put pricer
        /// </summary>
        [TestMethod]
        public void TestSkewDigitalEuroCallPut()
        {
            DateTime today = new DateTime(2010, 06, 22);
            DateTime expiry = new DateTime(2011, 03, 24);
            double tau = System.Convert.ToDouble(expiry.Subtract(today).Days) / 365.0;
            double spot = 42.00;
            double upperbarrier = 44.00;
            double lowerbarrier = 40.00;
            int[] zerodays = { 0, 10 };
            double[] zerorates = { 0.05, 0.05 };
            int[] divdays = new int[] { 0, 51, 247 };
            double[] divAmts = new double[] { 4, 1.350, 1.420 };
            double fwd = EquityAnalytics.GetForwardCCLin365(spot, tau, divdays, divAmts, zerodays, zerorates);
            OrcWingParameters owp = new OrcWingParameters() { AtmForward = fwd, CallCurve = 0.1250, CurrentVol = 0.26, DnCutoff = -0.25, Dsr = 0.9, PutCurve = 0.10, RefFwd = fwd, RefVol = 0.26, Scr = 0.0, SlopeRef = -0.1750, Ssr = 100, TimeToMaturity = tau, UpCutoff = 0.20, Usr = 0.50, Vcr = 0.0 };
            List<OrcWingParameters> owpList = new List<OrcWingParameters>();
            owpList.Add(owp);
            double skew1 = EquityAnalytics.GetWingSkew(owpList, new LinearInterpolation(), tau, upperbarrier);
            double vol = EquityAnalytics.GetWingValue(owpList, new LinearInterpolation(), tau, upperbarrier);
            BinaryEuro op1 = new BinaryEuro(spot, upperbarrier, true, tau, vol, zerodays, zerorates, divdays, divAmts, skew1);
            double pr1 = op1.GetPrice();
            Assert.AreEqual(0.35784, pr1, 0.005);
            double skew2 = EquityAnalytics.GetWingSkew(owpList, new LinearInterpolation(), tau, lowerbarrier);
            BinaryEuro op2 = new BinaryEuro(spot, lowerbarrier, false, tau, vol, zerodays, zerorates, divdays, divAmts, skew2);
            double pr2 = op2.GetPrice();
            Assert.AreEqual(0.43275, pr2, 0.005);
        }

        [TestMethod]
        public void TestCrankNicholson()
        {
            double spot = 385.5;
            double epsilon = 0.0001;
            double strike = 400;
            double[] rt = {  0.0425,
                                0.0452,
                                0.0457,
                                0.0462 };
            int[] tt = { 1 , 32 , 63 , 93 };
            int[] divdays = { 0, 48 };
            double[] divs = { 20, 10 };
            double vol = 0.2046;
            int steps = 80;
            double tStepSize = 0.01;
            DateTime today = new DateTime(2010, 05, 07);
            DateTime expiry = new DateTime(2010, 06, 24);       
            //string style = "European";
            double t = expiry.Subtract(today).Days / 365.0 + epsilon;  // For CN backwards propagation div time needs to be strictly less than expiry time
            double fwd = EquityAnalytics.GetForwardCCLin365(spot, t, divdays, divs, tt, rt);
            double df = EquityAnalytics.GetDFCCLin365(0, t, tt, rt);
            BlackScholes bs = new BlackScholes(spot, strike, false, t, vol, tt, rt, divdays, divs);
            CrankNicholson lhs = new CrankNicholson(false, false, spot, strike, t, vol, steps, tStepSize, 8.0, divdays, divs, tt, rt);
            double[,] res0 = OptionAnalytics.BlackScholesWithGreeks(false, fwd, strike, vol, t);            
            double[] res_cn = lhs.GetPriceAndGreeks();
            double pr_bs = bs.GetPrice();
            double delta_bs = bs.GetDelta();
            double gamma_bs = bs.GetGamma();
            double theta_bs = bs.GetTheta();
            double pr_cn = res_cn[0];
            double delta_cn = res_cn[1];
            double gamma_cn = res_cn[2];
            double theta_cn = res_cn[3];
            Assert.AreEqual(pr_cn, pr_bs, 0.50);
            Assert.AreEqual(delta_cn, delta_bs, 0.03);
            Assert.AreEqual(gamma_cn, 0.012931145370580023, 0.005);
            Assert.AreEqual(bs.GetTheta(), theta_cn, 0.01);


        }


        /// <summary>
        /// Further Crank Nicolson Tests
        /// </summary>
        [TestMethod]
        public void FurtherCNTests()
        {

            int[] dt = {   448, 813, 1179, 1544, 1909, 2274, 2640, 3005, 3370, 3735 };
            double[] divs = { 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2 };

            int[] rtdt = {  74, 439 };
            double[] rtam = { 0.03, 0.03 };         

            DateTime today = new DateTime(2009, 4, 13);
            DateTime expiry = new DateTime(2010, 04, 13);
            double tau = expiry.Subtract(today).Days / 365.0;
            double spot = 26.4;
            double strike = 30;
            double vol = 0.20;


            CrankNicholson cn_ep = new CrankNicholson(false, false, spot, strike, tau, vol, 800, 0.05, 8, dt, divs, rtdt, rtam);
            CrankNicholson cn_ec = new CrankNicholson(true, false, spot, strike, tau, vol, 100, 0.05, 8, dt, divs, rtdt, rtam);
            CrankNicholson cn_ac = new CrankNicholson(true, true, spot, strike, tau, vol, 100, 0.05, 8, dt, divs, rtdt, rtam);
            CrankNicholson cn_ap = new CrankNicholson(false, true, spot, strike, tau, vol, 100, 0.05, 8, dt, divs, rtdt, rtam);
            //CrankNicolson cn_ep = new CrankNicolson(ep, rc, dl, 8, 800, 0.05); //extra fine grid
            //CrankNicolson cn_ec = new CrankNicolson(ec, rc, dl, 8, 100, 0.05);
            //CrankNicolson cn_ac = new CrankNicolson(ac, rc, dl, 8, 100, 0.05);
            //CrankNicolson cn_ap = new CrankNicolson(ap, rc, dl, 8, 100, 0.05);
          
            double[] ep_pr = cn_ep.GetPriceAndGreeks();
            double[] ec_pr = cn_ec.GetPriceAndGreeks();
            double[] ac_pr = cn_ac.GetPriceAndGreeks();
            double[] ap_pr = cn_ap.GetPriceAndGreeks();

            Assert.AreEqual(ep_pr[0], 3.825991125, 0.001);
            Assert.AreEqual(ec_pr[0], 1.121514221, 0.001);
            Assert.AreEqual(ac_pr[0], 1.121514221, 0.001);
            Assert.AreEqual(ap_pr[0], 4.065906254, 0.001);

        }

        [TestMethod]
        public void FurtherCNTestsLongDated()
        {
            int[] dt = {   83,                              
                            2274,
                            2640,
                            3005,
                            3370,
                            3735 };

            double[] divs = { 5, 0, 0, 0, 0, 0 };

            int[] rtdt = { 74, 439};

            double[] rtam = { 0.05, 0.05 };      

            DateTime today = new DateTime(2009, 4, 13);
            DateTime expiry = new DateTime(2014, 04, 13);
            double spot = 26.4;
            double strike = 30;
            double vol = 0.30;
            double tau = expiry.Subtract(today).Days / 365.0;

            CrankNicholson cn_ep = new CrankNicholson(false, false, spot, strike, tau, vol, 200, 0.05, 8, dt, divs, rtdt, rtam);
            CrankNicholson cn_ec = new CrankNicholson(true, false, spot, strike, tau, vol, 200, 0.05, 8, dt, divs, rtdt, rtam);
            CrankNicholson cn_ac = new CrankNicholson(true, true, spot, strike, tau, vol, 200, 0.05, 8, dt, divs, rtdt, rtam);
            CrankNicholson cn_ap = new CrankNicholson(false, true, spot, strike, tau, vol, 200, 0.05, 8, dt, divs, rtdt, rtam);
                      
            double[] ep = cn_ep.GetPriceAndGreeks();
            double[] ec = cn_ec.GetPriceAndGreeks();
            double[] ac = cn_ac.GetPriceAndGreeks();
            double[] ap = cn_ap.GetPriceAndGreeks();

            double volback = cn_ep.ImpVol(ep[0]);

            Assert.AreEqual(vol, volback, 0.001);


            double ep_delta = ep[1];
            double ec_delta = ec[1];
            double ac_delta = ac[1];
            double ap_delta = ap[1];

            double ep_gamma = ep[2];
            double ec_gamma = ec[2];
            double ac_gamma = ac[2];
            double ap_gamma = ap[2];

            double ep_theta = ep[3];
            double ec_theta = ec[3];
            double ac_theta = ac[3];
            double ap_theta = ap[3];

            Assert.AreEqual(ep[0], 6.9534149, 0.001);
            Assert.AreEqual(ec[0], 5.0652783, 0.001);
            Assert.AreEqual(ac[0], 5.0652783, 0.001);
            Assert.AreEqual(ap[0], 9.3556003, 0.001);

            Assert.AreEqual(ep_delta, -0.41851728556981282, 0.001);
            Assert.AreEqual(ec_delta, 0.58178041660965252, 0.001);
            Assert.AreEqual(ac_delta, 0.58178041660965252, 0.001);
            Assert.AreEqual(ap_delta, -0.6687734186983475, 0.001);

            Assert.AreEqual(ep_gamma, 0.026900894453080339, 0.001);
            Assert.AreEqual(ec_gamma, 0.026899818384012897, 0.001);
            Assert.AreEqual(ac_gamma, 0.026899818384012897, 0.001);
            Assert.AreEqual(ap_gamma, 0.052277076176625865, 0.001);


            //Assert.AreEqual(ep_theta, 0 , 0.001);
            //Assert.AreEqual(ec_theta, 0.026899818384012897, 0.001);
            //Assert.AreEqual(ac_theta, 0.026899818384012897, 0.001);
            //Assert.AreEqual(ap_theta, 0.052277076176625865, 0.001);


        }


        [TestMethod]
        public void TestARO()
        {
            DateTime valueDate = new DateTime(2010, 05, 26);
            DateTime expiryDate = new DateTime(2013, 08, 20);
            double _tau = Convert.ToDouble(expiryDate.Subtract(valueDate).Days)/365.0;
            double spot = 42.50;
            double strike = 32.36;
            bool isCall = true;
            int[] ratedays = new int[] { 1, 2925 };
            double[] rateamts = new double[] { 0.044997, 0.044997 };
            int[] divdays = new int[] { 103,
                                        278,
                                        467,
                                        642,
                                        831,
                                        1006};

            double[] divAmts = new double[] { 0.4400, 0.4800, 0.5200, 0.5400, 0.5800, 0.6000 };
            
            int[] resetDays = new int[] {983,
                                        1013,
                                        1043,
                                        1073,
                                        1103,                                                    
                                        1133,
                                        1163,
                                        1182};

            double[] resetAmts = new double[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            double[] volatilities = new double[] { 0.3612, 0.3612, 0.3612, 0.3612, 0.3612, 0.3612, 0.3612, 0.3612 };

            ARO AROpricer = new ARO(isCall, spot, strike, _tau, volatilities, resetDays, resetAmts, ratedays, rateamts, divdays, divAmts);

            double pr = AROpricer.GetPrice();

            Assert.AreEqual(pr, 14.8100, 0.01);

        }


        [TestMethod]
        public void AROPartialReadings()
        {
            DateTime valueDate = new DateTime(2013, 03, 12);
            DateTime expiryDate = new DateTime(2013, 08, 20);
            double _tau = Convert.ToDouble(expiryDate.Subtract(valueDate).Days) / 365.0;
            double spot = 42.50;
            double strike = 32.36;
            bool isCall = true;
            int[] ratedays = new int[] { 1, 2925 };
            double[] rateamts = new double[] { 0.044997, 0.044997 };

            int[] divdays = new int[] { -918,
                                        -743,
                                        -554,
                                        -379,
                                        -190,
                                        -15};

            double[] divAmts = new double[] { 0.4400, 0.4800, 0.5200, 0.5400, 0.5800, 0.5983 };

            int[] resetDays = new int[] {-38,
                                        -8,
                                        22,
                                        52,
                                        82,                                                    
                                        112,
                                        142,
                                        161};

            double[] resetAmts = new double[] { 42.69, 42.1, 0, 0, 0, 0, 0, 0 };

            double[] volatilities = new double[] { 0.3612, 0.3612, 0.3612, 0.3612, 0.3612, 0.3612, 0.3612, 0.3612 };

            ARO AROpricer = new ARO(isCall, spot, strike, _tau, volatilities, resetDays, resetAmts, ratedays, rateamts, divdays, divAmts);

            double pr = AROpricer.GetPrice();

            Assert.AreEqual(pr, 10.28959, 0.0001);

        }
        





    }
}
