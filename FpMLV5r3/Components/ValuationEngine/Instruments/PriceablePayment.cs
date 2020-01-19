/*
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
using Highlander.Reporting.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;
using Math = System.Math;

#endregion

namespace Highlander.ValuationEngine.V5r3.Instruments
{
    [Serializable]
    public class PriceablePayment : PriceableCashflow, IPriceableInstrumentController<Payment>
    {
        /// <summary>
        /// 
        /// </summary>
        public IdentifiedDate AdjustedPaymentDate => IdentifiedDateHelper.Create(ItemsChoiceType.adjustedDate.ToString(), PaymentDate);

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceablePayment"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="payerPartyReference">The payer.</param>
        /// <param name="receiverPartyReference">The receiver.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="paymentCalendar">Type paymentCalendar.</param>
        public PriceablePayment
            (
            string id
            , string payerPartyReference
            , string receiverPartyReference
            , bool payerIsBase
            , Money amount
            , AdjustableOrAdjustedDate paymentDate
            , IBusinessCalendar paymentCalendar) :
            base(id, "DiscountedCashflow", payerIsBase, amount, paymentDate, PaymentTypeHelper.Create("Certain"),
            CashflowTypeHelper.Create(CashflowTypeEnum.PrincipalExchange.ToString()), false, paymentCalendar)
        {
            PayerPartyReference = PartyReferenceFactory.Create(payerPartyReference);
            ReceiverPartyReference = PartyReferenceFactory.Create(receiverPartyReference);
            OrderedPartyNames.Add(PayerPartyReference.href);
            OrderedPartyNames.Add(ReceiverPartyReference.href);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceablePayment"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="receiverPartyReference">The receiver.</param>
        /// <param name="payerIsBase">The flag determining if the payer is the base party.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="adjustedPaymentDate">The adjusted payment date.</param>
        /// <param name="payerPartyReference">The payer.</param>
        /// <param name="paymentCalendar">Type paymentCalendar.</param>
        public PriceablePayment
            (
            string id
            , string payerPartyReference
            , string receiverPartyReference
            , bool payerIsBase
            , Decimal amount
            , string currency
            , DateTime adjustedPaymentDate
            , IBusinessCalendar paymentCalendar) :
            base(id, "DiscountedCashflow", payerIsBase, MoneyHelper.GetAmount(amount, currency),
            AdjustableOrAdjustedDateHelper.CreateAdjustedDate(adjustedPaymentDate),
            PaymentTypeHelper.Create("Certain"), CashflowTypeHelper.Create(CashflowTypeEnum.PrincipalExchange.ToString()), false, paymentCalendar)
        {
            PayerPartyReference = PartyReferenceFactory.Create(payerPartyReference);
            ReceiverPartyReference = PartyReferenceFactory.Create(receiverPartyReference);
            OrderedPartyNames.Add(PayerPartyReference.href);
            OrderedPartyNames.Add(ReceiverPartyReference.href);
        }

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public Payment Build()
        {
            var px = PaymentHelper.Create(PayerPartyReference.href, ReceiverPartyReference.href, PaymentAmount.currency.Value, Math.Abs(PaymentAmount.amount), PaymentDate);
            px.id = Id;
            //px.paymentAmount = PaymentAmount;//this is the raw amount without the multiplier effect.
            px.paymentDate = AdjustableOrAdjustedDateHelper.CreateAdjustedDate(AdjustedPaymentDate.Value, PaymentDateAdjustments);
            if (CalculationPerformedIndicator)
            {
                var payment = Math.Abs(ForecastAmount.amount);
                px.paymentAmount = MoneyHelper.GetNonNegativeAmount(payment, ForecastAmount.currency.Value);
                var npv = Math.Abs(NPV.amount);
                var money = MoneyHelper.GetAmount(npv, PaymentAmount.currency.Value);
                px.discountFactor = PaymentDiscountFactor;
                px.discountFactorSpecified = true;
                px.presentValueAmount = money;
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
