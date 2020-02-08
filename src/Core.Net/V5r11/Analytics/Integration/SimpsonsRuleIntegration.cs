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
using Highlander.Reporting.Analytics.V5r3.Distributions;

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
    public class SimpsonsRuleIntegration
    {
        private const double C1 = 0.0804492816131978, /* sqrt(sqrt(2) - 1) / 8       */
                             C2 = -0.0441941738241592,  /* -1 / (8 * sqrt(8))          */
                             C3 = 1.2871885058111600,  /* 2 * sqrt(sqrt(2) - 1)       */
                             C4 = -0.3884434935075090,  /* -sqrt(sqrt(2) + 1) / 4      */
                             C5 = 0.0568862325702784,  /* sqrt(sqrt(2)-1)/(4*sqrt(8)) */
                             C6 = 3.1075479480600700,  /* 2 * sqrt(sqrt(2) + 1)       */
                             R8 = 2.8284271247461900;  /* sqrt(8)                     */

        public const double Eps = 1e-7;


        /// <summary>
        /// The integral I1 used above is given by I1(u, a) =
        /// \int_{-\infinity}^u dx {a exp(-x^2/2) \over a^2 + x^2}.
        /// We integrate from zero, since the integrand is even in x and
        /// \int_0^\infinity dx {a exp(-x^2/2) \over a^2 + x^2} = \pi exp(a^2/2) cn(-a)
        /// The integral \int _0 ^u dx {a exp(-x^2/2) \over a^2  + x^2}.
        /// (where u > 0), is done using Simpson's rule.   
        /// </summary>
        /// <param name="u"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double Value(double u, double a)
        {
            double sm;
            double x = Math.Abs(u), aa = Math.Abs(a);
            /* outside -5 < u < 5, both the Simpson's rule evaluation & the analytic
               approximation start to get a bit kooky, so just assume infinity values */
            if (u < -5) return 0; /* Value will be less than 7.19e-8 anyway */
            var a2 = aa * aa;
            var q = Math.PI * Math.Exp(a2 / 2) * new NormalDistribution().CumulativeDistribution(-aa);
            if (u > 5) return 2 * (a > 0 ? q : -q);
            /* For small a, we approximate the function with a polynomial:
              a e^(-x^2/2)/(x^2 + a^2)  ~  a / ((x^2 + a^2)(1 + x^2/2 + x^4/8)),
              which we can do analytically, thereby bypassing the need for 10000000000
              Simpson's rule evaluations. Max error 1.5e-4 (for large |x|) */
            if (aa < 0.01)
            {
                var u2 = u * u;
                sm = (Math.Atan(x / aa) + aa * ((C1 + C2 * a2) * Math.Log((u2 + R8 - C3 * x) /
                       (u2 + R8 + C3 * x)) + (C4 + C5 * a2) * Math.Atan2(C6 * x, R8 - u2))) /
                       (1 - a2 / 8 * (4 - a2));
                if (sm > q) sm = q; /* The expression overestimates the integral:
                                   we prevent it from being greater than q here */
            }
            else
            {
                /*  FUNC(x) (aa * exp(-(x) * (x) / 2) / (a2 + (x) * (x))) */
                sm = 0;
                double h;
                var func0 = (aa * Math.Exp(-(0.0) * (0.0) / 2) / (a2 + (0.0) * (0.0)));
                var funcX = (aa * Math.Exp(-(x) * (x) / 2) / (a2 + (x) * (x)));
                var s = (h = x) * (func0 + funcX) / 2;
                double osm;
                do
                {
                    var os = s;
                    osm = sm;
                    /* We are calculating the sum of FUNC(z) for z from h/2 to x-h/2 in steps of
                       h, but have arranged it in this way to maximize speed ... */
                    double zi;
                    double zii;
                    var z = (zi = zii = h * h) / 8;
                    var f = Math.Exp(-z) * aa / 2;
                    z += a2 / 2;
                    var zMax = (x * x + a2) / 2;
                    double r;
                    var g = r = Math.Exp(-zii);
                    double sum;
                    for (sum = 0; z < zMax; z += zi, zi += zii, f *= g, g *= r)
                        sum += f / z;
                    s = s / 2 + (h /= 2) * sum;
                    sm = (4 * s - os) / 3;
                } while (Math.Abs(sm - osm) >= Eps * (1e-3 + Math.Abs(u > 0 ? q + osm : q - osm)));
            }
            q += u > 0 ? sm : -sm;
            q = q > 0 ? q : 0;
            return a > 0 ? q : -q;
        }
    }
}