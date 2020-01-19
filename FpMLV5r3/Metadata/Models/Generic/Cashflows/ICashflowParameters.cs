/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;

#endregion

namespace Highlander.Reporting.Models.V5r3.Generic.Cashflows
{
    public interface ICashflowParameters
    {
        /// <summary>
        /// Gets or sets the fx rate.
        /// </summary>
        ///  <value>The fx rate.</value>
        Decimal? ToReportingCurrencyRate { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal? Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the Payment Discount Factor.
        /// </summary>
        ///  <value>The Payment Discount Factor.</value>
        Decimal? PaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the discount curve.
        /// </summary>
        /// <value>The discount curve.</value>
        IRateCurve DiscountCurve { get; set; }

        /// <summary>
        /// Gets or sets the discount curves for calculating Delta1PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        ICollection<IPricingStructure> Delta1PDHCurves { get; set; }

        /// <summary>
        /// Gets or sets the perturbation for the Delta1PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        Decimal Delta1PDHPerturbation { get; set; }

        /// <summary>
        /// Gets or sets the fx curve.
        /// </summary>
        /// <value>The fx curve.</value>
        IFxCurve ReportingCurrencyFxCurve { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>The currency.</value>
        String Currency { get; set; }

        /// <summary>
        /// Gets or sets the valuation date.
        /// </summary>
        /// <value>The valuation date.</value>
        DateTime ValuationDate { get; set; }

        /// <summary>
        /// Gets or sets the payment date.
        /// </summary>
        /// <value>The payment date.</value>
        DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the reporting currency.
        /// </summary>
        /// <value>The currency.</value>
        String ReportingCurrency { get; set; }

        /// <summary>
        /// Gets or sets the notional amount.
        /// </summary>
        /// <value>The payment amount.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the market quote.
        /// </summary>
        /// <value>The  market quote.</value>
        Decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets or sets the realised flag.
        /// </summary>
        /// <value>The IsRealised flag.</value>
        Boolean IsRealised { get; set; }

        /// <summary>
        /// Gets or sets the curve year fraction.
        /// </summary>
        /// <value>The curve year fraction.</value>
        Decimal CurveYearFraction { get; set; }

        /// <summary>
        /// Gets or sets the bucketed dates.
        /// </summary>
        /// <value>The bucketed dates.</value>
        DateTime[] BucketedDates { get; set; }

        /// <summary>
        /// Gets or sets the compounding frequency.
        /// </summary>
        ///  <value>The frequency.</value>
        Decimal PeriodAsTimesPerYear { get; set; }

        /// <summary>
        /// Gets or sets the rate to be used for bucketing.
        /// </summary>
        /// <value>The bucketing rate.</value>
        Decimal BucketingRate { get; set; }

        /// <summary>
        /// Gets or sets the bucketed discount factors.
        /// </summary>
        /// <value>The bucketed discount factors.</value>
        Decimal[] BucketedDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the bucketed year fractions.
        /// </summary>
        /// <value>The bucketed year fractions.</value>
        Decimal[] BucketedYearFractions { get; set; }
    }
}