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

#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Highlander.Reporting.V5r3
{
    public partial class PaymentCalculationPeriod
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<string> GetRequiredPricingStructures() 
        {
            var result = new List<string>();
            if (forecastPaymentAmount != null)
            {
                var currency = forecastPaymentAmount.currency;
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(currency, true);
                result.Add(discountCurve);
            }
            if(Items != null)
            {
                result.AddRange(Items.Select(calculationPeriod => ((CalculationPeriod)calculationPeriod).forecastAmount).Select(forecastAmount => CurveNameHelpers.GetDiscountCurveName(forecastAmount.currency, true)));
            }
            return result;
        }
    }
}
