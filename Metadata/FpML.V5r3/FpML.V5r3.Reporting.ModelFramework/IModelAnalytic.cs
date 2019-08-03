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

namespace Orion.ModelFramework
{
    /// <summary>
    /// Base Analytic Model Interface (i.e. Type B Model)
    /// TP - Denotes the parameters Interface
    /// TEnumM - Denotes the enum interface for the input to the calculation
    /// </summary>
    /// <typeparam name="TP">Denotes the parameters Interface</typeparam>
    /// <typeparam name="TEnumM">Denotes the enum interface for the input to the calculation</typeparam>
    public interface IModelAnalytic<TP, TEnumM>
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        string Id { get; }

        /// <summary>
        /// Gets or sets the analytics parameters.
        /// </summary>
        /// <value>The analytics parameters.</value>
        TP AnalyticParameters { get; set; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        List<TEnumM> Metrics { get; }

        /// <summary>
        /// Sets the metrics.
        /// </summary>
        void SetMetrics();

        /// <summary>
        /// Calculates the specified metrics.
        /// </summary>
        /// <typeparam name="TR">The results interface.</typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <param name="metrics">The metrics.</param>
        /// <returns></returns>
        TR Calculate<TR, TC>(TEnumM[] metrics);

        /// <summary>
        /// Calculates the specified analytic parameters.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <param name="analyticParameters">The analytic parameters.</param>
        /// <param name="metrics">The metrics.</param>
        /// <returns></returns>
        TR Calculate<TR, TC>(TP analyticParameters, TEnumM[] metrics);
     }
}
