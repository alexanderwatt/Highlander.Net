#region Usings

using System;
using FpML.V5r10.Reporting.Models.Generic.Coupons;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates
{
    public interface IRateInstrumentResults : ICouponResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenRate { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal DeltaR { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal LocalCurrencyDeltaR { get; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal DiscountFactorAtMaturity { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        Decimal HistoricalDeltaR { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        Decimal LocalCurrencyHistoricalDeltaR { get; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        Decimal[] PCE { get; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        Decimal[] PCETerm { get; }

        /// <summary>
        /// Gets the break even stike.
        /// </summary>
        /// <value>The break even strike.</value>
        Decimal BreakEvenStrike { get; }
    }
}
