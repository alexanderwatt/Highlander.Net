using System;
using FpML.V5r10.Reporting.Models.Rates.Coupons;

namespace FpML.V5r10.Reporting.Models.Rates.Stream
{
    public interface IStructuredStreamParameters : IRateCouponParameters
    {
        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted fla.</value>
        bool IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the floating npv.
        /// </summary>
        /// <value>The floating npv.</value>
        Decimal NPV { get; set; }

        /// <summary>
        /// Gets or sets the floating npv.
        /// </summary>
        /// <value>The floating npv.</value>
        Decimal FloatingNPV { get; set; }

        /// <summary>
        /// Gets/Sets the weightings to be used for break even rate.
        /// </summary>
        /// <value>The Weightings.</value>
        Decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets/Sets the notional to be used for break even rate.
        /// </summary>
        /// <value>The notionals.</value>
        Decimal[] CouponNotionals { get; set; }

        /// <summary>
        /// Gets/Sets the year fractions to be used for break even rate.
        /// </summary>
        Decimal[] CouponYearFractions { get; set; }

        /// <summary>
        /// Gets/Sets the payment discount fractions to be used for break even rate.
        /// </summary>
        Decimal[] PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the target npv for solvers.
        /// </summary>
        /// <value>The target npv.</value>
        Decimal TargetNPV { get; set; }
    }
}