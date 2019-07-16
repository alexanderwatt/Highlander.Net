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
using System.Linq;

#endregion

namespace FpML.V5r10.Reporting
{
    public partial class CapFloor
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures()
        {
            var result = new List<String>();
            if (capFloorStream != null)
            {
                result.AddRange(capFloorStream.GetRequiredPricingStructures());
                result.AddRange(capFloorStream.GetRequiredVolatilitySurfaces());
            }
            foreach (var payment in (additionalPayment ?? new Payment[] { }))
                result.AddRange(payment.GetRequiredPricingStructures());
            foreach (var prem in (premium ?? new Payment[] { }))
                result.AddRange(prem.GetRequiredPricingStructures());
            return result.Distinct().ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String>();
            if (capFloorStream != null)
            {
                result.AddRange(capFloorStream.GetRequiredCurrencies());
            }
            foreach (var payment in (additionalPayment ?? new Payment[] { }))
                result.AddRange(payment.GetRequiredCurrencies());
            foreach (var prem in (premium ?? new Payment[] { }))
                result.AddRange(prem.GetRequiredCurrencies());
            return result.Distinct().ToList();
        }
    }
}
