/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Analytics.Solvers;

namespace Orion.Analytics.Distributions
{
    /// <summary>
    /// Inverse cumulative normal distribution class.
    /// </summary>
    /// <remarks>
    /// <p>This function returns an approximation of the inverse cumulative
    /// normal distribution function. I.e., given P, it returns
    /// an approximation to the X satisfying P = Pr(Z &lt;= X) where Z is a
    /// random variable from the standard normal distribution.</p>
    /// 
    /// <p>The algorithm uses a mini-max approximation by rational functions
    /// and the result has a relative error whose absolute value is less
    /// than 1.15e-9.</p>
    /// 
    /// <p>See the page <see href="http://home.online.no/~pjacklam/notes/invnorm/"/>
    /// for more details.</p>
    /// </remarks>
    public class InvCumulativeNormalDistribution : IUnaryFunction//, IRealFunction
    {
        #region Coefficients in rational approximations

        private static readonly double[] A = {-3.969683028665376e+01, 2.209460984245205e+02,
                                              -2.759285104469687e+02, 1.383577518672690e+02,
                                              -3.066479806614716e+01, 2.506628277459239e+00};

        private static readonly double[] B = {-5.447609879822406e+01, 1.615858368580409e+02,
                                              -1.556989798598866e+02, 6.680131188771972e+01, -1.328068155288572e+01};

        private static readonly double[] C = {-7.784894002430293e-03, -3.223964580411365e-01,
                                              -2.400758277161838e+00, -2.549732539343734e+00,
                                              4.374664141464968e+00, 2.938163982698783e+00};

        private static readonly double[] D = {7.784695709041462e-03, 3.224671290700398e-01,
                                              2.445134137142996e+00, 3.754408661907416e+00};

        #endregion

        /// <summary>Inverse cumulative standard normal distribution.</summary>
        /// <remarks>The <b>standard</b> normal distribution has a
        /// mean equal to zero and a standard deviation equal to one.</remarks>
        public InvCumulativeNormalDistribution()
        {
            Mean = 0d;
            Sigma = 1d;
        }

        /// <summary>Inverse cumulative standard normal distribution.</summary>
        /// <remarks>The <b>standard</b> normal distribution has a
        /// mean equal to zero and a standard deviation equal to one.</remarks>
        public InvCumulativeNormalDistribution(double mean)
        {
            Mean = mean;
            Sigma = 1d;
        }

        /// <summary>Inverse cumulative normal distribution.</summary>
        /// <remarks>The normal distribution has a mean equal to <c>mean</c> 
        /// and a standard deviation equal to <c>sigma</c>.</remarks>
        public InvCumulativeNormalDistribution(double mean, double sigma)
        {
            Mean = mean;
            Sigma = sigma;
        }

        /// <summary>Gets or sets the mean of the normal distribution.</summary>
        public double Mean { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation of the normal distribution.
        /// </summary>
        public double Sigma { get; set; }

        /// <summary>
        /// Gets the inverse cumulative normal distribution function.
        /// </summary>
        /// <param name="p">A <c>double</c> in <c>[0,1]</c> expected.</param>
        public double ValueOf(double p)
        {
            return Mean + Sigma * StandardValueOf(p);
        }
     
        /// <summary>
        /// Gets the inverse cumulative normal distribution function.
        /// </summary>
        /// <param name="p">A <c>double</c> in <c>[0,1]</c> expected.</param>
        public double Value(double p)
        {
            return Mean + Sigma * StandardValueOf(p);
        }
     
        /// <summary>Returns the inverse cumulative <b>standard</b> normal 
        /// distribution for the probability <c>p</c>.</summary>
        private static double StandardValueOf(double p)
        {
            if(p < 0.0 || p > 1.0) throw new ArgumentOutOfRangeException(
                nameof(p), p, "The probability must be comprised in [0, 1].");

            // Define break-points.
            const double plow = 0.02425;
            const double phigh = 1 - plow;

            double q;

            // Rational approximation for lower region:
            if ( p < plow ) 
            {
                q = Math.Sqrt(-2*Math.Log(p));
                return (((((C[0]*q+C[1])*q+C[2])*q+C[3])*q+C[4])*q+C[5]) /
                       ((((D[0]*q+D[1])*q+D[2])*q+D[3])*q+1);
            }

            // Rational approximation for upper region:
            if ( phigh < p ) 
            {
                q = Math.Sqrt(-2*Math.Log(1-p));
                return -(((((C[0]*q+C[1])*q+C[2])*q+C[3])*q+C[4])*q+C[5]) /
                       ((((D[0]*q+D[1])*q+D[2])*q+D[3])*q+1);
            }

            // Rational approximation for central region:
            q = p - 0.5;
            double r = q*q;
            return (((((A[0]*r+A[1])*r+A[2])*r+A[3])*r+A[4])*r+A[5])*q /
                   (((((B[0]*r+B[1])*r+B[2])*r+B[3])*r+B[4])*r+1);
        }

        /// <summary>
        /// A lightweight static delegate for average=0.0, sigma=1.0.
        /// Note: We do NOT check for x in ]0,1[ here!
        /// You can also instantiate this class and then use
        /// instance.Value as well.
        /// </summary>
        public static double Function(double x)
        {
            double result;
            double temp = x - 0.5;
            if (Math.Abs(temp) < 0.42)
            {
                // Beasley and Springer, 1977
                result = temp * temp;
                return temp * (((A3 * result + A2) * result + A1) * result + A0) /
                       ((((B3 * result + B2) * result + B1) * result + B0) * result + 1.0);
            }
            // improved approximation for the tail (Moro 1995)
            if (x < 0.5)
                result = x;
            else
                result = 1.0 - x;
            result = Math.Log(-Math.Log(result));
            result = C0 + result * (C1 + result * (C2 + result * (C3 + result *
                                                                       (C4 + result * (C5 + result * (C6 + result * (C7 + result * C8)))))));
            if (x < 0.5)
                return -result;
            return result;
        }

        private const double A0 = 2.50662823884;
        private const double A1 = -18.61500062529;
        private const double A2 = 41.39119773534;
        private const double A3 = -25.44106049637;
        private const double B0 = -8.47351093090;
        private const double B1 = 23.08336743743;
        private const double B2 = -21.06224101826;
        private const double B3 = 3.13082909833;
        private const double C0 = 0.3374754822726147;
        private const double C1 = 0.9761690190917186;
        private const double C2 = 0.1607979714918209;
        private const double C3 = 0.0276438810333863;
        private const double C4 = 0.0038405729373609;
        private const double C5 = 0.0003951896511919;
        private const double C6 = 0.0000321767881768;
        private const double C7 = 0.0000002888167364;
        private const double C8 = 0.0000003960315187;
    }
}