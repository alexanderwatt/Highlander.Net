

using System;
using Orion.Numerics.MonteCarlo;

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
            this.optionType = optionType;
            this.underlying = underlying;
        }

        private Option.Type optionType;
        private double underlying;

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
            double averageStrike1 = underlying *
                                    Math.Exp( (geoLogDrift+geoLogDiffusion) / n );

            if (useAntitheticVariance) 
            {
                double averageStrike2 = underlying *
                                        Math.Exp( (geoLogDrift-geoLogDiffusion) / n );
                return discount * ( 
                                      Option.ExercisePayoff(optionType, underlying *
                                                                        Math.Exp(logDrift+logDiffusion), averageStrike1) +
                                      Option.ExercisePayoff(optionType, underlying *
                                                                        Math.Exp(logDrift-logDiffusion), averageStrike2) 
                                  ) / 2.0;
            }
            return discount * Option.ExercisePayoff(optionType, underlying *
                                                                Math.Exp(logDrift+logDiffusion), averageStrike1);
        }
    }
}