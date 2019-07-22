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
using Highlander.Numerics.Maths;

namespace Highlander.Numerics.Distributions
{
    /// <summary>
    /// Provides generation of lognormal distributed random numbers.
    /// </summary>
    /// <remarks>
    /// The implementation of the <see cref="LognormalDistribution"/> type bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Log-normal_distribution">Wikipedia - Lognormal Distribution</a> and
    ///   the implementation in the <a href="http://www.boost.org/libs/random/index.html">Boost Random Number Library</a>.
    /// </remarks>
    public sealed class LognormalDistribution : ContinuousDistribution
    {
        private double _mu;
        private double _sigma;
        private readonly StandardDistribution _standard;
        private double _sigma2;

        #region Construction
        /// <summary>
        /// Initializes a new instance, using a <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        public LognormalDistribution()
        {
            SetDistributionParameters(0.0, 1.0);
            _standard = new StandardDistribution();
        }

        /// <summary>
        /// Initializes a new instance, using the specified <see cref="RandomSource"/>
        /// as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public LognormalDistribution(Random random)
            : base(random)
        {
            SetDistributionParameters(0.0, 1.0);
            _standard = new StandardDistribution(random);
        }

        /// <summary>
        /// Initializes a new instance, using a <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        public LognormalDistribution(double mu, double sigma)
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
            _sigma2 = sigma * sigma;
        }

        /// <summary>
        /// Determines whether the specified parameters is valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if sigma is greater than 0.0; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsValidParameterSet(double mu, double sigma)
        {
            return sigma > 0.0;
        }
        #endregion

        #region Distribution Properties
        /// <summary>
        /// Gets the minimum possible value of generated random numbers.
        /// </summary>
        public override double Minimum => 0.0;

        /// <summary>
        /// Gets the maximum possible value of generated random numbers.
        /// </summary>
        public override double Maximum => double.MaxValue;

        /// <summary>
        /// Gets the mean value of generated random numbers.
        /// </summary>
        public override double Mean => Math.Exp(_mu + 0.5 * _sigma2);

        /// <summary>
        /// Gets the median of generated random numbers.
        /// </summary>
        public override double Median => Math.Exp(_mu);

        /// <summary>
        /// Gets the variance of generated random numbers.
        /// </summary>
        public override double Variance => (Math.Exp(_sigma2) - 1.0) * Math.Exp(_mu + _mu + _sigma2);

        /// <summary>
        /// Gets the skewness of generated random numbers.
        /// </summary>
        public override double Skewness
        {
            get
            {
                double expsigma2 = Math.Exp(_sigma2);
                return (expsigma2 + 2.0) * Math.Sqrt(expsigma2 - 1);
            }
        }

        public override double ProbabilityDensity(double x)
        {
            double a = (Math.Log(x) - _mu) / _sigma;
            return Math.Exp(-0.5 * a * a) / (x * _sigma * Constants.Sqrt2Pi);
        }

        public override double CumulativeDistribution(double x)
        {
            return 0.5 * (1.0 + Fn.Erf((Math.Log(x) - _mu) / (_sigma * Constants.Sqrt2)));
        }
        #endregion

        #region Generator
        /// <summary>
        /// Returns a lognormal distributed floating point random number.
        /// </summary>
        /// <returns>A lognormal distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            return Math.Exp(_standard.NextDouble() * _sigma + _mu);
        }
        #endregion
    }
}