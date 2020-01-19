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
using Highlander.Reporting.Analytics.V5r3.Options;

namespace Highlander.Reporting.Analytics.V5r3.Stochastics.MonteCarlo
{
    /// <summary>
    /// Path pricer for geometric average price option.
    /// </summary>
    public class GeometricApoPathPricer : PathPricer
    {
        ///<summary>
        ///</summary>
        ///<param name="optionType"></param>
        ///<param name="underlying"></param>
        ///<param name="strike"></param>
        ///<param name="discountFactor"></param>
        ///<param name="useAntitheticVariance"></param>
        ///<exception cref="ArgumentException"></exception>
        public GeometricApoPathPricer(Option.Type optionType, double underlying, 
                                      double strike, double discountFactor, bool useAntitheticVariance) 
            : base( discountFactor, useAntitheticVariance)
        {
            if( underlying <= 0.0 )
                throw new ArgumentException( "TODO: Price of underlying(s) must be positive.");
            if( strike <= 0.0 )
                throw new ArgumentException( "TODO: Strike price must be positive.");
            _optionType = optionType;
            _underlying = underlying;
            _strike = strike;
        }

        private readonly Option.Type _optionType;
        private readonly double _underlying;
        private readonly double _strike;

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

            double geoLogDrift = 0.0;
            double geoLogDiffusion = 0.0;
            for (int i=0, j=n; i<n; i++, j--) 
            {
                geoLogDrift     += j*path[0].Drift.Data[i];
                geoLogDiffusion += j*path[0].Diffusion.Data[i];
            }
            double averagePrice1 = _underlying *
                                   Math.Exp( (geoLogDrift+geoLogDiffusion) / n );

            if (UseAntitheticVariance) 
            {
                double averagePrice2 = _underlying *
                                       Math.Exp( (geoLogDrift-geoLogDiffusion) / n );
                return Discount * ( 
                                      Option.ExercisePayoff(_optionType, averagePrice1, _strike) +
                                      Option.ExercisePayoff(_optionType, averagePrice2, _strike) 
                                  ) / 2.0;
            }
            return Discount * 
                   Option.ExercisePayoff(_optionType, averagePrice1, _strike);
        }
    }
}