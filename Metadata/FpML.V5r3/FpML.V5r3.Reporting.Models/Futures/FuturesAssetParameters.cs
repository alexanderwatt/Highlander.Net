using System;

namespace Orion.Models.Futures
{
    public class FuturesAssetParameters : IFuturesAssetParameters
    {
        public string[] Metrics { get; set; }

        #region IFuturesAssetParameters Members

        /// <summary>
        /// Gets the base NPV.
        /// </summary>
        /// <value>The npv.</value>
        public Decimal BaseNPV { get; set; }

        /// <summary>
        /// Flag that sets whether the contract is deliverable.
        /// </summary>
        public bool IsDeliverable { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        public decimal Quote { get; set; }

        /// <summary>
        /// The number of contracts
        /// </summary>
        public int NumberOfContracts { get; set; }

        /// <summary>
        /// The trade price
        /// </summary>
        public decimal TradePrice { get; set; }

        /// <summary>
        /// The multiplier which must be set.
        /// </summary>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// The accrual period as a decimal.
        /// </summary>
        public decimal AccrualPeriod { get; set; }

        /// <summary>
        /// Gets or sets the contract notional.
        /// </summary>
        ///  <value>The contract notional.</value>
        public Decimal ContractNotional { get; set; }

        /// <summary>
        /// Gets or sets the settlement discount factor.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal SettlementDiscountFactor { get; set; }

        #endregion
    }
}