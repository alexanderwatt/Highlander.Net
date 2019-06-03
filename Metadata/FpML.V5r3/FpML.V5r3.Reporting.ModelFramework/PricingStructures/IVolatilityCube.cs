#region Using directives

using System;

#endregion

namespace Orion.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IVolatilityCube : IPricingStructure
    {
        /// <summary>
        /// Returns the value of the volatility; interpolated or not - depends on the implementation.
        /// </summary>
        double GetValue(string expiryInterval, string termInterval, decimal strike);

        /// <summary>
        /// Returns the value of the volatility; interpolated or not - depends on the implementation.
        /// </summary>
        double GetValue(string expiryInterval, string termInterval, double strike);

        /// <summary>
        /// Returns the value of the volatility; interpolated or not - depends on the implementation.
        /// </summary>
        double GetValue(DateTime baseDate, DateTime expirationAsDate, double maturityYearFraction, decimal strike);
    }
}