﻿using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base interface for all priceable cashflows
    /// </summary>
    public interface IPriceableCashflow<AMP, AMR> : IMetricsCalculation<AMP, AMR>
    {
        /// <summary>
        /// Gets the payment type.
        /// </summary>
        /// <value>The payment type.</value>
        PaymentType PaymentType { get; set; }

        /// <summary>
        /// Gets the cash flow type.
        /// </summary>
        /// <value>The cash flow type.</value>
        CashflowType CashflowType { get; set; }

        /// <summary>
        /// Gets boolean flag indicating if the cash flow is realised.
        /// </summary>
        /// <value>The boolean flag indicating if the cash flow is realised.</value>
        Boolean IsRealised { get; }

        /// <summary>
        /// Gets the boolean flag indicating whether the payment date is included in npv.
        /// </summary>
        /// <value>The boolean flag indicating whether the payment date is included in npv.</value>
        Boolean PaymentDateIncluded { get; set; }

        /// <summary>
        /// Gets the payment date.
        /// </summary>
        /// <value>The payment date.</value>
        DateTime PaymentDate { get; }

        /// <summary>
        /// The payment discount factor.
        /// </summary>
        Decimal PaymentDiscountFactor { get; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        Money NPV { get; }

        /// <summary>
        /// Gets the name of the discount curve.
        /// </summary>
        string DiscountCurveName { get; }
    }
}