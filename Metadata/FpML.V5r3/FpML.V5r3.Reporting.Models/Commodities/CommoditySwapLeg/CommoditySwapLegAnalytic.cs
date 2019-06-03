#region Usings

using System;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Models.Commodities.CommoditySwapLeg
{
    public class CommoditySwapLegAnalytic : ModelAnalyticBase<ICommoditySwapLegParameters, CommoditySwapLegInstrumentMetrics>, ICommoditySwapLegInstrumentResults
    {
        #region Propereties

        /// <summary>
        /// Gets or sets the forward fx rate.
        /// </summary>
        /// <value>The forward fx rate.</value>
        public Decimal Units { get; protected set; }

        #endregion

        #region Constructor

        public CommoditySwapLegAnalytic()
        {
            //ToReportingCurrencyRate = 1.0m;
        }

        /// <summary>
        /// This assumes that the rest dates are consistent with the curve.
        /// </summary>
        /// <param name="valuationDate"></param>
        /// <param name="paymentDate"></param>
        /// <param name="indexCurve"></param>
        public CommoditySwapLegAnalytic(DateTime valuationDate, DateTime paymentDate, ICommodityCurve indexCurve)
        {
            //ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            Units = (decimal)indexCurve.GetForward(valuationDate, paymentDate);
        }

        /// <summary>
        /// This assumes that the rest dates are consistent with the curve.
        /// </summary>
        /// <param name="valuationDate"></param>
        /// <param name="paymentDate">The payment date. The same rest period is assumed as with the spot date.</param>
        /// <param name="indexCurve">The index curve should be already in the correct form for the fx.</param>
        /// <param name="currency1">Normally the domestic rate curve. </param>
        public CommoditySwapLegAnalytic(DateTime valuationDate, DateTime paymentDate, ICommodityCurve indexCurve, IRateCurve currency1)
        {
            //ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            var todayRate = indexCurve.GetForward(valuationDate, valuationDate); //TODO The spot rate may not be the same due to the carry effect, but the evolution works.
            var df1 = currency1.GetDiscountFactor(valuationDate, paymentDate);
        }

        #endregion

        #region Implementation of IFxLegInstrumentResults

        /// <summary>
        /// Gets the implied fx rate.
        /// </summary>
        /// <value>The implied fx rate.</value>
        public decimal ImpliedQuote => BreakEvenIndex;

        /// <summary>
        /// Gets the market fx rate.
        /// </summary>
        /// <value>The market fx rate.</value>
        public decimal MarketQuote => AnalyticParameters.MarketQuote;

        /// <summary>
        /// Gets the break even index.
        /// </summary>
        /// <value>The break even index.</value>
        public decimal BreakEvenIndex => Units;

        #endregion
    }
}