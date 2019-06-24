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

#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r11.Reporting
{
    public partial class BondTransaction
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures()//TODO Need to add the case of floaters where there is a forecast curve.
        {
            var result = new List<String>();
            if (bond?.Item is string && bond.currency != null)
            {
                var tempId = bond.id.Split('-');
                var bondId = tempId[0];
                if (tempId.Length > 2)
                {
                    bondId = tempId[2];
                }
                var bondCurve = CurveNameHelpers.GetBondCurveName(bond.currency.Value, bondId);
                result.Add(bondCurve);
            }
            if (bond?.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(bond.currency.Value, true);
                result.Add(discountCurve);
                if (notionalAmount.currency != null && notionalAmount.currency.Value != bond.currency.Value)
                {
                    var discountCurve2 = CurveNameHelpers.GetDiscountCurveName(notionalAmount.currency, true);
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
            var result = new List<String> { notionalAmount.currency.Value };
            if (notionalAmount.currency.Value != bond.currency.Value)
            {
                result.Add(bond.currency.Value);
            }
            return result;
        }
    }
}