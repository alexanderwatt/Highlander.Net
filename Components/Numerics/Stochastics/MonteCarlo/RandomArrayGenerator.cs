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
using Highlander.Numerics.Distributions;
using Highlander.Numerics.LinearAlgebra;
using Highlander.Numerics.LinearAlgebra.Sparse;
using Highlander.Numerics.Statistics;

namespace Highlander.Numerics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Generates random arrays from a random number generator.
    /// </summary>
    public class RandomArrayGenerator
    {
        /// <summary>
        /// equal average, different variances, no covariance
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="variance"></param>
        public RandomArrayGenerator(
            IContinuousRng generator, SparseVector variance)
        {
            Count = variance.Length;
            _generator = generator;
            // TODO: replace by VML equiv. and check return 
            // status for exceptions
            _sqrtVariance = SparseVector.Sqrt(variance);
            for(int i=0; i<_sqrtVariance.Length; i++)
            {
                if( variance[i] < 0.0)
                    throw new ArgumentOutOfRangeException( nameof(generator));
            }
        }

        /// <summary>
        /// different averages, different variances, covariance
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="covariance"></param>
        public RandomArrayGenerator(
            IContinuousRng generator, Matrix covariance)
        {
            if( covariance.RowCount != covariance.ColumnCount )
                throw new ArgumentException( "TODO: Covariance matrix must be square.");
            if (covariance.RowCount == 0)
                throw new ArgumentException( "TODO: Null covariance matrix given.");
            Count = covariance.RowCount;
            _generator = generator;
            _sqrtCovariance = Matrix.Sqrt(covariance);
        }

        private readonly IContinuousRng _generator;
        private readonly SparseVector _sqrtVariance;
        private readonly Matrix _sqrtCovariance;

        ///<summary>
        ///</summary>
        public int Count { get; }

        ///<summary>
        ///</summary>
        public bool IsConstantWeight { get; } = true;

        /// <summary>
        /// Generate a <see cref="Sample"/> containing
        /// a <see cref="SparseVector"/> of random numbers.
        /// </summary>
        /// <returns>A <see cref="SparseVector"/> of random numbers.</returns>
        public Sample NextSample()
        {
            // generate and consume the necessary samples
            // TODO: SparseVector.Random(generator)
            var r = new double[Count];
            _generator.Next(r);
            var samples = new SparseVector( r, false /* no copy, consume */);
            if (_sqrtCovariance != null)
                samples = _sqrtCovariance * samples;		// general case
            else
                samples.Multiply(_sqrtVariance);			// degenerate case
            return new Sample( samples );
        }
    }
}