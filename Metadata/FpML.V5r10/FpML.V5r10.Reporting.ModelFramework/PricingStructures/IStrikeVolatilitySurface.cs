#region Using directives

using System;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IStrikeVolatilitySurface : IVolatilitySurface
    {
        /// <summary>
        /// Gets the volatility using a DateTime expiry and a strike value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="expirationAsDate">The expiration date.</param>
        /// <param name="strike">The strike required.</param>
        /// <returns>The interpolated value.</returns>
        Double GetValueByExpiryDateAndStrike(DateTime baseDate, DateTime expirationAsDate, double strike);

        /// <summary>
        /// Gets the volatility using a term expiry and a strike value.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="strike"></param>
        /// <returns></returns>
        Double GetValueByExpiryTermAndStrike(String term, double strike);
    }
}