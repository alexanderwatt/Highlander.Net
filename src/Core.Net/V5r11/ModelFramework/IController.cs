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

namespace Highlander.Reporting.ModelFramework.V5r3
{
    /// <summary>
    ///  D - Denotes a generic type for model data
    ///  R - Denotes a generic type for the results
    /// </summary>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="R"></typeparam>
    public interface IController<D, R>
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
        R Calculate(D modelData);
    }
}
