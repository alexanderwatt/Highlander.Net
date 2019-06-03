
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
        /// Given one or more pathes, the value of an option is returned.
        /// </summary>
        /// <param name="pathes"></param>
        /// <returns></returns>
        public override double Value(Path[] pathes)
        {
            // would throw anyways: QL_RequireArgNotNull(pathes, "pathes", null);
            if( pathes.Length != _underlying.Length )
                throw new ArgumentException( "TODO: Not enough pathes.");
            if( pathes[0].Count == 0 )
                throw new ArgumentException( "TODO: the path cannot be empty.");

            int numAssets = pathes.Length;

            var logDrift     = new SparseVector(numAssets);
            var logDiffusion = new SparseVector(numAssets);
			
            for(int j=0; j < numAssets; j++)
            {
                logDrift.Data[j]     = pathes[j].Drift.Sum();
                logDiffusion.Data[j] = pathes[j].Diffusion.Sum();
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