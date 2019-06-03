using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Numerics.Distributions.Continuous;
using Math = Orion.Numerics.Maths;

namespace Orion.Analytics.Equities
{
    public class BlackScholes
    {
        public BlackScholes()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackScholes"/> class.
        /// </summary>
        /// <param name="spot">The spot.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="isCall">if set to <c>true</c> [is call].</param>
        /// <param name="tau">The tau.</param>
        /// <param name="vol">The vol.</param>
        /// <param name="rtdays">The rtdays.</param>
        /// <param name="rtamts">The rtamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        public BlackScholes(double spot, double strike, Boolean isCall, double tau, double vol,
                           int[] rtdays, double[] rtamts, int[] divdays, double[] divamts)
        {
            _spot = spot;
            _strike = strike;
            _isCall = isCall;
            _tau = tau;
            _vol = vol;
            _rtdays = rtdays;
            _rtamts = rtamts;
            _divdays = divdays;
            _divamts = divamts;
        }

        private double _spot;
        private double _strike;
        private Boolean _isCall;
        private double _tau;
        private double _vol;
        private int[] _rtdays;
        private double[] _rtamts;
        private int[] _divdays;
        private double[] _divamts;
        private NormalDistribution _nd = new NormalDistribution(0, 1);        

        /// <summary>
        /// D1s the specified FWD.
        /// </summary>
        /// <param name="fwd">The FWD.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="sigma">The sigma.</param>
        /// <param name="tau">The tau.</param>
        /// <returns></returns>
        public static double Getd1(double fwd, double strike, double sigma, double tau)
        {
            double res = (System.Math.Log(fwd / strike) + 0.5 * sigma * sigma * tau)
                / sigma / System.Math.Pow(tau, 0.5);
            return res;
        }

        /// <summary>
        /// Getd2s the specified FWD.
        /// </summary>
        /// <param name="fwd">The FWD.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="sigma">The sigma.</param>
        /// <param name="tau">The tau.</param>
        /// <returns></returns>
        public static double Getd2(double fwd, double strike, double sigma, double tau)
        {
            double res = (System.Math.Log(fwd / strike) + 0.5 * sigma * sigma * tau)
                / sigma / System.Math.Pow(tau, 0.5);
            return res - sigma * System.Math.Pow(tau, 0.5);            
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <returns></returns>
        public double GetPrice()
        {            
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot, _tau, _divdays, _divamts,_rtdays,_rtamts);
            double rate = EquityAnalytics.GetRateCCLin365(0,_tau,_rtdays,_rtamts);
            return BSprice(fwd, _tau, _strike, rate, _vol, _isCall);            
        }

        /// <summary>
        /// BS Price.
        /// </summary>
        /// <param name="Fwd">The FWD.</param>
        /// <param name="Tau1">The tau1.</param>
        /// <param name="Strike1">The strike1.</param>
        /// <param name="Rate1">The rate1.</param>
        /// <param name="Sigma1">The sigma1.</param>
        /// <param name="Payoff">The payoff.</param>
        /// <returns></returns>
        public double BSprice(double Fwd, double Tau1, double Strike1, double Rate1,
                double Sigma1, bool isCall)
        {
            double d1, d2, n1, n2;
            int S1 = 0;
            if (!isCall)
            {
                S1 = -1;
            }
            else
            {
                S1 = 1;
            }

            d1 = BlackScholes.Getd1(Fwd, Strike1, Sigma1, Tau1);
            d2 = BlackScholes.Getd2(Fwd, Strike1, Sigma1, Tau1);
            n1 = _nd.CumulativeDistribution(S1 * d1);
            n2 = _nd.CumulativeDistribution(S1 * d2);
            return S1 * (Fwd * n1 - Strike1 * n2) * System.Math.Exp(-Rate1 * Tau1);
        }

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <returns></returns>
        public double GetDelta()
        {
            double q = EquityAnalytics.GetYieldCCLin365(_spot,0,_tau,_divdays,_divamts,_rtdays,_rtamts);
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot,_tau,_divdays,_divamts,_rtdays,_rtamts);                
            double d1 = (System.Math.Log(fwd / _strike) + _vol * _vol * _tau / 2) / _vol / System.Math.Sqrt(_tau);
            double temp;
            if (!_isCall)
                temp = Math.BasicMath.ndist(d1) - 1;
            else
                temp = Math.BasicMath.ndist(d1);
            return System.Math.Exp(-q * _tau) * temp;    
        }


        /// <summary>
        /// Gets the gamma.
        /// </summary>
        /// <returns></returns>
        public double GetGamma()
        {
            double q = EquityAnalytics.GetYieldCCLin365(_spot, 0, _tau, _divdays, _divamts, _rtdays, _rtamts);
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot, _tau, _divdays, _divamts, _rtdays, _rtamts);                           
            double d1 = (System.Math.Log(fwd / _strike) + _vol * _vol * _tau / 2) / _vol / System.Math.Sqrt(_tau);
            double temp;
            temp = _nd.ProbabilityDensity(d1);
            return System.Math.Exp(q * _tau) * temp / _spot / _vol / System.Math.Sqrt(_tau);
        }



        /// <summary>
        /// Gets the vega.
        /// </summary>
        /// <returns></returns>
        public double GetVega()
        {
            double q = EquityAnalytics.GetYieldCCLin365(_spot, 0, _tau, _divdays, _divamts, _rtdays, _rtamts);
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot, _tau, _divdays, _divamts, _rtdays, _rtamts);
            double d1 = (System.Math.Log(fwd / _strike) + _vol * _vol * _tau / 2) / _vol / System.Math.Sqrt(_tau);
            double lhs = _spot * System.Math.Sqrt(_tau) * _nd.ProbabilityDensity(d1) * 0.01 * System.Math.Exp(-q * _tau);
            return lhs;
        }


        /// <summary>
        /// Gets the theta.
        /// </summary>
        /// <returns></returns>
        public double GetTheta()
        {
            double q = EquityAnalytics.GetYieldCCLin365(_spot, 0, _tau, _divdays, _divamts, _rtdays, _rtamts);
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot, _tau, _divdays, _divamts, _rtdays, _rtamts);
            double d1 = (System.Math.Log(fwd / _strike) + _vol * _vol * _tau / 2) / _vol / System.Math.Sqrt(_tau);
            double rate = EquityAnalytics.GetRateCCLin365(0, _tau, _rtdays, _rtamts);           
            double d2 = d1 - _vol * System.Math.Sqrt(_tau);
            double rhs1 = -System.Math.Exp(-q * _tau) * _spot * _nd.ProbabilityDensity(d1) * _vol / 2 / System.Math.Sqrt(_tau);
            double rhs3 = 0;
            if (_isCall)
            {
                double rhs2 = -rate * _strike * System.Math.Exp(-rate * _tau) * _nd.CumulativeDistribution(d2);
                //rhs3 = q * _spot * System.Math.Exp(-q * _tau) * _nd.CumulativeDistribution(d1);
                return (rhs1 + rhs2 + rhs3) / 365.0;
            }
            else
            {
                double rhs2 = rate * _strike * System.Math.Exp(-rate * _tau) * _nd.CumulativeDistribution(-d2);
                //rhs3 = -q * _spot * System.Math.Exp(-q * _tau) * _nd.CumulativeDistribution(-d1);
                return (rhs1 + rhs2 + rhs3) / 365.0;
                
            }

        }






    }
}
