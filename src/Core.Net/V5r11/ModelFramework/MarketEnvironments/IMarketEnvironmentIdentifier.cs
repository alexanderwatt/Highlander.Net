﻿/*
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

namespace Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IMarketEnvironmentIdentifier : IIdentifier
    {
        /// <summary>
        /// Gets the Market.
        /// </summary>
        /// <value>The Market.</value>
        string Market { get; }

        ///<summary>
        /// Gets the market date.
        ///</summary>
        DateTime? Date { get; }
    }
}
