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
    public partial class EquityTransaction
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<string> GetRequiredPricingStructures()//TODO Need to add the case of floaters where there is a forecast curve.
        {
            var result = new List<string>();
            if (equity.currency != null)
            {
                var bondCurve = CurveNameHelpers.GetEquityCurveName(equity.currency.Value,
                                                                      equity.id);
                result.Add(bondCurve);
            }
            if (equity.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(equity.currency.Value, true);
                result.Add(discountCurve);
                if (unitPrice.currency != null && unitPrice.currency.Value != equity.currency.Value)
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
            var result = new List<String> { equity.currency.Value };
            if (unitPrice.currency.Value != equity.currency.Value)
            {
                result.Add(unitPrice.currency.Value);
            }
            return result;
        }
    }
}