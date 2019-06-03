using System;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Trades
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