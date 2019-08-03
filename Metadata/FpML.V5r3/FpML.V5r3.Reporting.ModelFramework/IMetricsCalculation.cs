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

namespace Orion.ModelFramework
{
    /// <summary>
    /// Base Interface for retrieving the input and output from a controllers calculation
    /// </summary>
    /// <typeparam name="IAP">The type of the AP.</typeparam>
    /// <typeparam name="IR">The type of the R.</typeparam>
    public interface IMetricsCalculation<IAP, IR>
    {
        /// <summary>
        /// Gets or sets a value indicating whether [calculation performed indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [calculation performed indicator]; otherwise, <c>false</c>.
        /// </value>
        Boolean CalculationPerformedIndicator { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        IAP AnalyticModelParameters { get; }

        /// <summary>
        /// Gets the calculation results.
        /// </summary>
        /// <value>The calculation results.</value>
        IR CalculationResults { get; }
    }
}
