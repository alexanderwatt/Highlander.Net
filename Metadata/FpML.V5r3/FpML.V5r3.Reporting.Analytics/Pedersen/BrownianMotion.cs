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

#region Usings

using System;
using MathNet.Numerics.LinearAlgebra.Double;
using Orion.Analytics.Distributions;

#endregion

namespace Orion.Analytics.Pedersen
{
    class BrownianMotion
    {
        public static double BMStep(double stepSize)
        {
            NormalDistribution n = new NormalDistribution(0, Math.Sqrt(stepSize));
            double r = n.NextDouble();
            return r;
        }

        public static Vector BMStep(double stepSize, int dimensions)
        {
            NormalDistribution n = new NormalDistribution(0, Math.Sqrt(stepSize));
            var v = new DenseVector(dimensions);
            for (var i = 0; i < v.Count; i++)
            {
                v[i] = n.NextDouble();
            }
            return v;
        }
    }
}
