#region Usings

using System;
using System.Collections.Generic;
using Orion.ModelFramework.Assets;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IRateCurve : ICurve, ICloneable
    {
        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        List<IPriceableRateAssetController> PriceableRateAssets { get; }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetDiscountFactor(DateTime baseDate, DateTime targetDate);

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        double GetDiscountFactor(double time);

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetDiscountFactor(DateTime targetDate);

        /// <summary>
        /// Gets the asset quotations.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, Decimal> GetAssetQuotations();

        /// <summary>
        /// Gets the assets.
        /// </summary>
        /// <returns></returns>
        Asset[] GetAssets();

        /// <summary>
        /// Gets the days and zero rates.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="compounding">The compounding.</param>
        /// <returns></returns>
        IDictionary<int, Double> GetDaysAndZeroRates(DateTime baseDate, string compounding);

        /// <summary>
        /// Creates the basic rate curve risk set, using the current curve as the base curve.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point pertubation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of pertubed rate curves</returns>
        List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation);
    }
}