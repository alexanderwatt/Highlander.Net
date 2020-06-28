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
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.ModelFramework.V5r3
{
    /// <summary>
    /// The base market environment interface
    /// </summary>
    public interface IMarketEnvironment
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        object GetProperty(string name);

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> GetProperties();

        /// <summary>
        /// Gets the fp ML data.
        /// </summary>
        /// <returns></returns>
        Market GetFpMLData();

        /// <summary>
        /// Gets the pricing structure.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IPricingStructure GetPricingStructure(string name);

        /// <summary>
        /// Gets the pricing structures.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, IPricingStructure> GetPricingStructures();


        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <returns></returns>
        IList<IMarketEnvironment> GetChildren();

        ///<summary>
        /// Returns the market.
        ///</summary>
        ///<returns></returns>
        Market GetMarket();
    }
}
