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

using Orion.Util.Helpers;

namespace FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Base class which defines the data required by all bootstrap controllers
    /// </summary>
    public class BootstrapControllerData: IBootstrapControllerData
    {

        #region IBootstrapControllerData Members

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <value>The quoted asset set.</value>
        public Pair<PricingStructure, PricingStructureValuation> PricingStructureData   { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BootstrapControllerData"/> class.
        /// </summary>
        /// <param name="yieldCurve">The yield curve.</param>
        /// <param name="pricingStructureValuation">The pricing structure valuation.</param>
        public BootstrapControllerData(PricingStructure yieldCurve, PricingStructureValuation pricingStructureValuation)
        {
            PricingStructureData = new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, pricingStructureValuation);
        }
    }
}