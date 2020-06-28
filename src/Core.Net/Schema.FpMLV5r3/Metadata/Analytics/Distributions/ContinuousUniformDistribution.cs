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

namespace Highlander.Reporting.Analytics.V5r3.Distributions
{
    /// <summary>
    /// Provides generation of continuous uniformly distributed random numbers.
    /// </summary>
    /// <remarks>
    /// The implementation of the <see cref="ContinuousUniformDistribution"/> type bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Uniform_distribution_%28continuous%29">
    ///   Wikipedia - Uniform distribution (continuous)</a>.
    /// </remarks>
    public sealed class ContinuousUniformDistribution : ContinuousDistribution
    {
        private double _lower;
        private double _upper;
        private double _diff;

        #region Construction
        /// <summary>
        /// Initializes a new instance, using a <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        public ContinuousUniformDistribution()
        {
            SetDistributionParameters(0.0, 1.0);
        }

        /// <summary>
        /// Initializes a new instance, using the specified <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="Random"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public ContinuousUniformDistribution(Random random)
            : base(random)
        {
            SetDistributionParameters(0.0, 1.0);
        }

        /// <summary>
        /// Initializes a new instance, using a <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        public ContinuousUniformDistribution(double lowerLimit, double upperLimit)
        {
            SetDistributionParameters(lowerLimit, upperLimit);
        }

        /// <summary>
        /// Initializes a new instance, using a <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        public ContinuousUniformDistribution(double lowerLimit, double upperLimit, Random random)
            :base(random)
        {
            SetDistributionParameters(lowerLimit, upperLimit);
        }

        #endregion

        #region Distribution Parameters
        /// <summary>
        /// Gets or sets the lower limit parameter.
        /// To set all parameters at once consider using
        /// <see cref="SetDistributionParameters"/> instead.
        /// </summary>
        public double LowerLimit
        {
            get => _lower;
            set => SetDistributionParameters(value, _upper);
        }

        /// <summary>
        /// Gets or sets the upper limit parameter.
        /// To set all parameters at once consider using
        /// <see cref="SetDistributionParameters"/> instead.
        /// </summary>
        public double UpperLimit
        {
            get => _upper;
            set => SetDistributionParameters(_lower, value);
        }

        ///<summary>
        ///</summary>
        ///<param name="lowerLimit"></param>
        ///<param name="upperLimit"></param>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public void SetDistributionParameters(double lowerLimit, double upperLimit)
        {
            if(!IsValidParameterSet(lowerLimit, upperLimit))
                throw new ArgumentOutOfRangeException();

            _lower = lowerLimit;
            _upper = upperLimit;
            _diff = _upper - _lower;
        }

        /// <summary>
        /// Determines whether the specified parameters is valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if lowerLimit &lt;= upperLimit; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsValidParameterSet(double lowerLimit, double upperLimit)
        {
            return lowerLimit <= upperLimit;
        }
        #endregion

        #region Distribution Properties
        /// <summary>
        /// Gets the minimum possible value of generated random numbers.
        /// </summary>
        public override double Minimum => _lower;

        /// <summary>
        /// Gets the maximum possible value of generated random numbers.
        /// </summary>
        public override double Maximum => _upper;

        /// <summary>
        /// Gets the mean value of generated random numbers.
        /// </summary>
        public override double Mean => 0.5 * (_lower + _upper);

        /// <summary>
        /// Gets the median of generated random numbers.
        /// </summary>
        public override double Median => 0.5 * (_lower + _upper);

        /// <summary>
        /// Gets the variance of generated random numbers.
        /// </summary>
        public override double Variance => _diff * _diff / 12.0;

        /// <summary>
        /// Gets the skewness of generated random numbers.
        /// </summary>
        public override double Skewness => 0.0;

        public override double ProbabilityDensity(double x)
        {
            if(_lower <= x && x <= _upper)
                return 1.0 / _diff;
            return 0.0;
        }

        public override double CumulativeDistribution(double x)
        {
            if(x < _lower)
                return 0.0;
            if(x < _upper)
                return (x - _lower) / _diff;
            return 1.0;
        }

        /// <summary>
        /// Computes the inverse of the cumulative distribution function (InvCDF) for the distribution
        /// at the given probability. This is also known as the quantile or percent point function.
        /// </summary>
        /// <param name="p">The location at which to compute the inverse cumulative density.</param>
        /// <returns>the inverse cumulative density at <paramref name="p"/>.</returns>
        /// <seealso cref="InverseCumulativeDistribution"/>
        public double InverseCumulativeDistribution(double p)
        {
            return p <= 0.0 ? _lower : p >= 1.0 ? _upper : _lower * (1.0 - p) + _upper * p;
        }

        /// <summary>
        /// Generates a sample from the <c>ContinuousUniform</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public double Sample()
        {
            return SampleUnchecked(new Random(), _lower, _upper);
        }

        static double SampleUnchecked(Random rnd, double lower, double upper)
        {
            return lower + rnd.NextDouble() * (upper - lower);
        }
        #endregion

        #region Generator
        /// <summary>
        /// Returns a uniformly distributed floating point random number.
        /// </summary>
        /// <returns>A uniformly distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            return _lower + RandomSource.NextDouble() * _diff;
        }
        #endregion
    }
}