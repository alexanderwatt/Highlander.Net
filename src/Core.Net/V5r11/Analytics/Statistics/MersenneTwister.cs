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

using System.Collections.Generic;

namespace Orion.Analytics.Statistics
{
    //! Uniform random number generator
    /*! Mersenne Twister random number generator of period 2**19937-1
        For more details see http://www.math.keio.ac.jp/matumoto/emt.html
        \test the correctness of the returned values is tested by
              checking them against known good results.
    */
    public class MersenneTwister : IRngTraits
    {
        private static int N = 624; // state size
        private static int M = 397; // shift size

        /*! if the given seed is 0, a random seed will be chosen based on clock() */
        public MersenneTwister() : this(0) { }

        public MersenneTwister(ulong seed)
        {
            _mt = new List<ulong>(N);
            SeedInitialization(seed);
        }

        public MersenneTwister(List<ulong> seeds)
        {
            _mt = new List<ulong>(N);
            SeedInitialization(19650218UL);
            int i = 1, j = 0, k = (N > seeds.Count ? N : seeds.Count);
            for (; k != 0; k--)
            {
                _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30)) * 1664525UL)) + seeds[j] + (ulong)j; /* non linear */
                _mt[i] &= 0xffffffffUL; /* for WORDSIZE > 32 machines */
                i++; j++;
                if (i >= N) { _mt[0] = _mt[N - 1]; i = 1; }
                if (j >= seeds.Count) j = 0;
            }
            for (k = N - 1; k != 0; k--)
            {
                _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30)) * 1566083941UL)) - (ulong)i; /* non linear */
                _mt[i] &= 0xffffffffUL; /* for WORDSIZE > 32 machines */
                i++;
                if (i >= N) { _mt[0] = _mt[N - 1]; i = 1; }
            }
            _mt[0] = 0x80000000UL; /*MSB is 1; assuring non-zero initial array*/
        }

        public IRngTraits Factory(ulong seed) { return new MersenneTwister(seed); }

        /*! returns a sample with weight 1.0 containing a random number on (0.0, 1.0)-real-interval  */
        public Sample<double> Next()
        {
            return new Sample<double>(NextReal(), 1.0);
        }

        //! return a random number in the (0.0, 1.0)-interval
        public double NextReal()
        {
            return (NextInt32() + 0.5) / 4294967296.0;
        }

        //! return  a random number on [0,0xffffffff]-interval
        public ulong NextInt32()
        {
            if (_mti == N)
                Twist(); /* generate N words at a time */
            ulong y = _mt[_mti++];
            /* Tempering */
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680UL;
            y ^= (y << 15) & 0xefc60000UL;
            y ^= y >> 18;
            return y;
        }

        private void SeedInitialization(ulong seed)
        {
            /* initializes mt with a seed */
            ulong s = (seed != 0 ? seed : SeedGenerator.Instance().Get());
            _mt[0] = s & 0xffffffffUL;
            for (_mti = 1; _mti < N; _mti++)
            {
                _mt[_mti] = (1812433253UL * (_mt[_mti - 1] ^ (_mt[_mti - 1] >> 30)) + (ulong)_mti);
                /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                /* In the previous versions, MSBs of the seed affect   */
                /* only MSBs of the array mt[].                        */
                /* 2002/01/09 modified by Makoto Matsumoto             */
                _mt[_mti] &= 0xffffffffUL;
                /* for >32 bit machines */
            }
        }

        private void Twist()
        {
            ulong[] mag01 = { 0x0UL, MatrixA };
            /* mag01[x] = x * MATRIX_A  for x=0,1 */
            int kk;
            ulong y;
            for (kk = 0; kk < N - M; kk++)
            {
                y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1UL];
            }
            for (; kk < N - 1; kk++)
            {
                y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                _mt[kk] = _mt[(kk + M) - N] ^ (y >> 1) ^ mag01[y & 0x1UL];
            }
            y = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
            _mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1UL];
            _mti = 0;
        }

        private readonly List<ulong> _mt;

        private int _mti;

        // constant vector a
        const ulong MatrixA = 0x9908b0dfUL;

        // most significant w-r bits
        const ulong UpperMask = 0x80000000UL;

        // least significant r bits
        const ulong LowerMask = 0x7fffffffUL;
    }
}