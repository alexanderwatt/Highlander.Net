using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Orion.Analytics.Statistics
{
    /// <summary>Base class for the <i>histogram</i> algorithms.</summary>
    [Serializable]
    public class Histogram
    {
        /// <summary>
        /// Contains all the <c>Bucket</c>s of the <c>Histogram</c>.
        /// </summary>
        readonly ArrayList _buckets;

        /// <summary>
        /// Indicates whether the elements of <c>buckets</c> are
        /// currently sorted.
        /// </summary>
        bool _areBucketsSorted;

        /// <summary>Constructs an empty <c>Histogram</c>.</summary>
        public Histogram()
        {
            _buckets = new ArrayList();
            _areBucketsSorted = true;
        }

        /// <summary>Adds a <c>Bucket</c> to the <c>Histogram</c>.</summary>
        public void Add(Bucket bucket)
        {
            _buckets.Add(bucket);
            _areBucketsSorted = false;
        }

        /// <summary>
        /// Returns the <c>Bucket</c> that contains the value <c>v</c>. 
        /// </summary>
        public Bucket GetContainerOf(double v)
        {
            LazySort();
            return (Bucket)_buckets[_buckets.BinarySearch(v, Bucket.DefaultPointComparer)];
        }

        /// <summary>
        /// Returns the index in the <c>Histogram</c> of the <c>Bucket</c>
        /// that contains the value <c>v</c>.
        /// </summary>
        public int GetContainerIndexOf(double v)
        {
            LazySort();
            int index = _buckets.BinarySearch(v, Bucket.DefaultPointComparer);

            if (index < 0)
                throw new ArgumentException("The histogram does not contains the value " + v);

            return index;
        }

        /// <summary>
        /// Joins the boundaries of the successive buckets.
        /// </summary>
        public void JoinBuckets()
        {
            if (_buckets.Count == 0)
                throw new ArgumentException("Empty histogram.");

            LazySort();
            for (int i = 0; i < _buckets.Count - 2; i++)
            {
                double middle = (((Bucket)_buckets[i]).UpperBound
                                 + ((Bucket)_buckets[i + 1]).LowerBound) / 2;

                ((Bucket)_buckets[i]).UpperBound = middle;
                ((Bucket)_buckets[i + 1]).LowerBound = middle;
            }
        }

        private void LazySort()
        {
            if (!_areBucketsSorted)
            {
                _buckets.Sort();
                _areBucketsSorted = true;
            }
        }

        public void Sort()
        {
            _buckets.Sort();
        }

        /// <summary>
        /// Gets the <c>Bucket</c> indexed by <c>index</c>.
        /// </summary>
        public Bucket this[int index]
        {
            get
            {
                LazySort();
                return (Bucket)_buckets[index];
            }
            set
            {
                LazySort();
                _buckets[index] = value;
            }
        }

        /// <summary>Gets the number of buckets.</summary>
        public int Count => _buckets.Count;

        /// <summary>Gets the sum of the bucket depths.</summary>
        public double TotalDepth
        {
            get
            {
                double totalDepth = 0;
                for (int i = 0; i < Count; i++)
                    totalDepth += this[i].Depth;

                return totalDepth;
            }
        }

        /// <summary>Prints the buckets contained in the <see cref="Histogram"/>.</summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (Bucket b in _buckets)
                sb.Append(b);

            return sb.ToString();
        }


        public static Histogram OptimalDispersion(int bucketCount, ICollection distribution)
        {
            if (distribution.Count < Math.Max(bucketCount, 2))
                throw new ArgumentException("Not enough points in the distribution.");

            // "values" contains the sorted distribution.
            double[] values = new double[distribution.Count];
            distribution.CopyTo(values, 0);
            Array.Sort(values);

            // 'optimalCost[i,k]' contains the optimal costs for an
            // histogram with the 'i+1' first values and 'k' buckets.
            double[,] optimalCost = new double[values.Length, bucketCount];

            // 'lastBucketIndex[i,k]' contains the index of the first
            // value of the last bucket for optimal histogram comprising
            // the 'i+1' first values and 'k' buckets.
            int[,] lastBucketIndex = new int[values.Length, bucketCount];

            // 'prefixSum[i]' contains the sum of the 'i-1' first values.
            double[] prefixSum = new double[values.Length + 1];

            // Initialization of the prefix sums
            for (int i = 0; i < values.Length; i++)
                prefixSum[i + 1] = prefixSum[i] + values[i];

            // "One bucket" histograms initialization
            for (int i = 0, avg = 0; i < values.Length; i++)
            {
                while ((avg + 1) < values.Length &&
                       values[avg + 1] < prefixSum[i + 1] / (i + 1)) avg++;

                optimalCost[i, 0] =
                    prefixSum[i + 1] - 2 * prefixSum[avg + 1]
                    + (2 * avg - i + 1) * (prefixSum[i + 1] / (i + 1));
            }

            // "One value per bucket" histograms initialization
            for (int k = 0; k < bucketCount; k++)
            {
                // optimalCost[k, k] = 0;
                lastBucketIndex[k, k] = k;
            }

            // ----- Dynamic programming part -----

            // Loop on the number of buckets 
            // (note that there are 'k+1' buckets)
            for (int k = 1; k < bucketCount; k++)
                // Loop on the number of considered values
                // (note that there are 'i+1' considered values)
                for (int i = k; i < values.Length; i++)
                {
                    optimalCost[i, k] = double.PositiveInfinity;
                    // Loop for finding the optimal boundary of the last bucket
                    // ('j+1' is the index of the first value in the last bucket)
                    for (int j = (k - 1), avg = (k - 1); j < i; j++)
                    {
                        while ((avg + 1) < values.Length &&
                               values[avg + 1] < (prefixSum[i + 1] - prefixSum[j + 1]) / (i - j)) avg++;

                        double currentCost = optimalCost[j, k - 1] +
                                             prefixSum[i + 1] + prefixSum[j + 1] - 2 * prefixSum[avg + 1]
                                             + (2 * avg - i - j) * (prefixSum[i + 1] - prefixSum[j + 1]) / (i - j);

                        if (currentCost < optimalCost[i, k])
                        {
                            optimalCost[i, k] = currentCost;
                            lastBucketIndex[i, k] = j + 1;
                        }
                    }
                }

            // ----- Reconstitution of the histogram -----
            Histogram histogram = new Histogram();
            int index = values.Length - 1;
            for (int k = (bucketCount - 1); k >= 0; k--)
            {
                histogram.Add(new Bucket(values[lastBucketIndex[index, k]],
                                         values[index], index - lastBucketIndex[index, k] + 1));

                index = lastBucketIndex[index, k] - 1;
            }

            //histogram.JoinBuckets();
            return histogram;
        }


        /// <summary>Returns the optimal variance histogram.</summary>
        /// <param name="bucketCount">The number of buckets in the histogram.</param>
        /// <param name="distribution"><c>double</c> elements expected.</param>
        /// <remarks>Requires a computations time quadratic to 
        /// <c>distribution.Length</c>.</remarks>
        public static Histogram OptimalVariance(int bucketCount, ICollection distribution)
        {
            if (distribution.Count < bucketCount)
                throw new ArgumentException("Not enough points in the distribution.");

            // "values" contains the sorted distribution.
            double[] values = new double[distribution.Count];
            distribution.CopyTo(values, 0);
            Array.Sort(values);

            // 'optimalCost[i,k]' contains the optimal costs for an
            // histogram with the 'i+1' first values and 'k' buckets.
            double[,] optimalCost = new double[values.Length, bucketCount];

            // 'lastBucketIndex[i,k]' contains the index of the first
            // value of the last bucket for optimal histogram comprising
            // the 'i+1' first values and 'k' buckets.
            int[,] lastBucketIndex = new int[values.Length, bucketCount];

            // 'prefixSum[i]' contains the sum of the 'i-1' first values.
            double[] prefixSum = new double[values.Length + 1],
                     // 'sqPrefixSum' contains the sum of the 'i-1' first squared values.
                     sqPrefixSum = new double[values.Length + 1];

            // Initialization of the prefix sums
            for (int i = 0; i < values.Length; i++)
            {
                prefixSum[i + 1] = prefixSum[i] + values[i];
                sqPrefixSum[i + 1] = sqPrefixSum[i] + values[i] * values[i];
            }

            // "One bucket" histograms initialization
            for (int i = 0; i < values.Length; i++)
                optimalCost[i, 0] = sqPrefixSum[i + 1] -
                                    prefixSum[i + 1] * prefixSum[i + 1] / (i + 1);

            // "One value per bucket" histograms initialization
            for (int k = 0; k < bucketCount; k++)
            {
                // optimalCost[k, k] = 0;
                lastBucketIndex[k, k] = k;
            }

            // ----- Dynamic programming part -----

            // Loop on the number of buckets 
            // (note that there are 'k+1' buckets)
            for (int k = 1; k < bucketCount; k++)
                // Loop on the number of considered values
                // (note that there are 'i+1' considered values)
                for (int i = k; i < values.Length; i++)
                {
                    optimalCost[i, k] = double.PositiveInfinity;
                    // Loop for finding the optimal boundary of the last bucket
                    // ('j+1' is the index of the first value in the last bucket)
                    for (int j = (k - 1); j < i; j++)
                    {
                        double currentCost = optimalCost[j, k - 1] + sqPrefixSum[i + 1] - sqPrefixSum[j + 1]
                                             - (prefixSum[i + 1] - prefixSum[j + 1]) * (prefixSum[i + 1] - prefixSum[j + 1]) / (i - j);

                        if (currentCost < optimalCost[i, k])
                        {
                            optimalCost[i, k] = currentCost;
                            lastBucketIndex[i, k] = j + 1;
                        }
                    }
                }

            // ----- Reconstitution of the histogram -----
            Histogram histogram = new Histogram();
            int index = values.Length - 1;
            for (int k = (bucketCount - 1); k >= 0; k--)
            {
                histogram.Add(new Bucket(values[lastBucketIndex[index, k]],
                                         values[index], index - lastBucketIndex[index, k] + 1));

                index = lastBucketIndex[index, k] - 1;
            }

            //histogram.JoinBuckets();
            return histogram;
        }


        public static Histogram OptimalFreedom(int bucketCount, ICollection distribution)
        {
            if (distribution.Count < Math.Max(bucketCount, 2))
                throw new ArgumentException("Not enough points in the distribution.");

            // "values" contains the sorted distribution.
            double[] values = new double[distribution.Count];
            distribution.CopyTo(values, 0);
            Array.Sort(values);

            // 'optimalCost[i,k]' contains the optimal costs for an
            // histogram with the 'i+1' first values and 'k' buckets.
            double[,] optimalCost = new double[values.Length, bucketCount];

            // 'lastBucketIndex[i,k]' contains the index of the first
            // value of the last bucket for optimal histogram comprising
            // the 'i+1' first values and 'k' buckets.
            int[,] lastBucketIndex = new int[values.Length, bucketCount];

            // "One bucket" histograms initialization
            for (int i = 0; i < values.Length; i++)
                optimalCost[i, 0] = (values[i] - values[0]) * (i + 1);

            // "One value per bucket" histograms initialization
            for (int k = 0; k < bucketCount; k++)
            {
                // optimalCost[k, k] = 0;
                lastBucketIndex[k, k] = k;
            }

            // ----- Dynamic programming part -----

            // Loop on the number of buckets 
            // (note that there are 'k+1' buckets)
            for (int k = 1; k < bucketCount; k++)
                // Loop on the number of considered values
                // (note that there are 'i+1' considered values)
                for (int i = k; i < values.Length; i++)
                {
                    optimalCost[i, k] = double.PositiveInfinity;
                    // Loop for finding the optimal boundary of the last bucket
                    // ('j+1' is the index of the first value in the last bucket)
                    for (int j = (k - 1), avg = (k - 1); j < i; j++)
                    {
                        double currentCost = optimalCost[j, k - 1] +
                                             (values[i] - values[j + 1]) * (i - j);

                        if (currentCost < optimalCost[i, k])
                        {
                            optimalCost[i, k] = currentCost;
                            lastBucketIndex[i, k] = j + 1;
                        }
                    }
                }

            // ----- Reconstitution of the histogram -----
            Histogram histogram = new Histogram();
            int index = values.Length - 1;
            for (int k = (bucketCount - 1); k >= 0; k--)
            {
                histogram.Add(new Bucket(values[lastBucketIndex[index, k]],
                                         values[index], index - lastBucketIndex[index, k] + 1));

                index = lastBucketIndex[index, k] - 1;
            }

            //histogram.JoinBuckets();
            return histogram;
        }

        public static Histogram OptimalSquaredFreedom(int histSize, ICollection distribution)
        {
            if (distribution.Count < Math.Max(histSize, 2))
                throw new ArgumentException("Not enough points in the distribution.");

            // "values" contains the sorted distribution.
            double[] values = new double[distribution.Count];
            distribution.CopyTo(values, 0);
            Array.Sort(values);

            // 'optimalCost[i,k]' contains the optimal costs for an
            // histogram with the 'i+1' first values and 'k' buckets.
            double[,] optimalCost = new double[values.Length, histSize];

            // 'lastBucketIndex[i,k]' contains the index of the first
            // value of the last bucket for optimal histogram comprising
            // the 'i+1' first values and 'k' buckets.
            int[,] lastBucketIndex = new int[values.Length, histSize];

            // "One bucket" histograms initialization
            for (int i = 0; i < values.Length; i++)
                optimalCost[i, 0] =
                    (values[i] - values[0]) * (values[i] - values[0]) * (i + 1);

            // "One value per bucket" histograms initialization
            for (int k = 0; k < histSize; k++)
            {
                // optimalCost[k, k] = 0;
                lastBucketIndex[k, k] = k;
            }

            // ----- Dynamic programming part -----

            // Loop on the number of buckets 
            // (note that there are 'k+1' buckets)
            for (int k = 1; k < histSize; k++)
                // Loop on the number of considered values
                // (note that there are 'i+1' considered values)
                for (int i = k; i < values.Length; i++)
                {
                    optimalCost[i, k] = double.PositiveInfinity;
                    // Loop for finding the optimal boundary of the last bucket
                    // ('j+1' is the index of the first value in the last bucket)
                    for (int j = (k - 1), avg = (k - 1); j < i; j++)
                    {
                        double currentCost = optimalCost[j, k - 1] +
                                             (values[i] - values[j + 1]) * (values[i] - values[j + 1]) * (i - j);

                        if (currentCost < optimalCost[i, k])
                        {
                            optimalCost[i, k] = currentCost;
                            lastBucketIndex[i, k] = j + 1;
                        }
                    }
                }

            // ----- Reconstitution of the histogram -----
            Histogram histogram = new Histogram();
            int index = values.Length - 1;
            for (int k = (histSize - 1); k >= 0; k--)
            {
                histogram.Add(new Bucket(values[lastBucketIndex[index, k]],
                                         values[index], index - lastBucketIndex[index, k] + 1));

                index = lastBucketIndex[index, k] - 1;
            }

            //histogram.JoinBuckets();
            return histogram;
        }
    }

    /// <summary>An <see cref="Histogram"/> is build from a serie of <see cref="Bucket"/>s.</summary>
    [Serializable]
    public class Bucket : IComparable, ICloneable
    {
        /// <summary>
        /// This <c>IComparer</c> performs comparisons between a
        /// <c>double</c> and a <c>Bucket</c> objet.
        /// </summary>
        private class PointComparer : IComparer
        {
            /// <summary>Compares a <c>double</c> and <c>Bucket</c>.</summary>
            /// <returns>Zero if the <c>double</c> is included
            /// in the bucket.</returns>
            public int Compare(object obj1, object obj2)
            {
                Bucket bucket;
                double val = 0;
                int unit;

                var bucket1 = obj1 as Bucket;
                if (bucket1 != null)
                {
                    bucket = bucket1;
                    if (obj2 != null) val = (double) obj2;
                    unit = 1;
                }
                else
                {
                    bucket = (Bucket)obj2;
                    if (obj1 != null) val = (double) obj1;
                    unit = -1;
                }
                if (bucket != null && bucket.UpperBound < val) return -unit;
                return bucket != null && bucket.LowerBound <= val ? 0 : unit;
            }
        }

        static readonly PointComparer pointComparer = new PointComparer();

        /// <summary>Constructor.</summary>
        public Bucket(double lowerBound, double upperBound)
        {
            Debug.Assert(lowerBound <= upperBound,
                         "lowerBound should be smaller than the upperBound.");

            LowerBound = lowerBound;
            UpperBound = upperBound;
            //depth = 0;
        }

        /// <summary>Full constructor.</summary>
        public Bucket(double lowerBound, double upperBound, double depth)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Depth = depth;
        }

        /// <summary>Deep copy constructor.</summary>
        private Bucket(Bucket bucket)
        {
            LowerBound = bucket.LowerBound;
            UpperBound = bucket.UpperBound;
            Depth = bucket.Depth;
        }

        ///<summary>
        ///</summary>
        public double LowerBound { get; set; }

        ///<summary>
        ///</summary>
        public double UpperBound { get; set; }

        ///<summary>
        ///</summary>
        public double Width => UpperBound - LowerBound;

        ///<summary>
        ///</summary>
        public double Depth { get; set; }

        ///<summary>
        ///</summary>
        public static IComparer DefaultPointComparer => pointComparer;

        /// <summary>Comparison of two disjoint buckets.</summary>
        public int CompareTo(object bkt)
        {
            Bucket bucket = (Bucket)bkt;

            Debug.Assert(this.UpperBound <= bucket.LowerBound
                         || LowerBound >= bucket.UpperBound,
                         "Could not compare two intersecting buckets.");

            if (Width == 0 && bucket.Width == 0
                && LowerBound == bucket.LowerBound) return 0;

            if (bucket.UpperBound - LowerBound <= 0) return 1;
            return -1;
        }

        /// <summary>Returns a deep copy of this instance.</summary>
        public object Clone()
        {
            return new Bucket(this);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Bucket)) return false;

            Bucket b = (Bucket)obj;
            return (LowerBound == b.LowerBound)
                   && (UpperBound == b.UpperBound)
                   && (Depth == b.Depth);
        }

        public override string ToString()
        {
            return "[" + LowerBound + ";" + UpperBound + "]";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}