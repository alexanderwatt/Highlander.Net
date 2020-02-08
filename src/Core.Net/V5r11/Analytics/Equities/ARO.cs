/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Linq;
using Highlander.Reporting.Analytics.V5r3.Distributions;

namespace Highlander.Reporting.Analytics.V5r3.Equities
{
    public class ARO 
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ARO"/> class. Uses the Curran method.
        /// </summary>
        /// <param name="isCall">if set to <c>true</c> [is call].</param>
        /// <param name="spot">The spot.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="tau">The tau.</param>
        /// <param name="vols">The vols.</param>
        /// <param name="resetDays">The reset days.</param>
        /// <param name="resetAmts">The reset amts.</param>
        /// <param name="rtdays">The rtdays.</param>
        /// <param name="rtamts">The rtamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        public ARO(bool isCall, double spot, double strike, double tau, double[] vols, int[] resetDays, double[] resetAmts,
                           int[] rtdays, double[] rtamts, int[] divdays, double[] divamts)    
        {
            _isCall = isCall;
            _spot = spot;
            _strike = strike;
            _tau = tau;
            _vols = vols;
            _rtdays = rtdays;
            _rtamts = rtamts;
            _divdays = divdays;
            _divamts = divamts;
            _resetDays = resetDays;
            _resetAmts = resetAmts;
        }

        private double _spot;
        private double _strike;
        private Boolean _isCall;
        private double _tau;
        private double[] _vols;
        private int[] _rtdays;
        private double[] _rtamts;
        private int[] _divdays;
        private double[] _divamts;
        private int[] _resetDays;
        private double[] _resetAmts;
        private const int daybasis = 365;
        private const double cEpsilon = 0.000001;
        private NormalDistribution _nd = new NormalDistribution(0, 1);

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <returns></returns>
        public double GetPrice()
        {
            int numresets = _resetDays.Length;
            int readings = _resetAmts.Count(item => (item > 0));
            if (readings == numresets)
            {
                double av = _resetAmts.Average();
                double df = EquityAnalytics.GetDFCCLin365(0, _tau, _rtdays, _rtamts);
                return (av - _strike) * df;
            }
            double newstrike = TransformStrike(numresets, readings);
            int nleft = numresets - readings;
            int[] reset_days = new int[nleft];
            double[] vols = new double[nleft];
            for (int idx = 0; idx < nleft; idx++)
            {
                reset_days[idx] = _resetDays[idx + readings];
                vols[idx] = _vols[idx + readings];
            }
            double newprice = GetPrice(newstrike, numresets - readings, reset_days, vols);
            return newprice * (numresets - readings) / numresets;
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <param name="strike">The strike.</param>
        /// <param name="numResets">The num_resets.</param>
        /// <param name="resetDays"></param>
        /// <param name="vols">The vols.</param>
        /// <returns></returns>
        public double GetPrice(double strike, int numResets, int[] resetDays, double[] vols)
        {
            double bond = 0.0;
            int numVols = _vols.Length;
            double[] covxxi;
            double[] varlogs;
            double[] forwards = new double[numResets];
            double mu = CalcMu(ref forwards, resetDays, vols);
            double sigma = CalcSigma(resetDays, vols);
            varlogs = CalcVarLogs(resetDays, vols);
            covxxi = CalcCovXXI(varlogs, vols.Length);
            if (strike <= 0)
            {
                bond = System.Math.Abs(strike);
                strike = cEpsilon;
            }
            double z0 = (mu - System.Math.Log(strike)) / sigma;
            double i1 = 0.0;
            for (int idx = 0; idx < numResets; idx++)
            {
                double x1 = z0 + (covxxi[idx] / sigma);
                double n1 = _nd.CumulativeDistribution(x1);
                i1 += n1 * forwards[idx] / (double)numResets;
            }
            double i2 = strike * _nd.CumulativeDistribution(z0);
            double prem = i1 - i2;
            double df = EquityAnalytics.GetDFCCLin365(0, _tau, _rtdays, _rtamts);
            return (prem + bond) * df;
        }


        /// <summary>
        /// Transforms the strike.
        /// </summary>
        /// <param name="len">The len.</param>
        /// <param name="readings">The readings.</param>
        /// <returns></returns>
        private double TransformStrike(int len, int readings)
        {
            double sum = Sum(_resetAmts);
            double newstrike = _strike;
            if (readings > 0)
            {
                double av = sum / readings;
                int remaining = len - readings;
                if (remaining > 0)
                    newstrike = (len * _strike - readings * av) / remaining;
            }
            return newstrike;
        }

        /// <summary>
        /// Calcs the cov XXI.
        /// </summary>
        /// <param name="varlogs">The varlogs.</param>
        /// <param name="numVols">The numvols.</param>
        private double[] CalcCovXXI(double[] varlogs, int numVols)
        {
            double trm1 = 0.0;
            double[] results = new double[numVols];
            for (int idx = 0; idx < numVols; idx++)
            {
                if (idx > 0)
                {
                    trm1 += varlogs[idx - 1];
                }
                var trm2 = varlogs[idx];
                var trm3 = (numVols - idx - 1) * trm2;
                results[idx] = 1.0 / numVols * (trm1 + trm2 + trm3);
            }
            return results;
        }

        /// <summary>
        /// Calcs the mu.
        /// </summary>
        /// <param name="resetDays"></param>
        /// <param name="vols">The vols.</param>
        /// <param name="forwards">The forwards.</param>
        /// <returns></returns>
        private double CalcMu(ref double[] forwards,int[] resetDays, double[] vols)
        {
            double musum = 0;
            int numresets = resetDays.Length;
            for (int idx = 0; idx < numresets; idx++)
            {
                double dt0 = Convert.ToDouble(resetDays[idx])/daybasis;
                double r0 = EquityAnalytics.GetRateCCLin365(0, dt0, _rtdays, _rtamts);                
                double q0 = EquityAnalytics.GetYieldCCLin365(_spot,0, dt0, _divdays, _divamts, _rtdays,_rtamts);             

                musum += System.Math.Log(_spot) + (r0 - q0) * dt0 - vols[idx] * vols[idx] * dt0 / 2;
                forwards[idx] = _spot * System.Math.Exp((r0 - q0) * dt0);
            }
            return musum / numresets;
        }

        /// <summary>
        /// Calcs the sigma.
        /// </summary>
        /// <param name="resetDays">The reset dates.</param>
        /// <param name="vols">The vols.</param>
        /// <returns></returns>
        private double CalcSigma(int[] resetDays, double[] vols)
        {
            int numResets = resetDays.Length;
            double crossterms = 0;
            double cumvar = 0;
            for (int idx = 0; idx < numResets; idx++)
            {               
                double dt0 = Convert.ToDouble(resetDays[idx]) / daybasis;
                double var = vols[idx] * vols[idx] * dt0;
                cumvar += var;
                crossterms += (numResets - (idx + 1)) * var;
            }
            double temp1 = System.Math.Sqrt(2 * crossterms + cumvar);
            double res = 1.0 / (double)numResets * temp1;
            return res;
        }

        /// <summary>
        /// Calcs the var logs.
        /// </summary>
        /// <param name="resetDays">The reset_days.</param>
        /// <param name="vols">The vols.</param>
        /// <returns></returns>
        private double[] CalcVarLogs(int[] resetDays, double[] vols)
        {
            int numresets = resetDays.Length;
            double[] res = new double[numresets];
            for (int idx = 0; idx < numresets; idx++)
            {
                double dt0 = Convert.ToDouble(resetDays[idx]) / daybasis;
                double varlogs = vols[idx] * vols[idx] * dt0;
                res[idx] = varlogs;
            }
            return res;
        }

        public static double Sum(double[] array)
        {
            double temp = 0;
            foreach (double x in array)
            {
                temp += x;
            }
            return temp;
        }
    }
}
