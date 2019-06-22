/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace FpML.V5r3.Reporting
{
    public partial class ValuationReport
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="metricName"></param>
        /// <returns></returns>
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
