#region Using Directives

using System;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;

#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    ///Class that encapsulates functionality to perform piecewise Linear
    /// interpolation with flat-line extrapolation.
    /// </summary>
    public class CommodityBasisSpreadInterpolation : IInterpolation
    {
        private IInterpolation _curve;

        ///<summary>
        /// The base curve interpolated space. This must be one dimensional.
        /// Also, the curve itself is assumed to be a discount factor curve.
        ///</summary>
        public ICommodityCurve BaseCurve { get; set; }

        #region Constructors

        /// <summary>
        /// Constructor for the <see cref="CommodityBasisSpreadInterpolation"/> class that
        /// transfers the array of x and y values into the data structure that
        /// stores (x,y) points into ascending order based on the x-value of 
        /// each point.
        /// </summary>
        public CommodityBasisSpreadInterpolation()
        { }

        /// <summary>
        /// Constructor for the <see cref="CommodityBasisSpreadInterpolation"/> class that
        /// transfers the array of x and y values into the data structure that
        /// stores (x,y) points into ascending order based on the x-value of 
        /// each point.
        /// </summary>
        public CommodityBasisSpreadInterpolation(ICommodityCurve baseCurve)
        {
            BaseCurve = baseCurve;
        }

        #endregion Constructors

        #region Overrides

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
        public double ValueAt(double x, bool allowExtrapolation)
        {
            var baseValue = BaseCurve.GetForward(x);
            var spreadValue = _curve.ValueAt(x, allowExtrapolation);
            var spreadDf = baseValue + spreadValue;
            return spreadDf;
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
            _curve = new LinearInterpolation();
            _curve.Initialize(x, y);
        }

        #endregion Constructors

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new CommodityBasisSpreadInterpolation(BaseCurve);
        }
    }
}
