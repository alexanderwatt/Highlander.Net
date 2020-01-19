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
    /// Declares common functionality for all continuous random number
    /// distributions based on a random source.
    /// </summary>
    public abstract class ContinuousDistribution : IContinuousProbabilityDistribution
    {
        #region instance fields
        /// <summary>
        /// Gets or sets a <see cref="RandomSource"/> object that can be used
        /// as underlying random number generator.
        /// </summary>
        public virtual Random RandomSource
        {
            get => _random;
            set => _random = value;
        }

        private Random _random;

        #endregion

        #region construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousDistribution"/> class, using a 
        /// <see cref="Random"/> as underlying random number generator.
        /// </summary>
        protected ContinuousDistribution()
            : this(new Random())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousDistribution"/> class, using the
        /// specified <see cref="RandomSource"/> as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        protected ContinuousDistribution(Random random)
        {
            if (random == null)
            {
                string message = string.Format(null, "ArgumentNull", "generator");
                throw new ArgumentNullException($"RandomNumberGenerator", message);
            }
            _random = random;
        }
        #endregion

        #region instance methods
        /// <summary>
        /// Resets the random number distribution, so that it produces the same random number sequence again.
        /// </summary>
        public void Reset()
        {
            _random = new Random();
        }
        #endregion

        #region abstract members
        /// <summary>
        /// Gets the minimum possible value of distributed random numbers.
        /// </summary>
        public abstract double Minimum
        {
            get;
        }

        /// <summary>
        /// Gets the maximum possible value of distributed random numbers.
        /// </summary>
        public abstract double Maximum
        {
            get;
        }

        /// <summary>
        /// Gets the mean of distributed random numbers.
        /// </summary>
        public abstract double Mean
        {
            get;
        }
		
        /// <summary>
        /// Gets the median of distributed random numbers.
        /// </summary>
        public abstract double Median
        {
            get;
        }

        /// <summary>
        /// Gets the variance of distributed random numbers.
        /// </summary>
        public abstract double Variance
        {
            get;
        }
		
        public abstract double Skewness
        {
            get;
        }
		
        /// <summary>
        /// Returns a distributed floating point random number.
        /// </summary>
        /// <returns>A distributed double-precision floating point number.</returns>
        public abstract double NextDouble();

        public abstract double ProbabilityDensity(double x);

        public abstract double CumulativeDistribution(double x);
        #endregion
    }
}