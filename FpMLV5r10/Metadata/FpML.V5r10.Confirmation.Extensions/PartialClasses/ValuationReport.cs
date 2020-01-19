using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nab.QDS.FpML.V47
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
                        if (String.Compare(metricName, quote.measureType.Value, true) == 0)
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
