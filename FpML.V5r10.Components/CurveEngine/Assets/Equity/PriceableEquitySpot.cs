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
    public class PriceableEquitySpot : PriceableEquityAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableEquitySpot"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="position">The underlysing prosiiotn.</param>
        /// <param name="equity">The equity</param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        public PriceableEquitySpot(DateTime baseDate, int position, EquityNodeStruct equity, 
        IBusinessCalendar settlementCalendar, BasicQuotation marketQuote)
            : base(baseDate, position, equity.SettlementDate, settlementCalendar, marketQuote)
        {
            //Issuer = equity.Equity.id;
            IsXD = IsExDiv();
            //EquityCurveName = CurveNameHelpers.GetEquityCurveName(Currency.Value, Issuer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            return Quote.value;//TODO This will only work for ytm quotes.
        }
    }
}