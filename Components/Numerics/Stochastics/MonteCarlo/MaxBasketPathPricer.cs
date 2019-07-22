/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Highlander.Numerics.LinearAlgebra.Sparse;

namespace Highlander.Numerics.Stochastics.MonteCarlo
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
