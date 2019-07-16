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

#region Using directives

using Orion.ModelFramework.Trades;

#endregion

namespace Orion.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class PartyIdentifier : Identifier, IPartyIdentifier
    {
        /// <summary>
        /// The Source System.
        /// </summary>
        /// <value></value>
        public string SourceSystem {get; set;}

        ///<summary>
        /// The base party.
        ///</summary>
        public string PartyName { get; set; }

        ///<summary>
        /// An id for a party.
        ///</summary>
        ///<param name="partyId">The party Id.</param>
        public PartyIdentifier(string partyId)
            : base(partyId)
        {}
    }
}