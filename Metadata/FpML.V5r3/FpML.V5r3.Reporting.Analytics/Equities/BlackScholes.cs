/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Analytics.Distributions;
using Orion.Analytics.Maths;


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

        private readonly double _spot;
        private readonly double _strike;
        private readonly Boolean _isCall;
        private readonly double _tau;
        private readonly double _vol;
        private readonly int[] _rtdays;
        private readonly double[] _rtamts;
        private readonly int[] _divdays;
        private readonly double[] _divamts;
        private readonly NormalDistribution _nd = new NormalDistribution(0, 1);        

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
            double res = (Math.Log(fwd / strike) + 0.5 * sigma * sigma * tau)
                / sigma / Math.Pow(tau, 0.5);
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
            double res = (Math.Log(fwd / strike) + 0.5 * sigma * sigma * tau)
                / sigma / Math.Pow(tau, 0.5);
            return res - sigma * Math.Pow(tau, 0.5);            
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
        /// <param name="fwd">The FWD.</param>
        /// <param name="tau1">The tau1.</param>
        /// <param name="strike1">The strike1.</param>
        /// <param name="rate1">The rate1.</param>
        /// <param name="sigma1">The sigma1.</param>
        /// <param name="isCall"></param>
        /// <returns></returns>
        public double BSprice(double fwd, double tau1, double strike1, double rate1,
                double sigma1, bool isCall)
        {
            int s1;
            if (!isCall)
            {
                s1 = -1;
            }
            else
            {
                s1 = 1;
            }
            var d1 = Getd1(fwd, strike1, sigma1, tau1);
            var d2 = Getd2(fwd, strike1, sigma1, tau1);
            var n1 = _nd.CumulativeDistribution(s1 * d1);
            var n2 = _nd.CumulativeDistribution(s1 * d2);
            return s1 * (fwd * n1 - strike1 * n2) * Math.Exp(-rate1 * tau1);
        }

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <returns></returns>
        public double GetDelta()
        {
            double q = EquityAnalytics.GetYieldCCLin365(_spot,0,_tau,_divdays,_divamts,_rtdays,_rtamts);
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot,_tau,_divdays,_divamts,_rtdays,_rtamts);                
            double d1 = (Math.Log(fwd / _strike) + _vol * _vol * _tau / 2) / _vol / Math.Sqrt(_tau);
            double temp;
            if (!_isCall)
                temp = BasicMath.Ndist(d1) - 1;
            else
                temp = BasicMath.Ndist(d1);
            return Math.Exp(-q * _tau) * temp;    
        }

        /// <summary>
        /// Gets the gamma.
        /// </summary>
        /// <returns></returns>
        public double GetGamma()
        {
            double q = EquityAnalytics.GetYieldCCLin365(_spot, 0, _tau, _divdays, _divamts, _rtdays, _rtamts);
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot, _tau, _divdays, _divamts, _rtdays, _rtamts);                           
            double d1 = (Math.Log(fwd / _strike) + _vol * _vol * _tau / 2) / _vol / Math.Sqrt(_tau);
            var temp = _nd.ProbabilityDensity(d1);
            return Math.Exp(q * _tau) * temp / _spot / _vol / Math.Sqrt(_tau);
        }

        /// <summary>
        /// Gets the vega.
        /// </summary>
        /// <returns></returns>
        public double GetVega()
        {
            double q = EquityAnalytics.GetYieldCCLin365(_spot, 0, _tau, _divdays, _divamts, _rtdays, _rtamts);
            double fwd = EquityAnalytics.GetForwardCCLin365(_spot, _tau, _divdays, _divamts, _rtdays, _rtamts);
            double d1 = (Math.Log(fwd / _strike) + _vol * _vol * _tau / 2) / _vol / Math.Sqrt(_tau);
            double lhs = _spot * Math.Sqrt(_tau) * _nd.ProbabilityDensity(d1) * 0.01 * Math.Exp(-q * _tau);
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
            double d1 = (Math.Log(fwd / _strike) + _vol * _vol * _tau / 2) / _vol / Math.Sqrt(_tau);
            double rate = EquityAnalytics.GetRateCCLin365(0, _tau, _rtdays, _rtamts);           
            double d2 = d1 - _vol * Math.Sqrt(_tau);
            double rhs1 = -Math.Exp(-q * _tau) * _spot * _nd.ProbabilityDensity(d1) * _vol / 2 / Math.Sqrt(_tau);
            double rhs3 = 0;
            if (_isCall)
            {
                double rhs2 = -rate * _strike * Math.Exp(-rate * _tau) * _nd.CumulativeDistribution(d2);
                //rhs3 = q * _spot * System.Math.Exp(-q * _tau) * _nd.CumulativeDistribution(d1);
                return (rhs1 + rhs2 + rhs3) / 365.0;
            }
            else
            {
                double rhs2 = rate * _strike * Math.Exp(-rate * _tau) * _nd.CumulativeDistribution(-d2);
                //rhs3 = -q * _spot * System.Math.Exp(-q * _tau) * _nd.CumulativeDistribution(-d1);
                return (rhs1 + rhs2 + rhs3) / 365.0;
               
            }
        }
    }
}
