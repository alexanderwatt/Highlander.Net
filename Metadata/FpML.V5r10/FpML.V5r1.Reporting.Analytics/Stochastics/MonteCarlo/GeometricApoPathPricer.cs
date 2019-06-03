
using System;
using Orion.Numerics.MonteCarlo;

namespace Orion.Analytics.Stochastics.MonteCarlo
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
            this.optionType = optionType;
            this.underlying = underlying;
            this.strike = strike;
        }

        private Option.Type optionType;
        private double underlying, strike;

        /// <summary>
        /// Given one or more pathes, the value of an option is returned.
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
            double averagePrice1 = underlying *
                                   Math.Exp( (geoLogDrift+geoLogDiffusion) / n );

            if (useAntitheticVariance) 
            {
                double averagePrice2 = underlying *
                                       Math.Exp( (geoLogDrift-geoLogDiffusion) / n );
                return discount * ( 
                                      Option.ExercisePayoff(optionType, averagePrice1, strike) +
                                      Option.ExercisePayoff(optionType, averagePrice2, strike) 
                                  ) / 2.0;
            }
            return discount * 
                   Option.ExercisePayoff(optionType, averagePrice1, strike);
        }
    }
}