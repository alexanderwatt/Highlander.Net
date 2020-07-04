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

#region Using directives

using System;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Distributions
{
    /// <summary>
    /// Abstract base for a cached continuous random number generator.
    /// </summary>
    /// <remarks>
    /// Derived classes must override and supply the concrete RNG 
    /// implementation in <see cref="Generate"/>.
    /// </remarks>
    public abstract class CachedContinuousRng : IContinuousRng

    {
        /// <summary>
        /// Initialize a cached continuous RNG with the default _cache size (256 numbers).
        /// </summary>
        /// <overloads>
        /// Initialize a cached continuous random number generator.
        /// </overloads>
        protected CachedContinuousRng() : this(256)
        {}

        /// <summary>
        /// Initialize a cached continuous RNG with the given _cache size..
        /// </summary>
        /// <param name="cacheSize">Size of the _cache for random numbers.</param>
        /// <remarks>
        /// Adjust the <paramref name="cacheSize"/> to optimize the performance of the
        /// underlying library.
        /// </remarks>
        protected CachedContinuousRng(int cacheSize)
        {
            _cache = new Double[cacheSize];
        }

        /// <summary>
        /// Array acting as a _cache for random numbers.
        /// </summary>
        readonly double[] _cache;

        /// <summary>
        /// Index to first "fresh" random number in <see cref="_cache"/>.
        /// </summary>
        int _current = int.MaxValue;


        /// <summary>
        /// Draw random samples from the generator.
        /// </summary>
        /// <remarks>
        /// Must be overridden by derived classes with a concrete RNG implementation.
        /// </remarks>
        /// <param name="r">
        ///		An <see cref="Array"/> to be filled with double-precision floating point 
        ///		numbers.</param>
        /// <param name="start">
        ///		The (zero-based) index of the first array element to fill.
        /// </param>
        /// <param name="length">
        ///		The number of random samples to generate.
        /// </param>
        protected abstract void Generate(double[] r, int start, int length);

        #region Implementation of IContinousRng

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, 
        /// and less than 1.0.
        /// </returns>
        /// <remarks>
        /// The value is returned from a _cache that is filled by the underlying library.
        /// </remarks>
        public double NextDouble()
        {
            if( _current >= _cache.Length )
            {
                Generate(_cache, 0, _cache.Length);
                _current = 0;
            }
            return _cache[_current++];
        }

        /// <summary>
        /// Returns an <see cref="Array"/> of random number between 0.0 and 1.0.
        /// </summary>
        /// <param name="r">
        /// A double-precision floating point <see cref="Array"/> to be filled
        /// with random numbers number greater than or equal to 0.0, and less than 1.0.
        /// </param>
        /// <remarks>
        /// The value can be returned from a _cache that is filled by the underlying library.
        /// <para>
        /// When the requested number of samples is larger than the size of the _cache, the
        /// underlying library is used to fill the passed array directly. If you want to make
        /// sure that random numbers are returned in the same order as generated by the
        /// underlying RNG make the _cache size at least the same size as the
        /// largest number of samples requested in one call.
        /// </para>
        /// </remarks>
        public void Next(double[] r)
        {
            Next(r, r.GetLowerBound(0), r.Length);
        }

        /// <summary>
        /// Returns an <see cref="Array"/> of random number between 0.0 and 1.0.
        /// </summary>
        /// <param name="r">
        /// A double-precision floating point <see cref="Array"/> to be filled
        /// with random numbers number greater than or equal to 0.0, and less than 1.0.
        /// </param>
        /// <param name="start">Index of the first element to fill.</param>
        /// <remarks>
        /// The value can be returned from a _cache that is filled by the underlying library.
        /// <para>
        /// When the requested number of samples is larger than the size of the _cache, the
        /// underlying library is used to fill the passed array directly. If you want to make
        /// sure that random numbers are returned in the same order as generated by the
        /// underlying RNG make the _cache size at least the same size as the
        /// largest number of samples requested in one call.
        /// </para>
        /// </remarks>
        public void Next(double[] r, int start)
        {
            Next( r, start, r.GetUpperBound(0)-start+1);
        }

        /// <summary>
        /// Returns an <see cref="Array"/> of random number between 0.0 and 1.0.
        /// </summary>
        /// <param name="r">
        /// A double-precision floating point <see cref="Array"/> to be filled
        /// with random numbers number greater than or equal to 0.0, and less than 1.0.
        /// </param>
        /// <param name="start">Index of the first element to fill.</param>
        /// <param name="length">Number of random numbers to generate.</param>
        /// <remarks>
        /// The value can be returned from a _cache that is filled by the underlying library.
        /// <para>
        /// When the requested number of samples is larger than the size of the _cache, the
        /// underlying library is used to fill the passed array directly. If you want to make
        /// sure that random numbers are returned in the same order as generated by the
        /// underlying RNG make the _cache size at least the same size as the
        /// largest number of samples requested in one call.
        /// </para>
        /// </remarks>
        public virtual void Next(double[] r, int start, int length)
        {
            if( _current >= _cache.Length ) 
            {
                Generate( r, start, length);	// no _cache
                return;						// quick exit
            }

            // use elements from _cache if available
            int cached = Math.Min(length, _cache.Length - _current);
            if( cached > 0 )	
            {
                Array.Copy(_cache, _current, r, start, cached);
                _current += cached;
            }
            // generate remaining elements
            if( cached < length )
                Generate( r, start+cached, length-cached);
		
        }

        #endregion

    }
}