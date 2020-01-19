/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace FpML.V5r11.Reporting
{
    internal static class PaymentHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payerPartyReference"></param>
        /// <param name="receiverPartyReference"></param>
        /// <param name="currency"></param>
        /// <param name="paymentAmount"></param>
        /// <returns></returns>
        public static Payment Create(String payerPartyReference,
            String receiverPartyReference, String currency, Decimal paymentAmount)
        {
            var payment = new Payment
            {
                payerPartyReference = CreatePartyReference(payerPartyReference),
                paymentAmount = new NonNegativeMoney { amount = paymentAmount, currency = new Currency { Value = currency }, amountSpecified = true},
                receiverPartyReference = CreatePartyReference(receiverPartyReference)
            };
            return payment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payerPartyReference"></param>
        /// <param name="receiverPartyReference"></param>
        /// <param name="currency"></param>
        /// <param name="paymentAmount"></param>
        /// <param name="valueDate"></param>
        /// <returns></returns>
        public static Payment Create(String payerPartyReference,
            String receiverPartyReference, String currency, Decimal paymentAmount,
            DateTime valueDate)
        {
            var payment = new Payment
            {
                paymentDate = CreateAdjustedDate(valueDate),
                payerPartyReference = CreatePartyReference(payerPartyReference),
                paymentAmount = new NonNegativeMoney { amount = paymentAmount, currency = new Currency { Value = currency }, amountSpecified = true },
                receiverPartyReference = CreatePartyReference(receiverPartyReference)
            };
            return payment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="pay"></param>
        /// <param name="paymentAmount"></param>
        /// <param name="adjustedPaymentDate"></param>
        /// <param name="paymentType"></param>
        /// <param name="discountFactor"></param>
        /// <param name="presentValueAmount"></param>
        /// <returns></returns>
        public static Payment Create(string identifier, bool pay, NonNegativeMoney paymentAmount,
            DateTime adjustedPaymentDate, PaymentType paymentType,
            decimal discountFactor, Money presentValueAmount)
        {
            var payment = new Payment();
            var result = CreateAdjustedDate(adjustedPaymentDate);
            payment.paymentDate = result;
            payment.discountFactor = discountFactor;
            payment.discountFactorSpecified = true;
            payment.href = identifier;
            payment.payerPartyReference = (pay) ? CreatePartyReference("CBA") : CreatePartyReference("");
            payment.paymentAmount = paymentAmount;
            payment.paymentType = paymentType;
            payment.presentValueAmount = presentValueAmount;
            payment.receiverPartyReference = (pay) ? CreatePartyReference("") : CreatePartyReference("CBA");
            return payment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="payerPartyReference"></param>
        /// <param name="receiverPartyReference"></param>
        /// <param name="paymentAmount"></param>
        /// <param name="adjustablePaymentDate"></param>
        /// <param name="settlementInformation"></param>
        /// <param name="paymentType"></param>
        /// <param name="discountFactor"></param>
        /// <param name="presentValueAmount"></param>
        /// <returns></returns>
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

        private static PartyReference CreatePartyReference(string href)
        {
            var partyOrTradeSideReference = new PartyReference { href = href };
            return partyOrTradeSideReference;
        }

        private static AdjustableOrAdjustedDate CreateAdjustedDate(DateTime adjustedDate)
        {
            var date = new AdjustableOrAdjustedDate();
            var identifiedDate = CreateIdentifiedDate("AdjustedDate", adjustedDate);
            var items = new object[1];
            items[0] = identifiedDate;
            date.Items = items;
            var itemsElementName = new ItemsChoiceType1[1];
            itemsElementName[0] = ItemsChoiceType1.adjustedDate;
            date.ItemsElementName = itemsElementName;
            return date;
        }

        private static IdentifiedDate CreateIdentifiedDate(string id, DateTime value)
        {
            var identifiedDate = new IdentifiedDate { id = id, Value = value };
            return identifiedDate;
        }
    }
}