// COM interop attributes

// some useful attributes

namespace Orion.Analytics.Lattices
{
    /// <summary>
    /// Binomial tree abstract base class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The binomial method is the simplest numerical method that can be used to
    /// price path-independent derivatives. It is usually the preferred lattice
    /// method under the Black-Scholes-Merton model. 
    /// See <see cref="BlackScholesLattice"/> for a class based on 
    /// a <see cref="BinomialTree"/>.
    /// </para>
    /// <para>
    /// There are several approaches to build the underlying binomial tree,
    /// like <see cref="JarrowRudd">Jarrow-Rudd</see> or 
    /// <see cref="CoxRossRubinstein">Cox-Ross-Rubinstein</see>.
    /// </para>
    /// </remarks>
    /// <seealso cref="StandardBinomialTree"/>
    /// <seealso cref="JarrowRudd"/>
    /// <seealso cref="CoxRossRubinstein"/>
    public abstract class BinomialTree : Tree 
    {
        protected BinomialTree(int nColumns) : base(nColumns) 
        {}

        public override int Count(int i)
        {
            return i+1;
        }

        public override int Descendant(int i, int index, int branch)
        {
            return index + branch;
        }
    }
}