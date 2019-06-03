using System;
using Orion.Analytics.LinearAlgebra.Sparse;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
	/// <summary>
	/// Path pricer for European-type Everest option.
	/// </summary>
	public class EverestPathPricer : PathPricer
	{
		public EverestPathPricer(double discountFactor, 
			bool useAntitheticVariance) 
			: base( discountFactor, useAntitheticVariance)
		{}

        /// <summary>
        /// Given one or more paths, the value of an option is returned.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public override double Value(Path[] paths)
		{
			if(paths.Length == 0 )
				throw new ArgumentException(nameof(paths));
			if(paths[0].Count == 0 )
				throw new ArgumentException( nameof(paths));
			int numAssets = paths.Length;
			var logDrift     = new SparseVector(numAssets);
			var logDiffusion = new SparseVector(numAssets);
			for(int j=0; j<numAssets; j++)
			{
				logDrift.Data[j]     = paths[j].Drift.Sum();
				logDiffusion.Data[j] = paths[j].Diffusion.Sum();
			}
			// Exp(Min(SparseVector)) should be Min(Exp(SparseVector))
			double minPrice1 = Math.Exp( (logDrift+logDiffusion).Min() );
			if (UseAntitheticVariance)
			{
				double minPrice2 = Math.Exp( (logDrift-logDiffusion).Min() );
				return Discount * ( minPrice1+minPrice2) / 2.0;
			}
		    return Discount * minPrice1;
		}
	}
}
