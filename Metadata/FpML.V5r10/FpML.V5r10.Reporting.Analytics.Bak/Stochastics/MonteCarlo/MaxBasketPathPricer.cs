using System;
using Orion.Analytics.LinearAlgebra.Sparse;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
	/// <summary>
	/// Multipath pricer for European-type basket option.
	/// </summary>
	/// <remarks>
	/// The value of the option at expiration is given by the value
	/// of the underlying which has best performed.
	/// </remarks>
	public class MaxBasketPathPricer : PathPricer
	{
		public MaxBasketPathPricer( SparseVector underlying, 
			double discountFactor, bool useAntitheticVariance) 
			: base( discountFactor, useAntitheticVariance)
		{
			if( underlying.Min() <= 0.0 )
				throw new ArgumentException(nameof(underlying));

			_underlying = underlying;
		}

	    readonly SparseVector _underlying;

        /// <summary>
        /// Given one or more paths, the value of an option is returned.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public override double Value(Path[] paths)
		{
			// would throw anyways: QL_RequireArgNotNull(pathes, "pathes", null);
			if(paths.Length != _underlying.Length )
				throw new ArgumentException(nameof(paths));
			if(paths[0].Count == 0 )
				throw new ArgumentException(nameof(paths));

			int numAssets = paths.Length;
			var logDrift     = new SparseVector(numAssets);
			var logDiffusion = new SparseVector(numAssets);
			for(int j=0; j<numAssets; j++)
			{
				logDrift.Data[j]     = paths[j].Drift.Sum();
				logDiffusion.Data[j] = paths[j].Diffusion.Sum();
			}

			double maxPrice1 = ( _underlying * SparseVector.Exp( 
				logDrift + logDiffusion) ).Max();
			if (UseAntitheticVariance)
			{
				double maxPrice2 = ( _underlying * SparseVector.Exp( 
					logDrift - logDiffusion) ).Max();
				return Discount * ( maxPrice1+maxPrice2) / 2.0;
			}
		    return Discount * maxPrice1;
		}
	}
}
