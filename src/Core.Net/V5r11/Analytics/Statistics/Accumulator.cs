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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Highlander.Reporting.Analytics.V5r3.Statistics
{
    /// <summary>
    /// The <c>Accumulator</c> provides online algorithms to computes the first
    /// statistical moments and their derivatives.
    /// </summary>
    /// <remarks>
    /// <p>The <c>Accumulator</c> provides memory efficient online algorithms
    /// to compute the first statistical moments (mean, variance) and their
    /// derivatives (sigma, error estimate).</p>
    /// <p>The memory required by the accumulator is <c>O(1)</c> independent
    /// from the distribution size. All methods are executed in a <c>O(1)</c>
    /// computational time.
    /// </p>
    /// <p>The <c>Accumulator</c> is not thread safe.</p>
    /// </remarks>
    public class Accumulator
    {
        /* Design note (joannes):
         * The Min/Max have not been included on purpose. It usually clearer
         * (because being trivial) to manage explicitely in the client the Min/Max 
         * than using a library to do so.
         * 
         * The skewness and kurtosis have not been included because I never heard of
         * anyone using those indicator in practice.
         * */

        #region Private members
        /// <summary>
        /// Sum of the values added to the accumulator.
        /// </summary>
        private double _sum;

        /// <summary>
        /// Sum of the square of the values added to the accumulator.
        /// </summary>
        private double _squaredSum;

        /// <summary>
        /// Number of values added to the accumulator.
        /// </summary>
        private int _count;

        private double _max = Double.MaxValue;
        private double _min = Double.MinValue;
        private int _sampleNumber; // = 0;
        private double _sampleWeight; //  = 0.0;
        private double _quadraticSum; //  = 0.0;
        private double _downsideQuadraticSum; //  = 0.0;
        private double _cubicSum; //  = 0.0;
        private double _fourthPowerSum; //  = 0.0;
        
        private readonly List<double> _values = new List<double>();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty <c>Accumulator</c>.
        /// </summary>
        public Accumulator()
        {
            Clear();
        }

        /// <summary>
        /// Creates an <c>Accumulator</c> that contains the provided values.
        /// </summary>
        public Accumulator(double[] values)
        {
            Clear();
            AddRange(values);
        }

        /// <summary>
        /// Creates an <c>Accumulator</c> that contains the provided values.
        /// </summary>
        public Accumulator(ICollection values)
        {
            Clear();
            AddRange(values);
        }

        /// <summary>
        /// Creates an <c>Accumulator</c> that contains the provided values.
        /// </summary>
        public Accumulator(Sample[] values)
        {
            Clear();
            AddRange(values);
        }
        
        #endregion

        #region Add/Remove
        /// <summary>
        /// Adds a real value to the accumulator.
        /// </summary>
        public void Add(double value)
        {
            Add(value, 1.0);
        }

        /// <summary>
        /// Adds a weighted datum to the set.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="weight">Weights must be positive or null.</param>
        public void Add(double value, double weight)
        {
            if (weight < 0.0)
                throw new ArgumentOutOfRangeException(nameof(weight), weight,
                                                      "StatAddNeg");
            if (_sampleNumber == int.MaxValue)
                throw new OverflowException("StatMax");

            _sampleNumber++;
            _sampleWeight += weight;

            double temp = weight * value;
            _sum += temp;
            temp *= value;
            _quadraticSum += temp;
            _downsideQuadraticSum += value < 0.0 ? temp : 0.0;
            temp *= value;
            _cubicSum += temp;
            temp *= value;
            _fourthPowerSum += temp;
            _min = Math.Min(value, _min);
            _max = Math.Max(value, _max);
            _count = _sampleNumber;
            _squaredSum = _quadraticSum;
            _values.Add(weight * value);
        }

        /// <summary>
        /// Adds a range of values to the accumulator.
        /// </summary>
        public void AddRange(double[] values)
        {
            foreach (double t in values)
                Add(t);
        }

        /// <summary>
        /// Adds a range of values to the accumulator.
        /// </summary>
        public void AddRange(ICollection values)
        {
            foreach(object obj in values)
            {
                if(!(obj is double))
                    throw new ArgumentException("#E00 Only 'double's could be added to the accumulator.");
                Add((double) obj);
            }
        }

        /// <summary>
        /// Adds a weighted sample to the set.
        /// </summary>
        public void Add(Sample sample)
        {
            Add((double)sample, sample.Weight);
        }

        ///<summary>
        ///</summary>
        ///<param name="samples"></param>
        public void AddRange(Sample[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
                Add((double)samples[i], samples[i].Weight);	// unsafe=faster?
        }

        /// <summary>
        /// Adds a sequence of data (doubles or Samples) to the set.
        /// See the other overloads for faster methods on arrays.
        /// </summary>
        /// <param name="sequence">A sequence of Samples or doubles.</param>
        public void AddRange(IEnumerable sequence)
        {
            IEnumerator it = sequence.GetEnumerator();
            it.MoveNext();
            if (it.Current is Sample)
            {
                foreach (Sample sample in sequence)
                    Add((double)sample, sample.Weight);
            }
            else // assume doubles...
            {
                foreach (double d in sequence)
                    Add(d, 1.0);
            }
        }
        
        /// <summary>
        /// Clears (re-initialize) the accumulator.
        /// </summary>
        public void Clear()
        {
            _sum = 0d;
            _squaredSum =0d;
            _count = 0;
            _min = Double.MaxValue;
            _max = Double.MinValue;
            _sampleNumber = 0;
            _sampleWeight = 0.0;
            _sum = 0.0;
            _quadraticSum = 0.0;
            _downsideQuadraticSum = 0.0;
            _cubicSum = 0.0;
            _fourthPowerSum = 0.0;
        }


        /// <summary>
        /// Removes a value from the accumulator.
        /// </summary>
        /// <remarks>
        /// <p>Caution: the <c>Accumulator</c> does not explicitely records the
        /// added values. Therefore, no exception will be thrown if an attemp
        /// is made to remove a value that have not been previously added to
        /// the accumulator.</p>
        /// </remarks>
        public void Remove(double value)
        {
            if(_count <= 0) throw new InvalidOperationException(
                "#E00 No value could be removed because the accumulator is empty.");

            _sum -= value;
            _squaredSum -= value * value;
            _count--;
        }

        /// <summary>
        /// Removes a range of values from the accumulator.
        /// </summary>
        public void RemoveRange(double[] values)
        {
            foreach (double t in values)
                Remove(t);
        }

        /// <summary>
        /// Removes a range of values from the accumulator.
        /// </summary>
        public void RemoveRange(ICollection values)
        {
            foreach(object obj in values)
            {
                if(!(obj is double))
                    throw new ArgumentException("#E00 Only 'double's could be removed from the accumulator.");
                Remove((double) obj);
            }
        }
        #endregion

        #region Get Sample

        /// <summary>
        /// Get samples. 
        /// </summary>
        /// <returns></returns>
        public double[] Sample()
        {
            return _values.ToArray();
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Gets the number of values added to the accumulator.
        /// </summary>
        public int Count => _sampleNumber;

        /// <summary>
        /// Sum of data weights.
        /// </summary>
        public double WeightSum => _sampleWeight;

        /// <summary>
        /// Gets the mean error estimate defined as the square root of the ratio of 
        /// the standard deviation to the number of values added to the accumulator.
        /// </summary>
        public double ErrorEstimate
        {
            get
            {
                if (_count <= 0) throw new InvalidOperationException(
                    "#E00 No error estimate available. The accumulator is empty.");

                return Sigma / Math.Sqrt(_count);
            }
        }

        /// <summary>
        /// Gets the mean of the values added to the accumulator.
        /// </summary>
        public double Mean
        {
            get
            {
                if (_count <= 0) throw new InvalidOperationException(
                    "#E00 No mean available. The accumulator is empty.");

                return (_sum / _count);
            }
        }

        /// <summary>
        /// Gets the standard deviation of the values added to the accumulator.
        /// </summary>
        public double Sigma => Math.Sqrt(Variance);

        /// <summary>
        /// Gets the variance of the values added to the accumulator.
        /// </summary>
        public double Variance
        {
            get
            {
                if(_count <= 0) throw new InvalidOperationException(
                    "#E00 No variance available. The accumulator is empty.");

                double mean = Mean;
                return (_squaredSum / _count - mean * mean);
            }
        }

        /// <summary>
        /// The standard deviation sigma.
        /// </summary>
        /// <latex>
        /// Standard deviation \f$ \sigma \f$, defined as the
        /// square root of the variance.
        /// </latex>
        public double StandardDeviation => Math.Sqrt(Variance);

        /// <summary>
        /// The downside variance.
        /// </summary>
        /// <latex>
        /// Defined as
        /// \f[ \frac{N}{N-1} \times \frac{ \sum_{i=1}^{N}
        /// \theta \times x_i^{2}}{ \sum_{i=1}^{N} w_i} \f],
        /// where \f$ \theta \f$ = 0 if x > 0 and \f$ \theta \f$ =1 if x &lt;0
        /// </latex>
        public double DownsideVariance
        {
            get
            {
                if (_sampleNumber <= 0)
                    throw new InvalidOperationException("StatEmpty");
                if (_sampleWeight <= 0.0)
                    throw new InvalidOperationException("StatWeight0");

                return _sampleNumber / (_sampleNumber - 1.0) *
                       _downsideQuadraticSum / _sampleWeight;
            }
        }

        /// <summary>
        /// Downside deviation, defined as the square root of the 
        /// downside variance.
        /// </summary>
        public double DownsideDeviation => Math.Sqrt(DownsideVariance);


        /// <summary>
        /// Skewness.
        /// </summary>
        /// <latex>
        /// defined as
        /// \f[ 
        ///		\frac{N^2}{(N-1)(N-2)} \frac{\left\langle \left(
        ///		x-\langle x \rangle \right)^3 \right\rangle}{\sigma^3}. 
        ///	\f]
        /// </latex>
        /// <remarks>
        /// The above evaluates to 0.0 for a Gaussian distribution.
        /// </remarks>
        public double Skewness
        {
            get
            {
                if (_sampleNumber <= 2)
                    throw new InvalidOperationException("StatSkew2");

                double s = StandardDeviation;
                if (s == 0.0) return 0.0;
                double m = Mean;

                return _sampleNumber * _sampleNumber /
                       ((_sampleNumber - 1.0) * (_sampleNumber - 2.0) * s * s * s) *
                       (_cubicSum - 3.0 * m * _quadraticSum + 2.0 * m * m * _sum) / _sampleWeight;
            }
        }

        /// <summary>
        /// Excess kurtosis.
        /// </summary>
        /// <latex>
        /// defined as
        /// \f[ \frac{N(N+1)}{(N-1)(N-2)(N-3)}
        /// \frac{\left\langle \left( x-\langle x \rangle \right)^4
        /// \right\rangle}{\sigma^4} - \frac{3(N-1)^2}{(N-2)(N-3)}. \f]
        /// </latex>
        /// <remarks>
        /// The above evaluates to 0 for a Gaussian distribution.
        /// </remarks>
        public double Kurtosis
        {
            get
            {
                if (_sampleNumber <= 3)
                    throw new InvalidOperationException("StatKurt3");

                double m = Mean;
                double v = Variance;

                if (v == 0)
                    return -3.0 * (_sampleNumber - 1.0) * (_sampleNumber - 1.0) /
                           ((_sampleNumber - 2.0) * (_sampleNumber - 3.0));

                return _sampleNumber * _sampleNumber * (_sampleNumber + 1.0) /
                       ((_sampleNumber - 1.0) * (_sampleNumber - 2.0) *
                        (_sampleNumber - 3.0) * v * v) *
                       (_fourthPowerSum - 4.0 * m * _cubicSum + 6.0 * m * m * _quadraticSum -
                        3.0 * m * m * m * _sum) / _sampleWeight -
                       3.0 * (_sampleNumber - 1.0) * (_sampleNumber - 1.0) /
                       ((_sampleNumber - 2.0) * (_sampleNumber - 3.0));
            }
        }

        /// <summary>
        /// The minimum sample value.
        /// </summary>
        public double Min
        {
            get
            {
                if (_sampleNumber <= 0)
                    throw new InvalidOperationException("StatEmpty");
                return _min;
            }
        }

        /// <summary>
        /// The maximum sample value.
        /// </summary>
        public double Max
        {
            get
            {
                if (_sampleNumber <= 0)
                    throw new InvalidOperationException("StatEmpty");
                return _max;
            }
        }

        #endregion
    }
}