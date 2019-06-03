using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    public class ZeroRateAssetParameters : IZeroRateAssetParameters
    {
        public string[] Metrics { get; set; }

        #region IZeroRateAssetParameters Members

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
        /// Gets or sets the compounding frequency.
        /// </summary>
        ///  <value>The frequency.</value>
        public Decimal PeriodAsTimesPerYear { get; set; }

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
        /// The payment discountfactor
        /// </summary>
        public Decimal PaymentDiscountFactor { get; set; }

        #endregion
    }
}