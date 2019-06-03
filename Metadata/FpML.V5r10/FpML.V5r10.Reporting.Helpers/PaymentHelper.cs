using System;

namespace FpML.V5r10.Reporting.Helpers
{
    public static class PaymentHelper
    {
        public static Payment Create(String payerPartyReference,
            String receiverPartyReference, String currency, Decimal paymentAmount)
        {
            var payment = new Payment
            {
                payerPartyReference = PartyReferenceFactory.Create(payerPartyReference),
                paymentAmount = new NonNegativeMoney { amount = paymentAmount, currency = new Currency { Value = currency }, amountSpecified = true },
                receiverPartyReference = PartyReferenceFactory.Create(receiverPartyReference)
            };
            return payment;
        }

        public static Payment Create(String payerPartyReference,
            String receiverPartyReference, String currency, Decimal paymentAmount,
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