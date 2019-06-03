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
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Statistics
{
    /// <summary>
    /// Class that contains various methods to perform statistical calculations,
    /// for example calculate a particular percentile.
    /// </summary>
    public class Statistics
    {
        /// <summary>
        /// A percentile is the value of a variable below which a certain
        /// percentage of observations reside.
        /// If percentile*(data size - 1) is not an integer, then linear
        /// interpolation around the relevant index is applied to compute
        /// the percentile.
        /// </summary>
        /// <param name="data">Data from which to compute the
        /// percentile. There is no requirement for the array to be sorted
        /// into ascending/descending order.</param>
        /// <param name="percentile">The requested percentile as an integer
        /// in the range [0,100].
        /// Example: To request the 99'th percentile use the value 99.</param>
        /// <returns></returns>
        public double Percentile(ref double[] data, int percentile)
        {
            // Check for valid inputs.
            if(data.Length < 1)
            {
                const string errorMessage = "Data array used to calculate a percentile cannot be empty.";
                throw new ArgumentException(errorMessage);
            }
            if(percentile < 0 || percentile > 100)
            {
                const string errorMessage = "Percentile is restricted to the range [0,100].";
                throw new ArgumentException(errorMessage);
            }
            // Copy the array into a temporary array and then sort into
            // ascending order.
            long numElements = data.Length;
            var yArray = new double[numElements];
            Array.Copy(data, yArray, numElements);
            Array.Sort(yArray);
            // Compute the index at which the percentile is to be located.
            var percentileIndex = (numElements - 1)*percentile/100.0;
            // Compute the percentile by linear interpolation at the 
            // target percentile index.
            var xArray = new double[numElements]; // array index
            for (long i = 0; i < numElements; ++i )
            {
                xArray[i] = i;
            }
            var interpObj = LinearInterpolation.Interpolate(xArray, yArray);
            var percentileValue = interpObj.ValueAt(percentileIndex, true);
            return percentileValue;
        }
    }
}