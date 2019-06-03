using System;

namespace FpML.V5r10.Reporting
{
    internal static class PaymentHelper
    {
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
            var itemsElementName = new ItemsChoiceType[1];
            itemsElementName[0] = ItemsChoiceType.adjustedDate;
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