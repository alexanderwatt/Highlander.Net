#region Using directives

using System;
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