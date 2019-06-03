#region Using directives

using FpML.V5r10.Reporting.ModelFramework.Identifiers;
using Orion.Constants;

#endregion

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class EquityIdentifier : Identifier, IEquityIdentifier
    {
        /// <summary>
        /// The Source System.
        /// </summary>
        /// <value></value>
        public string SourceSystem {get; set;}

        ///<summary>
        /// The base party.
        ///</summary>
        public string MarketSector { get; set; }

        /// <summary>
        ///  An id for a bond.
        /// </summary>
        /// <param name="ticker">The bond ticker. </param>
        /// <param name="pricingSource">The oricing source is required for uniqueness. For example, AU for Australian shares.</param>
        public EquityIdentifier(string ticker, string pricingSource)
            : base(BuildUniqueId(ticker, pricingSource))
        {
            MarketSector = MarketSectorEnum.Equity.ToString();
            Id = BuildId(ticker, pricingSource);
        }

        private static string BuildUniqueId(string ticker, string pricingSource)
        {
            return FunctionProp.ReferenceData + "." + ReferenceDataProp.Equity + "." + ticker + "." + pricingSource;
        }

        public static string BuildId(string ticker, string pricingSource)
        {
            return MarketSectorEnum.Equity + "." + ticker + "." + pricingSource;
        }
    }
}