using System;
using Orion.Analytics.Processes;

// COM interop attributes
// some useful attributes

namespace Orion.Analytics.Lattices
{
    /// <summary>
    /// Simple binomial lattice approximating the Black-Scholes model.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The binomial method is the simplest numerical method that can be used to
    /// price path-independent derivatives. It is usually the preferred lattice
    /// method under the Black-Scholes-Merton model. 
    /// </para>
    /// <para>
    /// There are several approaches to build the underlying binomial tree,
    /// like <see cref="JarrowRudd">Jarrow-Rudd</see> or 
    /// <see cref="CoxRossRubinstein">Cox-Ross-Rubinstein</see>.
    /// </para>
    /// </remarks>
    public class BlackScholesLattice : Lattice 
    {
        ///<summary>
        ///</summary>
        ///<param name="tree"></param>
        ///<param name="riskFreeRate"></param>
        ///<param name="endTime"></param>
        ///<param name="steps"></param>
        public BlackScholesLattice(Tree tree, 
                                   double riskFreeRate, double endTime, int steps)
            : base(new TimeGrid(endTime, steps), 2)
        {
            _tree = tree;
            _discount = Math.Exp(-riskFreeRate*(endTime/steps));
            _pd = tree.Probability(0,0,0);
            _pu = tree.Probability(0,0,1);
        }

        private readonly Tree _tree;
        private readonly double _discount;
        private readonly double _pd;
        private readonly double _pu;

        ///<summary>
        ///</summary>
        public Tree Tree => _tree;

        #region Lattice implementation
        public override int Count(int i) 
        { 
            return _tree.Count(i); 
        }
        public override double Discount(int i, int j) 
        { 
            return _discount; 
        }

        protected override void Stepback(int i, double[] values, double[] newValues)
        {
            for (int j=0; j<Count(i); j++)
                newValues[j] = (_pd*values[j] + _pu*values[j+1]) * _discount;
        }

        protected override int Descendant(int i, int index, int branch)
        {
            return _tree.Descendant(i, index, branch);
        }
        protected override double Probability(int i, int index, int branch)
        {
            return _tree.Probability(i, index, branch);
        }
        #endregion
    }
}