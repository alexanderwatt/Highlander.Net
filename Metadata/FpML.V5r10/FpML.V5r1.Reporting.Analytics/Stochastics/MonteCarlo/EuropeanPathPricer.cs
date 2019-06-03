
using System;
using Orion.Numerics.MonteCarlo;
using Orion.Numerics.Resources;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Path pricer for European options.
    /// </summary>
    public class EuropeanPathPricer : PathPricer
    {
        ///<summary>
        ///</summary>
        ///<param name="optionType"></param>
        ///<param name="underlying"></param>
        ///<param name="strike"></param>
        ///<param name="discountFactor"></param>
        ///<param name="useAntitheticVariance"></param>
        ///<exception cref="ArgumentException"></exception>
        public EuropeanPathPricer(Option.Type optionType, double underlying, 
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

        private readonly Option.Type optionType;
        private readonly double underlying;
        private readonly double strike;

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

            double logDrift = path[0].Drift.Sum();
            double logDiffusion = path[0].Diffusion.Sum();

            if (useAntitheticVariance)
                return discount * ( 
                                      Option.ExercisePayoff(optionType, 
                                                            underlying * Math.Exp(logDrift+logDiffusion), strike) +
                                      Option.ExercisePayoff(optionType, 
                                                            underlying * Math.Exp(logDrift-logDiffusion), strike) 
                                  ) / 2.0;
            return discount * Option.ExercisePayoff(optionType, 
                                                    underlying * Math.Exp(logDrift+logDiffusion), strike);
        }

    }
}