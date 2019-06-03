#region Using directives

using System;
using Orion.Constants;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableBondForward : PriceableBondAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleBond"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The bond nodeStruct</param>
        /// <param name="tenor">The forward tenor.</param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="quoteType">THe quote Type</param>
        public PriceableBondForward(DateTime baseDate, BondNodeStruct nodeStruct, Period tenor, IBusinessCalendar settlementCalendar, IBusinessCalendar paymentCalendar, BasicQuotation marketQuote, BondPriceEnum quoteType)
            : base(baseDate, nodeStruct.Bond.faceAmount, nodeStruct.Bond.currency, nodeStruct.SettlementDate, nodeStruct.ExDivDate, nodeStruct.BusinessDayAdjustments, marketQuote, quoteType)
        {
            Id = nodeStruct.Bond.id;
            SettlementDateCalendar = settlementCalendar;       
            //Get the settlement date
            var settlement1 = GetEffectiveDate(baseDate, settlementCalendar, tenor, nodeStruct.SettlementDate.businessDayConvention);
            var settlement2 = GetSettlementDate(baseDate, settlementCalendar, nodeStruct.SettlementDate);
            if (settlement1 <= settlement2)
            {
                SettlementDate = settlement1;
                MaturityDate = settlement1;
            }
            else
            {
                SettlementDate = settlement2;
                MaturityDate = GetEffectiveDate(SettlementDate, paymentCalendar, tenor,
                                                nodeStruct.SettlementDate.businessDayConvention);
            }
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

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return MaturityDate;
        }
    }
}