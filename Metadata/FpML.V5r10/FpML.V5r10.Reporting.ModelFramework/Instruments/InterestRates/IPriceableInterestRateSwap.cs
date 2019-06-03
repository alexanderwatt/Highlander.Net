using System;

namespace FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates
{
    ///<summary>
    ///</summary>
    ///<typeparam name="AMP"></typeparam>
    ///<typeparam name="AMR"></typeparam>
    public interface IPriceableInterestRateSwap<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets a value indicating whether [base party paying fixed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party paying fixed]; otherwise, <c>false</c>.
        /// </value>
        Boolean BasePartyPayingFixed { get; }

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
    }
}