using System;
using Orion.Models.Rates.Coupons;

namespace Orion.Models.Rates.Stream
{
    public class StructuredStreamParameters : RateCouponParameters, IStructuredStreamParameters
    {
        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted fla.</value>
        public bool IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the floating npv.
        /// </summary>
        /// <value>The floating npv.</value>
        public Decimal NPV { get; set; }

        /// <summary>
        /// Gets or sets the floating npv.
        /// </summary>
        /// <value>The floating npv.</value>
        public Decimal FloatingNPV { get; set; }

        /// <summary>
        /// Gets/Sets the weightings to be used for break even rate.
        /// </summary>
        /// <value>The Weightings.</value>
        public Decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets/Sets the notional to be used for break even rate.
        /// </summary>
        /// <value>The notionals.</value>
        public Decimal[] CouponNotionals { get; set; }

        /// <summary>
        /// Gets/Sets the year fractions to be used for break even rate.
        /// </summary>
        public Decimal[] CouponYearFractions { get; set; }

        /// <summary>
        /// Gets/Sets the payment discount fractions to be used for break even rate.
        /// </summary>
        public Decimal[] PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the target npv for solvers.
        /// </summary>
        /// <value>The target npv.</value>
        public Decimal TargetNPV { get; set; }
    }
}