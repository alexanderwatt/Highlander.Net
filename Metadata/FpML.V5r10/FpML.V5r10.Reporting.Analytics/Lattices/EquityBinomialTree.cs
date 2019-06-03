using System;

namespace Orion.Analytics.Lattices
{
    public abstract class EquityBinomialTree : BinomialTree, ICloneable
    {
        protected EquityBinomialTree(int nColumns)
            : base(nColumns) 
        {}
      

        /// <summary>
        /// The forward rate applying at the i-th node to the (i+1)-th node of the tree.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public abstract double Rate(int i);

        /// <summary>
        /// The present value of dividends at the i-th node of the tree
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public abstract double Dividend(int i);


        /// <summary>
        /// Volatility on tree
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public abstract double Volatility { get; }

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <value>The time.</value>
        public abstract double Time { get; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return this;
        }


    }
}
