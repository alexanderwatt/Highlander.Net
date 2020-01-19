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

#endregion

namespace Highlander.Reporting.V5r3
{
    public partial class FxOptionPremium
    {
        ///// <summary>
        ///// Gets and sets the required pricing structures to value this leg.
        ///// </summary>
        //public List<String> GetRequiredPricingStructures() 
        //{
        //    var result = new List<String>();
        //    if (paymentAmount != null)
        //    {
        //        var discountCurve = Helpers.GetDiscountCurveName(paymentAmount.currency);
        //        result.Add(discountCurve);
        //    }
        //    return result;
        //}

        //public List<String> GetRequiredCurrencies()
        //{
        //    var result = new List<String> { paymentAmount.currency.Value };
        //    return result;
        //}

        public static FxOptionPremium Create(string payer, string receiver, string currency, decimal amount, DateTime settlementDate)
        {
            var fxOptionPremium = new FxOptionPremium();
            return fxOptionPremium;
        }
    }
}
