#region Using directives


#endregion

using System;
using FpML.V5r10.Reporting.ModelFramework;

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    /// Linear Interpolator base class
    /// </summary>
    public class SpreadInterpolation : IInterpolation
    {
        private LinearRateInterpolation BaseCurve{ get; }

        private LinearInterpolation SpreadCurve { get; }

        /// <summary>
        /// Perform a linear interpolation on a sorted array of times, where the value
        /// is relative to a base value that has been transfored to a gog linear zero rate interpolated base value/>
        /// </summary>
        /// <param name="basetimes">The base times array of values</param>
        /// <param name="basevalues">The base values array to intepolate</param>
        /// <param name="spreadtimes">The spread times array of values</param>
        /// <param name="spreadvalues">The spread values array to intepolate</param>
        protected SpreadInterpolation(double[] basetimes, double[] basevalues, double[] spreadtimes, double[] spreadvalues)
        {
            BaseCurve = LinearRateInterpolation.Interpolate(basetimes, basevalues);
            SpreadCurve = LinearInterpolation.Interpolate(spreadtimes, spreadvalues);
        }

        /// <summary>
        /// Perform a linear interpolation on a sorted array of times, where the value
        /// is relative to a base value that has been transfored to a gog linear zero rate interpolated base value/>
        /// </summary>
        /// <param name="basetimes">The base times array of values</param>
        /// <param name="basevalues">The base values array to intepolate</param>
        /// <param name="spreadtimes">The spread times array of values</param>
        /// <param name="spreadvalues">The spread values array to intepolate</param>
        /// <returns></returns>
        public static SpreadInterpolation Interpolate(double[] basetimes, double[] basevalues, double[] spreadtimes, double[] spreadvalues)
        {
            if (spreadtimes.Length != spreadvalues.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            return new SpreadInterpolation(basetimes, basevalues, spreadtimes, spreadvalues);
        }

        /// <summary>
        /// Perform a model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time value</param>
        /// <param name="extrapolation">This is not currently implemented.</param>
        /// <returns></returns>
        public double ValueAt(double time, bool extrapolation)
        {
            var result = BaseCurve.ValueAt(time, extrapolation) + SpreadCurve.ValueAt(time, extrapolation);
            return result;
        }

        /// <summary>
        /// Initialize a class constructed by the default constructor.
        /// </summary>
        /// <remarks>
        /// The sequence of values for x must have been sorted.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the passed lists do not have the same size
        /// or do not provide enough points to interpolate.
        /// </exception>
        public void Initialize(double[] x, double[] y)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Perform an interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time axis value</param>
        /// <returns></returns>
        public double ValueAt(double time)
        {
            return ValueAt(time, true);
        }
    }
}