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

#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r3.Reporting
{
    public partial class Fra
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<string> GetRequiredPricingStructures() 
        {
            var result = new List<string>();
            if (notional.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(notional.currency, true);
                result.Add(discountCurve);
            }           
            if (floatingRateIndex != null && indexTenor!=null)
            {
                result.Add(CurveNameHelpers.GetForecastCurveName(floatingRateIndex, indexTenor[0]));
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<string> {notional.currency.Value};
            return result;
        }
    }
}
