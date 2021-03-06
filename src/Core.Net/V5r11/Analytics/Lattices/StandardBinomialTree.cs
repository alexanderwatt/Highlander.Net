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
using Highlander.Reporting.Analytics.V5r3.Processes;

namespace Highlander.Reporting.Analytics.V5r3.Lattices
{
    /// <summary>
    /// Additive binomial tree class with equal probabilities.
    /// </summary>
    public class StandardBinomialTree : BinomialTree 
    {
        ///<summary>
        ///</summary>
        ///<param name="process"></param>
        ///<param name="endTime"></param>
        ///<param name="steps"></param>
        public StandardBinomialTree( IDiffusionProcess process,
                                     double endTime, int steps) : base(steps + 1)
        {
            double dt = endTime/steps;
            _x0 = process.X0;
            _dx = Math.Sqrt(process.Variance(0.0, 0.0, dt));
        }

        private readonly double _x0;
        private readonly double _dx;

        public override double Probability(int i, int index, int branch)  
        {
            return 0.5;
        }
        public override double Underlying(int i, int index)  
        {
            int j = (2*index - i);
            return _x0 + j*_dx;
        }
    }
}