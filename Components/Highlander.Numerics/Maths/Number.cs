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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Highlander.Numerics.Maths
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct FloatingPoint64
    {
        [FieldOffset(0)]
        public Double Float64;

        [FieldOffset(0)]
        public Int64 Signed64;

        [FieldOffset(0)]
        public UInt64 Unsigned64;
    }

    //! Prime numbers calculator
    /*! Taken from "Monte Carlo Methods in Finance", by Peter Jäckel
     */
    public class PrimeNumber
    {
        //! Get and store one after another.
        private static readonly ulong[] FirstPrimes = {
            // the first two primes are mandatory for bootstrapping
            2,  3,
            // optional additional precomputed primes
            5,  7, 11, 13, 17, 19, 23, 29,
            31, 37, 41, 43, 47 };

        private static readonly List<ulong> PrimeNumbers = new List<ulong>();

        private PrimeNumber()
        { }

        public static ulong Get(int absoluteIndex)
        {
            if (PrimeNumbers.Count==0)
                PrimeNumbers.AddRange(FirstPrimes);
            while (PrimeNumbers.Count <= absoluteIndex)
                NextPrimeNumber();
            return PrimeNumbers[absoluteIndex];
        }

        private static void NextPrimeNumber()
        {
            ulong p, n, m = PrimeNumbers.Last();
            do
            {
                // skip the even numbers
                m += 2;
                n = (ulong)Math.Sqrt(m);
                // i=1 since the even numbers have already been skipped
                int i = 1;
                do
                {
                    p = PrimeNumbers[i];
                    ++i;
                }
                while ((m % p != 0) && p <= n);
            } while (p <= n);
            PrimeNumbers.Add(m);
        }
    }

    /// <summary>
    /// Helper functions for dealing with floating point numbers.
    /// </summary>
    public static class Number
    {
        ///<summary>
        ///</summary>
        public const double SmallestNumberGreaterThanZero = double.Epsilon;
        ///<summary>
        ///</summary>
        public static readonly double RelativeAccuracy = EpsilonOf(1.0);

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <returns>Relative Epsilon (positive double or NaN).</returns>
        /// <remarks>Evaluates the <b>negative</b> epsilon. The more common positive epsilon is equal to two times this negative epsilon.</remarks>
        public static double EpsilonOf(double value)
        {
            if(double.IsInfinity(value) || double.IsNaN(value))
                return double.NaN;

            long signed64 = BitConverter.DoubleToInt64Bits(value);
            if(signed64 == 0)
            {
                signed64++;
                return BitConverter.Int64BitsToDouble(signed64) - value;
            }
            if(signed64-- < 0)
                return BitConverter.Int64BitsToDouble(signed64) - value;
            return value - BitConverter.Int64BitsToDouble(signed64);
        }

        /// <summary>
        /// Increments a floating point number to the next bigger number representable by the data type.
        /// </summary>
        /// <remarks>
        /// The incrementation step length depends on the provided value.
        /// Increment(double.MaxValue) will return positive infinity.
        /// </remarks>
        public static double Increment(double value)
        {
            if(double.IsInfinity(value) || double.IsNaN(value))
                return value;
            long signed64 = BitConverter.DoubleToInt64Bits(value);
            if(signed64 < 0)
                signed64--;
            else
                signed64++;
            if(signed64 == -9223372036854775808) // = "-0", make it "+0"
                return 0;
            value = BitConverter.Int64BitsToDouble(signed64);
            return double.IsNaN(value) ? double.NaN : value;
        }

        /// <summary>
        /// Decrements a floating point number to the next smaller number representable by the data type.
        /// </summary>
        /// <remarks>
        /// The decrementation step length depends on the provided value.
        /// Decrement(double.MinValue) will return negative infinity.
        /// </remarks>
        public static double Decrement(double value)
        {
            if(double.IsInfinity(value) || double.IsNaN(value))
                return value;
            long signed64 = BitConverter.DoubleToInt64Bits(value);
            if(signed64 == 0)
                return -double.Epsilon;
            if(signed64 < 0)
                signed64++;
            else
                signed64--;
            value = BitConverter.Int64BitsToDouble(signed64);
            return double.IsNaN(value) ? double.NaN : value;
        }

        /// <summary>
        /// Evaluates the count of numbers between two double numbers
        /// </summary>
        /// <remarks>The second number is included in the number, thus two equal numbers evaluate to zero and two neighbour numbers evaluate to one. Therefore, what is returned is actually the count of numbers between plus 1.</remarks>
        public static ulong NumbersBetween(double a, double b)
        {
            if(double.IsNaN(a) || double.IsInfinity(a))
                throw new ArgumentException("ArgumentNotInfinityNaN");
            if(double.IsNaN(b) || double.IsInfinity(b))
                throw new ArgumentException("ArgumentNotInfinityNaN");
            ulong ua = ToLexicographicalOrderedUInt64(a);
            ulong ub = ToLexicographicalOrderedUInt64(b);
            return (a >= b) ? ua - ub : ub - ua;
        }

        ///<summary>
        ///</summary>
        ///<param name="value"></param>
        ///<returns></returns>
        public static ulong ToLexicographicalOrderedUInt64(double value)
        {
            long signed64 = BitConverter.DoubleToInt64Bits(value);
            var unsigned64 = (ulong)signed64;
            return (signed64 >= 0) ? unsigned64 : 0x8000000000000000 - unsigned64;
        }

        ///<summary>
        ///</summary>
        ///<param name="value"></param>
        ///<returns></returns>
        public static long ToLexicographicalOrderedInt64(double value)
        {
            long signed64 = BitConverter.DoubleToInt64Bits(value);
            return (signed64 >= 0) ? signed64 : (long)(0x8000000000000000 - (ulong)signed64);
        }

        ///<summary>
        ///</summary>
        ///<param name="value"></param>
        ///<returns></returns>
        public static ulong SignedMagnitudeToTwosComplementUInt64(long value)
        {
            return (value >= 0) ? (ulong)value : 0x8000000000000000 - (ulong)value;
        }

        ///<summary>
        ///</summary>
        ///<param name="value"></param>
        ///<returns></returns>
        public static long SignedMagnitudeToTwosComplementInt64(long value)
        {
            return (value >= 0) ? value : (long)(0x8000000000000000 - (ulong)value);
        }

        /// <param name="b"></param>
        /// <param name="maxNumbersBetween">The maximum count of numbers between the two numbers plus one ([a,a] -> 0, [a,a+e] -> 1, [a,a+2e] -> 2, ...).</param>
        /// <param name="a"></param>
        public static bool AlmostEqual(double a, double b, int maxNumbersBetween)
        {
            return AlmostEqual(a, b, (ulong)maxNumbersBetween);
        }
        
        /// <param name="b"></param>
        /// <param name="maxNumbersBetween">The maximum count of numbers between the two numbers plus one ([a,a] -> 0, [a,a+e] -> 1, [a,a+2e] -> 2, ...).</param>
        /// <param name="a"></param>
        public static bool AlmostEqual(double a, double b, ulong maxNumbersBetween)
        {
            // NaN's should never equal to anything
            if(double.IsNaN(a) || double.IsNaN(b)) //(a != a || b != b)
                return false;
            if(a == b)
                return true;
            // false, if only one of them is infinity or they differ on the infinity sign
            if(double.IsInfinity(a) || double.IsInfinity(b))
                return false;
            ulong between = NumbersBetween(a, b);
            return between <= maxNumbersBetween;
        }
    }
}