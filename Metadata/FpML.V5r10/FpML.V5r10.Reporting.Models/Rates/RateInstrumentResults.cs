#region Usings

using System;
using FpML.V5r10.Reporting.Models.Generic.Coupons;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates
{
    [Serializable]
    public class RateInstrumentResults : CouponResults, IRateInstrumentResults
    {
        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DeltaR { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal LocalCurrencyDeltaR { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal DiscountFactorAtMaturity { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal HistoricalDeltaR { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal LocalCurrencyHistoricalDeltaR { get; set; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public Decimal[] PCE { get; set; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public Decimal[] PCETerm { get; set; }

        /// <summary>
        /// Gets the break even stike.
        /// </summary>
        /// <value>The break even strike.</value>
        public Decimal BreakEvenStrike { get; set; }
    }
}
