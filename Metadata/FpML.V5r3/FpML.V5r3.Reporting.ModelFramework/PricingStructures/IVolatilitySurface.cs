#region Using directives

using System;

#endregion

namespace Orion.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IVolatilitySurface : IPricingStructure
    {
        /// <summary>
        /// Returns the (interpolated or not - depends on the implementation) value of the volatility.
        /// </summary>
        double GetValue(double dimension1, double dimension2);

        ///<summary>
        /// Returns the underlying surface data
        ///</summary>
        ///<returns></returns>
        object[,] GetSurface();
    }
}