#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Builds a cross currency basis swap.
    /// </summary>
    public class PriceableXccyBasisSwap : PriceableBasisSwap
    {
        #region Properties

        /// <summary>
        /// The today fx rate
        /// </summary>
        public Decimal FxRate { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXccyBasisSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixing Calendar</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="spread">The spread.</param>
        public PriceableXccyBasisSwap(DateTime baseDate, BasisSwapNodeStruct nodeStruct,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, BasicQuotation spread)
            : base(baseDate, nodeStruct, fixingCalendar, paymentCalendar, spread)
        {}
    }
}