/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Highlander.Numerics.Lattices
{
    /// <summary>
    /// Binomial tree abstract base class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The binomial method is the simplest numerical method that can be used to
    /// price path-independent derivatives. It is usually the preferred lattice
    /// method under the Black-Scholes-Merton model. 
    /// See <see cref="BinomialTree"/> for a class based on 
    /// a <see cref="JarrowRudd"/>.
    /// </para>
    /// <para>
    /// There are several approaches to build the underlying binomial tree,
    /// like <see cref="CoxRossRubinstein">Jarrow-Rudd</see> or 
    /// <see cref="StandardBinomialTree">Cox-Ross-Rubinstein</see>.
    /// </para>
    /// </remarks>
    /// <seealso cref="JarrowRudd"/>
    /// <seealso cref="CoxRossRubinstein"/>
    /// <seealso cref="BlackScholesLattice"/>
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