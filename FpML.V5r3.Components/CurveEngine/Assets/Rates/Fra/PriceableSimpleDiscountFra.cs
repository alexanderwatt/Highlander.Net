#region Using directives

using System;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableSimpleDiscountFra : PriceableSimpleFra
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleDiscountFra"/> class.
        /// This is a special case for use with the factry for bootstrapping, as it
        /// uses no calendar logic. This is done by the factory.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">A special class containing all salient data required.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.></param>
        /// <param name="notional">The notional. The default value is 1.00m.</param>
        /// <param name="normalisedRate">Thhhe fixed rate as a decimal contained in a basic quotation.</param>
        public PriceableSimpleDiscountFra(DateTime baseDate, SimpleFraNodeStruct nodeStruct, IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, Decimal notional, BasicQuotation normalisedRate)
            : base(baseDate, nodeStruct, fixingCalendar, paymentCalendar, notional, normalisedRate)
        {
            ModelIdentifier = "SimpleDiscountAsset";
        }
    }
}