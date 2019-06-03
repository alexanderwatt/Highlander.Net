using System;
using Extreme.Statistics.Distributions;
using Extreme.Mathematics.LinearAlgebra;

namespace PedersenHost.Utilities
{


    class BrownianMotion
    {
        private static readonly Random Rand = new Random();
        public static double BMStep(double stepsize)
        {
            var n = new NormalDistribution(0, Math.Sqrt(stepsize));
            double r = n.GetRandomVariate(Rand);
            return r;
        }

        public static GeneralVector BMStep(double stepsize, int dimensions)
        {
            var n = new NormalDistribution(0, Math.Sqrt(stepsize));
            var v = new GeneralVector(dimensions);
            n.GetRandomVariates(Rand, v);
            return v;
        }
    }
}
