
using System;
using Orion.Analytics.Options;

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
            _optionType = optionType;
            _underlying = underlying;
            _strike = strike;
        }

        private readonly Option.Type _optionType;
        private readonly double _underlying;
        private readonly double _strike;

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

            if (UseAntitheticVariance)
                return Discount * ( 
                                      Option.ExercisePayoff(_optionType, 
                                                            _underlying * Math.Exp(logDrift+logDiffusion), _strike) +
                                      Option.ExercisePayoff(_optionType, 
                                                            _underlying * Math.Exp(logDrift-logDiffusion), _strike) 
                                  ) / 2.0;
            return Discount * Option.ExercisePayoff(_optionType, 
                                                    _underlying * Math.Exp(logDrift+logDiffusion), _strike);
        }

    }
}