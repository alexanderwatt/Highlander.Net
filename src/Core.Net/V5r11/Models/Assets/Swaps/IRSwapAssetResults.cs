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

using System.Collections.Generic;

namespace Highlander.Reporting.Models.V5r3.Assets.Swaps
{
    public class IRSwapAssetResults : RateAssetResults, IIRSwapAssetResults
    {
        #region Implementation of IIRSwapAssetResults

        /// <summary>
        /// Gets the all the details for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg1CashFlowDetails => throw new System.NotImplementedException();

        /// <summary>
        /// Gets the all the risks for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg1RiskDetails => throw new System.NotImplementedException();

        /// <summary>
        /// Gets the all the details for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg2CashFlowDetails => throw new System.NotImplementedException();

        /// <summary>
        /// Gets the all the risks for each cashflow in the leg.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double[]> Leg2RiskDetails => throw new System.NotImplementedException();

        #endregion
    }
}