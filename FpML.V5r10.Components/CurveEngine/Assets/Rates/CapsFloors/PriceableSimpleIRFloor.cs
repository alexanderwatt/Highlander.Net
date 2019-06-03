#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Util.NamedValues;

#endregion

namespace Orion.CurveEngine.Assets.Rates.CapsFloors
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableSimpleIRFloor : PriceableSimpleIRCap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRCap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interestRateCap">An interest Rate Cap.</param>
        /// <param name="properties">THe properties, including strike information.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="marketQuotes">The market Quote: premium, normal volatility or lognormal volatility.</param>
        public PriceableSimpleIRFloor(DateTime baseDate, SimpleIRCapNodeStruct interestRateCap,
            NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicAssetValuation marketQuotes)
            : base(baseDate, interestRateCap, properties, fixingCalendar, paymentCalendar, marketQuotes)
        {
            IsCap = false;
        }
    }
}