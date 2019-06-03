#region Using directives

using System;
using FpML.V5r3.Reporting;
using Orion.CurveEngine.Assets.Inflation.Swaps;

#endregion

namespace Orion.CurveEngine.Assets.Inflation.Index
{
    ///<summary>
    ///</summary>
    public abstract class PriceableInflationIndex : PriceableSimpleInflationAsset
    {
        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset ResetDateConvention { get; protected set; }

        /// <summary>
        /// Gets the index of the underlying rate.
        /// </summary>
        /// <value>The index of the underlying rate.</value>
        public RateIndex UnderlyingRateIndex { get; protected set; }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <returns></returns>
        protected abstract DateTime GetEffectiveDate();

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateIndex"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">The amount</param>
        /// <param name="rateIndex">Index of the rate.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="resetDateConvention">The reset date convention.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        protected PriceableInflationIndex(DateTime baseDate, Decimal amount, RateIndex rateIndex,
                                          BusinessDayAdjustments businessDayAdjustments, RelativeDateOffset resetDateConvention,
                                          BasicQuotation fixedRate)
            : base(baseDate, amount, businessDayAdjustments, fixedRate)
        {
            Id = rateIndex.id;
            UnderlyingRateIndex = rateIndex;
            if (PeriodEnum.D != resetDateConvention.period)
            {
                throw new System.Exception("Only day units are supported!");
            }
            ResetDateConvention = resetDateConvention;
            Id = rateIndex.id;
        }
    }
}