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

#region Using directives

using System;
using Highlander.Reporting.Analytics.V5r3.Maths.Collections;
using Highlander.Reporting.Analytics.V5r3.Solvers;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Integration
{
    /// <summary>
    /// Integral of a one-dimensional function.
    /// </summary>
    /// <latex>
    /// Given a number \f$ N \f$ of intervals, the integral of
    /// a function \f$ f \f$ between \f$ a \f$ and \f$ b \f$ is 
    /// calculated by means of the trapezoid formula
    /// \f[
    /// 	\int_{a}^{b} f \mathrm{d}x = 
    /// 	\frac{1}{2} f(x_{0}) + f(x_{1}) + f(x_{2}) + \dots 
    /// 	+ f(x_{N-1}) + \frac{1}{2} f(x_{N})
    /// \f]
    /// where \f$ x_0 = a \f$, \f$ x_N = b \f$, and 
    /// \f$ x_i = a+i \Delta x \f$ with \f$ \Delta x = (b-a)/N \f$.
    /// </latex>
    public class IntegrationHelpers
    {
        const double LniMax = 0.9868;

        const double Eps = 1e-7;

        const double Oo2Pi = 0.159154943091895;  // 1/(2.pi}

        const double Oor2Pi = 0.398942280401433; // 1/sqrt(2.pi)  


        /// <summary>
        /// Numerical integration of the lognormal, i.e.
        /// I = \int_L^U pd(S) dS func(S)
        /// where pd(S) is the measure exp(-(ln(S/S0)+vol^2*t/2)^2/(2*vol^2*t))/(S*vol*sqrt(2*pi*t))
        /// which applies to a random walk of volatility vol after time t such that
        /// S = S0.
        /// Uses Simpson's rule evaluation, after a strategic change of variables.
        /// </summary>
        /// <param name="nSteps">Max no. of ordinates; if zero or negative no limit;</param>
        /// <param name="fn">The function of S to be evaluated</param>
        /// <param name="l">The lower limit (minimum 0)</param>
        /// <param name="u">The upper limit: -ve value means infinity</param>
        /// <param name="q">volatility * sqrt(t)</param>
        /// <param name="s0">The expectation value of the underlying at time t</param>
        /// <param name="parameters">The parameter Vector of the multivariate function:  the first parameter elements is the asset price S.</param>
        /// <returns>A vector array of results: value, 
        /// delta = {\partial I \over \partial S_0}, 
        /// gamma = {\partial^2 I \over \partial S_0^2}, 
        /// vega = {\partial I \over \partial q}, 
        /// nSteps = The actual number used is returned here.</returns>
        public static double[] LogNormalIntegration(long nSteps, QRMultivariateRealFunction fn,
          double l, double u, double q, double s0, DoubleVector parameters)
        {
            double lb, ub, s, s1, s2, sm1 = 0d, sm2 = 0d;
            double v, w, y;
            double yy;
            long nMax = nSteps, nUsed;
            var result = new double[5];
            if (s0 <= 0 || q <= 0) throw new Exception("Invalid LogNormInt parameters");
            var q2 = q * q;
            var lnS1 = Math.Log(s0) - .5 * q2;
            /* Change limits to new variables ...  */
            if (l > 0)
            {
                v = (Math.Log(l) - lnS1) / q;
                lb = (Math.Sqrt(.25 + v * v) - .5) / v;
            }
            else lb = -1;
            if (u < 0) ub = 1;
            else if (u == 0) ub = -1;
            else
            {
                v = (Math.Log(u) - lnS1) / q;
                ub = (Math.Sqrt(.25 + v * v) - .5) / v;
            }
            if (ub < lb) { v = ub; ub = lb; lb = v; }
            else if (ub == lb)
            {
                return result;
            }
            if (lb > -LniMax)
            {
                v = q * lb / (1 - lb * lb) + lnS1;
                var xx = Math.Exp(v);
                parameters[0] = xx;
                yy = fn(parameters);
                s = Math.Exp(-.5 * Math.Pow(lb / (1 - lb * lb), 2.0)) * (1 + lb * lb) /
                  Math.Pow(1 - lb * lb, 2.0) * yy;
                s1 = s * v;
                s2 = s1 * v;
            }
            else s = s1 = s2 = 0;
            if (ub < LniMax)
            {
                v = q * ub / (1 - ub * ub) + lnS1;
                var xx = Math.Exp(v);
                parameters[0] = xx;
                yy = fn(parameters);
                w = Math.Exp(-.5 * Math.Pow(ub / (1 - ub * ub), 2.0)) * (1 + ub * ub) / Math.Pow(1 - ub * ub, 2.0) * yy;
                s += w;
                w *= v;
                s1 += w;
                s2 += w * v;
            }
            var h = ub - lb;
            var h2 = h / 2;
            s *= h2;
            s1 *= h2;
            s2 *= h2;
            double sm = 0;
            for (nUsed = 2; 2 * nUsed - 1 <= nMax || nMax <= 0; nUsed += nUsed - 1)
            {
                var os = s;
                var os1 = s1;
                var os2 = s2;
                var osm = sm;
                double sum;
                double sum1;
                double sum2;
                double z;
                for (z = lb + h / 2, sum = sum1 = sum2 = 0; z < ub; z += h)
                {
                    if (z < -LniMax || z > LniMax) continue;
                    y = 1 / (1 - z * z);
                    v = z * y;
                    w = v * v;
                    var p = q * v + lnS1;
                    var xx = Math.Exp(p);
                    parameters[0] = xx;
                    yy = fn(parameters);
                    v = Math.Exp(-.5 * w) * (y * y + w) * yy;
                    sum += v;
                    v *= p;
                    sum1 += v;
                    sum2 += v * p;
                }
                h /= 2;
                s = s / 2 + h * sum;  /* include midpoints under trapezoid rule */
                s1 = s1 / 2 + h * sum1;
                s2 = s2 / 2 + h * sum2;
                sm = (4 * s - os) / 3;       /* convert to Simpson's rule */
                sm1 = (4 * s1 - os1) / 3;
                sm2 = (4 * s2 - os2) / 3;
                if (Math.Abs(sm - osm) < 1e-9 + Eps * Math.Abs(osm) && nUsed >= 33) break;
            }
            sm *= Oor2Pi;
            sm1 *= Oor2Pi;
            sm2 *= Oor2Pi;
            w = lnS1 / q2;
            v = ((w + 1) * lnS1 - 1) * sm - (1 + 2 * w) * sm1 + sm2 / q2;
            y = s0 * q2;
            result[0] = sm;
            result[1] = (sm1 - lnS1 * sm) / y;
            result[2] = v / (s0 * y);
            result[3] = v / q;
            result[4] = nSteps;
            return result;
        }


        /// <summary>
        /// Numerical integration of the lognormal, i.e.
        /// I = \int_L^U pd(S) dS func(S)
        /// where pd(S) is the measure exp(-(ln(S/S0)+vol^2*t/2)^2/(2*vol^2*t))/(S*vol*sqrt(2*pi*t))
        /// which applies to a random walk of volatility vol after time t such that
        /// S = S0.
        /// Uses Simpson's rule evaluation, after a strategic change of variables.       
        /// </summary>
        /// <param name="nSteps">Max no. of ordinates; if zero or negative no limit;</param>
        /// <param name="fn">The function of S to be evaluated</param>
        /// <param name="L">The lower limit (minimum 0)</param>
        /// <param name="U">The upper limit: -ve value means infinity</param>
        /// <param name="q">volatility * sqrt(t)</param>
        /// <param name="S0">The expectation value of the underlying at time t</param>
        /// <param name="parameters">The parameter Vector of the multivariate function:  the first parameter elements is the asset price S.</param>
        /// <returns>A vector array of results: value, 
        /// delta = {\partial I \over \partial S_0}, 
        /// gamma = {\partial^2 I \over \partial S_0^2}, 
        /// vega = {\partial I \over \partial q}, 
        /// nSteps = The actual number used is returned here.</returns>
        public static double[] LogNormIntegration2(long nSteps, QRMultivariateRealFunction fn,
          double L, double U, double q, double S0, DoubleVector parameters)
        {
            double lb, ub, s, s1, s2, sm1 = 0d, sm2 = 0d;
            double v, w, y;
            double yy;
            long nMax = nSteps, nUsed;
            var result = new double[5];
            if (S0 <= 0 || q <= 0) throw new Exception("Invalid LogNormInt parameters");
            var q2 = q * q;
            var lnS1 = Math.Log(S0) - .5 * q2;
            /* Change limits to new variables ...  */
            if (L > 0)
            {
                v = (Math.Log(L) - lnS1) / q;
                lb = (Math.Sqrt(.25 + v * v) - .5) / v;
            }
            else lb = -1;
            if (U < 0) ub = 1;
            else if (U == 0) ub = -1;
            else
            {
                v = (Math.Log(U) - lnS1) / q;
                ub = (Math.Sqrt(.25 + v * v) - .5) / v;
            }
            if (ub < lb) { v = ub; ub = lb; lb = v; }
            else if (ub == lb)
            {
                return result;
            }
            if (lb > -LniMax)
            {
                v = q * lb / (1 - lb * lb) + lnS1;
                var xx = Math.Exp(v);
                parameters[0] = xx;
                yy = fn(parameters);
                s = Math.Exp(-.5 * Math.Pow(lb / (1 - lb * lb), 2.0)) * (1 + lb * lb) /
                  Math.Pow(1 - lb * lb, 2.0) * yy;
                s1 = s * v;
                s2 = s1 * v;
            }
            else s = s1 = s2 = 0;
            if (ub < LniMax)
            {
                v = q * ub / (1 - ub * ub) + lnS1;
                var xx = Math.Exp(v);
                parameters[0] = xx;
                yy = fn(parameters);
                w = Math.Exp(-.5 * Math.Pow(ub / (1 - ub * ub), 2.0)) * (1 + ub * ub) / Math.Pow(1 - ub * ub, 2.0) * yy;
                s += w;
                w *= v;
                s1 += w;
                s2 += w * v;
            }
            var h = ub - lb;
            var h2 = h / 2;
            s *= h2;
            s1 *= h2;
            s2 *= h2;
            double sm = 0;
            for (nUsed = 2; 2 * nUsed - 1 <= nMax || nMax <= 0; nUsed += nUsed - 1)
            {
                var os = s;
                var os1 = s1;
                var os2 = s2;
                var osm = sm;
                double sum;
                double sum1;
                double sum2;
                double z;
                for (z = lb + h / 2, sum = sum1 = sum2 = 0; z < ub; z += h)
                {
                    if (z < -LniMax || z > LniMax) continue;
                    y = 1 / (1 - z * z);
                    v = z * y;
                    w = v * v;
                    var p = q * v + lnS1;
                    var xx = Math.Exp(v);
                    parameters[0] = xx;
                    yy = fn(parameters);
                    v = Math.Exp(-.5 * w) * (y * y + w) * yy;
                    sum += v;
                    v *= p;
                    sum1 += v;
                    sum2 += v * p;
                }
                h /= 2;
                s = s / 2 + h * sum;  /* include midpoints under trapezoid rule */
                s1 = s1 / 2 + h * sum1;
                s2 = s2 / 2 + h * sum2;
                sm = (4 * s - os) / 3;       /* convert to Simpson's rule */
                sm1 = (4 * s1 - os1) / 3;
                sm2 = (4 * s2 - os2) / 3;
                if (Math.Abs(sm - osm) < 1e-9 + Eps * Math.Abs(osm) && nUsed >= 33) break;
            }
            sm *= Oor2Pi;
            sm1 *= Oor2Pi;
            sm2 *= Oor2Pi;
            w = lnS1 / q2;
            v = ((w + 1) * lnS1 - 1) * sm - (1 + 2 * w) * sm1 + sm2 / q2;
            y = S0 * q2;
            result[0] = sm;
            result[1] = (sm1 - lnS1 * sm) / y;
            result[2] = v / (S0 * y);
            result[3] = v / q;
            result[4] = nSteps;
            return result;
        }
    }
}