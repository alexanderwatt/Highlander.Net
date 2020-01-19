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

using Highlander.Reporting.Analytics.V5r3.Processes;

namespace Highlander.Reporting.Analytics.V5r3.Lattices
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