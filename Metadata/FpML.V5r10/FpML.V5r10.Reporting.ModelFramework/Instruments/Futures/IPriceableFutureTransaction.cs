using System;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.Instruments.Futures
{
    ///<summary>
    ///</summary>
    ///<typeparam name="TAmp"></typeparam>
    ///<typeparam name="TAmr"></typeparam>
    public interface IPriceableFutureTransaction<TAmp, TAmr> : IMetricsCalculation<TAmp, TAmr>
    {
        /// <summary>
        /// Gets a value indicating whether [base party paying fixed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party buyer]; otherwise, <c>false</c>.
        /// </value>
        Boolean BasePartyBuyer { get; }

        /// <summary>
        /// Gets the last margin payment date.
        /// </summary>
        /// <value>The last margin payment date.</value>
        DateTime LastMarginPaymentDate { get; }

        /// <summary>
        /// Gets the next margin payment date.
        /// </summary>
        /// <value>The next margin payment date.</value>
        DateTime NextMarginPaymentDate { get; }

        /// <summary>
        /// Gets the expiry date.
        /// </summary>
        /// <value>The expiry date.</value>
        DateTime ExpiryDate { get; }

        /// <summary>
        /// Gets the delivery date.
        /// </summary>
        /// <value>The delivery date.</value>
        DateTime DeliveryDate { get; }

        /// <summary>
        /// Gets the previous settlement value.
        /// </summary>
        /// <value>The previous settlement value.</value>
        Decimal PreviousSettlementValue { get; }

        /// <summary>
        /// Gets the current settlement value.
        /// </summary>
        /// <value>The current settlement value.</value>
        Decimal CurrentSettlementValue { get; }

        /// <summary>
        /// Gets the trade price.
        /// </summary>
        /// <value>The trade price.</value>
        Decimal TradePrice { get; }

        /// <summary>
        /// Gets the underlying future.
        /// </summary>
        /// <value>The underlying future.</value>
        IPriceableFuturesAssetController UnderlyingFuture { get; }
    }
}