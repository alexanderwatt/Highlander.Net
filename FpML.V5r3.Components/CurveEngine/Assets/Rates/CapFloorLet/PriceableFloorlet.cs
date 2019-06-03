#region Using directives

using System;
using FpML.V5r3.Codes;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.CurveEngine.Assets.Rates.CapFloorLet;
using Orion.Util.NamedValues;
using DayCounterHelper=Orion.Analytics.DayCounters.DayCounterHelper;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableFloorlet : PriceableCaplet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFloorlet"/> class.
        /// This is a special case for use with the factry for bootstrapping, as it
        /// uses no calendar logic. This is done by the factory.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">A special class containing all salient data required.</param>
        /// <param name="fixingCalendar">The fixing/expiry calendar></param>
        /// <param name="paymentCalendar">The paymentCalendar calendar.</param>
        /// <param name="notional">The notional. The default value is 1.00m.</param>
        /// <param name="properties">The properties, including strike information.</param>
        /// <param name="marketQuotes">The fixed rate as a decimal contained in a basic quotation.</param>
        public PriceableFloorlet(DateTime baseDate, RateOptionNodeStruct nodeStruct, NamedValueSet properties, IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, Decimal notional, BasicAssetValuation marketQuotes)
            : base(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar, notional, marketQuotes)
        {
            IsCap = false;
        }
    }
}