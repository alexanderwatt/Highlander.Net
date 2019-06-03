using System;

namespace Orion.Models.ForeignExchange
{
    public class CashAssetParameters : ICashAssetParameters
    {
        private decimal _notionalAmount = 1.0m;

        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get { return _notionalAmount; } set { _notionalAmount = value; } }
    }
}
