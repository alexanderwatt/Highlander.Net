#region Using directives

using System;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IVolatilitySurface2 : IVolatilitySurface
    {
        /// <summary>
        /// For Igor.
        /// </summary>
        /// <param name="expirationAsDate"></param>
        /// <param name="strike"></param>
        /// <returns></returns>
        double GetValueByExpirationAndStrike(DateTime expirationAsDate, double strike);
    }
}