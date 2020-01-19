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


using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.ModelFramework.Identifiers
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IValuationReportIdentifier : IIdentifier
    {
        /// <summary>
        /// TradeDate
        /// </summary>
        DateTime CalculationDateTime { get; set; }

        ///// <summary>
        ///// Properties
        ///// </summary>
        //NamedValueSet Properties { get; set; }

        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }

        ///<summary>
        /// The base party.
        ///</summary>
        string BaseParty { get; set; }

        ///<summary>
        /// The parties.
        ///</summary>
        List<Party> Parties { get; set; }

        ///<summary>
        /// The trades.
        ///</summary>
        List<IIdentifier> TradeList { get; set; }

    }
}