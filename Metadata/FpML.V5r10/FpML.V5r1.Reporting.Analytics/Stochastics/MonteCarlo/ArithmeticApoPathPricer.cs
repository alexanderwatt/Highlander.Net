
using System;
using Orion.Numerics.Functions;
using Orion.Numerics.LinearAlgebra.Sparse;
using Orion.Numerics.MonteCarlo;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Path pricer for arithmetric average price option.
    /// </summary>
    public class ArithmetricApoPathPricer : PathPricer
    {
        ///<summary>
        ///</summary>
        ///<param name="optionType"></param>
        ///<param name="underlying"></param>
        ///<param name="strike"></param>
        ///<param name="discountFactor"></param>
        ///<param name="useAntitheticVariance"></param>
        ///<exception cref="ArgumentException"></exception>
        public ArithmetricApoPathPricer(Option.Type optionType, double underlying, 
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

            double price1 = 1.0;
            double averagePrice1 = 0.0;

            var p1 = path[0].Drift + path[0].Diffusion;
            UnaryFunction uf = Math.Exp;
            p1.Apply(uf);

            if (useAntitheticVariance) 
            {
                // see below for a SparseVectorized version
                double price2 = 1.0;
                double averagePrice2 = 0.0;
                SparseVector p2 = path[0].Drift - path[0].Diffusion;
                p2.Apply(uf);
                for(int i=0; i<n; i++)
                {
                    price1 *= p1.Data[i];
                    averagePrice1 += price1;
                    price2 *= p2.Data[i];
                    averagePrice2 += price2;
                }
                averagePrice1 = underlying * averagePrice1 / n;
                averagePrice2 = underlying * averagePrice2 / n;
                return discount * ( 
                                      Option.ExercisePayoff(optionType, averagePrice1, strike) +
                                      Option.ExercisePayoff(optionType, averagePrice2, strike) 
                                  ) / 2.0;
            }
            // p1.AdjacentDifference(new BinaryFunction(mult));
            // averagePrice1 = underlying * p1.Sum() / n
            for(int i=0; i<n; i++)
            {
                price1 *= p1.Data[i];
                averagePrice1 += price1;
            }
            averagePrice1 = underlying * averagePrice1 / n;

            return discount * 
                   Option.ExercisePayoff(optionType, averagePrice1, strike);
        }
    }
}