#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;

#endregion

namespace FpML.V5r10.Reporting.Models.Generic.Cashflows
{
    public class CashflowParameters: ICashflowParameters
    {
        /// <summary>
        /// Gets or sets the fx rate.
        /// </summary>
        ///  <value>The fx rate.</value>
        public Decimal? ToReportingCurrencyRate { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        public Decimal? Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the Payment Discount Factor.
        /// </summary>
        ///  <value>The Payment Discount Factor.</value>
        public Decimal? PaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the discount curve.
        /// </summary>
        /// <value>The discount curve.</value>
        public IRateCurve DiscountCurve { get; set; }

        /// <summary>
        /// Gets or sets the discount curves for calculating Delta1PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        public ICollection<IPricingStructure> Delta1PDHCurves { get; set; }

        /// <summary>
        /// Gets or sets the perturbation for the Delta1PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        public Decimal Delta1PDHPerturbation { get; set; }

        /// <summary>
        /// Gets or sets the fx curve.
        /// </summary>
        /// <value>The fx curve.</value>
        public IFxCurve ReportingCurrencyFxCurve { get; set; }

        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>The currency.</value>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the valuation date.
        /// </summary>
        /// <value>The valuation date.</value>
        public DateTime ValuationDate { get; set; }

        /// <summary>
        /// Gets or sets the payment date.
        /// </summary>
        /// <value>The payment date.</value>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the notional amount.
        /// </summary>
        /// <value>The notional amount.</value>
        public Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the reporting currency.
        /// </summary>
        /// <value>The reporting currency.</value>
        public string ReportingCurrency { get; set; }

        /// <summary>
        /// Gets or sets the market quote.
        /// </summary>
        /// <value>The  market quote.</value>
        public decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets or sets the realised flag.
        /// </summary>
        /// <value>The IsRealised flag.</value>
        public bool IsRealised { get; set; }

        /// <summary>
        /// Gets or sets the curve year fraction.
        /// </summary>
        /// <value>The curve year fraction.</value>
        public Decimal CurveYearFraction { get; set; }

        /// <summary>
        /// Gets or sets the bucketed dates.
        /// </summary>
        /// <value>The bucketed dates.</value>
        public DateTime[] BucketedDates { get; set; }
        /// <summary>
        /// Gets or sets the compounding frequency.
        /// </summary>
        ///  <value>The frequency.</value>
        public Decimal PeriodAsTimesPerYear { get; set; }

        /// <summary>
        /// Gets or sets the rate to be used for bucketting.
        /// </summary>
        /// <value>The bucketting rate.</value>
        public decimal BuckettingRate { get; set; }

        /// <summary>
        /// Gets or sets the bucketed discount factors.
        /// </summary>
        /// <value>The bucketed discount factors.</value>
        public Decimal[] BucketedDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the bucketed year fractions.
        /// </summary>
        /// <value>The bucketed year fractions.</value>
        public Decimal[] BucketedYearFractions { get; set; }
    }
}