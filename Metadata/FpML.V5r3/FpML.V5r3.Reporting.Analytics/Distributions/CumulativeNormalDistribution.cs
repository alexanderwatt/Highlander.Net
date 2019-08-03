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

#region Usings

using System;
using System.Diagnostics;
using Orion.Analytics.Maths.Collections;
using Orion.Analytics.Solvers;

#endregion

namespace Orion.Analytics.Distributions
{
    /// <summary>Cumulative normal distribution.</summary>
    /// <remarks>
    /// <p>Returns the value (Maple)
    /// <c>int(1 / (sigma * sqrt(2 * Pi)) * exp(-(x - mu)^2/ (2 * sigma ^ 2)), x = -infinity..t)</c>
    /// for the specified <c>t</c>, <c>mu</c> and <c>sigma</c>.</p>
    /// <p>The <i>CND</i> is related to the <i>erf</i> with the formula
    /// <i>CND(x)=(erf(x/sqrt(2))+1)/2</i>.</p>
    /// <p>For more details about the cumulative normal distribution, look at
    /// the <a href="http://en.wikipedia.org/wiki/Erf">WikiPedia</a>.</p>
    /// <p>See <a href="http://www.library.cornell.edu/nr/">Numerical recipees in C</a> 
    /// (chapter 6) for the detail of the algorithm.</p>
    /// </remarks>
    public class CumulativeNormalDistribution : ContinuousDistribution, IUnaryFunction //, IRealFunction
    {
        // A small number close to the smallest representable floating point number
        private const double Fpmin = 1e-300;

        //  Lanczos Gamma Function approximation - N (number of coefficients -1)
        private const int LgfN = 6;

        //  Lanczos Gamma Function approximation - Coefficients
        private static readonly double[] LgfCoeff = { 1.000000000190015, 76.18009172947146, -86.50532032941677, 24.01409824083091, -1.231739572450155, 0.1208650973866179E-2, -0.5395239384953E-5 };

        //  Lanczos Gamma Function approximation - small gamma
        private const double LgfGamma = 5.0;

        //  Maximum number of iterations allowed in Incomplete Gamma Function calculations
        private const int Igfiter = 1000;

        //  Tolerance used in terminating series in Incomplete Gamma Function calculations
        private const double Igfeps = 1e-8;

        private readonly double _mean;
        private readonly double _sigma;

        /// <summary>
        /// Distribution corresponding to the standard normal distribution
        /// (mean equal to <c>0.0</c> and sigma equal to <c>1.0</c>).
        /// </summary>
        public CumulativeNormalDistribution()
        {
            _mean = 0.0;
            _sigma = 1.0;
        }

        /// <summary>
        /// Distribution corresponding to the standard normal distribution
        /// of given <c>mean</c>.
        /// </summary>
        public CumulativeNormalDistribution(double mean)
        {
            _mean = mean;
            _sigma = 1;
        }
        
        /// <summary>
        /// Distribution corresponding to the standard normal distribution
        /// of given <c>mean</c> and <c>sigma</c>.
        /// </summary>
        public CumulativeNormalDistribution(double mean, double sigma)
        {
            _mean = mean;
            _sigma = sigma;
        }

        private readonly NormalDistribution _gaussian = new NormalDistribution();

        /// <summary>
        /// Gets the cumulative normal distribution function.
        /// </summary>
        public double ValueOf(double x)
        {
            x = (x - _mean) / (Math.Sqrt(2) * _sigma);
            double erf = 0.0;
            if (x != 0.0)
            {
                if (double.IsPositiveInfinity(x))
                {
                    erf = 1.0;
                }
                else if (double.IsNegativeInfinity(x))
                {
                    erf = -1.0;
                }
                else
                {
                    if (x >= 0.0)
                    {
                        erf = IncompleteGamma(0.5, x * x);
                    }
                    else
                    {
                        erf = -IncompleteGamma(0.5, x * x);
                    }
                }
            }

            return (erf + 1.0) / 2.0;
        }

        // Incomplete Gamma Function P(a,x) = integral from zero to x of (exp(-t)t^(a-1))dt
        private static double IncompleteGamma(double a, double x)
        {
            Debug.Assert(a >= 0.0 && x >= 0.0);

            var igf = x < a + 1.0 ? IncompleteGammaSer(a, x) : IncompleteGammaFract(a, x);

            return igf;
        }

        // Incomplete Gamma Function P(a,x) = integral from zero to x of (exp(-t)t^(a-1))dt
        // Series representation of the function - valid for x < a + 1
        private static double IncompleteGammaSer(double a, double x)
        {
            Debug.Assert(a >= 0.0 && x >= 0.0);
            Debug.Assert(x < a + 1.0, "#E00 Continued Fraction Representation.");
            int i = 0;
            double igf = 0.0D;
            bool check = true;
            double acopy = a;
            double sum = 1.0 / a;
            double incr = sum;
            double loggamma = LogGamma(a);
            while (check)
            {
                ++i;
                ++a;
                incr *= x / a;
                sum += incr;
                if (Math.Abs(incr) < Math.Abs(sum) * Igfeps)
                {
                    igf = sum * Math.Exp(-x + acopy * Math.Log(x) - loggamma);
                    check = false;
                }
                if (i >= Igfiter)
                {
                    check = false;
                    igf = sum * Math.Exp(-x + acopy * Math.Log(x) - loggamma);
                }
            }
            return igf;
        }

        // Incomplete Gamma Function P(a,x) = integral from zero to x of (exp(-t)t^(a-1))dt
        // Continued Fraction representation of the function - valid for x >= a + 1
        // This method follows the general procedure used in Numerical Recipes for C,
        // The Art of Scientific Computing
        // by W H Press, S A Teukolsky, W T Vetterling & B P Flannery
        // Cambridge University Press,   http://www.nr.com/
        private static double IncompleteGammaFract(double a, double x)
        {
            Debug.Assert(a >= 0.0 && x >= 0.0);
            Debug.Assert(x >= a + 1, "#E00 Use Series Representation.");
            int i = 0;
            bool check = true;
            double loggamma = LogGamma(a);
            double denom = x - a + 1.0D;
            double first = 1.0 / denom;
            double term = 1.0 / Fpmin;
            double prod = first;
            while (check)
            {
                ++i;
                double ii = i;
                double numer = -ii * (ii - a);
                denom += 2.0D;
                first = numer * first + denom;
                if (Math.Abs(first) < Fpmin)
                {
                    first = Fpmin;
                }
                term = denom + numer / term;
                if (Math.Abs(term) < Fpmin)
                {
                    term = Fpmin;
                }
                first = 1.0 / first;
                double incr = first * term;
                prod *= incr;
                if (Math.Abs(incr - 1.0) < Igfeps) check = false;
                if (i >= Igfiter)
                {
                    check = false;
                }
            }
            double igf = 1.0 - Math.Exp(-x + a * Math.Log(x) - loggamma) * prod;
            return igf;
        }

        private static double LogGamma(double x)
        {
            double xcopy = x;
            double fg;
            double first = x + LgfGamma + 0.5;
            double second = LgfCoeff[0];
            if (x >= 0.0)
            {
                if (x >= 1.0 && x - (int)x == 0.0)
                {
                    fg = LogFactorial(x) - Math.Log(x);
                }
                else
                {
                    first -= (x + 0.5) * Math.Log(first);
                    for (int i = 1; i <= LgfN; i++) second += LgfCoeff[i] / ++xcopy;
                    fg = Math.Log(Math.Sqrt(2.0 * Math.PI) * second / x) - first;
                }
            }
            else
            {
                fg = Math.PI / (Gamma(1.0 - x) * Math.Sin(Math.PI * x));
                if (!double.IsInfinity(fg))
                {
                    if (fg < 0)
                    {
                        throw new ArgumentException("The gamma function is negative.");
                    }
                    fg = Math.Log(fg);
                }
            }

            return fg;
        }

        // Gamma function
        // Lanczos approximation (6 terms)
        private static double Gamma(double x)
        {
            double xcopy = x;
            double first = x + LgfGamma + 0.5;
            double second = LgfCoeff[0];
            double fg;
            if (x >= 0.0)
            {
                if (x >= 1.0 && x - (int)x == 0.0)
                {
                    fg = Factorial(x) / x;
                }
                else
                {
                    first = Math.Pow(first, x + 0.5) * Math.Exp(-first);
                    for (int i = 1; i <= LgfN; i++) second += LgfCoeff[i] / ++xcopy;
                    fg = first * Math.Sqrt(2.0 * Math.PI) * second / x;
                }
            }
            else
            {
                fg = -Math.PI / (x * Gamma(-x) * Math.Sin(Math.PI * x));
            }
            return fg;
        }

        // factorial of n
        // Argument is of type double but must be, numerically, an integer
        // factorial returned as double but is, numerically, should be an integer
        // numerical rounding may makes this an approximation after n = 21
        private static double Factorial(double n)
        {
            if (n < 0 || (n - (int)n) != 0)
                throw new ArgumentOutOfRangeException($"n must be a positive integer.");
            double f = 1.0D;
            int nn = (int)n;
            for (int i = 1; i <= nn; i++) f *= i;
            return f;
        }

        // log to base e of the factorial of n
        // Argument is of type double but must be, numerically, an integer
        // log[e](factorial) returned as double
        // numerical rounding may makes this an approximation
        private static double LogFactorial(double n)
        {
            if (n < 0 || (n - (int)n) != 0)
                throw new ArgumentOutOfRangeException($"n must be a positive integer.");
            double f = 0.0;
            int nn = (int)n;
            for (int i = 2; i <= nn; i++) f += Math.Log(i);
            return f;
        }

        /// <summary>
        /// A lightweight static delegate for average=0.0, sigma=1.0.
        /// You can also instatiate this class and then use
        /// instance.Value as well.
        /// </summary>
        public static double Function(double x)
        {
            if (x >= 0.0)
            {
                double k = 1.0 / (1.0 + gamma * x);
                double temp = NormalDistribution.Function(x) * k *
                              (A1 + k * (A2 + k * (A3 + k * (A4 + k * A5))));
                if (temp < Precision)
                    return 1.0;
                temp = 1.0 - temp;
                if (temp < Precision)
                    return 0.0;
                return temp;
            }
            return 1.0 - Function(-x);
        }

        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public static double FunctionDerivative(double x)
        {
            return NormalDistribution.Function(x);
        }

        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public double Derivative(double x)
        {
            return _gaussian.Value(x);
        }

        private const double A1 = 0.319381530;
        private const double A2 = -0.356563782;
        private const double A3 = 1.781477937;
        private const double A4 = -1.821255978;
        private const double A5 = 1.330274429;
        private const double gamma = 0.2316419;
        private const double Precision = 1e-6;

        public double Value(double x)
        {

            // 23.12.2002 jT
            // copied from changes in QL: not sure if this is necessary or even makes sense
#if DEBUG
            if ((x >= _mean) && (2.0 * _mean - x > _mean))
                throw new InvalidOperationException( 
                    "TODO: not a real number.");
#endif
            if (x >= _mean)
            {
                double xn = (x - _mean) / _sigma;
                double k = 1.0 / (1.0 + gamma * xn);
                double temp = _gaussian.Value(xn) * k *
                              (A1 + k * (A2 + k * (A3 + k * (A4 + k * A5))));
                if (temp < Precision)
                    return 1.0;
                temp = 1.0 - temp;
                if (temp < Precision)
                    return 0.0;
                return temp;
            }
            return 1.0 - Value(2.0 * _mean - x);
        }

        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public double Gaussian(double x)
        {
            double normFact = _sigma * Math.Sqrt(2 * Math.PI);
            double dx = x - _mean;
            return Math.Exp(-dx * dx / (2.0 * _sigma * _sigma)) / normFact;
        }

        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public double GaussianDerivative(double x)
        {
            double normFact = _sigma * _sigma * _sigma * Math.Sqrt(2 * Math.PI);
            double dx = x - _mean;
            return -dx * Math.Exp(-dx * dx / (2.0 * _sigma * _sigma)) / normFact;
        }

        ///<summary>
        ///</summary>
        ///<param name="f"></param>
        ///<param name="h"></param>
        ///<returns></returns>
        public double Norm(DoubleVector f, double h)
        {
            double sum = 0.0;
            foreach (double d in f)
                sum += d * d;
            // numeric integral of f^2
            double res = h * (sum - 0.5 * f[0] * f[0] -
                              0.5 * f[f.Count - 1] * f[f.Count - 1]);
            return Math.Sqrt(res);
        }

        public override double Median => throw new NotImplementedException();

        public override double Variance => _sigma;

        public override double Skewness => throw new NotImplementedException();

        public override double NextDouble()
        {
            throw new NotImplementedException();
        }

        public override double ProbabilityDensity(double x)
        {
            throw new NotImplementedException();
        }

        public override double CumulativeDistribution(double x)
        {
            throw new NotImplementedException();
        }

        public override double Minimum => throw new NotImplementedException();

        public override double Maximum => throw new NotImplementedException();

        public override double Mean => _mean;
    }
}