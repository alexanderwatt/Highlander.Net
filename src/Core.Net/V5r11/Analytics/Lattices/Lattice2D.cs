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

namespace Highlander.Reporting.Analytics.V5r3.Lattices
{
    /// <summary>
    /// Two-dimensional lattice.
    /// </summary>
    /// <remarks>
    /// This lattice is based on two tri-nomial trees and primarily
    /// used for the G2 short-rate model.
    /// </remarks>
    public abstract class Lattice2D : Lattice 
    {
        protected Lattice2D(TrinomialTree tree1, Tree tree2, 
                         double correlation) : base(tree1.TimeGrid, 9)
        {
            Tree1 = tree1;
            Tree2 = tree2;
            _rho   = Math.Abs(correlation);

            _m = correlation < 0.0 ? new[,]
                                        {
                                            { -1.0, -4.0, 5.0 },
                                            { -4.0, 8.0, -4.0 },
                                            { 5.0, -4.0, -1.0 }
                                        } : new[,]
                                                {
                                                    { 5.0, -4.0, -1.0 },
                                                    { -4.0, 8.0, -4.0 },
                                                    { -1.0, -4.0, 5.0 }
                                                };
        }

        protected Tree Tree1, Tree2;
        private readonly double[,] _m;
        private readonly double _rho;

        public override int Count(int i)  
        { 
            return Tree1.Count(i)*Tree2.Count(i); 
        }

        protected override int Descendant(int i, int index, int branch)
        {
            int modulo = Tree1.Count(i);
            int index1 = index % modulo;
            int index2 = index / modulo;
            int branch1 = branch % 3;
            int branch2 = branch / 3;

            modulo = Tree1.Count(i+1);
            return Tree1.Descendant(i, index1, branch1) +
                   Tree2.Descendant(i, index2, branch2) * modulo;
        }

        protected override double Probability(int i, int index, int branch)
        {
            int modulo = Tree1.Count(i);
            int index1 = index % modulo;
            int index2 = index / modulo;
            int branch1 = branch % 3;
            int branch2 = branch / 3;
            double prob1 = Tree1.Probability(i, index1, branch1);
            double prob2 = Tree2.Probability(i, index2, branch2);
            return prob1*prob2 + _rho * _m[branch1,branch2] / 36.0;
        }
    }
}