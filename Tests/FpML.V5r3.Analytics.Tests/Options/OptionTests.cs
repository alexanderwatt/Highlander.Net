#region Using Directives

using System;
using System.Diagnostics;
using Orion.Analytics.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Integration;
using Orion.Analytics.Maths.Collections;

#endregion

namespace Orion.Analytics.Tests.Options
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class OptionTests 
    {
        private const double _fwdPrice = .1;
        private const double _strike = .1;
        private const double _volatility = .2;
        
        
        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionTest1()
        {
            RunOptTest(1, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionBSCallTest()
        {
            RunBSOptTest(true, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionBSVolTest()
        {
            RunBSOptVolTest(true, _fwdPrice, _strike, new[] { 0.0, .05 },
            new[] {0.00,.05}, 5.0);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionBSPutTest()
        {
            RunBSOptTest(false, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCoxCallTest()
        {
            RunCoxOptTest(true, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCoxputTest()
        {
            RunCoxOptTest(false, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCox1CallTest()
        {
            RunCox1OptTest(true, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCox1putTest()
        {
            RunCox1OptTest(false, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCox2CallTest()
        {
            RunCox2OptTest(true, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCox2putTest()
        {
            RunCox2OptTest(false, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCox3CallTest()
        {
            RunCox3OptTest(true, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCox3putTest()
        {
            RunCox3OptTest(false, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCox4CallTest()
        {
            RunCox4OptTest(true, _fwdPrice, _strike, _volatility, .05, 5);
        }

        /// <summary>
        /// Testing the integration of the BS.
        /// </summary>
        [TestMethod]
        public void OptionCox4putTest()
        {
            RunCox4OptTest(false, _fwdPrice, _strike, _volatility, .05, 5);
        }


        [TestMethod]
        public void OptSolveStrikeTest()
        {
            double fwdPrice = 0.12705691222694671;
            double vol = 0.3235535788541;
            double r = 0.0;
            double t = 1;

            double prem = 0.000145252447697802;

            

            double strike = Analytics.Options.OptionAnalytics.OptSolveStrike(true, fwdPrice, vol, r, t, prem);

        }

        [TestMethod]
        public void OptSolveFwdPriceTest()
        {

            double strike = 0.1;
            double vol = 0.2;
            double r = 0.05;
            double t = 5;

            double prem = 0.013779846;

            double fwd = Analytics.Options.OptionAnalytics.OptSolveFwd(true, strike, vol, r, t, prem);
        } 

  //      public static void CompoundOptTest(long nSteps, short CPS, double strikeS, double rS, double tS,
  //short CPL, double strikeL, double rL, double tL, double fwdPrice, double vol)
  //      {
  //          Option.CompOptParam P;

  //          if (CPL != 1 && CPL != -1 || CPS != 1 && CPS != -1) throw new Exception("Option must be call or put");
  //          P.cps = CPL;
  //          P.CPs = CPS;
  //          P.vols = vol;
  //          P.ks = strikeL;
  //          P.Ks = strikeS;
  //          P.tTs = tL - tS;
  //          P.rtTs = (rL * tL - rS * tS) / P.tTs;
  //          return Math.Exp(-rS * tS) * Option.LogNormInt(nSteps, (BivariateRealFunction)Option.fComp, 0.0, -1.0,
  //            vol * Math.Sqrt(tS), fwdPrice * Math.Exp(rS * tS));
  //          Debug.WriteLine(String.Format("Premium : {0} Delta : {1} Gamma : {2} Vega : {3} Theta : {4} Rho : {5}", prem, delta, gamma, vega, theta, rho));
  //      }

        public static void RunOptTest(short CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            const long N = 32769;
            var vector = new DoubleVector(new[] {fwdPrice, strike})
            {
                [0] = fwdPrice,
                [1] = strike
            };
            var rf = CP * Math.Exp(-r * t);
            var values = IntegrationHelpers.LogNormalIntegration(N, OptionAnalytics.FOpt, CP > 0? strike: 0, CP > 0? -1: strike,
                                        vol * Math.Sqrt(t), fwdPrice, vector);
            var prem = rf * values[0];
            var delta = rf * values[1];
            var gamma = rf * values[2];
            var vega = rf * values[3] * Math.Sqrt(t);
            var theta = -r * prem + rf * values[3] * vol / (2 * Math.Sqrt(t));
            var rho = -t*prem;
            Debug.WriteLine(
                $"Premium : {prem} Delta : {delta} Gamma : {gamma} Vega : {vega} Theta : {theta} Rho : {rho}");
        }

        //public static void RunOptTest2(short CP, double fwdPrice, double strike, double vol, double r, double t)
        //{
        //    const long N = 32769;

        //    var rf = CP * Math.Exp(-r * t);
        //    var values = Option.LogNormInt(N, (PayOffFunction)Option.fOpt, CP > 0 ? strike : 0, CP > 0 ? -1 : strike,
        //                                vol * Math.Sqrt(t), fwdPrice);
        //    var prem = rf * values[0];
        //    var delta = rf * values[1];
        //    var gamma = rf * values[2];
        //    var vega = rf * values[3] * Math.Sqrt(t);
        //    var theta = -r * prem + rf * values[3] * vol / (2 * Math.Sqrt(t));
        //    var rho = -t * prem;
        //    Debug.WriteLine(String.Format("Premium : {0} Delta : {1} Gamma : {2} Vega : {3} Theta : {4} Rho : {5}", prem, delta, gamma, vega, theta, rho));
        //}

        public static void RunBSOptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            var result = Analytics.Options.OptionAnalytics.BlackScholesWithGreeks(CP, fwdPrice, strike, vol, t);
            Debug.WriteLine(String.Format("Premium : {0} Delta : {1} Gamma : {2} Vega : {3} Theta : {4} Rho : {5}", result[0, 0], result[0, 1], result[0, 2], result[0, 3], result[0, 4], result[0,5]));
        }

        public static void RunBSOptVolTest(Boolean CP, double fwdPrice, double strike, double[] vols, double[] rs, double t)
        {
            foreach(var vol in vols)
            {
                foreach (var r in rs)
                {
                    var prem = Analytics.Options.OptionAnalytics.BlackScholesWithGreeks(CP, fwdPrice, strike, vol, t);
                    var result = Analytics.Options.OptionAnalytics.OptSolveVol(CP, fwdPrice, strike, prem[0, 0], r, t);
                    Debug.WriteLine(String.Format("Premium : {0} Volatility : {1}", vol, result));
                }
            }
        }

        public static void RunCoxOptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            var result = Analytics.Options.OptionAnalytics.CoxEuroOption(CP, fwdPrice, fwdPrice, 100, strike, vol, 1.0, t);
            Debug.WriteLine(String.Format("Premium : {0} ", result));
        }

        public static void RunCox1OptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            var result = Analytics.Options.OptionAnalytics.CoxEuroOption1(CP, fwdPrice, fwdPrice, 100, strike, vol, 1.0, t);
            Debug.WriteLine(String.Format("Premium : {0} ", result));
        }

        public static void RunCox2OptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            var result = Analytics.Options.OptionAnalytics.CoxEuroOption2(CP, fwdPrice, 100, strike, vol, 1.0, t);
            Debug.WriteLine(String.Format("Premium : {0} ", result));
        }

        public static void RunCox3OptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            var result = Analytics.Options.OptionAnalytics.CoxEuroOption3(CP, fwdPrice, 100, strike, vol, 1.0, t);
            Debug.WriteLine(String.Format("Premium : {0} ", result));
        }

        public static void RunCox4OptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            var result = Analytics.Options.OptionAnalytics.CoxEuroOption4(CP, fwdPrice, 100, strike, vol, 1.0, t);
            Debug.WriteLine(String.Format("Premium : {0} ", result));
        }

        public static void RunCoxAmericanOptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        {
            var result = Analytics.Options.OptionAnalytics.CoxFuturesAmerOption(CP, fwdPrice, 100, strike, vol, 1.0, t);
            Debug.WriteLine(String.Format("Premium : {0} ", result));
        }

        //public static void RunCoxAmericanOptTest(Boolean CP, double fwdPrice, double strike, double vol, double r, double t)
        //{
        //    var result = Option.(CP, fwdPrice, 100, strike, vol, 1.0, t);
        //    Debug.WriteLine(String.Format("Premium : {0} ", result));
        //}

    }
}