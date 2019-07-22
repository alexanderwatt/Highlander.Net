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

namespace Orion.Analytics.Distributions
{
    /// <summary>
    /// Encapsulates an engine to generate Gaussian deviates from a standard 
    /// normal distribution.
    /// The Box-Muller method is used to map a pair of uniformly distributed
    /// random numbers on the interval (0,1) into a pair of numbers that have a
    /// standard Gaussian distribution. 
    /// </summary>
    public class GaussianRandomNumbers
    {
        // ---------------------------------------------------------------------
        // Constructor.
        // ---------------------------------------------------------------------
        /// <summary>
        /// Instantiates a new instance of the class
        /// <see cref="GaussianRandomNumbers"/>.
        /// </summary>
        /// <param name="seed">Value used to seed the random number generator.
        /// A client of the class can have the seed internally generated by
        /// instantiating the class instance with a negative seed.</param>
        public GaussianRandomNumbers(int seed)
        {
            _seed = seed;
        }

        // ---------------------------------------------------------------------
        // Business logic methods.
        // ---------------------------------------------------------------------
        /// <summary>
        /// Generates the requested Gaussian deviates and stores these in the
        /// array passed as an argument to the function..
        /// </summary>
        /// <param name="numDeviates">The num deviates.</param>
        /// <param name="storage">The storage.</param>
        public void GetGaussianDeviates(long numDeviates, ref double[] storage)
        {
            // Check that the number of deviates requested is a positive number.
            if(numDeviates <= 0)
            {
                const string errorMessage = "Invalid number of Gaussian deviates requested.";
                throw new ArgumentException(errorMessage);
            }

            // Allocate space for the array that will store the Gaussian
            // deviates.
            storage = new double[numDeviates];

            // Seed the random number generator.
            if(_seed < 0)
            {
                // Set the seed internally.
                _seed = unchecked((int) DateTime.Now.Ticks);
            }
            
            // Generate and store the random numbers.
            var randObj = new Random(_seed);
            long counter = 0; // tracks the number of generated deviates

            while(counter < numDeviates)
            {
                var x1 = randObj.NextDouble();
                var x2 = randObj.NextDouble();
                var modulus = Math.Sqrt(-2*Math.Log(x1));

                // Apply Box-Muller method to generate the first of the 
                // pair of Gaussian deviates.
                storage[counter] = modulus*Math.Cos((2*Math.PI*x2));
                ++counter;

                if (counter >= numDeviates) continue;
                // Apply the Box-Muller method to generate the second of
                // the pair of Gaussian deviates.
                storage[counter] = modulus * Math.Sin((2 * Math.PI * x2));
                ++counter;
            }
        }


        // ---------------------------------------------------------------------
        // Private data members.
        // ---------------------------------------------------------------------
        /// <summary>
        /// Seed for the random number generator.
        /// </summary>
        private int _seed;
    }
}