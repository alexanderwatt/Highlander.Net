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
using Orion.Analytics.Interpolations.Points;
using Orion.Models.Generic.Cashflows;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Models.ForeignExchange
{
    public class FxRateCashflowAnalytic : FloatingCashflowAnalytic
    {

        #region Constructor

        /// <summary>
        /// Initiates a new model.
        /// </summary>
        public FxRateCashflowAnalytic()
        {
            ToReportingCurrencyRate = 1.0m;
        }

        /// <summary>
        /// Initiates a new model.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="paymentDate">The payment date of the cash flow.</param>
        /// <param name="reportingCurrencyFxCurve">THe fx curve. It must already be normalised.</param>
        /// <param name="indexCurve">The rate curve to use for calculating the forward index.</param>
        /// <param name="discountCurve">The rate curve to use for discounting.</param>
        public FxRateCashflowAnalytic(DateTime valuationDate, DateTime paymentDate,
            IFxCurve reportingCurrencyFxCurve, ICurve indexCurve, IRateCurve discountCurve)
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            if (indexCurve != null)
            {
                FloatingIndex = (decimal)indexCurve.GetValue(new DateTimePoint1D(valuationDate, paymentDate)).Value;
            }
            if (discountCurve != null)
            {
                PaymentDiscountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, paymentDate);
            }
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
        /// <param name="currency1Settlement">Does settlement occur in currency1. If not, then it must be currency2. </param>
        /// <param name="reportingCurrencyFxCurve">The reporting current fx curve from settlement currency to reporting currency. It must already be normalised.</param>
        public FxRateCashflowAnalytic(DateTime valuationDate, DateTime paymentDate, IFxCurve indexCurve, IRateCurve currency1, IRateCurve currency2, 
            bool currency2PerCurrency1, bool currency1Settlement, IFxCurve reportingCurrencyFxCurve)
        {
            ToReportingCurrencyRate = EvaluateReportingCurrencyFxRate(valuationDate, reportingCurrencyFxCurve);
            var todayRate = indexCurve.GetForward(valuationDate, valuationDate); //TODO The spot rate may not be the same due to the carry effect, but the evolution works.
            var df1 = currency1.GetDiscountFactor(valuationDate, paymentDate);
            var df2 = currency2.GetDiscountFactor(valuationDate, paymentDate);
            var forward = df1 / df2;
            if (!currency2PerCurrency1)
            {
                forward = df2 / df1;
            }
            FloatingIndex = (decimal)(todayRate * forward);
            if (currency1Settlement)
            {
                PaymentDiscountFactor = (decimal)currency1.GetDiscountFactor(valuationDate, paymentDate);
            }
            else
            {
                PaymentDiscountFactor = (decimal)currency2.GetDiscountFactor(valuationDate, paymentDate);
            }
        }

        #endregion
    }
}