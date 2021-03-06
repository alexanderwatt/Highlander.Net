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
using Highlander.Reporting.Analytics.V5r3.Maths;

namespace Highlander.Reporting.Analytics.V5r3.Distributions
{
    /// <summary>
    /// Pseudo-random generation of standard distributed deviates.
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
    /// <para>pdf: f(x) = 1/sqrt(2*Pi)*exp(-x^2/2)</para>
    /// </remarks>
    public sealed class StandardDistribution : ContinuousDistribution
    {
        double? _extraNormal;
        
        #region Construction
        /// <summary>
        /// Initializes a new instance, using a <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        public StandardDistribution()
        {
        }

        /// <summary>
        /// Initializes a new instance, using the specified <see cref="Random"/>
        /// as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="Random"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public StandardDistribution(Random random)
            : base(random)
        {
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
        public override double Mean => 0.0;

        /// <summary>
        /// Gets the median of generated random numbers.
        /// </summary>
        public override double Median => 0.0;

        /// <summary>
        /// Gets the variance of generated random numbers.
        /// </summary>
        public override double Variance => 1.0;

        /// <summary>
        /// Gets the skewness of generated random numbers.
        /// </summary>
        public override double Skewness => 0.0;

        public override double  ProbabilityDensity(double x)
        {
            return Maths.Constants.InvSqrt2Pi * Math.Exp(x * x / -2.0);
        }

        public override double CumulativeDistribution(double x)
        {
            return 0.5 * (1.0 + Fn.Erf(x * Maths.Constants.Sqrt12));
        }

        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public static double InverseCumulativeDistribution(double x)
        {
            return Maths.Constants.Sqrt12 * Fn.ErfInverse(2.0 * x - 1.0);
        }
        #endregion

        #region Generator
        /// <summary>
        /// Returns a standard distributed floating point random number.
        /// </summary>
        /// <returns>A standard distributed double-precision floating point number.</returns>
        public override double NextDouble()
        {
            // Note that this method generate two gaussian deviates
            // at once. The additional deviate is recorded in
            // <c>extraNormal</c>.
            // Using the extra gaussian deviate if available
            if(_extraNormal.HasValue)
            {
                double extraNormalCpy = _extraNormal.Value;
                _extraNormal = null;
                return extraNormalCpy;
            }
                // Generating two new gaussian deviates
            double rsq, v1, v2;
            // We need a non-zero random point inside the unit circle.
            do
            {
                v1 = 2.0 * RandomSource.NextDouble() - 1.0;
                v2 = 2.0 * RandomSource.NextDouble() - 1.0;
                rsq = v1 * v1 + v2 * v2;
            }
            while(rsq > 1.0 || rsq == 0);
            // Make the Box-Muller transformation
            var fac = Math.Sqrt(-2.0 * Math.Log(rsq) / rsq);
            _extraNormal = v1 * fac;
            return (v2 * fac);
        }
        #endregion
    }
}