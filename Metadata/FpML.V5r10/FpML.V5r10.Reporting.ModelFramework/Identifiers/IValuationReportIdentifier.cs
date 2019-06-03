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