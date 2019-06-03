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

#region Usings

using System;
using System.Collections.Generic;
using Orion.Analytics.Distributions;
using Orion.Analytics.Interpolations;
using Orion.Analytics.LinearAlgebra.Sparse;
using Orion.Analytics.Maths.Collections;
using Orion.Analytics.Statistics;
using Orion.Analytics.Stochastics.MonteCarlo;

#endregion

namespace Orion.Analytics.Equities
{
    /// <summary>
    /// Asian Hybrid Cliquet - As in the ASX Principal Series 1
    /// </summary>
    public class CliquetPricer 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsianHybrid"/> class.
        /// </summary>
        /// <param name="payoffFunc"></param>
        /// <param name="spot">The spot.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="yearFraction">The year fraction.</param>
        /// <param name="divDays">The div days.</param>
        /// <param name="divAmts">The div amts.</param>
        /// <param name="rateDays">The rate days.</param>
        /// <param name="rateAmts">The rate amts.</param>
        /// <param name="resetDays">The reset days.</param>
        /// <param name="resetAmounts">The reset amounts.</param>
        /// <param name="volsToResets">The vols to resets.</param>
        /// <param name="floor">The floor.</param>
        /// <param name="cap">The cap.</param>
        /// <param name="numSimulations">The num simulations.</param>
        /// <param name="nobsin">The nobsin.</param>
        /// <param name="nobsout">The nobsout.</param>
        /// <param name="seed"></param>
        /// <param name="antithetic"></param>
        public CliquetPricer(String payoffFunc, double spot, double strike, double yearFraction, int[] divDays, 
                             double[] divAmts, int[] rateDays, double[] rateAmts, int[] resetDays, double[] resetAmounts,
                             double[] volsToResets, double floor, double cap, int numSimulations,
                             int nobsin, int nobsout, int seed, bool antithetic)
        {
            _spot = spot;
            _strike = strike;
            _timeToExpiry = yearFraction;
            _resetDays = resetDays;
            _resetAmounts = resetAmounts;           
            _floor = floor;
            _cap = cap;
            _numSimulations = numSimulations;
            _nobsin = nobsin;
            _nobsout = nobsout;
            _volsToResets = volsToResets;
            _rateDays = rateDays;
            _rateAmts = rateAmts;
            _divDays = divDays;
            _divAmts = divAmts;
            _useAntithetic = antithetic;
            InitialiseRateInterp();
            _seed = seed;
            _payoffFunc = payoffFunc;
        }

        #region Private fields

      
        private readonly double _strike;
        private readonly double _spot;
        private readonly double _timeToExpiry;
        private readonly int _seed;
        private readonly int[] _resetDays;
        private readonly double[] _resetAmounts;
        private readonly int[] _divDays;
        private readonly double[] _divAmts;
        private readonly int[] _rateDays;
        private readonly double[] _rateAmts;
        private readonly double[] _volsToResets;
        private readonly double _floor;
        private readonly double _cap;
        private readonly int _numSimulations;
        private const double CEpsilon = 0.000001;
        private LinearInterpolation _rateInterp;
        private NormalDistribution _nd = new NormalDistribution(0, 1);        
        private readonly bool _useAntithetic;
        private readonly int _nobsin;
        private readonly int _nobsout;
        int daybasis = 365;
        private readonly String _payoffFunc;

        #endregion

        /// <summary>
        /// Rates the years.
        /// </summary>
        /// <returns></returns>
        private double[] RateYears()
        {
            int n = _rateDays.Length;
            double[] rateYears = new double[n];
            for (int idx=0;idx<n;idx++)
            {
                rateYears[idx] = _rateDays[idx] / daybasis;
            }
            return rateYears;
        }

        /// <summary>
        /// Initialises the rate interp.
        /// </summary>
        private void InitialiseRateInterp()
        {
            // Initialise rate curve
            int n = _rateDays.Length;
            double[] rateYrs = new double[n];
            for (int idx = 0; idx < n; idx++)
            {
                rateYrs[idx] = Convert.ToDouble(_rateDays[idx] / daybasis);
            }
            _rateInterp = new LinearInterpolation(rateYrs, _rateAmts);
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <returns></returns>
        public double[,] GetPriceAndGreeks()
        {           
            int n = _resetAmounts.Length;
            List<double> tList = new List<double>();
            List<double> volList = new List<double>();
            for (int i = 0; i < n; i++)
            {
                if (_resetDays[i] > 0)
                {
                    tList.Add(Convert.ToDouble(_resetDays[i]) / daybasis);
                    volList.Add(_volsToResets[i]);
                }
            }            
            IVector t = new DoubleVector(tList.ToArray());
            SparseVector times = new SparseVector(t);

            // Vols                 
            double[,] v = new double[n, 1];
            int n1 = volList.Count;
            for (int idx = 0; idx < n1; idx++)
            {
                v[idx, 0] = volList[idx];
            }
            //Matrix vols = new Matrix(v);
            double[] variances = CalcForwardVariance(times.Data, volList.ToArray());
            double[] sigmas = Sqrt(variances);
            DoubleVector _vf = new DoubleVector(variances);            
            SparseVector vf = new SparseVector(_vf, _vf.Length);
            //Drift
            double[] rates = GetForwardRates(times.Data);
            double[] yields = GetForwardYields(times.Data);
            int nrates = times.Length;
            double[] _drifts = new double[nrates];
            double[] _logdrifts = new double[nrates];
            //IVector driftsVector = new DoubleVector(_drifts);
            //SparseVector drifts = new SparseVector(driftsVector, driftsVector.Length);
            for (int idx = 0; idx < nrates; idx++)
            {
                _drifts[idx] = rates[idx] - yields[idx];
                _logdrifts[idx] = _drifts[idx] - _vf[idx] * 0.5;

            }
            DoubleVector ld = new DoubleVector(_logdrifts);
            SparseVector logdrifts = new SparseVector(ld,ld.Length);
            // Discount            
            double df = Math.Exp(-ForwardRate(0, _timeToExpiry)*_timeToExpiry );
            // 6 identical paths, price, delta, gamma, vega, theta, rho
            PathGenerator pathGen = new SinglePathGenerator(Rng(_seed), logdrifts, vf, times);
            MultiVarPathPricer pathPr = new CliquetPathPricer(_payoffFunc, df, _useAntithetic, _spot, _strike, _floor, _cap, _nobsin, _nobsout, _resetDays, _resetAmounts, rates, yields, sigmas);
            //PathPricer deltaPr = PathFactory.GetPath("Delta", df, true, _spot, _strike, _floor, _cap, _resetDays, _resetAmounts, sigmas, _nobsin, _nobsout);
            List<RiskStatistics> accumulatorList = new List<RiskStatistics>();
            for (int i=0; i<6;i++)
               accumulatorList.Add(new RiskStatistics());
            // Price path
            MonteCarloModel mcm = new MonteCarloModel(pathGen, pathPr, accumulatorList);                                              
            mcm.AddMultiVarSamples(_numSimulations);
            double price = accumulatorList[0].Mean;
            double delta = accumulatorList[1].Mean;
            double gamma = accumulatorList[2].Mean;
            double vega = accumulatorList[3].Mean;
            double theta = accumulatorList[4].Mean;
            double rho = accumulatorList[5].Mean;
            double[,] res = new[,] {     {price, accumulatorList[0].ErrorEstimate}, 
                                                {delta, accumulatorList[1].ErrorEstimate}, 
                                                {gamma, accumulatorList[2].ErrorEstimate},
                                                {vega, accumulatorList[3].ErrorEstimate},
                                                {theta, accumulatorList[4].ErrorEstimate},
                                                {rho, accumulatorList[5].ErrorEstimate}   };        
            return res;
        }

        /// <summary>
        /// SQRTs the specified arr.
        /// </summary>
        /// <param name="arr">The arr.</param>
        /// <returns></returns>
        private double[] Sqrt(double[] arr)
        {
            int n = arr.Length;
            double[] x = new double[n];
            for (int idx = 0; idx < n; idx++)
            {
                x[idx] = Math.Sqrt(arr[idx]);
            }
            return x;
        }

        /// <summary>
        /// Gets the forward rates.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        private double[] GetForwardRates(double[] array)
        {
            int n = array.Length;
            double[] frates = new double[n];
            for (int idx=0; idx<n;idx++)
            {
                if (idx == 0)
                    frates[0] = ForwardRate(0, array[idx]);
                else
                   frates[idx] = ForwardRate(array[idx-1], array[idx]);
            }
            return frates;
        }

        /// <summary>
        /// Forwards the rate.
        /// </summary>
        /// <param name="yearFraction1">The t1.</param>
        /// <param name="yearFraction2">The t2.</param>
        /// <returns></returns>
        private double ForwardRate(double yearFraction1, double yearFraction2)
        {
            return EquityAnalytics.GetRateCCLin365(yearFraction1, yearFraction2, _rateDays, _rateAmts);
            //double rt1 = _rateInterp.ValueAt(yearFraction1, false);
            //double rt2 = _rateInterp.ValueAt(yearFraction2, false);
            //if (yearFraction2 > yearFraction1)
            //    return (rt2 * yearFraction2 - rt1 * yearFraction1) / (yearFraction2 - yearFraction1);        
            //else
            //    return 0;
        }

        /// <summary>
        /// Gets the yields.
        /// </summary>
        /// <param name="yearFractions">The year fractions.</param>
        /// <returns></returns>
        private double[] GetYields(double[] yearFractions)
        {
            int n = yearFractions.Length;
            double[] yields = new double[n];
            for (int idx = 0; idx < n; idx++)
            {
                yields[idx] = GetYield(yearFractions[idx]);
            }
            return yields;
        }


        /// <summary>
        /// Gets the forward yields.
        /// </summary>
        /// <param name="yearFractions">The year fractions.</param>
        /// <returns></returns>
        private double[] GetForwardYields(double[] yearFractions)
        {
            double[] q = GetYields(yearFractions);
            int n = yearFractions.Length;
            double[] qf = new double[n];
            qf[0]=q[0];
            for (int idx = 1; idx < n; idx++ )
            {
                qf[idx] = (yearFractions[idx]*q[idx] - yearFractions[idx - 1]*q[idx - 1]) / (yearFractions[idx] - yearFractions[idx - 1]);
            }
            return qf;
        }

        /// <summary>
        /// Gets the yield.
        /// </summary>
        /// <param name="yearFraction">The year fraction.</param>
        /// <returns></returns>
        private double GetYield(double yearFraction)
        {
            return EquityAnalytics.GetYieldCCLin365(_spot, 0, yearFraction, _divDays, _divAmts, _rateDays, _rateAmts);
        }

        /// <summary>
        /// PVs the divs.
        /// </summary>
        /// <param name="yearFraction1">The year fraction1.</param>
        /// <param name="yearFraction2">The year fraction2.</param>
        /// <returns></returns>
        private double PVDivs(double yearFraction1, double yearFraction2)
        {
            double res = EquityAnalytics.GetPVDivsCCLin365(yearFraction1, yearFraction2, _divDays, _divAmts, _rateDays, _rateAmts);           
            return res;
        }

        /// <summary>
        /// Zeroes the specified times.
        /// </summary>
        /// <param name="times">The times.</param>
        /// <returns></returns>
        private void Zero(double[] times)
        {
            int n = times.Length;
            for (int idx = 0; idx < n; idx++)
            {
                if (times[idx] < 0)
                    times[idx] = 0;
            }
        }


        /// <summary>
        /// RNGs this instance.
        /// </summary>
        /// <returns></returns>
        private IContinuousRng Rng(int seed)
        {
            IBasicRng basRng = new MCG31vsl(seed);
            //IContinuousRng unifRng = new UniformRng(basRng,0,1);
            var lhs = new BoxMullerGaussianRng(basRng, 0, 1);
            return lhs;
        }

        /// <summary>
        /// Calcs the forward vols.
        /// </summary>
        /// <param name="times">The times.</param>
        /// <param name="vols">The vols.</param>
        /// <returns></returns>
        double[] CalcForwardVariance(double[] times, double[] vols)
        {
            int n = times.Length;
            double[] quadvar = new double[n];           
            List<double> quadvar0 = new List<double>();
            List<double> quadt0 = new List<double>();
            double[] quadvar1 = new double[n];
            double[] fwdvols = new double[n];
            for (int idx=0;idx<n;idx++)
            { 
                quadvar[idx] = vols[idx]*vols[idx]*times[idx];
            }
            //create List quadvar0 of quadratic variations and associated times,
            //dropping negative fwd vols
            int jdx=1;
            quadvar0.Add(quadvar[0]);
            quadt0.Add(times[0]);
            for (int idx=1;idx<n;idx++)
            {               
                if (quadvar[idx] > quadvar[idx-1]  )  
                {                    
                    quadvar0.Add(quadvar[idx]);
                    quadt0.Add(times[idx]);
                    jdx++;
                }
            }
            //interpolate cut down list to original time vector
            for (int idx=0;idx<n;idx++)
            {
                double time1 = times[idx];
                var li = new LinearInterpolation();
                li.Initialize(quadt0.ToArray(), quadvar0.ToArray());
                double y = li.ValueAt(time1, false) ;
                quadvar1[idx] = y;
            }       
    
            //convert back from quadratic variatons to forward vols
            fwdvols[0] = vols[0]*vols[0];
            for (int idx = 0; idx < n-1; idx++)
            {                
                double vol0 =quadvar1[idx];
                double vol1 =quadvar1[idx+1];
                double fwdvol = Math.Sqrt((vol1 - vol0) / (times[idx + 1] - times[idx]));
                fwdvols[idx+1] = fwdvol*fwdvol;
            }
                            
            return fwdvols;
        }
    }

    #region Path Pricers


    /// <summary>
    /// Vanilla pricer to test Monte Carlo implementation
    /// </summary>
    public class CliquetPathPricer : MultiVarPathPricer
    {
        private readonly double[] _forwardVols;
        private readonly double[] _rates;
        private readonly double[] _yields;
        public double Strike;
        public double Floor;
        public double Cap;
        public int Nobsin;
        public int Nobsout;
        public double[] Resets;
        public int[] ResetDays;
        public double Spot;
        public double DiscountFactor;      
        public double Delta;
        public string PayoffFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliquetPathPricer"/> class.
        /// </summary>
        /// <param name="payoffFunc"></param>
        /// <param name="discountFactor">The discount factor.</param>
        /// <param name="useAntitheticVariance">if set to <c>true</c> [use antithetic variance].</param>
        /// <param name="spot"></param>
        /// <param name="strike">The strike.</param>
        /// <param name="floor">The floor.</param>
        /// <param name="resets"></param>
        /// <param name="rates"></param>
        /// <param name="yields"></param>
        /// <param name="forwardvols"></param>
        /// <param name="cap"></param>
        /// <param name="nobsin"></param>
        /// <param name="nobsout"></param>
        /// <param name="resetdays"></param>
        public CliquetPathPricer(string payoffFunc,double discountFactor, bool useAntitheticVariance, double spot, 
                             double strike, double floor, double cap,int nobsin, int nobsout ,int[] resetdays, double[] resets,
                             double[] rates, double[] yields, double[] forwardvols)
            : base(discountFactor, useAntitheticVariance)
        {
            Strike = strike;
            Floor = floor;
            Cap = cap;
            Nobsin = nobsin;
            Nobsout = nobsout;
            Resets = resets;
            ResetDays = resetdays;
            Spot = spot;
            DiscountFactor = discountFactor;
            _forwardVols= forwardvols;
            _rates = rates;
            _yields = yields;
            PayoffFunc = payoffFunc;
        }

        /// <summary>
        /// Payoffs the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="up">if set to <c>true</c> [up].</param>
        /// <param name="payoffFunc">The payoff func.</param>
        /// <returns></returns>
        private double Payoff(Path[] path, bool up, string payoffFunc)
        {
            if (payoffFunc == "AsianHybrid")
                return AsianHybrid(path, up);
            if (payoffFunc == "Vanilla")
                return Vanilla(path, up);
            if (payoffFunc == "TaurusCliquet")
                return TaurusCliquet(path, up);
            return 0.0;
        }

        /// <summary>
        /// Vanillas the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="up">if set to <c>true</c> [up].</param>
        /// <returns></returns>
        private double Vanilla(Path[] path, bool up )
        {
            int n = ResetDays.Length;
            int n0 = path[0].Count;
            double logReset = 0;
            //double[] spot = new double[n];
            //double[] times = path[0].Times.Data;
            if (up)
            {
                for (int idx = n - n0; idx < n; idx++)
                {
                    logReset += path[0][idx - n + n0] + (idx == n - n0 ? Math.Log(Spot) : 0);
                    Resets[idx] = Math.Exp(logReset);
                }

                var result1 = Math.Max(Resets[n - 1] - Strike, 0);
                return result1;
            }
            logReset = 0;
            for (int idx = n - n0; idx < n; idx++)
            {
                logReset += path[0].Drift[idx - n + n0] - path[0].Diffusion[idx - n + n0] + (idx == n - n0 ? Math.Log(Spot) : 0);
                Resets[idx] = Math.Exp(logReset);
            }
            var result2 = Math.Max(Resets[n - 1] - Strike, 0);
            return result2;
        }

         /// <summary>
        /// Payoffs the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="up">if set to <c>true</c> [up].</param>
        /// <returns></returns>
        private double TaurusCliquet(Path[] path, bool up)
        {
            int n = ResetDays.Length;
            int n0 = path[0].Count;
            double logReset = 0;
            double value;
            //double[] spot = new double[n];
            double result1 = 0.0;
            double result2 = 0.0;
            //double[] times = path[0].Times.Data;
            if (up)
            {
                for (int idx = n - n0; idx < n; idx++)
                {
                    var priceUp = path[0].Drift[idx - n + n0] + path[0].Diffusion[idx - n + n0] + (idx == n - n0 ? Math.Log(Spot) : 0);
                    logReset += priceUp;
                    Resets[idx] = Math.Exp(logReset);
                }
                for (int idx = 1; idx < n; idx++)
                {
                    result1 += Math.Min(Resets[idx] / Resets[idx - 1] - Strike, Cap);
                }
                value = Math.Max(result1, Floor);
                return value;
            }
            logReset = 0;
            for (int idx = n - n0; idx < n; idx++)
            {
                var priceDn = path[0].Drift[idx - n + n0] - path[0].Diffusion[idx - n + n0] + (idx == n - n0 ? Math.Log(Spot) : 0);
                logReset += priceDn;
                Resets[idx] = Math.Exp(logReset);
            }
            for (int idx = 1; idx < n; idx++)
            {
                result2 += Math.Min(Resets[idx] / Resets[idx - 1] - Strike, Cap);
            }
            value = (Math.Max(result2, Floor));
            
            return value;
        }

         /// <summary>
        /// Asians the hybrid.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="up">if set to <c>true</c> [up].</param>
        /// <returns></returns>
        private double AsianHybrid(Path[] path, bool up)
        {
            double logReset = 0;
            int n0 = path[0].Count;
            int n = ResetDays.Length;
            double inSum = 0.0;
            double outSum = 0.0;
            if (up)
            {
                for (int idx = n - n0; idx < n; idx++)
                {
                    logReset += path[0].Drift[idx - n + n0] + path[0].Diffusion[idx - n + n0] + (idx == n - n0 ? Math.Log(Spot) : 0);
                    Resets[idx] = Math.Exp(logReset);
                }
                for (int idx = 0; idx < Nobsin; idx++)
                    inSum += Resets[idx];
                inSum /= Nobsin;
                for (int idx = Nobsin; idx < Nobsin + Nobsout; idx++)
                    outSum += Resets[idx];
                outSum /= Nobsout;
                var result1 = Math.Max(outSum / inSum - Strike, 0.0);
                return result1;
            }
            logReset = 0;
            outSum = 0;
            inSum = 0;
            for (int idx = n - n0; idx < n; idx++)
            {
                logReset += path[0].Drift[idx - n + n0] - path[0].Diffusion[idx - n + n0] + (idx == n - n0 ? Math.Log(Spot) : 0);
                Resets[idx] = Math.Exp(logReset);
            }
            for (int idx = 0; idx < Nobsin; idx++)
                inSum += Resets[idx];
            inSum /= Nobsin;
            for (int idx = Nobsin; idx < Nobsin + Nobsout; idx++)
                outSum += Resets[idx];
            outSum /= Nobsout;
            var result2 = Math.Max(outSum / inSum - Strike, 0.0);
            return result2;
        }

        /// <summary>
        /// Given one or more paths, the value of an option is returned.
        /// </summary>
        /// <remarks>
        /// This method must be overriden by derived classes. 
        /// </remarks>
        /// <param name="path">
        /// An array of one or more <see cref="Path"/>s depending on the derivative
        /// whose value must be calculated.
        /// </param>
        /// <returns>
        /// The price of the derivative had the evolution of the underlying(s)
        /// followed the path passed as argument.
        /// </returns>
        public override double[] Value(Path[] path)
        {            
            double priceUp = Payoff(path, true, PayoffFunc);
            double sqrtT = Math.Sqrt(path[0].Times[0]);
            double f = _forwardVols[0] *sqrtT;
            double adj = path[0].Diffusion[0] / (f* f);
            double[] zf = path[0].Diffusion.Data;
            double[] z = new double[zf.Length];
            z[0] = zf[0] / (_forwardVols[0] * Math.Sqrt(path[0].Times[0]));
            for (int idx = 1; idx < zf.Length; idx++)
                z[idx] = zf[idx] / (_forwardVols[idx] * Math.Sqrt(path[0].Times[idx] - path[0].Times[idx-1]));
            double mu = _rates[0] - _yields[0] - 0.5*_forwardVols [0]*_forwardVols [0];
            double sig = _forwardVols[0];
            double texp = path[0].Times[path[0].Times.Length - 1];
            int n = _forwardVols.Length;
            double price;
            double delta;
            double gamma;
            double theta;
            double vega;
            double rho;
            double vup = (z[0] * z[0] - 1.0) / sig - z[0] * sqrtT;
            double vdn = (z[0] * z[0] - 1.0) / sig + z[0] * sqrtT;
            double rup = z[0] * sqrtT / sig;
            double rdn = -z[0] * sqrtT / sig;
            for (int jdx = 1; jdx != n; jdx++)
            {
                vup += (z[jdx] * z[jdx] - 1.0) / _forwardVols[jdx] - z[jdx] * Math.Sqrt(path[0].Times[jdx] - path[0].Times[jdx - 1]);
                vdn += (z[jdx] * z[jdx] - 1.0) / _forwardVols[jdx] + z[jdx] * Math.Sqrt(path[0].Times[jdx] - path[0].Times[jdx - 1]);
                rup += z[jdx] * Math.Sqrt(path[0].Times[jdx] - path[0].Times[jdx - 1]) / _forwardVols[jdx];
                rdn += -z[jdx] * Math.Sqrt(path[0].Times[jdx] - path[0].Times[jdx - 1]) / _forwardVols[jdx];
            }
            double af = 1;
            for (int idx = 0; idx < _rates.Length; idx++)
            {
                double t1;
                if (idx==0)
                   t1 = path[0].Times[idx];
                else
                   t1 = path[0].Times[idx] - path[0].Times[idx-1] ;
                af *= Math.Exp(_rates[idx] * t1);
            }
            double rZero = 1/texp*Math.Log(af);
            if (UseAntitheticVariance)
            {
                double priceDn = Payoff(path, false,PayoffFunc);
                price = DiscountFactor * (priceUp + priceDn) / 2;
                delta = DiscountFactor * (priceUp * adj/Spot - priceDn * adj/Spot) / 2;                                       
                gamma = DiscountFactor * (priceUp * (adj*adj - adj - 1/ (f * f)) / Spot / Spot
                                       +  priceDn * (adj*adj + adj - 1/ (f * f)) / Spot / Spot)/2;             
                vega = DiscountFactor * (priceUp * vup + priceDn * vdn) / 2 ;
                rho = DiscountFactor * (priceUp * (rup - texp) + priceDn * (rdn - texp))/2 * 0.01;
                theta =  DiscountFactor * (priceUp*( rZero  -adj*mu - 0.5*adj*adj*sig*sig + 0.5/ path[0].Times[0])
                                         + priceDn*( rZero + adj*mu - 0.5*adj*adj*sig*sig + 0.5/ path[0].Times[0]))/2;
            }
            else
            {
                price = DiscountFactor * priceUp;
                delta = DiscountFactor * priceUp * adj/Spot;
                gamma = DiscountFactor * (priceUp * (adj*adj + 2 - adj - 1/ (f * f)) / Spot / Spot);
                vega = DiscountFactor * priceUp * vup ;
                rho = DiscountFactor * priceUp * (rup - texp) * 0.01;
                theta = DiscountFactor * priceUp*(rZero - adj*mu - 0.5*adj*adj*sig*sig + 0.5/ path[0].Times[0]);

            }
            return new[] { price, delta, gamma, vega*0.01, theta/365.0, rho };
        }

        #endregion Path Pricers

    }
}
