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
using Orion.Analytics.Options;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Path pricer for geometric average strike option.
    /// </summary>
    public class GeometricAsoPathPricer : PathPricer
    {
        ///<summary>
        ///</summary>
        ///<param name="optionType"></param>
        ///<param name="underlying"></param>
        ///<param name="discountFactor"></param>
        ///<param name="useAntitheticVariance"></param>
        ///<exception cref="ArgumentException"></exception>
        public GeometricAsoPathPricer(Option.Type optionType, double underlying, 
                                      double discountFactor, bool useAntitheticVariance) 
            : base( discountFactor, useAntitheticVariance)
        {
            if( underlying <= 0.0 )
                throw new ArgumentException( "TODO: Price of underlying(s) must be positive.");
            _optionType = optionType;
            _underlying = underlying;
        }

        private readonly Option.Type _optionType;
        private readonly double _underlying;

        /// <summary>
        /// Given one or more paths, the value of an option is returned.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override double Value(Path[] path)
        {
            if( path.Length != 1 )
                throw new ArgumentException( "TODO: exactly one path required ({0} given).");
            int n = path[0].Count;
            if( n == 0 )
                throw new ArgumentException( "TODO: the path cannot be empty.");

            double logDrift = 0.0;		// path[0].Drift.Sum();
            double logDiffusion = 0.0;	// path[0].Diffusion.Sum();
            double geoLogDrift = 0.0;
            double geoLogDiffusion = 0.0;
            for (int i=0, j=n; i<n; i++, j--) 
            {
                double drift     = path[0].Drift.Data[i];
                double diffusion = path[0].Diffusion.Data[i];
                logDrift        += drift;
                logDiffusion    += diffusion;
                geoLogDrift     += j*drift;
                geoLogDiffusion += j*diffusion;
            }
            double averageStrike1 = _underlying *
                                    Math.Exp( (geoLogDrift+geoLogDiffusion) / n );

            if (UseAntitheticVariance) 
            {
                double averageStrike2 = _underlying *
                                        Math.Exp( (geoLogDrift-geoLogDiffusion) / n );
                return Discount * ( 
                                      Option.ExercisePayoff(_optionType, _underlying *
                                                                        Math.Exp(logDrift+logDiffusion), averageStrike1) +
                                      Option.ExercisePayoff(_optionType, _underlying *
                                                                        Math.Exp(logDrift-logDiffusion), averageStrike2) 
                                  ) / 2.0;
            }
            return Discount * Option.ExercisePayoff(_optionType, _underlying *
                                                                Math.Exp(logDrift+logDiffusion), averageStrike1);
        }
    }
}