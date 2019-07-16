/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

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
    public partial class FxSwap
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<string> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            result.AddRange(nearLeg.GetRequiredPricingStructures());
            var curves = farLeg.GetRequiredPricingStructures();
            foreach (var curve in curves)
            {
                if (!result.Contains(curve))
                {
                    result.Add(curve);
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
            var result = new List<string>();
            result.AddRange(nearLeg.GetRequiredCurrencies());
            var currencies = farLeg.GetRequiredCurrencies();
            foreach (var currency in currencies)
            {
                if (!result.Contains(currency))
                {
                    result.Add(currency);
                }
            }
            return result;
        }
    }
}
