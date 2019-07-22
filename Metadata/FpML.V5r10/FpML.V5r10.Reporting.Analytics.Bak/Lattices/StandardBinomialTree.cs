using System;
using Orion.Analytics.Processes;

// COM interop attributes
// some useful attributes

namespace Orion.Analytics.Lattices
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