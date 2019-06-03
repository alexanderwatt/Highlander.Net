using System;

namespace Orion.Models.Futures
{
    public interface IFuturesAssetParameters
    {
        /// <summary>
        /// Flag that sets whether the contract is deliverable.
        /// </summary>
        Boolean IsDeliverable{ get; set; }

        /// <summary>
        /// The starting npv.
        /// </summary>
        Decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        Decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>The number.</value>
        int NumberOfContracts { get; set; }

        /// <summary>
        /// Gets or sets the trade price.
        /// </summary>
        /// <value>The trade price.</value>
        Decimal TradePrice { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal Multiplier { get; set; }

        /// <summary>
        /// The accrual period as a decimal.
        /// </summary>
        Decimal AccrualPeriod { get; set; }

        /// <summary>
        /// Gets or sets the contract notional.
        /// </summary>
        ///  <value>The contract notional.</value>
        Decimal ContractNotional { get; set; }

        /// <summary>
        /// Gets or sets the settlement discount factor.
        /// </summary>
        /// <value>The quote.</value>
        Decimal SettlementDiscountFactor { get; set; }
    }
}