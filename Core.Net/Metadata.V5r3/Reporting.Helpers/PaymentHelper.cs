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

using System;
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class PaymentHelper
    {
        public static Payment Create(string payerPartyReference,
            string receiverPartyReference, string currency, decimal paymentAmount)
        {
            var payment = new Payment
            {
                payerPartyReference = PartyReferenceFactory.Create(payerPartyReference),
                paymentAmount = new NonNegativeMoney { amount = paymentAmount, currency = new Currency { Value = currency }, amountSpecified = true },
                receiverPartyReference = PartyReferenceFactory.Create(receiverPartyReference)
            };
            return payment;
        }

        public static Payment Create(string payerPartyReference,
            string receiverPartyReference, string currency, Decimal paymentAmount,
            DateTime valueDate)
        {
            var payment = new Payment
            {
                paymentDate = AdjustableOrAdjustedDateHelper.CreateAdjustedDate(valueDate),
                payerPartyReference = PartyReferenceFactory.Create(payerPartyReference),
                paymentAmount = new NonNegativeMoney { amount = paymentAmount, currency = new Currency { Value = currency }, amountSpecified = true },
                receiverPartyReference = PartyReferenceFactory.Create(receiverPartyReference)
            };
            return payment;
        }

        public static Payment Create(string identifier, bool pay, NonNegativeMoney paymentAmount,
            DateTime adjustedPaymentDate, PaymentType paymentType,
            decimal discountFactor, Money presentValueAmount)
        {
            var payment = new Payment();
            var result = AdjustableOrAdjustedDateHelper.CreateAdjustedDate(adjustedPaymentDate);
            payment.paymentDate = result;
            payment.discountFactor = discountFactor;
            payment.discountFactorSpecified = true;
            payment.href = identifier;
            payment.payerPartyReference = (pay) ? PartyReferenceFactory.Create("CBA") : PartyReferenceFactory.Create("");
            payment.paymentAmount = paymentAmount;
            payment.paymentType = paymentType;
            payment.presentValueAmount = presentValueAmount;
            payment.receiverPartyReference = (pay) ? PartyReferenceFactory.Create("") : PartyReferenceFactory.Create("CBA");
            return payment;
        }

        public static Payment Create(string identifier, PartyReference payerPartyReference,
            PartyReference receiverPartyReference, NonNegativeMoney paymentAmount,
            AdjustableOrAdjustedDate adjustablePaymentDate, SettlementInformation settlementInformation, PaymentType paymentType,
            decimal discountFactor, Money presentValueAmount)
        {
            var payment = new Payment
                              {
                                  paymentDate = adjustablePaymentDate,
                                  discountFactor = discountFactor,
                                  discountFactorSpecified = true,
                                  href = identifier,
                                  payerPartyReference = payerPartyReference,
                                  paymentAmount = paymentAmount,
                                  paymentType = paymentType,
                                  presentValueAmount = presentValueAmount,
                                  receiverPartyReference = receiverPartyReference,
                                  settlementInformation = settlementInformation
                              };
            return payment;
        }
    }
}