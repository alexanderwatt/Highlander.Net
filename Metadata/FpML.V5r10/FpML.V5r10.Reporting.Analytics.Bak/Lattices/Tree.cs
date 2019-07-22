using Orion.Analytics.Processes;

namespace Orion.Analytics.Lattices
{
    /// <summary>
    /// Tree abstract base class approximating a single-factor diffusion.
    /// </summary>
    /// <remarks>
    /// A lattice, i.e. an instance of the abstract class <see cref="Lattice"/>,
    /// relies on one or several trees (each one approximating a
    /// diffusion process) to price an instance of the <see cref="DiscretizedAsset"/>
    /// class. Trees are instances of classes derived from <see cref="Tree"/> that
    /// define the branching between nodes and transition probabilities.
    /// </remarks>
    /// <seealso cref="BinomialTree"/>
    /// <seealso cref="TrinomialTree"/>
    public abstract class Tree
    {
        protected Tree(int columns) 
        {
            Columns = columns;
        }

        ///<summary>
        ///</summary>
        public int Columns { get; }

        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<param name="index"></param>
        ///<returns></returns>
        public abstract double Underlying(int i, int index);
        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<returns></returns>
        public abstract int Count(int i);
        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<param name="index"></param>
        ///<param name="branch"></param>
        ///<returns></returns>
        public abstract int Descendant(int i, int index, int branch);
        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<param name="index"></param>
        ///<param name="branch"></param>
        ///<returns></returns>
        public abstract double Probability(int i, int index, int branch);
    }
}