#region Usings

using System;
using FpML.V5r10.Reporting.Models.Generic.Cashflows;

#endregion

namespace FpML.V5r10.Reporting.Models.Generic.Coupons
{
    [Serializable]
    public class CouponResults : CashflowResults, ICouponResults
    {
        /// <summary>
        /// Gets the accrual factor
        /// </summary>
        public decimal AccrualFactor { get; set; }

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal LocalCurrencyAccrualFactor { get; set; }

        ///// <summary>
        ///// Gets the accrual factor.
        ///// </summary>
        ///// <value>The accrual factor.</value>
        //public decimal ReportingCurrencyAccrualFactor { get; set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The net present value of a floating coupon.</value>
        public decimal FloatingNPV { get; set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The net present value of a floating coupon.</value>
        public decimal LocalCurrencyFloatingNPV { get; set; }

        ///// <summary>
        ///// Gets the npv.
        ///// </summary>
        ///// <value>The net present value of a floating coupon.</value>
        //public decimal ReportingCurrencyFloatingNPV { get; set; }

        /// <summary>
        /// Gets the historical accrual factor.
        /// </summary>
        /// <value>The historical accrual factor.</value>
        public decimal HistoricalAccrualFactor { get; set; }

        /// <summary>
        /// Gets the historical accrual factor.
        /// </summary>
        /// <value>The historical accrual factor.</value>
        public decimal LocalCurrencyHistoricalAccrualFactor { get; set; }

        ///// <summary>
        ///// Gets the historical accrual factor.
        ///// </summary>
        ///// <value>The historical accrual factor.</value>
        //public decimal ReportingCurrencyHistoricalAccrualFactor { get; set; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public Decimal BreakEvenRate { get; set; }

        /// <summary>
        /// Gets the delta wrt the fixed rate.
        /// </summary>
        public decimal Delta0 { get; set; }

        /// <summary>
        /// The gamma0.
        /// </summary>
        public decimal Gamma0 { get; set; }

        /// <summary>
        /// The cross gamma.
        /// </summary>
        public decimal Delta0Delta1 { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal LocalCurrencyDelta0 { get; set; }

        /// <summary>
        /// The local currency gamma0
        /// </summary>
        public decimal LocalCurrencyGamma0 { get; set; }

        /// <summary>
        /// The local currency cross gamma
        /// </summary>
        public decimal LocalCurrencyDelta0Delta1 { get; set; }

        ///// <summary>
        ///// Gets the derivative with respect to the forward Rate.
        ///// </summary>
        ///// <value>The delta wrt the fixed rate.</value>
        //public decimal ReportingCurrencyDelta0 { get; set; }

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal HistoricalDelta0 { get; set; }

        /// <summary>
        /// Gets the historical derivative with respect to the forward Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal LocalCurrencyHistoricalDelta0 { get; set; }

        ///// <summary>
        ///// Gets the historical derivative with respect to the forward Rate.
        ///// </summary>
        ///// <value>The historical delta wrt the fixed rate.</value>
        //public decimal ReportingCurrencyHistoricalDelta0 { get; set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote { get; set; }
    }
}