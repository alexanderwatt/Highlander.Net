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
using Highlander.Numerics.LinearAlgebra.Sparse;
using Highlander.Numerics.Statistics;

namespace Highlander.Numerics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Abstract base class for random path generators.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Black-Scholes equation with the the risk-free rate <i>r</i>, 
    /// volatility of the underlying <i>s</i> and <i>v=r-s<super>2</super>/2</i>
    /// has the form of a diffusion process. According to a heuristic 
    /// interpretation, paths followed by the logarithm of the underlying would
    /// be Brownian random walks with a constant drift <i>v</i> per unit time
    /// and a standard deviation <i>s*sqrt(T)</i> over a time <i>T</i>.
    /// </para>
    /// <para>
    /// Therefore, the paths to be generated for a Monte Carlo model of the
    /// Black-Scholes equation will be SparseVectors of successive variations of
    /// the logarithm of the underlying price over <i>M</i> consecutive time
    /// intervals <i>dt<sub>i</sub>, i = 0..M-1</i>.
    /// Each such variation will be drawn from a Gaussian distribution with
    /// average <i>v dt<sub>i</sub></i> and standard deviation
    /// <i>s sqrt(dt<sub>i</sub>)</i> - or possibly <i>v<sub>i</sub> dt<sub>i</sub></i>
    /// and <i>s<sub>i</sub> sqrt(dt<sub>i</sub>)</i> should <i>v</i> and 
    /// <i>s</i> vary in time.
    /// </para>
    /// <para>
    /// The <see cref="Path" /> class stores the variation SparseVector decomposed in 
    /// its drift (determined) and diffusion (random) components.
    /// This allows the implementation of antithetic variance reduction techniques.
    /// </para>
    /// <para>
    /// Derived classes are provided which generate paths and multi-paths with the 
    /// desired drift and diffusion components, namely, 
    /// <see cref="SinglePathGenerator"/> and <see cref="SinglePathGenerator"/>.
    /// </para>
    /// <para>
    /// The <see cref="MultiPathGenerator"/> is initialized with a constant
    /// drift and variance or time dependent parameters. 
    /// The <see cref="MultiPathGenerator"/> is initialized with an array of
    /// constant drifts - one for each single asset - and a covariance matrix
    /// which encapsulates the relations between the diffusion components of
    /// the single assets.
    /// </para>
    /// <para>
    /// The time discretization of the (multi)paths can be specified either as
    /// a given number of equal time steps over a given time span, or as a
    /// SparseVector of explicitly specified times at which the path will be sampled.
    /// </para>
    /// </remarks>
    public abstract class PathGenerator
    {
        /// <summary>
        /// Protected constructor for abstract base class.
        /// </summary>
        protected PathGenerator(int assets, int timeSteps)
        {
            if( assets <= 0) 
                throw new ArgumentException( "TODO: Number of assets must be > 0.");
            if( timeSteps <= 0) 
                throw new ArgumentException( "TODO: Time steps must be > 0.");
            Count = assets;
            TimeSteps = timeSteps;
            Drift = new SparseVector[assets];
        }

        /// <summary>
        /// Protected constructor for abstract base class.
        /// </summary>
        protected PathGenerator(SparseVector drifts, SparseVector times)
            : this(drifts.Length, times.Length)
        {
            if( times.Data[0] < 0.0 ) 
                throw new ArgumentException( "TODO: First time must be >= 0.");
            Times = times;
            TimeDelays = AdjacentDifference(times);
            for( int i=0; i<Count; i++)
                Drift[i] = /* SparseVector */ TimeDelays * drifts.Data[i];
        }

        /// <summary>
        /// Protected constructor for abstract base class.
        /// </summary>
        protected PathGenerator(SparseVector drifts, double time, int timeSteps)
            : this(drifts.Length, timeSteps)
        {
            if( time <= 0.0) 
                throw new ArgumentException( "TODO: Time must be > 0.");

            double dt = time/TimeSteps;
            Times = new SparseVector(TimeSteps, dt, dt);
            TimeDelays = new SparseVector( TimeSteps, dt);
            for( int i=0; i<Count; i++)
                Drift[i] = new SparseVector(TimeSteps, drifts.Data[i]*dt);
        }

        protected SparseVector Times, TimeDelays;
        protected SparseVector[] Drift;
        protected RandomArrayGenerator ArrayGenerator;

        /// <summary>
        /// Number of assets = number of paths generated by the path generator.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Number of times at which the path will be sampled.
        /// </summary>
        /// <remarks>
        /// Depending on the iinitialization, these could be equal time steps
        /// or a SparseVector of explicitly specified times at which the path will
        /// be sampled.
        /// </remarks>
        public int TimeSteps { get; }

        // returns Sample<Path[]>
        /// <summary>
        /// Generate next path.
        /// </summary>
        /// <remarks>
        /// Derived path generators must override this method to generate one or more
        /// <see cref="Path"/>es.
        /// </remarks>
        /// <returns>
        /// An array containing a single <see cref="Path"/>, 
        /// wrapped in a (possibly weighted) <see cref="Sample"/>.
        /// </returns>
        public abstract Sample Next();

        protected static SparseVector AdjacentDifference(SparseVector times)
        {
            int n = times.Length;
            SparseVector timeDelays = new SparseVector(n);
            double t = 0.0;
            for(int i=0; i<n; i++) 
            {
                double ti = times.Data[i];
                if( t>ti )
                    throw new ArgumentException( "TODO: Sample times are not in ascending order");
                timeDelays.AddValue(i,ti-t);
                t = ti;
            }
            return timeDelays;
        }
    }
}