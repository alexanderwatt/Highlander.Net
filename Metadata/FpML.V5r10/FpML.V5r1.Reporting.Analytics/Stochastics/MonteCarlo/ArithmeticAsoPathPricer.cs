
using System;
using Orion.Numerics.Functions;
using Orion.Numerics.LinearAlgebra.Sparse;
using Orion.Numerics.MonteCarlo;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Path pricer for geometric average strike option.
    /// </summary>
    public class ArithmetricAsoPathPricer : PathPricer
    {
        ///<summary>
        ///</summary>
        ///<param name="optionType"></param>
        ///<param name="underlying"></param>
        ///<param name="discountFactor"></param>
        ///<param name="useAntitheticVariance"></param>
        ///<exception cref="ArgumentException"></exception>
        public ArithmetricAsoPathPricer(Option.Type optionType, double underlying, 
                                        double discountFactor, bool useAntitheticVariance) 
            : base( discountFactor, useAntitheticVariance)
        {
            if( underlying <= 0.0 )
                throw new ArgumentException( "TODO: Price of underlying(s) must be positive.");
            this.optionType = optionType;
            this.underlying = underlying;
        }

        private readonly Option.Type optionType;
        private readonly double underlying;


      
        
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
            double averageStrike1 = 0.0;

            SparseVector p1 = path[0].Drift + path[0].Diffusion;
            UnaryFunction uf = new UnaryFunction(Math.Exp);
            p1.Apply(uf);

            if (useAntitheticVariance) 
            {
                // see below for a SparseVectorized version
                double price2 = 1.0;
                double averageStrike2 = 0.0;
                SparseVector p2 = path[0].Drift - path[0].Diffusion;
                p2.Apply(uf);
                for(int i=0; i<n; i++)
                {
                    price1 *= p1.Data[i];
                    averageStrike1 += price1;
                    price2 *= p2.Data[i];
                    averageStrike2 += price2;
                }
                averageStrike1 = underlying * averageStrike1 / n;
                averageStrike2 = underlying * averageStrike2 / n;
                return discount * ( 
                                      Option.ExercisePayoff(optionType, price1, averageStrike1) +
                                      Option.ExercisePayoff(optionType, price2, averageStrike2) 
                                  ) / 2.0;
            }
            // p1.AdjacentDifference(new BinaryFunction(mult)); // defaults to -
            // averageStrike1 = underlying * p1.Sum() / n
            // price1 == p1[p1.Count-1]
            for(int i=0; i<n; i++)
            {
                price1 *= p1.Data[i];
                averageStrike1 += price1;
            }
            averageStrike1 = underlying * averageStrike1 / n;

            return discount * 
                   Option.ExercisePayoff(optionType, price1, averageStrike1);
        }
    }
}