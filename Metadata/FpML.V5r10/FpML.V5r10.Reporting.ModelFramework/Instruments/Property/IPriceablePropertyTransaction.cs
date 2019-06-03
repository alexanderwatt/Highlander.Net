using System;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.Instruments.Property
{
    ///<summary>
    ///</summary>
    ///<typeparam name="AMP"></typeparam>
    ///<typeparam name="AMR"></typeparam>
    public interface IPriceablePropertyTransaction<AMP, AMR> : IMetricsCalculation<AMP, AMR>
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
        DateTime PaymentDate { get; }

        /// <summary>
        /// Gets the underlying equity.
        /// </summary>
        /// <value>The underlying equity.</value>
        IPriceablePropertyAssetController UnderlyingProperty { get; }
    }
}