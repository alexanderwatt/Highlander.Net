using System;
using Orion.Analytics.Distributions;
using Orion.Analytics.LinearAlgebra;
using Orion.Analytics.LinearAlgebra.Sparse;
using Orion.Analytics.Statistics;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Generates multiple random pathes from a continous RNG.
    /// </summary>
    /// <remarks>
    /// See introduction to <see cref="PathGenerator"/> base class for more
    /// information about path generators.
    /// <para>
    /// The <see cref="MultiPathGenerator"/> is initialized with an array of
    /// constant drifts - one for each single asset - and a covariance matrix
    /// which encapsulates the relations between the diffusion components of
    /// the single assets.
    /// </para>
    /// <para>
    /// The time discretization of the paths can be specified either as
    /// a given number of equal time steps over a given time span, or as a
    /// SparseVector of explicitly specified times at which the path will be sampled.
    /// </para>
    /// </remarks>
    public class MultiPathGenerator : PathGenerator
    {
        /// <overloads>
        /// Initialize a new MultiPathGenerator.
        /// </overloads>
        /// <summary>
        /// Initialize a new MultiPathGenerator which samples in equal time steps.
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="drifts">
        /// A <see cref="SparseVector"/> of constant drifts - one for each single asset.
        /// </param>
        /// <param name="covariance">
        /// A covariance <see cref="Matrix"/> which encapsulates the relations between
        /// the diffusion components of the single assets.
        /// </param>
        /// <param name="time">
        /// Time span which is sampled in <paramref name="timeSteps"/> equal time steps.
        /// </param>
        /// <param name="timeSteps">
        /// Number of equal time steps at which the path will be sampled.
        /// </param>
        public MultiPathGenerator(IContinuousRng generator,
                                  SparseVector drifts, Matrix covariance, double time, int timeSteps)
            : base(drifts, time, timeSteps)
        {
            InitializeGenerator(generator, covariance);		
        }

        /// <summary>
        /// Initialize a new MultiPathGenerator with constant drifts and variances.
        /// </summary>
        /// <summary>
        /// Initialize a new MultiPathGenerator which samples at explicitly specified times.
        /// </summary>
        /// <remarks>
        /// The initial time is assumed to be zero and must 
        /// <b>not</b> be included in the passed SparseVector.
        /// </remarks>
        /// <param name="generator"></param>
        /// <param name="drifts">
        /// A <see cref="SparseVector"/> of constant drifts - one for each single asset.
        /// </param>
        /// <param name="covariance">
        /// A covariance <see cref="Matrix"/> which encapsulates the relations between
        /// the diffusion components of the single assets.
        /// </param>
        /// <param name="times">
        /// A <see cref="SparseVector"/> of explicitly specified times at which the
        /// path will be sampled. 
        /// The initial time is assumed to be zero and must 
        /// <b>not</b> be included in the passed SparseVector.
        /// </param>
        public MultiPathGenerator(IContinuousRng generator,
                                  SparseVector drifts, Matrix covariance, SparseVector times) 
            : base(drifts, times)
        {
            InitializeGenerator(generator, covariance);
        }

        private void InitializeGenerator(IContinuousRng generator, Matrix covariance)
        {
            SparseVector diagonal = covariance.Diagonal;
            if( Count != diagonal.Length )
                throw new ArgumentException( "TODO: Covariance matrix does not match number of assets" );
            if( diagonal.Min() < 0.0)
                throw new ArgumentException( "TODO: Covariance matrix contains negative variance.");

            ArrayGenerator = new RandomArrayGenerator( generator, covariance);
        }

        /// <summary>
        /// Generate next pathes.
        /// </summary>
        /// <returns>
        /// An array containing <see cref="PathGenerator.Count"/> <see cref="Path"/>s, 
        /// wrapped in a (possibly weighted) <see cref="Sample"/>.
        /// </returns>
        public override Sample Next()
        {
            Matrix diffusion = new Matrix(Count, TimeSteps);
            double weight = 1.0;
            for( int i=0; i<TimeSteps; i++)
            {
                Sample sample = ArrayGenerator.NextSample();  // get a weighted SparseVector
                weight *= sample.Weight;
                SparseVector v = (SparseVector)sample.Value;
                v.Multiply( Math.Sqrt( TimeDelays.Data[i] ));
                v.Copy( diffusion.Column(i) );
            }

            Path[] pathes = new Path[Count];
            for( int j=0; j<Count; j++)
                pathes[j] = new Path( Times, Drift[j], diffusion.Row(j) );

            return new Sample( pathes, weight);
        }
    }
}