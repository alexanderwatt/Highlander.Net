using System;

namespace Orion.Models.Commodities
{
    public class CommodityAverageAssetResults : ICommodityAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public Decimal NPV { get; set; }

        ///// <summary>
        ///// Gets the npv.
        ///// </summary>
        ///// <value>The implied quote.</value>
        //public decimal BaseCcyNPV { get; set; }

        ///// <summary>
        ///// Gets the npv.
        ///// </summary>
        ///// <value>The implied quote.</value>
        //public decimal ForeignCcyNPV { get; set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the fx spot.
        /// </summary>
        /// <value>The delta wrt the fx forward.</value>
        public decimal SpotDelta { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal IndexAtMaturity { get; set; }

        /// <summary>
        /// Gets the forward delta the fixed rate.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote { get; set; }

    }
}