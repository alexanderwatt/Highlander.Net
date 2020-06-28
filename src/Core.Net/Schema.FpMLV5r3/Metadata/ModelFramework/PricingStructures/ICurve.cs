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
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.ModelFramework.V5r3.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface ICurve : IPricingStructure
    {
        ///// <summary>
        ///// Updates a basic quotation value and then perturbs and rebuilds the curve. 
        ///// Uses the measure type to determine which one.
        ///// </summary>
        ///// <param name="values">The perturbation value array. This must be the same length as the number of assets in the QuotedAssetSet,
        ///// or it will not work.</param>
        ///// <param name="measureType">The measureType of the quotation required.</param>
        ///// <returns></returns>
        //Boolean PerturbCurve(Decimal[] values, String measureType);

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns>The FpML QuotedAssetSet.</returns>
        QuotedAssetSet GetQuotedAssetSet();

        /// <summary>
        /// Gets the TermCurve.
        /// </summary>
        /// <returns>The FpML TermCurve.</returns>
        TermCurve GetTermCurve();

        IDictionary<string, Decimal> GetInputs();
    }
}