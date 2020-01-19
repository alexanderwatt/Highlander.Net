using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableRateSpreadAssetController : PriceableRateAssetController, IPriceableRateSpreadAssetController
    {
        #region IPriceableAssetController Members

        /// <summary>
        /// The spread quotation
        /// </summary>
        public BasicQuotation Spread => MarketQuote;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public abstract IList<decimal> Values { get; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public abstract decimal ValueAtMaturity { get; }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public abstract IList<DateTime> GetRiskDates();

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public abstract decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace);

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the marketquote of the asset.</returns>
        public abstract decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace);

        #endregion
    }
}