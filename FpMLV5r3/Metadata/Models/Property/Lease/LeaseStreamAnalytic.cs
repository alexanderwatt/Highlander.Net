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

using Highlander.Reporting.ModelFramework.V5r3;

#endregion

namespace Highlander.Reporting.Models.V5r3.Property.Lease
{
    public class LeaseStreamAnalytic : ModelAnalyticBase<ILeaseStreamParameters, LeaseInstrumentMetrics>, ILeaseStreamInstrumentResults
    {
        #region Properties

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal NPV => EvaluateNPV();

        /// <summary>
        /// Evaluates the break even rate without solver.
        /// </summary>
        /// <returns></returns>
        public virtual decimal EvaluateNPV()
        {
            var result = 0.0m;
            return result;         
        }


        #endregion

        public decimal GetMultiplier()
        {
            var multiplier = AnalyticParameters.Multiplier;
            return multiplier;
        }
    }
}