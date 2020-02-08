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

namespace Highlander.Reporting.ModelFramework.V5r3
{
    /// <summary>
    ///  TD - Denotes a generic type for model data
    ///  TR - Denotes a generic type for the results
    /// </summary>
    /// <typeparam name="TD"></typeparam>
    /// <typeparam name="TR"></typeparam>
    public interface IModelController<TD, out TR> //: IController<TD,TR>
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        string Id { get; set; }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        TR Calculate(TD modelData);

        /// <summary>
        /// Gets the model data.
        /// </summary>
        /// <value>The model data.</value>
        TD ModelData { get; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        IList<string> Metrics { get; set; }
    }
}
