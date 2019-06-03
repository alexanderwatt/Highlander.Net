using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base spread asset controller interface
    /// </summary>
    public interface IPriceableSpreadAssetController2 : IPriceableSpreadAssetController
    {
        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace);

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the marketquote of the asset.</returns>
        decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace);
    }

    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableSpreadAssetController : IPriceableAssetController
    {

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        // promised by alex not used.        
        Decimal ValueAtMaturity { get; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        IList<Decimal> Values { get; }

        /// <summary>
        /// Returns a set of risk dates
        /// </summary>
        /// <returns></returns>
        IList<DateTime> GetRiskDates();
    }
}