using System.Collections.Generic;
using Orion.Analytics.Maths;

namespace Orion.Analytics.Statistics
{

    /// <summary>
    /// Halton low-discrepancy sequence generator
    /// Halton algorithm for low-discrepancy sequence.  For more
    ///    details see chapter 8, paragraph 2 of "Monte Carlo Methods in
    ///    Finance", by Peter Jäckel
    ///    \test
    ///    - the correctness of the returned values is tested by
    ///      reproducing known good values.
    ///    - the correctness of the returned values is tested by checking
    ///      their discrepancy against known good values.
    /// </summary>
    public class HaltonSequence //static
    {
        //    private static List<int> CalculateBaseBExpansion(int k, ulong b)
        //    {
        //        if (k > 0)
        //        {
        //            var jMax = (int)Math.Floor(Math.Log(k) / Math.Log(b));
        //            var a = new IntVector(jMax + 1);
        //            var q = Math.Pow(b, jMax);
        //            for (var i = 1; i <= jMax; i++)
        //            {
        //                var ratio = Convert.ToDecimal(k / q);
        //                a[i - 1] = (int)Math.Floor(ratio);
        //                k = k - (int)q * a[i - 1];
        //                q = q / b;                   
        //            }
        //            return a.Vector.ToList();
        //        }
        //        return new List<int> {0};
        //    }

        //    private static void IncrementBaseBExpansion(List<int> a, ulong b)
        //    {
        //        var carry = true;
        //        var m = a.Count;
        //        for (var i = 0; i < m; i++)
        //        {
        //            if (carry)
        //            {
        //                if (a[i] == (int)b - 1)
        //                {
        //                    a[i] = 0;
        //                }
        //                else
        //                {
        //                    a[i] += 1;
        //                    carry = false;
        //                }
        //            }
        //        }
        //        if (carry)
        //        {
        //            a.Add(1);
        //        }
        //    }

        //    public static List<double> GetHaltonSequence(int start, int end, int dimension)
        //    {
        //        var b = PrimeNumber.Get(dimension);
        //        double q = 1.0 / b;
        //        var xs = new List<double>();
        //        var a = CalculateBaseBExpansion(start, b);
        //        var m = a.Count;
        //        for (var i = start; i <= end; i++)
        //        {
        //            var xn = 0.0;
        //            q = 1.0 / b;
        //            for (var j = 0; j < m; j++)
        //            {
        //                xn += q * a[j];
        //                q /= b;
        //            }
        //            xs.Add(xn);
        //            IncrementBaseBExpansion(a, b);
        //        }
        //        return xs;
        //    }
        //}

        private readonly int _dimensionality;
        private ulong _sequenceCounter;
        private readonly Sample<List<double>> _sequence;
        private readonly List<ulong> _randomStart;
        private readonly List<double> _randomShift;

        public HaltonSequence(int dimensionality,
            ulong seed = 0,
            bool randomStart = true,
            bool randomShift = false)
        {
            _dimensionality = dimensionality;
            _sequenceCounter = 0;
            _sequence = new Sample<List<double>>(new List<double>(dimensionality), 1.0);
            _randomStart = new List<ulong>(dimensionality);
            _randomShift = new List<double>(dimensionality);
            //dimensionality must be greater than 0"
            if (randomStart || randomShift)
            {
                RandomSequenceGenerator<MersenneTwister> uniformRsg =
                    new RandomSequenceGenerator<MersenneTwister>(_dimensionality, seed);
                if (randomStart)
                    _randomStart = uniformRsg.NextInt32Sequence();
                if (randomShift)
                    _randomShift = uniformRsg.NextSequence().Value;
            }
        }

        public Sample<List<double>> NextSequence()
        {
            ++_sequenceCounter;
            for (int i = 0; i < _dimensionality; ++i)
            {
                var h = 0.0;
                var b = PrimeNumber.Get(i);
                var f = 1.0;
                var k = _sequenceCounter + _randomStart[i];
                while (k != 0)
                {
                    f /= b;
                    h += (k % b) * f;
                    k /= b;
                }
                _sequence.Value[i] = h + _randomShift[i];
                _sequence.Value[i] -= (long) _sequence.Value[i];
            }
            return _sequence;
        }

        public Sample<List<double>> LastSequence()
        {
            return _sequence;
        }

        public int Dimension()
        {
            return _dimensionality;
        }
    }
}