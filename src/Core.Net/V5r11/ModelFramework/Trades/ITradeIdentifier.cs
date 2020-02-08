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
using Highlander.Codes.V5r3;
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.ModelFramework.V5r3.Trades
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface ITradeIdentifier : IIdentifier
    {
        /// <summary>
        /// TradeDate
        /// </summary>
        DateTime TradeDate { get; set; }

        /// <summary>
        /// TradeType
        /// </summary>
        ItemChoiceType15 TradeType { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>
        ProductTypeSimpleEnum? ProductType { get; set; }

        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }
        
        ///<summary>
        /// The party1.
        ///</summary>
        string Party1 { get; set; }

        ///<summary>
        /// The party2.
        ///</summary>
        string Party2 { get; set; }

        ///<summary>
        /// The base party.
        ///</summary>
        string BaseParty { get; set; }

        ///<summary>
        /// The counter party.
        ///</summary>
        string CounterParty { get; set; }
    }
}