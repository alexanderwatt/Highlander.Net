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

namespace FpML.V5r3.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public partial class BulletPayment
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures() 
        {
            var result = payment.GetRequiredPricingStructures();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<string> GetRequiredCurrencies()
        {
            var result = payment.GetRequiredCurrencies();
            return result;
        }
    }
}
