#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.Models.Generic.Cashflows;

#endregion

namespace FpML.V5r10.Reporting.Models.Generic.Coupons
{
    public interface ICouponParameters : ICashflowParameters
    {
        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        Decimal AccrualFactor { get; set; }

        /// <summary>
        /// Gets or sets the discount curves for calculating Delta0PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        ICollection<IPricingStructure> Delta0PDHCurves { get; set; }

        /// <summary>
        /// Gets or sets the perturbation for the Delta0PDH metrics.
        /// </summary>
        /// <value>The discount curves.</value>
        Decimal Delta0PDHPerturbation { get; set; }

        /// <summary>
        /// Gets or sets the discounting flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        Boolean HasReset { get; set; }

        /// <summary>
        /// Gets or sets the expected amount.
        /// </summary>
        /// <value>The expected amount.</value>
        Decimal? ExpectedAmount { get; set; }

        /// <summary>
        /// Gets or sets the spread.
        /// </summary>
        /// <value>The spread.</value>
        Decimal Spread { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        Decimal YearFraction { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        Decimal Index { get; set; }

    }
}