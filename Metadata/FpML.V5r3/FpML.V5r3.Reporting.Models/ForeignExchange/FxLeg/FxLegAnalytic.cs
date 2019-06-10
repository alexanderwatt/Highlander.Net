/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Models.ForeignExchange.FxLeg
{
    public class FxLegAnalytic : ModelAnalyticBase<IFxLegParameters, FxLegInstrumentMetrics>, IFxLegInstrumentResults
    {
        #region Propereties

        /// <summary>
        /// Gets or sets the forward fx rate.
        /// </summary>
        /// <value>The forward fx rate.</value>
        public Decimal ForwardFxRate { get; protected set; }

        #endregion

        #region Constructor

        public FxLegAnalytic()
        {
            //ToReportingCurrencyRate = 1.0m;
        }

        /// <summary>
        /// This assumes that the rest dates are consistent with the curve.
        /// </summary>
        /// <param name="valuationDate"></param>
        /// <param name="paymentDate"></param>
        /// <param name="indexCurve"></param>
        public FxLegAnalytic(DateTime valuationDate, DateTime paymentDate, IFxCurve indexCurve)
        {
            //ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            ForwardFxRate = (decimal)indexCurve.GetForward(valuationDate, paymentDate);
        }

        /// <summary>
        /// This assumes that the rest dates are consistent with the curve.
        /// </summary>
        /// <param name="valuationDate"></param>
        /// <param name="paymentDate">The payment date. The same rest period is assumed as with the spot date.</param>
        /// <param name="indexCurve">The index curve should be already in the correct form for the fx.</param>
        /// <param name="currency2">Normally the foreign rate curve.</param>
        /// <param name="currency2PerCurrency1">The currency2PerCurrency1 flag. </param>
        /// <param name="currency1">Normally the domestic rate curve. </param>
        public FxLegAnalytic(DateTime valuationDate, DateTime paymentDate, IFxCurve indexCurve, IRateCurve currency1, IRateCurve currency2, 
            bool currency2PerCurrency1)
        {
            //ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            var todayRate = indexCurve.GetForward(valuationDate, valuationDate); //TODO The spot rate may not be the same due to the carry effect, but the evolution works.
            var df1 = currency1.GetDiscountFactor(valuationDate, paymentDate);
            var df2 = currency2.GetDiscountFactor(valuationDate, paymentDate);
            var forward = df1 / df2;
            if (!currency2PerCurrency1)
            {
                forward = df2 / df1;
            }
            ForwardFxRate = (decimal)(todayRate * forward);
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
        public decimal BreakEvenIndex => ForwardFxRate;

        #endregion
    }
}