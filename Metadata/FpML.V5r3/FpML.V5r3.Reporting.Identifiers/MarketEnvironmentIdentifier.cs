#region Using directives

using System;
using Orion.ModelFramework.MarketEnvironments;

#endregion

namespace Orion.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class MarketEnvironmentIdentifier : Identifier, IMarketEnvironmentIdentifier
    {
        /// <summary>
        /// The Market.
        /// </summary>
        /// <value></value>
        public string Market {get; set;}

        ///<summary>
        /// The market date.
        ///</summary>
        public DateTime? Date { get; set; }

        ///<summary>
        /// An id for a party.
        ///</summary>
        ///<param name="market">The market Id.</param>
        public MarketEnvironmentIdentifier(string market)
            : base(market)
        {}

        ///<summary>
        /// An id for a party.
        ///</summary>
        ///<param name="market">The market: e.g. EOD, LIVE, QR_LIVE etc.</param>
        ///<param name="marketDate">The market date.</param>
        public MarketEnvironmentIdentifier(string market, DateTime? marketDate)
            : base(market)
        {
            Market = market;
            Date = marketDate;           
            Properties.Set("Market", market);
            if (Date != null)
            {
                UniqueIdentifier = market + "." + (DateTime)Date;
                Properties.Set("MarketDate", (DateTime)Date);
            }
            else
            {
                UniqueIdentifier = market;
            }
            Properties.Set("UniqueIdentifier", UniqueIdentifier);
        }
    }
}