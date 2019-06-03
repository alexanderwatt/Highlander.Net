using System;

namespace Orion.Models.Assets
{
    public class RateAssetParameters : ISimpleRateAssetParameters
    {
        public string[] Metrics { get; set; }

        #region ISimpleAssetParameters Members

        /// <summary>
        /// Gets or sets the dual curve approach.
        /// </summary>
        /// <value>The notional.</value>
        public bool IsDualCurve { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the base NPV.
        /// </summary>
        /// <value>The notional.</value>
        public decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal EndDiscountFactor { get; set; }

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


        /// <summary>
        /// Gets or sets the payment discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public Decimal PaymentDiscountFactor { get; set; }

        #endregion
    }
}