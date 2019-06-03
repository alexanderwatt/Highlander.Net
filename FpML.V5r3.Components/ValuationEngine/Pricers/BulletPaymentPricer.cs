#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Helpers;
using Orion.ValuationEngine.Instruments;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ValuationEngine.Helpers;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    [Serializable]
    public class BulletPaymentPricer : PriceableCashflow, IPriceableInstrumentController<BulletPayment>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceablePrincipalExchange"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="payerIsBase">The payerIsBase flag.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public BulletPaymentPricer
            (
            string id
            , bool payerIsBase
            , Money amount
            , AdjustableOrAdjustedDate paymentDate
            , IBusinessCalendar paymentCalendar) :
                base(id, "DiscountedCashflow", payerIsBase, amount, paymentDate,
            PaymentTypeHelper.Create("Certain"), CashflowTypeHelper.Create(CashflowTypeEnum.PrincipalExchange.ToString()), 
            false, paymentCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="BulletPaymentPricer"/> class.
        /// </summary>
        /// <param name="payment">The FpML payment.</param>
        /// <param name="basePartyReference">The base party.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public BulletPaymentPricer(BulletPayment payment, string basePartyReference, IBusinessCalendar paymentCalendar) :
            this(payment.id, IsBaseParty(basePartyReference, payment), AdjustAmount(basePartyReference, payment), 
            payment.payment.paymentDate, paymentCalendar)
        {}

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public BulletPayment Build()
        {
            var px = new BulletPayment
                {
                    payment =
                        new Payment
                            {
                                paymentAmount =
                                    MoneyHelper.GetNonNegativeAmount(PaymentAmount.amount,
                                PaymentAmount.currency.Value)
                            },
                    Items = new object[] {ProductTypeHelper.Create(ProductTypeSimpleEnum.BulletPayment.ToString())},
                    ItemsElementName = new[] {ItemsChoiceType2.productType}
                };
            //Settting the items array which containes product type and oroduct is information.
            //payment type informtation
            px.payment.paymentType = PaymentTypeHelper.Create("Payment");
            //Set the party information
            px.payment.payerPartyReference = new PartyReference {href = "Party2"};
            px.payment.receiverPartyReference = new PartyReference {href = "Party1"};
            if (PayerIsBaseParty)
            {
                px.payment.payerPartyReference = new PartyReference {href = "Party1"};
                px.payment.receiverPartyReference = new PartyReference {href = "Party2"};
            }
            //The payment date
            px.payment.paymentDate = AdjustableOrAdjustedDateHelper.Create(null, PaymentDate, PaymentDateAdjustments);
            if (CalculationPerfomedIndicator)
            {
                px.payment.discountFactor = PaymentDiscountFactor;
                px.payment.discountFactorSpecified = true;
                px.payment.presentValueAmount = MoneyHelper.GetAmount(CalculationResults.NPV, PaymentAmount.currency.Value);
            }
            return px;
        }

        #endregion

        #region Build Product

        /// <summary>
        /// Builds a bullet payment.
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="payerIsBaseParty"></param>
        /// <param name="paymentDate"></param>
        /// <param name="businessDayCalendar"></param>
        /// <param name="businessDayAdjustements"> </param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static BulletPayment Parse(string productType, Boolean payerIsBaseParty,
            DateTime paymentDate, string businessDayCalendar,
            string businessDayAdjustements, string currency, decimal amount)
        {
            var px = new BulletPayment
                {
                    payment =
                        new Payment
                            {
                                paymentAmount =
                                    MoneyHelper.GetNonNegativeAmount(amount,currency)
                            },
                    Items = new object[] {ProductTypeHelper.Create("BulletPayment")},
                    ItemsElementName = new[] {ItemsChoiceType2.productType}
                };
            var tempDate = DateTypesHelper.ToAdjustableDate(paymentDate, businessDayAdjustements,
                                                            businessDayCalendar);
            px.payment.paymentDate = AdjustableOrAdjustedDateHelper.Create(tempDate.unadjustedDate.Value, null, tempDate.dateAdjustments);//TODO
            //Settting the items array which containes product type and oroduct is information.
            //payment type informtation
            px.payment.paymentType = PaymentTypeHelper.Create("Payment");
            //Set the party information
            px.payment.payerPartyReference = new PartyReference {href = "Party2"};
            px.payment.receiverPartyReference = new PartyReference {href = "Party1"};
            if (payerIsBaseParty)
            {
                px.payment.payerPartyReference = new PartyReference {href = "Party1"};
                px.payment.receiverPartyReference = new PartyReference {href = "Party2"};
            }
            return px;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tradeId"></param>
        /// <param name="tradeDate"></param>
        /// <param name="productType"></param>
        /// <param name="payerIsBaseParty"></param>
        /// <param name="paymentDate"></param>
        /// <param name="businessDayCalendar"></param>
        /// <param name="businessDayAdjustements"></param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Trade CreateBulletPayment(string tradeId, DateTime tradeDate, string productType, Boolean payerIsBaseParty,
            DateTime paymentDate, string businessDayCalendar, string businessDayAdjustements, string currency, decimal amount)
        {
            var trade = new Trade {id = tradeId, tradeHeader = new TradeHeader()};
            var party1 =  PartyTradeIdentifierHelper.Parse(tradeId, "party1") ;
            var party2 =  PartyTradeIdentifierHelper.Parse(tradeId, "party2") ;
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2};
            trade.tradeHeader.tradeDate = new IdentifiedDate {Value = tradeDate};
            var payment = Parse(productType, payerIsBaseParty, paymentDate, businessDayCalendar, businessDayAdjustements, currency, amount);
            FpMLFieldResolver.TradeSetBulletPayment(trade, payment);
            return trade;
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return Build();
        }

        #endregion

        #region Interface

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return null;
        }

        private static Money AdjustAmount(string basePartyReference, BulletPayment payment)
        {
            var result = new Money
                             {
                                 currency = new Currency
                                                {
                                                    Value = payment.payment.paymentAmount.currency.Value
                                                },
                                 amount = payment.payment.paymentAmount.amount
                             };
            if (basePartyReference == payment.payment.payerPartyReference.href)
            {
                result.amount = -1 * payment.payment.paymentAmount.amount;
            }
            return result;
        }

        private static bool IsBaseParty(string basePartyReference, BulletPayment payment)
        {
            bool result = basePartyReference == payment.payment.payerPartyReference.href;
            return result;
        }

        #endregion
    }
}