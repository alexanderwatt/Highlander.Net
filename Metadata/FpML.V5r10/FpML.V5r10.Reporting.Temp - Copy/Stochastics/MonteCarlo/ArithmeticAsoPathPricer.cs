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
using Orion.Analytics.LinearAlgebra.Sparse;
using Orion.Analytics.Options;
using Orion.Analytics.Solvers;

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
            double price1 = 1.0;
            double averageStrike1 = 0.0;
            SparseVector p1 = path[0].Drift + path[0].Diffusion;
            UnaryFunction uf = Math.Exp;
            p1.Apply(uf);
            if (UseAntitheticVariance) 
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
                averageStrike1 = _underlying * averageStrike1 / n;
                averageStrike2 = _underlying * averageStrike2 / n;
                return Discount * ( 
                                      Option.ExercisePayoff(_optionType, price1, averageStrike1) +
                                      Option.ExercisePayoff(_optionType, price2, averageStrike2) 
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
            averageStrike1 = _underlying * averageStrike1 / n;
            return Discount * 
                   Option.ExercisePayoff(_optionType, price1, averageStrike1);
        }
    }
}