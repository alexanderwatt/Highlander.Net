using System;

namespace nab.QDS.FpML.V47
{
    public static class PaymentHelper
    {
        public static Payment Create(String payerPartyReference,
            String receiverPartyReference, String currency, Decimal paymentAmount)
        {
            var payment = new Payment
            {
                payerPartyReference = PartyOrAccountReferenceFactory.Create(payerPartyReference),
                paymentAmount = new Money { amount = paymentAmount, currency = new Currency { Value = currency } },
                receiverPartyReference = PartyOrAccountReferenceFactory.Create(receiverPartyReference)
            };
            return payment;
        }

        public static Payment Create(String payerPartyReference,
            String receiverPartyReference, String currency, Decimal paymentAmount,
            DateTime valueDate)
        {
            var payment = new Payment
            {
                adjustedPaymentDate = new IdentifiedDate { Value = valueDate },
                payerPartyReference = PartyOrAccountReferenceFactory.Create(payerPartyReference),
                paymentAmount = new Money { amount = paymentAmount, currency = new Currency { Value = currency }},
                receiverPartyReference = PartyOrAccountReferenceFactory.Create(receiverPartyReference)
            };
            return payment;
        }

        public static Payment Create(string identifier, bool pay, Money paymentAmount,
            DateTime adjustedPaymentDate, PaymentType paymentType,
            decimal discountFactor, Money presentValueAmount)
        {
            var payment = new Payment();
            var result = new IdentifiedDate {Value = adjustedPaymentDate};
            payment.adjustedPaymentDate = result;
            payment.discountFactor = discountFactor;
            payment.discountFactorSpecified = true;
            payment.href = identifier;
            payment.payerPartyReference = (pay) ? PartyOrAccountReferenceFactory.Create("CBA") : PartyOrAccountReferenceFactory.Create("");
            payment.paymentAmount = paymentAmount;
            payment.paymentType = paymentType;
            payment.presentValueAmount = presentValueAmount;
            payment.receiverPartyReference = (pay) ? PartyOrAccountReferenceFactory.Create("") : PartyOrAccountReferenceFactory.Create("CBA");
            return payment;
        }

        public static Payment Create(string identifier, PartyOrAccountReference payerPartyReference,
            PartyOrAccountReference receiverPartyReference, Money paymentAmount,
            IdentifiedDate adjustedPaymentDate, SettlementInformation settlementInformation, PaymentType paymentType,
            decimal discountFactor, Money presentValueAmount)
        {
            var payment = new Payment
                              {
                                  adjustedPaymentDate = adjustedPaymentDate,
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

        public static Payment Create(string identifier, PartyOrAccountReference payerPartyReference,
            PartyOrAccountReference receiverPartyReference, Money paymentAmount,
            AdjustableDate paymentDate, SettlementInformation settlementInformation, PaymentType paymentType,
            decimal discountFactor, Money presentValueAmount)
        {
            var payment = new Payment
                              {
                                  discountFactor = discountFactor,
                                  discountFactorSpecified = true,
                                  href = identifier,
                                  payerPartyReference = payerPartyReference,
                                  paymentAmount = paymentAmount,
                                  paymentDate = paymentDate,
                                  paymentType = paymentType,
                                  presentValueAmount = presentValueAmount,
                                  receiverPartyReference = receiverPartyReference,
                                  settlementInformation = settlementInformation
                              };
            return payment;
        }

        public static Payment Create(string identifier, PartyOrAccountReference payerPartyReference, 
            PartyOrAccountReference receiverPartyReference, Money paymentAmount,
            IdentifiedDate adjustedPaymentDate,  AdjustableDate paymentDate,
            SettlementInformation settlementInformation, PaymentType paymentType,
            decimal discountFactor, Money presentValueAmount)
        {
            var payment = new Payment
                              {
                                  adjustedPaymentDate = adjustedPaymentDate,
                                  discountFactor = discountFactor,
                                  discountFactorSpecified = true,
                                  href = identifier,
                                  payerPartyReference = payerPartyReference,
                                  paymentAmount = paymentAmount,
                                  paymentDate = paymentDate,
                                  paymentType = paymentType,
                                  presentValueAmount = presentValueAmount,
                                  receiverPartyReference = receiverPartyReference,
                                  settlementInformation = settlementInformation
                              };
            return payment;
        }
    }
}