#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Util.NamedValues;

#endregion

namespace Orion.CurveEngine.Assets.Rates.CapFloorLet
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableDiscountCaplet : PriceableCaplet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCaplet"/> class.
        /// This is a special case for use with the factry for bootstrapping, as it
        /// uses no calendar logic. This is done by the factory.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">A special class containing all salient data required.</param>
        /// <param name="properties">The properties set This includes strike information.</param>
        /// <param name="fixingCalendar">The fixing/expiry calendar></param>
        /// <param name="paymentCalendar">The paymentCalendar calendar.</param>
        /// <param name="notional">The notional. The default value is 1.00m.</param>
        /// <param name="marketQuotes">The market quotes, including the volatility and possibly the fixed rate as a decimal contained in a basic quotation.</param>
        public PriceableDiscountCaplet(DateTime baseDate, RateOptionNodeStruct nodeStruct, NamedValueSet properties, IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, Decimal notional, BasicAssetValuation marketQuotes)
            : base(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar, notional, marketQuotes)
        {
            IsDiscounted = true;
        }    
    }
}
