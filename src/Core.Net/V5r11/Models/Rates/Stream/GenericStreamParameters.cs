using System;
using System.Collections.Generic;
using System.Text;

namespace nabCap.QR.AnalyticModels.Rates
{
    public class GenericStreamParameters: IGenericStreamParameters
    {
        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the notional amount.
        /// </summary>
        /// <value>The notional amount.</value>
        public Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public Decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public Decimal AccrualFactor { get; set; }

        /// <summary>
        /// Gets or sets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV { get; set; }
    }
}
