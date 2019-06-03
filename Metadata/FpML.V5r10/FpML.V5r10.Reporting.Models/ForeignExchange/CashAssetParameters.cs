namespace FpML.V5r10.Reporting.Models.ForeignExchange
{
    public class CashAssetParameters : ICashAssetParameters
    {
        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get; set; } = 1.0m;
    }
}
