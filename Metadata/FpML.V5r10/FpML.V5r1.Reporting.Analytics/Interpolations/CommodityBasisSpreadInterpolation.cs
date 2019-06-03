#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Extreme.Mathematics.Curves;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;
using Orion.Numerics.Interpolations;
using Orion.Analytics.Utilities;
using Orion.Analytics.PricingEngines;
using Orion.Analytics.Stochastics.SABR;


#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    ///Class that encapsulates functionality to perform piecewise Linear
    /// interpolation with flat-line extrapolation.
    /// </summary>
    public class CommodityBasisSpreadInterpolation : EOLinearInterpolation, IInterpolation
    {
        ///<summary>
        /// The base curve interpolated space. This must be one dimensional.
        /// Also, the curve itself is assumed to be a discount factor curve.
        ///</summary>
        public ICommodityCurve BaseCurve { get; set; }

        #region Constructors

        /// <summary>
        /// Constructor for the <see cref="LinearInterpolation"/> class that
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
        public new double ValueAt(double x, bool allowExtrapolation)//TODO how to extrapolate??
        {
            var baseValue = BaseCurve.GetForward(x);
            var spreadValue = Curve.ValueAt(x);
            var spreadDf = baseValue + spreadValue;
            return spreadDf;
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
