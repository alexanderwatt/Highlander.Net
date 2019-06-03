using System;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates
{
    ///<summary>
    ///</summary>
    ///<typeparam name="AMP"></typeparam>
    ///<typeparam name="AMR"></typeparam>
    public interface IPriceableBondTransaction<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets a value indicating whether [base party paying fixed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party buyer]; otherwise, <c>false</c>.
        /// </value>
        Boolean BasePartyBuyer { get; }

        /// <summary>
        /// Gets the settlement date.
        /// </summary>
        /// <value>The settlement date.</value>
        DateTime SettlementDate { get; }

        /// <summary>
        /// Gets the maturity date.
        /// </summary>
        /// <value>The maturity date.</value>
        DateTime MaturityDate { get; }

        /// <summary>
        /// Gets the underlying bond.
        /// </summary>
        /// <value>The underlying bond.</value>
        IPriceableBondAssetController UnderlyingBond { get; }

        /// <summary>
        /// Gets a value indicating whether [adjust calculation dates indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust calculation dates indicator]; otherwise, <c>false</c>.
        /// </value>
        Boolean AdjustCalculationDatesIndicator { get; }
    }
}