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

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// Base class for path pricers.
    /// </summary>
    /// <remarks>
    /// This is the base class from which path pricers must inherit. 
    /// <para>
    /// The only method which subclasses are required to implement is
    /// <c>double Value(<see cref="Path"/>[] path);</c> where <c>path[]</c> 
    /// is an array of one or more <see cref="Path"/>s depending on the derivative
    /// whose value must be calculated.
    /// </para>
    /// <para>
    /// Similarly, the term <em>path</em> will be used in the following
    /// discussion as meaning either path or multi-path depending on the
    /// context.
    /// The term <em>single path</em> is not to be taken as opposite to
    /// multi-path, but rather as meaning "a single instance of a (multi)path"
    /// as opposed to the set of all generated (multi)paths.
    /// </para>
    /// <para>
    /// The above method encapsulates the pricing of the derivative on a single
    /// path and must return its value had the evolution of the underlying(s)
    /// followed the path passed as argument.
    /// </para>
    /// <para>
    /// For this reason, control variate techniques (see below) must not be
    /// implemented at this level since they would cause the returned value
    /// to differ from the actual price of the derivative on the path.
    /// </para>
    /// <para>
    /// Instead, antithetic variance-reduction techniques can be effectively
    /// implemented at this level and indeed are used in the pricers currently
    /// included in the library.
    /// </para>
    /// <para>
    /// In short, such techniques consist in pricing an option on both the
    /// given path and its antithetic, the latter being a path with the same
    /// drift and the opposite diffusion component. The value of the sample
    /// is defined as the average of the prices on the two paths.
    /// </para>
    /// <para>
    /// A generic implementation of antithetic techniques could consist of a
    /// path pricer class which takes a concrete path pricer upon construction
    /// and whose <c>Value</c> method (as described above) simply proxies two
    /// calls to the contained pricer, passing the given path and its antithetic, 
    /// and averages the result.
    /// However, this would not take full advantage of the technique.
    /// </para>
    /// <para>
    /// In fact, it must be noted that using antithetic paths not only reduces
    /// the variance <em>per se</em> but also allows to factor out
    /// calculations commons to a path and its antithetic, thus reducing greatly
    /// the computation time. Therefore, such techniques are best implemented
    /// inside the path pricer itself, whose algorithm can fully exploit such
    /// factorization.
    /// </para>
    /// <para>
    /// A number of path pricers are available in the library and are listed
    /// in the namespace reference.
    /// </para>
    /// </remarks>
    public abstract class PathPricer
    {
        /// <summary>
        /// Protected constructor for abstract base class.
        /// </summary>
        protected PathPricer(double discountFactor, bool useAntitheticVariance)
        {
            if( discountFactor <= 0.0 || discountFactor > 1.0 )
                throw new ArgumentOutOfRangeException( nameof(discountFactor), discountFactor, 
                                                       "TODO: Discount factor must be in (0%;100%]." );
            Discount = discountFactor;
            UseAntitheticVariance = useAntitheticVariance;
        }

        protected double Discount;
        protected bool UseAntitheticVariance;

        /// <summary>
        /// Given one or more paths, the value of an option is returned.
        /// </summary>
        /// <remarks>
        /// This method must be overriden by derived classes. 
        /// </remarks>
        /// <param name="path">
        /// An array of one or more <see cref="Path"/>s depending on the derivative
        /// whose value must be calculated.
        /// </param>
        /// <returns>
        /// The price of the derivative had the evolution of the underlying(s)
        /// followed the path passed as argument.
        /// </returns>
        public abstract double Value(Path[] path);

    }

    /// <summary>
    /// As above but extends the path prices Value function to return an array
    /// This is useful for when we want to return multiple calculated values
    /// from one simulation
    /// </summary>
    public abstract class MultiVarPathPricer
    {
        /// <summary>
        /// Protected constructor for abstract base class.
        /// </summary>
        protected MultiVarPathPricer(double discountFactor, bool useAntitheticVariance)
        {
            if (discountFactor <= 0.0 || discountFactor > 1.0)
                throw new ArgumentOutOfRangeException(nameof(discountFactor), discountFactor,
                                                       "TODO: Discount factor must be in (0%;100%].");
            Discount = discountFactor;
            UseAntitheticVariance = useAntitheticVariance;
        }

        protected double Discount;
        protected bool UseAntitheticVariance;

        /// <summary>
        /// Given one or more paths, the value of an option is returned.
        /// </summary>
        /// <remarks>
        /// This method must be overriden by derived classes. 
        /// </remarks>
        /// <param name="path">
        /// An array of one or more <see cref="Path"/>s depending on the derivative
        /// whose value must be calculated.
        /// </param>
        /// <returns>
        /// The price of the derivative had the evolution of the underlying(s)
        /// followed the path passed as argument.
        /// </returns>
        public abstract double[] Value(Path[] path);
    }
}