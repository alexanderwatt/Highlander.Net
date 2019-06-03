#region Using directives

using System;
using System.Collections.Generic;

using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolators
{
    /// <summary>
    /// This class will not implement any of IInterpolation but needs it's signature
    /// It is the abstract base for n-dimensional linear interpolators
    /// </summary>
    public abstract class Interpolator : IInterpolation
    {
        /// <summary>
        /// A method to interpolate boundary points to generate a value for an enclosed point
        /// </summary>
        /// <param name="point">The point to value</param>
        /// <param name="bounds">The bounding points to interpolate on</param>
        /// <returns></returns>
        public abstract double Value(IPoint point, List<IPoint> bounds);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public double ValueAt(double point, bool bounds)
        {
            throw new NotImplementedException();
        }

        #region IInterpolation Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public Curve GetExtremeCurve()
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="previousIndex"></param>
        /// <returns></returns>
        public int Locate(double point, bool previousIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="allowExtrapolation"></param>
        /// <returns></returns>
        public double Value(double point, bool allowExtrapolation)
        {
            throw new NotImplementedException();
        }        


        #endregion
    }
}
