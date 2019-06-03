#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.CurveEngine.Assets.Equity;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableEquityForward : PriceableEquityAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableEquityForward"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="position">The underlysing prosiiotn.</param>
        /// <param name="equity">The equity</param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        public PriceableEquityForward(DateTime baseDate, int position, EquityNodeStruct equity, 
        IBusinessCalendar settlementCalendar, BasicQuotation marketQuote)
            : base(baseDate, position, equity.SettlementDate, settlementCalendar, marketQuote)
        {
            IsXD = IsExDiv();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            return Quote.value;
        }
    }
}