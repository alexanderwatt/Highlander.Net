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
using Orion.Analytics.Distributions;

namespace Orion.Analytics.Processes
{


    class BrownianMotion
    {
        private static readonly Random Rand = new Random();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepsize"></param>
        /// <returns></returns>
        public static double BMStep(double stepsize)
        {
            NormalDistribution n = new NormalDistribution(0, Math.Sqrt(stepsize));
            double r = n.GetRandomVariate(Rand);
            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepsize"></param>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public static GeneralVector BMStep(double stepsize, int dimensions)
        {
            NormalDistribution n = new NormalDistribution(0, Math.Sqrt(stepsize));
            GeneralVector v = new GeneralVector(dimensions);
            n.GetRandomVariates(Rand, v);
            return v;
        }
    }
}
