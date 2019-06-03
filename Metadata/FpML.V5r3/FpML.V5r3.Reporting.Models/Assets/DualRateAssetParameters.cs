using System;

namespace Orion.Models.Assets
{
    public class DualRateAssetParameters : ISimpleDualAssetParameters
    {
        public string[] Metrics { get; set; }

        #region ISimpleDualAssetParameters Members

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the base npv.
        /// </summary>
        /// <value>The base npv.</value>
        public decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal PaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public Decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public Decimal YearFraction { get; set; }

        #endregion
    }
}