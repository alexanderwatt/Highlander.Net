using System;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Instruments.InterestRates
{
    ///<summary>
    ///</summary>
    public interface IPriceableCapFloor<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets the seller party reference.
        /// </summary>
        /// <value>The seller party reference.</value>
        string SellerPartyReference { get; }

        /// <summary>
        /// Gets the buyer party reference.
        /// </summary>
        /// <value>The buyer party reference.</value>
        string BuyerPartyReference { get; }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <value>The effective date.</value>
        DateTime EffectiveDate { get; }

        /// <summary>
        /// Gets the termination date.
        /// </summary>
        /// <value>The termination date.</value>
        DateTime TerminationDate { get; }

        /// <summary>
        /// Gets a value indicating whether [adjust calculation dates indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust calculation dates indicator]; otherwise, <c>false</c>.
        /// </value>
        Boolean AdjustCalculationDatesIndicator { get; }

        ///// <summary>
        ///// Gets or sets the cap floor stream.
        ///// </summary>
        ///// <value>The cap floor stream.</value>
        //IPriceableInstrumentController<InterestRateStream> CapFloorStream { get; set; }
    }
}