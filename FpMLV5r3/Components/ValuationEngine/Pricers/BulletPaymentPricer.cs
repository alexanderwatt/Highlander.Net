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
using Highlander.ValuationEngine.V5r3.Instruments;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;
using Highlander.ValuationEngine.V5r3.Helpers;

#endregion

namespace Highlander.ValuationEngine.V5r3.Pricers
{
    [Serializable]
    public class BulletPaymentPricer : PriceableCashflow, IPriceableInstrumentController<BulletPayment>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BulletPaymentPricer"/> class.
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
                PaymentTypeHelper.Create("Certain"),
                CashflowTypeHelper.Create(CashflowTypeEnum.PrincipalExchange.ToString()),
                false, paymentCalendar)
        {
            ProductType = ProductTypeSimpleEnum.BulletPayment;
        }

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
            //Setting the items array which contains product type and product is information.
            //payment type information
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
            if (CalculationPerformedIndicator)
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
        /// <param name="businessDayAdjustments"> </param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static BulletPayment Parse(string productType, Boolean payerIsBaseParty,
            DateTime paymentDate, string businessDayCalendar,
            string businessDayAdjustments, string currency, decimal amount)
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
            var tempDate = DateTypesHelper.ToAdjustableDate(paymentDate, businessDayAdjustments,
                                                            businessDayCalendar);
            px.payment.paymentDate = AdjustableOrAdjustedDateHelper.Create(tempDate.unadjustedDate.Value, null, tempDate.dateAdjustments);//TODO
            //Setting the items array which contains product type and product is information.
            //payment type information
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
        /// <param name="businessDayAdjustments"></param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Trade CreateBulletPayment(string tradeId, DateTime tradeDate, string productType, Boolean payerIsBaseParty,
            DateTime paymentDate, string businessDayCalendar, string businessDayAdjustments, string currency, decimal amount)
        {
            var trade = new Trade {id = tradeId, tradeHeader = new TradeHeader()};
            var party1 =  PartyTradeIdentifierHelper.Parse(tradeId, "party1") ;
            var party2 =  PartyTradeIdentifierHelper.Parse(tradeId, "party2") ;
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2};
            trade.tradeHeader.tradeDate = new IdentifiedDate {Value = tradeDate};
            var payment = Parse(productType, payerIsBaseParty, paymentDate, businessDayCalendar, businessDayAdjustments, currency, amount);
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
            return new List<InstrumentControllerBase> {this};
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