using System;
using Orion.Analytics.Distributions;
using Orion.Analytics.LinearAlgebra.Sparse;
using Orion.Analytics.Statistics;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Generates a single random path from a continous RNG.
    /// </summary>
    /// <remarks>
    /// See introduction to <see cref="PathGenerator"/> base class for more
    /// information about path generators.
    /// <para>
    /// The <see cref="SinglePathGenerator"/> is initialized with a constant
    /// drift and variance or time dependent parameters. 
    /// </para>
    /// <para>
    /// The time discretization of the paths can be specified either as
    /// a given number of equal time steps over a given time span, or as a
    /// SparseVector of explicitly specified times at which the path will be sampled.
    /// </para>
    /// </remarks>
    public class SinglePathGenerator : PathGenerator
    {
        /// <overloads>
        /// Initialize a new SinglePathGenerator.
        /// </overloads>
        /// <summary>
        /// Initialize a new SinglePathGenerator with constant drift and variance,
        /// sampled at the given number of equal time steps.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="drift">
        /// Constant drift of the Brownian random walks.
        /// </param>
        /// <param name="variance">
        /// Constant variance.
        /// </param>
        /// <param name="time">
        /// Time span which is sampled in <paramref name="timeSteps"/> equal time steps.
        /// </param>
        /// <param name="timeSteps">
        /// Number of equal time steps at which the path will be sampled.
        /// </param>
        public SinglePathGenerator(IContinuousRng generator,
                                   double drift, double variance, double time, int timeSteps)
            : base(new SparseVector(1, drift), time, timeSteps)
        {
            if( variance < 0.0 )
                throw new ArgumentException( "TODO: Variance can not be negative.");
            ArrayGenerator = new RandomArrayGenerator( generator, 
                                                            new SparseVector(TimeSteps, variance * time/TimeSteps ));
        }

        /// <summary>
        /// Initialize a new SinglePathGenerator with constant drift and variance.
        /// </summary>
        /// <remarks>
        /// The initial time is assumed to be zero and must 
        /// <b>not</b> be included in the passed SparseVector.
        /// </remarks>
        /// <param name="generator">The generator.</param>
        /// <param name="drift">
        /// Constant drift of the Brownian random walks.
        /// </param>
        /// <param name="variance">
        /// Constant variance.
        /// </param>
        /// <param name="times">
        /// A <see cref="SparseVector"/> of explicitly specified times at which the
        /// path will be sampled. 
        /// The initial time is assumed to be zero and must 
        /// <b>not</b> be included in the passed SparseVector.
        /// </param>
        public SinglePathGenerator(IContinuousRng generator,
                                   double drift, double variance, SparseVector times) 
            : base(new SparseVector(1, drift), times)
        {
            if( variance < 0.0 )
                throw new ArgumentException( "TODO: Variance can not be negative.");
            ArrayGenerator = new RandomArrayGenerator( generator, 
                                                            TimeDelays * variance); 
        }

        /// <summary>
        /// Initialize a new SinglePathGenerator with a zero drift and time dependent path.
        /// </summary>
        /// <remarks>
        /// The initial time is assumed to be zero and must 
        /// <b>not</b> be included in the passed SparseVector.
        /// </remarks>
        /// <param name="generator">The generator.</param>        
        /// <param name="variance">
        /// A <see cref="SparseVector"/> of time-dependent variances.
        /// </param>
        /// <param name="times">
        /// A <see cref="SparseVector"/> of explicitly specified times at which the
        /// path will be sampled. 
        /// The initial time is assumed to be zero and must 
        /// <b>not</b> be included in the passed SparseVector.
        /// </param>
        public SinglePathGenerator( IContinuousRng generator,
                                    SparseVector variance, SparseVector times) 
            : base(1, times.Length)
        {
            if( times[0] < 0.0 )
                throw new ArgumentException( "TODO: First time can not be negative (times[0] must be >= 0.0)." );
            if( variance.Length != TimeSteps )
                throw new ArgumentException( "TODO: Size mismatch between variance and time SparseVectors.");           
            Times = times;
            TimeDelays = AdjacentDifference(times);
            Drift[0] = new SparseVector(times.Length);
            ArrayGenerator = new RandomArrayGenerator( generator, 
                                                            TimeDelays * variance); 
        }

        public SinglePathGenerator(IContinuousRng generator,
                                   SparseVector drift, SparseVector variance, SparseVector times)
            : base(1, times.Length)
        {
            if (times[0] < 0.0)
                throw new ArgumentException("TODO: First time can not be negative (times[0] must be >= 0.0).");
            if (variance.Length != TimeSteps)
                throw new ArgumentException("TODO: Size mismatch between variance and time SparseVectors.");
            if (drift.Length != TimeSteps)
                throw new ArgumentException("TODO: Size mismatch between drift and time SparseVectors.");
            Times = times;
            TimeDelays = AdjacentDifference(times);
            Drift[0] = TimeDelays * drift;
            ArrayGenerator = new RandomArrayGenerator(generator,
                                                            TimeDelays * variance);
        }

        /// <summary>
        /// Generate next path.
        /// </summary>
        /// <returns>
        /// An array containing a single <see cref="Path"/>, 
        /// wrapped in a (possibly weighted) <see cref="Sample"/>.
        /// </returns>
        public override Sample Next()
        {
            var pathes = new Path[1];
            var sample = ArrayGenerator.NextSample();  // get a weighted SparseVector
            pathes[0] = new Path(Times, Drift[0], (SparseVector)sample.Value);
            return new Sample( pathes, sample.Weight);
        }
    }
}