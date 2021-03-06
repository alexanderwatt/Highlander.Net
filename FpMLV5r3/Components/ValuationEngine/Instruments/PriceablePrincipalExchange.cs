﻿/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using Highlander.Codes.V5r3;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;

#endregion

namespace Highlander.ValuationEngine.V5r3.Instruments
{
    public enum PrincipalExchangeTypes { Initial, Final, Intermediate };

    [Serializable]
    public class PriceablePrincipalExchange : PriceableCashflow, IPriceableInstrumentController<PrincipalExchange>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceablePrincipalExchange"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="payerIsBaseParty">A flag determining if the sign on the amount is relative to the base party.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="adjustedPaymentDate">The adjusted payment date.</param>
        /// <param name="paymentCalendar">Type paymentCalendar.</param>
        public PriceablePrincipalExchange
            (
            string id
            , bool payerIsBaseParty
            , Decimal amount
            , string currency
            , DateTime adjustedPaymentDate
            , IBusinessCalendar paymentCalendar) :
            base(id, "DiscountedCashflow", payerIsBaseParty, MoneyHelper.GetAmount(amount, currency), DateTypesHelper.ToAdjustableOrAdjustedDate(adjustedPaymentDate),
            PaymentTypeHelper.Create("Certain"), CashflowTypeHelper.Create(CashflowTypeEnum.PrincipalExchange.ToString()), false, paymentCalendar)
        {}

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public PrincipalExchange Build()
        {
            var money = MoneyHelper.Mul(PaymentAmount, PayerIsBaseParty);
            var px = PrincipalExchangeHelper.Create(PaymentDate, money.amount);
            px.id = Id;
            //px.
            px.adjustedPrincipalExchangeDate = PaymentDate;
            px.adjustedPrincipalExchangeDateSpecified = true;         
            if (CalculationPerformedIndicator)
            {
                px.discountFactor = PaymentDiscountFactor;
                var npv = System.Math.Abs(ForecastAmount.amount) * PaymentDiscountFactor;
                px.discountFactorSpecified = true;
                px.presentValuePrincipalExchangeAmount = MoneyHelper.Mul(npv, PaymentAmount.currency.Value, PayerIsBaseParty);
            }
            return px;
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return null;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return null;
        }

        #endregion
    }
}
