/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;

namespace FpML.V5r3.Reporting
{
    public partial class FutureTransaction
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<string> GetRequiredPricingStructures()
        {
            var result = new List<string>();
            if (future.currency != null && future.exchangeId != null)
            {
                var items = future.id.Split('-');
                if(items.Length > 2)
                {
                    var exchangeTradeCurve = CurveNameHelpers.GetExchangeTradedCurveName(future.currency.Value,
                        future.exchangeId.Value, items[2]);
                    result.Add(exchangeTradeCurve);
                }             
            }
            if (future.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(future.currency.Value, true);
                result.Add(discountCurve);
                if (unitPrice.currency != null && unitPrice.currency.Value != future.currency.Value)
                {
                    var discountCurve2 = CurveNameHelpers.GetDiscountCurveName(unitPrice.currency, true);
                    result.Add(discountCurve2);
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String> { future.currency.Value };
            if (unitPrice.currency.Value != future.currency.Value)
            {
                result.Add(unitPrice.currency.Value);
            }
            return result;
        }
    }
}