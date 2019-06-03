#region Using directives

using System;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface ISwaptionATMVolatilitySurface : IVolatilitySurface
    {
        /// <summary>
        /// Gets the volatility using a DateTime expiry and a tenor value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="expirationAsDate">The expiration date.</param>
        /// <param name="tenor">The underlying tenor.</param>
        /// <returns>The interpolated value.</returns>
        Double GetValueByExpiryDateAndTenor(DateTime baseDate, DateTime expirationAsDate, String tenor);

        /// <summary>
        /// Gets the volatility using a DateTime expiry and a tenor value.
        /// </summary>
        /// <param name="expirationTerm">The expiration term.</param>
        /// <param name="tenor">The underlying tenor.</param>
        /// <returns>The interpolated value.</returns>
        Double GetValueByExpiryTermAndTenor(String expirationTerm, String tenor);
    }
}