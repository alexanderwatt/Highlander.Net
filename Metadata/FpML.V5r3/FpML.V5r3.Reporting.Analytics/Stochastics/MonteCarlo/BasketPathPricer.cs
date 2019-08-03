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

#region Using directives

using System;
using Orion.Analytics.LinearAlgebra.Sparse;
using Orion.Analytics.Options;

#endregion

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Multipath pricer for European-type basket option.
    /// </summary>
    /// <remarks>
    /// The value of the option at expiration is given by the value
    /// of the underlying which has best performed.
    /// </remarks>
    public class BasketPathPricer : PathPricer
    {
        ///<summary>
        ///</summary>
        ///<param name="optionType"></param>
        ///<param name="underlying"></param>
        ///<param name="strike"></param>
        ///<param name="discountFactor"></param>
        ///<param name="useAntitheticVariance"></param>
        ///<exception cref="ArgumentException"></exception>
        public BasketPathPricer(Option.Type optionType, SparseVector underlying, 
                                double strike, double discountFactor, bool useAntitheticVariance) 
            : base( discountFactor, useAntitheticVariance)
        {
            if( underlying.Min() <= 0.0 )
            {
                throw new ArgumentException("TODO: Price of underlying(s) must be positive.");
            }

            if( strike <= 0.0 )
            {
                throw new ArgumentException("TODO: Strike price must be positive.");
            }
			
            _optionType = optionType;
            _underlying = underlying;
            _strike     = strike;
        }	
        private readonly Option.Type    _optionType;
        readonly SparseVector           _underlying;
        private readonly double         _strike;

        /// <summary>
        /// Given one or more paths, the value of an option is returned.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public override double Value(Path[] paths)
        {
            // would throw anyways: QL_RequireArgNotNull(paths, "paths", null);
            if( paths.Length != _underlying.Length )
                throw new ArgumentException( "TODO: Not enough paths.");
            if( paths[0].Count == 0 )
                throw new ArgumentException( "TODO: the path cannot be empty.");
            int numAssets = paths.Length;
            var logDrift     = new SparseVector(numAssets);
            var logDiffusion = new SparseVector(numAssets);
            for(int j=0; j < numAssets; j++)
            {
                logDrift.Data[j]     = paths[j].Drift.Sum();
                logDiffusion.Data[j] = paths[j].Diffusion.Sum();
            }
            double basketPrice1 = SparseVector.DotProduct( _underlying, SparseVector.Exp( logDrift + logDiffusion) );
            if (UseAntitheticVariance)
            {
                double basketPrice2 = SparseVector.DotProduct( _underlying, SparseVector.Exp( logDrift - logDiffusion) );
                return Discount * ( 
                                      Option.ExercisePayoff( _optionType, basketPrice1, _strike) +
                                      Option.ExercisePayoff( _optionType, basketPrice2, _strike)
                                  ) / 2.0;
            }
            return Discount * Option.ExercisePayoff( 
                                  _optionType, basketPrice1, _strike);
        }
    }
}