#region Using Directives

using System;
using System.Collections.Generic;
using Extreme.Mathematics.Curves;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;
using Orion.Numerics.Interpolations;

#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    ///Class that encapsulates functionality to perform piecewise Linear
    /// interpolation with flat-line extrapolation.
    /// </summary>
    public class RateBasisSpreadInterpolation : EOLinearInterpolation, IInterpolation
    {
        ///<summary>
        /// The base curve interpolated space. This must be one dimensional.
        /// Also, the curve itself is assumed to be a discount factor curve.
        ///</summary>
        public IRateCurve BaseCurve { get; set; }

        #region Constructors

        /// <summary>
        /// Constructor for the <see cref="LinearInterpolation"/> class that
        /// transfers the array of x and y values into the data structure that
        /// stores (x,y) points into ascending order based on the x-value of 
        /// each point.
        /// </summary>
        public RateBasisSpreadInterpolation(IRateCurve baseCurve)
        {
            BaseCurve = baseCurve;
        }

        #endregion Constructors

        #region Overrides

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
        public new void Initialize(double[] x, double[] discountFactors)
        {
            var zeroes = new List<double>();
            var index = 0;
            foreach (var df in discountFactors)
            {
                var time = x[index];
                var baseDf = BaseCurve.GetDiscountFactor(time);
                var ratio = df/baseDf;
                var zero = 0.0;
                if (ratio != 1)
                {
                    zero = Math.Log(ratio)/-time;
                }
                zeroes.Add(zero);
                index++;
            }
            var linearCurve = new PiecewiseLinearCurve(x, zeroes.ToArray());
            Curve = linearCurve;
        }

        /// <summary>
        /// Interpolated value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="allowExtrapolation"></param>
        /// <returns>The interpolated value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when extrapolation has not been allowed and the passed value
        /// is outside the allowed range.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the class was not properly initialized.
        /// </exception>
        public new double ValueAt(double x, bool allowExtrapolation)//TODO how to extrapolate??
        {
            var baseDf = BaseCurve.GetDiscountFactor(x);
            var zero = Curve.ValueAt(x);
            var spreadDf = Math.Exp(-zero*x);
            var df = baseDf*spreadDf;
            return df;
        }

        #endregion Constructors

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new RateBasisSpreadInterpolation(BaseCurve);
        }
    }
}
