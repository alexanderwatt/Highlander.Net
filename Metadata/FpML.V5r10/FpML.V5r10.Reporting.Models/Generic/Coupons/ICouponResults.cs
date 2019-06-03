using System;
using FpML.V5r10.Reporting.Models.Generic.Cashflows;

namespace FpML.V5r10.Reporting.Models.Generic.Coupons
{
    public interface ICouponResults : ICashflowResults
    {
        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        Decimal AccrualFactor { get; }

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        Decimal LocalCurrencyAccrualFactor { get; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The net present value of a floating coupon.</value>
        Decimal FloatingNPV { get; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The net present value of a floating coupon.</value>
        Decimal LocalCurrencyFloatingNPV { get; }

        /// <summary>
        /// Gets the historical accrual factor.
        /// </summary>
        /// <value>The historical accrual factor.</value>
        Decimal HistoricalAccrualFactor { get; }

        /// <summary>
        /// Gets the historical accrual factor.
        /// </summary>
        /// <value>The historical accrual factor.</value>
        Decimal LocalCurrencyHistoricalAccrualFactor { get; }

        /// <summary>
        /// Gets the derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal Delta0 { get; }

        /// <summary>
        /// Gets the second derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The gamma wrt the fixed rate.</value>
        Decimal Gamma0 { get; }

        /// <summary>
        /// Gets the cross derivative with respect to the forward Rate and the discount rate.
        /// </summary>
        /// <value>The cross gamma wrt the fixed rate and the discount rate.</value>
        Decimal Delta0Delta1 { get; }

        /// <summary>
        /// Gets the derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal LocalCurrencyDelta0 { get; }

        /// <summary>
        /// Gets the second derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The gamma wrt the fixed rate.</value>
        Decimal LocalCurrencyGamma0 { get; }

        /// <summary>
        /// Gets the cross derivative with respect to the forward Rate and the discount rate.
        /// </summary>
        /// <value>The cross gamma wrt the fixed rate and the discount rate.</value>
        Decimal LocalCurrencyDelta0Delta1 { get; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        Decimal HistoricalDelta0 { get; }

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        Decimal LocalCurrencyHistoricalDelta0 { get; }
    }
}