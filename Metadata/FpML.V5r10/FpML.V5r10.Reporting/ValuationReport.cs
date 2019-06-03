namespace FpML.V5r10.Reporting
{
    public partial class ValuationReport
    {
        public Quotation GetFirstQuotationForMetricName(string metricName)
        {
            foreach (TradeValuationItem item in (tradeValuationItem ?? new TradeValuationItem[] { }))
            {
                foreach (AssetValuation assetValuation in (item.valuationSet.assetValuation ?? new AssetValuation[] { }))
                {
                    foreach (Quotation quote in assetValuation.quote)
                    {
                        if (System.String.Compare(metricName, quote.measureType.Value, System.StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return quote;
                        }
                    }
                }
            }
            return null;
        }
    }
}
