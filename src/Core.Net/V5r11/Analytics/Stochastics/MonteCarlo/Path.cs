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
using Highlander.Reporting.Analytics.V5r3.LinearAlgebra.Sparse;

namespace Highlander.Reporting.Analytics.V5r3.Stochastics.MonteCarlo
{
    /// <summary>
    /// Single factor random walk.
    /// </summary>
    /// <remarks>
    /// This class stores the variation SparseVector decomposed in its 
    /// <see cref="Drift"/> (determined) and <see cref="Diffusion"/> (random) 
    /// components. 
    /// <para>
    /// This allows the implementation of antithetic variance reduction techniques.
    /// In short, such techniques consist in pricing an option on both the
    /// given path and its antithetic, the latter being a path with the same
    /// drift and the opposite diffusion component. The value of the sample
    /// is defined as the average of the prices on the two paths.
    /// </para>
    /// <para>
    /// The term <em>path</em> will be used as meaning either path or multi-path
    /// depending on the context. The term <em>single path</em> is not to be taken
    /// as opposite to multi-path, but rather as meaning "a single instance of a 
    /// (multi)path" as opposed to the set of all generated (multi)paths.
    /// <see cref="PathPricer"/>s are used to price the generated path.
    /// </para>
    /// </remarks>
    public class Path
    {
        //			public Path(int size)
        //			{
        //				QL_RequireArgRange(size>0, "size", "TODO", size);
        //				this.times = new SparseVector(size);
        //				this.drift = new SparseVector(size);
        //				this.diffusion = new SparseVector(size);
        //			}

        /// <summary>
        /// Initialize a new Path.
        /// </summary>
        /// <param name="times">
        /// Discrete times at which the path was sampled.
        /// </param>
        /// <param name="drift">Deterministic components.</param>
        /// <param name="diffusion">Random components.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the times, drifts and diffusion SparseVectors have
        /// not the same length.
        /// </exception>
        public Path(SparseVector times, SparseVector drift, SparseVector diffusion)
        {
            if (times.Length != drift.Length)
                throw new ArgumentException( "TODO: times and drift SparseVectors have different size." );
            if (times.Length != diffusion.Length)
                throw new ArgumentException( "TODO: times and diffusion SparseVectors have different size." );
            Times = times;
            Drift = drift;
            _diffusion = diffusion;
        }

        private readonly SparseVector _diffusion;

        /// <summary>
        /// The added value of the deterministic and random components, sampled
        /// at the discrete values in <see cref="Times"/>.
        /// </summary>
        public double this[int i] => Drift[i] + _diffusion[i];

        /// <summary>
        /// Number of time steps / samples.
        /// </summary>
        public int Count => Drift.Length;

        /// <summary>
        /// Discrete times <see cref="SparseVector"/> at which the path was sampled.
        /// </summary>
        public SparseVector Times
        {
            get;
            //				set { times = value; }
        }

        /// <summary>
        /// Drift <see cref="SparseVector"/>.
        /// </summary>
        public SparseVector Drift
        {
            get;
            //				set { drift = value; }
        }

        /// <summary>
        /// Diffusion <see cref="SparseVector"/>.
        /// </summary>
        public SparseVector Diffusion => _diffusion;
    }
}