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
using Orion.Analytics.Maths;

namespace Orion.Analytics.Distributions
{
    /// <summary>
    /// Pseudo-random generation of normal distributed deviates.
    /// </summary>
    /// 
    /// <remarks> 
    /// <para>For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Normal_distribution">
    /// Wikipedia - Normal distribution</a>.</para>
    /// 
    /// <para>This implementation is based on the <i>Box-Muller</i> algorithm
    /// for generating random deviates with a normal distribution.</para>
    /// 
    /// <para>For details of the algorithm, see
    /// <a href="http://www.library.cornell.edu/nr/">
    /// Numerical recipes in C</a> (chapter 7)</para>
    ///
    /// <para>pdf: f(x) = 1/(s*sqrt(2*Pi))*exp(-(x-m)^2/(2*s^2)); m = mu (location), s = sigma (scale)</para>
    /// </remarks>
    public sealed class NormalDistribution : ContinuousDistribution, IUnaryFunction//, IRealFunction
    {
        private double _mu;
        private double _sigma;
        private readonly StandardDistribution _standard;

        #region Construction
        /// <summary>
        /// Initializes a new instance, using a <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        public NormalDistribution()
        {
            SetDistributionParameters(0.0, 1.0);
            _standard = new StandardDistribution(RandomSource);
        }

        /// <summary>
        /// Initializes a new instance, using the specified <see cref="RandomSource"/>
        /// as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public NormalDistribution(Random random)
            : base(random)
        {
            SetDistributionParameters(0.0, 1.0);
            _standard = new StandardDistribution(random);
        }

        /// <summary>
        /// Initializes a new instance, using a <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        public NormalDistribution(double mu, double sigma)
        {
            SetDistributionParameters(mu, sigma);
            _standard = new StandardDistribution(RandomSource);
        }
        #endregion

        public override Random RandomSource
        {
            set
            {
                base.RandomSource = value;
                _standard.RandomSource = value;
            }
        }

        #region Distribution Parameters
        /// <summary>
        /// Gets or sets the mu parameter.
        /// </summary>
        public double Mu
        {
            get => _mu;
            set => SetDistributionParameters(value, _sigma);
        }

        /// <summary>
        /// Gets or sets the sigma parameter.
        /// </summary>
        public double Sigma
        {
            get => _sigma;
            set => SetDistributionParameters(_mu, value);
        }

        ///<summary>
        ///</summary>
        ///<param name="mu"></param>
        ///<param name="sigma"></param>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public void SetDistributionParameters(double mu, double sigma)
        {
            if(!IsValidParameterSet(mu, sigma))
                throw new ArgumentOutOfRangeException();

            _mu = mu;
            _sigma = sigma;
        }

        /// <summary>
        /// Determines whether the specified parameters is valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if sigma is greater than 0.0; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidParameterSet(double mu, double sigma)
        {
            return sigma >= 0.0;
        }

        #endregion

        #region Distribution Properties
        /// <summary>
        /// Gets the minimum possible value of generated random numbers.
        /// </summary>
        public override double Minimum => double.MinValue;

        /// <summary>
        /// Gets the maximum possible value of generated random numbers.
        /// </summary>
        public override double Maximum => double.MaxValue;

        /// <summary>
        /// Gets the mean value of generated random numbers.
        /// </summary>
        public override double Mean => _mu;

        /// <summary>
        /// Gets the median of generated random numbers.
        /// </summary>
        public override double Median => _mu;

        /// <summary>
        /// Gets the variance of generated random numbers.
        /// </summary>
        public override double Variance => _sigma * _sigma;

        /// <summary>
        /// Gets the skewness of generated random numbers.
        /// </summary>
        public override double Skewness => 0.0;

        public override double ProbabilityDensity(double x)
        {
            double xmu = x - _mu;
            return Maths.Constants.InvSqrt2Pi / _sigma * Math.Exp(xmu * xmu / (-2.0 * _sigma * _sigma));
        }

        public override double CumulativeDistribution(double x)
        {
            // Declare and initialise the return variable.
            double result;

            // Constants required to evaluate the cumulative distribution 
            // function for the standard normal variable.
            const double upperBound = 8;

            // Evaluate the cumulative normal distribution.
            if (x < -upperBound)
            {
                // Left boundary.
                result = 0.0;
            }
            else if (x > upperBound)
            {
                // Right boundary.
                result = 1.0;
            }
            else
            {
                // Generic case.
                double stdDevs = x / Math.Sqrt(upperBound);
                const double mult = -0.0625; // = -0.5 / UpperBound;
                const int maxIterations = 25;
                result = 0.5 * stdDevs;
                int n = 1;
                double oldResult;
                do
                {
                    oldResult = result;
                    result += Math.Exp(mult * n * n) * Math.Sin(n * stdDevs) / n;
                    n++;
                } while (result != oldResult && n <= maxIterations);
                result = 0.5 + result / Math.PI;
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public double InverseCumulativeDistribution(double x)
        {
            return _sigma * Maths.Constants.Sqrt12 * Fn.ErfInverse(2.0 * x - 1.0) + _mu;
        }
        #endregion

        #region Generator
        /// <summary>
        /// Returns a normal/gaussion distributed floating point random number.
        /// </summary>
        /// <returns>A normal distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            return _mu + _standard.NextDouble() * _sigma;
        }
        #endregion

        #region IUnaryFunction

        // standard normal density function
        public static double Ndf(double t)
        {
            return 0.398942280401433D * Math.Exp(-t * t / 2);
        }

        /// <summary>
        /// Normal distribution probabilities accurate to 1.e-15. 
        /// Z = no. of standard deviations from the mean. 
        /// 
        /// Based upon algorithm 5666 for the error function, from: 
        /// Hart, J.F. et al, 'Computer Approximations', Wiley 1968 
        /// 
        /// Programmer: Alan Miller 
        /// 
        /// Latest revision - 30 March 1986 
        /// </summary>
        /// <param name="numberOfStandardDeviations">The number Of Standard Deviations from the mean.</param>
        /// <returns></returns>
        public static double Probability(double numberOfStandardDeviations)
        {
            double p;

            const double p0 = 220.206867912376;
            const double p1 = 221.213596169931;
            const double p2 = 112.079291497871;
            const double p3 = 33.912866078383;
            const double p4 = 6.37396220353165;
            const double p5 = 0.700383064443688;
            const double p6 = 0.0352624965998911;

            const double q0 = 440.413735824752;
            const double q1 = 793.826512519948;
            const double q2 = 637.333633378831;
            const double q3 = 296.564248779674;
            const double q4 = 86.7807322029461;
            const double q5 = 16.064177579207;
            const double q6 = 1.75566716318264;
            const double q7 = 0.0883883476483184;

            const double rootpi = 2.506628274631;
            const double cutoff = 7.07106781186547;
            // 
            var zabs = Math.Abs(numberOfStandardDeviations);
            // 
            // |Z| > 37 
            // 
            if (zabs > 37)
            {
                p = 0;
            }
            else
            {
                // 
                // |Z| <= 37 
                // 
                var expntl = Math.Exp(-Math.Pow(zabs, 2) / 2);
                // 
                // |Z| < CUTOFF = 10/SQRT(2) 
                // 
                if (zabs < cutoff)
                {
                    p = expntl * ((((((p6 * zabs + p5) * zabs + p4) * zabs + p3) * zabs + p2) * zabs + p1) * zabs + p0) / (((((((q7 * zabs + q6) * zabs + q5) * zabs + q4) * zabs + q3) * zabs + q2) * zabs + q1) * zabs + q0);
                }
                // 
                // |Z| >= CUTOFF. 
                // 
                else
                {
                    //*** Grant: Fix to handle overflow 

                    //*** Original Code 
                    //P = EXPNTL / (ZABS + 1 / (ZABS + 2 / (ZABS + 3 / (ZABS + 4 / (ZABS + 0.65))))) / ROOTPI 

                    p = 4 / (zabs + 0.65);
                    p = 3 / (zabs + p);
                    p = 2 / (zabs + p);
                    p = 1 / (zabs + p);
                    p = expntl / (zabs + p) / rootpi;
                }
            }

            if (numberOfStandardDeviations > 0)
            {
                p = 1 - p;
            }

            return p;

        }

        public double Value(double x)
        {
            var normalizationFactor = 1.0 / (_sigma * Math.Sqrt(2.0 * Math.PI));
            var denominator = 2.0 * _sigma * _sigma;
            double deltax = x - Mean;
            double exponent = -deltax * deltax / denominator;
            // debian alpha had some strange problem in the very-low range
            return exponent <= -690.0 ? 0.0 :  // exp(x) < 1.0e-300 anyway
                normalizationFactor * Math.Exp(exponent);
        }

        // IRealFunction
        public double ValueOf(double x)
        {
            var normalizationFactor = 1.0 / (_sigma * Math.Sqrt(2.0 * Math.PI));
            var denominator = 2.0 * _sigma * _sigma;
            double deltax = x - Mean;
            double exponent = -deltax * deltax / denominator;
            // debian alpha had some strange problem in the very-low range
            return exponent <= -690.0 ? 0.0 :  // exp(x) < 1.0e-300 anyway
                normalizationFactor * Math.Exp(exponent);
        }

        /// <summary>
        /// A lightweight static delegate for average=0.0, sigma=1.0. 
        /// You can also instatiate this class and then use
        /// instance.Value as well.
        /// </summary>
        public static double Function(double x)
        {
            double exponent = -x * x / 2.0;
            // debian alpha had some strange problem in the very-low range
            return exponent <= -690.0 ? 0.0 :  // exp(x) < 1.0e-300 anyway
                1.0 / (Math.Sqrt(2.0 * Math.PI)) * Math.Exp(exponent);
        }

        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public double Derivative(double x)
        {
            return Value(x) * (Mean - x) / _sigma;
        }

        /// <summary>
        /// Cumulative bivariate normal distribution,
        /// N_2 (x_1, x_2; \rho) =
        /// {1 \over 2\pi\sqrt{1-\rho^2}} \int_{-\infty}^{x_1} dx\int_{-\infty}^{x_2} dy
        /// exp(-{1\over 2}{(x^2 - 2\rho xy + y^2 \over 1-\rho^2)})
        /// where \rho is the correlation coefficient.
        /// This is needed to value options on options and complex choosers.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="corr"></param>
        /// <returns></returns>
        public double BivariateNormal(double x1, double x2, double corr)
        {
            const double OO2PI = 0.159154943091895;  // 1/(2.pi}

            if (corr < -1 || corr > 1) throw new Exception("Correlation must be between -1 and 1");
            if (corr == -1) return x1 > -x2 ? CumulativeDistribution(x1) - CumulativeDistribution(-x2) : 0;
            if (corr == 1) return CumulativeDistribution(Math.Min(x1, x2));
            var s = 1 / Math.Sqrt(1 - corr * corr);
            if (x1 == 0)
            {
                if (x2 == 0) return 0.25 + OO2PI * Math.Atan(corr * s) * s;
                return (x2 > 0 ? 0.5 : 0) - OO2PI * Math.Exp(-x2 * x2 / 2) * SimpsonsRuleIntegration.Value(-corr * s * x2, x2);
            }
            if (x2 == 0)
                return (x1 > 0 ? 0.5 : 0) - OO2PI * Math.Exp(-x1 * x1 / 2) * SimpsonsRuleIntegration.Value(-corr * s * x1, x1);
            return (x1 < 0 || x2 < 0 ? 0 : 1) - OO2PI * (
               Math.Exp(-x2 * x2 / 2) * SimpsonsRuleIntegration.Value(s * (x1 - corr * x2), x2) +
               Math.Exp(-x1 * x1 / 2) * SimpsonsRuleIntegration.Value(s * (x2 - corr * x1), x1));
        }

        #endregion
    }
}